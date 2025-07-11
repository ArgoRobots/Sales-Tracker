using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class General_Form : BaseForm
    {
        // Properties
        private static General_Form _instance;

        // Getter
        public static General_Form Instance => _instance;

        // Init.
        public General_Form()
        {
            InitializeComponent();
            _instance = this;

            InitializeAdminModeControls();
            ThemeManager.SetThemeForForm(this);
            SetAccessibleDescription();
            LanguageManager.UpdateLanguageForControl(this);
            UpdateControls();
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private static string GetThemeDisplayText(ThemeManager.ThemeType theme)
        {
            return theme switch
            {
                ThemeManager.ThemeType.Light => LanguageManager.TranslateString("Light"),
                ThemeManager.ThemeType.Dark => LanguageManager.TranslateString("Dark"),
                ThemeManager.ThemeType.Windows => "Windows",  // Keep Windows untranslated
                _ => theme.ToString()
            };
        }
        private void SetAccessibleDescription()
        {
            Language_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            Currency_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ColorTheme_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ShowTooltips_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ShowDebugInfo_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            SendAnonymousInformation_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            PurchaseReceipts_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            SalesReceipts_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            AnimateButtons_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ShowHasReceiptColumn_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            EnableAISearch_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;

            Language_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Currency_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;

            // Prevent automatic translation since we handle it manually
            ColorTheme_ComboBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(Language_TextBox);
            List<SearchResult> searchResult = LanguageManager.GetLanguageSearchResults();
            SearchBox.Attach(Language_TextBox, this, () => searchResult, searchBoxMaxHeight, false, false, false, false);
            Language_TextBox.TextChanged += (_, _) => ValidateInputs();

            TextBoxManager.Attach(Currency_TextBox);
            SearchBox.Attach(Currency_TextBox, this, Currency.GetSearchResults, searchBoxMaxHeight, false, false, false, false);
            Currency_TextBox.TextChanged += (_, _) => ValidateInputs();
        }
        private void InitializeAdminModeControls()
        {
            if (!MainMenu_Form.EnableAdminMode)
            {
                return;
            }

            Guna2Button generateTranslationsButton = new()
            {
                Text = LanguageManager.TranslateString("Generate Translations"),
                Size = new Size(200, 40),
                Location = new Point((Width - 200) / 2, EnableAISearch_CheckBox.Bottom + 80),
                Anchor = AnchorStyles.Top
            };
            generateTranslationsButton.Click += GenerateTranslationsButton_Click;
            ThemeManager.MakeGButtonBlueSecondary(generateTranslationsButton);

            Controls.Add(generateTranslationsButton);
        }

        // Form event handlers
        private void General_form_Shown(object sender, EventArgs e)
        {
            General_Label.Focus();  // Deselect controls
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void ShowDebugInfo_Label_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ShowDebugInfo_CheckBox.Checked = !ShowDebugInfo_CheckBox.Checked;
        }
        private void SendAnonymousInformation_Label_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            SendAnonymousInformation_CheckBox.Checked = !SendAnonymousInformation_CheckBox.Checked;
        }
        private void PurchaseReceipts_Label_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            PurchaseReceipts_CheckBox.Checked = !PurchaseReceipts_CheckBox.Checked;
        }
        private void SalesReceipts_Label_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            SalesReceipts_CheckBox.Checked = !SalesReceipts_CheckBox.Checked;
        }
        private void MoreInformation_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenLink("https://argorobots.com/documentation/index.html#anonymous-data");
        }
        private void ExportData_Button_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Directories.AnonymousUserData_file) || AnonymousDataManager.GetUserDataCacheSize() == 0)
            {
                CustomMessageBox.Show("No user data",
                    "No user data exists. Either the setting was disabled or the cache was cleared.",
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            using SaveFileDialog dialog = new();
            dialog.FileName = $"AnonymousUsageData_{DateTime.Now:yyyyMMdd}{ArgoFiles.JsonFileExtension}";
            dialog.DefaultExt = ArgoFiles.JsonFileExtension;
            dialog.Filter = $"JSON files|*{ArgoFiles.JsonFileExtension}";
            dialog.Title = "Export user data";
            dialog.OverwritePrompt = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    AnonymousDataManager.ExportOrganizedData(dialog.FileName, true);

                    Log.Write(2, $"Exported organized anonymous usage data to '{Path.GetFileName(dialog.FileName)}'");

                    CustomMessageBox.Show("Export Successful",
                        "Successfully exported anonymous usage data",
                        CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
                }
                catch (Exception ex)
                {
                    Log.Error_AnonymousDataCollection($"Failed to export organized user data: {ex.Message}");
                }
            }
        }
        private void DeleteData_Button_Click(object sender, EventArgs e)
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Delete anonymous usage data", $"Are you sure you want to delete your anonymous usage data?",
                 CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                AnonymousDataManager.ClearUserData();

                CustomMessageBox.Show("Export Successful", "Successfully deleted anonymous usage data",
                    CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
            }
        }
        private async void GenerateTranslationsButton_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Language.ToString() != "English")
            {
                CustomMessageBox.Show("Success", "The deafult language must be set to English",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return;
            }

            try
            {
                await TranslationGenerator.GenerateAllLanguageTranslationFiles();

                CustomMessageBox.Show("Success", "Translation files generated successfully",
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", $"Error generating translations: {ex.Message}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }
        }

        // Methods
        private void ValidateInputs()
        {
            bool isValid = (Language_TextBox.Tag == null || Language_TextBox.Tag.ToString() != "0") &&
                    (Currency_TextBox.Tag == null || Currency_TextBox.Tag.ToString() != "0");

            Settings_Form.Instance.Ok_Button.Enabled = isValid;
            Settings_Form.Instance.Apply_Button.Enabled = isValid;
        }
        public void UpdateControls()
        {
            Language_TextBox.Text = Properties.Settings.Default.Language;
            Currency_TextBox.Text = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);
            PopulateThemeComboBox();

            ShowTooltips_CheckBox.Checked = Properties.Settings.Default.ShowTooltips;
            ShowDebugInfo_CheckBox.Checked = Properties.Settings.Default.ShowDebugInfo;
            SendAnonymousInformation_CheckBox.Checked = Properties.Settings.Default.SendAnonymousInformation;
            PurchaseReceipts_CheckBox.Checked = Properties.Settings.Default.PurchaseReceipts;
            SalesReceipts_CheckBox.Checked = Properties.Settings.Default.SaleReceipts;
            AnimateButtons_CheckBox.Checked = Properties.Settings.Default.AnimateButtons;
            ShowHasReceiptColumn_CheckBox.Checked = Properties.Settings.Default.ShowHasReceiptColumn;
            EnableAISearch_CheckBox.Checked = Properties.Settings.Default.AISearchEnabled;
        }
        public void PopulateThemeComboBox()
        {
            // Clear and repopulate with translated text
            ColorTheme_ComboBox.Items.Clear();

            foreach (ThemeManager.ThemeType theme in Enum.GetValues<ThemeManager.ThemeType>())
            {
                string displayText = GetThemeDisplayText(theme);
                ColorTheme_ComboBox.Items.Add(displayText);
            }

            // Restore selection
            ColorTheme_ComboBox.SelectedItem = GetThemeDisplayText(ThemeManager.CurrentTheme);
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}