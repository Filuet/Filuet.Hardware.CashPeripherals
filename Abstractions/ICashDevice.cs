using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Events;
using Filuet.Infrastructure.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filuet.Hardware.CashAcceptors.Abstractions
{
    public interface ICashDevice
    {
        CashDeviceMode Mode { get; }

        CashDeviceState State { get; }

        event EventHandler<CashAcceptorOnInsertedArgs> OnInserted;
        event EventHandler<CashAcceptorOnDispensedArgs> OnDispensed;
        event EventHandler<CashAcceptorLogArgs> OnEvent;
        event EventHandler<CashAcceptorResetArgs> OnReset;
        event EventHandler<CashAcceptorOnFullCashbox> OnFullCashbox;

        Task Run();

        void Stop();

        void Reset();

        void Extract(Denomination bill, ushort quantity);

        void PushAllToCashBox();

        string GetSerialNumber(byte? device = null);

        ICollection<(Denomination denomination, ushort qty, ushort maxQty)> GetPayoutStock();

        IDictionary<Denomination, ushort> GetCashboxStock();

        IDictionary<Denomination, CashRoute> GetRoutes();
    }
}