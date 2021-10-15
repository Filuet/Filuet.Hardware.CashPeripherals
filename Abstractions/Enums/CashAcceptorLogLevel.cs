using Filuet.Hardware.CashAcceptors.Common.Attributes;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Enums
{
    public enum CashAcceptorLogLevel : uint
    {
        [Code("Info")]
        Info = 0x00,
        [Code("Warning")]
        Warning,
        [Code("Error")]
        Error
    }
}