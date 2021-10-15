namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Generic response codes
    /// </summary>
    public enum IpcSimpleResponseIndex : int
    {
        /// <summary>
        /// Start of Text
        /// </summary>
        Stx = 0,

        /// <summary>
        /// Device address
        /// </summary>
        Address = 1,

        /// <summary>
        /// Length
        /// </summary>
        Length = 2,

        /// <summary>
        /// ACK
        /// </summary>
        Ack = 3,

        /// <summary>
        /// Checksumma
        /// </summary>
        CheckSumma = 4
    }
}