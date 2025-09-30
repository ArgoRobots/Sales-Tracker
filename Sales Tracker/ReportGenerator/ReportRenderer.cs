using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Handles the rendering of reports from ReportConfiguration to various output formats.
    /// </summary>
    public class ReportRenderer(ReportConfiguration config, ExportSettings exportSettings)
    {
        // Properties
        private readonly ReportConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly ExportSettings _exportSettings = exportSettings ?? throw new ArgumentNullException(nameof(exportSettings));

        // Init.
        /// <summary>
        /// Renders the report to a Bitmap for preview purposes.
        /// </summary>
        public Bitmap RenderToPreview(int maxWidth = 800, int maxHeight = 600)
        {
            Size pageSize = PageDimensions.GetDimensions(_config.PageSize, _config.Orientation);

            // Calculate preview scale to fit within max dimensions
            float scaleX = (float)maxWidth / pageSize.Width;
            float scaleY = (float)maxHeight / pageSize.Height;
            float scale = Math.Min(scaleX, scaleY);

            int previewWidth = (int)(pageSize.Width * scale);
            int previewHeight = (int)(pageSize.Height * scale);

            Bitmap bitmap = new(previewWidth, previewHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                SetupGraphics(graphics);
                graphics.ScaleTransform(scale, scale);
                RenderReport(graphics, pageSize);
            }

            return bitmap;
        }

        /// <summary>
        /// Renders and exports the report to the specified file.
        /// </summary>
        public void ExportReport()
        {
            Size pageSize = PageDimensions.GetDimensions(_config.PageSize, _config.Orientation);

            // Calculate dimensions based on DPI
            float scaleFactor = _exportSettings.DPI / 96f; // 96 DPI is standard
            int exportWidth = (int)(pageSize.Width * scaleFactor);
            int exportHeight = (int)(pageSize.Height * scaleFactor);

            using Bitmap bitmap = new(exportWidth, exportHeight);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                SetupGraphics(graphics);
                graphics.ScaleTransform(scaleFactor, scaleFactor);
                RenderReport(graphics, pageSize);
            }

            SaveBitmap(bitmap);
        }

        // Private rendering methods
        private static void SetupGraphics(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }
        private void RenderReport(Graphics graphics, Size pageSize)
        {
            // Clear background
            graphics.Clear(_config.BackgroundColor);

            // Render header if enabled
            if (_config.ShowHeader)
            {
                RenderHeader(graphics, pageSize);
            }

            // Render footer if enabled
            if (_config.ShowFooter)
            {
                RenderFooter(graphics, pageSize);
            }

            // Render all report elements
            List<ReportElement> elements = _config.GetElementsByZOrder();
            foreach (ReportElement? element in elements.Where(e => e.IsVisible))
            {
                RenderElement(graphics, element);
            }
        }
        private void RenderHeader(Graphics graphics, Size pageSize)
        {
            Rectangle headerRect = new(
                _config.PageMargins.Left,
                _config.PageMargins.Top,
                pageSize.Width - _config.PageMargins.Left - _config.PageMargins.Right,
                50
            );

            using (Font titleFont = new("Segoe UI", 18, FontStyle.Bold))
            using (SolidBrush titleBrush = new(CustomColors.Text))
            {
                StringFormat titleFormat = new()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString(_config.Title, titleFont, titleBrush, headerRect, titleFormat);
            }

            // Draw separator line
            using Pen pen = new(CustomColors.ControlBorder, 1);
            graphics.DrawLine(pen,
                headerRect.Left, headerRect.Bottom + 5,
                headerRect.Right, headerRect.Bottom + 5);
        }
        private void RenderFooter(Graphics graphics, Size pageSize)
        {
            Rectangle footerRect = new(
                _config.PageMargins.Left,
                pageSize.Height - _config.PageMargins.Bottom - 30,
                pageSize.Width - _config.PageMargins.Left - _config.PageMargins.Right,
                30
            );

            using Font footerFont = new("Segoe UI", 9);
            using SolidBrush footerBrush = new(Color.Gray);
            // Left side - generation date
            StringFormat leftFormat = new() { Alignment = StringAlignment.Near };
            graphics.DrawString($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}",
                footerFont, footerBrush, footerRect, leftFormat);

            // Right side - page number (if enabled)
            if (_config.ShowPageNumbers)
            {
                StringFormat rightFormat = new() { Alignment = StringAlignment.Far };
                graphics.DrawString("Page 1", footerFont, footerBrush, footerRect, rightFormat);
            }
        }
        private void RenderElement(Graphics graphics, ReportElement element)
        {
            switch (element.Type)
            {
                case ReportElementType.Chart:
                    RenderChartElement(graphics, element);
                    break;

                case ReportElementType.TransactionTable:
                    RenderTableElement(graphics, element);
                    break;

                case ReportElementType.TextLabel:
                    RenderTextElement(graphics, element);
                    break;

                case ReportElementType.Image:
                    RenderImageElement(graphics, element);
                    break;

                case ReportElementType.DateRange:
                    RenderDateRangeElement(graphics, element);
                    break;

                case ReportElementType.Summary:
                    RenderSummaryElement(graphics, element);
                    break;
            }
        }
        private static void RenderChartElement(Graphics graphics, ReportElement element)
        {
            try
            {
                // Get the chart image from the chart data
                if (element.Data is MainMenu_Form.ChartDataType chartType)
                {
                    // Get chart image from existing charts
                    Control chartControl = GetChartControlForType(chartType);

                    if (chartControl != null)
                    {
                        // Create a bitmap of the chart at its original size
                        Bitmap originalChart = new(chartControl.Width, chartControl.Height);
                        chartControl.DrawToBitmap(originalChart, new Rectangle(0, 0, chartControl.Width, chartControl.Height));

                        // Draw the chart scaled to fit the element bounds
                        graphics.DrawImage(originalChart, element.Bounds);

                        // Clean up
                        originalChart.Dispose();
                    }
                    else
                    {
                        // Fallback to placeholder if chart control not found
                        RenderPlaceholder(graphics, element.Bounds, $"Chart: {chartType}", Color.LightBlue);
                    }
                }
                else if (element.Data is Image directImage)
                {
                    // Scale the direct image to fit the element bounds
                    graphics.DrawImage(directImage, element.Bounds);
                }
            }
            catch (Exception ex)
            {
                // Draw error placeholder
                RenderErrorPlaceholder(graphics, element.Bounds, $"Chart Error: {ex.Message}");
            }
        }
        private static void RenderTableElement(Graphics graphics, ReportElement element)
        {
            try
            {
                // This would render transaction data in table format
                // For now, render a placeholder
                RenderPlaceholder(graphics, element.Bounds, "Transaction Table", Color.LightBlue);
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, element.Bounds, $"Table Error: {ex.Message}");
            }
        }
        private static void RenderTextElement(Graphics graphics, ReportElement element)
        {
            try
            {
                string text = element.Data?.ToString() ?? element.DisplayName ?? "Text";
                float fontSize = element.Properties.TryGetValue("FontSize", out object? value) ?
                    Convert.ToSingle(value) : 12f;
                FontStyle fontStyle = element.Properties.TryGetValue("FontStyle", out object? value1) ?
                    (FontStyle)value1 : FontStyle.Regular;

                using Font font = new("Segoe UI", fontSize, fontStyle);
                using SolidBrush brush = new(CustomColors.Text);
                StringFormat format = new()
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.LineLimit
                };

                graphics.DrawString(text, font, brush, element.Bounds, format);
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, element.Bounds, $"Text Error: {ex.Message}");
            }
        }
        private static void RenderImageElement(Graphics graphics, ReportElement element)
        {
            try
            {
                if (element.Data is Image directImage)
                {
                    graphics.DrawImage(directImage, element.Bounds);
                }
                else if (element.Data is string imagePath && File.Exists(imagePath))
                {
                    using Image imageFromFile = Image.FromFile(imagePath);
                    graphics.DrawImage(imageFromFile, element.Bounds);
                }
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, element.Bounds, $"Image Error: {ex.Message}");
            }
        }
        private void RenderDateRangeElement(Graphics graphics, ReportElement element)
        {
            string dateText = $"Period: {_config.Filters.StartDate:yyyy-MM-dd} to {_config.Filters.EndDate:yyyy-MM-dd}";

            using Font font = new("Segoe UI", 10, FontStyle.Italic);
            using SolidBrush brush = new(Color.Gray);
            graphics.DrawString(dateText, font, brush, element.Bounds);
        }
        private static void RenderSummaryElement(Graphics graphics, ReportElement element)
        {
            // Render summary statistics
            RenderPlaceholder(graphics, element.Bounds, "Summary Statistics", Color.LightGreen);
        }
        private static void RenderPlaceholder(Graphics graphics, Rectangle bounds, string text, Color backgroundColor)
        {
            using SolidBrush brush = new(backgroundColor);
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);
            graphics.FillRectangle(brush, bounds);
            graphics.DrawRectangle(pen, bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(text, font, textBrush, bounds, format);
        }
        private static void RenderErrorPlaceholder(Graphics graphics, Rectangle bounds, string errorMessage)
        {
            using SolidBrush brush = new(Color.LightPink);
            using Pen pen = new(Color.Red, 2);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.DarkRed);
            graphics.FillRectangle(brush, bounds);
            graphics.DrawRectangle(pen, bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(errorMessage, font, textBrush, bounds, format);
        }

        // Helper methods
        /// <summary>
        /// Gets the appropriate chart control for the specified chart type.
        /// </summary>
        private static Control? GetChartControlForType(MainMenu_Form.ChartDataType chartType)
        {
            MainMenu_Form mainForm = MainMenu_Form.Instance;

            switch (chartType)
            {
                case MainMenu_Form.ChartDataType.TotalSales:
                    return mainForm.TotalSales_Chart;

                case MainMenu_Form.ChartDataType.TotalPurchases:
                    return mainForm.TotalPurchases_Chart;

                case MainMenu_Form.ChartDataType.DistributionOfSales:
                    return mainForm.DistributionOfSales_Chart;

                case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                    return mainForm.TotalExpensesVsSales_Chart;

                case MainMenu_Form.ChartDataType.GrowthRates:
                    return mainForm.GrowthRates_Chart;

                case MainMenu_Form.ChartDataType.AverageTransactionValue:
                    return mainForm.AverageTransactionValue_Chart;

                case MainMenu_Form.ChartDataType.Profits:
                    return mainForm.Profits_Chart;

                case MainMenu_Form.ChartDataType.TotalTransactions:
                    return mainForm.TotalTransactions_Chart;

                case MainMenu_Form.ChartDataType.ReturnsOverTime:
                    return mainForm.ReturnsOverTime_Chart;

                case MainMenu_Form.ChartDataType.ReturnReasons:
                    return mainForm.ReturnReasons_Chart;

                case MainMenu_Form.ChartDataType.ReturnFinancialImpact:
                    return mainForm.ReturnFinancialImpact_Chart;

                case MainMenu_Form.ChartDataType.LossesOverTime:
                    return mainForm.LossesOverTime_Chart;

                case MainMenu_Form.ChartDataType.LossReasons:
                    return mainForm.LossReasons_Chart;

                case MainMenu_Form.ChartDataType.WorldMap:
                    return mainForm.WorldMap_GeoMap;

                case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                    return mainForm.CountriesOfOrigin_Chart;

                case MainMenu_Form.ChartDataType.CountriesOfDestination:
                    return mainForm.CountriesOfDestination_Chart;

                case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                    return mainForm.CompaniesOfOrigin_Chart;

                default:
                    Log.Write(0, $"Unknown chart type: {chartType}");
                    return null;
            }
        }
        private void SaveBitmap(Bitmap bitmap)
        {
            string? directory = Path.GetDirectoryName(_exportSettings.FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            ImageFormat format = _exportSettings.Format switch
            {
                ExportFormat.PNG => ImageFormat.Png,
                ExportFormat.JPG => ImageFormat.Jpeg,
                _ => ImageFormat.Png
            };

            if (_exportSettings.Format == ExportFormat.JPG && _exportSettings.Quality < 100)
            {
                // Use quality setting for JPEG
                EncoderParameters encoderParams = new(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, _exportSettings.Quality);
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                bitmap.Save(_exportSettings.FilePath, jpegCodec, encoderParams);
            }
            else
            {
                bitmap.Save(_exportSettings.FilePath, format);
            }
        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Find the codec matching the mimeType
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo? codec = codecs.FirstOrDefault(c => c.MimeType == mimeType);

            return codec ?? throw new ArgumentException($"Encoder for MIME type '{mimeType}' not found.");
        }
    }
}