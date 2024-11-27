using Guna.UI2.WinForms;
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