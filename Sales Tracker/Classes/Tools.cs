using Guna.UI2.WinForms;
using System.Diagnostics;

namespace Sales_Tracker.Classes
{
    internal static class Tools
    {
        // Formatting
        /// <summary>
        /// Formats a DateTime into a time format.
        /// </summary>
        public static string FormatTime(DateTime dateTime)
        {
            return string.Format(@"{0:hh\:mm\:ss\.ff}", dateTime);
        }
        /// <summary>
        /// Formats a DateTime into a date format.
        /// </summary>
        public static string FormatDate(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static void OpenLink(string URL)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(URL)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
        /// <summary>
        /// Check if a form of the specified type is already open.
        /// </summary>
        public static bool IsFormOpen(Type formType)
        {
            return Application.OpenForms.OfType<Form>().Any(f => f.GetType() == formType);
        }

        /// <summary>
        /// Returns the first focused Guna2TextBox found, or null if none are found.
        /// </summary>
        public static Guna2TextBox FindSelectedGTextBox(Control control)
        {
            // Check if any immediate child Guna2TextBox is focused
            var focusedTextBox = control.Controls.OfType<Guna2TextBox>().FirstOrDefault(gTextBox => gTextBox.Focused);
            if (focusedTextBox != null)
            {
                return focusedTextBox;
            }

            // Recursively check all child controls
            foreach (Control childControl in control.Controls)
            {
                var result = FindSelectedGTextBox(childControl);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        /// <summary>
        /// Allow numbers and one period.
        /// </summary>
        public static void OnlyAllowNumbersAndOneDecimalInGunaTextBox(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || (sender as Guna2TextBox).Text.Contains('.')))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// Allow numbers, one period, and one minus sign at the front.
        /// </summary>
        public static void OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, one period, and one minus sign at the front
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || (sender as Guna2TextBox).Text.Contains('.')) &&
                (e.KeyChar != '-' || (sender as Guna2TextBox).SelectionStart != 0))
            {
                e.Handled = true;
            }
        }
        public static void OnlyAllowNumbersInTextBox(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        public static void OnlyAllowLettersInTextBox(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }
        public static void MakeSureTextIsNotSelectedAndCursorIsAtEnd(object? sender, EventArgs e)
        {
            Guna2TextBox senderTextBox = (Guna2TextBox)sender;
            senderTextBox.SelectionStart = senderTextBox.Text.Length;
            senderTextBox.SelectionLength = 0;
        }
        public static string AddNumberForAStringThatAlreadyExists(string name, List<string> list)
        {
            name = RemoveNumAfterString(name);

            // Add a new " (num)"
            int count = 2;
            while (true)
            {
                bool pass2 = false;
                foreach (string item in list)
                {
                    if (item == name + " (" + count.ToString() + ")")
                    {
                        pass2 = true;
                        break;
                    }
                }
                // If this name already exists
                if (pass2)
                    count++;
                else
                {
                    name += " (" + count.ToString() + ")";
                    break;
                }
            }
            return name;
        }
        /// <summary>
        /// Check if the string has " (num)" at the end and remove it.
        /// </summary>
        public static string RemoveNumAfterString(string name)
        {
            if (name.EndsWith(")"))
            {
                int lastOpenParenthesisIndex = name.LastIndexOf('(');
                if (lastOpenParenthesisIndex != -1 && int.TryParse(name.AsSpan(lastOpenParenthesisIndex + 1, name.Length - lastOpenParenthesisIndex - 2), out _))
                {
                    return name.Substring(0, lastOpenParenthesisIndex).TrimEnd();  // Remove the " (num)"
                }
            }
            return name;
        }
    }
}