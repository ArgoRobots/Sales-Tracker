using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using Sales_Tracker.Classes;
using Sales_Tracker.Language;
using Sales_Tracker.ReportGenerator.Menus;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Base class for all report elements.
    /// </summary>
    public abstract class BaseElement
    {
        // Properties
        /// <summary>
        /// Gets the user-friendly display name for this element type.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Unique identifier for the element.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Position and size of the element on the report canvas.
        /// </summary>
        public Rectangle Bounds { get; set; }

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
        public static byte ControlRowHeight { get; } = 55;
        public static byte CheckBoxRowHeight { get; } = 40;

        public const int ControlHeight = 45;

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
        public abstract void RenderElement(Graphics graphics, ReportConfiguration config, float renderScale);

        public Panel CachedPropertyPanel { get; private set; }
        private bool _controlsCreated = false;
        private readonly Dictionary<string, Control> _controlCache = [];
        public Dictionary<string, Action> UpdateActionCache { get; } = [];

        /// <summary>
        /// Creates or retrieves cached property controls. Only creates controls once.
        /// </summary>
        public virtual int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Create controls only once
            if (!_controlsCreated)
            {
                CachedPropertyPanel = new Panel
                {
                    Location = new Point(0, 0),
                    AutoSize = false,
                    AutoScroll = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };

                // Create common controls if element doesn't handle its own
                if (!HandlesOwnCommonControls)
                {
                    Dictionary<string, Control> commonControls = CreateCommonPropertyControls(
                        CachedPropertyPanel,
                        this,
                        yPosition,
                        onPropertyChanged,
                        out Dictionary<string, Action> commonUpdateActions);

                    foreach (KeyValuePair<string, Control> kvp in commonControls)
                    {
                        _controlCache[kvp.Key] = kvp.Value;
                    }
                    foreach (KeyValuePair<string, Action> kvp in commonUpdateActions)
                    {
                        UpdateActionCache[kvp.Key] = kvp.Value;
                    }

                    yPosition += ControlRowHeight * 4;
                }

                // Let derived classes create their specific controls
                yPosition = CreateElementSpecificControls(CachedPropertyPanel, yPosition, onPropertyChanged);

                _controlsCreated = true;
            }

            // Only add CachedPropertyPanel if it's not already in the container
            if (CachedPropertyPanel != null && CachedPropertyPanel.Parent != container)
            {
                List<Control> controlsToRemove = container.Controls
                   .Cast<Control>()
                   .Where(c => c != LoadingPanel.BlankLoadingPanelInstance)
                   .ToList();

                foreach (Control ctrl in controlsToRemove)
                {
                    container.Controls.Remove(ctrl);
                }

                CachedPropertyPanel.Size = new Size(container.Width - 5, container.Height);
                container.Controls.Add(CachedPropertyPanel);
            }

            UpdateAllControlValues();

            return yPosition;
        }

        /// <summary>
        /// Creates element-specific property controls. Override this in derived classes.
        /// Store controls and update actions in _controlCache and _updateActionCache.
        /// </summary>
        protected virtual int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            return yPosition;
        }

        /// <summary>
        /// Updates all cached control values without recreating them.
        /// </summary>
        protected void UpdateAllControlValues()
        {
            foreach (Action updateAction in UpdateActionCache.Values)
            {
                updateAction?.Invoke();
            }
        }

        /// <summary>
        /// Adds a control and its update action to the cache.
        /// </summary>
        protected void CacheControl(string key, Control control, Action updateAction)
        {
            _controlCache[key] = control;
            if (updateAction != null)
            {
                UpdateActionCache[key] = updateAction;
            }
        }

        /// <summary>
        /// Clears the cached property panel, forcing recreation on next CreatePropertyControls call.
        /// Used when language changes to recreate controls with new translations.
        /// </summary>
        public void ClearPropertyControlCache()
        {
            _controlsCreated = false;
            CachedPropertyPanel?.Dispose();
            CachedPropertyPanel = null;
            _controlCache.Clear();
            UpdateActionCache.Clear();
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
            string text;

            // Get the UndoRedoManager instance
            UndoRedoManager undoRedoManager = ReportLayoutDesigner_Form.Instance?.GetUndoRedoManager();

            // X position
            AddPropertyLabel(container, "X:", yPosition);
            int oldX = element?.Bounds.X ?? 0;
            Guna2NumericUpDown xNumeric = AddPropertyNumericUpDown(container, oldX, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle oldBounds = element.Bounds;
                        Rectangle newBounds = element.Bounds;
                        newBounds.X = (int)value;

                        // Record undo action if value actually changed
                        if (oldBounds.X != newBounds.X && undoRedoManager != null)
                        {
                            undoRedoManager.RecordAction(new PropertyChangeAction(
                                element,
                                nameof(Bounds),
                                oldBounds,
                                newBounds,
                                () =>
                                {
                                    container.Invalidate();
                                    onPropertyChanged();
                                }));
                        }

                        element.Bounds = newBounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 0, 9999);
            controls["X"] = xNumeric;
            updateActions["X"] = () => xNumeric.Value = element?.Bounds.X ?? 0;
            yPosition += ControlRowHeight;

            // Y position
            AddPropertyLabel(container, "Y:", yPosition);
            int oldY = element?.Bounds.Y ?? 0;
            Guna2NumericUpDown yNumeric = AddPropertyNumericUpDown(container, oldY, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle oldBounds = element.Bounds;
                        Rectangle newBounds = element.Bounds;
                        newBounds.Y = (int)value;

                        if (oldBounds.Y != newBounds.Y && undoRedoManager != null)
                        {
                            undoRedoManager.RecordAction(new PropertyChangeAction(
                                element,
                                nameof(Bounds),
                                oldBounds,
                                newBounds,
                                () =>
                                {
                                    container.Invalidate();
                                    onPropertyChanged();
                                }));
                        }

                        element.Bounds = newBounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 0, 9999);
            controls["Y"] = yNumeric;
            updateActions["Y"] = () => yNumeric.Value = element?.Bounds.Y ?? 0;
            yPosition += ControlRowHeight;

            // Width
            text = LanguageManager.TranslateString("Width");
            AddPropertyLabel(container, text + ":", yPosition);
            int oldWidth = element?.Bounds.Width ?? 100;
            Guna2NumericUpDown widthNumeric = AddPropertyNumericUpDown(container, oldWidth, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle oldBounds = element.Bounds;
                        Rectangle newBounds = element.Bounds;
                        newBounds.Width = (int)value;

                        if (oldBounds.Width != newBounds.Width && undoRedoManager != null)
                        {
                            undoRedoManager.RecordAction(new PropertyChangeAction(
                                element,
                                nameof(Bounds),
                                oldBounds,
                                newBounds,
                                () =>
                                {
                                    container.Invalidate();
                                    onPropertyChanged();
                                }));
                        }

                        element.Bounds = newBounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 10, 9999);
            controls["Width"] = widthNumeric;
            updateActions["Width"] = () => widthNumeric.Value = element?.Bounds.Width ?? 100;
            yPosition += ControlRowHeight;

            // Height
            text = LanguageManager.TranslateString("Height");
            AddPropertyLabel(container, text + ":", yPosition);
            int oldHeight = element?.Bounds.Height ?? 100;
            Guna2NumericUpDown heightNumeric = AddPropertyNumericUpDown(container, oldHeight, yPosition,
                value =>
                {
                    if (element != null)
                    {
                        Rectangle oldBounds = element.Bounds;
                        Rectangle newBounds = element.Bounds;
                        newBounds.Height = (int)value;

                        if (oldBounds.Height != newBounds.Height && undoRedoManager != null)
                        {
                            undoRedoManager.RecordAction(new PropertyChangeAction(
                                element,
                                nameof(Bounds),
                                oldBounds,
                                newBounds,
                                () =>
                                {
                                    container.Invalidate();
                                    onPropertyChanged();
                                }));
                        }

                        element.Bounds = newBounds;
                        container.Invalidate();
                        onPropertyChanged();
                    }
                }, 10, 9999);
            controls["Height"] = heightNumeric;
            updateActions["Height"] = () => heightNumeric.Value = element?.Bounds.Height ?? 100;

            return controls;
        }

        /// <summary>
        /// Adds a property label to the container with overflow handling.
        /// </summary>
        public static Label AddPropertyLabel(Panel container, string text, int yPosition, bool bold = false)
        {
            const int leftMargin = 10;
            const int rightMargin = 10;
            const int scrollBarWidth = 20; // Account for potential scrollbar
            const int controlMinWidth = 120; // Minimum space for the control on the right

            // Calculate maximum width for label (leaving space for control on right)
            int maxLabelWidth = container.Width - leftMargin - rightMargin - scrollBarWidth - controlMinWidth;
            if (maxLabelWidth < 50) maxLabelWidth = 50; // Ensure minimum label width

            Font labelFont = new("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular);

            // Measure text with current font
            using (Graphics g = container.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(text, labelFont);

                // If text is too wide, try smaller font first
                if (textSize.Width > maxLabelWidth)
                {
                    labelFont = new Font("Segoe UI", 8, bold ? FontStyle.Bold : FontStyle.Regular);
                }
            }

            Label label = new()
            {
                Text = text,
                Font = labelFont,
                ForeColor = CustomColors.Text,
                Location = new Point(leftMargin, yPosition + 8),
                AutoSize = false,
                Size = new Size(maxLabelWidth, 20),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            container.Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Adds a text box property control aligned to the right.
        /// </summary>
        public static Guna2TextBox AddPropertyTextBox(Panel container, string value, int yPosition, Action<string> onChange)
        {
            const int rightMargin = 10;
            const int scrollBarWidth = 20;
            const int controlWidth = 170; // Width of the textbox

            int xPosition = container.Width - controlWidth - rightMargin - scrollBarWidth;

            Guna2TextBox textBox = new()
            {
                Text = value,
                Size = new Size(controlWidth, ControlHeight),
                Location = new Point(xPosition, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            textBox.TextChanged += (s, e) => onChange(textBox.Text);
            container.Controls.Add(textBox);
            return textBox;
        }

        /// <summary>
        /// Adds a numeric up/down property control aligned to the right.
        /// </summary>
        public static Guna2NumericUpDown AddPropertyNumericUpDown(Panel container, decimal value, int yPosition, Action<decimal> onChange, decimal min = 0, decimal max = 9999)
        {
            const int rightMargin = 10;
            const int scrollBarWidth = 20;
            const int controlWidth = 100;

            int xPosition = container.Width - controlWidth - rightMargin - scrollBarWidth;

            Guna2NumericUpDown numericUpDown = new()
            {
                Size = new Size(controlWidth, ControlHeight),
                Location = new Point(xPosition, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Minimum = min,
                Maximum = max,
                Value = value,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            numericUpDown.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Remove Windows "ding" noise when user presses enter
                    e.SuppressKeyPress = true;
                }
            };

            numericUpDown.DisableScrollAndForwardToPanel();

            numericUpDown.ValueChanged += (s, e) => onChange(numericUpDown.Value);
            container.Controls.Add(numericUpDown);
            return numericUpDown;
        }

        /// <summary>
        /// Adds a combo box property control aligned to the right.
        /// </summary>
        protected static Guna2ComboBox AddPropertyComboBox(Panel container, string value, int yPosition, string[] items, Action<string> onChange)
        {
            const int rightMargin = 10;
            const int scrollBarWidth = 20;
            const int controlWidth = 170; // Width of the combobox

            int xPosition = container.Width - controlWidth - rightMargin - scrollBarWidth;

            Guna2ComboBox comboBox = new()
            {
                Size = new Size(controlWidth, ControlHeight),
                ItemHeight = 39,  // Needed to make the height equal to ControlHeight
                Location = new Point(xPosition, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
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

        public Panel Tab_Panel { get; private set; }
        public static readonly string ColorPickerTag = "ColorPicker";

        /// <summary>
        /// Adds a color picker control with an optional label, aligned to the right.
        /// </summary>
        protected static Panel AddColorPicker(
            Panel container,
            int yPosition,
            Color currentColor,
            Action<Color> onColorChanged,
            bool showLabel = true)
        {
            const int rightMargin = 10;
            const int scrollBarWidth = 20;
            const int colorPickerWidth = 50;
            const int labelWidth = 100; // Approximate width for "Click to change" label

            int totalWidth = showLabel ? colorPickerWidth + 5 + labelWidth : colorPickerWidth;
            int xPosition = container.Width - totalWidth - rightMargin - scrollBarWidth;

            Panel colorPreview = new()
            {
                BackColor = currentColor,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(colorPickerWidth, 30),
                Location = new Point(xPosition, yPosition + 8),
                Cursor = Cursors.Hand,
                Tag = ColorPickerTag,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            colorPreview.Click += (s, e) =>
            {
                ColorDialog colorDialog = new()
                {
                    Color = colorPreview.BackColor,
                    FullOpen = true
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    colorPreview.BackColor = colorDialog.Color;
                    onColorChanged(colorDialog.Color);
                }
            };

            container.Controls.Add(colorPreview);

            if (showLabel)
            {
                Label colorLabel = new()
                {
                    Text = LanguageManager.TranslateString("Click to change"),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.Gray,
                    Location = new Point(xPosition + 55, yPosition + 11),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                container.Controls.Add(colorLabel);
            }

            return colorPreview;
        }

        /// <summary>
        /// Creates tab buttons for organizing properties.
        /// </summary>
        protected Panel CreateTabButtons(string[] tabNames, Action<int> onTabChanged)
        {
            int parentWidth = ReportLayoutDesigner_Form.Instance.PropertiesContainer_Panel.Width;

            Tab_Panel = new()
            {
                Location = new Point(0, 0),
                Size = new Size(parentWidth, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int buttonWidth = (parentWidth - 10) / tabNames.Length;
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

            CachedPropertyPanel.Controls.Add(Tab_Panel);
            return Tab_Panel;
        }
        public static Guna2Button CreateFontStyleButton(
            string text,
            FontStyle fontStyle,
            int x,
            int y,
            int width,
            int height)
        {
            Guna2Button button = new()
            {
                Size = new Size(width, height),
                Location = new Point(x, y),
                Text = text,
                Font = new Font("Segoe UI", 9, fontStyle),
                BorderRadius = 2,
                ButtonMode = ButtonMode.ToogleButton,
                CheckedState =
                {
                    FillColor = CustomColors.AccentBlue,
                    ForeColor = Color.White
                }
            };

            return button;
        }
    }
}