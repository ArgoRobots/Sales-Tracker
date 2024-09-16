using Sales_Tracker.Classes;

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

            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            ColorTheme_ComboBox.DataSource = Enum.GetValues(typeof(Theme.ThemeType));
            UpdateControls();
            UpdateTheme();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
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
            Language_ComboBox.Text = Properties.Settings.Default.Language;
            Currency_ComboBox.Text = Properties.Settings.Default.Currency;
            ColorTheme_ComboBox.Text = Theme.CurrentTheme.ToString();
            ShowDebugInfo_CheckBox.Checked = Properties.Settings.Default.ShowDebugInfo;
            SendAnonymousInformation_CheckBox.Checked = Properties.Settings.Default.SendAnonymousInformation;
            PurchaseReceipts_CheckBox.Checked = Properties.Settings.Default.PurchaseReceipts;
            SalesReceipts_CheckBox.Checked = Properties.Settings.Default.SalesReceipts;
        }
    }
}