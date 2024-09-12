using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : Form
    {
        // Properties
        private readonly string selectedTag = "";
        private readonly DataGridViewRow selectedRow;
        private string receiptFilePath;

        // Init
        public ModifyRow_Form(DataGridViewRow row)
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            selectedRow = row;
            selectedTag = row.DataGridView.Tag.ToString();
            receiptFilePath = MainMenu_Form.GetFilePathFromRowTag(row.Tag);

            if (receiptFilePath != "" && !File.Exists(receiptFilePath))
            {
                CustomMessageBox.Show("Argo Sales Tracker", "The receipt no longer exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                Log.Error_FileDoesNotExist(receiptFilePath);
            }

            ConstructControls();
            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private Control controlToFocus;
        private void ModifyRow_Form_Shown(object sender, EventArgs e)
        {
            controlToFocus?.Focus();
            CenterControls();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            SaveInRow();
            SaveInListsAndUpdateDataGridViews();

            // If the user selected a new receipt
            if (receiptFilePath != null)
            {
                if (removedReceipt)
                {
                    MainMenu_Form.RemoveReceiptFromFile(selectedRow);
                }
                if (addedReceipt)
                {
                    MainMenu_Form.SaveReceiptInFile(receiptFilePath);
                    MainMenu_Form.AddReceiptToTag(selectedRow, receiptFilePath);
                }
            }

            MainMenu_Form.Instance.DataGridViewRowChanged();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Construct controls
        private void ConstructControls()
        {
            int left = 0, secondLeft = 0;

            if (selectedTag == MainMenu_Form.DataGridViewTag.Accountant.ToString())
            {
                ConstructControlsForAccountant();
                left = controlWidth + 10;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Category.ToString())
            {
                ConstructControlsForCategory();
                left = controlWidth;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Company.ToString())
            {
                ConstructControlsForCompany();
                left = controlWidth;
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.Product.ToString())
            {
                left = ConstructControlsForProduct();
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                (left, secondLeft) = ConstructControlsForSaleOrPurchase();
            }
            else if (selectedTag == MainMenu_Form.DataGridViewTag.ItemsInPurchase.ToString())
            {
                left = ConstructControlsForItemsInPurchase();
            }

            SizeControls(left, secondLeft);
        }
        private void SizeControls(int left, int secondLeft)
        {
            Width = left + 80;
            MinimumSize = new Size(500, 0);
            Panel.Width = left;

            if (secondRow)
            {
                if (secondLeft > left)
                {
                    Width = secondLeft + 80;
                }

                Height += 100;
                SecondPanel.Width = secondLeft;
            }
        }
        private void CenterControls()
        {
            Panel.Left = (ClientSize.Width - Panel.Width) / 2;
            if (secondRow)
            {
                SecondPanel.Left = (ClientSize.Width - SecondPanel.Width) / 2;
            }

            if (selectedRow.Tag != null && SelectedReceipt_Label != null)
            {
                containsReceipt = true;
                ShowReceiptLabel(Path.GetFileName(receiptFilePath));
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
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, UI.KeyPressValidation.OnlyLetters, true, false, Panel);
                        controlToFocus.TextChanged += Accountant_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Accountant_TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }
            bool containsAccountant = MainMenu_Form.Instance.accountantList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsAccountant && isOldValueDifferent)
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
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, UI.KeyPressValidation.None, true, false, Panel);
                        controlToFocus.TextChanged += Category_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Category_TextBox_TextChanged(object sender, EventArgs e)
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

            bool containsCategory = categoriesList.Any(category => category.Name.Equals(textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsCategory && isOldValueDifferent)
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
                        controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, UI.KeyPressValidation.None, true, false, Panel);
                        controlToFocus.TextChanged += Company_TextBox_TextChanged;
                        ConstructWarningLabel();
                        break;
                }
            }
        }
        private void Company_TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            bool containsCompany = MainMenu_Form.Instance.companyList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsCompany && isOldValueDifferent)
            {
                DisableSaveButton();
                UI.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Company already exists");
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
                int searchBoxMaxHeight = 100;

                switch (columnName)
                {
                    case nameof(Products_Form.Column.ProductID):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.ProductID], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        break;

                    case nameof(Products_Form.Column.ProductName):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.ProductName], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        break;

                    case nameof(Products_Form.Column.ProductCategory):
                        List<string> array;
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
                        {
                            array = MainMenu_Form.Instance.GetProductCategorySaleNames();
                        }
                        else { array = MainMenu_Form.Instance.GetProductCategoryPurchaseNames(); }

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.ProductCategory], left, Panel);
                        Guna2TextBox ProductCategory_TextBox = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);

                        ProductCategory_TextBox.Click += (sender, e) =>
                        {
                            List<SearchResult> searchResults = SearchBox.ConvertToSearchResults(array);
                            SearchBox.ShowSearchBox(this, ProductCategory_TextBox, searchResults, this, searchBoxMaxHeight);
                        };
                        ProductCategory_TextBox.TextChanged += (sender, e) =>
                        {
                            List<SearchResult> searchResults = SearchBox.ConvertToSearchResults(array);
                            SearchBox.SearchTextBoxChanged(this, ProductCategory_TextBox, searchResults, this, searchBoxMaxHeight);
                        };
                        ProductCategory_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        ProductCategory_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductCategory_TextBox, this, ModifyRow_Label, e); };

                        break;

                    case nameof(Products_Form.Column.CountryOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.CountryOfOrigin], left, Panel);
                        Guna2TextBox gTextBox = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        gTextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, gTextBox, Country.countries, this, searchBoxMaxHeight); };
                        gTextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, gTextBox, Country.countries, this, searchBoxMaxHeight); };
                        gTextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        gTextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(gTextBox, this, ModifyRow_Label, e); };
                        break;

                    case nameof(Products_Form.Column.CompanyOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.CompanyOfOrigin], left, Panel);
                        gTextBox = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        gTextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, gTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, searchBoxMaxHeight); };
                        gTextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, gTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, searchBoxMaxHeight); };
                        gTextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        gTextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(gTextBox, this, ModifyRow_Label, e); };
                        break;
                }
                left += controlWidth + UI.spaceBetweenControls;
            }
            return left - UI.spaceBetweenControls;
        }
        private Label SelectedReceipt_Label;
        private Guna2ImageButton RemoveReceipt_ImageButton;
        private Guna2Button Receipt_Button;
        private bool containsReceipt, removedReceipt, addedReceipt;
        private (int, int) ConstructControlsForSaleOrPurchase()
        {
            ConstructPanel();
            int left = 0, secondLeft = 0;
            byte searchBoxMaxHeight = 200;

            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);
                string text;

                switch (columnName)
                {
                    case nameof(MainMenu_Form.Column.OrderNumber):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.OrderNumber];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.OrderNumber]; }

                        ConstructLabel(text, left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        left += controlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Name):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Name];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Name]; }

                        ConstructLabel(text, left, Panel);
                        Guna2TextBox BuyerName_TextBox = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        BuyerName_TextBox.Click += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
                        BuyerName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
                        BuyerName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), this, searchBoxMaxHeight); };
                        BuyerName_TextBox.TextChanged += ValidateInputs;
                        BuyerName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        BuyerName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(BuyerName_TextBox, this, ModifyRow_Label, e); };
                        left += controlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Product):
                        if (cellValue != MainMenu_Form.multipleItems_text)
                        {
                            ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Product], left, Panel);
                            Guna2TextBox ProductName_TextBox = ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                            ProductName_TextBox.Click += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
                            ProductName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
                            ProductName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), this, searchBoxMaxHeight); };
                            ProductName_TextBox.TextChanged += ValidateInputs;
                            ProductName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                            ProductName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductName_TextBox, this, ModifyRow_Label, e); };
                            left += controlWidth + UI.spaceBetweenControls;
                        }

                        // Button
                        byte buttonWidth = 200;
                        Receipt_Button = new()
                        {
                            Location = new Point(left, 43 + UI.spaceBetweenControls),
                            Text = "Change receipt",
                            BackColor = CustomColors.controlBack,
                            FillColor = CustomColors.controlBack,
                            Size = new Size(buttonWidth, controlHeight),
                            BorderRadius = 2,
                            BorderThickness = 1,
                            Font = new Font("Segoe UI", 10),
                        };
                        Receipt_Button.Click += (sender, e) =>
                        {
                            // Select file
                            OpenFileDialog dialog = new();
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                if (Controls.Contains(SelectedReceipt_Label))
                                {
                                    removedReceipt = true;
                                }
                                ShowReceiptLabel(dialog.SafeFileName);
                                receiptFilePath = dialog.FileName;
                                addedReceipt = true;
                            }
                        };
                        Panel.Controls.Add(Receipt_Button);

                        left += buttonWidth + UI.spaceBetweenControls;

                        // ImageButton
                        RemoveReceipt_ImageButton = new()
                        {
                            Size = new Size(38, 38),
                            ImageSize = new Size(30, 30),
                            Image = Properties.Resources.CloseGray
                        };
                        RemoveReceipt_ImageButton.HoverState.ImageSize = new Size(30, 30);
                        RemoveReceipt_ImageButton.PressedState.ImageSize = new Size(30, 30);
                        RemoveReceipt_ImageButton.Click += (sender, e) =>
                        {
                            CloseAllPanels(null, null);
                            RemoveReceiptLabel();
                            removedReceipt = true;
                        };
                        RemoveReceipt_ImageButton.MouseEnter += (sender, e) =>
                        {
                            RemoveReceipt_ImageButton.BackColor = CustomColors.fileHover;
                        };
                        RemoveReceipt_ImageButton.MouseLeave += (sender, e) =>
                        {
                            RemoveReceipt_ImageButton.BackColor = CustomColors.mainBackground;
                        };

                        // Label
                        SelectedReceipt_Label = new()
                        {
                            ForeColor = CustomColors.text,
                            Font = new Font("Segoe UI", 10),
                            AutoSize = true
                        };
                        SelectedReceipt_Label.Click += CloseAllPanels;
                        break;

                    case nameof(MainMenu_Form.Column.Date):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Date], left, Panel);
                        ConstructDatePicker(left, columnName, DateTime.Parse(cellValue), Panel);
                        left += controlWidth;
                        break;

                    case nameof(MainMenu_Form.Column.Quantity):
                        secondRow = true;
                        if (cellValue == MainMenu_Form.emptyCell) { continue; }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Quantity], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbers, false, true, SecondPanel);
                        secondLeft += smallControlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        if (cellValue == MainMenu_Form.emptyCell) { continue; }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Shipping):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Shipping], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbersAndDecimal, true, true, SecondPanel);
                        secondLeft += smallControlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Tax):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Tax], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Fee):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Fee], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        ConstructLabel("Charged amount", secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, UI.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth;
                        break;
                }
            }
            return (left, secondLeft);
        }
        private int ConstructControlsForItemsInPurchase()
        {
            int left = 0;
            byte searchBoxMaxHeight = 200;

            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(MainMenu_Form.Column.Product):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Product], 0, Panel);
                        Guna2TextBox ProductName_TextBox = ConstructTextBox(0, columnName, cellValue, 50, UI.KeyPressValidation.None, false, false, Panel);
                        ProductName_TextBox.Click += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
                        ProductName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
                        ProductName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), this, searchBoxMaxHeight); };
                        ProductName_TextBox.TextChanged += ValidateInputs;
                        ProductName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
                        ProductName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductName_TextBox, this, ModifyRow_Label, e); };
                        left += controlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Quantity):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Quantity], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.OnlyNumbers, true, false, Panel);
                        left += controlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.OnlyNumbersAndDecimal, true, false, Panel);
                        left += controlWidth + UI.spaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Total], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, UI.KeyPressValidation.OnlyNumbersAndDecimal, true, false, Panel);
                        left += controlWidth;
                        break;
                }
            }
            return left;
        }

        // Methods for receipts
        private void ShowReceiptLabel(string text)
        {
            SelectedReceipt_Label.Text = text;

            Controls.Add(SelectedReceipt_Label);
            Controls.Add(RemoveReceipt_ImageButton);
            SetReceiptLabelLocation();
            SelectedReceipt_Label.BringToFront();
            RemoveReceipt_ImageButton.BringToFront();

            ValidateInputs(null, null);
        }
        private void SetReceiptLabelLocation()
        {
            RemoveReceipt_ImageButton.Location = new Point(
                Receipt_Button.Parent.Left + Receipt_Button.Right - RemoveReceipt_ImageButton.Width,
                Receipt_Button.Parent.Top + Receipt_Button.Bottom + UI.spaceBetweenControls);

            SelectedReceipt_Label.Location = new Point(
                RemoveReceipt_ImageButton.Left - SelectedReceipt_Label.Width,
                RemoveReceipt_ImageButton.Top + (RemoveReceipt_ImageButton.Height - SelectedReceipt_Label.Height) / 2 - 1);
        }
        private void RemoveReceiptLabel()
        {
            Controls.Remove(SelectedReceipt_Label);
            Controls.Remove(RemoveReceipt_ImageButton);
            ValidateInputs(null, null);
        }

        // Warning label
        private PictureBox WarningAccountantName_PictureBox;
        private Label WarningAccountantName_Label;
        private void ConstructWarningLabel()
        {
            WarningAccountantName_PictureBox = new()
            {
                Size = new Size(19, 19),
                Image = Properties.Resources.ExclamationMark,
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

        // Methods
        private void ShowSearchBox(Guna2TextBox gTextBox, List<SearchResult> results, int maxHeight)
        {
            SearchBox.ShowSearchBox(this, gTextBox, results, this, maxHeight);
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            IEnumerable<Control> allControls = Panel.Controls.Cast<Control>();

            if (SecondPanel != null)
            {
                allControls = allControls.Concat(SecondPanel.Controls.Cast<Control>());
            }

            foreach (Control control in allControls)
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
            if (!Controls.Contains(SelectedReceipt_Label) && containsReceipt && Properties.Settings.Default.PurchaseReceipts)
            {
                Save_Button.Enabled = false;
                return;
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

                    if (columnName == MainMenu_Form.Column.PricePerUnit.ToString() ||
                        columnName == MainMenu_Form.Column.Shipping.ToString() ||
                        columnName == MainMenu_Form.Column.Tax.ToString() ||
                        columnName == MainMenu_Form.Column.Fee.ToString() ||
                        columnName == MainMenu_Form.Column.Total.ToString())
                    {
                        // Format the number with two decimal places
                        if (decimal.TryParse(gTextBox.Text, out decimal number))
                        {
                            selectedRow.Cells[columnName].Value = string.Format("{0:N2}", number);
                        }
                    }
                    else
                    {
                        selectedRow.Cells[columnName].Value = gTextBox.Text;
                    }
                }
                else if (control is Guna2ComboBox gComboBox)
                {
                    string columnName = gComboBox.Name;
                    selectedRow.Cells[columnName].Value = gComboBox.SelectedItem;
                }
                else if (control is Guna2DateTimePicker gDatePicker)
                {
                    string columnName = gDatePicker.Name;
                    selectedRow.Cells[columnName].Value = Tools.FormatDate(gDatePicker.Value);
                }
            }

            // Update charged difference
            if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                int quantity = int.Parse(selectedRow.Cells[MainMenu_Form.Column.Quantity.ToString()].Value.ToString());
                decimal pricePerUnit = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value.ToString());
                decimal shipping = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Shipping.ToString()].Value.ToString());
                decimal tax = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Tax.ToString()].Value.ToString());
                decimal totalPrice = quantity * pricePerUnit + shipping + tax;
                selectedRow.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = Convert.ToDecimal(selectedRow.Cells[MainMenu_Form.Column.Total.ToString()].Value) - totalPrice;
            }

            MainMenu_Form.Instance.DataGridViewRowChanged();
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
                if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
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
                string categoryColumn = MainMenu_Form.Column.Category.ToString();
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
                if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
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
                    string productColumn = MainMenu_Form.Column.Product.ToString();
                    foreach (DataGridViewRow row in GetRows())
                    {
                        if (row.Cells[productColumn].Value.ToString() == oldName)
                        {
                            row.Cells[productColumn].Value = product.Name;

                            string idColumn = MainMenu_Form.Column.OrderNumber.ToString();
                            if (row.Cells[idColumn].Value.ToString() == oldID)
                            {
                                row.Cells[idColumn].Value = product.ProductID;
                            }

                            string countryColumn = MainMenu_Form.Column.Country.ToString();
                            if (row.Cells[countryColumn].Value.ToString() == oldCountry)
                            {
                                row.Cells[countryColumn].Value = product.CountryOfOrigin;
                            }

                            string companyColumn = MainMenu_Form.Column.Company.ToString();
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
                    if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
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
                    string categoryColumn = MainMenu_Form.Column.Category.ToString();
                    string nameColumn = MainMenu_Form.Column.Product.ToString();
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

                UpdateDataGridViewRows(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.Column.Company.ToString(), accountant, newValue);
                UpdateDataGridViewRows(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.Column.Company.ToString(), accountant, newValue);
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

                UpdateDataGridViewRows(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.Column.Name.ToString(), accountant, newValue);
                UpdateDataGridViewRows(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.Column.Name.ToString(), accountant, newValue);
            }

            if (MainMenu_Form.Instance.IsPurchasesSelected())
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategoryPurchases);
            }
            else
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategorySales);
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
        private readonly byte controlWidth = 250, smallControlWidth = 180, controlHeight = 45;
        private Panel SecondPanel;
        private void ConstructPanel()
        {
            SecondPanel = new()
            {
                Size = Panel.Size,
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
                Font = new Font("Segoe UI", 11),
                AutoSize = true
            };
            label.Click += CloseAllPanels;
            control.Controls.Add(label);

            return label;
        }
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, UI.KeyPressValidation keyPressValidation, bool pressSaveButton, bool smallWidth, Panel control)
        {
            Guna2TextBox textBox = new()
            {
                Location = new Point(left, 43 + UI.spaceBetweenControls),
                Height = controlHeight,
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
            textBox.FocusedState.FillColor = CustomColors.controlBack;
            textBox.HoverState.BorderColor = CustomColors.accent_blue;
            textBox.FocusedState.BorderColor = CustomColors.accent_blue;

            if (smallWidth)
            {
                textBox.Width = smallControlWidth;
            }
            else { textBox.Width = controlWidth; }

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case UI.KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case UI.KeyPressValidation.OnlyNumbersAndDecimal:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
                    break;
                case UI.KeyPressValidation.OnlyNumbers:
                    textBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case UI.KeyPressValidation.OnlyLetters:
                    textBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
                    break;
                case UI.KeyPressValidation.None:
                    break;
            }

            textBox.KeyDown += (sender, e) =>
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
            textBox.Click += CloseAllPanels;
            textBox.TextChanged += ValidateInputs;
            textBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            textBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            textBox.KeyDown += UI.TextBox_KeyDown;
            control.Controls.Add(textBox);

            return textBox;
        }
        private Guna2DateTimePicker ConstructDatePicker(int left, string name, DateTime value, Panel control)
        {
            Guna2DateTimePicker gDatePicker = new()
            {
                Location = new Point(left, 43 + UI.spaceBetweenControls),
                Size = new Size(controlWidth, controlHeight),
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