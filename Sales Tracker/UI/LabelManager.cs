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
            totalLabel.Text = $"{baseText}: {dataGridView.Rows.Count}";

            // Position the label near the bottom-right of the DataGridView
            totalLabel.Location = new Point(dataGridView.Right - totalLabel.Width, dataGridView.Bottom + 10);
        }
    }
}