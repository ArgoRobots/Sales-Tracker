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
            double totalCost = 0;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                double tax = Convert.ToDouble(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                double shipping = Convert.ToDouble(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                int quantity = Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                double pricePerUnit = Convert.ToDouble(row.Cells[PurchaseColumns.PricePerUnit.ToString()].Value);
                double cost = quantity * pricePerUnit;

                totalTax += tax;
                totalShipping += shipping;
                totalCost += cost;
            }

            dataset.DataPoints.Add("Tax", totalTax);
            dataset.DataPoints.Add("Shipping", totalShipping);
            dataset.DataPoints.Add("Cost", totalCost);

            chart.Datasets.Clear();
            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}