using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.GridView;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : BaseForm
    {
        // Properties
        private readonly string _selectedTag = "";
        private readonly DataGridViewRow _selectedRow;
        private string _receiptFilePath;

        // Form Width/Height properties
        private static int ScaledLargeWidth => (int)(300 * DpiHelper.GetRelativeDpiScale()); 
        private static int ScaledStandardWidth => (int)(150 * DpiHelper.GetRelativeDpiScale()); 
        private static int ScaledDatePickerWidth => (int)(300 * DpiHelper.GetRelativeDpiScale()); 
        private static int ScaledControlHeight => (int)(50 * DpiHelper.GetRelativeDpiScale()); 

        // Init.
        public ModifyRow_Form() : this(null) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public ModifyRow_Form(DataGridViewRow row)
        {
            InitializeComponent();
            if (row == null) { return; }

            _selectedRow = row;
            _selectedTag = row.DataGridView.Tag.ToString();
            _receiptFilePath = RightClickRowMenu.GetFilePathFromRowTag(row.Tag);

            if (_receiptFilePath != "" && !File.Exists(_receiptFilePath))
            {
                CustomMessageBox.Show("Receipt does not exist",
                    "The receipt no longer exists.",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);

                Log.Error_FileDoesNotExist(_receiptFilePath);
            }

            ConstructControls();
            ValidateInputs(null, null);  // This is needed in case the cell contains "-", which is possible if it was imported from a spreadsheet
            AttachChangeHandlers();
            SetAccessibleDescriptions();
            UpdateTheme();
            
            if (_removeReceipt_ImageButton != null)
            {
                DpiHelper.ScaleImageButton(_removeReceipt_ImageButton);
            }
            
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void SetAccessibleDescriptions()
        {
            ModifyRow_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Save_Button);
            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
        }

        // Form event handlers
        private Control _controlToFocus;
        private void ModifyRow_Form_Shown(object sender, EventArgs e)
        {
            _controlToFocus?.Focus();
            CenterControls();
            ModifyRow_Label.Focus();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ModifyRow_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            CustomControls.CloseAllPanels();
        }

        // Methods for checking if there are changes
        private static bool _hasChanges = false;
        private void AttachChangeHandlers()
        {
            foreach (Control control in Panel.Controls)
            {
                AttachChangeHandler(control);
            }

            if (_secondPanel != null)
            {
                foreach (Control control in _secondPanel.Controls)
                {
                    AttachChangeHandler(control);
                }
            }
        }
        private void AttachChangeHandler(Control control)
        {
            if (control is Guna2TextBox textBox)
            {
                textBox.TextChanged += (s, e) => CheckForChanges((Guna2TextBox)s);
            }
            else if (control is Guna2ComboBox comboBox)
            {
                comboBox.SelectedIndexChanged += (s, e) => CheckForChanges((Guna2ComboBox)s);
            }
            else if (control is Guna2DateTimePicker datePicker)
            {
                datePicker.ValueChanged += (s, e) => CheckForChanges((Guna2DateTimePicker)s);
            }
        }
        private void CheckForChanges(Control control)
        {
            int index = GetControlIndex(control.Name);
            if (index < 0 || index >= _listOfOldValues.Count) { return; }
        }
        private int GetControlIndex(string controlName)
        {
            int index = 0;
            foreach (DataGridViewColumn column in _selectedRow.DataGridView.Columns)
            {
                if (column.Name == controlName)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {
            CloseSearchBox(null, null);
            SaveInListsAndUpdateMainMenuForm();
            SaveInMainMenuRow();
            UpdateChargedDifferenceInMainMenuRow();
            HandleReceiptChanges();

            DataGridViewManager.DataGridViewRowChanged((Guna2DataGridView)_selectedRow.DataGridView, MainMenu_Form.Instance.Selected);
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Construct controls
        private void ConstructControls()
        {
            int left = 0, secondLeft = 0;

            if (_selectedTag == MainMenu_Form.DataGridViewTag.Accountant.ToString())
            {
                ConstructControlsForAccountant();
                left = ScaledStandardWidth; 
            }
            else if (_selectedTag == MainMenu_Form.DataGridViewTag.Category.ToString())
            {
                ConstructControlsForCategory();
                left = ScaledStandardWidth; 
            }
            else if (_selectedTag == MainMenu_Form.DataGridViewTag.Company.ToString())
            {
                ConstructControlsForCompany();
                left = ScaledStandardWidth; 
            }
            else if (_selectedTag == MainMenu_Form.DataGridViewTag.Product.ToString())
            {
                left = ConstructControlsForProduct();
            }
            else if (_selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                (left, secondLeft) = ConstructControlsForSaleOrPurchase();
            }
            else if (_selectedTag == MainMenu_Form.DataGridViewTag.ItemsInPurchase.ToString())
            {
                left = ConstructControlsForItemsInTransaction();
            }

            SizeControls(left, secondLeft);
        }
        private void SizeControls(int left, int secondLeft)
        {
            float scale = DpiHelper.GetRelativeDpiScale();
            int scaledPadding = (int)(140 * scale);
            
            Width = left + scaledPadding;
            Panel.Width = left;

            if (_secondRow)
            {
                if (secondLeft > left)
                {
                    Width = secondLeft + scaledPadding;
                }

                Height += (int)(100 * scale);
                _secondPanel.Width = secondLeft;

            }

            if (_notes)
            {
                Height += (int)(180 * scale);
            }
            
            MinimumSize = new Size(Math.Max((int)(600 * scale), Width), Height); // Increased minimum width
        }
        private void CenterControls()
        {
            Panel.Left = (ClientSize.Width - Panel.Width) / 2;
            ModifyRow_Label.Left = (ClientSize.Width - ModifyRow_Label.Width) / 2;

            if (_secondRow)
            {
                _secondPanel.Left = (ClientSize.Width - _secondPanel.Width) / 2;
            }

            if (_receiptFilePath == "")
            {
                RemoveReceiptLabel();
            }
            else if (_selectedRow.Tag != null && _selectedReceipt_Label != null)
            {
                _containsReceipt = true;
                ShowReceiptLabel(Path.GetFileName(_receiptFilePath));
            }
        }

        private bool _secondRow = false, _notes = false;
        private readonly List<string> _listOfOldValues = [];
        private string _oldProductName, _oldCategoryName;
        private void ConstructControlsForAccountant()
        {
            string columnName = _selectedRow.DataGridView.Columns[0].Name;
            string cellValue = _selectedRow.Cells[0].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
            _listOfOldValues.Add(cellValue);

            ConstructLabel(Accountants_Form.ColumnHeaders[Accountants_Form.Column.AccountantName], 0, Panel);

            _controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyLetters, true, false, false, Panel, "standard");
            _controlToFocus.TextChanged += Accountant_TextBox_TextChanged;

            ConstructWarningLabel();
        }
        private void Accountant_TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }
            bool containsAccountant = MainMenu_Form.Instance.AccountantList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(_listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsAccountant && isOldValueDifferent)
            {
                DisableSaveButton();
                CustomControls.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Accountant already exists");
            }
            else
            {
                EnableSaveButton();
                CustomControls.SetGTextBoxToValid(textBox);
                HideWarning();
            }
        }
        private void ConstructControlsForCategory()
        {
            string columnName = _selectedRow.DataGridView.Columns[0].Name;
            string cellValue = _selectedRow.Cells[0].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
            _listOfOldValues.Add(cellValue);

            ConstructLabel(Categories_Form.Instance.ColumnHeaders[Categories_Form.Column.CategoryName], 0, Panel);

            _controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, true, false, false, Panel, "standard");
            _controlToFocus.TextChanged += Category_TextBox_TextChanged;

            ConstructWarningLabel();
        }
        private void Category_TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            // Get list
            List<Category> categoriesList;
            if (Categories_Form.Instance.Purchase_RadioButton.Checked)
            {
                categoriesList = MainMenu_Form.Instance.CategoryPurchaseList;
            }
            else
            {
                categoriesList = MainMenu_Form.Instance.CategorySaleList;
            }

            bool containsCategory = categoriesList.Any(category => category.Name.Equals(textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            bool isOldValueDifferent = !string.Equals(_listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsCategory && isOldValueDifferent)
            {
                DisableSaveButton();
                CustomControls.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Category already exists");
            }
            else
            {
                EnableSaveButton();
                CustomControls.SetGTextBoxToValid(textBox);
                HideWarning();
            }
        }
        private void ConstructControlsForCompany()
        {
            string columnName = _selectedRow.DataGridView.Columns[0].Name;
            string cellValue = _selectedRow.Cells[0].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
            _listOfOldValues.Add(cellValue);

            ConstructLabel(Companies_Form.ColumnHeaders[Companies_Form.Column.Company], 0, Panel);

            _controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, true, false, false, Panel, "standard");
            _controlToFocus.TextChanged += Company_TextBox_TextChanged;

            ConstructWarningLabel();
        }
        private void Company_TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            if (textBox.Text == "")
            {
                SetControlForTextBoxEmpty(textBox);
                return;
            }

            bool containsCompany = MainMenu_Form.Instance.CompanyList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(_listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

            if (containsCompany && isOldValueDifferent)
            {
                DisableSaveButton();
                CustomControls.SetGTextBoxToInvalid(textBox);
                ShowWarning(textBox, "Company already exists");
            }
            else
            {
                EnableSaveButton();
                CustomControls.SetGTextBoxToValid(textBox);
                HideWarning();
            }
        }
        private int ConstructControlsForProduct()
        {
            int left = 0;

            foreach (DataGridViewColumn column in _selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = _selectedRow.Cells[column.Index].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
                _listOfOldValues.Add(cellValue);
                int searchBoxMaxHeight = 100;

                switch (columnName)
                {
                    case nameof(Products_Form.Column.ProductID):
                        ConstructLabel(Products_Form.ColumnHeaders[Products_Form.Column.ProductID], left, Panel);
                        _controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, true, Panel, "standard");
                        break;

                    case nameof(Products_Form.Column.ProductName):
                        ConstructLabel(Products_Form.ColumnHeaders[Products_Form.Column.ProductName], left, Panel);
                        _controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, true, Panel, "standard");
                        _oldProductName = cellValue;
                        break;

                    case nameof(Products_Form.Column.ProductCategory):
                        ConstructLabel(Products_Form.ColumnHeaders[Products_Form.Column.ProductCategory], left, Panel);

                        Guna2TextBox ProductCategory_TextBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "standard");
                        SearchBox.Attach(ProductCategory_TextBox, this, GetSearchResults, searchBoxMaxHeight, false, false, false, true);
                        ProductCategory_TextBox.TextChanged += ValidateInputs;
                        _oldCategoryName = cellValue;
                        break;

                    case nameof(Products_Form.Column.CountryOfOrigin):
                        ConstructLabel(Products_Form.ColumnHeaders[Products_Form.Column.CountryOfOrigin], left, Panel);

                        Guna2TextBox textBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "standard");
                        SearchBox.Attach(textBox, this, () => Country.CountrySearchResults, searchBoxMaxHeight, false, true, false, false);
                        textBox.TextChanged += ValidateInputs;
                        break;

                    case nameof(Products_Form.Column.CompanyOfOrigin):
                        ConstructLabel(Products_Form.ColumnHeaders[Products_Form.Column.CompanyOfOrigin], left, Panel);

                        textBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "standard");
                        List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.CompanyList);
                        SearchBox.Attach(textBox, this, () => searchResult, searchBoxMaxHeight, false, false, false, true);
                        textBox.TextChanged += ValidateInputs;
                        break;
                }
                left += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
            }
            return left - CustomControls.SpaceBetweenControls;
        }
        private List<SearchResult> GetSearchResults()
        {
            List<string> categoryNames = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales
                ? MainMenu_Form.Instance.GetCategorySaleNames()
                : MainMenu_Form.Instance.GetCategoryPurchaseNames();

            return SearchBox.ConvertToSearchResults(categoryNames);
        }

        private Label _selectedReceipt_Label;
        private Guna2ImageButton _removeReceipt_ImageButton;
        private Guna2Button _receipt_Button;
        private bool _containsReceipt, _removedReceipt, _addedReceipt;
        private (int, int) ConstructControlsForSaleOrPurchase()
        {
            ConstructPanel();
            int left = 0, secondLeft = 0;
            byte searchBoxMaxHeight = 200;

            foreach (DataGridViewColumn column in _selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = _selectedRow.Cells[column.Index].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
                _listOfOldValues.Add(cellValue);
                string text;
                List<SearchResult> searchResult;

                switch (columnName)
                {
                    case nameof(MainMenu_Form.Column.ID):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.ID];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.ID]; }

                        ConstructLabel(text, left, Panel);
                        _controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "large");
                        left += ScaledLargeWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Accountant):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Accountant];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Accountant]; }

                        ConstructLabel(text, left, Panel);
                        Guna2TextBox Accountant_TextBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "standard");
                        searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.AccountantList);
                        SearchBox.Attach(Accountant_TextBox, this, () => searchResult, searchBoxMaxHeight, false, false, false, true);
                        Accountant_TextBox.TextChanged += ValidateInputs;
                        left += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Product):
                        if (cellValue != ReadOnlyVariables.MultipleItems_text)
                        {
                            ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Product], left, Panel);
                            string fullProductPath = GetFullProductPath(_selectedRow, cellValue);
                            Guna2TextBox ProductName_TextBox = ConstructTextBox(left, columnName, fullProductPath, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "large");
                            SearchBox.Attach(ProductName_TextBox, this, GetProductListForSearchBox, searchBoxMaxHeight, true, false, false, true);
                            ProductName_TextBox.TextChanged += ValidateInputs;
                            left += ScaledLargeWidth + CustomControls.SpaceBetweenControls;
                            _oldProductName = cellValue;
                        }

                        int buttonWidth = (int)(210 * DpiHelper.GetRelativeDpiScale());
                        _receipt_Button = new()
                        {
                            Location = new Point(left, 43 + CustomControls.SpaceBetweenControls),
                            Text = "Add receipt",
                            BackColor = CustomColors.ControlBack,
                            FillColor = CustomColors.ControlBack,
                            Size = new Size(buttonWidth, ScaledControlHeight),
                            BorderRadius = 2,
                            BorderThickness = 1,
                            Font = new Font("Segoe UI", 10),
                        };
                        _receipt_Button.Click += (_, _) =>
                        {
                            OpenFileDialog dialog = new();
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                if (Controls.Contains(_selectedReceipt_Label))
                                {
                                    _removedReceipt = true;
                                }
                                ShowReceiptLabel(dialog.SafeFileName);
                                _receiptFilePath = dialog.FileName;
                                _addedReceipt = true;
                            }
                        };
                        Panel.Controls.Add(_receipt_Button);
                        left += buttonWidth + CustomControls.SpaceBetweenControls;

                        // ImageButton
                        _removeReceipt_ImageButton = new()
                        {
                            Size = new Size(38, 38),
                            ImageSize = new Size(30, 30),
                            Image = Properties.Resources.CloseGray,
                            Anchor = AnchorStyles.Top,
                            HoverState = { ImageSize = new Size(30, 30) },
                            PressedState = { ImageSize = new Size(30, 30) }
                        };
                        _removeReceipt_ImageButton.Click += (_, _) =>
                        {
                            CloseSearchBox(null, null);
                            RemoveReceiptLabel();
                            _removedReceipt = true;
                        };
                        _removeReceipt_ImageButton.MouseEnter += (_, _) =>
                        {
                            _removeReceipt_ImageButton.BackColor = CustomColors.MouseHover;
                        };
                        _removeReceipt_ImageButton.MouseLeave += (_, _) =>
                        {
                            _removeReceipt_ImageButton.BackColor = CustomColors.MainBackground;
                        };

                        // Label
                        _selectedReceipt_Label = new()
                        {
                            ForeColor = CustomColors.Text,
                            Font = new Font("Segoe UI", 10),
                            AutoSize = true,
                            Anchor = AnchorStyles.Top
                        };
                        _selectedReceipt_Label.Click += CloseSearchBox;
                        break;

                    case nameof(MainMenu_Form.Column.Date):
                        _secondRow = true;
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Date], secondLeft, _secondPanel);
                        DateTime date = Tools.ParseDateOrToday(cellValue);
                        ConstructDatePicker(secondLeft, columnName, date, _secondPanel);
                        secondLeft += ScaledDatePickerWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.TotalItems):
                        _secondRow = true;
                        if (cellValue == ReadOnlyVariables.EmptyCell) { continue; }

                        string productName = _selectedRow.Cells[ReadOnlyVariables.Product_column].Value.ToString();
                        if (productName == ReadOnlyVariables.MultipleItems_text) { continue; }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.TotalItems], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbers, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        if (cellValue == ReadOnlyVariables.EmptyCell) { continue; }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Shipping):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Shipping], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Tax):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Tax], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Fee):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Fee], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Discount):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Discount], secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Total];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Total]; }

                        ConstructLabel(text, secondLeft, _secondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, true, _secondPanel, "standard");
                        secondLeft += ScaledStandardWidth;
                        break;

                    case nameof(MainMenu_Form.Column.Note):
                        Label label = ConstructLabel("Notes", secondLeft, this);

                        string note = _selectedRow.Cells[column.Index].Tag?.ToString() ?? "";

                        Guna2TextBox textBox = ConstructTextBox(0, columnName, note, 10, CustomControls.KeyPressValidation.None, false, false, true, this);

                        label.Location = new Point((ClientSize.Width - label.Width) / 2, _secondPanel.Bottom);
                        label.Anchor = AnchorStyles.Top;

                        int notesWidth = (int)(525 * DpiHelper.GetRelativeDpiScale());
                        int notesHeight = (int)(105 * DpiHelper.GetRelativeDpiScale());
                        
                        textBox.Size = new Size(notesWidth, notesHeight);
                        textBox.MinimumSize = new Size(notesWidth, notesHeight);
                        textBox.Location = new Point((ClientSize.Width - textBox.Width) / 2, label.Bottom + CustomControls.SpaceBetweenControls);
                        textBox.Anchor = AnchorStyles.Top;
                        textBox.MaxLength = 1000;
                        textBox.Multiline = true;

                        _notes = true;
                        break;
                }
            }
            return (left, secondLeft);
        }
        private int ConstructControlsForItemsInTransaction()
        {
            int left = 0;
            byte searchBoxMaxHeight = 200;

            foreach (DataGridViewColumn column in _selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = _selectedRow.Cells[column.Index].Value?.ToString() ?? ReadOnlyVariables.EmptyCell;
                _listOfOldValues.Add(cellValue);

                switch (columnName)
                {
                    case nameof(MainMenu_Form.Column.Product):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Product], 0, Panel);

                        string fullProductPath = GetFullProductPath(_selectedRow, cellValue);
                        Guna2TextBox ProductName_TextBox = ConstructTextBox(left, columnName, fullProductPath, 50, CustomControls.KeyPressValidation.None, false, false, false, Panel, "standard");
                        SearchBox.Attach(ProductName_TextBox, this, GetProductListForSearchBox, searchBoxMaxHeight, true, false, false, true);
                        ProductName_TextBox.TextChanged += ValidateInputs;

                        left += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.TotalItems):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.TotalItems], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbers, true, false, true, Panel, "standard");
                        left += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, false, true, Panel, "standard");
                        left += ScaledStandardWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Total], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, false, true, Panel, "standard");
                        left += ScaledStandardWidth;
                        break;
                }
            }
            return left;
        }
        private static List<SearchResult> GetProductListForSearchBox()
        {
            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ItemsInSale or MainMenu_Form.SelectedOption.Sales)
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames());
            }
            else
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames());
            }
        }
        private static string GetFullProductPath(DataGridViewRow row, string productName)
        {
            string company = row.Cells[ReadOnlyVariables.Company_column].Value.ToString();
            string category = row.Cells[ReadOnlyVariables.Category_column].Value.ToString();
            return $"{company} > {category} > {productName}";
        }

        // Methods for receipts
        private void ShowReceiptLabel(string text)
        {
            _selectedReceipt_Label.Text = text;
            _receipt_Button.Text = LanguageManager.TranslateString("Change receipt");

            Controls.Add(_selectedReceipt_Label);
            Controls.Add(_removeReceipt_ImageButton);
            SetReceiptLabelLocation();
            _selectedReceipt_Label.BringToFront();
            _removeReceipt_ImageButton.BringToFront();

            ValidateInputs(null, null);
        }
        private void SetReceiptLabelLocation()
        {
            _removeReceipt_ImageButton.Location = new Point(
                _receipt_Button.Parent.Left + _receipt_Button.Right - _removeReceipt_ImageButton.Width,
                _receipt_Button.Parent.Top + _receipt_Button.Bottom + CustomControls.SpaceBetweenControls);

            _selectedReceipt_Label.Location = new Point(
                _removeReceipt_ImageButton.Left - _selectedReceipt_Label.Width,
                _removeReceipt_ImageButton.Top + (_removeReceipt_ImageButton.Height - _selectedReceipt_Label.Height) / 2 - 1);
        }
        private void RemoveReceiptLabel()
        {
            if (_receipt_Button != null)
            {
                Controls.Remove(_selectedReceipt_Label);
                Controls.Remove(_removeReceipt_ImageButton);

                _receipt_Button.Text = LanguageManager.TranslateString("Add receipt");

                ValidateInputs(null, null);
            }
        }

        // Warning label
        private PictureBox _warning_PictureBox;
        private Label _warning_Label;
        private void ConstructWarningLabel()
        {
            _warning_PictureBox = new()
            {
                Size = new Size(28, 28),
                Image = Properties.Resources.ExclamationMark,
                SizeMode = PictureBoxSizeMode.StretchImage,
            };

            _warning_Label = new()
            {
                ForeColor = CustomColors.Text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
        }
        private void ShowWarning(Guna2TextBox textBox, string text)
        {
            _warning_PictureBox.Top = textBox.Top + textBox.Height + CustomControls.SpaceBetweenControls;
            _warning_PictureBox.Left = textBox.Left;

            _warning_Label.Top = _warning_PictureBox.Top;
            _warning_Label.Left = _warning_PictureBox.Right + CustomControls.SpaceBetweenControls;
            _warning_Label.Text = LanguageManager.TranslateString(text);

            Panel.Controls.Add(_warning_PictureBox);
            Panel.Controls.Add(_warning_Label);
        }
        private void HideWarning()
        {
            Panel.Controls.Remove(_warning_PictureBox);
            Panel.Controls.Remove(_warning_Label);
        }

        // Save in row
        private void SaveInMainMenuRow()
        {
            IEnumerable<Control> allControls = Panel.Controls.Cast<Control>();

            if (_secondPanel != null)
            {
                allControls = allControls.Concat(_secondPanel.Controls.Cast<Control>());
            }
            if (_notes)
            {
                allControls = allControls.Concat(Controls.OfType<Guna2TextBox>());
            }

            foreach (Control control in allControls)
            {
                if (control is Guna2TextBox textBox)
                {
                    string column = textBox.Name;

                    if (column == ReadOnlyVariables.PricePerUnit_column ||
                        column == ReadOnlyVariables.Shipping_column ||
                        column == ReadOnlyVariables.Tax_column ||
                        column == ReadOnlyVariables.Fee_column ||
                        column == ReadOnlyVariables.Discount_column ||
                        column == ReadOnlyVariables.Total_column)
                    {
                        ProcessNumericColumn(textBox, column);
                    }
                    else if (column == ReadOnlyVariables.Product_column)
                    {
                        ProcessProductColumn(textBox);
                    }
                    else if (column == Products_Form.Column.ProductCategory.ToString())
                    {
                        ProcessProductCategoryColumn(textBox, allControls);
                    }
                    else if (column == ReadOnlyVariables.Note_column)
                    {
                        ProcessNoteColumn(textBox);
                    }
                    else
                    {
                        _selectedRow.Cells[column].Value = textBox.Text.Trim();
                    }
                }
                else if (control is Guna2ComboBox comboBox)
                {
                    string columnName = comboBox.Name;
                    _selectedRow.Cells[columnName].Value = comboBox.SelectedItem.ToString().Trim();
                }
                else if (control is Guna2DateTimePicker datePicker)
                {
                    string columnName = datePicker.Name;
                    _selectedRow.Cells[columnName].Value = Tools.FormatDate(datePicker.Value);
                }
            }
            Close();
        }
        private void ProcessNumericColumn(Guna2TextBox textBox, string column)
        {
            if (decimal.TryParse(textBox.Text.Trim(), out decimal number))
            {
                _selectedRow.Cells[column].Value = string.Format("{0:N2}", number);
            }
        }
        private void ProcessProductColumn(Guna2TextBox textBox)
        {
            (string productName, string companyName) = ParseProductInfo(textBox.Text);
            MainMenu_Form.SelectedOption selected = MainMenu_Form.Instance.Selected;
            List<Category> categoryList = selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.ItemsInPurchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            if (selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales)
            {
                UpdateItemsInTransaction(productName, companyName, categoryList, false);
            }
            else if (selected is MainMenu_Form.SelectedOption.ItemsInPurchase or MainMenu_Form.SelectedOption.ItemsInSale)
            {
                _selectedRow.Cells[ReadOnlyVariables.Product_column].Value = productName;

                Product product = MainMenu_Form.GetProductProductNameIsFrom(categoryList, productName, companyName);
                Category category = MainMenu_Form.GetCategoryProductNameIsFrom(categoryList, productName, companyName);

                DataGridViewCellCollection cells = _selectedRow.Cells;
                cells[ReadOnlyVariables.Category_column].Value = category.Name;
                cells[ReadOnlyVariables.Country_column].Value = product.CountryOfOrigin;
                cells[ReadOnlyVariables.Company_column].Value = product.CompanyOfOrigin;
            }
        }
        private static (string ProductName, string CompanyName) ParseProductInfo(string text)
        {
            string[] parts = text.Split('>');
            if (parts.Length > 2)
            {
                return (
                    ProductName: parts[2].Trim(),
                    CompanyName: parts[0].Trim()
                );
            }
            return (
                ProductName: text.Trim(),
                CompanyName: text.Trim()
            );
        }
        private void ProcessProductCategoryColumn(Guna2TextBox textBox, IEnumerable<Control> allControls)
        {
            List<Category> categoryList = [];
            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ProductPurchases)
            {
                categoryList = MainMenu_Form.Instance.CategoryPurchaseList;
            }
            else if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ProductSales)
            {
                categoryList = MainMenu_Form.Instance.CategorySaleList;
            }

            string newCategoryName = textBox.Text.Trim();
            string newProductName = allControls.FirstOrDefault(item => item.Name == Products_Form.Column.ProductName.ToString())?.Text;
            string companyName = allControls.FirstOrDefault(item =>
                item.Name == Products_Form.Column.CompanyOfOrigin.ToString())?.Text;

            Category category = MainMenu_Form.GetCategoryProductNameIsFrom(categoryList, newProductName, companyName);
            Product product = MainMenu_Form.GetProductProductNameIsFrom(categoryList, newProductName, companyName);

            if (category.Name != textBox.Text)
            {
                // Remove product from old category
                category.ProductList.Remove(product);

                // Get new category
                Category newCategory;
                if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales ||
                    MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
                {
                    newCategory = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == textBox.Text);
                }
                else
                {
                    newCategory = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == textBox.Text);
                }

                // Add product to new category
                newCategory.ProductList.Add(product);

                // Update all instances in DataGridViews
                string categoryColumn = ReadOnlyVariables.Category_column;
                string nameColumn = ReadOnlyVariables.Product_column;
                foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
                {
                    if (row.Cells[categoryColumn].Value.ToString() == category.Name
                        && row.Cells[nameColumn].Value.ToString() == product.Name)
                    {
                        row.Cells[categoryColumn].Value = newCategory.Name;
                    }
                }
            }

            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.Instance.Selected);

            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ProductPurchases)
            {
                UpdateItemsInTransaction(product.Name, companyName, MainMenu_Form.Instance.CategoryPurchaseList, true);
            }
            else if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ProductSales)
            {
                UpdateItemsInTransaction(product.Name, companyName, MainMenu_Form.Instance.CategorySaleList, true);
            }
        }
        private void ProcessNoteColumn(Guna2TextBox textBox)
        {
            DataGridViewCell cell = _selectedRow.Cells[ReadOnlyVariables.Note_column];
            string note = textBox.Text.Trim();

            if (note == "")
            {
                cell.Value = ReadOnlyVariables.EmptyCell;
                DataGridViewManager.RemoveUnderlineFromCell(cell);
            }
            else
            {
                cell.Value = ReadOnlyVariables.Show_text;
                DataGridViewManager.AddUnderlineToCell(cell);
            }

            cell.Tag = note;
        }

        // Methods
        private void ValidateInputs(object sender, EventArgs e)
        {
            IEnumerable<Control> allControls = Panel.Controls.Cast<Control>();

            if (_secondPanel != null)
            {
                allControls = allControls.Concat(_secondPanel.Controls.Cast<Control>());
            }

            foreach (Control control in allControls)
            {
                if (control is Guna2TextBox gunaTextBox)
                {
                    if (string.IsNullOrEmpty(gunaTextBox.Text)
                        || gunaTextBox.Text == ReadOnlyVariables.EmptyCell
                        || gunaTextBox.Tag?.ToString() == "0")
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
            if (!Controls.Contains(_selectedReceipt_Label) && _containsReceipt && Properties.Settings.Default.PurchaseReceipts)
            {
                Save_Button.Enabled = false;
                return;
            }
            Save_Button.Enabled = true;
        }
        private void UpdateItemsInTransaction(string productName, string companyName, List<Category> categoryList, bool isProductForm)
        {
            string productColumn = isProductForm ? Products_Form.Column.ProductName.ToString() : ReadOnlyVariables.Product_column;
            _selectedRow.Cells[productColumn].Value = productName;

            string categoryColumn = isProductForm ? Products_Form.Column.ProductCategory.ToString() : ReadOnlyVariables.Category_column;
            string category = MainMenu_Form.GetCategoryNameProductIsFrom(categoryList, productName, companyName);
            _selectedRow.Cells[categoryColumn].Value = category;

            string countryColumn = isProductForm ? Products_Form.Column.CountryOfOrigin.ToString() : ReadOnlyVariables.Country_column;
            string country = MainMenu_Form.GetCountryProductIsFrom(categoryList, productName);
            _selectedRow.Cells[countryColumn].Value = country;

            string companyColumn = isProductForm ? Products_Form.Column.CompanyOfOrigin.ToString() : ReadOnlyVariables.Company_column;
            _selectedRow.Cells[companyColumn].Value = companyName;

            if (_selectedRow.Tag is not (List<string> itemList, TagData tagData))
            {
                return;
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                string item = itemList[i];
                if (item.Contains(ReadOnlyVariables.Receipt_text))
                {
                    continue;
                }

                string[] items = item.Split(',');

                if (items[0] == _oldProductName && items[1] == _oldCategoryName)
                {
                    items[0] = productName;
                    items[1] = category;
                    items[2] = country;
                    items[3] = companyName;

                    itemList[i] = string.Join(",", items);
                }
            }

            _selectedRow.Tag = (itemList, tagData);
        }
        private void UpdateChargedDifferenceInMainMenuRow()
        {
            if (_selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                MainMenu_Form.IsProgramLoading = true;

                string productName = _selectedRow.Cells[ReadOnlyVariables.Product_column].Value.ToString();

                if (productName == ReadOnlyVariables.MultipleItems_text)
                {
                    DataGridViewManager.UpdateChargedDifferenceInRowWithMultipleItems(_selectedRow);
                }
                else
                {
                    DataGridViewManager.UpdateChargedDifferenceInRowWithNoItems(_selectedRow);
                }

                MainMenu_Form.IsProgramLoading = false;
            }
        }
        private void HandleReceiptChanges()
        {
            if (_receiptFilePath == null) { return; }

            // Case 1: Adding a receipt to a row that didn't have one
            if (_addedReceipt && !_removedReceipt)
            {
                (string newPath, bool saved) = ReceiptManager.SaveReceiptInFile(_receiptFilePath);
                if (saved)
                {
                    ReceiptManager.AddReceiptToTag(_selectedRow, newPath);
                }
            }

            // Case 2: Replacing an existing receipt with a new one
            else if (_removedReceipt && _addedReceipt)
            {
                ReceiptManager.RemoveReceiptFromTagAndFile(_selectedRow);

                // Then save and add the new receipt
                (string newPath, bool saved) = ReceiptManager.SaveReceiptInFile(_receiptFilePath);
                if (saved)
                {
                    ReceiptManager.AddReceiptToTag(_selectedRow, newPath);
                }
            }

            // Case 3: Removing a receipt
            else if (_removedReceipt && !_addedReceipt)
            {
                ReceiptManager.RemoveReceiptFromTagAndFile(_selectedRow);
            }

            MainMenu_Form.SetHasReceiptColumn(_selectedRow, _receiptFilePath);
        }
        private static void UpdateAllDataGridViewRows(string columnName, string oldValue, string newValue, bool updateItemsInTransaction = false)
        {
            UpdateRowsInDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, columnName, oldValue, newValue, updateItemsInTransaction);
            UpdateRowsInDataGridView(MainMenu_Form.Instance.Sale_DataGridView, columnName, oldValue, newValue, updateItemsInTransaction);
        }
        private static void UpdateRowsInDataGridView(DataGridView dataGridView, string columnName, string oldValue, string newValue, bool updateItemsInTransaction)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[columnName].Value.ToString() == oldValue)
                {
                    row.Cells[columnName].Value = newValue;
                    _hasChanges = true;

                    // Update items in transaction if requested
                    if (updateItemsInTransaction && row.Tag is (List<string> itemList, TagData tagData))
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            string item = itemList[i];
                            if (item.Contains(ReadOnlyVariables.Receipt_text))
                            {
                                continue;
                            }

                            string[] items = item.Split(',');

                            // Update the relevant field based on the column name
                            switch (columnName)
                            {
                                case nameof(MainMenu_Form.Column.Product):
                                    items[0] = items[0] == oldValue ? newValue : items[0];
                                    break;
                                case nameof(MainMenu_Form.Column.Category):
                                    items[1] = items[1] == oldValue ? newValue : items[1];
                                    break;
                                case nameof(MainMenu_Form.Column.Country):
                                    items[2] = items[2] == oldValue ? newValue : items[2];
                                    break;
                                case nameof(MainMenu_Form.Column.Company):
                                    items[3] = items[3] == oldValue ? newValue : items[3];
                                    break;
                            }
                            itemList[i] = string.Join(",", items);
                        }
                        row.Tag = (itemList, tagData);
                    }
                }
            }
        }

        // Save in lists and update MainMenu_Form
        /// <summary>
        /// When changes are made to Categories_Form, Accountants_Form, etc., the changes are reflected in MainMenu_Form's lists and DataGridViews.
        /// </summary>
        private void SaveInListsAndUpdateMainMenuForm()
        {
            if (_selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                string type = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases ? "purchase" : "sale";
                string id = _selectedRow.Cells[ReadOnlyVariables.ID_column].Value.ToString();
                CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, $"Modified {type} '{id}'");
                return;
            }

            switch (_selectedTag)
            {
                case nameof(MainMenu_Form.DataGridViewTag.Category):
                    UpdateCategory();
                    break;
                case nameof(MainMenu_Form.DataGridViewTag.Product):
                    UpdateProduct();
                    break;
                case nameof(MainMenu_Form.DataGridViewTag.Company):
                    UpdateCompany();
                    break;
                case nameof(MainMenu_Form.DataGridViewTag.Accountant):
                    UpdateAccountant();
                    break;
            }

            if (_hasChanges && _selectedTag != MainMenu_Form.SelectedOption.ItemsInPurchase.ToString())
            {
                MainMenu_Form.Instance.UpdateTotalLabels();
                MainMenu_Form.Instance.LoadOrRefreshMainCharts();
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
            }

            UpdateValidationInOpenForms();
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, $"Modified {_selectedTag} list");
        }
        private void UpdateCategory()
        {
            Category category;
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
            {
                category = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == _listOfOldValues[0]);
            }
            else
            {
                category = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == _listOfOldValues[0]);
            }

            string oldCategory = category.Name;
            Guna2TextBox textBox = Panel.Controls.OfType<Guna2TextBox>().FirstOrDefault();
            string newCategory = textBox.Text;
            category.Name = newCategory;

            UpdateAllDataGridViewRows(ReadOnlyVariables.Category_column, oldCategory, newCategory, true);
            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.Instance.Selected);
        }
        private void UpdateProduct()
        {
            Category category;
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
            {
                category = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == _listOfOldValues[2]);
            }
            else
            {
                category = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == _listOfOldValues[2]);
            }

            Product product = category.ProductList.FirstOrDefault(p => p.Name == _listOfOldValues[1]);

            if (HasProductChanged(product))
            {
                UpdateProductDetails(product);
                UpdateProductInDataGridViews(product);
            }
        }
        private bool HasProductChanged(Product product)
        {
            Product newProduct = GetNewProductInfo();

            return product.ProductID != newProduct.ProductID ||
                   product.Name != newProduct.Name ||
                   product.CountryOfOrigin != newProduct.CountryOfOrigin ||
                   product.CompanyOfOrigin != newProduct.CompanyOfOrigin;
        }
        private void UpdateProductDetails(Product product)
        {
            Product newProduct = GetNewProductInfo();

            product.ProductID = newProduct.ProductID;
            product.Name = newProduct.Name;
            product.CountryOfOrigin = newProduct.CountryOfOrigin;
            product.CompanyOfOrigin = newProduct.CompanyOfOrigin;
        }
        private Product GetNewProductInfo()
        {
            Product product = new();
            IEnumerable<Control> allControls = Panel.Controls.Cast<Control>();

            foreach (Control control in allControls)
            {
                if (control is Guna2TextBox textBox)
                {
                    switch (textBox.Name)
                    {
                        case nameof(Products_Form.Column.ProductID):
                            product.ProductID = textBox.Text;
                            break;
                        case nameof(Products_Form.Column.ProductName):
                            product.Name = textBox.Text;
                            break;
                        case nameof(Products_Form.Column.CountryOfOrigin):
                            product.CountryOfOrigin = textBox.Text;
                            break;
                        case nameof(Products_Form.Column.CompanyOfOrigin):
                            product.CompanyOfOrigin = textBox.Text;
                            break;
                    }
                }
            }
            return product;
        }
        private void UpdateProductInDataGridViews(Product product)
        {
            string productColumn = ReadOnlyVariables.Product_column;
            string oldName = _listOfOldValues[1];
            string oldID = _listOfOldValues[0];
            string oldCountry = _listOfOldValues[3];
            string oldCompany = _listOfOldValues[4];

            foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
            {
                if (row.Cells[productColumn].Value.ToString() == oldName)
                {
                    UpdateProductRowValues(row, product, oldID, oldCountry, oldCompany);
                }
            }
        }
        private static void UpdateProductRowValues(DataGridViewRow row, Product product, string oldID, string oldCountry, string oldCompany)
        {
            row.Cells[ReadOnlyVariables.Product_column].Value = product.Name;

            string idColumn = ReadOnlyVariables.ID_column;
            if (row.Cells[idColumn].Value.ToString() == oldID)
            {
                row.Cells[idColumn].Value = product.ProductID;
            }

            string countryColumn = ReadOnlyVariables.Country_column;
            if (row.Cells[countryColumn].Value.ToString() == oldCountry)
            {
                row.Cells[countryColumn].Value = product.CountryOfOrigin;
            }

            string companyColumn = ReadOnlyVariables.Company_column;
            if (row.Cells[companyColumn].Value.ToString() == oldCompany)
            {
                row.Cells[companyColumn].Value = product.CompanyOfOrigin;
            }
        }
        private void UpdateCompany()
        {
            string oldCompany = MainMenu_Form.Instance.CompanyList.FirstOrDefault(a => a == _listOfOldValues[0]);
            Guna2TextBox textBox = Panel.Controls.OfType<Guna2TextBox>().FirstOrDefault();
            string newCompany = textBox.Text;

            // Update company in the company list
            if (oldCompany != null)
            {
                int index = MainMenu_Form.Instance.CompanyList.IndexOf(oldCompany);
                MainMenu_Form.Instance.CompanyList[index] = newCompany;
            }

            UpdateCompanyInProducts(oldCompany, newCompany, MainMenu_Form.Instance.CategoryPurchaseList);
            UpdateCompanyInProducts(oldCompany, newCompany, MainMenu_Form.Instance.CategorySaleList);
            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategoryPurchases);
            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategorySales);

            UpdateAllDataGridViewRows(ReadOnlyVariables.Company_column, oldCompany, newCompany, true);
        }
        private static void UpdateCompanyInProducts(string oldCompany, string newCompany, List<Category> categoryList)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.CompanyOfOrigin == oldCompany)
                    {
                        product.CompanyOfOrigin = newCompany;
                        _hasChanges = true;
                    }
                }
            }
        }
        private void UpdateAccountant()
        {
            string oldAccountant = MainMenu_Form.Instance.AccountantList.FirstOrDefault(a => a == _listOfOldValues[0]);
            Guna2TextBox textBox = Panel.Controls.OfType<Guna2TextBox>().FirstOrDefault();
            string newAccountant = textBox.Text;

            if (oldAccountant != null)
            {
                int index = MainMenu_Form.Instance.AccountantList.IndexOf(oldAccountant);
                MainMenu_Form.Instance.AccountantList[index] = newAccountant;
            }

            UpdateAllDataGridViewRows(ReadOnlyVariables.Accountant_column, oldAccountant, newAccountant, false);
        }

        // Validate TextBoxes in other forms
        private void UpdateValidationInOpenForms()
        {
            string newText = GetNewTextValue();

            if (Application.OpenForms[nameof(AddPurchase_Form)] is AddPurchase_Form purchaseForm)
            {
                switch (_selectedTag)
                {
                    case nameof(MainMenu_Form.DataGridViewTag.Product):
                        UpdateProductTextBox(purchaseForm.ProductName_TextBox, _listOfOldValues[1], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Category):
                        UpdateCategoryInProductPath(purchaseForm.ProductName_TextBox, _listOfOldValues[0], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Company):
                        UpdateCompanyInProductPath(purchaseForm.ProductName_TextBox, _listOfOldValues[0], newText);
                        break;
                }
            }

            if (Application.OpenForms[nameof(AddSale_Form)] is AddSale_Form saleForm)
            {
                switch (_selectedTag)
                {
                    case nameof(MainMenu_Form.DataGridViewTag.Product):
                        UpdateProductTextBox(saleForm.ProductName_TextBox, _listOfOldValues[1], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Category):
                        UpdateCategoryInProductPath(saleForm.ProductName_TextBox, _listOfOldValues[0], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Company):
                        UpdateCompanyInProductPath(saleForm.ProductName_TextBox, _listOfOldValues[0], newText);
                        break;
                }
            }

            if (Application.OpenForms[nameof(Products_Form)] is Products_Form productsForm)
            {
                switch (_selectedTag)
                {
                    case nameof(MainMenu_Form.DataGridViewTag.Product):
                        UpdateTextBox(productsForm.ProductName_TextBox, _listOfOldValues[1], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Category):
                        UpdateTextBox(productsForm.ProductCategory_TextBox, _listOfOldValues[0], newText);
                        break;
                    case nameof(MainMenu_Form.DataGridViewTag.Company):
                        UpdateTextBox(productsForm.CompanyOfOrigin_TextBox, _listOfOldValues[0], newText);
                        break;
                }
            }
        }
        private string GetNewTextValue()
        {
            if (_selectedTag == nameof(MainMenu_Form.DataGridViewTag.Product))
            {
                return Panel.Controls.OfType<Guna2TextBox>()
                    .First(t => t.Name == Products_Form.Column.ProductName.ToString()).Text;
            }
            return Panel.Controls.OfType<Guna2TextBox>().First().Text;
        }
        private static void UpdateTextBox(Guna2TextBox textBox, string oldValue, string newValue)
        {
            if (textBox.Text == oldValue)
            {
                textBox.Text = newValue;
            }
        }
        private static void UpdateProductTextBox(Guna2TextBox textBox, string oldProduct, string newProduct)
        {
            if (textBox.Text.EndsWith($"> {oldProduct}"))
            {
                textBox.Text = textBox.Text.Replace($"> {oldProduct}", $"> {newProduct}");
            }
        }
        private static void UpdateCategoryInProductPath(Guna2TextBox textBox, string oldCategory, string newCategory)
        {
            if (textBox.Text.Contains($"> {oldCategory} >"))
            {
                textBox.Text = textBox.Text.Replace($"> {oldCategory} >", $"> {newCategory} >");
            }
        }
        private static void UpdateCompanyInProductPath(Guna2TextBox textBox, string oldCompany, string newCompany)
        {
            if (textBox.Text.StartsWith($"{oldCompany} >"))
            {
                textBox.Text = textBox.Text.Replace($"{oldCompany} >", $"{newCompany} >");
            }
        }

        // Construct controls
        private Panel _secondPanel;
        private void ConstructPanel()
        {
            _secondPanel = new()
            {
                Size = Panel.Size,
                Location = new Point(Panel.Left, Panel.Bottom + 40),
                Anchor = AnchorStyles.Top
            };
            _secondPanel.Click += CloseSearchBox;
            Controls.Add(_secondPanel);
        }
        private Label ConstructLabel(string text, int left, Control control)
        {
            Label label = new()
            {
                ForeColor = CustomColors.Text,
                Cursor = Cursors.Arrow,
                Location = new Point(left, 20),
                Text = text,
                Name = text + "_Label",  // This is needed for the language translation
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                AccessibleDescription = AccessibleDescriptionManager.DoNotCache
            };
            label.Click += CloseSearchBox;
            control.Controls.Add(label);

            return label;
        }

        
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, CustomControls.KeyPressValidation keyPressValidation, bool pressSaveButton, bool smallWidth, bool closeSearchBox, Control control, string sizeType = "standard")
        {
            Guna2TextBox textBox = new()
            {
                Location = new Point(left, 43 + CustomControls.SpaceBetweenControls),
                Height = ScaledControlHeight, 
                Name = name,
                Text = text,
                ForeColor = CustomColors.Text,
                BackColor = CustomColors.ControlBack,
                Font = new Font("Segoe UI", 9F), 
                MaxLength = maxLength,
                FillColor = CustomColors.ControlBack,
                BorderColor = CustomColors.ControlBorder,
                BorderRadius = 3,
                Cursor = Cursors.Hand,
                ShortcutsEnabled = false,
                AccessibleDescription = AccessibleDescriptionManager.DoNotCache,
                FocusedState = {
                    FillColor = CustomColors.ControlBack,
                    BorderColor = CustomColors.AccentBlue
                },
                HoverState = { BorderColor = CustomColors.AccentBlue }
            };

            switch (sizeType)
            {
                case "large":
                    textBox.Width = ScaledLargeWidth; 
                    break;
                case "standard":
                    textBox.Width = ScaledStandardWidth; 
                    break;
                default:
                    textBox.Width = ScaledStandardWidth;
                    break;
            }

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case CustomControls.KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    TextBoxValidation.OnlyAllowNumbersAndOneDecimalAndOneMinus(textBox);
                    break;
                case CustomControls.KeyPressValidation.OnlyNumbersAndDecimal:
                    TextBoxValidation.OnlyAllowNumbersAndOneDecimal(textBox);
                    break;
                case CustomControls.KeyPressValidation.OnlyNumbers:
                    TextBoxValidation.OnlyAllowNumbers(textBox);
                    break;
                case CustomControls.KeyPressValidation.OnlyLetters:
                    TextBoxValidation.OnlyAllowLetters(textBox);
                    break;
                case CustomControls.KeyPressValidation.None:
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
            if (closeSearchBox)
            {
                textBox.Click += CloseSearchBox;
            }
            textBox.TextChanged += ValidateInputs;
            TextBoxManager.Attach(textBox);
            control.Controls.Add(textBox);

            return textBox;
        }
        private Guna2DateTimePicker ConstructDatePicker(int left, string name, DateTime value, Control control)
        {
            Guna2DateTimePicker gDatePicker = new()
            {
                Location = new Point(left, 43 + CustomControls.SpaceBetweenControls),
                Size = new Size(ScaledDatePickerWidth, ScaledControlHeight), 
                FillColor = CustomColors.ControlBack,
                ForeColor = CustomColors.Text,
                BorderColor = CustomColors.ControlBorder,
                BorderRadius = 3,
                Name = name,
                Value = value,
                Font = new Font("Segoe UI", 9F), // Changed to match other text boxes
                AccessibleDescription = AccessibleDescriptionManager.DoNotCache,
                HoverState = { BorderColor = CustomColors.AccentBlue }
            };
            gDatePicker.Click += CloseSearchBox;
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
            CustomControls.SetGTextBoxToValid(textBox);
            HideWarning();
            DisableSaveButton();
        }
        public void CloseSearchBox(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}