using Sales_Tracker.DataClasses;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages the display and positioning of labels in the Sales Tracker application, ensuring that labels
    /// for showing results and totals maintain consistent wording across language changes.
    /// </summary>
    public class LabelManager
    {
        /// <summary>
        /// Updates the label text to show the specified results message while preserving the original base text.
        /// This method ensures that the label’s prefix remains consistent even if the application language changes.
        /// The label is then centered horizontally within the specified parent control.
        /// </summary>
        public static void ShowShowingResultsLabel(Label label, string text, Control parentControl)
        {
            // Keep the first part (before ":") the same in case it's been translated
            string[] parts = label.Text.Split(':');
            string baseText = parts[0].Trim();
            label.Text = $"{baseText}: {text}";

            // Center the label horizontally
            label.Left = (parentControl.ClientSize.Width - label.Width) / 2;

            label.Visible = true;
        }

        /// <summary>
        /// Updates the total label text to display the row count of the specified DataGridView while preserving
        /// the original base text to maintain consistency across language changes. The label is positioned near
        /// the bottom-right of the DataGridView.
        /// </summary>
        public static void ShowTotalLabel(Label totalLabel, DataGridView dataGridView)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            // Keep the first part (before ":") the same in case it's been translated
            string[] parts = totalLabel.Text.Split(':');
            string baseText = parts[0].Trim();
            int count = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);
            totalLabel.Text = $"{baseText}: {count}";

            // Position the label near the bottom-right of the DataGridView
            totalLabel.Location = new Point(dataGridView.Right - totalLabel.Width, dataGridView.Bottom + 10);
        }

        /// <summary>
        /// If there is no data, then it adds a Label to the control.
        /// </summary>
        /// <returns>True if there is any data, False if there is no data.</returns>
        public static bool ManageNoDataLabelOnControl(bool hasData, Control control)
        {
            string text = ReadOnlyVariables.NoData_text;
            Label existingLabel = control.Controls.OfType<Label>().FirstOrDefault(label => label.Tag.ToString() == text);

            if (!hasData)
            {
                string textWithoutWhitespace = string.Concat(text.Where(c => !char.IsWhiteSpace(c)));

                // If there's no data and the label doesn't exist, create and add it
                if (existingLabel == null)
                {
                    Label label = new()
                    {
                        Font = new Font("Segoe UI", 12),
                        ForeColor = CustomColors.text,
                        Text = text,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Tag = text,
                        Anchor = AnchorStyles.Top,
                        Name = $"{textWithoutWhitespace}_Label"  // This is needed for the language translation
                    };

                    control.Controls.Add(label);
                    CenterLabelInParent(label);

                    control.Resize += delegate { CenterLabelInParent(label); };

                    label.BringToFront();
                }
                return false;
            }
            else
            {
                // If there's data and the label exists, remove it
                if (existingLabel != null)
                {
                    control.Controls.Remove(existingLabel);
                    existingLabel.Dispose();
                }
                return true;
            }
        }
        public static void CenterLabelInParent(Label label)
        {
            Control parent = label.Parent;

            if (label != null && parent != null)
            {
                label.Location = new Point((parent.Width - label.Width) / 2, (parent.Height - label.Height) / 2);
            }
        }
        public static void AddNoRecentlyOpenedCompanies(Control parent)
        {
            Label noProjectsLabel = new()
            {
                Text = "No recently opened projects",
                Size = new Size(parent.Width, CustomControls.PanelButtonHeight),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = CustomColors.text,
            };
            parent.Controls.Add(noProjectsLabel);
        }
    }
}