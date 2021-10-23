using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Filuet.ASC.Kiosk.OnBoard.Cashbox.Core
{
    public class CashierService
    {
        public CashierService() { }

        public void IssueChange(Denomination change)
        {
            if (change <= 0)
                throw new ArgumentException("The change isn't specified");

            IList<ICashDeviceAdapter> devices = CashDevices.OrderBy(x => x.IssueIndex).ToList(); // Sort devices in priority order

            Money changeDebt = Money.From(change);
            var @lock = new ReaderWriterLockSlim();

            foreach (var device in devices)
            {
                @lock.EnterWriteLock();
                try
                {
                    while (changeDebt > 0)
                    {
                        (Money change, Money nativeChange) nextChange = device.GiveChange(changeDebt);
                        if (nextChange.nativeChange == null)
                            break;
                        else changeDebt = changeDebt - nextChange.nativeChange; // Decrease change debt
                    }
                }
                finally
                {
                    @lock.ExitWriteLock();
                }
            }
        }

        public void Stop()
        {
            //CashDevice.StopPayment();
        }

        public IEnumerable<ICashDeviceAdapter> CashDevices { get; set; }

        public event EventHandler<CashIncomeEventArgs> OnReceived;
        public event EventHandler<CashIncomeEventArgs> OnGivedChange;
        public event EventHandler<StopCashDeviceEventArgs> OnStop;
    }
}
