using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public class ITLCashValidatorSettings
    {
        public uint ComPort { get; internal set; }
        public byte SSPAddress { get; internal set; } = 0;

        public CashValidatorIlluminationMode this[CashValidatorState deviceState]
            => IlluminationModes.FirstOrDefault(x => x.DeviceState == deviceState);

        public uint GetDenominationMaxQuantityInPayout(Denomination bill) => DenominationMaxQuantityInPayout.FirstOrDefault(x => x.bill == bill).qty;

        internal List<(Denomination bill, ushort qty)> DenominationMaxQuantityInPayout = new List<(Denomination, ushort)>();

        internal IEnumerable<CashValidatorIlluminationMode> IlluminationModes;
    }
}
