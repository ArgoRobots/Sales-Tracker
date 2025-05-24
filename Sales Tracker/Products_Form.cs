using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Products_Form : Form
    {
        // Properties
        private static readonly List<string> _thingsThatHaveChangedInFile = [];
        private static Products_Form _instance;
        private static bool isProgramLoading;

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;
        public static Products_Form Instance => _instance;

        // Init.
        private readonly MainMenu_Form.SelectedOption oldOption;
        public Products_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            AddSearchBoxEvents();

            isProgramLoading = true;
            ConstructDataGridViews();
            LoadProducts();
            isProgramLoading = false;

            ValidateCompanyTextBox();
            ThemeManager.SetThemeForForm(this);
            CheckRadioButton(checkPurchaseRadioButton);
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(purchase_DataGridView, sale_DataGridView);
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

            purchase_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, purchase_DataGridView);
            purchase_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, purchase_DataGridView);

            sale_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, sale_DataGridView);
            sale_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, sale_DataGridView);

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
                    purchase_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin);
                }
            }
            DataGridViewManager.ScrollToTopOfDataGridView(purchase_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    sale_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin);
                }
            }
            DataGridViewManager.ScrollToTopOfDataGridView(sale_DataGridView);
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
        }

        // Form event handlers
        private void Products_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Products_Form_Shown(object sender, EventArgs e)
        {
            purchase_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Products_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            CustomControls.CloseAllPanels();
            MainMenu_Form.Instance.Selected = oldOption;
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
                DataGridViewManager.DoesValueExistInDataGridView(selectedDataGridView, Column.ProductID.ToString(), productID))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Product already exists",
                    $"The product #{productID} already exists. Would you like to add this product anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return;
                }
            }

            string name = ProductName_TextBox.Text.Trim();
            Product product = new(productID, name, CountryOfOrigin_TextBox.Text, CompanyOfOrigin_TextBox.Text);
            string category = ProductCategory_TextBox.Text;

            if (Sale_RadioButton.Checked)
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategorySaleList, category, product);
                int newRowIndex = sale_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategoryPurchaseList, category, product);
                int newRowIndex = purchase_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
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
                purchase_DataGridView.Visible = true;
                sale_DataGridView.Visible = false;
                selectedDataGridView = purchase_DataGridView;
                purchase_DataGridView.ClearSelection();
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductPurchases;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Clear();
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
                LabelManager.ShowTotalLabel(Total_Label, purchase_DataGridView);
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (Sale_RadioButton.Checked)
            {
                sale_DataGridView.Visible = true;
                purchase_DataGridView.Visible = false;
                selectedDataGridView = sale_DataGridView;
                sale_DataGridView.ClearSelection();
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductSales;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Clear();
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
                LabelManager.ShowTotalLabel(Total_Label, sale_DataGridView);
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

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
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

            DataGridViewManager.UpdateAlternatingRowColors(selectedDataGridView);
            LabelManager.ShowTotalLabel(Total_Label, selectedDataGridView);
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Clear();
        }
        private void ProductsRemaining_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("https://argorobots.com/upgrade/index.html");
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
            return 10 - selectedDataGridView.Rows.Count;
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

            ProductsRemaining_LinkLabel.LinkArea = new LinkArea(0, 0);  // This fixes a rendering bug. The last letter "w" was being cut off
            ProductsRemaining_LinkLabel.Text = $"{productsRemaining} products remaining. Upgrade now";
            ProductsRemaining_LinkLabel.LinkArea = new LinkArea(ProductsRemaining_LinkLabel.Text.IndexOf("Upgrade now"), "Upgrade now".Length);
            ProductsRemaining_LinkLabel.Left = CompanyOfOrigin_TextBox.Right - ProductsRemaining_LinkLabel.Width;
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
            List<Category> categories;
            if (Sale_RadioButton.Checked)
            {
                categories = MainMenu_Form.Instance.CategorySaleList;
            }
            else
            {
                categories = MainMenu_Form.Instance.CategoryPurchaseList;
            }

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
            List<Category> categoryList;
            if (Sale_RadioButton.Checked)
            {
                categoryList = MainMenu_Form.Instance.CategorySaleList;
            }
            else
            {
                categoryList = MainMenu_Form.Instance.CategoryPurchaseList;
            }

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
            CompanyOfOrigin
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.ProductID, "Product ID" },
            { Column.ProductName, "Product name" },
            { Column.ProductCategory, "Product category" },
            { Column.CountryOfOrigin, "Country of origin" },
            { Column.CompanyOfOrigin, "Company of origin" },
        };
        private const short topForDataGridView = 380;
        private Guna2DataGridView purchase_DataGridView, sale_DataGridView, selectedDataGridView;

        // DataGridView methods
        private void ConstructDataGridViews()
        {
            Size size = new(840, 270);

            purchase_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(purchase_DataGridView, "purchases_DataGridView", size, ColumnHeaders, null, this);
            purchase_DataGridView.RowsAdded += Sale_DataGridView_RowsChanged;
            purchase_DataGridView.RowsRemoved += Sale_DataGridView_RowsChanged;
            purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            purchase_DataGridView.Location = new Point((ClientSize.Width - purchase_DataGridView.Width) / 2, topForDataGridView);
            purchase_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;

            sale_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(sale_DataGridView, "sales_DataGridView", size, ColumnHeaders, null, this);
            sale_DataGridView.RowsAdded += Sale_DataGridView_RowsChanged;
            sale_DataGridView.RowsRemoved += Sale_DataGridView_RowsChanged;
            sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            sale_DataGridView.Location = new Point((ClientSize.Width - sale_DataGridView.Width) / 2, topForDataGridView);
            sale_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;
            ThemeManager.CustomizeScrollBar(sale_DataGridView);
        }
        private void Sale_DataGridView_RowsChanged(object sender, EventArgs e)
        {
            if (isProgramLoading) { return; }
            SetProductsRemainingLabel();
        }
        private void CenterSelectedDataGridView()
        {
            if (selectedDataGridView == null) { return; }
            selectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            selectedDataGridView.Location = new Point((ClientSize.Width - selectedDataGridView.Width) / 2, topForDataGridView);
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
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}