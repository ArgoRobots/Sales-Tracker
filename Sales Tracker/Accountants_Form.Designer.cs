namespace Sales_Tracker
{
    partial class Accountants_Form
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
            WarningAccountantName_PictureBox = new PictureBox();
            WarningAccountantName_Label = new Label();
            AccountantName_Label = new Label();
            Accountant_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddAccountant_Button = new Guna.UI2.WinForms.Guna2Button();
            AddCategory_Label = new Label();
            ((System.ComponentModel.ISupportInitialize)WarningAccountantName_PictureBox).BeginInit();
            SuspendLayout();
            // 
            // WarningAccountantName_PictureBox
            // 
            WarningAccountantName_PictureBox.Image = Properties.Resources.Warning;
            WarningAccountantName_PictureBox.Location = new Point(217, 145);
            WarningAccountantName_PictureBox.Margin = new Padding(4, 3, 4, 3);
            WarningAccountantName_PictureBox.Name = "WarningAccountantName_PictureBox";
            WarningAccountantName_PictureBox.Size = new Size(19, 19);
            WarningAccountantName_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningAccountantName_PictureBox.TabIndex = 47;
            WarningAccountantName_PictureBox.TabStop = false;
            WarningAccountantName_PictureBox.Visible = false;
            WarningAccountantName_PictureBox.Click += CloseAllPanels;
            // 
            // WarningAccountantName_Label
            // 
            WarningAccountantName_Label.AutoSize = true;
            WarningAccountantName_Label.Font = new Font("Segoe UI", 10F);
            WarningAccountantName_Label.Location = new Point(244, 145);
            WarningAccountantName_Label.Margin = new Padding(4, 0, 4, 0);
            WarningAccountantName_Label.Name = "WarningAccountantName_Label";
            WarningAccountantName_Label.Size = new Size(164, 19);
            WarningAccountantName_Label.TabIndex = 46;
            WarningAccountantName_Label.Text = "Accountant already exists";
            WarningAccountantName_Label.Visible = false;
            WarningAccountantName_Label.Click += CloseAllPanels;
            // 
            // AccountantName_Label
            // 
            AccountantName_Label.Anchor = AnchorStyles.Top;
            AccountantName_Label.AutoSize = true;
            AccountantName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AccountantName_Label.Location = new Point(217, 80);
            AccountantName_Label.Name = "AccountantName_Label";
            AccountantName_Label.Size = new Size(125, 20);
            AccountantName_Label.TabIndex = 40;
            AccountantName_Label.Text = "Accountant name";
            AccountantName_Label.Click += CloseAllPanels;
            // 
            // Accountant_TextBox
            // 
            Accountant_TextBox.Anchor = AnchorStyles.Top;
            Accountant_TextBox.CustomizableEdges = customizableEdges1;
            Accountant_TextBox.DefaultText = "";
            Accountant_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Accountant_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Accountant_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Accountant_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Accountant_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Accountant_TextBox.Font = new Font("Segoe UI", 9F);
            Accountant_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Accountant_TextBox.Location = new Point(217, 103);
            Accountant_TextBox.MaxLength = 32;
            Accountant_TextBox.Name = "Accountant_TextBox";
            Accountant_TextBox.PasswordChar = '\0';
            Accountant_TextBox.PlaceholderText = "";
            Accountant_TextBox.SelectedText = "";
            Accountant_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Accountant_TextBox.Size = new Size(200, 36);
            Accountant_TextBox.TabIndex = 42;
            Accountant_TextBox.TextChanged += Accountant_TextBox_TextChanged;
            Accountant_TextBox.Click += CloseAllPanels;
            Accountant_TextBox.KeyDown += Accountant_TextBox_KeyDown;
            // 
            // AddAccountant_Button
            // 
            AddAccountant_Button.Anchor = AnchorStyles.Top;
            AddAccountant_Button.BackColor = Color.Transparent;
            AddAccountant_Button.BorderColor = Color.LightGray;
            AddAccountant_Button.BorderRadius = 2;
            AddAccountant_Button.BorderThickness = 1;
            AddAccountant_Button.CustomizableEdges = customizableEdges3;
            AddAccountant_Button.Enabled = false;
            AddAccountant_Button.FillColor = Color.White;
            AddAccountant_Button.Font = new Font("Segoe UI", 9.5F);
            AddAccountant_Button.ForeColor = Color.Black;
            AddAccountant_Button.Location = new Point(424, 103);
            AddAccountant_Button.Margin = new Padding(4, 3, 4, 3);
            AddAccountant_Button.Name = "AddAccountant_Button";
            AddAccountant_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddAccountant_Button.Size = new Size(143, 36);
            AddAccountant_Button.TabIndex = 43;
            AddAccountant_Button.Tag = "0";
            AddAccountant_Button.Text = "Add accountant";
            AddAccountant_Button.Click += AddAccountant_Button_Click;
            // 
            // AddCategory_Label
            // 
            AddCategory_Label.Anchor = AnchorStyles.Top;
            AddCategory_Label.AutoSize = true;
            AddCategory_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddCategory_Label.Location = new Point(324, 20);
            AddCategory_Label.Name = "AddCategory_Label";
            AddCategory_Label.Size = new Size(161, 30);
            AddCategory_Label.TabIndex = 41;
            AddCategory_Label.Text = "Add accountant";
            AddCategory_Label.Click += CloseAllPanels;
            // 
            // Accountants_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 471);
            Controls.Add(WarningAccountantName_PictureBox);
            Controls.Add(WarningAccountantName_Label);
            Controls.Add(AccountantName_Label);
            Controls.Add(Accountant_TextBox);
            Controls.Add(AddAccountant_Button);
            Controls.Add(AddCategory_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(800, 510);
            Name = "Accountants_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Accountants_Form_FormClosed;
            Click += CloseAllPanels;
            Resize += Accountants_Form_Resize;
            ((System.ComponentModel.ISupportInitialize)WarningAccountantName_PictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox WarningAccountantName_PictureBox;
        private Label WarningAccountantName_Label;
        private Label AccountantName_Label;
        private Guna.UI2.WinForms.Guna2TextBox Accountant_TextBox;
        private Guna.UI2.WinForms.Guna2Button AddAccountant_Button;
        private Label AddCategory_Label;
    }
}