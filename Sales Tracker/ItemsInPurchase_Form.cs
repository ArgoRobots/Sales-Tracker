using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class ItemsInPurchase_Form : Form
    {
        // Properties
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
        public ItemsInPurchase_Form(List<string> tag)
        {
            InitializeComponent();
            SetDataGridView(tag);
            Theme.SetThemeForForm(this);
        }


        // Methods
        private void SetDataGridView(List<string> tag)
        {
            MainMenu_Form.Instance.isProgramLoading = true;
            MainMenu_Form.Instance.InitializeDataGridView(Items_DataGridView, Items_DataGridView.Size);
            Items_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Items_DataGridView.RowsAdded -= MainMenu_Form.Instance.DataGridView_RowsAdded;
            Items_DataGridView.RowsRemoved -= MainMenu_Form.Instance.DataGridView_RowsRemoved;
            Items_DataGridView.UserDeletingRow -= MainMenu_Form.Instance.DataGridView_UserDeletingRow;
            Items_DataGridView.KeyDown -= MainMenu_Form.Instance.DataGridView_KeyDown;
            MainMenu_Form.LoadColumnsInDataGridView(Items_DataGridView, MainMenu_Form.Instance.PurchaseColumnHeaders, columnsToLoad);

            LoadAllItemsInDataGridView(tag);
            MainMenu_Form.Instance.isProgramLoading = false;
        }

        private void LoadAllItemsInDataGridView(List<string> tag)
        {
            string receiptFilePath = tag[0];

            foreach (string row in tag.Skip(1))
            {
                string[] values = row.Split(',');

                int rowIndex = Items_DataGridView.Rows.Add(values);
                Items_DataGridView.Rows[rowIndex].Tag = receiptFilePath;
            }
        }
    }
}