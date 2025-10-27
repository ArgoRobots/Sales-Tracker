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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Title_Label = new Label();
            Templates_ListBox = new ListBox();
            Load_Button = new Guna.UI2.WinForms.Guna2Button();
            Delete_Button = new Guna.UI2.WinForms.Guna2Button();
            Close_Button = new Guna.UI2.WinForms.Guna2Button();
            NoTemplates_Label = new Label();
            SuspendLayout();
            //
            // Title_Label
            //
            Title_Label.Anchor = AnchorStyles.Top;
            Title_Label.AutoSize = true;
            Title_Label.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            Title_Label.Location = new Point(120, 20);
            Title_Label.Name = "Title_Label";
            Title_Label.Size = new Size(241, 38);
            Title_Label.TabIndex = 0;
            Title_Label.Text = "Custom Templates";
            //
            // Templates_ListBox
            //
            Templates_ListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Templates_ListBox.Font = new Font("Segoe UI", 10F);
            Templates_ListBox.FormattingEnabled = true;
            Templates_ListBox.ItemHeight = 28;
            Templates_ListBox.Location = new Point(30, 80);
            Templates_ListBox.Name = "Templates_ListBox";
            Templates_ListBox.Size = new Size(410, 284);
            Templates_ListBox.TabIndex = 1;
            Templates_ListBox.SelectedIndexChanged += Templates_ListBox_SelectedIndexChanged;
            Templates_ListBox.DoubleClick += Templates_ListBox_DoubleClick;
            //
            // NoTemplates_Label
            //
            NoTemplates_Label.Anchor = AnchorStyles.None;
            NoTemplates_Label.AutoSize = true;
            NoTemplates_Label.Font = new Font("Segoe UI", 10F);
            NoTemplates_Label.ForeColor = Color.Gray;
            NoTemplates_Label.Location = new Point(105, 200);
            NoTemplates_Label.Name = "NoTemplates_Label";
            NoTemplates_Label.Size = new Size(260, 28);
            NoTemplates_Label.TabIndex = 2;
            NoTemplates_Label.Text = "No custom templates saved";
            NoTemplates_Label.TextAlign = ContentAlignment.MiddleCenter;
            NoTemplates_Label.Visible = false;
            //
            // Load_Button
            //
            Load_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Load_Button.BorderRadius = 4;
            Load_Button.CustomizableEdges = customizableEdges1;
            Load_Button.DisabledState.BorderColor = Color.DarkGray;
            Load_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Load_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Load_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Load_Button.Enabled = false;
            Load_Button.Font = new Font("Segoe UI", 9.5F);
            Load_Button.ForeColor = Color.White;
            Load_Button.Location = new Point(30, 385);
            Load_Button.Name = "Load_Button";
            Load_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Load_Button.Size = new Size(100, 45);
            Load_Button.TabIndex = 3;
            Load_Button.Text = "Load";
            Load_Button.Click += Load_Button_Click;
            //
            // Delete_Button
            //
            Delete_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Delete_Button.BorderColor = Color.FromArgb(220, 53, 69);
            Delete_Button.BorderRadius = 4;
            Delete_Button.BorderThickness = 1;
            Delete_Button.CustomizableEdges = customizableEdges3;
            Delete_Button.DisabledState.BorderColor = Color.DarkGray;
            Delete_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Delete_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Delete_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Delete_Button.Enabled = false;
            Delete_Button.FillColor = Color.White;
            Delete_Button.Font = new Font("Segoe UI", 9.5F);
            Delete_Button.ForeColor = Color.FromArgb(220, 53, 69);
            Delete_Button.Location = new Point(136, 385);
            Delete_Button.Name = "Delete_Button";
            Delete_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Delete_Button.Size = new Size(100, 45);
            Delete_Button.TabIndex = 4;
            Delete_Button.Text = "Delete";
            Delete_Button.Click += Delete_Button_Click;
            //
            // Close_Button
            //
            Close_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Close_Button.BorderColor = Color.LightGray;
            Close_Button.BorderRadius = 4;
            Close_Button.BorderThickness = 1;
            Close_Button.CustomizableEdges = customizableEdges5;
            Close_Button.DisabledState.BorderColor = Color.DarkGray;
            Close_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Close_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Close_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Close_Button.FillColor = Color.White;
            Close_Button.Font = new Font("Segoe UI", 9.5F);
            Close_Button.ForeColor = Color.Black;
            Close_Button.Location = new Point(340, 385);
            Close_Button.Name = "Close_Button";
            Close_Button.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Close_Button.Size = new Size(100, 45);
            Close_Button.TabIndex = 5;
            Close_Button.Text = "Close";
            Close_Button.Click += Close_Button_Click;
            //
            // CustomTemplateManager_Form
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(470, 450);
            Controls.Add(NoTemplates_Label);
            Controls.Add(Close_Button);
            Controls.Add(Delete_Button);
            Controls.Add(Load_Button);
            Controls.Add(Templates_ListBox);
            Controls.Add(Title_Label);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CustomTemplateManager_Form";
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
        private Guna.UI2.WinForms.Guna2Button Close_Button;
    }
}
