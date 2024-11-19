using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using OfficeOpenXml.Drawing.Chart;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.Data;

namespace Sales_Tracker.Charts
{
    public enum PieChartGrouping
    {
        Unlimited = 0,
        Top12 = 12
    }

    /// <summary>
    /// The LoadChart class provides methods to configure, clear, and load data into GunaChart charts, 
    /// including bar, line, and pie charts, based on data provided by DataGridViews. 
    /// This class supports displaying revenue, profit, distribution, 
    /// and statistics like average order values, country of origin, and sales vs expenses.
    /// </summary>
    internal class LoadChart
    {
        // Configuration
        public static void ConfigureChartForBar(GunaChart chart)
        {
            if (chart == null) { return; }
            ClearChartConfig(chart);
            ClearChart(chart);

            chart.XAxes.Display = true;
            chart.XAxes.Ticks.Display = true;

            chart.YAxes.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.YAxes.Ticks.Display = true;
        }
        public static void ConfigureChartForLine(GunaChart chart)
        {
            if (chart == null) { return; }
            ClearChartConfig(chart);
            ClearChart(chart);

            chart.XAxes.Display = true;
            chart.XAxes.GridLines.Display = true;
            chart.XAxes.Ticks.Display = true;

            chart.YAxes.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.YAxes.Ticks.Display = true;
        }
        public static void ConfigureChartForPie(GunaChart chart)
        {
            if (chart == null) { return; }
            ClearChartConfig(chart);
            ClearChart(chart);

            chart.Legend.Display = true;
            chart.Legend.Position = LegendPosition.Right;
        }
        private static void ClearChartConfig(GunaChart chart)
        {
            chart.XAxes.Display = false;
            chart.XAxes.GridLines.Display = false;
            chart.XAxes.Ticks.Display = false;

            chart.YAxes.Display = false;
            chart.YAxes.GridLines.Display = false;
            chart.YAxes.Ticks.Display = false;

            chart.Legend.Display = false;
        }

        // Main charts
        public static ChartData LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null)
        {
            bool hasData = DataGridViewManager.HasVisibleRows(dataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            if (!exportToExcel)
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

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.TotalItems.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Tax.ToString()].Value);
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
                string chartTitle = MainMenu_Form.Instance.Sale_DataGridView.Visible
                    ? LanguageManager.TranslateSingleString("Total revenue")
                    : LanguageManager.TranslateSingleString("Total expenses");

                eChartType chartType = isLineChart ? eChartType.Line : eChartType.ColumnClustered;
                ExcelSheetManager.ExportChartToExcel(revenueByDate, filePath, chartType, chartTitle);
            }
            else
            {
                SortAndAddDatasetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, revenueByDate);
        }
        public static ChartData LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null)
        {
            bool hasData = DataGridViewManager.HasVisibleRows(dataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new();
            double totalTax = 0;
            double totalShipping = 0;
            double totalFee = 0;
            Dictionary<string, double> allData = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double cost))
                {
                    continue;
                }

                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Tax.ToString()].Value);
                double fee = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Fee.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.TotalItems.ToString()].Value);
                string category = row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString();

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
                    ? LanguageManager.TranslateSingleString("Distribution of revenue")
                    : LanguageManager.TranslateSingleString("Distribution of expenses");

                ExcelSheetManager.ExportChartToExcel(sortedData, filePath, eChartType.Pie, chartTitle);
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

                dataset.FillColors = GetChartColors();
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(totalCost, sortedData);
        }
        public static ChartData LoadProfitsIntoChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            if (!exportToExcel)
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

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
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

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
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
                ExcelSheetManager.ExportChartToExcel(profitByDate, filePath, chartType, LanguageManager.TranslateSingleString("Total profits"));
            }
            else
            {
                SortAndAddDatasetAndSetBarPercentage(profitByDate, dateFormat, dataset, isLineChart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(grandTotal, profitByDate);
        }

        // Statistics charts
        public static ChartData LoadCountriesOfOriginForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new();
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
                    string country = row.Cells[MainMenu_Form.Column.Country.ToString()].Value.ToString();
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
                string title = LanguageManager.TranslateSingleString(MainMenu_Form.ChartTitles.CountriesOfOrigin);
                ExcelSheetManager.ExportChartToExcel(groupedCountryCounts, filePath, eChartType.Pie, title);
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

                dataset.FillColors = GetChartColors();
                UpdateChart(chart, dataset, false);
            }

            return new ChartData(totalCount, groupedCountryCounts);
        }
        public static ChartData LoadCompaniesOfOriginForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(purchasesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new();
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
                    string company = row.Cells[MainMenu_Form.Column.Company.ToString()].Value.ToString();
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
                string title = LanguageManager.TranslateSingleString(MainMenu_Form.ChartTitles.CompaniesOfOrigin);
                ExcelSheetManager.ExportChartToExcel(groupedCompanyCounts, filePath, eChartType.Pie, title);
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

                dataset.FillColors = GetChartColors();
                UpdateChart(chart, dataset, false);
            }

            return new ChartData(totalCount, groupedCompanyCounts);
        }
        public static ChartData LoadCountriesOfDestinationForProductsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new();
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
                    string country = row.Cells[MainMenu_Form.Column.Country.ToString()].Value.ToString();
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
                string title = LanguageManager.TranslateSingleString(MainMenu_Form.ChartTitles.CountriesOfDestination);
                ExcelSheetManager.ExportChartToExcel(groupedCountryCounts, filePath, eChartType.Pie, title);
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

                dataset.FillColors = GetChartColors();
                UpdateChart(chart, dataset, false);
            }

            return new ChartData(totalCount, groupedCountryCounts);
        }
        public static ChartData LoadAccountantsIntoChart(GunaChart chart, PieChartGrouping grouping, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView[] dataGridViews = [
                MainMenu_Form.Instance.Sale_DataGridView,
                MainMenu_Form.Instance.Purchase_DataGridView
            ];

            bool hasData = DataGridViewManager.HasVisibleRows(dataGridViews);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                ConfigureChartForPie(chart);
            }

            GunaPieDataset dataset = new();
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

                    string accountant = row.Cells[MainMenu_Form.Column.Accountant.ToString()].Value.ToString();

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
                string title = LanguageManager.TranslateSingleString(MainMenu_Form.ChartTitles.AccountantsTransactions);
                ExcelSheetManager.ExportChartToExcel(groupedAccountantCounts, filePath, eChartType.Pie, title);
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

                dataset.FillColors = GetChartColors();
                UpdateChart(chart, dataset, false);
            }

            return new ChartData(totalCount, groupedAccountantCounts);
        }
        public static SalesExpensesChartData LoadSalesVsExpensesChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;
            Guna2DataGridView purchasesDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView, purchasesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return SalesExpensesChartData.Empty;
            }

            if (!exportToExcel)
            {
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            // Create the datasets for expenses and sales with labels
            IGunaDataset expensesDataset;
            IGunaDataset salesDataset;
            if (isLineChart)
            {
                expensesDataset = new GunaLineDataset { Label = LanguageManager.TranslateSingleString("Total expenses") };
                salesDataset = new GunaLineDataset { Label = LanguageManager.TranslateSingleString("Total sales") };
            }
            else
            {
                expensesDataset = new GunaBarDataset { Label = LanguageManager.TranslateSingleString("Total expenses") };
                salesDataset = new GunaBarDataset { Label = LanguageManager.TranslateSingleString("Total sales") };
            }

            if (!exportToExcel)
            {
                ApplyStyleToBarOrLineDataSet(expensesDataset, isLineChart, CustomColors.PastelGreen);
                ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> expensesByDate = [];
            Dictionary<string, double> salesByDate = [];
            HashSet<string> allDates = [];

            // Calculate expenses totals by date
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
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
                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
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
                        { LanguageManager.TranslateSingleString("Total expenses"), expensesByDate.TryGetValue(date, out double eValue) ? eValue : 0 },
                        { LanguageManager.TranslateSingleString("Total sales"), salesByDate.TryGetValue(date, out double sValue) ? sValue : 0 }
                    };
                }

                ExcelSheetManager.ExportMultiDataSetChartToExcel(combinedData, filePath, isLineChart ? eChartType.Line : eChartType.ColumnClustered, "Sales vs Expenses");
            }
            else
            {
                UpdateChartWithData(chart, sortedDates, salesDataset, expensesDataset, salesByDate, expensesByDate, isLineChart);
            }

            return new SalesExpensesChartData(
                salesByDate.Values.Sum(),
                expensesByDate.Values.Sum(),
                salesByDate,
                expensesByDate,
                sortedDates
            );
        }
        private static void UpdateChartWithData(
            GunaChart chart,
            List<string> sortedDates,
            IGunaDataset salesDataset,
            IGunaDataset expensesDataset,
            Dictionary<string, double> salesByDate,
            Dictionary<string, double> expensesByDate,
            bool isLineChart)
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

            chart.Datasets.Clear();
            chart.Datasets.Add(expensesDataset);
            chart.Datasets.Add(salesDataset);

            ApplyCurrencyFormatToDataset(expensesDataset);
            ApplyCurrencyFormatToDataset(salesDataset);

            chart.Legend.Display = true;
            chart.Legend.Position = LegendPosition.Top;

            chart.Update();
        }
        public static ChartData LoadAverageOrderValueChart(GunaChart chart, bool isLineChart, bool exportToExcel = false, string filePath = null)
        {
            Guna2DataGridView salesDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            bool hasData = DataGridViewManager.HasVisibleRows(salesDataGridView);
            if (!LabelManager.ManageNoDataLabelOnControl(hasData, chart))
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            if (!exportToExcel)
            {
                if (isLineChart)
                {
                    ConfigureChartForLine(chart);
                }
                else { ConfigureChartForBar(chart); }
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            if (!exportToExcel)
            {
                ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.PastelBlue);
            }

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> totalByDate = [];
            Dictionary<string, int> ordersByDate = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.Total.ToString()], out double total)) { continue; }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
                string formattedDate = date.ToString(dateFormat);

                if (totalByDate.ContainsKey(formattedDate))
                {
                    totalByDate[formattedDate] += total;
                    ordersByDate[formattedDate]++;
                }
                else
                {
                    totalByDate[formattedDate] = total;
                    ordersByDate[formattedDate] = 1;
                }
            }

            if (!anyRowsVisible || totalByDate.Count == 0)
            {
                ClearChart(chart);
                return ChartData.Empty;
            }

            Dictionary<string, double> averageOrderValueByDate = [];
            double totalAverageOrderValue = 0;

            // Calculate averages and total
            foreach (string date in totalByDate.Keys)
            {
                double averageValue = Math.Round(totalByDate[date] / ordersByDate[date], 2);
                averageOrderValueByDate[date] = averageValue;
                totalAverageOrderValue += averageValue;
            }

            // Calculate the overall average
            double overallAverage = Math.Round(totalAverageOrderValue / averageOrderValueByDate.Count, 2);

            if (exportToExcel && !string.IsNullOrEmpty(filePath))
            {
                eChartType chartType = isLineChart ? eChartType.Line : eChartType.ColumnClustered;
                string title = LanguageManager.TranslateSingleString("Average Order Value");
                ExcelSheetManager.ExportChartToExcel(averageOrderValueByDate, filePath, chartType, title);
            }
            else
            {
                SortAndAddDatasetAndSetBarPercentage(averageOrderValueByDate, dateFormat, dataset, isLineChart);
                UpdateChart(chart, dataset, true);
            }

            return new ChartData(overallAverage, averageOrderValueByDate);
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

                    if (row.Cells[MainMenu_Form.Column.Date.ToString()].Value != null)
                    {
                        DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
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
            chart.Update();
        }
        private static void ApplyCurrencyFormatToDataset(object dataset)
        {
            switch (dataset)
            {
                case GunaBarDataset barDataset:
                    barDataset.YFormat = "{0:C}";
                    break;

                case GunaLineDataset lineDataset:
                    lineDataset.YFormat = "{0:C}";
                    break;

                case GunaPieDataset pieDataset:
                    pieDataset.YFormat = "{0:C}";
                    break;
            }
        }
        private static void ClearChart(GunaChart chart)
        {
            ClearChartConfig(chart);
            chart.Datasets.Clear();
            chart.Update();
        }
    }
}