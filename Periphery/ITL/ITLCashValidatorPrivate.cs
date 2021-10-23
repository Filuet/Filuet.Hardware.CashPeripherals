using Filuet.Hardware.CashAcceptors.Abstractions.Converters;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common.Ssp;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using Filuet.Infrastructure.Abstractions.Enums;
using Filuet.Infrastructure.Abstractions.Helpers;
using ITLlib;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public partial class ITLCashValidator
    {
        private Denomination? _creditEvent;
        //private string GetCounters()
        //{
        //    lock (_command)
        //    {
        //        _command.CommandData[0] = SspCommand.SSP_CMD_GET_COUNTERS;
        //        _command.CommandDataLength = 1;
        //        if (SendCommand() && _command.ResponseData.Length == 22)
        //        {
        //            uint counterStacked = BitConverter.ToUInt32(_command.ResponseData, 2);
        //            uint counterStored = BitConverter.ToUInt32(_command.ResponseData, 6);
        //            uint counterDispensed = BitConverter.ToUInt32(_command.ResponseData, 10);
        //            uint counterTransferredToStack = BitConverter.ToUInt32(_command.ResponseData, 14);

        //            return $"{counterStacked}/{counterStored}/{counterDispensed}/{counterTransferredToStack}";
        //        }

        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// Keys exchange
        /// </summary>
        /// <remarks>Performs a number of commands in order to setup the encryption between the host and the validator</remarks>
        /// <returns></returns>
        private bool Handshake()
        {
            lock (_command)
            {
                _command.EncryptionStatus = false; // Make sure encryption is off

                Func<byte, bool, bool> _composeCommandAndSend = (x, withBitConversion) =>
                {
                    _command.CommandData[0] = x;
                    _command.CommandDataLength = (byte)(withBitConversion ? 9 : 1);
                    if (withBitConversion)
                    {
                        if (x == SspCommand.SSP_CMD_SET_GENERATOR)
                            BitConverter.GetBytes(keys.Generator).CopyTo(_command.CommandData, 1);
                        else if (x == SspCommand.SSP_CMD_SET_MODULUS)
                            BitConverter.GetBytes(keys.Modulus).CopyTo(_command.CommandData, 1);
                        else if (x == SspCommand.SSP_CMD_REQUEST_KEY_EXCHANGE)
                            BitConverter.GetBytes(keys.HostInter).CopyTo(_command.CommandData, 1);
                    }

                    if (!SendCommand())
                    {
                        OnEvent!.Invoke(this, CashAcceptorLogArgs.Error($"Handshake failed at {x}"));
                        return false;
                    }

                    return true;
                };

                if (!_composeCommandAndSend(SspCommand.SSP_CMD_SYNC, false))
                    return false;

                SSPComms.InitiateSSPHostKeys(keys, _command);

                if (!_composeCommandAndSend(SspCommand.SSP_CMD_SET_GENERATOR, true)
                    || !_composeCommandAndSend(SspCommand.SSP_CMD_SET_MODULUS, true)
                    || !_composeCommandAndSend(SspCommand.SSP_CMD_REQUEST_KEY_EXCHANGE, true))
                    return false;

                keys.SlaveInterKey = BitConverter.ToUInt64(_command.ResponseData, 1); // Read slave intermediate key

                SSPComms.CreateSSPHostEncryptionKey(keys);

                _command.Key.FixedKey = 0x0123456701234567; // get full encryption key
                _command.Key.VariableKey = keys.KeyHost;

                return true;
            }
        }

        /// <summary>
        /// Uses the setup request command to get all the information about the validator
        /// </summary>
        private ITLCashValidatorInfo ValidatorSetupRequest()
        {
            ITLCashValidatorInfo result = new ITLCashValidatorInfo();

            lock (_command)
            {
                // send setup request
                _command.CommandData[0] = SspCommand.SSP_CMD_SETUP_REQUEST;
                _command.CommandDataLength = 1;

                if (!SendCommand())
                    return null;

                // unit type
                uint index = 1;
                UnitType = (char)_command.ResponseData[index++];
                switch (UnitType)
                {
                    case (char)0x00: result.Type = "Validator"; break;
                    case (char)0x03: result.Type = "SMART Hopper"; break;
                    case (char)0x06: result.Type = "SMART Payout"; break;
                    case (char)0x07: result.Type = "NV11"; break;
                    case (char)0x0D: result.Type = "TEBS"; break;
                    default: result.Type = "Unknown"; break;
                }

                result.Firmware = $"{(char)_command.ResponseData[index++]}{(char)_command.ResponseData[index++]}.{(char)_command.ResponseData[index++]}{(char)_command.ResponseData[index++]}";

                index += 3; // Country code (it's obsolete, so skip it)
                index += 3; // Value multiplier (it's obsolete, so skip it)

                // Number of channels
                result.NumberOfChannels = _command.ResponseData[index++];

                index += result.NumberOfChannels; // Channel values (it's obsolete, so skip it)
                index += result.NumberOfChannels; // Channel security (it's obsolete, so skip it)

                result.ValueMultiplier = _command.ResponseData[index + 2]; // Real value multiplier (big endian)
                result.ValueMultiplier += (ushort)(_command.ResponseData[index + 1] << 8);
                result.ValueMultiplier += (ushort)(_command.ResponseData[index] << 16);

                index += 3;

                result.Protocol = _command.ResponseData[index++]; // Protocol version

                result.UnitDataList.Clear();

                for (byte i = 0; i < result.NumberOfChannels; i++) // Loop through all channels and add channel data to list
                    result.UnitDataList.Add(new ITLChannel
                    {
                        Channel = (byte)(i + 1), // Number
                        Nominal = new Denomination
                        {
                            Amount = (uint)BitConverter.ToInt32(_command.ResponseData, (int)(index + (result.NumberOfChannels * 3) + (i * 4))) * result.ValueMultiplier,
                            Currency = EnumHelpers.GetValueFromCode<Currency>(new string(new char[] { (char)_command.ResponseData[index + (i * 3)], (char)_command.ResponseData[(index + 1) + (i * 3)], (char)_command.ResponseData[(index + 2) + (i * 3)] }))
                        },
                        Multiplier = result.ValueMultiplier
                    });
            }

            string serial = GetSerialNumber();
            if (string.IsNullOrWhiteSpace(serial))
            {
                int index = 0;
                while (index < 3)
                {
                    serial = GetSerialNumber();
                    if (!string.IsNullOrWhiteSpace(serial))
                        break;
                    index++;
                }
            }

            result.SerialNumber = serial.Trim();
            result.CachePath = $"{Directory.GetCurrentDirectory()}/cache";

            return result;
        }

        private void SyncChannelLevels()
        {
            if (_info == null)
                throw new Exception("Bind the settings of the device first");

            _info.UnitDataList = _info.UnitDataList.Select(x => { x.Level = GetNoteLevel(x.Nominal); return x; }).OrderBy(x => x.Nominal.Amount).ToList();

            foreach (var ch in _info.UnitDataList)
                SetDenominationRoute(ch.Nominal, ch.Level >= _settings.GetDenominationMaxQuantityInPayout(ch.Nominal) ? CashRoute.Stacker : CashRoute.Payout);
        }

        /// <summary>
        /// Get saved cashbox levels from cache file
        /// </summary>
        private void ReadCashboxLevels()
        {
            if (!Directory.Exists(_info.CachePath))
                Directory.CreateDirectory(_info.CachePath);

            string cacheFile = $"{_info.CachePath}/{_info.SerialNumber}.json";

            if (!string.IsNullOrWhiteSpace(_info.SerialNumber) && File.Exists(cacheFile))
            {
                try
                {
                    string cached = File.ReadAllText(cacheFile);

                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new BillJsonConverter());

                    StockCacheDto cache = JsonSerializer.Deserialize<StockCacheDto>(cached, options);
                    if (cache != null)
                    {
                        _info.CashboxStock = cache.ToDictionary(x => x.Note, y => y.Quantity);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Error($"Failed to read cache data of the device: {ex.Message}"));
                    return;
                }
            }

            OnEvent?.Invoke(this, CashAcceptorLogArgs.Warning($"Unable to find cache data of the device"));
        }

        /// <summary>
        /// Sends the set inhibits command to set the inhibits on the validator
        /// An additional two bytes are sent along with the command byte to indicate the status of the inhibits on the channels
        /// </summary>
        /// <example>
        /// 0xFF and 0xFF in binary is 11111111 11111111. This indicates all 16 channels supported by the validator are uninhibited
        /// If a user wants to inhibit channels 8-16, they would send 0x00 and 0xFF
        /// </example>
        private void SetInhibits()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_SET_CHANNEL_INHIBITS;
                _command.CommandData[1] = 0xFF;
                _command.CommandData[2] = 0xFF;
                _command.CommandDataLength = 3;

                SendCommand();
            }
        }

        /// <summary>
        /// The poll function is called repeatedly to poll to validator for information, it returns as a response in the command structure what events are currently happening
        /// </summary>
        /// <returns></returns>
        private bool DoPoll()
        {
            if (HoldCount > 0) // If a not is to be held in escrow, send hold commands, as poll releases note
            {
                NoteHeld = true;
                HoldCount--;
                lock (_command)
                {
                    _command.CommandData[0] = SspCommand.SSP_CMD_HOLD;
                    _command.CommandDataLength = 1;
                    return SendCommand();
                }
            }

            // store response locally so data can't get corrupted by other use of the cmd variable
            byte[] response = new byte[255];
            byte responseLength = 0;
            Action postAction = null;

            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_POLL; // Send poll
                _command.CommandDataLength = 1;
                NoteHeld = false;

                if (!SendCommand())
                    return false;

                if (_command.ResponseData[0] == 0xFA)
                    return false;

                _command.ResponseData.CopyTo(response, 0);
                responseLength = _command.ResponseDataLength;
            }

            string info = string.Empty;
            string warning = string.Empty;
            string error = string.Empty;

            for (byte i = 1; i < responseLength; ++i) // parse poll response
            {
                switch (response[i])
                {
                    // This response indicates that the unit was reset and this is the first time a poll
                    // has been called since the reset.
                    case SspCommand.SSP_POLL_SLAVE_RESET:
                        SyncChannelLevels();
                        warning = "Unit reset";
                        break;
                    // The validator is disabled, it will not execute any commands or do any actions until enabled.
                    case SspCommand.SSP_POLL_DISABLED:
                        warning = "Unit disabled";
                        return false;
                    // A note is currently being read by the validator sensors. The second byte of this response
                    // is zero until the note's type has been determined, it then changes to the channel of the 
                    // scanned note.
                    case SspCommand.SSP_POLL_READ_NOTE:
                        if (response[i + 1] > 0)
                        {
                            info = $"Note in escrow, amount: {_info.GetBillByChannel(response[i + 1])}";
                            HoldCount = HoldNumber;
                        }
                        else
                            info = "Reading note...";
                        i++;
                        break;
                    // A credit event has been detected, this is when the validator has accepted a note as legal currency.
                    case SspCommand.SSP_POLL_CREDIT_NOTE:
                        byte channel = response[i + 1];
                        _creditEvent = _info.UnitDataList.FirstOrDefault(x => x.Channel == channel)?.Nominal;
                        if (_creditEvent.HasValue)
                        {
                            info = $"Credit {_creditEvent.Value}";
                            OnInserted?.Invoke(this, new CashAcceptorOnInsertedArgs { Inserted = _creditEvent.Value });
                        }
                        i++;
                        break;
                    // A note is being rejected from the validator. This will carry on polling while the note is in transit.
                    case SspCommand.SSP_POLL_NOTE_REJECTING:
                        warning = "Rejecting note...";
                        break;
                    // A note has been rejected from the validator, the note will be resting in the bezel. This response only
                    // appears once.
                    case SspCommand.SSP_POLL_NOTE_REJECTED:
                        warning = $"Note rejected: {GetQueryRejectionReason()}";
                        break;
                    // A note is in transit to the cashbox
                    case SspCommand.SSP_POLL_NOTE_STACKING:
                        info = "Stacking note...";
                        break;
                    // The payout device is 'floating' a specified amount of notes. It will transfer some to the cashbox and leave the specified amount in the payout device
                    case SspCommand.SSP_POLL_FLOATING:
                        info = "Floating notes";
                        // Now the index needs to be moved on to skip over the data provided by this response so it is not parsed as a normal poll response.
                        // In this response, the data includes the number of countries being floated (1 byte), then a 4 byte value and 3 byte currency code for each country
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // A note has reached the cashbox
                    case SspCommand.SSP_POLL_NOTE_STACKED:
                        if (_creditEvent.HasValue)
                        {
                            OnNominalMoved(_creditEvent.Value, true, CashRoute.Stacker);
                            _creditEvent = null;
                        }
                        info = "Note stacked";
                        break;
                    // The float operation has been completed
                    case SspCommand.SSP_POLL_FLOATED:
                        info = "Completed floated";
                        SyncChannelLevels();
                        EnableValidator();
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // A note has been stored in the payout device to be paid out instead of going into the cashbox
                    case SspCommand.SSP_POLL_NOTE_STORED_IN_PAYOUT:
                        if (_creditEvent.HasValue)
                        {
                            OnNominalMoved(_creditEvent.Value, true, CashRoute.Payout);
                            _creditEvent = null;
                        }
                        info = "Note stored";
                        break;
                    // A safe jam has been detected. This is where the user has inserted a note and the note is jammed somewhere that the user cannot reach
                    case SspCommand.SSP_POLL_SAFE_NOTE_JAM:
                        error = "Safe jam";
                        break;
                    // An unsafe jam has been detected. This is where a user has inserted a note and the note is jammed somewhere that the user can potentially recover the note from
                    case SspCommand.SSP_POLL_UNSAFE_NOTE_JAM:
                        error = "Unsafe jam";
                        break;
                    // An error has been detected by the payout unit
                    case SspCommand.SSP_POLL_ERROR_DURING_PAYOUT: // Note: Will be reported only when Protocol version >= 7
                        info = "Detected error with payout device";
                        i += (byte)((response[i + 1] * 7) + 2);
                        break;
                    // A fraud attempt has been detected. The second byte indicates the channel of the note that a fraud has been attempted on
                    case SspCommand.SSP_POLL_FRAUD_ATTEMPT:
                        error = $"Fraud attempt, note type: {_info.GetBillByChannel(response[i + 1])}";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // The stacker (cashbox) is full
                    case SspCommand.SSP_POLL_STACKER_FULL:
                        OnFullCashbox?.Invoke(this, new CashAcceptorOnFullCashbox());
                        error = "Stacker full";
                        break;
                    // A note was detected somewhere inside the validator on startup and was rejected from the front of the unit
                    case SspCommand.SSP_POLL_NOTE_CLEARED_FROM_FRONT:
                        error = "Note cleared from front at reset";
                        i++;
                        break;
                    // A note was detected somewhere inside the validator on startup and was cleared into the cashbox
                    case SspCommand.SSP_POLL_NOTE_CLEARED_TO_CASHBOX:
                        error = "Note cleared to stacker at reset";
                        i++;
                        break;
                    // The cashbox has been removed from the unit. This will continue to poll until the cashbox is replaced
                    case SspCommand.SSP_POLL_CASHBOX_REMOVED:
                        error = "Cashbox removed";
                        break;
                    // The cashbox has been replaced, this will only display on a poll once
                    case SspCommand.SSP_POLL_CASHBOX_REPLACED:
                        warning = "Cashbox replaced";
                        break;
                    // The validator is in the process of paying out a note, this will continue to poll until the note has been fully dispensed and removed from the front of the validator by the user
                    case SspCommand.SSP_POLL_DISPENSING:
                        info = "Dispensing note(s)";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // The note has been dispensed and removed from the bezel by the user.
                    case SspCommand.SSP_POLL_DISPENSED:
                        SyncChannelLevels();
                        info = "Note(s) dispensed";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // The payout device is in the process of emptying all its stored notes to the cashbox. This will continue to poll until the device is empty
                    case SspCommand.SSP_POLL_EMPTYING:
                        info = "Emptying";
                        break;
                    // This single poll response indicates that the payout device has finished emptying.
                    case SspCommand.SSP_POLL_EMPTIED:
                        info = "Emptied";
                        SyncChannelLevels();
                        EnableValidator();
                        break;
                    // The payout device is in the process of SMART emptying all its stored notes to the cashbox, keeping a count of the notes emptied. This will continue to poll until the device is empty
                    case SspCommand.SSP_POLL_SMART_EMPTYING:
                        info = "SMART Emptying";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // The payout device has finished SMART emptying, the information of what was emptied can now be displayed
                    // using the CASHBOX PAYOUT OPERATION DATA command.
                    case SspCommand.SSP_POLL_SMART_EMPTIED:
                        info = "SMART Emptied, getting info...";
                        SyncChannelLevels();
                        //GetCashboxPayoutOpData(log);
                        EnableValidator();
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // The payout device has encountered a jam. This will not clear until the jam has been removed and the unit reset
                    case SspCommand.SSP_POLL_JAMMED:
                        info = "Unit jammed...";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // This is reported when the payout has been halted by a host command. This will report the value of currency dispensed upto the point it was halted
                    case SspCommand.SSP_POLL_HALTED:
                        info = "Halted...";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    // This is reported when the payout was powered down during a payout operation. It reports the original amount requested and the amount paid out up to this point for each currency
                    case SspCommand.SSP_POLL_INCOMPLETE_PAYOUT:
                        error = "Incomplete payout";
                        i += (byte)((response[i + 1] * 11) + 1);
                        break;
                    // This is reported when the payout was powered down during a float operation. It reports the original amount requested and the amount paid out up to this point for each currency
                    case SspCommand.SSP_POLL_INCOMPLETE_FLOAT:
                        error = "Incomplete float";
                        i += (byte)((response[i + 1] * 11) + 1);
                        break;
                    // A note has been transferred from the payout unit to the stacker
                    case SspCommand.SSP_POLL_NOTE_TRANSFERED_TO_STACKER:
                        error = "Note transferred to stacker";
                        i += 7;
                        break;
                    // A note is resting in the bezel waiting to be removed by the user
                    case SspCommand.SSP_POLL_NOTE_HELD_IN_BEZEL:
                        info = "Note in bezel...";
                        i += 7;
                        break;
                    // The payout has gone out of service, the host can attempt to re-enable the payout by sending the enable payout command
                    case SspCommand.SSP_POLL_PAYOUT_OUT_OF_SERVICE:
                        info = "Payout out of service...";
                        break;
                    // The unit has timed out while searching for a note to payout. It reports the value dispensed before the timeout event
                    case SspCommand.SSP_POLL_TIME_OUT:
                        error = "Timed out searching for a note";
                        i += (byte)((response[i + 1] * 7) + 1);
                        break;
                    case SspCommand.SSP_POLL_NOTE_PATH_OPEN:
                        error = "Note path open";
                        break;
                    // All channels on the validator have been inhibited so the validator is disabled. Only available on protocol versions 7 and above
                    case SspCommand.SSP_POLL_CHANNEL_DISABLE:
                        error = "All channels inhibited, unit disabled";
                        break;
                    default:
                        error = $"Unrecognised poll response detected {(int)response[i]}";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(info))
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Info(info));
                if (!string.IsNullOrWhiteSpace(warning))
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Warning(warning));
                if (!string.IsNullOrWhiteSpace(error))
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Error(error));
            }

            postAction?.Invoke();

            return true;
        }

        private void OnNominalMoved(Denomination note, bool isCredit, CashRoute route)
        {
            _info.UnitDataList = _info.UnitDataList.Select(x =>
            {
                if (x.Nominal == note)
                {
                    if (isCredit)
                    {
                        if (route == CashRoute.Payout) // Level is payout property only
                            x.Level++;
                        else
                        {
                            if (_info.CashboxStock.ContainsKey(note))
                                _info.CashboxStock[note]++;
                            else _info.CashboxStock.Add(note, 1);

                            // save cache to the disk
                            if (!Directory.Exists(_info.CachePath))
                                Directory.CreateDirectory(_info.CachePath);

                            string cacheFile = $"{_info.CachePath}/{_info.SerialNumber}.json";

                            if (!string.IsNullOrWhiteSpace(_info.SerialNumber))
                            {
                                JsonSerializerOptions options = new JsonSerializerOptions();
                                options.Converters.Add(new BillJsonConverter());
                                File.WriteAllText(cacheFile, JsonSerializer.Serialize(new StockCacheDto(_info.CashboxStock), options));
                            }
                        }
                    }

                    if (!isCredit)
                        x.Level--;

                    if (isCredit && route != CashRoute.Stacker && x.Level >= _settings.GetDenominationMaxQuantityInPayout(x.Nominal))
                    {
                        int index = 0;
                        while (!SetDenominationRoute(x.Nominal, CashRoute.Stacker) && index < 5)
                        {
                            index++;
                            Thread.Sleep(200);
                        }
                    }

                    if (!isCredit && route != CashRoute.Payout && x.Level < _settings.GetDenominationMaxQuantityInPayout(x.Nominal))
                    {
                        int index = 0;
                        while (!SetDenominationRoute(x.Nominal, CashRoute.Payout) && index < 5)
                        {
                            index++;
                            Thread.Sleep(200);
                        }
                    }
                }

                return x;
            }).ToList();
        }

        private bool SendCommand()
        {
            byte[] backup = new byte[255]; // Backup data and length in case we need to retry
            _command.CommandData.CopyTo(backup, 0);
            _command.ResponseData = new byte[255];

            if (!SSPComms.SSPSendCommand(_command, SSPCommangInfo))
            {
                SSPComms.CloseComPort();
                return false;
            }

            if (_command.ResponseData[0] == SspCommand.SSP_RESPONSE_OK)
                return true;

            string error = $"Unknown error {_command.ResponseData[0]}";

            switch (_command.ResponseData[0])
            {
                case SspCommand.SSP_RESPONSE_COMMAND_CANNOT_BE_PROCESSED:
                    error = _command.ResponseData[1] == 0x03 ?
                        "Validator has responded with 'Busy', command cannot be processed at this time" :
                        $"Command response is CANNOT PROCESS COMMAND, error code - 0x{BitConverter.ToString(_command.ResponseData, 1, 1)}";
                    break;
                case SspCommand.SSP_RESPONSE_FAIL:
                    error = "Command response is FAIL";
                    break;
                case SspCommand.SSP_RESPONSE_KEY_NOT_SET:
                    error = "Command response is KEY NOT SET, Validator requires encryption on this command or there is a problem with the encryption on this request";
                    break;
                case SspCommand.SSP_RESPONSE_PARAMETER_OUT_OF_RANGE:
                    error = "Command response is PARAM OUT OF RANGE";
                    break;
                case SspCommand.SSP_RESPONSE_SOFTWARE_ERROR:
                    error = "Command response is SOFTWARE ERROR";
                    break;
                case SspCommand.SSP_RESPONSE_COMMAND_NOT_KNOWN:
                    error = "Command response is UNKNOWN";
                    break;
                case SspCommand.SSP_RESPONSE_WRONG_NO_PARAMETERS:
                    error = "Command response is WRONG PARAMETERS";
                    break;
                default:
                    break;
            }

            OnEvent?.Invoke(this, CashAcceptorLogArgs.Error(error));
            return false;
        }

        private byte FindMaxProtocolVersion()
        {
            byte b = 0x06;
            while (true) // not dealing with protocol under level 6. Attempt to set in validator
            {
                SetProtocolVersion(b);
                if (_command.ResponseData[0] == SspCommand.SSP_RESPONSE_FAIL)
                    return --b;
                b++;
                if (b > 20)
                    return 0x06; // return default if protocol 'runs away'
            }
        }

        /// <summary>
        /// This function sets the protocol version in the validator to the version passed across.
        /// Whoever calls this needs to check the response to make sure the version is supported
        /// </summary>
        /// <param name="pVersion"></param>
        private void SetProtocolVersion(byte pVersion)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_HOST_PROTOCOL_VERSION;
                _command.CommandData[1] = pVersion;
                _command.CommandDataLength = 2;
                SendCommand();
            }
        }

        /// <summary>
        /// Sends the command LAST REJECT CODE which gives info about why a note has been rejected
        /// </summary>
        private string GetQueryRejectionReason()
        {
            // do not lock this session. It's already locked by action above
            _command.CommandData[0] = SspCommand.SSP_CMD_LAST_REJECT_CODE;
            _command.CommandDataLength = 1;
            if (!SendCommand())
                return "Failed to identify";

            switch (_command.ResponseData[1])
            {
                case 0x00: return "Note accepted";
                case 0x01: return "Note length incorrect";
                case 0x02: return "Invalid note";
                case 0x04: return "Invalid note";
                case 0x05: return "Invalid note";
                case 0x06: return "Channel inhibited";
                case 0x07: return "Second note inserted during read";
                case 0x08: return "Host rejected note";
                case 0x09: return "Invalid note";
                case 0x0A: return "Invalid note read";
                case 0x0B: return "Note too long";
                case 0x0C: return "Validator disabled";
                case 0x0D: return "Mechanism slow/stalled";
                case 0x0E: return "Strim attempt";
                case 0x0F: return "Fraud channel reject";
                case 0x10: return "No notes inserted";
                case 0x11: return "Invalid note read";
                case 0x12: return "Twisted note detected";
                case 0x13: return "Escrow time-out";
                case 0x14: return "Bar code scan fail";
                case 0x15: return "Invalid note read";
                case 0x16: return "Invalid note read";
                case 0x17: return "Invalid note read";
                case 0x18: return "Invalid note read";
                case 0x19: return "Incorrect note width";
                case 0x1A: return "Note too short";
                default: return "Failed to identify";
            }
        }

        /// <summary>
        /// GET DENOMINATION LEVEL to find out the number of a specified type of note stored in the payout
        /// </summary>
        /// <param name="bill"></param>
        /// <returns>Returns the number of notes stored of that denomination</returns>
        private ushort GetNoteLevel(Denomination bill)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_GET_DENOMINATION_LEVEL;
                byte[] b = CHelpers.ConvertIntToBytes((int)(bill.Amount * _info.ValueMultiplier));
                _command.CommandData[1] = b[0];
                _command.CommandData[2] = b[1];
                _command.CommandData[3] = b[2];
                _command.CommandData[4] = b[3];

                string currency = bill.Currency.GetCode();
                _command.CommandData[5] = (byte)currency[0];
                _command.CommandData[6] = (byte)currency[1];
                _command.CommandData[7] = (byte)currency[2];
                _command.CommandDataLength = 8;

                return SendCommand() ? _command.ResponseData[1] : (ushort)0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        /// <param name="route">to stacker or to recycler</param>
        /// <returns></returns>
        private bool SetDenominationRoute(Denomination note, CashRoute route)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_SET_DENOMINATION_ROUTE;
                _command.CommandData[1] = (byte)(route == CashRoute.Stacker ? 0x01 : 0x00);

                // get the note as a byte array
                byte[] b = BitConverter.GetBytes((int)note.Amount * _info.ValueMultiplier);
                _command.CommandData[2] = b[0];
                _command.CommandData[3] = b[1];
                _command.CommandData[4] = b[2];
                _command.CommandData[5] = b[3];

                // send country code (protocol 6+)
                _command.CommandData[6] = (byte)note.Currency.GetCode()[0];
                _command.CommandData[7] = (byte)note.Currency.GetCode()[1];
                _command.CommandData[8] = (byte)note.Currency.GetCode()[2];

                _command.CommandDataLength = 9;

                if (!SendCommand())
                {
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Error($"Failed to change route of {note} to {(route == CashRoute.Stacker ? "stacker" : "recycler")}"));
                    return false;
                }

                return true;
            }
        }

        private CashRoute GetDenominationRoute(Denomination note)
        {
            // Determine if the note is currently being recycled
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_GET_DENOMINATION_ROUTE;
                byte[] b = BitConverter.GetBytes((int)note.Amount * _info.ValueMultiplier);
                _command.CommandData[1] = b[0];
                _command.CommandData[2] = b[1];
                _command.CommandData[3] = b[2];
                _command.CommandData[4] = b[3];

                // Add currency
                _command.CommandData[5] = (byte)note.Currency.GetCode()[0];
                _command.CommandData[6] = (byte)note.Currency.GetCode()[1];
                _command.CommandData[7] = (byte)note.Currency.GetCode()[2];
                _command.CommandDataLength = 8;

                if (!SendCommand())
                {
                    OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Failed to get route"));
                    return CashRoute.Unknown;
                }

                // True if it is currently being recycled
                if (_command.ResponseData[1] == 0x00)
                    return CashRoute.Payout;
                // False if cashbox (stacker)
                else if (_command.ResponseData[1] == 0x01)
                    return CashRoute.Stacker;

                return CashRoute.Unknown;
            }
        }
    }
}