using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Receipts_Form : BaseForm
    {
        // Properties
        public static Receipts_Form Instance { get; private set; }
        private DateTime oldestDate;

        // Init.
        public Receipts_Form()
        {
            InitializeComponent();
            Instance = this;

            To_DateTimePicker.Value = DateTime.Now;
            Sort_ComboBox.SelectedIndex = 0;

            AddAllReceiptsAndGetOldestDate();
            MainMenu_Form.Instance.InitializeDataGridView(Receipts_DataGridView, Receipts_DataGridView.Size);
            Theme.SetThemeForForm(this);
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

        // Methods
        private void AddAllReceiptsAndGetOldestDate()
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.Purchases_DataGridView.Rows)
            {
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

                DateTime date = DateTime.Parse(row.Cells[Column.Date.ToString()].Value.ToString());
                if (date < From_DateTimePicker.Value || date > To_DateTimePicker.Value)
                {
                    visible = false;
                }

                row.Visible = visible;
            }
        }
        private void SortReceipts(object sender, EventArgs e)
        {
            switch (Sort_ComboBox.Text)
            {
                case "Most recent":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Date.ToString()], System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "Least recent":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Date.ToString()], System.ComponentModel.ListSortDirection.Ascending);
                    break;
                case "Most expensive":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Total.ToString()], System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "Least expensive":
                    Receipts_DataGridView.Sort(Receipts_DataGridView.Columns[Column.Total.ToString()], System.ComponentModel.ListSortDirection.Ascending);
                    break;
            }
        }
    }
}