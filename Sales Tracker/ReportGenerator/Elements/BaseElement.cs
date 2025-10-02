using Guna.UI2.WinForms;
using Sales_Tracker.Theme;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Base class for all report elements.
    /// </summary>
    public abstract class BaseElement
    {
        // Properties
        /// <summary>
        /// Unique identifier for the element.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Position and size of the element on the report canvas.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Display name for the element (used in UI).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Z-order for layering elements.
        /// </summary>
        public int ZOrder { get; set; }

        /// <summary>
        /// Whether the element is currently selected in the designer.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether the element is visible in the report.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets the height of each row for the element in the designer.
        /// </summary>
        public static byte RowHeight { get; } = 55;
        public static byte CheckBoxRowHeight { get; } = 45;

        private const int ControlHeight = 45;

        // Abstract methods
        /// <summary>
        /// Gets the element type.
        /// </summary>
        public abstract ReportElementType GetElementType();

        /// <summary>
        /// Creates a copy of this element.
        /// </summary>
        public abstract BaseElement Clone();

        /// <summary>
        /// Renders the element to a graphics context for the report.
        /// </summary>
        public abstract void RenderElement(Graphics graphics, ReportConfiguration config);

        /// <summary>
        /// Draws the element in the designer canvas.
        /// </summary>
        public abstract void DrawDesignerElement(Graphics graphics);

        /// <summary>
        /// Creates property controls for the element in the designer.
        /// </summary>
        /// <param name="container">Container panel for controls</param>
        /// <param name="yPosition">Starting Y position</param>
        /// <param name="onPropertyChanged">Callback when property changes</param>
        /// <returns>New Y position after adding controls</returns>
        public abstract int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged);

        // Helper methods
        /// <summary>
        /// Gets the designer color for this element type.
        /// </summary>
        protected virtual Color GetDesignerColor()
        {
            return Color.LightGray;
        }

        /// <summary>
        /// Adds a property label to the container.
        /// </summary>
        public static Label AddPropertyLabel(Panel container, string text, int yPosition, bool bold = false)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = CustomColors.Text,
                Location = new Point(10, yPosition + 3),
                AutoSize = true
            };

            container.Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Adds a text box property control.
        /// </summary>
        public static Guna2TextBox AddPropertyTextBox(Panel container, string value, int yPosition, Action<string> onChange)
        {
            Guna2TextBox textBox = new()
            {
                Text = value,
                Size = new Size(180, ControlHeight),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9)
            };

            textBox.TextChanged += (s, e) => onChange(textBox.Text);
            container.Controls.Add(textBox);
            return textBox;
        }

        /// <summary>
        /// Adds a numeric up/down property control.
        /// </summary>
        public static Guna2NumericUpDown AddPropertyNumericUpDown(Panel container, decimal value, int yPosition, Action<decimal> onChange, decimal min = 0, decimal max = 9999)
        {
            Guna2NumericUpDown numericUpDown = new()
            {
                Size = new Size(100, ControlHeight),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Minimum = min,
                Maximum = max,
                Value = value
            };

            numericUpDown.ValueChanged += (s, e) => onChange(numericUpDown.Value);
            container.Controls.Add(numericUpDown);
            return numericUpDown;
        }

        /// <summary>
        /// Adds a combo box property control.
        /// </summary>
        protected static Guna2ComboBox AddPropertyComboBox(Panel container, string value, int yPosition, string[] items, Action<string> onChange)
        {
            Guna2ComboBox comboBox = new()
            {
                Size = new Size(180, ControlHeight),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                ItemHeight = 39  // Needed to make the height equal to ControlHeight
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedItem = value;
            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null)
                {
                    onChange(comboBox.SelectedItem.ToString());
                }
            };

            container.Controls.Add(comboBox);
            return comboBox;
        }

        /// <summary>
        /// Adds a checkbox with an accompanying text label that can be clicked to toggle the checkbox.
        /// </summary>
        public static Guna2CustomCheckBox AddPropertyCheckBoxWithLabel(
            Panel container,
            string labelText,
            bool isChecked,
            int yPosition,
            Action<bool> onChange)
        {
            int checkBoxY = yPosition + 2;

            // Add the checkbox
            Guna2CustomCheckBox checkBox = new()
            {
                Checked = isChecked,
                Location = new Point(10, checkBoxY),
                Size = new Size(22, 22),
                Padding = new Padding(5),
                Animated = true
            };

            checkBox.CheckedChanged += (s, e) => onChange(checkBox.Checked);
            container.Controls.Add(checkBox);

            // Add the label after the checkbox
            Label label = new()
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9),
                ForeColor = CustomColors.Text,
                Location = new Point(checkBox.Right + 5, yPosition),
                AutoSize = true
            };

            container.Controls.Add(label);

            // Clicking the label toggles the checkbox
            label.Click += (s, e) => { checkBox.Checked = !checkBox.Checked; };

            return checkBox;
        }

    }
}