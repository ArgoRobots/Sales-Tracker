using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Padding = LiveChartsCore.Drawing.Padding;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// A custom legend for LiveCharts that dynamically adjusts its layout, padding, and size
    /// based on the chart height to improve visual fit and avoid overflow.
    /// Supports multiline wrapping for bottom legends.
    /// </summary>
    public class CustomLegend : SKDefaultLegend
    {
        private StackLayout _existingLayout;

        protected override Layout<SkiaSharpDrawingContext> GetLayout(Chart chart)
        {
            float chartHeight = GetChartHeight(chart);
            float chartWidth = GetChartWidth(chart);

            // Calculate dynamic padding based on chart height
            (float horizontalPadding, float verticalPadding) = CalculateDynamicPadding(chartHeight);

            // Determine legend position
            LegendPosition legendPosition = chart.LegendPosition;

            // For bottom legends, use a wrapping layout
            if (legendPosition == LegendPosition.Bottom)
            {
                return CreateWrappingLayout(chart, chartWidth, chartHeight, horizontalPadding, verticalPadding);
            }

            // For other positions, use vertical stack layout
            _existingLayout ??= new StackLayout
            {
                Orientation = ContainerOrientation.Vertical,
                HorizontalAlignment = Align.Start,
                VerticalAlignment = Align.Middle
            };

            _existingLayout.Children.Clear();
            _existingLayout.Padding = new Padding(horizontalPadding, verticalPadding + 50, horizontalPadding, verticalPadding);

            // Add fresh legend items
            foreach (ISeries series in chart.Series.Where(x => x.IsVisibleAtLegend))
            {
                _existingLayout.Children.Add(new CustomLegendItem(series, chartHeight));
            }

            return _existingLayout;
        }
        private static Layout<SkiaSharpDrawingContext> CreateWrappingLayout(Chart chart, float chartWidth, float chartHeight, float horizontalPadding, float verticalPadding)
        {
            // Create main vertical container for rows
            StackLayout mainLayout = new()
            {
                Orientation = ContainerOrientation.Vertical,
                HorizontalAlignment = Align.Middle,
                VerticalAlignment = Align.Start,
                Padding = new Padding(horizontalPadding, verticalPadding + 20, horizontalPadding, verticalPadding)
            };

            List<CustomLegendItem> legendItems = chart.Series
                .Where(x => x.IsVisibleAtLegend)
                .Select(series => new CustomLegendItem(series, chartHeight))
                .ToList();

            if (legendItems.Count == 0)
            {
                return mainLayout;
            }

            // Calculate available width for legend (leave some margin)
            float availableWidth = chartWidth - (horizontalPadding * 2) - 40;
            float currentRowWidth = 0;
            StackLayout currentRow = CreateNewRow();

            foreach (CustomLegendItem item in legendItems)
            {
                // Estimate item width (this is approximate)
                float estimatedItemWidth = EstimateLegendItemWidth(item, chartHeight);

                // Check if item fits in current row
                if (currentRow.Children.Count > 0 && currentRowWidth + estimatedItemWidth > availableWidth)
                {
                    // Start new row
                    mainLayout.Children.Add(currentRow);
                    currentRow = CreateNewRow();
                    currentRowWidth = 0;
                }

                currentRow.Children.Add(item);
                currentRowWidth += estimatedItemWidth;
            }

            // Add the last row
            if (currentRow.Children.Count > 0)
            {
                mainLayout.Children.Add(currentRow);
            }

            return mainLayout;
        }
        private static StackLayout CreateNewRow()
        {
            return new StackLayout
            {
                Orientation = ContainerOrientation.Horizontal,
                HorizontalAlignment = Align.Middle,
                VerticalAlignment = Align.Start
            };
        }
        private static float EstimateLegendItemWidth(CustomLegendItem item, float chartHeight)
        {
            // Estimate width based on text length and chart height
            // This is an approximation - actual width depends on font metrics
            (float bulletSize, float textSize, float textPadding) = CalculateDynamicSizes(chartHeight);

            // Rough estimation: bullet + padding + text width
            // Assume average character width is about 60% of text size
            string text = item.Children.OfType<LabelGeometry>().FirstOrDefault()?.Text ?? "";
            float estimatedTextWidth = text.Length * textSize * 0.6f;

            return bulletSize + textPadding + estimatedTextWidth + 20;  // Add some extra margin
        }
        public void ClearLayout()
        {
            _existingLayout?.Children.Clear();
            _existingLayout = null;
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
        /// Gets the chart width from the chart view control.
        /// </summary>
        private static float GetChartWidth(Chart chart)
        {
            // Try to get the chart view from the chart's view property
            if (chart.View != null)
            {
                // Access the actual chart control (CartesianChart, PieChart, etc.)
                if (chart.View is Control chartControl)
                {
                    return chartControl.Width;
                }
            }

            // Default width if we can't access the control
            return 600f;
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

        /// <summary>
        /// Calculates dynamic sizes for legend elements based on chart height.
        /// </summary>
        public static (float bulletSize, float textSize, float textPadding) CalculateDynamicSizes(float chartHeight)
        {
            // Define height thresholds
            const float minHeight = 200f;
            const float normalHeight = 400f;

            // Default sizes
            const float defaultBulletSize = 14f;
            const float defaultTextSize = 20f;
            const float defaultTextPadding = 10f;

            // Minimum sizes
            const float minBulletSize = 10f;
            const float minTextSize = 14f;

            if (chartHeight >= normalHeight)
            {
                return (defaultBulletSize, defaultTextSize, defaultTextPadding);
            }
            else if (chartHeight <= minHeight)
            {
                return (minBulletSize, minTextSize, defaultTextPadding);
            }
            else
            {
                // Linearly interpolate sizes based on height
                float ratio = (chartHeight - minHeight) / (normalHeight - minHeight);
                float bulletSize = minBulletSize + (defaultBulletSize - minBulletSize) * ratio;
                float textSize = minTextSize + (defaultTextSize - minTextSize) * ratio;

                return (bulletSize, textSize, defaultTextPadding);
            }
        }
    }
}