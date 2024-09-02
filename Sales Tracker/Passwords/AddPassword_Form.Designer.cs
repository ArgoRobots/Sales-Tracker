namespace Sales_Tracker.Passwords
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
            NumberRequirement_Label = new Label();
            SpecialCharacterRequirement_Label = new Label();
            Message_Label = new Label();
            SuspendLayout();
            // 
            // AddPassword_Label
            // 
            AddPassword_Label.Anchor = AnchorStyles.Top;
            AddPassword_Label.AutoSize = true;
            AddPassword_Label.Font = new Font("Segoe UI", 16F);
            AddPassword_Label.Location = new Point(228, 23);
            AddPassword_Label.Margin = new Padding(4, 0, 4, 0);
            AddPassword_Label.Name = "AddPassword_Label";
            AddPassword_Label.Size = new Size(223, 45);
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
            Password_TextBox.Location = new Point(164, 135);
            Password_TextBox.Margin = new Padding(6, 8, 6, 8);
            Password_TextBox.MaxLength = 32;
            Password_TextBox.Name = "Password_TextBox";
            Password_TextBox.PasswordChar = '\0';
            Password_TextBox.PlaceholderText = "Password";
            Password_TextBox.SelectedText = "";
            Password_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Password_TextBox.ShortcutsEnabled = false;
            Password_TextBox.Size = new Size(350, 50);
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
            SetPassword_Button.Font = new Font("Segoe UI", 10F);
            SetPassword_Button.ForeColor = Color.Black;
            SetPassword_Button.Location = new Point(214, 558);
            SetPassword_Button.Margin = new Padding(6, 5, 6, 5);
            SetPassword_Button.Name = "SetPassword_Button";
            SetPassword_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            SetPassword_Button.Size = new Size(250, 50);
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
            LengthRequirement_Label.Location = new Point(194, 220);
            LengthRequirement_Label.Margin = new Padding(4, 0, 4, 0);
            LengthRequirement_Label.Name = "LengthRequirement_Label";
            LengthRequirement_Label.Size = new Size(227, 32);
            LengthRequirement_Label.TabIndex = 57;
            LengthRequirement_Label.Text = "At least 8 characters";
            // 
            // UppercaseRequirement_Label
            // 
            UppercaseRequirement_Label.Anchor = AnchorStyles.Top;
            UppercaseRequirement_Label.AutoSize = true;
            UppercaseRequirement_Label.Font = new Font("Segoe UI", 12F);
            UppercaseRequirement_Label.Location = new Point(194, 320);
            UppercaseRequirement_Label.Margin = new Padding(4, 0, 4, 0);
            UppercaseRequirement_Label.Name = "UppercaseRequirement_Label";
            UppercaseRequirement_Label.Size = new Size(286, 32);
            UppercaseRequirement_Label.TabIndex = 58;
            UppercaseRequirement_Label.Text = "Contains uppercase letter";
            // 
            // NumberRequirement_Label
            // 
            NumberRequirement_Label.Anchor = AnchorStyles.Top;
            NumberRequirement_Label.AutoSize = true;
            NumberRequirement_Label.Font = new Font("Segoe UI", 12F);
            NumberRequirement_Label.Location = new Point(194, 270);
            NumberRequirement_Label.Margin = new Padding(4, 0, 4, 0);
            NumberRequirement_Label.Name = "NumberRequirement_Label";
            NumberRequirement_Label.Size = new Size(198, 32);
            NumberRequirement_Label.TabIndex = 60;
            NumberRequirement_Label.Text = "Contains number";
            // 
            // SpecialCharacterRequirement_Label
            // 
            SpecialCharacterRequirement_Label.Anchor = AnchorStyles.Top;
            SpecialCharacterRequirement_Label.AutoSize = true;
            SpecialCharacterRequirement_Label.Font = new Font("Segoe UI", 12F);
            SpecialCharacterRequirement_Label.Location = new Point(194, 370);
            SpecialCharacterRequirement_Label.Margin = new Padding(4, 0, 4, 0);
            SpecialCharacterRequirement_Label.Name = "SpecialCharacterRequirement_Label";
            SpecialCharacterRequirement_Label.Size = new Size(290, 32);
            SpecialCharacterRequirement_Label.TabIndex = 61;
            SpecialCharacterRequirement_Label.Text = "Contains special character";
            // 
            // Message_Label
            // 
            Message_Label.Anchor = AnchorStyles.Top;
            Message_Label.AutoSize = true;
            Message_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Message_Label.Location = new Point(62, 465);
            Message_Label.Margin = new Padding(4, 0, 4, 0);
            Message_Label.Name = "Message_Label";
            Message_Label.Size = new Size(554, 62);
            Message_Label.TabIndex = 62;
            Message_Label.Text = "If you forget your password, all your data will be lost!\r\nMake sure you record it somewhere safe.";
            Message_Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AddPassword_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(678, 634);
            Controls.Add(Message_Label);
            Controls.Add(SpecialCharacterRequirement_Label);
            Controls.Add(NumberRequirement_Label);
            Controls.Add(UppercaseRequirement_Label);
            Controls.Add(LengthRequirement_Label);
            Controls.Add(SetPassword_Button);
            Controls.Add(Password_TextBox);
            Controls.Add(AddPassword_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(700, 690);
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
        private Label NumberRequirement_Label;
        private Label SpecialCharacterRequirement_Label;
        private Label Message_Label;
    }
}