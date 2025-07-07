
namespace Sales_Tracker.Settings.Menus
{
    partial class Updates_Form
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
            UpToDate_Label = new Label();
            Updates_Label = new Label();
            LastCheck_Label = new Label();
            CheckForUpdates_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // UpToDate_Label
            // 
            UpToDate_Label.Anchor = AnchorStyles.Top;
            UpToDate_Label.AutoSize = true;
            UpToDate_Label.Font = new Font("Segoe UI", 15F);
            UpToDate_Label.Location = new Point(409, 338);
            UpToDate_Label.Name = "UpToDate_Label";
            UpToDate_Label.Size = new Size(433, 41);
            UpToDate_Label.TabIndex = 0;
            UpToDate_Label.Text = "Argo Sales Tracker is up to date";
            // 
            // Updates_Label
            // 
            Updates_Label.Anchor = AnchorStyles.Top;
            Updates_Label.AutoSize = true;
            Updates_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            Updates_Label.Location = new Point(563, 37);
            Updates_Label.Name = "Updates_Label";
            Updates_Label.Size = new Size(124, 40);
            Updates_Label.TabIndex = 0;
            Updates_Label.Text = "Updates";
            // 
            // LastCheck_Label
            // 
            LastCheck_Label.Anchor = AnchorStyles.Top;
            LastCheck_Label.AutoSize = true;
            LastCheck_Label.Font = new Font("Segoe UI", 10F);
            LastCheck_Label.Location = new Point(488, 383);
            LastCheck_Label.Name = "LastCheck_Label";
            LastCheck_Label.Size = new Size(274, 28);
            LastCheck_Label.TabIndex = 0;
            LastCheck_Label.Text = "Last checked: Today, 12:00 PM";
            // 
            // CheckForUpdates_Button
            // 
            CheckForUpdates_Button.Anchor = AnchorStyles.Top;
            CheckForUpdates_Button.BorderRadius = 3;
            CheckForUpdates_Button.CustomizableEdges = customizableEdges1;
            CheckForUpdates_Button.DisabledState.BorderColor = Color.DarkGray;
            CheckForUpdates_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            CheckForUpdates_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            CheckForUpdates_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            CheckForUpdates_Button.FillColor = Color.FromArgb(0, 103, 192);
            CheckForUpdates_Button.Font = new Font("Segoe UI", 9.75F);
            CheckForUpdates_Button.ForeColor = Color.White;
            CheckForUpdates_Button.Location = new Point(485, 477);
            CheckForUpdates_Button.Name = "CheckForUpdates_Button";
            CheckForUpdates_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            CheckForUpdates_Button.Size = new Size(280, 45);
            CheckForUpdates_Button.TabIndex = 1;
            CheckForUpdates_Button.Text = "Check for updates";
            CheckForUpdates_Button.Click += CheckForUpdates_Button_Click;
            // 
            // Updates_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(1250, 865);
            Controls.Add(CheckForUpdates_Button);
            Controls.Add(UpToDate_Label);
            Controls.Add(LastCheck_Label);
            Controls.Add(Updates_Label);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Updates_Form";
            Shown += Updates_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label UpToDate_Label;
        private System.Windows.Forms.Label Updates_Label;
        private System.Windows.Forms.Label LastCheck_Label;
        private Guna.UI2.WinForms.Guna2Button CheckForUpdates_Button;
    }
}