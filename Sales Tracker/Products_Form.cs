using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Products_Form : Form
    {
        // Properties
        private static List<string> _thingsThatHaveChangedInFile = [];
        private static Products_Form _instance;

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile
        {
            get => _thingsThatHaveChangedInFile;
            private set => _thingsThatHaveChangedInFile = value;
        }
        public static Products_Form Instance
        {
            get => _instance;
        }

        // Init.
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Products_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            ConstructDataGridViews();
            LoadProducts();
            CheckRadioButton(checkPurchaseRadioButton);
            ValidateCompanyTextBox();
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
        }
        private void AddEventHandlersToTextBoxes()
        {
            ProductID_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            ProductID_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            ProductID_TextBox.KeyDown += UI.TextBox_KeyDown;

            ProductName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            ProductName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            ProductName_TextBox.KeyDown += UI.TextBox_KeyDown;

            ProductCategory_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            ProductCategory_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            ProductCategory_TextBox.KeyDown += UI.TextBox_KeyDown;

            CountryOfOrigin_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            CountryOfOrigin_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            CountryOfOrigin_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            CountryOfOrigin_TextBox.KeyDown += UI.TextBox_KeyDown;

            CompanyOfOrigin_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            CompanyOfOrigin_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            CompanyOfOrigin_TextBox.KeyDown += UI.TextBox_KeyDown;
        }
        private void AddSearchBoxEvents()
        {
            int maxHeight = 300;

            ProductCategory_TextBox.Click += (sender, e) =>
            {
                List<SearchResult> searchResults = SearchBox.ConvertToSearchResults(GetListForSearchBox());
                SearchBox.ShowSearchBox(this, ProductCategory_TextBox, searchResults, this, maxHeight);
            };
            ProductCategory_TextBox.TextChanged += (sender, e) =>
            {
                List<SearchResult> searchResults = SearchBox.ConvertToSearchResults(GetListForSearchBox());
                SearchBox.SearchTextBoxChanged(this, ProductCategory_TextBox, searchResults, this, maxHeight);
            };
            ProductCategory_TextBox.TextChanged += ValidateInputs;
            ProductCategory_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ProductCategory_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductCategory_TextBox, this, AddProduct_Label, e); };

            CountryOfOrigin_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, CountryOfOrigin_TextBox, Country.countries, this, maxHeight); };
            CountryOfOrigin_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, CountryOfOrigin_TextBox, Country.countries, this, maxHeight); };
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;
            CountryOfOrigin_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            CountryOfOrigin_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(CountryOfOrigin_TextBox, this, AddProduct_Label, e); };

            CompanyOfOrigin_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, CompanyOfOrigin_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, maxHeight); };
            CompanyOfOrigin_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, CompanyOfOrigin_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.companyList), this, maxHeight); };
            CompanyOfOrigin_TextBox.TextChanged += ValidateInputs;
            CompanyOfOrigin_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            CompanyOfOrigin_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(CompanyOfOrigin_TextBox, this, AddProduct_Label, e); };
        }
        private List<string> GetListForSearchBox()
        {
            if (Sale_RadioButton.Checked)
            {
                return MainMenu_Form.Instance.GetProductCategorySaleNames();
            }
            else
            {
                return MainMenu_Form.Instance.GetProductCategoryPurchaseNames();
            }
        }
        private void LoadProducts()
        {
            foreach (Category category in MainMenu_Form.Instance.categoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    Purchases_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin);
                }
            }
            Tools.ScrollToTopOfDataGridView(Purchases_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.categorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    Sales_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin, product.CompanyOfOrigin);
                }
            }
            Tools.ScrollToTopOfDataGridView(Sales_DataGridView);
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

        // Form event handlers
        private void Products_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Products_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void AddProduct_Button_Click(object sender, EventArgs e)
        {
            // Check if product ID already exists
            string productID = ProductID_TextBox.Text.Trim();
            if (productID != "-" && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.selectedDataGridView, Column.ProductID.ToString(), productID))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", $"The product #{productID} already exists. Would you like to add this product anyways?", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

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
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.categorySaleList, category, product);
                int newRowIndex = Sales_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.categoryPurchaseList, category, product);
                int newRowIndex = Purchases_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin, product.CompanyOfOrigin);
                MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
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
                Controls.Add(Purchases_DataGridView);
                Controls.Remove(Sales_DataGridView);
                MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductPurchases;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Text = "";
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (Sale_RadioButton.Checked)
            {
                Controls.Add(Sales_DataGridView);
                Controls.Remove(Purchases_DataGridView);
                MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ProductSales;
                CenterSelectedDataGridView();
                ProductCategory_TextBox.Text = "";
                ValidateCategoryTextBox();
                SetProductsRemainingLabel();
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
                HideShowingResultsForLabel();
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
            return 10 - MainMenu_Form.Instance.selectedDataGridView.Rows.Count;
        }
        private void SetProductsRemainingLabel()
        {
            if (MainMenu_Form.IsFullVersion)
            {
                Controls.Remove(ProductsRemaining_LinkLabel);
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
                categories = MainMenu_Form.Instance.categorySaleList;
            }
            else
            {
                categories = MainMenu_Form.Instance.categoryPurchaseList;
            }

            if (MainMenu_Form.DoesProductExist(ProductName_TextBox.Text, categories))
            {
                AddProduct_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(ProductName_TextBox);
                ShowProductNameWarning();
            }
            else
            {
                AddProduct_Button.Enabled = true;
                UI.SetGTextBoxToValid(ProductName_TextBox);
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
                categoryList = MainMenu_Form.Instance.categorySaleList;
            }
            else
            {
                categoryList = MainMenu_Form.Instance.categoryPurchaseList;
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
            if (MainMenu_Form.Instance.companyList.Count == 0)
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
            ShowingResultsFor_Label.Left = (Width - ShowingResultsFor_Label.Width) / 2 - UI.spaceToOffsetFormNotCenter;
            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
        }

        // DataGridView
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
        public Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const int topForDataGridView = 380;
        private void ConstructDataGridViews()
        {
            Size size = new(840, 270);

            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size, ColumnHeaders);
            Purchases_DataGridView.RowsAdded += Sales_DataGridView_RowsChanged;
            Purchases_DataGridView.RowsRemoved += Sales_DataGridView_RowsChanged;
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, topForDataGridView);
            Purchases_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size, ColumnHeaders);
            Sales_DataGridView.RowsAdded += Sales_DataGridView_RowsChanged;
            Sales_DataGridView.RowsRemoved += Sales_DataGridView_RowsChanged;
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, topForDataGridView);
            Sales_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Product;
        }
        void Sales_DataGridView_RowsChanged(object sender, EventArgs e)
        {
            SetProductsRemainingLabel();
        }
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(Width - 80, Height - topForDataGridView - 85);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - UI.spaceToOffsetFormNotCenter, topForDataGridView);
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
        public void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox(this);
            MainMenu_Form.Instance.CloseRightClickPanels();
        }
    }
}