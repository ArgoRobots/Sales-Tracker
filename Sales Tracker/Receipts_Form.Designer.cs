namespace Sales_Tracker
{
    partial class Receipts_Form
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Receipts_Form));
            ExportReceipts_Label = new Label();
            Category_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ProductID_Label = new Label();
            From_Label = new Label();
            From_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            To_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            To_Label = new Label();
            FilterByDate_CheckBox = new Guna.UI2.WinForms.Guna2CheckBox();
            Receipts_DataGridView = new Guna.UI2.WinForms.Guna2DataGridView();
            ClearFilters_Button = new Guna.UI2.WinForms.Guna2Button();
            Product_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            Sort_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            label1 = new Label();
            ExportSelected_Button = new Guna.UI2.WinForms.Guna2Button();
            ((System.ComponentModel.ISupportInitialize)Receipts_DataGridView).BeginInit();
            SuspendLayout();
            // 
            // ExportReceipts_Label
            // 
            ExportReceipts_Label.Anchor = AnchorStyles.Top;
            ExportReceipts_Label.AutoSize = true;
            ExportReceipts_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ExportReceipts_Label.Location = new Point(400, 23);
            ExportReceipts_Label.Name = "ExportReceipts_Label";
            ExportReceipts_Label.Size = new Size(150, 30);
            ExportReceipts_Label.TabIndex = 1;
            ExportReceipts_Label.Text = "Export receipts";
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
            Category_TextBox.Location = new Point(51, 103);
            Category_TextBox.Name = "Category_TextBox";
            Category_TextBox.PasswordChar = '\0';
            Category_TextBox.PlaceholderText = "Category";
            Category_TextBox.SelectedText = "";
            Category_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Category_TextBox.Size = new Size(200, 36);
            Category_TextBox.TabIndex = 2;
            Category_TextBox.TextChanged += FilterReceipts;
            // 
            // ProductID_Label
            // 
            ProductID_Label.Anchor = AnchorStyles.Top;
            ProductID_Label.AutoSize = true;
            ProductID_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ProductID_Label.Location = new Point(51, 80);
            ProductID_Label.Name = "ProductID_Label";
            ProductID_Label.Size = new Size(118, 20);
            ProductID_Label.TabIndex = 0;
            ProductID_Label.Text = "Filter by product";
            // 
            // From_Label
            // 
            From_Label.Anchor = AnchorStyles.Top;
            From_Label.AutoSize = true;
            From_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            From_Label.Location = new Point(295, 107);
            From_Label.Name = "From_Label";
            From_Label.Size = new Size(43, 20);
            From_Label.TabIndex = 0;
            From_Label.Text = "From";
            // 
            // From_DateTimePicker
            // 
            From_DateTimePicker.Anchor = AnchorStyles.Top;
            From_DateTimePicker.Checked = true;
            From_DateTimePicker.CustomizableEdges = customizableEdges3;
            From_DateTimePicker.FillColor = Color.White;
            From_DateTimePicker.Font = new Font("Segoe UI", 9F);
            From_DateTimePicker.Format = DateTimePickerFormat.Long;
            From_DateTimePicker.Location = new Point(295, 130);
            From_DateTimePicker.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            From_DateTimePicker.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            From_DateTimePicker.Name = "From_DateTimePicker";
            From_DateTimePicker.ShadowDecoration.CustomizableEdges = customizableEdges4;
            From_DateTimePicker.Size = new Size(200, 36);
            From_DateTimePicker.TabIndex = 6;
            From_DateTimePicker.Value = new DateTime(2024, 6, 6, 19, 37, 49, 128);
            From_DateTimePicker.ValueChanged += FilterReceipts;
            // 
            // To_DateTimePicker
            // 
            To_DateTimePicker.Anchor = AnchorStyles.Top;
            To_DateTimePicker.Checked = true;
            To_DateTimePicker.CustomizableEdges = customizableEdges5;
            To_DateTimePicker.FillColor = Color.White;
            To_DateTimePicker.Font = new Font("Segoe UI", 9F);
            To_DateTimePicker.Format = DateTimePickerFormat.Long;
            To_DateTimePicker.Location = new Point(295, 192);
            To_DateTimePicker.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            To_DateTimePicker.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            To_DateTimePicker.Name = "To_DateTimePicker";
            To_DateTimePicker.ShadowDecoration.CustomizableEdges = customizableEdges6;
            To_DateTimePicker.Size = new Size(200, 36);
            To_DateTimePicker.TabIndex = 7;
            To_DateTimePicker.Value = new DateTime(2024, 6, 6, 19, 37, 49, 128);
            To_DateTimePicker.ValueChanged += FilterReceipts;
            // 
            // To_Label
            // 
            To_Label.Anchor = AnchorStyles.Top;
            To_Label.AutoSize = true;
            To_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            To_Label.Location = new Point(295, 169);
            To_Label.Name = "To_Label";
            To_Label.Size = new Size(25, 20);
            To_Label.TabIndex = 0;
            To_Label.Text = "To";
            // 
            // FilterByDate_CheckBox
            // 
            FilterByDate_CheckBox.AutoSize = true;
            FilterByDate_CheckBox.Checked = true;
            FilterByDate_CheckBox.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            FilterByDate_CheckBox.CheckedState.BorderRadius = 0;
            FilterByDate_CheckBox.CheckedState.BorderThickness = 0;
            FilterByDate_CheckBox.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            FilterByDate_CheckBox.CheckState = CheckState.Checked;
            FilterByDate_CheckBox.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FilterByDate_CheckBox.Location = new Point(295, 80);
            FilterByDate_CheckBox.Name = "FilterByDate_CheckBox";
            FilterByDate_CheckBox.Size = new Size(115, 24);
            FilterByDate_CheckBox.TabIndex = 9;
            FilterByDate_CheckBox.Text = "Filter by date";
            FilterByDate_CheckBox.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            FilterByDate_CheckBox.UncheckedState.BorderRadius = 0;
            FilterByDate_CheckBox.UncheckedState.BorderThickness = 0;
            FilterByDate_CheckBox.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            FilterByDate_CheckBox.CheckedChanged += FilterReceipts;
            // 
            // Receipts_DataGridView
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            Receipts_DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            Receipts_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            Receipts_DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            Receipts_DataGridView.ColumnHeadersHeight = 4;
            Receipts_DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            Receipts_DataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            Receipts_DataGridView.GridColor = Color.FromArgb(231, 229, 255);
            Receipts_DataGridView.Location = new Point(12, 241);
            Receipts_DataGridView.Name = "Receipts_DataGridView";
            Receipts_DataGridView.RowHeadersVisible = false;
            Receipts_DataGridView.Size = new Size(926, 277);
            Receipts_DataGridView.TabIndex = 10;
            Receipts_DataGridView.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            Receipts_DataGridView.ThemeStyle.AlternatingRowsStyle.Font = null;
            Receipts_DataGridView.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            Receipts_DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            Receipts_DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            Receipts_DataGridView.ThemeStyle.BackColor = Color.White;
            Receipts_DataGridView.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            Receipts_DataGridView.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            Receipts_DataGridView.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            Receipts_DataGridView.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            Receipts_DataGridView.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            Receipts_DataGridView.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            Receipts_DataGridView.ThemeStyle.HeaderStyle.Height = 4;
            Receipts_DataGridView.ThemeStyle.ReadOnly = false;
            Receipts_DataGridView.ThemeStyle.RowsStyle.BackColor = Color.White;
            Receipts_DataGridView.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Receipts_DataGridView.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            Receipts_DataGridView.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            Receipts_DataGridView.ThemeStyle.RowsStyle.Height = 25;
            Receipts_DataGridView.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            Receipts_DataGridView.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // ClearFilters_Button
            // 
            ClearFilters_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ClearFilters_Button.BackColor = Color.Transparent;
            ClearFilters_Button.BorderColor = Color.LightGray;
            ClearFilters_Button.BorderRadius = 2;
            ClearFilters_Button.BorderThickness = 1;
            ClearFilters_Button.CustomizableEdges = customizableEdges7;
            ClearFilters_Button.DisabledState.BorderColor = Color.DarkGray;
            ClearFilters_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            ClearFilters_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            ClearFilters_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            ClearFilters_Button.FillColor = Color.White;
            ClearFilters_Button.Font = new Font("Segoe UI", 9F);
            ClearFilters_Button.ForeColor = Color.Black;
            ClearFilters_Button.Location = new Point(750, 103);
            ClearFilters_Button.Name = "ClearFilters_Button";
            ClearFilters_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            ClearFilters_Button.Size = new Size(150, 36);
            ClearFilters_Button.TabIndex = 13;
            ClearFilters_Button.Text = "Clear filters";
            ClearFilters_Button.Click += ClearFilters_Button_Click;
            // 
            // Product_TextBox
            // 
            Product_TextBox.Anchor = AnchorStyles.Top;
            Product_TextBox.CustomizableEdges = customizableEdges9;
            Product_TextBox.DefaultText = "";
            Product_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Product_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Product_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Product_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Product_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Product_TextBox.Font = new Font("Segoe UI", 9F);
            Product_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Product_TextBox.Location = new Point(51, 145);
            Product_TextBox.Name = "Product_TextBox";
            Product_TextBox.PasswordChar = '\0';
            Product_TextBox.PlaceholderText = "Product";
            Product_TextBox.SelectedText = "";
            Product_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges10;
            Product_TextBox.Size = new Size(200, 36);
            Product_TextBox.TabIndex = 14;
            Product_TextBox.TextChanged += FilterReceipts;
            // 
            // Sort_ComboBox
            // 
            Sort_ComboBox.Anchor = AnchorStyles.Top;
            Sort_ComboBox.BackColor = Color.Transparent;
            Sort_ComboBox.CustomizableEdges = customizableEdges11;
            Sort_ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            Sort_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Sort_ComboBox.FocusedColor = Color.FromArgb(94, 148, 255);
            Sort_ComboBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Sort_ComboBox.Font = new Font("Segoe UI", 10F);
            Sort_ComboBox.ForeColor = Color.FromArgb(68, 88, 112);
            Sort_ComboBox.ItemHeight = 30;
            Sort_ComboBox.Items.AddRange(new object[] { "Most recent", "Least recent", "Most expensive", "Least expensive" });
            Sort_ComboBox.Location = new Point(539, 103);
            Sort_ComboBox.Name = "Sort_ComboBox";
            Sort_ComboBox.ShadowDecoration.CustomizableEdges = customizableEdges12;
            Sort_ComboBox.Size = new Size(167, 36);
            Sort_ComboBox.TabIndex = 55;
            Sort_ComboBox.SelectedIndexChanged += SortReceipts;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(539, 80);
            label1.Name = "label1";
            label1.Size = new Size(36, 20);
            label1.TabIndex = 0;
            label1.Text = "Sort";
            // 
            // ExportSelected_Button
            // 
            ExportSelected_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ExportSelected_Button.BackColor = Color.Transparent;
            ExportSelected_Button.BorderColor = Color.LightGray;
            ExportSelected_Button.BorderRadius = 2;
            ExportSelected_Button.BorderThickness = 1;
            ExportSelected_Button.CustomizableEdges = customizableEdges13;
            ExportSelected_Button.DisabledState.BorderColor = Color.DarkGray;
            ExportSelected_Button.DisabledState.CustomBorderColor = Color.DarkGray;
            ExportSelected_Button.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            ExportSelected_Button.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            ExportSelected_Button.Enabled = false;
            ExportSelected_Button.FillColor = Color.White;
            ExportSelected_Button.Font = new Font("Segoe UI", 9F);
            ExportSelected_Button.ForeColor = Color.Black;
            ExportSelected_Button.Location = new Point(750, 145);
            ExportSelected_Button.Name = "ExportSelected_Button";
            ExportSelected_Button.ShadowDecoration.CustomizableEdges = customizableEdges14;
            ExportSelected_Button.Size = new Size(150, 36);
            ExportSelected_Button.TabIndex = 57;
            ExportSelected_Button.Text = "Export selected";
            ExportSelected_Button.Click += ExportSelected_Button_Click;
            // 
            // Receipts_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(950, 530);
            Controls.Add(ExportSelected_Button);
            Controls.Add(label1);
            Controls.Add(Sort_ComboBox);
            Controls.Add(Product_TextBox);
            Controls.Add(ClearFilters_Button);
            Controls.Add(Receipts_DataGridView);
            Controls.Add(FilterByDate_CheckBox);
            Controls.Add(To_Label);
            Controls.Add(To_DateTimePicker);
            Controls.Add(From_DateTimePicker);
            Controls.Add(From_Label);
            Controls.Add(ProductID_Label);
            Controls.Add(Category_TextBox);
            Controls.Add(ExportReceipts_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Receipts_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Receipts_Form_FormClosed;
            ((System.ComponentModel.ISupportInitialize)Receipts_DataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ExportReceipts_Label;
        private Guna.UI2.WinForms.Guna2TextBox Category_TextBox;
        private Label ProductID_Label;
        private Label From_Label;
        private Guna.UI2.WinForms.Guna2DateTimePicker From_DateTimePicker;
        private Guna.UI2.WinForms.Guna2DateTimePicker To_DateTimePicker;
        private Label To_Label;
        private Guna.UI2.WinForms.Guna2CheckBox FilterByDate_CheckBox;
        private Guna.UI2.WinForms.Guna2DataGridView Receipts_DataGridView;
        public Guna.UI2.WinForms.Guna2Button ClearFilters_Button;
        private Guna.UI2.WinForms.Guna2TextBox Product_TextBox;
        private Guna.UI2.WinForms.Guna2ComboBox Sort_ComboBox;
        private Label label1;
        public Guna.UI2.WinForms.Guna2Button DownloadSelected_Button;
        public Guna.UI2.WinForms.Guna2Button ExportSelected_Button;
    }
}