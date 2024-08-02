using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Text;

namespace Sales_Tracker
{
    public partial class ItemsInPurchase_Form : Form
    {
        // Properties
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        private readonly DataGridViewRow selectedRow;
        private readonly List<MainMenu_Form.Column> columnsToLoad = [
            MainMenu_Form.Column.Product,
            MainMenu_Form.Column.Category,
            MainMenu_Form.Column.Country,
            MainMenu_Form.Column.Company,
            MainMenu_Form.Column.Quantity,
            MainMenu_Form.Column.PricePerUnit,
            MainMenu_Form.Column.Total
        ];

        // Init.
        public ItemsInPurchase_Form(DataGridViewRow row)
        {
            InitializeComponent();

            selectedRow = row;
            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            SetDataGridView((List<string>)row.Tag);
            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private void ItemsInPurchase_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Reset();

            if (Items_DataGridView.Rows.Count == 0)
            {
                MainMenu_Form.Instance.selectedDataGridView.Rows.Remove(MainMenu_Form.Instance.selectedRowInMainMenu);
            }
            else
            {
                UpdateRowTag();
                MainMenu_Form.Instance.UpdateRow();
            }

            MainMenu_Form.Instance.DataGridViewRowChanged();
        }

        // Methods
        private void UpdateRowTag()
        {
            List<string> items = new();

            if (oldSelectedDataGridView.SelectedRows[0].Tag is List<string> existingItems && existingItems.Count > 0)
            {
                // The last item which is the receipt file path needs to stay the same
                string lastItem = null;
                if (existingItems.Last().StartsWith(MainMenu_Form.receipt))
                {
                    lastItem = existingItems.Last();
                }

                foreach (DataGridViewRow row in Items_DataGridView.Rows)
                {
                    StringBuilder itemBuilder = new();
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        if (i > 0) { itemBuilder.Append(','); }
                        itemBuilder.Append(row.Cells[i].Value);
                    }
                    items.Add(itemBuilder.ToString());
                }

                // Add the receipt file path
                if (lastItem != null)
                {
                    items.Add(lastItem);
                }
            }
            oldSelectedDataGridView.SelectedRows[0].Tag = items;
        }
        private void SetDataGridView(List<string> tag)
        {
            MainMenu_Form.Instance.InitializeDataGridView(Items_DataGridView, Items_DataGridView.Size);
            Items_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Items_DataGridView.RowsAdded -= MainMenu_Form.Instance.DataGridView_RowsAdded;
            Items_DataGridView.RowsRemoved -= MainMenu_Form.Instance.DataGridView_RowsRemoved;
            Items_DataGridView.UserDeletingRow -= MainMenu_Form.Instance.DataGridView_UserDeletingRow;
            Items_DataGridView.Tag = MainMenu_Form.DataGridViewTag.ItemsInPurchase;
            MainMenu_Form.LoadColumnsInDataGridView(Items_DataGridView, MainMenu_Form.Instance.PurchaseColumnHeaders, columnsToLoad);

            LoadAllItemsInDataGridView(tag);

            MainMenu_Form.Instance.selectedDataGridView = Items_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ItemsInPurchase;
        }
        private void LoadAllItemsInDataGridView(List<string> tag)
        {
            string receiptFilePath = null;

            if (tag.Last().StartsWith(MainMenu_Form.receipt))
            {
                receiptFilePath = tag.Last();
                tag.RemoveAt(tag.Count - 1);  // Remove the receipt file path
            }

            foreach (string row in tag)
            {
                string[] values = row.Split(',');
                int rowIndex = Items_DataGridView.Rows.Add(values);
                Items_DataGridView.Rows[rowIndex].Tag = receiptFilePath;
            }
        }
        public void Reset()
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }
    }
}