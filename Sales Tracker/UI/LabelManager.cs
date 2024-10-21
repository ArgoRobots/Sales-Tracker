namespace Sales_Tracker.UI
{
    public class LabelManager
    {
        public static void ShowShowingResultsLabel(Label label, string text, Control parentControl)
        {
            // Keep the first part (before ":") the same in case it's been translated
            string[] parts = label.Text.Split(':');
            string baseText = parts[0].Trim();
            label.Text = $"{baseText}: {text}";

            // Center the label horizontally
            label.Left = (parentControl.ClientSize.Width - label.Width) / 2;

            if (!parentControl.Controls.Contains(label))
            {
                parentControl.Controls.Add(label);
            }
        }
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