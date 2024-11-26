using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages user settings, including saving, updating, and resetting settings.
    /// </summary>
    internal class UserSettings
    {
        /// <summary>
        /// Saves user settings, checking for changes in language, tooltip visibility, debug info, anonymous info sharing, and receipt settings.
        /// </summary>
        public static void SaveUserSettings(bool includeGeneralFormForLanguage)
        {
            if (Properties.Settings.Default.Language != General_Form.Instance.Language_TextBox.Text)
            {
                UpdateLanguage(includeGeneralFormForLanguage);
            }

            if (Properties.Settings.Default.ShowTooltips != General_Form.Instance.ShowTooltips_CheckBox.Checked)
            {
                Properties.Settings.Default.ShowTooltips = General_Form.Instance.ShowTooltips_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Show tooltips' setting");
            }

            if (Properties.Settings.Default.ShowDebugInfo != General_Form.Instance.ShowDebugInfo_CheckBox.Checked)
            {
                Properties.Settings.Default.ShowDebugInfo = General_Form.Instance.ShowDebugInfo_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Show debug info' setting");
            }

            if (Properties.Settings.Default.SendAnonymousInformation != General_Form.Instance.SendAnonymousInformation_CheckBox.Checked)
            {
                Properties.Settings.Default.SendAnonymousInformation = General_Form.Instance.SendAnonymousInformation_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Send anonymous information' setting");
            }

            if (Properties.Settings.Default.PurchaseReceipts != General_Form.Instance.PurchaseReceipts_CheckBox.Checked)
            {
                Properties.Settings.Default.PurchaseReceipts = General_Form.Instance.PurchaseReceipts_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Purchase receipts' setting");
            }

            if (Properties.Settings.Default.SaleReceipts != General_Form.Instance.SalesReceipts_CheckBox.Checked)
            {
                Properties.Settings.Default.SaleReceipts = General_Form.Instance.SalesReceipts_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Sale receipts' setting");
            }

            if (Properties.Settings.Default.AnimateButtons != General_Form.Instance.AnimateButtons_CheckBox.Checked)
            {
                Properties.Settings.Default.AnimateButtons = General_Form.Instance.AnimateButtons_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Animate buttons' setting");

                Settings_Form.Instance.AnimateButtons();
                MainMenu_Form.Instance.AnimateButtons();
                if (Tools.IsFormOpen(typeof(Log_Form)))
                {
                    Log_Form.Instance.AnimateButtons();
                }
            }

            if (Properties.Settings.Default.ShowHasReceiptColumn != General_Form.Instance.ShowHasReceiptColumn_CheckBox.Checked)
            {
                Properties.Settings.Default.ShowHasReceiptColumn = General_Form.Instance.ShowHasReceiptColumn_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the 'Show has receipt column' setting");

                MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();
            }

            string oldCurrency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            if (oldCurrency != General_Form.Instance.Currency_TextBox.Text)
            {
                UpdateCurrency(oldCurrency);
            }

            if (Properties.Settings.Default.EncryptFiles != Security_Form.Instance.EncryptFiles_CheckBox.Checked)
            {
                Properties.Settings.Default.EncryptFiles = Security_Form.Instance.EncryptFiles_CheckBox.Checked;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed 'file encryption' setting");
            }
        }

        /// <summary>
        /// Updates the application language across all open forms and UI elements.
        /// </summary>
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

            MainMenu_Form.Instance.CenterAndResizeControls();

            // Remove previous messages that mention language changes
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains("Changed the language to"));

            // Add the new language change message
            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"Changed the language to {Properties.Settings.Default.Language}");
        }

        /// <summary>
        /// Updates currency settings, including recalculating values and updating data grids.
        /// </summary>
        private static void UpdateCurrency(string oldCurrency)
        {
            string newCurrency = General_Form.Instance.Currency_TextBox.Text;
            DataFileManager.SetValue(DataFileManager.AppDataSettings.DefaultCurrencyType, newCurrency);
            MainMenu_Form.CurrencySymbol = Currency.GetSymbol();

            MainMenu_Form.IsProgramLoading = true;

            UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Purchase_DataGridView);
            UpdateCurrencyValuesInGridView(MainMenu_Form.Instance.Sale_DataGridView);

            MainMenu_Form.Instance.LoadOrRefreshMainCharts();
            MainMenu_Form.Instance.UpdateTotalLabels();

            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);

            MainMenu_Form.IsProgramLoading = false;

            // Remove previous messages that mention currency changes
            string message = "Changed the currency from";
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains(message));

            // Add the new currency change message
            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.SettingsThatHaveChangedInFile, $"{message} {oldCurrency} to {newCurrency}");
        }

        /// <summary>
        /// Updates currency values in a specified DataGridView based on the current exchange rate.
        /// </summary>
        private static void UpdateCurrencyValuesInGridView(Guna2DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count == 0) { return; }

            // Get the current exchange rate from USD to the default currency
            string currentCurrency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            decimal exchangeRateToDefault;
            decimal pricePerUnit, shipping, tax, fee, chargedDifference, chargedorCredited;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // Get exchange rate from the row's date column
                string rowDate = row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString();
                exchangeRateToDefault = Currency.GetExchangeRate("USD", currentCurrency, rowDate);
                if (exchangeRateToDefault == -1) { return; }

                if (row.Tag is (string, TagData tagData))
                {
                    // Convert the USD values to the current currency
                    pricePerUnit = tagData.PricePerUnitUSD * exchangeRateToDefault;
                    shipping = tagData.ShippingUSD * exchangeRateToDefault;
                    tax = tagData.TaxUSD * exchangeRateToDefault;
                    fee = tagData.FeeUSD * exchangeRateToDefault;
                    chargedDifference = tagData.ChargedDifferenceUSD * exchangeRateToDefault;
                    chargedorCredited = tagData.ChargedOrCreditedUSD * exchangeRateToDefault;

                    // Update the row values with the converted amounts
                    row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value = pricePerUnit.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = shipping.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tax.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = fee.ToString("N2");
                    row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = chargedDifference.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value = chargedorCredited.ToString("N2");
                }
                else if (row.Tag is (List<string> itemList, TagData tagData1))
                {
                    // Check for receipt and set offset
                    bool hasReceipt = itemList.Last().StartsWith(ReadOnlyVariables.Receipt_text);

                    // Convert the USD values to the default currency
                    shipping = tagData1.ShippingUSD * exchangeRateToDefault;
                    tax = tagData1.TaxUSD * exchangeRateToDefault;
                    fee = tagData1.FeeUSD * exchangeRateToDefault;
                    chargedDifference = tagData1.ChargedDifferenceUSD * exchangeRateToDefault;
                    chargedorCredited = tagData1.ChargedOrCreditedUSD * exchangeRateToDefault;

                    row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = shipping.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tax.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = fee.ToString("N2");
                    row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = chargedDifference.ToString("N2");
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value = chargedorCredited.ToString("N2");

                    // Set the price per unit for items in the transaction
                    List<string> valuesList = [];
                    for (int i = 0; i < itemList.Count - (hasReceipt ? 1 : 0); i++)
                    {
                        string[] values = itemList[i].Split(',');
                        decimal itemQuantity = decimal.Parse(values[4]);
                        decimal itemPricePerUnitUSD = decimal.Parse(values[6]);

                        values[5] = (itemPricePerUnitUSD * exchangeRateToDefault).ToString("N2");
                        valuesList.Add(string.Join(",", values));
                    }

                    // Add the receipt file path if it exists
                    if (hasReceipt)
                    {
                        valuesList.Add(itemList.Last());
                    }

                    // Save
                    row.Tag = (valuesList, tagData1);
                }
            }
        }

        /// <summary>
        /// Resets all user settings to their default values.
        /// </summary>
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