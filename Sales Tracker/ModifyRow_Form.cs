using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : Form
    {
        // Properties
        private readonly string selectedTag = "";
        private readonly DataGridViewRow selectedRow;
        private string receiptFilePath;

        // Init.
        public ModifyRow_Form(DataGridViewRow row)
        {
            InitializeComponent();

            selectedRow = row;
            selectedTag = row.DataGridView.Tag.ToString();
            receiptFilePath = DataGridViewManager.GetFilePathFromRowTag(row.Tag);

            if (receiptFilePath != "" && !File.Exists(receiptFilePath))
            {
                CustomMessageBox.Show("Receipt does not exist", "The receipt no longer exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                Log.Error_FileDoesNotExist(receiptFilePath);
            }

            ConstructControls();
            AttachChangeHandlers();
            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this, true);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(Save_Button);
            Theme.MakeGButtonBlueSecondary(Cancel_Button);
        }

        // Form event handlers
        private Control controlToFocus;
        private void ModifyRow_Form_Shown(object sender, EventArgs e)
        {
            controlToFocus?.Focus();
            CenterControls();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Methods for checking if there are changes
        private bool hasChanges = false;
        private void AttachChangeHandlers()
        {
            foreach (Control control in Panel.Controls)
            {
                AttachChangeHandler(control);
            }

            if (SecondPanel != null)
            {
                foreach (Control control in SecondPanel.Controls)
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
            if (index < 0 || index >= listOfOldValues.Count) { return; }

            string currentValue = GetControlValue(control);
            hasChanges = currentValue != listOfOldValues[index];
        }
        private static string GetControlValue(Control control)
        {
            return control switch
            {
                Guna2TextBox textBox => textBox.Text.Trim(),
                Guna2ComboBox comboBox => comboBox.SelectedItem?.ToString().Trim() ?? "",
                Guna2DateTimePicker datePicker => Tools.FormatDate(datePicker.Value),
                _ => ""
            };
        }
        private int GetControlIndex(string controlName)
        {
            int index = 0;
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
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
            CloseAllPanels(null, null);
            SaveInRow();
            UpdateRow();
            SaveInListsAndUpdateMainMenuForm();

            // If the user selected a new receipt
            if (receiptFilePath != null)
            {
                if (removedReceipt)
                {
                    ReceiptsManager.RemoveReceiptFromFile(selectedRow);
                }
                if (addedReceipt)
                {
                    ReceiptsManager.SaveReceiptInFile(receiptFilePath);
                    ReceiptsManager.AddReceiptToTag(selectedRow, receiptFilePath);
                }
            }

            DataGridViewManager.DataGridViewRowChanged(MainMenu_Form.Instance.SelectedDataGridView, MainMenu_Form.Instance.Selected);
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
                left = controlWidth;
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
                left = ConstructControlsForItemsInTransaction();
            }

            SizeControls(left, secondLeft);
        }
        private void SizeControls(int left, int secondLeft)
        {
            Width = left + 80;
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

            if (notes)
            {
                Height += 180;
            }
            MinimumSize = new Size(Math.Max(500, Width), Height);
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
        private bool secondRow = false, notes = false;
        private readonly List<string> listOfOldValues = [];
        private string oldProductName, oldCategoryName;
        private void ConstructControlsForAccountant()
        {
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                ConstructLabel(Accountants_Form.Instance.ColumnHeaders[Accountants_Form.Column.AccountantName], 0, Panel);

                controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyLetters, true, false, Panel);
                controlToFocus.TextChanged += Accountant_TextBox_TextChanged;

                ConstructWarningLabel();
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
            bool containsAccountant = MainMenu_Form.Instance.AccountantList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

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
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                ConstructLabel(Categories_Form.Instance.ColumnHeaders[Categories_Form.Column.CategoryName], 0, Panel);

                controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, true, false, Panel);
                controlToFocus.TextChanged += Category_TextBox_TextChanged;

                ConstructWarningLabel();
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
                categoriesList = MainMenu_Form.Instance.CategoryPurchaseList;
            }
            else
            {
                categoriesList = MainMenu_Form.Instance.CategorySaleList;
            }

            bool containsCategory = categoriesList.Any(category => category.Name.Equals(textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

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
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                listOfOldValues.Add(cellValue);

                ConstructLabel(Companies_Form.Instance.ColumnHeaders[Companies_Form.Column.Company], 0, Panel);

                controlToFocus = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, true, false, Panel);
                controlToFocus.TextChanged += Company_TextBox_TextChanged;

                ConstructWarningLabel();
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

            bool containsCompany = MainMenu_Form.Instance.CompanyList.Contains(textBox.Text.Trim(), StringComparer.OrdinalIgnoreCase);
            bool isOldValueDifferent = !string.Equals(listOfOldValues[0], textBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);

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
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        break;

                    case nameof(Products_Form.Column.ProductName):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.ProductName], left, Panel);
                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        oldProductName = cellValue;
                        break;

                    case nameof(Products_Form.Column.ProductCategory):
                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.ProductCategory], left, Panel);

                        Guna2TextBox ProductCategory_TextBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        SearchBox.Attach(ProductCategory_TextBox, this, GetSearchResults, searchBoxMaxHeight);
                        ProductCategory_TextBox.TextChanged += ValidateInputs;
                        oldCategoryName = cellValue;
                        break;

                    case nameof(Products_Form.Column.CountryOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.CountryOfOrigin], left, Panel);

                        Guna2TextBox textBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        SearchBox.Attach(textBox, this, () => Country.countries, searchBoxMaxHeight);
                        textBox.TextChanged += ValidateInputs;
                        break;

                    case nameof(Products_Form.Column.CompanyOfOrigin):

                        ConstructLabel(Products_Form.Instance.ColumnHeaders[Products_Form.Column.CompanyOfOrigin], left, Panel);

                        textBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.CompanyList);
                        SearchBox.Attach(textBox, this, () => searchResult, searchBoxMaxHeight);
                        textBox.TextChanged += ValidateInputs;
                        break;
                }
                left += controlWidth + CustomControls.SpaceBetweenControls;
            }
            return left - CustomControls.SpaceBetweenControls;
        }
        private List<SearchResult> GetSearchResults()
        {
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategorySaleNames());
            }
            else { return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryPurchaseNames()); }
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

                        controlToFocus = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);

                        left += controlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Accountant):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Accountant];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Accountant]; }

                        ConstructLabel(text, left, Panel);

                        Guna2TextBox Accountant_TextBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.AccountantList);
                        SearchBox.Attach(Accountant_TextBox, this, () => searchResult, searchBoxMaxHeight);
                        Accountant_TextBox.TextChanged += ValidateInputs;

                        left += controlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Product):
                        if (cellValue != ReadOnlyVariables.MultipleItems_text)
                        {
                            ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Product], left, Panel);

                            Guna2TextBox ProductName_TextBox = ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                            SearchBox.Attach(ProductName_TextBox, this, GetListForSearchBox, searchBoxMaxHeight, false, true);
                            ProductName_TextBox.TextChanged += ValidateInputs;

                            left += controlWidth + CustomControls.SpaceBetweenControls;
                            oldProductName = cellValue;
                        }

                        // Button
                        byte buttonWidth = 200;
                        Receipt_Button = new()
                        {
                            Location = new Point(left, 43 + CustomControls.SpaceBetweenControls),
                            Text = "Change receipt",
                            BackColor = CustomColors.ControlBack,
                            FillColor = CustomColors.ControlBack,
                            Size = new Size(buttonWidth, controlHeight),
                            BorderRadius = 2,
                            BorderThickness = 1,
                            Font = new Font("Segoe UI", 10),
                        };
                        Receipt_Button.Click += (_, _) =>
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

                        left += buttonWidth + CustomControls.SpaceBetweenControls;

                        // ImageButton
                        RemoveReceipt_ImageButton = new()
                        {
                            Size = new Size(38, 38),
                            ImageSize = new Size(30, 30),
                            Image = Properties.Resources.CloseGray,
                            Anchor = AnchorStyles.Top
                        };
                        RemoveReceipt_ImageButton.HoverState.ImageSize = new Size(30, 30);
                        RemoveReceipt_ImageButton.PressedState.ImageSize = new Size(30, 30);
                        RemoveReceipt_ImageButton.Click += (_, _) =>
                        {
                            CloseAllPanels(null, null);
                            RemoveReceiptLabel();
                            removedReceipt = true;
                        };
                        RemoveReceipt_ImageButton.MouseEnter += (_, _) =>
                        {
                            RemoveReceipt_ImageButton.BackColor = CustomColors.FileHover;
                        };
                        RemoveReceipt_ImageButton.MouseLeave += (_, _) =>
                        {
                            RemoveReceipt_ImageButton.BackColor = CustomColors.MainBackground;
                        };

                        // Label
                        SelectedReceipt_Label = new()
                        {
                            ForeColor = CustomColors.Text,
                            Font = new Font("Segoe UI", 10),
                            AutoSize = true,
                            Anchor = AnchorStyles.Top
                        };
                        SelectedReceipt_Label.Click += CloseAllPanels;
                        break;

                    case nameof(MainMenu_Form.Column.Date):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Date], left, Panel);
                        ConstructDatePicker(left, columnName, DateTime.Parse(cellValue), Panel);
                        left += controlWidth;
                        break;

                    case nameof(MainMenu_Form.Column.TotalItems):
                        secondRow = true;
                        if (cellValue == ReadOnlyVariables.EmptyCell) { continue; }

                        string productName = selectedRow.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString();

                        if (productName == ReadOnlyVariables.MultipleItems_text)
                        {
                            continue;
                        }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.TotalItems], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbers, false, true, SecondPanel);
                        secondLeft += smallControlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        if (cellValue == ReadOnlyVariables.EmptyCell) { continue; }

                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Shipping):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Shipping], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, true, SecondPanel);
                        secondLeft += smallControlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Tax):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Tax], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Fee):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Fee], secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Total];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Total]; }

                        ConstructLabel(text, secondLeft, SecondPanel);
                        ConstructTextBox(secondLeft, columnName, cellValue, 10, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, false, true, SecondPanel);
                        secondLeft += smallControlWidth;
                        break;

                    case nameof(MainMenu_Form.Column.Note):
                        Label label = ConstructLabel("Notes", secondLeft, this);

                        string note = selectedRow.Cells[column.Index].Tag?.ToString() ?? "";

                        Guna2TextBox textBox = ConstructTextBox(0, columnName, note, 10, CustomControls.KeyPressValidation.None, false, false, this);

                        label.Location = new Point((ClientSize.Width - label.Width) / 2, SecondPanel.Bottom);
                        label.Anchor = AnchorStyles.Top;

                        textBox.Size = new Size(400, 100);
                        textBox.Location = new Point((ClientSize.Width - textBox.Width) / 2, label.Bottom + CustomControls.SpaceBetweenControls);
                        textBox.Anchor = AnchorStyles.Top;
                        textBox.MaxLength = 1000;
                        textBox.Multiline = true;

                        notes = true;
                        break;
                }
            }
            return (left, secondLeft);
        }
        private int ConstructControlsForItemsInTransaction()
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

                        Guna2TextBox ProductName_TextBox = ConstructTextBox(0, columnName, cellValue, 50, CustomControls.KeyPressValidation.None, false, false, Panel);
                        SearchBox.Attach(ProductName_TextBox, this, GetListForSearchBox, searchBoxMaxHeight, false, true);
                        ProductName_TextBox.TextChanged += ValidateInputs;

                        left += controlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.TotalItems):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.TotalItems], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbers, true, false, Panel);
                        left += controlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.PricePerUnit):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.PricePerUnit], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, false, Panel);
                        left += controlWidth + CustomControls.SpaceBetweenControls;
                        break;

                    case nameof(MainMenu_Form.Column.Total):
                        ConstructLabel(MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Total], left, Panel);
                        ConstructTextBox(left, columnName, cellValue, 50, CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, true, false, Panel);
                        left += controlWidth;
                        break;
                }
            }
            return left;
        }
        private static List<SearchResult> GetListForSearchBox()
        {
            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ItemsInSale or MainMenu_Form.SelectedOption.Sales)
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductSaleNames());
            }
            else
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames());
            }
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
                Receipt_Button.Parent.Top + Receipt_Button.Bottom + CustomControls.SpaceBetweenControls);

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
        private PictureBox Warning_PictureBox;
        private Label Warning_Label;
        private void ConstructWarningLabel()
        {
            Warning_PictureBox = new()
            {
                Size = new Size(28, 28),
                Image = Properties.Resources.ExclamationMark,
                SizeMode = PictureBoxSizeMode.StretchImage,
            };

            Warning_Label = new()
            {
                ForeColor = CustomColors.Text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
        }
        private void ShowWarning(Guna2TextBox textBox, string text)
        {
            Warning_PictureBox.Top = textBox.Top + textBox.Height + CustomControls.SpaceBetweenControls;
            Warning_PictureBox.Left = textBox.Left;
            Warning_Label.Top = Warning_PictureBox.Top;
            Warning_Label.Left = Warning_PictureBox.Right + CustomControls.SpaceBetweenControls;
            Warning_Label.Text = text;
            Panel.Controls.Add(Warning_PictureBox);
            Panel.Controls.Add(Warning_Label);
        }
        private void HideWarning()
        {
            Panel.Controls.Remove(Warning_PictureBox);
            Panel.Controls.Remove(Warning_Label);
        }

        // Methods
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
                    if (string.IsNullOrEmpty(gunaTextBox.Text) || gunaTextBox.Tag?.ToString() == "0")
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
            if (notes)
            {
                allControls = allControls.Concat(Controls.OfType<Guna2TextBox>());
            }

            foreach (Control control in allControls)
            {
                if (control is Guna2TextBox gTextBox)
                {
                    string column = gTextBox.Name;

                    if (column == MainMenu_Form.Column.PricePerUnit.ToString() ||
                        column == MainMenu_Form.Column.Shipping.ToString() ||
                        column == MainMenu_Form.Column.Tax.ToString() ||
                        column == MainMenu_Form.Column.Fee.ToString() ||
                        column == MainMenu_Form.Column.Total.ToString())
                    {
                        // Format the number with two decimal places
                        if (decimal.TryParse(gTextBox.Text.Trim(), out decimal number))
                        {
                            selectedRow.Cells[column].Value = string.Format("{0:N2}", number);
                        }
                    }
                    else if (column == MainMenu_Form.Column.Product.ToString())
                    {
                        string productName = gTextBox.Text.Contains('>')
                            ? gTextBox.Text.Split('>')[1].Trim()
                            : gTextBox.Text.Trim();

                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases)
                        {
                            UpdateItemsInTransaction(selectedRow, productName, MainMenu_Form.Instance.CategoryPurchaseList, false);
                        }
                        else if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
                        {
                            UpdateItemsInTransaction(selectedRow, productName, MainMenu_Form.Instance.CategorySaleList, false);
                        }
                        else if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ItemsInPurchase or MainMenu_Form.SelectedOption.ItemsInSale)
                        {
                            string productColumn = MainMenu_Form.Column.Product.ToString();
                            selectedRow.Cells[productColumn].Value = productName;
                        }
                    }
                    else if (column == Products_Form.Column.ProductName.ToString())
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

                        string newProductName = gTextBox.Text.Trim();
                        Category category = MainMenu_Form.GetCategoryProductNameIsFrom(categoryList, oldProductName);
                        Product product = MainMenu_Form.GetProductProductNameIsFrom(categoryList, oldProductName);

                        // If product was moved to a different category
                        if (category.Name != selectedRow.Cells[2].Value.ToString())
                        {
                            // Remove product from old category
                            category.ProductList.Remove(product);

                            // Get new category
                            Category newCategory;
                            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales || MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
                            {
                                newCategory = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == selectedRow.Cells[2].Value.ToString());
                            }
                            else
                            {
                                newCategory = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == selectedRow.Cells[2].Value.ToString());
                            }

                            // Add product to new category
                            newCategory.ProductList.Add(product);

                            // Update all instances in DataGridViews
                            string categoryColumn = MainMenu_Form.Column.Category.ToString();
                            string nameColumn = MainMenu_Form.Column.Product.ToString();
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
                            UpdateItemsInTransaction(selectedRow, newProductName, MainMenu_Form.Instance.CategoryPurchaseList, true);
                        }
                        else if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.ProductSales)
                        {
                            UpdateItemsInTransaction(selectedRow, newProductName, MainMenu_Form.Instance.CategorySaleList, true);
                        }
                    }
                    else if (column == MainMenu_Form.Column.Note.ToString())
                    {
                        DataGridViewCell cell = selectedRow.Cells[column];

                        if (gTextBox.Text == "")
                        {
                            cell.Value = ReadOnlyVariables.EmptyCell;
                            DataGridViewManager.RemoveUnderlineFromCell(cell);
                        }
                        else
                        {
                            cell.Value = ReadOnlyVariables.Show_text;
                            DataGridViewManager.AddUnderlineToCell(cell);
                        }

                        cell.Tag = gTextBox.Text.Trim();
                    }
                    else
                    {
                        selectedRow.Cells[column].Value = gTextBox.Text.Trim();
                    }
                }
                else if (control is Guna2ComboBox gComboBox)
                {
                    string columnName = gComboBox.Name;
                    selectedRow.Cells[columnName].Value = gComboBox.SelectedItem.ToString().Trim();
                }
                else if (control is Guna2DateTimePicker gDatePicker)
                {
                    string columnName = gDatePicker.Name;
                    selectedRow.Cells[columnName].Value = Tools.FormatDate(gDatePicker.Value);
                }
            }
            Close();
        }
        private void UpdateItemsInTransaction(DataGridViewRow row, string productName, List<Category> categoryList, bool isProduct)
        {
            string productColumn = isProduct ? Products_Form.Column.ProductName.ToString() : MainMenu_Form.Column.Product.ToString();
            row.Cells[productColumn].Value = productName;

            string categoryColumn = isProduct ? Products_Form.Column.ProductCategory.ToString() : MainMenu_Form.Column.Category.ToString();
            string category = MainMenu_Form.GetCategoryNameProductIsFrom(categoryList, oldProductName);
            row.Cells[categoryColumn].Value = category;

            string countryColumn = isProduct ? Products_Form.Column.CountryOfOrigin.ToString() : MainMenu_Form.Column.Country.ToString();
            string country = MainMenu_Form.GetCountryProductIsFrom(categoryList, oldProductName);
            row.Cells[countryColumn].Value = country;

            string companyColumn = isProduct ? Products_Form.Column.CompanyOfOrigin.ToString() : MainMenu_Form.Column.Company.ToString();
            string company = MainMenu_Form.GetCompanyProductIsFrom(categoryList, oldProductName);
            row.Cells[companyColumn].Value = company;

            foreach (DataGridViewRow mainRow in MainMenu_Form.Instance.GetAllRows())
            {
                if (mainRow.Tag is not (List<string> itemList, TagData tagData))
                {
                    continue;
                }

                for (int i = 0; i < itemList.Count; i++)
                {
                    string item = itemList[i];
                    if (item.Contains(ReadOnlyVariables.Receipt_text))
                    {
                        continue;
                    }

                    string[] items = item.Split(',');

                    if (items[0] == oldProductName && items[1] == oldCategoryName)
                    {
                        items[0] = productName;
                        items[1] = category;
                        items[2] = country;
                        items[3] = company;

                        itemList[i] = string.Join(",", items);
                    }
                }

                mainRow.Tag = (itemList, tagData);
            }
        }
        private void UpdateRow()
        {
            if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                MainMenu_Form.IsProgramLoading = true;

                string productName = selectedRow.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString();

                if (productName == ReadOnlyVariables.MultipleItems_text)
                {
                    DataGridViewManager.UpdateRowWithMultipleItems(selectedRow);
                }
                else
                {
                    DataGridViewManager.UpdateRowWithNoItems(selectedRow);
                }

                MainMenu_Form.IsProgramLoading = false;
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

        /// <summary>
        /// When changes are made to Categories_Form, Accountants_Form, etc., the changes are reflected in MainMenu_Form's lists and DataGridViews.
        /// </summary>
        private void SaveInListsAndUpdateMainMenuForm()
        {
            if (selectedTag == MainMenu_Form.DataGridViewTag.SaleOrPurchase.ToString())
            {
                string type = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases ? "purchase" : "sale";
                string id = selectedRow.Cells[MainMenu_Form.Column.ID.ToString()].Value.ToString();
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Modified {type} '{id}'");
                return;
            }

            switch (selectedTag)
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

            bool product = selectedTag == MainMenu_Form.DataGridViewTag.Product.ToString();

            if (hasChanges && IsThisBeingUsedByDataGridView(selectedRow.Cells[product ? 1 : 0].Value.ToString())
                && selectedTag != MainMenu_Form.SelectedOption.ItemsInPurchase.ToString())
            {

                MainMenu_Form.Instance.UpdateTotalLabels();
                MainMenu_Form.Instance.LoadOrRefreshCharts();
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
            }

            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Modified {selectedTag} list");
        }
        private static bool IsThisBeingUsedByDataGridView(string value)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value?.ToString() == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void UpdateCategory()
        {
            Category category;
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
            {
                category = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
            }
            else
            {
                category = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[0]);
            }

            string oldName = category.Name;
            category.Name = selectedRow.Cells[0].Value.ToString();

            // Update all instances in DataGridViews
            string categoryColumn = MainMenu_Form.Column.Category.ToString();
            foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
            {
                if (row.Cells[categoryColumn].Value.ToString() == oldName)
                {
                    row.Cells[categoryColumn].Value = category.Name;
                }
            }

            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.Instance.Selected);
        }
        private void UpdateProduct()
        {
            Category category;
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ProductSales)
            {
                category = MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == listOfOldValues[2]);
            }
            else
            {
                category = MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == listOfOldValues[2]);
            }

            Product product = category.ProductList.FirstOrDefault(p => p.Name == listOfOldValues[1]);

            if (HasProductChanged(product))
            {
                UpdateProductDetails(product);
                UpdateProductInDataGridViews(product);
            }
        }
        private bool HasProductChanged(Product product)
        {
            return product.ProductID != selectedRow.Cells[0].Value.ToString() ||
                   product.Name != selectedRow.Cells[1].Value.ToString() ||
                   product.CountryOfOrigin != selectedRow.Cells[3].Value.ToString() ||
                   product.CompanyOfOrigin != selectedRow.Cells[4].Value.ToString();
        }
        private void UpdateProductDetails(Product product)
        {
            product.ProductID = selectedRow.Cells[0].Value.ToString();
            product.Name = selectedRow.Cells[1].Value.ToString();
            product.CountryOfOrigin = selectedRow.Cells[3].Value.ToString();
            product.CompanyOfOrigin = selectedRow.Cells[4].Value.ToString();
        }
        private void UpdateProductInDataGridViews(Product product)
        {
            string productColumn = MainMenu_Form.Column.Product.ToString();
            string oldName = listOfOldValues[1];
            string oldID = listOfOldValues[0];
            string oldCountry = listOfOldValues[3];
            string oldCompany = listOfOldValues[4];

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
            row.Cells[MainMenu_Form.Column.Product.ToString()].Value = product.Name;

            string idColumn = MainMenu_Form.Column.ID.ToString();
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
        private void UpdateCompany()
        {
            string company = MainMenu_Form.Instance.CompanyList.FirstOrDefault(a => a == listOfOldValues[0]);

            if (company != null)
            {
                int index = MainMenu_Form.Instance.CompanyList.IndexOf(company);
                MainMenu_Form.Instance.CompanyList[index] = selectedRow.Cells[0].Value.ToString();
            }

            string newValue = selectedRow.Cells[0].Value.ToString();
            UpdateDataGridViewRows(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.Column.Company.ToString(), company, newValue);
            UpdateDataGridViewRows(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Column.Company.ToString(), company, newValue);
        }
        private void UpdateAccountant()
        {
            string accountant = MainMenu_Form.Instance.AccountantList.FirstOrDefault(a => a == listOfOldValues[0]);

            if (accountant != null)
            {
                int index = MainMenu_Form.Instance.AccountantList.IndexOf(accountant);
                MainMenu_Form.Instance.AccountantList[index] = selectedRow.Cells[0].Value.ToString();
            }

            string newValue = selectedRow.Cells[0].Value.ToString();
            UpdateDataGridViewRows(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.Column.Accountant.ToString(), accountant, newValue);
            UpdateDataGridViewRows(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Column.Accountant.ToString(), accountant, newValue);
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
                Anchor = AnchorStyles.Top
            };
            SecondPanel.Click += CloseAllPanels;
            Controls.Add(SecondPanel);
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
                AutoSize = true
            };
            label.Click += CloseAllPanels;
            control.Controls.Add(label);

            return label;
        }
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, CustomControls.KeyPressValidation keyPressValidation, bool pressSaveButton, bool smallWidth, Control control)
        {
            Guna2TextBox textBox = new()
            {
                Location = new Point(left, 43 + CustomControls.SpaceBetweenControls),
                Height = controlHeight,
                Name = name,
                Text = text,
                ForeColor = CustomColors.Text,
                BackColor = CustomColors.ControlBack,
                Font = new Font("Segoe UI", 10),
                MaxLength = maxLength,
                FillColor = CustomColors.ControlBack,
                BorderColor = CustomColors.ControlBorder,
                BorderRadius = 3,
                Cursor = Cursors.Hand,
                ShortcutsEnabled = false,
                AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate
            };
            textBox.FocusedState.FillColor = CustomColors.ControlBack;
            textBox.HoverState.BorderColor = CustomColors.AccentBlue;
            textBox.FocusedState.BorderColor = CustomColors.AccentBlue;

            if (smallWidth)
            {
                textBox.Width = smallControlWidth;
            }
            else { textBox.Width = controlWidth; }

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
            textBox.Click += CloseAllPanels;
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
                Size = new Size(controlWidth, controlHeight),
                FillColor = CustomColors.ControlBack,
                ForeColor = CustomColors.Text,
                BorderColor = CustomColors.ControlBorder,
                BorderRadius = 3,
                Name = name,
                Value = value
            };
            gDatePicker.Click += CloseAllPanels;
            gDatePicker.HoverState.BorderColor = CustomColors.AccentBlue;
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
        public void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}