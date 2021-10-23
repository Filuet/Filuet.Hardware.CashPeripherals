using Filuet.Infrastructure.Abstractions.Business;
using System;

namespace Filuet.Hardware.CashAcceptors.Services.Events
{
    public sealed class MoneyEventArgs : EventArgs
    {
        public Money Money { get; set; }
    }
}