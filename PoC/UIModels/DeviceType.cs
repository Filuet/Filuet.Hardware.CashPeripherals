using Filuet.Infrastructure.Attributes;

namespace PoC.UIModels
{
    public enum DeviceType 
    {
        [Code("ITL smart payout")]
        ITLSmartPayout = 1,
        [Code("ITL smart hopper")]
        ITLSmartHopper = 2,
        [Code("J2000 coin mechanism")]
        J2000 = 3
    }
}
