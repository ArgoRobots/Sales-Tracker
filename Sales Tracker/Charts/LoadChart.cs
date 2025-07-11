using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using OfficeOpenXml.Drawing.Chart;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
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
    /// Provides methods to configure, clear, and load data into GunaCharts.
    /// </summary>
    internal class LoadChart
    {
        // Configuration
        public static void ConfigureChartForBar(GunaChart chart)
        {
            ClearChart(chart);

            chart.XAxes.Display = true;
            chart.XAxes.Ticks.Display = true;

            chart.YAxes.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.YAxes.Ticks.Display = true;
            chart.Legend.Position = LegendPosition.Top;
        }
        public static void ConfigureChartForLine(GunaChart chart)
        {
            ClearChart(chart);

            chart.XAxes.Display = true;
            chart.XAxes.GridLines.Display = true;
            chart.XAxes.Ticks.Display = true;

            chart.YAxes.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.YAxes.Ticks.Display = true;
            chart.Legend.Position = LegendPosition.Top;
        }
        public static void ConfigureChartForPie(GunaChart chart)
        {
            ClearChart(chart);
            chart.Legend.Position = GetLegendPosition(chart);
        }
        public static LegendPosition GetLegendPosition(GunaChart chart)
        {
            // Use a ratio to allow Bottom legend even if height is slightly less than width
            double ratio = (double)chart.Height / chart.Width;
            return ratio >= 0.8 || chart.Width < 550 ? LegendPosition.Bottom : LegendPosition.Right;
        }

        // Helper methods
        private static IGunaDataset CreateStyledDataset(string label, bool isLineChart, Color color, bool exportToExcel, bool canUpdateChart)
        {
            IGunaDataset dataset = isLineChart
                ? new GunaLineDataset { Label = label }
                : new GunaBarDataset { Label = label };

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(dataset, isLineChart, color);
            }

            return dataset;
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
                    dataByDate[formattedDate] = existing + value.Value;
                }
                else
                {
                    dataByDate[formattedDate] = value.Value;
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
            GunaPieDataset dataset,
            bool exportToExcel,
            string filePath,
            string chartTitle,
            string categoryLabel,
            string valueLabel,
            bool canUpdateChart,
            GunaChart chart) where T : struct, IConvertible
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
                    double percentage = item.Value / totalCount * 100;
                    dataset.DataPoints.Add(item.Key, item.Value);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{item.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = _chartColors;
                    UpdateChart(chart, dataset, false);
                }
            }
        }

        private static void UpdateChartWithTwoDatasets(GunaChart chart, IGunaDataset dataset1, IGunaDataset dataset2, bool formatAsCurrency = true)
        {
            ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
            {
                chart.Datasets.Clear();
                chart.Datasets.Add(dataset1);
                chart.Datasets.Add(dataset2);

                if (formatAsCurrency)
                {
                    ApplyCurrencyFormatToDataset(dataset1);
                    ApplyCurrencyFormatToDataset(dataset2);
                }

                chart.Update();
            });
        }

        private static void AddDataPointsToDatasets(
            List<string> sortedDates,
            Dictionary<string, double> data1,
            Dictionary<string, double> data2,
            IGunaDataset dataset1,
            IGunaDataset dataset2,
            bool isLineChart)
        {
            foreach (string date in sortedDates)
            {
                double value1 = data1.TryGetValue(date, out double v1) ? v1 : 0;
                double value2 = data2.TryGetValue(date, out double v2) ? v2 : 0;

                if (isLineChart)
                {
                    ((GunaLineDataset)dataset1).DataPoints.Add(date, value1);
                    ((GunaLineDataset)dataset2).DataPoints.Add(date, value2);
                }
                else
                {
                    ((GunaBarDataset)dataset1).DataPoints.Add(date, value1);
                    ((GunaBarDataset)dataset2).DataPoints.Add(date, value2);

                    float barPercentage = dataset1.DataPointCount + dataset2.DataPointCount == 1 ? 0.2f : 0.4f;
                    ((GunaBarDataset)dataset1).BarPercentage = barPercentage;
                    ((GunaBarDataset)dataset2).BarPercentage = barPercentage;
                }
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
        public static ChartData LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            int visibleRows = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, visibleRows);

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

            chart.Legend.Display = false;

            IGunaDataset dataset = CreateStyledDataset(label, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated method for processing rows
            Dictionary<string, double> revenueByDate = ProcessRowsForTimeData(
                dataGridView.Rows,
                dateFormat,
                row => TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total) ? total : null
            );

            double grandTotal = revenueByDate.Values.Sum();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                eChartType chartType = isLineChart ? eChartType.Line : eChartType.ColumnClustered;
                string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                    ? TranslatedChartTitles.TotalRevenue
                    : TranslatedChartTitles.TotalExpenses;
                string date = LanguageManager.TranslateString("Date");

                ExcelSheetManager.ExportChartToExcel(revenueByDate, filePath, chartType, chartTitle, date, label);
            }
            else if (canUpdateChart)
            {
                SortAndAddDatasetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, revenueByDate);
        }
        public static ChartData LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            int visibleRows = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, visibleRows);

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
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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

            // Use consolidated pie chart processing
            string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? TranslatedChartTitles.RevenueDistribution
                : TranslatedChartTitles.ExpensesDistribution;
            string category1 = LanguageManager.TranslateString("Category");

            ProcessPieChartData(allData, grouping, dataset, exportToExcel, filePath, chartTitle, category1, label, canUpdateChart, chart);

            return new ChartData(totalCost, SortAndGroupData(allData, grouping));
        }
        public static ChartData LoadProfitsIntoChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            chart.Legend.Display = false;

            IGunaDataset dataset = CreateStyledDataset(label, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

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
                SortAndAddDatasetAndSetBarPercentage(profitByDate, dateFormat, dataset, isLineChart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, profitByDate);
        }

        // Statistics charts
        public static ChartData LoadCountriesOfOriginForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static ChartData LoadCompaniesOfOriginForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static ChartData LoadCountriesOfDestinationForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            string label = LanguageManager.TranslateString("# of items");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static ChartData LoadAccountantsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView[] dataGridViews = [
                MainMenu_Form.Instance.Sale_DataGridView,
                MainMenu_Form.Instance.Purchase_DataGridView
            ];

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridViews);
            string label = LanguageManager.TranslateString("# of transactions");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static SalesExpensesChartData LoadSalesVsExpensesChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset expensesDataset = CreateStyledDataset(expensesLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset salesDataset = CreateStyledDataset(salesLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for processing data
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
                UpdateChartWithTwoDatasets(chart, expensesDataset, salesDataset);
            }

            return new SalesExpensesChartData(expensesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageTransactionValueChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset purchasesDataset = CreateStyledDataset(purchaseLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset salesDataset = CreateStyledDataset(saleLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for processing average data
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
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset);
            }

            return new SalesExpensesChartData(avgPurchasesByDate, avgSalesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadTotalTransactionsChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset purchasesDataset = CreateStyledDataset(purchasesLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset salesDataset = CreateStyledDataset(salesLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for counting transactions
            Dictionary<string, int> purchaseCountsByDate = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => IsRowValid(row)
            );

            Dictionary<string, int> salesCountsByDate = ProcessRowsForCountData(
                salesDataGridView.Rows,
                dateFormat,
                row => IsRowValid(row)
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
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset, false); // No currency formatting for count data
            }

            return new SalesExpensesChartData(purchasesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageShippingCostsChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true, bool includeZeroShipping = false)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset purchasesDataset = CreateStyledDataset(purchaseLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset salesDataset = CreateStyledDataset(saleLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for processing shipping data
            Dictionary<string, (double total, int count)> purchaseShippingAggregateByDate = ProcessRowsForAverageData(
                purchasesDataGridView.Rows,
                dateFormat,
                row =>
                {
                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) return null;
                    if (shipping == 0 && !includeZeroShipping) return null;
                    return shipping;
                }
            );

            Dictionary<string, (double total, int count)> salesShippingAggregateByDate = ProcessRowsForAverageData(
                salesDataGridView.Rows,
                dateFormat,
                row =>
                {
                    if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) return null;
                    if (shipping == 0 && !includeZeroShipping) return null;
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
                UpdateChartWithTwoDatasets(chart, purchasesDataset, salesDataset);
            }

            return new SalesExpensesChartData(avgPurchaseShipping, avgSalesShipping, sortedDates);
        }
        public static SalesExpensesChartData LoadGrowthRateChart(GunaChart chart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            // Create spline datasets for revenue and expense growth
            GunaSplineDataset expenseDataset = new() { Label = expensesLabel };
            GunaSplineDataset revenueDataset = new() { Label = revenueLabel };

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToSplineDataset(revenueDataset, CustomColors.PastelBlue);
                ApplyStyleToSplineDataset(expenseDataset, CustomColors.PastelGreen);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for processing base data
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
                foreach (string date in sortedDates)
                {
                    double expenseValue = expenseGrowth.TryGetValue(date, out double eValue) ? eValue : 0;
                    double revenueValue = revenueGrowth.TryGetValue(date, out double rValue) ? rValue : 0;

                    expenseDataset.DataPoints.Add(date, expenseValue);
                    revenueDataset.DataPoints.Add(date, revenueValue);
                }

                ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
                {
                    chart.Datasets.Clear();
                    chart.Datasets.Add(expenseDataset);
                    chart.Datasets.Add(revenueDataset);

                    // Set Y-axis format to show percentage
                    revenueDataset.YFormat = "{0:0.0}%";
                    expenseDataset.YFormat = "{0:0.0}%";

                    chart.Update();
                });
            }

            return new SalesExpensesChartData(expenseGrowth, revenueGrowth, sortedDates);
        }
        public static SalesExpensesChartData LoadReturnsOverTimeChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset purchaseReturnsDataset = CreateStyledDataset(purchaseReturnsLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset saleReturnsDataset = CreateStyledDataset(saleReturnsLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Use consolidated methods for counting returns
            Dictionary<string, int> purchaseReturnCountsByDate = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                dateFormat,
                row => ReturnManager.IsTransactionReturned(row)
            );

            Dictionary<string, int> saleReturnCountsByDate = ProcessRowsForCountData(
                salesDataGridView.Rows,
                dateFormat,
                row => ReturnManager.IsTransactionReturned(row)
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

                string chartTitle = TranslatedChartTitles.ReturnsOverTime;
                ExcelSheetManager.ExportMultiDataSetCountChartToExcel(
                    combinedData,
                    filePath,
                    isLineChart ? eChartType.Line : eChartType.ColumnClustered,
                    chartTitle
                );
            }
            else if (canUpdateChart)
            {
                AddDataPointsToDatasets(sortedDates, purchaseReturnsDouble, saleReturnsDouble, purchaseReturnsDataset, saleReturnsDataset, isLineChart);
                UpdateChartWithTwoDatasets(chart, purchaseReturnsDataset, saleReturnsDataset, false); // No currency formatting for count data
            }

            return new SalesExpensesChartData(purchaseReturnsDouble, saleReturnsDouble, sortedDates);
        }
        public static SalesExpensesChartData LoadReturnFinancialImpactChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

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

            IGunaDataset purchaseReturnValueDataset = CreateStyledDataset(purchaseReturnValueLabel, isLineChart, CustomColors.PastelGreen, exportToExcel, canUpdateChart);
            IGunaDataset saleReturnValueDataset = CreateStyledDataset(saleReturnValueLabel, isLineChart, CustomColors.PastelBlue, exportToExcel, canUpdateChart);

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
                UpdateChartWithTwoDatasets(chart, purchaseReturnValueDataset, saleReturnValueDataset);
            }

            return new SalesExpensesChartData(purchaseReturnValueByDate, saleReturnValueByDate, sortedDates);
        }
        public static ChartCountData LoadReturnReasonsChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
            Dictionary<string, int> reasonCounts = [];

            // Process both purchase and sale returns
            Guna2DataGridView[] dataGridViews = [purchasesDataGridView, salesDataGridView];

            foreach (Guna2DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.Visible || !ReturnManager.IsTransactionReturned(row)) { continue; }

                    // Get return reason from TagData
                    (DateTime? returnDate, string returnReason, string returnedBy) = ReturnManager.GetReturnInfo(row);

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
        public static ChartCountData LoadReturnsByCategoryChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static ChartCountData LoadReturnsByProductChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
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
        public static ChartCountData LoadPurchaseVsSaleReturnsChart(GunaChart chart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name);

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRowsForReturn(purchasesDataGridView, salesDataGridView);
            string label = LanguageManager.TranslateString("# of returns");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartCountData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new() { Label = label };
            Dictionary<string, int> returnCounts = [];

            // Count returns using the consolidated method
            int purchaseReturns = ProcessRowsForCountData(
                purchasesDataGridView.Rows,
                "dummy", // Not used for counting
                row => ReturnManager.IsTransactionReturned(row)
            ).Values.Sum();

            int saleReturns = ProcessRowsForCountData(
                salesDataGridView.Rows,
                "dummy", // Not used for counting
                row => ReturnManager.IsTransactionReturned(row)
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
        private static void ApplyStyleToBarOrLineDataSet(IGunaDataset dataset, bool isLineChart, Color color)
        {
            if (isLineChart)
            {
                ((GunaLineDataset)dataset).FillColor = color;
                ((GunaLineDataset)dataset).BorderColor = color;
                ((GunaLineDataset)dataset).PointRadius = 8;
                ((GunaLineDataset)dataset).PointStyle = PointStyle.Circle;
                ((GunaLineDataset)dataset).PointFillColors = [color];
                ((GunaLineDataset)dataset).PointBorderColors = [color];
                ((GunaLineDataset)dataset).BorderWidth = 5;
            }
            else
            {
                ((GunaBarDataset)dataset).FillColors = [color];
            }
        }
        private static void ApplyStyleToSplineDataset(GunaSplineDataset dataset, Color color)
        {
            dataset.FillColor = color;
            dataset.BorderColor = color;
            dataset.PointRadius = 8;
            dataset.PointStyle = PointStyle.Circle;
            dataset.PointFillColors = [color];
            dataset.PointBorderColors = [color];
            dataset.BorderWidth = 5;
        }
        private static readonly ColorCollection _chartColors =
        [
            CustomColors.PastelBlue,
            CustomColors.PastelGreen,
            Color.FromArgb(102, 204, 153),  // Muted teal
            Color.FromArgb(204, 102, 153),  // Soft pink
            Color.FromArgb(153, 102, 204),  // Soft purple
            Color.FromArgb(204, 153, 102),  // Soft orange
            Color.FromArgb(102, 178, 178),  // Sea green
            Color.FromArgb(204, 102, 102),  // Soft red
            Color.FromArgb(153, 153, 204),  // Muted lavender
            Color.FromArgb(153, 204, 153),  // Soft sage
            Color.FromArgb(204, 204, 153),  // Muted gold
            Color.FromArgb(178, 102, 178),  // Soft magenta
            Color.FromArgb(102, 127, 204),  // Ocean blue
            Color.FromArgb(204, 153, 204),  // Soft lilac
            Color.FromArgb(153, 153, 153),  // Muted gray
            Color.FromArgb(178, 178, 102),  // Olive gold
        ];
        private static void SortAndAddDatasetAndSetBarPercentage(Dictionary<string, double> list, string dateFormat, IGunaDataset dataset, bool isLineChart)
        {
            // Sort the dictionary by date keys
            IOrderedEnumerable<KeyValuePair<string, double>> sortedProfitByDate = list.OrderBy(kvp => DateTime.ParseExact(kvp.Key, dateFormat, null));

            // Add dataset
            if (isLineChart)
            {
                foreach (KeyValuePair<string, double> kvp in sortedProfitByDate)
                {
                    ((GunaLineDataset)dataset).DataPoints.Add(kvp.Key, kvp.Value);
                }
            }
            else
            {
                foreach (KeyValuePair<string, double> kvp in sortedProfitByDate)
                {
                    ((GunaBarDataset)dataset).DataPoints.Add(kvp.Key, kvp.Value);
                }
            }

            // Set BarPercentage for bar charts
            if (!isLineChart)
            {
                if (dataset.DataPointCount == 1)
                {
                    ((GunaBarDataset)dataset).BarPercentage = 0.2f;
                }
                else
                {
                    ((GunaBarDataset)dataset).BarPercentage = 0.4f;
                }
            }
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
        private static void UpdateChart(GunaChart chart, IGunaDataset dataset, bool formatCurrency)
        {
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);

            if (formatCurrency)
            {
                ApplyCurrencyFormatToDataset(dataset);
            }

            ChartUpdateManager.UpdateChartWithRendering(chart);
        }
        public static void ApplyCurrencyFormatToDataset(object dataset)
        {
            string currencySymbol = MainMenu_Form.CurrencySymbol;

            switch (dataset)
            {
                case GunaBarDataset barDataset:
                    barDataset.YFormat = $"{currencySymbol}{{0:N2}}";
                    break;

                case GunaLineDataset lineDataset:
                    lineDataset.YFormat = $"{currencySymbol}{{0:N2}}";
                    break;

                case GunaPieDataset pieDataset:
                    pieDataset.YFormat = $"{currencySymbol}{{0:N2}}";
                    break;
            }
        }
        public static void ClearChart(GunaChart chart)
        {
            // Suspend updates during clearing
            chart.SuspendLayout();

            try
            {
                chart.Zoom = ZoomMode.Y;
                chart.XAxes.Display = false;
                chart.XAxes.GridLines.Display = false;
                chart.XAxes.Ticks.Display = false;

                chart.YAxes.Display = false;
                chart.YAxes.GridLines.Display = false;
                chart.YAxes.Ticks.Display = false;

                chart.Datasets.Clear();
            }
            finally
            {
                chart.ResumeLayout(false);  // Don't perform layout yet
                chart.Update();  // Single update call
            }
        }
    }
}