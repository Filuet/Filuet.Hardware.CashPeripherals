namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Standard Ipc commands (the list is not full)
    /// </summary>
    public enum IpcControl : byte
    {
        /// <summary>
        /// Start of Text
        /// </summary>
        STX = 0x35,
        /// <summary>
        /// Acknowledgement
        /// </summary>
        ACK = 0x00,
        /// <summary>
        /// Negative-Acknowledgement
        /// </summary>
        NAK = 0xff
    }
}