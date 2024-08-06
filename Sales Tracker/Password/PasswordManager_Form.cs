using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class PasswordManager_Form : Form
    {
        // Init.
        public PasswordManager_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowLoadingPanel(this);
            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private void PasswordManager_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideLoadingPanel(this);
        }
    }
}