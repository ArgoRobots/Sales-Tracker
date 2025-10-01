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

        /// <summary>
        /// Validates the element configuration.
        /// </summary>
        public virtual bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (Bounds.Width < 10 || Bounds.Height < 10)
            {
                errorMessage = "Element size too small";
                return false;
            }

            return true;
        }

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
        public static Label AddPropertyLabel(Panel container, string text, int yPosition)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = CustomColors.Text,
                Location = new Point(10, yPosition + 3),
                Size = new Size(70, 20)
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
                Size = new Size(180, 26),
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
                Size = new Size(100, 26),
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
                Size = new Size(180, 26),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedItem = value;
            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null)
                    onChange(comboBox.SelectedItem.ToString());
            };
            container.Controls.Add(comboBox);
            return comboBox;
        }
    }
}
