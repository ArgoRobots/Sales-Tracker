namespace Sales_Tracker
{
    partial class Products_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges19 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges20 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            CountryOfOrigin_Label = new Label();
            CountryOfOrigin_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddProduct_Label = new Label();
            ProductName_Label = new Label();
            ProductName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddProduct_Button = new Guna.UI2.WinForms.Guna2Button();
            ProductCategory_Label = new Label();
            ProductCategory_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Sale_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            Purchase_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            ProductID_Label = new Label();
            ProductID_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            WarningProductName_PictureBox = new PictureBox();
            WarningProductName_Label = new Label();
            WarningCategory_PictureBox = new PictureBox();
            WarningCategory_LinkLabel = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)WarningProductName_PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)WarningCategory_PictureBox).BeginInit();
            SuspendLayout();
            // 
            // CountryOfOrigin_Label
            // 
            CountryOfOrigin_Label.Anchor = AnchorStyles.Top;
            CountryOfOrigin_Label.AutoSize = true;
            CountryOfOrigin_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CountryOfOrigin_Label.Location = new Point(651, 80);
            CountryOfOrigin_Label.Name = "CountryOfOrigin_Label";
            CountryOfOrigin_Label.Size = new Size(121, 20);
            CountryOfOrigin_Label.TabIndex = 0;
            CountryOfOrigin_Label.Text = "Country of origin";
            CountryOfOrigin_Label.Click += CloseAllPanels;
            // 
            // CountryOfOrigin_TextBox
            // 
            CountryOfOrigin_TextBox.Anchor = AnchorStyles.Top;
            CountryOfOrigin_TextBox.CustomizableEdges = customizableEdges11;
            CountryOfOrigin_TextBox.DefaultText = "";
            CountryOfOrigin_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            CountryOfOrigin_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            CountryOfOrigin_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            CountryOfOrigin_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            CountryOfOrigin_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            CountryOfOrigin_TextBox.Font = new Font("Segoe UI", 9F);
            CountryOfOrigin_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            CountryOfOrigin_TextBox.Location = new Point(651, 103);
            CountryOfOrigin_TextBox.MaxLength = 32;
            CountryOfOrigin_TextBox.Name = "CountryOfOrigin_TextBox";
            CountryOfOrigin_TextBox.PasswordChar = '\0';
            CountryOfOrigin_TextBox.PlaceholderText = "";
            CountryOfOrigin_TextBox.SelectedText = "";
            CountryOfOrigin_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges12;
            CountryOfOrigin_TextBox.Size = new Size(200, 36);
            CountryOfOrigin_TextBox.TabIndex = 4;
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;
            CountryOfOrigin_TextBox.Click += CloseAllPanels;
            // 
            // AddProduct_Label
            // 
            AddProduct_Label.Anchor = AnchorStyles.Top;
            AddProduct_Label.AutoSize = true;
            AddProduct_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddProduct_Label.Location = new Point(378, 23);
            AddProduct_Label.Name = "AddProduct_Label";
            AddProduct_Label.Size = new Size(129, 30);
            AddProduct_Label.TabIndex = 0;
            AddProduct_Label.Text = "Add product";
            AddProduct_Label.Click += CloseAllPanels;
            // 
            // ProductName_Label
            // 
            ProductName_Label.Anchor = AnchorStyles.Top;
            ProductName_Label.AutoSize = true;
            ProductName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ProductName_Label.Location = new Point(239, 80);
            ProductName_Label.Name = "ProductName_Label";
            ProductName_Label.Size = new Size(101, 20);
            ProductName_Label.TabIndex = 0;
            ProductName_Label.Text = "Product name";
            ProductName_Label.Click += CloseAllPanels;
            // 
            // ProductName_TextBox
            // 
            ProductName_TextBox.Anchor = AnchorStyles.Top;
            ProductName_TextBox.CustomizableEdges = customizableEdges13;
            ProductName_TextBox.DefaultText = "";
            ProductName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ProductName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ProductName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ProductName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ProductName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductName_TextBox.Font = new Font("Segoe UI", 9F);
            ProductName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductName_TextBox.Location = new Point(239, 103);
            ProductName_TextBox.MaxLength = 32;
            ProductName_TextBox.Name = "ProductName_TextBox";
            ProductName_TextBox.PasswordChar = '\0';
            ProductName_TextBox.PlaceholderText = "";
            ProductName_TextBox.SelectedText = "";
            ProductName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges14;
            ProductName_TextBox.Size = new Size(200, 36);
            ProductName_TextBox.TabIndex = 2;
            ProductName_TextBox.TextChanged += ProductName_TextBox_TextChanged;
            ProductName_TextBox.Click += CloseAllPanels;
            // 
            // AddProduct_Button
            // 
            AddProduct_Button.Anchor = AnchorStyles.Top;
            AddProduct_Button.BackColor = Color.Transparent;
            AddProduct_Button.BorderColor = Color.LightGray;
            AddProduct_Button.BorderRadius = 2;
            AddProduct_Button.BorderThickness = 1;
            AddProduct_Button.CustomizableEdges = customizableEdges15;
            AddProduct_Button.Enabled = false;
            AddProduct_Button.FillColor = Color.White;
            AddProduct_Button.Font = new Font("Segoe UI", 9.5F);
            AddProduct_Button.ForeColor = Color.Black;
            AddProduct_Button.Location = new Point(371, 170);
            AddProduct_Button.Margin = new Padding(4, 3, 4, 3);
            AddProduct_Button.Name = "AddProduct_Button";
            AddProduct_Button.ShadowDecoration.CustomizableEdges = customizableEdges16;
            AddProduct_Button.Size = new Size(143, 32);
            AddProduct_Button.TabIndex = 5;
            AddProduct_Button.Tag = "0";
            AddProduct_Button.Text = "Add product";
            AddProduct_Button.Click += AddProduct_Button_Click;
            // 
            // ProductCategory_Label
            // 
            ProductCategory_Label.Anchor = AnchorStyles.Top;
            ProductCategory_Label.AutoSize = true;
            ProductCategory_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ProductCategory_Label.Location = new Point(445, 80);
            ProductCategory_Label.Name = "ProductCategory_Label";
            ProductCategory_Label.Size = new Size(122, 20);
            ProductCategory_Label.TabIndex = 0;
            ProductCategory_Label.Text = "Product category";
            ProductCategory_Label.Click += CloseAllPanels;
            // 
            // ProductCategory_TextBox
            // 
            ProductCategory_TextBox.Anchor = AnchorStyles.Top;
            ProductCategory_TextBox.CustomizableEdges = customizableEdges17;
            ProductCategory_TextBox.DefaultText = "";
            ProductCategory_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ProductCategory_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ProductCategory_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ProductCategory_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ProductCategory_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductCategory_TextBox.Font = new Font("Segoe UI", 9F);
            ProductCategory_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductCategory_TextBox.Location = new Point(445, 103);
            ProductCategory_TextBox.MaxLength = 32;
            ProductCategory_TextBox.Name = "ProductCategory_TextBox";
            ProductCategory_TextBox.PasswordChar = '\0';
            ProductCategory_TextBox.PlaceholderText = "";
            ProductCategory_TextBox.SelectedText = "";
            ProductCategory_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges18;
            ProductCategory_TextBox.Size = new Size(200, 36);
            ProductCategory_TextBox.TabIndex = 3;
            ProductCategory_TextBox.Click += CloseAllPanels;
            // 
            // Sale_RadioButton
            // 
            Sale_RadioButton.AutoSize = true;
            Sale_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Sale_RadioButton.CheckedState.BorderThickness = 0;
            Sale_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Sale_RadioButton.CheckedState.InnerColor = Color.White;
            Sale_RadioButton.CheckedState.InnerOffset = -4;
            Sale_RadioButton.Font = new Font("Segoe UI", 11F);
            Sale_RadioButton.Location = new Point(33, 23);
            Sale_RadioButton.Name = "Sale_RadioButton";
            Sale_RadioButton.Size = new Size(78, 24);
            Sale_RadioButton.TabIndex = 6;
            Sale_RadioButton.Text = "For sale";
            Sale_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Sale_RadioButton.UncheckedState.BorderThickness = 2;
            Sale_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Sale_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Sale_RadioButton.CheckedChanged += Sale_RadioButton_CheckedChanged;
            Sale_RadioButton.Click += CloseAllPanels;
            // 
            // Purchase_RadioButton
            // 
            Purchase_RadioButton.AutoSize = true;
            Purchase_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Purchase_RadioButton.CheckedState.BorderThickness = 0;
            Purchase_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Purchase_RadioButton.CheckedState.InnerColor = Color.White;
            Purchase_RadioButton.CheckedState.InnerOffset = -4;
            Purchase_RadioButton.Font = new Font("Segoe UI", 11F);
            Purchase_RadioButton.Location = new Point(117, 23);
            Purchase_RadioButton.Name = "Purchase_RadioButton";
            Purchase_RadioButton.Size = new Size(111, 24);
            Purchase_RadioButton.TabIndex = 7;
            Purchase_RadioButton.Text = "For purchase";
            Purchase_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Purchase_RadioButton.UncheckedState.BorderThickness = 2;
            Purchase_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Purchase_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Purchase_RadioButton.CheckedChanged += Purchase_RadioButton_CheckedChanged;
            Purchase_RadioButton.Click += CloseAllPanels;
            // 
            // ProductID_Label
            // 
            ProductID_Label.Anchor = AnchorStyles.Top;
            ProductID_Label.AutoSize = true;
            ProductID_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ProductID_Label.Location = new Point(33, 80);
            ProductID_Label.Name = "ProductID_Label";
            ProductID_Label.Size = new Size(79, 20);
            ProductID_Label.TabIndex = 0;
            ProductID_Label.Text = "Product ID";
            // 
            // ProductID_TextBox
            // 
            ProductID_TextBox.Anchor = AnchorStyles.Top;
            ProductID_TextBox.CustomizableEdges = customizableEdges19;
            ProductID_TextBox.DefaultText = "";
            ProductID_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ProductID_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ProductID_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ProductID_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ProductID_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductID_TextBox.Font = new Font("Segoe UI", 9F);
            ProductID_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductID_TextBox.Location = new Point(33, 103);
            ProductID_TextBox.MaxLength = 32;
            ProductID_TextBox.Name = "ProductID_TextBox";
            ProductID_TextBox.PasswordChar = '\0';
            ProductID_TextBox.PlaceholderText = "";
            ProductID_TextBox.SelectedText = "";
            ProductID_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges20;
            ProductID_TextBox.Size = new Size(200, 36);
            ProductID_TextBox.TabIndex = 1;
            // 
            // WarningProductName_PictureBox
            // 
            WarningProductName_PictureBox.Image = Properties.Resources.Warning;
            WarningProductName_PictureBox.Location = new Point(239, 145);
            WarningProductName_PictureBox.Margin = new Padding(4, 3, 4, 3);
            WarningProductName_PictureBox.Name = "WarningProductName_PictureBox";
            WarningProductName_PictureBox.Size = new Size(19, 18);
            WarningProductName_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningProductName_PictureBox.TabIndex = 37;
            WarningProductName_PictureBox.TabStop = false;
            WarningProductName_PictureBox.Visible = false;
            // 
            // WarningProductName_Label
            // 
            WarningProductName_Label.AutoSize = true;
            WarningProductName_Label.Font = new Font("Segoe UI", 10F);
            WarningProductName_Label.Location = new Point(265, 144);
            WarningProductName_Label.Margin = new Padding(4, 0, 4, 0);
            WarningProductName_Label.Name = "WarningProductName_Label";
            WarningProductName_Label.Size = new Size(142, 19);
            WarningProductName_Label.TabIndex = 36;
            WarningProductName_Label.Text = "Product already exists";
            WarningProductName_Label.Visible = false;
            // 
            // WarningCategory_PictureBox
            // 
            WarningCategory_PictureBox.Image = Properties.Resources.Warning;
            WarningCategory_PictureBox.Location = new Point(445, 145);
            WarningCategory_PictureBox.Margin = new Padding(4, 3, 4, 3);
            WarningCategory_PictureBox.Name = "WarningCategory_PictureBox";
            WarningCategory_PictureBox.Size = new Size(19, 18);
            WarningCategory_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningCategory_PictureBox.TabIndex = 39;
            WarningCategory_PictureBox.TabStop = false;
            WarningCategory_PictureBox.Visible = false;
            // 
            // WarningCategory_LinkLabel
            // 
            WarningCategory_LinkLabel.AutoSize = true;
            WarningCategory_LinkLabel.Font = new Font("Segoe UI", 10F);
            WarningCategory_LinkLabel.LinkArea = new LinkArea(21, 15);
            WarningCategory_LinkLabel.Location = new Point(471, 144);
            WarningCategory_LinkLabel.Name = "WarningCategory_LinkLabel";
            WarningCategory_LinkLabel.Size = new Size(227, 23);
            WarningCategory_LinkLabel.TabIndex = 40;
            WarningCategory_LinkLabel.TabStop = true;
            WarningCategory_LinkLabel.Text = "No categories exist. Create one here.";
            WarningCategory_LinkLabel.UseCompatibleTextRendering = true;
            WarningCategory_LinkLabel.Visible = false;
            WarningCategory_LinkLabel.LinkClicked += CategoryWarning_LinkLabel_LinkClicked;
            // 
            // Products_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 521);
            Controls.Add(WarningCategory_LinkLabel);
            Controls.Add(WarningCategory_PictureBox);
            Controls.Add(WarningProductName_PictureBox);
            Controls.Add(WarningProductName_Label);
            Controls.Add(ProductID_Label);
            Controls.Add(ProductID_TextBox);
            Controls.Add(Purchase_RadioButton);
            Controls.Add(Sale_RadioButton);
            Controls.Add(ProductCategory_Label);
            Controls.Add(ProductCategory_TextBox);
            Controls.Add(AddProduct_Button);
            Controls.Add(CountryOfOrigin_Label);
            Controls.Add(CountryOfOrigin_TextBox);
            Controls.Add(AddProduct_Label);
            Controls.Add(ProductName_Label);
            Controls.Add(ProductName_TextBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(900, 560);
            Name = "Products_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Products_Form_FormClosed;
            Click += CloseAllPanels;
            Resize += Products_Form_Resize;
            ((System.ComponentModel.ISupportInitialize)WarningProductName_PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)WarningCategory_PictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label CountryOfOrigin_Label;
        private Guna.UI2.WinForms.Guna2TextBox CountryOfOrigin_TextBox;
        private Label SellerName_Label;
        private Label AddProduct_Label;
        private Label ProductName_Label;
        private Guna.UI2.WinForms.Guna2TextBox ProductName_TextBox;
        private Guna.UI2.WinForms.Guna2Button AddProduct_Button;
        private Label ProductCategory_Label;
        private Guna.UI2.WinForms.Guna2TextBox ProductCategory_TextBox;
        private Guna.UI2.WinForms.Guna2RadioButton Sale_RadioButton;
        private Guna.UI2.WinForms.Guna2RadioButton Purchase_RadioButton;
        private Label ProductID_Label;
        private Guna.UI2.WinForms.Guna2TextBox ProductID_TextBox;
        private PictureBox WarningProductName_PictureBox;
        private Label WarningProductName_Label;
        private PictureBox WarningCategory_PictureBox;
        private LinkLabel WarningCategory_LinkLabel;
    }
}