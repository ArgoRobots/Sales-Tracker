using Ookii.Dialogs.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.Theme;
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
            SetControls();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void SetControls()
        {
            if (Properties.Settings.Default.ExportDirectory == "")
            {
                Properties.Settings.Default.ExportDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
            }

            Directory_TextBox.Text = Properties.Settings.Default.ExportDirectory;
            FileType_ComboBox.SelectedIndex = 0;
            Name_TextBox.Text = Directories.CompanyName + " " + Tools.FormatDate(DateTime.Today);

            UpdateReceiptExportControlsVisibility();
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Export_Button);

            if (ThemeManager.IsDarkTheme())
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
        private void Export_Form_Shown(object sender, EventArgs e)
        {
            ExportCompany_Label.Focus();
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
        private void FileType_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReceiptExportControlsVisibility();
        }
        private void ExportReceipts_Label_Click(object sender, EventArgs e)
        {
            ExportReceipts_CheckBox.Checked = !ExportReceipts_CheckBox.Checked;
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            // Select folder
            VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
            }
        }
        private async void Export_Button_Click(object sender, EventArgs e)
        {
            string loadingText = ExportReceipts_CheckBox.Checked && ExportReceipts_CheckBox.Visible
                ? $"Exporting {FileType_ComboBox.Text} and receipts..."
                : $"Exporting {FileType_ComboBox.Text}...";

            LoadingPanel.ShowLoadingScreen(this, loadingText);

            string fileType = FileType_ComboBox.Text;
            bool exportReceipts = ExportReceipts_CheckBox.Checked && ExportReceipts_CheckBox.Visible;

            await Task.Run(() => { Export(fileType, exportReceipts); });
        }

        // Methods
        private void UpdateReceiptExportControlsVisibility()
        {
            bool isExcelSelected = FileType_ComboBox.Text == "Excel spreadsheet (.xlsx)";

            ExportReceipts_CheckBox.Visible = isExcelSelected;
            ExportReceipts_Label.Visible = isExcelSelected;
        }
        private void Export(string fileType, bool exportReceipts = false)
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
                    string exportFolder = Path.Combine(Directory_TextBox.Text, Name_TextBox.Text);
                    exportFolder = Directories.GetNewDirectoryNameIfItAlreadyExists(exportFolder);
                    Directory.CreateDirectory(exportFolder);

                    // Create xlsx file inside the export folder
                    string xlsxPath = Path.Combine(exportFolder, Name_TextBox.Text + ArgoFiles.XlsxFileExtension);
                    ExcelSheetManager.ExportSpreadsheet(xlsxPath);
                    exportType = ExportType.XLSX;

                    string successMessage = $"Successfully created spreadsheet for '{Directories.CompanyName}'";

                    // Export receipts if checkbox is checked
                    if (exportReceipts)
                    {
                        try
                        {
                            ExportReceiptsToFolder(Path.Combine(exportFolder, Name_TextBox.Text));
                            successMessage += " and exported receipts";
                        }
                        catch (Exception ex)
                        {
                            successMessage += $", but failed to export receipts: {ex.Message}";
                        }
                    }

                    TrackExport(stopwatch, xlsxPath, exportType);
                    FinalizeExport(successMessage);
                    break;
            }
        }
        private static void ExportReceiptsToFolder(string basePath)
        {
            // Create receipts folder
            string receiptsFolder = basePath + "_Receipts";
            Directory.CreateDirectory(receiptsFolder);

            List<string> allReceipts = [];

            // Collect receipt paths from sales
            foreach (DataGridViewRow row in MainMenu_Form.Instance.Sale_DataGridView.Rows)
            {
                string receiptPath = GetReceiptPathFromRow(row);
                if (!string.IsNullOrEmpty(receiptPath) && File.Exists(receiptPath))
                {
                    allReceipts.Add(receiptPath);
                }
            }

            // Collect receipt paths from purchases
            foreach (DataGridViewRow row in MainMenu_Form.Instance.Purchase_DataGridView.Rows)
            {
                string receiptPath = GetReceiptPathFromRow(row);
                if (!string.IsNullOrEmpty(receiptPath) && File.Exists(receiptPath))
                {
                    allReceipts.Add(receiptPath);
                }
            }

            // Copy receipts to the export folder
            foreach (string receiptPath in allReceipts)
            {
                string fileName = Path.GetFileName(receiptPath);
                string destinationPath = Path.Combine(receiptsFolder, fileName);
                File.Copy(receiptPath, destinationPath, true);
            }
        }
        private static string GetReceiptPathFromRow(DataGridViewRow row)
        {
            if (row.Tag == null) { return ""; }

            // Handle different tag formats based on Receipts_Form.cs pattern
            switch (row.Tag)
            {
                case (string dir, TagData):
                    return ReceiptManager.ProcessReceiptTextFromRowTag(dir);

                case (List<string> items, TagData) when items.Count > 0:
                    string receipt = items[^1];
                    return ReceiptManager.ProcessReceiptTextFromRowTag(receipt);

                default:
                    return "";
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