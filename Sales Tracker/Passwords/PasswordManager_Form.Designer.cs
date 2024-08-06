namespace Sales_Tracker.Passwords
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            PasswordManager_Label = new Label();
            Modify_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            Remove_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            CurrentPassword_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Update_Button = new Guna.UI2.WinForms.Guna2Button();
            Message_Label = new Label();
            SpecialCharacterRequirement_Label = new Label();
            NumberRequirement_Label = new Label();
            UppercaseRequirement_Label = new Label();
            LengthRequirement_Label = new Label();
            NewPassword_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            SuspendLayout();
            // 
            // PasswordManager_Label
            // 
            PasswordManager_Label.Anchor = AnchorStyles.Top;
            PasswordManager_Label.AutoSize = true;
            PasswordManager_Label.Font = new Font("Segoe UI", 16F);
            PasswordManager_Label.Location = new Point(145, 14);
            PasswordManager_Label.Name = "PasswordManager_Label";
            PasswordManager_Label.Size = new Size(195, 30);
            PasswordManager_Label.TabIndex = 2;
            PasswordManager_Label.Text = "Password manager";
            // 
            // Modify_RadioButton
            // 
            Modify_RadioButton.Anchor = AnchorStyles.Top;
            Modify_RadioButton.AutoSize = true;
            Modify_RadioButton.Checked = true;
            Modify_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Modify_RadioButton.CheckedState.BorderThickness = 0;
            Modify_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Modify_RadioButton.CheckedState.InnerColor = Color.White;
            Modify_RadioButton.CheckedState.InnerOffset = -4;
            Modify_RadioButton.Font = new Font("Segoe UI", 11F);
            Modify_RadioButton.Location = new Point(129, 64);
            Modify_RadioButton.Name = "Modify_RadioButton";
            Modify_RadioButton.Size = new Size(74, 24);
            Modify_RadioButton.TabIndex = 5;
            Modify_RadioButton.TabStop = true;
            Modify_RadioButton.Text = "Modify";
            Modify_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Modify_RadioButton.UncheckedState.BorderThickness = 2;
            Modify_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Modify_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Modify_RadioButton.CheckedChanged += Modify_RadioButton_CheckedChanged;
            // 
            // Remove_RadioButton
            // 
            Remove_RadioButton.Anchor = AnchorStyles.Top;
            Remove_RadioButton.AutoSize = true;
            Remove_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Remove_RadioButton.CheckedState.BorderThickness = 0;
            Remove_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Remove_RadioButton.CheckedState.InnerColor = Color.White;
            Remove_RadioButton.CheckedState.InnerOffset = -4;
            Remove_RadioButton.Font = new Font("Segoe UI", 11F);
            Remove_RadioButton.Location = new Point(275, 64);
            Remove_RadioButton.Name = "Remove_RadioButton";
            Remove_RadioButton.Size = new Size(81, 24);
            Remove_RadioButton.TabIndex = 6;
            Remove_RadioButton.Text = "Remove";
            Remove_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Remove_RadioButton.UncheckedState.BorderThickness = 2;
            Remove_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Remove_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Remove_RadioButton.CheckedChanged += Remove_RadioButton_CheckedChanged;
            // 
            // CurrentPassword_TextBox
            // 
            CurrentPassword_TextBox.Anchor = AnchorStyles.Top;
            CurrentPassword_TextBox.CustomizableEdges = customizableEdges1;
            CurrentPassword_TextBox.DefaultText = "";
            CurrentPassword_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            CurrentPassword_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            CurrentPassword_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            CurrentPassword_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            CurrentPassword_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            CurrentPassword_TextBox.Font = new Font("Segoe UI", 9F);
            CurrentPassword_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            CurrentPassword_TextBox.Location = new Point(117, 113);
            CurrentPassword_TextBox.MaxLength = 32;
            CurrentPassword_TextBox.Name = "CurrentPassword_TextBox";
            CurrentPassword_TextBox.PasswordChar = '\0';
            CurrentPassword_TextBox.PlaceholderText = "Current password";
            CurrentPassword_TextBox.SelectedText = "";
            CurrentPassword_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            CurrentPassword_TextBox.ShortcutsEnabled = false;
            CurrentPassword_TextBox.Size = new Size(250, 36);
            CurrentPassword_TextBox.TabIndex = 7;
            CurrentPassword_TextBox.KeyDown += CurrentPassword_TextBox_KeyDown;
            // 
            // Update_Button
            // 
            Update_Button.Anchor = AnchorStyles.Bottom;
            Update_Button.BackColor = Color.Transparent;
            Update_Button.BorderColor = Color.LightGray;
            Update_Button.BorderRadius = 2;
            Update_Button.BorderThickness = 1;
            Update_Button.CustomizableEdges = customizableEdges3;
            Update_Button.Enabled = false;
            Update_Button.FillColor = Color.White;
            Update_Button.Font = new Font("Segoe UI", 9.5F);
            Update_Button.ForeColor = Color.Black;
            Update_Button.Location = new Point(167, 413);
            Update_Button.Margin = new Padding(4, 3, 4, 3);
            Update_Button.Name = "Update_Button";
            Update_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Update_Button.Size = new Size(150, 36);
            Update_Button.TabIndex = 8;
            Update_Button.Tag = "";
            Update_Button.Text = "Update password";
            Update_Button.Click += Update_Button_Click;
            // 
            // Message_Label
            // 
            Message_Label.Anchor = AnchorStyles.Top;
            Message_Label.AutoSize = true;
            Message_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Message_Label.Location = new Point(62, 353);
            Message_Label.Name = "Message_Label";
            Message_Label.Size = new Size(361, 40);
            Message_Label.TabIndex = 68;
            Message_Label.Text = "If you forget your password, all your data will be lost!\r\nMake sure you record it somewhere safe.";
            Message_Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SpecialCharacterRequirement_Label
            // 
            SpecialCharacterRequirement_Label.Anchor = AnchorStyles.Top;
            SpecialCharacterRequirement_Label.AutoSize = true;
            SpecialCharacterRequirement_Label.Font = new Font("Segoe UI", 12F);
            SpecialCharacterRequirement_Label.Location = new Point(147, 296);
            SpecialCharacterRequirement_Label.Name = "SpecialCharacterRequirement_Label";
            SpecialCharacterRequirement_Label.Size = new Size(190, 21);
            SpecialCharacterRequirement_Label.TabIndex = 67;
            SpecialCharacterRequirement_Label.Text = "Contains special character";
            // 
            // NumberRequirement_Label
            // 
            NumberRequirement_Label.Anchor = AnchorStyles.Top;
            NumberRequirement_Label.AutoSize = true;
            NumberRequirement_Label.Font = new Font("Segoe UI", 12F);
            NumberRequirement_Label.Location = new Point(147, 236);
            NumberRequirement_Label.Name = "NumberRequirement_Label";
            NumberRequirement_Label.Size = new Size(130, 21);
            NumberRequirement_Label.TabIndex = 66;
            NumberRequirement_Label.Text = "Contains number";
            // 
            // UppercaseRequirement_Label
            // 
            UppercaseRequirement_Label.Anchor = AnchorStyles.Top;
            UppercaseRequirement_Label.AutoSize = true;
            UppercaseRequirement_Label.Font = new Font("Segoe UI", 12F);
            UppercaseRequirement_Label.Location = new Point(147, 266);
            UppercaseRequirement_Label.Name = "UppercaseRequirement_Label";
            UppercaseRequirement_Label.Size = new Size(186, 21);
            UppercaseRequirement_Label.TabIndex = 65;
            UppercaseRequirement_Label.Text = "Contains uppercase letter";
            // 
            // LengthRequirement_Label
            // 
            LengthRequirement_Label.Anchor = AnchorStyles.Top;
            LengthRequirement_Label.AutoSize = true;
            LengthRequirement_Label.Font = new Font("Segoe UI", 12F);
            LengthRequirement_Label.Location = new Point(147, 206);
            LengthRequirement_Label.Name = "LengthRequirement_Label";
            LengthRequirement_Label.Size = new Size(149, 21);
            LengthRequirement_Label.TabIndex = 64;
            LengthRequirement_Label.Text = "At least 8 characters";
            // 
            // NewPassword_TextBox
            // 
            NewPassword_TextBox.Anchor = AnchorStyles.Top;
            NewPassword_TextBox.CustomizableEdges = customizableEdges5;
            NewPassword_TextBox.DefaultText = "";
            NewPassword_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            NewPassword_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            NewPassword_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            NewPassword_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            NewPassword_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            NewPassword_TextBox.Font = new Font("Segoe UI", 9F);
            NewPassword_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            NewPassword_TextBox.Location = new Point(117, 155);
            NewPassword_TextBox.MaxLength = 32;
            NewPassword_TextBox.Name = "NewPassword_TextBox";
            NewPassword_TextBox.PasswordChar = '\0';
            NewPassword_TextBox.PlaceholderText = "New password";
            NewPassword_TextBox.SelectedText = "";
            NewPassword_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            NewPassword_TextBox.ShortcutsEnabled = false;
            NewPassword_TextBox.Size = new Size(250, 36);
            NewPassword_TextBox.TabIndex = 63;
            NewPassword_TextBox.TextChanged += NewPassword_TextBox_TextChanged;
            // 
            // PasswordManager_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 461);
            Controls.Add(Message_Label);
            Controls.Add(SpecialCharacterRequirement_Label);
            Controls.Add(NumberRequirement_Label);
            Controls.Add(UppercaseRequirement_Label);
            Controls.Add(LengthRequirement_Label);
            Controls.Add(NewPassword_TextBox);
            Controls.Add(CurrentPassword_TextBox);
            Controls.Add(Update_Button);
            Controls.Add(Remove_RadioButton);
            Controls.Add(Modify_RadioButton);
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
        public Guna.UI2.WinForms.Guna2RadioButton Modify_RadioButton;
        public Guna.UI2.WinForms.Guna2RadioButton Remove_RadioButton;
        private Guna.UI2.WinForms.Guna2TextBox CurrentPassword_TextBox;
        private Guna.UI2.WinForms.Guna2Button Update_Button;
        private Label Message_Label;
        private Label SpecialCharacterRequirement_Label;
        private Label NumberRequirement_Label;
        private Label UppercaseRequirement_Label;
        private Label LengthRequirement_Label;
        private Guna.UI2.WinForms.Guna2TextBox NewPassword_TextBox;
    }
}