using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Data;

namespace Sales_Tracker.Graphs
{
    class Bar
    {
        public static void ConfigureChart(GunaChart chart, bool isLineChart)
        {
            if (isLineChart)
            {
                chart.XAxes.GridLines.Display = true;
            }
            else
            {
                chart.XAxes.GridLines.Display = false;
            }
            chart.YAxes.GridLines.Display = true;
            chart.Legend.Display = false;
        }
        public static double LoadTotalsIntoChart(Guna2DataGridView dataGridView, GunaChart chart, bool isLineChart)
        {
            ConfigureChart(chart, isLineChart);

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

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.PricePerUnit.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Tax.ToString()].Value);

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

            SortAndAddDataSetAndSetBarPercentage(revenueByDate, dateFormat, dataset, isLineChart);

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();

            return grandTotal;
        }
        public static double LoadProfitsIntoChart(Guna2DataGridView salesDataGridView, Guna2DataGridView purchasesDataGridView, GunaChart chart, bool isLineChart)
        {
            ConfigureChart(chart, isLineChart);

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

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.PricePerUnit.ToString()].Value);

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

                DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.PurchaseColumns.Date.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.PurchaseColumns.Quantity.ToString()].Value);
                double costPerUnit = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.PricePerUnit.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Tax.ToString()].Value);

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

            // Get grandTotal
            foreach (KeyValuePair<string, double> item in profitByDate)
            {
                grandTotal += item.Value;
            }

            SortAndAddDataSetAndSetBarPercentage(profitByDate, dateFormat, dataset, isLineChart);

            // Update the chart
            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();

            return grandTotal;
        }

        // Methods
        private static void SortAndAddDataSetAndSetBarPercentage(Dictionary<string, double> list, string dateFormat, IGunaDataset dataset, bool isLineChart)
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
                    if (row.Cells[MainMenu_Form.PurchaseColumns.Date.ToString()].Value != null)
                    {
                        DateTime date = Convert.ToDateTime(row.Cells[MainMenu_Form.PurchaseColumns.Date.ToString()].Value);
                        if (date < minDate) { minDate = date; }
                        if (date > maxDate) { maxDate = date; }
                    }
                }
            }
            return (minDate, maxDate);
        }
    }
}