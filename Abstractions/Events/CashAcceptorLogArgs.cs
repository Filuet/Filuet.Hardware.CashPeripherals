using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using System;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Events
{
    public class CashAcceptorLogArgs : EventArgs
    {
        public CashAcceptorLogLevel Level { get; set; }
        public string Message { get; set; }

        public static CashAcceptorLogArgs Info(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(message);

            return new CashAcceptorLogArgs { Level = CashAcceptorLogLevel.Info, Message = message };
        }

        public static CashAcceptorLogArgs Warning(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(message);

            return new CashAcceptorLogArgs { Level = CashAcceptorLogLevel.Warning, Message = message };
        }

        public static CashAcceptorLogArgs Error(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(message);

            return new CashAcceptorLogArgs { Level = CashAcceptorLogLevel.Error, Message = message };
        }
    }
}