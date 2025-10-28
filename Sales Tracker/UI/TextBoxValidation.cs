﻿using Guna.UI2.WinForms;
using Sales_Tracker.Theme;
using System.Text;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides validation methods for Guna2TextBox controls to restrict input to specific formats.
    /// </summary>
    public static class TextBoxValidation
    {
        // Public methods
        /// <summary>
        /// Allows numbers only.
        /// </summary>
        public static void OnlyAllowNumbers(Guna2TextBox textBox)
        {
            textBox.KeyPress += OnlyAllowNumbersInTextBox;
            textBox.TextChanged += ValidateNumbersOnly;
        }

        /// <summary>
        /// Allows digits and a single decimal point.
        /// </summary>
        public static void OnlyAllowNumbersAndOneDecimal(Guna2TextBox textBox)
        {
            textBox.KeyPress += OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            textBox.TextChanged += ValidateDecimalNumbers;
        }

        /// <summary>
        /// Allows digits, a single decimal point, and a negative sign at the start.
        /// </summary>
        public static void OnlyAllowNumbersAndOneDecimalAndOneMinus(Guna2TextBox textBox)
        {
            textBox.KeyPress += OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
            textBox.TextChanged += ValidateDecimalAndNegativeNumbers;
        }

        /// <summary>
        /// Allows letters and spaces only.
        /// </summary>
        public static void OnlyAllowLetters(Guna2TextBox textBox)
        {
            textBox.KeyPress += OnlyAllowLettersInTextBox;
            textBox.TextChanged += ValidateLettersOnly;
        }

        /// <summary>
        /// Allows email format (requires @ sign).
        /// </summary>
        public static void ValidateEmail(Guna2TextBox textBox)
        {
            textBox.TextChanged += ValidateEmailFormat;
        }

        /// <summary>
        /// Allows phone number format: digits, parentheses, dashes, spaces, and "ext".
        /// </summary>
        public static void ValidatePhoneNumber(Guna2TextBox textBox)
        {
            textBox.KeyPress += OnlyAllowPhoneCharactersInTextBox;
            textBox.TextChanged += ValidatePhoneNumberFormat;
        }

        /// <summary>
        /// Checks if email contains @ sign and sets border color accordingly.
        /// </summary>
        private static void ValidateEmailFormat(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                // Reset to default when empty
                textBox.BorderColor = CustomColors.ControlBorder;
                textBox.Tag = "valid";
            }
            else if (!text.Contains('@'))
            {
                // Invalid - missing @ sign
                textBox.BorderColor = CustomColors.AccentRed;
                textBox.Tag = "invalid";
            }
            else
            {
                // Valid
                textBox.BorderColor = CustomColors.AccentGreen;
                textBox.Tag = "valid";
            }
        }

        /// <summary>
        /// Allows only valid phone number characters during typing.
        /// </summary>
        private static void OnlyAllowPhoneCharactersInTextBox(object sender, KeyPressEventArgs e)
        {
            // Allow digits, parentheses, dashes, spaces, and letters (for "ext")
            if (!char.IsControl(e.KeyChar) && 
                !char.IsDigit(e.KeyChar) && 
                e.KeyChar != '(' && 
                e.KeyChar != ')' && 
                e.KeyChar != '-' && 
                e.KeyChar != ' ' &&
                !char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Validates phone number format and ensures only valid sequences.
        /// </summary>
        private static void ValidatePhoneNumberFormat(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            int cursorPosition = textBox.SelectionStart;
            string text = textBox.Text;
            string cleanedText = CleanPhoneNumber(text);

            if (text != cleanedText)
            {
                textBox.Text = cleanedText;
                textBox.SelectionStart = Math.Min(cursorPosition, cleanedText.Length);
            }
        }

        /// <summary>
        /// Cleans phone number to only allow valid characters and "ext" sequence.
        /// </summary>
        private static string CleanPhoneNumber(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            StringBuilder result = new();
            string lowerInput = input.ToLower();
            
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                
                // Allow digits, parentheses, dashes, and spaces
                if (char.IsDigit(c) || c == '(' || c == ')' || c == '-' || c == ' ')
                {
                    result.Append(c);
                }
                // Check for "ext" sequence
                else if (char.IsLetter(c))
                {
                    // Check if this could be part of "ext"
                    if (i + 2 < input.Length)
                    {
                        string sequence = lowerInput.Substring(i, 3);
                        if (sequence == "ext")
                        {
                            result.Append(input.Substring(i, 3));
                            i += 2; 
                        }
                    }
                    else if (i + 1 < input.Length)
                    {
                        string sequence = lowerInput.Substring(i, 2);
                        if ("ext".StartsWith(sequence))
                        {
                            result.Append(input.Substring(i, 2));
                            i += 1;
                        }
                    }
                    else if (i < input.Length)
                    {
                        char lowerChar = lowerInput[i];
                        if (lowerChar == 'e')
                        {
                            result.Append(c);
                        }
                    }
                }
            }

            return result.ToString();
        }

        // Private methods
        private static void OnlyAllowNumbersInTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private static void OnlyAllowNumbersAndOneDecimalInGunaTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || (sender as Guna2TextBox).Text.Contains('.')))
            {
                e.Handled = true;
            }
        }
        private static void OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || (sender as Guna2TextBox).Text.Contains('.')) &&
                (e.KeyChar != '-' || (sender as Guna2TextBox).SelectionStart != 0))
            {
                e.Handled = true;
            }
        }
        private static void OnlyAllowLettersInTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }
        private static void ValidateNumbersOnly(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            int cursorPosition = textBox.SelectionStart;
            string cleanedText = new(textBox.Text.Where(char.IsDigit).ToArray());

            if (textBox.Text != cleanedText)
            {
                textBox.Text = cleanedText;
                textBox.SelectionStart = Math.Min(cursorPosition, cleanedText.Length);
            }
        }
        private static void ValidateDecimalNumbers(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            int cursorPosition = textBox.SelectionStart;
            string text = textBox.Text;
            string cleanedText = CleanDecimalNumber(text);

            if (text != cleanedText)
            {
                textBox.Text = cleanedText;
                textBox.SelectionStart = Math.Min(cursorPosition, cleanedText.Length);
            }
        }
        private static void ValidateDecimalAndNegativeNumbers(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            int cursorPosition = textBox.SelectionStart;
            string text = textBox.Text;
            string cleanedText = CleanDecimalAndNegativeNumber(text);

            if (text != cleanedText)
            {
                textBox.Text = cleanedText;
                textBox.SelectionStart = Math.Min(cursorPosition, cleanedText.Length);
            }
        }
        private static void ValidateLettersOnly(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            int cursorPosition = textBox.SelectionStart;
            string text = textBox.Text;
            string cleanedText = new(text.Where(c => char.IsLetter(c) || c == ' ').ToArray());

            if (text != cleanedText)
            {
                textBox.Text = cleanedText;
                textBox.SelectionStart = Math.Min(cursorPosition, cleanedText.Length);
            }
        }
        private static string CleanDecimalNumber(string input)
        {
            if (string.IsNullOrEmpty(input)) { return ""; }

            StringBuilder result = new();
            bool hasDecimal = false;

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else if (c == '.' && !hasDecimal)
                {
                    result.Append(c);
                    hasDecimal = true;
                }
            }

            return result.ToString();
        }
        private static string CleanDecimalAndNegativeNumber(string input)
        {
            if (string.IsNullOrEmpty(input)) { return ""; }

            StringBuilder result = new();
            bool hasDecimal = false;

            if (input.StartsWith('-'))
            {
                result.Append('-');
                input = input.Substring(1);
            }

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else if (c == '.' && !hasDecimal)
                {
                    result.Append(c);
                    hasDecimal = true;
                }
            }

            return result.ToString();
        }
    }
}