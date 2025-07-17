using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Sales_Tracker.Theme;
using SkiaSharp;
using System.Reflection;
using Padding = LiveChartsCore.Drawing.Padding;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// A custom legend for LiveCharts that dynamically adjusts its layout, padding, and size
    /// based on the chart height to improve visual fit and avoid overflow.
    /// </summary>
    public class CustomLegend : SKDefaultLegend
    {
        protected override Layout<SkiaSharpDrawingContext> GetLayout(Chart chart)
        {
            float chartHeight = GetChartHeight(chart);

            // Calculate dynamic padding based on chart height
            (float horizontalPadding, float verticalPadding) = CalculateDynamicPadding(chartHeight);

            StackLayout stackLayout = new()
            {
                Orientation = ContainerOrientation.Vertical,
                Padding = new Padding(horizontalPadding, verticalPadding),
                HorizontalAlignment = Align.Start,
                VerticalAlignment = Align.Middle
            };

            foreach (ISeries series in chart.Series.Where(x => x.IsVisibleAtLegend))
            {
                stackLayout.Children.Add(new LegendItem(series, chartHeight));
            }
            return stackLayout;
        }

        /// <summary>
        /// Gets the chart height from the chart view control.
        /// </summary>
        private static float GetChartHeight(Chart chart)
        {
            // Try to get the chart view from the chart's view property
            if (chart.View != null)
            {
                // Access the actual chart control (CartesianChart, PieChart, etc.)
                if (chart.View is Control chartControl)
                {
                    return chartControl.Height;
                }
            }

            // Default height if we can't access the control
            return 400f;
        }

        /// <summary>
        /// Calculates dynamic padding based on chart height to prevent legend overflow.
        /// </summary>
        private static (float horizontal, float vertical) CalculateDynamicPadding(float chartHeight)
        {
            // Define height thresholds and corresponding padding values
            const float minHeight = 200f;
            const float normalHeight = 400f;

            // Default padding values
            const float defaultHorizontalPadding = 15f;
            const float defaultVerticalPadding = 4f;

            // Minimum padding values to maintain readability
            const float minHorizontalPadding = 8f;
            const float minVerticalPadding = 2f;

            if (chartHeight >= normalHeight)
            {
                // Use default padding for normal-sized charts
                return (defaultHorizontalPadding, defaultVerticalPadding);
            }
            else if (chartHeight <= minHeight)
            {
                // Use minimum padding for very small charts
                return (minHorizontalPadding, minVerticalPadding);
            }
            else
            {
                // Linearly interpolate padding based on height
                float ratio = (chartHeight - minHeight) / (normalHeight - minHeight);
                float horizontalPadding = minHorizontalPadding + (defaultHorizontalPadding - minHorizontalPadding) * ratio;
                float verticalPadding = minVerticalPadding + (defaultVerticalPadding - minVerticalPadding) * ratio;

                return (horizontalPadding, verticalPadding);
            }
        }
    }

    public class LegendItem : StackLayout
    {
        public LegendItem(ISeries series, float chartHeight = 400f)
        {
            Orientation = ContainerOrientation.Horizontal;

            // Calculate dynamic padding for legend items
            (float horizontalPadding, float verticalPadding) = CalculateDynamicItemPadding(chartHeight);
            Padding = new Padding(horizontalPadding, verticalPadding);

            VerticalAlignment = Align.Middle;
            HorizontalAlignment = Align.Start;

            // Get the actual color from the series Fill property
            SKColor seriesColor = GetActualSeriesColor(series);

            // Calculate dynamic sizes based on chart height
            (float bulletSize, float textSize, float textPadding) = CalculateDynamicSizes(chartHeight);

            // Add colored circle as bullet point
            CircleGeometry bulletGeometry = new()
            {
                Width = bulletSize,
                Height = bulletSize,
                Fill = new SolidColorPaint(seriesColor)
            };

            // Add text label
            LabelGeometry textGeometry = new()
            {
                Text = series.Name ?? "?",
                TextSize = textSize,
                Paint = new SolidColorPaint(ChartColors.ToSKColor(CustomColors.Text)),
                Padding = new Padding(textPadding, -5, 0, -5),
                VerticalAlign = Align.Start,
                HorizontalAlign = Align.Start
            };

            Children = [bulletGeometry, textGeometry];
        }

        /// <summary>
        /// Calculates dynamic padding for individual legend items based on chart height.
        /// </summary>
        private static (float horizontal, float vertical) CalculateDynamicItemPadding(float chartHeight)
        {
            // Define height thresholds and corresponding padding values
            const float minHeight = 200f;
            const float normalHeight = 400f;

            // Default padding values
            const float defaultHorizontalPadding = 12f;
            const float defaultVerticalPadding = 6f;

            // Minimum padding values
            const float minHorizontalPadding = 6f;
            const float minVerticalPadding = 3f;

            if (chartHeight >= normalHeight)
            {
                return (defaultHorizontalPadding, defaultVerticalPadding);
            }
            else if (chartHeight <= minHeight)
            {
                return (minHorizontalPadding, minVerticalPadding);
            }
            else
            {
                // Linearly interpolate padding based on height
                float ratio = (chartHeight - minHeight) / (normalHeight - minHeight);
                float horizontalPadding = minHorizontalPadding + (defaultHorizontalPadding - minHorizontalPadding) * ratio;
                float verticalPadding = minVerticalPadding + (defaultVerticalPadding - minVerticalPadding) * ratio;

                return (horizontalPadding, verticalPadding);
            }
        }

        /// <summary>
        /// Calculates dynamic sizes for legend elements based on chart height.
        /// </summary>
        private static (float bulletSize, float textSize, float textPadding) CalculateDynamicSizes(float chartHeight)
        {
            // Define height thresholds
            const float minHeight = 200f;
            const float normalHeight = 400f;

            // Default sizes
            const float defaultBulletSize = 14f;
            const float defaultTextSize = 20f;
            const float defaultTextPadding = 25f;

            // Minimum sizes
            const float minBulletSize = 10f;
            const float minTextSize = 14f;
            const float minTextPadding = 15f;

            if (chartHeight >= normalHeight)
            {
                return (defaultBulletSize, defaultTextSize, defaultTextPadding);
            }
            else if (chartHeight <= minHeight)
            {
                return (minBulletSize, minTextSize, minTextPadding);
            }
            else
            {
                // Linearly interpolate sizes based on height
                float ratio = (chartHeight - minHeight) / (normalHeight - minHeight);
                float bulletSize = minBulletSize + (defaultBulletSize - minBulletSize) * ratio;
                float textSize = minTextSize + (defaultTextSize - minTextSize) * ratio;
                float textPadding = minTextPadding + (defaultTextPadding - minTextPadding) * ratio;

                return (bulletSize, textSize, textPadding);
            }
        }

        private static SKColor GetActualSeriesColor(ISeries series)
        {
            // Try to get the color directly from the series Fill property
            Type seriesType = series.GetType();
            PropertyInfo? fillProperty = seriesType.GetProperty("Fill");

            if (fillProperty != null)
            {
                object? fillValue = fillProperty.GetValue(series);
                if (fillValue is SolidColorPaint solidColorPaint)
                {
                    return solidColorPaint.Color;
                }
            }

            // Fallback
            return ChartColors.ToSKColor(CustomColors.Text);
        }
    }
}