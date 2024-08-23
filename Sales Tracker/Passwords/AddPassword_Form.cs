using Sales_Tracker.Classes;

namespace Sales_Tracker.Passwords
{
    public partial class AddPassword_Form : Form
    {
        // Init.
        public AddPassword_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowLoadingPanel(this);
            AddEventHandlersToTextBoxes();
            Theme.SetThemeForForm(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            Password_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Password_TextBox.KeyDown += UI.TextBox_KeyDown;
        }

        // Form event handlers
        private void AddPassword_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideLoadingPanel(this);
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
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
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
            CustomMessageBox.Show("Argo Sales Tracker", "Password set successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            Close();
        }
    }
}