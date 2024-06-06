namespace Sales_Tracker
{
    partial class MainMenu_Form
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
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenu_Form));
            MainTop_Panel = new Guna.UI2.WinForms.Guna2Panel();
            ColorTheme_label = new Label();
            DarkMode_ToggleSwitch = new Guna.UI2.WinForms.Guna2ToggleSwitch();
            ManageProducts_Button = new Guna.UI2.WinForms.Guna2Button();
            label1 = new Label();
            MessagePanel_timer = new System.Windows.Forms.Timer(components);
            Top_Panel = new Panel();
            Saved_Label = new Label();
            Help_Button = new Guna.UI2.WinForms.Guna2Button();
            Save_Button = new Guna.UI2.WinForms.Guna2Button();
            File_Button = new Guna.UI2.WinForms.Guna2Button();
            Main_Panel = new Guna.UI2.WinForms.Guna2Panel();
            HideMenu_timer = new System.Windows.Forms.Timer(components);
            DataGridView = new Guna.UI2.WinForms.Guna2DataGridView();
            MainTop_Panel.SuspendLayout();
            Top_Panel.SuspendLayout();
            Main_Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DataGridView).BeginInit();
            SuspendLayout();
            // 
            // MainTop_Panel
            // 
            MainTop_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MainTop_Panel.BackColor = Color.FromArgb(242, 242, 242);
            MainTop_Panel.Controls.Add(ColorTheme_label);
            MainTop_Panel.Controls.Add(DarkMode_ToggleSwitch);
            MainTop_Panel.Controls.Add(ManageProducts_Button);
            MainTop_Panel.Controls.Add(label1);
            MainTop_Panel.CustomizableEdges = customizableEdges5;
            MainTop_Panel.Location = new Point(0, 30);
            MainTop_Panel.Name = "MainTop_Panel";
            MainTop_Panel.ShadowDecoration.CustomizableEdges = customizableEdges6;
            MainTop_Panel.Size = new Size(1604, 80);
            MainTop_Panel.TabIndex = 6;
            MainTop_Panel.Click += CloseAllPanels;
            // 
            // ColorTheme_label
            // 
            ColorTheme_label.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ColorTheme_label.AutoSize = true;
            ColorTheme_label.BackColor = Color.Transparent;
            ColorTheme_label.Font = new Font("Segoe UI", 11.25F);
            ColorTheme_label.Location = new Point(1299, 30);
            ColorTheme_label.Name = "ColorTheme_label";
            ColorTheme_label.Size = new Size(83, 20);
            ColorTheme_label.TabIndex = 11;
            ColorTheme_label.Text = "Dark mode";
            // 
            // DarkMode_ToggleSwitch
            // 
            DarkMode_ToggleSwitch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DarkMode_ToggleSwitch.Animated = true;
            DarkMode_ToggleSwitch.AutoRoundedCorners = true;
            DarkMode_ToggleSwitch.BackColor = Color.Transparent;
            DarkMode_ToggleSwitch.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            DarkMode_ToggleSwitch.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            DarkMode_ToggleSwitch.CheckedState.InnerBorderColor = Color.White;
            DarkMode_ToggleSwitch.CheckedState.InnerColor = Color.White;
            DarkMode_ToggleSwitch.CustomizableEdges = customizableEdges1;
            DarkMode_ToggleSwitch.Location = new Point(1388, 30);
            DarkMode_ToggleSwitch.Name = "DarkMode_ToggleSwitch";
            DarkMode_ToggleSwitch.ShadowDecoration.CustomizableEdges = customizableEdges2;
            DarkMode_ToggleSwitch.Size = new Size(40, 20);
            DarkMode_ToggleSwitch.TabIndex = 10;
            DarkMode_ToggleSwitch.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            DarkMode_ToggleSwitch.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            DarkMode_ToggleSwitch.UncheckedState.InnerBorderColor = Color.White;
            DarkMode_ToggleSwitch.UncheckedState.InnerColor = Color.White;
            DarkMode_ToggleSwitch.CheckedChanged += DarkMode_ToggleSwitch_CheckedChanged;
            // 
            // ManageProducts_Button
            // 
            ManageProducts_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ManageProducts_Button.BackColor = Color.Transparent;
            ManageProducts_Button.BorderColor = Color.LightGray;
            ManageProducts_Button.BorderRadius = 2;
            ManageProducts_Button.BorderThickness = 1;
            ManageProducts_Button.CustomizableEdges = customizableEdges3;
            ManageProducts_Button.FillColor = Color.White;
            ManageProducts_Button.Font = new Font("Segoe UI", 9.5F);
            ManageProducts_Button.ForeColor = Color.Black;
            ManageProducts_Button.Location = new Point(1448, 24);
            ManageProducts_Button.Margin = new Padding(4, 3, 4, 3);
            ManageProducts_Button.Name = "ManageProducts_Button";
            ManageProducts_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            ManageProducts_Button.Size = new Size(143, 32);
            ManageProducts_Button.TabIndex = 9;
            ManageProducts_Button.Text = "Manage products";
            ManageProducts_Button.Click += ManageProducts_Button_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
            label1.Location = new Point(12, 24);
            label1.Name = "label1";
            label1.Size = new Size(185, 32);
            label1.TabIndex = 1;
            label1.Text = "Company name";
            // 
            // MessagePanel_timer
            // 
            MessagePanel_timer.Interval = 4500;
            MessagePanel_timer.Tick += MessagePanelTimer_Tick;
            // 
            // Top_Panel
            // 
            Top_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Top_Panel.BackColor = Color.FromArgb(204, 204, 204);
            Top_Panel.Controls.Add(Saved_Label);
            Top_Panel.Controls.Add(Help_Button);
            Top_Panel.Controls.Add(Save_Button);
            Top_Panel.Controls.Add(File_Button);
            Top_Panel.Location = new Point(0, 0);
            Top_Panel.Name = "Top_Panel";
            Top_Panel.Size = new Size(1604, 30);
            Top_Panel.TabIndex = 0;
            Top_Panel.Click += CloseAllPanels;
            // 
            // Saved_Label
            // 
            Saved_Label.AutoSize = true;
            Saved_Label.Font = new Font("Segoe UI", 11.25F);
            Saved_Label.Location = new Point(78, 5);
            Saved_Label.Name = "Saved_Label";
            Saved_Label.Size = new Size(49, 20);
            Saved_Label.TabIndex = 2;
            Saved_Label.Text = "Saved";
            Saved_Label.Visible = false;
            // 
            // Help_Button
            // 
            Help_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Help_Button.CustomizableEdges = customizableEdges7;
            Help_Button.DisabledState.BorderColor = Color.DarkGray;
            Help_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Help_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Help_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Help_Button.FillColor = Color.FromArgb(204, 204, 204);
            Help_Button.Font = new Font("Segoe UI", 9F);
            Help_Button.ForeColor = Color.White;
            Help_Button.HoverState.FillColor = Color.FromArgb(187, 187, 187);
            Help_Button.Image = Properties.Resources.HelpGray;
            Help_Button.Location = new Point(1574, 0);
            Help_Button.Name = "Help_Button";
            Help_Button.PressedColor = Color.Empty;
            Help_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            Help_Button.Size = new Size(30, 30);
            Help_Button.TabIndex = 5;
            Help_Button.Click += Help_Button_Click;
            // 
            // Save_Button
            // 
            Save_Button.BorderColor = Color.Empty;
            Save_Button.CustomizableEdges = customizableEdges9;
            Save_Button.DisabledState.BorderColor = Color.DarkGray;
            Save_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Save_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Save_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Save_Button.FillColor = Color.Empty;
            Save_Button.Font = new Font("Segoe UI", 9F);
            Save_Button.ForeColor = Color.White;
            Save_Button.HoverState.FillColor = Color.FromArgb(187, 187, 187);
            Save_Button.Image = Properties.Resources.SaveGray;
            Save_Button.ImageSize = new Size(18, 18);
            Save_Button.Location = new Point(42, 0);
            Save_Button.Name = "Save_Button";
            Save_Button.ShadowDecoration.CustomizableEdges = customizableEdges10;
            Save_Button.Size = new Size(30, 30);
            Save_Button.TabIndex = 4;
            Save_Button.Click += Save_Button_Click;
            Save_Button.MouseDown += Save_Button_MouseDown;
            Save_Button.MouseUp += Save_Button_MouseUp;
            // 
            // File_Button
            // 
            File_Button.BorderColor = Color.Empty;
            File_Button.CustomizableEdges = customizableEdges11;
            File_Button.DisabledState.BorderColor = Color.DarkGray;
            File_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            File_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            File_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            File_Button.FillColor = Color.Empty;
            File_Button.Font = new Font("Segoe UI", 9F);
            File_Button.ForeColor = Color.White;
            File_Button.HoverState.FillColor = Color.FromArgb(187, 187, 187);
            File_Button.Image = Properties.Resources.FileGray;
            File_Button.ImageSize = new Size(35, 25);
            File_Button.Location = new Point(0, 0);
            File_Button.Name = "File_Button";
            File_Button.ShadowDecoration.CustomizableEdges = customizableEdges12;
            File_Button.Size = new Size(42, 30);
            File_Button.TabIndex = 3;
            File_Button.Click += File_Button_Click;
            // 
            // Main_Panel
            // 
            Main_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Main_Panel.BackColor = Color.FromArgb(250, 250, 250);
            Main_Panel.Controls.Add(DataGridView);
            Main_Panel.CustomizableEdges = customizableEdges13;
            Main_Panel.Location = new Point(0, 110);
            Main_Panel.Name = "Main_Panel";
            Main_Panel.ShadowDecoration.CustomizableEdges = customizableEdges14;
            Main_Panel.Size = new Size(1604, 930);
            Main_Panel.TabIndex = 8;
            Main_Panel.Click += CloseAllPanels;
            // 
            // HideMenu_timer
            // 
            HideMenu_timer.Interval = 800;
            HideMenu_timer.Tick += HideMenu_timer_Tick;
            // 
            // DataGridView
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            DataGridView.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            DataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            DataGridView.GridColor = Color.FromArgb(231, 229, 255);
            DataGridView.Location = new Point(151, 521);
            DataGridView.Name = "DataGridView";
            DataGridView.RowHeadersVisible = false;
            DataGridView.Size = new Size(1303, 305);
            DataGridView.TabIndex = 0;
            DataGridView.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            DataGridView.ThemeStyle.AlternatingRowsStyle.Font = null;
            DataGridView.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            DataGridView.ThemeStyle.BackColor = Color.White;
            DataGridView.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            DataGridView.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            DataGridView.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            DataGridView.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            DataGridView.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            DataGridView.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridView.ThemeStyle.HeaderStyle.Height = 4;
            DataGridView.ThemeStyle.ReadOnly = false;
            DataGridView.ThemeStyle.RowsStyle.BackColor = Color.White;
            DataGridView.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            DataGridView.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            DataGridView.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            DataGridView.ThemeStyle.RowsStyle.Height = 25;
            DataGridView.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            DataGridView.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // MainMenu_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1604, 1041);
            Controls.Add(Top_Panel);
            Controls.Add(MainTop_Panel);
            Controls.Add(Main_Panel);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(1000, 670);
            Name = "MainMenu_Form";
            Text = "Argo Sales Tracker";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainMenu_form_FormClosing;
            Shown += MainMenu_form_Shown;
            ResizeBegin += MainMenu_form_ResizeBegin;
            KeyDown += MainMenu_form_KeyDown;
            Resize += MainMenu_form_Resize;
            MainTop_Panel.ResumeLayout(false);
            MainTop_Panel.PerformLayout();
            Top_Panel.ResumeLayout(false);
            Top_Panel.PerformLayout();
            Main_Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Timer MessagePanel_timer;
        public Guna.UI2.WinForms.Guna2Panel MainTop_Panel;
        public System.Windows.Forms.Panel Top_Panel;
        public Guna.UI2.WinForms.Guna2Button File_Button;
        public Guna.UI2.WinForms.Guna2Button Save_Button;
        public Guna.UI2.WinForms.Guna2Button Help_Button;
        public Guna.UI2.WinForms.Guna2Panel Main_Panel;
        public System.Windows.Forms.Timer HideMenu_timer;
        public System.Windows.Forms.Label Saved_Label;
        private Label label1;
        private Guna.UI2.WinForms.Guna2Button ManageProducts_Button;
        private Guna.UI2.WinForms.Guna2ToggleSwitch DarkMode_ToggleSwitch;
        private Label ColorTheme_label;
        private Guna.UI2.WinForms.Guna2DataGridView DataGridView;
    }
}