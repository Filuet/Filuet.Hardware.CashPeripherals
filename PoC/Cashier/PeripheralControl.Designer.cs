
namespace PoC
{
    partial class PeripheralControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.devicesLabel = new System.Windows.Forms.Label();
            this.devicesBox = new System.Windows.Forms.ListBox();
            this.deviceTesterControl1 = new PoC.deviceTesterControl();
            this.SuspendLayout();
            // 
            // devicesLabel
            // 
            this.devicesLabel.AutoSize = true;
            this.devicesLabel.Location = new System.Drawing.Point(3, 7);
            this.devicesLabel.Name = "devicesLabel";
            this.devicesLabel.Size = new System.Drawing.Size(47, 15);
            this.devicesLabel.TabIndex = 0;
            this.devicesLabel.Text = "Devices";
            // 
            // devicesBox
            // 
            this.devicesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesBox.FormattingEnabled = true;
            this.devicesBox.ItemHeight = 15;
            this.devicesBox.Location = new System.Drawing.Point(3, 29);
            this.devicesBox.Name = "devicesBox";
            this.devicesBox.Size = new System.Drawing.Size(386, 184);
            this.devicesBox.TabIndex = 1;
            // 
            // deviceTesterControl1
            // 
            this.deviceTesterControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceTesterControl1.Location = new System.Drawing.Point(3, 219);
            this.deviceTesterControl1.Name = "deviceTesterControl1";
            this.deviceTesterControl1.Size = new System.Drawing.Size(386, 362);
            this.deviceTesterControl1.TabIndex = 4;
            // 
            // PeripheralControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.deviceTesterControl1);
            this.Controls.Add(this.devicesBox);
            this.Controls.Add(this.devicesLabel);
            this.Name = "PeripheralControl";
            this.Size = new System.Drawing.Size(392, 584);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label devicesLabel;
        private System.Windows.Forms.ListBox devicesBox;
        private System.Windows.Forms.Button removebutton;
        private System.Windows.Forms.Button addButton;
        private deviceTesterControl deviceTesterControl1;
    }
}
