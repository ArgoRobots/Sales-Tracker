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
            Log_RichTextBox = new RichTextBox();
            AutoScroll_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            Clear_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // RichTextBox
            // 
            Log_RichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Log_RichTextBox.BackColor = Color.FromArgb(250, 250, 250);
            Log_RichTextBox.BorderStyle = BorderStyle.None;
            Log_RichTextBox.Font = new Font("Segoe UI", 12F);
            Log_RichTextBox.Location = new Point(18, 18);
            Log_RichTextBox.Name = "RichTextBox";
            Log_RichTextBox.ReadOnly = true;
            Log_RichTextBox.Size = new Size(1342, 690);
            Log_RichTextBox.TabIndex = 0;
            Log_RichTextBox.Text = "";
            Log_RichTextBox.LinkClicked += RichTextBox_LinkClicked;
            Log_RichTextBox.TextChanged += RichTextBox_TextChanged;
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
            AutoScroll_ComboBox.Font = new Font("Segoe UI", 10F);
            AutoScroll_ComboBox.ForeColor = Color.FromArgb(68, 88, 112);
            AutoScroll_ComboBox.ItemHeight = 39;
            AutoScroll_ComboBox.Items.AddRange(new object[] { "Enable autoscroll", "Disable autoscroll" });
            AutoScroll_ComboBox.Location = new Point(910, 737);
            AutoScroll_ComboBox.Name = "AutoScroll_ComboBox";
            AutoScroll_ComboBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            AutoScroll_ComboBox.Size = new Size(270, 45);
            AutoScroll_ComboBox.TabIndex = 1;
            // 
            // Clear_Button
            // 
            Clear_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Clear_Button.BorderThickness = 1;
            Clear_Button.CustomizableEdges = customizableEdges3;
            Clear_Button.DisabledState.BorderColor = Color.DarkGray;
            Clear_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Clear_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Clear_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Clear_Button.FillColor = Color.White;
            Clear_Button.Font = new Font("Segoe UI", 10F);
            Clear_Button.ForeColor = Color.Black;
            Clear_Button.Location = new Point(1186, 737);
            Clear_Button.Name = "Clear_Button";
            Clear_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Clear_Button.Size = new Size(180, 45);
            Clear_Button.TabIndex = 3;
            Clear_Button.Text = "Clear";
            Clear_Button.Click += ClearButton_Click;
            // 
            // Log_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(1378, 794);
            Controls.Add(Clear_Button);
            Controls.Add(AutoScroll_ComboBox);
            Controls.Add(Log_RichTextBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 600);
            Name = "Log_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Log";
            Load += Log_form_Load;
            Shown += Log_form_Shown;
            ResumeLayout(false);
        }

        #endregion
        public System.Windows.Forms.RichTextBox Log_RichTextBox;
        private Guna.UI2.WinForms.Guna2ComboBox AutoScroll_ComboBox;
        public Guna.UI2.WinForms.Guna2Button Clear_Button;
    }
}