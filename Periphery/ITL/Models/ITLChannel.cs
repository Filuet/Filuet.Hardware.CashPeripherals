﻿using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common.Helpers;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL.Models
{
    public class ITLChannel : DenominationChannel
    {
        public char[] CurrencyAsChars => Nominal.Currency.GetCode().ToCharArray();

        public int Multiplier { get; set; } = 100;
    }
}
