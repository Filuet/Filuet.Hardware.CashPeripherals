using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Common.Helpers;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Models
{
    public struct Money
    {
        public decimal Value;

        public Currency Currency;

        public Money(decimal value, Currency currency)
        {
            Value = value;
            Currency = currency;
        }

        public override string ToString() => $"{Value} {Currency.GetCode()}";
    }
}
