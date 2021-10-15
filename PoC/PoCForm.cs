﻿using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common.Helpers;
using Filuet.Hardware.CashAcceptors.Periphery.ITL;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000;
using PoC.UIModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoC
{
    public partial class PoCForm : Form
    {
        private ICashDevice _cashAcceptor;
        private DeviceType? _currentlySelectedSevice;

        public PoCForm()
        {
            InitializeComponent();
        }

        private void PoCForm_Load(object sender, EventArgs e)
        {
            deviceComboBox.Items.AddRange(EnumHelpers.GetValues<DeviceType>().Select(x=> new Device { Type = x }).ToArray());

          //  if (deviceComboBox.Items.Count > 0)
           //     deviceComboBox.SelectedIndex = 0;
        }

        private void PoCForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cashAcceptor != null)
                _cashAcceptor.Stop();

            Thread.Sleep(1000); // Wait until money the device is turning off
        }

        private void deviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Device selectedDevice = deviceComboBox.SelectedItem as Device;

            if (_cashAcceptor != null && selectedDevice.Type != _currentlySelectedSevice)
                _cashAcceptor.Stop();

            if (selectedDevice.Type == _currentlySelectedSevice)
                return;

            switch (selectedDevice.Type)
            {
                case DeviceType.ITL:
                    _cashAcceptor = RunITLSsp();
                    break;
                case DeviceType.J2000:
                    _cashAcceptor = RunJ2000();
                    break;
                default:
                    break;
            }

            _currentlySelectedSevice = selectedDevice.Type;

            if (_cashAcceptor.State == CashDeviceState.OK)
                UpdateBalance();
            else MessageBox.Show($"{_currentlySelectedSevice.Value.GetCode()} is currently disabled");
        }

        private void UpdateBalance()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                UpdateLevels();
                UpdateCashboxLevels();
                UpdateTotal();
            });
        }

        private void UpdateLevels()
        {
            Invoke(new MethodInvoker(delegate ()
            {
                payoutStockListBox.Items.Clear();
                payoutStockListBox.Items.AddRange(_cashAcceptor.GetPayoutStock().Select(x => new Channel { Denomination = x.denomination, Count = x.qty, MaxCount = x.maxQty }).ToArray());
            }));
        }

        private void UpdateCashboxLevels()
        {
            Invoke(new MethodInvoker(delegate ()
            {
                cashboxStockListBox.Items.Clear();
                cashboxStockListBox.Items.AddRange(_cashAcceptor.GetCashboxStock().Select(x => new Channel { Denomination = x.Key, Count = x.Value }).ToArray());
                cashboxStockListBox.Enabled = cashboxStockListBox.Items.Count > 0;
            }));
        }

        private void UpdateTotal()
        {
            Invoke(new MethodInvoker(delegate ()
            {
                string result = string.Empty;
                // Define all currencies
                List<Currency> detectedCurrencies =
                    _cashAcceptor.GetPayoutStock().GroupBy(x => x.denomination.Currency).Select(x=>x.Key).Distinct().ToList();

                detectedCurrencies.AddRange(
                    _cashAcceptor.GetCashboxStock().GroupBy(x => x.Key.Currency).Select(x => x.Key).Distinct());

                detectedCurrencies = detectedCurrencies.Distinct().ToList();

                foreach (Currency curr in detectedCurrencies)
                {
                    long atThePayout = _cashAcceptor.GetPayoutStock().Where(x => x.denomination.Currency == curr).Sum(x => x.denomination.Amount * x.qty);
                    long atTheCashbox = _cashAcceptor.GetCashboxStock().Where(x => x.Key.Currency == curr).Sum(x => x.Key.Amount * x.Value);

                    result += $"{new Denomination((uint)(atThePayout + atTheCashbox), curr)}; ";
                }

                totalTextBox.Text = result.Substring(0, (result.Length >=2 ? result.Length - 2: result.Length));
            }));
        }

        private ICashDevice RunITLSsp()
        {
            ICashDevice itl = new ITLCashValidator(setup => {
                setup.WithComPort(3).WithSspAddress(0)
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Receiving, CashValidatorIlluminationKind.Solid, Color.Green))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Idle, CashValidatorIlluminationKind.Solid, Color.FromArgb(0, 10, 0)))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Error, CashValidatorIlluminationKind.Solid, Color.FromArgb(180, 0, 0)))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Extracting, CashValidatorIlluminationKind.Solid, Color.Yellow))
                .SetMaxBillsInPayout(70)
                .WithBillUpperLimitInPayout(new Denomination(10, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(50, Currency.RussianRuble), 2)
                .WithBillUpperLimitInPayout(new Denomination(100, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(200, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(500, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(1000, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(2000, Currency.RussianRuble), 3)
                .WithBillUpperLimitInPayout(new Denomination(5000, Currency.RussianRuble), 3);
            });

            itl.OnEvent += (sender, e) => Invoke(new MethodInvoker(delegate () {
                if (richTextBox1 != null)
                    richTextBox1.Text = $"ITL {DateTime.Now.ToString("HH:mm:ss")} {e.Level.GetCode().ToLower()}: {e.Message} {Environment.NewLine}{richTextBox1.Text}";
            }));
            itl.OnDispensed += (sender, e) => UpdateBalance();
            itl.OnInserted += (sender, e) => UpdateBalance();

            itl.Run();
            return itl;
        }

        private ICashDevice RunJ2000()
        {
            ICashDevice itl = new J2000CoinMechanism(setup => {
                setup.WithRoutes(true, true)
                .WithChannel(2, new Denomination(1, Currency.RussianRuble), 5)
                .WithChannel(3, new Denomination(2, Currency.RussianRuble), 5)
                .WithChannel(4, new Denomination(5, Currency.RussianRuble), 5)
                .WithChannel(6, new Denomination(10, Currency.RussianRuble), 5);
            });
            
            itl.OnEvent += (sender, e) => Invoke(new MethodInvoker(delegate () {
                if (richTextBox1 != null)
                    richTextBox1.Text = $"J2000 {DateTime.Now.ToString("HH:mm:ss")} {e.Level.GetCode().ToLower()}: {e.Message} {Environment.NewLine}{richTextBox1.Text}";
            }));
            itl.OnDispensed += (sender, e)
                => UpdateBalance();
            itl.OnInserted += (sender, e)
                => UpdateBalance();

            itl.Run();
            return itl;
        }

        private void payout2CashboxButton_Click(object sender, EventArgs e)
        {
            _cashAcceptor.PushAllToCashBox();
            UpdateBalance();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            _cashAcceptor.Reset();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _cashAcceptor.Extract(new Denomination { Currency = Currency.RussianRuble, Amount = 2 }, 1);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            IDictionary<Denomination, CashRoute> routes = _cashAcceptor.GetRoutes();

            foreach (var x in routes)
            {
                richTextBox1.Text = $"{DateTime.Now.ToString("HH:mm:ss")} info: {x.Key} => {x.Value.GetCode()}{Environment.NewLine}{richTextBox1.Text}";
            }
        }
    }
}
