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
            UpdateTheme();
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
            MainMenu_Form.Instance.productsList.Add(new Product(ProductName_TextBox.Text, SellerName_TextBox.Text, CountryOfOrigin_TextBox.Text));
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
    }
}