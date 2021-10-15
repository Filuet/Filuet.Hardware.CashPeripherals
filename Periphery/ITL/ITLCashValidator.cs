using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common;
using Filuet.Hardware.CashAcceptors.Common.Helpers;
using Filuet.Hardware.CashAcceptors.Common.Ssp;
using ITLlib;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public partial class ITLCashValidator : ICashDevice
    {
        public event EventHandler<CashAcceptorOnInsertedArgs> OnInserted;
        public event EventHandler<CashAcceptorOnDispensedArgs> OnDispensed;
        public event EventHandler<CashAcceptorLogArgs> OnEvent;
        public event EventHandler<CashAcceptorResetArgs> OnReset;
        public event EventHandler<CashAcceptorOnFullCashbox> OnFullCashbox;

        private readonly ITLCashValidatorSettings _settings;
        private ITLCashValidatorInfo _info;
        private bool _isRunning = false;

        /// <summary>
        /// A pointer to the command structure, this struct is filled with info and then compiled into a packet by the library and sent to the validator
        /// </summary>
        private SSP_COMMAND _command { get; set; } = new SSP_COMMAND();

        /// <summary>
        /// Access to ssp variables the pointer which gives access to library functions such as open com port, send command etc
        /// </summary>
        private SSPComms SSPComms { get; set; } = new SSPComms();

        private SSP_COMMAND_INFO SSPCommangInfo { get; set; } = new SSP_COMMAND_INFO();


        /// <summary>
        /// A variable to hold the type of validator, this variable is initialised using the setup request command
        /// </summary>
        private char UnitType { get; set; } = (char)0xFF;

        /// <summary>
        /// Integer to hold total number of Hold messages to be issued before releasing note from escrow
        /// </summary>
        private int HoldNumber { get; set; } = 0;

        /// <summary>
        /// Bool to hold flag set to true if a note is being held in escrow
        /// </summary>
        private bool NoteHeld { get; set; } = false;

        /// <summary>
        /// Integer to hold number of hold messages still to be issued
        /// </summary>
        private int HoldCount = 0;

        private SSP_KEYS keys { get; set; } = new SSP_KEYS();

        public CashDeviceState State { get; private set; }

        public ITLCashValidator(Action<ITLCashValidatorSettingsBuilder> setupAction)
        {
            _settings = setupAction?.CreateTargetAndInvoke().Build();
            if (_settings == null)
                throw new InvalidOperationException("Device setup failed");
        }

        public async Task Run()
        {
            if (!ConnectAndSetup())
            {
                State = CashDeviceState.Disabled;
                throw new InvalidOperationException("Failed to connect to the device");
            }

            _isRunning = true;
            Illuminate(CashValidatorState.Idle);

            await Task.Factory.StartNew(() =>
            {
                while (_isRunning)
                {
                    // if the poll fails, try to reconnect
                    if (!DoPoll())
                    {
                        OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Poll failed, attempting to reconnect..."));

                        if (!ConnectAndSetup())
                        {
                            OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Failed to reconnect to validator"));
                            return;
                        }
                    }
                }

                Disconnect();
            });
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private bool ConnectAndSetup()
        {
            bool result = false;

            _command.ComPort = $"COM {_settings.ComPort}";
            _command.SSPAddress = _settings.SSPAddress;
            _command.Timeout = 3000;

            for (int i = 0; i < 3; i++)
            {
                SSPComms.CloseComPort(); // close com port in case it was open

                _command.EncryptionStatus = false; // turn encryption off for first stage

                // open com port and negotiate keys
                if (SSPComms.OpenSSPComPort(_command) && Handshake())
                {
                    _command.EncryptionStatus = true; // now encrypting

                    byte maxPVersion = FindMaxProtocolVersion(); // find the max protocol version this validator supports
                    if (maxPVersion > 6)
                        SetProtocolVersion(maxPVersion);
                    else
                    {
                        OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("This program does not support units under protocol version 6, update firmware"));
                        result = false;
                        break;
                    }

                    _info = ValidatorSetupRequest(); // Get info from the validator and store useful vars
                    SetInhibits(); // This sets which channels can receive notes
                    result = EnableValidator(); // This allows the validator to operate
                    SyncChannelLevels();
                    ReadCashboxLevels();
                }

                if (result)
                    break;

                Thread.Sleep(1000);
            }

            return result;
        }

        private bool Disconnect()
        {
            bool result = DisableValidator() || SSPComms.CloseComPort(); // If one of the actions is successful

            if (result)
                ExtinguishIllumination();

            return result;
        }

        /// <summary>
        /// The enable command allows the validator to receive and act on commands sent to it
        /// </summary>
        /// <returns></returns>
        private bool EnableValidator()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_ENABLE;
                _command.CommandDataLength = 1;

                return SendCommand();
            }
        }

        /// <summary>
        /// Disable command stops the validator from acting on commands
        /// </summary>
        /// <returns></returns>
        private bool DisableValidator()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_DISABLE;
                _command.CommandDataLength = 1;

                return SendCommand();
            }
        }

        /// <summary>
        /// Return Note command returns note held in escrow to bezel
        /// </summary>
        public void ReturnNote()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_REJECT_BANKNOTE;
                _command.CommandDataLength = 1;

                if (SendCommand())
                    HoldCount = 0;
            }
        }

        /// <summary>
        /// The reset command instructs the validator to restart (same effect as switching on and off)
        /// </summary>
        public void Reset()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_RESET;
                _command.CommandDataLength = 1;
                if (SendCommand())
                    OnReset?.Invoke(this, new CashAcceptorResetArgs());
            }
        }

        public void Extract(Denomination bill, uint quantity)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_PAYOUT_BY_DENOMINATION;
                _command.CommandData[1] = 1;

                _command.CommandDataLength = 12;

                Array.Copy(BitConverter.GetBytes(quantity), 0, _command.CommandData, 2, 2);
                Array.Copy(BitConverter.GetBytes(Convert.ToInt32(bill.Amount * _info.ValueMultiplier)), 0, _command.CommandData, 4, 4);
                Array.Copy(Encoding.ASCII.GetBytes(bill.Currency.GetCode()), 0, _command.CommandData, 8, 3);
                _command.CommandData[11] = 0x58;

                SendCommand(); // Do not send OnDispensed in this procedure- onpoll will send it
            }
        }

        /// <summary>
        /// Empty payout device takes all the notes stored and moves them to the cashbox
        /// </summary>
        public void PushAllToCashBox()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_EMPTY_ALL;
                _command.CommandDataLength = 1;

                SendCommand();
            }

            // it may hang
            //lock (_command)
            //{
            //    _command.CommandData[0] = SspCommand.SSP_CMD_SMART_EMPTY;
            //    _command.CommandDataLength = 1;

            //    SendCommand();
            //}
        }

        /// <summary>
        /// Get the serial number of the device
        /// An optional Device parameter can be used for TEBS systems to specify which device's serial number should be returned
        /// </summary>
        /// <param name="device">0x00 = NV200, 0x01 = SMART Payout, 0x02 = Tamper Evident Cash Box</param>
        /// <returns></returns>
        public string GetSerialNumber(byte? device = null)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_GET_SERIAL_NUMBER;
                _command.CommandDataLength = 1;
                if (device.HasValue)
                {
                    _command.CommandData[1] = device.Value;
                    _command.CommandDataLength = 2;
                }

                if (!SendCommand())
                    return string.Empty;

                Array.Reverse(_command.ResponseData, 1, 4); // Response data is big endian, so reverse bytes 1 to 4
                return BitConverter.ToUInt32(_command.ResponseData, 1).ToString();
            }
        }

        /// <summary>
        /// Get available denominations to extract with theirs quantity
        /// </summary>
        /// <returns></returns>
        public ICollection<(Denomination denomination, ushort qty, ushort maxQty)> GetPayoutStock() => new PayoutStock()
            .Populate(x =>
            {
                foreach (var u in _info.UnitDataList)
                    x.Populate(u.Nominal, u.Level, _settings.DenominationMaxQuantityInPayout.FirstOrDefault(x => x.bill == u.Nominal).qty);
            }).Stock;

        public IDictionary<Denomination, ushort> GetCashboxStock() => _info.CashboxStock;

        public IDictionary<Denomination, CashRoute> GetRoutes()
        {
            Dictionary<Denomination, CashRoute> result = new Dictionary<Denomination, CashRoute>();

            foreach (var x in _info.UnitDataList)
            {
                result.Add(x.Nominal, GetDenominationRoute(x.Nominal));
            }

            return result;
        }
    }
}