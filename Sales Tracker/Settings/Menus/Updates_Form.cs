using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : Form
    {
        // Properties
        private static Updates_Form _instance;

        // Getters and setters
        public static Updates_Form Instance
        {
            get => _instance;
        }

        // Init.
        public Updates_Form()
        {
            InitializeComponent();
            _instance = this;

            UpdateTheme();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            CheckForUpdates_Button.FillColor = CustomColors.accent_blue;
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