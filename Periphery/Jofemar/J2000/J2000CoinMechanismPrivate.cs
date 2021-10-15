using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums;
using FTD2XX_NET;
using System;
using System.Linq;
using System.Threading;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    public partial class J2000CoinMechanism
    {
        /// <summary>
        /// Establish connection with J2000
        /// </summary>
        /// <returns></returns>
        private FTDI RunFtdi()
        {
            FTDI result = new FTDI();

            uint deviceNo = 0;
            bool deviceExists = false;

            uint numOfDevices = 0;
            if (result.GetNumberOfDevices(ref numOfDevices) != FTDI.FT_STATUS.FT_OK || numOfDevices == 0)
            {
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Failed to open IPC device: ftdi driver can't get device count"));
                return null;
            }

            var devicesInfo = new FTDI.FT_DEVICE_INFO_NODE[numOfDevices];
            if (result.GetDeviceList(devicesInfo) != FTDI.FT_STATUS.FT_OK)
            {
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Failed to init FTDI device"));
                return null;
            }

            int i = -1;
            foreach (var node in devicesInfo)
            {
                i++;
                if (string.IsNullOrEmpty(node.Description) || node.Description.Length < 2)
                    continue;

                if (!node.Description.StartsWith("IPC") && !node.Description.StartsWith("VIP"))
                    continue;

                deviceNo = (uint)i;
                _info.Serial = node.SerialNumber;
                deviceExists = true;
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Info($"IPC device detected #{deviceNo}"));
                break;
            }

            string error = string.Empty;
            if (!deviceExists)
                error = "Unable to open IPC: device not found";

            if (result.OpenByIndex(deviceNo) != FTDI.FT_STATUS.FT_OK)
                error = "Unable to open IPC";

            // set port parameters
            if (result.SetBaudRate(19200) != FTDI.FT_STATUS.FT_OK)
                error = "Unable to set IPC baud rate";

            if (result.SetDataCharacteristics(8, 0, 0) != FTDI.FT_STATUS.FT_OK)
                error = "Unable to set IPC data characteristics";

            if (result.SetFlowControl(0, 0, 0) != FTDI.FT_STATUS.FT_OK)
                error = "Unable to set IPC flow control";

            if (result.SetTimeouts(1000, 0) != FTDI.FT_STATUS.FT_OK)
                error = "Unable to set IPC timeouts";


            if (!string.IsNullOrWhiteSpace(error))
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error($"Unable to open IPC: device not found"));
            else return result;

            return null;
        }

        private bool EnableAllChannels()
        {
            byte[] response;
            bool result = WriteAndRead((byte)JofemarIpcDeviceAddress.Device, J2000IpcCommands.SetCoinsEnable(_inhibitMask2, _inhibitMask1), out response);
            if (result && response.Length >= (int)JofemarIpcResponseLength.SetCoinsEnable &&
                (IpcControl)response[(int)IpcSimpleResponseIndex.Ack] == IpcControl.ACK)
                return true;

            OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Error during enabling ipc device"));
            return false;
        }

        private bool GetAllLevels()
        {
            byte[] response;
            var result = WriteAndRead((byte)JofemarIpcDeviceAddress.Device, J2000IpcCommands.GetCcChangerStatus, out response);

            if (!result)
            {
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Unable to get J2000 shanger status"));
                return false;
            }

            if (response.Length > J2000IpcCommands.ResponseCcChangerDataLenght)
            {
                if (response[4] != 0 || response[5] != 0)
                {
                    // Проверяем проблемные тубусы
                    int tubes = response[4] * 256 + response[5];
                    int x = tubes;
                    for (int i = 0; i <= J2000IpcCommands.CcChangerDataLenght; i++)
                    {
                        int y = x % 2;
                        if (y > 0 && response.Length >= (i + 6) && (response[i + 6] == 0))
                        {
                            OnEvent?.Invoke(this, CashAcceptorLogArgs.Error($"Some tubes are broken: {tubes:X}"));
                            State = CashDeviceState.Error;
                            return false;
                        }

                        if (y > 0)
                        {
                            var ch = _settings.Channels.FirstOrDefault(x => x.channelId == i);
                            OnEvent?.Invoke(this, CashAcceptorLogArgs.Error(_settings.Channels.Any(x => x.channelId == i) ? $"Tube #{ch.channelId} [{ch.nominal}] is full" : $"Tube {ch.channelId} do not exist in J2000 list"));
                        }

                        x /= 2;
                    }
                }

                _info.Channels.Clear();

                for (int i = 0; i < J2000IpcCommands.CcChangerDataLenght; i++)
                {
                    int pos = i + 6;
                    if (response[pos] == (byte)JofemarIpcTubeStatus.FullOrBad)
                        continue;

                    if (!_settings.Channels.Any(x => x.channelId == i))
                    {
                        if (response[pos] > 0)
                            OnEvent?.Invoke(this, CashAcceptorLogArgs.Warning($"Nominal {i} is disabled, but the tube count is {response[pos]}"));

                        continue;
                    }

                    DenominationChannel channel = new DenominationChannel { Channel = (byte)i, Level = response[pos], Nominal = _settings.Channels.FirstOrDefault(x=>x.channelId == i).nominal };
                    _info.Channels.Add(channel);
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Info($"Nominal {channel.Nominal} count is {channel.Level}"));
                }
            }

            return true;
        }

        private bool WriteReadAndParseResponse(byte deviceAddress, byte[] command)
        {
            if (WriteAndRead(deviceAddress, command, out byte[] response))
            {
                ParseResponse(deviceAddress, response);
                return true;
            }

            return false;
        }

        private bool WriteAndRead(byte deviceAddress, byte[] command, out byte[] response)
        {
            response = null;
            bool result = false;

            lock (_cmdLocker)
            {
                var cnt = 0;
                // Awake the device
                while (!(result = Write(deviceAddress, command)) && cnt < WakeUpCount)
                {
                    Thread.Sleep(_readDelay);
                    cnt++;
                }

                if (result)
                {
                    Thread.Sleep(_readDelay);
                    result = Read(deviceAddress, out response);
                    if (!result)
                    {
                        Thread.Sleep(_readDelay * 2);
                        result = Read(deviceAddress, out response);
                    }
                }
            }

            if (response == null)
                return false;

            return result;
        }

        /// <summary>
        /// DEVICE TO PC
        /// STX + DEVICE_ADDRESS + LENGTH + COMMAND + ... DATA ... + CHECKSUMM
        /// </summary>
        private bool Read(byte deviceAddress, out byte[] response)
        {
            uint readed = 1024;
            byte[] buffer = new byte[readed];
            FTD2XX_NET.FTDI.FT_STATUS result = _ftdi.Read(buffer, (uint)buffer.Length, ref readed);
            response = buffer.Take((int)readed).ToArray();
            // Maybe log ?
            return result == FTD2XX_NET.FTDI.FT_STATUS.FT_OK;
        }

        /// <summary>
        /// PC TO DEVICE: STX + DEVICE_ADDRESS + LENGTH + COMMAND + CHECKSUMM
        /// </summary>
        private bool Write(byte deviceAddress, byte[] command)
        {
            try
            {
                int checksumm = 0;
                byte[] buffer = new byte[command.Length + 4];
                int pos = 0;
                buffer[pos] = (byte)IpcControl.STX;
                checksumm = (int)buffer[pos];
                pos++; // stx (send test)
                buffer[pos] = deviceAddress;
                checksumm ^= (int)buffer[pos];
                pos++; // cnrt
                buffer[pos] = Convert.ToByte(command.Length + 4);
                checksumm ^= (int)buffer[pos];
                pos++; // length
                for (int i = 0; i < command.Length; i++)
                {
                    buffer[pos] = command[i];
                    checksumm ^= (int)buffer[pos];
                    pos++;
                }
                buffer[pos] = Convert.ToByte(checksumm);
                pos++;

                ////LogManager.Log(LogType, LogLevels.PeripheralCommunication, Module, "Write", "Write", buffer, 0, buffer.Length);

                // sending
                uint lenghtWritten = 0;
                _ftdi.Write(buffer, buffer.Length, ref lenghtWritten);
                return lenghtWritten == buffer.Length;
            }
            catch (Exception ex)
            {
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error(ex.Message));
                return false;
            }
        }

        private void ParseResponse(byte deviceAddress, byte[] response)
        {
            int counter = response[4];
            int messagesToParse = counter - _messageCounter;

            if (messagesToParse < 0)
                messagesToParse += 255;

            _messageCounter = counter;
            if (messagesToParse > J2000IpcCommands.PollCcMaxEventCount)
            {
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Info($"Messages to parse {messagesToParse} > 12"));
                return;
            }
            int pos = messagesToParse + 4;

            for (int i = 0; i < messagesToParse; i++)
            {
                int evnt = response[pos - i];
                if (evnt >= J2000IpcCommands.PollCcMinCreditChanel && evnt <= J2000IpcCommands.PollCcMaxCreditChanel)
                {
                    // credit accepted
                    int coinNumber = evnt - J2000IpcCommands.PollCcMinCreditChanel;
                    Denomination coin = new Denomination();
                    if (coinNumber >= 0)
                        coin = _settings.Channels.FirstOrDefault(x=> x.channelId == coinNumber).nominal;

                    switch ((JofemarIpcCoinEvents)response[pos - i - 1])
                    {
                        case JofemarIpcCoinEvents.RoutedToReject:
                            OnEvent?.Invoke(this, CashAcceptorLogArgs.Warning($"Coin was rejected {coin}"));
                            //// SaveStoredTubesCounters(CashDataEventTypes.REJECTED);
                            continue;
                        case JofemarIpcCoinEvents.DepositedIntoCashbox:
                            OnInserted?.Invoke(this, new CashAcceptorOnInsertedArgs { Inserted = coin });
                            if (!_info.CashboxStock.ContainsKey(coin))
                                _info.CashboxStock.Add(coin, 1);
                            else _info.CashboxStock[coin] += 1;
                            // save cashbox
                            break;
                        case JofemarIpcCoinEvents.DepositedIntoTubes:
                        default:
                            OnInserted?.Invoke(this, new CashAcceptorOnInsertedArgs { Inserted = coin });
                            ////SaveStoredTubesCounters(CashDataEventTypes.STORED_IN_PAYOUT);
                            break;
                    }
                    i++;
                    continue;
                }

                string error = string.Empty;
                switch ((IpcErrorStatus)evnt)
                {
                    case IpcErrorStatus.InternalEepromCorrupted:
                        error = "Controller internal eeprom corrupted";
                        break;
                    case IpcErrorStatus.OscillatorNotCalibrated:
                        error = "Controller oscillator not calibrated";
                        break;
                    case IpcErrorStatus.ChannelJammed:
                        error = "CC channel jammed";
                        //// SaveStoredTubesCounters(CashDataEventTypes.JAMMED);
                        break;
                    case IpcErrorStatus.PeripheralFailure:
                        error = "CC peripherial failure";
                        break;
                    case IpcErrorStatus.NoCommunicationToPeripheral:
                        OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("CC no communication to peripherial"));
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Error(error));
                    State = CashDeviceState.Error;
                }
            }
        }
    }
}
