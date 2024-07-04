using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : BaseForm
    {
        // Properties
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

            if (selectedTag == MainMenu_Form.DataGridViewTag.Accountant.ToString())
            {
                ConstructControlsForAccountant();
                left = 300;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Category.ToString())
            {
                ConstructControlsForCategory();
                left = 300;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Company.ToString())
            {
                ConstructControlsForCompany();
                left = 300;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Product.ToString())
            {
                left = ConstructControlsForProduct();
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                (left, secondLeft) = ConstructControlsForSaleOrPurchase();
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
        private void ConstructControlsForAccountant()
        {
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(Accountants_Form.Columns.AccountantName):
                        ConstructLabel(Accountants_Form.Instance.ColumnHeaders[Accountants_Form.Columns.AccountantName], 0, Panel);
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, KeyPressValidation.OnlyLetters, true, false, Panel);
                        controlToFocus.TextChanged += Accountant_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Accountant_TextBox_TextChanged(object? sender, EventArgs e)
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            if (MainMenu_Form.Instance.accountantList.Contains(textBox.Text) && listOfOldValues[0] != textBox.Text)
            {
                DisableSaveButton();
                UI.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Accountant already exists");
            }
            else
            {
                EnableSaveButton();
                UI.SetGTextBoxToValid(textBox);
                HideAccountantWarning();
            }
        }
        private void ConstructControlsForCategory()
        {
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(Categories_Form.Columns.CategoryName):
                        ConstructLabel(Categories_Form.Instance.ColumnHeaders[Categories_Form.Columns.CategoryName], 0, Panel);
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, KeyPressValidation.None, true, false, Panel);
                        controlToFocus.TextChanged += Category_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Category_TextBox_TextChanged(object? sender, EventArgs e)
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            // Get list
            List<Category> categoriesList;
            if (Categories_Form.Instance.Purchase_RadioButton.Checked)
            {
                categoriesList = MainMenu_Form.Instance.categoryPurchaseList;
            }
            else
            {
                categoriesList = MainMenu_Form.Instance.categorySaleList;
            }
            bool containsCategory = categoriesList.Any(category => category.Name.Contains(textBox.Text));

            if (containsCategory && listOfOldValues[0] != textBox.Text)
            {
                DisableSaveButton();
                UI.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Category already exists");
            }
            else
            {
                EnableSaveButton();
                UI.SetGTextBoxToValid(textBox);
                HideAccountantWarning();
            }
        }
        private void ConstructControlsForCompany()
        {
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(Companies_Form.Columns.Company):
                        ConstructLabel(Companies_Form.Instance.ColumnHeaders[Companies_Form.Columns.Company], 0, Panel);
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, KeyPressValidation.None, true, false, Panel);
                        controlToFocus.TextChanged += Company_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Company_TextBox_TextChanged(object? sender, EventArgs e)
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            if(textBox.Text== "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            if (MainMenu_Form.Instance.companyList.Contains(textBox.Text) && listOfOldValues[0] != textBox.Text)
            {
                DisableSaveButton();
                UI.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Category already exists");
            }
            else
            {
                EnableSaveButton();
                UI.SetGTextBoxToValid(textBox);
                HideAccountantWarning();
            }
        }
        private int ConstructControlsForProduct()
        {
            int left = 0;

            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);
                int maxHeight = 200;

                switch (columnName)
                {
                    case nameof(Products_Form.Columns.ProductID):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.ProductID], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        break;

                    case nameof(Products_Form.Columns.ProductName):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.ProductName], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        break;

                    case nameof(Products_Form.Columns.ProductCategory):
                        string[] array;
                        if (MainMenu_Form.Instance.Selected == SelectedOption.ProductSales)
                        {
                            array = MainMenu_Form.Instance.GetProductCategorySaleNames().ToArray();
                        }
                        else { array = MainMenu_Form.Instance.GetProductCategoryPurchaseNames().ToArray(); }

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.ProductCategory], left, Panel);
                        ConstructGunaComboBox(left, columnName, array, cellValue, Panel);
                        break;

                    case nameof(Products_Form.Columns.CountryOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.CountryOfOrigin], left, Panel);
                        Guna2TextBox gTextBox = ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        gTextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, gTextBox, Country.countries, this, maxHeight); };
                        gTextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, gTextBox, Country.countries, this, null, maxHeight); };
                        gTextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        gTextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(gTextBox, this, ModifyRow_Label, e); };
                        break;

                    case nameof(Products_Form.Columns.CompanyOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Columns.CompanyOfOrigin], left, Panel);
                        gTextBox = ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        gTextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, gTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, maxHeight); };
                        gTextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, gTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, null, maxHeight); };
                        gTextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        gTextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(gTextBox, this, ModifyRow_Label, e); };
                        break;
                }
                left += controlWidth + 10;
            }
            return left;
        }
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
                    case nameof(Column.ID):
                        if (MainMenu_Form.Instance.Selected == SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[Column.ID];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[Column.ID]; }

                        ConstructLabel(text, left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, false, Panel);
                        left += controlWidth + 10;
                        break;

                    case nameof(Column.Name):
                        if (MainMenu_Form.Instance.Selected == SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[Column.Name];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Name]; }

                        ConstructLabel(text, left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        left += controlWidth + 10;
                        break;

                    case nameof(Column.Product):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Product], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, false, false, Panel);
                        left += controlWidth + 10;
                        break;

                    case nameof(Column.Date):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Date], left, Panel);
                        ConstructDatePicker(left, columnName, DateTime.Parse(cellValue), Panel);
                        left += controlWidth + 10;
                        break;

                    case nameof(Column.Quantity):
                        secondRow = true;

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Quantity], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbers, false, true, SecondPanel);
                        secondLeft += smallControlWidth + 10;
                        break;

                    case nameof(Column.PricePerUnit):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.PricePerUnit], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, true, SecondPanel);
                        secondLeft += smallControlWidth + 10;
                        break;

                    case nameof(Column.Shipping):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Shipping], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, true, true, SecondPanel);
                        secondLeft += smallControlWidth + 10;
                        break;

                    case nameof(Column.Tax):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[Column.Tax], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, false, true, SecondPanel);
                        secondLeft += smallControlWidth + 10;
                        break;
                }
            }
            return (left, secondLeft);
        }


        // Warning label
        private PictureBox WarningAccountantName_PictureBox;
        private Label WarningAccountantName_Label;
        private void ConstructWarningLabel()
        {
            WarningAccountantName_PictureBox = new()
            {
                Size = new Size(19, 19),
                Image = Properties.Resources.Warning,
                SizeMode = PictureBoxSizeMode.StretchImage,
            };

            WarningAccountantName_Label = new()
            {
                ForeColor = CustomColors.text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
        }
        private void ShowWarning(Guna2TextBox textBox, string text)
        {
            WarningAccountantName_PictureBox.Top = textBox.Top + textBox.Height + 6;
            WarningAccountantName_PictureBox.Left = textBox.Left;
            WarningAccountantName_Label.Top = WarningAccountantName_PictureBox.Top;
            WarningAccountantName_Label.Left = WarningAccountantName_PictureBox.Right + 8;
            WarningAccountantName_Label.Text = text;
            Panel.Controls.Add(WarningAccountantName_PictureBox);
            Panel.Controls.Add(WarningAccountantName_Label);
        }
        private void HideAccountantWarning()
        {
            Panel.Controls.Remove(WarningAccountantName_PictureBox);
            Panel.Controls.Remove(WarningAccountantName_Label);
        }

        // Form event handlers
        private Control controlToFocus;
        private void ModifyRow_Form_Shown(object sender, EventArgs e)
        {
            controlToFocus?.Focus();
        }


        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            SaveInRow();
            SaveInListsAndUpdateDataGridViews();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Close();
        }


        // Methods
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

            // Update total value
            if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                int quantity = int.Parse(selectedRow.Cells[Column.Quantity.ToString()].Value.ToString());
                decimal pricePerUnit = decimal.Parse(selectedRow.Cells[Column.PricePerUnit.ToString()].Value.ToString());
                decimal shipping = decimal.Parse(selectedRow.Cells[Column.Shipping.ToString()].Value.ToString());
                decimal tax = decimal.Parse(selectedRow.Cells[Column.Tax.ToString()].Value.ToString());
                decimal totalPrice = quantity * pricePerUnit + shipping + tax;
                selectedRow.Cells[Column.Total.ToString()].Value = totalPrice;
            }
            Close();
        }
        private void SaveInListsAndUpdateDataGridViews()
        {
            if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString()) { return; }

            string oldName = "", oldID = "", oldCountry = "", oldCompany = "";

            if (selectedTag == MainMenu_Form.DataGridViewTag.Category.ToString())
            {
                // Get category
                Category category;
                if (MainMenu_Form.Instance.Selected == SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == SelectedOption.ProductSales)
                {
                    category = MainMenu_Form.Instance.categorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
                }
                else
                {
                    category = MainMenu_Form.Instance.categoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
                }

                // Update list
                oldName = category.Name;
                category.Name = selectedRow.Cells[0].Value.ToString();

                // Update all instances in DataGridViews
                string categoryColumn = Column.Category.ToString();
                foreach (DataGridViewRow row in GetRows())
                {
                    if (row.Cells[categoryColumn].Value.ToString() == oldName)
                    {
                        row.Cells[categoryColumn].Value = category.Name;
                    }
                }
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Product.ToString())
            {
                // Get category
                Category category;
                if (MainMenu_Form.Instance.Selected == SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == SelectedOption.ProductSales)
                {
                    category = MainMenu_Form.Instance.categorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[2]);
                }
                else
                {
                    category = MainMenu_Form.Instance.categoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[2]);
                }

                // Get product
                Product product = category.ProductList.FirstOrDefault(p => p.Name == listOfOldValues[1]);

                // If product changed
                if (product.ProductID != selectedRow.Cells[0].Value.ToString() ||
                    product.Name != selectedRow.Cells[1].Value.ToString() ||
                    product.CountryOfOrigin != selectedRow.Cells[3].Value.ToString() ||
                    product.CompanyOfOrigin != selectedRow.Cells[4].Value.ToString())
                {
                    // Update product list
                    oldName = product.Name;
                    oldID = product.ProductID;
                    oldCountry = product.CountryOfOrigin;
                    oldCompany = product.CompanyOfOrigin;
                    product.ProductID = selectedRow.Cells[0].Value.ToString();
                    product.Name = selectedRow.Cells[1].Value.ToString();
                    product.CountryOfOrigin = selectedRow.Cells[3].Value.ToString();
                    product.CompanyOfOrigin = selectedRow.Cells[4].Value.ToString();

                    // Update all instances in DataGridViews
                    string productColumn = Column.Product.ToString();
                    foreach (DataGridViewRow row in GetRows())
                    {
                        if (row.Cells[productColumn].Value.ToString() == oldName)
                        {
                            row.Cells[productColumn].Value = product.Name;

                            string idColumn = Column.ID.ToString();
                            if (row.Cells[idColumn].Value.ToString() == oldID)
                            {
                                row.Cells[idColumn].Value = product.ProductID;
                            }

                            string countryColumn = Column.Country.ToString();
                            if (row.Cells[countryColumn].Value.ToString() == oldCountry)
                            {
                                row.Cells[countryColumn].Value = product.CountryOfOrigin;
                            }

                            string companyColumn = Column.Company.ToString();
                            if (row.Cells[companyColumn].Value.ToString() == oldCompany)
                            {
                                row.Cells[companyColumn].Value = product.CompanyOfOrigin;
                            }
                        }
                    }
                }

                // If product was moved to a different category
                if (category.Name != selectedRow.Cells[2].Value.ToString())
                {
                    // Remove product from old category
                    category.ProductList.Remove(product);

                    // Get new category
                    Category newCategory;
                    if (MainMenu_Form.Instance.Selected == SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == SelectedOption.ProductSales)
                    {
                        newCategory = MainMenu_Form.Instance.categorySaleList.FirstOrDefault(c => c.Name == selectedRow.Cells[2].Value.ToString());
                    }
                    else
                    {
                        newCategory = MainMenu_Form.Instance.categoryPurchaseList.FirstOrDefault(c => c.Name == selectedRow.Cells[2].Value.ToString());
                    }

                    // Add product to new category
                    newCategory.ProductList.Add(product);

                    // Update all instances in DataGridViews
                    string categoryColumn = Column.Category.ToString();
                    string nameColumn = Column.Product.ToString();
                    foreach (DataGridViewRow row in GetRows())
                    {
                        if (row.Cells[categoryColumn].Value.ToString() == category.Name
                            && row.Cells[nameColumn].Value.ToString() == product.Name)
                        {
                            row.Cells[categoryColumn].Value = newCategory.Name;
                        }
                    }
                }
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Company.ToString())
            {
                // Get accountant
                string accountant = MainMenu_Form.Instance.companyList.FirstOrDefault(a => a == listOfOldValues[0]);

                // Update list
                if (accountant != null)
                {
                    int index = MainMenu_Form.Instance.companyList.IndexOf(accountant);
                    MainMenu_Form.Instance.companyList[index] = selectedRow.Cells[0].Value.ToString();
                }

                // Update all instances in DataGridViews
                string newValue = selectedRow.Cells[0].Value.ToString();

                UpdateDataGridViewRows(MainMenu_Form.Instance.Purchases_DataGridView, Column.Company.ToString(), accountant, newValue);
                UpdateDataGridViewRows(MainMenu_Form.Instance.Sales_DataGridView, Column.Company.ToString(), accountant, newValue);
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Accountant.ToString())
            {
                // Get accountant
                string accountant = MainMenu_Form.Instance.accountantList.FirstOrDefault(a => a == listOfOldValues[0]);

                // Update list
                if (accountant != null)
                {
                    int index = MainMenu_Form.Instance.accountantList.IndexOf(accountant);
                    MainMenu_Form.Instance.accountantList[index] = selectedRow.Cells[0].Value.ToString();
                }

                // Update all instances in DataGridViews
                string newValue = selectedRow.Cells[0].Value.ToString();

                UpdateDataGridViewRows(MainMenu_Form.Instance.Purchases_DataGridView, Column.Name.ToString(), accountant, newValue);
                UpdateDataGridViewRows(MainMenu_Form.Instance.Sales_DataGridView, Column.Name.ToString(), accountant, newValue);
            }

            if (MainMenu_Form.Instance.IsPurchasesSelected())
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(SelectedOption.Purchases);
            }
            else
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(SelectedOption.Sales);
            }
        }
        private static DataGridViewRowCollection GetRows()
        {
            if (MainMenu_Form.Instance.IsPurchasesSelected())
            {
                return MainMenu_Form.Instance.Purchases_DataGridView.Rows;
            }
            else
            {
                return MainMenu_Form.Instance.Sales_DataGridView.Rows;
            }
        }
        private static void UpdateDataGridViewRows(DataGridView dataGridView, string columnName, string oldValue, string newValue)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[columnName].Value.ToString() == oldValue)
                {
                    row.Cells[columnName].Value = newValue;
                }
            }
        }

        // Construct controls
        private readonly byte controlWidth = 180, smallControlWidth = 120;
        Panel SecondPanel;
        private void ConstructPanel()
        {
            SecondPanel = new()
            {
                Size = new Size(558, 95),
                Location = new Point(Panel.Left, Panel.Bottom),
            };
            SecondPanel.Click += CloseAllPanels;
            Controls.Add(SecondPanel);
        }
        private Label ConstructLabel(string text, int left, Panel control)
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
            label.Click += CloseAllPanels;
            control.Controls.Add(label);

            return label;
        }
        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbers,
            OnlyLetters,
            None
        }
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, KeyPressValidation keyPressValidation, bool pressSaveButton, bool smallWidth, Panel control)
        {
            Guna2TextBox gTextBox = new()
            {
                Location = new Point(left, 45),
                Height = 30,
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
            gTextBox.Click += CloseAllPanels;
            gTextBox.FocusedState.FillColor = CustomColors.controlBack;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;

            if (smallWidth)
            {
                gTextBox.Width = smallControlWidth;
            }
            else { gTextBox.Width = controlWidth; }

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case KeyPressValidation.OnlyNumbers:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case KeyPressValidation.OnlyLetters:
                    gTextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
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
                        if (Save_Button.Tag is bool tag && tag == true)
                        {
                            Save_Button.PerformClick();
                        }
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
                Size = new Size(controlWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name
            };
            gComboBox.Click += CloseAllPanels;
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
                Size = new Size(controlWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name,
                Value = value
            };
            gDatePicker.Click += CloseAllPanels;
            gDatePicker.HoverState.BorderColor = CustomColors.accent_blue;
            control.Controls.Add(gDatePicker);

            return gDatePicker;
        }


        // Misc.
        private void DisableSaveButton()
        {
            Save_Button.Enabled = false;
            Save_Button.Tag = false;
        }
        private void EnableSaveButton()
        {
            Save_Button.Enabled = true;
            Save_Button.Tag = true;
        }
        private void SetControlForTextBoxEmpty(Guna2TextBox textBox)
        {
            UI.SetGTextBoxToValid(textBox);
            HideAccountantWarning();
            DisableSaveButton();
        }
        public void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}