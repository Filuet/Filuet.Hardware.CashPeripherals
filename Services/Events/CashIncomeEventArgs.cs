using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Infrastructure.Abstractions.Business;
using System;

namespace Filuet.Hardware.CashAcceptors.Services.Events
{
    public sealed class CashIncomeEventArgs : EventArgs
    {
        public Money Money { get; private set; }

        public static CashIncomeEventArgs Income(Denomination denomination) => new CashIncomeEventArgs { Money = Money.Create(denomination.Amount, denomination.Currency) };
    }
}