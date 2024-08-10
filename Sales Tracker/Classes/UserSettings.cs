using Guna.UI2.WinForms;
using Sales_Tracker.Settings.Menus;

namespace Sales_Tracker.Classes
{
    internal class UserSettings
    {
        public static void SaveUserSettings()
        {
            Properties.Settings.Default.Language = General_Form.Instance.Language_ComboBox.Text;
            Properties.Settings.Default.ShowDebugInfo = General_Form.Instance.ShowDebugInfo_CheckBox.Checked;
            Properties.Settings.Default.SendAnonymousInformation = General_Form.Instance.SendAnonymousInformation_CheckBox.Checked;
            Properties.Settings.Default.PurchaseReceipts = General_Form.Instance.PurchaseReceipts_CheckBox.Checked;
            Properties.Settings.Default.SalesReceipts = General_Form.Instance.SalesReceipts_CheckBox.Checked;

            if (Properties.Settings.Default.Currency != General_Form.Instance.Currency_ComboBox.Text)
            {
                Properties.Settings.Default.Currency = General_Form.Instance.Currency_ComboBox.Text;
                MainMenu_Form.CurrencySymbol = Currency.GetSymbol(Properties.Settings.Default.Currency);

                UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Purchases_DataGridView);
                UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Sales_DataGridView);
            }

            if (Properties.Settings.Default.EncryptFiles != Security_Form.Instance.EncryptFiles_CheckBox.Checked)
            {
                Properties.Settings.Default.EncryptFiles = Security_Form.Instance.EncryptFiles_CheckBox.Checked;
                ArgoCompany.SaveAll();
            }

            Properties.Settings.Default.Save();
        }
        private static void UpdateCurrencyValuesInGridView(Guna2DataGridView dataGridView)
        {
            // Get the current exchange rate from USD to the default currency
            string currentCurrency = Properties.Settings.Default.Currency;
            string currentDate = Tools.FormatDate(DateTime.Now);
            decimal exchangeRate = Currency.GetExchangeRate("USD", currentCurrency, currentDate);

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Tag is (List<string>, MainMenu_Form.TagData tagData))
                {
                    // Convert the USD values to the current currency
                    decimal pricePerUnit = tagData.PricePerUnitUSD * exchangeRate;
                    decimal shipping = tagData.ShippingUSD * exchangeRate;
                    decimal tax = tagData.TaxUSD * exchangeRate;
                    decimal fee = tagData.FeeUSD * exchangeRate;
                    decimal totalPrice = tagData.TotalPriceUSD * exchangeRate;

                    // Update the row values with the converted amounts using enum-based column access
                    row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value = pricePerUnit.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = shipping.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tax.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = fee.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value = totalPrice.ToString("N2");
                }
                else
                {
                    Log.Write(0, "Row tag does not contain the expected data. Cannot convert the currency.");
                }
            }
        }
        public static void ResetAllToDefault()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            General_Form.Instance.UpdateControls();
            Security_Form.Instance.UpdateControls();
        }
    }
}