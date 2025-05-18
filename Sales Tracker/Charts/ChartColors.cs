using Guna.Charts.WinForms;
using Sales_Tracker.Theme;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Provides configuration for consistent theming of charts.
    /// Configures title, legend, and axis colors to match the application's current theme, utilizing colors from <see cref="CustomColors"/>.
    /// </summary>
    internal class ChartColors
    {
        public static ChartConfig Config()
        {
            ChartConfig config = new();
            Color gridColor = CustomColors.GrayText;
            Color foreColor = CustomColors.Text;

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