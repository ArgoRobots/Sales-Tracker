using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Products_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        // Init
        public static Products_Form Instance { get; private set; }
        private readonly MainMenu_Form.Options oldOption;
        Guna2DataGridView oldselectedDataGridView;
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
            Theme.SetThemeForForm(this);
            Purchase_RadioButton.Checked = true;
        }
        private void AddEventHandlersToTextBoxes()
        {
            ProductName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            SellerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            CountryOfOrigin_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            CountryOfOrigin_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
        }
        private void AddSearchBoxEvents()
        {
            int maxHeight = 150;
            ItemCategory_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, ItemCategory_TextBox, GetListForSearchBox(), this, maxHeight); };
            ItemCategory_TextBox.TextChanged += (sender, e) => { SearchBox.VariableTextBoxChanged(this, ItemCategory_TextBox, GetListForSearchBox(), this, AddProduct_Button, maxHeight); };
            ItemCategory_TextBox.TextChanged += ValidateInputs;
            ItemCategory_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ItemCategory_TextBox.KeyDown += (sender, e) => { SearchBox.VariableTextBox_KeyDown(ItemCategory_TextBox, this, AddProduct_Label, e); };
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

            foreach (Category category in MainMenu_Form.Instance.productCategoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    Purchases_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
                }
            }
            foreach (Category category in MainMenu_Form.Instance.productCategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    Sales_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
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
            SellerName,
            CountryOfOrigin
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.ProductName, "Product name" },
            { Columns.SellerName, "Seller name" },
            { Columns.CountryOfOrigin, "Country of origin" },
        };
        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte heightForDataGridView = 230;
        private void ConstructDataGridViews()
        {
            Size size = new(840, 280);
            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Purchases_DataGridView, ColumnHeaders);
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, heightForDataGridView);

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Sales_DataGridView, ColumnHeaders);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, heightForDataGridView);
        }

        // Event handlers
        private void AddProduct_Button_Click(object sender, EventArgs e)
        {
            Product product = new(ProductName_TextBox.Text, SellerName_TextBox.Text, CountryOfOrigin_TextBox.Text);
            if (Purchase_RadioButton.Checked)
            {
                Purchases_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.productCategoryPurchaseList, ItemCategory_TextBox.Text, product);
            }
            else
            {
                Sales_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
                MainMenu_Form.AddProductToCategoryByName(MainMenu_Form.Instance.productCategorySaleList, ItemCategory_TextBox.Text, product);
            }
            thingsThatHaveChangedInFile.Add(ProductName_TextBox.Text);
            Log.Write(3, $"Added product '{ProductName_TextBox.Text}'");
        }

        // Functions
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(ProductName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(SellerName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(CountryOfOrigin_TextBox.Text);

            AddProduct_Button.Enabled = allFieldsFilled;
        }
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2, heightForDataGridView);
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            Controls.Remove(Sales_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductPurchases;
            CenterSelectedDataGridView();
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            Controls.Remove(Purchases_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.ProductSales;
            CenterSelectedDataGridView();
        }
    }
}