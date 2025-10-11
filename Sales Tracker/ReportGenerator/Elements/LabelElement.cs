using Guna.UI2.WinForms;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Text label element for displaying text content.
    /// </summary>
    public class LabelElement : BaseElement
    {
        public string Text { get; set; } = "Sample Text";
        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 12f;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
        public Color TextColor { get; set; } = Color.Black;
        public StringAlignment Alignment { get; set; } = StringAlignment.Center;
        public StringAlignment VerticalAlignment { get; set; } = StringAlignment.Center;

        public override ReportElementType GetElementType() => ReportElementType.TextLabel;
        public override BaseElement Clone()
        {
            return new LabelElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
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
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                using Font font = new(FontFamily, FontSize, FontStyle);
                using SolidBrush brush = new(TextColor);

                StringFormat format = new()
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
        public override void DrawDesignerElement(Graphics graphics)
        {
            // Draw background
            using SolidBrush bgBrush = new(Color.LightYellow);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(bgBrush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            // Draw text with actual settings
            RenderElement(graphics, null);
        }
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Text content
            AddPropertyLabel(container, "Text:", yPosition);
            Guna2TextBox textBox = AddPropertyTextBox(container, Text, yPosition, value =>
            {
                Text = value;
                onPropertyChanged();
            });
            CacheControl("Text", textBox, () => textBox.Text = Text);
            yPosition += ControlRowHeight;

            // Font family
            AddPropertyLabel(container, "Font:", yPosition);
            Guna2ComboBox fontCombo = AddPropertyComboBox(container, FontFamily, yPosition,
                ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana"],
                value =>
                {
                    FontFamily = value;
                    onPropertyChanged();
                });
            CacheControl("FontFamily", fontCombo, () => fontCombo.SelectedItem = FontFamily);
            yPosition += ControlRowHeight;

            // Font size
            AddPropertyLabel(container, "Size:", yPosition);
            Guna2NumericUpDown sizeNumeric = AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition, value =>
            {
                FontSize = (float)value;
                onPropertyChanged();
            }, 6, 72);
            CacheControl("FontSize", sizeNumeric, () => sizeNumeric.Value = (decimal)FontSize);
            yPosition += ControlRowHeight;

            // Font style toggle buttons
            AddPropertyLabel(container, "Style:", yPosition);
            AddFontStyleToggleButtons(container, yPosition, FontStyle, style =>
            {
                FontStyle = style;
                onPropertyChanged();
            });
            // Note: Font style buttons need special handling for updates
            yPosition += ControlRowHeight;

            // Text alignment
            AddPropertyLabel(container, "Align:", yPosition);
            Guna2ComboBox alignCombo = AddPropertyComboBox(container, AlignmentToDisplayText(Alignment), yPosition,
                ["Left", "Center", "Right"],
                value =>
                {
                    Alignment = DisplayTextToAlignment(value);
                    onPropertyChanged();
                });
            CacheControl("Alignment", alignCombo, () => alignCombo.SelectedItem = AlignmentToDisplayText(Alignment));
            yPosition += ControlRowHeight;

            // Vertical alignment
            AddPropertyLabel(container, "V-Align:", yPosition);
            Guna2ComboBox vAlignCombo = AddPropertyComboBox(container, VerticalAlignmentToDisplayText(VerticalAlignment), yPosition,
                ["Top", "Middle", "Bottom"],
                value =>
                {
                    VerticalAlignment = DisplayTextToVerticalAlignment(value);
                    onPropertyChanged();
                });
            CacheControl("VerticalAlignment", vAlignCombo, () => vAlignCombo.SelectedItem = VerticalAlignmentToDisplayText(VerticalAlignment));
            yPosition += ControlRowHeight;

            // Text color
            AddPropertyLabel(container, "Color:", yPosition);
            Panel colorPanel = AddColorPicker(container, yPosition, 85, TextColor, color =>
            {
                TextColor = color;
                onPropertyChanged();
            });
            CacheControl("TextColor", colorPanel, () => colorPanel.BackColor = TextColor);
            yPosition += ControlRowHeight;

            return yPosition;
        }
        private static string AlignmentToDisplayText(StringAlignment alignment)
        {
            return alignment switch
            {
                StringAlignment.Near => "Left",
                StringAlignment.Center => "Center",
                StringAlignment.Far => "Right",
                _ => "Center"
            };
        }
        private static StringAlignment DisplayTextToAlignment(string displayText)
        {
            return displayText switch
            {
                "Left" => StringAlignment.Near,
                "Center" => StringAlignment.Center,
                "Right" => StringAlignment.Far,
                _ => StringAlignment.Center
            };
        }
        private static string VerticalAlignmentToDisplayText(StringAlignment alignment)
        {
            return alignment switch
            {
                StringAlignment.Near => "Top",
                StringAlignment.Center => "Middle",
                StringAlignment.Far => "Bottom",
                _ => "Middle"
            };
        }
        private static StringAlignment DisplayTextToVerticalAlignment(string displayText)
        {
            return displayText switch
            {
                "Top" => StringAlignment.Near,
                "Middle" => StringAlignment.Center,
                "Bottom" => StringAlignment.Far,
                _ => StringAlignment.Center
            };
        }
    }
}