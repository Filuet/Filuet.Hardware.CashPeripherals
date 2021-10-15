using System.ComponentModel;

namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Jofemar device addresses
    /// </summary>
    public enum JofemarIpcDeviceAddress : byte
    {
        [Description("IPC adapter")]
        Controller = 0x00,
        [Description("J2000 Coin acceptor")]
        Device = 0x12
    }
}