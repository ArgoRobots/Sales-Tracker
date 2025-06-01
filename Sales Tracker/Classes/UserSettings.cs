using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.Theme;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages user settings, including saving, updating, and resetting.
    /// </summary>
    internal class UserSettings
    {
        public static void SaveUserSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;
            General_Form form = General_Form.Instance;

            // Handle language change
            if (settings.Language != form.Language_TextBox.Text)
            {
                UpdateLanguage();
            }

            // Handle theme change
            if (settings.ColorTheme != ThemeManager.CurrentTheme.ToString())
            {
                UpdateTheme();
            }

            // Update checkbox settings
            UpdateSetting("Show tooltips", settings.ShowTooltips, form.ShowTooltips_CheckBox.Checked,
                value => settings.ShowTooltips = value);
            UpdateSetting("Show debug info", settings.ShowDebugInfo, form.ShowDebugInfo_CheckBox.Checked,
                value => settings.ShowDebugInfo = value);
            UpdateSetting("Send anonymous information", settings.SendAnonymousInformation, form.SendAnonymousInformation_CheckBox.Checked,
                value => settings.SendAnonymousInformation = value);
            UpdateSetting("Purchase receipts", settings.PurchaseReceipts, form.PurchaseReceipts_CheckBox.Checked,
                value => settings.PurchaseReceipts = value);
            UpdateSetting("Sale receipts", settings.SaleReceipts, form.SalesReceipts_CheckBox.Checked,
                value => settings.SaleReceipts = value);
            UpdateSetting("AI search", settings.AISearchEnabled, form.EnableAISearch_CheckBox.Checked,
              value => settings.AISearchEnabled = value);
            UpdateSetting("File encryption", settings.EncryptFiles, Security_Form.Instance.EncryptFiles_CheckBox.Checked,
                value => settings.EncryptFiles = value);

            // Handle animate buttons
            if (settings.AnimateButtons != form.AnimateButtons_CheckBox.Checked)
            {
                UpdateSetting("Animate buttons", settings.AnimateButtons, form.AnimateButtons_CheckBox.Checked,
                    value => settings.AnimateButtons = value);

                Settings_Form.Instance.AnimateButtons();
                MainMenu_Form.Instance.AnimateButtons();
                if (Tools.IsFormOpen<Log_Form>())
                {
                    Log_Form.Instance.AnimateButtons();
                }
                if (Tools.IsFormOpen<Receipts_Form>())
                {
                    Receipts_Form.Instance.AnimateButtons();
                }
            }

            // Handle receipt column visibility
            if (settings.ShowHasReceiptColumn != form.ShowHasReceiptColumn_CheckBox.Checked)
            {
                UpdateSetting("Show has receipt column", settings.ShowHasReceiptColumn, form.ShowHasReceiptColumn_CheckBox.Checked,
                    value => settings.ShowHasReceiptColumn = value);
                MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();
            }

            // Handle currency change
            string oldCurrency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);
            if (oldCurrency != form.Currency_TextBox.Text)
            {
                UpdateCurrency(oldCurrency);
            }
        }
        public static void UpdateSetting(string settingName, bool currentValue, bool newValue, Action<bool> setter)
        {
            if (currentValue != newValue)
            {
                setter(newValue);
                string message = $"Changed the '{settingName}' setting";
                CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, message);
            }
        }

        /// <summary>
        /// Updates the application language setting and logs the change.
        /// The actual UI translation is handled by the async method in Settings_Form.
        /// </summary>
        private static void UpdateLanguage()
        {
            Properties.Settings.Default.Language = General_Form.Instance.Language_TextBox.Text;

            // Remove previous messages that mention language changes
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains("Changed the language to"));

            // Add the new language change message
            string fullMessage = $"Changed the language to {Properties.Settings.Default.Language}";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, fullMessage);
        }
        private static void UpdateTheme()
        {
            string selectedTheme = Properties.Settings.Default.ColorTheme;

            if (selectedTheme == ThemeManager.ThemeType.Dark.ToString())
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Dark;
            }
            else if (selectedTheme == ThemeManager.ThemeType.Light.ToString())
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Light;
            }
            else // Windows theme
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Windows;
            }

            CustomColors.SetColors();
            FormThemeManager.UpdateAllForms();
            ThemeManager.UpdateOtherControls();
            MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, $"Changed the 'color theme' setting to {selectedTheme}");
        }

        /// <summary>
        /// Updates currency settings, including recalculating values and updating data grids.
        /// </summary>
        private static void UpdateCurrency(string oldCurrency)
        {
            string newCurrency = General_Form.Instance.Currency_TextBox.Text;
            DataFileManager.SetValue(AppDataSettings.DefaultCurrencyType, newCurrency);
            MainMenu_Form.CurrencySymbol = Currency.GetSymbol();

            MainMenu_Form.IsProgramLoading = true;

            UpdateCurrencyValuesInDataGridView(MainMenu_Form.Instance.Purchase_DataGridView);
            UpdateCurrencyValuesInDataGridView(MainMenu_Form.Instance.Sale_DataGridView);

            MainMenu_Form.Instance.LoadOrRefreshMainCharts();
            MainMenu_Form.Instance.UpdateTotalLabels();

            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);

            MainMenu_Form.IsProgramLoading = false;

            // Remove previous messages that mention currency changes
            string message = "Changed the currency from";
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains(message));

            string fullMessage = $"{message} {oldCurrency} to {newCurrency}";

            // Add the new currency change message
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, fullMessage);
        }

        /// <summary>
        /// Updates currency values in a specified DataGridView based on the default currency.
        /// </summary>
        private static void UpdateCurrencyValuesInDataGridView(Guna2DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count == 0) { return; }

            string defaultCurrency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Tag is (string, TagData tagData))
                {
                    // Skip conversion if currencies match
                    if (defaultCurrency == tagData.OriginalCurrency)
                    {
                        UpdateRowWithOriginalValues(row, tagData);
                    }
                    else
                    {
                        // Only get exchange rate when needed
                        string rowDate = row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString();
                        decimal USDToDefault = Currency.GetExchangeRate("USD", defaultCurrency, rowDate);
                        if (USDToDefault == -1) { return; }

                        UpdateRowWithConvertedValues(row, tagData, USDToDefault);
                    }
                }
                else if (row.Tag is (List<string> itemList, TagData tagData1))
                {
                    // Skip conversion if currencies match
                    if (defaultCurrency == tagData1.OriginalCurrency)
                    {
                        UpdateMultiItemRowWithOriginalValues(row, tagData1, itemList);
                    }
                    else
                    {
                        // Only get exchange rate when needed
                        string rowDate = row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString();
                        decimal USDToDefault = Currency.GetExchangeRate("USD", defaultCurrency, rowDate);
                        if (USDToDefault == -1) { return; }

                        UpdateMultiItemRowWithConvertedValues(row, tagData1, itemList, USDToDefault);
                    }
                }
            }
        }
        private static void UpdateRowWithOriginalValues(DataGridViewRow row, TagData tagData)
        {
            row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value = tagData.OriginalPricePerUnit.ToString("N2");
            row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = tagData.OriginalShipping.ToString("N2");
            row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tagData.OriginalTax.ToString("N2");
            row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = tagData.OriginalFee.ToString("N2");
            row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = tagData.OriginalChargedDifference.ToString("N2");
            row.Cells[MainMenu_Form.Column.Total.ToString()].Value = tagData.OriginalChargedOrCredited.ToString("N2");
        }
        private static void UpdateRowWithConvertedValues(DataGridViewRow row, TagData tagData, decimal USDToDefault)
        {
            row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value = (tagData.PricePerUnitUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = (tagData.ShippingUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = (tagData.TaxUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = (tagData.FeeUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = (tagData.ChargedDifferenceUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Total.ToString()].Value = (tagData.ChargedOrCreditedUSD * USDToDefault).ToString("N2");
        }
        private static void UpdateMultiItemRowWithOriginalValues(DataGridViewRow row, TagData tagData, List<string> itemList)
        {
            row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = tagData.OriginalShipping.ToString("N2");
            row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = tagData.OriginalTax.ToString("N2");
            row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = tagData.OriginalFee.ToString("N2");
            row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = tagData.OriginalChargedDifference.ToString("N2");
            row.Cells[MainMenu_Form.Column.Total.ToString()].Value = tagData.OriginalChargedOrCredited.ToString("N2");

            UpdateItemList(row, itemList, useOriginalPrice: true, tagData);
        }
        private static void UpdateMultiItemRowWithConvertedValues(DataGridViewRow row, TagData tagData, List<string> itemList, decimal USDToDefault)
        {
            row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value = (tagData.ShippingUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Tax.ToString()].Value = (tagData.TaxUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Fee.ToString()].Value = (tagData.FeeUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = (tagData.ChargedDifferenceUSD * USDToDefault).ToString("N2");
            row.Cells[MainMenu_Form.Column.Total.ToString()].Value = (tagData.ChargedOrCreditedUSD * USDToDefault).ToString("N2");

            UpdateItemList(row, itemList, useOriginalPrice: false, tagData, USDToDefault);
        }
        private static void UpdateItemList(DataGridViewRow row, List<string> itemList, bool useOriginalPrice, TagData tagData, decimal USDToDefault = 1m)
        {
            bool hasReceipt = itemList.Last().StartsWith(ReadOnlyVariables.Receipt_text);
            List<string> valuesList = [];

            for (int i = 0; i < itemList.Count - (hasReceipt ? 1 : 0); i++)
            {
                string[] values = itemList[i].Split(',');

                if (useOriginalPrice)
                {
                    decimal originalItemPrice = decimal.Parse(values[5]);
                    values[5] = originalItemPrice.ToString("N2");
                }
                else
                {
                    decimal itemPricePerUnitUSD = decimal.Parse(values[6]);
                    values[5] = (itemPricePerUnitUSD * USDToDefault).ToString("N2");
                }

                valuesList.Add(string.Join(",", values));
            }

            if (hasReceipt)
            {
                valuesList.Add(itemList.Last());
            }

            row.Tag = (valuesList, tagData);
        }

        /// <summary>
        /// Resets all user settings to their default values.
        /// </summary>
        public static void ResetAllToDefault()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            General_Form.Instance.UpdateControls();
            Security_Form.Instance.UpdateControls();
        }
    }
}