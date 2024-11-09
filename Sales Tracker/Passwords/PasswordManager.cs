using Sales_Tracker.UI;

namespace Sales_Tracker.Passwords
{
    /// <summary>
    /// Provides password management functionality, including password validation and prompting the user for password entry.
    /// </summary>
    internal class PasswordManager
    {
        // Properties
        private static bool _isPasswordValid;
        private static string _password;

        // Getters and setters
        public static bool IsPasswordValid
        {
            get => _isPasswordValid;
            set => _isPasswordValid = value;
        }
        public static string Password
        {
            get => _password;
            set => _password = value;
        }

        /// <summary>
        /// Validates the password based on specified requirements and updates the label colors.
        /// </summary>
        /// <returns>True if the password meets all the requirements, otherwise false.</returns>
        public static bool ValidatePassword(Label length, Label uppercase, Label digit, Label specialChar, string password)
        {
            bool isValid = true;

            if (password.Length >= 8)
            {
                length.ForeColor = CustomColors.AccentGreen;
            }
            else
            {
                length.ForeColor = CustomColors.Text;
                isValid = false;
            }

            if (password.Any(char.IsUpper))
            {
                uppercase.ForeColor = CustomColors.AccentGreen;
            }
            else
            {
                uppercase.ForeColor = CustomColors.Text;
                isValid = false;
            }

            if (password.Any(char.IsDigit))
            {
                digit.ForeColor = CustomColors.AccentGreen;
            }
            else
            {
                digit.ForeColor = CustomColors.Text;
                isValid = false;
            }

            if (password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                specialChar.ForeColor = CustomColors.AccentGreen;
            }
            else
            {
                specialChar.ForeColor = CustomColors.Text;
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// If a password is set, the user is prompted to enter it.
        /// </summary>
        /// <returns>True if the password is entered correctly or if a password is not set. False if the password is incorrect.</returns>
        public static bool EnterPassword()
        {
            if (Password != null)
            {
                new EnterPassword_Form().ShowDialog();
                return _isPasswordValid;
            }
            return true;
        }
    }
}