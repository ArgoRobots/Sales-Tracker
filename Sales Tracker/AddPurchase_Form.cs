using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class AddPurchase_Form : Form
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        public AddPurchase_Form()
        {
            InitializeComponent();
            AddEventHandlersToTextBoxes();
            UpdateTheme();
        }
        private void AddEventHandlersToTextBoxes()
        {
            BuyerName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            BuyerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Quantity_TextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
            Quantity_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            PricePerUnit_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            PricePerUnit_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Shipping_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Shipping_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;

            Tax_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Tax_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
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
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.Options.Sales)
            {
                MainMenu_Form.Instance.Purchases_Button.PerformClick();
            }

            // Retrieve the input values
            string purchaseID = PurchaseID_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string itemName = ItemName_TextBox.Text;
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit + shipping + tax;

            MainMenu_Form.Instance.selectedDataGridView.Rows.Add(purchaseID, buyerName, itemName, date, quantity, pricePerUnit, shipping, tax, totalPrice);
            thingsThatHaveChangedInFile.Add(ItemName_TextBox.Text);
        }
        private void ImportAmazon_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportEbay_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportBLANK_Button_Click(object sender, EventArgs e)
        {

        }


        // Functions
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(PurchaseID_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(BuyerName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(ItemName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Quantity_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PricePerUnit_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   Date_DateTimePicker.Value != null;

            AddPurchase_Button.Enabled = allFieldsFilled;
        }
    }
}