
namespace PoC
{
    partial class PoCForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cashierTesterControl1 = new PoC.CashierTesterControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.deviceSelector1 = new PoC.DeviceSelector();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(791, 552);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cashierTesterControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(783, 524);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Cashier tester";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cashierTesterControl1
            // 
            this.cashierTesterControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cashierTesterControl1.Location = new System.Drawing.Point(3, 3);
            this.cashierTesterControl1.Name = "cashierTesterControl1";
            this.cashierTesterControl1.Size = new System.Drawing.Size(777, 518);
            this.cashierTesterControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.deviceSelector1);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(783, 524);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Device tester";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // deviceSelector1
            // 
            this.deviceSelector1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceSelector1.Location = new System.Drawing.Point(3, 3);
            this.deviceSelector1.Name = "deviceSelector1";
            this.deviceSelector1.Size = new System.Drawing.Size(777, 518);
            this.deviceSelector1.TabIndex = 0;
            // 
            // PoCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 552);
            this.Controls.Add(this.tabControl1);
            this.Name = "PoCForm";
            this.Text = "Cash tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PoCForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private CashierTesterControl cashierTesterControl1;
        private DeviceSelector deviceSelector1;
    }
}