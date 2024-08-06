namespace Sales_Tracker.Password
{
    partial class EnterPassword_Form
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
            Enter_Button = new Guna.UI2.WinForms.Guna2Button();
            Password_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            EnterPassword_Label = new Label();
            SuspendLayout();
            // 
            // Enter_Button
            // 
            Enter_Button.Anchor = AnchorStyles.Top;
            Enter_Button.BackColor = Color.Transparent;
            Enter_Button.BorderColor = Color.LightGray;
            Enter_Button.BorderRadius = 2;
            Enter_Button.BorderThickness = 1;
            Enter_Button.CustomizableEdges = customizableEdges1;
            Enter_Button.FillColor = Color.White;
            Enter_Button.Font = new Font("Segoe UI", 9.5F);
            Enter_Button.ForeColor = Color.Black;
            Enter_Button.Location = new Point(167, 155);
            Enter_Button.Margin = new Padding(4, 3, 4, 3);
            Enter_Button.Name = "Enter_Button";
            Enter_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Enter_Button.Size = new Size(150, 36);
            Enter_Button.TabIndex = 59;
            Enter_Button.Tag = "";
            Enter_Button.Text = "Enter";
            Enter_Button.Click += Enter_Button_Click;
            // 
            // Password_TextBox
            // 
            Password_TextBox.Anchor = AnchorStyles.Top;
            Password_TextBox.CustomizableEdges = customizableEdges3;
            Password_TextBox.DefaultText = "";
            Password_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Password_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Password_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Password_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Password_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Password_TextBox.Font = new Font("Segoe UI", 9F);
            Password_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Password_TextBox.Location = new Point(117, 93);
            Password_TextBox.MaxLength = 32;
            Password_TextBox.Name = "Password_TextBox";
            Password_TextBox.PasswordChar = '\0';
            Password_TextBox.PlaceholderText = "Password";
            Password_TextBox.SelectedText = "";
            Password_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Password_TextBox.ShortcutsEnabled = false;
            Password_TextBox.Size = new Size(250, 36);
            Password_TextBox.TabIndex = 58;
            Password_TextBox.KeyDown += Password_TextBox_KeyDown;
            // 
            // EnterPassword_Label
            // 
            EnterPassword_Label.Anchor = AnchorStyles.Top;
            EnterPassword_Label.AutoSize = true;
            EnterPassword_Label.Font = new Font("Segoe UI", 16F);
            EnterPassword_Label.Location = new Point(162, 14);
            EnterPassword_Label.Name = "EnterPassword_Label";
            EnterPassword_Label.Size = new Size(161, 30);
            EnterPassword_Label.TabIndex = 57;
            EnterPassword_Label.Text = "Enter password";
            // 
            // EnterPassword_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 241);
            Controls.Add(Enter_Button);
            Controls.Add(Password_TextBox);
            Controls.Add(EnterPassword_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "EnterPassword_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Shown += EnterPassword_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Button Enter_Button;
        private Guna.UI2.WinForms.Guna2TextBox Password_TextBox;
        private Label EnterPassword_Label;
    }
}