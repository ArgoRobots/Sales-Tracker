using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Categories_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        // Init
        private readonly MainMenu_Form.Options oldOption;
        private readonly Guna2DataGridView oldselectedDataGridView;
        public Categories_Form()
        {
            InitializeComponent();

            oldOption = MainMenu_Form.Instance.Selected;
            oldselectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            LoadProducts();
            Purchase_RadioButton.Checked = true;
            Theme.SetThemeForForm(this);
        }

        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte heightForDataGridView = 160;
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);
            string header = MainMenu_Form.Instance.CategoryColumn;

            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Purchases_DataGridView.Columns.Add(header, header);
            Theme.UpdateDataGridViewHeaderTheme(Purchases_DataGridView);
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, heightForDataGridView);

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Sales_DataGridView.Columns.Add(header, header);
            Theme.UpdateDataGridViewHeaderTheme(Sales_DataGridView);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, heightForDataGridView);
        }
        private void LoadProducts()
        {
            MainMenu_Form.Instance.isDataGridViewLoading = true;

            foreach (Category category in MainMenu_Form.Instance.productCategoryPurchaseList)
            {
                Purchases_DataGridView.Rows.Add(category.Name);
            }
            foreach (Category category in MainMenu_Form.Instance.productCategorySaleList)
            {
                Sales_DataGridView.Rows.Add(category.Name);
            }
            MainMenu_Form.Instance.isDataGridViewLoading = false;
        }

        // Form
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldselectedDataGridView;
        }

        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            if (Purchase_RadioButton.Checked)
            {
                Purchases_DataGridView.Rows.Add(Category_TextBox.Text);
                MainMenu_Form.Instance.productCategoryPurchaseList.Add(new Category(Category_TextBox.Text));
            }
            else
            {
                Sales_DataGridView.Rows.Add(Category_TextBox.Text);
                MainMenu_Form.Instance.productCategorySaleList.Add(new Category(Category_TextBox.Text));
            }
            thingsThatHaveChangedInFile.Add(Category_TextBox.Text);
            Log.Write(3, $"Added category '{Category_TextBox.Text}'");
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            Controls.Remove(Sales_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.CategoryPurchases;
            CenterSelectedDataGridView();
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            Controls.Remove(Purchases_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.CategorySales;
            CenterSelectedDataGridView();
        }
        private void Category_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
                AddCategory_Button.PerformClick();
            }
        }

        // Functions
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - 8, heightForDataGridView);
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(Category_TextBox.Text);
            AddCategory_Button.Enabled = allFieldsFilled;
        }
    }
}