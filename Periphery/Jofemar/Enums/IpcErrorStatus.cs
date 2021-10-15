namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.Enums
{
    /// <summary>
    /// Error status
    /// </summary>
    public enum IpcErrorStatus : byte
    {
        /// <summary>
        /// internal eeprom corrupted
        /// </summary>
        InternalEepromCorrupted = 0x90,

        /// <summary>
        /// oscillator not calibrated
        /// </summary>
        OscillatorNotCalibrated = 0x91,

        /// <summary>
        /// channel jammed
        /// </summary>
        ChannelJammed = 0x1a,

        /// <summary>
        /// peripheral failure
        /// </summary>
        PeripheralFailure = 0x1d,

        /// <summary>
        /// no communication to peripheral
        /// </summary>
        NoCommunicationToPeripheral = 0x1f,
    }
}
