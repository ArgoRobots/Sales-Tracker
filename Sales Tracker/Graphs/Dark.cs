using Guna.Charts.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Graphs
{
    class Dark
    {
        public static ChartConfig Config()
        {
            ChartConfig config = new();
            Color gridColor = CustomColors.grayText;
            Color foreColor = CustomColors.text;

            var chartFont = new ChartFont()
            {
                FontName = "Segoe UI",
                Size = 10,
                Style = ChartFontStyle.Normal
            };

            config.Title.ForeColor = foreColor;

            config.Legend.LabelFont = chartFont;
            config.Legend.LabelForeColor = foreColor;

            config.XAxes.GridLines.Color = gridColor;
            config.XAxes.GridLines.ZeroLineColor = gridColor;
            config.XAxes.Ticks.Font = chartFont;
            config.XAxes.Ticks.ForeColor = foreColor;

            config.YAxes.GridLines.Color = gridColor;
            config.YAxes.GridLines.ZeroLineColor = gridColor;
            config.YAxes.Ticks.Font = chartFont;
            config.YAxes.Ticks.ForeColor = foreColor;

            config.ZAxes.GridLines.Color = gridColor;
            config.ZAxes.GridLines.ZeroLineColor = gridColor;
            config.ZAxes.Ticks.Font = chartFont;
            config.ZAxes.Ticks.ForeColor = foreColor;
            config.ZAxes.PointLabels.Font = chartFont;
            config.ZAxes.PointLabels.ForeColor = foreColor;

            return config;
        }
    }
}