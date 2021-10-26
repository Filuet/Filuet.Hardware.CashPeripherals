using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            deviceTesterControl1.PoCForm_FormClosing(this, e);

            this.Visible = false;

            Thread.Sleep(3000); // Wait until money the device is turning off
        }
    }
}
