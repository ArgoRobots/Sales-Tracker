namespace Sales_Tracker
{
    partial class Log_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Log_Form));
            RichTextBox = new System.Windows.Forms.RichTextBox();
            AutoScroll_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            BtnClear = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // RichTextBox
            // 
            RichTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            RichTextBox.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RichTextBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            RichTextBox.Location = new System.Drawing.Point(12, 12);
            RichTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RichTextBox.Name = "RichTextBox";
            RichTextBox.ReadOnly = true;
            RichTextBox.Size = new System.Drawing.Size(891, 463);
            RichTextBox.TabIndex = 0;
            RichTextBox.Text = "";
            RichTextBox.LinkClicked += RichTextBox_LinkClicked;
            RichTextBox.TextChanged += RichTextBox_TextChanged;
            // 
            // AutoScroll_ComboBox
            // 
            AutoScroll_ComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            AutoScroll_ComboBox.BackColor = System.Drawing.Color.Transparent;
            AutoScroll_ComboBox.CustomizableEdges = customizableEdges1;
            AutoScroll_ComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            AutoScroll_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            AutoScroll_ComboBox.FocusedColor = System.Drawing.Color.FromArgb(94, 148, 255);
            AutoScroll_ComboBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(94, 148, 255);
            AutoScroll_ComboBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            AutoScroll_ComboBox.ForeColor = System.Drawing.Color.FromArgb(68, 88, 112);
            AutoScroll_ComboBox.ItemHeight = 24;
            AutoScroll_ComboBox.Items.AddRange(new object[] { "Enable autoscroll", "Disable autoscroll" });
            AutoScroll_ComboBox.Location = new System.Drawing.Point(600, 492);
            AutoScroll_ComboBox.Margin = new System.Windows.Forms.Padding(2);
            AutoScroll_ComboBox.Name = "AutoScroll_ComboBox";
            AutoScroll_ComboBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            AutoScroll_ComboBox.Size = new System.Drawing.Size(185, 30);
            AutoScroll_ComboBox.TabIndex = 1;
            // 
            // BtnClear
            // 
            BtnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            BtnClear.BorderThickness = 1;
            BtnClear.CustomizableEdges = customizableEdges3;
            BtnClear.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            BtnClear.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            BtnClear.DisabledState.FillColor = System.Drawing.Color.FromArgb(169, 169, 169);
            BtnClear.DisabledState.ForeColor = System.Drawing.Color.FromArgb(141, 141, 141);
            BtnClear.FillColor = System.Drawing.Color.White;
            BtnClear.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            BtnClear.ForeColor = System.Drawing.Color.Black;
            BtnClear.Location = new System.Drawing.Point(789, 492);
            BtnClear.Margin = new System.Windows.Forms.Padding(2);
            BtnClear.Name = "BtnClear";
            BtnClear.ShadowDecoration.CustomizableEdges = customizableEdges4;
            BtnClear.Size = new System.Drawing.Size(117, 30);
            BtnClear.TabIndex = 3;
            BtnClear.Text = "Clear";
            BtnClear.Click += BtnClear_Click;
            // 
            // Log_Form
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            ClientSize = new System.Drawing.Size(915, 532);
            Controls.Add(BtnClear);
            Controls.Add(AutoScroll_ComboBox);
            Controls.Add(RichTextBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(464, 340);
            Name = "Log_Form";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Log";
            Load += Log_form_Load;
            Shown += Log_form_Shown;
            ResumeLayout(false);
        }

        #endregion
        public System.Windows.Forms.RichTextBox RichTextBox;
        private Guna.UI2.WinForms.Guna2ComboBox AutoScroll_ComboBox;
        private Guna.UI2.WinForms.Guna2Button BtnClear;
    }
}