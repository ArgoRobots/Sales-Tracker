using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

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
                RemoveAllFlowLayoutPanels();

                ShowReceiptLabel(dialog.SafeFileName);
                ShowLoadingIndicator("Loading spreadsheet data");

                List<FlowLayoutPanel> panels = await LoadSpreadsheetDataAsync();

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

                    // Importing each worksheet data
                    if (workbook.Worksheets.Contains("Accountants"))
                    {
                        IXLWorksheet accountantsWorksheet = workbook.Worksheet("Accountants");
                        SpreadsheetManager.ImportAccountantsData(accountantsWorksheet);
                    }
                    if (workbook.Worksheets.Contains("Companies"))
                    {
                        IXLWorksheet companiesWorksheet = workbook.Worksheet("Companies");
                        SpreadsheetManager.ImportCompaniesData(companiesWorksheet);
                    }
                    if (workbook.Worksheets.Contains("Products"))
                    {
                        IXLWorksheet productsWorksheet = workbook.Worksheet("Products");
                        SpreadsheetManager.ImportProductsData(productsWorksheet);
                    }
                    if (workbook.Worksheets.Contains("Purchases"))
                    {
                        IXLWorksheet purchaseWorksheet = workbook.Worksheet("Purchases");
                        SpreadsheetManager.ImportPurchaseData(purchaseWorksheet);
                    }
                    if (workbook.Worksheets.Contains("Sales"))
                    {
                        IXLWorksheet salesWorksheet = workbook.Worksheet("Sales");
                        SpreadsheetManager.ImportSalesData(salesWorksheet);
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
                RemoveAllFlowLayoutPanels();
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
            RemoveAllFlowLayoutPanels();
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

        // Show things to import
        private static FlowLayoutPanel CreateFlowLayoutPanel(List<string> items, string title)
        {
            FlowLayoutPanel panel = new()
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Title for the section
            Label titleLabel = new()
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = CustomColors.text
            };
            panel.Controls.Add(titleLabel);

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
                panel.Controls.Add(itemLabel);
            }

            // If there are more than 5 items, show a "plus (x) more" label
            if (items.Count >= 5)
            {
                Label moreLabel = new()
                {
                    Text = $"...plus {items.Count - 5} more",
                    Font = new Font("Segoe UI", 11),
                    AutoSize = true,
                    ForeColor = CustomColors.text
                };
                panel.Controls.Add(moreLabel);
            }

            return panel;
        }
        private void ArrangePanels(List<FlowLayoutPanel> panels)
        {
            RemoveAllFlowLayoutPanels();

            FlowLayoutPanel containerPanel = new()
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                Padding = new Padding(20),
                Anchor = AnchorStyles.None
            };

            // Add each FlowLayoutPanel to the container
            foreach (FlowLayoutPanel panel in panels)
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
            containerPanel.Location = new Point((ClientSize.Width - containerPanel.PreferredSize.Width) / 2, 250);

            // Add the container to the form
            Controls.Add(containerPanel);
            containerPanel.BringToFront();
        }
        private void RemoveAllFlowLayoutPanels()
        {
            foreach (Control control in Controls)
            {
                if (control is FlowLayoutPanel)
                {
                    Controls.Remove(control);
                }
            }
        }

        // Loading controls
        private Guna2WinProgressIndicator progressIndicator;
        private Label loadingLabel;
        private void InitLoadingComponents()
        {
            // Create and configure the loading label
            loadingLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = CustomColors.text,
                Anchor = AnchorStyles.Top
            };

            // Create and configure the Guna2WinProgressIndicator
            progressIndicator = new Guna2WinProgressIndicator
            {
                AutoStart = true,
                ProgressColor = CustomColors.accent_blue,
                Anchor = AnchorStyles.Top
            };
        }
        private void ShowLoadingIndicator(string text)
        {
            loadingLabel.Text = text;
            Controls.Add(progressIndicator);
            Controls.Add(loadingLabel);

            // Position the label and progress indicator in the center of the form
            progressIndicator.Location = new Point((ClientSize.Width - progressIndicator.Width) / 2, 270);
            loadingLabel.Location = new Point((ClientSize.Width - loadingLabel.Width) / 2, progressIndicator.Bottom + 30);
        }
        private void HideLoadingIndicator()
        {
            Controls.Remove(progressIndicator);
            Controls.Remove(loadingLabel);
        }

        // Methods

        private Task<List<FlowLayoutPanel>> LoadSpreadsheetDataAsync()
        {
            return Task.Run(() =>
            {
                List<FlowLayoutPanel> panels = new();

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
                // Additional worksheet processing can be added similarly

                return panels;
            });
        }
        private static List<string> ExtractFirstCells(IXLWorksheet worksheet)
        {
            List<string> firstCells = new();

            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))  // Skip header row
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