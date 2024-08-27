
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Security_Label = new Label();
            Back_Panel = new Panel();
            EncryptFiles_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            AddPassword_Button = new Guna.UI2.WinForms.Guna2Button();
            EncryptFiles_Label = new Label();
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
            Back_Panel.Controls.Add(EncryptFiles_CheckBox);
            Back_Panel.Controls.Add(AddPassword_Button);
            Back_Panel.Controls.Add(EncryptFiles_Label);
            Back_Panel.Location = new Point(124, 362);
            Back_Panel.Margin = new Padding(4, 5, 4, 5);
            Back_Panel.Name = "Back_Panel";
            Back_Panel.Size = new Size(1000, 140);
            Back_Panel.TabIndex = 327;
            // 
            // EncryptFiles_CheckBox
            // 
            EncryptFiles_CheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EncryptFiles_CheckBox.Animated = true;
            EncryptFiles_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            EncryptFiles_CheckBox.CheckedState.BorderRadius = 2;
            EncryptFiles_CheckBox.CheckedState.BorderThickness = 0;
            EncryptFiles_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            EncryptFiles_CheckBox.CustomizableEdges = customizableEdges1;
            EncryptFiles_CheckBox.Location = new Point(635, 19);
            EncryptFiles_CheckBox.Name = "EncryptFiles_CheckBox";
            EncryptFiles_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            EncryptFiles_CheckBox.Size = new Size(20, 20);
            EncryptFiles_CheckBox.TabIndex = 329;
            EncryptFiles_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            EncryptFiles_CheckBox.UncheckedState.BorderRadius = 2;
            EncryptFiles_CheckBox.UncheckedState.BorderThickness = 0;
            EncryptFiles_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
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
            AddPassword_Button.Location = new Point(375, 80);
            AddPassword_Button.Margin = new Padding(4, 7, 4, 7);
            AddPassword_Button.Name = "AddPassword_Button";
            AddPassword_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddPassword_Button.Size = new Size(250, 45);
            AddPassword_Button.TabIndex = 276;
            AddPassword_Button.Text = "Add password protection";
            AddPassword_Button.Click += AddPassword_Button_Click;
            // 
            // EncryptFiles_Label
            // 
            EncryptFiles_Label.Anchor = AnchorStyles.Top;
            EncryptFiles_Label.AutoSize = true;
            EncryptFiles_Label.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            EncryptFiles_Label.Location = new Point(346, 10);
            EncryptFiles_Label.Margin = new Padding(0);
            EncryptFiles_Label.Name = "EncryptFiles_Label";
            EncryptFiles_Label.Padding = new Padding(5);
            EncryptFiles_Label.Size = new Size(291, 38);
            EncryptFiles_Label.TabIndex = 328;
            EncryptFiles_Label.Text = "Encrypt Argo Sales Tracker files";
            EncryptFiles_Label.Click += EncryptFiles_Label_Click;
            // 
            // Security_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
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
        private Guna.UI2.WinForms.Guna2Button AddPassword_Button;
        private Label EncryptFiles_Label;
        public Guna.UI2.WinForms.Guna2CustomCheckBox EncryptFiles_CheckBox;
    }
}