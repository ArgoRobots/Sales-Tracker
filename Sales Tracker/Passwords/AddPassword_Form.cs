using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Sales_Tracker.Passwords
{
    public partial class AddPassword_Form : Form
    {
        // Init.
        public AddPassword_Form()
        {
            InitializeComponent();

            AddEventHandlersToTextBoxes();
            Theme.SetThemeForForm(this);
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Password_TextBox);
        }

        // Form event handlers
        private void AddPassword_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Password_TextBox_TextChanged(object sender, EventArgs e)
        {
            SetPassword_Button.Enabled = PasswordManager.ValidatePassword(
                LengthRequirement_Label,
                UppercaseRequirement_Label,
                NumberRequirement_Label,
                SpecialCharacterRequirement_Label,
                Password_TextBox.Text
            );
        }
        private void Password_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetPassword();
            }
        }
        private void SetPassword_Button_Click(object sender, EventArgs e)
        {
            SetPassword();
        }

        // Methods
        private void SetPassword()
        {
            PasswordManager.Password = Password_TextBox.Text;
            CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Added password");
            CustomMessageBox.Show("Password set", "Password set successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            Close();
        }
    }
}