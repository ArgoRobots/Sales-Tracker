using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Summary element for displaying statistics and key metrics.
    /// </summary>
    public class SummaryElement : BaseElement
    {
        public TransactionType TransactionType { get; set; } = TransactionType.Both;
        public bool IncludeReturns { get; set; } = true;
        public bool IncludeLosses { get; set; } = true;
        public bool ShowTotalSales { get; set; } = true;
        public bool ShowTotalTransactions { get; set; } = true;
        public bool ShowAverageValue { get; set; } = true;
        public bool ShowGrowthRate { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.WhiteSmoke;
        public Color BorderColor { get; set; } = Color.LightGray;
        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 10f;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;
        public StringAlignment VerticalAlignment { get; set; } = StringAlignment.Near;

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
                TransactionType = TransactionType,
                IncludeReturns = IncludeReturns,
                IncludeLosses = IncludeLosses,
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ShowTotalSales = ShowTotalSales,
                ShowTotalTransactions = ShowTotalTransactions,
                ShowAverageValue = ShowAverageValue,
                ShowGrowthRate = ShowGrowthRate,
                BackgroundColor = BackgroundColor,
                BorderColor = BorderColor,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle,
                Alignment = Alignment,
                VerticalAlignment = VerticalAlignment
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
            using Font titleFont = new(FontFamily, FontSize + 1, FontStyle.Bold | FontStyle);
            using Font valueFont = new(FontFamily, FontSize, FontStyle);
            using SolidBrush textBrush = new(Color.Black);
            using SolidBrush positiveBrush = new(Color.Green);
            using SolidBrush negativeBrush = new(Color.Red);

            // Calculate starting position based on alignment
            int contentHeight = CalculateContentHeight();
            int startY = VerticalAlignment switch
            {
                StringAlignment.Near => Bounds.Y + 10,
                StringAlignment.Center => Bounds.Y + (Bounds.Height - contentHeight) / 2,
                StringAlignment.Far => Bounds.Bottom - contentHeight - 10,
                _ => Bounds.Y + 10
            };

            int x = Alignment switch
            {
                StringAlignment.Near => Bounds.X + 10,
                StringAlignment.Center => Bounds.X + Bounds.Width / 2,
                StringAlignment.Far => Bounds.Right - 10,
                _ => Bounds.X + 10
            };

            StringFormat format = new()
            {
                Alignment = Alignment,
                LineAlignment = StringAlignment.Near
            };

            RectangleF textBounds = new(Bounds.X + 10, startY, Bounds.Width - 20, 25);
            graphics.DrawString("Summary Statistics", titleFont, textBrush, textBounds, format);
            startY += 25;

            if (ShowTotalSales)
            {
                string totalText = TransactionType switch
                {
                    TransactionType.Sales => $"Total Sales: {FormatCurrency(stats.TotalSales)}",
                    TransactionType.Purchases => $"Total Purchases: {FormatCurrency(stats.TotalPurchases)}",
                    _ => $"Total: {FormatCurrency(stats.CombinedTotal)}"
                };
                textBounds.Y = startY;
                graphics.DrawString(totalText, valueFont, textBrush, textBounds, format);
                startY += 20;
            }

            if (ShowTotalTransactions)
            {
                textBounds.Y = startY;
                graphics.DrawString($"Transactions: {stats.TransactionCount:N0}", valueFont, textBrush, textBounds, format);
                startY += 20;
            }

            if (ShowAverageValue)
            {
                textBounds.Y = startY;
                graphics.DrawString($"Average Value: {FormatCurrency(stats.AverageValue)}", valueFont, textBrush, textBounds, format);
                startY += 20;
            }

            if (ShowGrowthRate)
            {
                SolidBrush growthBrush = stats.GrowthRate >= 0 ? positiveBrush : negativeBrush;
                string growthSymbol = stats.GrowthRate >= 0 ? "↑" : "↓";
                textBounds.Y = startY;
                graphics.DrawString($"Growth Rate: {growthSymbol} {Math.Abs(stats.GrowthRate):F1}%", valueFont, growthBrush, textBounds, format);
            }
        }
        private int CalculateContentHeight()
        {
            int height = 25; // Title
            if (ShowTotalSales) height += 20;
            if (ShowTotalTransactions) height += 20;
            if (ShowAverageValue) height += 20;
            if (ShowGrowthRate) height += 20;
            return height;
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
                if (TransactionType == TransactionType.Sales || TransactionType == TransactionType.Both)
                {
                    (decimal Total, int Count) = CalculateGridViewTotal(
                        MainMenu_Form.Instance.Sale_DataGridView,
                        startDate,
                        endDate,
                        IncludeReturns);

                    stats.TotalSales = Total;
                    stats.TransactionCount += Count;
                }

                // Calculate purchases if needed
                if (TransactionType == TransactionType.Purchases || TransactionType == TransactionType.Both)
                {
                    (decimal Total, int Count) = CalculateGridViewTotal(
                        MainMenu_Form.Instance.Purchase_DataGridView,
                        startDate,
                        endDate,
                        IncludeReturns);

                    stats.TotalPurchases = Total;
                    if (TransactionType == TransactionType.Purchases)
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

        private decimal CalculateGrowthRate(
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
                    IncludeReturns);
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
                    IncludeReturns);
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
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Section header for included metrics
            AddPropertyLabel(container, "Include:", yPosition, true);
            yPosition += 35;

            // Transaction type
            AddPropertyLabel(container, "Type:", yPosition);
            Guna2ComboBox typeCombo = AddPropertyComboBox(container, TransactionType.ToString(), yPosition,
                ["Sales", "Purchases", "Both"],
                value =>
                {
                    TransactionType = Enum.Parse<TransactionType>(value);
                    onPropertyChanged();
                });
            CacheControl("TransactionType", typeCombo, () => typeCombo.SelectedItem = TransactionType.ToString());
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
            }, 8, 20);
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

            // Include returns checkbox
            AddPropertyCheckBoxWithLabel(container, "Include Returns", IncludeReturns, yPosition,
                value =>
                {
                    IncludeReturns = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox returnsCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("IncludeReturns", returnsCheck, () => returnsCheck.Checked = IncludeReturns);
            yPosition += CheckBoxRowHeight;

            // Include losses checkbox  
            AddPropertyCheckBoxWithLabel(container, "Include Losses", IncludeLosses, yPosition,
                value =>
                {
                    IncludeLosses = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox lossesCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("IncludeLosses", lossesCheck, () => lossesCheck.Checked = IncludeLosses);
            yPosition += CheckBoxRowHeight;

            // Total Sales checkbox with clickable label
            AddPropertyCheckBoxWithLabel(container, "Total Sales", ShowTotalSales, yPosition,
                value =>
                {
                    ShowTotalSales = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox salesCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowTotalSales", salesCheck, () => salesCheck.Checked = ShowTotalSales);
            yPosition += CheckBoxRowHeight;

            // Total Transactions checkbox
            AddPropertyCheckBoxWithLabel(container, "Total Transactions", ShowTotalTransactions, yPosition,
                value =>
                {
                    ShowTotalTransactions = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox transactionsCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowTotalTransactions", transactionsCheck, () => transactionsCheck.Checked = ShowTotalTransactions);
            yPosition += CheckBoxRowHeight;

            // Average Value checkbox
            AddPropertyCheckBoxWithLabel(container, "Average Value", ShowAverageValue, yPosition,
                value =>
                {
                    ShowAverageValue = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox avgCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowAverageValue", avgCheck, () => avgCheck.Checked = ShowAverageValue);
            yPosition += CheckBoxRowHeight;

            // Growth Rate checkbox
            AddPropertyCheckBoxWithLabel(container, "Growth Rate", ShowGrowthRate, yPosition,
                value =>
                {
                    ShowGrowthRate = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox growthCheck = container.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowGrowthRate", growthCheck, () => growthCheck.Checked = ShowGrowthRate);
            yPosition += CheckBoxRowHeight + 10;

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
                _ => "Top"
            };
        }
        private static StringAlignment DisplayTextToVerticalAlignment(string displayText)
        {
            return displayText switch
            {
                "Top" => StringAlignment.Near,
                "Middle" => StringAlignment.Center,
                "Bottom" => StringAlignment.Far,
                _ => StringAlignment.Near
            };
        }
    }
}