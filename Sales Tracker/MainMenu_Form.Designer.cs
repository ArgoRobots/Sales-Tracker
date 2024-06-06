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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenu_Form));
            MainTop_Panel = new Guna.UI2.WinForms.Guna2Panel();
            MessagePanel_timer = new System.Windows.Forms.Timer(components);
            Top_Panel = new Panel();
            Saved_Label = new Label();
            Help_Button = new Guna.UI2.WinForms.Guna2Button();
            Save_Button = new Guna.UI2.WinForms.Guna2Button();
            File_Button = new Guna.UI2.WinForms.Guna2Button();
            Main_Panel = new Guna.UI2.WinForms.Guna2Panel();
            HideMenu_timer = new System.Windows.Forms.Timer(components);
            Top_Panel.SuspendLayout();
            SuspendLayout();
            // 
            // MainTop_Panel
            // 
            MainTop_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MainTop_Panel.BackColor = Color.FromArgb(242, 242, 242);
            MainTop_Panel.CustomizableEdges = customizableEdges1;
            MainTop_Panel.Location = new Point(0, 30);
            MainTop_Panel.Name = "MainTop_Panel";
            MainTop_Panel.ShadowDecoration.CustomizableEdges = customizableEdges2;
            MainTop_Panel.Size = new Size(1604, 80);
            MainTop_Panel.TabIndex = 6;
            MainTop_Panel.Click += CloseAllPanels;
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
            Help_Button.CustomizableEdges = customizableEdges3;
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
            Help_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Help_Button.Size = new Size(30, 30);
            Help_Button.TabIndex = 5;
            Help_Button.Click += Help_Button_Click;
            // 
            // Save_Button
            // 
            Save_Button.BorderColor = Color.Empty;
            Save_Button.CustomizableEdges = customizableEdges5;
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
            Save_Button.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Save_Button.Size = new Size(30, 30);
            Save_Button.TabIndex = 4;
            Save_Button.Click += Save_Button_Click;
            Save_Button.MouseDown += Save_Button_MouseDown;
            Save_Button.MouseUp += Save_Button_MouseUp;
            // 
            // File_Button
            // 
            File_Button.BorderColor = Color.Empty;
            File_Button.CustomizableEdges = customizableEdges7;
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
            File_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            File_Button.Size = new Size(42, 30);
            File_Button.TabIndex = 3;
            File_Button.Click += File_Button_Click;
            // 
            // Main_Panel
            // 
            Main_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Main_Panel.BackColor = Color.FromArgb(250, 250, 250);
            Main_Panel.CustomizableEdges = customizableEdges9;
            Main_Panel.Location = new Point(0, 110);
            Main_Panel.Name = "Main_Panel";
            Main_Panel.ShadowDecoration.CustomizableEdges = customizableEdges10;
            Main_Panel.Size = new Size(1604, 930);
            Main_Panel.TabIndex = 8;
            Main_Panel.Click += CloseAllPanels;
            // 
            // HideMenu_timer
            // 
            HideMenu_timer.Interval = 800;
            HideMenu_timer.Tick += HideMenu_timer_Tick;
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
            Text = "Argo Studio";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainMenu_form_FormClosing;
            Shown += MainMenu_form_Shown;
            ResizeBegin += MainMenu_form_ResizeBegin;
            KeyDown += MainMenu_form_KeyDown;
            Resize += MainMenu_form_Resize;
            Top_Panel.ResumeLayout(false);
            Top_Panel.PerformLayout();
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
    }
}