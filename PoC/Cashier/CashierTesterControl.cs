using Filuet.ASC.Kiosk.OnBoard.Cashbox.Core;
using System;
using System.Windows.Forms;

namespace PoC
{
    public partial class CashierTesterControl : UserControl
    {
        private CashierService _cashier;

        public CashierTesterControl()
        {
            InitializeComponent();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            stopButton.Enabled = setupButton.Enabled = false;
        }

        private void setupButton_Click(object sender, EventArgs e)
        {
            runButton.Enabled = stopButton.Enabled = false;
        }

        private void CashierTesterControl_Load(object sender, EventArgs e)
        {
            //_cashier = new CashierService();
        }
    }
}
