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

        public override ReportElementType GetElementType() => ReportElementType.DateRange;
        public override BaseElement Clone()
        {
            return new DateRangeElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                DateFormat = DateFormat,
                TextColor = TextColor,
                FontSize = FontSize,
                FontStyle = FontStyle
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

            using Font font = new("Segoe UI", FontSize, FontStyle);
            using SolidBrush brush = new(TextColor);
            graphics.DrawString(dateText, font, brush, Bounds);
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.LightGreen);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Black);
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(DisplayName ?? "Date Range", font, textBrush, Bounds, format);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Date format
            AddPropertyLabel(container, "Format:", yPosition);
            AddPropertyComboBox(container, DateFormat, yPosition,
                ["yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "MMM dd, yyyy", "MMMM dd, yyyy"],
                value =>
                {
                    DateFormat = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Font size
            AddPropertyLabel(container, "Size:", yPosition);
            AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition, value =>
            {
                FontSize = (float)value;
                onPropertyChanged();
            }, 6, 24);
            yPosition += RowHeight;

            return yPosition;
        }
        protected override Color GetDesignerColor() => Color.LightGreen;
    }
}