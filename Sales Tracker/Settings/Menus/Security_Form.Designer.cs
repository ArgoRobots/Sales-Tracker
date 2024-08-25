
namespace Sales_Tracker.Settings.Menus
{
    partial class Security_Form
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
            Security_Label = new Label();
            Back_Panel = new Panel();
            AddPassword_Button = new Guna.UI2.WinForms.Guna2Button();
            EncryptFiles_CheckBox = new Guna.UI2.WinForms.Guna2CheckBox();
            guna2vSeparator1 = new Guna.UI2.WinForms.Guna2VSeparator();
            Back_Panel.SuspendLayout();
            SuspendLayout();
            // 
            // Security_Label
            // 
            Security_Label.AutoSize = true;
            Security_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            Security_Label.Location = new Point(57, 37);
            Security_Label.Margin = new Padding(6, 0, 6, 0);
            Security_Label.Name = "Security_Label";
            Security_Label.Size = new Size(123, 40);
            Security_Label.TabIndex = 326;
            Security_Label.Text = "Security";
            // 
            // Back_Panel
            // 
            Back_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Back_Panel.Controls.Add(AddPassword_Button);
            Back_Panel.Controls.Add(EncryptFiles_CheckBox);
            Back_Panel.Controls.Add(guna2vSeparator1);
            Back_Panel.Location = new Point(124, 348);
            Back_Panel.Margin = new Padding(4, 5, 4, 5);
            Back_Panel.Name = "Back_Panel";
            Back_Panel.Size = new Size(1000, 167);
            Back_Panel.TabIndex = 327;
            // 
            // AddPassword_Button
            // 
            AddPassword_Button.BorderColor = Color.LightGray;
            AddPassword_Button.BorderRadius = 2;
            AddPassword_Button.BorderThickness = 1;
            AddPassword_Button.CustomizableEdges = customizableEdges3;
            AddPassword_Button.FillColor = Color.FromArgb(250, 250, 250);
            AddPassword_Button.Font = new Font("Segoe UI", 9F);
            AddPassword_Button.ForeColor = Color.Black;
            AddPassword_Button.Location = new Point(375, 78);
            AddPassword_Button.Margin = new Padding(4, 7, 4, 7);
            AddPassword_Button.Name = "AddPassword_Button";
            AddPassword_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddPassword_Button.Size = new Size(250, 45);
            AddPassword_Button.TabIndex = 276;
            AddPassword_Button.Text = "Add password protection";
            AddPassword_Button.Click += AddPassword_Button_Click;
            // 
            // EncryptFiles_CheckBox
            // 
            EncryptFiles_CheckBox.Animated = true;
            EncryptFiles_CheckBox.AutoSize = true;
            EncryptFiles_CheckBox.CheckedState.BorderColor = Color.FromArgb(62, 101, 207);
            EncryptFiles_CheckBox.CheckedState.BorderRadius = 0;
            EncryptFiles_CheckBox.CheckedState.BorderThickness = 0;
            EncryptFiles_CheckBox.CheckedState.FillColor = Color.FromArgb(62, 101, 207);
            EncryptFiles_CheckBox.Font = new Font("Segoe UI", 9.75F);
            EncryptFiles_CheckBox.Location = new Point(221, 7);
            EncryptFiles_CheckBox.Margin = new Padding(4, 7, 4, 7);
            EncryptFiles_CheckBox.Name = "EncryptFiles_CheckBox";
            EncryptFiles_CheckBox.RightToLeft = RightToLeft.Yes;
            EncryptFiles_CheckBox.Size = new Size(307, 32);
            EncryptFiles_CheckBox.TabIndex = 275;
            EncryptFiles_CheckBox.Text = "Encrypt Argo Sales Tracker files";
            EncryptFiles_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            EncryptFiles_CheckBox.UncheckedState.BorderRadius = 0;
            EncryptFiles_CheckBox.UncheckedState.BorderThickness = 0;
            EncryptFiles_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            EncryptFiles_CheckBox.CheckedChanged += EncryptFiles_CheckBox_CheckedChanged;
            // 
            // guna2vSeparator1
            // 
            guna2vSeparator1.Location = new Point(493, 0);
            guna2vSeparator1.Margin = new Padding(4, 5, 4, 5);
            guna2vSeparator1.Name = "guna2vSeparator1";
            guna2vSeparator1.Size = new Size(14, 417);
            guna2vSeparator1.TabIndex = 274;
            guna2vSeparator1.Visible = false;
            // 
            // Security_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1250, 865);
            Controls.Add(Back_Panel);
            Controls.Add(Security_Label);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(6, 5, 6, 5);
            Name = "Security_Form";
            Shown += Security_Form_Shown;
            Back_Panel.ResumeLayout(false);
            Back_Panel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label Security_Label;
        private Panel Back_Panel;
        public Guna.UI2.WinForms.Guna2CheckBox EncryptFiles_CheckBox;
        private Guna.UI2.WinForms.Guna2VSeparator guna2vSeparator1;
        private Guna.UI2.WinForms.Guna2Button AddPassword_Button;
    }
}