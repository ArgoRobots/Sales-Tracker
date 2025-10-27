namespace Sales_Tracker.ReportGenerator.Menus
{
    partial class SaveTemplate_Form
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
            Title_Label = new Label();
            TemplateName_Label = new Label();
            TemplateName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Save_Button = new Guna.UI2.WinForms.Guna2Button();
            Cancel_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            //
            // Title_Label
            //
            Title_Label.Anchor = AnchorStyles.Top;
            Title_Label.AutoSize = true;
            Title_Label.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            Title_Label.Location = new Point(128, 20);
            Title_Label.Name = "Title_Label";
            Title_Label.Size = new Size(190, 38);
            Title_Label.TabIndex = 0;
            Title_Label.Text = "Save Template";
            //
            // TemplateName_Label
            //
            TemplateName_Label.AutoSize = true;
            TemplateName_Label.Font = new Font("Segoe UI", 10F);
            TemplateName_Label.Location = new Point(30, 80);
            TemplateName_Label.Name = "TemplateName_Label";
            TemplateName_Label.Size = new Size(152, 28);
            TemplateName_Label.TabIndex = 1;
            TemplateName_Label.Text = "Template Name:";
            //
            // TemplateName_TextBox
            //
            TemplateName_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TemplateName_TextBox.BorderRadius = 4;
            TemplateName_TextBox.Cursor = Cursors.IBeam;
            TemplateName_TextBox.CustomizableEdges = customizableEdges1;
            TemplateName_TextBox.DefaultText = "";
            TemplateName_TextBox.Font = new Font("Segoe UI", 10F);
            TemplateName_TextBox.ForeColor = Color.Black;
            TemplateName_TextBox.Location = new Point(30, 115);
            TemplateName_TextBox.Margin = new Padding(4, 5, 4, 5);
            TemplateName_TextBox.MaxLength = 100;
            TemplateName_TextBox.Name = "TemplateName_TextBox";
            TemplateName_TextBox.PlaceholderText = "Enter template name";
            TemplateName_TextBox.SelectedText = "";
            TemplateName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            TemplateName_TextBox.Size = new Size(380, 45);
            TemplateName_TextBox.TabIndex = 2;
            TemplateName_TextBox.KeyDown += TemplateName_TextBox_KeyDown;
            //
            // Save_Button
            //
            Save_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Save_Button.BorderRadius = 4;
            Save_Button.CustomizableEdges = customizableEdges3;
            Save_Button.DisabledState.BorderColor = Color.DarkGray;
            Save_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Save_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Save_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Save_Button.Font = new Font("Segoe UI", 9.5F);
            Save_Button.ForeColor = Color.White;
            Save_Button.Location = new Point(230, 185);
            Save_Button.Name = "Save_Button";
            Save_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Save_Button.Size = new Size(90, 45);
            Save_Button.TabIndex = 3;
            Save_Button.Text = "Save";
            Save_Button.Click += Save_Button_Click;
            //
            // Cancel_Button
            //
            Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel_Button.BorderColor = Color.LightGray;
            Cancel_Button.BorderRadius = 4;
            Cancel_Button.BorderThickness = 1;
            Cancel_Button.CustomizableEdges = customizableEdges5;
            Cancel_Button.DisabledState.BorderColor = Color.DarkGray;
            Cancel_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Cancel_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Cancel_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Cancel_Button.FillColor = Color.White;
            Cancel_Button.Font = new Font("Segoe UI", 9.5F);
            Cancel_Button.ForeColor = Color.Black;
            Cancel_Button.Location = new Point(326, 185);
            Cancel_Button.Name = "Cancel_Button";
            Cancel_Button.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Cancel_Button.Size = new Size(84, 45);
            Cancel_Button.TabIndex = 4;
            Cancel_Button.Text = "Cancel";
            Cancel_Button.Click += Cancel_Button_Click;
            //
            // SaveTemplate_Form
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 250);
            Controls.Add(Cancel_Button);
            Controls.Add(Save_Button);
            Controls.Add(TemplateName_TextBox);
            Controls.Add(TemplateName_Label);
            Controls.Add(Title_Label);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SaveTemplate_Form";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Save Template";
            Shown += SaveTemplate_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Title_Label;
        private Label TemplateName_Label;
        private Guna.UI2.WinForms.Guna2TextBox TemplateName_TextBox;
        private Guna.UI2.WinForms.Guna2Button Save_Button;
        private Guna.UI2.WinForms.Guna2Button Cancel_Button;
    }
}
