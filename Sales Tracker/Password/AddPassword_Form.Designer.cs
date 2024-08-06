namespace Sales_Tracker.Settings.Menus
{
    partial class AddPassword_Form
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
            AddPassword_Label = new Label();
            Password_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            SetPassword_Button = new Guna.UI2.WinForms.Guna2Button();
            LengthRequirement_Label = new Label();
            UppercaseRequirement_Label = new Label();
            DigitRequirement_Label = new Label();
            SpecialCharacterRequirement_Label = new Label();
            Message_Label = new Label();
            SuspendLayout();
            // 
            // AddPassword_Label
            // 
            AddPassword_Label.AutoSize = true;
            AddPassword_Label.Font = new Font("Segoe UI", 16F);
            AddPassword_Label.Location = new Point(167, 14);
            AddPassword_Label.Name = "AddPassword_Label";
            AddPassword_Label.Size = new Size(151, 30);
            AddPassword_Label.TabIndex = 3;
            AddPassword_Label.Text = "Add password";
            // 
            // Password_TextBox
            // 
            Password_TextBox.Anchor = AnchorStyles.Top;
            Password_TextBox.CustomizableEdges = customizableEdges1;
            Password_TextBox.DefaultText = "";
            Password_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Password_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Password_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Password_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Password_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Password_TextBox.Font = new Font("Segoe UI", 9F);
            Password_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Password_TextBox.Location = new Point(117, 81);
            Password_TextBox.MaxLength = 32;
            Password_TextBox.Name = "Password_TextBox";
            Password_TextBox.PasswordChar = '\0';
            Password_TextBox.PlaceholderText = "Password";
            Password_TextBox.SelectedText = "";
            Password_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Password_TextBox.ShortcutsEnabled = false;
            Password_TextBox.Size = new Size(250, 36);
            Password_TextBox.TabIndex = 55;
            Password_TextBox.TextChanged += Password_TextBox_TextChanged;
            Password_TextBox.KeyDown += Password_TextBox_KeyDown;
            // 
            // SetPassword_Button
            // 
            SetPassword_Button.Anchor = AnchorStyles.Top;
            SetPassword_Button.BackColor = Color.Transparent;
            SetPassword_Button.BorderColor = Color.LightGray;
            SetPassword_Button.BorderRadius = 2;
            SetPassword_Button.BorderThickness = 1;
            SetPassword_Button.CustomizableEdges = customizableEdges3;
            SetPassword_Button.Enabled = false;
            SetPassword_Button.FillColor = Color.White;
            SetPassword_Button.Font = new Font("Segoe UI", 9.5F);
            SetPassword_Button.ForeColor = Color.Black;
            SetPassword_Button.Location = new Point(167, 335);
            SetPassword_Button.Margin = new Padding(4, 3, 4, 3);
            SetPassword_Button.Name = "SetPassword_Button";
            SetPassword_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            SetPassword_Button.Size = new Size(150, 36);
            SetPassword_Button.TabIndex = 56;
            SetPassword_Button.Tag = "";
            SetPassword_Button.Text = "Set password";
            SetPassword_Button.Click += SetPassword_Button_Click;
            // 
            // LengthRequirement_Label
            // 
            LengthRequirement_Label.Anchor = AnchorStyles.Top;
            LengthRequirement_Label.AutoSize = true;
            LengthRequirement_Label.Font = new Font("Segoe UI", 12F);
            LengthRequirement_Label.Location = new Point(142, 132);
            LengthRequirement_Label.Name = "LengthRequirement_Label";
            LengthRequirement_Label.Size = new Size(149, 21);
            LengthRequirement_Label.TabIndex = 57;
            LengthRequirement_Label.Text = "At least 8 characters";
            // 
            // UppercaseRequirement_Label
            // 
            UppercaseRequirement_Label.Anchor = AnchorStyles.Top;
            UppercaseRequirement_Label.AutoSize = true;
            UppercaseRequirement_Label.Font = new Font("Segoe UI", 12F);
            UppercaseRequirement_Label.Location = new Point(142, 192);
            UppercaseRequirement_Label.Name = "UppercaseRequirement_Label";
            UppercaseRequirement_Label.Size = new Size(186, 21);
            UppercaseRequirement_Label.TabIndex = 58;
            UppercaseRequirement_Label.Text = "Contains uppercase letter";
            // 
            // DigitRequirement_Label
            // 
            DigitRequirement_Label.Anchor = AnchorStyles.Top;
            DigitRequirement_Label.AutoSize = true;
            DigitRequirement_Label.Font = new Font("Segoe UI", 12F);
            DigitRequirement_Label.Location = new Point(142, 162);
            DigitRequirement_Label.Name = "DigitRequirement_Label";
            DigitRequirement_Label.Size = new Size(130, 21);
            DigitRequirement_Label.TabIndex = 60;
            DigitRequirement_Label.Text = "Contains number";
            // 
            // SpecialCharacterRequirement_Label
            // 
            SpecialCharacterRequirement_Label.Anchor = AnchorStyles.Top;
            SpecialCharacterRequirement_Label.AutoSize = true;
            SpecialCharacterRequirement_Label.Font = new Font("Segoe UI", 12F);
            SpecialCharacterRequirement_Label.Location = new Point(142, 222);
            SpecialCharacterRequirement_Label.Name = "SpecialCharacterRequirement_Label";
            SpecialCharacterRequirement_Label.Size = new Size(190, 21);
            SpecialCharacterRequirement_Label.TabIndex = 61;
            SpecialCharacterRequirement_Label.Text = "Contains special character";
            // 
            // Message_Label
            // 
            Message_Label.Anchor = AnchorStyles.Top;
            Message_Label.AutoSize = true;
            Message_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Message_Label.Location = new Point(62, 279);
            Message_Label.Name = "Message_Label";
            Message_Label.Size = new Size(361, 40);
            Message_Label.TabIndex = 62;
            Message_Label.Text = "If you forget your password, all your data will be lost!\r\nMake sure you record it somewhere safe.";
            Message_Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AddPassword_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 381);
            Controls.Add(Message_Label);
            Controls.Add(SpecialCharacterRequirement_Label);
            Controls.Add(DigitRequirement_Label);
            Controls.Add(UppercaseRequirement_Label);
            Controls.Add(LengthRequirement_Label);
            Controls.Add(SetPassword_Button);
            Controls.Add(Password_TextBox);
            Controls.Add(AddPassword_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "AddPassword_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Shown += AddPassword_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label AddPassword_Label;
        private Guna.UI2.WinForms.Guna2TextBox Password_TextBox;
        private Guna.UI2.WinForms.Guna2Button SetPassword_Button;
        private Label LengthRequirement_Label;
        private Label UppercaseRequirement_Label;
        private Label DigitRequirement_Label;
        private Label SpecialCharacterRequirement_Label;
        private Label Message_Label;
    }
}