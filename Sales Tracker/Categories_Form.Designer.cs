namespace Sales_Tracker
{
    partial class Categories_Form
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
            Category_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddCategory_Button = new Guna.UI2.WinForms.Guna2Button();
            AddCategory_Label = new Label();
            CategoryName_Label = new Label();
            Sale_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            Purchase_RadioButton = new Guna.UI2.WinForms.Guna2RadioButton();
            SuspendLayout();
            // 
            // Category_TextBox
            // 
            Category_TextBox.Anchor = AnchorStyles.Top;
            Category_TextBox.CustomizableEdges = customizableEdges1;
            Category_TextBox.DefaultText = "";
            Category_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Category_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Category_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Category_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Category_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Category_TextBox.Font = new Font("Segoe UI", 9F);
            Category_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Category_TextBox.Location = new Point(217, 103);
            Category_TextBox.MaxLength = 32;
            Category_TextBox.Name = "Category_TextBox";
            Category_TextBox.PasswordChar = '\0';
            Category_TextBox.PlaceholderText = "";
            Category_TextBox.SelectedText = "";
            Category_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Category_TextBox.Size = new Size(200, 36);
            Category_TextBox.TabIndex = 29;
            Category_TextBox.TextChanged += ValidateInputs;
            Category_TextBox.KeyDown += Category_TextBox_KeyDown;
            // 
            // AddCategory_Button
            // 
            AddCategory_Button.Anchor = AnchorStyles.Top;
            AddCategory_Button.BackColor = Color.Transparent;
            AddCategory_Button.BorderColor = Color.LightGray;
            AddCategory_Button.BorderRadius = 2;
            AddCategory_Button.BorderThickness = 1;
            AddCategory_Button.CustomizableEdges = customizableEdges3;
            AddCategory_Button.Enabled = false;
            AddCategory_Button.FillColor = Color.White;
            AddCategory_Button.Font = new Font("Segoe UI", 9.5F);
            AddCategory_Button.ForeColor = Color.Black;
            AddCategory_Button.Location = new Point(424, 103);
            AddCategory_Button.Margin = new Padding(4, 3, 4, 3);
            AddCategory_Button.Name = "AddCategory_Button";
            AddCategory_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddCategory_Button.Size = new Size(143, 36);
            AddCategory_Button.TabIndex = 28;
            AddCategory_Button.Tag = "0";
            AddCategory_Button.Text = "Add category";
            AddCategory_Button.Click += AddCategory_Button_Click;
            // 
            // AddCategory_Label
            // 
            AddCategory_Label.Anchor = AnchorStyles.Top;
            AddCategory_Label.AutoSize = true;
            AddCategory_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddCategory_Label.Location = new Point(324, 20);
            AddCategory_Label.Name = "AddCategory_Label";
            AddCategory_Label.Size = new Size(137, 30);
            AddCategory_Label.TabIndex = 27;
            AddCategory_Label.Text = "Add category";
            // 
            // CategoryName_Label
            // 
            CategoryName_Label.Anchor = AnchorStyles.Top;
            CategoryName_Label.AutoSize = true;
            CategoryName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CategoryName_Label.Location = new Point(217, 80);
            CategoryName_Label.Name = "CategoryName_Label";
            CategoryName_Label.Size = new Size(110, 20);
            CategoryName_Label.TabIndex = 30;
            CategoryName_Label.Text = "Category name";
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
            Sale_RadioButton.Location = new Point(169, 26);
            Sale_RadioButton.Name = "Sale_RadioButton";
            Sale_RadioButton.Size = new Size(78, 24);
            Sale_RadioButton.TabIndex = 32;
            Sale_RadioButton.Text = "For sale";
            Sale_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Sale_RadioButton.UncheckedState.BorderThickness = 2;
            Sale_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Sale_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Sale_RadioButton.CheckedChanged += Sale_RadioButton_CheckedChanged;
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
            Purchase_RadioButton.Location = new Point(52, 26);
            Purchase_RadioButton.Name = "Purchase_RadioButton";
            Purchase_RadioButton.Size = new Size(111, 24);
            Purchase_RadioButton.TabIndex = 31;
            Purchase_RadioButton.Text = "For purchase";
            Purchase_RadioButton.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            Purchase_RadioButton.UncheckedState.BorderThickness = 2;
            Purchase_RadioButton.UncheckedState.FillColor = Color.Transparent;
            Purchase_RadioButton.UncheckedState.InnerColor = Color.Transparent;
            Purchase_RadioButton.CheckedChanged += Purchase_RadioButton_CheckedChanged;
            // 
            // Categories_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(Sale_RadioButton);
            Controls.Add(Purchase_RadioButton);
            Controls.Add(CategoryName_Label);
            Controls.Add(Category_TextBox);
            Controls.Add(AddCategory_Button);
            Controls.Add(AddCategory_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(800, 500);
            Name = "Categories_Form";
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Categories_Form_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Guna.UI2.WinForms.Guna2TextBox Category_TextBox;
        private Guna.UI2.WinForms.Guna2Button AddCategory_Button;
        private Label AddCategory_Label;
        private Label CategoryName_Label;
        private Guna.UI2.WinForms.Guna2RadioButton Sale_RadioButton;
        private Guna.UI2.WinForms.Guna2RadioButton Purchase_RadioButton;
    }
}