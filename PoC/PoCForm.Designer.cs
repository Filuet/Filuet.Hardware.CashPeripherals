
namespace PoC
{
    partial class PoCForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.getRoutesButton = new System.Windows.Forms.Button();
            this.payout2CashboxButton = new System.Windows.Forms.Button();
            this.extractButton = new System.Windows.Forms.Button();
            this.totalTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cashboxStockListBox = new System.Windows.Forms.ListBox();
            this.deviceComboBox = new System.Windows.Forms.ComboBox();
            this.cashboxStockLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.payoutStockListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Location = new System.Drawing.Point(4, 245);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(652, 266);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(582, 216);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(64, 23);
            this.resetButton.TabIndex = 11;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // getRoutesButton
            // 
            this.getRoutesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.getRoutesButton.Location = new System.Drawing.Point(472, 216);
            this.getRoutesButton.Name = "getRoutesButton";
            this.getRoutesButton.Size = new System.Drawing.Size(104, 23);
            this.getRoutesButton.TabIndex = 14;
            this.getRoutesButton.Text = "Get routes";
            this.getRoutesButton.UseVisualStyleBackColor = true;
            this.getRoutesButton.Click += new System.EventHandler(this.getRoutesButton_Click);
            // 
            // payout2CashboxButton
            // 
            this.payout2CashboxButton.Location = new System.Drawing.Point(125, 216);
            this.payout2CashboxButton.Name = "payout2CashboxButton";
            this.payout2CashboxButton.Size = new System.Drawing.Size(104, 23);
            this.payout2CashboxButton.TabIndex = 10;
            this.payout2CashboxButton.Text = "Payout2Cashbox";
            this.payout2CashboxButton.UseVisualStyleBackColor = true;
            this.payout2CashboxButton.Click += new System.EventHandler(this.payout2CashboxButton_Click);
            // 
            // extractButton
            // 
            this.extractButton.Location = new System.Drawing.Point(235, 216);
            this.extractButton.Name = "extractButton";
            this.extractButton.Size = new System.Drawing.Size(86, 23);
            this.extractButton.TabIndex = 21;
            this.extractButton.Text = "Extract";
            this.extractButton.UseVisualStyleBackColor = true;
            this.extractButton.Click += new System.EventHandler(this.extractButton_Click);
            // 
            // totalTextBox
            // 
            this.totalTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.totalTextBox.Location = new System.Drawing.Point(433, 12);
            this.totalTextBox.Name = "totalTextBox";
            this.totalTextBox.ReadOnly = true;
            this.totalTextBox.Size = new System.Drawing.Size(213, 23);
            this.totalTextBox.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(393, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "Total";
            // 
            // cashboxStockListBox
            // 
            this.cashboxStockListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cashboxStockListBox.FormattingEnabled = true;
            this.cashboxStockListBox.ItemHeight = 15;
            this.cashboxStockListBox.Location = new System.Drawing.Point(433, 41);
            this.cashboxStockListBox.Name = "cashboxStockListBox";
            this.cashboxStockListBox.Size = new System.Drawing.Size(213, 169);
            this.cashboxStockListBox.TabIndex = 20;
            // 
            // deviceComboBox
            // 
            this.deviceComboBox.FormattingEnabled = true;
            this.deviceComboBox.Location = new System.Drawing.Point(125, 12);
            this.deviceComboBox.Name = "deviceComboBox";
            this.deviceComboBox.Size = new System.Drawing.Size(196, 23);
            this.deviceComboBox.TabIndex = 13;
            this.deviceComboBox.SelectedIndexChanged += new System.EventHandler(this.deviceComboBox_SelectedIndexChanged);
            // 
            // cashboxStockLabel
            // 
            this.cashboxStockLabel.AutoSize = true;
            this.cashboxStockLabel.Location = new System.Drawing.Point(343, 41);
            this.cashboxStockLabel.Name = "cashboxStockLabel";
            this.cashboxStockLabel.Size = new System.Drawing.Size(84, 15);
            this.cashboxStockLabel.TabIndex = 19;
            this.cashboxStockLabel.Text = "Cashbox stock";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 15);
            this.label1.TabIndex = 12;
            this.label1.Text = "Choose the device";
            // 
            // payoutStockListBox
            // 
            this.payoutStockListBox.FormattingEnabled = true;
            this.payoutStockListBox.ItemHeight = 15;
            this.payoutStockListBox.Location = new System.Drawing.Point(125, 41);
            this.payoutStockListBox.Name = "payoutStockListBox";
            this.payoutStockListBox.Size = new System.Drawing.Size(196, 169);
            this.payoutStockListBox.TabIndex = 18;
            this.payoutStockListBox.SelectedIndexChanged += new System.EventHandler(this.payoutStockListBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 17;
            this.label2.Text = "Payout stock";
            // 
            // PoCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 516);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.getRoutesButton);
            this.Controls.Add(this.payout2CashboxButton);
            this.Controls.Add(this.extractButton);
            this.Controls.Add(this.totalTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cashboxStockListBox);
            this.Controls.Add(this.deviceComboBox);
            this.Controls.Add(this.cashboxStockLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.payoutStockListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.richTextBox1);
            this.Name = "PoCForm";
            this.Text = "Cash acceptance PoC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PoCForm_FormClosing);
            this.Load += new System.EventHandler(this.PoCForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button getRoutesButton;
        private System.Windows.Forms.Button payout2CashboxButton;
        private System.Windows.Forms.Button extractButton;
        private System.Windows.Forms.TextBox totalTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox cashboxStockListBox;
        private System.Windows.Forms.ComboBox deviceComboBox;
        private System.Windows.Forms.Label cashboxStockLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox payoutStockListBox;
        private System.Windows.Forms.Label label2;
    }
}

