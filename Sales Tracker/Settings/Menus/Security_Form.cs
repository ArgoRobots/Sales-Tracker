using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Passwords;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Security_Form : Form
    {
        // Properties
        private static Security_Form _instance;

        // Getters
        public static Security_Form Instance => _instance;

        // Init.
        public Security_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            UpdateControls();
            SetPasswordButton();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }
        private void SetAccessibleDescriptions()
        {
            EncryptFiles_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
        }

        // Form event handlers
        private void Security_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void EncryptFiles_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!EncryptFiles_CheckBox.Checked)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    "Argo Sales Tracker",
                    "Disabling this feature will make your data vulnerable to unauthorized access. Are you sure you want to disable this feature?",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    EncryptFiles_CheckBox.Checked = true;
                }
            }
        }
        private void EncryptFiles_Label_Click(object sender, EventArgs e)
        {
            EncryptFiles_CheckBox.Checked = !EncryptFiles_CheckBox.Checked;
        }
        private void AddPassword_Button_Click(object sender, EventArgs e)
        {
            string password = PasswordManager.Password;
            if (password == null)
            {
                new AddPassword_Form().ShowDialog();
            }
            else
            {
                new PasswordManager_Form().ShowDialog();
            }
            SetPasswordButton();
        }

        // Methods
        private void SetPasswordButton()
        {
            string password = PasswordManager.Password;
            if (password == null)
            {
                AddPassword_Button.Text = "Add password protection";
            }
            else
            {
                AddPassword_Button.Text = "Manage password";
            }
        }
        public void UpdateControls()
        {
            EncryptFiles_CheckBox.Checked = Properties.Settings.Default.EncryptFiles;
        }
    }
}