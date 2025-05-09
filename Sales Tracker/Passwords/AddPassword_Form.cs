using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings.Menus;
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

            // Center the label
            PasswordsMatch_Label.Width = ClientSize.Width;
            PasswordsMatch_Label.Left = 0;

            AlignValidationLabels();
            SetTheme();
        }
        private void AlignValidationLabels()
        {
            // Find the widest label
            int maxWidth = Math.Max(
                Math.Max(LengthRequirement_Label.Width, UppercaseRequirement_Label.Width),
                Math.Max(NumberRequirement_Label.Width, SpecialCharacterRequirement_Label.Width)
            );

            // Calculate center position
            int centerX = ClientSize.Width / 2;
            int labelLeft = centerX - (maxWidth / 2);

            // Set all labels to the same left position
            LengthRequirement_Label.Left = labelLeft;
            UppercaseRequirement_Label.Left = labelLeft;
            NumberRequirement_Label.Left = labelLeft;
            SpecialCharacterRequirement_Label.Left = labelLeft;

            // Position checkmarks next to each label
            int checkMarkWidth = labelLeft - Length_Checkmark.Width - 5;
            Length_Checkmark.Left = checkMarkWidth;
            Uppercase_Checkmark.Left = checkMarkWidth;
            Number_Checkmark.Left = checkMarkWidth;
            SpecialChar_Checkmark.Left = checkMarkWidth;
        }
        private void SetTheme()
        {
            Length_Checkmark.BackColor = CustomColors.MainBackground;
            Uppercase_Checkmark.BackColor = CustomColors.MainBackground;
            Number_Checkmark.BackColor = CustomColors.MainBackground;
            SpecialChar_Checkmark.BackColor = CustomColors.MainBackground;

            Length_Checkmark.FillColor = CustomColors.MainBackground;
            Uppercase_Checkmark.FillColor = CustomColors.MainBackground;
            Number_Checkmark.FillColor = CustomColors.MainBackground;
            SpecialChar_Checkmark.FillColor = CustomColors.MainBackground;

            PasswordEye_Button.BackColor = CustomColors.ControlBack;
            ConfirmPasswordEye_Button.BackColor = CustomColors.ControlBack;
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Password_TextBox);
            TextBoxManager.Attach(ConfirmPassword_TextBox);
        }

        // Form event handlers
        private void AddPassword_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Password_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswordInputs();
        }
        private void ConfirmPassword_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswordInputs();
        }
        private void Password_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetPassword();
            }
        }
        private void ConfirmPassword_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetPassword();
            }
        }
        private void PasswordEye_Button_Click(object sender, EventArgs e)
        {
            // Toggle the password char
            if (Password_TextBox.PasswordChar == '\0')
            {
                Password_TextBox.PasswordChar = '•';
                PasswordEye_Button.Image = Resources.View;
            }
            else
            {
                Password_TextBox.PasswordChar = '\0';
                PasswordEye_Button.Image = Resources.Hide;
            }

            // Set focus back to the password field
            Password_TextBox.Focus();
        }
        private void ConfirmPasswordEye_Button_Click(object sender, EventArgs e)
        {
            // Toggle the password char
            if (ConfirmPassword_TextBox.PasswordChar == '\0')
            {
                ConfirmPassword_TextBox.PasswordChar = '•';
                ConfirmPasswordEye_Button.Image = Resources.View;
            }
            else
            {
                ConfirmPassword_TextBox.PasswordChar = '\0';
                ConfirmPasswordEye_Button.Image = Resources.Hide;
            }

            // Set focus back to the confirm password field
            ConfirmPassword_TextBox.Focus();
        }
        private void SetPassword_Button_Click(object sender, EventArgs e)
        {
            SetPassword();
        }

        // Methods
        private void ValidatePasswordInputs()
        {
            // Call the updated ValidatePassword method that now returns a tuple with requirement flags
            (bool isValid, bool lengthValid, bool uppercaseValid, bool digitValid, bool specialCharValid) = PasswordManager.ValidatePasswordWithFlags(
                LengthRequirement_Label,
                UppercaseRequirement_Label,
                NumberRequirement_Label,
                SpecialCharacterRequirement_Label,
                Password_TextBox.Text
            );

            // Update checkmark visibility based on validation results
            Length_Checkmark.Visible = lengthValid;
            Uppercase_Checkmark.Visible = uppercaseValid;
            Number_Checkmark.Visible = digitValid;
            SpecialChar_Checkmark.Visible = specialCharValid;

            // Check if both password fields have content before showing match status
            if (!string.IsNullOrEmpty(Password_TextBox.Text) && !string.IsNullOrEmpty(ConfirmPassword_TextBox.Text))
            {
                bool passwordsMatch = Password_TextBox.Text == ConfirmPassword_TextBox.Text;

                // Show the match status label
                PasswordsMatch_Label.Visible = true;

                if (passwordsMatch)
                {
                    PasswordsMatch_Label.Text = "Passwords match";
                    PasswordsMatch_Label.ForeColor = CustomColors.AccentGreen;
                }
                else
                {
                    PasswordsMatch_Label.Text = "Passwords do not match";
                    PasswordsMatch_Label.ForeColor = CustomColors.AccentRed;
                }

                // Only enable the button if all requirements are met AND passwords match
                SetPassword_Button.Enabled = isValid && passwordsMatch;
            }
            else
            {
                // Hide the match label if one of the fields is empty
                PasswordsMatch_Label.Visible = false;
                SetPassword_Button.Enabled = false;
            }
        }
        private void SetPassword()
        {
            if (Password_TextBox.Text != ConfirmPassword_TextBox.Text)
            {
                CustomMessageBox.Show("Error", "Passwords do not match", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            // Show warning message about not losing the password
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Warning", "IMPORTANT: This password cannot be recovered if lost. Are you sure you want to set this password?",
                CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                PasswordManager.Password = Password_TextBox.Text;
                Security_Form.Instance.SetPasswordButton();
                CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 4, $"Added password");
                CustomMessageBox.Show("Password set", "Password set successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                Close();
            }
        }
    }
}