using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ImportSpreadsheet
{
    public partial class ImportSpreadsheet_Form : BaseForm
    {
        // Properties
        private string _spreadsheetFilePath, _receiptsFolderPath;
        private readonly MainMenu_Form.SelectedOption _oldOption;
        private string _selectedSourceCurrency = "USD";  // Default to USD

        // Init.
        public ImportSpreadsheet_Form()
        {
            InitializeComponent();

            _oldOption = MainMenu_Form.Instance.Selected;
            InitContainerPanel();
            InitCurrencyControls();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            RemoveSpreadsheetLabel();
            RemoveReceiptsFolderLabel();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void InitCurrencyControls()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(Currency_TextBox);
            SearchBox.Attach(Currency_TextBox, this, Currency.GetSearchResults, searchBoxMaxHeight, false, false, false, false);
            Currency_TextBox.TextChanged += Currency_TextBox_TextChanged;

            HideCurrencyControls();
        }
        private void Currency_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Currency_TextBox.Text))
            {
                _selectedSourceCurrency = Currency_TextBox.Text.Split(' ')[0];  // Get just the currency code
            }
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Import_Button);
            ThemeManager.MakeGButtonBlueSecondary(SelectSpreadsheet_Button);
            ThemeManager.MakeGButtonBlueSecondary(SelectReceiptsFolder_Button);
            ThemeManager.MakeGButtonBlueSecondary(DetectCurrency_Button);
        }
        private void SetAccessibleDescriptions()
        {
            Currency_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
        }

        // Form event handlers
        private void ImportSpreadSheets_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ImportSpreadSheets_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = _oldOption;
        }

        // Select spreadsheet
        private async void SelectSpreadsheet_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // File selection dialog
            OpenFileDialog dialog = new()
            {
                Filter = $"Spreadsheet (*{ArgoFiles.XlsxFileExtension})|*{ArgoFiles.XlsxFileExtension}",
                Title = "Select spreadsheet to import"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _spreadsheetFilePath = dialog.FileName;
                if (!ValidateSpreadsheet()) { return; }

                ShowSpreadsheetLabel(dialog.SafeFileName);
                AutoDetectReceiptsFolder();

                await DetectCurrencyAsync();
                await RefreshPanelsAsync();
            }
        }
        private bool ValidateSpreadsheet()
        {
            try
            {
                using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);

                List<string> validationErrors = [];

                foreach (IXLWorksheet worksheet in workbook.Worksheets)
                {
                    // Skip empty worksheets
                    if (!worksheet.RowsUsed().Any()) { continue; }

                    string worksheetNameLower = worksheet.Name.ToLowerInvariant();

                    // Validate based on worksheet name
                    if (worksheetNameLower.Contains("accountant"))
                    {
                        ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateSimpleListColumns(
                            worksheet,
                            Accountants_Form.Column.AccountantName,
                            Accountants_Form.ColumnHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                        if (!validation.IsValid)
                        {
                            validationErrors.Add($"Accountants worksheet is missing required columns: {string.Join(", ", validation.MissingColumns)}");
                        }
                    }
                    else if (worksheetNameLower.Contains("compan"))  // No, this is not a typo. This allows for both "company" and "companies".
                    {
                        ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateSimpleListColumns(
                            worksheet,
                            Companies_Form.Column.Company,
                            Companies_Form.ColumnHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                        if (!validation.IsValid)
                        {
                            validationErrors.Add($"Companies worksheet is missing required columns: {string.Join(", ", validation.MissingColumns)}");
                        }
                    }
                    else if (worksheetNameLower.Contains("product"))
                    {
                        ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateProductColumns(worksheet);

                        if (!validation.IsValid)
                        {
                            validationErrors.Add($"{worksheet.Name} worksheet is missing required columns: {string.Join(", ", validation.MissingColumns)}");
                        }
                    }
                    else if (worksheetNameLower.Contains("purchase") || worksheetNameLower.Contains("sale"))
                    {
                        bool isPurchase = worksheetNameLower.Contains("purchase");

                        ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateTransactionColumns(worksheet, isPurchase);
                        if (!validation.IsValid)
                        {
                            validationErrors.Add($"{worksheet.Name} worksheet is missing required columns: {string.Join(", ", validation.MissingColumns)}");
                        }
                    }
                }

                if (validationErrors.Count > 0)
                {
                    CustomMessageBox.Show("Spreadsheet Structure Issues",
                        "The spreadsheet has the following structure issues:\n\n" +
                        string.Join("\n", validationErrors) +
                        "\n\nPlease correct these issues before importing.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok);
                    return false;
                }

                return true;
            }
            catch
            {
                CustomMessageBox.Show("Spreadsheet is invalid",
                    "This spreadsheet is invalid or corrupted. Please choose a valid spreadsheet.",
                     CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);

                return false;
            }
        }

        // Currency Detection Methods
        private async Task DetectCurrencyAsync()
        {
            if (string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                return;
            }

            string detectedCurrency = await Task.Run(DetectCurrencyFromSpreadsheet);

            Currency_TextBox.Text = GetCurrencyDisplayText(detectedCurrency ?? "USD");
            ShowCurrencyControls();

            // Prevent the SearchBox from opening
            SearchBox.DebounceTimer.Stop();
        }
        private string DetectCurrencyFromSpreadsheet()
        {
            using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XLWorkbook workbook = new(stream);

            foreach (IXLWorksheet worksheet in workbook.Worksheets)
            {
                // Skip if worksheet is empty
                if (!worksheet.RowsUsed().Any()) { continue; }

                // Get the first data row (skip header)
                IXLRow dataRow = worksheet.RowsUsed().Skip(1).FirstOrDefault();
                if (dataRow == null) { continue; }

                // Check currency columns (8-14)
                for (int col = 8; col <= 14; col++)
                {
                    IXLCell cell = dataRow.Cell(col);
                    if (cell.IsEmpty()) { continue; }

                    // Check the cell's number format for currency symbols
                    string numberFormat = cell.Style.NumberFormat.Format;

                    // Find currency pattern in the number format
                    foreach (KeyValuePair<string, List<string>> currencyPattern in Currency.CurrencyPatterns)
                    {
                        string currency = currencyPattern.Key;
                        List<string> patterns = currencyPattern.Value;

                        foreach (string pattern in patterns)
                        {
                            if (numberFormat.Contains(pattern))
                            {
                                return currency;
                            }
                        }
                    }
                }
            }

            return "";
        }
        private void HideCurrencyControls()
        {
            Currency_Label.Visible = false;
            Currency_TextBox.Visible = false;
            DetectCurrency_Button.Visible = false;
        }
        private void ShowCurrencyControls()
        {
            Currency_Label.Visible = true;
            Currency_TextBox.Visible = true;
            DetectCurrency_Button.Visible = true;
        }
        private static string GetCurrencyDisplayText(string currencyCode)
        {
            List<string> currencies = Currency.GetCurrencyTypes();
            return currencies.FirstOrDefault(c => c.StartsWith(currencyCode, StringComparison.OrdinalIgnoreCase)) ?? currencyCode;
        }
        private void DetectCurrency_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            _ = DetectCurrencyAsync();
        }
        private void RemoveSpreadsheet_ImageButton_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            RemoveSpreadsheetLabel();
            Controls.Remove(_centeredFlowPanel);
            Import_Button.Enabled = false;
            _receiptsFolderPath = "";

            HideCurrencyControls();
        }
        private void RemoveSpreadhseet_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveSpreadsheet_ImageButton.BackColor = CustomColors.MouseHover;
        }
        private void RemoveSpreadsheet_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveSpreadsheet_ImageButton.BackColor = CustomColors.MainBackground;
        }

        // Select receipts folder
        private void SelectReceiptsFolder_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            FolderBrowserDialog dialog = new()
            {
                Description = "Select folder containing receipt files",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrEmpty(_receiptsFolderPath) && Directory.Exists(_receiptsFolderPath))
            {
                dialog.SelectedPath = _receiptsFolderPath;
            }
            else if (!string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                dialog.SelectedPath = Path.GetDirectoryName(_spreadsheetFilePath);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _receiptsFolderPath = dialog.SelectedPath;
                ShowReceiptsFolderLabel();
            }
        }
        private async void RemoveReceiptsFolder_ImageButton_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            RemoveReceiptsFolderLabel();
            await RefreshPanelsAsync();
        }
        private void RemoveReceiptsFolder_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveReceiptsFolder_ImageButton.BackColor = CustomColors.MouseHover;
        }
        private void RemoveReceiptsFolder_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveReceiptsFolder_ImageButton.BackColor = CustomColors.MainBackground;
        }

        // Refresh panels
        private async Task RefreshPanelsAsync()
        {
            if (string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                return;
            }

            Import_Button.Enabled = false;
            Controls.Remove(_centeredFlowPanel);
            LoadingPanel.ShowLoadingScreen(this, "Loading...");

            // Remember current checkbox states
            Dictionary<string, bool> checkboxStates = [];
            foreach (Panel panel in _centeredFlowPanel.Controls.OfType<Panel>())
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();

                if (checkBox != null)
                {
                    string worksheetName = panel.Tag.ToString();
                    checkboxStates[worksheetName] = checkBox.Checked;
                }
            }

            List<Panel> panels = await Task.Run(LoadSpreadsheetData);

            // Set checkbox states
            foreach (Panel panel in panels)
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();

                if (checkBox != null)
                {
                    string worksheetName = panel.Tag.ToString();
                    checkBox.Checked = !checkboxStates.TryGetValue(worksheetName, out bool savedState) || savedState;
                }
            }

            if (panels.Count > 0)
            {
                Import_Button.Enabled = true;
                AddPanels(panels);
            }
            else
            {
                // No panels were created
                ShowEmptySpreadsheetPanel();
            }

            LoadingPanel.HideLoadingScreen(this);
        }
        private void ShowEmptySpreadsheetPanel()
        {
            Panel emptyPanel = new()
            {
                Size = new Size(_panelWidth, _panelHeight),
                BackColor = CustomColors.ContentPanelBackground,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label emptyLabel = new()
            {
                Text = "No Data Found",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = CustomColors.Text,
                Size = new Size(emptyPanel.Width - 20, 40),
                Location = new Point(10, emptyPanel.Height / 2 - 40)
            };

            Label instructionLabel = new()
            {
                Text = "Please select a spreadsheet\nwith valid data",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10),
                ForeColor = CustomColors.Text,
                Size = new Size(emptyPanel.Width - 20, 60),
                Location = new Point(10, emptyPanel.Height / 2)
            };

            emptyPanel.Controls.Add(emptyLabel);
            emptyPanel.Controls.Add(instructionLabel);

            _centeredFlowPanel.Controls.Clear();
            _centeredFlowPanel.Controls.Add(emptyPanel);

            Controls.Add(_centeredFlowPanel);
            _centeredFlowPanel.Left = (ClientSize.Width - _centeredFlowPanel.Width) / 2;
        }

        // Import data with rollback and cancellation support
        private async void Import_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (!ValidateSpreadsheet()) { return; }

            // Check if any checkboxes are selected
            List<Panel> selectedPanels = _centeredFlowPanel.Controls.OfType<Panel>()
                .Where(panel => panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault()?.Checked == true)
                .ToList();

            if (selectedPanels.Count == 0)
            {
                CustomMessageBox.Show("No Data Selected",
                    "Please select at least one data section to import.",
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            Controls.Remove(_centeredFlowPanel);
            LoadingPanel.ShowLoadingScreen(this, "Importing...");
            Import_Button.Enabled = false;

            ExcelSheetManager.ImportSession importSession = new();
            bool importSuccessful = false;

            try
            {
                ExcelSheetManager.ImportSummary summary = await Task.Run(() => ImportSpreadsheetAndReceipts(importSession));

                if (summary.WasCancelled)
                {
                    // Rollback all changes
                    ExcelSheetManager.RollbackImportSession(importSession);
                    LoadingPanel.HideLoadingScreen(this);
                    await RefreshPanelsAsync();
                    ShowImportCancelledMessage();
                }
                else if (summary.HasAnyImports)
                {
                    // Commit all changes
                    ExcelSheetManager.CommitImportSession(importSession);
                    importSuccessful = true;

                    MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
                    MainMenu_Form.Instance.UpdateTotalLabels();
                    LoadingPanel.HideLoadingScreen(this);

                    ShowImportSuccessMessage(summary);

                    string message = $"Imported '{Path.GetFileName(_spreadsheetFilePath)}' ({_selectedSourceCurrency})";
                    CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);

                    Close();
                }
                else
                {
                    // Nothing was imported, but no cancellation - still rollback to be safe
                    ExcelSheetManager.RollbackImportSession(importSession);
                    LoadingPanel.HideLoadingScreen(this);
                    await RefreshPanelsAsync();
                    ShowNoImportMessage(summary);
                }
            }
            catch (Exception ex)
            {
                // On any exception, rollback changes
                ExcelSheetManager.RollbackImportSession(importSession);
                LoadingPanel.HideLoadingScreen(this);

                CustomMessageBox.ShowWithFormat("Error", "An error occurred while importing the spreadsheet: {0}. All changes have been rolled back.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok,
                    ex.Message);
            }
            finally
            {
                LoadingPanel.HideLoadingScreen(this);
                Import_Button.Enabled = true;

                // Refresh the UI if import was not successful
                if (!importSuccessful)
                {
                    MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
                    MainMenu_Form.Instance.UpdateTotalLabels();
                }
            }
        }
        private static void ShowImportCancelledMessage()
        {
            CustomMessageBox.Show("Import Cancelled", "The import was cancelled. All changes have been rolled back",
                CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
        }
        private static void ShowImportSuccessMessage(ExcelSheetManager.ImportSummary summary)
        {
            List<string> importedItems = [];

            if (summary.AccountantsImported > 0)
            {
                importedItems.Add($"{summary.AccountantsImported} accountant{(summary.AccountantsImported == 1 ? "" : "s")}");
            }
            if (summary.CompaniesImported > 0)
            {
                importedItems.Add($"{summary.CompaniesImported} compan{(summary.CompaniesImported == 1 ? "y" : "ies")}");
            }
            if (summary.PurchaseProductsImported > 0)
            {
                importedItems.Add($"{summary.PurchaseProductsImported} purchase product{(summary.PurchaseProductsImported == 1 ? "" : "s")}");
            }
            if (summary.SaleProductsImported > 0)
            {
                importedItems.Add($"{summary.SaleProductsImported} sale product{(summary.SaleProductsImported == 1 ? "" : "s")}");
            }
            if (summary.PurchaseTransactionsImported > 0)
            {
                importedItems.Add($"{summary.PurchaseTransactionsImported} purchase{(summary.PurchaseTransactionsImported == 1 ? "" : "s")}");
            }
            if (summary.SaleTransactionsImported > 0)
            {
                importedItems.Add($"{summary.SaleTransactionsImported} sale{(summary.SaleTransactionsImported == 1 ? "" : "s")}");
            }
            if (summary.ReceiptsImported > 0)
            {
                importedItems.Add($"{summary.ReceiptsImported} receipt{(summary.ReceiptsImported == 1 ? "" : "s")}");
            }

            string message;
            if (importedItems.Count == 0)
            {
                message = "No items were imported.";
            }
            else if (importedItems.Count == 1)
            {
                message = $"Successfully imported {importedItems[0]}.";
            }
            else if (importedItems.Count == 2)
            {
                message = $"Successfully imported {importedItems[0]} and {importedItems[1]}.";
            }
            else
            {
                message = $"Successfully imported {string.Join(", ", importedItems.Take(importedItems.Count - 1))}, and {importedItems.Last()}.";
            }

            // Only show skipped rows if there were actual problems (not just item rows)
            if (summary.SkippedRows > 0)
            {
                message += $"\n\n{summary.SkippedRows} transaction{(summary.SkippedRows == 1 ? "" : "s")} {(summary.SkippedRows == 1 ? "was" : "were")} skipped.";
            }

            // Show item rows information if there were multi-item transactions
            if (summary.ItemRowsProcessed > 0)
            {
                message += $"\n{summary.ItemRowsProcessed} item{(summary.ItemRowsProcessed == 1 ? "" : "s")} processed within multi-item transactions.";
            }

            if (summary.Errors.Count > 0)
            {
                message += $"\nTotal error{(summary.Errors.Count == 1 ? "" : "s")} encountered: {summary.Errors.Count}";
            }

            CustomMessageBox.Show("Import Completed", message, CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
        }
        private static void ShowNoImportMessage(ExcelSheetManager.ImportSummary summary)
        {
            string message = "Nothing was imported.";

            if (summary.Errors.Count > 0)
            {
                message += $"\n{summary.Errors.Count} errors were encountered during the import process.";

                // Show first few errors as examples
                if (summary.Errors.Count > 0)
                {
                    message += "\nFirst error example:";
                    ExcelSheetManager.ImportError firstError = summary.Errors.First();
                    message += $"\nWorksheet: {firstError.WorksheetName}";
                    message += $"\nRow: {firstError.RowNumber}";
                    message += $"\nField: {firstError.FieldName}";
                    message += $"\nInvalid Value: '{firstError.InvalidValue}'";
                }
            }

            CustomMessageBox.Show("No Data Imported", message, CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
        }
        private ExcelSheetManager.ImportSummary ImportSpreadsheetAndReceipts(ExcelSheetManager.ImportSession importSession)
        {
            MainMenu_Form.IsProgramLoading = true;

            ExcelSheetManager.ImportSummary aggregatedSummary = new();

            using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XLWorkbook workbook = new(stream);

            foreach (Panel panel in _centeredFlowPanel.Controls.OfType<Panel>())
            {
                // Check for cancellation before processing each worksheet
                if (importSession.IsCancelled)
                {
                    aggregatedSummary.WasCancelled = true;
                    break;
                }

                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                string worksheetName = panel.Tag.ToString();

                // If the CheckBox is not checked
                if (checkBox == null || !checkBox.Checked)
                {
                    continue;
                }

                // If the sheet no longer exists
                if (!workbook.Worksheets.Any(ws => ws.Name.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    CustomMessageBox.ShowWithFormat("Sheet no longer exists",
                        "The sheet {0} no longer exists and will not be imported",
                        CustomMessageBoxIcon.Exclamation,
                        CustomMessageBoxButtons.Ok,
                        worksheetName);
                    continue;
                }

                IXLWorksheet worksheet = workbook.Worksheet(worksheetName);

                switch (worksheetName)
                {
                    case _accountantsName:
                        bool accountantsImported = ExcelSheetManager.ImportAccountantsData(worksheet, importSession);
                        if (accountantsImported)
                        {
                            aggregatedSummary.AccountantsImported = CountRowsToImport(worksheet);
                        }
                        break;

                    case _companiesName:
                        bool companiesImported = ExcelSheetManager.ImportCompaniesData(worksheet, importSession);
                        if (companiesImported)
                        {
                            aggregatedSummary.CompaniesImported = CountRowsToImport(worksheet);
                        }
                        break;

                    case _purchaseProductsName:
                        bool purchaseProductsImported = ExcelSheetManager.ImportProductsData(worksheet, true, importSession);
                        if (purchaseProductsImported)
                        {
                            aggregatedSummary.PurchaseProductsImported = CountRowsToImport(worksheet);
                        }
                        break;

                    case _saleProductsName:
                        bool saleProductsImported = ExcelSheetManager.ImportProductsData(worksheet, false, importSession);
                        if (saleProductsImported)
                        {
                            aggregatedSummary.SaleProductsImported = CountRowsToImport(worksheet);
                        }
                        break;

                    case _purchasesName:
                        ExcelSheetManager.ImportSummary purchaseSummary = ExcelSheetManager.ImportPurchaseData(
                            worksheet, _selectedSourceCurrency, importSession);

                        // Aggregate the results
                        aggregatedSummary.SkippedRows += purchaseSummary.SkippedRows;
                        aggregatedSummary.Errors.AddRange(purchaseSummary.Errors);
                        aggregatedSummary.PurchaseTransactionsImported += purchaseSummary.PurchaseTransactionsImported;

                        if (purchaseSummary.WasCancelled || importSession.IsCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Import receipts if receipts folder is specified
                        if (!string.IsNullOrEmpty(_receiptsFolderPath) && Directory.Exists(_receiptsFolderPath))
                        {
                            int importedReceiptCount = ExcelSheetManager.ImportReceiptsData(worksheet, _receiptsFolderPath, true, importSession);
                            aggregatedSummary.ReceiptsImported += importedReceiptCount;
                        }
                        break;

                    case _salesName:
                        ExcelSheetManager.ImportSummary salesSummary = ExcelSheetManager.ImportSalesData(
                            worksheet, _selectedSourceCurrency, importSession);

                        // Aggregate the results
                        aggregatedSummary.SkippedRows += salesSummary.SkippedRows;
                        aggregatedSummary.Errors.AddRange(salesSummary.Errors);
                        aggregatedSummary.SaleTransactionsImported += salesSummary.SaleTransactionsImported;

                        if (salesSummary.WasCancelled || importSession.IsCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Import receipts if receipts folder is specified
                        if (!string.IsNullOrEmpty(_receiptsFolderPath) && Directory.Exists(_receiptsFolderPath))
                        {
                            int importedReceiptCount = ExcelSheetManager.ImportReceiptsData(worksheet, _receiptsFolderPath, false, importSession);
                            aggregatedSummary.ReceiptsImported += importedReceiptCount;
                        }
                        break;
                }

                // Check for cancellation after each worksheet processing
                if (importSession.IsCancelled)
                {
                    aggregatedSummary.WasCancelled = true;
                    break;
                }
            }

            // Set the final cancellation state
            aggregatedSummary.WasCancelled = importSession.IsCancelled;

            MainMenu_Form.IsProgramLoading = false;
            return aggregatedSummary;
        }

        /// <summary>
        /// Helper method to count rows that would be imported (for non-transaction imports).
        /// </summary>
        private static int CountRowsToImport(IXLWorksheet worksheet)
        {
            IEnumerable<IXLRow> rowsToProcess = worksheet.RowsUsed().Skip(1);  // Skip header

            int count = 0;

            foreach (IXLRow row in rowsToProcess)
            {
                string firstCell = row.Cell(1).GetValue<string>();
                if (!string.IsNullOrWhiteSpace(firstCell))
                {
                    count++;
                }
            }

            return count;
        }

        // Other event handlers
        private void OpenTutorial_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenLink("https://argorobots.com/documentation/index.php#spreadsheets");
        }
        private void AutoDetectReceiptsFolder()
        {
            _receiptsFolderPath = "";
            if (string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                return;
            }

            string spreadsheetDirectory = Path.GetDirectoryName(_spreadsheetFilePath);
            string spreadsheetNameWithoutExtension = Path.GetFileNameWithoutExtension(_spreadsheetFilePath);

            // Check for various receipts folder naming conventions (File Explorer is not case sensitive)
            string[] possibleFolderNames =
            [
                $"{spreadsheetNameWithoutExtension}_Receipts",
                $"{spreadsheetNameWithoutExtension} Receipts",
                "Receipts",
            ];

            foreach (string folderName in possibleFolderNames)
            {
                string possiblePath = Path.Combine(spreadsheetDirectory, folderName);
                if (Directory.Exists(possiblePath))
                {
                    // Get the actual folder name with correct casing from the file system
                    DirectoryInfo dirInfo = new(possiblePath);
                    _receiptsFolderPath = dirInfo.FullName;  // This preserves the actual case
                    ShowReceiptsFolderLabel();
                    break;
                }
            }
        }

        // Show things to import
        private readonly byte _panelPadding = 25, _panelHeight = 240;
        private readonly short _panelWidth = 300;
        private CenteredFlowLayoutPanel _centeredFlowPanel;
        private void InitContainerPanel()
        {
            _centeredFlowPanel = new()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(_panelPadding * 6 + _panelWidth * 3 + 50, _panelHeight * 2 + _panelPadding),
                Top = Currency_TextBox.Bottom + 40,
                Spacing = _panelPadding
            };
        }
        private Panel CreatePanel(List<string> items, string worksheetName)
        {
            Panel outerPanel = new()
            {
                Size = new Size(_panelWidth, _panelHeight),
                BackColor = CustomColors.ContentPanelBackground,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = worksheetName
            };

            // Title for the section
            Label titleLabel = new()
            {
                Text = worksheetName,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Top = outerPanel.Padding.Top,
                ForeColor = CustomColors.Text
            };
            outerPanel.Controls.Add(titleLabel);
            titleLabel.Left = (outerPanel.Width - titleLabel.Width) / 2;

            int flowPanelY = titleLabel.Height + titleLabel.Location.Y + CustomControls.SpaceBetweenControls;

            // Checkbox to disable importing certain data
            Guna2CustomCheckBox customCheckBox = new()
            {
                Size = new Size(20, 20),
                Location = new Point(outerPanel.Width - 30, 10),
                Animated = true
            };
            ThemeManager.SetThemeForControls([customCheckBox]);
            outerPanel.Controls.Add(customCheckBox);

            // FlowLayoutPanel to hold items
            FlowLayoutPanel flowPanel = new()
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Size = new Size(outerPanel.Width, outerPanel.Height - flowPanelY - 50),
                Padding = new Padding(15, 0, 0, 0),
                Location = new Point(outerPanel.Padding.Left, flowPanelY)
            };

            // Display the first five items
            for (int i = 0; i < Math.Min(items.Count, 5); i++)
            {
                Label itemLabel = new()
                {
                    Font = new Font("Segoe UI", 11),
                    ForeColor = CustomColors.Text,
                    AutoSize = false,
                    AutoEllipsis = true,
                    Text = items[i],
                    Size = new Size(flowPanel.Width - flowPanel.Padding.Left - 5, 30)
                };

                flowPanel.Controls.Add(itemLabel);
            }
            outerPanel.Controls.Add(flowPanel);

            // If there are more than 5 items, show a "plus (x) more" label
            int moreLabelY = flowPanel.Location.Y + flowPanel.Height + CustomControls.SpaceBetweenControls;
            int remaining = items.Count - 5;

            if (items.Count > 5)
            {
                Label moreLabel = new()
                {
                    Text = $"...plus {remaining} more",
                    Font = new Font("Segoe UI", 11),
                    AutoSize = true,
                    Location = new Point(outerPanel.Padding.Left, moreLabelY),
                    ForeColor = CustomColors.Text
                };
                outerPanel.Controls.Add(moreLabel);
            }

            return outerPanel;
        }
        private void AddPanels(List<Panel> panelsToAdd)
        {
            List<Panel> panelsToRemove = _centeredFlowPanel.Controls.OfType<Panel>().ToList();
            foreach (Panel? panel in panelsToRemove)
            {
                _centeredFlowPanel.Controls.Remove(panel);
            }

            foreach (Panel panel in panelsToAdd)
            {
                _centeredFlowPanel.Controls.Add(panel);
            }

            _centeredFlowPanel.SuspendLayout();
            Controls.Add(_centeredFlowPanel);
            ThemeManager.CustomizeScrollBar(_centeredFlowPanel);
            _centeredFlowPanel.ResumeLayout();
            _centeredFlowPanel.Left = (ClientSize.Width - _centeredFlowPanel.Width) / 2;
        }

        // Load spreadsheets
        private const string _accountantsName = "Accountants", _companiesName = "Companies", _purchaseProductsName = "Purchase products",
            _saleProductsName = "Sale products", _purchasesName = "Purchases", _salesName = "Sales";
        private Task<List<Panel>> LoadSpreadsheetData()
        {
            if (string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                return Task.FromResult(new List<Panel>());
            }

            return Task.Run(() =>
            {
                List<Panel> panels = [];

                try
                {
                    // Open the file in read-only and shared mode in case the file is being used by another program
                    using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using XLWorkbook workbook = new(stream);

                    if (workbook.Worksheets.Count == 0)
                    {
                        ShowNoDataMessage("This spreadsheet doesn't contain any sheets");
                        return panels;
                    }

                    bool hasAnyData = false;
                    List<string> emptyWorksheets = [];

                    // Check each expected worksheet
                    hasAnyData |= ProcessWorksheet(workbook, _accountantsName,
                        ws => ExtractFirstCellsFromSimpleList(ws, Accountants_Form.Column.AccountantName),
                        panels, emptyWorksheets);

                    hasAnyData |= ProcessWorksheet(workbook, _companiesName,
                        ws => ExtractFirstCellsFromSimpleList(ws, Companies_Form.Column.Company),
                        panels, emptyWorksheets);
                    hasAnyData |= ProcessWorksheet(workbook, _purchaseProductsName, ExtractProducts, panels, emptyWorksheets);
                    hasAnyData |= ProcessWorksheet(workbook, _saleProductsName, ExtractProducts, panels, emptyWorksheets);
                    hasAnyData |= ProcessWorksheet(workbook, _purchasesName, ExtractTransactions, panels, emptyWorksheets);
                    hasAnyData |= ProcessWorksheet(workbook, _salesName, ExtractTransactions, panels, emptyWorksheets);

                    // Handle case where no data was found in any worksheet
                    if (!hasAnyData)
                    {
                        HandleNoDataFound(emptyWorksheets);
                    }

                    return panels;
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowWithFormat("Error Reading Spreadsheet",
                        "An error occurred while reading the spreadsheet: {0}",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok,
                        ex.Message);
                    return panels;
                }
            });
        }
        private void HandleNoDataFound(List<string> emptyWorksheets)
        {
            string message;

            if (emptyWorksheets.Count == 0)
            {
                message = "The spreadsheet contains no recognizable data sheets. " +
                          "Please ensure your spreadsheet contains sheets named: " +
                          $"{_accountantsName}, {_companiesName}, {_purchaseProductsName}, " +
                          $"{_saleProductsName}, {_purchasesName}, or {_salesName}.";
            }
            else if (emptyWorksheets.Count == 1)
            {
                message = $"The '{emptyWorksheets[0]}' sheet exists but contains no data to import.";
            }
            else
            {
                message = $"The following sheets exist but contain no data to import: {string.Join(", ", emptyWorksheets)}.";
            }

            this.InvokeIfRequired(() => ShowNoDataMessage(message));
        }
        private static void ShowNoDataMessage(string message)
        {
            CustomMessageBox.ShowWithFormat("No Data Found",
                "{0}\n\nPlease check your spreadsheet and ensure it contains data in the expected format.",
                CustomMessageBoxIcon.Info,
                CustomMessageBoxButtons.Ok,
                message);
        }
        private bool ProcessWorksheet<T>(XLWorkbook workbook, string worksheetName,
            Func<IXLWorksheet, List<T>> extractionMethod, List<Panel> panels, List<string> emptyWorksheets)
        {
            if (!workbook.Worksheets.Any(ws => ws.Name.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;  // Worksheet doesn't exist
            }

            IXLWorksheet worksheet = workbook.Worksheet(worksheetName);

            // Check if worksheet is completely empty
            if (!worksheet.RowsUsed().Any())
            {
                emptyWorksheets.Add(worksheetName);
                return false;
            }

            // Check if this worksheet has any columns
            if (!ExcelColumnHelper.ContainsAnyColumns(worksheet))
            {
                emptyWorksheets.Add($"{worksheetName} (no headers)");
                return false;
            }

            List<T> extractedData = extractionMethod(worksheet);

            if (extractedData.Count > 0)
            {
                Panel panel = CreatePanel(extractedData.Cast<string>().ToList(), worksheet.Name);
                panels.Add(panel);
                return true;
            }
            else
            {
                emptyWorksheets.Add($"{worksheetName} (no valid data)");
                return false;
            }
        }
        private static List<string> ExtractFirstCellsFromSimpleList(IXLWorksheet worksheet, Enum column)
        {
            List<string> firstCells = [];
            IEnumerable<IXLRow> rows = worksheet.RowsUsed().Skip(1);  // Skip header

            foreach (IXLRow row in rows)
            {
                string cellValue = ExcelColumnHelper.GetCellValue(row, column);

                if (!string.IsNullOrWhiteSpace(cellValue))
                {
                    firstCells.Add(cellValue.Trim());
                }
            }

            return firstCells;
        }
        private List<string> ExtractProducts(IXLWorksheet productsWorksheet)
        {
            List<string> products = [];
            IEnumerable<IXLRow> rows = productsWorksheet.RowsUsed().Skip(1);  // Skip header

            foreach (IXLRow row in rows)
            {
                string productName = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.ProductName);
                string categoryName = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.ProductCategory);

                // Only add if both fields have values
                if (!string.IsNullOrWhiteSpace(productName) && !string.IsNullOrWhiteSpace(categoryName))
                {
                    products.Add($"{categoryName.Trim()} > {productName.Trim()}");
                }
            }

            return products;
        }
        private List<string> ExtractTransactions(IXLWorksheet transactionsWorksheet)
        {
            List<string> transactions = [];
            IEnumerable<IXLRow> rows = transactionsWorksheet.RowsUsed().Skip(1);  // Skip header

            foreach (IXLRow row in rows)
            {
                string productName = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Product);
                string date = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Date);

                // Only add if both fields have values and this is a main transaction row (not an item row)
                if (!string.IsNullOrWhiteSpace(productName) && !string.IsNullOrWhiteSpace(date))
                {
                    // Check if this row has a transaction ID (indicating it's a main transaction, not an item)
                    string transactionId = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ID);
                    if (!string.IsNullOrWhiteSpace(transactionId))
                    {
                        transactions.Add($"{productName.Trim()} - {date.Trim()}");
                    }
                }
            }

            return transactions;
        }
        // Spreadsheet helper methods
        private void ShowSpreadsheetLabel(string spreadsheetName)
        {
            SelectedSpreadsheet_Label.Text = spreadsheetName;

            // Show the controls
            Controls.Add(SelectedSpreadsheet_Label);
            Controls.Add(RemoveSpreadsheet_ImageButton);

            PositionLabelAndImageButtonBelowButton(
                SelectSpreadsheet_Button,
                SelectedSpreadsheet_Label,
                RemoveSpreadsheet_ImageButton);
        }
        private void RemoveSpreadsheetLabel()
        {
            Controls.Remove(SelectedSpreadsheet_Label);
            Controls.Remove(RemoveSpreadsheet_ImageButton);
            _spreadsheetFilePath = "";
        }

        // Receipt folder helper methods
        private void ShowReceiptsFolderLabel()
        {
            if (!Controls.Contains(SelectedSpreadsheet_Label))
            {
                return;
            }

            if (Directory.Exists(_receiptsFolderPath))
            {
                SelectedReceiptsFolder_Label.Text = Path.GetFileName(_receiptsFolderPath);
                SelectReceiptsFolder_Button.Text = LanguageManager.TranslateString("Change folder");
            }
            else
            {
                SelectedReceiptsFolder_Label.Text = LanguageManager.TranslateString("Select receipts folder");
                SelectReceiptsFolder_Button.Text = LanguageManager.TranslateString("Select receipts folder");
            }

            // Show the controls
            Controls.Add(SelectedReceiptsFolder_Label);
            Controls.Add(RemoveReceiptsFolder_ImageButton);

            PositionLabelAndImageButtonBelowButton(
                SelectReceiptsFolder_Button,
                SelectedReceiptsFolder_Label,
                RemoveReceiptsFolder_ImageButton);
        }
        private void RemoveReceiptsFolderLabel()
        {
            Controls.Remove(SelectedReceiptsFolder_Label);
            Controls.Remove(RemoveReceiptsFolder_ImageButton);
            _receiptsFolderPath = "";
        }

        // Helper methods
        private static void PositionLabelAndImageButtonBelowButton(
            Control button,
            Label label,
            Control imageButton)
        {
            // Position label below the button
            label.Location = new Point(
                button.Left,
                button.Bottom + CustomControls.SpaceBetweenControls);

            // Position image button to the right of the label and vertically centered with the label
            imageButton.Location = new Point(
                label.Right + CustomControls.SpaceBetweenControls,
                button.Bottom + (imageButton.Height - label.Height) / 2);
        }

        // Other methods
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}