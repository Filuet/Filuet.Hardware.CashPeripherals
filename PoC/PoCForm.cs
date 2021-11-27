using System.Threading;
using System.Windows.Forms;

namespace PoC
{
    public partial class PoCForm : Form
    {
        public PoCForm()
        {
            InitializeComponent();
        }

        private void PoCForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            deviceSelector1.PoCForm_FormClosing(this, e);

            this.Visible = false;

            Thread.Sleep(3000); // Wait until money the device is turning off
        }
    }
}
