using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
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
            MainMenu_Form.Column.UniqueProducts,
            MainMenu_Form.Column.PricePerUnit
        ];
        private bool hasChanges = false;

        // Init.
        public ItemsInTransaction_Form(DataGridViewRow row)
        {
            InitializeComponent();
            DataGridViewManager.SelectedRowInMainMenu = row;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;

            SetTitle();

            if (row.Tag is (List<string> itemList, TagData))
            {
                SetDataGridView(itemList);
            }

            Theme.SetThemeForForm(this);
            LanguageManager.UpdateLanguageForControl(this);

            // Attach event handlers to detect changes in DataGridView
            Items_DataGridView.CellValueChanged += delegate { hasChanges = true; };
            Items_DataGridView.RowsAdded += delegate { hasChanges = true; };
            Items_DataGridView.RowsRemoved += delegate { hasChanges = true; };
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
            MainMenu_Form.IsProgramLoading = true;
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;

            if (hasChanges)
            {
                if (Items_DataGridView.Rows.Count == 0)
                {
                    MainMenu_Form.Instance.SelectedDataGridView.Rows.Remove(DataGridViewManager.SelectedRowInMainMenu);
                }
                else
                {
                    UpdateMainMenuRowTag();
                    DataGridViewManager.UpdateRowWithMultipleItems(DataGridViewManager.SelectedRowInMainMenu);
                }

                DataGridViewManager.DataGridViewRowChanged(MainMenu_Form.Instance.SelectedDataGridView, MainMenu_Form.Instance.Selected);
            }
            MainMenu_Form.IsProgramLoading = false;
        }
        private void ItemsInTransaction_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Methods
        private void UpdateMainMenuRowTag()
        {
            List<string> items = [];

            if (oldSelectedDataGridView.SelectedRows[0].Tag is (List<string> existingItems, TagData tagData))
            {
                string lastItem = null;
                if (existingItems.Last().StartsWith(ReadOnlyVariables.Receipt_text))
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
        private void SetDataGridView(List<string> itemList)
        {
            Dictionary<MainMenu_Form.Column, string> columnHeaders;
            columnHeaders = (oldOption == MainMenu_Form.SelectedOption.Purchases)
                ? MainMenu_Form.Instance.PurchaseColumnHeaders
                : MainMenu_Form.Instance.SalesColumnHeaders;

            DataGridViewManager.InitializeDataGridView(Items_DataGridView, "Items_DataGridView", Items_DataGridView.Size, columnHeaders, columnsToLoad, this);
            Items_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            Items_DataGridView.RowsRemoved -= DataGridViewManager.DataGridView_RowsRemoved;
            Items_DataGridView.UserDeletingRow -= DataGridViewManager.DataGridView_UserDeletingRow;
            Items_DataGridView.Tag = MainMenu_Form.DataGridViewTag.ItemsInPurchase;

            LoadAllItemsInDataGridView(itemList);

            MainMenu_Form.Instance.SelectedDataGridView = Items_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Sales
                ? MainMenu_Form.SelectedOption.ItemsInSale
                : MainMenu_Form.SelectedOption.ItemsInPurchase;
        }
        private void LoadAllItemsInDataGridView(List<string> itemList)
        {
            string defaultCurrencyType = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            string receiptFilePath = null;
            int index = 0;

            // Check if the last item is the receipt file path
            if (itemList.Count > 0 && itemList[^1].StartsWith(ReadOnlyVariables.Receipt_text))
            {
                receiptFilePath = itemList[^1];
                index = 1;
            }

            // Add rows to DataGridView, and convert money values
            for (int i = 0; i < itemList.Count - index; i++)
            {
                string[] values = itemList[i].Split(',');

                int sourceIndex = (defaultCurrencyType == "USD") ? 6 : 5;
                values[5] = decimal.Parse(values[sourceIndex]).ToString("N2");

                int rowIndex = Items_DataGridView.Rows.Add(values);
                Items_DataGridView.Rows[rowIndex].Tag = receiptFilePath;
            }
        }
    }
}