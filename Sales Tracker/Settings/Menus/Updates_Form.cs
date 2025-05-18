using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : Form
    {
        // Properties
        private static Updates_Form _instance;

        // Getters
        public static Updates_Form Instance => _instance;

        // Init.
        public Updates_Form()
        {
            InitializeComponent();
            _instance = this;

            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            CheckForUpdates_Button.FillColor = CustomColors.AccentBlue;
            CheckForUpdates_Button.ForeColor = Color.White;
        }

        // Form event handlers
        private void Updates_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void CheckForUpdates_Button_Click(object sender, EventArgs e)
        {
        }
    }
}