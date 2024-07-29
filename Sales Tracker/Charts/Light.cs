using Guna.Charts.WinForms;

namespace Sales_Tracker.Charts
{
    internal class Light
    {
        public static ChartConfig Config()
        {
            Color gridColor = Color.FromArgb(214, 219, 224);
            Color foreColor = Color.FromArgb(105, 121, 139);

            var font = new ChartFont()
            {
                FontName = "Segoe UI",
                Size = 10,
                Style = ChartFontStyle.Normal
            };

            ChartConfig config = new()
            {
                Title =
                {
                    ForeColor = foreColor
                },
                Legend =
                {
                    LabelFont = font,
                    LabelForeColor = foreColor
                },
                XAxes =
                {
                    GridLines =
                    {
                        Color = gridColor,
                        ZeroLineColor = gridColor
                    },
                    Ticks =
                    {
                        Font = font,
                        ForeColor = foreColor
                    }
                },
                YAxes =
                {
                    GridLines =
                    {
                        Color = gridColor,
                        ZeroLineColor = gridColor
                    },
                    Ticks =
                    {
                        Font = font,
                        ForeColor = foreColor
                    }
                },
                ZAxes =
                {
                    GridLines =
                    {
                        Color = gridColor,
                        ZeroLineColor = gridColor
                    },
                    Ticks =
                    {
                        Font = font,
                        ForeColor = foreColor
                    },
                    PointLabels =
                    {
                        Font = font,
                        ForeColor = foreColor
                    }
                },
            };
            return config;
        }
    }
}