using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker.Graphs
{
    class Bar
    {
        public static void ConfigureChart(GunaChart chart)
        {
            chart.XAxes.GridLines.Display = false;
            chart.Legend.Display = false;
        }
        public static void LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            ConfigureChart(chart);

            if (dataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaBarDataset dataset = new()
            {
                BarPercentage = 0.6f,  // Set a maximum width for the bars
                FillColors = [CustomColors.accent_blue]
            };

            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                if (date < minDate) { minDate = date; }
                if (date > maxDate) { maxDate = date; }
            }

            // Determine the grouping type
            TimeSpan dateRange = maxDate - minDate;
            string dateFormat;

            if (dateRange.TotalDays > 365) // More than a year
            {
                dateFormat = "yyyy";
            }
            else if (dateRange.TotalDays > 30) // More than a month
            {
                dateFormat = "yyyy-MM";
            }
            else // Default to days
            {
                dateFormat = "yyyy-MM-dd";
            }

            // Group and sum the total revenue based on the determined date format
            Dictionary<string, double> revenueByDate = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[PurchaseColumns.PricePerUnit.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[PurchaseColumns.Tax.ToString()].Value);

                double totalRevenue = quantity * pricePerUnit + shipping + tax;
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

            // Sort the dictionary by date keys
            var sortedRevenueByDate = revenueByDate.OrderBy(kvp => DateTime.ParseExact(kvp.Key, dateFormat, null));

            // Add the data points to the dataset
            foreach (KeyValuePair<string, double> kvp in sortedRevenueByDate)
            {
                dataset.DataPoints.Add(kvp.Key, kvp.Value);
            }

            if (dataset.DataPoints.Count == 1)
            {
                dataset.BarPercentage = 0.2f;
            }
            else if (dataset.DataPoints.Count < 3)
            {
                dataset.BarPercentage = 0.4f;
            }

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
        public static void LoadProfitsIntoChart(Guna2DataGridView salesDataGridView, Guna2DataGridView purchasesDataGridView, GunaChart chart)
        {
            ConfigureChart(chart);

            if (salesDataGridView.Rows.Count == 0 && purchasesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update(); return;
            }

            GunaBarDataset dataset = new()
            {
                BarPercentage = 0.6f,  // Set a maximum width for the bars
                FillColors = [CustomColors.accent_blue]
            };

            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;

            // Determine the minimum and maximum date from sales and purchases
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                if (date < minDate) { minDate = date; }
                if (date > maxDate) { maxDate = date; }
            }
            foreach (DataGridViewRow row in purchasesDataGridView.Rows)
            {
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                if (date < minDate) { minDate = date; }
                if (date > maxDate) { maxDate = date; }
            }

            // Determine the grouping type
            TimeSpan dateRange = maxDate - minDate;
            string dateFormat;

            if (dateRange.TotalDays > 365) // More than a year
            {
                dateFormat = "yyyy";
            }
            else if (dateRange.TotalDays > 30) // More than a month
            {
                dateFormat = "yyyy-MM";
            }
            else // Default to days
            {
                dateFormat = "yyyy-MM-dd";
            }

            // Group and sum the total profit based on the determined date format
            Dictionary<string, double> profitByDate = [];

            // Calculate total revenue from sales
            foreach (DataGridViewRow row in salesDataGridView.Rows)
            {
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[PurchaseColumns.PricePerUnit.ToString()].Value);

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
                DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                double costPerUnit = Convert.ToDouble(row.Cells[PurchaseColumns.PricePerUnit.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                double totalCost = quantity * costPerUnit + shipping + tax;

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

            // Sort the dictionary by date keys
            var sortedProfitByDate = profitByDate.OrderBy(kvp => DateTime.ParseExact(kvp.Key, dateFormat, null));

            // Add the data points to the dataset
            foreach (KeyValuePair<string, double> kvp in sortedProfitByDate)
            {
                dataset.DataPoints.Add(kvp.Key, kvp.Value);
            }

            if (dataset.DataPoints.Count == 1)
            {
                dataset.BarPercentage = 0.2f;
            }
            else if (dataset.DataPoints.Count < 3)
            {
                dataset.BarPercentage = 0.4f;
            }

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}