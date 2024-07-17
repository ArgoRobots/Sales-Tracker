using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.ComponentModel;

namespace Sales_Tracker
{
    public partial class Receipts_Form : BaseForm
    {
        // Properties
        public static Receipts_Form Instance { get; private set; }
        private DateTime oldestDate;
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Init.
        public Receipts_Form()
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;

            MainMenu_Form.Instance.isDataGridViewLoading = true;
            MainMenu_Form.Instance.InitializeDataGridView(Receipts_DataGridView, Receipts_DataGridView.Size);
            MainMenu_Form.LoadColumnsInDataGridView(Receipts_DataGridView, ColumnHeaders);
            MainMenu_Form.Instance.selectedDataGridView = Receipts_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Receipts;
            AddAllReceiptsAndGetOldestDate();
            MainMenu_Form.Instance.isDataGridViewLoading = false;

            From_DateTimePicker.Value = oldestDate;
            To_DateTimePicker.Value = DateTime.Now;
            Sort_ComboBox.SelectedIndex = 0;

            Theme.SetThemeForForm(this);
        }


        // Form event handlers
        private void Receipts_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }


        // Event handlers
        private void ClearFilters_Button_Click(object sender, EventArgs e)
        {
            Category_TextBox.Text = "";
            Product_TextBox.Text = "";
            From_DateTimePicker.Value = oldestDate;
            To_DateTimePicker.Value = DateTime.Now;
            Sort_ComboBox.SelectedIndex = 0;
        }
        private void DownloadSelected_Button_Click(object sender, EventArgs e)
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
                    if (row.Tag != null)
                    {
                        string sourceFilePath = row.Tag.ToString();
                        string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));
                        File.Copy(sourceFilePath, destinationFilePath);
                    }
                }
            }
        }


        public enum Column
        {
            Product,
            Category,
            Date,
            Total
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.Product, "Product name" },
            { Column.Category, "Category" },
            { Column.Date, "Date" },
            { Column.Total, "Total revenue" }
        };

        // Methods
        private void AddAllReceiptsAndGetOldestDate()
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.Purchases_DataGridView.Rows)
            {
                if (row.Tag == null)
                {
                    continue;
                }
                Receipts_DataGridView.Rows.Add(
                    row.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value.ToString());

                // Add receipt filepath to row tag
                Receipts_DataGridView.Rows[^1].Tag = row.Tag;

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
        private void SortReceipts(object sender, EventArgs e)
        {
            switch (Sort_ComboBox.Text)
            {
                case "Most recent":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Date.ToString()], ListSortDirection.Descending);
                    break;
                case "Least recent":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Date.ToString()], ListSortDirection.Ascending);
                    break;
                case "Most expensive":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Total.ToString()], ListSortDirection.Descending);
                    break;
                case "Least expensive":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Total.ToString()], ListSortDirection.Ascending);
                    break;
            }
        }
    }
}