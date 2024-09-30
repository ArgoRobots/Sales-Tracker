using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;

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

            LoadingPanel.ShowBlankLoadingPanel(this);

            InitComboBoxDataSources();
            UpdateTheme();
            SetDoNotTranslateControls();
            LanguageManager.UpdateLanguageForForm(this);
            UpdateControls();
        }
        private void InitComboBoxDataSources()
        {
            Language_ComboBox.DataSource = LanguageManager.GetLanguages();
            Language_ComboBox.DisplayMember = "Key";   // Display language name
            Language_ComboBox.ValueMember = "Value";   // Use language code as value

            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            ColorTheme_ComboBox.DataSource = Enum.GetValues(typeof(Theme.ThemeType));
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }
        private void SetDoNotTranslateControls()
        {
            Language_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            Currency_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ColorTheme_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ShowDebugInfo_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            SendAnonymousInformation_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            PurchaseReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            SalesReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
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
            ShowDebugInfo_CheckBox.Checked = !ShowDebugInfo_CheckBox.Checked;
        }
        private void SendAnonymousInformation_Label_Click(object sender, EventArgs e)
        {
            SendAnonymousInformation_CheckBox.Checked = !SendAnonymousInformation_CheckBox.Checked;
        }
        private void PurchaseReceipts_Label_Click(object sender, EventArgs e)
        {
            PurchaseReceipts_CheckBox.Checked = !PurchaseReceipts_CheckBox.Checked;
        }
        private void SalesReceipts_Label_Click(object sender, EventArgs e)
        {
            SalesReceipts_CheckBox.Checked = !SalesReceipts_CheckBox.Checked;
        }

        // Methods
        public void UpdateControls()
        {
            Language_ComboBox.SelectedValue = LanguageManager.GetDefaultLanguageAbbreviation();
            string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            Currency_ComboBox.Text = currency;
            ColorTheme_ComboBox.Text = Theme.CurrentTheme.ToString();
            ShowDebugInfo_CheckBox.Checked = Properties.Settings.Default.ShowDebugInfo;
            SendAnonymousInformation_CheckBox.Checked = Properties.Settings.Default.SendAnonymousInformation;
            PurchaseReceipts_CheckBox.Checked = Properties.Settings.Default.PurchaseReceipts;
            SalesReceipts_CheckBox.Checked = Properties.Settings.Default.SalesReceipts;
        }
    }
}