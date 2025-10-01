using System.Drawing.Drawing2D;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Adds zoom and pan functionality to an existing PictureBox control.
    /// Attaches event handlers to enable mouse wheel zoom-to-cursor and drag-to-pan.
    /// </summary>
    public class ZoomableImageViewer
    {
        // Target control
        private readonly PictureBox _pictureBox;
        private readonly VScrollBar _vScrollBar;
        private readonly HScrollBar _hScrollBar;

        // Original image
        private Image _originalImage;

        // Zoom and pan state
        private float _zoomFactor = 1.0f;
        private PointF _imageOffset = PointF.Empty;
        private bool _isPanning = false;
        private Point _panStartPoint;
        private PointF _panStartOffset;

        // Configuration
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 10.0f;
        private const float ZoomStep = 1.25f;  // 25% zoom per wheel step
        private const float ButtonZoomStep = 0.25f;  // 25% for button clicks

        // Events
        public event EventHandler ZoomChanged;
        public event EventHandler ImageChanged;

        // Properties
        public Image Image
        {
            get => _originalImage;
            set
            {
                _originalImage?.Dispose();
                _originalImage = value;
                UpdatePictureBoxImage();
                FitToWindow();
                ImageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public float ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                _zoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, value));
                UpdateScrollBars();
                UpdatePictureBoxImage();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string ZoomPercentageText => $"{(int)(_zoomFactor * 100)}%";

        // Constructor
        public ZoomableImageViewer(PictureBox pictureBox, VScrollBar vScrollBar = null, HScrollBar hScrollBar = null)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));
            _vScrollBar = vScrollBar;
            _hScrollBar = hScrollBar;

            AttachEventHandlers();
            ConfigurePictureBox();
        }
        private void ConfigurePictureBox()
        {
            _pictureBox.SizeMode = PictureBoxSizeMode.Normal;
            _pictureBox.BorderStyle = BorderStyle.FixedSingle;

            // Store original image if one exists
            if (_pictureBox.Image != null)
            {
                _originalImage = _pictureBox.Image;
            }
        }
        private void AttachEventHandlers()
        {
            // Detach first to prevent duplicates
            DetachEventHandlers();

            // Picture box events
            _pictureBox.Paint += PictureBox_Paint;
            _pictureBox.MouseDown += PictureBox_MouseDown;
            _pictureBox.MouseMove += PictureBox_MouseMove;
            _pictureBox.MouseUp += PictureBox_MouseUp;
            _pictureBox.MouseWheel += PictureBox_MouseWheel;
            _pictureBox.MouseEnter += PictureBox_MouseEnter;
            _pictureBox.Resize += PictureBox_Resize;

            // Scroll bar events if provided
            if (_vScrollBar != null)
            {
                _vScrollBar.Scroll += VScrollBar_Scroll;
            }

            if (_hScrollBar != null)
            {
                _hScrollBar.Scroll += HScrollBar_Scroll;
            }
        }
        private void DetachEventHandlers()
        {
            _pictureBox.Paint -= PictureBox_Paint;
            _pictureBox.MouseDown -= PictureBox_MouseDown;
            _pictureBox.MouseMove -= PictureBox_MouseMove;
            _pictureBox.MouseUp -= PictureBox_MouseUp;
            _pictureBox.MouseWheel -= PictureBox_MouseWheel;
            _pictureBox.MouseEnter -= PictureBox_MouseEnter;
            _pictureBox.Resize -= PictureBox_Resize;

            if (_vScrollBar != null)
            {
                _vScrollBar.Scroll -= VScrollBar_Scroll;
            }

            if (_hScrollBar != null)
            {
                _hScrollBar.Scroll -= HScrollBar_Scroll;
            }
        }

        // Public zoom methods
        public void ZoomIn()
        {
            Point center = new(_pictureBox.Width / 2, _pictureBox.Height / 2);
            float newZoom = _zoomFactor * (1 + ButtonZoomStep);
            ZoomAtPoint(center, newZoom);
        }
        public void ZoomOut()
        {
            Point center = new(_pictureBox.Width / 2, _pictureBox.Height / 2);
            float newZoom = _zoomFactor * (1 - ButtonZoomStep);
            ZoomAtPoint(center, newZoom);
        }
        public void FitToWindow()
        {
            if (_originalImage == null) { return; }

            float scaleX = (float)_pictureBox.Width / _originalImage.Width;
            float scaleY = (float)_pictureBox.Height / _originalImage.Height;
            _zoomFactor = Math.Min(scaleX, scaleY) * 0.95f;  // 95% to show borders

            CenterImage();
            UpdateScrollBars();
            UpdatePictureBoxImage();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
        public void ActualSize()
        {
            _zoomFactor = 1.0f;
            CenterImage();
            UpdateScrollBars();
            UpdatePictureBoxImage();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
        public void SetZoom(float zoomFactor)
        {
            Point center = new(_pictureBox.Width / 2, _pictureBox.Height / 2);
            ZoomAtPoint(center, zoomFactor);
        }

        // Event handlers
        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_originalImage == null) { return; }

            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Clear background
            g.Clear(_pictureBox.BackColor);

            // Calculate scaled dimensions
            float scaledWidth = _originalImage.Width * _zoomFactor;
            float scaledHeight = _originalImage.Height * _zoomFactor;

            // Draw image
            RectangleF destRect = new(
                _imageOffset.X,
                _imageOffset.Y,
                scaledWidth,
                scaledHeight
            );

            g.DrawImage(_originalImage, destRect,
                new RectangleF(0, 0, _originalImage.Width, _originalImage.Height),
                GraphicsUnit.Pixel);
        }
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _originalImage != null)
            {
                _isPanning = true;
                _panStartPoint = e.Location;
                _panStartOffset = _imageOffset;
                _pictureBox.Cursor = Cursors.Hand;
            }
        }
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && _originalImage != null)
            {
                _imageOffset.X = _panStartOffset.X + (e.X - _panStartPoint.X);
                _imageOffset.Y = _panStartOffset.Y + (e.Y - _panStartPoint.Y);

                ConstrainOffset();
                UpdateScrollBars();
                _pictureBox.Invalidate();
            }
        }
        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = false;
                _pictureBox.Cursor = Cursors.Default;
            }
        }
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_originalImage == null) { return; }

            float zoomMultiplier = e.Delta > 0 ? ZoomStep : 1f / ZoomStep;
            float newZoom = _zoomFactor * zoomMultiplier;

            ZoomAtPoint(e.Location, newZoom);
        }
        private void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            _pictureBox.Focus();
        }
        private void PictureBox_Resize(object sender, EventArgs e)
        {
            if (_originalImage != null)
            {
                ConstrainOffset();
                UpdateScrollBars();
                _pictureBox.Invalidate();
            }
        }
        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            _imageOffset.Y = -e.NewValue;
            _pictureBox.Invalidate();
        }
        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            _imageOffset.X = -e.NewValue;
            _pictureBox.Invalidate();
        }

        // Private helper methods
        private void ZoomAtPoint(Point mousePos, float newZoom)
        {
            if (_originalImage == null) { return; }

            float oldZoom = _zoomFactor;
            _zoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));

            if (Math.Abs(_zoomFactor - oldZoom) < 0.001f) { return; }

            // Calculate the point on the image that the mouse is over
            float imageX = (mousePos.X - _imageOffset.X) / oldZoom;
            float imageY = (mousePos.Y - _imageOffset.Y) / oldZoom;

            // Calculate new offset to keep the same point under the mouse
            _imageOffset.X = mousePos.X - imageX * _zoomFactor;
            _imageOffset.Y = mousePos.Y - imageY * _zoomFactor;

            ConstrainOffset();
            UpdateScrollBars();
            UpdatePictureBoxImage();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
        private void CenterImage()
        {
            if (_originalImage == null) { return; }

            float scaledWidth = _originalImage.Width * _zoomFactor;
            float scaledHeight = _originalImage.Height * _zoomFactor;

            _imageOffset = new PointF(
                (_pictureBox.Width - scaledWidth) / 2,
                (_pictureBox.Height - scaledHeight) / 2
            );

            ConstrainOffset();
        }
        private void ConstrainOffset()
        {
            if (_originalImage == null) return;

            float scaledWidth = _originalImage.Width * _zoomFactor;
            float scaledHeight = _originalImage.Height * _zoomFactor;

            // If image is smaller than view, center it
            if (scaledWidth <= _pictureBox.Width)
            {
                _imageOffset.X = (_pictureBox.Width - scaledWidth) / 2;
            }
            else
            {
                // Constrain panning
                _imageOffset.X = Math.Min(0, _imageOffset.X);
                _imageOffset.X = Math.Max(_pictureBox.Width - scaledWidth, _imageOffset.X);
            }

            if (scaledHeight <= _pictureBox.Height)
            {
                _imageOffset.Y = (_pictureBox.Height - scaledHeight) / 2;
            }
            else
            {
                // Constrain panning
                _imageOffset.Y = Math.Min(0, _imageOffset.Y);
                _imageOffset.Y = Math.Max(_pictureBox.Height - scaledHeight, _imageOffset.Y);
            }
        }
        private void UpdateScrollBars()
        {
            if (_originalImage == null)
            {
                if (_vScrollBar != null) { _vScrollBar.Visible = false; }
                if (_hScrollBar != null) { _hScrollBar.Visible = false; }
                return;
            }

            float scaledWidth = _originalImage.Width * _zoomFactor;
            float scaledHeight = _originalImage.Height * _zoomFactor;

            // Vertical scroll bar
            if (_vScrollBar != null)
            {
                if (scaledHeight > _pictureBox.Height)
                {
                    _vScrollBar.Visible = true;
                    _vScrollBar.Maximum = (int)(scaledHeight - _pictureBox.Height);
                    _vScrollBar.LargeChange = Math.Max(1, _pictureBox.Height / 10);
                    _vScrollBar.SmallChange = Math.Max(1, _pictureBox.Height / 20);
                    _vScrollBar.Value = Math.Max(0, Math.Min(_vScrollBar.Maximum, (int)-_imageOffset.Y));
                }
                else
                {
                    _vScrollBar.Visible = false;
                }
            }

            // Horizontal scroll bar
            if (_hScrollBar != null)
            {
                if (scaledWidth > _pictureBox.Width)
                {
                    _hScrollBar.Visible = true;
                    _hScrollBar.Maximum = (int)(scaledWidth - _pictureBox.Width);
                    _hScrollBar.LargeChange = Math.Max(1, _pictureBox.Width / 10);
                    _hScrollBar.SmallChange = Math.Max(1, _pictureBox.Width / 20);
                    _hScrollBar.Value = Math.Max(0, Math.Min(_hScrollBar.Maximum, (int)-_imageOffset.X));
                }
                else
                {
                    _hScrollBar.Visible = false;
                }
            }
        }
        private void UpdatePictureBoxImage()
        {
            // Force repaint
            _pictureBox.Invalidate();
        }
    }
}