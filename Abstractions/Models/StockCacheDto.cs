using Filuet.Infrastructure.Abstractions.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Models
{
    public class StockCacheDto : List<StockCacheItem>
    {
        public StockCacheDto() {}

        public StockCacheDto(IDictionary<Denomination, ushort> data)
        {
            foreach (var x in data)
                Add(new StockCacheItem { Note = x.Key, Quantity = x.Value });
        }
    }

    public class StockCacheItem
    {
        [JsonPropertyName("note")]
        public Denomination Note { get; set; }

        [JsonPropertyName("qty")]
        public ushort Quantity { get; set; }

        public override string ToString() => $"{Note}: {Quantity}";
    }
}
