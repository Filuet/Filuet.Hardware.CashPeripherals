using Filuet.Infrastructure.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Models
{
    /// <summary>
    /// List of bills that available to be used in change and extraction
    /// </summary>
    public class PayoutStock
    {
        private ICollection<(Denomination denomination, ushort qty, ushort maxQty)> _toIssue = new List<(Denomination denomination, ushort qty, ushort maxQty)>();

        public ICollection<(Denomination denomination, ushort qty, ushort maxQty)> Stock => _toIssue.OrderBy(x => x.denomination.Amount).ToList();

        public PayoutStock Populate(Action<PayoutStock> setup)
        {
            setup(this);
            return this;
        }

        public void Populate(Denomination denomination, ushort qty, ushort maxQty)
        {
            if (maxQty < qty)
                throw new ArgumentException("Denomination max quantity violation");

            if (!_toIssue.Any(x => x.denomination == denomination))
            {
                _toIssue.Add((denomination, qty, maxQty));
                return;
            }

            throw new ArgumentException($"{denomination} is already added");
        }
    }
}
