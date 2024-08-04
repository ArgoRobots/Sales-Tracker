using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class General_Form : Form
    {
        // Init.
        public static General_Form Instance { get; private set; }
        public General_Form()
        {
            InitializeComponent();
            Instance = this;

            LoadingPanel.ShowLoadingPanel(this);

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

            LoadingPanel.HideLoadingPanel(this);
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
        }
    }
}