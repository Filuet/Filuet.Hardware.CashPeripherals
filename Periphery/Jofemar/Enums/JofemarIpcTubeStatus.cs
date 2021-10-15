namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Tube status
    /// </summary>
    public enum JofemarIpcTubeStatus : byte
    {
        /// <summary>
        /// Not full
        /// </summary>
        NotFull = 0x00,

        /// <summary>
        /// Full
        /// </summary>
        Full = 1,

        /// <summary>
        /// Full or bad
        /// If a changer can detect a tube jam, defective tube sensor, or other malfunction, it will indicate the tube
        /// is "bad" by sending a Tube Full status and a count of zero for the malfunctioning coin type.
        /// </summary>
        FullOrBad = 0xff
    }
}
