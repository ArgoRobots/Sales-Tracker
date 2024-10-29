using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Products_Form : Form
    {
        // Properties
        private static readonly List<string> _thingsThatHaveChangedInFile = [];
        private static Products_Form _instance;

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;
        public static Products_Form Instance => _instance;

        // Init.
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Products_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;
            AddSearchBoxEvents();
            ConstructDataGridViews();
            LoadProducts();
            CheckRadioButton(checkPurchaseRadioButton);
            ValidateCompanyTextBox();
            Theme.SetThemeForForm(this);
            SetAccessibleDescriptions();
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(purchase_DataGridView, sale_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(ProductID_TextBox);

            TextBoxManager.Attach(ProductName_TextBox);

            TextBoxManager.Attach(ProductCategory_TextBox);

            CountryOfOrigin_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(CountryOfOrigin_TextBox);

            TextBoxManager.Attach(CompanyOfOrigin_TextBox);

            purchase_DataGridView.RowsAdded += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, purchase_DataGridView); };
            purchase_DataGridView.RowsRemoved += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, purchase_DataGridView); };

            sale_DataGridView.RowsAdded += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, sale_DataGridView); };
            sale_DataGridView.RowsRemoved += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, sale_DataGridView); };
        }
        private void AddSearchBoxEvents()
        {
            int searchBoxMaxHeight = 300;

            SearchBox.Attach(ProductCategory_TextBox, this, GetListForSearchBox, searchBoxMaxHeight);
            ProductCategory_TextBox.TextChanged += ValidateInputs;

            SearchBox.Attach(CountryOfOrigin_TextBox, this, () => Country.countries, searchBoxMaxHeight);
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;

            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.CompanyList);
            SearchBox.Attach(CompanyOfOrigin_TextBox, this, () => searchResult, searchBoxMaxHeight);
            CompanyOfOrigin_TextBox.TextChanged += ValidateInputs;
        }
        private List<SearchResult> GetListForSearchBox()
        {
            if (Sale_RadioButton.Checked)
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategorySaleNames());
            }
            else
            {
                return SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryPurchaseNames());
            }
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
            Tools.ScrollToTopOfDataGridView(purchase_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    sale_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin);
                }
            }
            Tools.ScrollToTopOfDataGridView(sale_DataGridView);
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
            ProductsRemaining_LinkLabel.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ForPurchase_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ForSale_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ProductID_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ProductName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ProductCategory_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            CountryOfOrigin_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            CompanyOfOrigin_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            WarningProductName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            WarningCategory_LinkLabel.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            WarningCompany_LinkLabel.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
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
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void AddProduct_Button_Click(object sender, EventArgs e)
        {
            // Check if product ID already exists
            string productID = ProductID_TextBox.Text.Trim();
            if (productID != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.SelectedDataGridView, Column.ProductID.ToString(), productID))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                    $"The product #{productID} already exists. Would you like to add this product anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return;
                }
            }

            string name = ProductName_TextBox.Text.Trim();
            Product product = new(ProductID_TextBox.Text.Trim(), name, CountryOfOrigin_TextBox.Text, CompanyOfOrigin_TextBox.Text);
            string category = ProductCategory_TextBox.Text;

            if (Sale_RadioButton.Checked)
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategorySaleList, category, product);
                int newRowIndex = sale_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.CategoryPurchaseList, category, product);
                int newRowIndex = purchase_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added product '{name}'");

            ProductName_TextBox.Text = "";
            ProductID_TextBox.Text = "";
        }
        public void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (Purchase_RadioButton.Checked)
            {
                purchase_DataGridView.Visible = true;
                sale_DataGridView.Visible = false;
                purchase_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = purchase_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductPurchases;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Text = "";
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
                sale_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = sale_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductSales;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Text = "";
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
            new Categories_Form(Purchase_RadioButton.Checked).ShowDialog();
            ValidateCategoryTextBox();
        }
        private void WarningCompany_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Companies_Form().ShowDialog();
            ValidateCompanyTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Tools.SearchSelectedDataGridView(Search_TextBox))
            {
                ShowShowingResultsForLabel(Search_TextBox.Text);
            }
            else
            {
                ShowingResultsFor_Label.Visible = false;
            }
        }
        private void ProductsRemaining_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("");
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
        private static int GetProductsRemaining()
        {
            return 10 - MainMenu_Form.Instance.SelectedDataGridView.Rows.Count;
        }
        private void SetProductsRemainingLabel()
        {
            if (MainMenu_Form.IsFullVersion)
            {
                ProductsRemaining_LinkLabel.Visible = false;
                return;
            }

            int productsRemaining = GetProductsRemaining();
            if (productsRemaining <= 0)
            {
                AddProduct_Button.Enabled = false;
                ProductsRemaining_LinkLabel.ForeColor = CustomColors.accent_red;
            }
            else
            {
                AddProduct_Button.Enabled = true;
                ProductsRemaining_LinkLabel.ForeColor = CustomColors.text;
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
            // Get list
            List<Category> categories;
            if (Sale_RadioButton.Checked)
            {
                categories = MainMenu_Form.Instance.CategorySaleList;
            }
            else
            {
                categories = MainMenu_Form.Instance.CategoryPurchaseList;
            }

            if (MainMenu_Form.DoesProductExist(ProductName_TextBox.Text, categories))
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

        // SearchingFor_Label
        private void ShowShowingResultsForLabel(string text)
        {
            ShowingResultsFor_Label.Text = $"Showing results for: {text}";
            ShowingResultsFor_Label.Left = (ClientSize.Width - ShowingResultsFor_Label.Width) / 2;
            Controls.Add(ShowingResultsFor_Label);
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
        private const int topForDataGridView = 380;
        private Guna2DataGridView purchase_DataGridView, sale_DataGridView;

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
            Theme.CustomizeScrollBar(sale_DataGridView);
        }
        void Sale_DataGridView_RowsChanged(object sender, EventArgs e)
        {
            SetProductsRemainingLabel();
        }
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.SelectedDataGridView == null) { return; }
            MainMenu_Form.Instance.SelectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.SelectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.SelectedDataGridView.Width) / 2, topForDataGridView);
        }

        // Methods
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(ProductID_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ProductName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ProductCategory_TextBox.Text) && ProductCategory_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(CountryOfOrigin_TextBox.Text) && CountryOfOrigin_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(CompanyOfOrigin_TextBox.Text) && CompanyOfOrigin_TextBox.Tag.ToString() != "0";

            if (!MainMenu_Form.IsFullVersion)
            {
                allFieldsFilled &= GetProductsRemaining() > 0;
            }
            AddProduct_Button.Enabled = allFieldsFilled;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}