using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.ComponentModel;

namespace Sales_Tracker
{
    public partial class Receipts_Form : Form
    {
        // Properties
        private DateTime oldestDate;
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Init.
        public Receipts_Form()
        {
            InitializeComponent();

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;

            MainMenu_Form.IsProgramLoading = true;
            DataGridViewManager.InitializeDataGridView(Receipts_DataGridView, "Receipts_DataGridView", Receipts_DataGridView.Size, ColumnHeaders, null, this);
            Receipts_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            MainMenu_Form.Instance.SelectedDataGridView = Receipts_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Receipts;
            AddAllReceiptsAndGetOldestDate();
            Receipts_DataGridView.SelectionChanged += Receipts_DataGridView_SelectionChanged;

            if (Receipts_DataGridView.Rows.Count > 0)
            {
                From_DateTimePicker.Value = oldestDate;
                To_DateTimePicker.Value = DateTime.Now;
            }
            MainMenu_Form.IsProgramLoading = false;

            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Receipts_DataGridView);

            CenterControls();
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(ExportSelected_Button);
            Theme.MakeGButtonBlueSecondary(ClearFilters_Button);
        }
        private void Receipts_DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            ExportSelected_Button.Enabled = Receipts_DataGridView.SelectedRows.Count > 0;
        }
        private void SetAccessibleDescriptions()
        {
            Search_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            FilterByDate_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            From_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            To_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            IncludePurchaseReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            IncludeSaleReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
        }

        // Form event handlers
        private void Receipts_Form_Shown(object sender, EventArgs e)
        {
            CenterControls();
        }
        private void Receipts_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void ClearFilters_Button_Click(object sender, EventArgs e)
        {
            Search_TextBox.Text = "";
            From_DateTimePicker.Value = oldestDate;
            To_DateTimePicker.Value = DateTime.Now;
        }
        private void ExportSelected_Button_Click(object sender, EventArgs e)
        {
            ExportSelectedReceipts(Receipts_DataGridView);
        }
        public static void ExportSelectedReceipts(Guna2DataGridView dataGridView)
        {
            // Select directory
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string destinationPath = dialog.SelectedPath;
                int selectedRowCount = dataGridView.SelectedRows.Count;

                if (selectedRowCount > 1)
                {
                    // Create a new folder for multiple files
                    string newFolderPath = Path.Combine(destinationPath, $"Argo Sales Tracker receipts for {Directories.CompanyName} - "
                        + DateTime.Now.ToString("yyyyMMddHHmmss"));

                    Directory.CreateDirectory(newFolderPath);
                    destinationPath = newFolderPath;
                }

                bool isAnyReceiptExported = false, doAllRowsHaveReceipt = true;

                // Iterate through selected rows and copy files
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    string receipt = DataGridViewManager.GetFilePathFromRowTag(row.Tag);
                    if (receipt == "")
                    {
                        doAllRowsHaveReceipt = false;
                        continue;
                    }

                    receipt = receipt.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName)
                        .Replace(ReadOnlyVariables.Receipt_text, "");

                    if (!File.Exists(receipt))
                    {
                        Log.Error_FileDoesNotExist(receipt);
                        continue;
                    }

                    string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(receipt));
                    Directories.CopyFile(receipt, destinationFilePath);
                    isAnyReceiptExported = true;
                }

                if (isAnyReceiptExported)
                {
                    string message = "Receipts exported successfully";

                    if (!doAllRowsHaveReceipt) { message += " Note: Not all the selected rows contain a receipt."; }

                    CustomMessageBox.Show("Receipts exported",
                        message,
                        CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                }
            }
        }
        private void FilterByDate_Label_Click(object sender, EventArgs e)
        {
            FilterByDate_CheckBox.Checked = !FilterByDate_CheckBox.Checked;
        }
        private void IncludeSaleReceipts_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AddAllReceiptsAndGetOldestDate();
        }
        private void IncludeSaleReceipts_Label_Click(object sender, EventArgs e)
        {
            IncludeSaleReceipts_CheckBox.Checked = !IncludeSaleReceipts_CheckBox.Checked;
        }
        private void IncludePurchaseReceipts_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AddAllReceiptsAndGetOldestDate();
        }
        private void IncludePurchaseReceipts_Label_Click(object sender, EventArgs e)
        {
            IncludePurchaseReceipts_CheckBox.Checked = !IncludePurchaseReceipts_CheckBox.Checked;
        }

        // DataGridView
        public enum Column
        {
            Type,
            ID,
            Product,
            Category,
            Date,
            Total
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.Type, "Type" },
            { Column.ID, "Transaction ID" },
            { Column.Product, "Product name" },
            { Column.Category, "Category" },
            { Column.Date, "Date" },
            { Column.Total, "Total revenue" }
        };

        // Methods
        /// <summary>
        /// Center the checkboxes between form left and DateTimePicker, and search controls between DateTimePicker and form right.
        /// </summary>
        private void CenterControls()
        {
            // Center checkboxes between form left and DateTimePicker
            int leftEdge = 0;
            int rightEdge = From_DateTimePicker.Left;
            int centerX = leftEdge + (rightEdge - leftEdge) / 2;
            int spacing = IncludePurchaseReceipts_Label.Left - IncludePurchaseReceipts_CheckBox.Right;

            // Calculate total widths of the checkboxes and labels
            int purchaseTotalWidth = IncludePurchaseReceipts_CheckBox.Width + spacing + IncludePurchaseReceipts_Label.Width;
            int saleTotalWidth = IncludeSaleReceipts_CheckBox.Width + spacing + IncludeSaleReceipts_Label.Width;
            int maxTotalWidth = Math.Max(purchaseTotalWidth, saleTotalWidth);

            // Compute the left position to center the checkbox controls
            int controlsLeft = centerX - (maxTotalWidth / 2);

            // Position the checkbox controls
            IncludePurchaseReceipts_CheckBox.Left = controlsLeft;
            IncludePurchaseReceipts_Label.Left = IncludePurchaseReceipts_CheckBox.Right + spacing;
            IncludeSaleReceipts_CheckBox.Left = controlsLeft;
            IncludeSaleReceipts_Label.Left = IncludeSaleReceipts_CheckBox.Right + spacing;

            // Center search controls between DateTimePicker right and form right
            int searchLeftEdge = From_DateTimePicker.Right;
            int searchRightEdge = ClientSize.Width;
            int searchCenterX = searchLeftEdge + (searchRightEdge - searchLeftEdge) / 2;
            int searchSpacing = Search_TextBox.Left - Search_Label.Right;

            // Calculate total width of search controls
            int searchTotalWidth = Search_Label.Width + searchSpacing + Search_TextBox.Width;

            // Compute the left position to center the search controls
            int searchControlsLeft = searchCenterX - (searchTotalWidth / 2);

            // Position the search controls
            Search_Label.Left = searchControlsLeft;
            Search_TextBox.Left = Search_Label.Right + searchSpacing;
        }
        private void AddAllReceiptsAndGetOldestDate()
        {
            // Save the current sort order
            DataGridViewColumn sortedColumn = Receipts_DataGridView.SortedColumn;
            ListSortDirection sortDirection = ListSortDirection.Ascending;
            if (Receipts_DataGridView.SortOrder == SortOrder.Descending)
            {
                sortDirection = ListSortDirection.Descending;
            }

            Receipts_DataGridView.Rows.Clear();
            oldestDate = default;

            if (IncludeSaleReceipts_CheckBox.Checked)
            {
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Sale_DataGridView, "Sale");
            }

            if (IncludePurchaseReceipts_CheckBox.Checked)
            {
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, "Purchase");
            }

            Tools.ScrollToTopOfDataGridView(Receipts_DataGridView);

            // Restore the previous sort order
            if (sortedColumn != null)
            {
                Receipts_DataGridView.Sort(sortedColumn, sortDirection);
            }

            FilterDataGridView(null, null);
        }
        private void AddReceiptsFromDataGridView(Guna2DataGridView sourceDataGridView, string type)
        {
            foreach (DataGridViewRow row in sourceDataGridView.Rows)
            {
                if (row.Tag == null)
                {
                    continue;
                }

                Receipts_DataGridView.Rows.Add(
                    type,
                    row.Cells[MainMenu_Form.Column.ID.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value.ToString());

                // Add receipt filepath to row tag
                string receipt = "";
                if (row.Tag is (string dir, TagData))
                {
                    receipt = dir.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName)
                        .Replace(ReadOnlyVariables.Receipt_text, "");
                }
                else if (row.Tag is (List<string> items, TagData))
                {
                    receipt = items[^1];
                    receipt = receipt.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName)
                        .Replace(ReadOnlyVariables.Receipt_text, "");
                }
                Receipts_DataGridView.Rows[^1].Tag = receipt;

                // Get oldest date
                DateTime currentDate = DateTime.Parse(row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString());
                if (oldestDate == default || oldestDate > currentDate)
                {
                    oldestDate = currentDate;
                }
            }
        }
        private void FilterDataGridView(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in Receipts_DataGridView.Rows)
            {
                bool visible = row.Cells.Cast<DataGridViewCell>()
                    .Any(cell => cell.Value != null &&
                        cell.Value.ToString().Contains(Search_TextBox.Text, StringComparison.OrdinalIgnoreCase));

                if (FilterByDate_CheckBox.Checked)
                {
                    DateTime date = DateTime.Parse(row.Cells[Column.Date.ToString()].Value.ToString());
                    if (date < From_DateTimePicker.Value || date > To_DateTimePicker.Value)
                    {
                        visible = false;
                    }
                }

                row.Visible = visible;
            }
        }
    }
}