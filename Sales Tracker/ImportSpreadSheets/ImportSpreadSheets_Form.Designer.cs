namespace Sales_Tracker.ImportSpreadSheets
{
    partial class ImportSpreadSheets_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportSpreadSheets_Form));
            ImportSpreadsheet_Label = new Label();
            SelectFile_Button = new Guna.UI2.WinForms.Guna2Button();
            Import_Button = new Guna.UI2.WinForms.Guna2Button();
            RemoveReceipt_ImageButton = new Guna.UI2.WinForms.Guna2ImageButton();
            SelectedReceipt_Label = new Label();
            OpenTutorial_Button = new Guna.UI2.WinForms.Guna2Button();
            SkipHeaderRow_CheckBox = new Guna.UI2.WinForms.Guna2CustomCheckBox();
            SkipHeaderRow_Label = new Label();
            SuspendLayout();
            // 
            // ImportSpreadsheet_Label
            // 
            ImportSpreadsheet_Label.Anchor = AnchorStyles.Top;
            ImportSpreadsheet_Label.AutoSize = true;
            ImportSpreadsheet_Label.Font = new Font("Segoe UI", 16F);
            ImportSpreadsheet_Label.Location = new Point(390, 30);
            ImportSpreadsheet_Label.Margin = new Padding(6, 0, 6, 0);
            ImportSpreadsheet_Label.Name = "ImportSpreadsheet_Label";
            ImportSpreadsheet_Label.Size = new Size(299, 45);
            ImportSpreadsheet_Label.TabIndex = 543;
            ImportSpreadsheet_Label.Text = "Import spreadsheet";
            // 
            // SelectFile_Button
            // 
            SelectFile_Button.Anchor = AnchorStyles.Top;
            SelectFile_Button.BorderColor = Color.FromArgb(217, 221, 226);
            SelectFile_Button.BorderRadius = 3;
            SelectFile_Button.BorderThickness = 1;
            SelectFile_Button.CustomizableEdges = customizableEdges1;
            SelectFile_Button.DisabledState.BorderColor = Color.DarkGray;
            SelectFile_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            SelectFile_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            SelectFile_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            SelectFile_Button.FillColor = Color.White;
            SelectFile_Button.Font = new Font("Segoe UI", 10F);
            SelectFile_Button.ForeColor = Color.Black;
            SelectFile_Button.Location = new Point(429, 110);
            SelectFile_Button.Margin = new Padding(6, 5, 6, 5);
            SelectFile_Button.Name = "SelectFile_Button";
            SelectFile_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            SelectFile_Button.Size = new Size(220, 45);
            SelectFile_Button.TabIndex = 558;
            SelectFile_Button.Text = "Select file";
            SelectFile_Button.Click += SelectFile_Button_Click;
            // 
            // Import_Button
            // 
            Import_Button.Anchor = AnchorStyles.Bottom;
            Import_Button.BorderColor = Color.FromArgb(217, 221, 226);
            Import_Button.BorderRadius = 3;
            Import_Button.BorderThickness = 1;
            Import_Button.CustomizableEdges = customizableEdges3;
            Import_Button.DisabledState.BorderColor = Color.DarkGray;
            Import_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            Import_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            Import_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            Import_Button.Enabled = false;
            Import_Button.FillColor = Color.White;
            Import_Button.Font = new Font("Segoe UI", 10F);
            Import_Button.ForeColor = Color.Black;
            Import_Button.Location = new Point(429, 585);
            Import_Button.Margin = new Padding(6, 5, 6, 5);
            Import_Button.Name = "Import_Button";
            Import_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Import_Button.Size = new Size(220, 45);
            Import_Button.TabIndex = 559;
            Import_Button.Text = "Import";
            Import_Button.Click += Import_Button_Click;
            // 
            // RemoveReceipt_ImageButton
            // 
            RemoveReceipt_ImageButton.Anchor = AnchorStyles.Top;
            RemoveReceipt_ImageButton.CheckedState.ImageSize = new Size(64, 64);
            RemoveReceipt_ImageButton.HoverState.ImageSize = new Size(30, 30);
            RemoveReceipt_ImageButton.Image = Properties.Resources.CloseGray;
            RemoveReceipt_ImageButton.ImageOffset = new Point(0, 0);
            RemoveReceipt_ImageButton.ImageRotate = 0F;
            RemoveReceipt_ImageButton.ImageSize = new Size(30, 30);
            RemoveReceipt_ImageButton.Location = new Point(525, 163);
            RemoveReceipt_ImageButton.Name = "RemoveReceipt_ImageButton";
            RemoveReceipt_ImageButton.PressedState.ImageSize = new Size(30, 30);
            RemoveReceipt_ImageButton.ShadowDecoration.CustomizableEdges = customizableEdges5;
            RemoveReceipt_ImageButton.Size = new Size(38, 38);
            RemoveReceipt_ImageButton.TabIndex = 561;
            RemoveReceipt_ImageButton.Click += RemoveReceipt_ImageButton_Click;
            RemoveReceipt_ImageButton.MouseEnter += RemoveReceipt_ImageButton_MouseEnter;
            RemoveReceipt_ImageButton.MouseLeave += RemoveReceipt_ImageButton_MouseLeave;
            // 
            // SelectedReceipt_Label
            // 
            SelectedReceipt_Label.Anchor = AnchorStyles.Top;
            SelectedReceipt_Label.AutoSize = true;
            SelectedReceipt_Label.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SelectedReceipt_Label.Location = new Point(431, 168);
            SelectedReceipt_Label.Name = "SelectedReceipt_Label";
            SelectedReceipt_Label.Size = new Size(86, 28);
            SelectedReceipt_Label.TabIndex = 560;
            SelectedReceipt_Label.Text = "Selected";
            // 
            // OpenTutorial_Button
            // 
            OpenTutorial_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OpenTutorial_Button.BorderColor = Color.FromArgb(217, 221, 226);
            OpenTutorial_Button.BorderRadius = 3;
            OpenTutorial_Button.BorderThickness = 1;
            OpenTutorial_Button.CustomizableEdges = customizableEdges6;
            OpenTutorial_Button.DisabledState.BorderColor = Color.DarkGray;
            OpenTutorial_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            OpenTutorial_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            OpenTutorial_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            OpenTutorial_Button.FillColor = Color.White;
            OpenTutorial_Button.Font = new Font("Segoe UI", 10F);
            OpenTutorial_Button.ForeColor = Color.Black;
            OpenTutorial_Button.Location = new Point(843, 585);
            OpenTutorial_Button.Margin = new Padding(6, 5, 6, 5);
            OpenTutorial_Button.Name = "OpenTutorial_Button";
            OpenTutorial_Button.ShadowDecoration.CustomizableEdges = customizableEdges7;
            OpenTutorial_Button.Size = new Size(220, 45);
            OpenTutorial_Button.TabIndex = 562;
            OpenTutorial_Button.Text = "Open tutorial";
            OpenTutorial_Button.Click += OpenTutorial_Button_Click;
            // 
            // SkipHeaderRow_CheckBox
            // 
            SkipHeaderRow_CheckBox.Anchor = AnchorStyles.Top;
            SkipHeaderRow_CheckBox.Animated = true;
            SkipHeaderRow_CheckBox.Checked = true;
            SkipHeaderRow_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            SkipHeaderRow_CheckBox.CheckedState.BorderRadius = 2;
            SkipHeaderRow_CheckBox.CheckedState.BorderThickness = 0;
            SkipHeaderRow_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            SkipHeaderRow_CheckBox.CustomizableEdges = customizableEdges8;
            SkipHeaderRow_CheckBox.Location = new Point(746, 122);
            SkipHeaderRow_CheckBox.Name = "SkipHeaderRow_CheckBox";
            SkipHeaderRow_CheckBox.ShadowDecoration.CustomizableEdges = customizableEdges9;
            SkipHeaderRow_CheckBox.Size = new Size(20, 20);
            SkipHeaderRow_CheckBox.TabIndex = 564;
            SkipHeaderRow_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            SkipHeaderRow_CheckBox.UncheckedState.BorderRadius = 2;
            SkipHeaderRow_CheckBox.UncheckedState.BorderThickness = 0;
            SkipHeaderRow_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            SkipHeaderRow_CheckBox.CheckedChanged += SkipHeaderRow_CheckBox_CheckedChanged;
            // 
            // SkipHeaderRow_Label
            // 
            SkipHeaderRow_Label.Anchor = AnchorStyles.Top;
            SkipHeaderRow_Label.AutoSize = true;
            SkipHeaderRow_Label.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SkipHeaderRow_Label.Location = new Point(764, 112);
            SkipHeaderRow_Label.Name = "SkipHeaderRow_Label";
            SkipHeaderRow_Label.Padding = new Padding(5);
            SkipHeaderRow_Label.Size = new Size(181, 40);
            SkipHeaderRow_Label.TabIndex = 563;
            SkipHeaderRow_Label.Text = "Skip header row";
            SkipHeaderRow_Label.Click += SkipHeaderRow_Label_Click;
            // 
            // ImportSpreadSheets_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1078, 644);
            Controls.Add(SkipHeaderRow_CheckBox);
            Controls.Add(SkipHeaderRow_Label);
            Controls.Add(OpenTutorial_Button);
            Controls.Add(RemoveReceipt_ImageButton);
            Controls.Add(SelectedReceipt_Label);
            Controls.Add(Import_Button);
            Controls.Add(SelectFile_Button);
            Controls.Add(ImportSpreadsheet_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1100, 700);
            Name = "ImportSpreadSheets_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += ImportSpreadSheets_Form_FormClosed;
            Shown += ImportSpreadSheets_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ImportSpreadsheet_Label;
        public Guna.UI2.WinForms.Guna2Button SelectFile_Button;
        public Guna.UI2.WinForms.Guna2Button Import_Button;
        private Guna.UI2.WinForms.Guna2ImageButton RemoveReceipt_ImageButton;
        private Label SelectedReceipt_Label;
        public Guna.UI2.WinForms.Guna2Button OpenTutorial_Button;
        private Guna.UI2.WinForms.Guna2CustomCheckBox SkipHeaderRow_CheckBox;
        private Label SkipHeaderRow_Label;
    }
}