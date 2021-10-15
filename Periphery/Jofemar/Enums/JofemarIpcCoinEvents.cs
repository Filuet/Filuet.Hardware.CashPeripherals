namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Coin accepted events
    /// </summary>
    public enum JofemarIpcCoinEvents : byte
    {
        /// <summary>
        /// Deposited into Cashbox
        /// </summary>
        DepositedIntoCashbox = 0x00,

        /// <summary>
        /// Deposited into Tubes
        /// </summary>
        DepositedIntoTubes = 0x01,

        /// <summary>
        /// Not used
        /// </summary>
        NotUsed = 0x02,

        /// <summary>
        /// Routed to Reject
        /// </summary>
        RoutedToReject = 0x03
    }
}