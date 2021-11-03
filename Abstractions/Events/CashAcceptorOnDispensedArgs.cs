using Filuet.Infrastructure.Abstractions.Models;
using System;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Events
{
    public class CashAcceptorOnDispensedArgs : EventArgs
    {
        public Denomination Dispensed { get; set; }
    }
}