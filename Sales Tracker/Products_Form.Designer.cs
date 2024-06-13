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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            CountryOfOrigin_Label = new Label();
            CountryOfOrigin_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            SellerName_Label = new Label();
            SellerName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddProduct_Label = new Label();
            ProductName_Label = new Label();
            ProductName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddProduct_Button = new Guna.UI2.WinForms.Guna2Button();
            Purchase_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            Sale_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            ItemCategory_Label = new Label();
            ItemCategory_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
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
            CountryOfOrigin_Label.TabIndex = 13;
            CountryOfOrigin_Label.Text = "Country of origin";
            // 
            // CountryOfOrigin_TextBox
            // 
            CountryOfOrigin_TextBox.Anchor = AnchorStyles.Top;
            CountryOfOrigin_TextBox.CustomizableEdges = customizableEdges1;
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
            CountryOfOrigin_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            CountryOfOrigin_TextBox.Size = new Size(200, 36);
            CountryOfOrigin_TextBox.TabIndex = 12;
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;
            // 
            // SellerName_Label
            // 
            SellerName_Label.Anchor = AnchorStyles.Top;
            SellerName_Label.AutoSize = true;
            SellerName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SellerName_Label.Location = new Point(445, 80);
            SellerName_Label.Name = "SellerName_Label";
            SellerName_Label.Size = new Size(87, 20);
            SellerName_Label.TabIndex = 11;
            SellerName_Label.Text = "Seller name";
            // 
            // SellerName_TextBox
            // 
            SellerName_TextBox.Anchor = AnchorStyles.Top;
            SellerName_TextBox.CustomizableEdges = customizableEdges3;
            SellerName_TextBox.DefaultText = "";
            SellerName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            SellerName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            SellerName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            SellerName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            SellerName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            SellerName_TextBox.Font = new Font("Segoe UI", 9F);
            SellerName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            SellerName_TextBox.Location = new Point(445, 103);
            SellerName_TextBox.MaxLength = 32;
            SellerName_TextBox.Name = "SellerName_TextBox";
            SellerName_TextBox.PasswordChar = '\0';
            SellerName_TextBox.PlaceholderText = "";
            SellerName_TextBox.SelectedText = "";
            SellerName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges4;
            SellerName_TextBox.Size = new Size(200, 36);
            SellerName_TextBox.TabIndex = 10;
            SellerName_TextBox.TextChanged += ValidateInputs;
            // 
            // AddProduct_Label
            // 
            AddProduct_Label.Anchor = AnchorStyles.Top;
            AddProduct_Label.AutoSize = true;
            AddProduct_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddProduct_Label.Location = new Point(378, 20);
            AddProduct_Label.Name = "AddProduct_Label";
            AddProduct_Label.Size = new Size(129, 30);
            AddProduct_Label.TabIndex = 9;
            AddProduct_Label.Text = "Add product";
            // 
            // ProductName_Label
            // 
            ProductName_Label.Anchor = AnchorStyles.Top;
            ProductName_Label.AutoSize = true;
            ProductName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ProductName_Label.Location = new Point(33, 80);
            ProductName_Label.Name = "ProductName_Label";
            ProductName_Label.Size = new Size(101, 20);
            ProductName_Label.TabIndex = 8;
            ProductName_Label.Text = "Product name";
            // 
            // ProductName_TextBox
            // 
            ProductName_TextBox.Anchor = AnchorStyles.Top;
            ProductName_TextBox.CustomizableEdges = customizableEdges5;
            ProductName_TextBox.DefaultText = "";
            ProductName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ProductName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ProductName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ProductName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ProductName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductName_TextBox.Font = new Font("Segoe UI", 9F);
            ProductName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ProductName_TextBox.Location = new Point(33, 103);
            ProductName_TextBox.MaxLength = 32;
            ProductName_TextBox.Name = "ProductName_TextBox";
            ProductName_TextBox.PasswordChar = '\0';
            ProductName_TextBox.PlaceholderText = "";
            ProductName_TextBox.SelectedText = "";
            ProductName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            ProductName_TextBox.Size = new Size(200, 36);
            ProductName_TextBox.TabIndex = 7;
            ProductName_TextBox.TextChanged += ValidateInputs;
            // 
            // AddProduct_Button
            // 
            AddProduct_Button.Anchor = AnchorStyles.Top;
            AddProduct_Button.BackColor = Color.Transparent;
            AddProduct_Button.BorderColor = Color.LightGray;
            AddProduct_Button.BorderRadius = 2;
            AddProduct_Button.BorderThickness = 1;
            AddProduct_Button.CustomizableEdges = customizableEdges7;
            AddProduct_Button.Enabled = false;
            AddProduct_Button.FillColor = Color.White;
            AddProduct_Button.Font = new Font("Segoe UI", 9.5F);
            AddProduct_Button.ForeColor = Color.Black;
            AddProduct_Button.Location = new Point(371, 180);
            AddProduct_Button.Margin = new Padding(4, 3, 4, 3);
            AddProduct_Button.Name = "AddProduct_Button";
            AddProduct_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            AddProduct_Button.Size = new Size(143, 32);
            AddProduct_Button.TabIndex = 18;
            AddProduct_Button.Tag = "0";
            AddProduct_Button.Text = "Add product";
            AddProduct_Button.Click += AddProduct_Button_Click;
            // 
            // Purchase_RadioButton
            // 
            Purchase_RadioButton.Anchor = AnchorStyles.Top;
            Purchase_RadioButton.AutoSize = true;
            Purchase_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Purchase_RadioButton.CheckedState.BorderThickness = 0;
            Purchase_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Purchase_RadioButton.CheckedState.InnerColor = Color.White;
            Purchase_RadioButton.CheckedState.InnerOffset = -4;
            Purchase_RadioButton.Font = new Font("Segoe UI", 11F);
            Purchase_RadioButton.Location = new Point(33, 145);
            Purchase_RadioButton.Name = "Purchase_RadioButton";
            Purchase_RadioButton.Size = new Size(111, 24);
            Purchase_RadioButton.TabIndex = 19;
            Purchase_RadioButton.Text = "For purchase";
            Purchase_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Purchase_RadioButton.UncheckedState.BorderThickness = 2;
            Purchase_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Purchase_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Purchase_RadioButton.CheckedChanged += Purchase_RadioButton_CheckedChanged;
            // 
            // Sale_RadioButton
            // 
            Sale_RadioButton.Anchor = AnchorStyles.Top;
            Sale_RadioButton.AutoSize = true;
            Sale_RadioButton.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            Sale_RadioButton.CheckedState.BorderThickness = 0;
            Sale_RadioButton.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            Sale_RadioButton.CheckedState.InnerColor = Color.White;
            Sale_RadioButton.CheckedState.InnerOffset = -4;
            Sale_RadioButton.Font = new Font("Segoe UI", 11F);
            Sale_RadioButton.Location = new Point(150, 145);
            Sale_RadioButton.Name = "Sale_RadioButton";
            Sale_RadioButton.Size = new Size(78, 24);
            Sale_RadioButton.TabIndex = 20;
            Sale_RadioButton.Text = "For sale";
            Sale_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Sale_RadioButton.UncheckedState.BorderThickness = 2;
            Sale_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Sale_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Sale_RadioButton.CheckedChanged += Sale_RadioButton_CheckedChanged;
            // 
            // ItemCategory_Label
            // 
            ItemCategory_Label.Anchor = AnchorStyles.Top;
            ItemCategory_Label.AutoSize = true;
            ItemCategory_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ItemCategory_Label.Location = new Point(239, 80);
            ItemCategory_Label.Name = "ItemCategory_Label";
            ItemCategory_Label.Size = new Size(101, 20);
            ItemCategory_Label.TabIndex = 26;
            ItemCategory_Label.Text = "Item category";
            // 
            // ItemCategory_TextBox
            // 
            ItemCategory_TextBox.Anchor = AnchorStyles.Top;
            ItemCategory_TextBox.CustomizableEdges = customizableEdges9;
            ItemCategory_TextBox.DefaultText = "";
            ItemCategory_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ItemCategory_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ItemCategory_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ItemCategory_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ItemCategory_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ItemCategory_TextBox.Font = new Font("Segoe UI", 9F);
            ItemCategory_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ItemCategory_TextBox.Location = new Point(239, 103);
            ItemCategory_TextBox.MaxLength = 32;
            ItemCategory_TextBox.Name = "ItemCategory_TextBox";
            ItemCategory_TextBox.PasswordChar = '\0';
            ItemCategory_TextBox.PlaceholderText = "";
            ItemCategory_TextBox.SelectedText = "";
            ItemCategory_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges10;
            ItemCategory_TextBox.Size = new Size(200, 36);
            ItemCategory_TextBox.TabIndex = 25;
            // 
            // Products_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 521);
            Controls.Add(ItemCategory_Label);
            Controls.Add(ItemCategory_TextBox);
            Controls.Add(Sale_RadioButton);
            Controls.Add(Purchase_RadioButton);
            Controls.Add(AddProduct_Button);
            Controls.Add(CountryOfOrigin_Label);
            Controls.Add(CountryOfOrigin_TextBox);
            Controls.Add(SellerName_Label);
            Controls.Add(SellerName_TextBox);
            Controls.Add(AddProduct_Label);
            Controls.Add(ProductName_Label);
            Controls.Add(ProductName_TextBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(900, 560);
            Name = "Products_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Products_Form_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label CountryOfOrigin_Label;
        private Guna.UI2.WinForms.Guna2TextBox CountryOfOrigin_TextBox;
        private Label SellerName_Label;
        private Guna.UI2.WinForms.Guna2TextBox SellerName_TextBox;
        private Label AddProduct_Label;
        private Label ProductName_Label;
        private Guna.UI2.WinForms.Guna2TextBox ProductName_TextBox;
        private Guna.UI2.WinForms.Guna2Button AddProduct_Button;
        private Guna.UI2.WinForms.Guna2RadioButton Purchase_RadioButton;
        private Guna.UI2.WinForms.Guna2RadioButton Sale_RadioButton;
        private Label ItemCategory_Label;
        private Guna.UI2.WinForms.Guna2TextBox ItemCategory_TextBox;
    }
}