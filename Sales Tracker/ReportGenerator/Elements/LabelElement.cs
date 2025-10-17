using Guna.UI2.WinForms;
using Sales_Tracker.ReportGenerator.Menus;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Text label element for displaying text content.
    /// </summary>
    public class LabelElement : BaseElement
    {
        // Properties
        public string Text { get; set; } = "Sample Text";
        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 12f;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
        public Color TextColor { get; set; } = Color.Black;
        public StringAlignment Alignment { get; set; } = StringAlignment.Center;
        public StringAlignment VerticalAlignment { get; set; } = StringAlignment.Center;

        // Overrides
        public override string DisplayName => "Label";
        public override ReportElementType GetElementType() => ReportElementType.Label;
        public override BaseElement Clone()
        {
            return new LabelElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                Text = Text,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle,
                TextColor = TextColor,
                Alignment = Alignment,
                VerticalAlignment = VerticalAlignment
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config, float renderScale)
        {
            try
            {
                using Font font = new(FontFamily, FontSize, FontStyle);
                using SolidBrush brush = new(TextColor);

                using StringFormat format = new()
                {
                    Alignment = Alignment,
                    LineAlignment = VerticalAlignment,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                graphics.DrawString(Text, font, brush, Bounds, format);
            }
            catch (ArgumentException)
            {
                // Fallback if font family is not available
                using Font fallbackFont = new("Segoe UI", FontSize, FontStyle);
                using SolidBrush fallbackBrush = new(TextColor);

                StringFormat fallbackFormat = new()
                {
                    Alignment = Alignment,
                    LineAlignment = VerticalAlignment,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                graphics.DrawString(Text, fallbackFont, fallbackBrush, Bounds, fallbackFormat);
            }
        }
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Get undo manager for recording property changes
            UndoRedoManager? undoRedoManager = ReportLayoutDesigner_Form.Instance?.GetUndoRedoManager();

            // Text content
            AddPropertyLabel(container, "Text:", yPosition);
            Guna2TextBox textBox = AddPropertyTextBox(container, Text, yPosition,
                newText =>
                {
                    if (Text != newText && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(Text),
                            Text,
                            newText,
                            onPropertyChanged));
                    }
                    Text = newText;
                    onPropertyChanged();
                });
            CacheControl("Text", textBox, () => textBox.Text = Text);
            yPosition += ControlRowHeight;

            // Font Family
            AddPropertyLabel(container, "Font:", yPosition);
            string[] fontFamilies = ["Arial", "Calibri", "Cambria", "Comic Sans MS", "Consolas",
                             "Courier New", "Georgia", "Impact", "Segoe UI", "Tahoma",
                             "Times New Roman", "Trebuchet MS", "Verdana"];
            Guna2ComboBox fontCombo = AddPropertyComboBox(container, FontFamily, yPosition, fontFamilies,
                value =>
                {
                    if (FontFamily != value && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontFamily),
                            FontFamily,
                            value,
                            onPropertyChanged));
                    }
                    FontFamily = value;
                    onPropertyChanged();
                });
            CacheControl("FontFamily", fontCombo, () => fontCombo.SelectedItem = FontFamily);
            yPosition += ControlRowHeight;

            // Font Size
            AddPropertyLabel(container, "Size:", yPosition);
            Guna2NumericUpDown fontSizeNumeric = AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition,
                value =>
                {
                    float newFontSize = (float)value;
                    if (Math.Abs(FontSize - newFontSize) > 0.01f && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontSize),
                            FontSize,
                            newFontSize,
                            onPropertyChanged));
                    }
                    FontSize = newFontSize;
                    onPropertyChanged();
                }, 6, 72);
            CacheControl("FontSize", fontSizeNumeric, () => fontSizeNumeric.Value = (decimal)FontSize);
            yPosition += ControlRowHeight;

            // Font Style (Bold, Italic, Underline)
            AddPropertyLabel(container, "Style:", yPosition);

            // Store references to the font style buttons for the update action
            Guna2Button boldButton = null;
            Guna2Button italicButton = null;
            Guna2Button underlineButton = null;

            // Create the font style buttons container
            int styleButtonsY = yPosition;
            int xPosition = 85;
            const int buttonWidth = 35;
            const int buttonHeight = 30;
            const int spacing = 5;
            int buttonY = yPosition + 2;

            // Bold button
            boldButton = CreateFontStyleButton("B", FontStyle.Bold, xPosition, buttonY, buttonWidth, buttonHeight);
            boldButton.Checked = FontStyle.HasFlag(FontStyle.Bold);
            boldButton.Tag = "Bold";  // Tag for identification
            container.Controls.Add(boldButton);
            xPosition += buttonWidth + spacing;

            // Italic button
            italicButton = CreateFontStyleButton("I", FontStyle.Italic, xPosition, buttonY, buttonWidth, buttonHeight);
            italicButton.Checked = FontStyle.HasFlag(FontStyle.Italic);
            italicButton.Tag = "Italic";  // Tag for identification
            container.Controls.Add(italicButton);
            xPosition += buttonWidth + spacing;

            // Underline button
            underlineButton = CreateFontStyleButton("U", FontStyle.Underline, xPosition, buttonY, buttonWidth, buttonHeight);
            underlineButton.Checked = FontStyle.HasFlag(FontStyle.Underline);
            underlineButton.Tag = "Underline";  // Tag for identification
            container.Controls.Add(underlineButton);

            // Store the buttons in cache for later updates
            CacheControl("BoldButton", boldButton, () => boldButton.Checked = FontStyle.HasFlag(FontStyle.Bold));
            CacheControl("ItalicButton", italicButton, () => italicButton.Checked = FontStyle.HasFlag(FontStyle.Italic));
            CacheControl("UnderlineButton", underlineButton, () => underlineButton.Checked = FontStyle.HasFlag(FontStyle.Underline));

            // Attach event handlers to update the FontStyle property
            FontStyle currentFontStyle = FontStyle;

            boldButton.CheckedChanged += (s, e) =>
            {
                FontStyle newStyle = currentFontStyle;
                if (boldButton.Checked)
                {
                    newStyle |= FontStyle.Bold;
                }
                else
                {
                    newStyle &= ~FontStyle.Bold;
                }

                if (currentFontStyle != newStyle)
                {
                    undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontStyle),
                            FontStyle,
                            newStyle,
                            onPropertyChanged));
                    FontStyle = newStyle;
                    currentFontStyle = newStyle;
                    onPropertyChanged();
                }
            };

            italicButton.CheckedChanged += (s, e) =>
            {
                FontStyle newStyle = currentFontStyle;
                if (italicButton.Checked)
                {
                    newStyle |= FontStyle.Italic;
                }
                else
                {
                    newStyle &= ~FontStyle.Italic;
                }

                if (currentFontStyle != newStyle)
                {
                    undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontStyle),
                            FontStyle,
                            newStyle,
                            onPropertyChanged));
                    FontStyle = newStyle;
                    currentFontStyle = newStyle;
                    onPropertyChanged();
                }
            };

            underlineButton.CheckedChanged += (s, e) =>
            {
                FontStyle newStyle = currentFontStyle;
                if (underlineButton.Checked)
                {
                    newStyle |= FontStyle.Underline;
                }
                else
                {
                    newStyle &= ~FontStyle.Underline;
                }

                if (currentFontStyle != newStyle)
                {
                    undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontStyle),
                            FontStyle,
                            newStyle,
                            onPropertyChanged));
                    FontStyle = newStyle;
                    currentFontStyle = newStyle;
                    onPropertyChanged();
                }
            };

            yPosition += ControlRowHeight;

            // Text Color
            AddPropertyLabel(container, "Color:", yPosition);
            Panel colorPanel = AddColorPicker(container, yPosition, 85, TextColor,
                newColor =>
                {
                    // Colors need special comparison
                    if (TextColor.ToArgb() != newColor.ToArgb() && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(TextColor),
                            TextColor,
                            newColor,
                            onPropertyChanged));
                    }
                    TextColor = newColor;
                    onPropertyChanged();
                }, false);

            // Add label next to color picker
            Label colorLabel = new()
            {
                Text = "Click to change",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(140, yPosition + 11),
                AutoSize = true
            };
            container.Controls.Add(colorLabel);

            CacheControl("TextColor", colorPanel, () => colorPanel.BackColor = TextColor);
            yPosition += ControlRowHeight;

            // Horizontal Alignment
            AddPropertyLabel(container, "H-Align:", yPosition);
            string[] hAlignmentOptions = ["Near", "Center", "Far"];
            Guna2ComboBox hAlignCombo = AddPropertyComboBox(container, Alignment.ToString(), yPosition, hAlignmentOptions,
                value =>
                {
                    StringAlignment newAlignment = Enum.Parse<StringAlignment>(value);
                    if (Alignment != newAlignment && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(Alignment),
                            Alignment,
                            newAlignment,
                            onPropertyChanged));
                    }
                    Alignment = newAlignment;
                    onPropertyChanged();
                });
            CacheControl("Alignment", hAlignCombo, () => hAlignCombo.SelectedItem = Alignment.ToString());
            yPosition += ControlRowHeight;

            // Vertical Alignment
            AddPropertyLabel(container, "V-Align:", yPosition);
            string[] vAlignmentOptions = ["Near", "Center", "Far"];
            Guna2ComboBox vAlignCombo = AddPropertyComboBox(container, VerticalAlignment.ToString(), yPosition, vAlignmentOptions,
                value =>
                {
                    StringAlignment newVAlignment = Enum.Parse<StringAlignment>(value);
                    if (VerticalAlignment != newVAlignment && undoRedoManager != null)
                    {
                        undoRedoManager.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(VerticalAlignment),
                            VerticalAlignment,
                            newVAlignment,
                            onPropertyChanged));
                    }
                    VerticalAlignment = newVAlignment;
                    onPropertyChanged();
                });
            CacheControl("VerticalAlignment", vAlignCombo, () => vAlignCombo.SelectedItem = VerticalAlignment.ToString());
            yPosition += ControlRowHeight;

            return yPosition;
        }
    }
}