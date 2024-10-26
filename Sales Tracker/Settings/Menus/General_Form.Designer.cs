
namespace Sales_Tracker.Settings.Menus
{
    partial class General_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            General_Label = new Label();
            Language_Label = new Label();
            MoreInformation_Button = new Guna.UI2.WinForms.Guna2Button();
            Currency_Label = new Label();
            ShowDebugInfo_Label = new Label();
            SendAnonymousInformation_Label = new Label();
            PurchaseReceipts_Label = new Label();
            SalesReceipts_Label = new Label();
            ColorTheme_Label = new Label();
            SalesReceipts_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            PurchaseReceipts_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            ShowDebugInfo_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            SendAnonymousInformation_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            Language_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ColorTheme_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            Currency_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            SuspendLayout();
            // 
            // General_Label
            // 
            General_Label.AutoSize = true;
            General_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            General_Label.Location = new Point(57, 37);
            General_Label.Name = "General_Label";
            General_Label.Size = new Size(118, 40);
            General_Label.TabIndex = 249;
            General_Label.Text = "General";
            // 
            // Language_Label
            // 
            Language_Label.Anchor = AnchorStyles.Top;
            Language_Label.AutoSize = true;
            Language_Label.Font = new Font("Segoe UI", 10F);
            Language_Label.Location = new Point(536, 269);
            Language_Label.Name = "Language_Label";
            Language_Label.Size = new Size(97, 28);
            Language_Label.TabIndex = 0;
            Language_Label.Text = "Language";
            Language_Label.Click += CloseAllPanels;
            // 
            // MoreInformation_Button
            // 
            MoreInformation_Button.Anchor = AnchorStyles.Top;
            MoreInformation_Button.BorderColor = Color.LightGray;
            MoreInformation_Button.BorderRadius = 2;
            MoreInformation_Button.BorderThickness = 1;
            MoreInformation_Button.CustomizableEdges = customizableEdges1;
            MoreInformation_Button.FillColor = Color.FromArgb(250, 250, 250);
            MoreInformation_Button.Font = new Font("Segoe UI", 9F);
            MoreInformation_Button.ForeColor = Color.Black;
            MoreInformation_Button.Location = new Point(668, 470);
            MoreInformation_Button.Name = "MoreInformation_Button";
            MoreInformation_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            MoreInformation_Button.Size = new Size(220, 45);
            MoreInformation_Button.TabIndex = 6;
            MoreInformation_Button.Text = "More information";
            MoreInformation_Button.Click += MoreInformation_Button_Click;
            // 
            // Currency_Label
            // 
            Currency_Label.Anchor = AnchorStyles.Top;
            Currency_Label.AutoSize = true;
            Currency_Label.Font = new Font("Segoe UI", 10F);
            Currency_Label.Location = new Point(479, 328);
            Currency_Label.Name = "Currency_Label";
            Currency_Label.Size = new Size(154, 28);
            Currency_Label.TabIndex = 0;
            Currency_Label.Text = "Default currency";
            Currency_Label.Click += CloseAllPanels;
            // 
            // ShowDebugInfo_Label
            // 
            ShowDebugInfo_Label.Anchor = AnchorStyles.Top;
            ShowDebugInfo_Label.AutoSize = true;
            ShowDebugInfo_Label.Font = new Font("Segoe UI", 10F);
            ShowDebugInfo_Label.Location = new Point(305, 427);
            ShowDebugInfo_Label.Name = "ShowDebugInfo_Label";
            ShowDebugInfo_Label.Padding = new Padding(5);
            ShowDebugInfo_Label.Size = new Size(328, 38);
            ShowDebugInfo_Label.TabIndex = 0;
            ShowDebugInfo_Label.Tag = "";
            ShowDebugInfo_Label.Text = "Show debug info in message boxes";
            ShowDebugInfo_Label.Click += ShowDebugInfo_Label_Click;
            // 
            // SendAnonymousInformation_Label
            // 
            SendAnonymousInformation_Label.Anchor = AnchorStyles.Top;
            SendAnonymousInformation_Label.AutoSize = true;
            SendAnonymousInformation_Label.Font = new Font("Segoe UI", 10F);
            SendAnonymousInformation_Label.Location = new Point(243, 473);
            SendAnonymousInformation_Label.Name = "SendAnonymousInformation_Label";
            SendAnonymousInformation_Label.Padding = new Padding(5);
            SendAnonymousInformation_Label.Size = new Size(390, 38);
            SendAnonymousInformation_Label.TabIndex = 0;
            SendAnonymousInformation_Label.Tag = "";
            SendAnonymousInformation_Label.Text = "Send anonymous statistics and usage data";
            SendAnonymousInformation_Label.Click += SendAnonymousInformation_Label_Click;
            // 
            // PurchaseReceipts_Label
            // 
            PurchaseReceipts_Label.Anchor = AnchorStyles.Top;
            PurchaseReceipts_Label.AutoSize = true;
            PurchaseReceipts_Label.Font = new Font("Segoe UI", 10F);
            PurchaseReceipts_Label.Location = new Point(306, 519);
            PurchaseReceipts_Label.Name = "PurchaseReceipts_Label";
            PurchaseReceipts_Label.Padding = new Padding(5);
            PurchaseReceipts_Label.Size = new Size(327, 38);
            PurchaseReceipts_Label.TabIndex = 0;
            PurchaseReceipts_Label.Tag = "";
            PurchaseReceipts_Label.Text = "Make purchase receipts mandatory";
            PurchaseReceipts_Label.Click += PurchaseReceipts_Label_Click;
            // 
            // SalesReceipts_Label
            // 
            SalesReceipts_Label.Anchor = AnchorStyles.Top;
            SalesReceipts_Label.AutoSize = true;
            SalesReceipts_Label.Font = new Font("Segoe UI", 10F);
            SalesReceipts_Label.Location = new Point(343, 565);
            SalesReceipts_Label.Name = "SalesReceipts_Label";
            SalesReceipts_Label.Padding = new Padding(5);
            SalesReceipts_Label.Size = new Size(290, 38);
            SalesReceipts_Label.TabIndex = 0;
            SalesReceipts_Label.Tag = "";
            SalesReceipts_Label.Text = "Make sales receipts mandatory";
            SalesReceipts_Label.Click += SalesReceipts_Label_Click;
            // 
            // ColorTheme_Label
            // 
            ColorTheme_Label.Anchor = AnchorStyles.Top;
            ColorTheme_Label.AutoSize = true;
            ColorTheme_Label.Font = new Font("Segoe UI", 10F);
            ColorTheme_Label.Location = new Point(513, 385);
            ColorTheme_Label.Name = "ColorTheme_Label";
            ColorTheme_Label.Size = new Size(120, 28);
            ColorTheme_Label.TabIndex = 0;
            ColorTheme_Label.Text = "Color theme";
            ColorTheme_Label.Click += CloseAllPanels;
            // 
            // SalesReceipts_CheckBox
            // 
            SalesReceipts_CheckBox.Anchor = AnchorStyles.Top;
            SalesReceipts_CheckBox.Animated = true;
            SalesReceipts_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            SalesReceipts_CheckBox.CheckedState.BorderRadius = 2;
            SalesReceipts_CheckBox.CheckedState.BorderThickness = 0;
            SalesReceipts_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            SalesReceipts_CheckBox.CustomizableEdges = customizableEdges3;
            SalesReceipts_CheckBox.Location = new Point(631, 574);
            SalesReceipts_CheckBox.Name = "SalesReceipts_CheckBox";
            SalesReceipts_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges4;
            SalesReceipts_CheckBox.Size = new Size(20, 20);
            SalesReceipts_CheckBox.TabIndex = 8;
            SalesReceipts_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            SalesReceipts_CheckBox.UncheckedState.BorderRadius = 2;
            SalesReceipts_CheckBox.UncheckedState.BorderThickness = 0;
            SalesReceipts_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            SalesReceipts_CheckBox.Click += CloseAllPanels;
            // 
            // PurchaseReceipts_CheckBox
            // 
            PurchaseReceipts_CheckBox.Anchor = AnchorStyles.Top;
            PurchaseReceipts_CheckBox.Animated = true;
            PurchaseReceipts_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            PurchaseReceipts_CheckBox.CheckedState.BorderRadius = 2;
            PurchaseReceipts_CheckBox.CheckedState.BorderThickness = 0;
            PurchaseReceipts_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            PurchaseReceipts_CheckBox.CustomizableEdges = customizableEdges5;
            PurchaseReceipts_CheckBox.Location = new Point(631, 528);
            PurchaseReceipts_CheckBox.Name = "PurchaseReceipts_CheckBox";
            PurchaseReceipts_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            PurchaseReceipts_CheckBox.Size = new Size(20, 20);
            PurchaseReceipts_CheckBox.TabIndex = 7;
            PurchaseReceipts_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            PurchaseReceipts_CheckBox.UncheckedState.BorderRadius = 2;
            PurchaseReceipts_CheckBox.UncheckedState.BorderThickness = 0;
            PurchaseReceipts_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            PurchaseReceipts_CheckBox.Click += CloseAllPanels;
            // 
            // ShowDebugInfo_CheckBox
            // 
            ShowDebugInfo_CheckBox.Anchor = AnchorStyles.Top;
            ShowDebugInfo_CheckBox.Animated = true;
            ShowDebugInfo_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            ShowDebugInfo_CheckBox.CheckedState.BorderRadius = 2;
            ShowDebugInfo_CheckBox.CheckedState.BorderThickness = 0;
            ShowDebugInfo_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            ShowDebugInfo_CheckBox.CustomizableEdges = customizableEdges7;
            ShowDebugInfo_CheckBox.Location = new Point(631, 436);
            ShowDebugInfo_CheckBox.Name = "ShowDebugInfo_CheckBox";
            ShowDebugInfo_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges8;
            ShowDebugInfo_CheckBox.Size = new Size(20, 20);
            ShowDebugInfo_CheckBox.TabIndex = 4;
            ShowDebugInfo_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            ShowDebugInfo_CheckBox.UncheckedState.BorderRadius = 2;
            ShowDebugInfo_CheckBox.UncheckedState.BorderThickness = 0;
            ShowDebugInfo_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            ShowDebugInfo_CheckBox.Click += CloseAllPanels;
            // 
            // SendAnonymousInformation_CheckBox
            // 
            SendAnonymousInformation_CheckBox.Anchor = AnchorStyles.Top;
            SendAnonymousInformation_CheckBox.Animated = true;
            SendAnonymousInformation_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            SendAnonymousInformation_CheckBox.CheckedState.BorderRadius = 2;
            SendAnonymousInformation_CheckBox.CheckedState.BorderThickness = 0;
            SendAnonymousInformation_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            SendAnonymousInformation_CheckBox.CustomizableEdges = customizableEdges9;
            SendAnonymousInformation_CheckBox.Location = new Point(631, 482);
            SendAnonymousInformation_CheckBox.Name = "SendAnonymousInformation_CheckBox";
            SendAnonymousInformation_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges10;
            SendAnonymousInformation_CheckBox.Size = new Size(20, 20);
            SendAnonymousInformation_CheckBox.TabIndex = 5;
            SendAnonymousInformation_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            SendAnonymousInformation_CheckBox.UncheckedState.BorderRadius = 2;
            SendAnonymousInformation_CheckBox.UncheckedState.BorderThickness = 0;
            SendAnonymousInformation_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            SendAnonymousInformation_CheckBox.Click += CloseAllPanels;
            // 
            // Language_TextBox
            // 
            Language_TextBox.Anchor = AnchorStyles.Top;
            Language_TextBox.CustomizableEdges = customizableEdges11;
            Language_TextBox.DefaultText = "";
            Language_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Language_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Language_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Language_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Language_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Language_TextBox.Font = new Font("Segoe UI", 11F);
            Language_TextBox.ForeColor = SystemColors.ControlText;
            Language_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Language_TextBox.Location = new Point(633, 267);
            Language_TextBox.Margin = new Padding(4, 5, 4, 5);
            Language_TextBox.Name = "Language_TextBox";
            Language_TextBox.PasswordChar = '\0';
            Language_TextBox.PlaceholderText = "";
            Language_TextBox.SelectedText = "";
            Language_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges12;
            Language_TextBox.ShortcutsEnabled = false;
            Language_TextBox.Size = new Size(255, 45);
            Language_TextBox.TabIndex = 1;
            // 
            // ColorTheme_ComboBox
            // 
            ColorTheme_ComboBox.Anchor = AnchorStyles.Top;
            ColorTheme_ComboBox.BackColor = Color.Transparent;
            ColorTheme_ComboBox.CustomizableEdges = customizableEdges13;
            ColorTheme_ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            ColorTheme_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ColorTheme_ComboBox.FocusedColor = Color.FromArgb(94, 148, 255);
            ColorTheme_ComboBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ColorTheme_ComboBox.Font = new Font("Segoe UI", 10F);
            ColorTheme_ComboBox.ForeColor = Color.Black;
            ColorTheme_ComboBox.ItemHeight = 39;
            ColorTheme_ComboBox.Location = new Point(633, 377);
            ColorTheme_ComboBox.Name = "ColorTheme_ComboBox";
            ColorTheme_ComboBox.ShadowDecoration.CustomizableEdges = customizableEdges14;
            ColorTheme_ComboBox.Size = new Size(255, 45);
            ColorTheme_ComboBox.TabIndex = 3;
            ColorTheme_ComboBox.Click += CloseAllPanels;
            // 
            // Currency_TextBox
            // 
            Currency_TextBox.Anchor = AnchorStyles.Top;
            Currency_TextBox.CustomizableEdges = customizableEdges15;
            Currency_TextBox.DefaultText = "";
            Currency_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Currency_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Currency_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Currency_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Currency_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Currency_TextBox.Font = new Font("Segoe UI", 11F);
            Currency_TextBox.ForeColor = SystemColors.ControlText;
            Currency_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Currency_TextBox.Location = new Point(633, 322);
            Currency_TextBox.Margin = new Padding(4, 5, 4, 5);
            Currency_TextBox.Name = "Currency_TextBox";
            Currency_TextBox.PasswordChar = '\0';
            Currency_TextBox.PlaceholderText = "";
            Currency_TextBox.SelectedText = "";
            Currency_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges16;
            Currency_TextBox.ShortcutsEnabled = false;
            Currency_TextBox.Size = new Size(255, 45);
            Currency_TextBox.TabIndex = 2;
            // 
            // General_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(1250, 865);
            Controls.Add(Currency_TextBox);
            Controls.Add(Language_TextBox);
            Controls.Add(SendAnonymousInformation_CheckBox);
            Controls.Add(ShowDebugInfo_CheckBox);
            Controls.Add(PurchaseReceipts_CheckBox);
            Controls.Add(SalesReceipts_CheckBox);
            Controls.Add(ColorTheme_ComboBox);
            Controls.Add(General_Label);
            Controls.Add(Language_Label);
            Controls.Add(ShowDebugInfo_Label);
            Controls.Add(MoreInformation_Button);
            Controls.Add(SendAnonymousInformation_Label);
            Controls.Add(PurchaseReceipts_Label);
            Controls.Add(SalesReceipts_Label);
            Controls.Add(ColorTheme_Label);
            Controls.Add(Currency_Label);
            FormBorderStyle = FormBorderStyle.None;
            Name = "General_Form";
            Shown += General_form_Shown;
            Click += CloseAllPanels;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label General_Label;
        private System.Windows.Forms.Label Language_Label;
        private Guna.UI2.WinForms.Guna2Button MoreInformation_Button;
        private System.Windows.Forms.Label Currency_Label;
        private Label ColorTheme_Label;
        private Label ShowDebugInfo_Label;
        private Label SendAnonymousInformation_Label;
        private Label PurchaseReceipts_Label;
        private Label SalesReceipts_Label;
        public Guna.UI2.WinForms.Guna2CustomCheckBox ShowDebugInfo_CheckBox;
        public Guna.UI2.WinForms.Guna2CustomCheckBox SalesReceipts_CheckBox;
        public Guna.UI2.WinForms.Guna2CustomCheckBox PurchaseReceipts_CheckBox;
        public Guna.UI2.WinForms.Guna2CustomCheckBox SendAnonymousInformation_CheckBox;
        public Guna.UI2.WinForms.Guna2TextBox Language_TextBox;
        public Guna.UI2.WinForms.Guna2ComboBox ColorTheme_ComboBox;
        public Guna.UI2.WinForms.Guna2TextBox Currency_TextBox;
    }
}