
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
            Security_Label = new Label();
            Back_Panel = new Panel();
            EncryptFiles_CheckBox = new Guna.UI2.WinForms.Guna2CheckBox();
            guna2vSeparator1 = new Guna.UI2.WinForms.Guna2VSeparator();
            Back_Panel.SuspendLayout();
            SuspendLayout();
            // 
            // Security_Label
            // 
            Security_Label.AutoSize = true;
            Security_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            Security_Label.Location = new Point(40, 22);
            Security_Label.Margin = new Padding(4, 0, 4, 0);
            Security_Label.Name = "Security_Label";
            Security_Label.Size = new Size(81, 25);
            Security_Label.TabIndex = 326;
            Security_Label.Text = "Security";
            // 
            // Back_Panel
            // 
            Back_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Back_Panel.Controls.Add(EncryptFiles_CheckBox);
            Back_Panel.Controls.Add(guna2vSeparator1);
            Back_Panel.Location = new Point(87, 209);
            Back_Panel.Name = "Back_Panel";
            Back_Panel.Size = new Size(700, 100);
            Back_Panel.TabIndex = 327;
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
            EncryptFiles_CheckBox.Location = new Point(241, 4);
            EncryptFiles_CheckBox.Margin = new Padding(3, 4, 3, 4);
            EncryptFiles_CheckBox.Name = "EncryptFiles_CheckBox";
            EncryptFiles_CheckBox.RightToLeft = RightToLeft.Yes;
            EncryptFiles_CheckBox.Size = new Size(130, 21);
            EncryptFiles_CheckBox.TabIndex = 275;
            EncryptFiles_CheckBox.Text = "Encrypt Argo files";
            EncryptFiles_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            EncryptFiles_CheckBox.UncheckedState.BorderRadius = 0;
            EncryptFiles_CheckBox.UncheckedState.BorderThickness = 0;
            EncryptFiles_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            // 
            // guna2vSeparator1
            // 
            guna2vSeparator1.Location = new Point(345, 0);
            guna2vSeparator1.Name = "guna2vSeparator1";
            guna2vSeparator1.Size = new Size(10, 250);
            guna2vSeparator1.TabIndex = 274;
            guna2vSeparator1.Visible = false;
            // 
            // Security_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(875, 519);
            Controls.Add(Back_Panel);
            Controls.Add(Security_Label);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Security_Form";
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
    }
}