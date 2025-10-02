using Sales_Tracker.DataClasses;

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
        public bool ShowGrowthRate { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.WhiteSmoke;
        public Color BorderColor { get; set; } = Color.LightGray;

        private class SummaryStatistics
        {
            public decimal TotalSales { get; set; }
            public decimal TotalPurchases { get; set; }
            public decimal CombinedTotal { get; set; }
            public int TransactionCount { get; set; }
            public decimal AverageValue { get; set; }
            public decimal GrowthRate { get; set; }
        }

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

            // Calculate statistics
            SummaryStatistics stats = CalculateStatistics(config);

            // Draw summary content with actual values
            using Font titleFont = new("Segoe UI", 11, FontStyle.Bold);
            using Font valueFont = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);
            using SolidBrush positiveBrush = new(Color.Green);
            using SolidBrush negativeBrush = new(Color.Red);

            int y = Bounds.Y + 10;
            int x = Bounds.X + 10;

            graphics.DrawString("Summary Statistics", titleFont, textBrush, x, y);
            y += 25;

            if (ShowTotalSales)
            {
                string totalText = config?.Filters?.TransactionType switch
                {
                    TransactionType.Sales => $"Total Sales: {FormatCurrency(stats.TotalSales)}",
                    TransactionType.Purchases => $"Total Purchases: {FormatCurrency(stats.TotalPurchases)}",
                    _ => $"Total: {FormatCurrency(stats.CombinedTotal)}"
                };
                graphics.DrawString(totalText, valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowTotalTransactions)
            {
                graphics.DrawString($"Transactions: {stats.TransactionCount:N0}", valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowAverageValue)
            {
                graphics.DrawString($"Average Value: {FormatCurrency(stats.AverageValue)}", valueFont, textBrush, x, y);
                y += 20;
            }

            if (ShowGrowthRate)
            {
                SolidBrush growthBrush = stats.GrowthRate >= 0 ? positiveBrush : negativeBrush;
                string growthSymbol = stats.GrowthRate >= 0 ? "↑" : "↓";
                graphics.DrawString($"Growth Rate: {growthSymbol} {Math.Abs(stats.GrowthRate):F1}%", valueFont, growthBrush, x, y);
            }
        }
        private SummaryStatistics CalculateStatistics(ReportConfiguration config)
        {
            SummaryStatistics stats = new();

            if (config?.Filters == null)
            {
                return stats;
            }

            DateTime startDate = config.Filters.StartDate ?? DateTime.MinValue;
            DateTime endDate = config.Filters.EndDate ?? DateTime.MaxValue;

            try
            {
                // Calculate sales if needed
                if (config.Filters.TransactionType == TransactionType.Sales ||
                    config.Filters.TransactionType == TransactionType.Both)
                {
                    (decimal Total, int Count) = CalculateGridViewTotal(
                        MainMenu_Form.Instance.Sale_DataGridView,
                        startDate,
                        endDate,
                        config.Filters.IncludeReturns);

                    stats.TotalSales = Total;
                    stats.TransactionCount += Count;
                }

                // Calculate purchases if needed
                if (config.Filters.TransactionType == TransactionType.Purchases ||
                    config.Filters.TransactionType == TransactionType.Both)
                {
                    (decimal Total, int Count) = CalculateGridViewTotal(
                        MainMenu_Form.Instance.Purchase_DataGridView,
                        startDate,
                        endDate,
                        config.Filters.IncludeReturns);

                    stats.TotalPurchases = Total;
                    if (config.Filters.TransactionType == TransactionType.Purchases)
                    {
                        stats.TransactionCount += Count;
                    }
                }

                // Calculate derived values
                stats.CombinedTotal = stats.TotalSales + stats.TotalPurchases;
                stats.AverageValue = stats.TransactionCount > 0 ? stats.CombinedTotal / stats.TransactionCount : 0;

                // Calculate growth rate if needed
                if (ShowGrowthRate)
                {
                    stats.GrowthRate = CalculateGrowthRate(config, startDate, endDate, stats.CombinedTotal);
                }
            }
            catch
            {
                // Return default stats on error
            }

            return stats;
        }

        private static (decimal Total, int Count) CalculateGridViewTotal(
            DataGridView gridView,
            DateTime startDate,
            DateTime endDate,
            bool includeReturns)
        {
            decimal total = 0;
            int count = 0;

            foreach (DataGridViewRow row in gridView.Rows)
            {
                if (row.Cells[ReadOnlyVariables.Date_column].Value != null &&
                    DateTime.TryParse(row.Cells[ReadOnlyVariables.Date_column].Value.ToString(), out DateTime rowDate))
                {
                    if (rowDate >= startDate && rowDate <= endDate)
                    {
                        if (row.Cells[ReadOnlyVariables.Total_column].Value != null &&
                            decimal.TryParse(row.Cells[ReadOnlyVariables.Total_column].Value.ToString(), out decimal amount))
                        {
                            bool isReturn = amount < 0;
                            if (!isReturn || includeReturns)
                            {
                                total += Math.Abs(amount);
                                count++;
                            }
                        }
                    }
                }
            }

            return (total, count);
        }

        private static decimal CalculateGrowthRate(
            ReportConfiguration config,
            DateTime startDate,
            DateTime endDate,
            decimal currentPeriodTotal)
        {
            TimeSpan periodLength = endDate - startDate;
            DateTime prevStart = startDate.AddDays(-periodLength.TotalDays);
            DateTime prevEnd = startDate.AddDays(-1);

            decimal previousPeriodTotal = 0;

            // Get previous period sales
            if (config.Filters.TransactionType == TransactionType.Sales ||
                config.Filters.TransactionType == TransactionType.Both)
            {
                (decimal Total, _) = CalculateGridViewTotal(
                    MainMenu_Form.Instance.Sale_DataGridView,
                    prevStart,
                    prevEnd,
                    config.Filters.IncludeReturns);
                previousPeriodTotal += Total;
            }

            // Get previous period purchases
            if (config.Filters.TransactionType == TransactionType.Purchases ||
                config.Filters.TransactionType == TransactionType.Both)
            {
                (decimal Total, _) = CalculateGridViewTotal(
                    MainMenu_Form.Instance.Purchase_DataGridView,
                    prevStart,
                    prevEnd,
                    config.Filters.IncludeReturns);
                previousPeriodTotal += Total;
            }

            if (previousPeriodTotal > 0)
            {
                return ((currentPeriodTotal - previousPeriodTotal) / previousPeriodTotal) * 100;
            }

            return 0;
        }

        private static string FormatCurrency(decimal amount)
        {
            // Format based on size
            if (amount >= 1000000)
            {
                return $"${amount / 1000000:F1}M";
            }
            else if (amount >= 1000)
            {
                return $"${amount / 1000:F1}K";
            }
            else
            {
                return $"${amount:F2}";
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
            yPosition += 35;

            // Total Sales checkbox with clickable label
            AddPropertyCheckBoxWithLabel(container, "Total Sales", ShowTotalSales, yPosition,
                value =>
                {
                    ShowTotalSales = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            // Total Transactions checkbox
            AddPropertyCheckBoxWithLabel(container, "Total Transactions", ShowTotalTransactions, yPosition,
                value =>
                {
                    ShowTotalTransactions = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            // Average Value checkbox
            AddPropertyCheckBoxWithLabel(container, "Average Value", ShowAverageValue, yPosition,
                value =>
                {
                    ShowAverageValue = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            // Growth Rate checkbox
            AddPropertyCheckBoxWithLabel(container, "Growth Rate", ShowGrowthRate, yPosition,
                value =>
                {
                    ShowGrowthRate = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight + 10;

            return yPosition;
        }
        protected override Color GetDesignerColor() => Color.LightCyan;
    }
}