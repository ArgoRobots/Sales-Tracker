using Guna.UI2.WinForms;
using Sales_Tracker.UI;
using System.Diagnostics;
using System.Reflection;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Provides a collection of utility methods for various formatting, file management, and input validation tasks.
    /// </summary>
    public static class Tools
    {
        // Formatting
        /// <summary>
        /// Formats a DateTime into a time format.
        /// </summary>
        public static string FormatTime(DateTime dateTime)
        {
            return $@"{dateTime:hh\:mm\:ss\.ff}";
        }

        /// <summary>
        /// Formats a DateTime into a date format.
        /// </summary>
        public static string FormatDate(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        // General
        /// <summary>
        /// Opens the specified URL in the default web browser.
        /// </summary>
        public static void OpenLink(string URL)
        {
            if (string.IsNullOrEmpty(URL))
            {
                Log.Write(0, "URL is empty");
                return;
            }

            new Process
            {
                StartInfo = new ProcessStartInfo(URL)
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        /// <summary>
        /// Retrieves the version number of the currently executing assembly.
        /// </summary>
        public static string GetVersionNumber()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Opens Windows File Explorer and selects the specified file in its folder.
        /// </summary>
        public static void ShowFileInFolder(string filePath)
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }

        /// <summary>
        /// Searches a DataGridView for the text in the search_TextBox.
        /// </summary>
        /// <returns>True if the searching label should be shown, or false if it should not be shown.</returns>
        public static bool SearchSelectedDataGridView(Guna2TextBox search_TextBox)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.SelectedDataGridView.Rows)
            {
                bool isVisible = row.Cells.Cast<DataGridViewCell>()
                                          .Any(cell => cell.Value != null && cell.Value.ToString().Contains(search_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                row.Visible = isVisible;
            }

            DataGridViewManager.UpdateAlternatingRowColors(MainMenu_Form.Instance.SelectedDataGridView);

            return !string.IsNullOrEmpty(search_TextBox.Text.Trim());
        }
        public static void ScrollToTopOfDataGridView(Guna2DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = 0;
            }
        }

        // TextBoxes
        /// <summary>
        /// Returns the first focused Guna2TextBox found, or null if none are found.
        /// </summary>
        public static Guna2TextBox? FindSelectedGTextBox(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                if (childControl is Guna2TextBox { Focused: true } textBox)
                {
                    return textBox;
                }
                Guna2TextBox? result = FindSelectedGTextBox(childControl);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        // Strings
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
                    if (item == name + $" ({count})")
                    {
                        pass2 = true;
                        break;
                    }
                }
                // If this name already exists
                if (pass2)
                {
                    count++;
                }
                else
                {
                    name += $" ({count})";
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
            if (name.EndsWith(')'))
            {
                int lastOpenParenthesisIndex = name.LastIndexOf('(');
                if (lastOpenParenthesisIndex != -1 && int.TryParse(name.AsSpan(lastOpenParenthesisIndex + 1, name.Length - lastOpenParenthesisIndex - 2), out _))
                {
                    return name.Substring(0, lastOpenParenthesisIndex).TrimEnd();  // Remove the " (num)"
                }
            }
            return name;
        }
        public static string AddEllipsisToString(string text, Font font, int availableWidth)
        {
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            SizeF textSize = g.MeasureString(text, font);

            // If text fits, return it as-is
            if (textSize.Width <= availableWidth)
            {
                return text;
            }

            // Binary search for the optimal truncation point
            int low = 0;
            int high = text.Length;
            string ellipsis = "...";

            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                string truncated = text[..mid] + ellipsis;
                SizeF truncatedSize = g.MeasureString(truncated, font);

                if (truncatedSize.Width <= availableWidth)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return text[..low] + ellipsis;
        }

        // Bytes
        /// <summary>
        /// Converts a size in bytes to a human-readable string with appropriate units (Bytes, KB, MB, GB, or TB).
        /// </summary>
        /// <returns>
        /// A string representing the size converted to the largest possible unit that results in a value greater than or equal to 1,
        /// formatted to two decimal places.
        /// </returns>
        public static string ConvertBytesToReadableSize(long bytes)
        {
            string[] sizes = ["Bytes", "KB", "MB", "GB", "TB"];
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // Forms
        /// <summary>
        /// Returns true if a form of a specified type is already open.
        /// </summary>
        public static bool IsFormOpen(Type formType)
        {
            return Application.OpenForms.OfType<Form>().Any(f => f.GetType() == formType);
        }
        public static void OpenForm(Form form)
        {
            Form existingForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f.GetType() == form.GetType());
            if (existingForm != null)
            {
                existingForm.BringToFront();
            }
            else
            {
                // Set the Owner property to ensure the new form stays on top of the current form
                // without using TopMost. This creates a parent-child relationship between forms
                Form callingForm = MainMenu_Form.Instance;
                form.Owner = callingForm;
                form.Show();
            }
        }
        public static void CloseOpenForm<T>() where T : Form
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is T)
                {
                    openForm.Close();
                    return;
                }
            }
        }
    }
}