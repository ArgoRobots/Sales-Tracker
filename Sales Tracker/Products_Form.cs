using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Products_Form : Form
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        // Init
        public static Products_Form Instance { get; set; }
        public Products_Form()
        {
            InitializeComponent();
            Instance = this;
            AddEventHandlersToTextBoxes();
            ConstructDataGridViews();
            UpdateTheme();
            Purchase_RadioButton.Checked = true;
        }
        private void AddEventHandlersToTextBoxes()
        {
            ProductName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            SellerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            SellerName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            SellerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            CountryOfOrigin_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            CountryOfOrigin_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
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
        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView, selectedDataGridView;
        private const byte heightForDataGridView = 230;
        private void ConstructDataGridViews()
        {
            Size size = new(690, 280);
            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.Instance.LoadColumnsInDataGridView(Purchases_DataGridView, ColumnHeaders);
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, heightForDataGridView);

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.Instance.LoadColumnsInDataGridView(Sales_DataGridView, ColumnHeaders);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, heightForDataGridView);
        }
        public void UpdateTheme()
        {
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

            }
        }

        // Event handlers
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            Product product = new(ProductName_TextBox.Text, SellerName_TextBox.Text, CountryOfOrigin_TextBox.Text);
            if (Purchase_RadioButton.Checked)
            {
                Purchases_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
                MainMenu_Form.Instance.productPurchaseList.Add(product);
            }
            else
            {
                Sales_DataGridView.Rows.Add(product.ProductName, product.SellerName, product.CountryOfOrigin);
                MainMenu_Form.Instance.productSaleList.Add(product);
            }
            thingsThatHaveChangedInFile.Add(ProductName_TextBox.Text);
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
            if (selectedDataGridView == null) { return; }
            selectedDataGridView.Location = new Point((Width - selectedDataGridView.Width) / 2, heightForDataGridView);
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            CenterSelectedDataGridView();
            Controls.Remove(Sales_DataGridView);
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            CenterSelectedDataGridView();
            Controls.Remove(Purchases_DataGridView);
        }
    }
}