using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public class ITLCashValidatorInfo
    {
        public string Type { get; set; }
        public string Firmware { get; set; }

        /// <summary>
        /// Variable to hold the number of channels in the validator dataset access to number of channels being used by the validator
        /// </summary>
        public uint NumberOfChannels { get; set; }

        /// <summary>
        /// The multiplier by which the channel values are multiplied to give their real penny value. E.g. £5.00 on channel 1, the value would be 5 and the multiplier 100
        /// </summary>
        public ushort ValueMultiplier { get; set; }

        public int Protocol { get; set; }

        public string SerialNumber { get; set; }

        public string CachePath { get; set; }

        /// <summary>
        /// A list of dataset data, sorted by value. Holds the info on channel number, value, currency, level and whether it is being recycled
        /// </summary>
        public ICollection<ITLChannel> UnitDataList = new List<ITLChannel>();

        /// <summary>
        /// How much money in cashbox
        /// </summary>
        internal IDictionary<Denomination, ushort> CashboxStock = new Dictionary<Denomination, ushort>();

        public Denomination GetBillByChannel(byte channel)
            => UnitDataList.FirstOrDefault(x => x.Channel == channel).Nominal;
    }
}