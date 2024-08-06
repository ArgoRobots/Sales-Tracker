namespace Sales_Tracker.Settings.Menus
{
    partial class PasswordManager_Form
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
            PasswordManager_Label = new Label();
            SuspendLayout();
            // 
            // PasswordManager_Label
            // 
            PasswordManager_Label.AutoSize = true;
            PasswordManager_Label.Font = new Font("Segoe UI", 16F);
            PasswordManager_Label.Location = new Point(145, 14);
            PasswordManager_Label.Name = "PasswordManager_Label";
            PasswordManager_Label.Size = new Size(195, 30);
            PasswordManager_Label.TabIndex = 2;
            PasswordManager_Label.Text = "Password manager";
            // 
            // PasswordManager_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 381);
            Controls.Add(PasswordManager_Label);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "PasswordManager_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Shown += PasswordManager_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label PasswordManager_Label;
    }
}