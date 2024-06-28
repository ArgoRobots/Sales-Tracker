using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Data;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class Products_Form : BaseForm
    {
        // Properties
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        public static Products_Form Instance { get; private set; }

        // Init
        private readonly MainMenu_Form.Options oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Products_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            ConstructDataGridViews();
            LoadProducts();
            CheckRadioButton(checkPurchaseRadioButton);
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
        }
        private void AddEventHandlersToTextBoxes()
        {
            ProductName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            ProductCategory_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            CountryOfOrigin_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            CountryOfOrigin_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
        }
        private void AddSearchBoxEvents()
        {
            int maxHeight = 200;

            ProductCategory_TextBox.Click += (sender, e) =>
            {
                List<SearchBox.SearchResult> searchResults = SearchBox.ConvertToSearchResults(GetListForSearchBox());
                SearchBox.ShowSearchBox(this, ProductCategory_TextBox, searchResults, this, maxHeight);
            };
            ProductCategory_TextBox.TextChanged += (sender, e) =>
            {
                List<SearchBox.SearchResult> searchResults = SearchBox.ConvertToSearchResults(GetListForSearchBox());
                SearchBox.SearchTextBoxChanged(this, ProductCategory_TextBox, searchResults, this, AddProduct_Button, maxHeight);
            };
            ProductCategory_TextBox.TextChanged += ValidateInputs;
            ProductCategory_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ProductCategory_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductCategory_TextBox, this, AddProduct_Label, e); };

            CountryOfOrigin_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, CountryOfOrigin_TextBox, Country.countries, this, maxHeight); };
            CountryOfOrigin_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, CountryOfOrigin_TextBox, Country.countries, this, AddProduct_Button, maxHeight); };
            CountryOfOrigin_TextBox.TextChanged += ValidateInputs;
            CountryOfOrigin_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            CountryOfOrigin_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(CountryOfOrigin_TextBox, this, AddProduct_Label, e); };
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
            MainMenu_Form.Instance.isDataGridViewLoading = true;

            foreach (Category category in MainMenu_Form.Instance.categorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    Sales_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin);
                }
            }
            foreach (Category category in MainMenu_Form.Instance.categoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    Purchases_DataGridView.Rows.Add(product.ProductID, product.Name, category.Name, product.CountryOfOrigin);
                }
            }
            MainMenu_Form.Instance.isDataGridViewLoading = false;
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


        // Form
        private void Products_Form_Resize(object sender, EventArgs e)
        {
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
            CloseAllPanels(null, null);
            Product product = new(ProductID_TextBox.Text, ProductName_TextBox.Text, CountryOfOrigin_TextBox.Text);
            string category = ProductCategory_TextBox.Text;

            if (Sale_RadioButton.Checked)
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.categorySaleList, category, product);
                Sales_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin);
            }
            else
            {
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.categoryPurchaseList, category, product);
                Purchases_DataGridView.Rows.Add(product.ProductID, product.Name, category, product.CountryOfOrigin);
            }

            ProductName_TextBox.Text = "";
            thingsThatHaveChangedInFile.Add(ProductName_TextBox.Text);
            Log.Write(3, $"Added product '{ProductName_TextBox.Text}'");
            ValidateInputs(null, null);
        }
        public void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            Controls.Remove(Sales_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductPurchases;
            CenterSelectedDataGridView();
            ProductCategory_TextBox.Text = "";
            VaidateCategoryTextBox();
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            Controls.Remove(Purchases_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductSales;
            CenterSelectedDataGridView();
            ProductCategory_TextBox.Text = "";
            VaidateCategoryTextBox();
        }
        private void ProductName_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateProductNameTextBox();
        }
        private void CategoryWarning_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Categories_Form(Purchase_RadioButton.Checked).ShowDialog();
            VaidateCategoryTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.selectedDataGridView.Rows)
            {
                bool isVisible = row.Cells.Cast<DataGridViewCell>()
                                          .Any(cell => cell.Value != null && cell.Value.ToString().Contains(Search_TextBox.Text, StringComparison.OrdinalIgnoreCase));
                row.Visible = isVisible;
            }
            if (Search_TextBox.Text != "")
            {
                ShowShowingResultsForLabel(Search_TextBox.Text);
            }
            else
            {
                HideShowingResultsForLabel();
            }
        }


        // DataGridView
        public enum Columns
        {
            ProductID,
            ProductName,
            ProductCategory,
            CountryOfOrigin
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.ProductID, "Product ID" },
            { Columns.ProductName, "Product name" },
            { Columns.ProductCategory, "Product category" },
            { Columns.CountryOfOrigin, "Country of origin" },
        };
        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte topForDataGridView = 240;
        private void ConstructDataGridViews()
        {
            Size size = new(840, 270);

            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Purchases_DataGridView, ColumnHeaders);
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, topForDataGridView);
            Purchases_DataGridView.Tag = DataGridViewTags.AddProduct;

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Sales_DataGridView, ColumnHeaders);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, topForDataGridView);
            Sales_DataGridView.Tag = DataGridViewTags.AddProduct;
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

            if (MainMenu_Form.IsProductInCategory(ProductName_TextBox.Text, ProductCategory_TextBox.Text, categories))
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
        private void VaidateCategoryTextBox()
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

        // SearchingFor_Label
        private void ShowShowingResultsForLabel(string text)
        {
            ShowingResultsFor_Label.Text = $"Showing results for: {text}";
            ShowingResultsFor_Label.Left = (Width - ShowingResultsFor_Label.Width) / 2 - 8;
            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
        }


        // Functions
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(ProductName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ProductCategory_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(CountryOfOrigin_TextBox.Text);

            AddProduct_Button.Enabled = allFieldsFilled;
        }
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(Width - 55, Height - topForDataGridView - 57);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - 8, topForDataGridView);
        }
        public void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}