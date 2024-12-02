using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class General_Form : Form
    {
        // Properties
        private static General_Form _instance;

        // Getters
        public static General_Form Instance => _instance;

        // Init.
        public General_Form()
        {
            InitializeComponent();
            _instance = this;

            InitComboBoxDataSources();
            UpdateTheme();
            SetAccessibleDescription();
            LanguageManager.UpdateLanguageForControl(this);
            UpdateControls();
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void InitComboBoxDataSources()
        {
            ColorTheme_ComboBox.DataSource = Enum.GetValues(typeof(Theme.ThemeType));
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }
        private void SetAccessibleDescription()
        {
            ShowTooltips_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            Language_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            Currency_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ColorTheme_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ShowDebugInfo_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            SendAnonymousInformation_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            PurchaseReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            SalesReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            AnimateButtons_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;

            Language_TextBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
            Currency_TextBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(Language_TextBox);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(LanguageManager.GetLanguageNames());
            SearchBox.Attach(Language_TextBox, this, () => searchResult, searchBoxMaxHeight, false, false, false, false);
            Language_TextBox.TextChanged += (_, _) => ValidateInputs();

            TextBoxManager.Attach(Currency_TextBox);
            List<SearchResult> searchResult1 = SearchBox.ConvertToSearchResults(Currency.GetCurrencyTypesList());
            SearchBox.Attach(Currency_TextBox, this, () => searchResult1, searchBoxMaxHeight, false, false, false, false);
            Currency_TextBox.TextChanged += (_, _) => ValidateInputs();
        }

        // Form event handlers
        private void General_form_Shown(object sender, EventArgs e)
        {
            // Deselect controls
            General_Label.Focus();

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
            Tools.OpenLink("");
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
            Currency_TextBox.Text = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            ColorTheme_ComboBox.Text = Theme.CurrentTheme.ToString();
            ShowTooltips_CheckBox.Checked = Properties.Settings.Default.ShowTooltips;
            ShowDebugInfo_CheckBox.Checked = Properties.Settings.Default.ShowDebugInfo;
            SendAnonymousInformation_CheckBox.Checked = Properties.Settings.Default.SendAnonymousInformation;
            PurchaseReceipts_CheckBox.Checked = Properties.Settings.Default.PurchaseReceipts;
            SalesReceipts_CheckBox.Checked = Properties.Settings.Default.SaleReceipts;
            AnimateButtons_CheckBox.Checked = Properties.Settings.Default.AnimateButtons;
            ShowHasReceiptColumn_CheckBox.Checked = Properties.Settings.Default.ShowHasReceiptColumn;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}