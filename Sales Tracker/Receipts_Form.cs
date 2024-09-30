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

            MainMenu_Form.Instance.isProgramLoading = true;
            DataGridViewManager.InitializeDataGridView(Receipts_DataGridView, Receipts_DataGridView.Size, ColumnHeaders);
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

            Theme.SetThemeForForm(this);
            LanguageManager.UpdateLanguage(this);
            MainMenu_Form.Instance.isProgramLoading = false;
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Receipts_DataGridView);
        }
        private void Receipts_DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (Receipts_DataGridView.SelectedRows.Count > 0)
            {
                ExportSelected_Button.Enabled = true;
            }
        }

        // Form event handlers
        private void Receipts_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void ClearFilters_Button_Click(object sender, EventArgs e)
        {
            Category_TextBox.Text = "";
            Product_TextBox.Text = "";
            From_DateTimePicker.Value = oldestDate;
            To_DateTimePicker.Value = DateTime.Now;
        }
        private void ExportSelected_Button_Click(object sender, EventArgs e)
        {
            // Select directory
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string destinationPath = dialog.SelectedPath;
                int selectedRowCount = Receipts_DataGridView.SelectedRows.Count;

                if (selectedRowCount > 1)
                {
                    // Create a new folder for multiple files
                    string newFolderPath = Path.Combine(destinationPath, "Receipts_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    Directory.CreateDirectory(newFolderPath);
                    destinationPath = newFolderPath;
                }

                // Iterate through selected rows and copy files
                foreach (DataGridViewRow row in Receipts_DataGridView.SelectedRows)
                {
                    string receipt = row.Tag.ToString();
                    receipt = receipt.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName).Replace(ReadOnlyVariables.Receipt_text, "");
                    string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(receipt));
                    destinationFilePath = Directories.GetNewFileNameIfItAlreadyExists(destinationFilePath);
                    Directories.CopyFile(receipt, destinationFilePath);
                }

                CustomMessageBox.Show("Argo Sales Tracker",
                   "Receipts exported successfully",
                   CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
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

        public enum Column
        {
            Type,
            Product,
            Category,
            Date,
            Total
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.Type, "Type" },
            { Column.Product, "Product name" },
            { Column.Category, "Category" },
            { Column.Date, "Date" },
            { Column.Total, "Total revenue" }
        };

        // Methods
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
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Sales_DataGridView, "Sale");
            }

            if (IncludePurchaseReceipts_CheckBox.Checked)
            {
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Purchases_DataGridView, "Purchase");
            }

            Tools.ScrollToTopOfDataGridView(Receipts_DataGridView);

            // Restore the previous sort order
            if (sortedColumn != null)
            {
                Receipts_DataGridView.Sort(sortedColumn, sortDirection);
            }

            FilterReceipts(null, null);
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
                    row.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value.ToString());

                // Add receipt filepath to row tag
                string receipt = "";
                if (row.Tag is (string dir, TagData))
                {
                    receipt = dir.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName).Replace(ReadOnlyVariables.Receipt_text, "");
                }
                else if (row.Tag is (List<string> items, TagData))
                {
                    receipt = items[^1];
                    receipt = receipt.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName).Replace(ReadOnlyVariables.Receipt_text, "");
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
        private void FilterReceipts(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in Receipts_DataGridView.Rows)
            {
                bool visible = true;

                if (!string.IsNullOrEmpty(Category_TextBox.Text) &&
                    !row.Cells[Column.Category.ToString()].Value.ToString().Contains(Category_TextBox.Text, StringComparison.CurrentCultureIgnoreCase))
                {
                    visible = false;
                }

                if (!string.IsNullOrEmpty(Product_TextBox.Text) &&
                    !row.Cells[Column.Product.ToString()].Value.ToString().Contains(Product_TextBox.Text, StringComparison.CurrentCultureIgnoreCase))
                {
                    visible = false;
                }

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