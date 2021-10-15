namespace Filuet.Hardware.CashAcceptors.Abstractions.Models
{
    public class DenominationChannel
    {        
        public byte Channel { get; set; } = 0;

        public Denomination Nominal { get; set; }

        /// <summary>
        /// Stored quantity in the payout
        /// </summary>
        public ushort Level { get; set; } = 0;

        public override string ToString() => $"Channel {Channel}: {Nominal}";
    }
}
