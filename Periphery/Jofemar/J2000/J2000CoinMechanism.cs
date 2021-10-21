using Filuet.Hardware.CashAcceptors.Abstractions;
using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common;
using Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    public partial class J2000CoinMechanism : ICashDevice
    {
        public CashDeviceState State { get; private set; }

        public event EventHandler<CashAcceptorOnInsertedArgs> OnInserted;
        public event EventHandler<CashAcceptorOnDispensedArgs> OnDispensed;
        public event EventHandler<CashAcceptorLogArgs> OnEvent;
        public event EventHandler<CashAcceptorResetArgs> OnReset;
        public event EventHandler<CashAcceptorOnFullCashbox> OnFullCashbox;

        private int _messageCounter;

        private object _cmdLocker = new object();

        public J2000CoinMechanism(Action<J2000CoinMechanismSettingsBuilder> setupAction)
        {
            _settings = setupAction?.CreateTargetAndInvoke().Build();
            if (_settings == null)
                throw new ArgumentException("J2000 settings are not configured");

            _info = new J2000CoinMechanismInfo();

            int mask1 = 0, mask2 = 0, mask3 = 0;

            foreach (var channel in _settings.Channels)
            {
                if (channel.channelId < 8)
                    mask1 |= 1 << channel.channelId;
                else if (channel.channelId < 16)
                    mask2 |= 1 << (channel.channelId - 8);
                else mask3 |= 1 << (channel.channelId - 16);
            }

            _inhibitMask1 = (byte)mask1;
            _inhibitMask2 = (byte)mask2;
            _inhibitMask3 = (byte)mask3;

            _ftdi = RunFtdi();

            if (_ftdi == null)
            {
                State = CashDeviceState.Disabled;
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Unable to establish connection with J2000"));
            }
            else if (!EnableAllChannels())
            {
                State = CashDeviceState.Error;
                OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("Failed to enable J2000 channels"));
            }
            else State = CashDeviceState.OK;
        }

        public Task Run()
            => Task.Factory.StartNew(() => {
                if (_ftdi == null)
                    return;

                while (true)
                {
                    WriteReadAndParseResponse((byte)JofemarIpcDeviceAddress.Device, J2000IpcCommands.PollCc);
                    Thread.Sleep(50);
                }
            });

        public void Stop()
        {
            if (_ftdi == null)
                return;

            if (WriteAndRead((byte)JofemarIpcDeviceAddress.Device, J2000IpcCommands.SetCoinsEnable(0, 0), out byte[] response) &&
                response.Length >= (int)JofemarIpcResponseLength.SetCoinsEnable &&
                (IpcControl)response[(int)IpcSimpleResponseIndex.Ack] == IpcControl.ACK)
            {
                State = CashDeviceState.Disabled;
                ////SaveStoredTubesCounters(CashDataEventTypes.DISABLED);
            }
            else OnEvent?.Invoke(this, CashAcceptorLogArgs.Error("An error occured while disabling ipc device"));
        }

        public void Reset()
        {
            WriteAndRead((byte)JofemarIpcDeviceAddress.Controller, J2000IpcCommands.ResetController, out byte[] response);
            if (response.Length > 0)
                OnReset?.Invoke(this, new CashAcceptorResetArgs { });
        }

        public void Extract(Denomination bill, ushort quantity)
        {
            var channel = _settings.Channels.FirstOrDefault(x => x.nominal == bill);

            if (channel.channelId > 0)
            {
                for (ushort i = 0; i < quantity; i++)
                {
                    bool result = WriteAndRead((byte)JofemarIpcDeviceAddress.Device,
                        J2000IpcCommands.GiveTheChangeByCoins(channel.channelId, quantity), out byte[] response); // 4/*5 rub*/

                    if (result)
                        OnDispensed?.Invoke(this, new CashAcceptorOnDispensedArgs { Dispensed = bill });
                    else OnEvent?.Invoke(this, CashAcceptorLogArgs.Error($"An error occured when extracting {bill}"));
                }
            }
            else throw new Exception($"Unknown denomination {bill}");
        }

        public void PushAllToCashBox()
        {
            throw new NotImplementedException("J2000 haven't got a feature for pushing coins from tubes to the cashbox");
        }

        public string GetSerialNumber(byte? device = null) => _info.Serial;

        public ICollection<(Denomination denomination, ushort qty, ushort maxQty)> GetPayoutStock()
        {
            GetAllLevels();
            return _info.Channels.Select(x => (x.Nominal, x.Level, _settings.Channels.FirstOrDefault(c=>c.nominal == x.Nominal).maxlevel)).ToList();
        }

        public IDictionary<Denomination, ushort> GetCashboxStock()
        {
            if (!_info.CashboxStock.Any())
            {
                foreach (var i in _info.Channels) // probably upload from cache?
                    _info.CashboxStock.Add(i.Nominal, (ushort)0);
            }

            return _info.CashboxStock;
        }

        public IDictionary<Denomination, CashRoute> GetRoutes()
        {
            throw new NotImplementedException("Routes are not applicable for J2000. We can setup max qty in tubes only");
        }

        //private void SaveStoredTubesCounters(CashDataEventTypes devEvent, string description = "")
        //{
        //    byte[] response;
        //    if (WriteAndRead((byte)IpcDeviceAddress.DEVICE, _cmd.GetCcChangerStatus, out response) != PortResultCodes.Success)
        //    {
        //        ////OnLog?.Invoke(this, new CashDeviceLogEventArgs { Message = $"SaveStoredTubesCounters request error to IPC device: [{devEvent}] {description}" });
        //        return;
        //    }

        //    if (response.Length >= _cmd.ResponseCcChangerDataLenght)
        //    {
        //        for (int i = 0; i < _cmd.CcChangerDataLenght; i++)
        //        {
        //            int pos = i + 6;
        //            if ((IpcTubeStatus)response[pos] != IpcTubeStatus.FullorBad && Nominals.ContainsKey(i))
        //            {
        //                EntitiesHelper.SaveCashDeviceCounters((byte)devEvent,
        //                    CashDeviceProtocolTypes.IPC,
        //                    2000,
        //                    Nominals[i].Nominal,
        //                    description,
        //                    0,
        //                    response[pos],
        //                    0,
        //                    0);
        //            }
        //        }
        //        return;
        //    }
        //}
     
        private readonly J2000CoinMechanismSettings _settings;
        private readonly J2000CoinMechanismInfo _info;
        private readonly FTD2XX_NET.FTDI _ftdi;

        private byte _inhibitMask1;
        private byte _inhibitMask2;
        private byte _inhibitMask3;

        private const int WakeUpCount = 3;
        private const int _readDelay = 200;
    }
}