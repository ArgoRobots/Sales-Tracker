using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Data;

namespace Sales_Tracker.Charts
{
    class LoadChart
    {
        public static void ConfigureChartForBar(GunaChart chart)
        {
            chart.XAxes.GridLines.Display = false;
            chart.YAxes.GridLines.Display = true;
            chart.Legend.Display = false;
        }
        public static void ConfigureChartForLine(GunaChart chart)
        {
            chart.XAxes.GridLines.Display = true;
            chart.YAxes.GridLines.Display = true;
            chart.Legend.Display = false;
        }
        public static void ConfigureChartForPie(GunaChart chart)
        {
            chart.XAxes.Display = false;
            chart.YAxes.Display = false;
            chart.Legend.Position = LegendPosition.Right;
            chart.Legend.Display = true;
        }

        private static bool TryGetValue<T>(DataGridViewCell cell, out T value)
        {
            value = default;
            if (cell.Value.ToString() == "-")
            {
                return false;
            }

            value = (T)Convert.ChangeType(cell.Value, typeof(T));
            return true;
        }

        // Main charts
        public static double LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart)
        {
            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else { ConfigureChartForBar(chart); }

            if (dataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return 0;
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            if (isLineChart)
            {
                ((GunaLineDataset)dataset).FillColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).BorderColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).PointRadius = 5;
                ((GunaLineDataset)dataset).PointStyle = PointStyle.Circle;
                ((GunaLineDataset)dataset).PointFillColors = [CustomColors.accent_blue];
                ((GunaLineDataset)dataset).PointBorderColors = [CustomColors.accent_blue];
            }
            else
            {
                ((GunaBarDataset)dataset).BarPercentage = 0.6f;
                ((GunaBarDataset)dataset).FillColors = [CustomColors.accent_blue];
            }

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);
            string dateFormat = GetDateFormat(maxDate - minDate);
            Dictionary<string, double> revenueByDate = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()], out double pricePerUnit))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Tax.ToString()].Value);
                double totalRevenue = quantity * pricePerUnit + shipping + tax;
                grandTotal += totalRevenue;
                string formattedDate = date.ToString(dateFormat);

                if (revenueByDate.ContainsKey(formattedDate))
                {
                    revenueByDate[formattedDate] += totalRevenue;
                }
                else
                {
                    revenueByDate[formattedDate] = totalRevenue;
                }
            }

            SortAndAddDatasetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);
            UpdateChart(chart, dataset);
            return grandTotal;
        }
        public static void LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            ConfigureChartForPie(chart);

            if (dataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaPieDataset dataset = new();

            double totalTax = 0;
            double totalShipping = 0;
            double totalFee = 0;
            Dictionary<string, double> categoryCosts = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()], out double pricePerUnit))
                {
                    continue;
                }

                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Tax.ToString()].Value);
                double fee = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Fee.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
                string category = row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString();
                double cost = quantity * pricePerUnit;

                totalTax += tax;
                totalShipping += shipping;
                totalFee += fee;

                if (categoryCosts.ContainsKey(category))
                {
                    categoryCosts[category] += cost;
                }
                else
                {
                    categoryCosts[category] = cost;
                }
            }

            // Calculate total count for percentages
            double totalCost = totalTax + totalShipping + totalFee + categoryCosts.Values.Sum();

            // Add combined category costs with percentage labels
            foreach (KeyValuePair<string, double> category in categoryCosts)
            {
                double percentage = category.Value / totalCost * 100;
                dataset.DataPoints.Add(category.Key, category.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{category.Key} ({percentage:F2}%)";
            }

            // Add separate datapoints with percentage labels
            double shippingPercentage = (totalShipping / totalCost) * 100;
            double taxPercentage = (totalTax / totalCost) * 100;
            double feePercentage = (totalFee / totalCost) * 100;

            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Shipping], totalShipping);
            dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Shipping ({shippingPercentage:F2}%)";

            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Tax], totalTax);
            dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Tax ({taxPercentage:F2}%)";

            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.Column.Fee], totalFee);
            dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"Fee ({feePercentage:F2}%)";

            UpdateChart(chart, dataset);
        }
        public static double LoadProfitsIntoChart(Guna2DataGridView salesDataGridView, Guna2DataGridView purchasesDataGridView, GunaChart chart, bool isLineChart)
        {
            if (isLineChart)
            {
                ConfigureChartForLine(chart);
            }
            else { ConfigureChartForBar(chart); }

            if (salesDataGridView.Rows.Count == 0 && purchasesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return 0;
            }

            IGunaDataset dataset;
            if (isLineChart) { dataset = new GunaLineDataset(); }
            else { dataset = new GunaBarDataset(); }

            if (isLineChart)
            {
                ((GunaLineDataset)dataset).FillColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).BorderColor = CustomColors.accent_blue;
                ((GunaLineDataset)dataset).PointRadius = 5;
                ((GunaLineDataset)dataset).PointStyle = PointStyle.Circle;
                ((GunaLineDataset)dataset).PointFillColors = [CustomColors.accent_blue];
                ((GunaLineDataset)dataset).PointBorderColors = [CustomColors.accent_blue];
            }
            else
            {
                ((GunaBarDataset)dataset).BarPercentage = 0.6f;
                ((GunaBarDataset)dataset).FillColors = [CustomColors.accent_blue];
            }

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

                if (!TryGetValue(row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()], out double pricePerUnit))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
                double totalRevenue = quantity * pricePerUnit;
                string formattedDate = date.ToString(dateFormat);

                if (profitByDate.ContainsKey(formattedDate))
                {
                    profitByDate[formattedDate] += totalRevenue;
                }
                else
                {
                    profitByDate[formattedDate] = totalRevenue;
                }
            }

            // Subtract total cost from purchases
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }


                if (!TryGetValue(row.Cells[MainMenu_Form.Column.PricePerUnit.ToString()], out double pricePerUnit))
                {
                    continue;
                }

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.Column.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.Column.Quantity.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.Column.Tax.ToString()].Value);
                double totalCost = quantity * pricePerUnit + shipping + tax;
                string formattedDate = date.ToString(dateFormat);

                if (profitByDate.ContainsKey(formattedDate))
                {
                    profitByDate[formattedDate] -= totalCost;
                }
                else
                {
                    profitByDate[formattedDate] = -totalCost;
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
            ConfigureChartForPie(chart);

            if (purchasesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];

            // Process purchases data
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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

            // Calculate total count for percentages
            double totalCount = countryCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> countryCount in countryCounts)
            {
                double percentage = countryCount.Value / totalCount * 100;
                dataset.DataPoints.Add(countryCount.Key, countryCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{countryCount.Key} ({percentage:F2}%)";
            }

            UpdateChart(chart, dataset);
        }
        public static void LoadCompaniesOfOriginForProductsIntoChart(Guna2DataGridView purchasesDataGridView, GunaChart chart)
        {
            ConfigureChartForPie(chart);

            if (purchasesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaPieDataset dataset = new();
            Dictionary<string, double> companyCounts = [];

            // Process purchases data
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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

            // Calculate total count for percentages
            double totalCount = companyCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> companyCount in companyCounts)
            {
                double percentage = companyCount.Value / totalCount * 100;
                dataset.DataPoints.Add(companyCount.Key, companyCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{companyCount.Key} ({percentage:F2}%)";
            }

            UpdateChart(chart, dataset);
        }
        public static void LoadCountriesOfDestinationForProductsIntoChart(Guna2DataGridView salesDataGridView, GunaChart chart)
        {
            ConfigureChartForPie(chart);

            if (salesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaPieDataset dataset = new();
            Dictionary<string, double> countryCounts = [];

            // Process sales data
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

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

            // Calculate total count for percentages
            double totalCount = countryCounts.Values.Sum();

            // Add data points to the dataset with percentage labels
            foreach (KeyValuePair<string, double> companyCount in countryCounts)
            {
                double percentage = companyCount.Value / totalCount * 100;
                dataset.DataPoints.Add(companyCount.Key, companyCount.Value);
                dataset.DataPoints[dataset.DataPoints.Count - 1].Label = $"{companyCount.Key} ({percentage:F2}%)";
            }

            UpdateChart(chart, dataset);
        }


        // Methods
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

            foreach (var rows in rowCollections)
            {
                foreach (DataGridViewRow row in rows)
                {
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
    }
}