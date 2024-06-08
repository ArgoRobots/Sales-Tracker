namespace Sales_Tracker
{
    partial class AddSale_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Date_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            Date_Label = new Label();
            AddSale_Button = new Guna.UI2.WinForms.Guna2Button();
            Tax_Label = new Label();
            Tax_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Shipping_Label = new Label();
            Shipping_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            PricePerUnit_Label = new Label();
            PricePerUnit_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Quantity_Label = new Label();
            Quantity_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ItemName_Label = new Label();
            ItemName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            BuyerName_Label = new Label();
            BuyerName_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            label2 = new Label();
            SaleID_Label = new Label();
            SaleID_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            SuspendLayout();
            // 
            // Date_DateTimePicker
            // 
            Date_DateTimePicker.Checked = true;
            Date_DateTimePicker.CustomizableEdges = customizableEdges1;
            Date_DateTimePicker.FillColor = Color.White;
            Date_DateTimePicker.Font = new Font("Segoe UI", 9F);
            Date_DateTimePicker.Format = DateTimePickerFormat.Long;
            Date_DateTimePicker.Location = new Point(55, 193);
            Date_DateTimePicker.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            Date_DateTimePicker.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            Date_DateTimePicker.Name = "Date_DateTimePicker";
            Date_DateTimePicker.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Date_DateTimePicker.Size = new Size(200, 36);
            Date_DateTimePicker.TabIndex = 43;
            Date_DateTimePicker.Value = new DateTime(2024, 6, 6, 19, 37, 49, 128);
            Date_DateTimePicker.ValueChanged += ValidateInputs;
            // 
            // Date_Label
            // 
            Date_Label.AutoSize = true;
            Date_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Date_Label.Location = new Point(55, 170);
            Date_Label.Name = "Date_Label";
            Date_Label.Size = new Size(41, 20);
            Date_Label.TabIndex = 42;
            Date_Label.Text = "Date";
            // 
            // AddSale_Button
            // 
            AddSale_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddSale_Button.BackColor = Color.Transparent;
            AddSale_Button.BorderColor = Color.LightGray;
            AddSale_Button.BorderRadius = 2;
            AddSale_Button.BorderThickness = 1;
            AddSale_Button.CustomizableEdges = customizableEdges3;
            AddSale_Button.Enabled = false;
            AddSale_Button.FillColor = Color.White;
            AddSale_Button.Font = new Font("Segoe UI", 9.5F);
            AddSale_Button.ForeColor = Color.Black;
            AddSale_Button.Location = new Point(296, 260);
            AddSale_Button.Margin = new Padding(4, 3, 4, 3);
            AddSale_Button.Name = "AddSale_Button";
            AddSale_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddSale_Button.Size = new Size(143, 32);
            AddSale_Button.TabIndex = 38;
            AddSale_Button.Text = "Add sale";
            AddSale_Button.Click += AddSale_Button_Click;
            // 
            // Tax_Label
            // 
            Tax_Label.AutoSize = true;
            Tax_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Tax_Label.Location = new Point(579, 170);
            Tax_Label.Name = "Tax_Label";
            Tax_Label.Size = new Size(30, 20);
            Tax_Label.TabIndex = 37;
            Tax_Label.Text = "Tax";
            // 
            // Tax_TextBox
            // 
            Tax_TextBox.CustomizableEdges = customizableEdges5;
            Tax_TextBox.DefaultText = "";
            Tax_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Tax_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Tax_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Tax_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Tax_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Tax_TextBox.Font = new Font("Segoe UI", 9F);
            Tax_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Tax_TextBox.Location = new Point(579, 193);
            Tax_TextBox.MaxLength = 10;
            Tax_TextBox.Name = "Tax_TextBox";
            Tax_TextBox.PasswordChar = '\0';
            Tax_TextBox.PlaceholderText = "";
            Tax_TextBox.SelectedText = "";
            Tax_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Tax_TextBox.Size = new Size(100, 36);
            Tax_TextBox.TabIndex = 36;
            Tax_TextBox.TextChanged += ValidateInputs;
            // 
            // Shipping_Label
            // 
            Shipping_Label.AutoSize = true;
            Shipping_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Shipping_Label.Location = new Point(473, 170);
            Shipping_Label.Name = "Shipping_Label";
            Shipping_Label.Size = new Size(68, 20);
            Shipping_Label.TabIndex = 35;
            Shipping_Label.Text = "Shipping";
            // 
            // Shipping_TextBox
            // 
            Shipping_TextBox.CustomizableEdges = customizableEdges7;
            Shipping_TextBox.DefaultText = "";
            Shipping_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Shipping_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Shipping_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Shipping_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Shipping_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Shipping_TextBox.Font = new Font("Segoe UI", 9F);
            Shipping_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Shipping_TextBox.Location = new Point(473, 193);
            Shipping_TextBox.MaxLength = 10;
            Shipping_TextBox.Name = "Shipping_TextBox";
            Shipping_TextBox.PasswordChar = '\0';
            Shipping_TextBox.PlaceholderText = "";
            Shipping_TextBox.SelectedText = "";
            Shipping_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges8;
            Shipping_TextBox.Size = new Size(100, 36);
            Shipping_TextBox.TabIndex = 34;
            Shipping_TextBox.TextChanged += ValidateInputs;
            // 
            // PricePerUnit_Label
            // 
            PricePerUnit_Label.AutoSize = true;
            PricePerUnit_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            PricePerUnit_Label.Location = new Point(367, 170);
            PricePerUnit_Label.Name = "PricePerUnit_Label";
            PricePerUnit_Label.Size = new Size(96, 20);
            PricePerUnit_Label.TabIndex = 33;
            PricePerUnit_Label.Text = "Price per unit";
            // 
            // PricePerUnit_TextBox
            // 
            PricePerUnit_TextBox.CustomizableEdges = customizableEdges9;
            PricePerUnit_TextBox.DefaultText = "";
            PricePerUnit_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            PricePerUnit_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            PricePerUnit_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            PricePerUnit_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            PricePerUnit_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            PricePerUnit_TextBox.Font = new Font("Segoe UI", 9F);
            PricePerUnit_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            PricePerUnit_TextBox.Location = new Point(367, 193);
            PricePerUnit_TextBox.MaxLength = 10;
            PricePerUnit_TextBox.Name = "PricePerUnit_TextBox";
            PricePerUnit_TextBox.PasswordChar = '\0';
            PricePerUnit_TextBox.PlaceholderText = "";
            PricePerUnit_TextBox.SelectedText = "";
            PricePerUnit_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges10;
            PricePerUnit_TextBox.Size = new Size(100, 36);
            PricePerUnit_TextBox.TabIndex = 32;
            PricePerUnit_TextBox.TextChanged += ValidateInputs;
            // 
            // Quantity_Label
            // 
            Quantity_Label.AutoSize = true;
            Quantity_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Quantity_Label.Location = new Point(261, 170);
            Quantity_Label.Name = "Quantity_Label";
            Quantity_Label.Size = new Size(65, 20);
            Quantity_Label.TabIndex = 31;
            Quantity_Label.Text = "Quantity";
            // 
            // Quantity_TextBox
            // 
            Quantity_TextBox.CustomizableEdges = customizableEdges11;
            Quantity_TextBox.DefaultText = "";
            Quantity_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Quantity_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Quantity_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Quantity_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Quantity_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Quantity_TextBox.Font = new Font("Segoe UI", 9F);
            Quantity_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Quantity_TextBox.Location = new Point(261, 193);
            Quantity_TextBox.MaxLength = 10;
            Quantity_TextBox.Name = "Quantity_TextBox";
            Quantity_TextBox.PasswordChar = '\0';
            Quantity_TextBox.PlaceholderText = "";
            Quantity_TextBox.SelectedText = "";
            Quantity_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges12;
            Quantity_TextBox.Size = new Size(100, 36);
            Quantity_TextBox.TabIndex = 30;
            Quantity_TextBox.TextChanged += ValidateInputs;
            // 
            // ItemName_Label
            // 
            ItemName_Label.AutoSize = true;
            ItemName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ItemName_Label.Location = new Point(473, 80);
            ItemName_Label.Name = "ItemName_Label";
            ItemName_Label.Size = new Size(80, 20);
            ItemName_Label.TabIndex = 29;
            ItemName_Label.Text = "Item name";
            // 
            // ItemName_TextBox
            // 
            ItemName_TextBox.CustomizableEdges = customizableEdges13;
            ItemName_TextBox.DefaultText = "";
            ItemName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            ItemName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            ItemName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            ItemName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            ItemName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            ItemName_TextBox.Font = new Font("Segoe UI", 9F);
            ItemName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            ItemName_TextBox.Location = new Point(473, 103);
            ItemName_TextBox.MaxLength = 32;
            ItemName_TextBox.Name = "ItemName_TextBox";
            ItemName_TextBox.PasswordChar = '\0';
            ItemName_TextBox.PlaceholderText = "";
            ItemName_TextBox.SelectedText = "";
            ItemName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges14;
            ItemName_TextBox.Size = new Size(200, 36);
            ItemName_TextBox.TabIndex = 28;
            ItemName_TextBox.TextChanged += ValidateInputs;
            // 
            // BuyerName_Label
            // 
            BuyerName_Label.AutoSize = true;
            BuyerName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BuyerName_Label.Location = new Point(267, 80);
            BuyerName_Label.Name = "BuyerName_Label";
            BuyerName_Label.Size = new Size(87, 20);
            BuyerName_Label.TabIndex = 27;
            BuyerName_Label.Text = "Buyer name";
            // 
            // BuyerName_TextBox
            // 
            BuyerName_TextBox.CustomizableEdges = customizableEdges15;
            BuyerName_TextBox.DefaultText = "";
            BuyerName_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            BuyerName_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            BuyerName_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            BuyerName_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            BuyerName_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            BuyerName_TextBox.Font = new Font("Segoe UI", 9F);
            BuyerName_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            BuyerName_TextBox.Location = new Point(267, 103);
            BuyerName_TextBox.MaxLength = 32;
            BuyerName_TextBox.Name = "BuyerName_TextBox";
            BuyerName_TextBox.PasswordChar = '\0';
            BuyerName_TextBox.PlaceholderText = "";
            BuyerName_TextBox.SelectedText = "";
            BuyerName_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges16;
            BuyerName_TextBox.Size = new Size(200, 36);
            BuyerName_TextBox.TabIndex = 26;
            BuyerName_TextBox.TextChanged += ValidateInputs;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(321, 20);
            label2.Name = "label2";
            label2.Size = new Size(93, 30);
            label2.TabIndex = 25;
            label2.Text = "Add sale";
            // 
            // SaleID_Label
            // 
            SaleID_Label.AutoSize = true;
            SaleID_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SaleID_Label.Location = new Point(61, 80);
            SaleID_Label.Name = "SaleID_Label";
            SaleID_Label.Size = new Size(56, 20);
            SaleID_Label.TabIndex = 24;
            SaleID_Label.Text = "Sale ID";
            // 
            // SaleID_TextBox
            // 
            SaleID_TextBox.CustomizableEdges = customizableEdges17;
            SaleID_TextBox.DefaultText = "";
            SaleID_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            SaleID_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            SaleID_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            SaleID_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            SaleID_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            SaleID_TextBox.Font = new Font("Segoe UI", 9F);
            SaleID_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            SaleID_TextBox.Location = new Point(61, 103);
            SaleID_TextBox.MaxLength = 32;
            SaleID_TextBox.Name = "SaleID_TextBox";
            SaleID_TextBox.PasswordChar = '\0';
            SaleID_TextBox.PlaceholderText = "";
            SaleID_TextBox.SelectedText = "";
            SaleID_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges18;
            SaleID_TextBox.Size = new Size(200, 36);
            SaleID_TextBox.TabIndex = 23;
            SaleID_TextBox.TextChanged += ValidateInputs;
            // 
            // AddSale_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(734, 391);
            Controls.Add(Date_DateTimePicker);
            Controls.Add(Date_Label);
            Controls.Add(AddSale_Button);
            Controls.Add(Tax_Label);
            Controls.Add(Tax_TextBox);
            Controls.Add(Shipping_Label);
            Controls.Add(Shipping_TextBox);
            Controls.Add(PricePerUnit_Label);
            Controls.Add(PricePerUnit_TextBox);
            Controls.Add(Quantity_Label);
            Controls.Add(Quantity_TextBox);
            Controls.Add(ItemName_Label);
            Controls.Add(ItemName_TextBox);
            Controls.Add(BuyerName_Label);
            Controls.Add(BuyerName_TextBox);
            Controls.Add(label2);
            Controls.Add(SaleID_Label);
            Controls.Add(SaleID_TextBox);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "AddSale_Form";
            StartPosition = FormStartPosition.CenterScreen;
            TextChanged += ValidateInputs;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2DateTimePicker Date_DateTimePicker;
        private Label Date_Label;
        private Guna.UI2.WinForms.Guna2Button AddSale_Button;
        private Label Tax_Label;
        private Guna.UI2.WinForms.Guna2TextBox Tax_TextBox;
        private Label Shipping_Label;
        private Guna.UI2.WinForms.Guna2TextBox Shipping_TextBox;
        private Label PricePerUnit_Label;
        private Guna.UI2.WinForms.Guna2TextBox PricePerUnit_TextBox;
        private Label Quantity_Label;
        private Guna.UI2.WinForms.Guna2TextBox Quantity_TextBox;
        private Label ItemName_Label;
        private Guna.UI2.WinForms.Guna2TextBox ItemName_TextBox;
        private Label BuyerName_Label;
        private Guna.UI2.WinForms.Guna2TextBox BuyerName_TextBox;
        private Label label2;
        private Label SaleID_Label;
        private Guna.UI2.WinForms.Guna2TextBox SaleID_TextBox;
    }
}