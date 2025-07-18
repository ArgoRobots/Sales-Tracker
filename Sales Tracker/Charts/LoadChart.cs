using Guna.UI2.WinForms;
using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using OfficeOpenXml.Drawing.Chart;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using SkiaSharp;
using System.Data;

namespace Sales_Tracker.Charts
{
    public enum PieChartGrouping
    {
        Unlimited = 0,
        Top12 = 12,
        Top8 = 8
    }

    /// <summary>
    /// Provides methods to configure, clear, and load data into LiveCharts.
    /// </summary>
    internal class LoadChart
    {
        // Configuration
        public static void ConfigureChartForBar(CartesianChart chart)
        {
            ClearChart(chart);
            SKColor textColor = ChartColors.ToSKColor(CustomColors.Text);

            chart.XAxes =
            [
                new Axis
                {
                    TextSize = 21,
                    LabelsPaint = new SolidColorPaint(textColor)
                }
            ];

            chart.YAxes =
            [
                new Axis
                {
                    TextSize = 21,
                    SeparatorsPaint = new SolidColorPaint(textColor) { StrokeThickness = 1f },
                    LabelsPaint = new SolidColorPaint(textColor),
                    TicksPaint = new SolidColorPaint(textColor) { StrokeThickness = 1f }
                }
            ];

            chart.LegendPosition = LegendPosition.Top;
            chart.LegendTextPaint = new SolidColorPaint(textColor);
            chart.LegendTextSize = 21;
            chart.ZoomMode = ZoomAndPanMode.Both;
        }
        public static void ConfigureChartForLine(CartesianChart chart)
        {
            ClearChart(chart);
            SKColor textColor = ChartColors.ToSKColor(CustomColors.Text);

            chart.XAxes =
            [
                new Axis
                {
                    TextSize = 21,
                    LabelsPaint = new SolidColorPaint(textColor)
                }
            ];

            chart.YAxes =
            [
                new Axis
                {
                    TextSize = 21,
                    SeparatorsPaint = new SolidColorPaint(textColor) { StrokeThickness = 1f },
                    LabelsPaint = new SolidColorPaint(textColor),
                    TicksPaint = new SolidColorPaint(textColor) { StrokeThickness = 1f }
                }
            ];

            chart.LegendPosition = LegendPosition.Top;
            chart.LegendTextPaint = new SolidColorPaint(textColor);
            chart.LegendTextSize = 21;
            chart.ZoomMode = ZoomAndPanMode.Both;
        }
        public static void ConfigureChartForPie(PieChart chart)
        {
            ClearPieChart(chart);
            chart.Legend = new CustomLegend();

            SKColor textColor = ChartColors.ToSKColor(CustomColors.Text);

            chart.LegendPosition = GetLegendPosition(chart);
            chart.LegendTextPaint = new SolidColorPaint(textColor);
            chart.LegendTextSize = 21;
        }
        public static LegendPosition GetLegendPosition(PieChart chart)
        {
            // Use a ratio to allow Bottom legend even if height is slightly less than width
            double ratio = (double)chart.Height / chart.Width;
            return ratio >= 0.8 || chart.Width < 550 ? LegendPosition.Bottom : LegendPosition.Right;
        }

        // Helper methods
        private static ISeries CreateStyledDataset(string label, bool isLineChart, SKColor color)
        {
            if (isLineChart)
            {
                return new LineSeries<double>
                {
                    Name = label,
                    Values = [],
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 5 },
                    GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 5 },
                    Fill = null,
                    GeometrySize = 15
                };
            }
            else
            {
                return new ColumnSeries<double>
                {
                    Name = label,
                    Values = [],
                    Fill = new SolidColorPaint(color)
                };
            }
        }

        private static Dictionary<string, double> ProcessRowsForTimeData(
            DataGridViewRowCollection rows,
            string dateFormat,
            Func<DataGridViewRow, double?> valueExtractor)
        {
            Dictionary<string, double> dataByDate = [];

            foreach (DataGridViewRow row in rows)
            {
                if (!IsRowValid(row)) { continue; }

                double? value = valueExtractor(row);
                if (!value.HasValue) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (dataByDate.TryGetValue(formattedDate, out double existing))
                {
                    dataByDate[formattedDate] = Math.Round(existing + value.Value, 2);
                }
                else
                {
                    dataByDate[formattedDate] = Math.Round(value.Value, 2);
                }
            }

            return dataByDate;
        }

        private static Dictionary<string, (double total, int count)> ProcessRowsForAverageData(
            DataGridViewRowCollection rows,
            string dateFormat,
            Func<DataGridViewRow, double?> valueExtractor)
        {
            Dictionary<string, (double total, int count)> dataByDate = [];

            foreach (DataGridViewRow row in rows)
            {
                if (!IsRowValid(row)) { continue; }

                double? value = valueExtractor(row);
                if (!value.HasValue) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (dataByDate.TryGetValue(formattedDate, out (double total, int count) existing))
                {
                    dataByDate[formattedDate] = (existing.total + value.Value, existing.count + 1);
                }
                else
                {
                    dataByDate[formattedDate] = (value.Value, 1);
                }
            }

            return dataByDate;
        }

        private static Dictionary<string, int> ProcessRowsForCountData(
            DataGridViewRowCollection rows,
            string dateFormat,
            Func<DataGridViewRow, bool> shouldCount)
        {
            Dictionary<string, int> dataByDate = [];

            foreach (DataGridViewRow row in rows)
            {
                if (!row.Visible) { continue; }

                if (!shouldCount(row)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (dataByDate.TryGetValue(formattedDate, out int existing))
                {
                    dataByDate[formattedDate] = existing + 1;
                }
                else
                {
                    dataByDate[formattedDate] = 1;
                }
            }

            return dataByDate;
        }

        private static void ProcessPieChartData<T>(
       Dictionary<string, T> data,
       PieChartGrouping grouping,
       List<PieSeries<double>> dataset,
       bool exportToExcel,
       string filePath,
       string chartTitle,
       string categoryLabel,
       string valueLabel,
       bool canUpdateChart,
       PieChart chart) where T : struct, IConvertible
        {
            double totalCount = data.Values.Sum(x => Convert.ToDouble(x));

            Dictionary<string, double> doubleData = data.ToDictionary(kvp => kvp.Key, kvp => Convert.ToDouble(kvp.Value));
            Dictionary<string, double> groupedData = SortAndGroupData(doubleData, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                if (typeof(T) == typeof(int))
                {
                    Dictionary<string, int> intData = data.ToDictionary(kvp => kvp.Key, kvp => Convert.ToInt32(kvp.Value));
                    Dictionary<string, int> groupedIntData = SortAndGroupCountData(intData, grouping);
                    ExcelSheetManager.ExportCountChartToExcel(groupedIntData, filePath, eChartType.Pie, chartTitle, categoryLabel, valueLabel);
                }
                else
                {
                    ExcelSheetManager.ExportChartToExcel(groupedData, filePath, eChartType.Pie, chartTitle, categoryLabel, valueLabel);
                }
            }
            else
            {
                foreach (KeyValuePair<string, double> item in groupedData)
                {
                    double percentage = Math.Round(item.Value / totalCount * 100, 2);
                    double roundedValue = Math.Round(item.Value, 2);

                    PieSeries<double> pieSeries = new()
                    {
                        Name = $"{item.Key} ({percentage:F2}%)",
                        Values = [roundedValue], // Use the rounded value
                        Fill = new SolidColorPaint(GetColorForIndex(dataset.Count))
                    };
                    dataset.Add(pieSeries);
                }

                if (canUpdateChart)
                {
                    UpdatePieChart(chart, dataset);
                }
            }
        }

        public enum ChartFormatting
        {
            None = 0,
            Currency = 1,
            Percentage = 2
        }

        private static void ApplyPercentageFormatToChart(CartesianChart chart)
        {
            // Set Y-axis formatter for percentage
            if (chart.YAxes != null && chart.YAxes.Any())
            {
                chart.YAxes = chart.YAxes.Select(axis => new Axis
                {
                    Labeler = value => $"{value:F1}%",
                    TextSize = axis.TextSize,
                    SeparatorsPaint = axis.SeparatorsPaint,
                    LabelsPaint = axis.LabelsPaint,
                    TicksPaint = axis.TicksPaint
                }).ToArray();
            }
        }
        public static void ApplyCurrencyFormatToChart(CartesianChart chart)
        {
            string currencySymbol = MainMenu_Form.CurrencySymbol;

            // Set Y-axis formatter for currency
            if (chart.YAxes != null && chart.YAxes.Any())
            {
                chart.YAxes = chart.YAxes.Select(axis => new Axis
                {
                    Labeler = value => $"{currencySymbol}{value:N2}",
                    TextSize = axis.TextSize,
                    SeparatorsPaint = axis.SeparatorsPaint,
                    LabelsPaint = axis.LabelsPaint,
                    TicksPaint = axis.TicksPaint
                }).ToArray();
            }
        }
        private static void UpdateChartWithTwoDatasets(
            CartesianChart chart,
            ISeries dataset1,
            ISeries dataset2,
            List<string> sortedDates = null,
            ChartFormatting formatting = ChartFormatting.Currency)
        {
            chart.Series = [dataset1, dataset2];

            // Set X-axis labels if provided
            if (sortedDates != null && sortedDates.Count > 0)
            {
                SKColor textColor = ChartColors.ToSKColor(CustomColors.Text);
                chart.XAxes = [new Axis
                {
                    Labels = sortedDates.ToArray(),
                    TextSize = 20,
                    LabelsPaint = new SolidColorPaint(textColor)
                }];
            }

            // Apply the appropriate formatting
            switch (formatting)
            {
                case ChartFormatting.Currency:
                    ApplyCurrencyFormatToChart(chart);
                    break;
                case ChartFormatting.Percentage:
                    ApplyPercentageFormatToChart(chart);
                    break;
                case ChartFormatting.None:
                    break;
            }
        }

        private static void AddDataPointsToDatasets(
            List<string> sortedDates,
            Dictionary<string, double> data1,
            Dictionary<string, double> data2,
            ISeries dataset1,
            ISeries dataset2,
            bool isLineChart)
        {
            List<double> values1 = [];
            List<double> values2 = [];
            List<string> labels = [];

            foreach (string date in sortedDates)
            {
                double value1 = data1.TryGetValue(date, out double v1) ? v1 : 0;
                double value2 = data2.TryGetValue(date, out double v2) ? v2 : 0;

                values1.Add(value1);
                values2.Add(value2);
                labels.Add(date);
            }

            if (isLineChart)
            {
                ((LineSeries<double>)dataset1).Values = values1;
                ((LineSeries<double>)dataset2).Values = values2;
            }
            else
            {
                ((ColumnSeries<double>)dataset1).Values = values1;
                ((ColumnSeries<double>)dataset2).Values = values2;
            }
        }

        private static void ExportMultiDatasetToExcel(
            List<string> sortedDates,
            Dictionary<string, double> data1,
            Dictionary<string, double> data2,
            string label1,
            string label2,
            string filePath,
            bool isLineChart,
            string chartTitle,
            bool isPercentage = false)
        {
            Dictionary<string, Dictionary<string, double>> combinedData = [];
            foreach (string date in sortedDates)
            {
                combinedData[date] = new Dictionary<string, double>
                {
                    { label1, data1.TryGetValue(date, out double v1) ? v1 : 0 },
                    { label2, data2.TryGetValue(date, out double v2) ? v2 : 0 }
                };
            }

            if (isPercentage)
            {
                ExcelSheetManager.ExportMultiDataSetChartToExcel(
                    combinedData,
                    filePath,
                    eChartType.Line,
                    chartTitle,
                    true
                );
            }
            else
            {
                ExcelSheetManager.ExportMultiDataSetChartToExcel(
                    combinedData,
                    filePath,
                    isLineChart ? eChartType.Line : eChartType.ColumnClustered,
                    chartTitle
                );
            }
        }

        // Main charts
        public static ChartData LoadTotalsIntoChart(Guna2DataGridView dataGridView, CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            int visibleRows = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridView);
            string label = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? LanguageManager.TranslateString("Revenue")
                : LanguageManager.TranslateString("Expenses");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries dataset = CreateStyledDataset(label, isLineChart, GetDefaultColor());

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Process rows
            Dictionary<string, double> revenueByDate = ProcessRowsForTimeData(
                dataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            double grandTotal = revenueByDate.Values.Sum();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                eChartType chartType = isLineChart ? eChartType.Line : eChartType.ColumnClustered;
                bool isSalesData = dataGridView == MainMenu_Form.Instance.Sale_DataGridView;
                string chartTitle = isSalesData
                     ? TranslatedChartTitles.TotalRevenue
                     : TranslatedChartTitles.TotalExpenses;
                string date = LanguageManager.TranslateString("Date");

                ExcelSheetManager.ExportChartToExcel(revenueByDate, filePath, chartType, chartTitle, date, label);
            }
            else if (canUpdateChart)
            {
                SortAndAddDatasetAndSetLabels(revenueByDate, dateFormat, dataset, isLineChart, chart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, revenueByDate);
        }
        public static ChartData LoadDistributionIntoChart(Guna2DataGridView dataGridView, PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            int visibleRows = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridView);
            string label = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? LanguageManager.TranslateString("Revenue")
                : LanguageManager.TranslateString("Expenses");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            double totalTax = 0;
            double totalShipping = 0;
            double totalFee = 0;
            Dictionary<string, double> allData = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!IsRowValid(row)) { continue; }

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double cost))
                {
                    continue;
                }

                double shipping = Convert.ToDouble(row.Cells[ReadOnlyVariables.Shipping_column].Value);
                double tax = Convert.ToDouble(row.Cells[ReadOnlyVariables.Tax_column].Value);
                double fee = Convert.ToDouble(row.Cells[ReadOnlyVariables.Fee_column].Value);
                string category = row.Cells[ReadOnlyVariables.Category_column].Value.ToString();

                totalTax += tax;
                totalShipping += shipping;
                totalFee += fee;

                if (category == ReadOnlyVariables.EmptyCell)
                {
                    // Extract categories and costs from the Tag
                    if (row.Tag is (List<string> items, TagData))
                    {
                        foreach (string item in items)
                        {
                            // Items are in the format: "itemName,categoryName,country,company,quantity,pricePerUnit,totalPrice"
                            string[] itemDetails = item.Split(',');
                            if (itemDetails.Length > 5)
                            {
                                string itemCategory = itemDetails[1];
                                double itemCost = double.Parse(itemDetails[6]);

                                if (allData.ContainsKey(itemCategory))
                                {
                                    allData[itemCategory] += itemCost;
                                }
                                else
                                {
                                    allData[itemCategory] = itemCost;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (allData.ContainsKey(category))
                    {
                        allData[category] += cost;
                    }
                    else
                    {
                        allData[category] = cost;
                    }
                }
            }

            totalTax = Math.Round(totalTax, 2);
            totalShipping = Math.Round(totalShipping, 2);
            totalFee = Math.Round(totalFee, 2);

            // Get total count to calculate percentages
            double totalCost = Math.Round(totalTax + totalShipping + totalFee + allData.Values.Sum(), 2);

            // Combine all data into one dictionary
            if (totalShipping != 0)
            {
                allData.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Shipping], totalShipping);
            }
            if (totalTax != 0)
            {
                allData.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Tax], totalTax);
            }
            if (totalFee != 0)
            {
                allData.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Fee], totalFee);
            }

            // Pie chart processing
            bool isSalesData = dataGridView == MainMenu_Form.Instance.Sale_DataGridView;
            string chartTitle = isSalesData
                ? TranslatedChartTitles.RevenueDistribution
                : TranslatedChartTitles.ExpensesDistribution;
            string category1 = LanguageManager.TranslateString("Category");

            ProcessPieChartData(allData, grouping, dataset, exportToExcel, filePath, chartTitle, category1, label, canUpdateChart, chart);

            return new ChartData(totalCost, SortAndGroupData(allData, grouping));
        }
        public static ChartData LoadProfitsIntoChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            string label = LanguageManager.TranslateString("Profits");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries dataset = CreateStyledDataset(label, isLineChart, GetDefaultColor());

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Calculate revenue from sales
            Dictionary<string, double> profitByDate = ProcessRowsForTimeData(
                salesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            // Subtract expenses from purchases
            Dictionary<string, double> expensesByDate = ProcessRowsForTimeData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            // Calculate net profit
            foreach (KeyValuePair<string, double> expense in expensesByDate)
            {
                if (profitByDate.TryGetValue(expense.Key, out double revenue))
                {
                    profitByDate[expense.Key] = Math.Round(revenue - expense.Value, 2);
                }
                else
                {
                    profitByDate[expense.Key] = Math.Round(-expense.Value, 2);
                }
            }

            double grandTotal = profitByDate.Values.Sum();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                eChartType chartType = isLineChart ? eChartType.Line : eChartType.ColumnClustered;
                string chartTitle = TranslatedChartTitles.TotalProfits;
                string date = LanguageManager.TranslateString("Date");

                ExcelSheetManager.ExportChartToExcel(profitByDate, filePath, chartType, chartTitle, date, label);
            }
            else if (canUpdateChart)
            {
                SortAndAddDatasetAndSetLabels(profitByDate, dateFormat, dataset, isLineChart, chart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, profitByDate);
        }

        // Statistics charts
        public static ChartData LoadCountriesOfOriginForProductsIntoChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> countryCounts = [];

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!IsRowValid(row)) { continue; }

                // Extract countries from the Tag
                if (row.Tag is (List<string> items, TagData))
                {
                    foreach (string item in items)
                    {
                        // Item format: "itemName,categoryName,country,company,quantity,pricePerUnit,totalPrice"
                        string[] itemDetails = item.Split(',');
                        if (itemDetails.Length > 2)
                        {
                            string itemCountry = itemDetails[2];
                            if (countryCounts.TryGetValue(itemCountry, out int itemValue))
                            {
                                countryCounts[itemCountry] = ++itemValue;
                            }
                            else
                            {
                                countryCounts[itemCountry] = 1;
                            }
                        }
                    }
                }
                else
                {
                    // Process the row normally if Tag is not set
                    string country = row.Cells[ReadOnlyVariables.Country_column].Value.ToString();
                    if (countryCounts.TryGetValue(country, out int value))
                    {
                        countryCounts[country] = ++value;
                    }
                    else
                    {
                        countryCounts[country] = 1;
                    }
                }
            }

            double totalCount = countryCounts.Values.Sum();
            string chartTitle = TranslatedChartTitles.CountriesOfOrigin;
            string countries = LanguageManager.TranslateString("Countries");

            ProcessPieChartData(countryCounts, grouping, dataset, exportToExcel, filePath, chartTitle, countries, label, canUpdateChart, chart);

            return new ChartData(totalCount, SortAndGroupCountData(countryCounts, grouping).ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value));
        }
        public static ChartData LoadCompaniesOfOriginForProductsIntoChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> companyCounts = [];

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!IsRowValid(row)) { continue; }

                // Extract companies from the Tag
                if (row.Tag is (List<string> items, TagData))
                {
                    foreach (string item in items)
                    {
                        // Item format: "itemName,categoryName,country,company,quantity,pricePerUnit,totalPrice"
                        string[] itemDetails = item.Split(',');
                        if (itemDetails.Length > 3)
                        {
                            string itemCompany = itemDetails[3];
                            if (companyCounts.TryGetValue(itemCompany, out int itemValue))
                            {
                                companyCounts[itemCompany] = ++itemValue;
                            }
                            else
                            {
                                companyCounts[itemCompany] = 1;
                            }
                        }
                    }
                }
                else
                {
                    // Process the row normally if Tag is not set
                    string company = row.Cells[ReadOnlyVariables.Company_column].Value.ToString();
                    if (companyCounts.TryGetValue(company, out int value))
                    {
                        companyCounts[company] = ++value;
                    }
                    else
                    {
                        companyCounts[company] = 1;
                    }
                }
            }

            double totalCount = companyCounts.Values.Sum();
            string chartTitle = TranslatedChartTitles.CompaniesOfOrigin;
            string companies = LanguageManager.TranslateString("Companies");

            ProcessPieChartData(companyCounts, grouping, dataset, exportToExcel, filePath, chartTitle, companies, label, canUpdateChart, chart);

            return new ChartData(totalCount, SortAndGroupCountData(companyCounts, grouping).ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value));
        }
        public static ChartData LoadCountriesOfDestinationForProductsIntoChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> countryCounts = [];

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!IsRowValid(row)) { continue; }

                // Extract countries from the Tag
                if (row.Tag is (List<string> items, TagData))
                {
                    foreach (string item in items)
                    {
                        // Item format: "itemName,categoryName,country,company,quantity,pricePerUnit,totalPrice"
                        string[] itemDetails = item.Split(',');
                        if (itemDetails.Length > 2)
                        {
                            string itemCountry = itemDetails[2];
                            if (countryCounts.TryGetValue(itemCountry, out int itemValue))
                            {
                                countryCounts[itemCountry] = ++itemValue;
                            }
                            else
                            {
                                countryCounts[itemCountry] = 1;
                            }
                        }
                    }
                }
                else
                {
                    // Process the row normally if Tag is not set
                    string country = row.Cells[ReadOnlyVariables.Country_column].Value.ToString();
                    if (countryCounts.TryGetValue(country, out int value))
                    {
                        countryCounts[country] = ++value;
                    }
                    else
                    {
                        countryCounts[country] = 1;
                    }
                }
            }

            double totalCount = countryCounts.Values.Sum();
            string chartTitle = TranslatedChartTitles.CountriesOfDestination;
            string countries = LanguageManager.TranslateString("Countries");

            ProcessPieChartData(countryCounts, grouping, dataset, exportToExcel, filePath, chartTitle, countries, label, canUpdateChart, chart);

            return new ChartData(totalCount, SortAndGroupCountData(countryCounts, grouping).ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value));
        }
        public static ChartData LoadWorldMapChart(GeoMap geoMap, MainMenu_Form.GeoMapDataType dataType = MainMenu_Form.GeoMapDataType.Combined)
        {
            Guna2DataGridView[] dataGridViews = dataType switch
            {
                MainMenu_Form.GeoMapDataType.PurchasesOnly => [MainMenu_Form.Instance.Purchase_DataGridView],
                MainMenu_Form.GeoMapDataType.SalesOnly => [MainMenu_Form.Instance.Sale_DataGridView],
                MainMenu_Form.GeoMapDataType.Combined => [MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Instance.Purchase_DataGridView],
                _ => [MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Instance.Purchase_DataGridView]
            };

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridViews);

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, geoMap))
            {
                geoMap.Series = [];
                return ChartData.Empty;
            }

            ConfigureGeoMap(geoMap);

            // Collect country data based on selected type
            Dictionary<string, double> countryData = [];

            if (dataType == MainMenu_Form.GeoMapDataType.Combined || dataType == MainMenu_Form.GeoMapDataType.PurchasesOnly)
            {
                // Process purchase data (country of origin)
                foreach (DataGridViewRow row in MainMenu_Form.Instance.Purchase_DataGridView.Rows)
                {
                    if (!IsRowValid(row)) { continue; }

                    string country = GetCountryFromRow(row);
                    if (string.IsNullOrEmpty(country) || country == ReadOnlyVariables.EmptyCell) { continue; }

                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                    if (countryData.TryGetValue(country, out double existing))
                    {
                        countryData[country] = existing + total;
                    }
                    else
                    {
                        countryData[country] = total;
                    }
                }
            }

            if (dataType == MainMenu_Form.GeoMapDataType.Combined || dataType == MainMenu_Form.GeoMapDataType.SalesOnly)
            {
                // Process sales data (country of destination)
                foreach (DataGridViewRow row in MainMenu_Form.Instance.Sale_DataGridView.Rows)
                {
                    if (!IsRowValid(row)) { continue; }

                    string country = GetCountryFromRow(row);
                    if (string.IsNullOrEmpty(country) || country == ReadOnlyVariables.EmptyCell) { continue; }

                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                    if (countryData.TryGetValue(country, out double existing))
                    {
                        countryData[country] = existing + total;
                    }
                    else
                    {
                        countryData[country] = total;
                    }
                }
            }

            UpdateGeoMap(geoMap, countryData);

            double totalValue = countryData.Values.Sum();
            return new ChartData(totalValue, countryData);
        }
        public static ChartData LoadAccountantsIntoChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView[] dataGridViews = [
                MainMenu_Form.Instance.Sale_DataGridView,
                MainMenu_Form.Instance.Purchase_DataGridView
            ];

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridViews);
            string label = LanguageManager.TranslateString("# of transactions");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> accountantCounts = [];

            foreach (Guna2DataGridView purchasesDataGridView in dataGridViews)
            {
                if (!DataGridViewManager.HasVisibleRows(purchasesDataGridView))
                {
                    continue;
                }

                foreach (DataGridViewRow row in purchasesDataGridView.Rows)
                {
                    if (!IsRowValid(row)) { continue; }

                    string accountant = row.Cells[ReadOnlyVariables.Accountant_column].Value.ToString();

                    if (!string.IsNullOrEmpty(accountant))
                    {
                        if (accountantCounts.TryGetValue(accountant, out int value))
                        {
                            accountantCounts[accountant] = ++value;
                        }
                        else
                        {
                            accountantCounts[accountant] = 1;
                        }
                    }
                }
            }

            double totalCount = accountantCounts.Values.Sum();
            string chartTitle = TranslatedChartTitles.AccountantsTransactions;
            string accountants = LanguageManager.TranslateString("Accountants");

            ProcessPieChartData(accountantCounts, grouping, dataset, exportToExcel, filePath, chartTitle, accountants, label, canUpdateChart, chart);

            return new ChartData(totalCount, SortAndGroupCountData(accountantCounts, grouping).ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value));
        }
        public static SalesExpensesChartData LoadSalesVsExpensesChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView, purchasesDataGridView);
            string expensesLabel = LanguageManager.TranslateString("Total expenses");
            string salesLabel = LanguageManager.TranslateString("Total sales");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries expensesDataset = CreateStyledDataset(expensesLabel, isLineChart, GetColorForIndex(1));
            ISeries salesDataset = CreateStyledDataset(salesLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Process data
            Dictionary<string, double> expensesByDate = ProcessRowsForTimeData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            Dictionary<string, double> salesByDate = ProcessRowsForTimeData(
                salesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            if (expensesByDate.Count == 0 && salesByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            HashSet<string> allDates = [.. expensesByDate.Keys, .. salesByDate.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, expensesByDate, salesByDate, expensesLabel, salesLabel, filePath, isLineChart, TranslatedChartTitles.SalesVsExpenses);
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, expensesByDate, salesByDate, expensesDataset, salesDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, expensesDataset, salesDataset, sortedDates);
            }

            return new SalesExpensesChartData(expensesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageTransactionValueChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView, salesDataGridView);
            string purchaseLabel = LanguageManager.TranslateString("Average purchase value");
            string saleLabel = LanguageManager.TranslateString("Average sale value");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries purchasesDataset = CreateStyledDataset(purchaseLabel, isLineChart, GetColorForIndex(1));
            ISeries salesDataset = CreateStyledDataset(saleLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Process average data
            Dictionary<string, (double total, int count)> salesAggregateByDate = ProcessRowsForAverageData(
                salesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            Dictionary<string, (double total, int count)> purchasesAggregateByDate = ProcessRowsForAverageData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            if (salesAggregateByDate.Count == 0 && purchasesAggregateByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Calculate averages
            Dictionary<string, double> avgPurchasesByDate = purchasesAggregateByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );
            Dictionary<string, double> avgSalesByDate = salesAggregateByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );

            HashSet<string> allDates = [.. avgPurchasesByDate.Keys, .. avgSalesByDate.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, avgPurchasesByDate, avgSalesByDate, purchaseLabel, saleLabel, filePath, isLineChart, TranslatedChartTitles.AverageTransactionValue);
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, avgPurchasesByDate, avgSalesByDate, purchasesDataset, salesDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset, sortedDates);
            }

            return new SalesExpensesChartData(avgPurchasesByDate, avgSalesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadTotalTransactionsChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView, purchasesDataGridView);
            string purchasesLabel = LanguageManager.TranslateString("Purchases");
            string salesLabel = LanguageManager.TranslateString("Sales");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries purchasesDataset = CreateStyledDataset(purchasesLabel, isLineChart, GetColorForIndex(1));
            ISeries salesDataset = CreateStyledDataset(salesLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Count transactions
            Dictionary<string, int> purchaseCountsByDate = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                dateFormat,
                IsRowValid
            );

            Dictionary<string, int> salesCountsByDate = ProcessRowsForCountData(
                salesDataGridView.Rows,
                dateFormat,
                IsRowValid
            );

            // Convert to double dictionaries
            Dictionary<string, double> purchasesByDate = purchaseCountsByDate.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);
            Dictionary<string, double> salesByDate = salesCountsByDate.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);

            HashSet<string> allDates = [.. purchasesByDate.Keys, .. salesByDate.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, purchasesByDate, salesByDate, purchasesLabel, salesLabel, filePath, isLineChart, TranslatedChartTitles.TotalTransactions);
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, purchasesByDate, salesByDate, purchasesDataset, salesDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset, sortedDates, ChartFormatting.None);
            }

            return new SalesExpensesChartData(purchasesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageShippingCostsChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true, bool includeZeroShipping = false)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(salesDataGridView, purchasesDataGridView);
            string purchaseLabel = LanguageManager.TranslateString("Purchases");
            string saleLabel = LanguageManager.TranslateString("Sales");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries purchasesDataset = CreateStyledDataset(purchaseLabel, isLineChart, GetColorForIndex(1));
            ISeries salesDataset = CreateStyledDataset(saleLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Process shipping data
            Dictionary<string, (double total, int count)> purchaseShippingAggregateByDate = ProcessRowsForAverageData(
                purchasesDataGridView.Rows,
                dateFormat,
                row =>
                {
                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) { return null; }
                    if (shipping == 0 && !includeZeroShipping) { return null; }
                    return shipping;
                }
            );

            Dictionary<string, (double total, int count)> salesShippingAggregateByDate = ProcessRowsForAverageData(
                salesDataGridView.Rows,
                dateFormat,
                row =>
                {
                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) { return null; }
                    if (shipping == 0 && !includeZeroShipping) { return null; }
                    return shipping;
                }
            );

            if (purchaseShippingAggregateByDate.Count == 0 && salesShippingAggregateByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Calculate averages
            Dictionary<string, double> avgPurchaseShipping = purchaseShippingAggregateByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );
            Dictionary<string, double> avgSalesShipping = salesShippingAggregateByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );

            HashSet<string> allDates = [.. avgPurchaseShipping.Keys, .. avgSalesShipping.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, avgPurchaseShipping, avgSalesShipping, purchaseLabel, saleLabel, filePath, isLineChart, TranslatedChartTitles.AverageShippingCosts);
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, avgPurchaseShipping, avgSalesShipping, purchasesDataset, salesDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset, sortedDates);
            }

            return new SalesExpensesChartData(avgPurchaseShipping, avgSalesShipping, sortedDates);
        }
        public static SalesExpensesChartData LoadGrowthRateChart(CartesianChart chart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(salesDataGridView, purchasesDataGridView);
            string expensesLabel = LanguageManager.TranslateString("Expenses growth %");
            string revenueLabel = LanguageManager.TranslateString("Revenue growth %");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForLine(chart);
            }

            // Create line datasets
            LineSeries<double> expenseDataset = new()
            {
                Name = revenueLabel,
                Values = [],
                Stroke = new SolidColorPaint(GetColorForIndex(1)) { StrokeThickness = 5 },
                Fill = null,
                GeometryStroke = new SolidColorPaint(GetColorForIndex(1)) { StrokeThickness = 5 },
                GeometrySize = 15,
                LineSmoothness = 0.5
            };
            LineSeries<double> revenueDataset = new()
            {
                Name = revenueLabel,
                Values = [],
                Stroke = new SolidColorPaint(GetColorForIndex(0)) { StrokeThickness = 5 },
                Fill = null,
                GeometryStroke = new SolidColorPaint(GetColorForIndex(0)) { StrokeThickness = 5 },
                GeometrySize = 15,
                LineSmoothness = 0.5
            };

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Process base data
            Dictionary<string, double> expensesByDate = ProcessRowsForTimeData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            Dictionary<string, double> revenueByDate = ProcessRowsForTimeData(
                salesDataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            if (expensesByDate.Count == 0 && revenueByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            HashSet<string> allDates = [.. expensesByDate.Keys, .. revenueByDate.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            // Calculate growth rates
            Dictionary<string, double> expenseGrowth = [], revenueGrowth = [];
            double? previousExpense = null, previousRevenue = null;

            foreach (string date in sortedDates)
            {
                // Calculate revenue growth
                if (revenueByDate.TryGetValue(date, out double currentRevenue))
                {
                    if (previousRevenue.HasValue && previousRevenue.Value != 0)
                    {
                        double growthRate = ((currentRevenue - previousRevenue.Value) / previousRevenue.Value) * 100;
                        revenueGrowth[date] = Math.Round(growthRate, 2);
                    }
                    else
                    {
                        revenueGrowth[date] = 0;
                    }
                    previousRevenue = currentRevenue;
                }

                // Calculate expense growth
                if (expensesByDate.TryGetValue(date, out double currentExpense))
                {
                    if (previousExpense.HasValue && previousExpense.Value != 0)
                    {
                        double growthRate = ((currentExpense - previousExpense.Value) / previousExpense.Value) * 100;
                        expenseGrowth[date] = Math.Round(growthRate, 2);
                    }
                    else
                    {
                        expenseGrowth[date] = 0;
                    }
                    previousExpense = currentExpense;
                }
            }

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, expenseGrowth, revenueGrowth, expensesLabel, revenueLabel, filePath, true, TranslatedChartTitles.GrowthRates, true);
            }
            else if (canUpdateChart)
            {
                List<double> expenseValues = [];
                List<double> revenueValues = [];

                foreach (string date in sortedDates)
                {
                    double expenseValue = expenseGrowth.TryGetValue(date, out double eValue) ? eValue : 0;
                    double revenueValue = revenueGrowth.TryGetValue(date, out double rValue) ? rValue : 0;

                    expenseValues.Add(expenseValue);
                    revenueValues.Add(revenueValue);
                }

                expenseDataset.Values = expenseValues;
                revenueDataset.Values = revenueValues;

                chart.Series = [expenseDataset, revenueDataset];

                SKColor foreColor = ChartColors.ToSKColor(CustomColors.Text);

                // Properly set X-axis labels with dates
                chart.XAxes = [new Axis {
                    Labels = sortedDates.ToArray(),
                    TextSize = 20,
                    LabelsPaint = new SolidColorPaint(foreColor),
                }];

                // Set Y-axis with percentage formatting
                chart.YAxes = [new Axis {
                    TextSize = 20,
                    LabelsPaint = new SolidColorPaint(foreColor),
                    SeparatorsPaint = new SolidColorPaint(foreColor) { StrokeThickness = 1 },
                    Labeler = value => $"{value:F1}%"  // Format as percentage
                }];
            }

            return new SalesExpensesChartData(expenseGrowth, revenueGrowth, sortedDates);
        }
        public static SalesExpensesChartData LoadReturnsOverTimeChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string purchaseReturnsLabel = LanguageManager.TranslateString("Purchase returns");
            string saleReturnsLabel = LanguageManager.TranslateString("Sale returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries purchaseReturnsDataset = CreateStyledDataset(purchaseReturnsLabel, isLineChart, GetColorForIndex(1));
            ISeries saleReturnsDataset = CreateStyledDataset(saleReturnsLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Count returns
            Dictionary<string, int> purchaseReturnCountsByDate = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                dateFormat,
                ReturnManager.IsTransactionReturned
            );

            Dictionary<string, int> saleReturnCountsByDate = ProcessRowsForCountData(
                salesDataGridView.Rows,
                dateFormat,
                ReturnManager.IsTransactionReturned
            );

            if (purchaseReturnCountsByDate.Count == 0 && saleReturnCountsByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Convert to double dictionaries
            Dictionary<string, double> purchaseReturnsDouble = purchaseReturnCountsByDate.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);
            Dictionary<string, double> saleReturnsDouble = saleReturnCountsByDate.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);

            HashSet<string> allDates = [.. purchaseReturnsDouble.Keys, .. saleReturnsDouble.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
            {
                { purchaseReturnsLabel, purchaseReturnsDouble.TryGetValue(date, out double pValue) ? pValue : 0 },
                { saleReturnsLabel, saleReturnsDouble.TryGetValue(date, out double sValue) ? sValue : 0 }
            };
                }

                ExcelSheetManager.ExportMultiDataSetCountChartToExcel(
                    combinedData,
                    filePath,
                    isLineChart ? eChartType.Line : eChartType.ColumnClustered,
                    TranslatedChartTitles.ReturnsOverTime
                );
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, purchaseReturnsDouble, saleReturnsDouble, purchaseReturnsDataset, saleReturnsDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchaseReturnsDataset, saleReturnsDataset, sortedDates, ChartFormatting.None);
            }

            return new SalesExpensesChartData(purchaseReturnsDouble, saleReturnsDouble, sortedDates);
        }
        public static SalesExpensesChartData LoadReturnFinancialImpactChart(CartesianChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string purchaseReturnValueLabel = LanguageManager.TranslateString("Purchase return value");
            string saleReturnValueLabel = LanguageManager.TranslateString("Sale return value");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart) { ConfigureChartForLine(chart); }
                else { ConfigureChartForBar(chart); }
            }

            ISeries purchaseReturnValueDataset = CreateStyledDataset(purchaseReturnValueLabel, isLineChart, GetColorForIndex(1));
            ISeries saleReturnValueDataset = CreateStyledDataset(saleReturnValueLabel, isLineChart, GetColorForIndex(0));

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Create custom processing for return values
            Dictionary<string, double> purchaseReturnValueByDate = [];
            Dictionary<string, double> saleReturnValueByDate = [];

            // Process purchase returns
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (purchaseReturnValueByDate.TryGetValue(formattedDate, out double value))
                {
                    purchaseReturnValueByDate[formattedDate] = Math.Round(value + total, 2);
                }
                else
                {
                    purchaseReturnValueByDate[formattedDate] = Math.Round(total, 2);
                }
            }

            // Process sale returns
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (saleReturnValueByDate.TryGetValue(formattedDate, out double value))
                {
                    saleReturnValueByDate[formattedDate] = Math.Round(value + total, 2);
                }
                else
                {
                    saleReturnValueByDate[formattedDate] = Math.Round(total, 2);
                }
            }

            if (purchaseReturnValueByDate.Count == 0 && saleReturnValueByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            HashSet<string> allDates = [.. purchaseReturnValueByDate.Keys, .. saleReturnValueByDate.Keys];
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                ExportMultiDatasetToExcel(sortedDates, purchaseReturnValueByDate, saleReturnValueByDate, purchaseReturnValueLabel, saleReturnValueLabel, filePath, isLineChart, TranslatedChartTitles.ReturnFinancialImpact);
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, purchaseReturnValueByDate, saleReturnValueByDate, purchaseReturnValueDataset, saleReturnValueDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchaseReturnValueDataset, saleReturnValueDataset, sortedDates);
            }

            return new SalesExpensesChartData(purchaseReturnValueByDate, saleReturnValueByDate, sortedDates);
        }
        public static ChartCountData LoadReturnReasonsChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> reasonCounts = [];

            // Process both purchase and sale returns
            Guna2DataGridView[] dataGridViews = [purchasesDataGridView, salesDataGridView];

            foreach (Guna2DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                    // Get return reason from TagData
                    (_, string returnReason, _, _) = ReturnManager.GetReturnInfo(row);

                    if (string.IsNullOrEmpty(returnReason))
                    {
                        returnReason = "No reason provided";
                    }

                    // Extract base reason if it contains additional notes (format: "reason - notes")
                    string baseReason = returnReason.Split(" - ")[0];

                    if (reasonCounts.TryGetValue(baseReason, out int value))
                    {
                        reasonCounts[baseReason] = value + 1;
                    }
                    else
                    {
                        reasonCounts[baseReason] = 1;
                    }
                }
            }

            string chartTitle = TranslatedChartTitles.ReturnReasons;
            string reasons = LanguageManager.TranslateString("Reasons");

            ProcessPieChartData(reasonCounts, grouping, dataset, exportToExcel, filePath, chartTitle, reasons, label, canUpdateChart, chart);

            return new ChartCountData(SortAndGroupCountData(reasonCounts, grouping));
        }
        public static ChartCountData LoadReturnsByCategoryChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> categoryCounts = [];

            // Process both purchase and sale returns
            Guna2DataGridView[] dataGridViews = [purchasesDataGridView, salesDataGridView];

            foreach (Guna2DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                    string category = row.Cells[ReadOnlyVariables.Category_column].Value.ToString();

                    if (category == ReadOnlyVariables.EmptyCell)
                    {
                        // Extract categories from the Tag for multi-item transactions
                        if (row.Tag is (List<string> items, TagData))
                        {
                            foreach (string item in items)
                            {
                                string[] itemDetails = item.Split(',');
                                if (itemDetails.Length > 1)
                                {
                                    string itemCategory = itemDetails[1];
                                    if (categoryCounts.TryGetValue(itemCategory, out int itemValue))
                                    {
                                        categoryCounts[itemCategory] = itemValue + 1;
                                    }
                                    else
                                    {
                                        categoryCounts[itemCategory] = 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (categoryCounts.TryGetValue(category, out int value))
                        {
                            categoryCounts[category] = value + 1;
                        }
                        else
                        {
                            categoryCounts[category] = 1;
                        }
                    }
                }
            }

            string chartTitle = TranslatedChartTitles.ReturnsByCategory;
            string categories = LanguageManager.TranslateString("Categories");

            ProcessPieChartData(categoryCounts, grouping, dataset, exportToExcel, filePath, chartTitle, categories, label, canUpdateChart, chart);

            return new ChartCountData(SortAndGroupCountData(categoryCounts, grouping));
        }
        public static ChartCountData LoadReturnsByProductChart(PieChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> productCounts = [];

            // Process both purchase and sale returns
            Guna2DataGridView[] dataGridViews = [purchasesDataGridView, salesDataGridView];

            foreach (Guna2DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                    string product = row.Cells[ReadOnlyVariables.Product_column].Value.ToString();

                    if (product == ReadOnlyVariables.EmptyCell)
                    {
                        // Extract products from the Tag for multi-item transactions
                        if (row.Tag is (List<string> items, TagData))
                        {
                            foreach (string item in items)
                            {
                                string[] itemDetails = item.Split(',');
                                if (itemDetails.Length > 0)
                                {
                                    string itemProduct = itemDetails[0];
                                    if (productCounts.TryGetValue(itemProduct, out int itemValue))
                                    {
                                        productCounts[itemProduct] = itemValue + 1;
                                    }
                                    else
                                    {
                                        productCounts[itemProduct] = 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (productCounts.TryGetValue(product, out int value))
                        {
                            productCounts[product] = value + 1;
                        }
                        else
                        {
                            productCounts[product] = 1;
                        }
                    }
                }
            }

            string chartTitle = TranslatedChartTitles.ReturnsByProduct;
            string products = LanguageManager.TranslateString("Products");

            ProcessPieChartData(productCounts, grouping, dataset, exportToExcel, filePath, chartTitle, products, label, canUpdateChart, chart);

            return new ChartCountData(SortAndGroupCountData(productCounts, grouping));
        }
        public static ChartCountData LoadPurchaseVsSaleReturnsChart(PieChart chart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearPieChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            List<PieSeries<double>> dataset = [];
            Dictionary<string, int> returnCounts = [];

            // Count returns
            int purchaseReturns = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                "dummy",  // Not used for counting
                ReturnManager.IsTransactionReturned
            ).Values.Sum();

            int saleReturns = ProcessRowsForCountData(
                salesDataGridView.Rows,
                "dummy",  // Not used for counting
                ReturnManager.IsTransactionReturned
            ).Values.Sum();

            // Add to dictionary
            if (purchaseReturns > 0)
            {
                returnCounts.Add(LanguageManager.TranslateString("Purchase Returns"), purchaseReturns);
            }
            if (saleReturns > 0)
            {
                returnCounts.Add(LanguageManager.TranslateString("Sale Returns"), saleReturns);
            }

            string chartTitle = TranslatedChartTitles.PurchaseVsSaleReturns;
            string type = LanguageManager.TranslateString("Transaction Type");

            ProcessPieChartData(returnCounts, PieChartGrouping.Unlimited, dataset, exportToExcel, filePath, chartTitle, type, label, canUpdateChart, chart);

            return new ChartCountData(returnCounts);
        }

        // Methods
        public static bool IsRowValid(DataGridViewRow row)
        {
            return row.Visible && !ReturnManager.IsTransactionReturned(row);
        }

        /// <summary>
        /// Sorts dictionary items by value in descending order and optionally groups smaller items into an "Other" category.
        /// </summary>
        /// <returns>A new dictionary sorted by value in descending order, with optional "Other" category.</returns>
        private static Dictionary<string, double> SortAndGroupData(Dictionary<string, double> data, PieChartGrouping maxItems)
        {
            List<KeyValuePair<string, double>> sortedData = data.OrderByDescending(x => x.Value).ToList();

            if (maxItems == PieChartGrouping.Unlimited || sortedData.Count <= (int)maxItems)
            {
                return sortedData.ToDictionary(x => x.Key, x => x.Value);
            }

            Dictionary<string, double> result = sortedData.Take((int)maxItems).ToDictionary(x => x.Key, x => x.Value);

            double otherSum = sortedData.Skip((int)maxItems).Sum(x => x.Value);
            if (otherSum > 0)
            {
                result.Add("Other", otherSum);
            }

            return result;
        }
        private static Dictionary<string, int> SortAndGroupCountData(Dictionary<string, int> data, PieChartGrouping maxItems)
        {
            List<KeyValuePair<string, int>> sortedData = data.OrderByDescending(x => x.Value).ToList();

            if (maxItems == PieChartGrouping.Unlimited || sortedData.Count <= (int)maxItems)
            {
                return sortedData.ToDictionary(x => x.Key, x => x.Value);
            }

            Dictionary<string, int> result = sortedData.Take((int)maxItems).ToDictionary(x => x.Key, x => x.Value);

            int otherSum = sortedData.Skip((int)maxItems).Sum(x => x.Value);
            if (otherSum > 0)
            {
                result.Add("Other", otherSum);
            }

            return result;
        }

        private static readonly Color[] _chartColors =
        [
            CustomColors.PastelBlue,
            CustomColors.PastelGreen,
            Color.FromArgb(102, 204, 153), // Muted teal
            Color.FromArgb(204, 102, 153), // Soft pink
            Color.FromArgb(153, 102, 204), // Soft purple
            Color.FromArgb(204, 153, 102), // Soft orange
            Color.FromArgb(102, 178, 178), // Sea green
            Color.FromArgb(204, 102, 102), // Soft red
            Color.FromArgb(153, 153, 204), // Muted lavender
            Color.FromArgb(153, 204, 153), // Soft sage
            Color.FromArgb(204, 204, 153), // Muted gold
            Color.FromArgb(178, 102, 178), // Soft magenta
            Color.FromArgb(102, 127, 204), // Ocean blue
            Color.FromArgb(204, 153, 204), // Soft lilac
            Color.FromArgb(153, 153, 153), // Muted gray
            Color.FromArgb(178, 178, 102), // Olive gold
        ];
        private static SKColor GetColorForIndex(int index)
        {
            return ChartColors.ToSKColor(_chartColors[index % _chartColors.Length]);
        }
        private static SKColor GetDefaultColor()
        {
            return ChartColors.ToSKColor(CustomColors.PastelBlue);
        }
        private static void SortAndAddDatasetAndSetLabels(Dictionary<string, double> list, string dateFormat, ISeries dataset, bool isLineChart, CartesianChart chart)
        {
            // Sort the dictionary by date keys
            IOrderedEnumerable<KeyValuePair<string, double>> sortedProfitByDate = list.OrderBy(kvp => DateTime.ParseExact(kvp.Key, dateFormat, null));

            List<double> values = [];
            List<string> labels = [];

            foreach (KeyValuePair<string, double> kvp in sortedProfitByDate)
            {
                values.Add(kvp.Value);
                labels.Add(kvp.Key);
            }

            if (isLineChart)
            {
                ((LineSeries<double>)dataset).Values = values;
            }
            else
            {
                ((ColumnSeries<double>)dataset).Values = values;
            }

            SKColor textColor = ChartColors.ToSKColor(CustomColors.Text);
            chart.XAxes = [new Axis
            {
                Labels = labels.ToArray(),
                TextSize = 20,
                LabelsPaint = new SolidColorPaint(textColor)
            }];
        }
        private static bool TryGetValue<T>(DataGridViewCell cell, out T value)
        {
            value = default;

            if (cell.Value == null)
            {
                Log.Error_DataGridViewCellIsEmpty(MainMenu_Form.Instance.SelectedDataGridView.Name);
                return false;
            }

            if (cell.Value.ToString() == ReadOnlyVariables.EmptyCell)
            {
                return false;
            }

            value = (T)Convert.ChangeType(cell.Value, typeof(T));
            return true;
        }
        private static string GetDateFormat(TimeSpan dateRange)
        {
            if (dateRange.TotalDays > 365)  // More than a year
            {
                return "yyyy";
            }
            else if (dateRange.TotalDays > 30)  // More than a month
            {
                return "yyyy-MM";
            }
            else // Default to days
            {
                return "yyyy-MM-dd";
            }
        }
        private static (DateTime minDate, DateTime maxDate) GetMinMaxDate(params DataGridViewRowCollection[] rowCollections)
        {
            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;

            foreach (DataGridViewRowCollection rows in rowCollections)
            {
                foreach (DataGridViewRow row in rows)
                {
                    if (!IsRowValid(row)) { continue; }

                    if (row.Cells[ReadOnlyVariables.Date_column].Value != null)
                    {
                        DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                        if (date < minDate) { minDate = date; }
                        if (date > maxDate) { maxDate = date; }
                    }
                }
            }
            return (minDate, maxDate);
        }
        private static void UpdateChart(CartesianChart chart, ISeries dataset, bool formatCurrency)
        {
            chart.Series = [dataset];

            if (formatCurrency)
            {
                ApplyCurrencyFormatToChart(chart);
            }
        }
        private static void UpdatePieChart(PieChart chart, List<PieSeries<double>> dataset)
        {
            chart.Series = dataset.Cast<ISeries>().ToArray();
        }
        public static void ClearChart(CartesianChart chart)
        {
            chart.Series = [];
            chart.XAxes = [new Axis { }];
            chart.YAxes = [new Axis { }];
            chart.ZoomMode = ZoomAndPanMode.None;
        }
        public static void ClearPieChart(PieChart chart)
        {
            chart.Series = [];

            if (chart.Legend is CustomLegend customLegend)
            {
                customLegend.ClearLayout();
            }
        }

        // GeoMap helper methods
        private static string GetCountryFromRow(DataGridViewRow row)
        {
            string country = row.Cells[ReadOnlyVariables.Country_column].Value.ToString() ?? "";

            if (country == ReadOnlyVariables.EmptyCell)
            {
                // Extract countries from the Tag for multi-item transactions
                if (row.Tag is (List<string> items, TagData))
                {
                    foreach (string item in items)
                    {
                        string[] itemDetails = item.Split(',');
                        if (itemDetails.Length > 2)
                        {
                            return itemDetails[2];  // Return first country found
                        }
                    }
                }
            }

            return country;
        }
        private static void ConfigureGeoMap(GeoMap geoMap)
        {
            ChartColors.ApplyTheme(geoMap);

            // Configure map properties
            geoMap.MapProjection = MapProjection.Default;
        }
        private static void UpdateGeoMap(GeoMap geoMap, Dictionary<string, double> countryData)
        {
            if (countryData.Count == 0)
            {
                geoMap.Series = [];
                return;
            }

            // Convert country names to proper codes and create weighted map lands
            HeatLand[] mapLands = countryData
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && kvp.Key != ReadOnlyVariables.EmptyCell)
                .Select(kvp => new HeatLand
                {
                    Name = Country.MapCountryNameToCode(kvp.Key),
                    Value = (float)kvp.Value
                })
                .ToArray();

            if (mapLands.Length == 0)
            {
                geoMap.Series = [];
                return;
            }

            // Create heat land series
            HeatLandSeries heatSeries = new()
            {
                Name = "Transaction Values",
                Lands = mapLands,
                HeatMap =
                [
                    new LiveChartsCore.Drawing.LvcColor(173, 216, 230),  // Light blue
                    new LiveChartsCore.Drawing.LvcColor(100, 149, 237),  // Cornflower blue  
                    new LiveChartsCore.Drawing.LvcColor(0, 0, 139)       // Dark blue
                ]
            };

            geoMap.Series = [heatSeries];
        }
    }
}