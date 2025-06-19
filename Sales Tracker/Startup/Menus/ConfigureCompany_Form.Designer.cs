
namespace Sales_Tracker.Startup.Menus
{
    partial class ConfigureCompany_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges19 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges20 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges21 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges22 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges23 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges24 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Back_Button = new Guna.UI2.WinForms.Guna2Button();
            Create_Button = new Guna.UI2.WinForms.Guna2Button();
            ConfigureNewCompany_Label = new Label();
            CompanyName_Label = new Label();
            CompanyName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Directory_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Directory_Label = new Label();
            WarningName_Label = new Label();
            WarningDir_Label = new Label();
            WarningDir_PictureBox = new PictureBox();
            WarningName_PictureBox = new PictureBox();
            ThreeDots_Button = new Guna.UI2.WinForms.Guna2Button();
            Currency_Label = new Label();
            Currency_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ((System.ComponentModel.ISupportInitialize)WarningDir_PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)WarningName_PictureBox).BeginInit();
            SuspendLayout();
            // 
            // Back_Button
            // 
            Back_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Back_Button.BorderColor = Color.LightGray;
            Back_Button.BorderRadius = 2;
            Back_Button.BorderThickness = 1;
            Back_Button.CustomizableEdges = customizableEdges13;
            Back_Button.FillColor = Color.White;
            Back_Button.Font = new Font("Segoe UI", 10F);
            Back_Button.ForeColor = Color.Black;
            Back_Button.Location = new Point(844, 735);
            Back_Button.Name = "Back_Button";
            Back_Button.ShadowDecoration.CustomizableEdges = customizableEdges14;
            Back_Button.Size = new Size(200, 50);
            Back_Button.TabIndex = 7;
            Back_Button.Text = "Back";
            Back_Button.Click += Back_Button_Click;
            // 
            // Create_Button
            // 
            Create_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Create_Button.BorderColor = Color.LightGray;
            Create_Button.BorderRadius = 2;
            Create_Button.BorderThickness = 1;
            Create_Button.CustomizableEdges = customizableEdges15;
            Create_Button.FillColor = Color.White;
            Create_Button.Font = new Font("Segoe UI", 10F);
            Create_Button.ForeColor = Color.Black;
            Create_Button.Location = new Point(1056, 735);
            Create_Button.Name = "Create_Button";
            Create_Button.ShadowDecoration.CustomizableEdges = customizableEdges16;
            Create_Button.Size = new Size(200, 50);
            Create_Button.TabIndex = 8;
            Create_Button.Text = "Create";
            Create_Button.Click += Create_Button_Click;
            // 
            // ConfigureNewCompany_Label
            // 
            ConfigureNewCompany_Label.AutoSize = true;
            ConfigureNewCompany_Label.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            ConfigureNewCompany_Label.Location = new Point(30, 15);
            ConfigureNewCompany_Label.Name = "ConfigureNewCompany_Label";
            ConfigureNewCompany_Label.Size = new Size(450, 45);
            ConfigureNewCompany_Label.TabIndex = 0;
            ConfigureNewCompany_Label.Text = "Configure your new company";
            ConfigureNewCompany_Label.Click += CloseAllPanels;
            // 
            // CompanyName_Label
            // 
            CompanyName_Label.AutoSize = true;
            CompanyName_Label.Font = new Font("Segoe UI", 12F);
            CompanyName_Label.Location = new Point(48, 113);
            CompanyName_Label.Name = "CompanyName_Label";
            CompanyName_Label.Size = new Size(183, 32);
            CompanyName_Label.TabIndex = 0;
            CompanyName_Label.Text = "Company name";
            CompanyName_Label.Click += CloseAllPanels;
            // 
            // CompanyName_TextBox
            // 
            CompanyName_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CompanyName_TextBox.CustomizableEdges = customizableEdges17;
            CompanyName_TextBox.DefaultText = "";
            CompanyName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            CompanyName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            CompanyName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            CompanyName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            CompanyName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            CompanyName_TextBox.Font = new Font("Segoe UI", 11F);
            CompanyName_TextBox.ForeColor = SystemColors.ControlText;
            CompanyName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            CompanyName_TextBox.Location = new Point(48, 150);
            CompanyName_TextBox.Margin = new Padding(4, 5, 4, 5);
            CompanyName_TextBox.Name = "CompanyName_TextBox";
            CompanyName_TextBox.PlaceholderText = "";
            CompanyName_TextBox.SelectedText = "";
            CompanyName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges18;
            CompanyName_TextBox.ShortcutsEnabled = false;
            CompanyName_TextBox.Size = new Size(1135, 60);
            CompanyName_TextBox.TabIndex = 1;
            CompanyName_TextBox.TextChanged += CompanyName_TextChanged;
            CompanyName_TextBox.Click += CloseAllPanels;
            // 
            // Directory_TextBox
            // 
            Directory_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Directory_TextBox.CustomizableEdges = customizableEdges19;
            Directory_TextBox.DefaultText = "";
            Directory_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Directory_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Directory_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Directory_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Directory_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Directory_TextBox.Font = new Font("Segoe UI", 11F);
            Directory_TextBox.ForeColor = SystemColors.ControlText;
            Directory_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Directory_TextBox.Location = new Point(48, 330);
            Directory_TextBox.Margin = new Padding(4, 5, 4, 5);
            Directory_TextBox.Name = "Directory_TextBox";
            Directory_TextBox.PlaceholderText = "";
            Directory_TextBox.SelectedText = "";
            Directory_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges20;
            Directory_TextBox.ShortcutsEnabled = false;
            Directory_TextBox.Size = new Size(1135, 60);
            Directory_TextBox.TabIndex = 2;
            Directory_TextBox.TextChanged += Directory_textBox_TextChanged;
            Directory_TextBox.Click += CloseAllPanels;
            // 
            // Directory_Label
            // 
            Directory_Label.AutoSize = true;
            Directory_Label.Font = new Font("Segoe UI", 12F);
            Directory_Label.Location = new Point(48, 293);
            Directory_Label.Name = "Directory_Label";
            Directory_Label.Size = new Size(111, 32);
            Directory_Label.TabIndex = 0;
            Directory_Label.Text = "Directory";
            Directory_Label.Click += CloseAllPanels;
            // 
            // WarningName_Label
            // 
            WarningName_Label.AutoSize = true;
            WarningName_Label.Font = new Font("Segoe UI", 10F);
            WarningName_Label.Location = new Point(94, 218);
            WarningName_Label.Name = "WarningName_Label";
            WarningName_Label.Size = new Size(86, 28);
            WarningName_Label.TabIndex = 0;
            WarningName_Label.Text = "Warning";
            WarningName_Label.Visible = false;
            WarningName_Label.Click += CloseAllPanels;
            // 
            // WarningDir_Label
            // 
            WarningDir_Label.AutoSize = true;
            WarningDir_Label.Font = new Font("Segoe UI", 10F);
            WarningDir_Label.Location = new Point(94, 398);
            WarningDir_Label.Name = "WarningDir_Label";
            WarningDir_Label.Size = new Size(86, 28);
            WarningDir_Label.TabIndex = 0;
            WarningDir_Label.Text = "Warning";
            WarningDir_Label.Visible = false;
            WarningDir_Label.Click += CloseAllPanels;
            // 
            // WarningDir_PictureBox
            // 
            WarningDir_PictureBox.Image = Properties.Resources.ExclamationMark;
            WarningDir_PictureBox.Location = new Point(60, 398);
            WarningDir_PictureBox.Name = "WarningDir_PictureBox";
            WarningDir_PictureBox.Size = new Size(28, 28);
            WarningDir_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningDir_PictureBox.TabIndex = 35;
            WarningDir_PictureBox.TabStop = false;
            WarningDir_PictureBox.Visible = false;
            WarningDir_PictureBox.Click += CloseAllPanels;
            // 
            // WarningName_PictureBox
            // 
            WarningName_PictureBox.Image = Properties.Resources.ExclamationMark;
            WarningName_PictureBox.Location = new Point(60, 218);
            WarningName_PictureBox.Name = "WarningName_PictureBox";
            WarningName_PictureBox.Size = new Size(28, 28);
            WarningName_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningName_PictureBox.TabIndex = 33;
            WarningName_PictureBox.TabStop = false;
            WarningName_PictureBox.Visible = false;
            WarningName_PictureBox.Click += CloseAllPanels;
            // 
            // ThreeDots_Button
            // 
            ThreeDots_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ThreeDots_Button.BorderColor = Color.FromArgb(213, 218, 223);
            ThreeDots_Button.BorderThickness = 1;
            ThreeDots_Button.CustomizableEdges = customizableEdges21;
            ThreeDots_Button.FillColor = Color.White;
            ThreeDots_Button.Font = new Font("Segoe UI", 9F);
            ThreeDots_Button.ForeColor = Color.White;
            ThreeDots_Button.Image = Properties.Resources.ThreeDotsBlack;
            ThreeDots_Button.Location = new Point(1190, 330);
            ThreeDots_Button.Name = "ThreeDots_Button";
            ThreeDots_Button.ShadowDecoration.CustomizableEdges = customizableEdges22;
            ThreeDots_Button.Size = new Size(60, 60);
            ThreeDots_Button.TabIndex = 3;
            ThreeDots_Button.Click += ThreeDots_Button_Click;
            // 
            // Currency_Label
            // 
            Currency_Label.AutoSize = true;
            Currency_Label.Font = new Font("Segoe UI", 12F);
            Currency_Label.Location = new Point(48, 475);
            Currency_Label.Name = "Currency_Label";
            Currency_Label.Size = new Size(190, 32);
            Currency_Label.TabIndex = 273;
            Currency_Label.Text = "Default currency";
            Currency_Label.Click += CloseAllPanels;
            // 
            // Currency_TextBox
            // 
            Currency_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Currency_TextBox.CustomizableEdges = customizableEdges23;
            Currency_TextBox.DefaultText = "";
            Currency_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Currency_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Currency_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Currency_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Currency_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Currency_TextBox.Font = new Font("Segoe UI", 11F);
            Currency_TextBox.ForeColor = SystemColors.ControlText;
            Currency_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Currency_TextBox.Location = new Point(48, 512);
            Currency_TextBox.Margin = new Padding(4, 5, 4, 5);
            Currency_TextBox.Name = "Currency_TextBox";
            Currency_TextBox.PlaceholderText = "";
            Currency_TextBox.SelectedText = "";
            Currency_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges24;
            Currency_TextBox.ShortcutsEnabled = false;
            Currency_TextBox.Size = new Size(255, 50);
            Currency_TextBox.TabIndex = 275;
            // 
            // ConfigureCompany_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(1320, 825);
            Controls.Add(Currency_TextBox);
            Controls.Add(Currency_Label);
            Controls.Add(WarningDir_PictureBox);
            Controls.Add(WarningDir_Label);
            Controls.Add(WarningName_PictureBox);
            Controls.Add(WarningName_Label);
            Controls.Add(ThreeDots_Button);
            Controls.Add(Directory_TextBox);
            Controls.Add(Directory_Label);
            Controls.Add(CompanyName_TextBox);
            Controls.Add(ConfigureNewCompany_Label);
            Controls.Add(CompanyName_Label);
            Controls.Add(Back_Button);
            Controls.Add(Create_Button);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Name = "ConfigureCompany_Form";
            StartPosition = FormStartPosition.CenterScreen;
            Shown += ConfigureCompany_Form_Shown;
            Click += ConfigureCompany_Form_Click;
            ((System.ComponentModel.ISupportInitialize)WarningDir_PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)WarningName_PictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Guna.UI2.WinForms.Guna2Button Create_Button;
        private System.Windows.Forms.Label ConfigureNewCompany_Label;
        private System.Windows.Forms.Label CompanyName_Label;
        private Guna.UI2.WinForms.Guna2TextBox CompanyName_TextBox;
        private Guna.UI2.WinForms.Guna2TextBox Directory_TextBox;
        private System.Windows.Forms.Label Directory_Label;
        private Guna.UI2.WinForms.Guna2Button ThreeDots_Button;
        private System.Windows.Forms.Label WarningName_Label;
        private System.Windows.Forms.PictureBox WarningName_PictureBox;
        private System.Windows.Forms.PictureBox WarningDir_PictureBox;
        private System.Windows.Forms.Label WarningDir_Label;
        public Guna.UI2.WinForms.Guna2Button Back_Button;
        private Label Currency_Label;
        private Guna.UI2.WinForms.Guna2TextBox Currency_TextBox;
    }
}