using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class UndoReturn_Form : BaseForm
    {
        // Properties
        private readonly DataGridViewRow _transactionRow;

        // Init.
        public UndoReturn_Form(DataGridViewRow transactionRow)
        {
            _transactionRow = transactionRow;
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
            bool isPurchase = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases;
            string transactionType = isPurchase ? "Purchase" : "Sale";

            TransactinDetails_Label.Text = $"{transactionType} Transaction Details:";

            TransactionInfo_Label.Text = $"Transaction ID: {transactionId}\n" +
                                         $"Product: {productName}\n" +
                                         $"Category: {categoryName}\n" +
                                         $"Date: {date}";

            // Load return information
            (DateTime? returnDate, string returnReason, string returnedBy) = ReturnManager.GetReturnInfo(_transactionRow);

            ReturnInfo_Label.Text = $"Returned on: {returnDate?.ToString("MM/dd/yyyy HH:mm") ?? "Unknown"}\n" +
                                    $"Reason: {returnReason ?? "No reason provided"}\n" +
                                    $"Returned by: {returnedBy ?? "Unknown"}";
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
            ThemeManager.MakeGButtonBluePrimary(UndoReturn_Button);
        }

        // Event handlers
        private void UndoReturn_Button_Click(object sender, EventArgs e)
        {
            ReturnManager.UndoReturn(_transactionRow, UndoReason_TextBox.Text.Trim());

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