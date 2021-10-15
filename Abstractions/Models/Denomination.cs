using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Common.Helpers;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Models
{
    public struct Denomination
    {
        public uint Amount;

        public Currency Currency;

        public Denomination(uint nominal, Currency currency)
        {
            Amount = nominal;
            Currency = currency;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Denomination))
                return false;

            Denomination mys = (Denomination)obj;

            return Amount == mys.Amount && Currency == mys.Currency;
        }

        public override int GetHashCode() => (Amount, Currency).GetHashCode();

        public static bool operator ==(Denomination lhs, Denomination rhs) => lhs.Equals(rhs);

        public static bool operator !=(Denomination lhs, Denomination rhs) => !(lhs == rhs);

        public override string ToString() => $"{Amount} {Currency.GetCode()}";
    }
}