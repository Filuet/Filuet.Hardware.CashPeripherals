using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Periphery.ITL;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000;
using Filuet.Infrastructure.Abstractions.Enums;
using Filuet.Infrastructure.Abstractions.Helpers;
using Filuet.Infrastructure.Abstractions.Models;
using PoC.UIModels;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PoC
{
    public partial class DeviceSelector : UserControl
    {
        private DeviceType? _currentlySelectedSevice;

        private Device GetCurrentDevice
            => deviceComboBox.SelectedItem != null ? (Device)deviceComboBox.SelectedItem : null;

        public DeviceSelector()
        {
            InitializeComponent();
        }

        private void DeviceSelector_Load(object sender, System.EventArgs e)
        {
            deviceComboBox.Items.AddRange(EnumHelpers.GetValues<DeviceType>().Select(x => new Device { Type = x }).ToArray());
        }

        private void deviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Device selectedDevice = deviceComboBox.SelectedItem as Device;

            if (deviceTesterControl1.CashDevice != null && selectedDevice.Type != _currentlySelectedSevice)
                deviceTesterControl1.CashDevice.Stop();

            if (selectedDevice.Type == _currentlySelectedSevice)
                return;

            ICashDevice newDevice = null;
            switch (selectedDevice.Type)
            {
                case DeviceType.ITLSmartPayout:
                     newDevice = RunITLSmartPayout();
                    break;
                case DeviceType.ITLSmartHopper:
                    newDevice = RunITLSmartHopper();
                    break;
                case DeviceType.J2000:
                    newDevice = RunJ2000();
                    break;
                default:
                    break;
            }

            if (newDevice != null)
            {
                deviceTesterControl1.Initialize(selectedDevice, newDevice);

                _currentlySelectedSevice = selectedDevice.Type;

                if (deviceTesterControl1.CashDevice.State == CashDeviceState.OK)
                    deviceTesterControl1.UpdateBalance();
                else MessageBox.Show($"{_currentlySelectedSevice.Value.GetCode()} is currently disabled");
            }
        }

        private ICashDevice RunITLSmartPayout()
        {
            ICashDevice itl = new ITLCashValidator(setup =>
            {
                setup.WithComPort(3).WithSspAddress(0)
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Receiving, CashValidatorIlluminationKind.Solid, Color.Green))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Idle, CashValidatorIlluminationKind.Solid, Color.FromArgb(0, 10, 0)))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Error, CashValidatorIlluminationKind.Solid, Color.FromArgb(180, 0, 0)))
                .WithIlluminationMode(CashValidatorIlluminationMode.New(CashValidatorState.Extracting, CashValidatorIlluminationKind.Solid, Color.Yellow))
                .SetMaxBillsInPayout(70)
                .WithDenominationUpperLimitInPayout(new Denomination(10, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(50, Currency.RussianRuble), 2)
                .WithDenominationUpperLimitInPayout(new Denomination(100, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(200, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(500, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(1000, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(2000, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(5000, Currency.RussianRuble), 3);
            });

            itl.OnEvent += (sender, e) => Invoke(new MethodInvoker(delegate ()
            {
                if (richTextBox1 != null)
                    richTextBox1.Text = $"ITL {DateTime.Now.ToString("HH:mm:ss")} {e.Level.GetCode().ToLower()}: {e.Message} {Environment.NewLine}{richTextBox1.Text}";
            }));
            itl.OnDispensed += (sender, e) => deviceTesterControl1.UpdateBalance();
            itl.OnInserted += (sender, e) => deviceTesterControl1.UpdateBalance();

            itl.Run();
            return itl;
        }

        /// <summary>
        /// Coins. Looks like a coffee machine
        /// </summary>
        /// <returns></returns>
        private ICashDevice RunITLSmartHopper()
        {
            ICashDevice itl = new ITLCashValidator(setup =>
            {
                setup.WithComPort(3).WithSspAddress(16)
                .SetMaxBillsInPayout(70)
                .WithDenominationUpperLimitInPayout(new Denomination(10, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(50, Currency.RussianRuble), 2)
                .WithDenominationUpperLimitInPayout(new Denomination(100, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(200, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(500, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(1000, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(2000, Currency.RussianRuble), 3)
                .WithDenominationUpperLimitInPayout(new Denomination(5000, Currency.RussianRuble), 3);
            });

            itl.OnEvent += (sender, e) => Invoke(new MethodInvoker(delegate ()
            {
                if (richTextBox1 != null)
                    richTextBox1.Text = $"ITL {DateTime.Now.ToString("HH:mm:ss")} {e.Level.GetCode().ToLower()}: {e.Message} {Environment.NewLine}{richTextBox1.Text}";
            }));
            itl.OnDispensed += (sender, e) => deviceTesterControl1.UpdateBalance();
            itl.OnInserted += (sender, e) => deviceTesterControl1.UpdateBalance();

            itl.Run();
            return itl;
        }

        private ICashDevice RunJ2000()
        {
            ICashDevice j2000 = new J2000CoinMechanism(setup =>
            {
                setup.WithRoutes(true, true)
                .WithChannel(2, new Denomination(1, Currency.RussianRuble), 5)
                .WithChannel(3, new Denomination(2, Currency.RussianRuble), 5)
                .WithChannel(4, new Denomination(5, Currency.RussianRuble), 5)
                .WithChannel(6, new Denomination(10, Currency.RussianRuble), 5);
            });

            j2000.OnEvent += (sender, e) => Invoke(new MethodInvoker(delegate ()
            {
                if (richTextBox1 != null)
                    richTextBox1.Text = $"J2000 {DateTime.Now.ToString("HH:mm:ss")} {e.Level.GetCode().ToLower()}: {e.Message} {Environment.NewLine}{richTextBox1.Text}";
            }));

            j2000.OnDispensed += (sender, e) => deviceTesterControl1.UpdateBalance();
            j2000.OnInserted += (sender, e) => deviceTesterControl1.UpdateBalance();

            j2000.Run();
            return j2000;
        }

        public void PoCForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            deviceTesterControl1.PoCForm_FormClosing(sender, e);
        }
    }
}