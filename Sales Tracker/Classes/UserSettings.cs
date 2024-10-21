using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    internal class UserSettings
    {
        public static void SaveUserSettings(bool includeGeneralForm)
        {
            // Check if language changed
            if (Properties.Settings.Default.Language != General_Form.Instance.Language_TextBox.Text)
            {
                UpdateLanguage(includeGeneralForm);
            }

            // Check if debug info setting changed
            if (Properties.Settings.Default.ShowDebugInfo != General_Form.Instance.ShowDebugInfo_CheckBox.Checked)
            {
                Properties.Settings.Default.ShowDebugInfo = General_Form.Instance.ShowDebugInfo_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the debug info setting");
            }

            // Check if anonymous information setting changed
            if (Properties.Settings.Default.SendAnonymousInformation != General_Form.Instance.SendAnonymousInformation_CheckBox.Checked)
            {
                Properties.Settings.Default.SendAnonymousInformation = General_Form.Instance.SendAnonymousInformation_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the anonymous information setting");
            }

            // Check if purchase receipts setting changed
            if (Properties.Settings.Default.PurchaseReceipts != General_Form.Instance.PurchaseReceipts_CheckBox.Checked)
            {
                Properties.Settings.Default.PurchaseReceipts = General_Form.Instance.PurchaseReceipts_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the purchase receipts setting");
            }

            // Check if sales receipts setting changed
            if (Properties.Settings.Default.SalesReceipts != General_Form.Instance.SalesReceipts_CheckBox.Checked)
            {
                Properties.Settings.Default.SalesReceipts = General_Form.Instance.SalesReceipts_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the sales receipts setting");
            }

            // Check if currency changed
            string oldCurrency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            if (oldCurrency != General_Form.Instance.Currency_TextBox.Text)
            {
                UpdateCurrency(oldCurrency);
            }

            // Check if file encryption setting changed
            if (Properties.Settings.Default.EncryptFiles != Security_Form.Instance.EncryptFiles_CheckBox.Checked)
            {
                Properties.Settings.Default.EncryptFiles = Security_Form.Instance.EncryptFiles_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed file encryption setting");
            }
        }
        private static void UpdateLanguage(bool includeGeneralForm)
        {
            Properties.Settings.Default.Language = General_Form.Instance.Language_TextBox.Text;

            // Add all open forms
            List<Control> controlsList = [MainMenu_Form.Instance];

            if (includeGeneralForm)
            {
                controlsList.AddRange(
                [
                    Settings_Form.Instance,
                    General_Form.Instance,
                    Security_Form.Instance,
                    Updates_Form.Instance
                ]);
            }
            if (Tools.IsFormOpen(typeof(Log_Form)))
            {
                controlsList.Add(Log_Form.Instance);
            }

            // Add UI panels
            List<Control> panelsList = MainMenu_Form.GetMenus().Cast<Control>().ToList();
            controlsList.AddRange(panelsList);

            // Add other controls
            controlsList.Add(CustomControls.ControlsDropDown_Button);

            // Set the language
            foreach (Control control in controlsList)
            {
                LanguageManager.UpdateLanguageForControl(control);
            }

            // Remove previous messages that mention language changes
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains("Changed the language to"));

            // Add the new language change message
            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the language to {Properties.Settings.Default.Language}");
        }
        private static void UpdateCurrency(string oldCurrency)
        {
            string newCurrency = General_Form.Instance.Currency_TextBox.Text;
            DataFileManager.SetValue(DataFileManager.AppDataSettings.DefaultCurrencyType, newCurrency);
            MainMenu_Form.CurrencySymbol = Currency.GetSymbol();

            MainMenu_Form.IsProgramLoading = true;

            UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Purchases_DataGridView);
            UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Sales_DataGridView);

            DataGridViewManager.UpdateAllRows(MainMenu_Form.Instance.Purchases_DataGridView);
            DataGridViewManager.UpdateAllRows(MainMenu_Form.Instance.Sales_DataGridView);
            MainMenu_Form.Instance.LoadCharts();
            MainMenu_Form.Instance.UpdateTotals();

            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.SelectedOption.Sales);

            MainMenu_Form.IsProgramLoading = false;

            // Remove previous messages that mention currency changes
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains("Changed the currency from"));

            // Add the new currency change message
            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the currency from {oldCurrency} to {newCurrency}");
        }
        private static void UpdateCurrencyValuesInGridView(Guna2DataGridView dataGridView)
        {
            // Get the current exchange rate from USD to the default currency
            string currentCurrency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            string currentDate = Tools.FormatDate(DateTime.Now);
            decimal exchangeRateToDefault = Currency.GetExchangeRate("USD", currentCurrency, currentDate);
            if (exchangeRateToDefault == -1) { return; }

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

                    DataGridViewManager.UpdateRowWithNoItems(row);
                }
                else if (row.Tag is (List<string> itemList, TagData tagData1))
                {
                    string lastItem = null;
                    if (itemList.Last().StartsWith(ReadOnlyVariables.Receipt_text))
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
                    row.Tag = (valuesList, tagData1);
                }
            }
        }
        public static void ResetAllToDefault()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            DataFileManager.SetValue(DataFileManager.AppDataSettings.DefaultCurrencyType, "USD");

            General_Form.Instance.UpdateControls();
            Security_Form.Instance.UpdateControls();
        }
    }
}