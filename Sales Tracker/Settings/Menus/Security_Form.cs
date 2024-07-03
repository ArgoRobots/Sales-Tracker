using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Security_Form : BaseForm
    {
        public static Security_Form Instance { get; private set; }
        public Security_Form()
        {
            InitializeComponent();
            Instance = this;

            UpdateControls();
            UpdateTheme();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }

        // Form
        private void Security_form_Resize(object sender, EventArgs e)
        {
        }

        public void UpdateControls()
        {

        }
    }
}