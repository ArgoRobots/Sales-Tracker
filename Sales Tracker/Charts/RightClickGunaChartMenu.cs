using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    public static class RightClickGunaChartMenu
    {
        // Properties
        private static Guna2Panel _rightClickGunaChart_Panel;
        private static Guna2Button resetZoomButton;

        // Getter
        public static Guna2Panel RightClickGunaChart_Panel => _rightClickGunaChart_Panel;

        // Right click menu methods
        public static void ConstructRightClickGunaChartMenu()
        {
            Guna2Panel panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth - 50, 4 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickGunaChart_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
            int newBtnWidth = CustomControls.PanelBtnWidth - 50;

            Guna2Button button = CustomControls.ConstructBtnForMenu("Save image", newBtnWidth, true, flowPanel);
            button.Click += SaveImage;

            // Export buttons are only visible when chart has data
            button = CustomControls.ConstructBtnForMenu("Export to Microsoft Excel", newBtnWidth, true, flowPanel);
            button.Click += ExportToMicrosoftExcel;

            button = CustomControls.ConstructBtnForMenu("Export to Google Sheets", newBtnWidth, true, flowPanel);
            button.Click += ExportToGoogleSheets;

            resetZoomButton = CustomControls.ConstructBtnForMenu("Reset zoom", newBtnWidth, true, flowPanel);
            resetZoomButton.Click += ResetZoom;

            _rightClickGunaChart_Panel = panel;
        }
        private static void SaveImage(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            dialog.FileName = $"{Directories.CompanyName} {chart.Name} {date}";
            dialog.DefaultExt = ArgoFiles.PngFileExtension;
            dialog.Filter = $"PNG Image|*{ArgoFiles.PngFileExtension}";
            dialog.Title = "Save Chart Image";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                chart.Export(dialog.FileName);
            }
        }
        private static void ExportToMicrosoftExcel(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            string directory = "";

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            string name = chart.Title.Text.Split(':')[0];
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

            bool isLine = MainMenu_Form.Instance.LineGraph_ToggleSwitch.Checked;

            switch (chart.Tag)
            {
                case MainMenu_Form.ChartDataType.TotalRevenue:
                    LoadChart.LoadTotalsIntoChart(activeDataGridView, MainMenu_Form.Instance.GetTotalsChart(), isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.DistributionOfRevenue:
                    LoadChart.LoadDistributionIntoChart(activeDataGridView, MainMenu_Form.Instance.GetDistributionChart(), PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalProfits:
                    LoadChart.LoadProfitsIntoChart(MainMenu_Form.Instance.Profits_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                    LoadChart.LoadCountriesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CountriesOfOrigin_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                    LoadChart.LoadCompaniesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CompaniesOfOrigin_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfDestination:
                    LoadChart.LoadCountriesOfDestinationForProductsIntoChart(MainMenu_Form.Instance.CountriesOfDestination_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.Accountants:
                    LoadChart.LoadAccountantsIntoChart(MainMenu_Form.Instance.Accountants_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                    LoadChart.LoadSalesVsExpensesChart(MainMenu_Form.Instance.SalesVsExpenses_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.AverageOrderValue:
                    LoadChart.LoadAverageTransactionValueChart(MainMenu_Form.Instance.AverageTransactionValue_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalTransactions:
                    LoadChart.LoadTotalTransactionsChart(MainMenu_Form.Instance.TotalTransactions_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.AverageShippingCosts:
                    LoadChart.LoadAverageShippingCostsChart(MainMenu_Form.Instance.AverageShippingCosts_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.GrowthRates:
                    LoadChart.LoadGrowthRateChart(MainMenu_Form.Instance.GrowthRates_Chart, true, directory);
                    break;
            }
        }
        private static async void ExportToGoogleSheets(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            Guna2DataGridView activeDataGridView = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? MainMenu_Form.Instance.Sale_DataGridView
                : MainMenu_Form.Instance.Purchase_DataGridView;
            bool isLine = MainMenu_Form.Instance.LineGraph_ToggleSwitch.Checked;

            try
            {
                switch (chart.Tag)
                {
                    case MainMenu_Form.ChartDataType.TotalRevenue:
                        {
                            ChartData chartData = LoadChart.LoadTotalsIntoChart(activeDataGridView, MainMenu_Form.Instance.GetTotalsChart(), isLine, canUpdateChart: false);
                            string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? TranslatedChartTitles.TotalRevenue
                                : TranslatedChartTitles.TotalExpenses;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;
                            string first = LanguageManager.TranslateSingleString("Date");
                            string second = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? LanguageManager.TranslateSingleString("Revenue")
                                : LanguageManager.TranslateSingleString("Expenses");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, chartType, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.DistributionOfRevenue:
                        {
                            ChartData chartData = LoadChart.LoadDistributionIntoChart(activeDataGridView, MainMenu_Form.Instance.GetDistributionChart(), PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                                ? TranslatedChartTitles.RevenueDistribution
                                : TranslatedChartTitles.ExpensesDistribution;
                            string first = LanguageManager.TranslateSingleString("Category");
                            string second = MainMenu_Form.Instance.Sale_DataGridView.Visible
                               ? LanguageManager.TranslateSingleString("Revenue")
                               : LanguageManager.TranslateSingleString("Expenses");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.TotalProfits:
                        {
                            ChartData chartData = LoadChart.LoadProfitsIntoChart(MainMenu_Form.Instance.Profits_Chart, isLine, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.TotalProfits;
                            GoogleSheetManager.ChartType chartType = isLine
                                ? GoogleSheetManager.ChartType.Line
                                : GoogleSheetManager.ChartType.Column;
                            string first = LanguageManager.TranslateSingleString("Date");
                            string second = LanguageManager.TranslateSingleString("profits");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, chartType, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                        {
                            ChartData chartData = LoadChart.LoadCountriesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CountriesOfOrigin_Chart, PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CountriesOfOrigin;
                            string first = LanguageManager.TranslateSingleString("Countries");
                            string second = LanguageManager.TranslateSingleString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                        {
                            ChartData chartData = LoadChart.LoadCompaniesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CompaniesOfOrigin_Chart, PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CompaniesOfOrigin;
                            string first = LanguageManager.TranslateSingleString("Companies");
                            string second = LanguageManager.TranslateSingleString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.CountriesOfDestination:
                        {
                            ChartData chartData = LoadChart.LoadCountriesOfDestinationForProductsIntoChart(MainMenu_Form.Instance.CountriesOfDestination_Chart, PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.CountriesOfDestination;
                            string first = LanguageManager.TranslateSingleString("Countries");
                            string second = LanguageManager.TranslateSingleString("# of items");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.Accountants:
                        {
                            ChartData chartData = LoadChart.LoadAccountantsIntoChart(MainMenu_Form.Instance.Accountants_Chart, PieChartGrouping.Unlimited, canUpdateChart: false);
                            string chartTitle = TranslatedChartTitles.AccountantsTransactions;
                            string first = LanguageManager.TranslateSingleString("Accountants");
                            string second = LanguageManager.TranslateSingleString("# of transactions");

                            await GoogleSheetManager.ExportChartToGoogleSheetsAsync(chartData.GetData(), chartTitle, GoogleSheetManager.ChartType.Pie, first, second);
                        }
                        break;

                    case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                        {
                            SalesExpensesChartData salesExpensesData = LoadChart.LoadSalesVsExpensesChart(MainMenu_Form.Instance.SalesVsExpenses_Chart, isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in salesExpensesData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateSingleString("Total Expenses")] = salesExpensesData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateSingleString("Total Sales")] = salesExpensesData.GetSalesForDate(date)
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
                            SalesExpensesChartData chartData = LoadChart.LoadAverageTransactionValueChart(MainMenu_Form.Instance.AverageTransactionValue_Chart, isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateSingleString("Average purchase value")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateSingleString("Average sale value")] = chartData.GetSalesForDate(date)
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
                            SalesExpensesChartData chartData = LoadChart.LoadTotalTransactionsChart(MainMenu_Form.Instance.TotalTransactions_Chart, isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateSingleString("Purchases")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateSingleString("Sales")] = chartData.GetSalesForDate(date)
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
                            SalesExpensesChartData chartData = LoadChart.LoadAverageShippingCostsChart(MainMenu_Form.Instance.AverageShippingCosts_Chart, isLine, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateSingleString("Purchases")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateSingleString("Sales")] = chartData.GetSalesForDate(date)
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
                            SalesExpensesChartData chartData = LoadChart.LoadGrowthRateChart(MainMenu_Form.Instance.GrowthRates_Chart, exportToExcel: false, filePath: null, canUpdateChart: false);
                            Dictionary<string, Dictionary<string, double>> combinedData = [];

                            foreach (string date in chartData.GetDateOrder())
                            {
                                combinedData[date] = new Dictionary<string, double>
                                {
                                    [LanguageManager.TranslateSingleString("Expenses growth %")] = chartData.GetExpensesForDate(date),
                                    [LanguageManager.TranslateSingleString("Revenue growth %")] = chartData.GetSalesForDate(date)
                                };
                            }

                            string name = TranslatedChartTitles.GrowthRates;
                            GoogleSheetManager.ChartType chartType = GoogleSheetManager.ChartType.Spline;

                            await GoogleSheetManager.ExportMultiDataSetChartToGoogleSheetsAsync(combinedData, name, chartType);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error_ExportingChart(ex.Message);
            }
        }
        private static void ResetZoom(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            chart.ResetZoom();
        }

        // Other methods
        public static void ShowMenu(GunaChart chart, Point mousePosition)
        {
            if (chart.DatasetCount == 0) { return; }

            Form form = chart.FindForm();
            _rightClickGunaChart_Panel.Tag = chart;
            Point localMousePosition = form.PointToClient(mousePosition);
            int formWidth = form.ClientSize.Width;
            int formHeight = form.ClientSize.Height;
            byte offset = ReadOnlyVariables.OffsetRightClickPanel;
            byte padding = ReadOnlyVariables.PaddingRightClickPanel;

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickGunaChart_Panel.Controls[0];
            if (chart.Datasets[0] is GunaPieDataset)
            {
                flowPanel.Controls.Remove(resetZoomButton);
            }
            else
            {
                flowPanel.Controls.Add(resetZoomButton);
            }

            CustomControls.SetRightClickMenuHeight(_rightClickGunaChart_Panel);

            // Calculate the horizontal position
            bool tooFarRight = false;
            if (_rightClickGunaChart_Panel.Width + localMousePosition.X - offset + padding > formWidth)
            {
                _rightClickGunaChart_Panel.Left = formWidth - _rightClickGunaChart_Panel.Width - padding;
                tooFarRight = true;
            }
            else
            {
                _rightClickGunaChart_Panel.Left = localMousePosition.X - offset;
            }

            // Calculate the vertical position
            if (localMousePosition.Y + _rightClickGunaChart_Panel.Height + padding > formHeight)
            {
                _rightClickGunaChart_Panel.Top = formHeight - _rightClickGunaChart_Panel.Height - padding;
                if (!tooFarRight)
                {
                    _rightClickGunaChart_Panel.Left += offset;
                }
            }
            else
            {
                _rightClickGunaChart_Panel.Top = localMousePosition.Y;
            }

            form.Controls.Add(_rightClickGunaChart_Panel);
            _rightClickGunaChart_Panel.BringToFront();
        }
    }
}