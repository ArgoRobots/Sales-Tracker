using Sales_Tracker.Classes;

namespace Sales_Tracker.Passwords
{
    public partial class PasswordManager_Form : Form
    {
        // Init.
        public PasswordManager_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowBlankLoadingPanel(this);
            AddEventHandlersToTextBoxes();
            Theme.SetThemeForForm(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            CurrentPassword_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            CurrentPassword_TextBox.KeyDown += UI.TextBox_KeyDown;

            NewPassword_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            NewPassword_TextBox.KeyDown += UI.TextBox_KeyDown;
        }

        // Form event handlers
        private void PasswordManager_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Modify_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetUpdateButton();
        }
        private void Remove_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetUpdateButton();
        }
        private void CurrentPassword_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (Remove_RadioButton.Checked)
                {
                    Update_Button.PerformClick();
                }
            }
        }
        private void NewPassword_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateButton();
        }
        private void Update_Button_Click(object sender, EventArgs e)
        {
            if (Modify_RadioButton.Checked)
            {
                if (PasswordManager.Password != CurrentPassword_TextBox.Text)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", "The current password is incorrect", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                PasswordManager.Password = NewPassword_TextBox.Text;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Removed updated");
                CustomMessageBox.Show("Argo Sales Tracker", "Password updated successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                Close();
            }
            else
            {
                if (PasswordManager.Password != CurrentPassword_TextBox.Text)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", "The password is incorrect", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                CustomMessageBoxResult result = CustomMessageBox.Show(
                   "Argo Sales Tracker",
                   "Removing your password will make your data vulnerable to unauthorized access. Are you sure you want to continue?",
                   CustomMessageBoxIcon.Exclamation,
                   CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return;
                }

                PasswordManager.Password = null;
                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Removed password");
                CustomMessageBox.Show("Argo Sales Tracker", "Password removed successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                Close();
            }
        }
        private void Modify_Label_Click(object sender, EventArgs e)
        {
            Modify_RadioButton.Checked = !Modify_RadioButton.Checked;
        }
        private void Remove_Label_Click(object sender, EventArgs e)
        {
            Remove_RadioButton.Checked = !Remove_RadioButton.Checked;
        }

        // Methods
        private void SetUpdateButton()
        {
            if (Modify_RadioButton.Checked)
            {
                Update_Button.Text = "Update password";

                foreach (Control item in GetControls())
                {
                    Controls.Add(item);
                }
                Height = 500;
                Update_Button.Top = 413;
                ValidateButton();
            }
            else
            {
                Update_Button.Text = "Remove password";

                foreach (Control item in GetControls())
                {
                    Controls.Remove(item);
                }
                Height = 280;
                Update_Button.Top = 170;
                Update_Button.Enabled = true;
            }
        }
        private List<Control> GetControls()
        {
            return new List<Control>
            {
                NewPassword_TextBox,
                LengthRequirement_Label,
                NumberRequirement_Label,
                UppercaseRequirement_Label,
                SpecialCharacterRequirement_Label,
                Message_Label
            };
        }
        private void ValidateButton()
        {
            Update_Button.Enabled = PasswordManager.ValidatePassword(
                LengthRequirement_Label,
                UppercaseRequirement_Label,
                NumberRequirement_Label,
                SpecialCharacterRequirement_Label,
                NewPassword_TextBox.Text
            );
        }
    }
}