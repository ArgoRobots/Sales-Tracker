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
        private string _spreadsheetFilePath;
        private readonly MainMenu_Form.SelectedOption _oldOption;

        // Init.
        public ImportSpreadsheet_Form()
        {
            InitializeComponent();

            _oldOption = MainMenu_Form.Instance.Selected;
            InitLoadingComponents();
            InitContainerPanel();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            RemoveReceiptLabel();
            AlignControls();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Import_Button);
        }
        private void AlignControls()
        {
            int spaceAvailable = ClientSize.Width - Import_Button.Right;
            int newLeftPosition = Import_Button.Right + (spaceAvailable - SkipHeaderRow_CheckBox.Width - SkipHeaderRow_Label.Width) / 2;
            int initialSpacing = SkipHeaderRow_Label.Left - SkipHeaderRow_CheckBox.Right;

            SkipHeaderRow_CheckBox.Left = newLeftPosition;
            SkipHeaderRow_Label.Left = SkipHeaderRow_CheckBox.Right + initialSpacing;
        }
        private void SetAccessibleDescriptions()
        {
            SkipHeaderRow_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
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

        // Event handlers
        private async void SelectFile_Button_Click(object sender, EventArgs e)
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

                Import_Button.Enabled = false;
                Controls.Remove(_centeredFlowPanel);

                ShowReceiptLabel(dialog.SafeFileName);
                ShowLoadingIndicator();

                List<Panel> panels = await Task.Run(LoadSpreadsheetData);

                foreach (Panel panel in panels)
                {
                    Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                    checkBox.Checked = true;
                }

                if (panels.Count > 0)
                {
                    Import_Button.Enabled = true;
                    AddPanels(panels);
                }
                HideLoadingIndicator();
            }
        }
        private async void Import_Button_Click(object sender, EventArgs e)
        {
            if (!ValidateSpreadsheet()) { return; }

            Controls.Remove(_centeredFlowPanel);
            ShowLoadingIndicator();
            Import_Button.Enabled = false;

            try
            {
                if (await Task.Run(ImportSpreadsheet))
                {
                    MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
                    HideLoadingIndicator();
                    string message = $"Imported '{Path.GetFileName(_spreadsheetFilePath)}'";
                    CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);

                    CustomMessageBox.Show("Imported spreadsheet", "Finished importing spreadsheet",
                        CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    CustomMessageBox.Show("Nothing was imported", "Nothing was imported",
                        CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", $"An error occurred while importing the spreadsheet: {ex.Message}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }

            HideLoadingIndicator();
            RemoveReceiptLabel();
            Close();
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
        private void RemoveReceipt_ImageButton_Click(object sender, EventArgs e)
        {
            RemoveReceiptLabel();
            Controls.Remove(_centeredFlowPanel);
            Import_Button.Enabled = false;
        }
        private void RemoveReceipt_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.MouseHover;
        }
        private void RemoveReceipt_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.MainBackground;
        }
        private void OpenTutorial_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("https://argorobots.com/documentation/index.html#spreadsheets");
        }
        private async void SkipHeaderRow_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_spreadsheetFilePath))
            {
                return;
            }

            // Remember current checkbox states before reloading
            Dictionary<string, bool> checkboxStates = [];
            foreach (Panel panel in _centeredFlowPanel.Controls.OfType<Panel>())
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                string worksheetName = panel.Tag.ToString();
                if (checkBox != null)
                {
                    checkboxStates[worksheetName] = checkBox.Checked;
                }
            }

            Import_Button.Enabled = false;
            Controls.Remove(_centeredFlowPanel);

            ShowLoadingIndicator();

            List<Panel> panels = await LoadSpreadsheetData();

            // Restore checkbox states
            foreach (Panel panel in panels)
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                string worksheetName = panel.Tag.ToString();

                if (checkBox != null)
                {
                    // Use saved state if available, otherwise default to checked
                    checkBox.Checked = !checkboxStates.TryGetValue(worksheetName, out bool savedState) || savedState;
                }
            }

            if (panels.Count > 0)
            {
                Import_Button.Enabled = true;
                AddPanels(panels);
            }
            HideLoadingIndicator();
        }
        private void SkipHeaderRow_Label_Click(object sender, EventArgs e)
        {
            SkipHeaderRow_CheckBox.Checked = !SkipHeaderRow_CheckBox.Checked;
        }

        // Show things to import
        private readonly byte _panelPadding = 25, _panelHeight = 240;
        private readonly int _panelWidth = 300;
        private CenteredFlowLayoutPanel _centeredFlowPanel;
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
                Location = new Point(outerPanel.Padding.Left, outerPanel.Padding.Top),
                ForeColor = CustomColors.Text
            };
            outerPanel.Controls.Add(titleLabel);

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
        }

        // Loading controls
        private Guna2WinProgressIndicator _loadingIndicator;
        private const string _accountantsName = "Accountants", _companiesName = "Companies", _purchaseProductsName = "Purchase products",
            _saleProductsName = "Sale products", _purchasesName = "Purchases", _salesName = "Sales";
        private void InitLoadingComponents()
        {
            _loadingIndicator = new Guna2WinProgressIndicator
            {
                AutoStart = true,
                ProgressColor = CustomColors.AccentBlue,
                Anchor = AnchorStyles.Top
            };
        }
        private void ShowLoadingIndicator()
        {
            Controls.Add(_loadingIndicator);
            _loadingIndicator.Location = new Point((ClientSize.Width - _loadingIndicator.Width) / 2, 350);
        }
        private void HideLoadingIndicator()
        {
            _centeredFlowPanel.SuspendLayout();  // This prevenets the horizontal scroll bar from flashing

            Controls.Remove(_loadingIndicator);
            Controls.Add(_centeredFlowPanel);

            ThemeManager.CustomizeScrollBar(_centeredFlowPanel);
            _centeredFlowPanel.ResumeLayout();
            _centeredFlowPanel.Left = (ClientSize.Width - _centeredFlowPanel.Width) / 2;
        }

        // Load spreadsheets
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
            IEnumerable<IXLRow> rows = SkipHeaderRow_CheckBox.Checked ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();

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

            IEnumerable<IXLRow> rows = SkipHeaderRow_CheckBox.Checked
                ? productsWorksheet.RowsUsed().Skip(1)
                : productsWorksheet.RowsUsed();

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
            IEnumerable<IXLRow> rows = SkipHeaderRow_CheckBox.Checked
                ? productsWorksheet.RowsUsed().Skip(1)
                : productsWorksheet.RowsUsed();

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

        // Import
        private bool ImportSpreadsheet()
        {
            MainMenu_Form.IsProgramLoading = true;

            using FileStream stream = new(_spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XLWorkbook workbook = new(stream);
            bool skipheader = SkipHeaderRow_CheckBox.Checked;
            bool wasSomethingImported = false, purchaseImportFailed = false, salesImportFailed = false;

            foreach (Panel panel in _centeredFlowPanel.Controls.OfType<Panel>())
            {
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
                        wasSomethingImported |= ExcelSheetManager.ImportAccountantsData(worksheet, skipheader);
                        break;

                    case _companiesName:
                        wasSomethingImported |= ExcelSheetManager.ImportCompaniesData(worksheet, skipheader);
                        break;

                    case _purchaseProductsName:
                        wasSomethingImported |= ExcelSheetManager.ImportProductsData(worksheet, true, skipheader);
                        break;

                    case _saleProductsName:
                        wasSomethingImported |= ExcelSheetManager.ImportProductsData(worksheet, false, skipheader);
                        break;

                    case _purchasesName:
                        (bool connection, bool somethingImported) = ExcelSheetManager.ImportPurchaseData(worksheet, skipheader);
                        if (!connection) { purchaseImportFailed = true; }
                        wasSomethingImported |= somethingImported;
                        break;

                    case _salesName:
                        (bool connection1, bool somethingImported2) = ExcelSheetManager.ImportSalesData(worksheet, skipheader);
                        if (!connection1) { salesImportFailed = true; }
                        wasSomethingImported |= somethingImported2;
                        break;
                }
            }

            if (purchaseImportFailed || salesImportFailed)
            {
                string message = "Failed to import ";
                if (purchaseImportFailed) { message += "'Purchases'"; }
                if (purchaseImportFailed && salesImportFailed) { message += " and "; }
                if (salesImportFailed) { message += "'Sales'"; }
                message += " because it looks like you are not connected to the internet. A connection is needed to get the exchange rates. Please check your connection and try again";

                CustomMessageBox.Show("No connection", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
            }

            MainMenu_Form.IsProgramLoading = false;
            return wasSomethingImported;
        }

        // Methods
        private void ShowReceiptLabel(string text)
        {
            SelectedReceipt_Label.Text = text;

            Controls.Add(SelectedReceipt_Label);
            Controls.Add(RemoveReceipt_ImageButton);
            SetReceiptLabelLocation();
        }
        private void SetReceiptLabelLocation()
        {
            if (!Controls.Contains(SelectedReceipt_Label))
            {
                return;
            }

            SelectedReceipt_Label.Location = new Point(
                (ClientSize.Width - RemoveReceipt_ImageButton.Width - SelectedReceipt_Label.Width) / 2,
                RemoveReceipt_ImageButton.Top + (RemoveReceipt_ImageButton.Height - SelectedReceipt_Label.Height) / 2 - 1);

            RemoveReceipt_ImageButton.Location = new Point(
                SelectedReceipt_Label.Right + CustomControls.SpaceBetweenControls,
                SelectFile_Button.Bottom + CustomControls.SpaceBetweenControls);
        }
        private void RemoveReceiptLabel()
        {
            Controls.Remove(SelectedReceipt_Label);
            Controls.Remove(RemoveReceipt_ImageButton);
            _spreadsheetFilePath = "";
        }
    }
}