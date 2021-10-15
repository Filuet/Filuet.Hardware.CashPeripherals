using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Common.Ssp;
using Filuet.Hardware.CashAcceptors.Periphery.ITL.Models;
using System.Drawing;
using System.Threading;

namespace Filuet.Hardware.CashAcceptors.Periphery.ITL
{
    public partial class ITLCashValidator
    {
        private CancellationTokenSource _highlightCancelTokenSource = new CancellationTokenSource();
        private bool _isGlowing = false;

        private void SetLight(Color color)
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_CONFIGURE_BEZEL;
                _command.CommandData[1] = (byte)(color.R & 255);
                _command.CommandData[2] = (byte)(color.G & 255);
                _command.CommandData[3] = (byte)(color.B & 255);
                _command.CommandDataLength = 4;
                SendCommand();
            }
        }

        private void ExtinguishIllumination()
        {
            lock (_command)
            {
                _command.CommandData[0] = SspCommand.SSP_CMD_CONFIGURE_BEZEL;
                _command.CommandData[1] = 0;
                _command.CommandData[2] = 0;
                _command.CommandData[3] = 0;
                _command.CommandDataLength = 4;
                SendCommand();
            }
        }

        private void Illuminate(CashValidatorState state)
        {
            if (_isGlowing)
                _highlightCancelTokenSource.Cancel();

            CashValidatorIlluminationMode mode = _settings[state];

            _isGlowing = true;

            switch (mode.IlluminationKind)
            {
                case CashValidatorIlluminationKind.Solid:
                default:
                    SetLight(mode.Color.Value);
                    break;
            }

            _highlightCancelTokenSource = new CancellationTokenSource();
        }
    }
}