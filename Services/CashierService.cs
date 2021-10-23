using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Services.Events;
using Filuet.Infrastructure.Abstractions.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Filuet.ASC.Kiosk.OnBoard.Cashbox.Core
{
    public class CashierService
    {
        public event EventHandler<MoneyEventArgs> OnTotalAmountCollected;
        public event EventHandler<CashIncomeEventArgs> OnMoneyIncome;
        public event EventHandler<CashIssueEventArgs> OnChangeIssued;

        public CashierService(IEnumerable<ICashDevice> cashDevices)
        {
            _cashDevices = cashDevices;

            foreach (var device in _cashDevices)
                device.OnInserted += (sender, e) => OnMoneyIncome?.Invoke(this, CashIncomeEventArgs.Income(e.Inserted));
        }

        public void Collect(Money toCollect)
        {
            IEnumerable<ICashDevice> acceptors = _cashDevices.Where(x => x.Mode == CashDeviceMode.Acceptance || x.Mode == CashDeviceMode.Both);

            // TODO: activate the devices
            ////foreach (var device in acceptors) // start collecting the change
            ////    device.Start();

            decimal actualCollected = 0m;

            List<(ICashDevice dev, EventHandler<CashAcceptorOnInsertedArgs> hnd)> handlers = new List<(ICashDevice, EventHandler<CashAcceptorOnInsertedArgs>)>(); // handlers collection

            Action unsubscribeHandlersAndStopAllDevices = () =>
            {
                foreach (var device in acceptors)
                {
                    EventHandler<CashAcceptorOnInsertedArgs> toUnsubscribe = handlers.FirstOrDefault(x => x.dev == device).hnd;
                    if (toUnsubscribe != null)
                        device.OnInserted -= toUnsubscribe;

                    device.Stop();
                }
            };

            foreach (var device in acceptors) // start collecting money
            {
                var acceptorHandler = new EventHandler<CashAcceptorOnInsertedArgs>((sender, e) =>
                {
                    actualCollected += e.Inserted.Amount;
                    OnMoneyIncome?.Invoke(this, CashIncomeEventArgs.Income(e.Inserted));
                    if (actualCollected >= toCollect.Value)
                    {
                        OnTotalAmountCollected?.Invoke(this, new MoneyEventArgs { Money = Money.Create(actualCollected, toCollect.Currency) }); // Total amount collected
                        unsubscribeHandlersAndStopAllDevices();
                    }
                }); // A handler to intercept ACTUAL(collected) amount

                device.OnInserted += acceptorHandler;
                handlers.Add((device, acceptorHandler));
            }
        }

        public void IssueChange(Money change)
        {
            if (change <= 0)
                throw new ArgumentException("The change isn't specified");

            // Sort devices by value of nominals that are lower than change value
            IList<ICashDevice> dispensers = _cashDevices.Where(x => x.Mode == CashDeviceMode.Issuance || x.Mode == CashDeviceMode.Both) // get all dispensers
                .OrderByDescending(x => x.GetCashboxStock().Where(s => s.Value > 0 && s.Key.Amount < change.Value) // cash stock must be populated with nominals and value of the nominal must be lower than the change
                .Max(x => x.Key.Amount)).ToList();

            Money changeDebt = Money.From(change); // change debt is the amount that remains to pay for the time being

            // ! To protect against extra issues we're introducing 'orderedToIssue'- the total value of the extracting must not be greater than this amount  
            decimal orderedToIssue = changeDebt.Value; // ordered withdrawal amount

            var @lock = new ReaderWriterLockSlim();

            foreach (var device in dispensers) // start giving the change
            {
                var changeHandler = new EventHandler<CashAcceptorOnDispensedArgs>((sender, e)
                    =>
                {
                    changeDebt -= e.Dispensed.Amount;
                    OnChangeIssued?.Invoke(this, CashIssueEventArgs.Income(e.Dispensed));
                }); // A handler to intercept ACTUAL(confirmed) given change
                device.OnDispensed += changeHandler; // Subscribe on change event

                @lock.EnterWriteLock();
                try
                {
                    bool nothingElseToDo = false;
                    while (orderedToIssue > 0 && !nothingElseToDo)
                    {
                        IDictionary<Denomination, ushort> availableNominals = device.GetCashboxStock().Where(x => x.Value > 0)
                            .OrderByDescending(x => x.Key.Amount).ToDictionary(x => x.Key, y => y.Value); // Sort nominals of the device descending: max->min

                        foreach (var nominal in availableNominals)
                        {
                            if (orderedToIssue < nominal.Key.Amount) // nominal is too big, try next one
                                continue;

                            ushort nominalQtyToExtract = (ushort)(orderedToIssue / nominal.Key.Amount);

                            if (nominalQtyToExtract <= 0) // There is nothing we can do with the device. Try the next one which has lower nominals
                            {
                                nothingElseToDo = true;
                                break;
                            }

                            device.Extract(nominal.Key, nominalQtyToExtract);
                            orderedToIssue = Math.Round(orderedToIssue - nominal.Key.Amount * nominalQtyToExtract, 2);
                        }
                    }
                }
                finally
                {
                    device.OnDispensed -= changeHandler; // Stop working with the device: unsubscribe on change event
                    @lock.ExitWriteLock();
                }
            }
        }

        private readonly IEnumerable<ICashDevice> _cashDevices;

        private delegate void onChange(object sender, CashAcceptorOnDispensedArgs e);
    }
}