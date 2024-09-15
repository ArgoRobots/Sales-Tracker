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
            LoadingPanel.ShowBlankLoadingPanel(this);
            AddEventHandlersToTextBoxes();
            Theme.SetThemeForForm(this);
            SetWindowsHelloControls();
        }
        private void AddEventHandlersToTextBoxes()
        {
            Password_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            TextBoxManager.Attach(Password_TextBox);
        }

        // Form event handlers
        private void EnterPassword_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
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
        private void Message_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("");
        }

        // Windows Hello
        private Guna.UI2.WinForms.Guna2Button WindowsHello_Button;
        private void ConstructWindowsHelloButton()
        {
            if (WindowsHello_Button != null) { return; }

            WindowsHello_Button = new()
            {
                Anchor = AnchorStyles.Top,
                BorderRadius = 2,
                BorderThickness = 1,
                Enabled = false,
                Font = new Font("Segoe UI", 10),
                Top = 310,
                Size = new Size(215, 50),
                Text = "Windows Hello"
            };
            WindowsHello_Button.Left = (ClientSize.Width - WindowsHello_Button.Width) / 2;
            WindowsHello_Button.Click += WindowsHello_Button_Click;
            Controls.Add(WindowsHello_Button);
            Theme.SetThemeForControl([WindowsHello_Button]);
        }
        private async void WindowsHello_Button_Click(object sender, EventArgs e)
        {
            // https://stackoverflow.com/questions/78856599/how-to-add-windows-hello-in-c-sharp-net-8-winforms

            SetMessageText("Authorizing...");
            DisableControlsForAuthentication();

            KeyCredentialRetrievalResult result =
                await KeyCredentialManager.RequestCreateAsync("login",
                KeyCredentialCreationOption.ReplaceExisting);

            if (result.Status == KeyCredentialStatus.Success)
            {
                PasswordManager.IsPasswordValid = true;
                Close();
            }
            else
            {
                EnableControlsForAuthentication();
                Controls.Remove(Message_LinkLabel);
            }
        }
        private async void SetWindowsHelloControls()
        {
            if (MainMenu_Form.IsFullVersion)
            {
                ConstructWindowsHelloButton();

                bool supported = await KeyCredentialManager.IsSupportedAsync();
                if (supported)
                {
                    WindowsHello_Button.Enabled = true;
                    Controls.Remove(Message_LinkLabel);
                }
                else
                {
                    SetMessageText("Windows Hello is not supported on this device");
                }

                Message_LinkLabel.Top = WindowsHello_Button.Top - 40;
                Height = 460;
            }
            else
            {
                SetMessageText("Upgrade now to enable Windows Hello");
                Message_LinkLabel.LinkArea = new LinkArea(Message_LinkLabel.Text.IndexOf("Upgrade now"), "Upgrade now".Length);
                Message_LinkLabel.Top = Enter_Button.Bottom + 20;
                Height = 410;
            }
        }
        private void SetMessageText(string text)
        {
            Message_LinkLabel.Text = text;
            Controls.Add(Message_LinkLabel);
            Message_LinkLabel.Left = (ClientSize.Width - Message_LinkLabel.Width) / 2;
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
        private void CheckPassword()
        {
            if (PasswordManager.Password == Password_TextBox.Text)
            {
                PasswordManager.IsPasswordValid = true;
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