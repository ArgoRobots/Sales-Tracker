using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

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
        /// Formats a TimeSpan into a time format.
        /// </summary>
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return string.Format(@"{0:hh\:mm\:ss\.ff}", timeSpan);
        }
        /// <summary>
        /// Formats a DateTime into a date format.
        /// </summary>
        public static string FormatDate(DateTime dateTime)
        {
            return string.Format(@"{0:yyyy/MM/dd}", dateTime);
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
        public static string RemoveAllEmptyLines(string[] lines)
        {
            return string.Join("\r\n", lines.Where(item => !string.IsNullOrWhiteSpace(item)));
        }

        /// <summary>
        /// Retuens true if there are any characters that Windows doesn't allow in file names.
        /// </summary>
        public static bool AreThereInvalidCharactersInGunaTextBox(object sender)
        {
            Guna2TextBox senderControl = (Guna2TextBox)sender;
            return @"/\#%&*|;".Any(c => senderControl.Text.Contains(c));
        }
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
        /// <summary>
        /// Allow numbers, one period, and one minus sign at the front
        /// </summary>
        public static void OnlyAllowNumbersAndOneDecimalInGunaTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || (sender as Guna2TextBox).Text.Contains('.')))
            {
                e.Handled = true;
            }
        }
        public static void OnlyAllowNumbersInTextBox(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Set max number in a GunaTextBox
        public static void SetMaxNumberInGunaTextbox2(object sender, KeyEventArgs e)
        {
            Guna2TextBox textbox = (Guna2TextBox)sender;
            if (textbox.Text != "")
            {
                if (Convert.ToInt32(textbox.Text) > 2)
                {
                    textbox.Text = "2";
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        public static void SetMaxNumberInGunaTextbox12(object sender, KeyEventArgs e)
        {
            Guna2TextBox textbox = (Guna2TextBox)sender;
            if (textbox.Text != "")
            {
                if (Convert.ToInt32(textbox.Text) > 12)
                {
                    textbox.Text = "12";
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        public static void SetMaxNumberInGunaTextbox16(object sender, KeyEventArgs e)
        {
            Guna2TextBox textbox = (Guna2TextBox)sender;
            if (textbox.Text != "")
            {
                if (Convert.ToInt32(textbox.Text) > 16)
                {
                    textbox.Text = "16";
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        public static void SetMaxNumberInGunaTextbox100(object sender, KeyEventArgs e)
        {
            Guna2TextBox textbox = (Guna2TextBox)sender;
            if (textbox.Text != "")
            {
                if (Convert.ToInt32(textbox.Text) > 100)
                {
                    textbox.Text = "100";
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }
        public static void SetMaxNumberInGunaTextbox360(object sender, KeyEventArgs e)
        {
            Guna2TextBox textbox = (Guna2TextBox)sender;
            if (textbox.Text != "")
            {
                if (Convert.ToInt32(textbox.Text) > 360)
                {
                    textbox.Text = "360";
                    textbox.SelectionStart = textbox.Text.Length;
                }
            }
        }

        public static void MakeSureTextIsNotSelectedAndCursorIsAtEnd(object sender, EventArgs e)
        {
            Guna2TextBox senderTextBox = (Guna2TextBox)sender;
            senderTextBox.SelectionStart = senderTextBox.Text.Length;
            senderTextBox.SelectionLength = 0;
        }

        /// <summary>
        /// Adds a number to a name and checks to make sure it doesn't already exist. This is used to add a number for Function, Sequence and App names.
        /// </summary>
        /// <returns>The original name plus a number.</returns>
        public static string AddNumberForName(string name, List<string> list)
        {
            int count = 1;
            while (true)
            {
                bool pass2 = false;
                foreach (string item in list)
                {
                    // If this name already exists
                    if (item == name + count.ToString())
                    {
                        pass2 = true;
                        break;
                    }
                }
                if (pass2) count++;
                else break;
            }
            return name + count.ToString();
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