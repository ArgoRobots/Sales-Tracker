using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : BaseForm
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


        // Form event handlers
        private void Updates_form_Resize(object sender, EventArgs e)
        {
            Back_Panel.Left = (Width - Back_Panel.Width) / 2;
            Back_Panel.Top = (Height - Back_Panel.Height) / 2;
        }
        private void CheckForUpdates_Button_Click(object sender, EventArgs e)
        {

        }

        // Methods
        public void UpdateControls()
        {

        }
    }
}