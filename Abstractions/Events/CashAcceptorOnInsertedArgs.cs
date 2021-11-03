using Filuet.Infrastructure.Abstractions.Models;
using System;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Events
{
    public class CashAcceptorOnInsertedArgs : EventArgs
    {
        public Denomination Inserted { get; set; }
    }
}