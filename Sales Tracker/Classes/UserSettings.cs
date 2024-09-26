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
            }

            Properties.Settings.Default.Save();
            ArgoCompany.SaveAll();
        }
        private static void UpdateCurrencyValuesInGridView(Guna2DataGridView dataGridView)
        {
            MainMenu_Form.Instance.isProgramLoading = true;

            // Get the current exchange rate from USD to the default currency
            string currentCurrency = Properties.Settings.Default.Currency;
            string currentDate = Tools.FormatDate(DateTime.Now);
            decimal exchangeRateToDefault = Currency.GetExchangeRate("USD", currentCurrency, currentDate);
            if (exchangeRateToDefault == -1) { return ; }

            decimal pricePerUnit, shipping, tax, fee, total;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Tag is (string, TagData tagData))
                {
                    // Convert the USD values to the current currency
                    pricePerUnit = tagData.PricePerUnitUSD * exchangeRateToDefault;
                    shipping = tagData.ShippingUSD * exchangeRateToDefault;
                    tax = tagData.TaxUSD * exchangeRateToDefault;
                    fee = tagData.FeeUSD * exchangeRateToDefault;
                    total = tagData.TotalUSD * exchangeRateToDefault;

                    // Update the row values with the converted amounts
                    row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value = pricePerUnit.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = shipping.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tax.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = fee.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value = total.ToString("N2");

                    MainMenu_Form.Instance.UpdateRowWithNoItems(row);
                }
                else if (row.Tag is (List<string> itemList, TagData tagData1))
                {
                    string lastItem = null;
                    if (itemList.Last().StartsWith(MainMenu_Form.receipt_text))
                    {
                        lastItem = itemList.Last();
                    }

                    // Convert the USD values to the default currency
                    shipping = tagData1.ShippingUSD * exchangeRateToDefault;
                    tax = tagData1.TaxUSD * exchangeRateToDefault;
                    fee = tagData1.FeeUSD * exchangeRateToDefault;

                    row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = shipping.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tax.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = fee.ToString("N2");

                    // Set the default price per unit for items in the transaction
                    List<string> valuesList = [];
                    for (int i = 0; i < itemList.Count - 1; i++)
                    {
                        string[] values = itemList[i].Split(',');

                        decimal itemQuantity = decimal.Parse(values[4]);
                        decimal itemPricePerUnitUSD = decimal.Parse(values[6]);
                        values[5] = (itemPricePerUnitUSD * exchangeRateToDefault).ToString("N2");
                        valuesList.Add(string.Join(",", values));
                    }

                    // Add the receipt file path if it exists
                    if (lastItem != null)
                    {
                        valuesList.Add(lastItem);
                    }

                    // Save
                    tagData1.DefaultCurrencyType = currentCurrency;
                    row.Tag = (valuesList, tagData1);
                    MainMenu_Form.Instance.UpdateAllRows(MainMenu_Form.Instance.Purchases_DataGridView);
                    MainMenu_Form.Instance.UpdateAllRows(MainMenu_Form.Instance.Sales_DataGridView);
                }
            }

            MainMenu_Form.Instance.isProgramLoading = false;
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