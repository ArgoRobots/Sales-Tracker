using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Data;

namespace Sales_Tracker.Charts
{
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
        public static double LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(dataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text))
            {
                ClearChart(chart);
                return 0;
            }

            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else { ConfigureChartForBar(chart); }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.pastelBlue);

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
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
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
                return 0;
            }

            SortAndAddDatasetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);
            UpdateChart(chart, dataset);
            return grandTotal;
        }
        public static void LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(dataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text))
            {
                ClearChart(chart);
                return;
            }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();

            double totalTax = 0;
            double totalShipping = 0;
            double totalFee = 0;
            Dictionary<string, double> categoryCosts = [];
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
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
                string category = row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString();

                totalTax += tax;
                totalShipping += shipping;
                totalFee += fee;

                if (category == MainMenu_Form.emptyCell)
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

                                if (categoryCosts.ContainsKey(itemCategory))
                                {
                                    categoryCosts[itemCategory] += itemCost;
                                }
                                else
                                {
                                    categoryCosts[itemCategory] = itemCost;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (categoryCosts.ContainsKey(category))
                    {
                        categoryCosts[category] += cost;
                    }
                    else
                    {
                        categoryCosts[category] = cost;
                    }
                }
            }

            if (!anyRowsVisible)
            {
                ClearChart(chart);
                return;
            }

            totalTax = Math.Round(totalTax, 2);
            totalShipping = Math.Round(totalShipping, 2);
            totalFee = Math.Round(totalFee, 2);

            // Get total count to calculate percentages
            double totalCost = Math.Round(totalTax + totalShipping + totalFee + categoryCosts.Values.Sum(), 2);

            // Add combined category costs with percentage labels
            foreach (KeyValuePair<string, double> category in categoryCosts)
            {
                double roundedValue = Math.Round(category.Value, 2);
                double percentage = roundedValue / totalCost * 100;
                dataset.DataPoints.Add(category.Key, roundedValue);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{category.Key} ({percentage:F2}%)";
            }

            // Add separate datapoints with percentage labels
            double shippingPercentage = Math.Round(totalShipping / totalCost * 100, 2);
            double taxPercentage = Math.Round(totalTax / totalCost * 100, 2);
            double feePercentage = Math.Round(totalFee / totalCost * 100, 2);

            totalShipping = Math.Round(totalShipping, 2);

            if (totalShipping != 0)
            {
                dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Shipping], totalShipping);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Shipping ({shippingPercentage:F2}%)";
            }

            if (totalTax != 0)
            {
                dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Tax], totalTax);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Tax ({taxPercentage:F2}%)";
            }

            if (totalFee != 0)
            {
                dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Fee], totalFee);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Fee ({feePercentage:F2}%)";
            }

            ApplyCustomColorsToDataset(dataset);
            UpdateChart(chart, dataset);
        }
        public static double LoadProfitsIntoChart(Guna2DataGridView salesDataGridView, Guna2DataGridView purchasesDataGridView, GunaChart chart, bool isLineChart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(salesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text))
            {
                ClearChart(chart);
                return 0;
            }

            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else { ConfigureChartForBar(chart); }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.pastelBlue);

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
                return 0;
            }

            // Get grandTotal
            foreach (KeyValuePair<string, double> item in profitByDate)
            {
                grandTotal += item.Value;
            }

            SortAndAddDatasetAndSetBarPercentage(profitByDate, dateFormat, dataset, isLineChart);
            UpdateChart(chart, dataset);
            return grandTotal;
        }

        // Statistics charts
        public static void LoadCountriesOfOriginForProductsIntoChart(Guna2DataGridView purchasesDataGridView, GunaChart chart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(purchasesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                // Check for additional data in Tag property first
                if (row.Tag is List<string> items)
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
                return;
            }

            // Get total count to calculate percentages
            double totalCount = countryCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> countryCount in countryCounts)
            {
                double percentage = countryCount.Value / totalCount * 100;
                dataset.DataPoints.Add(countryCount.Key, countryCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{countryCount.Key} ({percentage:F2}%)";
            }

            ApplyCustomColorsToDataset(dataset);
            UpdateChart(chart, dataset);
        }
        public static void LoadCompaniesOfOriginForProductsIntoChart(Guna2DataGridView purchasesDataGridView, GunaChart chart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(purchasesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> companyCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                // Check for additional data in Tag property first
                if (row.Tag is List<string> items)
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
                return;
            }

            // Get total count to calculate percentages
            double totalCount = companyCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> companyCount in companyCounts)
            {
                double percentage = companyCount.Value / totalCount * 100;
                dataset.DataPoints.Add(companyCount.Key, companyCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{companyCount.Key} ({percentage:F2}%)";
            }

            ApplyCustomColorsToDataset(dataset);
            UpdateChart(chart, dataset);
        }
        public static void LoadCountriesOfDestinationForProductsIntoChart(Guna2DataGridView salesDataGridView, GunaChart chart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(salesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];
            bool anyRowsVisible = false;

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                anyRowsVisible = true;

                // Check for additional data in Tag property first
                if (row.Tag is List<string> items)
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
                return;
            }

            // Get total count to calculate percentages
            double totalCount = countryCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> countryCount in countryCounts)
            {
                double percentage = countryCount.Value / totalCount * 100;
                dataset.DataPoints.Add(countryCount.Key, countryCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{countryCount.Key} ({percentage:F2}%)";
            }

            ApplyCustomColorsToDataset(dataset);
            UpdateChart(chart, dataset);
        }
        public static void LoadAccountantsIntoChart(IEnumerable<Guna2DataGridView> dataGridViews, GunaChart chart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(dataGridViews.ToArray());
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> accountantCounts = new();
            bool anyRowsVisible = false;

            foreach (Guna2DataGridView purchasesDataGridView in dataGridViews)
            {
                if (!MainMenu_Form.DoDataGridViewsHaveVisibleRows(purchasesDataGridView))
                {
                    continue;
                }

                foreach (DataGridViewRow row in purchasesDataGridView.Rows)
                {
                    if (!row.Visible) { continue; }
                    anyRowsVisible = true;

                    string accountant = row.Cells[MainMenu_Form.Column.Name.ToString()].Value.ToString();

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
                return;
            }

            // Get total count to calculate percentages
            double totalCount = accountantCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> accountantCount in accountantCounts)
            {
                double percentage = accountantCount.Value / totalCount * 100;
                dataset.DataPoints.Add(accountantCount.Key, accountantCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{accountantCount.Key} ({percentage:F2}%)";
            }

            ApplyCustomColorsToDataset(dataset);
            UpdateChart(chart, dataset);
        }
        public static void LoadSalesVsExpensesChart(Guna2DataGridView purchasesDataGridView, Guna2DataGridView salesDataGridView, GunaChart chart, bool isLineChart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(salesDataGridView, purchasesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text))
            {
                ClearChart(chart);
                return;
            }

            // Ensure we're configuring the chart based on the chart type
            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else
            {
                ConfigureChartForBar(chart);
            }

            // Create the datasets for expenses and sales with labels
            IGunaDataset expensesDataset;
            IGunaDataset salesDataset;
            if (isLineChart)
            {
                expensesDataset = new GunaLineDataset { Label = "Total expenses" };
                salesDataset = new GunaLineDataset { Label = "Total sales" };
            }
            else
            {
                expensesDataset = new GunaBarDataset { Label = "Total expenses" };
                salesDataset = new GunaBarDataset { Label = "Total sales" };
            }

            // Apply styles to the datasets with different colors
            ApplyStyleToBarOrLineDataSet(expensesDataset, isLineChart, CustomColors.pastelGreen);
            ApplyStyleToBarOrLineDataSet(salesDataset, isLineChart, CustomColors.pastelBlue);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(purchasesDataGridView.Rows, salesDataGridView.Rows);

            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> expensesByDate = new();
            Dictionary<string, double> salesByDate = new();
            HashSet<string> allDates = new();

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
                return;
            }


            // Sort the dates
            List<string> sortedDates = allDates.OrderBy(date => DateTime.ParseExact(date, dateFormat, null)).ToList();

            // Add data points to the datasets
            foreach (string date in sortedDates)
            {
                // Add data point for sales
                double salesValue = salesByDate.TryGetValue(date, out double sValue) ? sValue : 0;

                // Add data point for expenses
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
                }
            }

            // Update the chart with both datasets
            chart.Datasets.Clear();
            chart.Datasets.Add(expensesDataset);
            chart.Datasets.Add(salesDataset);

            chart.Legend.Display = true;
            chart.Legend.Position = LegendPosition.Top;

            chart.Update();
        }
        public static void LoadAverageOrderValueChart(Guna2DataGridView salesDataGridView, GunaChart chart, bool isLineChart)
        {
            bool hasData = MainMenu_Form.DoDataGridViewsHaveVisibleRows(salesDataGridView);
            if (!MainMenu_Form.ManageNoDataLabelOnControl(hasData, chart, MainMenu_Form.noData_text))
            {
                ClearChart(chart);
                return;
            }

            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else
            {
                ConfigureChartForBar(chart);
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            ApplyStyleToBarOrLineDataSet(dataset, isLineChart, CustomColors.pastelBlue);

            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows);

            string dateFormat = GetDateFormat(maxDate - minDate);

            Dictionary<string, double> totalByDate = new();
            Dictionary<string, int> ordersByDate = new();

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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

            if (totalByDate.Count == 0)
            {
                ClearChart(chart);
                return;
            }

            Dictionary<string, double> averageOrderValueByDate = new();

            foreach (var date in totalByDate.Keys)
            {
                averageOrderValueByDate[date] = totalByDate[date] / ordersByDate[date];
            }

            SortAndAddDatasetAndSetBarPercentage(averageOrderValueByDate, dateFormat, dataset, isLineChart);
            UpdateChart(chart, dataset);
        }

        // Methods
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
                ((GunaBarDataset)dataset).BarPercentage = 0.6f;
                ((GunaBarDataset)dataset).FillColors = [color];
            }
        }
        public static void ApplyCustomColorsToDataset(GunaPieDataset dataset)
        {
            // Define the colors
            ColorCollection colors = new()
            {
                CustomColors.pastelBlue,
                CustomColors.pastelGreen,
                Color.FromArgb(204, 102, 153),  // Soft pink
                Color.FromArgb(153, 102, 204),  // Soft purple
                Color.FromArgb(102, 204, 153),  // Muted teal
                Color.FromArgb(204, 102, 102),  // Soft red
                Color.FromArgb(153, 153, 204),  // Muted lavender
                Color.FromArgb(204, 102, 102),  // Soft coral
                Color.FromArgb(153, 204, 204),  // Soft aqua
                Color.FromArgb(204, 153, 153),  // Muted peach
                Color.FromArgb(204, 153, 204),  // Soft lilac
                Color.FromArgb(153, 204, 153),  // Soft sage
                Color.FromArgb(204, 204, 153),  // Muted gold
                Color.FromArgb(204, 102, 102),  // Soft terracotta
                Color.FromArgb(153, 204, 153),  // Muted mint
                Color.FromArgb(153, 153, 204),  // Soft denim
                Color.FromArgb(204, 102, 102),  // Warm rust
                Color.FromArgb(153, 153, 153),  // Muted gray
            };

            // Apply the colors to the dataset
            dataset.FillColors = colors;
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
                ((GunaBarDataset)dataset).BarPercentage = 0.4f;
            }
        }
        private static bool TryGetValue<T>(DataGridViewCell cell, out T value)
        {
            value = default;

            if (cell.Value == null)
            {
                Log.Error_RowIsEmpty(MainMenu_Form.Instance.selectedDataGridView.Name);
                return false;
            }

            if (cell.Value.ToString() == MainMenu_Form.emptyCell)
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
        public static void UpdateChart(GunaChart chart, IGunaDataset dataset)
        {
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
        private static void ClearChart(GunaChart chart)
        {
            ClearChartConfig(chart);
            chart.Datasets.Clear();
            chart.Update();
        }
    }
}