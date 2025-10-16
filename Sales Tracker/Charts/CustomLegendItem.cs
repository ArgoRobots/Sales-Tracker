using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using Sales_Tracker.Theme;
using SkiaSharp;
using System.Reflection;
using Padding = LiveChartsCore.Drawing.Padding;

namespace Sales_Tracker.Charts
{
    public class CustomLegendItem : StackLayout
    {
        public CustomLegendItem(ISeries series, float chartHeight, bool forReport, float legendFontSize)
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
            (float bulletSize, float textSize, float textPadding) = CustomLegend.CalculateDynamicSizes(chartHeight, forReport, legendFontSize);

            // Add colored circle as bullet point
            CircleGeometry bulletGeometry = new()
            {
                Width = bulletSize,
                Height = bulletSize,
                Fill = new SolidColorPaint(seriesColor)
            };

            SKColor foreCcolor = forReport
                ? SKColors.Black  // Report generator does not have dark theme yet 
                : ChartColors.ToSKColor(CustomColors.Text);

            // Add text label
            LabelGeometry textGeometry = new()
            {
                Text = series.Name ?? "?",
                TextSize = textSize,
                Paint = new SolidColorPaint(foreCcolor),
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