namespace Sales_Tracker.UI
{
    public static class LayoutUtils
    {
        /// <summary>
        /// Recalculates and repositions a checkbox and label to fit within form boundaries.
        /// Positions them from the right edge, with fallback to left positioning if needed.
        /// </summary>
        public static void RecalculateCheckboxLabelLayout(Control checkbox, Label label, Form form, int rightMargin = 20, int spacing = 8)
        {
            if (checkbox == null || label == null || form == null)
            {
                return;
            }

            // Position the label first (from the right edge)
            label.Location = new Point(
                form.ClientSize.Width - rightMargin - label.PreferredWidth,
                label.Top);

            // Position the checkbox to the left of the label
            checkbox.Location = new Point(
                label.Left - spacing - checkbox.Width,
                checkbox.Top);

            // Ensure the checkbox doesn't go off the left side of the form
            if (checkbox.Left < 20)
            {
                // If it would go off screen, position checkbox at minimum left position
                checkbox.Left = 20;
                // And reposition label to the right of checkbox
                label.Location = new Point(
                    checkbox.Right + spacing,
                    label.Top);
            }
        }
    }
}
