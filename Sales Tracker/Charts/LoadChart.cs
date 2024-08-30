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

            chart.XAxes.GridLines.Display = false;
            chart.YAxes.GridLines.Display = true;
            chart.YAxes.Display = true;
            chart.Legend.Display = false;
        }
        public static void ConfigureChartForLine(GunaChart chart)
        {
            if (chart == null) { return; }

            chart.XAxes.GridLines.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.XAxes.Display = true;
            chart.YAxes.Display = true;
            chart.Legend.Display = false;
        }
        public static void ConfigureChartForPie(GunaChart chart)
        {
            if (chart == null) { return; }

            chart.XAxes.Display = false;
            chart.YAxes.Display = false;
            chart.Legend.Position = LegendPosition.Right;
            chart.Legend.Display = true;
        }
        private static void ClearChartConfig(GunaChart chart)
        {
            chart.XAxes.Display = false;
            chart.YAxes.Display = false;
            chart.XAxes.GridLines.Display = false;
            chart.YAxes.GridLines.Display = false;
            chart.Legend.Display = false;
        }

        // Main charts
        public static double LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart)
        {
            if (!CheckForAnyData(dataGridView.Rows.Count, chart))
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

            ApplyStyleToBarOrLineDataSet(dataset, isLineChart);

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);
            Dictionary<string, double> revenueByDate = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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

            SortAndAddDatasetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);
            UpdateChart(chart, dataset);
            return grandTotal;
        }
        public static void LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            if (!CheckForAnyData(dataGridView.Rows.Count, chart))
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

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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
                    if (row.Tag is List<string> items)
                    {
                        foreach (string item in items)
                        {
                            // Items are in the format: "itemName,categoryName,country,company,quantity,pricePerUnit,totalPrice"
                            string[] itemDetails = item.Split(',');
                            if (itemDetails.Length > 5)
                            {
                                string itemCategory = itemDetails[1];
                                double itemCost = double.Parse(itemDetails[6]);

                                if (categoryCosts.TryGetValue(itemCategory, out double value))
                                {
                                    categoryCosts[itemCategory] = Math.Round(value + itemCost, 2);
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

            // Get total count to calculate percentages
            double totalCost = totalTax + totalShipping + totalFee + categoryCosts.Values.Sum();

            // Add combined category costs with percentage labels
            foreach (KeyValuePair<string, double> category in categoryCosts)
            {
                double roundedValue = Math.Round(category.Value, 2);
                double percentage = roundedValue / totalCost * 100;
                dataset.DataPoints.Add(category.Key, roundedValue);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{category.Key} ({percentage:F2}%)";
            }

            // Add separate datapoints with percentage labels
            double shippingPercentage = totalShipping / totalCost * 100;
            double taxPercentage = totalTax / totalCost * 100;
            double feePercentage = totalFee / totalCost * 100;

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
            if (!CheckForAnyData(salesDataGridView.Rows.Count, chart))
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

            ApplyStyleToBarOrLineDataSet(dataset, isLineChart);

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);

            string dateFormat = GetDateFormat(maxDate - minDate);

            // Group and sum the total profit based on the determined date format
            Dictionary<string, double> profitByDate = [];

            // Calculate total revenue from sales
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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
            if (!CheckForAnyData(purchasesDataGridView.Rows.Count, chart)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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
            if (!CheckForAnyData(purchasesDataGridView.Rows.Count, chart)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> companyCounts = [];

            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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
            if (!CheckForAnyData(salesDataGridView.Rows.Count, chart)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];

            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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
        public static void LoadAccountantsIntoChart(List<Guna2DataGridView> purchasesDataGridViews, GunaChart chart)
        {
            int totalRowCount = purchasesDataGridViews.Sum(grid => grid.Rows.Count);
            if (!CheckForAnyData(totalRowCount, chart)) { return; }

            ConfigureChartForPie(chart);

            GunaPieDataset dataset = new();
            Dictionary<string, double> accountantCounts = new();

            foreach (Guna2DataGridView purchasesDataGridView in purchasesDataGridViews)
            {
                if (!MainMenu_Form.DoDataGridViewsHaveVisibleRows(purchasesDataGridView))
                {
                    continue;
                }

                foreach (DataGridViewRow row in purchasesDataGridView.Rows)
                {
                    if (!row.Visible) { continue; }

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

        // Methods
        /// <summary>
        /// If there is no data, then it adds a Label to the chart that says "No data".
        /// </summary>
        /// <returns>True if there is any data, False if there is no data.</returns>
        private static bool CheckForAnyData(int dataGridViewRowCount, GunaChart gunaChart)
        {
            Label existingLabel = gunaChart.Controls.OfType<Label>().FirstOrDefault(label => label.Text == "No data");

            if (dataGridViewRowCount == 0)
            {
                // If there's no data and the label doesn't exist, create and add it
                if (existingLabel == null)
                {
                    Label label = new()
                    {
                        Font = new Font("Segoe UI", 12),
                        ForeColor = CustomColors.text,
                        Text = "No data",
                        AutoSize = true
                    };

                    gunaChart.Controls.Add(label);
                    CenterLabelInControl(label, gunaChart);

                    gunaChart.Resize += (sender, e) => CenterLabelInControl(label, gunaChart);

                    label.BringToFront();
                }
                return false;
            }
            else
            {
                // If there's data and the label exists, remove it
                if (existingLabel != null)
                {
                    gunaChart.Controls.Remove(existingLabel);
                    existingLabel.Dispose();
                }
                return true;
            }
        }
        private static void CenterLabelInControl(Label label, Control parent)
        {
            if (label != null && parent != null)
            {
                label.Location = new Point((parent.Width - label.Width) / 2, (parent.Height - label.Height) / 2);
            }
        }
        private static void ApplyStyleToBarOrLineDataSet(IGunaDataset dataset, bool isLineChart)
        {
            if (isLineChart)
            {
                ((GunaLineDataset)dataset).FillColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).BorderColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).PointRadius = 8;
                ((GunaLineDataset)dataset).PointStyle = PointStyle.Circle;
                ((GunaLineDataset)dataset).PointFillColors = [CustomColors.accent_blue];
                ((GunaLineDataset)dataset).PointBorderColors = [CustomColors.accent_blue];
                ((GunaLineDataset)dataset).BorderWidth = 5;
            }
            else
            {
                ((GunaBarDataset)dataset).BarPercentage = 0.6f;
                ((GunaBarDataset)dataset).FillColors = [CustomColors.accent_blue];
            }
        }
        public static void ApplyCustomColorsToDataset(GunaPieDataset dataset)
        {
            // Define the colors
            ColorCollection colors = new()
            {
                Color.FromArgb(102, 153, 204),  // Soft blue
                Color.FromArgb(153, 204, 102),  // Muted green
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
                if (((GunaBarDataset)dataset).DataPoints.Count == 1)
                {
                    ((GunaBarDataset)dataset).BarPercentage = 0.2f;
                }
                else if (((GunaBarDataset)dataset).DataPoints.Count < 3)
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