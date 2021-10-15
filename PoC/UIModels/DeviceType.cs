using Filuet.Hardware.CashAcceptors.Common.Attributes;

namespace PoC.UIModels
{
    public enum DeviceType 
    {
        [Code("ITL cash acceptor")]
        ITL = 1,
        [Code("J2000 coin mechanism")]
        J2000 = 2
    }
}
