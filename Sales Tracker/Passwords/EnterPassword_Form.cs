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
                CustomMessageBox.Show("Windows Hello", "Login failed.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
        }

        // Methods
        private async void SetAuthenticationSupported()
        {
            bool supported = await KeyCredentialManager.IsSupportedAsync();
            if (supported)
            {
                WindowsHello_Button.Enabled = true;
            }
            else
            {
                Label label = new()
                {
                    Text = "Windows Hello is not supported on this device.",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = CustomColors.text,
                    MaximumSize = new Size(Width - 80, 80),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Top = WindowsHello_Button.Top - 30,
                    AutoSize = true,
                    Anchor = AnchorStyles.Top
                };
                Controls.Add(label);
                label.Left = (Width - label.Width) / 2;
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
            }
        }
    }
}