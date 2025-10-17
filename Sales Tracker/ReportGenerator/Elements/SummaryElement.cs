using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ReportGenerator.Menus;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Summary element for displaying statistics and key metrics.
    /// </summary>
    public class SummaryElement : BaseElement
    {
        // Properties
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

        // Overrides
        public override string DisplayName => "Summary";
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
        public override void RenderElement(Graphics graphics, ReportConfiguration config, float renderScale)
        {
            // Draw background
            using SolidBrush bgBrush = new(BackgroundColor);
            graphics.FillRectangle(bgBrush, Bounds);

            // Draw border
            using Pen borderPen = new(BorderColor, 1);
            graphics.DrawRectangle(borderPen, Bounds);

            // Add clipping region to prevent overflow
            Region originalClip = graphics.Clip;
            graphics.SetClip(Bounds);

            try
            {
                // Calculate statistics
                SummaryStatistics stats = CalculateStatistics(config);

                // Draw summary content with actual values
                using Font titleFont = new(FontFamily, FontSize + 1, FontStyle.Bold | FontStyle);
                using Font valueFont = new(FontFamily, FontSize, FontStyle);
                using SolidBrush textBrush = new(Color.Black);
                using SolidBrush positiveBrush = new(Color.Green);
                using SolidBrush negativeBrush = new(Color.Red);

                int titleHeight = titleFont.Height;
                int lineHeight = valueFont.Height;
                int padding = 10;

                // Calculate starting position based on alignment
                int contentHeight = CalculateContentHeight(titleFont, valueFont);
                int startY = VerticalAlignment switch
                {
                    StringAlignment.Near => Bounds.Y + padding,
                    StringAlignment.Center => Bounds.Y + (Bounds.Height - contentHeight) / 2,
                    StringAlignment.Far => Bounds.Bottom - contentHeight - padding,
                    _ => Bounds.Y + padding
                };

                StringFormat format = new()
                {
                    Alignment = Alignment,
                    LineAlignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                // Check if there's enough space for title
                if (startY + titleHeight <= Bounds.Bottom - padding)
                {
                    RectangleF textBounds = new(Bounds.X + padding, startY, Bounds.Width - (padding * 2), titleHeight);
                    graphics.DrawString("Summary Statistics", titleFont, textBrush, textBounds, format);
                    startY += titleHeight;
                }
                else
                {
                    return;  // Not enough space
                }

                if (ShowTotalSales && startY + lineHeight <= Bounds.Bottom - padding)
                {
                    string totalText = TransactionType switch
                    {
                        TransactionType.Sales => $"Total Sales: {FormatCurrency(stats.TotalSales)}",
                        TransactionType.Purchases => $"Total Purchases: {FormatCurrency(stats.TotalPurchases)}",
                        _ => $"Total: {FormatCurrency(stats.CombinedTotal)}"
                    };
                    RectangleF textBounds = new(Bounds.X + padding, startY, Bounds.Width - (padding * 2), lineHeight);
                    graphics.DrawString(totalText, valueFont, textBrush, textBounds, format);
                    startY += lineHeight;
                }

                if (ShowTotalTransactions && startY + lineHeight <= Bounds.Bottom - padding)
                {
                    RectangleF textBounds = new(Bounds.X + padding, startY, Bounds.Width - (padding * 2), lineHeight);
                    graphics.DrawString($"Transactions: {stats.TransactionCount:N0}", valueFont, textBrush, textBounds, format);
                    startY += lineHeight;
                }

                if (ShowAverageValue && startY + lineHeight <= Bounds.Bottom - padding)
                {
                    RectangleF textBounds = new(Bounds.X + padding, startY, Bounds.Width - (padding * 2), lineHeight);
                    graphics.DrawString($"Average Value: {FormatCurrency(stats.AverageValue)}", valueFont, textBrush, textBounds, format);
                    startY += lineHeight;
                }

                if (ShowGrowthRate && startY + lineHeight <= Bounds.Bottom - padding)
                {
                    SolidBrush growthBrush = stats.GrowthRate >= 0 ? positiveBrush : negativeBrush;
                    string growthSymbol = stats.GrowthRate >= 0 ? "↑" : "↓";
                    RectangleF textBounds = new(Bounds.X + padding, startY, Bounds.Width - (padding * 2), lineHeight);
                    graphics.DrawString($"Growth Rate: {growthSymbol} {Math.Abs(stats.GrowthRate):F1}%", valueFont, growthBrush, textBounds, format);
                }
            }
            finally
            {
                graphics.Clip = originalClip;
            }
        }
        private int CalculateContentHeight(Font titleFont, Font valueFont)
        {
            int height = titleFont.Height;  // Title height
            int lineHeight = valueFont.Height;

            if (ShowTotalSales) { height += lineHeight; }
            if (ShowTotalTransactions) { height += lineHeight; }
            if (ShowAverageValue) { height += lineHeight; }
            if (ShowGrowthRate) { height += lineHeight; }

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
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Get undo manager for recording property changes
            UndoRedoManager? undoRedoManager = ReportLayoutDesigner_Form.Instance?.GetUndoRedoManager();

            // Section header for included metrics
            AddPropertyLabel(container, "Include:", yPosition, true);
            yPosition += 35;

            // Transaction type
            AddPropertyLabel(container, "Type:", yPosition);
            Guna2ComboBox typeCombo = AddPropertyComboBox(container, TransactionType.ToString(), yPosition,
                ["Sales", "Purchases", "Both"],
                value =>
                {
                    TransactionType newType = Enum.Parse<TransactionType>(value);
                    if (TransactionType != newType)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(TransactionType),
                            TransactionType,
                            newType,
                            onPropertyChanged));
                        TransactionType = newType;
                        onPropertyChanged();
                    }
                });
            CacheControl("TransactionType", typeCombo, () => typeCombo.SelectedItem = TransactionType.ToString());
            yPosition += ControlRowHeight;

            // Font Family
            AddPropertyLabel(container, "Font:", yPosition);
            string[] fontFamilies = ["Arial", "Calibri", "Cambria", "Comic Sans MS", "Consolas",
                             "Courier New", "Georgia", "Impact", "Segoe UI", "Tahoma",
                             "Times New Roman", "Trebuchet MS", "Verdana"];
            Guna2ComboBox fontCombo = AddPropertyComboBox(container, FontFamily, yPosition, fontFamilies,
                value =>
                {
                    if (FontFamily != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontFamily),
                            FontFamily,
                            value,
                            onPropertyChanged));
                        FontFamily = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("FontFamily", fontCombo, () => fontCombo.SelectedItem = FontFamily);
            yPosition += ControlRowHeight;

            // Font Size
            AddPropertyLabel(container, "Size:", yPosition);
            Guna2NumericUpDown fontSizeNumeric = AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition,
                value =>
                {
                    float newFontSize = (float)value;
                    if (Math.Abs(FontSize - newFontSize) > 0.01f)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(FontSize),
                            FontSize,
                            newFontSize,
                            onPropertyChanged));
                        FontSize = newFontSize;
                        onPropertyChanged();
                    }
                }, 6, 72);
            CacheControl("FontSize", fontSizeNumeric, () => fontSizeNumeric.Value = (decimal)FontSize);
            yPosition += ControlRowHeight;

            // Font Style (Bold, Italic, Underline)
            AddPropertyLabel(container, "Style:", yPosition);

            // Create the font style buttons
            int xPosition = 85;
            const int buttonWidth = 35;
            const int buttonHeight = 30;
            const int spacing = 5;
            int buttonY = yPosition + 2;

            // Bold button
            Guna2Button boldButton = CreateFontStyleButton("B", FontStyle.Bold, xPosition, buttonY, buttonWidth, buttonHeight);
            boldButton.Checked = FontStyle.HasFlag(FontStyle.Bold);
            container.Controls.Add(boldButton);
            xPosition += buttonWidth + spacing;

            // Italic button
            Guna2Button italicButton = CreateFontStyleButton("I", FontStyle.Italic, xPosition, buttonY, buttonWidth, buttonHeight);
            italicButton.Checked = FontStyle.HasFlag(FontStyle.Italic);
            container.Controls.Add(italicButton);
            xPosition += buttonWidth + spacing;

            // Underline button
            Guna2Button underlineButton = CreateFontStyleButton("U", FontStyle.Underline, xPosition, buttonY, buttonWidth, buttonHeight);
            underlineButton.Checked = FontStyle.HasFlag(FontStyle.Underline);
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

            // Background Color
            AddPropertyLabel(container, "BG Color:", yPosition);
            Panel bgColorPanel = AddColorPicker(container, yPosition, 85, BackgroundColor,
                color =>
                {
                    if (BackgroundColor.ToArgb() != color.ToArgb())
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(BackgroundColor),
                            BackgroundColor,
                            color,
                            onPropertyChanged));
                        BackgroundColor = color;
                        onPropertyChanged();
                    }
                }, showLabel: false);
            CacheControl("BackgroundColor", bgColorPanel, () => bgColorPanel.BackColor = BackgroundColor);
            yPosition += ControlRowHeight;

            // Border Color
            AddPropertyLabel(container, "Border:", yPosition);
            Panel borderColorPanel = AddColorPicker(container, yPosition, 85, BorderColor,
                color =>
                {
                    if (BorderColor.ToArgb() != color.ToArgb())
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(BorderColor),
                            BorderColor,
                            color,
                            onPropertyChanged));
                        BorderColor = color;
                        onPropertyChanged();
                    }
                }, showLabel: false);
            CacheControl("BorderColor", borderColorPanel, () => borderColorPanel.BackColor = BorderColor);
            yPosition += ControlRowHeight;

            // Horizontal Alignment
            AddPropertyLabel(container, "H-Align:", yPosition);
            string[] hAlignmentOptions = ["Left", "Center", "Right"];
            Guna2ComboBox alignCombo = AddPropertyComboBox(container, AlignmentToDisplayText(Alignment), yPosition, hAlignmentOptions,
                value =>
                {
                    StringAlignment newAlignment = DisplayTextToAlignment(value);
                    if (Alignment != newAlignment)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(Alignment),
                            Alignment,
                            newAlignment,
                            onPropertyChanged));
                        Alignment = newAlignment;
                        onPropertyChanged();
                    }
                });
            CacheControl("Alignment", alignCombo, () => alignCombo.SelectedItem = AlignmentToDisplayText(Alignment));
            yPosition += ControlRowHeight;

            // Vertical Alignment
            AddPropertyLabel(container, "V-Align:", yPosition);
            string[] vAlignmentOptions = ["Top", "Middle", "Bottom"];
            Guna2ComboBox vAlignCombo = AddPropertyComboBox(container, VerticalAlignmentToDisplayText(VerticalAlignment), yPosition, vAlignmentOptions,
                value =>
                {
                    StringAlignment newVAlignment = DisplayTextToVerticalAlignment(value);
                    if (VerticalAlignment != newVAlignment)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(VerticalAlignment),
                            VerticalAlignment,
                            newVAlignment,
                            onPropertyChanged));
                        VerticalAlignment = newVAlignment;
                        onPropertyChanged();
                    }
                });
            CacheControl("VerticalAlignment", vAlignCombo, () => vAlignCombo.SelectedItem = VerticalAlignmentToDisplayText(VerticalAlignment));
            yPosition += ControlRowHeight;

            // Include returns checkbox
            Guna2CustomCheckBox returnsCheck = AddPropertyCheckBoxWithLabel(container, "Include Returns", IncludeReturns, yPosition,
                value =>
                {
                    if (IncludeReturns != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(IncludeReturns),
                            IncludeReturns,
                            value,
                            onPropertyChanged));
                        IncludeReturns = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("IncludeReturns", returnsCheck, () => returnsCheck.Checked = IncludeReturns);
            yPosition += CheckBoxRowHeight;

            // Include losses checkbox
            Guna2CustomCheckBox lossesCheck = AddPropertyCheckBoxWithLabel(container, "Include Losses", IncludeLosses, yPosition,
                value =>
                {
                    if (IncludeLosses != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(IncludeLosses),
                            IncludeLosses,
                            value,
                            onPropertyChanged));
                        IncludeLosses = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("IncludeLosses", lossesCheck, () => lossesCheck.Checked = IncludeLosses);
            yPosition += CheckBoxRowHeight;

            // Total Sales checkbox
            Guna2CustomCheckBox salesCheck = AddPropertyCheckBoxWithLabel(container, "Show Total Sales", ShowTotalSales, yPosition,
                value =>
                {
                    if (ShowTotalSales != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowTotalSales),
                            ShowTotalSales,
                            value,
                            onPropertyChanged));
                        ShowTotalSales = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("ShowTotalSales", salesCheck, () => salesCheck.Checked = ShowTotalSales);
            yPosition += CheckBoxRowHeight;

            // Total Transactions checkbox
            Guna2CustomCheckBox transactionsCheck = AddPropertyCheckBoxWithLabel(container, "Show Transactions", ShowTotalTransactions, yPosition,
                value =>
                {
                    if (ShowTotalTransactions != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowTotalTransactions),
                            ShowTotalTransactions,
                            value,
                            onPropertyChanged));
                        ShowTotalTransactions = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("ShowTotalTransactions", transactionsCheck, () => transactionsCheck.Checked = ShowTotalTransactions);
            yPosition += CheckBoxRowHeight;

            // Average Value checkbox
            Guna2CustomCheckBox avgCheck = AddPropertyCheckBoxWithLabel(container, "Show Average Value", ShowAverageValue, yPosition,
                value =>
                {
                    if (ShowAverageValue != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowAverageValue),
                            ShowAverageValue,
                            value,
                            onPropertyChanged));
                        ShowAverageValue = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("ShowAverageValue", avgCheck, () => avgCheck.Checked = ShowAverageValue);
            yPosition += CheckBoxRowHeight;

            // Growth Rate checkbox
            Guna2CustomCheckBox growthCheck = AddPropertyCheckBoxWithLabel(container, "Show Growth Rate", ShowGrowthRate, yPosition,
                value =>
                {
                    if (ShowGrowthRate != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowGrowthRate),
                            ShowGrowthRate,
                            value,
                            onPropertyChanged));
                        ShowGrowthRate = value;
                        onPropertyChanged();
                    }
                });
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