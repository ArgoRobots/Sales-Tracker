using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using OfficeOpenXml.Drawing.Chart;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
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
            return ratio >= 0.8 ? LegendPosition.Bottom : LegendPosition.Right;
        }

        // Main charts
        public static ChartData LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            int visibleRows = dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Totals Chart", visibleRows);

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
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            chart.Legend.Display = false;

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset { Label = label }; }
            else { dataset = new GunaBarDataset { Label = label }; }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.PastelBlue);
            }

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);
            Dictionary<string, double> revenueByDate = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                int quantity = Convert.ToInt32(row.Cells[ReadOnlyVariables.TotalItems_column].Value);
                double shipping = Convert.ToDouble(row.Cells[ReadOnlyVariables.Shipping_column].Value);
                double tax = Convert.ToDouble(row.Cells[ReadOnlyVariables.Tax_column].Value);
                grandTotal += total;
                string formattedDate = date.ToString(dateFormat);

                if (revenueByDate.TryGetValue(formattedDate, out double value))
                {
                    revenueByDate[formattedDate] = Math.Round(value + total, 2);
                }
                else
                {
                    revenueByDate[formattedDate] = Math.Round(total, 2);
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

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
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Distribution Chart", visibleRows);

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
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double cost))
                {
                    continue;
                }

                double shipping = Convert.ToDouble(row.Cells[ReadOnlyVariables.Shipping_column].Value);
                double tax = Convert.ToDouble(row.Cells[ReadOnlyVariables.Tax_column].Value);
                double fee = Convert.ToDouble(row.Cells[ReadOnlyVariables.Fee_column].Value);
                int quantity = Convert.ToInt32(row.Cells[ReadOnlyVariables.TotalItems_column].Value);
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

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
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

            // Sort and group all data together
            Dictionary<string, double> sortedData = SortAndGroupData(allData, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                    ? TranslatedChartTitles.RevenueDistribution
                    : TranslatedChartTitles.ExpensesDistribution;
                string category = LanguageManager.TranslateString("Category");

                ExcelSheetManager.ExportChartToExcel(sortedData, filePath, eChartType.Pie, chartTitle, category, label);
            }
            else
            {
                // Add all data points with percentage labels
                foreach (KeyValuePair<string, double> item in sortedData)
                {
                    double roundedValue = Math.Round(item.Value, 2);
                    double percentage = roundedValue / totalCost * 100;
                    dataset.DataPoints.Add(item.Key, roundedValue);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{item.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = GetChartColors();
                    UpdateChart(chart, dataset, true);
                }
            }

            return new ChartData(totalCost, sortedData);
        }
        public static ChartData LoadProfitsIntoChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Profits Chart");

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
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            chart.Legend.Display = false;

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset() { Label = label }; }
            else { dataset = new GunaBarDataset() { Label = label }; }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.PastelBlue);
            }

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            // Group and sum the total profit based on the determined date format
            Dictionary<string, double> profitByDate = [];
            bool anyRowsVisible = false;

            // Calculate total revenue from sales
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (profitByDate.ContainsKey(formattedDate))
                {
                    profitByDate[formattedDate] += total;
                }
                else
                {
                    profitByDate[formattedDate] = total;
                }
            }

            // Subtract total cost from purchases
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);

                if (profitByDate.TryGetValue(formattedDate, out double value))
                {
                    profitByDate[formattedDate] = Math.Round(value - total, 2);
                }
                else
                {
                    profitByDate[formattedDate] = Math.Round(-total, 2);
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            // Get grandTotal
            foreach (KeyValuePair<string, double> item in profitByDate)
            {
                grandTotal += item.Value;
            }

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
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Countries Origin Chart");

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
            Dictionary<string, double> countryCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

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
                            if (countryCounts.TryGetValue(itemCountry, out double itemValue))
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
                    if (countryCounts.TryGetValue(country, out double value))
                    {
                        countryCounts[country] = ++value;
                    }
                    else
                    {
                        countryCounts[country] = 1;
                    }
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            // Get total count to calculate percentages
            double totalCount = countryCounts.Values.Sum();

            // Group countries if needed
            Dictionary<string, double> groupedCountryCounts = SortAndGroupData(countryCounts, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                string chartTitle = TranslatedChartTitles.CountriesOfOrigin;
                string countries = LanguageManager.TranslateString("Countries");

                ExcelSheetManager.ExportChartToExcel(groupedCountryCounts, filePath, eChartType.Pie, chartTitle, countries, label);
            }
            else
            {
                // Add data points to the dataset with percentage labels
                foreach (KeyValuePair<string, double> countryCount in groupedCountryCounts)
                {
                    double percentage = countryCount.Value / totalCount * 100;
                    dataset.DataPoints.Add(countryCount.Key, countryCount.Value);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{countryCount.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = GetChartColors();
                    UpdateChart(chart, dataset, false);
                }
            }

            return new ChartData(totalCount, groupedCountryCounts);
        }
        public static ChartData LoadCompaniesOfOriginForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Companies Origin Chart");

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
            Dictionary<string, double> companyCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

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
                            if (companyCounts.TryGetValue(itemCompany, out double itemValue))
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
                    if (companyCounts.TryGetValue(company, out double value))
                    {
                        companyCounts[company] = ++value;
                    }
                    else
                    {
                        companyCounts[company] = 1;
                    }
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            // Get total count to calculate percentages
            double totalCount = companyCounts.Values.Sum();

            // Group companies if needed
            Dictionary<string, double> groupedCompanyCounts = SortAndGroupData(companyCounts, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                string chartTitle = TranslatedChartTitles.CompaniesOfOrigin;
                string companies = LanguageManager.TranslateString("Companies");

                ExcelSheetManager.ExportChartToExcel(groupedCompanyCounts, filePath, eChartType.Pie, chartTitle, companies, label);
            }
            else
            {
                // Add data points to the dataset with percentage labels
                foreach (KeyValuePair<string, double> companyCount in groupedCompanyCounts)
                {
                    double percentage = companyCount.Value / totalCount * 100;
                    dataset.DataPoints.Add(companyCount.Key, companyCount.Value);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{companyCount.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = GetChartColors();
                    UpdateChart(chart, dataset, false);
                }
            }

            return new ChartData(totalCount, groupedCompanyCounts);
        }
        public static ChartData LoadCountriesOfDestinationForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Countries Destination Chart");

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
            Dictionary<string, double> countryCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

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
                            if (countryCounts.TryGetValue(itemCountry, out double itemValue))
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
                    if (countryCounts.TryGetValue(country, out double value))
                    {
                        countryCounts[country] = ++value;
                    }
                    else
                    {
                        countryCounts[country] = 1;
                    }
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            // Get total count to calculate percentages
            double totalCount = countryCounts.Values.Sum();

            // Group countries if needed
            Dictionary<string, double> groupedCountryCounts = SortAndGroupData(countryCounts, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                string chartTitle = TranslatedChartTitles.CountriesOfDestination;
                string countries = LanguageManager.TranslateString("Countries");

                ExcelSheetManager.ExportChartToExcel(groupedCountryCounts, filePath, eChartType.Pie, chartTitle, countries, label);
            }
            else
            {
                // Add data points to the dataset with percentage labels
                foreach (KeyValuePair<string, double> countryCount in groupedCountryCounts)
                {
                    double percentage = countryCount.Value / totalCount * 100;
                    dataset.DataPoints.Add(countryCount.Key, countryCount.Value);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{countryCount.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = GetChartColors();
                    UpdateChart(chart, dataset, false);
                }
            }

            return new ChartData(totalCount, groupedCountryCounts);
        }
        public static ChartData LoadAccountantsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Accountants Chart");

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
            Dictionary<string, double> accountantCounts = [];
            bool anyRowsVisible = false;

            foreach (Guna2DataGridView purchasesDataGridView in dataGridViews)
            {
                if (!DataGridViewManager.HasVisibleRows(purchasesDataGridView))
                {
                    continue;
                }

                foreach (DataGridViewRow row in purchasesDataGridView.Rows)
                {
                    if (!row.Visible) { continue; }
                    anyRowsVisible = true;

                    string accountant = row.Cells[ReadOnlyVariables.Accountant_column].Value.ToString();

                    if (!string.IsNullOrEmpty(accountant))
                    {
                        if (accountantCounts.TryGetValue(accountant, out double value))
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

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            // Get total count to calculate percentages
            double totalCount = accountantCounts.Values.Sum();

            // Group accountants if needed
            Dictionary<string, double> groupedAccountantCounts = SortAndGroupData(accountantCounts, grouping);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                string chartTitle = TranslatedChartTitles.AccountantsTransactions;
                string accountants = LanguageManager.TranslateString("Accountants");

                ExcelSheetManager.ExportChartToExcel(groupedAccountantCounts, filePath, eChartType.Pie, chartTitle, accountants, label);
            }
            else
            {
                // Add data points to the dataset with percentage labels
                foreach (KeyValuePair<string, double> accountantCount in groupedAccountantCounts)
                {
                    double percentage = accountantCount.Value / totalCount * 100;
                    dataset.DataPoints.Add(accountantCount.Key, accountantCount.Value);
                    dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{accountantCount.Key} ({percentage:F2}%)";
                }

                if (canUpdateChart)
                {
                    dataset.FillColors = GetChartColors();
                    UpdateChart(chart, dataset, false);
                }
            }

            return new ChartData(totalCount, groupedAccountantCounts);
        }
        public static SalesExpensesChartData LoadSalesVsExpensesChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Sales vs Expenses Chart");

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
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            // Create the datasets for expenses and sales with labels
            IGunaDataset expensesDataset, salesDataset;
            if (isLineChart)
            {
                expensesDataset = new GunaLineDataset { Label = expensesLabel };
                salesDataset = new GunaLineDataset { Label = salesLabel };
            }
            else
            {
                expensesDataset = new GunaBarDataset { Label = expensesLabel };
                salesDataset = new GunaBarDataset { Label = salesLabel };
            }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(expensesDataset, isLineChart, CustomColors.PastelGreen);
                ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> expensesByDate = [], salesByDate = [];
            HashSet<string> allDates = [];

            // Calculate expenses totals by date
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (expensesByDate.ContainsKey(formattedDate))
                {
                    expensesByDate[formattedDate] += total;
                }
                else
                {
                    expensesByDate[formattedDate] = total;
                }
            }

            // Calculate sales totals by date
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (salesByDate.ContainsKey(formattedDate))
                {
                    salesByDate[formattedDate] += total;
                }
                else
                {
                    salesByDate[formattedDate] = total;
                }
            }

            if (expensesByDate.Count == 0 && salesByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Sort the dates
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
                    {
                        { expensesLabel, expensesByDate.TryGetValue(date, out double eValue) ? eValue : 0 },
                        { salesLabel, salesByDate.TryGetValue(date, out double sValue) ? sValue : 0 }
                    };
                }
                string name = TranslatedChartTitles.SalesVsExpenses;

                ExcelSheetManager.ExportMultiDataSetChartToExcel(combinedData, filePath, isLineChart ? eChartType.Line : eChartType.ColumnClustered, name);
            }
            else if (canUpdateChart)
            {
                foreach (string date in sortedDates)
                {
                    double salesValue = salesByDate.TryGetValue(date, out double sValue) ? sValue : 0;
                    double expensesValue = expensesByDate.TryGetValue(date, out double eValue) ? eValue : 0;

                    if (isLineChart)
                    {
                        ((GunaLineDataset)expensesDataset).DataPoints.Add(date, expensesValue);
                        ((GunaLineDataset)salesDataset).DataPoints.Add(date, salesValue);
                    }
                    else
                    {
                        ((GunaBarDataset)expensesDataset).DataPoints.Add(date, expensesValue);
                        ((GunaBarDataset)salesDataset).DataPoints.Add(date, salesValue);

                        float barPercentage = salesDataset.DataPointCount + expensesDataset.DataPointCount == 1 ? 0.2f : 0.4f;
                        ((GunaBarDataset)expensesDataset).BarPercentage = barPercentage;
                        ((GunaBarDataset)salesDataset).BarPercentage = barPercentage;
                    }
                }

                ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
                {
                    chart.Datasets.Clear();
                    chart.Datasets.Add(expensesDataset);
                    chart.Datasets.Add(salesDataset);

                    ApplyCurrencyFormatToDataset(expensesDataset);
                    ApplyCurrencyFormatToDataset(salesDataset);

                    chart.Update();
                    Application.DoEvents();
                });
            }

            return new SalesExpensesChartData(expensesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageTransactionValueChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Average Transaction Chart");

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
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            // Create the datasets for purchases and sales with labels
            IGunaDataset purchasesDataset, salesDataset;
            if (isLineChart)
            {
                purchasesDataset = new GunaLineDataset { Label = purchaseLabel };
                salesDataset = new GunaLineDataset { Label = saleLabel };
            }
            else
            {
                purchasesDataset = new GunaBarDataset { Label = purchaseLabel };
                salesDataset = new GunaBarDataset { Label = saleLabel };
            }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(purchasesDataset, isLineChart, CustomColors.PastelGreen);
                ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, (double total, int count)> purchasesByDate = [], salesByDate = [];
            HashSet<string> allDates = [];

            // Process sales data
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (salesByDate.TryGetValue(formattedDate, out (double total, int count) value))
                {
                    (double existingTotal, int existingCount) = value;
                    salesByDate[formattedDate] = (existingTotal + total, existingCount + 1);
                }
                else
                {
                    salesByDate[formattedDate] = (total, 1);
                }
            }

            // Process purchases data
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (purchasesByDate.TryGetValue(formattedDate, out (double total, int count) value))
                {
                    (double existingTotal, int existingCount) = value;
                    purchasesByDate[formattedDate] = (existingTotal + total, existingCount + 1);
                }
                else
                {
                    purchasesByDate[formattedDate] = (total, 1);
                }
            }

            if (salesByDate.Count == 0 && purchasesByDate.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Calculate averages
            Dictionary<string, double> avgPurchasesByDate = purchasesByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );
            Dictionary<string, double> avgSalesByDate = salesByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.total / kvp.Value.count, 2)
            );

            // Sort the dates
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
                    {
                        { purchaseLabel, avgPurchasesByDate.TryGetValue(date, out double pValue) ? pValue : 0 },
                        { saleLabel, avgSalesByDate.TryGetValue(date, out double sValue) ? sValue : 0 }
                    };
                }
                string name = TranslatedChartTitles.AverageTransactionValue;

                ExcelSheetManager.ExportMultiDataSetChartToExcel(combinedData, filePath, isLineChart ? eChartType.Line : eChartType.ColumnClustered, name);
            }
            else if (canUpdateChart)
            {
                foreach (string date in sortedDates)
                {
                    double purchasesValue = avgPurchasesByDate.TryGetValue(date, out double pValue) ? pValue : 0;
                    double salesValue = avgSalesByDate.TryGetValue(date, out double sValue) ? sValue : 0;

                    if (isLineChart)
                    {
                        ((GunaLineDataset)purchasesDataset).DataPoints.Add(date, purchasesValue);
                        ((GunaLineDataset)salesDataset).DataPoints.Add(date, salesValue);
                    }
                    else
                    {
                        ((GunaBarDataset)purchasesDataset).DataPoints.Add(date, purchasesValue);
                        ((GunaBarDataset)salesDataset).DataPoints.Add(date, salesValue);

                        float barPercentage = salesDataset.DataPointCount + purchasesDataset.DataPointCount == 1 ? 0.2f : 0.4f;
                        ((GunaBarDataset)purchasesDataset).BarPercentage = barPercentage;
                        ((GunaBarDataset)salesDataset).BarPercentage = barPercentage;
                    }
                }

                ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
                {
                    chart.Datasets.Clear();
                    chart.Datasets.Add(purchasesDataset);
                    chart.Datasets.Add(salesDataset);

                    ApplyCurrencyFormatToDataset(purchasesDataset);
                    ApplyCurrencyFormatToDataset(salesDataset);

                    chart.Update();
                    Application.DoEvents();
                });
            }

            return new SalesExpensesChartData(avgPurchasesByDate, avgSalesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadTotalTransactionsChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Total Transactions Chart");

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
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            // Create datasets for purchases and sales
            IGunaDataset purchasesDataset, salesDataset;
            if (isLineChart)
            {
                purchasesDataset = new GunaLineDataset { Label = purchasesLabel };
                salesDataset = new GunaLineDataset { Label = salesLabel };
            }
            else
            {
                purchasesDataset = new GunaBarDataset { Label = purchasesLabel };
                salesDataset = new GunaBarDataset { Label = salesLabel };
            }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(purchasesDataset, isLineChart, CustomColors.PastelGreen);
                ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> purchasesByDate = [], salesByDate = [];
            HashSet<string> allDates = [];
            bool anyRowsVisible = false;

            // Process purchase transactions
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (purchasesByDate.TryGetValue(formattedDate, out double value))
                {
                    purchasesByDate[formattedDate] = ++value;
                }
                else
                {
                    purchasesByDate[formattedDate] = 1;
                }
            }

            // Process sale transactions
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (salesByDate.TryGetValue(formattedDate, out double value))
                {
                    salesByDate[formattedDate] = ++value;
                }
                else
                {
                    salesByDate[formattedDate] = 1;
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Sort dates chronologically
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
                    {
                        { purchasesLabel, purchasesByDate.TryGetValue(date, out double pValue) ? pValue : 0 },
                        { salesLabel, salesByDate.TryGetValue(date, out double sValue) ? sValue : 0 }
                    };
                }

                string chartTitle = TranslatedChartTitles.TotalTransactions;
                ExcelSheetManager.ExportMultiDataSetChartToExcel(
                    combinedData,
                    filePath,
                    isLineChart ? eChartType.Line : eChartType.ColumnClustered,
                    chartTitle
                );
            }
            else if (canUpdateChart)
            {
                foreach (string date in sortedDates)
                {
                    double purchaseValue = purchasesByDate.TryGetValue(date, out double pValue) ? pValue : 0;
                    double salesValue = salesByDate.TryGetValue(date, out double sValue) ? sValue : 0;

                    if (isLineChart)
                    {
                        ((GunaLineDataset)purchasesDataset).DataPoints.Add(date, purchaseValue);
                        ((GunaLineDataset)salesDataset).DataPoints.Add(date, salesValue);
                    }
                    else
                    {
                        ((GunaBarDataset)purchasesDataset).DataPoints.Add(date, purchaseValue);
                        ((GunaBarDataset)salesDataset).DataPoints.Add(date, salesValue);

                        float barPercentage = purchasesDataset.DataPointCount + salesDataset.DataPointCount == 1 ? 0.2f : 0.4f;
                        ((GunaBarDataset)purchasesDataset).BarPercentage = barPercentage;
                        ((GunaBarDataset)salesDataset).BarPercentage = barPercentage;
                    }
                }

                ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
                {
                    chart.Datasets.Clear();
                    chart.Datasets.Add(purchasesDataset);
                    chart.Datasets.Add(salesDataset);

                    chart.Update();
                    Application.DoEvents();
                });
            }

            return new SalesExpensesChartData(purchasesByDate, salesByDate, sortedDates);
        }
        public static SalesExpensesChartData LoadAverageShippingCostsChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true, bool includeZeroShipping = false)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Average Shipping Chart");

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView, purchasesDataGridView);
            string purchaseLabel = LanguageManager.TranslateString("Purchases");
            string saleLabel = LanguageManager.TranslateString("Sales");

            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel && canUpdateChart)
            {
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            // Create datasets for purchases and sales shipping costs
            IGunaDataset purchasesDataset, salesDataset;
            if (isLineChart)
            {
                purchasesDataset = new GunaLineDataset { Label = purchaseLabel };
                salesDataset = new GunaLineDataset { Label = saleLabel };
            }
            else
            {
                purchasesDataset = new GunaBarDataset { Label = purchaseLabel };
                salesDataset = new GunaBarDataset { Label = saleLabel };
            }

            if (!exportToExcel && canUpdateChart)
            {
                ApplyStyleToBarOrLineDataSet(purchasesDataset, isLineChart, CustomColors.PastelGreen);
                ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, (double totalShipping, int orders)> purchaseShippingByDate = [], salesShippingByDate = [];
            HashSet<string> allDates = [];

            // Process purchase shipping costs
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) { continue; }
                // Only skip free shipping if includeZeroShipping is false
                if (shipping == 0 && !includeZeroShipping) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (purchaseShippingByDate.TryGetValue(formattedDate, out (double totalShipping, int orders) value))
                {
                    (double totalShipping, int orders) = value;
                    purchaseShippingByDate[formattedDate] = (totalShipping + shipping, orders + 1);
                }
                else
                {
                    purchaseShippingByDate[formattedDate] = (shipping, 1);
                }
            }

            // Process sale shipping costs
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Shipping_column], out double shipping)) { continue; }
                // Only skip free shipping if includeZeroShipping is false
                if (shipping == 0 && !includeZeroShipping) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (salesShippingByDate.TryGetValue(formattedDate, out (double totalShipping, int orders) value))
                {
                    (double totalShipping, int orders) = value;
                    salesShippingByDate[formattedDate] = (totalShipping + shipping, orders + 1);
                }
                else
                {
                    salesShippingByDate[formattedDate] = (shipping, 1);
                }
            }

            if (allDates.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Calculate averages
            Dictionary<string, double> avgPurchaseShipping = purchaseShippingByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.totalShipping / kvp.Value.orders, 2)
            );
            Dictionary<string, double> avgSalesShipping = salesShippingByDate.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.totalShipping / kvp.Value.orders, 2)
            );

            // Sort dates
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
                    {
                        { purchaseLabel, avgPurchaseShipping.TryGetValue(date, out double pValue) ? pValue : 0 },
                        { saleLabel, avgSalesShipping.TryGetValue(date, out double sValue) ? sValue : 0 }
                    };
                }

                string chartTitle = TranslatedChartTitles.AverageShippingCosts;
                ExcelSheetManager.ExportMultiDataSetChartToExcel(
                    combinedData,
                    filePath,
                    isLineChart ? eChartType.Line : eChartType.ColumnClustered,
                    chartTitle
                );
            }
            else if (canUpdateChart)
            {
                foreach (string date in sortedDates)
                {
                    double purchaseValue = avgPurchaseShipping.TryGetValue(date, out double pValue) ? pValue : 0;
                    double salesValue = avgSalesShipping.TryGetValue(date, out double sValue) ? sValue : 0;

                    if (isLineChart)
                    {
                        ((GunaLineDataset)purchasesDataset).DataPoints.Add(date, purchaseValue);
                        ((GunaLineDataset)salesDataset).DataPoints.Add(date, salesValue);
                    }
                    else
                    {
                        ((GunaBarDataset)purchasesDataset).DataPoints.Add(date, purchaseValue);
                        ((GunaBarDataset)salesDataset).DataPoints.Add(date, salesValue);

                        float barPercentage = purchasesDataset.DataPointCount + salesDataset.DataPointCount == 1 ? 0.2f : 0.4f;
                        ((GunaBarDataset)purchasesDataset).BarPercentage = barPercentage;
                        ((GunaBarDataset)salesDataset).BarPercentage = barPercentage;
                    }
                }

                ChartUpdateManager.UpdateChartWithRendering(chart, (c) =>
                {
                    chart.Datasets.Clear();
                    chart.Datasets.Add(purchasesDataset);
                    chart.Datasets.Add(salesDataset);

                    ApplyCurrencyFormatToDataset(purchasesDataset);
                    ApplyCurrencyFormatToDataset(salesDataset);

                    chart.Update();
                    Application.DoEvents();
                });
            }

            // Calculate overall averages for the return value
            double avgPurchaseTotal = avgPurchaseShipping.Values.Count != 0
                ? avgPurchaseShipping.Values.Average()
                : 0;
            double avgSalesTotal = avgSalesShipping.Values.Count != 0
                ? avgSalesShipping.Values.Average()
                : 0;

            return new SalesExpensesChartData(avgPurchaseShipping, avgSalesShipping, sortedDates);
        }
        public static SalesExpensesChartData LoadGrowthRateChart(GunaChart chart, bool exportToExcel = false, string filePath = null, bool canUpdateChart = true)
        {
            using IDisposable timer = ChartPerformanceMonitor.TimeChartOperation(chart.Name, "Growth Rate Chart");

            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView, purchasesDataGridView);
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

            Dictionary<string, double> expensesByDate = [], revenueByDate = [];
            HashSet<string> allDates = [];

            // Process expense data
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (expensesByDate.TryGetValue(formattedDate, out double value))
                {
                    expensesByDate[formattedDate] = value + total;
                }
                else
                {
                    expensesByDate[formattedDate] = total;
                }
            }

            // Process revenue data
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[ReadOnlyVariables.Total_column], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[ReadOnlyVariables.Date_column].Value);
                string formattedDate = date.ToString(dateFormat);
                allDates.Add(formattedDate);

                if (revenueByDate.TryGetValue(formattedDate, out double value))
                {
                    revenueByDate[formattedDate] = value + total;
                }
                else
                {
                    revenueByDate[formattedDate] = total;
                }
            }

            if (allDates.Count == 0)
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            // Sort dates chronologically
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            // Calculate growth rates
            Dictionary<string, double> expenseGrowth = [], revenueGrowth = [];

            double? previousExpense = null, previousRevenue = null; ;

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
                Dictionary<string, Dictionary<string, double>> combinedData = [];
                foreach (string date in sortedDates)
                {
                    combinedData[date] = new Dictionary<string, double>
                    {
                        { expensesLabel, expenseGrowth.TryGetValue(date, out double eValue) ? eValue : 0 },
                        { revenueLabel, revenueGrowth.TryGetValue(date, out double rValue) ? rValue : 0 }
                    };
                }

                string chartTitle = TranslatedChartTitles.GrowthRates;
                ExcelSheetManager.ExportMultiDataSetChartToExcel(
                    combinedData,
                    filePath,
                    eChartType.Line,
                    chartTitle,
                    true
                );
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
                    Application.DoEvents();
                });
            }

            return new SalesExpensesChartData(expenseGrowth, revenueGrowth, sortedDates);
        }

        // Methods
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
        private static ColorCollection GetChartColors()
        {
            return
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
        }
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
                    if (!row.Visible) { continue; }

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
            chart.Zoom = ZoomMode.Y;
            chart.XAxes.Display = false;
            chart.XAxes.GridLines.Display = false;
            chart.XAxes.Ticks.Display = false;

            chart.YAxes.Display = false;
            chart.YAxes.GridLines.Display = false;
            chart.YAxes.Ticks.Display = false;

            chart.Datasets.Clear();
            chart.Update();
        }
    }
}