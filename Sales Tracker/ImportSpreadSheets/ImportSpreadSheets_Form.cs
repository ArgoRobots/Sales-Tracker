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

        // Init.
        public ImportSpreadSheets_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowBlankLoadingPanel(this);
            InitLoadingComponents();
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
                Controls.Remove(containerPanel);

                ShowReceiptLabel(dialog.SafeFileName);
                ShowLoadingIndicator("Loading spreadsheet data");

                List<Panel> panels = await LoadSpreadsheetData();

                ArrangePanels(panels);
                Import_Button.Enabled = true;
                HideLoadingIndicator();
            }
        }
        private async void Import_Button_Click(object sender, EventArgs e)
        {
            if (!ValidateSpreadsheet()) { return; }

            ShowLoadingIndicator("Importing spreadsheet");
            Application.DoEvents();

            try
            {
                await Task.Run(() =>
                {
                    using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using XLWorkbook workbook = new(stream);
                    List<FlowLayoutPanel> panels = new();
                    bool skipheader = SkipHeaderRow_CheckBox.Checked;

                    // Importing each worksheet data
                    if (workbook.Worksheets.Contains("Accountants"))
                    {
                        IXLWorksheet accountantsWorksheet = workbook.Worksheet("Accountants");
                        SpreadsheetManager.ImportAccountantsData(accountantsWorksheet, skipheader);
                    }
                    if (workbook.Worksheets.Contains("Companies"))
                    {
                        IXLWorksheet companiesWorksheet = workbook.Worksheet("Companies");
                        SpreadsheetManager.ImportCompaniesData(companiesWorksheet, skipheader);
                    }
                    if (workbook.Worksheets.Contains("Products"))
                    {
                        IXLWorksheet productsWorksheet = workbook.Worksheet("Products");
                        SpreadsheetManager.ImportProductsData(productsWorksheet, skipheader);
                    }
                    if (workbook.Worksheets.Contains("Purchases"))
                    {
                        IXLWorksheet purchaseWorksheet = workbook.Worksheet("Purchases");
                        SpreadsheetManager.ImportPurchaseData(purchaseWorksheet, skipheader);
                    }
                    if (workbook.Worksheets.Contains("Sales"))
                    {
                        IXLWorksheet salesWorksheet = workbook.Worksheet("Sales");
                        SpreadsheetManager.ImportSalesData(salesWorksheet, skipheader);
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
                Controls.Remove(containerPanel);
                Import_Button.Enabled = false;
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
            Controls.Remove(containerPanel);
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
            Import_Button.Enabled = false;
            Controls.Remove(containerPanel);

            ShowLoadingIndicator("Reloading spreadsheet data");

            List<Panel> panels = await LoadSpreadsheetData();

            ArrangePanels(panels);
            Import_Button.Enabled = true;
            HideLoadingIndicator();
        }
        private void SkipHeaderRow_Label_Click(object sender, EventArgs e)
        {
            SkipHeaderRow_CheckBox.Checked = !SkipHeaderRow_CheckBox.Checked;
        }

        // Show things to import
        private Panel CreateFlowLayoutPanel(List<string> items, string title)
        {
            Panel outerPanel = new()
            {
                Padding = new Padding(15),
                AutoSize = true,
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
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
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
        private FlowLayoutPanel containerPanel;
        private void ArrangePanels(List<Panel> panels)
        {
            Controls.Remove(containerPanel);

            containerPanel = new()
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                Padding = new Padding(20),
                Anchor = AnchorStyles.None
            };

            // Add each Panel to the container
            foreach (Panel panel in panels)
            {
                containerPanel.Controls.Add(panel);
            }

            // Calculate if the container exceeds the form width
            int containerWidth = containerPanel.PreferredSize.Width;
            int formWidth = ClientSize.Width;

            // If the container width is larger than the form, make the form taller and add rows
            if (containerWidth > formWidth)
            {
                Height += (containerWidth / formWidth) * 50;
                containerPanel.FlowDirection = FlowDirection.TopDown;
            }

            // Set the container panel in the center of the form
            containerPanel.Location = new Point((ClientSize.Width - containerPanel.PreferredSize.Width) / 2, 240);
        }

        // Loading controls
        private Guna2WinProgressIndicator loadingIndicator;
        private Label loadingLabel;
        private Timer loadingTimer;
        private bool canRemoveLoader;
        private void InitLoadingComponents()
        {
            loadingLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.text,
                Anchor = AnchorStyles.Top
            };

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
        private void ShowLoadingIndicator(string text)
        {
            loadingTimer.Start();
            canRemoveLoader = false;

            loadingLabel.Text = text;
            Controls.Add(loadingIndicator);
            Controls.Add(loadingLabel);

            // Center controls
            loadingIndicator.Location = new Point((ClientSize.Width - loadingIndicator.Width) / 2, 270);
            loadingLabel.Location = new Point((ClientSize.Width - loadingLabel.Width) / 2, loadingIndicator.Bottom + 30);
        }
        private async void HideLoadingIndicator()
        {
            while (!canRemoveLoader)
            {
                await Task.Delay(10);
            }

            Controls.Remove(loadingIndicator);
            Controls.Remove(loadingLabel);
            Controls.Add(containerPanel);
        }

        // Methods
        private Task<List<Panel>> LoadSpreadsheetData()
        {
            return Task.Run(() =>
            {
                List<Panel> panels = new();

                // Open the file in read-only and shared mode
                using FileStream stream = new(spreadsheetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using XLWorkbook workbook = new(stream);

                if (workbook.Worksheets.Count == 0)
                {
                    // Safely call the message box from the UI thread
                    Invoke(() => CustomMessageBox.Show("Argo Sales Tracker", "This spreadsheet doesn't contain any sheets", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok));
                    return panels;
                }

                if (workbook.Worksheets.Contains("Accountants"))
                {
                    IXLWorksheet accountantsWorksheet = workbook.Worksheet("Accountants");
                    List<string> accountants = ExtractFirstCells(accountantsWorksheet);
                    panels.Add(CreateFlowLayoutPanel(accountants, "Accountants"));
                }
                if (workbook.Worksheets.Contains("Companies"))
                {
                    IXLWorksheet companiesWorksheet = workbook.Worksheet("Companies");
                    List<string> companies = ExtractFirstCells(companiesWorksheet);
                    panels.Add(CreateFlowLayoutPanel(companies, "Companies"));
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
        }
    }
}