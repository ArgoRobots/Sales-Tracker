using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Encryption;
using Sales_Tracker.Properties;
using Sales_Tracker.Theme;
using System.Formats.Tar;

namespace Sales_Tracker.Passwords
{
    /// <summary>
    /// Provides password management functionality, including password validation and prompting the user for password entry.
    /// </summary>
    internal class PasswordManager
    {
        // Getters and setters
        public static bool IsPasswordValid { get; set; }
        public static string Password { get; set; }

        /// <summary>
        /// Validates the password based on specified requirements and updates the label colors.
        /// </summary>
        /// <returns>True if the password meets all the requirements, otherwise false.</returns>
        public static bool ValidatePassword(Label length, Label uppercase, Label digit, Label specialChar, string password)
        {
            PasswordValidationResult result = ValidatePasswordWithFlags(length, uppercase, digit, specialChar, password);
            return result.IsValid;
        }

        /// <summary>
        /// Validates the password based on specified requirements, updates the label colors, and returns flags for each requirement.
        /// </summary>
        /// <returns>PasswordValidationResult containing all validation flags.</returns>
        public static PasswordValidationResult ValidatePasswordWithFlags(Label length, Label uppercase, Label digit, Label specialChar, string password)
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
            return new PasswordValidationResult(isValid, lengthValid, uppercaseValid, digitValid, specialCharValid);
        }

        /// <summary>
        /// Gets the list of accountants from the encrypted company file.
        /// Reads directly from the encrypted company archive without requiring MainMenu_Form to be instantiated.
        /// This method extracts and reads the accountants data entirely in memory without creating temporary files.
        /// </summary>
        public static List<string> GetAccountantList()
        {
            try
            {
                // Check if the company file exists
                if (!File.Exists(Directories.ArgoCompany_file))
                {
                    Log.WriteWithFormat(1, "Company file not found: {0}", Directories.ArgoCompany_file);
                    return [];
                }

                // Create a temporary directory for extraction
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directories.CreateDirectory(tempDir);

                string sourceFile = Directories.ArgoCompany_file;
                string decryptedTempFile = null;

                try
                {
                    // Check if the file is encrypted and decrypt if necessary
                    using (FileStream fs = new(sourceFile, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new(fs))
                    {
                        // Read to the end to find encryption markers
                        string content = reader.ReadToEnd();
                        string[] lines = content.Split([Environment.NewLine], StringSplitOptions.None);

                        if (lines.Length >= 2 && lines[^2].Contains(EncryptionManager.encryptedTag) &&
                            lines[^2].Split(':')[1] == EncryptionManager.encryptedValue)
                        {
                            // File is encrypted, decrypt it first
                            decryptedTempFile = Path.GetTempFileName();
                            EncryptionManager.DecryptAndWriteToFile(sourceFile, decryptedTempFile, EncryptionManager.AesKey, EncryptionManager.AesIV);
                            sourceFile = decryptedTempFile;
                        }
                    }

                    // Extract the tar file to temp directory
                    TarFile.ExtractToDirectory(sourceFile, tempDir, true);

                    // Find the accountants file in the extracted directory
                    string[] accountantFiles = Directory.GetFiles(tempDir, "accountants.txt", SearchOption.AllDirectories);

                    if (accountantFiles.Length > 0)
                    {
                        string accountantsFile = accountantFiles[0];
                        List<string> accountants = Directories.ReadAllLinesInFile(accountantsFile).ToList();

                        return accountants;
                    }
                    else
                    {
                        Log.Write(1, "No accountants.txt file found in company archive");
                        return [];
                    }
                }
                finally
                {
                    // Clean up temporary files
                    if (decryptedTempFile != null && File.Exists(decryptedTempFile))
                    {
                        File.Delete(decryptedTempFile);
                    }

                    if (Directory.Exists(tempDir))
                    {
                        Directories.DeleteDirectory(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error extracting accountants list from company file: {0}", ex.Message);
                return [];
            }
        }

        /// <summary>
        /// If a password is set, the user is prompted to enter it.
        /// If there are multiple accountants, the user is prompted to select one.
        /// If there's exactly one accountant, it's automatically selected.
        /// </summary>
        /// <returns>True if the password is entered correctly (or not required) and accountant is selected (if required). Otherwise, false.</returns>
        public static bool EnterPassword(bool allowWindowsHello = true)
        {
            List<string> accountants = GetAccountantList();
            bool requiresAccountantSelection = accountants.Count >= 2 && allowWindowsHello;

            Password = EncryptionManager.GetPasswordFromFile(Directories.ArgoCompany_file, EncryptionManager.AesKey, EncryptionManager.AesIV);
            bool requiresPassword = Password != null;

            // Auto-select single accountant
            if (accountants.Count == 1)
            {
                MainMenu_Form.SelectedAccountant = accountants[0];
            }
            else if (accountants.Count == 0)
            {
                MainMenu_Form.SelectedAccountant = null;
            }

            if (requiresAccountantSelection || requiresPassword)
            {
                new EnterPassword_Form(allowWindowsHello, requiresAccountantSelection, requiresPassword).ShowDialog();

                // Check if both required inputs are valid
                bool accountantValid = !requiresAccountantSelection || !string.IsNullOrEmpty(MainMenu_Form.SelectedAccountant);
                bool passwordValid = !requiresPassword || IsPasswordValid;

                return accountantValid && passwordValid;
            }

            return true;
        }

        /// <summary>
        /// Toggles the visibility of the password characters in a <see cref="TextBox"/> by switching 
        /// between the plain text and a masked character. Also updates the associated eye icon 
        /// based on the current application theme.
        /// </summary>
        public static void TogglePasswordVisibility(Guna2TextBox textBox, Guna2CircleButton eyeButton)
        {
            bool isDarkTheme = ThemeManager.IsDarkTheme();

            // Toggle the password character
            if (textBox.PasswordChar == '\0')
            {
                textBox.PasswordChar = '•';
                eyeButton.Image = isDarkTheme ? Resources.ViewWhite : Resources.ViewBlack;
            }
            else
            {
                textBox.PasswordChar = '\0';
                eyeButton.Image = isDarkTheme ? Resources.HideWhite : Resources.HideBlack;
            }

            textBox.Focus();
        }
    }
}