using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using Filuet.Infrastructure.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public class ITLCashValidatorSettingsBuilder
    {
        private List<CashValidatorIlluminationMode> _illuminations = new List<CashValidatorIlluminationMode>();
        private List<(Denomination bill, ushort qty)> _denominationMaxCountInPayout = new List<(Denomination, ushort)>();
        /// <summary>
        /// Payout it's a recycle to store notes to give a change
        /// </summary>
        private uint _maxBillQtyInPayout = 70;
        private uint _comPort;
        private byte _sspAddress;

        public ITLCashValidatorSettingsBuilder WithComPort(uint comPort)
        {
            if (comPort > 16)
                throw new ArgumentException("Invalid com port");

            _comPort = comPort;

            return this;
        }

        public ITLCashValidatorSettingsBuilder WithSspAddress(byte sspAddress)
        {
            _sspAddress = sspAddress;

            return this;
        }

        public ITLCashValidatorSettingsBuilder WithIlluminationMode(CashValidatorIlluminationMode mode)
        {
            if (_illuminations.Any(x => x.DeviceState == mode.DeviceState))
                throw new ArgumentException($"{mode.DeviceState} already added");

            _illuminations.Add(mode);
            return this;
        }

        public ITLCashValidatorSettingsBuilder SetMaxBillsInPayout(uint maxBillQtyInPayout)
        {
            if (maxBillQtyInPayout < 3)
                throw new ArgumentException("Too low upper limit of bills in the payout");

            _maxBillQtyInPayout = maxBillQtyInPayout;

            return this;
        }

        public ITLCashValidatorSettingsBuilder WithDenominationUpperLimitInPayout(Denomination bill, ushort maxQtyThresholdInPayout)
        {
            if (_maxBillQtyInPayout < 3)
                throw new ArgumentException("Too low upper limit of bills in the payout");

            if (_denominationMaxCountInPayout.Any(x => x.bill == bill))
                throw new ArgumentException($"{bill} is already defined");

            if ((_denominationMaxCountInPayout.Sum(x => x.qty) + maxQtyThresholdInPayout) > _maxBillQtyInPayout)
                throw new ArgumentException($"The upper limit of storage of bills exceeded {_maxBillQtyInPayout}");

            _denominationMaxCountInPayout.Add((bill, maxQtyThresholdInPayout));

            return this;
        }

        public ITLCashValidatorSettings Build() => new ITLCashValidatorSettings { IlluminationModes = _illuminations, 
            ComPort = _comPort, 
            SSPAddress = _sspAddress,
            DenominationMaxQuantityInPayout = _denominationMaxCountInPayout
        };
    }
}
