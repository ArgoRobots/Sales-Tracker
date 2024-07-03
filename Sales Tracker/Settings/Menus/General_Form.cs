using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class General_Form : BaseForm
    {
        public static General_Form Instance { get; private set; }
        public General_Form()
        {
            InitializeComponent();
            Instance = this;

            UpdateControls();
            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            UpdateTheme();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }

        // Form
        private void General_form_Shown(object sender, EventArgs e)
        {
            // Deselect controls
            General_Label.Focus();
        }
        private void General_form_Resize(object sender, EventArgs e)
        {
            Back_Panel.Left = (Width - Back_Panel.Width) / 2;
            Back_Panel.Top = (Height - Back_Panel.Height) / 2;
        }

        public void UpdateControls()
        {
            Language_ComboBox.SelectedItem = Properties.Settings.Default.Language;
            Currency_ComboBox.Text = Properties.Settings.Default.Currency;
            ColorTheme_ComboBox.Text = Properties.Settings.Default.ColorTheme;
            ShowDebugInfo_CheckBox.Checked = Properties.Settings.Default.ShowDebugInfo;
            SendAnonymousInformation_CheckBox.Checked = Properties.Settings.Default.SendAnonymousInformation;
        }
    }
}