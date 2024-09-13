using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.ImportSpreadSheets
{
    public partial class ImportSpreadSheets_Form : Form
    {
        // Properties
        private string spreadsheetFilePath;
        private readonly MainMenu_Form.SelectedOption oldOption;

        // Init.
        public ImportSpreadSheets_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            InitLoadingComponents();
            InitContainerPanel();
            Theme.SetThemeForForm(this);
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

                List<Panel> panels = await LoadSpreadsheetData();

                if (panels.Count > 0)
                {
                    Import_Button.Enabled = true;
                    HideLoadingIndicator();
                    AddPanels(panels);
                }
            }
        }
        private async void Import_Button_Click(object sender, EventArgs e)
        {
            if (!ValidateSpreadsheet()) { return; }

            ShowLoadingIndicator();

            try
            {
                await Task.Run(() =>
                {
                    using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using XLWorkbook workbook = new(stream);
                    List<FlowLayoutPanel> panels = new();
                    bool skipheader = SkipHeaderRow_CheckBox.Checked;

                    // Importing each worksheet data
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("accountants", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet accountantsWorksheet = workbook.Worksheet("Accountants");
                        MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Accountants;
                        Invoke(() => SpreadsheetManager.ImportAccountantsData(accountantsWorksheet, skipheader));
                    }
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("companies", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet companiesWorksheet = workbook.Worksheet("Companies");
                        MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Companies;
                        Invoke(() => SpreadsheetManager.ImportCompaniesData(companiesWorksheet, skipheader));
                    }
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("purchase products", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet productsWorksheet = workbook.Worksheet("Purchase products");
                        MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategoryPurchases;
                        Invoke(() => SpreadsheetManager.ImportProductsData(productsWorksheet, true, skipheader));
                    }
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("sale products", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet productsWorksheet = workbook.Worksheet("Sale products");
                        MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                        Invoke(() => SpreadsheetManager.ImportProductsData(productsWorksheet, false, skipheader));
                    }
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("purchases", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet purchaseWorksheet = workbook.Worksheet("Purchases");
                        Invoke(() => SpreadsheetManager.ImportPurchaseData(purchaseWorksheet, skipheader));
                    }
                    if (workbook.Worksheets.Any(ws => ws.Name.Equals("sales", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        IXLWorksheet salesWorksheet = workbook.Worksheet("Sales");
                        Invoke(() => SpreadsheetManager.ImportSalesData(salesWorksheet, skipheader));
                    }
                });

                CustomMessage_Form.AddThingThatHasChanged(MainMenu_Form.ThingsThatHaveChangedInFile, $"Imported {Path.GetFileName(spreadsheetFilePath)}");
                CustomMessageBox.Show("Argo Sales Tracker", "Spreadsheet imported successfully", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Argo Sales Tracker", $"An error occurred while importing the spreadsheet: {ex.Message}", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }
            finally
            {
                HideLoadingIndicator();
                RemoveReceiptLabel();
                Import_Button.Enabled = false;
                Controls.Remove(centeredFlowPanel);
            }
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
            Height = MinimumSize.Height;
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
                HideLoadingIndicator();
                AddPanels(panels);
            }
        }
        private void SkipHeaderRow_Label_Click(object sender, EventArgs e)
        {
            SkipHeaderRow_CheckBox.Checked = !SkipHeaderRow_CheckBox.Checked;
        }

        // Show things to import
        private readonly byte panelPadding = 10, panelHeight = 200;
        private readonly int panelWidth = 300;
        private CenteredFlowLayoutPanel centeredFlowPanel;
        private List<Panel> panels = new();
        private Panel CreateFlowLayoutPanel(List<string> items, string title)
        {
            Panel outerPanel = new()
            {
                Size = new Size(panelWidth, panelHeight),
                Margin = new Padding(panelPadding),
                BackColor = CustomColors.mainBackground
            };

            // Title for the section
            Label titleLabel = new()
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(outerPanel.Padding.Left, outerPanel.Padding.Top),
                ForeColor = CustomColors.text
            };
            outerPanel.Controls.Add(titleLabel);

            int flowPanelY = titleLabel.Height + titleLabel.Location.Y + UI.spaceBetweenControls;

            FlowLayoutPanel flowPanel = new()
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Size = new Size(outerPanel.Width, outerPanel.Height - flowPanelY),
                Padding = new Padding(panelPadding),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(outerPanel.Padding.Left, flowPanelY)
            };

            // Display the first five items
            for (int i = 0; i < Math.Min(items.Count, 5); i++)
            {
                Label itemLabel = new()
                {
                    Text = items[i],
                    Font = new Font("Segoe UI", 11),
                    AutoSize = true,
                    ForeColor = CustomColors.text
                };
                flowPanel.Controls.Add(itemLabel);
            }
            outerPanel.Controls.Add(flowPanel);

            // If there are more than 5 items, show a "plus (x) more" label
            int moreLabelY = flowPanel.Location.Y + flowPanel.Height + UI.spaceBetweenControls;
            int remaining = items.Count - (SkipHeaderRow_CheckBox.Checked ? 5 : 6);

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
                Size = new Size(panelPadding * 6 + panelWidth * 3 + 50, panelHeight + panelPadding * 2),
                Top = 240
            };
        }
        private void AddPanels(List<Panel> panelsToAdd)
        {
            panels = panelsToAdd;

            List<Panel> panelsToRemove = centeredFlowPanel.Controls.OfType<Panel>().ToList();
            foreach (Panel? panel in panelsToRemove)
            {
                centeredFlowPanel.Controls.Remove(panel);
            }

            foreach (Panel panel in panels)
            {
                centeredFlowPanel.Controls.Add(panel);
            }
        }

        // Loading controls
        private Guna2WinProgressIndicator loadingIndicator;
        private Timer loadingTimer;
        private bool canRemoveLoader;
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
            loadingIndicator.Location = new Point((ClientSize.Width - loadingIndicator.Width) / 2, 270);
        }
        private async void HideLoadingIndicator()
        {
            while (!canRemoveLoader)
            {
                await Task.Delay(10);
            }

            Controls.Remove(loadingIndicator);
            Controls.Add(centeredFlowPanel);
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

                // Open the file in read-only and shared mode
                using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);

                if (workbook.Worksheets.Count == 0)
                {
                    Invoke(() => CustomMessageBox.Show("Argo Sales Tracker", "This spreadsheet doesn't contain any sheets", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok));
                    return panels;
                }

                if (workbook.Worksheets.Any(ws => ws.Name.Equals("accountants", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet accountantsWorksheet = workbook.Worksheet("Accountants");
                    List<string> accountants = ExtractFirstCells(accountantsWorksheet);

                    if (accountants.Count > 0)
                    {
                        Panel panel = CreateFlowLayoutPanel(accountants, "Accountants");
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals("companies", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet companiesWorksheet = workbook.Worksheet("Companies");
                    List<string> companies = ExtractFirstCells(companiesWorksheet);

                    if (companies.Count > 0)
                    {
                        Panel panel = CreateFlowLayoutPanel(companies, "Companies");
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals("purchase products", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet("purchase products");
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreateFlowLayoutPanel(products, "Purchase products");
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals("Sale products", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet productsWorksheet = workbook.Worksheet("sale products");
                    List<string> products = ExtractProducts(productsWorksheet);

                    if (products.Count > 0)
                    {
                        Panel panel = CreateFlowLayoutPanel(products, "Sale products");
                        panels.Add(panel);
                    }
                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals("purchases", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet purchaseWorksheet = workbook.Worksheet("Purchases");

                }
                if (workbook.Worksheets.Any(ws => ws.Name.Equals("sales", StringComparison.CurrentCultureIgnoreCase)))
                {
                    IXLWorksheet salesWorksheet = workbook.Worksheet("Sales");

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
                SelectedReceipt_Label.Right + UI.spaceBetweenControls,
                SelectFile_Button.Bottom + UI.spaceBetweenControls);
        }
        private void RemoveReceiptLabel()
        {
            Controls.Remove(SelectedReceipt_Label);
            Controls.Remove(RemoveReceipt_ImageButton);
            spreadsheetFilePath = "";
        }
    }
}