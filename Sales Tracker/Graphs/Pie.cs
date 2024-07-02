using Guna.Charts.WinForms;
using Guna.UI2.WinForms;

namespace Sales_Tracker.Graphs
{
    class Pie
    {
        public static void ConfigureChart(GunaChart chart)
        {
            chart.Legend.Position = LegendPosition.Right;
            chart.XAxes.Display = false;
            chart.YAxes.Display = false;
        }
        public static void LoadDistributionIntoChart(Guna2DataGridView dataGridView, GunaChart chart)
        {
            ConfigureChart(chart);

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

                double shipping = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Shipping.ToString()].Value);
                double tax = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Tax.ToString()].Value);
                double fee = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.Fee.ToString()].Value);

                int quantity = Convert.ToInt32(row.Cells[MainMenu_Form.PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[MainMenu_Form.PurchaseColumns.PricePerUnit.ToString()].Value);
                string category = row.Cells[MainMenu_Form.PurchaseColumns.Category.ToString()].Value.ToString();
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

            // Add combined category costs
            foreach (KeyValuePair<string, double> category in categoryCosts)
            {
                dataset.DataPoints.Add(category.Key, category.Value);
            }

            // Add separate datapoints
            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.SalesColumns.Shipping], totalShipping);
            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.SalesColumns.Tax], totalTax);
            dataset.DataPoints.Add(MainMenu_Form.Instance.SalesColumnHeaders[MainMenu_Form.SalesColumns.Fee], totalFee);

            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}