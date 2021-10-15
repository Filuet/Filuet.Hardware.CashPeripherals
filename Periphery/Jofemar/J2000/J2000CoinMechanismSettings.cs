using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    public class J2000CoinMechanismSettings
    {
        public bool AcceptCoins { get; internal set; } = true;

        public bool ExtractCoins { get; internal set; } = true;

        public ICollection<(int channelId, Denomination nominal, ushort maxlevel)> Channels { get; internal set; }

        public J2000CoinMechanismSettings() { }
    }
}