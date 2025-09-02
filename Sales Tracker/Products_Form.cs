﻿using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Products_Form : BaseForm
    {
        private static Products_Form _instance;
        private static bool _isProgramLoading;
        private readonly MainMenu_Form.SelectedOption _oldOption;
        private readonly int _topForDataGridView;

        // Getters
        public static List<string> ThingsThatHaveChangedInFile { get; } = [];
        public static Products_Form Instance => _instance;

        // Init.
        public Products_Form() : this(false) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public Products_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            _oldOption = MainMenu_Form.Instance.Selected;
            _topForDataGridView = ShowingResultsFor_Label.Bottom + 20;
            AddSearchBoxEvents();

            _isProgramLoading = true;
            ConstructDataGridViews();
            LoadProducts();
            _isProgramLoading = false;

            ValidateCompanyTextBox();
            ThemeManager.SetThemeForForm(this);
            CheckRadioButton(checkPurchaseRadioButton);
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            PopulateTypeComboBox();
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(_purchase_DataGridView, _sale_DataGridView);
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(ProductID_TextBox);

            TextBoxManager.Attach(ProductName_TextBox);

            TextBoxManager.Attach(ProductCategory_TextBox);

            TextBoxValidation.OnlyAllowLetters(CountryOfOrigin_TextBox);
            TextBoxManager.Attach(CountryOfOrigin_TextBox);

            TextBoxManager.Attach(CompanyOfOrigin_TextBox);

            _purchase_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);
            _purchase_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);

            _sale_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);
            _sale_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);

            TextBoxManager.Attach(Search_TextBox);
        }
        private void AddSearchBoxEvents()
        {
            int searchBoxMaxHeight = 300;

            SearchBox.Attach(ProductCategory_TextBox, this, GetSearchResultsForCategory, searchBoxMaxHeight, false, false, true, true);
            ProductCategory_TextBox.TextChanged += ValidateInputs;

            SearchBox.Attach(CountryOfOrigin_TextBox, this, () => Country.CountrySearchResults, searchBoxMaxHeight, false, true, true, false);
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;

            SearchBox.Attach(CompanyOfOrigin_TextBox, this, GetSearchResultsForCompany, searchBoxMaxHeight, false, false, true, true);
            CompanyOfOrigin_TextBox.TextChanged += ValidateInputs;
        }
        private List<SearchResult> GetSearchResultsForCategory()
        {
            List<string> categoryNames = Sale_RadioButton.Checked
                ? MainMenu_Form.Instance.GetCategorySaleNames()
                : MainMenu_Form.Instance.GetCategoryPurchaseNames();

            return SearchBox.ConvertToSearchResults(categoryNames);
        }
        private List<SearchResult> GetSearchResultsForCompany()
        {
            return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.CompanyList);
        }

        private void LoadProducts()
        {
            foreach (Category category in MainMenu_Form.Instance.CategoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    Product.TypeOption type = product.ItemType ?? Product.TypeOption.Product;
                    _purchase_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin, type);
                }
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_purchase_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    Product.TypeOption type = product.ItemType ?? Product.TypeOption.Product;
                    _sale_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin, type);
                }
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_sale_DataGridView);
        }
        private void CheckRadioButton(bool selectPurchaseRadioButton)
        {
            if (selectPurchaseRadioButton)
            {
                Purchase_RadioButton.Checked = true;
            }
            else
            {
                Sale_RadioButton.Checked = true;
            }
        }
        private void SetAccessibleDescriptions()
        {
            ProductsRemaining_LinkLabel.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ForPurchase_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ForSale_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ProductID_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ProductName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ProductCategory_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            CountryOfOrigin_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            CompanyOfOrigin_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            WarningProductName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            WarningCategory_LinkLabel.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            WarningCompany_LinkLabel.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            Total_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;

            // Prevent automatic translation since we handle it manually
            Type_ComboBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }

        // Form event handlers
        private void Products_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Products_Form_Shown(object sender, EventArgs e)
        {
            _purchase_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Products_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            CustomControls.CloseAllPanels();
            MainMenu_Form.Instance.Selected = _oldOption;
        }

        // Event handlers
        private void AddProduct_Button_Click(object sender, EventArgs e)
        {
            string productID = ProductID_TextBox.Text.Trim();
            if (productID == "")
            {
                productID = ReadOnlyVariables.EmptyCell;
            }

            // Check if product ID already exists
            if (productID != ReadOnlyVariables.EmptyCell &&
                DataGridViewManager.DoesValueExistInDataGridView(_selectedDataGridView, Column.ProductID.ToString(), productID))
            {
                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat("Product already exists",
                    "The product #{0} already exists. Would you like to add this product anyways?",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.YesNo,
                    productID);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return;
                }
            }

            string name = ProductName_TextBox.Text.Trim();
            Product.TypeOption type = Type_ComboBox.SelectedIndex == 0 ? Product.TypeOption.Product : Product.TypeOption.Service;
            Product product = new(productID, name, CountryOfOrigin_TextBox.Text, CompanyOfOrigin_TextBox.Text, type);
            string category = ProductCategory_TextBox.Text;

            if (Sale_RadioButton.Checked)
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategorySaleList, category, product);
                int newRowIndex = _sale_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin, product.ItemType);
                DataGridViewManager.DataGridViewRowsAdded(_selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategoryPurchaseList, category, product);
                int newRowIndex = _purchase_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin, product.ItemType);
                DataGridViewManager.DataGridViewRowsAdded(_selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            string message = $"Added product '{name}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(ThingsThatHaveChangedInFile, 4, message);

            ProductName_TextBox.Clear();
            ProductID_TextBox.Clear();
        }
        public void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (Purchase_RadioButton.Checked)
            {
                _purchase_DataGridView.Visible = true;
                _sale_DataGridView.Visible = false;
                _selectedDataGridView = _purchase_DataGridView;
                _purchase_DataGridView.ClearSelection();
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductPurchases;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Clear();
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
                LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (Sale_RadioButton.Checked)
            {
                _sale_DataGridView.Visible = true;
                _purchase_DataGridView.Visible = false;
                _selectedDataGridView = _sale_DataGridView;
                _sale_DataGridView.ClearSelection();
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductSales;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Clear();
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
                LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);
            }
        }
        private void ProductName_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateProductNameTextBox();
            ValidateInputs(null, null);
        }
        private void CategoryWarning_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenForm(new Categories_Form(Purchase_RadioButton.Checked));
            ValidateCategoryTextBox();
        }
        private void WarningCompany_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenForm(new Companies_Form());
            ValidateCompanyTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = Search_TextBox.Text.Trim();
            bool hasVisibleRows = true;

            foreach (DataGridViewRow row in _selectedDataGridView.Rows)
            {
                row.Visible = SearchDataGridView.FilterRowByAdvancedSearch(row, searchText);
                if (row.Visible)
                {
                    hasVisibleRows = true;
                }
            }

            // Update UI based on search results
            if (hasVisibleRows && !string.IsNullOrEmpty(searchText))
            {
                LabelManager.ShowLabelWithBaseText(ShowingResultsFor_Label, searchText);
            }
            else
            {
                ShowingResultsFor_Label.Visible = false;
            }

            DataGridViewManager.UpdateAlternatingRowColors(_selectedDataGridView);
            LabelManager.ShowTotalLabel(Total_Label, _selectedDataGridView);
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Clear();
        }
        private void ProductsRemaining_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("https://argorobots.com/upgrade/index.php");
        }
        private void ForPurchase_Label_Click(object sender, EventArgs e)
        {
            Purchase_RadioButton.Checked = true;
        }
        private void ForSale_Label_Click(object sender, EventArgs e)
        {
            Sale_RadioButton.Checked = true;
        }

        // Products remaining
        private int GetProductsRemaining()
        {
            return 10 - _selectedDataGridView.Rows.Count;
        }
        private void SetProductsRemainingLabel()
        {
            if (Properties.Settings.Default.LicenseActivated)
            {
                ProductsRemaining_LinkLabel.Visible = false;
                return;
            }

            int productsRemaining = GetProductsRemaining();
            if (productsRemaining <= 0)
            {
                AddProduct_Button.Enabled = false;
                ProductsRemaining_LinkLabel.ForeColor = CustomColors.AccentRed;
            }
            else
            {
                if (IsProductValid())
                {
                    AddProduct_Button.Enabled = true;
                }
                ProductsRemaining_LinkLabel.ForeColor = CustomColors.Text;
            }

            ProductsRemaining_LinkLabel.Text = $"{productsRemaining} products remaining. Upgrade now";
            ProductsRemaining_LinkLabel.LinkArea = new LinkArea(ProductsRemaining_LinkLabel.Text.IndexOf("Upgrade now"), "Upgrade now".Length);
            ProductsRemaining_LinkLabel.Left = Width - 70 - ProductsRemaining_LinkLabel.Width;
            AddProduct_Label.Focus();
        }

        // Validate product name
        public void ValidateProductNameTextBox()
        {
            if (IsProductValid())
            {
                AddProduct_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(ProductName_TextBox);
                ShowProductNameWarning();
            }
            else
            {
                AddProduct_Button.Enabled = true;
                CustomControls.SetGTextBoxToValid(ProductName_TextBox);
                HideProductNameWarning();
            }
        }
        private bool IsProductValid()
        {
            List<Category> categories = Sale_RadioButton.Checked
                ? MainMenu_Form.Instance.CategorySaleList
                : MainMenu_Form.Instance.CategoryPurchaseList;

            string category = ProductCategory_TextBox.Text.Trim();

            return MainMenu_Form.DoesProductExistInCategory(ProductName_TextBox.Text, CompanyOfOrigin_TextBox.Text, categories, category);
        }
        private void ShowProductNameWarning()
        {
            WarningProductName_PictureBox.Visible = true;
            WarningProductName_Label.Visible = true;
        }
        private void HideProductNameWarning()
        {
            WarningProductName_PictureBox.Visible = false;
            WarningProductName_Label.Visible = false;
        }

        // Validate category name
        private void ValidateCategoryTextBox()
        {
            List<Category> categoryList = Sale_RadioButton.Checked
                ? MainMenu_Form.Instance.CategorySaleList
                : MainMenu_Form.Instance.CategoryPurchaseList;

            if (categoryList.Count == 0)
            {
                ShowCategoryWarning();
            }
            else
            {
                HideCategoryWarning();
            }
        }
        private void ShowCategoryWarning()
        {
            WarningCategory_PictureBox.Visible = true;
            WarningCategory_LinkLabel.Visible = true;
        }
        private void HideCategoryWarning()
        {
            WarningCategory_PictureBox.Visible = false;
            WarningCategory_LinkLabel.Visible = false;
        }

        // Validate company name
        private void ValidateCompanyTextBox()
        {
            if (MainMenu_Form.Instance.CompanyList.Count == 0)
            {
                ShowCompanyWarning();
            }
            else
            {
                HideCompanyWarning();
            }
        }
        private void ShowCompanyWarning()
        {
            WarningCompany_PictureBox.Visible = true;
            WarningCompany_LinkLabel.Visible = true;
        }
        private void HideCompanyWarning()
        {
            WarningCompany_PictureBox.Visible = false;
            WarningCompany_LinkLabel.Visible = false;
        }

        // DataGridView properties
        public enum Column
        {
            ProductID,
            ProductName,
            ProductCategory,
            CountryOfOrigin,
            CompanyOfOrigin,
            Type
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.ProductID, "Product ID" },
            { Column.ProductName, "Product name" },
            { Column.ProductCategory, "Product category" },
            { Column.CountryOfOrigin, "Country of origin" },
            { Column.CompanyOfOrigin, "Company of origin" },
            { Column.Type, "Type" }
        };
        private Guna2DataGridView _purchase_DataGridView, _sale_DataGridView, _selectedDataGridView;
        public Guna2DataGridView Purchase_DataGridView => _purchase_DataGridView;
        public Guna2DataGridView Sale_DataGridView => _sale_DataGridView;

        // DataGridView methods
        private void ConstructDataGridViews()
        {
            _purchase_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_purchase_DataGridView, "purchases_DataGridView", ColumnHeaders, null, this);
            _purchase_DataGridView.RowsAdded += DataGridView_RowsChanged;
            _purchase_DataGridView.RowsRemoved += DataGridView_RowsChanged;
            _purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _purchase_DataGridView.Location = new Point((ClientSize.Width - _purchase_DataGridView.Width) / 2, _topForDataGridView);
            _purchase_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;

            _sale_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_sale_DataGridView, "sales_DataGridView", ColumnHeaders, null, this);
            _sale_DataGridView.RowsAdded += DataGridView_RowsChanged;
            _sale_DataGridView.RowsRemoved += DataGridView_RowsChanged;
            _sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _sale_DataGridView.Location = new Point((ClientSize.Width - _sale_DataGridView.Width) / 2, _topForDataGridView);
            _sale_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;
            ThemeManager.CustomizeScrollBar(_sale_DataGridView);
        }
        private void DataGridView_RowsChanged(object sender, EventArgs e)
        {
            if (_isProgramLoading) { return; }
            SetProductsRemainingLabel();
        }
        private void CenterSelectedDataGridView()
        {
            if (_selectedDataGridView == null) { return; }
            _selectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - _topForDataGridView - 70);
            _selectedDataGridView.Location = new Point((ClientSize.Width - _selectedDataGridView.Width) / 2, _topForDataGridView);
        }

        // Methods
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(ProductName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ProductCategory_TextBox.Text) && ProductCategory_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(CountryOfOrigin_TextBox.Text) && CountryOfOrigin_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(CompanyOfOrigin_TextBox.Text) && CompanyOfOrigin_TextBox.Tag.ToString() != "0";

            if (!Properties.Settings.Default.LicenseActivated)
            {
                allFieldsFilled &= GetProductsRemaining() > 0;
            }
            AddProduct_Button.Enabled = allFieldsFilled;
        }
        public void PopulateTypeComboBox()
        {
            int index = Type_ComboBox.SelectedIndex;

            // Clear and repopulate with translated text
            Type_ComboBox.Items.Clear();

            Type_ComboBox.Items.Add(LanguageManager.TranslateString("Product"));
            Type_ComboBox.Items.Add(LanguageManager.TranslateString("Service"));

            // Restore selection
            Type_ComboBox.SelectedIndex = index != -1 ? index : 0;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}