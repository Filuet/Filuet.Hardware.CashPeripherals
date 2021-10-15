namespace Filuet.Hardware.CashAcceptors.Periphery.Jofemar.J2000
{
    internal class J2000IpcCommands
    {
        public static byte[] GetCcStatus => new byte[] { 0x25 };

        /// <summary>
        /// PC TO DEVICE
        /// 24H
        /// DEVICE TO PC
        /// 24H + DATA1 + DATA2 + … + DATA17
        /// DATA1 (word) bit0-bit15
        /// DATA2 - DATA17 (16 bytes)
        /// Tube Full status: 1= Full, 0= Not full
        /// Tube Status - Indicates the greatest number of coins that the changer "knows" definitely are present in the
        /// coin tubes. A byte position in the 16 byte string indicates the number of coins in a tube for a particular coin
        /// type. For example, the first byte sent indicates the number of coins in a tube for coin type 0. Unsent bytes
        /// are assumed to be zero. For tube counts greater than 255, counts should remain at 255.
        /// </summary>

        public static byte[] GetCcChangerStatus => new byte[] { 0x24 };
        public static int CcChangerDataLenght => 15;
        public static int ResponseCcChangerDataLenght => 20;

        /// <summary>
        /// PC TO DEVICE
        /// 20H
        /// DEVICE TO PC
        /// 20H + EVENT COUNTER +
        /// E1 + E2 + E3 + E4 + E5 + E6 + E7 + E8
        /// E9 + E10 + E11 + E12
        /// EVENT COUNTER starts from 0x00 after reset and increments on each new event and overflows
        /// after 0xFF to 0x01. Thus EVENT COUNTER = 0x00 means device has just been reset or powered up.
        /// E1 to E12 are event slots recording events happened in peripheral where E1 is the most recent event.
        /// On event equals CREDIT ACCEPTED, CHANNEL# 20H TO 2FH + DATA
        /// EVENT COUNTER increments by 2 and data occupies 2 event slots
        /// En 0x20 to 0x2F Represents credit channel 0 to 15
        /// En-1 
        /// 0x00 Deposited into Cashbox
        /// 0x01 Deposited into Tubes
        /// 0x02 Not used
        /// 0x03 Routed to Reject
        /// </summary>
        public static byte[] PollCc => new byte[] { 0x20 };
        public static int PollCcMaxEventCount => 12;
        public static int PollCcMinCreditChanel => 0x20;
        public static int PollCcMaxCreditChanel => 0x2f;

        /// <summary>
        /// PC TO DEVICE
        /// 21H + DATA
        /// DATA (word)
        /// bit0-bit15 Channel Enabling 1= On 0= Off
        /// Turn on / off for individual channels
        /// coins are checked for genuinity and accepted if that channel is turned on
        /// DEVICE TO PC
        /// ACK
        /// </summary>
        public static byte[] SetCoinsEnable(byte mask1, byte mask2) => new byte[3] { 0x21, mask1, mask2 };
  

        /// <summary>
        /// PC TO DEVICE
        /// 26H + DATA1 + DATA2
        /// DATA1 (byte) Coin type 0-15
        /// DATA2 (byte) Number of coins to be dispensed 1 to 15
        /// DEVICE TO PC
        /// ACK
        /// </summary>
        public static byte[] GiveTheChangeByCoins(int coinNo, int qty)
        {
            byte[] retval = new byte[3] { 0x26, (byte)(coinNo & 0xff), (byte)(qty & 0xff) };
            return retval;
        }

        public static byte[] ResetCc => new byte[] { 0x05 };
        public static byte[] ResetController => new byte[] { 0x01, 0xff };
    }
}
