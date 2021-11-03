using Filuet.Infrastructure.Abstractions.Models;

namespace PoC.UIModels
{
    public class Channel
    {
        public Denomination Denomination { get; set; }
        public uint Count { get; set; }
        public uint MaxCount { get; set; }

        public override string ToString() => MaxCount > 0 ? $"{Count}/{MaxCount} of {Denomination}" : $"{Count} of {Denomination}";
    }
}