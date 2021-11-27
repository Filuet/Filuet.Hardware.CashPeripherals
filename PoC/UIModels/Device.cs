using Filuet.Infrastructure.Abstractions.Helpers;

namespace PoC.UIModels
{
    public class Device
    {
        public DeviceType Type { get; set; }

        public override string ToString() => Type.GetCode();
    }
}