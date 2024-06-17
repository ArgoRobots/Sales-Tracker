using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker.Graphs
{
    class Pie
    {
        public static void LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            chart.Legend.Position = LegendPosition.Right;
            chart.XAxes.Display = false;
            chart.YAxes.Display = false;

            if (dataGridView.Rows.Count == 0)
            {
                chart.Datasets.Clear();
                chart.Update();
                return;
            }

            GunaPieDataset dataset = new();

            double totalTax = 0;
            double totalShipping = 0;
            Dictionary<string, double> categoryCosts = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                double tax = Convert.ToDouble(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[PurchaseColumns.PricePerUnit.ToString()].Value);
                string category = row.Cells[PurchaseColumns.Category.ToString()].Value.ToString();
                double cost = quantity * pricePerUnit;

                totalTax += tax;
                totalShipping += shipping;

                if (categoryCosts.ContainsKey(category))
                {
                    categoryCosts[category] += cost;
                }
                else
                {
                    categoryCosts[category] = cost;
                }
            }

            // Add tax and shipping as separate datapoints
            dataset.DataPoints.Add("Tax", totalTax);
            dataset.DataPoints.Add("Shipping", totalShipping);

            // Add combined category costs
            foreach (KeyValuePair<string, double> category in categoryCosts)
            {
                dataset.DataPoints.Add(category.Key, category.Value);
            }

            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}