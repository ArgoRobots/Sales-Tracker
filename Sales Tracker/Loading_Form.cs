using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Loading_Form : Form
    {
        // Init.
        public Loading_Form(string message)
        {
            InitializeComponent();
            Theme.SetThemeForForm(this);
            LoadingPanel.ShowLoadingScreen(this, message);
        }

        // Form event handlers
        private void Loading_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            LoadingPanel.HideLoadingScreen(this);
        }
    }
}