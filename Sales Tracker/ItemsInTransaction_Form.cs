using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Text;

namespace Sales_Tracker
{
    public partial class ItemsInTransaction_Form : Form
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
        private bool hasChanges = false;

        // Init.
        public ItemsInTransaction_Form(DataGridViewRow row)
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;

            SetTitle();

            if (row.Tag is (List<string> itemList, TagData tagData))
            {
                SetDataGridView(itemList, tagData.DefaultCurrencyType);
            }

            Theme.SetThemeForForm(this);

            // Attach event handlers to detect changes in DataGridView
            Items_DataGridView.CellValueChanged += (s, e) => hasChanges = true;
            Items_DataGridView.RowsAdded += (s, e) => hasChanges = true;
            Items_DataGridView.RowsRemoved += (s, e) => hasChanges = true;
        }
        private void SetTitle()
        {
            if (oldOption == MainMenu_Form.SelectedOption.Purchases)
            {
                Title_Label.Text = "Items in purchase";
            }
            else
            {
                Title_Label.Text = "Items in sale";
            }
        }

        // Form event handlers
        private void ItemsInTransaction_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;

            if (hasChanges)
            {
                if (Items_DataGridView.Rows.Count == 0)
                {
                    MainMenu_Form.Instance.selectedDataGridView.Rows.Remove(MainMenu_Form.Instance.selectedRowInMainMenu);
                }
                else
                {
                    UpdateMainMenuRowTag();
                    MainMenu_Form.Instance.UpdateRowWithMultipleItems(MainMenu_Form.Instance.selectedRowInMainMenu);
                }

                MainMenu_Form.Instance.DataGridViewRowChanged();
            }
        }
        private void ItemsInTransaction_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Methods
        private void UpdateMainMenuRowTag()
        {
            List<string> items = new();

            if (oldSelectedDataGridView.SelectedRows[0].Tag is (List<string> existingItems, TagData tagData))
            {
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
        }
        private void SetDataGridView(List<string> itemList, string defaultCurrencyType)
        {
            Dictionary<MainMenu_Form.Column, string> columnHeaders;
            columnHeaders = (oldOption == MainMenu_Form.SelectedOption.Purchases)
                ? MainMenu_Form.Instance.PurchaseColumnHeaders
                : MainMenu_Form.Instance.SalesColumnHeaders;

            MainMenu_Form.Instance.InitializeDataGridView(Items_DataGridView, Items_DataGridView.Size, columnHeaders, columnsToLoad);
            Items_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Items_DataGridView.RowsRemoved -= MainMenu_Form.Instance.DataGridView_RowsRemoved;
            Items_DataGridView.UserDeletingRow -= MainMenu_Form.Instance.DataGridView_UserDeletingRow;
            Items_DataGridView.Tag = MainMenu_Form.DataGridViewTag.ItemsInPurchase;

            LoadAllItemsInDataGridView(itemList, defaultCurrencyType);

            MainMenu_Form.Instance.selectedDataGridView = Items_DataGridView;
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ItemsInSale;
            }
            else
            {
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.ItemsInPurchase;
            }
        }
        private void LoadAllItemsInDataGridView(List<string> itemList, string defaultCurrencyType)
        {
            string receiptFilePath = null;
            int startIndex = 0;

            // Check if the last item is the receipt file path
            if (itemList.Count > 0 && itemList[^1].StartsWith(MainMenu_Form.receipt_text))
            {
                receiptFilePath = itemList[^1];
                startIndex = 1;
            }

            // Add rows to DataGridView, and convert money values
            for (int i = 0; i < itemList.Count - startIndex; i++)
            {
                string[] values = itemList[i].Split(',');
                decimal quantity = decimal.Parse(values[4]);

                if (defaultCurrencyType == "USD")
                {
                    decimal pricePerUnit = decimal.Parse(values[6]);
                    values[5] = pricePerUnit.ToString("N2");
                    values[6] = (quantity * pricePerUnit).ToString("N2");
                }
                else if (defaultCurrencyType == Properties.Settings.Default.Currency)
                {
                    decimal pricePerUnit = decimal.Parse(values[5]);
                    values[6] = (quantity * pricePerUnit).ToString("N2");
                }

                int rowIndex = Items_DataGridView.Rows.Add(values);
                Items_DataGridView.Rows[rowIndex].Tag = receiptFilePath;
            }
        }
    }
}