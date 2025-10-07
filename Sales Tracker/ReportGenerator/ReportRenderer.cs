using Sales_Tracker.Classes;
using Sales_Tracker.ReportGenerator.Elements;
using SkiaSharp;
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
            Size pageSize = PageDimensions.GetDimensions(_config.PageSize, _config.PageOrientation);

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
            if (_exportSettings.Format == ExportFormat.PDF)
            {
                ExportToPdf();
            }
            else
            {
                ExportToImage();
            }
        }

        // Private export methods
        private void ExportToImage()
        {
            Size pageSize = PageDimensions.GetDimensions(_config.PageSize, _config.PageOrientation);

            // Calculate dimensions based on DPI
            float scaleFactor = _exportSettings.DPI / 96f;  // 96 DPI is standard
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
        private void ExportToPdf()
        {
            Size pageSize = PageDimensions.GetDimensions(_config.PageSize, _config.PageOrientation);

            string? directory = Path.GetDirectoryName(_exportSettings.FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileName = Directories.GetNewFileNameIfItAlreadyExists(_exportSettings.FilePath);

            // Create PDF document using SkiaSharp
            using SKDocument document = SKDocument.CreatePdf(fileName);
            using SKCanvas canvas = document.BeginPage(pageSize.Width, pageSize.Height);

            // Create a Bitmap to render GDI+ content
            using Bitmap bitmap = new(pageSize.Width, pageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                SetupGraphics(graphics);
                RenderReport(graphics, pageSize);
            }

            // Convert System.Drawing.Bitmap to SKBitmap
            using SKBitmap skBitmap = BitmapToSKBitmap(bitmap);

            // Draw the bitmap onto the PDF canvas
            canvas.DrawBitmap(skBitmap, 0, 0);

            document.EndPage();
            document.Close();
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

            // Render all report elements using their own render methods
            List<BaseElement> elements = _config.GetElementsByZOrder();
            foreach (BaseElement element in elements.Where(e => e.IsVisible))
            {
                element.RenderElement(graphics, _config);
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
            using (SolidBrush titleBrush = new(Color.Black))
            {
                StringFormat titleFormat = new()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString(_config.Title, titleFont, titleBrush, headerRect, titleFormat);
            }

            // Draw separator line
            using Pen pen = new(Color.LightGray, 1);
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
            graphics.DrawString($"Generated: {DateTime.Now:yyyy-MM-dd hh:mm tt}",
                footerFont, footerBrush, footerRect, leftFormat);

            // Right side - page number (if enabled)
            if (_config.ShowPageNumbers)
            {
                StringFormat rightFormat = new() { Alignment = StringAlignment.Far };
                graphics.DrawString($"Page {_config.CurrentPageNumber}", footerFont, footerBrush, footerRect, rightFormat);
            }
        }
        private void SaveBitmap(Bitmap bitmap)
        {
            string? directory = Path.GetDirectoryName(_exportSettings.FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileName = Directories.GetNewFileNameIfItAlreadyExists(_exportSettings.FilePath);

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
                bitmap.Save(fileName, jpegCodec, encoderParams);
            }
            else
            {
                bitmap.Save(fileName, format);
            }
        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Find the codec matching the mimeType
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo? codec = codecs.FirstOrDefault(c => c.MimeType == mimeType);

            return codec ?? throw new ArgumentException($"Encoder for MIME type '{mimeType}' not found.");
        }
        private static SKBitmap BitmapToSKBitmap(Bitmap bitmap)
        {
            SKBitmap skBitmap = new(bitmap.Width, bitmap.Height);

            using (MemoryStream stream = new())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                using SKImage skImage = SKImage.FromEncodedData(stream);
                skImage.ScalePixels(skBitmap.PeekPixels(), new SKSamplingOptions(SKCubicResampler.Mitchell));
            }

            return skBitmap;
        }
    }
}