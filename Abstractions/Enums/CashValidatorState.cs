namespace Filuet.Hardware.CashAcceptors.Abstractions.Enums
{
    public enum CashValidatorState
    {
        Idle = 0x01,
        Receiving,
        Extracting,
        Error
    }
}
