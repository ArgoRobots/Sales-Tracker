namespace Sales_Tracker.ReportGenerator.Menus
{
    partial class CustomTemplateManager_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Title_Label = new Label();
            Templates_ListBox = new ListBox();
            Load_Button = new Guna.UI2.WinForms.Guna2Button();
            Delete_Button = new Guna.UI2.WinForms.Guna2Button();
            Rename_Button = new Guna.UI2.WinForms.Guna2Button();
            Close_Button = new Guna.UI2.WinForms.Guna2Button();
            NoTemplates_Label = new Label();
            SuspendLayout();
            // 
            // Title_Label
            // 
            Title_Label.Anchor = AnchorStyles.Top;
            Title_Label.AutoSize = true;
            Title_Label.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            Title_Label.Location = new Point(204, 20);
            Title_Label.Name = "Title_Label";
            Title_Label.Size = new Size(250, 38);
            Title_Label.TabIndex = 0;
            Title_Label.Text = "Custom Templates";
            // 
            // Templates_ListBox
            // 
            Templates_ListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Templates_ListBox.Font = new Font("Segoe UI", 10F);
            Templates_ListBox.FormattingEnabled = true;
            Templates_ListBox.Location = new Point(30, 80);
            Templates_ListBox.Name = "Templates_ListBox";
            Templates_ListBox.Size = new Size(598, 284);
            Templates_ListBox.TabIndex = 1;
            Templates_ListBox.SelectedIndexChanged += Templates_ListBox_SelectedIndexChanged;
            Templates_ListBox.DoubleClick += Templates_ListBox_DoubleClick;
            // 
            // Load_Button
            // 
            Load_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Load_Button.BorderRadius = 2;
            Load_Button.CustomizableEdges = customizableEdges9;
            Load_Button.DisabledState.BorderColor = Color.DarkGray;
            Load_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Load_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Load_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Load_Button.Enabled = false;
            Load_Button.Font = new Font("Segoe UI", 9.5F);
            Load_Button.ForeColor = Color.White;
            Load_Button.Location = new Point(30, 399);
            Load_Button.Name = "Load_Button";
            Load_Button.ShadowDecoration.CustomizableEdges = customizableEdges10;
            Load_Button.Size = new Size(130, 45);
            Load_Button.TabIndex = 3;
            Load_Button.Text = "Load";
            Load_Button.Click += Load_Button_Click;
            // 
            // Delete_Button
            // 
            Delete_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Delete_Button.BorderColor = Color.LightGray;
            Delete_Button.BorderRadius = 2;
            Delete_Button.BorderThickness = 1;
            Delete_Button.CustomizableEdges = customizableEdges11;
            Delete_Button.DisabledState.BorderColor = Color.DarkGray;
            Delete_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Delete_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Delete_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Delete_Button.Enabled = false;
            Delete_Button.FillColor = Color.White;
            Delete_Button.Font = new Font("Segoe UI", 9.5F);
            Delete_Button.ForeColor = Color.Black;
            Delete_Button.Location = new Point(166, 399);
            Delete_Button.Name = "Delete_Button";
            Delete_Button.ShadowDecoration.CustomizableEdges = customizableEdges12;
            Delete_Button.Size = new Size(130, 45);
            Delete_Button.TabIndex = 4;
            Delete_Button.Text = "Delete";
            Delete_Button.Click += Delete_Button_Click;
            // 
            // Rename_Button
            // 
            Rename_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Rename_Button.BorderColor = Color.LightGray;
            Rename_Button.BorderRadius = 2;
            Rename_Button.BorderThickness = 1;
            Rename_Button.CustomizableEdges = customizableEdges13;
            Rename_Button.DisabledState.BorderColor = Color.DarkGray;
            Rename_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Rename_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Rename_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Rename_Button.Enabled = false;
            Rename_Button.FillColor = Color.White;
            Rename_Button.Font = new Font("Segoe UI", 9.5F);
            Rename_Button.ForeColor = Color.Black;
            Rename_Button.Location = new Point(302, 399);
            Rename_Button.Name = "Rename_Button";
            Rename_Button.ShadowDecoration.CustomizableEdges = customizableEdges14;
            Rename_Button.Size = new Size(130, 45);
            Rename_Button.TabIndex = 5;
            Rename_Button.Text = "Rename";
            Rename_Button.Click += Rename_Button_Click;
            // 
            // Close_Button
            // 
            Close_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Close_Button.BorderColor = Color.LightGray;
            Close_Button.BorderRadius = 2;
            Close_Button.BorderThickness = 1;
            Close_Button.CustomizableEdges = customizableEdges15;
            Close_Button.DisabledState.BorderColor = Color.DarkGray;
            Close_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Close_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Close_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Close_Button.FillColor = Color.White;
            Close_Button.Font = new Font("Segoe UI", 9.5F);
            Close_Button.ForeColor = Color.Black;
            Close_Button.Location = new Point(498, 399);
            Close_Button.Name = "Close_Button";
            Close_Button.ShadowDecoration.CustomizableEdges = customizableEdges16;
            Close_Button.Size = new Size(130, 45);
            Close_Button.TabIndex = 6;
            Close_Button.Text = "Close";
            Close_Button.Click += Close_Button_Click;
            // 
            // NoTemplates_Label
            // 
            NoTemplates_Label.Anchor = AnchorStyles.None;
            NoTemplates_Label.AutoSize = true;
            NoTemplates_Label.BackColor = Color.White;
            NoTemplates_Label.Font = new Font("Segoe UI", 10F);
            NoTemplates_Label.Location = new Point(202, 207);
            NoTemplates_Label.Name = "NoTemplates_Label";
            NoTemplates_Label.Size = new Size(254, 28);
            NoTemplates_Label.TabIndex = 2;
            NoTemplates_Label.Text = "No custom templates saved";
            NoTemplates_Label.TextAlign = ContentAlignment.MiddleCenter;
            NoTemplates_Label.Visible = false;
            // 
            // CustomTemplateManager_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(658, 464);
            Controls.Add(NoTemplates_Label);
            Controls.Add(Close_Button);
            Controls.Add(Rename_Button);
            Controls.Add(Delete_Button);
            Controls.Add(Load_Button);
            Controls.Add(Templates_ListBox);
            Controls.Add(Title_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(646, 520);
            Name = "CustomTemplateManager_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Custom Templates";
            Shown += CustomTemplateManager_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Title_Label;
        private ListBox Templates_ListBox;
        private Label NoTemplates_Label;
        private Guna.UI2.WinForms.Guna2Button Load_Button;
        private Guna.UI2.WinForms.Guna2Button Delete_Button;
        private Guna.UI2.WinForms.Guna2Button Rename_Button;
        private Guna.UI2.WinForms.Guna2Button Close_Button;
    }
}
