using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.UI;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.ImportSpreadsheet
{
    public partial class ImportSpreadsheet_Form : Form
    {
        // Properties
        private string spreadsheetFilePath;
        private readonly MainMenu_Form.SelectedOption oldOption;

        // Init.
        public ImportSpreadsheet_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            InitLoadingComponents();
            InitContainerPanel();
            Theme.SetThemeForForm(this);
            LanguageManager.UpdateLanguageForControl(this);
            RemoveReceiptLabel();
            AlignControls();
        }
        private void AlignControls()
        {
            int spaceAvailable = ClientSize.Width - Import_Button.Right;
            int newLeftPosition = Import_Button.Right + (spaceAvailable - SkipHeaderRow_CheckBox.Width - SkipHeaderRow_Label.Width) / 2;
            int initialSpacing = SkipHeaderRow_Label.Left - SkipHeaderRow_CheckBox.Right;

            SkipHeaderRow_CheckBox.Left = newLeftPosition;
            SkipHeaderRow_Label.Left = SkipHeaderRow_CheckBox.Right + initialSpacing;
        }

        // Form event handlers
        private void ImportSpreadSheets_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ImportSpreadSheets_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
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
                spreadsheetFilePath = dialog.FileName;
                if (!ValidateSpreadsheet()) { return; }

                Import_Button.Enabled = false;
                Controls.Remove(centeredFlowPanel);

                ShowReceiptLabel(dialog.SafeFileName);
                ShowLoadingIndicator();

                List<Panel> panels = await Task.Run(LoadSpreadsheetData);

                // This needs to be done on the main UI thread for some reason
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

            Controls.Remove(centeredFlowPanel);
            ShowLoadingIndicator();
            Import_Button.Enabled = false;

            try
            {
                if (await Task.Run(ImportSpreadsheet))
                {
                    MainMenu_Form.Instance.RefreshDataGridView();

                    CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Imported '{Path.GetFileName(spreadsheetFilePath)}'");
                    CustomMessageBox.Show("Argo Sales Tracker", "Finished importing spreadsheet", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    CustomMessageBox.Show("Argo Sales Tracker", "Nothing was imported", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Argo Sales Tracker", $"An error occurred while importing the spreadsheet: {ex.Message}", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }

            HideLoadingIndicator();
            RemoveReceiptLabel();
            Close();
        }
        private bool ValidateSpreadsheet()
        {
            try
            {
                using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);
                return true;
            }
            catch
            {
                CustomMessageBox.Show("Argo Sales Tracker",
                    "This spreadsheet is invalid or corrupted. Please choose a valid spreadsheet.",
                     CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);

                return false;
            }
        }
        private void RemoveReceipt_ImageButton_Click(object sender, EventArgs e)
        {
            RemoveReceiptLabel();
            Controls.Remove(centeredFlowPanel);
            Import_Button.Enabled = false;
        }
        private void RemoveReceipt_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.fileHover;
        }
        private void RemoveReceipt_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.mainBackground;
        }
        private void OpenTutorial_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("");
        }
        private async void SkipHeaderRow_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(spreadsheetFilePath))
            {
                return;
            }

            Import_Button.Enabled = false;
            Controls.Remove(centeredFlowPanel);

            ShowLoadingIndicator();

            List<Panel> panels = await LoadSpreadsheetData();

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
        private readonly byte panelPadding = 25, panelHeight = 240;
        private readonly int panelWidth = 300;
        private CenteredFlowLayoutPanel centeredFlowPanel;
        private Panel CreatePanel(List<string> items, string worksheetName)
        {
            Panel outerPanel = new()
            {
                Size = new Size(panelWidth, panelHeight),
                BackColor = CustomColors.background4,
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
                ForeColor = CustomColors.text
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
            Theme.SetThemeForControl([customCheckBox]);
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
                    ForeColor = CustomColors.text,
                    AutoSize = false,
                    Size = new Size(flowPanel.Width - flowPanel.Padding.Left - 5, 30)  // Fixed height for single line
                };

                itemLabel.Text = Tools.ShortenTextWithEllipsis(itemLabel, items[i]);
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
                    ForeColor = CustomColors.text
                };
                outerPanel.Controls.Add(moreLabel);
            }

            return outerPanel;
        }
        private void InitContainerPanel()
        {
            centeredFlowPanel = new()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(panelPadding * 6 + panelWidth * 3 + 50, panelHeight * 2 + panelPadding),
                Top = 240,
                Spacing = panelPadding
            };
        }
        private void AddPanels(List<Panel> panelsToAdd)
        {
            List<Panel> panelsToRemove = centeredFlowPanel.Controls.OfType<Panel>().ToList();
            foreach (Panel? panel in panelsToRemove)
            {
                centeredFlowPanel.Controls.Remove(panel);
            }

            foreach (Panel panel in panelsToAdd)
            {
                centeredFlowPanel.Controls.Add(panel);
            }
        }

        // Loading controls
        private Guna2WinProgressIndicator loadingIndicator;
        private Timer loadingTimer;
        private bool canRemoveLoader;
        private const string accountantsName = "Accountants", companiesName = "Companies", purchaseProductsName = "Purchase products",
            saleProductsName = "Sale products", purchasesName = "Purchases", salesName = "Sales";
        private void InitLoadingComponents()
        {
            loadingIndicator = new Guna2WinProgressIndicator
            {
                AutoStart = true,
                ProgressColor = CustomColors.accent_blue,
                Anchor = AnchorStyles.Top
            };

            loadingTimer = new Timer()
            {
                Interval = 300
            };
            loadingTimer.Tick += (sender, args) =>
            {
                canRemoveLoader = true;
                loadingTimer.Stop();
            };
        }
        private void ShowLoadingIndicator()
        {
            loadingTimer.Start();
            canRemoveLoader = false;

            Controls.Add(loadingIndicator);
            loadingIndicator.Location = new Point((ClientSize.Width - loadingIndicator.Width) / 2, 350);
        }
        private async void HideLoadingIndicator()
        {
            while (!canRemoveLoader)
            {
                await Task.Delay(10);
            }

            centeredFlowPanel.SuspendLayout();  // This prevenets the horizontal scroll bar from flashing

            Controls.Remove(loadingIndicator);
            Controls.Add(centeredFlowPanel);

            Theme.CustomizeScrollBar(centeredFlowPanel);
            centeredFlowPanel.ResumeLayout();
            centeredFlowPanel.Left = (ClientSize.Width - centeredFlowPanel.Width) / 2;
        }

        // Load spreadsheets
        private Task<List<Panel>> LoadSpreadsheetData()
        {
            if (string.IsNullOrEmpty(spreadsheetFilePath))
            {
                return Task.FromResult(new List<Panel>());
            }

            return Task.Run(() =>
            {
                List<Panel> panels = new();

                // Open the file in read-only and shared mode in case the file is being used by another program
                using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);

                if (workbook.Worksheets.Count == 0)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", "This spreadsheet doesn't contain any sheets", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    return panels;
                }

                if (workbook.Worksheets.Any(ws => ws.Name.Equals(accountantsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet accountantsWorksheet = workbook.Worksheet(accountantsName);
                    List<string> accountants = ExtractFirstCells(accountantsWorksheet);

                    if (accountants.Count > 0)
                    {
                        Panel panel = CreatePanel(accountants, accountantsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(companiesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet companiesWorksheet = workbook.Worksheet(companiesName);
                    List<string> companies = ExtractFirstCells(companiesWorksheet);

                    if (companies.Count > 0)
                    {
                        Panel panel = CreatePanel(companies, companiesWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(purchaseProductsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet(purchaseProductsName);
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, productsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(saleProductsName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet(saleProductsName);
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, productsWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(purchasesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet purchaseWorksheet = workbook.Worksheet(purchasesName);
                    List<string> products = ExtractTransaction(purchaseWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreatePanel(products, purchaseWorksheet.Name);
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals(salesName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet salesWorksheet = workbook.Worksheet(salesName);
                    List<string> products = ExtractTransaction(salesWorksheet);

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
            List<string> firstCells = new();

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
            List<string> products = new();
            IEnumerable<IXLRow> rows = SkipHeaderRow_CheckBox.Checked ? productsWorksheet.RowsUsed().Skip(1) : productsWorksheet.RowsUsed();

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
        private List<string> ExtractTransaction(IXLWorksheet productsWorksheet)
        {
            List<string> products = new();
            IEnumerable<IXLRow> rows = SkipHeaderRow_CheckBox.Checked ? productsWorksheet.RowsUsed().Skip(1) : productsWorksheet.RowsUsed();

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
            MainMenu_Form.Instance.isProgramLoading = true;

            using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XLWorkbook workbook = new(stream);
            bool skipheader = SkipHeaderRow_CheckBox.Checked;
            bool wasSomethingImported = false, purchaseImportFailed = false, salesImportFailed = false;

            foreach (Panel panel in centeredFlowPanel.Controls.OfType<Panel>())
            {
                Guna2CustomCheckBox checkBox = panel.Controls.OfType<Guna2CustomCheckBox>().FirstOrDefault();
                string worksheetName = panel.Tag.ToString();

                // If the CheckBox is not checked
                if (checkBox == null || !checkBox.Checked)
                {
                    continue;
                }

                // If the sheet no longer not exists. The user may have deleted a sheet after selecting the spreadsheet file
                if (!workbook.Worksheets.Any(ws => ws.Name.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                IXLWorksheet worksheet = workbook.Worksheet(worksheetName);

                switch (worksheetName)
                {
                    case accountantsName:
                        wasSomethingImported |= SpreadsheetManager.ImportAccountantsData(worksheet, skipheader);
                        break;

                    case companiesName:
                        wasSomethingImported |= SpreadsheetManager.ImportCompaniesData(worksheet, skipheader);
                        break;

                    case purchaseProductsName:
                        wasSomethingImported |= SpreadsheetManager.ImportProductsData(worksheet, true, skipheader);
                        break;

                    case saleProductsName:
                        wasSomethingImported |= SpreadsheetManager.ImportProductsData(worksheet, false, skipheader);
                        break;

                    case purchasesName:
                        (bool connection, bool somethingImported) = SpreadsheetManager.ImportPurchaseData(worksheet, skipheader);
                        if (!connection) { purchaseImportFailed = true; }
                        wasSomethingImported |= somethingImported;
                        break;

                    case salesName:
                        (bool connection1, bool somethingImported2) = SpreadsheetManager.ImportSalesData(worksheet, skipheader);
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

                CustomMessageBox.Show("Argo Sales Tracker",
                    message,
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok);
            }

            MainMenu_Form.Instance.isProgramLoading = false;
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
            spreadsheetFilePath = "";
        }
    }
}