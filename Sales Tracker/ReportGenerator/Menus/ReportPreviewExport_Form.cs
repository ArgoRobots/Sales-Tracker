using Sales_Tracker.Language;
using Sales_Tracker.UI;
using System.Diagnostics;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Third step in report generation - preview and export functionality.
    /// </summary>
    public partial class ReportPreviewExport_Form : Form
    {
        // Properties
        private static ReportPreviewExport_Form _instance;
        private int _initialFormWidth;
        private int _initialLeftPanelWidth;
        private int _initialRightPanelWidth;
        private ExportSettings _exportSettings;
        private ZoomableImageViewer _zoomableViewer;

        /// <summary>
        /// Gets the parent report generator form.
        /// </summary>
        public ReportGenerator_Form ParentReportForm { get; private set; }

        /// <summary>
        /// Gets the current report configuration.
        /// </summary>
        private ReportConfiguration? ReportConfig => ParentReportForm?.CurrentReportConfiguration;

        /// <summary>
        /// Indicates if the form is currently being loaded/updated programmatically.
        /// </summary>
        private bool _isUpdating;

        // Getters
        public static ReportPreviewExport_Form Instance => _instance;

        // Init.
        public ReportPreviewExport_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            _instance = this;
            ParentReportForm = parentForm;

            InitializeExportSettings();
            SetupZoomableViewer();
            SetupPageSettings();
            SetupExportSettings();
            StoreInitialSizes();
            SetEventHandlers();
            ScaleControls();
        }
        private void InitializeExportSettings()
        {
            _exportSettings = new ExportSettings();
        }
        private void SetupZoomableViewer()
        {
            // Create the zoomable viewer handler and attach it to the existing PictureBox
            _zoomableViewer = new ZoomableImageViewer(Preview_PictureBox);

            // Subscribe to zoom changed event
            _zoomableViewer.ZoomChanged += (s, e) =>
            {
                ZoomStatus_Label.Text = _zoomableViewer.ZoomPercentageText;
            };

            // Add event handlers
            ZoomIn_Button.Click += (s, e) => _zoomableViewer.ZoomIn();
            ZoomOut_Button.Click += (s, e) => _zoomableViewer.ZoomOut();
            FitToWindow_Button.Click += (s, e) => _zoomableViewer.FitToWindow();
        }
        private void SetupPageSettings()
        {
            // Setup page size combo box
            PageSize_ComboBox.Items.Clear();
            PageSize_ComboBox.Items.Add("A4 (210 × 297 mm)");
            PageSize_ComboBox.Items.Add("Letter (8.5 × 11 in)");
            PageSize_ComboBox.Items.Add("Legal (8.5 × 14 in)");
            PageSize_ComboBox.Items.Add("Tabloid (11 × 17 in)");
            PageSize_ComboBox.SelectedIndex = 0;  // A4 default

            // Setup orientation combo box
            PageOrientation_ComboBox.Items.Clear();
            PageOrientation_ComboBox.Items.Add("Portrait");
            PageOrientation_ComboBox.Items.Add("Landscape");
            PageOrientation_ComboBox.SelectedIndex = 0;  // Portrait default
        }
        private void SetupExportSettings()
        {
            // Setup format combo box
            ExportFormat_ComboBox.Items.Clear();
            ExportFormat_ComboBox.Items.Add("PNG Image (*.png)");
            ExportFormat_ComboBox.Items.Add("JPEG Image (*.jpg)");
            ExportFormat_ComboBox.Items.Add("PDF Image (*.pdf)");
            ExportFormat_ComboBox.SelectedIndex = 0;

            // Setup quality slider (controls both resolution and compression)
            Quality_TrackBar.Value = _exportSettings.Quality;
            UpdateQualityLabel();

            // Setup checkboxes
            OpenAfterExport_CheckBox.Checked = _exportSettings.OpenAfterExport;
            IncludeHeader_CheckBox.Checked = ReportConfig?.ShowHeader ?? true;
            IncludeFooter_CheckBox.Checked = ReportConfig?.ShowFooter ?? true;
        }
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialLeftPanelWidth = LeftPreview_Panel.Width;
            _initialRightPanelWidth = RightSettings_Panel.Width;
        }
        private void SetEventHandlers()
        {
            ExportFormat_ComboBox.SelectedIndexChanged += ExportSettings_Changed;
            ExportFormat_ComboBox.SelectedIndexChanged += ExportFormat_ComboBox_SelectedIndexChanged;
        }
        private void ScaleControls()
        {
            DpiHelper.ScaleComboBox(ExportFormat_ComboBox);
            DpiHelper.ScaleComboBox(PageSize_ComboBox);
            DpiHelper.ScaleComboBox(PageOrientation_ComboBox);
            DpiHelper.ScaleGroupBox(Preview_GroupBox);
            DpiHelper.ScaleGroupBox(PageSettings_GroupBox);
            DpiHelper.ScaleGroupBox(ExportSettings_GroupBox);
        }

        // Form event handlers
        private void ReportPreviewExport_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && Visible)
            {
                LoadFromReportConfiguration();
                NotifyParentValidationChanged();
            }
        }
        private void ReportPreviewExport_Form_Resize(object sender, EventArgs e)
        {
            if (_initialFormWidth == 0) { return; }

            // Calculate the form's width change ratio
            float widthRatio = (float)Width / _initialFormWidth;

            // Calculate new panel widths while maintaining proportion
            LeftPreview_Panel.Width = (int)(_initialLeftPanelWidth * widthRatio);
            RightSettings_Panel.Width = (int)(_initialRightPanelWidth * widthRatio);

            // Position the right panel
            RightSettings_Panel.Left = LeftPreview_Panel.Width;
        }

        // Event handlers
        private void BrowseExportPath_Button_Click(object sender, EventArgs e)
        {
            using SaveFileDialog saveDialog = new();

            // Set filter based on selected format
            if (ExportFormat_ComboBox.SelectedIndex == 0)
            {
                saveDialog.Filter = "PNG Image|*.png|All Files|*.*";
                saveDialog.DefaultExt = "png";
            }
            else if (ExportFormat_ComboBox.SelectedIndex == 1)
            {
                saveDialog.Filter = "JPEG Image|*.jpg|All Files|*.*";
                saveDialog.DefaultExt = "jpg";
            }
            else if (ExportFormat_ComboBox.SelectedIndex == 2)
            {
                saveDialog.Filter = "PDF Document|*.pdf|All Files|*.*";
                saveDialog.DefaultExt = "pdf";
            }

            // Set default filename
            string defaultName = ReportConfig?.Title ?? "Report";
            defaultName = string.Join("_", defaultName.Split(Path.GetInvalidFileNameChars()));
            defaultName += $"_{DateTime.Now:yyyyMMdd_HHmmss}";
            saveDialog.FileName = defaultName;

            if (!string.IsNullOrEmpty(ExportPath_TextBox.Text))
            {
                try
                {
                    saveDialog.InitialDirectory = Path.GetDirectoryName(ExportPath_TextBox.Text);
                }
                catch
                {
                    // Use default directory if path is invalid
                    saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
            }
            else
            {
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ExportPath_TextBox.Text = saveDialog.FileName;
                NotifyParentValidationChanged();
            }
        }
        private void Quality_TrackBar_ValueChanged(object sender, EventArgs e)
        {
            UpdateQualityLabel();
        }
        private void PageSettings_Changed(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                GeneratePreview();
            }
        }
        private void ExportSettings_Changed(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                UpdateExportSettingsFromUI();
            }
        }
        private void IncludeHeader_Label_Click(object sender, EventArgs e)
        {
            IncludeHeader_CheckBox.Checked = !IncludeHeader_CheckBox.Checked;
        }
        private void IncludeFooter_Label_Click(object sender, EventArgs e)
        {
            IncludeFooter_CheckBox.Checked = !IncludeFooter_CheckBox.Checked;
        }
        private void OpenAfterExport_Label_Click(object sender, EventArgs e)
        {
            OpenAfterExport_CheckBox.Checked = !OpenAfterExport_CheckBox.Checked;
        }
        private void IncludeHeader_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && ReportConfig != null)
            {
                ReportConfig.ShowHeader = IncludeHeader_CheckBox.Checked;
                GeneratePreview();
            }
        }
        private void IncludeFooter_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && ReportConfig != null)
            {
                ReportConfig.ShowFooter = IncludeFooter_CheckBox.Checked;
                GeneratePreview();
            }
        }
        private void PageNumber_NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && ReportConfig != null)
            {
                // Update the page number in the configuration
                ReportConfig.CurrentPageNumber = (int)PageNumber_NumericUpDown.Value;

                GeneratePreview();
            }
        }
        private void ExportFormat_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                UpdateExportSettingsFromUI();

                // Update file extension when format changes
                if (!string.IsNullOrEmpty(ExportPath_TextBox.Text))
                {
                    string currentPath = ExportPath_TextBox.Text;
                    string newExtension = ExportFormat_ComboBox.SelectedIndex switch
                    {
                        0 => ".png",
                        1 => ".jpg",
                        2 => ".pdf",
                        _ => ".png"
                    };

                    string newPath = Path.ChangeExtension(currentPath, newExtension);
                    ExportPath_TextBox.Text = newPath;
                }
            }
        }

        // Preview generation
        private void GeneratePreview()
        {
            try
            {
                if (ReportConfig == null)
                {
                    _zoomableViewer.Image = CreatePreviewPlaceholder();
                    return;
                }

                UpdateReportConfigFromPageSettings();

                // Create renderer and generate preview
                ReportRenderer renderer = new(ReportConfig, _exportSettings);

                // Generate at higher resolution for better quality when zooming
                int previewWidth = Math.Max(1600, Preview_PictureBox.Width * 2);
                int previewHeight = Math.Max(1200, Preview_PictureBox.Height * 2);

                Bitmap previewImage = renderer.RenderToPreview(previewWidth, previewHeight);

                // Set the image in the zoomable viewer
                if (_zoomableViewer.Image != null)
                {
                    Image oldImage = _zoomableViewer.Image;
                    _zoomableViewer.Image = previewImage;
                    oldImage.Dispose();  // Dispose after setting new image
                }
                else
                {
                    _zoomableViewer.Image = previewImage;
                }
            }
            catch
            {
                CustomMessageBox.Show(
                    "Preview Error",
                    "Preview generation failed",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok
                );

                _zoomableViewer.Image = CreatePreviewPlaceholder();
            }
        }
        private static Bitmap CreatePreviewPlaceholder()
        {
            Bitmap bitmap = new(400, 300);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                using (Font font = new("Segoe UI", 16, FontStyle.Bold))
                using (SolidBrush brush = new(Color.Gray))
                {
                    string text = LanguageManager.TranslateString("Report Preview");
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (bitmap.Width - textSize.Width) / 2;
                    float y = (bitmap.Height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }

                using (Font font = new("Segoe UI", 10))
                using (SolidBrush brush = new(Color.LightGray))
                {
                    string text = LanguageManager.TranslateString("Configure settings and generate preview");
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (bitmap.Width - textSize.Width) / 2;
                    int y = (bitmap.Height / 2) + 30;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return bitmap;
        }

        // Export functionality
        public bool ExportReport()
        {
            try
            {
                if (!ValidateStep())
                {
                    return false;
                }

                if (ReportConfig == null)
                {
                    CustomMessageBox.Show(
                        "Export Error",
                        "No report configuration available for export.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok
                    );
                    return false;
                }

                UpdateExportSettingsFromUI();
                UpdateReportConfigFromPageSettings();

                // Show progress indication
                UseWaitCursor = true;

                try
                {
                    // Create renderer and export report
                    ReportRenderer renderer = new(ReportConfig, _exportSettings);
                    renderer.ExportReport();

                    CustomMessageBox.Show(
                        "Export Complete",
                        "Report exported successfully!",
                        CustomMessageBoxIcon.Info,
                        CustomMessageBoxButtons.Ok
                    );

                    // Open file if option is set
                    if (_exportSettings.OpenAfterExport && File.Exists(_exportSettings.FilePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _exportSettings.FilePath,
                            UseShellExecute = true
                        });
                    }

                    return true;
                }
                finally
                {
                    UseWaitCursor = false;
                }
            }
            catch
            {
                CustomMessageBox.Show(
                    "Export Error",
                    "Export failed",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }
        }
        private void UpdateExportSettingsFromUI()
        {
            _exportSettings.FilePath = ExportPath_TextBox.Text;
            _exportSettings.Format = ExportFormat_ComboBox.SelectedIndex switch
            {
                0 => ExportFormat.PNG,
                1 => ExportFormat.JPG,
                2 => ExportFormat.PDF,
                _ => ExportFormat.PNG
            };

            // Map quality slider to both DPI and compression quality
            int quality = Quality_TrackBar.Value;
            _exportSettings.DPI = GetDPIFromQuality(quality);
            _exportSettings.Quality = quality;  // JPEG compression quality

            _exportSettings.OpenAfterExport = OpenAfterExport_CheckBox.Checked;
        }

        /// <summary>
        /// Maps quality percentage to appropriate DPI value.
        /// </summary>
        private static int GetDPIFromQuality(int quality)
        {
            return quality switch
            {
                <= 10 => 72,   // Very low
                <= 20 => 96,   // Low
                <= 30 => 120,  // Low-medium
                <= 40 => 150,  // Medium
                <= 50 => 180,  // Medium-high
                <= 60 => 200,  // High
                <= 70 => 250,  // High quality print
                <= 80 => 300,  // Professional print
                <= 90 => 400,  // Very high quality
                <= 95 => 500,  // Exceptional quality
                _ => 600       // Maximum quality
            };
        }
        private void UpdateReportConfigFromPageSettings()
        {
            if (ReportConfig == null) { return; }

            // Update page size
            ReportConfig.PageSize = PageSize_ComboBox.SelectedIndex switch
            {
                0 => PageSize.A4,
                1 => PageSize.Letter,
                2 => PageSize.Legal,
                3 => PageSize.Tabloid,
                _ => PageSize.A4
            };

            // Update orientation
            ReportConfig.PageOrientation = PageOrientation_ComboBox.SelectedIndex == 0 ?
                PageOrientation.Portrait : PageOrientation.Landscape;
        }

        // Helper methods
        private void UpdateQualityLabel()
        {
            Quality_ValueLabel.Text = $"{Quality_TrackBar.Value}%";
        }

        // Form implementation methods
        public bool ValidateStep()
        {
            // Validate export path is set
            if (string.IsNullOrEmpty(ExportPath_TextBox.Text))
            {
                CustomMessageBox.Show(
                    "No Export Path",
                    "Please select an export location.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            // Validate export path is writable
            try
            {
                string directory = Path.GetDirectoryName(ExportPath_TextBox.Text);
                if (!Directory.Exists(directory))
                {
                    CustomMessageBox.Show(
                        "Invalid Export Path",
                        "The specified export directory does not exist.",
                        CustomMessageBoxIcon.Info,
                        CustomMessageBoxButtons.Ok
                    );
                    return false;
                }

                // Test write access
                string testFile = Path.Combine(directory, Path.GetRandomFileName());
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch
            {
                CustomMessageBox.Show(
                    "Invalid Export Path",
                    "Cannot write to the specified location",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            return true;
        }
        public void LoadFromReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            PerformUpdate(() =>
            {
                // Load page settings
                PageSize_ComboBox.SelectedIndex = (int)ReportConfig.PageSize;
                PageOrientation_ComboBox.SelectedIndex = (int)ReportConfig.PageOrientation;
                PageNumber_NumericUpDown.Value = ReportConfig.CurrentPageNumber;

                // Set default export path if not set
                if (string.IsNullOrEmpty(ExportPath_TextBox.Text))
                {
                    string fileName = ReportConfig.Title ?? "Report";
                    fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
                    fileName += $"_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                    ExportPath_TextBox.Text = defaultPath;
                }

                GeneratePreview();
            });
        }

        // Helper methods for base functionality
        /// <summary>
        /// Notifies the parent form that validation state has changed.
        /// </summary>
        private void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }

        /// <summary>
        /// Safely updates UI controls without triggering events.
        /// </summary>
        private void PerformUpdate(Action updateAction)
        {
            if (updateAction == null) { return; }

            _isUpdating = true;

            try
            {
                updateAction();
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}