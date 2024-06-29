using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Categories_Form : BaseForm
    {
        // Properties
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init.
        public static Categories_Form Instance { get; private set; }
        private readonly MainMenu_Form.Options oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Categories_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            LoadCategories();
            CheckRadioButton(checkPurchaseRadioButton);
            Theme.SetThemeForForm(this);
        }

        // Methods
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
        private void CheckRadioButton(bool selectPurchaseRadioButton)
        {
            if (selectPurchaseRadioButton)
            {
                Purchase_RadioButton.Checked = true;
            }
            else
            {
                Sale_RadioButton.Checked = true;
            }
        }


        // Form event handlers
        private void Categories_Form_Resize(object sender, EventArgs e)
        {
            CenterSelectedDataGridView();
        }
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }


        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            string name = Category_TextBox.Text.Trim();

            if (Purchase_RadioButton.Checked)
            {
                MainMenu_Form.Instance.categoryPurchaseList.Add(new Category(name));
                Purchases_DataGridView.Rows.Add(name);
            }
            else
            {
                MainMenu_Form.Instance.categorySaleList.Add(new Category(name));
                Sales_DataGridView.Rows.Add(name);
            }

            thingsThatHaveChangedInFile.Add(name);
            Log.Write(3, $"Added category '{name}'");

            Category_TextBox.Text = "";
            ValidateInputs();
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Purchases_DataGridView);
            Controls.Remove(Sales_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.CategoryPurchases;
            CenterSelectedDataGridView();
            VaidateCategoryTextBox();
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Controls.Add(Sales_DataGridView);
            Controls.Remove(Purchases_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.CategorySales;
            CenterSelectedDataGridView();
            VaidateCategoryTextBox();
        }
        private void Category_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
                AddCategory_Button.PerformClick();
            }
        }
        private void Category_TextBox_TextChanged(object sender, EventArgs e)
        {
            VaidateCategoryTextBox();
            ValidateInputs();
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
        private const byte topForDataGridView = 170;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(Width - 55, Height - topForDataGridView - 57);
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
            Purchases_DataGridView.Tag = MainMenu_Form.DataGridViewTags.Category;

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Sales_DataGridView, ColumnHeaders);
            Sales_DataGridView.Location = new Point((Width - Sales_DataGridView.Width) / 2, topForDataGridView);
            Sales_DataGridView.Tag = MainMenu_Form.DataGridViewTags.Category;
        }


        // Validate category name
        public void VaidateCategoryTextBox()
        {
            // Get list
            List<Category> categories;
            if (Sale_RadioButton.Checked)
            {
                categories = MainMenu_Form.Instance.categorySaleList;
            }
            else
            {
                categories = MainMenu_Form.Instance.categoryPurchaseList;
            }

            bool exists = categories.Any(category => category.Name == Category_TextBox.Text);
            if (exists)
            {
                AddCategory_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Category_TextBox);
                ShowCategoryWarning();
            }
            else
            {
                AddCategory_Button.Enabled = true;
                UI.SetGTextBoxToValid(Category_TextBox);
                HideCategoryWarning();
            }
        }
        private void ShowCategoryWarning()
        {
            WarningCategoryName_PictureBox.Visible = true;
            WarningCategoryName_Label.Visible = true;
        }
        private void HideCategoryWarning()
        {
            WarningCategoryName_PictureBox.Visible = false;
            WarningCategoryName_Label.Visible = false;
        }


        // Methods
        private void ValidateInputs()
        {
            AddCategory_Button.Enabled = !string.IsNullOrWhiteSpace(Category_TextBox.Text);
        }
    }
}