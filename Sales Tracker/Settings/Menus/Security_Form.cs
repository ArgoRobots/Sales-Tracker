using Sales_Tracker.Classes;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Security_Form : Form
    {
        // Init.
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


        // Methods
        public void UpdateControls()
        {
            EncryptFiles_CheckBox.Checked = Properties.Settings.Default.EncryptFiles;
        }
    }
}