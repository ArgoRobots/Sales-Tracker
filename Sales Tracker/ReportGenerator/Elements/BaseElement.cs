using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using Sales_Tracker.Classes;
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
        /// Cached property controls panel to avoid recreation.
        /// </summary>
        private Panel _cachedPropertyPanel;

        /// <summary>
        /// Flag to track if property controls have been created.
        /// </summary>
        private bool _propertyControlsCreated = false;

        /// <summary>
        /// Gets the height of each row for the element in the designer.
        /// </summary>
        public static byte ControlRowHeight { get; } = 55;
        public static byte CheckBoxRowHeight { get; } = 40;

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
        /// Public method that handles caching of property controls.
        /// </summary>
        public virtual int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // If controls haven't been created yet, create them
            if (!_propertyControlsCreated)
            {
                // Create a panel to hold all property controls
                _cachedPropertyPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = container.Size,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };

                _propertyControlsCreated = true;
            }

            // Clear the container
            container.Controls.Clear();

            // Add the cached panel to the container
            if (_cachedPropertyPanel != null)
            {
                // Remove from any previous parent
                _cachedPropertyPanel.Parent?.Controls.Remove(_cachedPropertyPanel);

                // Add to new container
                container.Controls.Add(_cachedPropertyPanel);
            }

            return yPosition;
        }

        /// <summary>
        /// Indicates if element handles its own common controls.
        /// </summary>
        public virtual bool HandlesOwnCommonControls => false;

        public static Dictionary<string, Control> CreateCommonPropertyControls(
            Panel container,
            BaseElement element,
            int yPosition,
            Action onPropertyChanged,
            out Dictionary<string, Action> updateActions)
        {
            Dictionary<string, Control> controls = [];
            updateActions = [];

            // Name property
            AddPropertyLabel(container, "Name:", yPosition);
            Guna2TextBox nameTextBox = AddPropertyTextBox(container, element?.DisplayName ?? "", yPosition,
                value =>
                {
                    if (element != null)
                    {
                        element.DisplayName = value;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                });
            controls["Name"] = nameTextBox;
            updateActions["Name"] = () => nameTextBox.Text = element?.DisplayName ?? "";
            yPosition += ControlRowHeight;

            // X position
            AddPropertyLabel(container, "X:", yPosition);
            Guna2NumericUpDown xNumeric = AddPropertyNumericUpDown(container, element?.Bounds.X ?? 0, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle bounds = element.Bounds;
                        bounds.X = (int)value;
                        element.Bounds = bounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 0, 9999);
            controls["X"] = xNumeric;
            updateActions["X"] = () => xNumeric.Value = element?.Bounds.X ?? 0;
            yPosition += ControlRowHeight;

            // Y position
            AddPropertyLabel(container, "Y:", yPosition);
            Guna2NumericUpDown yNumeric = AddPropertyNumericUpDown(container, element?.Bounds.Y ?? 0, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle bounds = element.Bounds;
                        bounds.Y = (int)value;
                        element.Bounds = bounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 0, 9999);
            controls["Y"] = yNumeric;
            updateActions["Y"] = () => yNumeric.Value = element?.Bounds.Y ?? 0;
            yPosition += ControlRowHeight;

            // Width
            AddPropertyLabel(container, "Width:", yPosition);
            Guna2NumericUpDown widthNumeric = AddPropertyNumericUpDown(container, element?.Bounds.Width ?? 100, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle bounds = element.Bounds;
                        bounds.Width = Math.Max(50, (int)value);
                        element.Bounds = bounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 50, 9999);
            controls["Width"] = widthNumeric;
            updateActions["Width"] = () => widthNumeric.Value = element?.Bounds.Width ?? 100;
            yPosition += ControlRowHeight;

            // Height
            AddPropertyLabel(container, "Height:", yPosition);
            Guna2NumericUpDown heightNumeric = AddPropertyNumericUpDown(container, element?.Bounds.Height ?? 100, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle bounds = element.Bounds;
                        bounds.Height = Math.Max(30, (int)value);
                        element.Bounds = bounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 30, 9999);
            controls["Height"] = heightNumeric;
            updateActions["Height"] = () => heightNumeric.Value = element?.Bounds.Height ?? 100;

            return controls;
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
                Location = new Point(10, yPosition + 8),
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
                Size = new Size(container.Width - 95, ControlHeight),  // 95 accounts for the label width of 85 + 10px padding
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
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

            numericUpDown.DisableScrollAndForwardToPanel();

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
                Size = new Size(container.Width - 95, ControlHeight),  // 95 accounts for the label width of 85 + 10px padding
                ItemHeight = 39,  // Needed to make the height equal to ControlHeight
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
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

            comboBox.DisableScrollAndForwardToPanel();

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
            // Add the checkbox
            Guna2CustomCheckBox checkBox = new()
            {
                Location = new Point(10, yPosition + 6),
                Size = new Size(22, 22),
                Animated = true,
                Checked = isChecked  // This must be set after 'Animated' because Guna is bugged
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
                Padding = new Padding(5),
                AutoSize = true
            };

            // Clicking the label toggles the checkbox
            label.Click += (s, e) => { checkBox.Checked = !checkBox.Checked; };
            container.Controls.Add(label);

            return checkBox;
        }

        public static Panel Tab_Panel { get; private set; }

        /// <summary>
        /// Creates tab buttons for organizing properties.
        /// </summary>
        protected static Panel CreateTabButtons(Panel container, string[] tabNames, Action<int> onTabChanged)
        {
            Tab_Panel = new()
            {
                Location = new Point(0, 0),
                Size = new Size(container.Width, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int buttonWidth = (container.Width - 10) / tabNames.Length;
            for (int i = 0; i < tabNames.Length; i++)
            {
                int tabIndex = i;
                Guna2Button tabButton = new()
                {
                    Text = tabNames[i],
                    Size = new Size(buttonWidth - 2, 30),
                    Location = new Point(5 + (i * buttonWidth), 2),
                    BorderRadius = 4,
                    BorderThickness = 1,
                    Font = new Font("Segoe UI", 9),
                    Tag = i,
                    FillColor = i == 0 ? CustomColors.AccentBlue : CustomColors.ControlBack,  // Select general button by default
                    BorderColor = i == 0 ? CustomColors.AccentBlue : CustomColors.ControlBorder
                };

                tabButton.Click += (s, e) =>
                {
                    foreach (Control control in Tab_Panel.Controls)
                    {
                        if (control is Guna2Button btn)
                        {
                            if (btn.Tag is int tagValue)
                            {
                                btn.FillColor = tagValue == tabIndex ? CustomColors.AccentBlue : CustomColors.ControlBack;
                                btn.BorderColor = tagValue == tabIndex ? CustomColors.AccentBlue : CustomColors.ControlBorder;
                            }
                        }
                    }
                    onTabChanged(tabIndex);
                };

                Tab_Panel.Controls.Add(tabButton);
            }

            container.Controls.Add(Tab_Panel);
            return Tab_Panel;
        }

        /// <summary>
        /// Adds font style toggle buttons (Bold, Italic, Underline) to the container.
        /// </summary>
        protected static void AddFontStyleToggleButtons(
            Panel container,
            int yPosition,
            FontStyle currentStyle,
        Action<FontStyle> onStyleChanged)
        {
            int xPosition = 85;
            const int buttonWidth = 35;
            const int buttonHeight = 30;
            const int spacing = 5;
            int buttonY = yPosition + 2;

            // Bold button
            Guna2Button boldButton = CreateFontStyleButton(
                "B", FontStyle.Bold, xPosition, buttonY, buttonWidth, buttonHeight,
                currentStyle, onStyleChanged);
            container.Controls.Add(boldButton);
            xPosition += buttonWidth + spacing;

            // Italic button
            Guna2Button italicButton = CreateFontStyleButton(
                "I", FontStyle.Italic, xPosition, buttonY, buttonWidth, buttonHeight,
                currentStyle, onStyleChanged);
            container.Controls.Add(italicButton);
            xPosition += buttonWidth + spacing;

            // Underline button
            Guna2Button underlineButton = CreateFontStyleButton(
                "U", FontStyle.Underline, xPosition, buttonY, buttonWidth, buttonHeight,
                currentStyle, onStyleChanged);
            container.Controls.Add(underlineButton);
        }
        private static Guna2Button CreateFontStyleButton(
            string text,
            FontStyle fontStyle,
            int x,
            int y,
            int width,
            int height,
            FontStyle currentStyle,
            Action<FontStyle> onStyleChanged)
        {
            Guna2Button button = new()
            {
                Size = new Size(width, height),
                Location = new Point(x, y),
                Text = text,
                Font = new Font("Segoe UI", 9, fontStyle),
                BorderRadius = 2,
                ButtonMode = ButtonMode.ToogleButton,
                Checked = currentStyle.HasFlag(fontStyle)
            };

            button.CheckedChanged += (s, e) =>
            {
                FontStyle newStyle = currentStyle;
                if (button.Checked)
                {
                    newStyle |= fontStyle;
                }
                else
                {
                    newStyle &= ~fontStyle;
                }

                currentStyle = newStyle;
                onStyleChanged(newStyle);
            };

            return button;
        }
    }
}