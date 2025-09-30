using Sales_Tracker.Language;
using System.Diagnostics;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Third step in report generation - preview and export functionality.
    /// </summary>
    public partial class ReportPreviewExport_Form : Form
    {
        // Properties  private int _initialFormWidth;
        private int _initialFormWidth;
        private int _initialFormHeight;
        private int _initialLeftPanelWidth;
        private int _initialRightPanelWidth;
        private ExportSettings _exportSettings;
        private float _currentZoom = 1.0f;
        private const float _zoomIncrement = 0.25f;
        private const float _minZoom = 0.25f;
        private const float _maxZoom = 4.0f;

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

        // Init.
        public ReportPreviewExport_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            ParentReportForm = parentForm;

            InitializeExportSettings();
            SetupPageSettings();
            SetupExportSettings();
            SetupPreviewControls();
            StoreInitialSizes();
        }
        private void InitializeExportSettings()
        {
            _exportSettings = new ExportSettings
            {
                Format = ExportFormat.PNG,
                DPI = 300,
                Quality = 95,
                OpenAfterExport = true
            };
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
            Orientation_ComboBox.Items.Clear();
            Orientation_ComboBox.Items.Add(LanguageManager.TranslateString("Portrait"));
            Orientation_ComboBox.Items.Add(LanguageManager.TranslateString("Landscape"));
            Orientation_ComboBox.SelectedIndex = 0;  // Portrait default
        }
        private void SetupExportSettings()
        {
            // Setup format combo box
            ExportFormat_ComboBox.Items.Add("PNG Image (*.png)");
            ExportFormat_ComboBox.Items.Add("JPEG Image (*.jpg)");
            ExportFormat_ComboBox.SelectedIndex = 0;

            // Setup DPI
            DPI_NumericUpDown.Value = _exportSettings.DPI;

            // Setup quality
            Quality_TrackBar.Value = _exportSettings.Quality;
            UpdateQualityLabel();

            // Setup checkboxes
            OpenAfterExport_CheckBox.Checked = _exportSettings.OpenAfterExport;
        }
        private void SetupPreviewControls()
        {
            Preview_PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            Preview_PictureBox.BackColor = Color.White;
        }
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialFormHeight = Height;
            _initialLeftPanelWidth = LeftPreviewPanel.Width;
            _initialRightPanelWidth = RightSettingsPanel.Width;
        }

        // Form event handlers
        private void ReportPreviewExport_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                if (Visible)
                {
                    OnStepActivated();
                }
                else
                {
                    OnStepDeactivated();
                }
            }
        }
        private void ReportPreviewExport_Form_Resize(object sender, EventArgs e)
        {
            if (_initialFormWidth == 0) { return; }

            // Calculate the form's width change ratio
            float widthRatio = (float)Width / _initialFormWidth;

            // Calculate new panel widths while maintaining proportion
            LeftPreviewPanel.Width = (int)(_initialLeftPanelWidth * widthRatio);
            RightSettingsPanel.Width = (int)(_initialRightPanelWidth * widthRatio);

            // Position the right panel
            RightSettingsPanel.Left = Width - RightSettingsPanel.Width;
        }

        // Event handlers
        private void RefreshPreview_Button_Click(object sender, EventArgs e)
        {
            GeneratePreview();
        }
        private void ZoomIn_Button_Click(object sender, EventArgs e)
        {
            if (_currentZoom < _maxZoom)
            {
                _currentZoom += _zoomIncrement;
                GeneratePreview();
            }
        }
        private void ZoomOut_Button_Click(object sender, EventArgs e)
        {
            if (_currentZoom > _minZoom)
            {
                _currentZoom -= _zoomIncrement;
                GeneratePreview();
            }
        }
        private void ZoomFit_Button_Click(object sender, EventArgs e)
        {
            _currentZoom = 1.0f;
            GeneratePreview();
        }
        private void BrowseExportPath_Button_Click(object sender, EventArgs e)
        {
            using SaveFileDialog saveDialog = new();

            // Set filter based on selected format
            if (ExportFormat_ComboBox.SelectedIndex == 0)
            {
                saveDialog.Filter = "PNG Image|*.png|All Files|*.*";
                saveDialog.DefaultExt = "png";
            }
            else
            {
                saveDialog.Filter = "JPEG Image|*.jpg|All Files|*.*";
                saveDialog.DefaultExt = "jpg";
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
        private void OpenAfterExport_Label_Click(object sender, EventArgs e)
        {
            OpenAfterExport_CheckBox.Checked = !OpenAfterExport_CheckBox.Checked;
        }

        // Preview generation
        private void GeneratePreview()
        {
            try
            {
                if (ReportConfig == null)
                {
                    Preview_PictureBox.Image = CreatePreviewPlaceholder();
                    return;
                }

                // Update report configuration with current page settings
                UpdateReportConfigFromPageSettings();

                // Create renderer and generate preview
                ReportRenderer renderer = new(ReportConfig, _exportSettings);
                Bitmap previewImage = renderer.RenderToPreview(
                    (int)(Preview_PictureBox.Width * _currentZoom),
                    (int)(Preview_PictureBox.Height * _currentZoom)
                );

                // Dispose previous image to prevent memory leaks
                Preview_PictureBox.Image?.Dispose();

                Preview_PictureBox.Image = previewImage;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    LanguageManager.TranslateString($"Preview generation failed: {ex.Message}"),
                    LanguageManager.TranslateString("Preview Error"),
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok
                );

                Preview_PictureBox.Image = CreatePreviewPlaceholder();
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
                    string text = LanguageManager.TranslateString("Click Refresh to generate preview");
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
                        LanguageManager.TranslateString("No report configuration available for export."),
                        LanguageManager.TranslateString("Export Error"),
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok
                    );
                    return false;
                }

                UpdateExportSettingsFromUI();
                UpdateReportConfigFromPageSettings();

                // Show progress indication
                UseWaitCursor = true;
                RefreshPreview_Button.Text = LanguageManager.TranslateString("Exporting...");
                RefreshPreview_Button.Enabled = false;

                try
                {
                    // Create renderer and export report
                    ReportRenderer renderer = new(ReportConfig, _exportSettings);
                    renderer.ExportReport();

                    CustomMessageBox.Show(
                        LanguageManager.TranslateString("Report exported successfully!"),
                        LanguageManager.TranslateString("Export Complete"),
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
                    RefreshPreview_Button.Text = LanguageManager.TranslateString("Refresh");
                    RefreshPreview_Button.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    LanguageManager.TranslateString($"Export failed: {ex.Message}"),
                    LanguageManager.TranslateString("Export Error"),
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }
        }
        private void UpdateExportSettingsFromUI()
        {
            _exportSettings.FilePath = ExportPath_TextBox.Text;
            _exportSettings.Format = ExportFormat_ComboBox.SelectedIndex == 0 ? ExportFormat.PNG : ExportFormat.JPG;
            _exportSettings.DPI = (int)DPI_NumericUpDown.Value;
            _exportSettings.Quality = Quality_TrackBar.Value;
            _exportSettings.OpenAfterExport = OpenAfterExport_CheckBox.Checked;
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
            ReportConfig.Orientation = Orientation_ComboBox.SelectedIndex == 0 ?
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
                    LanguageManager.TranslateString("Please select an export location."),
                    LanguageManager.TranslateString("No Export Path"),
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
                        LanguageManager.TranslateString("The specified export directory does not exist."),
                        LanguageManager.TranslateString("Invalid Export Path"),
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
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    LanguageManager.TranslateString($"Cannot write to the specified location: {ex.Message}"),
                    LanguageManager.TranslateString("Invalid Export Path"),
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            return true;
        }
        public void UpdateReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            // Update page settings
            UpdateReportConfigFromPageSettings();
            ReportConfig.LastModified = DateTime.Now;
        }
        public void LoadFromReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            PerformUpdate(() =>
            {
                // Load page settings
                PageSize_ComboBox.SelectedIndex = (int)ReportConfig.PageSize;
                Orientation_ComboBox.SelectedIndex = (int)ReportConfig.Orientation;

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

        /// <summary>
        /// Called when the form becomes active (user navigates to this step).
        /// </summary>
        public void OnStepActivated()
        {
            LoadFromReportConfiguration();
            NotifyParentValidationChanged();
        }

        /// <summary>
        /// Called when the form becomes inactive (user navigates away from this step).
        /// </summary>
        public void OnStepDeactivated()
        {
            UpdateReportConfiguration();
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
        /// Safely updates UI controls without triggering events
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