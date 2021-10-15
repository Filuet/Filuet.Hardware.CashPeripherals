using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    public class J2000CoinMechanismSettingsBuilder
    {
        private bool _acceptCoins = true;
        private bool _extractCoins = true;
        private ICollection<(int channelId, Denomination nominal, ushort maxlevel)> _channels = new List<(int, Denomination, ushort)>();

        public J2000CoinMechanismSettingsBuilder WithRoutes(bool acceptCoint, bool extractCoins)
        {
            _acceptCoins = acceptCoint;
            _extractCoins = extractCoins;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="nominal"></param>
        /// <param name="maxLevel">it is being adjusted on the device. We have to copy it as an informational field</param>
        /// <returns></returns>
        public J2000CoinMechanismSettingsBuilder WithChannel(int channelId, Denomination nominal, ushort maxLevel)
        {
            if (_channels.Any(x => x.channelId == channelId))
                throw new ArgumentException($"Channel {channelId} has already been added");

            _channels.Add((channelId, nominal, maxLevel));
            return this;
        }

        public J2000CoinMechanismSettings Build() => new J2000CoinMechanismSettings { AcceptCoins = _acceptCoins, ExtractCoins = _extractCoins, Channels = _channels };
    }
}