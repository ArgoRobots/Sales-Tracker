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
using Sales_Tracker.ReportGenerator.Menus;
using SkiaSharp;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Chart element for displaying graphs and charts.
    /// </summary>
    public class ChartElement : BaseElement, IDisposable
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
            }
        }
        public bool ShowLegend { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public Color BorderColor { get; set; } = Color.Gray;
        public string FontFamily { get; set; } = "Segoe UI";
        public float TitleFontSize { get; set; } = 12f;
        public float LegendFontSize { get; set; } = 11f;

        // Overrides
        public override ReportElementType GetElementType() => ReportElementType.Chart;
        public override BaseElement Clone()
        {
            return new ChartElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ChartType = ChartType,
                ShowLegend = ShowLegend,
                ShowTitle = ShowTitle,
                BorderColor = BorderColor,
                FontFamily = FontFamily,
                TitleFontSize = TitleFontSize
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config, float renderScale)
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

                // Generate server-side image with quality scaling
                // Get the current graphics transform to calculate render scale
                float scaleX = graphics.Transform.Elements[0];
                float scaleY = graphics.Transform.Elements[3];

                if (_chartControl != null && _chartControl is CartesianChart cartesianChart)
                {
                    using Bitmap chartImage = GenerateCartesianChartImage(cartesianChart, renderScale);
                    if (chartImage != null)
                    {
                        // Save graphics state to preserve transformation
                        GraphicsState state = graphics.Save();
                        try
                        {
                            // Reset transform for drawing
                            graphics.ResetTransform();

                            // Calculate the actual screen position accounting for the original transform
                            PointF[] points = [new PointF(Bounds.X, Bounds.Y)];
                            Matrix tempTransform = new(scaleX, 0, 0, scaleY, graphics.Transform.Elements[4], graphics.Transform.Elements[5]);
                            tempTransform.TransformPoints(points);

                            // Calculate target rectangle in screen coordinates
                            Rectangle targetRect = new(
                                (int)points[0].X,
                                (int)points[0].Y,
                                (int)(Bounds.Width * scaleX),
                                (int)(Bounds.Height * scaleY)
                            );

                            // Draw with high quality interpolation
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            graphics.DrawImage(chartImage, targetRect);
                        }
                        finally
                        {
                            graphics.Restore(state);
                        }
                    }
                }
                else if (_chartControl != null && _chartControl is PieChart pieChart)
                {
                    using Bitmap chartImage = GeneratePieChartImage(pieChart, renderScale);
                    if (chartImage != null)
                    {
                        GraphicsState state = graphics.Save();
                        try
                        {
                            graphics.ResetTransform();
                            PointF[] points = [new PointF(Bounds.X, Bounds.Y)];
                            Matrix tempTransform = new(scaleX, 0, 0, scaleY, graphics.Transform.Elements[4], graphics.Transform.Elements[5]);
                            tempTransform.TransformPoints(points);

                            Rectangle targetRect = new(
                                (int)points[0].X,
                                (int)points[0].Y,
                                (int)(Bounds.Width * scaleX),
                                (int)(Bounds.Height * scaleY)
                            );

                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            graphics.DrawImage(chartImage, targetRect);
                        }
                        finally
                        {
                            graphics.Restore(state);
                        }
                    }
                }
                else if (_chartControl != null && _chartControl is GeoMap geoMap)
                {
                    using Bitmap chartImage = GenerateGeoMapImage(geoMap, renderScale);
                    if (chartImage != null)
                    {
                        GraphicsState state = graphics.Save();
                        try
                        {
                            graphics.ResetTransform();
                            PointF[] points = [new PointF(Bounds.X, Bounds.Y)];
                            Matrix tempTransform = new(scaleX, 0, 0, scaleY, graphics.Transform.Elements[4], graphics.Transform.Elements[5]);
                            tempTransform.TransformPoints(points);

                            Rectangle targetRect = new(
                                (int)points[0].X,
                                (int)points[0].Y,
                                (int)(Bounds.Width * scaleX),
                                (int)(Bounds.Height * scaleY)
                            );

                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            graphics.DrawImage(chartImage, targetRect);
                        }
                        finally
                        {
                            graphics.Restore(state);
                        }
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
            if (config?.Filters == null) { return false; }

            if (!_lastLoadedConfig.TryGetValue(chartType, out (DateTime? StartDate, DateTime? EndDate, bool IncludeReturns, bool IncludeLosses) lastConfig))
            {
                return true;  // Never loaded before
            }

            // Use tuple comparison for cleaner code
            (DateTime? StartDate, DateTime? EndDate, bool IncludeReturns, bool IncludeLosses) currentConfig = (
                config.Filters.StartDate,
                config.Filters.EndDate,
                config.Filters.IncludeReturns,
                config.Filters.IncludeLosses
            );

            return lastConfig != currentConfig;
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
                    null
                );
            }
            else
            {
                DataGridViewManager.InitializeDataGridView(
                    filteredGrid,
                    "FilteredSales_DataGridView",
                    MainMenu_Form.Instance.SalesColumnHeaders,
                    null,
                    null
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
        private Bitmap GenerateCartesianChartImage(CartesianChart sourceChart, float renderScale = 1.0f)
        {
            // Render at much higher resolution for quality
            int renderWidth = (int)(Bounds.Width * renderScale);
            int renderHeight = (int)(Bounds.Height * renderScale);

            SKCartesianChart skChart = new()
            {
                Width = renderWidth,
                Height = renderHeight,
                Background = SKColors.White
            };

            // Create SKTypeface for font family
            SKTypeface typeface = SKTypeface.FromFamilyName(FontFamily);

            // Show title if enabled
            if (ShowTitle)
            {
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                SolidColorPaint titlePaint = new(SKColors.Black)
                {
                    SKTypeface = typeface
                };

                skChart.Title = new LabelVisual
                {
                    Text = titleText,
                    TextSize = (TitleFontSize + 3) * renderScale,
                    Paint = titlePaint,
                    Padding = new LiveChartsCore.Drawing.Padding(5 * renderScale)
                };
            }

            if (sourceChart.Series != null)
            {
                skChart.Series = sourceChart.Series.Select(series =>
                {
                    if (series is LineSeries<double> lineSeries)
                    {
                        return new LineSeries<double>
                        {
                            Values = lineSeries.Values,
                            Name = lineSeries.Name,
                            GeometrySize = lineSeries.GeometrySize * renderScale * 0.75,
                            GeometryFill = new SolidColorPaint(SKColors.White),
                            GeometryStroke = lineSeries.GeometryStroke != null && lineSeries.GeometryStroke is SolidColorPaint gsPaint
                                ? new SolidColorPaint(gsPaint.Color)
                                {
                                    StrokeThickness = (float)(gsPaint.StrokeThickness * renderScale * 0.75)
                                }
                                : lineSeries.GeometryStroke,
                            Fill = lineSeries.Fill != null && lineSeries.Fill is SolidColorPaint fillPaint
                                ? new SolidColorPaint(fillPaint.Color)
                                {
                                    StrokeThickness = (float)(fillPaint.StrokeThickness * renderScale)
                                }
                                : lineSeries.Fill,
                            Stroke = lineSeries.Stroke != null && lineSeries.Stroke is SolidColorPaint strokePaint
                                ? new SolidColorPaint(strokePaint.Color)
                                {
                                    StrokeThickness = (float)(strokePaint.StrokeThickness * renderScale * 0.75)
                                }
                                : lineSeries.Stroke,
                            LineSmoothness = lineSeries.LineSmoothness,
                            DataLabelsSize = lineSeries.DataLabelsSize * renderScale,
                            DataLabelsPaint = lineSeries.DataLabelsPaint != null && lineSeries.DataLabelsPaint is SolidColorPaint dlPaint
                                ? new SolidColorPaint(dlPaint.Color)
                                {
                                    SKTypeface = typeface
                                }
                                : lineSeries.DataLabelsPaint,
                            DataLabelsPosition = lineSeries.DataLabelsPosition
                        };
                    }
                    else if (series is ColumnSeries<double> columnSeries)
                    {
                        return new ColumnSeries<double>
                        {
                            Values = columnSeries.Values,
                            Name = columnSeries.Name,
                            MaxBarWidth = columnSeries.MaxBarWidth * renderScale,
                            Padding = columnSeries.Padding * renderScale,
                            Rx = columnSeries.Rx > 0 ? columnSeries.Rx * renderScale : 2 * renderScale,
                            Ry = columnSeries.Ry > 0 ? columnSeries.Ry * renderScale : 2 * renderScale,
                            Fill = columnSeries.Fill != null && columnSeries.Fill is SolidColorPaint fillPaint
                                ? new SolidColorPaint(fillPaint.Color)
                                {
                                    StrokeThickness = (float)(fillPaint.StrokeThickness * renderScale)
                                }
                                : columnSeries.Fill,
                            Stroke = columnSeries.Stroke != null && columnSeries.Stroke is SolidColorPaint strokePaint
                                ? new SolidColorPaint(strokePaint.Color)
                                {
                                    StrokeThickness = (float)(strokePaint.StrokeThickness * renderScale)
                                }
                                : columnSeries.Stroke,
                            DataLabelsSize = columnSeries.DataLabelsSize * renderScale,
                            DataLabelsPaint = columnSeries.DataLabelsPaint != null && columnSeries.DataLabelsPaint is SolidColorPaint dlPaint
                                ? new SolidColorPaint(dlPaint.Color)
                                {
                                    SKTypeface = typeface
                                }
                                : columnSeries.DataLabelsPaint,
                            DataLabelsPosition = columnSeries.DataLabelsPosition
                        };
                    }
                    return series;
                }).ToArray();
            }

            // Copy axis configuration with font properties
            if (sourceChart.XAxes != null)
            {
                skChart.XAxes = sourceChart.XAxes.Select(axis =>
                {
                    SolidColorPaint labelPaint = new(SKColors.Black)
                    {
                        SKTypeface = typeface
                    };

                    Axis newAxis = new()
                    {
                        Labels = axis.Labels,
                        Labeler = axis.Labeler,
                        MinLimit = axis.MinLimit,
                        MaxLimit = axis.MaxLimit,
                        LabelsPaint = labelPaint,
                        TextSize = LegendFontSize * renderScale
                    };
                    return newAxis;
                }).ToArray();
            }

            if (sourceChart.YAxes != null)
            {
                skChart.YAxes = sourceChart.YAxes.Select(axis =>
                {
                    SolidColorPaint labelPaint = new(SKColors.Black)
                    {
                        SKTypeface = typeface
                    };

                    Axis newAxis = new()
                    {
                        Labels = axis.Labels,
                        Labeler = axis.Labeler,
                        MinLimit = axis.MinLimit,
                        MaxLimit = axis.MaxLimit,
                        LabelsPaint = labelPaint,
                        SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                        {
                            StrokeThickness = 1 * renderScale
                        },
                        TextSize = LegendFontSize * renderScale
                    };
                    return newAxis;
                }).ToArray();
            }

            skChart.Legend = new CustomLegend(true, LegendFontSize);

            skChart.LegendPosition = ShowLegend
                ? LegendPosition.Top
                : LegendPosition.Hidden;

            SolidColorPaint legendPaint = new(SKColors.Black)
            {
                SKTypeface = typeface
            };
            skChart.LegendTextPaint = legendPaint;
            skChart.LegendTextSize = (LegendFontSize + 1) * renderScale;

            using SKImage image = skChart.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        private Bitmap GeneratePieChartImage(PieChart sourceChart, float renderScale)
        {
            // Render at much higher resolution for quality
            int renderWidth = (int)(Bounds.Width * renderScale);
            int renderHeight = (int)(Bounds.Height * renderScale);

            SKPieChart skChart = new()
            {
                Width = renderWidth,
                Height = renderHeight,
                Background = SKColors.White
            };

            // Create SKTypeface for font family
            SKTypeface typeface = SKTypeface.FromFamilyName(FontFamily);

            // Show title if enabled
            if (ShowTitle)
            {
                string titleText = TranslatedChartTitles.GetChartDisplayName(ChartType);

                SolidColorPaint titlePaint = new(SKColors.Black)
                {
                    SKTypeface = typeface
                };

                skChart.Title = new LabelVisual
                {
                    Text = titleText,
                    TextSize = (TitleFontSize + 3) * renderScale,
                    Paint = titlePaint,
                    Padding = new LiveChartsCore.Drawing.Padding(5 * renderScale)
                };
            }

            // Clone and scale series for pie charts
            if (sourceChart.Series != null)
            {
                skChart.Series = sourceChart.Series.Select(series =>
                {
                    if (series is PieSeries<double> pieSeries)
                    {
                        PieSeries<double> newPieSeries = new()
                        {
                            Values = pieSeries.Values,
                            Name = pieSeries.Name,
                            Fill = pieSeries.Fill,
                            Stroke = pieSeries.Stroke != null ?
                                new SolidColorPaint(((SolidColorPaint)pieSeries.Stroke).Color)
                                {
                                    StrokeThickness = (float)(pieSeries.Stroke.StrokeThickness * renderScale)
                                } : null,
                            Pushout = pieSeries.Pushout * renderScale,
                            InnerRadius = pieSeries.InnerRadius * renderScale,
                            HoverPushout = pieSeries.HoverPushout * renderScale,
                            DataLabelsSize = pieSeries.DataLabelsSize * renderScale,
                            DataLabelsPaint = pieSeries.DataLabelsPaint,
                            DataLabelsPosition = pieSeries.DataLabelsPosition
                        };
                        return newPieSeries;
                    }
                    return series;
                }).ToArray();
            }

            skChart.Legend = new CustomLegend(true, LegendFontSize);

            skChart.LegendPosition = ShowLegend
                ? LegendPosition.Right
                : LegendPosition.Hidden;

            using SKImage image = skChart.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        private Bitmap GenerateGeoMapImage(GeoMap sourceMap, float renderScale = 1.0f)
        {
            // Render at much higher resolution for quality
            int renderWidth = (int)(Bounds.Width * renderScale);
            int renderHeight = (int)(Bounds.Height * renderScale);

            // Create an SK map with the same data
            SKGeoMap skMap = new()
            {
                Width = renderWidth,
                Height = renderHeight,
                Background = SKColors.White
            };

            // Copy and scale series data from source map to SK map
            if (sourceMap.Series != null)
            {
                skMap.Series = sourceMap.Series.Select(series =>
                {
                    if (series is HeatLandSeries heatSeries)
                    {
                        HeatLandSeries newHeatSeries = new()
                        {
                            Lands = heatSeries.Lands,
                            Name = heatSeries.Name,
                            HeatMap = heatSeries.HeatMap
                        };
                        return newHeatSeries;
                    }
                    return series;
                }).ToArray();
            }

            // Copy map projection
            skMap.MapProjection = sourceMap.MapProjection;

            // Generate image
            using SKImage image = skMap.GetImage();
            using SKData data = image.Encode();
            using MemoryStream stream = new(data.ToArray());
            return new Bitmap(stream);
        }
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Get undo manager for recording property changes
            UndoRedoManager? undoRedoManager = ReportLayoutDesigner_Form.Instance?.GetUndoRedoManager();

            // Chart type selector
            AddPropertyLabel(container, "Chart:", yPosition);
            Guna2ComboBox chartCombo = AddPropertyComboBox(container, ChartType.ToString(), yPosition,
                GetAvailableChartTypes(),
                value =>
                {
                    MainMenu_Form.ChartDataType newChartType = Enum.Parse<MainMenu_Form.ChartDataType>(value);
                    if (ChartType != newChartType)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ChartType),
                            ChartType,
                            newChartType,
                            () =>
                            {
                                // Dispose old chart control when type changes
                                _chartControl?.Dispose();
                                _chartControl = null;
                                onPropertyChanged();
                            }));
                        ChartType = newChartType;
                        _chartControl?.Dispose();
                        _chartControl = null;
                        onPropertyChanged();
                    }
                });
            CacheControl("ChartType", chartCombo, () => chartCombo.SelectedItem = ChartType.ToString());
            yPosition += ControlRowHeight;

            // Font family
            AddPropertyLabel(container, "Font:", yPosition);
            string[] fontFamilies = ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana",
                             "Tahoma", "Georgia", "Courier New", "Consolas", "Trebuchet MS"];
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

            // Title font size
            AddPropertyLabel(container, "Title Size:", yPosition);
            Guna2NumericUpDown titleNumericUpDown = AddPropertyNumericUpDown(container, (decimal)TitleFontSize, yPosition,
                value =>
                {
                    float newTitleSize = (float)value;
                    if (Math.Abs(TitleFontSize - newTitleSize) > 0.01f)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(TitleFontSize),
                            TitleFontSize,
                            newTitleSize,
                            onPropertyChanged));
                        TitleFontSize = newTitleSize;
                        onPropertyChanged();
                    }
                }, 8, 20);
            titleNumericUpDown.Left = 130;
            CacheControl("TitleFontSize", titleNumericUpDown, () => titleNumericUpDown.Value = (decimal)TitleFontSize);
            yPosition += ControlRowHeight;

            // Legend font size
            AddPropertyLabel(container, "Legend Size:", yPosition);
            Guna2NumericUpDown legendNumericUpDown = AddPropertyNumericUpDown(container, (decimal)LegendFontSize, yPosition,
                value =>
                {
                    float newLegendSize = (float)value;
                    if (Math.Abs(LegendFontSize - newLegendSize) > 0.01f)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(LegendFontSize),
                            LegendFontSize,
                            newLegendSize,
                            onPropertyChanged));
                        LegendFontSize = newLegendSize;
                        onPropertyChanged();
                    }
                }, 8, 20);
            legendNumericUpDown.Left = 130;
            CacheControl("LegendFontSize", legendNumericUpDown, () => legendNumericUpDown.Value = (decimal)LegendFontSize);
            yPosition += ControlRowHeight;

            // Border color
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

            // Add label next to color picker
            Label borderColorLabel = new()
            {
                Text = "Click to change",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(140, yPosition + 11),
                AutoSize = true
            };
            container.Controls.Add(borderColorLabel);

            CacheControl("BorderColor", borderColorPanel, () => borderColorPanel.BackColor = BorderColor);
            yPosition += ControlRowHeight;

            // Show legend checkbox
            Guna2CustomCheckBox legendCheck = AddPropertyCheckBoxWithLabel(container, "Show Legend", ShowLegend, yPosition,
                value =>
                {
                    if (ShowLegend != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowLegend),
                            ShowLegend,
                            value,
                            onPropertyChanged));
                        ShowLegend = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("ShowLegend", legendCheck, () => legendCheck.Checked = ShowLegend);
            yPosition += CheckBoxRowHeight;

            // Show title checkbox
            Guna2CustomCheckBox titleCheck = AddPropertyCheckBoxWithLabel(container, "Show Title", ShowTitle, yPosition,
                value =>
                {
                    if (ShowTitle != value)
                    {
                        undoRedoManager?.RecordAction(new PropertyChangeAction(
                            this,
                            nameof(ShowTitle),
                            ShowTitle,
                            value,
                            onPropertyChanged));
                        ShowTitle = value;
                        onPropertyChanged();
                    }
                });
            CacheControl("ShowTitle", titleCheck, () => titleCheck.Checked = ShowTitle);
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

        // Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chartControl?.Dispose();
                _chartControl = null;
            }
        }
        ~ChartElement()
        {
            Dispose(false);
        }
    }
}