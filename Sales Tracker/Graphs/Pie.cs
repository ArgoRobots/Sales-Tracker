using Guna.Charts.WinForms;

namespace Sales_Tracker.Graphs
{
    class Pie
    {
        public static void Example(GunaChart chart)
        {
            string[] months = { "January", "February", "March", "April" };

            // Chart configuration  
            chart.Legend.Position = LegendPosition.Right;
            chart.XAxes.Display = false;
            chart.YAxes.Display = false;

            // Create a new dataset 
            GunaPieDataset dataset = new();
            var r = new Random();
            for (int i = 0; i < months.Length; i++)
            {
                // Random number
                int num = r.Next(10, 100);

                dataset.DataPoints.Add(months[i], num);
            }

            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}