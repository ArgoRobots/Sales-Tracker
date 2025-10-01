namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Chart element for displaying graphs and charts.
    /// </summary>
    public class ChartElement : BaseElement
    {
        public MainMenu_Form.ChartDataType ChartType { get; set; } = MainMenu_Form.ChartDataType.TotalSales;
        public bool ShowLegend { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public Color BorderColor { get; set; } = Color.Gray;

        public override ReportElementType GetElementType() => ReportElementType.Chart;
        public override BaseElement Clone()
        {
            return new ChartElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ChartType = ChartType,
                ShowLegend = ShowLegend,
                ShowTitle = ShowTitle,
                BorderColor = BorderColor
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                // Get the chart control from the main form
                Control chartControl = GetChartControl();

                if (chartControl != null)
                {
                    // Create a bitmap of the chart at its original size
                    Bitmap chartImage = new(chartControl.Width, chartControl.Height);
                    chartControl.DrawToBitmap(chartImage, new Rectangle(0, 0, chartControl.Width, chartControl.Height));

                    // Draw the chart scaled to fit the element bounds
                    graphics.DrawImage(chartImage, Bounds);

                    // Draw border if needed
                    if (BorderColor != Color.Transparent)
                    {
                        using Pen borderPen = new(BorderColor, 1);
                        graphics.DrawRectangle(borderPen, Bounds);
                    }

                    chartImage.Dispose();
                }
                else
                {
                    RenderPlaceholder(graphics, $"Chart: {ChartType}");
                }
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, $"Chart Error: {ex.Message}");
            }
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.LightBlue);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Black);
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            string text = DisplayName ?? ChartType.ToString();
            graphics.DrawString(text, font, textBrush, Bounds, format);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            const int rowHeight = 35;

            // Chart type selector
            AddPropertyLabel(container, "Chart:", yPosition);
            AddPropertyComboBox(container, ChartType.ToString(), yPosition,
                GetAvailableChartTypes(),
                value =>
                {
                    ChartType = Enum.Parse<MainMenu_Form.ChartDataType>(value);
                    onPropertyChanged();
                });
            yPosition += rowHeight;

            // Show legend checkbox
            AddPropertyLabel(container, "Legend:", yPosition);
            CheckBox legendCheck = new()
            {
                Checked = ShowLegend,
                Location = new Point(85, yPosition),
                Size = new Size(20, 20)
            };
            legendCheck.CheckedChanged += (s, e) =>
            {
                ShowLegend = legendCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(legendCheck);
            yPosition += rowHeight;

            // Show title checkbox
            AddPropertyLabel(container, "Title:", yPosition);
            CheckBox titleCheck = new()
            {
                Checked = ShowTitle,
                Location = new Point(85, yPosition),
                Size = new Size(20, 20)
            };
            titleCheck.CheckedChanged += (s, e) =>
            {
                ShowTitle = titleCheck.Checked;
                onPropertyChanged();
            };
            container.Controls.Add(titleCheck);
            yPosition += rowHeight;

            return yPosition;
        }
        private Control? GetChartControl()
        {
            MainMenu_Form mainForm = MainMenu_Form.Instance;

            return ChartType switch
            {
                MainMenu_Form.ChartDataType.TotalSales => mainForm.TotalSales_Chart,
                MainMenu_Form.ChartDataType.TotalPurchases => mainForm.TotalPurchases_Chart,
                MainMenu_Form.ChartDataType.DistributionOfSales => mainForm.DistributionOfSales_Chart,
                MainMenu_Form.ChartDataType.DistributionOfPurchases => mainForm.DistributionOfPurchases_Chart,
                MainMenu_Form.ChartDataType.Profits => mainForm.Profits_Chart,
                MainMenu_Form.ChartDataType.CountriesOfOrigin => mainForm.CountriesOfOrigin_Chart,
                MainMenu_Form.ChartDataType.CompaniesOfOrigin => mainForm.CompaniesOfOrigin_Chart,
                MainMenu_Form.ChartDataType.CountriesOfDestination => mainForm.CountriesOfDestination_Chart,
                MainMenu_Form.ChartDataType.Accountants => mainForm.Accountants_Chart,
                MainMenu_Form.ChartDataType.TotalExpensesVsSales => mainForm.TotalExpensesVsSales_Chart,
                MainMenu_Form.ChartDataType.AverageTransactionValue => mainForm.AverageTransactionValue_Chart,
                MainMenu_Form.ChartDataType.TotalTransactions => mainForm.TotalTransactions_Chart,
                MainMenu_Form.ChartDataType.AverageShippingCosts => mainForm.AverageShippingCosts_Chart,
                MainMenu_Form.ChartDataType.GrowthRates => mainForm.GrowthRates_Chart,
                MainMenu_Form.ChartDataType.ReturnsOverTime => mainForm.ReturnsOverTime_Chart,
                MainMenu_Form.ChartDataType.ReturnReasons => mainForm.ReturnReasons_Chart,
                MainMenu_Form.ChartDataType.ReturnFinancialImpact => mainForm.ReturnFinancialImpact_Chart,
                MainMenu_Form.ChartDataType.ReturnsByCategory => mainForm.ReturnsByCategory_Chart,
                MainMenu_Form.ChartDataType.ReturnsByProduct => mainForm.ReturnsByProduct_Chart,
                MainMenu_Form.ChartDataType.PurchaseVsSaleReturns => mainForm.PurchaseVsSaleReturns_Chart,
                MainMenu_Form.ChartDataType.WorldMap => mainForm.WorldMap_GeoMap,
                MainMenu_Form.ChartDataType.LossesOverTime => mainForm.LossesOverTime_Chart,
                MainMenu_Form.ChartDataType.LossReasons => mainForm.LossReasons_Chart,
                MainMenu_Form.ChartDataType.LossFinancialImpact => mainForm.LossFinancialImpact_Chart,
                MainMenu_Form.ChartDataType.LossesByCategory => mainForm.LossesByCategory_Chart,
                MainMenu_Form.ChartDataType.LossesByProduct => mainForm.LossesByProduct_Chart,
                MainMenu_Form.ChartDataType.PurchaseVsSaleLosses => mainForm.PurchaseVsSaleLosses_Chart,
                _ => null
            };
        }
        private static string[] GetAvailableChartTypes()
        {
            return Enum.GetNames(typeof(MainMenu_Form.ChartDataType));
        }
        private void RenderPlaceholder(Graphics graphics, string text)
        {
            using SolidBrush brush = new(Color.LightBlue);
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(text, font, textBrush, Bounds, format);
        }
        private void RenderErrorPlaceholder(Graphics graphics, string errorMessage)
        {
            using SolidBrush brush = new(Color.LightPink);
            using Pen pen = new(Color.Red, 2);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.DarkRed);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(errorMessage, font, textBrush, Bounds, format);
        }
        protected override Color GetDesignerColor() => Color.LightBlue;
    }
}