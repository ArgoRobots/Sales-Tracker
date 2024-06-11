using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Data;
using System.Windows.Forms;
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
        public static double LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            ConfigureChart(chart);

            if (dataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return 0;
            }

            GunaBarDataset dataset = new()
            {
                BarPercentage = 0.6f,  // Set a maximum width for the bars
                FillColors = [CustomColors.accent_blue]
            };

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(dataGridView.Rows);

            string dateFormat = GetDateFormat(maxDate - minDate);

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

            SortAndAddDataSetAndSetBarPercentage(revenueByDate, dateFormat, dataset);

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();

            return grandTotal;
        }
        public static double LoadProfitsIntoChart(Guna2DataGridView salesDataGridView, Guna2DataGridView purchasesDataGridView, GunaChart chart)
        {
            ConfigureChart(chart);

            if (salesDataGridView.Rows.Count == 0 && purchasesDataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return 0;
            }

            GunaBarDataset dataset = new()
            {
                BarPercentage = 0.6f,  // Set a maximum width for the bars
                FillColors = [CustomColors.accent_blue]
            };

            double grandTotal = 0;
            DateTime minDate, maxDate;
            (minDate, maxDate) = GetMinMaxDate(salesDataGridView.Rows, purchasesDataGridView.Rows);

            string dateFormat = GetDateFormat(maxDate - minDate);

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

            // get grandTotal
            foreach (KeyValuePair<string, double> item in profitByDate)
            {
                grandTotal += item.Value;
            }

            SortAndAddDataSetAndSetBarPercentage(profitByDate, dateFormat, dataset);

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();

            return grandTotal;
        }

        // Functions
        private static void SortAndAddDataSetAndSetBarPercentage(Dictionary<string, double> list, string dateFormat, GunaBarDataset dataset)
        {
            // Sort the dictionary by date keys
            IOrderedEnumerable<KeyValuePair<string, double>> sortedProfitByDate = list.OrderBy(kvp => DateTime.ParseExact(kvp.Key, dateFormat, null));

            // Add dataset
            foreach (KeyValuePair<string, double> kvp in sortedProfitByDate)
            {
                dataset.DataPoints.Add(kvp.Key, kvp.Value);
            }

            // Set BarPercentage
            if (dataset.DataPoints.Count == 1)
            {
                dataset.BarPercentage = 0.2f;
            }
            else if (dataset.DataPoints.Count < 3)
            {
                dataset.BarPercentage = 0.4f;
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
                    if (row.Cells[PurchaseColumns.Date.ToString()].Value != null)
                    {
                        DateTime date = Convert.ToDateTime(row.Cells[PurchaseColumns.Date.ToString()].Value);
                        if (date < minDate) { minDate = date; }
                        if (date > maxDate) { maxDate = date; }
                    }
                }
            }
            return (minDate, maxDate);
        }
    }
}