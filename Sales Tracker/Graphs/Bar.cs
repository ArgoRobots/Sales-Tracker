using Guna.Charts.WinForms;

namespace Sales_Tracker.Graphs
{
    class Bar
    {
        public static void Example(GunaChart chart)
        {
            // Chart configuration 
            chart.YAxes.GridLines.Display = false;

            // Create a new dataset 
            GunaBarDataset dataset = new();
            var r = new Random();
            dataset.DataPoints.Add("January", r.Next(-50, 100));
            dataset.DataPoints.Add("February", r.Next(-50, 100));
            dataset.DataPoints.Add("March", r.Next(-50, 100));
            dataset.DataPoints.Add("April", r.Next(-50, 100));
            dataset.DataPoints.Add("May", r.Next(-50, 100));
            dataset.DataPoints.Add("June", r.Next(-50, 100));
            dataset.DataPoints.Add("July", r.Next(-50, 100));

            chart.Datasets.Add(dataset);
            chart.Update();
        }
    }
}