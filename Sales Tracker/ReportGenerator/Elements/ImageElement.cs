using Guna.UI2.WinForms;
using SkiaSharp;
using Svg.Skia;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Image scaling modes for display.
    /// </summary>
    public enum ImageScaleMode
    {
        Stretch,  // Stretch to fill bounds (may distort)
        Fit,      // Fit within bounds (maintain aspect ratio)
        Fill,     // Fill bounds (maintain aspect ratio, may crop)
        Center,   // Center at original size
        Tile      // Tile the image
    }

    /// <summary>
    /// Image element for displaying PNG, JPEG, and SVG images.
    /// </summary>
    public class ImageElement : BaseElement
    {
        // Properties
        public string ImagePath { get; set; } = "";
        public ImageScaleMode ScaleMode { get; set; } = ImageScaleMode.Fit;
        public Color BackgroundColor { get; set; } = Color.Transparent;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderThickness { get; set; } = 0;
        public bool MaintainAspectRatio { get; set; } = true;
        public int CornerRadius { get; set; } = 0;
        public byte Opacity { get; set; } = 255;

        // Cached image data
        private Image _cachedImage;
        private SKSvg _cachedSvg;
        private string _cachedImagePath;

        // Constructor
        public ImageElement()
        {
            DisplayName = "Image";
        }

        // Overrides
        public override ReportElementType GetElementType() => ReportElementType.Image;
        public override BaseElement Clone()
        {
            return new ImageElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                ImagePath = ImagePath,
                ScaleMode = ScaleMode,
                BackgroundColor = BackgroundColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                MaintainAspectRatio = MaintainAspectRatio,
                CornerRadius = CornerRadius,
                Opacity = Opacity
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                // Draw background if not transparent
                if (BackgroundColor != Color.Transparent)
                {
                    using SolidBrush bgBrush = new(BackgroundColor);
                    if (CornerRadius > 0)
                    {
                        using GraphicsPath path = GetRoundedRectanglePath(Bounds, CornerRadius);
                        graphics.FillPath(bgBrush, path);
                    }
                    else
                    {
                        graphics.FillRectangle(bgBrush, Bounds);
                    }
                }

                // Load and render image
                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    string extension = Path.GetExtension(ImagePath).ToLowerInvariant();

                    if (extension == ".svg")
                    {
                        RenderSvgImage(graphics);
                    }
                    else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                    {
                        RenderBitmapImage(graphics);
                    }
                }
                else
                {
                    RenderPlaceholder(graphics);
                }

                // Draw border if specified
                if (BorderColor != Color.Transparent && BorderThickness > 0)
                {
                    using Pen borderPen = new(BorderColor, BorderThickness);
                    if (CornerRadius > 0)
                    {
                        using GraphicsPath path = GetRoundedRectanglePath(Bounds, CornerRadius);
                        graphics.DrawPath(borderPen, path);
                    }
                    else
                    {
                        graphics.DrawRectangle(borderPen, Bounds);
                    }
                }
            }
            catch (Exception ex)
            {
                RenderErrorPlaceholder(graphics, $"Error: {ex.Message}");
            }
        }

        private void RenderBitmapImage(Graphics graphics)
        {
            // Load image if not cached or path changed
            if (_cachedImage == null || _cachedImagePath != ImagePath)
            {
                _cachedImage?.Dispose();
                _cachedImage = Image.FromFile(ImagePath);
                _cachedImagePath = ImagePath;
            }

            if (_cachedImage == null) return;

            // Save graphics state
            GraphicsState state = graphics.Save();

            try
            {
                // Set up clipping if using rounded corners
                if (CornerRadius > 0)
                {
                    using GraphicsPath path = GetRoundedRectanglePath(Bounds, CornerRadius);
                    graphics.SetClip(path);
                }

                // Apply opacity
                if (Opacity < 255)
                {
                    ColorMatrix colorMatrix = new()
                    {
                        Matrix33 = Opacity / 255f
                    };
                    ImageAttributes imageAttributes = new();
                    imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    Rectangle destRect = CalculateImageBounds();
                    graphics.DrawImage(_cachedImage, destRect, 0, 0, _cachedImage.Width, _cachedImage.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                else
                {
                    Rectangle destRect = CalculateImageBounds();
                    graphics.DrawImage(_cachedImage, destRect);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }
        private void RenderSvgImage(Graphics graphics)
        {
            // Load SVG if not cached or path changed
            if (_cachedSvg == null || _cachedImagePath != ImagePath)
            {
                _cachedSvg = new SKSvg();
                _cachedSvg.Load(ImagePath);
                _cachedImagePath = ImagePath;
            }

            if (_cachedSvg?.Picture == null) return;

            // Save graphics state
            GraphicsState state = graphics.Save();

            try
            {
                // Set up clipping if using rounded corners
                if (CornerRadius > 0)
                {
                    using GraphicsPath path = GetRoundedRectanglePath(Bounds, CornerRadius);
                    graphics.SetClip(path);
                }

                // Calculate scale and position for SVG
                Rectangle destRect = CalculateSvgBounds();

                // Create a bitmap to render the SVG
                using SKBitmap bitmap = new(destRect.Width, destRect.Height);
                using SKCanvas canvas = new(bitmap);

                canvas.Clear(SKColors.Transparent);

                // Calculate scale to fit SVG in destination rectangle
                float scaleX = destRect.Width / _cachedSvg.Picture.CullRect.Width;
                float scaleY = destRect.Height / _cachedSvg.Picture.CullRect.Height;

                float scale = ScaleMode switch
                {
                    ImageScaleMode.Fit => Math.Min(scaleX, scaleY),
                    ImageScaleMode.Fill => Math.Max(scaleX, scaleY),
                    ImageScaleMode.Stretch => 1f,
                    _ => Math.Min(scaleX, scaleY)
                };

                if (ScaleMode == ImageScaleMode.Stretch)
                {
                    canvas.Scale(scaleX, scaleY);
                }
                else
                {
                    canvas.Scale(scale, scale);
                }

                // Center the SVG in the destination rectangle
                float offsetX = (destRect.Width / scale - _cachedSvg.Picture.CullRect.Width) / 2;
                float offsetY = (destRect.Height / scale - _cachedSvg.Picture.CullRect.Height) / 2;
                canvas.Translate(offsetX, offsetY);

                // Draw SVG
                if (Opacity < 255)
                {
                    using SKPaint paint = new();
                    paint.Color = new SKColor(255, 255, 255, Opacity);
                    canvas.DrawPicture(_cachedSvg.Picture, paint);
                }
                else
                {
                    canvas.DrawPicture(_cachedSvg.Picture);
                }

                canvas.Flush();

                // Convert SKBitmap to GDI+ Image
                using SKImage skImage = SKImage.FromBitmap(bitmap);
                using SKData data = skImage.Encode();
                using MemoryStream stream = new(data.ToArray());
                using Image renderedImage = Image.FromStream(stream);

                graphics.DrawImage(renderedImage, destRect);
            }
            finally
            {
                graphics.Restore(state);
            }
        }
        private Rectangle CalculateImageBounds()
        {
            if (_cachedImage == null || ScaleMode == ImageScaleMode.Stretch)
            {
                return Bounds;
            }

            float imageAspect = (float)_cachedImage.Width / _cachedImage.Height;
            float boundsAspect = (float)Bounds.Width / Bounds.Height;

            return ScaleMode switch
            {
                ImageScaleMode.Fit => CalculateFitBounds(imageAspect, boundsAspect),
                ImageScaleMode.Fill => CalculateFillBounds(imageAspect, boundsAspect),
                ImageScaleMode.Center => CalculateCenterBounds(),
                ImageScaleMode.Tile => Bounds,
                _ => Bounds
            };
        }
        private Rectangle CalculateSvgBounds()
        {
            if (_cachedSvg?.Picture == null || ScaleMode == ImageScaleMode.Stretch)
            {
                return Bounds;
            }

            float imageAspect = _cachedSvg.Picture.CullRect.Width / _cachedSvg.Picture.CullRect.Height;
            float boundsAspect = (float)Bounds.Width / Bounds.Height;

            return ScaleMode switch
            {
                ImageScaleMode.Fit => CalculateFitBounds(imageAspect, boundsAspect),
                ImageScaleMode.Fill => CalculateFillBounds(imageAspect, boundsAspect),
                ImageScaleMode.Center => CalculateCenterBounds(),
                _ => Bounds
            };
        }
        private Rectangle CalculateFitBounds(float imageAspect, float boundsAspect)
        {
            int width, height;

            if (imageAspect > boundsAspect)
            {
                // Image is wider than bounds
                width = Bounds.Width;
                height = (int)(Bounds.Width / imageAspect);
            }
            else
            {
                // Image is taller than bounds
                width = (int)(Bounds.Height * imageAspect);
                height = Bounds.Height;
            }

            int x = Bounds.X + (Bounds.Width - width) / 2;
            int y = Bounds.Y + (Bounds.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }
        private Rectangle CalculateFillBounds(float imageAspect, float boundsAspect)
        {
            int width, height;

            if (imageAspect > boundsAspect)
            {
                // Image is wider than bounds
                width = (int)(Bounds.Height * imageAspect);
                height = Bounds.Height;
            }
            else
            {
                // Image is taller than bounds
                width = Bounds.Width;
                height = (int)(Bounds.Width / imageAspect);
            }

            int x = Bounds.X + (Bounds.Width - width) / 2;
            int y = Bounds.Y + (Bounds.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }
        private Rectangle CalculateCenterBounds()
        {
            if (_cachedImage == null) return Bounds;

            int x = Bounds.X + (Bounds.Width - _cachedImage.Width) / 2;
            int y = Bounds.Y + (Bounds.Height - _cachedImage.Height) / 2;

            return new Rectangle(x, y, _cachedImage.Width, _cachedImage.Height);
        }
        private static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
        private void RenderPlaceholder(Graphics graphics)
        {
            using SolidBrush brush = new(Color.FromArgb(240, 240, 240));
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Gray);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            string message = string.IsNullOrEmpty(ImagePath)
                ? "No image selected"
                : "Image not found";

            graphics.DrawString(message, font, textBrush, Bounds, format);
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
        public override void DrawDesignerElement(Graphics graphics)
        {
            // Draw same as RenderElement for consistency
            RenderElement(graphics, null);

            // Add a subtle overlay to indicate it's in designer mode
            using SolidBrush overlayBrush = new(Color.FromArgb(10, 0, 0, 255));
            graphics.FillRectangle(overlayBrush, Bounds);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Image path
            AddPropertyLabel(container, "Image:", yPosition);

            // Browse button
            Guna2Button browseButton = new()
            {
                Text = "Browse...",
                Size = new Size(container.Width - 95, 45),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                BorderThickness = 1,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            browseButton.Click += (s, e) =>
            {
                using OpenFileDialog openDialog = new();
                openDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.svg|PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|SVG Files|*.svg|All Files|*.*";
                openDialog.Title = "Select Image";

                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    openDialog.InitialDirectory = Path.GetDirectoryName(ImagePath);
                }

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    ImagePath = openDialog.FileName;
                    _cachedImage?.Dispose();
                    _cachedImage = null;
                    _cachedSvg = null;
                    _cachedImagePath = null;
                    onPropertyChanged();
                }
            };

            container.Controls.Add(browseButton);
            yPosition += ControlRowHeight;

            // Display current path
            if (!string.IsNullOrEmpty(ImagePath))
            {
                Label pathLabel = new()
                {
                    Text = Path.GetFileName(ImagePath),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.Gray,
                    Location = new Point(85, yPosition),
                    AutoSize = true
                };
                container.Controls.Add(pathLabel);
                yPosition += 35;
            }

            // Scale mode
            AddPropertyLabel(container, "Scale:", yPosition);
            AddPropertyComboBox(container, ScaleMode.ToString(), yPosition,
                Enum.GetNames<ImageScaleMode>(),
                value =>
                {
                    ScaleMode = Enum.Parse<ImageScaleMode>(value);
                    onPropertyChanged();
                });
            yPosition += ControlRowHeight;

            // Opacity
            AddPropertyLabel(container, "Opacity:", yPosition);
            Guna2NumericUpDown guna2NumericUpDown = AddPropertyNumericUpDown(container, Opacity, yPosition, value =>
            {
                Opacity = (byte)value;
                onPropertyChanged();
            }, 0, 255);
            guna2NumericUpDown.Left = 170;
            yPosition += ControlRowHeight;

            // Border thickness
            AddPropertyLabel(container, "Border thickness:", yPosition);
            guna2NumericUpDown = AddPropertyNumericUpDown(container, BorderThickness, yPosition, value =>
              {
                  BorderThickness = (int)value;
                  onPropertyChanged();
              }, 0, 20);
            guna2NumericUpDown.Left = 170;
            yPosition += ControlRowHeight;

            // Corner radius
            AddPropertyLabel(container, "Radius:", yPosition);
            guna2NumericUpDown = AddPropertyNumericUpDown(container, CornerRadius, yPosition, value =>
            {
                CornerRadius = (int)value;
                onPropertyChanged();
            }, 0, 50);
            guna2NumericUpDown.Left = 170;
            yPosition += ControlRowHeight;

            // Border color
            AddPropertyLabel(container, "Border Color:", yPosition);
            AddColorPicker(container, yPosition, BorderColor, color =>
               {
                   BorderColor = color;
                   onPropertyChanged();
               });
            guna2NumericUpDown.Left = 170;
            yPosition += ControlRowHeight;

            // Background color
            AddPropertyLabel(container, "Background color:", yPosition);
            AddColorPicker(container, yPosition, BackgroundColor, color =>
            {
                BackgroundColor = color;
                onPropertyChanged();
            });
            yPosition += ControlRowHeight;

            return yPosition;
        }
        private static void AddColorPicker(Panel container, int yPosition, Color currentColor, Action<Color> onColorChanged)
        {
            Panel colorPreview = new()
            {
                BackColor = currentColor,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(50, 30),
                Location = new Point(170, yPosition + 8),
                Cursor = Cursors.Hand
            };

            colorPreview.Click += (s, e) =>
            {
                ColorDialog colorDialog = new()
                {
                    Color = currentColor,
                    FullOpen = true
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    colorPreview.BackColor = colorDialog.Color;
                    onColorChanged(colorDialog.Color);
                }
            };

            container.Controls.Add(colorPreview);
        }
    }
}