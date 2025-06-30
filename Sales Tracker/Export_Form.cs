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
        // Properties
        private int _originalDirectoryLabelY, _originalDirectoryTextBoxY, _originalWarningDirLabelY, _originalWarningDirPictureBoxY;
        private static Action _validationCallback;
        private static bool _isProgramLoading = true;

        // Init.
        public Export_Form()
        {
            InitializeComponent();

            StoreOriginalPositions();
            AddEventHandlersToTextBoxes();
            SetControls();
            UpdateTheme();
            SetAccessibleDescriptions();
            _isProgramLoading = false;
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void StoreOriginalPositions()
        {
            _originalDirectoryLabelY = Directory_Label.Location.Y;
            _originalDirectoryTextBoxY = Directory_TextBox.Location.Y;
            _originalWarningDirLabelY = WarningDir_Label.Location.Y;
            _originalWarningDirPictureBoxY = WarningDir_PictureBox.Location.Y;
        }
        private void SetControls()
        {
            if (Properties.Settings.Default.ExportDirectory == "")
            {
                Properties.Settings.Default.ExportDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
            }

            Name_TextBox.Text = Directories.CompanyName + " " + Tools.FormatDate(DateTime.Today);
            Directory_TextBox.Text = Properties.Settings.Default.ExportDirectory;
            FileType_ComboBox.SelectedIndex = 0;

            string defaultCurrency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);
            SetDefaultCurrency(defaultCurrency);

            SetExportSpreadsheetControls();
        }
        private void SetDefaultCurrency(string currencyType)
        {
            foreach (string currency in Currency.GetCurrencyTypes())
            {
                // Check if the currency type matches the item (exact match or starts with)
                if (currency.Equals(currencyType, StringComparison.OrdinalIgnoreCase) ||
                    currency.StartsWith(currencyType, StringComparison.OrdinalIgnoreCase))
                {
                    Currency_TextBox.Text = currency;
                    return;
                }
            }

            // If no match found, default to USD
            Currency_TextBox.Text = "USD";
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
        private void SetAccessibleDescriptions()
        {
            Name_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Directory_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(Name_TextBox);
            TextBoxManager.Attach(Directory_TextBox);
            TextBoxManager.Attach(Currency_TextBox);

            // Set the validation callback before attaching SearchBox
            _validationCallback = ValidateInputs;
            SearchBox.SetValidationCallback(_validationCallback);

            SearchBox.Attach(Currency_TextBox, this, Currency.GetSearchResults, searchBoxMaxHeight, false, false, false, false);
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
            bool isNameValid = !("/#%&*|;".Any(Name_TextBox.Text.Contains) || Name_TextBox.Text == "");

            if (isNameValid)
            {
                CustomControls.SetGTextBoxToValid(Name_TextBox);
                WarningName_Label.Visible = false;
                WarningName_PictureBox.Visible = false;
            }
            else
            {
                CustomControls.SetGTextBoxToInvalid(Name_TextBox);
                WarningName_Label.Visible = true;
                WarningName_PictureBox.Visible = true;
            }

            ValidateInputs();
        }
        private void Directory_TextBox_TextChanged(object sender, EventArgs e)
        {
            bool isDirectoryValid = !("/#%&*|;".Any(Directory_TextBox.Text.Contains) ||
                Directory_TextBox.Text == "" ||
                !Directory.Exists(Directory_TextBox.Text.TrimEnd('\\')));

            if (isDirectoryValid)
            {
                Directory_TextBox.BorderColor = CustomColors.ControlBorder;
                Directory_TextBox.FocusedState.BorderColor = CustomColors.ControlBorder;
                WarningDir_Label.Visible = false;
                WarningDir_PictureBox.Visible = false;
            }
            else
            {
                Directory_TextBox.BorderColor = Color.Red;
                Directory_TextBox.FocusedState.BorderColor = Color.Red;
                WarningDir_Label.Visible = true;
                WarningDir_PictureBox.Visible = true;
            }

            ValidateInputs();
        }
        private void FileType_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetExportSpreadsheetControls();
        }
        private void ExportReceipts_Label_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ExportReceipts_CheckBox.Checked = !ExportReceipts_CheckBox.Checked;
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Select folder
            VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
            }
        }
        private async void Export_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Check if there's any data to export
            if (!HasAnyDataToExport())
            {
                CustomMessageBox.Show(
                    "No Data to Export",
                    "There is no data available to export. Please add some transactions, products, companies, or accountants before exporting.",
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            string loadingText = ExportReceipts_CheckBox.Checked && ExportReceipts_CheckBox.Visible
                ? $"Exporting {FileType_ComboBox.Text} and receipts..."
                : $"Exporting {FileType_ComboBox.Text}...";

            LoadingPanel.ShowLoadingScreen(this, loadingText);

            // Capture all UI values on the UI thread
            string fileType = FileType_ComboBox.Text;
            string directoryPath = Directory_TextBox.Text;
            string fileName = Name_TextBox.Text;
            bool exportReceipts = ExportReceipts_CheckBox.Checked && ExportReceipts_CheckBox.Visible;
            string currency = Currency_TextBox.Text;

            // Run the export operation
            await Task.Run(() => Export(fileType, directoryPath, fileName, exportReceipts, currency));
        }

        // Methods
        private void ValidateInputs()
        {
            if (_isProgramLoading) { return; }

            Export_Button.Enabled = Name_TextBox.BorderColor != Color.Red
                && Directory_TextBox.BorderColor != Color.Red
                && !string.IsNullOrWhiteSpace(Currency_TextBox.Text) && Currency_TextBox.Tag?.ToString() != "0";
        }
        private void SetExportSpreadsheetControls()
        {
            bool isExcelSelected = FileType_ComboBox.Text == "Excel spreadsheet (.xlsx)";

            if (isExcelSelected)
            {
                SetSpreadsheetOptionControlVisibility(true);

                // Move directory controls down to make room for the GroupBox
                byte space = 50;
                Directory_Label.Top = _originalDirectoryLabelY + space;
                Directory_TextBox.Top = _originalDirectoryTextBoxY + space;
                ThreeDots_Button.Top = _originalDirectoryTextBoxY + space;
                WarningDir_Label.Top = _originalWarningDirLabelY + space;
                WarningDir_PictureBox.Top = _originalWarningDirPictureBoxY + space;
            }
            else
            {
                SetSpreadsheetOptionControlVisibility(false);

                // Move directory controls back to original positions
                Directory_Label.Top = _originalDirectoryLabelY;
                Directory_TextBox.Top = _originalDirectoryTextBoxY;
                ThreeDots_Button.Top = _originalDirectoryTextBoxY;
                WarningDir_Label.Top = _originalWarningDirLabelY;
                WarningDir_PictureBox.Top = _originalWarningDirPictureBoxY;
            }
        }
        private void SetSpreadsheetOptionControlVisibility(bool visible)
        {
            ExportReceipts_CheckBox.Visible = visible;
            ExportReceipts_Label.Visible = visible;
            Currency_Label.Visible = visible;
            Currency_TextBox.Visible = visible;
        }
        private void Export(string fileType, string directoryPath, string fileName, bool exportReceipts, string currency)
        {
            string filePath = Path.Combine(directoryPath, fileName);
            Stopwatch stopwatch = Stopwatch.StartNew();

            switch (fileType)
            {
                case "ArgoSales (.zip)":
                    Directories.CreateBackup(filePath);
                    TrackExport(stopwatch, filePath + ArgoFiles.ZipExtension, ExportType.Backup);
                    FinalizeExport($"Successfully backed up '{Directories.CompanyName}'");
                    break;

                case "Excel spreadsheet (.xlsx)":
                    string exportFolder = Path.Combine(directoryPath, fileName);

                    // Create export folder
                    exportFolder = Directories.GetNewDirectoryNameIfItAlreadyExists(exportFolder);
                    Directory.CreateDirectory(exportFolder);

                    // Create xlsx file inside the export folder
                    string xlsxPath = Path.Combine(exportFolder, fileName + ArgoFiles.XlsxFileExtension);
                    ExcelSheetManager.ExportSpreadsheet(xlsxPath, currency);

                    string successMessage = $"Successfully created spreadsheet for '{Directories.CompanyName}'";

                    // Export receipts if checkbox is checked
                    if (exportReceipts)
                    {
                        try
                        {
                            ExportReceiptsToFolder(Path.Combine(exportFolder, fileName));
                            successMessage += " and exported receipts";
                        }
                        catch (Exception ex)
                        {
                            successMessage += $", but failed to export receipts: {ex.Message}";
                        }
                    }

                    TrackExport(stopwatch, xlsxPath, ExportType.XLSX);
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
        private static bool HasAnyDataToExport()
        {
            MainMenu_Form main = MainMenu_Form.Instance;

            return main.Purchase_DataGridView.Rows.Count > 0 ||
                   main.Sale_DataGridView.Rows.Count > 0 ||
                   main.CategoryPurchaseList.Count > 0 ||
                   main.CategorySaleList.Count > 0 ||
                   main.CompanyList.Count > 0 ||
                   main.AccountantList.Count > 0;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}