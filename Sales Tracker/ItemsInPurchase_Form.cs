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

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;

            // Check if the Tag is a ValueTuple
            if (row.Tag is (List<string> itemList, TagData))
            {
                SetDataGridView(itemList);
            }
            else if (row.Tag is List<string> list)
            {
                SetDataGridView(list);
            }
            else
            {
                // Handle the case where the Tag is not what we expect
                throw new InvalidCastException("Unexpected Tag type. Expected List<string> or ValueTuple<List<string>, TagData>.");
            }

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
        private void ItemsInPurchase_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Methods
        private void UpdateRowTag()
        {
            List<string> items = new();

            // Check if the current Tag is a ValueTuple
            if (oldSelectedDataGridView.SelectedRows[0].Tag is (List<string> existingItems, TagData tagData))
            {
                // Handle the case where Tag is a ValueTuple

                string lastItem = null;
                if (existingItems.Last().StartsWith(MainMenu_Form.receipt_text))
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

                // Add the receipt file path if it exists
                if (lastItem != null)
                {
                    items.Add(lastItem);
                }

                oldSelectedDataGridView.SelectedRows[0].Tag = (items, tagData);
            }
            else
            {
                throw new InvalidCastException("Unexpected Tag type. Expected ValueTuple<List<string>, TagData>.");
            }
        }
        private void SetDataGridView(List<string> tag)
        {
            MainMenu_Form.Instance.InitializeDataGridView(Items_DataGridView, Items_DataGridView.Size, MainMenu_Form.Instance.PurchaseColumnHeaders, columnsToLoad);
            Items_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Items_DataGridView.RowsRemoved -= MainMenu_Form.Instance.DataGridView_RowsRemoved;
            Items_DataGridView.UserDeletingRow -= MainMenu_Form.Instance.DataGridView_UserDeletingRow;
            Items_DataGridView.Tag = MainMenu_Form.DataGridViewTag.ItemsInPurchase;

            LoadAllItemsInDataGridView(tag);

            MainMenu_Form.Instance.selectedDataGridView = Items_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ItemsInPurchase;
        }
        private void LoadAllItemsInDataGridView(List<string> tag)
        {
            string receiptFilePath = null;

            if (tag.Last().StartsWith(MainMenu_Form.receipt_text))
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