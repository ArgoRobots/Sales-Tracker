using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ImportSpreadsheet
{
    public partial class ImportSpreadsheet_Form : Form
    {
        // Properties
        private string _spreadsheetFilePath, _receiptsFolderPath;
        private readonly MainMenu_Form.SelectedOption _oldOption;

        // Init.
        public ImportSpreadsheet_Form()
        {
            InitializeComponent();

            _oldOption = MainMenu_Form.Instance.Selected;
            InitContainerPanel();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            RemoveSpreadsheetLabel();
            RemoveReceiptsFolderLabel();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Import_Button);
            ThemeManager.MakeGButtonBlueSecondary(SelectSpreadsheet_Button);
            ThemeManager.MakeGButtonBlueSecondary(SelectReceiptsFolder_Button);
        }
        private void SetAccessibleDescriptions()
        {
            IncludeHeaderRow_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
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

                AutoDetectReceiptsFolder();

                ShowSpreadsheetLabel(dialog.SafeFileName);
                ShowReceiptsFolderLabel();

                await RefreshPanelsAsync();
            }
        }
        private bool ValidateSpreadsheet()
        {
            try
            {
                using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);
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
        private void RemoveSpreadsheet_ImageButton_Click(object sender, EventArgs e)
        {
            RemoveSpreadsheetLabel();
            Controls.Remove(_centeredFlowPanel);
            Import_Button.Enabled = false;
            _receiptsFolderPath = "";
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
            if (_centeredFlowPanel != null)
            {
                foreach (Panel panel in _centeredFlowPanel.Controls.OfType<Panel>())
                {
                    Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                    string worksheetName = panel.Tag.ToString();
                    if (checkBox != null)
                    {
                        checkboxStates[worksheetName] = checkBox.Checked;
                    }
                }
            }

            List<Panel> panels = await Task.Run(LoadSpreadsheetData);

            // Set checkbox states
            foreach (Panel panel in panels)
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                string worksheetName = panel.Tag.ToString();

                if (checkBox != null)
                {
                    // Use saved state if available
                    checkBox.Checked = !checkboxStates.TryGetValue(worksheetName, out bool savedState) || savedState;
                }
            }

            if (panels.Count > 0)
            {
                Import_Button.Enabled = true;
                AddPanels(panels);
            }
            LoadingPanel.HideLoadingScreen(this);
        }

        // Import with rollback support
        private async void Import_Button_Click(object sender, EventArgs e)
        {
            if (!ValidateSpreadsheet()) { return; }

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
                else if (summary.SuccessfulImports > 0)
                {
                    // Commit all changes
                    ExcelSheetManager.CommitImportSession(importSession);
                    importSuccessful = true;

                    MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
                    MainMenu_Form.Instance.UpdateTotalLabels();
                    LoadingPanel.HideLoadingScreen(this);

                    ShowImportSuccessMessage(summary);

                    string message = $"Imported '{Path.GetFileName(_spreadsheetFilePath)}'";
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

                CustomMessageBox.Show("Error", $"An error occurred while importing the spreadsheet: {ex.Message}. All changes have been rolled back.",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
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
            string message = $"Successfully imported {summary.SuccessfulImports} transaction{(summary.SuccessfulImports == 1 ? "" : "s")}.";

            if (summary.SkippedRows > 0)
            {
                message += $"\n\n{summary.SkippedRows} transaction{(summary.SkippedRows == 1 ? "" : "s")} {(summary.SkippedRows == 1 ? "was" : "were")} skipped due to error{(summary.SkippedRows == 1 ? "" : "s")}.";
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
            bool includeheader = IncludeHeaderRow_CheckBox.Checked;

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

                // If the sheet no longer exists. The user may have deleted a sheet after selecting the spreadsheet file
                if (!workbook.Worksheets.Any(ws => ws.Name.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    CustomMessageBox.Show("Sheet no longer exists",
                        $"The sheet {worksheetName} no longer exists and will not be imported",
                        CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    continue;
                }

                IXLWorksheet worksheet = workbook.Worksheet(worksheetName);

                switch (worksheetName)
                {
                    case _accountantsName:
                        bool accountantsImported = ExcelSheetManager.ImportAccountantsData(worksheet, includeheader, importSession);
                        if (accountantsImported)
                        {
                            aggregatedSummary.SuccessfulImports += CountRowsToImport(worksheet, includeheader);
                        }
                        break;

                    case _companiesName:
                        bool companiesImported = ExcelSheetManager.ImportCompaniesData(worksheet, includeheader, importSession);
                        if (companiesImported)
                        {
                            aggregatedSummary.SuccessfulImports += CountRowsToImport(worksheet, includeheader);
                        }
                        break;

                    case _purchaseProductsName:
                        bool purchaseProductsImported = ExcelSheetManager.ImportProductsData(worksheet, true, includeheader, importSession);
                        if (purchaseProductsImported)
                        {
                            aggregatedSummary.SuccessfulImports += CountRowsToImport(worksheet, includeheader);
                        }
                        break;

                    case _saleProductsName:
                        bool saleProductsImported = ExcelSheetManager.ImportProductsData(worksheet, false, includeheader, importSession);
                        if (saleProductsImported)
                        {
                            aggregatedSummary.SuccessfulImports += CountRowsToImport(worksheet, includeheader);
                        }
                        break;

                    case _purchasesName:
                        ExcelSheetManager.ImportSummary purchaseSummary = ExcelSheetManager.ImportPurchaseData(worksheet, includeheader, importSession);

                        // Aggregate the results
                        aggregatedSummary.SuccessfulImports += purchaseSummary.SuccessfulImports;
                        aggregatedSummary.SkippedRows += purchaseSummary.SkippedRows;
                        aggregatedSummary.Errors.AddRange(purchaseSummary.Errors);

                        if (purchaseSummary.WasCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Check for cancellation after purchase import
                        if (importSession.IsCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Import receipts if receipts folder is specified
                        if (!string.IsNullOrEmpty(_receiptsFolderPath) && Directory.Exists(_receiptsFolderPath))
                        {
                            int importedReceiptCount = ExcelSheetManager.ImportReceiptsData(worksheet, includeheader, _receiptsFolderPath, true);  // true for purchases
                            aggregatedSummary.SuccessfulImports += importedReceiptCount;
                        }
                        break;

                    case _salesName:
                        ExcelSheetManager.ImportSummary salesSummary = ExcelSheetManager.ImportSalesData(worksheet, includeheader, importSession);

                        // Aggregate the results
                        aggregatedSummary.SuccessfulImports += salesSummary.SuccessfulImports;
                        aggregatedSummary.SkippedRows += salesSummary.SkippedRows;
                        aggregatedSummary.Errors.AddRange(salesSummary.Errors);

                        if (salesSummary.WasCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Check for cancellation after sales import
                        if (importSession.IsCancelled)
                        {
                            aggregatedSummary.WasCancelled = true;
                            break;
                        }

                        // Import receipts if receipts folder is specified
                        if (!string.IsNullOrEmpty(_receiptsFolderPath) && Directory.Exists(_receiptsFolderPath))
                        {
                            int importedReceiptCount = ExcelSheetManager.ImportReceiptsData(worksheet, includeheader, _receiptsFolderPath, false);  // false for sales
                            aggregatedSummary.SuccessfulImports += importedReceiptCount;
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
        private static int CountRowsToImport(IXLWorksheet worksheet, bool includeHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

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
            Tools.OpenLink("https://argorobots.com/documentation/index.html#spreadsheets");
        }
        private async void IncludeHeaderRow_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            await RefreshPanelsAsync();
        }
        private void IncludeHeaderRow_Label_Click(object sender, EventArgs e)
        {
            IncludeHeaderRow_CheckBox.Checked = !IncludeHeaderRow_CheckBox.Checked;
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

            // Check for various receipts folder naming conventions
            string[] possibleFolderNames =
            [
                $"{spreadsheetNameWithoutExtension}_receipts",
                $"{spreadsheetNameWithoutExtension}_Receipts",
                $"{spreadsheetNameWithoutExtension} receipts",
                $"{spreadsheetNameWithoutExtension} Receipts",
                "receipts",
                "Receipts"
            ];

            foreach (string folderName in possibleFolderNames)
            {
                string possiblePath = Path.Combine(spreadsheetDirectory, folderName);
                if (Directory.Exists(possiblePath))
                {
                    _receiptsFolderPath = possiblePath;
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
                Top = 240,
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
            ThemeManager.SetThemeForControl([customCheckBox]);
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

                // Open the file in read-only and shared mode in case the file is being used by another program
                using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);

                if (workbook.Worksheets.Count == 0)
                {
                    CustomMessageBox.Show("Spreadsheet is invalid", "This spreadsheet doesn't contain any sheets",
                        CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    return panels;
                }

                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_accountantsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet accountantsWorksheet = workbook.Worksheet(_accountantsName);
                    List<string> accountants = ExtractFirstCells(accountantsWorksheet);

                    if (accountants.Count > 0)
                    {
                        Panel panel = CreatePanel(accountants, accountantsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_companiesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet companiesWorksheet = workbook.Worksheet(_companiesName);
                    List<string> companies = ExtractFirstCells(companiesWorksheet);

                    if (companies.Count > 0)
                    {
                        Panel panel = CreatePanel(companies, companiesWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_purchaseProductsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet(_purchaseProductsName);
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, productsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_saleProductsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet(_saleProductsName);
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, productsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_purchasesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet purchaseWorksheet = workbook.Worksheet(_purchasesName);
                    List<string> products = ExtractTransactions(purchaseWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, purchaseWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(_salesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet salesWorksheet = workbook.Worksheet(_salesName);
                    List<string> products = ExtractTransactions(salesWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, salesWorksheet.Name);
                        panels.Add(panel);
                    }
                }

                return panels;
            });
        }
        private List<string> ExtractFirstCells(IXLWorksheet worksheet)
        {
            List<string> firstCells = [];
            IEnumerable<IXLRow> rows = IncludeHeaderRow_CheckBox.Checked
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            foreach (IXLRow row in rows)
            {
                string firstCellValue = row.Cell(1).GetValue<string>();
                firstCells.Add(firstCellValue);
            }

            return firstCells;
        }
        private List<string> ExtractProducts(IXLWorksheet productsWorksheet)
        {
            List<string> products = [];

            IEnumerable<IXLRow> rows = IncludeHeaderRow_CheckBox.Checked
                ? productsWorksheet.RowsUsed()
                : productsWorksheet.RowsUsed().Skip(1);

            foreach (IXLRow row in rows)
            {
                // Ensure the cells have values before attempting to read
                string productName = row.Cell(2).GetValue<string>();
                string categoryName = row.Cell(3).GetValue<string>();

                if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(categoryName))
                {
                    products.Add($"{categoryName} > {productName}");
                }
            }

            return products;
        }
        private List<string> ExtractTransactions(IXLWorksheet productsWorksheet)
        {
            List<string> products = [];
            IEnumerable<IXLRow> rows = IncludeHeaderRow_CheckBox.Checked
                ? productsWorksheet.RowsUsed()
                : productsWorksheet.RowsUsed().Skip(1);

            foreach (IXLRow row in rows)
            {
                // Ensure the cells have values before attempting to read
                string productName = row.Cell(3).GetValue<string>();
                string date = row.Cell(7).GetValue<string>();

                if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(date))
                {
                    products.Add($"{productName} - {date}");
                }
            }

            return products;
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

        // Helper method
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
    }
}