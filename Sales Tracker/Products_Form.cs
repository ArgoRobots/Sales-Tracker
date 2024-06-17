using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class Products_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        public static Products_Form Instance { get; private set; }
        // Init
        private readonly MainMenu_Form.Options oldOption;
        private readonly Guna2DataGridView oldselectedDataGridView;
        public Products_Form()
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldselectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            ConstructDataGridViews();
            LoadProducts();
            Purchase_RadioButton.Checked = true;
            Theme.SetThemeForForm(this);
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
            int maxHeight = 150;
            ProductCategory_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, ProductCategory_TextBox, GetListForSearchBox(), this, maxHeight); };
            ProductCategory_TextBox.TextChanged += (sender, e) => { SearchBox.VariableTextBoxChanged(this, ProductCategory_TextBox, GetListForSearchBox(), this, AddProduct_Button, maxHeight); };
            ProductCategory_TextBox.TextChanged += ValidateInputs;
            ProductCategory_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ProductCategory_TextBox.KeyDown += (sender, e) => { SearchBox.VariableTextBox_KeyDown(ProductCategory_TextBox, this, AddProduct_Label, e); };
        }
        private List<string> GetListForSearchBox()
        {
            if (Purchase_RadioButton.Checked)
            {
                return MainMenu_Form.Instance.GetProductCategoryPurchaseNames();
            }
            else
            {
                return MainMenu_Form.Instance.GetProductCategorySaleNames();
            }
        }
        private void LoadProducts()
        {
            MainMenu_Form.Instance.isDataGridViewLoading = true;

            foreach (Category category in MainMenu_Form.Instance.productCategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    Sales_DataGridView.Rows.Add(product.Name, product.Category, product.CountryOfOrigin);
                }
            }
            foreach (Category category in MainMenu_Form.Instance.productCategoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    Purchases_DataGridView.Rows.Add(product.Name, product.Category, product.CountryOfOrigin);
                }
            }
            MainMenu_Form.Instance.isDataGridViewLoading = false;
        }


        // Form
        private void Products_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldselectedDataGridView;
        }

        // DataGridView
        public enum Columns
        {
            ProductName,
            ProductCategory,
            CountryOfOrigin
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.ProductName, "Product name" },
            { Columns.ProductCategory, "Product category" },
            { Columns.CountryOfOrigin, "Country of origin" },
        };
        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte topForDataGridView = 230;
        private void ConstructDataGridViews()
        {
            Size size = new(640, 270);
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

        // Event handlers
        private void AddProduct_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Product product = new(ProductName_TextBox.Text, ProductCategory_TextBox.Text, CountryOfOrigin_TextBox.Text);
            if (Sale_RadioButton.Checked)
            {
                Sales_DataGridView.Rows.Add(product.Name, product.Category, product.CountryOfOrigin);
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.productCategorySaleList, ProductCategory_TextBox.Text, product);
            }
            else
            {
                Purchases_DataGridView.Rows.Add(product.Name, product.Category, product.CountryOfOrigin);
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.productCategoryPurchaseList, ProductCategory_TextBox.Text, product);
            }
            thingsThatHaveChangedInFile.Add(ProductName_TextBox.Text);
            Log.Write(3, $"Added product '{ProductName_TextBox.Text}'");
        }
        public void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            Controls.Remove(Sales_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductPurchases;
            CenterSelectedDataGridView();
            ProductCategory_TextBox.Text = "";
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            Controls.Remove(Purchases_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductSales;
            CenterSelectedDataGridView();
            ProductCategory_TextBox.Text = "";
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
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - 8, topForDataGridView);
        }
        public void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}