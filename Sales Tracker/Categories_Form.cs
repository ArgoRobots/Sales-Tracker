using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class Categories_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        public static Categories_Form Instance { get; private set; }
        // Init
        private readonly Options oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Categories_Form()
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            LoadCategories();
            Purchase_RadioButton.Checked = true;
            Theme.SetThemeForForm(this);
        }
        private void LoadCategories()
        {
            MainMenu_Form.Instance.isDataGridViewLoading = true;

            foreach (Category category in MainMenu_Form.Instance.categoryPurchaseList)
            {
                Purchases_DataGridView.Rows.Add(category.Name);
            }
            foreach (Category category in MainMenu_Form.Instance.categorySaleList)
            {
                Sales_DataGridView.Rows.Add(category.Name);
            }
            MainMenu_Form.Instance.isDataGridViewLoading = false;
        }

        // Form
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }


        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            if (Purchase_RadioButton.Checked)
            {
                MainMenu_Form.Instance.categoryPurchaseList.Add(new Category(Category_TextBox.Text));
                Purchases_DataGridView.Rows.Add(Category_TextBox.Text);
            }
            else
            {
                MainMenu_Form.Instance.categorySaleList.Add(new Category(Category_TextBox.Text));
                Sales_DataGridView.Rows.Add(Category_TextBox.Text);
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


        // DataGridView
        public enum Columns
        {
            CategoryName,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.CategoryName, "Category" },
        };
        private Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte topForDataGridView = 160;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - 8, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Purchases_DataGridView, ColumnHeaders);
            Purchases_DataGridView.Location = new Point((Width - Purchases_DataGridView.Width) / 2, topForDataGridView);
            Purchases_DataGridView.Tag = DataGridViewTags.AddCategory;

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Sales_DataGridView, ColumnHeaders);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, topForDataGridView);
            Sales_DataGridView.Tag = DataGridViewTags.AddCategory;
        }


        // Functions
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(Category_TextBox.Text);
            AddCategory_Button.Enabled = allFieldsFilled;
        }
    }
}