using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;
using System.Diagnostics;

namespace Sales_Tracker
{
    public partial class Export_Form : Form
    {
        // Init.
        public Export_Form()
        {
            InitializeComponent();

            AddEventHandlersToTextBoxes();
            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(Export_Button);

            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                ThreeDots_Button.Image = Resources.ThreeDotsWhite;
            }
            else
            {
                ThreeDots_Button.Image = Resources.ThreeDotsBlack;
            }
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Name_TextBox);
            TextBoxManager.Attach(Directory_TextBox);
        }

        // Form event handlers
        private void Export_Form_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ExportDirectory == "")
            {
                Properties.Settings.Default.ExportDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
            }

            Directory_TextBox.Text = Properties.Settings.Default.ExportDirectory;
            FileType_ComboBox.SelectedIndex = 0;
            Name_TextBox.Text = Directories.CompanyName + " " + Tools.FormatDate(DateTime.Today);
        }
        private void Export_Form_Shown(object sender, EventArgs e)
        {
            BeginInvoke(() => Export_Button.Focus());  // This fixes a bug
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Name_TextBox_TextChanged(object sender, EventArgs e)
        {
            if ("/#%&*|;".Any(Name_TextBox.Text.Contains) || Name_TextBox.Text == "")
            {
                Export_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(Name_TextBox);
                WarningName_Label.Visible = true;
                WarningName_PictureBox.Visible = true;
            }
            else
            {
                Export_Button.Enabled = true;
                CustomControls.SetGTextBoxToValid(Name_TextBox);
                WarningName_Label.Visible = false;
                WarningName_PictureBox.Visible = false;
            }
        }
        private void Directory_TextBox_TextChanged(object sender, EventArgs e)
        {
            if ("/#%&*|;".Any(Directory_TextBox.Text.Contains) || Directory_TextBox.Text == "" || !Directory_TextBox.Text.Contains('\\'))
            {
                Export_Button.Enabled = false;
                Directory_TextBox.BorderColor = Color.Red;
                Directory_TextBox.FocusedState.BorderColor = Color.Red;
                WarningDir_Label.Visible = true;
                WarningDir_PictureBox.Visible = true;
            }
            else
            {
                Export_Button.Enabled = true;
                Directory_TextBox.BorderColor = CustomColors.ControlBorder;
                Directory_TextBox.FocusedState.BorderColor = CustomColors.ControlBorder;
                WarningDir_Label.Visible = false;
                WarningDir_PictureBox.Visible = false;
            }
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
            }
        }
        private async void Export_Button_Click(object sender, EventArgs e)
        {
            LoadingPanel.ShowLoadingScreen(this, $"Exporting {FileType_ComboBox.Text}...");

            string fileType = FileType_ComboBox.Text;
            await Task.Run(() => { Export(fileType); });
        }

        // Methods
        private void Export(string fileType)
        {
            string filePath = Directory_TextBox.Text + "\\" + Name_TextBox.Text;
            Stopwatch stopwatch = Stopwatch.StartNew();
            ExportType exportType;

            switch (fileType)
            {
                case "ArgoSales (.zip)":
                    Directories.CreateBackup(filePath);
                    exportType = ExportType.Backup;
                    TrackExport(stopwatch, filePath + ArgoFiles.ZipExtension, exportType);
                    FinalizeExport($"Successfully backed up '{Directories.CompanyName}'");
                    break;

                case "Excel spreadsheet (.xlsx)":
                    string xlsxPath = filePath + ArgoFiles.XlsxFileExtension;
                    ExcelSheetManager.ExportSpreadsheet(xlsxPath);
                    exportType = ExportType.XLSX;
                    TrackExport(stopwatch, xlsxPath, exportType);
                    FinalizeExport($"Successfully created spreadsheet for '{Directories.CompanyName}'");
                    break;
            }
        }
        private static void TrackExport(Stopwatch stopwatch, string filePath, ExportType exportType)
        {
            stopwatch.Stop();
            string readableSize = "0 Bytes";

            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new(filePath);
                long fileSizeBytes = fileInfo.Length;
                readableSize = Tools.ConvertBytesToReadableSize(fileSizeBytes);
            }

            Dictionary<ExportDataField, object> exportData = new()
            {
                { ExportDataField.ExportType, exportType },
                { ExportDataField.DurationMS, stopwatch.ElapsedMilliseconds },
                { ExportDataField.FileSize, readableSize }
            };

            AnonymousDataManager.AddExportData(exportData);
        }
        private void FinalizeExport(string message)
        {
            Invoke(() =>
            {
                LoadingPanel.HideLoadingScreen(this);
                CustomMessageBox.Show("Successfully exported", message, CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
            });
        }
    }
}