using Guna.UI2.WinForms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.GridView;
using SkiaSharp;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Chart element for displaying graphs and charts.
    /// </summary>
    public class ChartElement : BaseElement
    {
        private static readonly Dictionary<MainMenu_Form.ChartDataType, (DateTime? StartDate, DateTime? EndDate, bool IncludeReturns, bool IncludeLosses)> _lastLoadedConfig = [];

        // Private properties
        private Control _chartControl;
        private MainMenu_Form.ChartDataType _chartType = MainMenu_Form.ChartDataType.TotalSales;

        // Public properties
        public MainMenu_Form.ChartDataType ChartType
        {
            get => _chartType;
            set
            {
                _chartType = value;
                // Automatically update DisplayName when ChartType changes
                DisplayName = TranslatedChartTitles.GetChartDisplayName(_chartType);
            }
        }
        public bool ShowLegend { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public Color BorderColor { get; set; } = Color.Gray;

        // Constructor
        public ChartElement()
        {
            DisplayName = TranslatedChartTitles.GetChartDisplayName(_chartType);
        }

        // Overrides
        public override ReportElementType GetElementType() => ReportElementType.Chart;
        public override BaseElement Clone()
        {
            return new ChartElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
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
                // Check if configuration has changed and reload if necessary
                bool needsReload = ShouldReloadChart(ChartType, config);

                if (_chartControl == null || needsReload)
                {
                    // Dispose of old control if reloading
                    if (_chartControl != null && needsReload)
                    {
                        _chartControl.Dispose();
                        _chartControl = null;
                    }

                    LoadChartData(ChartType, config);
                }

                // Generate server-side image
                if (_chartControl != null && _chartControl is CartesianChart cartesianChart)
                {
                    using Bitmap chartImage = GenerateCartesianChartImage(cartesianChart);
                    if (chartImage != null)
                    {
                        graphics.DrawImage(chartImage, Bounds);
                    }
                }
                else if (_chartControl != null && _chartControl is PieChart pieChart)
                {
                    using Bitmap chartImage = GeneratePieChartImage(pieChart);
                    if (chartImage != null)
                    {
                        graphics.DrawImage(chartImage, Bounds);
                    }
                }
                else if (_chartControl != null && _chartControl is GeoMap geoMap)
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
        private static bool ShouldReloadChart(MainMenu_Form.ChartDataType chartType, ReportConfiguration config)
        {
            if (config?.Filters == null)
            {
                return false;
            }

            // Check if we have a previous configuration for this chart type
            if (!_lastLoadedConfig.TryGetValue(chartType, out (DateTime? StartDate, DateTime? EndDate, bool IncludeReturns, bool IncludeLosses) lastConfig))
            {
                return true; // Never loaded before
            }

            // Check if any filter values have changed
            bool configChanged = lastConfig.StartDate != config.Filters.StartDate ||
                                lastConfig.EndDate != config.Filters.EndDate ||
                                lastConfig.IncludeReturns != config.Filters.IncludeReturns ||
                                lastConfig.IncludeLosses != config.Filters.IncludeLosses;

            return configChanged;
        }

        // Load chart methods
        private void LoadChartData(MainMenu_Form.ChartDataType chartType, ReportConfiguration config = null)
        {
            bool isLine = MainMenu_Form.Instance.LineChart_ToggleSwitch.Checked;

            try
            {
                // Update the tracking dictionary with the current configuration
                if (config?.Filters != null)
                {
                    _lastLoadedConfig[chartType] = (
                        config.Filters.StartDate,
                        config.Filters.EndDate,
                        config.Filters.IncludeReturns,
                        config.Filters.IncludeLosses
                    );
                }
                else
                {
                    _lastLoadedConfig.Remove(chartType);
                }

                CreateIndependentChartControl(chartType);

                // Create filtered clones of the DataGridViews
                Guna2DataGridView salesDataGridView = CreateFilteredDataGridView(
                    MainMenu_Form.Instance.Sale_DataGridView,
                    config?.Filters?.StartDate,
                    config?.Filters?.EndDate);

                Guna2DataGridView purchasesDataGridView = CreateFilteredDataGridView(
                    MainMenu_Form.Instance.Purchase_DataGridView,
                    config?.Filters?.StartDate,
                    config?.Filters?.EndDate);

                // Load charts with filtered DataGridViews
                switch (chartType)
                {
                    case MainMenu_Form.ChartDataType.TotalSales:
                        LoadChart.LoadTotalsIntoChart(salesDataGridView, (CartesianChart)_chartControl, isLine);
                        break;

                    case MainMenu_Form.ChartDataType.TotalPurchases:
                        LoadChart.LoadTotalsIntoChart(purchasesDataGridView, (CartesianChart)_chartControl, isLine);
                        break;

                    case MainMenu_Form.ChartDataType.DistributionOfSales:
                        LoadChart.LoadDistributionIntoChart(salesDataGridView, (PieChart)_chartControl, PieChartGrouping.Top12);
                        break;

                    case MainMenu_Form.ChartDataType.DistributionOfPurchases:
                        LoadChart.LoadDistributionIntoChart(purchasesDataGridView, (PieChart)_chartControl, PieChartGrouping.Top12);
                        break;

                    case MainMenu_Form.ChartDataType.Profits:
                        LoadChart.LoadProfitsIntoChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                        LoadChart.LoadCountriesOfOriginChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                        LoadChart.LoadCompaniesOfOriginChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfDestination:
                        LoadChart.LoadCountriesOfDestinationChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.Accountants:
                        LoadChart.LoadAccountantsIntoChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                        LoadChart.LoadSalesVsExpensesChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.AverageTransactionValue:
                        LoadChart.LoadAverageTransactionValueChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.TotalTransactions:
                        LoadChart.LoadTotalTransactionsChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.AverageShippingCosts:
                        LoadChart.LoadAverageShippingCostsChart((CartesianChart)_chartControl, isLine,
                            includeZeroShipping: true,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.GrowthRates:
                        LoadChart.LoadGrowthRateChart((CartesianChart)_chartControl,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsOverTime:
                        LoadChart.LoadReturnsOverTimeChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.ReturnReasons:
                        LoadChart.LoadReturnReasonsChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.ReturnFinancialImpact:
                        LoadChart.LoadReturnFinancialImpactChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsByCategory:
                        LoadChart.LoadReturnsByCategoryChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsByProduct:
                        LoadChart.LoadReturnsByProductChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.PurchaseVsSaleReturns:
                        LoadChart.LoadPurchaseVsSaleReturnsChart((PieChart)_chartControl,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.WorldMap:
                        LoadChart.LoadWorldMapChart((GeoMap)_chartControl, MainMenu_Form.GeoMapDataType.Combined,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.LossesOverTime:
                        LoadChart.LoadLossesOverTimeChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.LossReasons:
                        LoadChart.LoadLossReasonsChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.LossFinancialImpact:
                        LoadChart.LoadLossFinancialImpactChart((CartesianChart)_chartControl, isLine,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.LossesByCategory:
                        LoadChart.LoadLossesByCategoryChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.LossesByProduct:
                        LoadChart.LoadLossesByProductChart((PieChart)_chartControl, PieChartGrouping.Top8,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;

                    case MainMenu_Form.ChartDataType.PurchaseVsSaleLosses:
                        LoadChart.LoadPurchaseVsSaleLossesChart((PieChart)_chartControl,
                            purchasesDataGridView: purchasesDataGridView,
                            salesDataGridView: salesDataGridView);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error loading chart data for ChartElement: {ex.Message}");
            }
        }
        private static Guna2DataGridView CreateFilteredDataGridView(Guna2DataGridView sourceGrid, DateTime? startDate, DateTime? endDate)
        {
            // Create new DataGridView
            Guna2DataGridView filteredGrid = new();

            // Determine if this is a purchase or sale grid and initialize with appropriate columns
            bool isPurchaseGrid = sourceGrid == MainMenu_Form.Instance.Purchase_DataGridView;

            if (isPurchaseGrid)
            {
                DataGridViewManager.InitializeDataGridView(
                    filteredGrid,
                    "FilteredPurchases_DataGridView",
                    MainMenu_Form.Instance.PurchaseColumnHeaders,
                    null,
                    MainMenu_Form.Instance
                );
            }
            else
            {
                DataGridViewManager.InitializeDataGridView(
                    filteredGrid,
                    "FilteredSales_DataGridView",
                    MainMenu_Form.Instance.SalesColumnHeaders,
                    null,
                    MainMenu_Form.Instance
                );
            }

            // Get date range
            DateTime filterStart = startDate ?? DateTime.MinValue;
            DateTime filterEnd = endDate ?? DateTime.MaxValue;

            // Copy rows that fall within date range
            foreach (DataGridViewRow sourceRow in sourceGrid.Rows)
            {
                if (sourceRow.Cells[ReadOnlyVariables.Date_column].Value != null &&
                    DateTime.TryParse(sourceRow.Cells[ReadOnlyVariables.Date_column].Value.ToString(), out DateTime rowDate))
                {
                    // Check if row is within date range
                    if (rowDate >= filterStart && rowDate <= filterEnd)
                    {
                        DataGridViewRow newRow = (DataGridViewRow)sourceRow.Clone();

                        // Copy cell values
                        for (int i = 0; i < sourceRow.Cells.Count; i++)
                        {
                            newRow.Cells[i].Value = sourceRow.Cells[i].Value;
                        }

                        newRow.Tag = sourceRow.Tag;

                        filteredGrid.Rows.Add(newRow);
                    }
                }
            }

            return filteredGrid;
        }
        private void CreateIndependentChartControl(MainMenu_Form.ChartDataType chartType)
        {
            // Determine chart type and create appropriate control
            if (IsCartesianChartType(chartType))
            {
                _chartControl = new CartesianChart();
            }
            else if (IsPieChartType(chartType))
            {
                _chartControl = new PieChart();
            }
            else if (IsGeoMapType(chartType))
            {
                _chartControl = new GeoMap();
            }
        }
        private static bool IsCartesianChartType(MainMenu_Form.ChartDataType chartType)
        {
            return chartType is
                MainMenu_Form.ChartDataType.TotalSales or
                MainMenu_Form.ChartDataType.TotalPurchases or
                MainMenu_Form.ChartDataType.Profits or
                MainMenu_Form.ChartDataType.TotalExpensesVsSales or
                MainMenu_Form.ChartDataType.AverageTransactionValue or
                MainMenu_Form.ChartDataType.TotalTransactions or
                MainMenu_Form.ChartDataType.AverageShippingCosts or
                MainMenu_Form.ChartDataType.GrowthRates or
                MainMenu_Form.ChartDataType.ReturnsOverTime or
                MainMenu_Form.ChartDataType.ReturnFinancialImpact or
                MainMenu_Form.ChartDataType.PurchaseVsSaleReturns or
                MainMenu_Form.ChartDataType.LossesOverTime or
                MainMenu_Form.ChartDataType.LossFinancialImpact or
                MainMenu_Form.ChartDataType.PurchaseVsSaleLosses;
        }
        private static bool IsPieChartType(MainMenu_Form.ChartDataType chartType)
        {
            return chartType is
                MainMenu_Form.ChartDataType.DistributionOfSales or
                MainMenu_Form.ChartDataType.DistributionOfPurchases or
                MainMenu_Form.ChartDataType.CountriesOfOrigin or
                MainMenu_Form.ChartDataType.CompaniesOfOrigin or
                MainMenu_Form.ChartDataType.CountriesOfDestination or
                MainMenu_Form.ChartDataType.Accountants or
                MainMenu_Form.ChartDataType.ReturnReasons or
                MainMenu_Form.ChartDataType.ReturnsByCategory or
                MainMenu_Form.ChartDataType.ReturnsByProduct or
                MainMenu_Form.ChartDataType.LossReasons or
                MainMenu_Form.ChartDataType.LossesByCategory or
                MainMenu_Form.ChartDataType.LossesByProduct;
        }
        private static bool IsGeoMapType(MainMenu_Form.ChartDataType chartType)
        {
            return chartType is MainMenu_Form.ChartDataType.WorldMap;
        }

        // Render chart methods
        private Bitmap GenerateCartesianChartImage(CartesianChart sourceChart)
        {
            SKCartesianChart skChart = new()
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Background = SKColors.White
            };

            // Show title if enabled
            if (ShowTitle)
            {
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                skChart.Title = new LabelVisual
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

            // Copy axis configuration with proper SKia paints
            if (sourceChart.XAxes != null)
            {
                skChart.XAxes = sourceChart.XAxes.Select(axis =>
                {
                    Axis newAxis = new()
                    {
                        Labels = axis.Labels,
                        Labeler = axis.Labeler,
                        MinLimit = axis.MinLimit,
                        MaxLimit = axis.MaxLimit,
                        LabelsPaint = new SolidColorPaint(SKColors.Black),
                        TextSize = 11
                    };
                    return newAxis;
                }).ToArray();
            }

            if (sourceChart.YAxes != null)
            {
                skChart.YAxes = sourceChart.YAxes.Select(axis =>
                {
                    Axis newAxis = new()
                    {
                        Labels = axis.Labels,
                        Labeler = axis.Labeler,
                        MinLimit = axis.MinLimit,
                        MaxLimit = axis.MaxLimit,
                        LabelsPaint = new SolidColorPaint(SKColors.Black),
                        SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                        TextSize = 11
                    };
                    return newAxis;
                }).ToArray();
            }

            skChart.LegendPosition = ShowLegend
                ? LegendPosition.Top
                : LegendPosition.Hidden;

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

            // Show title if enabled
            if (ShowTitle)
            {
                // Get the title text based on chart type
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                skChart.Title = new LabelVisual
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
                ? LegendPosition.Right
                : LegendPosition.Hidden;

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