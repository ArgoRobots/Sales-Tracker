using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.DataClasses;
using SkiaSharp;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Chart element for displaying graphs and charts.
    /// </summary>
    public class ChartElement : BaseElement
    {
        public MainMenu_Form.ChartDataType ChartType { get; set; } = MainMenu_Form.ChartDataType.TotalSales;
        public bool ShowLegend { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public Color BorderColor { get; set; } = Color.Gray;

        public override ReportElementType GetElementType() => ReportElementType.Chart;
        public override BaseElement Clone()
        {
            return new ChartElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ChartType = ChartType,
                ShowLegend = ShowLegend,
                ShowTitle = ShowTitle,
                BorderColor = BorderColor
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                Control chartControl = GetChartControl();

                LoadChartData(ChartType, config);

                // Generate server-side image
                if (chartControl != null && chartControl is CartesianChart cartesianChart)
                {
                    using Bitmap chartImage = GenerateCartesianChartImage(cartesianChart);
                    if (chartImage != null)
                    {
                        graphics.DrawImage(chartImage, Bounds);
                    }
                }
                else if (chartControl != null && chartControl is PieChart pieChart)
                {
                    using Bitmap chartImage = GeneratePieChartImage(pieChart);
                    if (chartImage != null)
                    {
                        graphics.DrawImage(chartImage, Bounds);
                    }
                }
                else if (chartControl != null && chartControl is GeoMap geoMap)
                {
                    using Bitmap chartImage = GenerateGeoMapImage(geoMap);
                    if (chartImage != null)
                    {
                        graphics.DrawImage(chartImage, Bounds);
                    }
                }
                else
                {
                    // Fallback to placeholder if chart control is not available
                    RenderPlaceholder(graphics, $"Chart: {ChartType}");
                }

                // Draw border if needed
                if (BorderColor != Color.Transparent)
                {
                    using Pen borderPen = new(BorderColor, 1);
                    graphics.DrawRectangle(borderPen, Bounds);
                }
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, $"Chart Error: {ex.Message}");
            }
        }
        private static void LoadChartData(MainMenu_Form.ChartDataType chartType, ReportConfiguration config = null)
        {
            MainMenu_Form mainForm = MainMenu_Form.Instance;
            bool isLine = mainForm.LineChart_ToggleSwitch.Checked;

            // Store current filter values
            DateTime? originalFromDate = mainForm.SortFromDate;
            DateTime? originalToDate = mainForm.SortToDate;
            TimeSpan? originalTimeSpan = mainForm.SortTimeSpan;

            try
            {
                // Apply date filters from report configuration if provided
                if (config?.Filters != null)
                {
                    // Set the date range filters on MainMenu_Form
                    mainForm.SortFromDate = config.Filters.StartDate;
                    mainForm.SortToDate = config.Filters.EndDate;
                    mainForm.SortTimeSpan = null;  // Clear timespan when using custom date range

                    // Apply filters to both DataGridViews based on transaction type
                    if (config.Filters.TransactionType == TransactionType.Sales ||
                        config.Filters.TransactionType == TransactionType.Both)
                    {
                        ApplyDateFiltersToDataGridView(mainForm.Sale_DataGridView, config.Filters);
                    }

                    if (config.Filters.TransactionType == TransactionType.Purchases ||
                        config.Filters.TransactionType == TransactionType.Both)
                    {
                        ApplyDateFiltersToDataGridView(mainForm.Purchase_DataGridView, config.Filters);
                    }
                }

                // Load the chart with filtered data
                switch (chartType)
                {
                    case MainMenu_Form.ChartDataType.TotalSales:
                        LoadChart.LoadTotalsIntoChart(mainForm.Sale_DataGridView, mainForm.TotalSales_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.TotalPurchases:
                        LoadChart.LoadTotalsIntoChart(mainForm.Purchase_DataGridView, mainForm.TotalPurchases_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.DistributionOfSales:
                        LoadChart.LoadDistributionIntoChart(mainForm.Sale_DataGridView, mainForm.DistributionOfSales_Chart, PieChartGrouping.Top12);
                        break;
                    case MainMenu_Form.ChartDataType.DistributionOfPurchases:
                        LoadChart.LoadDistributionIntoChart(mainForm.Purchase_DataGridView, mainForm.DistributionOfPurchases_Chart, PieChartGrouping.Top12);
                        break;
                    case MainMenu_Form.ChartDataType.Profits:
                        LoadChart.LoadProfitsIntoChart(mainForm.Profits_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                        LoadChart.LoadCountriesOfOriginChart(mainForm.CountriesOfOrigin_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                        LoadChart.LoadCompaniesOfOriginChart(mainForm.CompaniesOfOrigin_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.CountriesOfDestination:
                        LoadChart.LoadCountriesOfDestinationChart(mainForm.CountriesOfDestination_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.Accountants:
                        LoadChart.LoadAccountantsIntoChart(mainForm.Accountants_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                        LoadChart.LoadSalesVsExpensesChart(mainForm.TotalExpensesVsSales_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.AverageTransactionValue:
                        LoadChart.LoadAverageTransactionValueChart(mainForm.AverageTransactionValue_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.TotalTransactions:
                        LoadChart.LoadTotalTransactionsChart(mainForm.TotalTransactions_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.AverageShippingCosts:
                        LoadChart.LoadAverageShippingCostsChart(mainForm.AverageShippingCosts_Chart, isLine, includeZeroShipping: true);
                        break;
                    case MainMenu_Form.ChartDataType.GrowthRates:
                        LoadChart.LoadGrowthRateChart(mainForm.GrowthRates_Chart);
                        break;
                    case MainMenu_Form.ChartDataType.ReturnsOverTime:
                        LoadChart.LoadReturnsOverTimeChart(mainForm.ReturnsOverTime_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.ReturnReasons:
                        LoadChart.LoadReturnReasonsChart(mainForm.ReturnReasons_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.ReturnFinancialImpact:
                        LoadChart.LoadReturnFinancialImpactChart(mainForm.ReturnFinancialImpact_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.ReturnsByCategory:
                        LoadChart.LoadReturnsByCategoryChart(mainForm.ReturnsByCategory_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.ReturnsByProduct:
                        LoadChart.LoadReturnsByProductChart(mainForm.ReturnsByProduct_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.PurchaseVsSaleReturns:
                        LoadChart.LoadPurchaseVsSaleReturnsChart(mainForm.PurchaseVsSaleReturns_Chart);
                        break;
                    case MainMenu_Form.ChartDataType.WorldMap:
                        LoadChart.LoadWorldMapChart(mainForm.WorldMap_GeoMap, MainMenu_Form.GeoMapDataType.Combined);
                        break;
                    case MainMenu_Form.ChartDataType.LossesOverTime:
                        LoadChart.LoadLossesOverTimeChart(mainForm.LossesOverTime_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.LossReasons:
                        LoadChart.LoadLossReasonsChart(mainForm.LossReasons_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.LossFinancialImpact:
                        LoadChart.LoadLossFinancialImpactChart(mainForm.LossFinancialImpact_Chart, isLine);
                        break;
                    case MainMenu_Form.ChartDataType.LossesByCategory:
                        LoadChart.LoadLossesByCategoryChart(mainForm.LossesByCategory_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.LossesByProduct:
                        LoadChart.LoadLossesByProductChart(mainForm.LossesByProduct_Chart, PieChartGrouping.Top8);
                        break;
                    case MainMenu_Form.ChartDataType.PurchaseVsSaleLosses:
                        LoadChart.LoadPurchaseVsSaleLossesChart(mainForm.PurchaseVsSaleLosses_Chart);
                        break;
                }
            }
            finally
            {
                // Restore original filter values
                mainForm.SortFromDate = originalFromDate;
                mainForm.SortToDate = originalToDate;
                mainForm.SortTimeSpan = originalTimeSpan;

                // Reset DataGridView visibility to show all rows
                foreach (DataGridViewRow row in mainForm.Sale_DataGridView.Rows)
                {
                    row.Visible = true;
                }
                foreach (DataGridViewRow row in mainForm.Purchase_DataGridView.Rows)
                {
                    row.Visible = true;
                }
            }
        }
        private static void ApplyDateFiltersToDataGridView(DataGridView dataGridView, ReportFilters filters)
        {
            if (filters.StartDate == null || filters.EndDate == null)
            {
                // If no date range specified, show all rows
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Visible = true;
                }
                return;
            }

            DateTime startDate = filters.StartDate.Value;
            DateTime endDate = filters.EndDate.Value;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                bool shouldShow = false;

                // Parse the date from the row
                if (row.Cells[ReadOnlyVariables.Date_column].Value != null &&
                    DateTime.TryParse(row.Cells[ReadOnlyVariables.Date_column].Value.ToString(), out DateTime rowDate))
                {
                    // Check if date is within range
                    if (rowDate >= startDate && rowDate <= endDate)
                    {
                        shouldShow = true;

                        // Check for returns filter
                        if (!filters.IncludeReturns)
                        {
                            // Check if the transaction is returned (fully or partially)
                            if (ReturnProduct.ReturnManager.IsTransactionReturned(row))
                            {
                                shouldShow = false;
                            }
                        }

                        // Check for losses filter
                        if (shouldShow && !filters.IncludeLosses)
                        {
                            // Check if the transaction is lost (fully or partially)
                            if (LostProduct.LostManager.IsTransactionLost(row))
                            {
                                shouldShow = false;
                            }
                        }
                    }
                }

                row.Visible = shouldShow;
            }
        }
        private Bitmap GenerateCartesianChartImage(CartesianChart sourceChart)
        {
            SKCartesianChart skChart = new()
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Background = SKColors.White
            };

            // Copy title if ShowTitle is enabled
            if (ShowTitle)
            {
                // Get the title text based on chart type
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                skChart.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
                {
                    Text = titleText,
                    TextSize = 14,
                    Paint = new SolidColorPaint(SKColors.Black),
                    Padding = new LiveChartsCore.Drawing.Padding(5)
                };
            }

            if (sourceChart.Series != null)
            {
                skChart.Series = sourceChart.Series;
            }

            skChart.LegendPosition = ShowLegend
                ? LiveChartsCore.Measure.LegendPosition.Top
                : LiveChartsCore.Measure.LegendPosition.Hidden;

            skChart.LegendTextPaint = new SolidColorPaint(SKColors.Black);
            skChart.LegendTextSize = 12;

            using SKImage image = skChart.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        private Bitmap GeneratePieChartImage(PieChart sourceChart)
        {
            SKPieChart skChart = new()
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Background = SKColors.White
            };

            // Copy title if ShowTitle is enabled
            if (ShowTitle)
            {
                // Get the title text based on chart type
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                skChart.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
                {
                    Text = titleText,
                    TextSize = 14,
                    Paint = new SolidColorPaint(SKColors.Black),
                    Padding = new LiveChartsCore.Drawing.Padding(5)
                };
            }

            if (sourceChart.Series != null)
            {
                skChart.Series = sourceChart.Series;
            }

            skChart.LegendPosition = ShowLegend
                ? LiveChartsCore.Measure.LegendPosition.Right
                : LiveChartsCore.Measure.LegendPosition.Hidden;

            skChart.LegendTextPaint = new SolidColorPaint(SKColors.Black);
            skChart.LegendTextSize = 12;

            using SKImage image = skChart.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        private Bitmap GenerateGeoMapImage(GeoMap sourceMap)
        {
            // Create an SK map with the same data
            SKGeoMap skMap = new()
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Background = SKColors.White
            };

            // Copy series data from source map to SK map
            if (sourceMap.Series != null)
            {
                skMap.Series = sourceMap.Series;
            }

            // Copy map projection
            skMap.MapProjection = sourceMap.MapProjection;

            // Generate image
            using SKImage image = skMap.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        private Control? GetChartControl()
        {
            MainMenu_Form mainForm = MainMenu_Form.Instance;

            return ChartType switch
            {
                MainMenu_Form.ChartDataType.TotalSales => mainForm.TotalSales_Chart,
                MainMenu_Form.ChartDataType.TotalPurchases => mainForm.TotalPurchases_Chart,
                MainMenu_Form.ChartDataType.DistributionOfSales => mainForm.DistributionOfSales_Chart,
                MainMenu_Form.ChartDataType.DistributionOfPurchases => mainForm.DistributionOfPurchases_Chart,
                MainMenu_Form.ChartDataType.Profits => mainForm.Profits_Chart,
                MainMenu_Form.ChartDataType.CountriesOfOrigin => mainForm.CountriesOfOrigin_Chart,
                MainMenu_Form.ChartDataType.CompaniesOfOrigin => mainForm.CompaniesOfOrigin_Chart,
                MainMenu_Form.ChartDataType.CountriesOfDestination => mainForm.CountriesOfDestination_Chart,
                MainMenu_Form.ChartDataType.Accountants => mainForm.Accountants_Chart,
                MainMenu_Form.ChartDataType.TotalExpensesVsSales => mainForm.TotalExpensesVsSales_Chart,
                MainMenu_Form.ChartDataType.AverageTransactionValue => mainForm.AverageTransactionValue_Chart,
                MainMenu_Form.ChartDataType.TotalTransactions => mainForm.TotalTransactions_Chart,
                MainMenu_Form.ChartDataType.AverageShippingCosts => mainForm.AverageShippingCosts_Chart,
                MainMenu_Form.ChartDataType.GrowthRates => mainForm.GrowthRates_Chart,
                MainMenu_Form.ChartDataType.ReturnsOverTime => mainForm.ReturnsOverTime_Chart,
                MainMenu_Form.ChartDataType.ReturnReasons => mainForm.ReturnReasons_Chart,
                MainMenu_Form.ChartDataType.ReturnFinancialImpact => mainForm.ReturnFinancialImpact_Chart,
                MainMenu_Form.ChartDataType.ReturnsByCategory => mainForm.ReturnsByCategory_Chart,
                MainMenu_Form.ChartDataType.ReturnsByProduct => mainForm.ReturnsByProduct_Chart,
                MainMenu_Form.ChartDataType.PurchaseVsSaleReturns => mainForm.PurchaseVsSaleReturns_Chart,
                MainMenu_Form.ChartDataType.WorldMap => mainForm.WorldMap_GeoMap,
                MainMenu_Form.ChartDataType.LossesOverTime => mainForm.LossesOverTime_Chart,
                MainMenu_Form.ChartDataType.LossReasons => mainForm.LossReasons_Chart,
                MainMenu_Form.ChartDataType.LossFinancialImpact => mainForm.LossFinancialImpact_Chart,
                MainMenu_Form.ChartDataType.LossesByCategory => mainForm.LossesByCategory_Chart,
                MainMenu_Form.ChartDataType.LossesByProduct => mainForm.LossesByProduct_Chart,
                MainMenu_Form.ChartDataType.PurchaseVsSaleLosses => mainForm.PurchaseVsSaleLosses_Chart,
                _ => null
            };
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.LightBlue);
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

            string text = DisplayName ?? ChartType.ToString();
            graphics.DrawString(text, font, textBrush, Bounds, format);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Chart type selector
            AddPropertyLabel(container, "Chart:", yPosition);
            AddPropertyComboBox(container, ChartType.ToString(), yPosition,
                GetAvailableChartTypes(),
                value =>
                {
                    ChartType = Enum.Parse<MainMenu_Form.ChartDataType>(value);
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Show legend checkbox
            AddPropertyCheckBoxWithLabel(container, "Legend", ShowLegend, yPosition,
                value =>
                {
                    ShowLegend = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            // Show title checkbox
            AddPropertyCheckBoxWithLabel(container, "Title", ShowTitle, yPosition,
                value =>
                {
                    ShowTitle = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            return yPosition;
        }
        private static string[] GetAvailableChartTypes()
        {
            return Enum.GetNames<MainMenu_Form.ChartDataType>();
        }
        private void RenderPlaceholder(Graphics graphics, string text)
        {
            using SolidBrush brush = new(Color.LightBlue);
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(text, font, textBrush, Bounds, format);
        }
        private void RenderErrorPlaceholder(Graphics graphics, string errorMessage)
        {
            using SolidBrush brush = new(Color.LightPink);
            using Pen pen = new(Color.Red, 2);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.DarkRed);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(errorMessage, font, textBrush, Bounds, format);
        }
        protected override Color GetDesignerColor() => Color.LightBlue;
    }
}