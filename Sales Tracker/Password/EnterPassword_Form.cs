using Sales_Tracker.Classes;

namespace Sales_Tracker.Password
{
    public partial class EnterPassword_Form : Form
    {
        // Init.
        public EnterPassword_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowLoadingPanel(this);
            Theme.SetThemeForForm(this);
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

        // Methods
        private void CheckPassword()
        {
            if (PasswordManager.Password == Password_TextBox.Text)
            {
                PasswordManager.isPasswordValid = true;
                Close();
            }
            else
            {
                CustomMessageBox.Show("Argo Sales Tracker", "Incorrect password", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
            }
        }
    }
}