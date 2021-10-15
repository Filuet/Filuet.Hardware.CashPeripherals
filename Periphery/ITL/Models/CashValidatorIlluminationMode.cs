using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using System;
using System.Drawing;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL.Models
{
    public class CashValidatorIlluminationMode
    {
        public CashValidatorState DeviceState { get; private set; }
        public CashValidatorIlluminationKind IlluminationKind { get; private set; }
        public Color? Color { get; private set; }

        public static CashValidatorIlluminationMode New(CashValidatorState deviceState, CashValidatorIlluminationKind illuminationKind, Color? color = null)
        {
            if (!color.HasValue && illuminationKind == CashValidatorIlluminationKind.Solid)
                throw new ArgumentException("Color is mandatory");

            return new CashValidatorIlluminationMode
            {
                DeviceState = deviceState,
                IlluminationKind = illuminationKind,
                Color = color
            };
        }
    }
}
