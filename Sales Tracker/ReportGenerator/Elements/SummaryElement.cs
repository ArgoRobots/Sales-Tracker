namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Summary element for displaying statistics and key metrics.
    /// </summary>
    public class SummaryElement : BaseElement
    {
        public bool ShowTotalSales { get; set; } = true;
        public bool ShowTotalTransactions { get; set; } = true;
        public bool ShowAverageValue { get; set; } = true;
        public bool ShowGrowthRate { get; set; } = false;
        public Color BackgroundColor { get; set; } = Color.WhiteSmoke;
        public Color BorderColor { get; set; } = Color.LightGray;

        public override ReportElementType GetElementType() => ReportElementType.Summary;
        public override BaseElement Clone()
        {
            return new SummaryElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ShowTotalSales = ShowTotalSales,
                ShowTotalTransactions = ShowTotalTransactions,
                ShowAverageValue = ShowAverageValue,
                ShowGrowthRate = ShowGrowthRate,
                BackgroundColor = BackgroundColor,
                BorderColor = BorderColor
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            // Draw background
            using SolidBrush bgBrush = new(BackgroundColor);
            graphics.FillRectangle(bgBrush, Bounds);

            // Draw border
            using Pen borderPen = new(BorderColor, 1);
            graphics.DrawRectangle(borderPen, Bounds);

            // Draw summary content (placeholder for now)
            using Font titleFont = new("Segoe UI", 11, FontStyle.Bold);
            using Font valueFont = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);

            int y = Bounds.Y + 10;
            int x = Bounds.X + 10;

            graphics.DrawString("Summary Statistics", titleFont, textBrush, x, y);
            y += 25;

            if (ShowTotalSales)
            {
                graphics.DrawString("Total Sales: $XXX,XXX", valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowTotalTransactions)
            {
                graphics.DrawString("Transactions: XXX", valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowAverageValue)
            {
                graphics.DrawString("Average Value: $XXX", valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowGrowthRate)
            {
                graphics.DrawString("Growth Rate: XX%", valueFont, textBrush, x, y);
            }
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.LightCyan);
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

            graphics.DrawString(DisplayName ?? "Summary", font, textBrush, Bounds, format);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Section header for included metrics
            AddPropertyLabel(container, "Include:", yPosition, true);
            yPosition += 25;

            // Total Sales checkbox with clickable label
            AddPropertyCheckBoxWithLabel(container, "Total Sales", ShowTotalSales, yPosition,
                value =>
                {
                    ShowTotalSales = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Total Transactions checkbox
            AddPropertyCheckBoxWithLabel(container, "Total Transactions", ShowTotalTransactions, yPosition,
                value =>
                {
                    ShowTotalTransactions = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Average Value checkbox
            AddPropertyCheckBoxWithLabel(container, "Average Value", ShowAverageValue, yPosition,
                value =>
                {
                    ShowAverageValue = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Growth Rate checkbox
            AddPropertyCheckBoxWithLabel(container, "Growth Rate", ShowGrowthRate, yPosition,
                value =>
                {
                    ShowGrowthRate = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight + 10;

            return yPosition;
        }
        protected override Color GetDesignerColor() => Color.LightCyan;
    }
}