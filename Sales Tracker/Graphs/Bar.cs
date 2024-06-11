using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker.Graphs
{
    class Bar
    {
        public static void LoadTotalMoneyIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            chart.XAxes.GridLines.Display = false;
            chart.Legend.Display = false;

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
                if (date < minDate) minDate = date;
                if (date > maxDate) maxDate = date;
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

            // Add the data points to the dataset
            foreach (KeyValuePair<string, double> kvp in revenueByDate)
            {
                dataset.DataPoints.Add(kvp.Key, kvp.Value);
            }

            if (dataset.DataPoints.Count == 1)
            {
                dataset.BarPercentage = 0.2f;
            }

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}