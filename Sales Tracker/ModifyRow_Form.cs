using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : BaseForm
    {
        // Variables
        private readonly string selectedTag = "";
        private readonly DataGridViewRow selectedRow;

        // Init
        public ModifyRow_Form(DataGridViewRow row)
        {
            InitializeComponent();

            selectedRow = row;
            selectedTag = row.DataGridView.Tag.ToString();

            ConstructControls();
            Theme.SetThemeForForm(this);
        }
        private void ConstructControls()
        {
            int left = 0, secondLeft = 0;

            if (selectedTag == MainMenu_Form.DataGridViewTags.SaleOrPurchase.ToString())
            {
                (left, secondLeft) = ConstructControlsForSaleOrPurchase();
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTags.AddCategory.ToString())
            {
                ConstructControlsForAddCategory();
                left = 300;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTags.AddProduct.ToString())
            {
                left = ConstructControlsForAddProduct();
            }

            CenterControls(left, secondLeft);
        }
        private void CenterControls(int left, int secondLeft)
        {
            Width = left + 80;
            Panel.Width = left;
            Panel.Left = (Width - Panel.Width) / 2 - 5;

            if (secondRow)
            {
                if (secondLeft > left)
                {
                    Width = secondLeft + 80;
                }

                Height += 100;
                SecondPanel.Width = secondLeft;
                SecondPanel.Left = (Width - SecondPanel.Width) / 2 - 5;
            }
        }
        private bool secondRow = false;
        private readonly List<string> listOfOldValues = [];
        private (int, int) ConstructControlsForSaleOrPurchase()
        {
            ConstructPanel();
            int left = 0, secondLeft = 0;

            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);
                string text;

                switch (columnName)
                {
                    case nameof(SalesColumns.SalesID):
                    case nameof(PurchaseColumns.PurchaseID):
                        if (MainMenu_Form.Instance.Selected == Options.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[SalesColumns.SalesID];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.PurchaseID]; }

                        ConstructLabel(text, left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, Panel);
                        left += 110;
                        break;

                    case nameof(SalesColumns.CustomerName):
                    case nameof(PurchaseColumns.BuyerName):
                        if (MainMenu_Form.Instance.Selected == Options.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[SalesColumns.CustomerName];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.BuyerName]; }

                        ConstructLabel(text, left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, Panel);
                        left += 110;
                        break;

                    case nameof(PurchaseColumns.Product):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.Product], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, Panel);
                        left += 110;
                        break;

                    case nameof(PurchaseColumns.Date):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.Date], left, Panel);
                        ConstructDatePicker(left, columnName, DateTime.Parse(cellValue), Panel);
                        left += 190;
                        break;

                    case nameof(PurchaseColumns.Quantity):
                        secondRow = true;

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.Quantity], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbers, false, SecondPanel);
                        secondLeft += 110;
                        break;

                    case nameof(PurchaseColumns.PricePerUnit):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.PricePerUnit], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, SecondPanel);
                        secondLeft += 110;
                        break;

                    case nameof(PurchaseColumns.Shipping):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.Shipping], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, SecondPanel);
                        secondLeft += 110;
                        break;

                    case nameof(PurchaseColumns.Tax):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.Tax], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, SecondPanel);
                        secondLeft += 110;
                        break;
                }
            }
            return (left, secondLeft);
        }
        private void ConstructControlsForAddCategory()
        {
            string cellValue = selectedRow.Cells[0].Value?.ToString() ?? "";
            listOfOldValues.Add(cellValue);

            ConstructLabel(MainMenu_Form.Instance.CategoryColumn, 0, Panel);
            controlToFocus = ConstructTextBox(0, MainMenu_Form.Instance.CategoryColumn, cellValue, 50, KeyPressValidation.None, true, Panel);
        }
        private int ConstructControlsForAddProduct()
        {
            int left = 0;

            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(Products_Form.Columns.ProductName):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.ProductName], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, Panel);
                        break;

                    case nameof(Products_Form.Columns.ProductCategory):
                        string[] array;
                        if (MainMenu_Form.Instance.Selected == Options.ProductSales)
                        {
                            array = MainMenu_Form.Instance.GetProductCategorySaleNames().ToArray();
                        }
                        else { array = MainMenu_Form.Instance.GetProductCategoryPurchaseNames().ToArray(); }

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.ProductCategory], left, Panel);
                        ConstructGunaComboBox(left, columnName, array, cellValue, Panel);
                        left += 80;
                        break;
                    case nameof(Products_Form.Columns.CountryOfOrigin):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.CountryOfOrigin], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, Panel);
                        break;
                }
                left += 110;
            }
            return left;
        }


        // Form
        private Control controlToFocus;
        private void ModifyRow_Form_Shown(object sender, EventArgs e)
        {
            controlToFocus?.Focus();
        }


        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {
            SaveInRow();
            SaveInListsAndUpdateDataGridViews();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }


        // Functions
        private void InputChanged(object sender, EventArgs e)
        {
            Control senderControl = sender as Control;
            foreach (Control control in senderControl.Parent.Controls)
            {
                if (control is Guna2TextBox gunaTextBox)
                {
                    if (string.IsNullOrEmpty(gunaTextBox.Text))
                    {
                        Save_Button.Enabled = false;
                        return;
                    }
                }
                else if (control is Guna2ComboBox gunaComboBox)
                {
                    if (string.IsNullOrEmpty(gunaComboBox.Text))
                    {
                        Save_Button.Enabled = false;
                        return;
                    }
                }
            }
            Save_Button.Enabled = true;
        }
        private void SaveInRow()
        {
            IEnumerable<Control> allControls = Panel.Controls.Cast<Control>();

            // If SecondPanel is not null, concatenate its controls
            if (SecondPanel != null)
            {
                allControls = allControls.Concat(SecondPanel.Controls.Cast<Control>());
            }

            foreach (Control control in allControls)
            {
                if (control is Guna2TextBox gTextBox)
                {
                    string columnName = gTextBox.Name;
                    selectedRow.Cells[columnName].Value = gTextBox.Text;
                }
                else if (control is Guna2ComboBox gComboBox)
                {
                    string columnName = gComboBox.Name;
                    selectedRow.Cells[columnName].Value = gComboBox.SelectedItem.ToString();
                }
                else if (control is Guna2DateTimePicker gDatePicker)
                {
                    string columnName = gDatePicker.Name;
                    selectedRow.Cells[columnName].Value = Tools.FormatDate(gDatePicker.Value);
                }
            }
            // Update total column
            if (selectedTag == MainMenu_Form.DataGridViewTags.SaleOrPurchase.ToString())
            {
                if (MainMenu_Form.Instance.Selected == Options.Sales)
                {
                    int quantity = int.Parse(selectedRow.Cells[PurchaseColumns.Quantity.ToString()].Value.ToString());
                    decimal pricePerUnit = decimal.Parse(selectedRow.Cells[PurchaseColumns.PricePerUnit.ToString()].Value.ToString());
                    decimal shipping = decimal.Parse(selectedRow.Cells[PurchaseColumns.Shipping.ToString()].Value.ToString());
                    decimal tax = decimal.Parse(selectedRow.Cells[PurchaseColumns.Tax.ToString()].Value.ToString());
                    decimal totalPrice = quantity * pricePerUnit + shipping + tax;
                    selectedRow.Cells[PurchaseColumns.TotalExpenses.ToString()].Value = totalPrice;
                }
                else
                {
                    int quantity = int.Parse(selectedRow.Cells[PurchaseColumns.Quantity.ToString()].Value.ToString());
                    decimal pricePerUnit = decimal.Parse(selectedRow.Cells[PurchaseColumns.PricePerUnit.ToString()].Value.ToString());
                    decimal shipping = decimal.Parse(selectedRow.Cells[PurchaseColumns.Shipping.ToString()].Value.ToString());
                    decimal tax = decimal.Parse(selectedRow.Cells[PurchaseColumns.Tax.ToString()].Value.ToString());
                    decimal totalPrice = quantity * pricePerUnit + shipping + tax;
                    selectedRow.Cells[PurchaseColumns.TotalExpenses.ToString()].Value = totalPrice;
                }
            }
            MainMenu_Form.Instance.LoadGraphs();
            Close();
        }
        private void SaveInListsAndUpdateDataGridViews()
        {
            if (selectedTag == MainMenu_Form.DataGridViewTags.SaleOrPurchase.ToString()) { return; }

            // Concatenate the rows from both DataGridViews
            IEnumerable<DataGridViewRow> allRows = MainMenu_Form.Instance.Sales_DataGridView.Rows.Cast<DataGridViewRow>()
                            .Concat(MainMenu_Form.Instance.Purchases_DataGridView.Rows.Cast<DataGridViewRow>());
            string oldName = "";


            if (selectedTag == MainMenu_Form.DataGridViewTags.AddCategory.ToString())
            {
                // Get category
                Category category;
                if (MainMenu_Form.Instance.Selected == Options.CategorySales || MainMenu_Form.Instance.Selected == Options.ProductSales)
                {
                    category = MainMenu_Form.Instance.productCategorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
                }
                else
                {
                    category = MainMenu_Form.Instance.productCategoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
                }

                // Update list
                oldName = category.Name;
                category.Name = selectedRow.Cells[0].Value.ToString();

                // Update all instances in DataGridViews
                string categoryColumn = SalesColumns.Category.ToString();
                foreach (DataGridViewRow row in allRows)
                {
                    if (row.Cells[categoryColumn].Value.ToString() == oldName)
                    {
                        row.Cells[categoryColumn].Value = category.Name;
                    }
                }
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTags.AddProduct.ToString())
            {
                // Get category
                Category category;
                if (MainMenu_Form.Instance.Selected == Options.CategorySales || MainMenu_Form.Instance.Selected == Options.ProductSales)
                {
                    category = MainMenu_Form.Instance.productCategorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[1]);
                }
                else
                {
                    category = MainMenu_Form.Instance.productCategoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[1]);
                }

                // Get product
                Product product = category.ProductList.FirstOrDefault(p => p.Name == listOfOldValues[0]);

                // If product changed
                if (product.Name != selectedRow.Cells[0].Value.ToString() || product.CountryOfOrigin != selectedRow.Cells[2].Value.ToString())
                {
                    // Update product list
                    oldName = product.Name;
                    product.Name = selectedRow.Cells[0].Value.ToString();
                    product.CountryOfOrigin = selectedRow.Cells[2].Value.ToString();

                    // Update all instances in DataGridViews
                    string productColumn = SalesColumns.Product.ToString();
                    foreach (DataGridViewRow row in allRows)
                    {
                        if (row.Cells[productColumn].Value.ToString() == oldName)
                        {
                            row.Cells[productColumn].Value = product.Name;
                        }
                    }
                }

                // If product was moved to a different category
                if (category.Name != selectedRow.Cells[1].Value.ToString())
                {
                    // Remove product from old category
                    category.ProductList.Remove(product);

                    // Get new category
                    Category newCategory;
                    if (MainMenu_Form.Instance.Selected == Options.CategorySales || MainMenu_Form.Instance.Selected == Options.ProductSales)
                    {
                        newCategory = MainMenu_Form.Instance.productCategorySaleList.FirstOrDefault(c => c.Name == selectedRow.Cells[1].Value.ToString());
                    }
                    else
                    {
                        newCategory = MainMenu_Form.Instance.productCategoryPurchaseList.FirstOrDefault(c => c.Name == selectedRow.Cells[1].Value.ToString());
                    }

                    // Add product to new category
                    newCategory.ProductList.Add(product);

                    // Update all instances in DataGridViews
                    string categoryColumn = SalesColumns.Category.ToString();
                    string nameColumn = SalesColumns.Product.ToString();
                    foreach (DataGridViewRow row in allRows)
                    {
                        if (row.Cells[categoryColumn].Value.ToString() == category.Name
                            && row.Cells[nameColumn].Value.ToString() == product.Name)
                        {
                            row.Cells[categoryColumn].Value = newCategory.Name;
                        }
                    }
                }
            }
            MainMenu_Form.Instance.SaveDataGridViewToFile(MainMenu_Form.Instance.Sales_DataGridView, Options.Sales);
            MainMenu_Form.Instance.SaveDataGridViewToFile(MainMenu_Form.Instance.Purchases_DataGridView, Options.Purchases);
        }


        // Construct controls
        private readonly byte textBoxWidth = 100, comboBoxWidth = 180;
        Panel SecondPanel;
        private void ConstructPanel()
        {
            SecondPanel = new()
            {
                Size = new Size(558, 95),
                Location = new Point(Panel.Left, Panel.Bottom),
            };
            Controls.Add(SecondPanel);
        }
        private static Label ConstructLabel(string text, int left, Panel control)
        {
            Label label = new()
            {
                ForeColor = CustomColors.text,
                Cursor = Cursors.Arrow,
                Location = new Point(left, 20),
                Text = text,
                Font = new Font("Segoe UI", 12),
                AutoSize = true
            };
            control.Controls.Add(label);

            return label;
        }
        // Define an enumeration for key press validation types
        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbers,
            None
        }
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, KeyPressValidation keyPressValidation, bool pressSaveButton, Panel control)
        {
            Guna2TextBox gTextBox = new()
            {
                Location = new Point(left, 45),
                Size = new Size(textBoxWidth, 30),
                Name = name,
                Text = text,
                ForeColor = CustomColors.text,
                BackColor = CustomColors.controlBack,
                Font = new Font("Segoe UI", 10),
                MaxLength = maxLength,
                FillColor = CustomColors.controlBack,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Cursor = Cursors.Hand,
                ShortcutsEnabled = false
            };
            gTextBox.FocusedState.FillColor = CustomColors.controlBack;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case KeyPressValidation.OnlyNumbers:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case KeyPressValidation.None:
                    break;
            }

            gTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Remove Windows "ding" noise when user presses enter
                    e.SuppressKeyPress = true;
                    if (pressSaveButton)
                    {
                        Save_Button.PerformClick();
                    }
                    else
                    {
                        // Press tab
                        Control nextControl = GetNextControl(this, true);
                        SendKeys.Send("{TAB}");
                    }
                }
            };
            gTextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            gTextBox.TextChanged += InputChanged;
            control.Controls.Add(gTextBox);

            return gTextBox;
        }
        private Guna2ComboBox ConstructGunaComboBox(int left, string name, string[] items, string text, Panel control)
        {
            Guna2ComboBox gComboBox = new()
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(left, 45),
                Size = new Size(comboBoxWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name
            };
            gComboBox.HoverState.BorderColor = CustomColors.accent_blue;
            gComboBox.FocusedState.BorderColor = CustomColors.accent_blue;
            gComboBox.Items.AddRange(items);
            gComboBox.Text = text;
            gComboBox.SelectedIndexChanged += InputChanged;
            control.Controls.Add(gComboBox);

            return gComboBox;
        }
        private Guna2DateTimePicker ConstructDatePicker(int left, string name, DateTime value, Panel control)
        {
            Guna2DateTimePicker gDatePicker = new()
            {
                Location = new Point(left, 45),
                Size = new Size(comboBoxWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name,
                Value = value
            };
            gDatePicker.HoverState.BorderColor = CustomColors.accent_blue;
            control.Controls.Add(gDatePicker);

            return gDatePicker;
        }
    }
}