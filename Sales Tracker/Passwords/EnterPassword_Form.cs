using Sales_Tracker.Classes;
using Windows.Security.Credentials;

namespace Sales_Tracker.Passwords
{
    public partial class EnterPassword_Form : Form
    {
        // Init.
        public EnterPassword_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowLoadingPanel(this);
            AddEventHandlersToTextBoxes();
            InitMessageLabel();
            SetAuthenticationSupported();
            Theme.SetThemeForForm(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            Password_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Password_TextBox.KeyDown += UI.TextBox_KeyDown;
        }

        // Form event handlers
        private void EnterPassword_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideLoadingPanel(this);
        }

        // Event handlers
        private void Enter_Button_Click(object sender, EventArgs e)
        {
            CheckPassword();
        }
        private void Password_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CheckPassword();
                e.SuppressKeyPress = true;
            }
        }
        private async void WindowsHello_Button_Click(object sender, EventArgs e)
        {
            // https://stackoverflow.com/questions/78856599/how-to-add-windows-hello-in-c-sharp-net-8-winforms

            message_Label.Text = "Authorizing...";
            DisableControlsForAuthentication();

            KeyCredentialRetrievalResult result =
                await KeyCredentialManager.RequestCreateAsync("login",
                KeyCredentialCreationOption.ReplaceExisting);

            if (result.Status == KeyCredentialStatus.Success)
            {
                PasswordManager.isPasswordValid = true;
                Close();
            }
            else
            {
                EnableControlsForAuthentication();
                Controls.Remove(message_Label);
            }
        }

        // Message label
        private Label message_Label;
        private void InitMessageLabel()
        {
            message_Label = new()
            {
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.text,
                MaximumSize = new Size(Width - 80, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Top = WindowsHello_Button.Top - 30,
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            message_Label.TextChanged += Message_Label_TextChanged;
        }
        private void Message_Label_TextChanged(object sender, EventArgs e)
        {
            message_Label.Left = (Width - message_Label.Width) / 2;
            Controls.Add(message_Label);
        }

        // Methods
        private void EnableControlsForAuthentication()
        {
            Password_TextBox.Enabled = true;
            Enter_Button.Enabled = true;
            WindowsHello_Button.Enabled = true;
        }
        private void DisableControlsForAuthentication()
        {
            Password_TextBox.Enabled = false;
            Enter_Button.Enabled = false;
            WindowsHello_Button.Enabled = false;
        }
        private async void SetAuthenticationSupported()
        {
            bool supported = await KeyCredentialManager.IsSupportedAsync();
            if (supported)
            {
                WindowsHello_Button.Enabled = true;
            }
            else
            {
                message_Label.Text = "Windows Hello is not supported on this device";
            }
        }
        private void CheckPassword()
        {
            if (PasswordManager.Password == Password_TextBox.Text)
            {
                PasswordManager.isPasswordValid = true;
                Close();
            }
            else
            {
                CustomMessageBox.Show("Argo Sales Tracker", "The password is incorrect", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                Password_TextBox.Focus();
            }
        }
    }
}