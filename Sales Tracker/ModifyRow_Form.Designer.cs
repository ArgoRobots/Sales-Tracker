namespace Sales_Tracker
{
    partial class ModifyRow_Form
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
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyRow_Form));
                ModifyRow_Label = new Label();
                Cancel_Button = new Guna.UI2.WinForms.Guna2Button();
                Save_Button = new Guna.UI2.WinForms.Guna2Button();
                Panel = new Panel();
                SuspendLayout();
                // 
                // ModifyRow_Label
                // 
                ModifyRow_Label.Anchor = AnchorStyles.Top;
                ModifyRow_Label.AutoSize = true;
                ModifyRow_Label.Font = new Font("Segoe UI", 16F);
                ModifyRow_Label.Location = new Point(265, 13);
                ModifyRow_Label.Margin = new Padding(2, 0, 2, 0);
                ModifyRow_Label.Name = "ModifyRow_Label";
                ModifyRow_Label.Size = new Size(125, 30);
                ModifyRow_Label.TabIndex = 0;
                ModifyRow_Label.Text = "Modify row";
                ModifyRow_Label.Click += CloseSearchBox;
                // 
                // Cancel_Button
                // 
                Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                Cancel_Button.BackColor = Color.Transparent;
                Cancel_Button.BorderColor = Color.LightGray;
                Cancel_Button.BorderRadius = 2;
                Cancel_Button.BorderThickness = 1;
                Cancel_Button.CustomizableEdges = customizableEdges1;
                Cancel_Button.DisabledState.BorderColor = Color.DarkGray;
                Cancel_Button.DisabledState.CustomBorderColor = Color.DarkGray;
                Cancel_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
                Cancel_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
                Cancel_Button.FillColor = Color.White;
                Cancel_Button.Font = new Font("Segoe UI", 9F);
                Cancel_Button.ForeColor = Color.Black;
                Cancel_Button.Location = new Point(395, 198);
                Cancel_Button.Margin = new Padding(2, 2, 2, 2);
                Cancel_Button.Name = "Cancel_Button";
                Cancel_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
                Cancel_Button.Size = new Size(122, 27);
                Cancel_Button.TabIndex = 13;
                Cancel_Button.Text = "Cancel";
                Cancel_Button.Click += Cancel_Button_Click;
                // 
                // Save_Button
                // 
                Save_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                Save_Button.BackColor = Color.Transparent;
                Save_Button.BorderColor = Color.LightGray;
                Save_Button.BorderRadius = 2;
                Save_Button.BorderThickness = 1;
                Save_Button.CustomizableEdges = customizableEdges3;
                Save_Button.DisabledState.BorderColor = Color.DarkGray;
                Save_Button.DisabledState.CustomBorderColor = Color.DarkGray;
                Save_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
                Save_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
                Save_Button.FillColor = Color.White;
                Save_Button.Font = new Font("Segoe UI", 9F);
                Save_Button.ForeColor = Color.Black;
                Save_Button.Location = new Point(522, 198);
                Save_Button.Margin = new Padding(2, 2, 2, 2);
                Save_Button.Name = "Save_Button";
                Save_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
                Save_Button.Size = new Size(122, 27);
                Save_Button.TabIndex = 14;
                Save_Button.Text = "Save";
                Save_Button.Click += Save_Button_Click;
                // 
                // Panel
                // 
                Panel.Anchor = AnchorStyles.Top;
                Panel.Location = new Point(34, 49);
                Panel.Margin = new Padding(2, 2, 2, 2);
                Panel.Name = "Panel";
                Panel.Size = new Size(588, 90);
                Panel.TabIndex = 15;
                Panel.Click += CloseSearchBox;
                // 
                // ModifyRow_Form
                // 
                AutoScaleDimensions = new SizeF(7F, 15F);
                AutoScaleMode = AutoScaleMode.Font;
                ClientSize = new Size(657, 232);
                Controls.Add(Panel);
                Controls.Add(Save_Button);
                Controls.Add(Cancel_Button);
                Controls.Add(ModifyRow_Label);
                FormBorderStyle = FormBorderStyle.SizableToolWindow;
                Icon = (Icon)resources.GetObject("$this.Icon");
                Margin = new Padding(2, 2, 2, 2);
                Name = "ModifyRow_Form";
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.CenterScreen;
                FormClosed += ModifyRow_Form_FormClosed;
                Shown += ModifyRow_Form_Shown;
                Click += CloseSearchBox;
                ResumeLayout(false);
                PerformLayout();
            }

            #endregion

        private Label ModifyRow_Label;
        private Guna.UI2.WinForms.Guna2Button Cancel_Button;
        private Guna.UI2.WinForms.Guna2Button Save_Button;
        private Panel Panel;
    }
}