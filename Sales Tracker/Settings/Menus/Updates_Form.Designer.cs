
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Status_Label = new Label();
            Updates_Label = new Label();
            LastCheck_Label = new Label();
            Updates_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // Status_Label
            // 
            Status_Label.Anchor = AnchorStyles.Top;
            Status_Label.AutoSize = true;
            Status_Label.Font = new Font("Segoe UI", 15F);
            Status_Label.Location = new Point(208, 170);
            Status_Label.Name = "Status_Label";
            Status_Label.Size = new Size(433, 41);
            Status_Label.TabIndex = 0;
            Status_Label.Text = "Argo Sales Tracker is up to date";
            // 
            // Updates_Label
            // 
            Updates_Label.Anchor = AnchorStyles.Top;
            Updates_Label.AutoSize = true;
            Updates_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            Updates_Label.Location = new Point(362, 37);
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
            LastCheck_Label.Location = new Point(287, 215);
            LastCheck_Label.Name = "LastCheck_Label";
            LastCheck_Label.Size = new Size(274, 28);
            LastCheck_Label.TabIndex = 0;
            LastCheck_Label.Text = "Last checked: Today, 12:00 PM";
            // 
            // Updates_Button
            // 
            Updates_Button.Anchor = AnchorStyles.Top;
            Updates_Button.BorderRadius = 3;
            Updates_Button.CustomizableEdges = customizableEdges3;
            Updates_Button.DisabledState.BorderColor = Color.DarkGray;
            Updates_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Updates_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Updates_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Updates_Button.FillColor = Color.FromArgb(0, 103, 192);
            Updates_Button.Font = new Font("Segoe UI", 9.75F);
            Updates_Button.ForeColor = Color.White;
            Updates_Button.Location = new Point(284, 309);
            Updates_Button.Name = "Updates_Button";
            Updates_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Updates_Button.Size = new Size(280, 45);
            Updates_Button.TabIndex = 1;
            Updates_Button.Text = "Update";
            Updates_Button.Click += Update_Button_Click;
            // 
            // Updates_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(848, 544);
            Controls.Add(Updates_Button);
            Controls.Add(Status_Label);
            Controls.Add(LastCheck_Label);
            Controls.Add(Updates_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "Updates_Form";
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += Updates_Form_FormClosing;
            Shown += Updates_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label Status_Label;
        private System.Windows.Forms.Label Updates_Label;
        private System.Windows.Forms.Label LastCheck_Label;
        private Guna.UI2.WinForms.Guna2Button Updates_Button;
    }
}