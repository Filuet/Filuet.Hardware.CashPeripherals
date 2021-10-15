using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    public class J2000CoinMechanismInfo
    {
        public string Serial { get; internal set; }

        /// <summary>
        ///  Holds the info on channel number, value, currency, level
        /// </summary>
        public ICollection<DenominationChannel> Channels = new List<DenominationChannel>();

        /// <summary>
        /// How much money in cashbox
        /// </summary>
        internal IDictionary<Denomination, ushort> CashboxStock = new Dictionary<Denomination, ushort>();
    }
}
