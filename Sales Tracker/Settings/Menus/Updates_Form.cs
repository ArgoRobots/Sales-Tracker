using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : Form
    {
        // Init.
        public static Updates_Form Instance { get; private set; }
        public Updates_Form()
        {
            InitializeComponent();
            Instance = this;

            UpdateTheme();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            CheckForUpdates_Button.FillColor = CustomColors.accent_blue;
        }

        // Event handlers
        private void CheckForUpdates_Button_Click(object sender, EventArgs e)
        {
        }

        // Methods
        public void UpdateControls()
        {
        }
    }
}