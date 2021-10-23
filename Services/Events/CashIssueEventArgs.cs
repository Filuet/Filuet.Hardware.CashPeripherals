using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Infrastructure.Abstractions.Business;
using System;

namespace Filuet.Hardware.CashAcceptors.Services.Events
{
    public sealed class CashIssueEventArgs : EventArgs
    {
        public Money Money { get; private set; }

        public static CashIssueEventArgs Income(Denomination denomination) => new CashIssueEventArgs { Money = Money.Create(denomination.Amount, denomination.Currency) };
    }
}