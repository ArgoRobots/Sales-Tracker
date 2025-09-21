using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class ReceiptViewer_Form : BaseForm
    {
        // Properties
        private readonly string _currentFilePath;
        private Image _originalImage;
        private float _zoomFactor = 1.0f;
        private float _effectiveMaxZoom = 5.0f;
        private const float _zoomIncrement = 0.1f;
        private const float _minZoom = 0.1f;
        private const float _maxZoom = 5.0f;

        // Supported file formats
        private static readonly HashSet<string> SupportedImageFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".webp"
        };
        private static readonly HashSet<string> SupportedDocumentFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf"
        };

        // Init.
        public ReceiptViewer_Form(string filePath)
        {
            InitializeComponent();

            _currentFilePath = filePath;
            SetupMouseWheelEvents();
            LoadReceipt();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ImagePanel.BackColor = CustomColors.ControlBack;
            ControlsPanel.BackColor = CustomColors.ToolbarBackground;
        }
        private void SetAccessibleDescriptions()
        {
            Zoom_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }

        // Mouse wheel setup
        private void SetupMouseWheelEvents()
        {
            // Add mouse wheel event handlers for zooming
            ImagePanel.MouseWheel += ImagePanel_MouseWheel;
            ReceiptPictureBox.MouseWheel += ImagePanel_MouseWheel;

            // Make sure the controls can receive focus for mouse wheel events
            ImagePanel.MouseEnter += (s, e) => ImagePanel.Focus();
            ReceiptPictureBox.MouseEnter += (s, e) => ReceiptPictureBox.Focus();
        }
        private void ImagePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Only handle mouse wheel for images, not PDFs
            if (!ReceiptPictureBox.Visible || _originalImage == null) { return; }

            // Determine zoom direction based on wheel delta
            bool zoomIn = e.Delta > 0;

            if (zoomIn && _zoomFactor < _effectiveMaxZoom)
            {
                _zoomFactor += _zoomIncrement;
                ApplyZoom();
            }
            else if (!zoomIn && _zoomFactor > _minZoom)
            {
                _zoomFactor -= _zoomIncrement;
                ApplyZoom();
            }
        }

        // Form event handlers
        private void ReceiptViewer_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ReceiptViewer_Form_Resize(object sender, EventArgs e)
        {
            // Re-fit image if it was fitted to window
            if (_originalImage != null && ReceiptPictureBox.Visible)
            {
                // Only auto-fit if image is smaller than the panel
                if (_originalImage.Width <= ImagePanel.ClientSize.Width &&
                    _originalImage.Height <= ImagePanel.ClientSize.Height)
                {
                    FitToWindow();
                }
            }
        }

        // Button event handlers
        private void ZoomIn_Button_Click(object sender, EventArgs e)
        {
            if (_zoomFactor < _effectiveMaxZoom)
            {
                _zoomFactor += _zoomIncrement;
                ApplyZoom();
            }
        }
        private void ZoomOut_Button_Click(object sender, EventArgs e)
        {
            if (_zoomFactor > _minZoom)
            {
                _zoomFactor -= _zoomIncrement;
                ApplyZoom();
            }
        }
        private void ResetZoom_Button_Click(object sender, EventArgs e)
        {
            _zoomFactor = 1.0f;
            ApplyZoom();
        }
        private void FitToWindow_Button_Click(object sender, EventArgs e)
        {
            FitToWindow();
        }
        private void Export_Button_Click(object sender, EventArgs e)
        {
            // Use SaveFileDialog to let user choose export location
            using SaveFileDialog dialog = new();
            dialog.FileName = Path.GetFileName(_currentFilePath);
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.Title = "Export Receipt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Copy(_currentFilePath, dialog.FileName, true);
                    CustomMessageBox.Show("Export successful", "Receipt exported successfully.",
                        CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
                }
                catch
                {
                    CustomMessageBox.Show("Export failed", "Failed to export receipt",
                        CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                }
            }
        }

        // Receipt loading methods
        private void LoadReceipt()
        {
            if (!File.Exists(_currentFilePath))
            {
                CustomMessageBox.Show("File not found", "The receipt file could not be found.",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            string extension = Path.GetExtension(_currentFilePath);
            FileInfo fileInfo = new(_currentFilePath);

            // Update Form Text with file name and size
            Text = LanguageManager.TranslateString("Receipt Manager") +
                $" • {Path.GetFileName(_currentFilePath)} • {Tools.ConvertBytesToReadableSize(fileInfo.Length)}";

            try
            {
                if (SupportedImageFormats.Contains(extension))
                {
                    LoadImageFile();
                }
                else if (SupportedDocumentFormats.Contains(extension))
                {
                    LoadDocumentFile();
                }
                else
                {
                    CustomMessageBox.ShowWithFormat("Unsupported format",
                        "The file format '{0}' is not supported for viewing.",
                        CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok,
                        extension);
                }
            }
            catch
            {
                CustomMessageBox.Show("Error loading receipt",
                    "An error occurred while loading the receipt",
                   CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }
        }
        private void LoadImageFile()
        {
            // Load image from file
            using (FileStream stream = new(_currentFilePath, FileMode.Open, FileAccess.Read))
            {
                _originalImage = Image.FromStream(stream);
            }

            ReceiptPictureBox.Image = _originalImage;
            ReceiptPictureBox.Visible = true;
            WebBrowser.Visible = false;

            SetZoomControlsEnabled(true);
            FitToWindow();
        }
        private void LoadDocumentFile()
        {
            // For PDFs, use WebBrowser control
            WebBrowser.Navigate(_currentFilePath);
            WebBrowser.Visible = true;
            ReceiptPictureBox.Visible = false;

            // Disable zoom controls for PDFs (browser handles its own zoom)
            SetZoomControlsEnabled(false);
            Zoom_Label.Text = "PDF";
        }

        // Zoom and display methods
        private void SetZoomControlsEnabled(bool enabled)
        {
            ZoomIn_Button.Enabled = enabled;
            ZoomOut_Button.Enabled = enabled;
            ResetZoom_Button.Enabled = enabled;
            FitToWindow_Button.Enabled = enabled;
        }
        private void ApplyZoom()
        {
            if (_originalImage == null) { return; }

            int newWidth = (int)(_originalImage.Width * _zoomFactor);
            int newHeight = (int)(_originalImage.Height * _zoomFactor);

            ReceiptPictureBox.Size = new Size(newWidth, newHeight);
            ReceiptPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            // Center the image in the panel
            ReceiptPictureBox.Location = new Point(
                Math.Max(0, (ImagePanel.ClientSize.Width - newWidth) / 2),
                Math.Max(0, (ImagePanel.ClientSize.Height - newHeight) / 2)
            );

            Zoom_Label.Text = $"{_zoomFactor * 100:F0}%";
        }
        private void FitToWindow()
        {
            if (_originalImage == null) { return; }

            float scaleWidth = (float)ImagePanel.ClientSize.Width / _originalImage.Width;
            float scaleHeight = (float)ImagePanel.ClientSize.Height / _originalImage.Height;
            _zoomFactor = Math.Min(scaleWidth, scaleHeight);

            // Ensure we don't go below minimum zoom
            _zoomFactor = Math.Max(_zoomFactor, _minZoom);

            // Update effective max zoom to be either the default MaxZoom or the fit-to-window zoom, whichever is larger
            _effectiveMaxZoom = Math.Max(_maxZoom, _zoomFactor);

            ApplyZoom();
        }

        // Helper methods
        /// <summary>
        /// Checks if a file format is supported by the receipt viewer.
        /// </summary>
        public static bool IsFormatSupported(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) { return false; }

            string extension = Path.GetExtension(filePath);
            return SupportedImageFormats.Contains(extension) || SupportedDocumentFormats.Contains(extension);
        }

        /// <summary>
        /// Gets a user-friendly description of supported formats.
        /// </summary>
        public static string GetSupportedFormatsDescription()
        {
            string imageFormats = string.Join(", ", SupportedImageFormats.Select(f => f.ToUpper()));
            string documentFormats = string.Join(", ", SupportedDocumentFormats.Select(f => f.ToUpper()));

            return $"Images: {imageFormats}\nDocuments: {documentFormats}";
        }
    }
}