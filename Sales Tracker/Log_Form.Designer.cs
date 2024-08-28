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
            RichTextBox = new RichTextBox();
            AutoScroll_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            BtnClear = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // RichTextBox
            // 
            RichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RichTextBox.BackColor = Color.FromArgb(250, 250, 250);
            RichTextBox.BorderStyle = BorderStyle.None;
            RichTextBox.Font = new Font("Segoe UI", 12F);
            RichTextBox.Location = new Point(18, 18);
            RichTextBox.Name = "RichTextBox";
            RichTextBox.ReadOnly = true;
            RichTextBox.Size = new Size(1342, 690);
            RichTextBox.TabIndex = 0;
            RichTextBox.Text = "";
            RichTextBox.LinkClicked += RichTextBox_LinkClicked;
            RichTextBox.TextChanged += RichTextBox_TextChanged;
            // 
            // AutoScroll_ComboBox
            // 
            AutoScroll_ComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AutoScroll_ComboBox.BackColor = Color.Transparent;
            AutoScroll_ComboBox.CustomizableEdges = customizableEdges1;
            AutoScroll_ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            AutoScroll_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            AutoScroll_ComboBox.FocusedColor = Color.FromArgb(94, 148, 255);
            AutoScroll_ComboBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            AutoScroll_ComboBox.Font = new Font("Segoe UI", 10);
            AutoScroll_ComboBox.ForeColor = Color.FromArgb(68, 88, 112);
            AutoScroll_ComboBox.ItemHeight = 39;
            AutoScroll_ComboBox.Items.AddRange(new object[] { "Enable autoscroll", "Disable autoscroll" });
            AutoScroll_ComboBox.Location = new Point(910, 737);
            AutoScroll_ComboBox.Name = "AutoScroll_ComboBox";
            AutoScroll_ComboBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            AutoScroll_ComboBox.Size = new Size(270, 45);
            AutoScroll_ComboBox.TabIndex = 1;
            // 
            // BtnClear
            // 
            BtnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnClear.BorderThickness = 1;
            BtnClear.CustomizableEdges = customizableEdges3;
            BtnClear.DisabledState.BorderColor = Color.DarkGray;
            BtnClear.DisabledState.CustomBorderColor = Color.DarkGray;
            BtnClear.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            BtnClear.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            BtnClear.FillColor = Color.White;
            BtnClear.Font = new Font("Segoe UI", 10);
            BtnClear.ForeColor = Color.Black;
            BtnClear.Location = new Point(1186, 737);
            BtnClear.Name = "BtnClear";
            BtnClear.ShadowDecoration.CustomizableEdges = customizableEdges4;
            BtnClear.Size = new Size(180, 45);
            BtnClear.TabIndex = 3;
            BtnClear.Text = "Clear";
            BtnClear.Click += BtnClear_Click;
            // 
            // Log_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(1378, 794);
            Controls.Add(BtnClear);
            Controls.Add(AutoScroll_ComboBox);
            Controls.Add(RichTextBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(800, 600);
            Name = "Log_Form";
            StartPosition = FormStartPosition.CenterScreen;
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