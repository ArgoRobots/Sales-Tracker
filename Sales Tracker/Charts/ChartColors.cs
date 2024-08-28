using Guna.Charts.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Charts
{
    internal class ChartColors
    {
        public static ChartConfig Config()
        {
            ChartConfig config = new();
            Color gridColor = CustomColors.grayText;
            Color foreColor = CustomColors.text;

            config.Title.ForeColor = foreColor;

            config.Legend.LabelForeColor = foreColor;

            config.XAxes.GridLines.Color = gridColor;
            config.XAxes.GridLines.ZeroLineColor = gridColor;
            config.XAxes.Ticks.ForeColor = foreColor;

            config.YAxes.GridLines.Color = gridColor;
            config.YAxes.GridLines.ZeroLineColor = gridColor;
            config.YAxes.Ticks.ForeColor = foreColor;

            config.ZAxes.GridLines.Color = gridColor;
            config.ZAxes.GridLines.ZeroLineColor = gridColor;
            config.ZAxes.Ticks.ForeColor = foreColor;
            config.ZAxes.PointLabels.ForeColor = foreColor;

            return config;
        }
    }
}