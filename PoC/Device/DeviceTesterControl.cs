using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Infrastructure.Abstractions.Enums;
using Filuet.Infrastructure.Abstractions.Helpers;
using Filuet.Infrastructure.Abstractions.Models;
using PoC.UIModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoC
{
    public partial class deviceTesterControl : UserControl
    {
        private Device _device;
        private ICashDevice _cashAcceptor;

        private Channel GetCurrentChannel
            => stockListBox.SelectedItem != null ? (Channel)stockListBox.SelectedItem : null;

        public ICashDevice CashDevice => _cashAcceptor;

        public deviceTesterControl()
        {
            InitializeComponent();
        }

        public void Initialize(Device device, ICashDevice cashAcceptor)
        {
            _device = device;
            _cashAcceptor = cashAcceptor;
            flushPayoutToolStripMenuItem.Visible = routesToolStripMenuItem.Visible = _device.Type == DeviceType.ITLSmartPayout;
        }

        public void PoCForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cashAcceptor != null)
                _cashAcceptor.Stop();
        }

        public void UpdateBalance()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);

                Invoke(new MethodInvoker(delegate ()
                {
                    stockListBox.Items.Clear();
                    stockListBox.Items.AddRange(_cashAcceptor.GetPayoutStock().Select(x => new Channel { Denomination = x.denomination, Count = x.qty, MaxCount = x.maxQty, Extractive = true }).ToArray());
                    stockListBox.Items.AddRange(_cashAcceptor.GetCashboxStock().Select(x => new Channel { Denomination = x.Key, Count = x.Value, Extractive = false }).ToArray());
                }));

                UpdateTotal();
            });
        }

        private void UpdateTotal()
        {
            Invoke(new MethodInvoker(delegate ()
            {
                string result = string.Empty;
                // Define all currencies
                List<Currency> detectedCurrencies =
                    _cashAcceptor.GetPayoutStock().GroupBy(x => x.denomination.Currency).Select(x => x.Key).Distinct().ToList();

                detectedCurrencies.AddRange(
                    _cashAcceptor.GetCashboxStock().GroupBy(x => x.Key.Currency).Select(x => x.Key).Distinct());

                detectedCurrencies = detectedCurrencies.Distinct().ToList();

                foreach (Currency curr in detectedCurrencies)
                {
                    decimal atThePayout = _cashAcceptor.GetPayoutStock().Where(x => x.denomination.Currency == curr).Sum(x => x.denomination.Amount * x.qty);
                    decimal atTheCashbox = _cashAcceptor.GetCashboxStock().Where(x => x.Key.Currency == curr).Sum(x => x.Key.Amount * x.Value);

                    result += $"{new Denomination((uint)(atThePayout + atTheCashbox), curr)}; ";
                }

                totalTextBox.Text = result.Substring(0, (result.Length >= 2 ? result.Length - 2 : result.Length));
            }));
        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            Channel toextract = GetCurrentChannel;
            if (toextract != null && toextract.Count > 0)
                _cashAcceptor.Extract(toextract.Denomination, 1);

            RefreshExtractButton();
        }

        private void RefreshExtractButton()
        {
            Channel toExtract = GetCurrentChannel;
            extractButton.Enabled = toExtract?.Count > 0 && toExtract.Extractive;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cashAcceptor.Reset();
        }

        private void flushPayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cashAcceptor.PushAllToCashBox(); // Move all money from payout to cashbox
            UpdateBalance();
        }

        private void routesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IDictionary<Denomination, CashRoute> routes = _cashAcceptor.GetRoutes();

            ////foreach (var x in routes)
            ////    richTextBox1.Text = $"{DateTime.Now.ToString("HH:mm:ss")} info: {x.Key} => {x.Value.GetCode()}{Environment.NewLine}{richTextBox1.Text}";
        }

        private void stockListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshExtractButton();
        }
    }
}