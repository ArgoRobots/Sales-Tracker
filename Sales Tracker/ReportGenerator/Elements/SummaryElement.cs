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
            // Checkboxes for included metrics
            AddPropertyLabel(container, "Include:", yPosition);
            yPosition += 20;

            CheckBox salesCheck = new()
            {
                Text = "Total Sales",
                Checked = ShowTotalSales,
                Location = new Point(85, yPosition),
                Size = new Size(150, 20)
            };
            salesCheck.CheckedChanged += (s, e) =>
            {
                ShowTotalSales = salesCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(salesCheck);
            yPosition += RowHeight;

            CheckBox transactionsCheck = new()
            {
                Text = "Total Transactions",
                Checked = ShowTotalTransactions,
                Location = new Point(85, yPosition),
                Size = new Size(150, 20)
            };
            transactionsCheck.CheckedChanged += (s, e) =>
            {
                ShowTotalTransactions = transactionsCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(transactionsCheck);
            yPosition += RowHeight;

            CheckBox avgValueCheck = new()
            {
                Text = "Average Value",
                Checked = ShowAverageValue,
                Location = new Point(85, yPosition),
                Size = new Size(150, 20)
            };
            avgValueCheck.CheckedChanged += (s, e) =>
            {
                ShowAverageValue = avgValueCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(avgValueCheck);
            yPosition += RowHeight;

            CheckBox growthCheck = new()
            {
                Text = "Growth Rate",
                Checked = ShowGrowthRate,
                Location = new Point(85, yPosition),
                Size = new Size(150, 20)
            };
            growthCheck.CheckedChanged += (s, e) =>
            {
                ShowGrowthRate = growthCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(growthCheck);
            yPosition += RowHeight + 10;

            return yPosition;
        }
        protected override Color GetDesignerColor() => Color.LightCyan;
    }
}
