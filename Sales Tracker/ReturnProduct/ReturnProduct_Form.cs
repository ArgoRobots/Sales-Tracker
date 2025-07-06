using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReturnProduct
{
    public partial class ReturnProduct_Form : BaseForm
    {
        // Properties
        private readonly DataGridViewRow _transactionRow;
        private readonly bool _isPurchase;

        // Init.
        public ReturnProduct_Form(DataGridViewRow transactionRow, bool isPurchase)
        {
            _transactionRow = transactionRow;
            _isPurchase = isPurchase;
            InitializeComponent();

            LoadTransactionData();
            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
        }
        private void LoadTransactionData()
        {
            string transactionId = _transactionRow.Cells[ReadOnlyVariables.ID_column].Value.ToString();
            string productName = _transactionRow.Cells[ReadOnlyVariables.Product_column].Value.ToString();
            string categoryName = _transactionRow.Cells[ReadOnlyVariables.Category_column].Value.ToString();
            string date = _transactionRow.Cells[ReadOnlyVariables.Date_column].Value.ToString();
            string transactionType = _isPurchase ? "Purchase" : "Sale";

            TransactinDetails_Label.Text = $"{transactionType} Transaction Details:";

            TransactionInfo_Label.Text = $"Transaction ID: {transactionId}\n" +
                                         $"Product: {productName}\n" +
                                         $"Category: {categoryName}\n" +
                                         $"Date: {date}";
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
            ThemeManager.MakeGButtonBluePrimary(ProcessReturn_Button);
        }

        // Event handlers
        private void ProcessReturn_Button_Click(object sender, EventArgs e)
        {
            // Get current user ... get it from a TextBox...
            string currentUser = "user";

            // Process the return
            ReturnManager.ProcessReturn(_transactionRow,
                ReturnReason_ComboBox.SelectedItem.ToString(),
                AdditionalNotes_TextBox.Text.Trim(),
                currentUser);

            DialogResult = DialogResult.OK;
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}