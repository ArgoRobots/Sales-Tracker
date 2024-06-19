using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class AddSale_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        public static AddSale_Form Instance { get; private set; }
        public AddSale_Form()
        {
            InitializeComponent();
            Instance = this;

            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            Date_DateTimePicker.Value = DateTime.Now;
            Theme.SetThemeForForm(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            SaleID_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            BuyerName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            BuyerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            ItemName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Quantity_TextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
            Quantity_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            PricePerUnit_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            PricePerUnit_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Shipping_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Shipping_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Tax_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Tax_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
        }
        private void AddSearchBoxEvents()
        {
            int maxHeight = 150;
            ItemName_TextBox.Click += (sender, e) => { ShowSearchBox(maxHeight); };
            ItemName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(maxHeight); };
            ItemName_TextBox.TextChanged += (sender, e) => { SearchBox.VariableTextBoxChanged(this, ItemName_TextBox, MainMenu_Form.Instance.GetProductSaleNames(), this, AddSale_Button, maxHeight); };
            ItemName_TextBox.TextChanged += ValidateInputs;
            ItemName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ItemName_TextBox.KeyDown += (sender, e) => { SearchBox.VariableTextBox_KeyDown(ItemName_TextBox, this, AddSale_Label, e); };
        }
        private void ShowSearchBox(int maxHeight)
        {
            SearchBox.ShowSearchBox(this, ItemName_TextBox, MainMenu_Form.Instance.GetProductSaleNames(), this, maxHeight);
        }

        // Event handlers
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.Options.Purchases)
            {
                MainMenu_Form.Instance.Sales_Button.PerformClick();
            }

            // Retrieve the input values
            string saleID = SaleID_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string itemName = ItemName_TextBox.Text;
            string categoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categorySaleList, itemName);
            string country = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categorySaleList, itemName);
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit + shipping + tax;

            MainMenu_Form.Instance.selectedDataGridView.Rows.Add(saleID, buyerName, itemName, categoryName, country, date, quantity, pricePerUnit, shipping, tax, totalPrice);
            thingsThatHaveChangedInFile.Add(ItemName_TextBox.Text);
            Log.Write(3, $"Added sale '{ItemName_TextBox.Text}'");
        }
        private void ImportAmazon_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportEbay_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportExcel_Button_Click(object sender, EventArgs e)
        {

        }

        // Functions
        private void ValidateInputs(object? sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(SaleID_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(BuyerName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ItemName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Quantity_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PricePerUnit_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   (int)AddSale_Button.Tag == 1;

            AddSale_Button.Enabled = allFieldsFilled;
        }
        public void CloseAllPanels(object? sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}