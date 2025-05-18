using Sales_Tracker.Theme;

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
            (bool isValid, bool _, bool _, bool _, bool _) = ValidatePasswordWithFlags(length, uppercase, digit, specialChar, password);
            return isValid;
        }

        /// <summary>
        /// Validates the password based on specified requirements, updates the label colors, and returns flags for each requirement.
        /// </summary>
        /// <returns>A tuple containing: (overall validity, length valid, uppercase valid, digit valid, special char valid)</returns>
        public static (bool isValid, bool lengthValid, bool uppercaseValid, bool digitValid, bool specialCharValid)
            ValidatePasswordWithFlags(Label length, Label uppercase, Label digit, Label specialChar, string password)
        {
            bool lengthValid = false;
            bool uppercaseValid = false;
            bool digitValid = false;
            bool specialCharValid = false;

            if (password.Length >= 8)
            {
                length.ForeColor = CustomColors.AccentGreen;
                lengthValid = true;
            }
            else
            {
                length.ForeColor = CustomColors.Text;
            }

            if (password.Any(char.IsUpper))
            {
                uppercase.ForeColor = CustomColors.AccentGreen;
                uppercaseValid = true;
            }
            else
            {
                uppercase.ForeColor = CustomColors.Text;
            }

            if (password.Any(char.IsDigit))
            {
                digit.ForeColor = CustomColors.AccentGreen;
                digitValid = true;
            }
            else
            {
                digit.ForeColor = CustomColors.Text;
            }

            if (password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                specialChar.ForeColor = CustomColors.AccentGreen;
                specialCharValid = true;
            }
            else
            {
                specialChar.ForeColor = CustomColors.Text;
            }

            bool isValid = lengthValid && uppercaseValid && digitValid && specialCharValid;
            return (isValid, lengthValid, uppercaseValid, digitValid, specialCharValid);
        }

        /// <summary>
        /// If a password is set, the user is prompted to enter it.
        /// </summary>
        /// <returns>True if the password is entered correctly or if a password is not set. False if the password is incorrect.</returns>
        public static bool EnterPassword(bool allowWindowsHello = true)
        {
            if (Password != null)
            {
                new EnterPassword_Form(allowWindowsHello).ShowDialog();
                return _isPasswordValid;
            }
            return true;
        }
    }
}