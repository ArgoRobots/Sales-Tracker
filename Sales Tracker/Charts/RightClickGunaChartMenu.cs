using Guna.UI2.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    public static class RightClickGunaChartMenu
    {
        // Properties
        private static Guna2Button _resetZoomButton;

        // Getter
        public static Guna2Panel RightClickGunaChart_Panel { get; private set; }

        // Right click menu methods
        public static void ConstructRightClickGunaChartMenu()
        {
            RightClickGunaChart_Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth - 50, 4 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickGunaChart_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)RightClickGunaChart_Panel.Controls[0];
            int newBtnWidth = CustomControls.PanelBtnWidth - 50;

            Guna2Button button = CustomControls.ConstructBtnForMenu("Save image", newBtnWidth, true, flowPanel);
            button.Click += SaveImage;

            button = CustomControls.ConstructBtnForMenu("Export to Microsoft Excel", newBtnWidth, true, flowPanel);
            button.Click += ExportToMicrosoftExcel;

            button = CustomControls.ConstructBtnForMenu("Export to Google Sheets", newBtnWidth, true, flowPanel);
            button.Click += ExportToGoogleSheets;

            _resetZoomButton = CustomControls.ConstructBtnForMenu("Reset zoom", newBtnWidth, true, flowPanel);
            _resetZoomButton.Click += ResetZoom;
        }
        private static void SaveImage(object sender, EventArgs e)
        {
            Control chart = (Control)RightClickGunaChart_Panel.Tag;

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            dialog.FileName = $"{Directories.CompanyName} {chart.Name} {date}";
            dialog.DefaultExt = ArgoFiles.PngFileExtension;
            dialog.Filter = $"PNG Image|*{ArgoFiles.PngFileExtension}";
            dialog.Title = "Save Chart Image";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveChartAsImage(chart, dialog.FileName);
            }
        }
        private static void SaveChartAsImage(Control chart, string fileName)
        {
            try
            {
                switch (chart)
                {
                    case CartesianChart cartesianChart:
                        SaveCartesianChartAsImage(cartesianChart, fileName);
                        break;
                    case PieChart pieChart:
                        SavePieChartAsImage(pieChart, fileName);
                        break;
                    default:
                        throw new NotSupportedException($"Chart type {chart.GetType().Name} is not supported for image export");
                }
            }
            catch (Exception ex)
            {
                Log.Error_ExportingChart($"Failed to save chart image: {ex.Message}");
            }
        }
        private static void SaveCartesianChartAsImage(CartesianChart chart, string fileName)
        {
            // Capture the chart as a bitmap
            using Bitmap bitmap = new(chart.Width, chart.Height);
            chart.DrawToBitmap(bitmap, new Rectangle(0, 0, chart.Width, chart.Height));
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
        private static void SavePieChartAsImage(PieChart chart, string fileName)
        {
            // Capture the chart as a bitmap
            using Bitmap bitmap = new(chart.Width, chart.Height);
            chart.DrawToBitmap(bitmap, new Rectangle(0, 0, chart.Width, chart.Height));
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
        private static void ExportToMicrosoftExcel(object sender, EventArgs e)
        {
            Control chart = (Control)RightClickGunaChart_Panel.Tag;
            string directory = "";

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            string name = GetChartTitle(chart).Split(':')[0];
            dialog.FileName = $"{Directories.CompanyName} {name} {date}";
            dialog.DefaultExt = ArgoFiles.XlsxFileExtension;
            dialog.Filter = $"XLSX spreadsheet|*{ArgoFiles.XlsxFileExtension}";
            dialog.Title = "Export Chart to XLSX";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                directory = dialog.FileName;
            }

            Guna2DataGridView activeDataGridView = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? MainMenu_Form.Instance.Sale_DataGridView
                : MainMenu_Form.Instance.Purchase_DataGridView;

            bool isLine = MainMenu_Form.Instance.LineChart_ToggleSwitch.Checked;

            switch (chart.Tag)
            {
                case MainMenu_Form.ChartDataType.TotalRevenue:
                    LoadChart.LoadTotalsIntoChart(activeDataGridView, GetCartesianChart(MainMenu_Form.Instance.GetTotalsChart()), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.DistributionOfRevenue:
                    LoadChart.LoadDistributionIntoChart(activeDataGridView, GetPieChart(MainMenu_Form.Instance.GetDistributionChart()), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalProfits:
                    LoadChart.LoadProfitsIntoChart(GetCartesianChart(MainMenu_Form.Instance.Profits_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                    LoadChart.LoadCountriesOfOriginForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CountriesOfOrigin_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                    LoadChart.LoadCompaniesOfOriginForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CompaniesOfOrigin_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfDestination:
                    LoadChart.LoadCountriesOfDestinationForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CountriesOfDestination_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.Accountants:
                    LoadChart.LoadAccountantsIntoChart(GetPieChart(MainMenu_Form.Instance.Accountants_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                    LoadChart.LoadSalesVsExpensesChart(GetCartesianChart(MainMenu_Form.Instance.SalesVsExpenses_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.AverageOrderValue:
                    LoadChart.LoadAverageTransactionValueChart(GetCartesianChart(MainMenu_Form.Instance.AverageTransactionValue_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalTransactions:
                    LoadChart.LoadTotalTransactionsChart(GetCartesianChart(MainMenu_Form.Instance.TotalTransactions_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.AverageShippingCosts:
                    LoadChart.LoadAverageShippingCostsChart(GetCartesianChart(MainMenu_Form.Instance.AverageShippingCosts_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.GrowthRates:
                    LoadChart.LoadGrowthRateChart(GetCartesianChart(MainMenu_Form.Instance.GrowthRates_Chart), true, directory);
                    break;

                case MainMenu_Form.ChartDataType.ReturnsOverTime:
                    LoadChart.LoadReturnsOverTimeChart(GetCartesianChart(MainMenu_Form.Instance.ReturnsOverTime_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.ReturnReasons:
                    LoadChart.LoadReturnReasonsChart(GetPieChart(MainMenu_Form.Instance.ReturnReasons_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.ReturnFinancialImpact:
                    LoadChart.LoadReturnFinancialImpactChart(GetCartesianChart(MainMenu_Form.Instance.ReturnFinancialImpact_Chart), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.ReturnsByCategory:
                    LoadChart.LoadReturnsByCategoryChart(GetPieChart(MainMenu_Form.Instance.ReturnsByCategory_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.ReturnsByProduct:
                    LoadChart.LoadReturnsByProductChart(GetPieChart(MainMenu_Form.Instance.ReturnsByProduct_Chart), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.PurchaseVsSaleReturns:
                    LoadChart.LoadPurchaseVsSaleReturnsChart(GetPieChart(MainMenu_Form.Instance.PurchaseVsSaleReturns_Chart), true, directory);
                    break;
            }
        }
        private static async void ExportToGoogleSheets(object sender, EventArgs e)
        {
            Control chart = (Control)RightClickGunaChart_Panel.Tag;
            Guna2DataGridView activeDataGridView = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? MainMenu_Form.Instance.Sale_DataGridView
                : MainMenu_Form.Instance.Purchase_DataGridView;
            bool isLine = MainMenu_Form.Instance.LineChart_ToggleSwitch.Checked;

            try
            {
                switch (chart.Tag)
                {
                    case MainMenu_Form.ChartDataType.TotalRevenue:
                        {
                            ChartData chartData = LoadChart.LoadTotalsIntoChart(activeDataGridView, GetCartesianChart(MainMenu_Form.Instance.GetTotalsChart()), isLine, canUpdateChart: false);
                            string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? TranslatedChartTitles.TotalRevenue
                                : TranslatedChartTitles.TotalExpenses;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;
                            string first = LanguageManager.TranslateString("Date");
                            string second = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? LanguageManager.TranslateString("Revenue")
                                : LanguageManager.TranslateString("Expenses");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, chartType, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.DistributionOfRevenue:
                        {
                            ChartData chartData = LoadChart.LoadDistributionIntoChart(activeDataGridView, GetPieChart(MainMenu_Form.Instance.GetDistributionChart()), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? TranslatedChartTitles.RevenueDistribution
                                : TranslatedChartTitles.ExpensesDistribution;
                            string first = LanguageManager.TranslateString("Category");
                            string second = MainMenu_Form.Instance.Sale_DataGridView.Visible
                               ? LanguageManager.TranslateString("Revenue")
                               : LanguageManager.TranslateString("Expenses");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.TotalProfits:
                        {
                            ChartData chartData = LoadChart.LoadProfitsIntoChart(GetCartesianChart(MainMenu_Form.Instance.Profits_Chart), isLine, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.TotalProfits;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;
                            string first = LanguageManager.TranslateString("Date");
                            string second = LanguageManager.TranslateString("profits");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, chartType, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                        {
                            ChartData chartData = LoadChart.LoadCountriesOfOriginForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CountriesOfOrigin_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CountriesOfOrigin;
                            string first = LanguageManager.TranslateString("Countries");
                            string second = LanguageManager.TranslateString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                        {
                            ChartData chartData = LoadChart.LoadCompaniesOfOriginForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CompaniesOfOrigin_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CompaniesOfOrigin;
                            string first = LanguageManager.TranslateString("Companies");
                            string second = LanguageManager.TranslateString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfDestination:
                        {
                            ChartData chartData = LoadChart.LoadCountriesOfDestinationForProductsIntoChart(GetPieChart(MainMenu_Form.Instance.CountriesOfDestination_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CountriesOfDestination;
                            string first = LanguageManager.TranslateString("Countries");
                            string second = LanguageManager.TranslateString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.Accountants:
                        {
                            ChartData chartData = LoadChart.LoadAccountantsIntoChart(GetPieChart(MainMenu_Form.Instance.Accountants_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.AccountantsTransactions;
                            string first = LanguageManager.TranslateString("Accountants");
                            string second = LanguageManager.TranslateString("# of transactions");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                        {
                            SalesExpensesChartData salesExpensesData = LoadChart.LoadSalesVsExpensesChart(GetCartesianChart(MainMenu_Form.Instance.SalesVsExpenses_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in salesExpensesData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Total Expenses")] = salesExpensesData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Total Sales")] = salesExpensesData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.SalesVsExpenses;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.AverageOrderValue:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadAverageTransactionValueChart(GetCartesianChart(MainMenu_Form.Instance.AverageTransactionValue_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Average purchase value")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Average sale value")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.AverageTransactionValue;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.TotalTransactions:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadTotalTransactionsChart(GetCartesianChart(MainMenu_Form.Instance.TotalTransactions_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Purchases")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Sales")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.TotalTransactions;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.AverageShippingCosts:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadAverageShippingCostsChart(GetCartesianChart(MainMenu_Form.Instance.AverageShippingCosts_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Purchases")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Sales")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.AverageShippingCosts;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.GrowthRates:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadGrowthRateChart(GetCartesianChart(MainMenu_Form.Instance.GrowthRates_Chart), exportToExcel: false, filePath: null, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Expenses growth %")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Revenue growth %")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.GrowthRates;
                            GoogleSheetManager.ChartType chartType = GoogleSheetManager.ChartType.Spline;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsOverTime:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadReturnsOverTimeChart(GetCartesianChart(MainMenu_Form.Instance.ReturnsOverTime_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Purchase returns")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Sale returns")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string chartTitle = TranslatedChartTitles.ReturnsOverTime;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetCountChartToGoogleSheetsAsync(combinedData, chartTitle, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.ReturnReasons:
                        {
                            ChartCountData chartData = LoadChart.LoadReturnReasonsChart(GetPieChart(MainMenu_Form.Instance.ReturnReasons_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.ReturnReasons;
                            string first = LanguageManager.TranslateString("Reasons");
                            string second = LanguageManager.TranslateString("# of returns");

                            await GoogleSheetManager.ExportCountChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.ReturnFinancialImpact:
                        {
                            SalesExpensesChartData chartData = LoadChart.LoadReturnFinancialImpactChart(GetCartesianChart(MainMenu_Form.Instance.ReturnFinancialImpact_Chart), isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateString("Purchase return value")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateString("Sale return value")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string chartTitle = TranslatedChartTitles.ReturnFinancialImpact;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, chartTitle, chartType);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsByCategory:
                        {
                            ChartCountData chartData = LoadChart.LoadReturnsByCategoryChart(GetPieChart(MainMenu_Form.Instance.ReturnsByCategory_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.ReturnsByCategory;
                            string first = LanguageManager.TranslateString("Categories");
                            string second = LanguageManager.TranslateString("# of returns");

                            await GoogleSheetManager.ExportCountChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.ReturnsByProduct:
                        {
                            ChartCountData chartData = LoadChart.LoadReturnsByProductChart(GetPieChart(MainMenu_Form.Instance.ReturnsByProduct_Chart), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.ReturnsByProduct;
                            string first = LanguageManager.TranslateString("Products");
                            string second = LanguageManager.TranslateString("# of returns");

                            await GoogleSheetManager.ExportCountChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.PurchaseVsSaleReturns:
                        {
                            ChartCountData chartData = LoadChart.LoadPurchaseVsSaleReturnsChart(GetPieChart(MainMenu_Form.Instance.PurchaseVsSaleReturns_Chart), canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.PurchaseVsSaleReturns;
                            string first = LanguageManager.TranslateString("Transaction Type");
                            string second = LanguageManager.TranslateString("# of returns");

                            await GoogleSheetManager.ExportCountChartToGoogleSheetsAsync(chartData.Data, chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Don't show error message for user-initiated cancellation
                // This is expected behavior, not an error
            }
            catch (Exception ex)
            {
                Log.Error_ExportingChart(ex.Message);
            }
        }
        private static void ResetZoom(object sender, EventArgs e)
        {
            Control chart = (Control)RightClickGunaChart_Panel.Tag;

            try
            {
                switch (chart)
                {
                    case CartesianChart cartesianChart:
                        ResetCartesianChartZoom(cartesianChart);
                        break;
                    case PieChart pieChart:
                        // Pie charts don't typically have zoom functionality
                        // But we can refresh them
                        pieChart.Invalidate();
                        break;
                    default:
                        throw new NotSupportedException($"Chart type {chart.GetType().Name} is not supported for zoom reset");
                }
            }
            catch (Exception ex)
            {
                Log.Error_ExportingChart($"Failed to reset chart zoom: {ex.Message}");
            }
        }
        private static void ResetCartesianChartZoom(CartesianChart chart)
        {
            // LiveCharts doesn't have a built-in ResetZoom method like Guna
            // We need to reset the axes to their default state
            if (chart.XAxes != null)
            {
                chart.XAxes = chart.XAxes.Select(axis => new Axis
                {
                    IsVisible = axis.IsVisible,
                    TextSize = axis.TextSize,
                    SeparatorsPaint = axis.SeparatorsPaint,
                    LabelsPaint = axis.LabelsPaint,
                    TicksPaint = axis.TicksPaint,
                    Labeler = axis.Labeler,
                    // Reset any zoom-related properties
                    MinLimit = null,
                    MaxLimit = null
                }).ToArray();
            }

            if (chart.YAxes != null)
            {
                chart.YAxes = chart.YAxes.Select(axis => new Axis
                {
                    IsVisible = axis.IsVisible,
                    TextSize = axis.TextSize,
                    SeparatorsPaint = axis.SeparatorsPaint,
                    LabelsPaint = axis.LabelsPaint,
                    TicksPaint = axis.TicksPaint,
                    Labeler = axis.Labeler,
                    // Reset any zoom-related properties
                    MinLimit = null,
                    MaxLimit = null
                }).ToArray();
            }

            chart.Invalidate();
        }

        // Other methods
        public static void ShowMenu(Control chart, Point mousePosition)
        {
            if (!ChartHasData(chart)) { return; }

            Form form = chart.FindForm();
            RightClickGunaChart_Panel.Tag = chart;
            Point localMousePosition = form.PointToClient(mousePosition);
            int formWidth = form.ClientSize.Width;
            int formHeight = form.ClientSize.Height;
            byte offset = ReadOnlyVariables.OffsetRightClickPanel;
            byte padding = ReadOnlyVariables.PaddingRightClickPanel;

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)RightClickGunaChart_Panel.Controls[0];
            if (IsPieChart(chart))
            {
                flowPanel.Controls.Remove(_resetZoomButton);
            }
            else
            {
                flowPanel.Controls.Add(_resetZoomButton);
            }

            CustomControls.SetRightClickMenuHeight(RightClickGunaChart_Panel);

            // Calculate the horizontal position
            bool tooFarRight = false;
            if (RightClickGunaChart_Panel.Width + localMousePosition.X - offset + padding > formWidth)
            {
                RightClickGunaChart_Panel.Left = formWidth - RightClickGunaChart_Panel.Width - padding;
                tooFarRight = true;
            }
            else
            {
                RightClickGunaChart_Panel.Left = localMousePosition.X - offset;
            }

            // Calculate the vertical position
            if (localMousePosition.Y + RightClickGunaChart_Panel.Height + padding > formHeight)
            {
                RightClickGunaChart_Panel.Top = formHeight - RightClickGunaChart_Panel.Height - padding;
                if (!tooFarRight)
                {
                    RightClickGunaChart_Panel.Left += offset;
                }
            }
            else
            {
                RightClickGunaChart_Panel.Top = localMousePosition.Y;
            }

            form.Controls.Add(RightClickGunaChart_Panel);
            RightClickGunaChart_Panel.BringToFront();
        }

        // Helper methods for chart type handling
        private static bool ChartHasData(Control chart)
        {
            return chart switch
            {
                CartesianChart cartesianChart => cartesianChart.Series?.Any() == true,
                PieChart pieChart => pieChart.Series?.Any() == true,
                _ => false
            };
        }
        private static bool IsPieChart(Control chart)
        {
            return chart is PieChart;
        }
        private static string GetChartTitle(Control chart)
        {
            return chart switch
            {
                CartesianChart cartesianChart => cartesianChart.Title.ToString() ?? chart.Name ?? "Chart",
                PieChart pieChart => pieChart.Title.ToString() ?? chart.Name ?? "Chart",
                _ => chart.Name ?? "Chart"
            };
        }

        // Helper methods to safely cast charts for LoadChart methods
        private static CartesianChart GetCartesianChart(object chart)
        {
            return chart as CartesianChart ?? throw new ArgumentException("Expected CartesianChart", nameof(chart));
        }
        private static PieChart GetPieChart(object chart)
        {
            return chart as PieChart ?? throw new ArgumentException("Expected PieChart", nameof(chart));
        }
    }
}