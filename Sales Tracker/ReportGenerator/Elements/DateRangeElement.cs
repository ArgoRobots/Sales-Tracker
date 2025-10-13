using Guna.UI2.WinForms;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Date range element for displaying report date filters.
    /// </summary>
    public class DateRangeElement : BaseElement
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public Color TextColor { get; set; } = Color.Gray;
        public float FontSize { get; set; } = 10f;
        public FontStyle FontStyle { get; set; } = FontStyle.Italic;
        public string FontFamily { get; set; } = "Segoe UI";
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;
        public StringAlignment VerticalAlignment { get; set; } = StringAlignment.Center;

        public override ReportElementType GetElementType() => ReportElementType.DateRange;
        public override BaseElement Clone()
        {
            return new DateRangeElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                DateFormat = DateFormat,
                TextColor = TextColor,
                FontSize = FontSize,
                FontStyle = FontStyle,
                FontFamily = FontFamily,
                Alignment = Alignment,
                VerticalAlignment = VerticalAlignment
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            if (config?.Filters == null) return;

            DateTime? startDate = config.Filters.StartDate;
            DateTime? endDate = config.Filters.EndDate;

            string dateText = "Period: ";
            if (startDate.HasValue && endDate.HasValue)
            {
                dateText += $"{startDate.Value.ToString(DateFormat)} to {endDate.Value.ToString(DateFormat)}";
            }
            else
            {
                dateText += "Not specified";
            }

            using Font font = new(FontFamily, FontSize, FontStyle);
            using SolidBrush brush = new(TextColor);

            StringFormat format = new()
            {
                Alignment = Alignment,
                LineAlignment = VerticalAlignment,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.EllipsisCharacter
            };

            graphics.DrawString(dateText, font, brush, Bounds, format);
        }
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Date format
            AddPropertyLabel(container, "Format:", yPosition);
            Guna2ComboBox formatCombo = AddPropertyComboBox(container, DateFormat, yPosition,
                ["yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "MMM dd, yyyy", "MMMM dd, yyyy"],
                value =>
                {
                    DateFormat = value;
                    onPropertyChanged();
                });
            CacheControl("DateFormat", formatCombo, () => formatCombo.SelectedItem = DateFormat);
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
            }, 6, 24);
            CacheControl("FontSize", sizeNumeric, () => sizeNumeric.Value = (decimal)FontSize);
            yPosition += ControlRowHeight;

            // Font style toggle buttons
            AddPropertyLabel(container, "Style:", yPosition);
            AddFontStyleToggleButtons(container, yPosition, FontStyle, style =>
            {
                FontStyle = style;
                onPropertyChanged();
            });
            yPosition += ControlRowHeight;

            // Horizontal alignment
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

        // Helper methods
        private static string AlignmentToDisplayText(StringAlignment alignment)
        {
            return alignment switch
            {
                StringAlignment.Near => "Left",
                StringAlignment.Center => "Center",
                StringAlignment.Far => "Right",
                _ => "Left"
            };
        }
        private static StringAlignment DisplayTextToAlignment(string displayText)
        {
            return displayText switch
            {
                "Left" => StringAlignment.Near,
                "Center" => StringAlignment.Center,
                "Right" => StringAlignment.Far,
                _ => StringAlignment.Near
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