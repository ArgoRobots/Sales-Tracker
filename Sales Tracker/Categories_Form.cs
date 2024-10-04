using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Categories_Form : Form
    {
        // Properties
        private static Categories_Form _instance;
        private static List<string> _thingsThatHaveChangedInFile = [];

        // Getters and setters
        public static Categories_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile
        {
            get => _thingsThatHaveChangedInFile;
            private set => _thingsThatHaveChangedInFile = value;
        }

        // Init.
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Categories_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;
            ConstructDataGridViews();
            LoadCategories();
            CheckRadioButton(checkPurchaseRadioButton);
            CenterSelectedDataGridView();
            Theme.SetThemeForForm(this);
            SetAccessibleDescriptions();
            LabelManager.SetTotalLabel(Total_Label, MainMenu_Form.Instance.SelectedDataGridView);
            Controls.Remove(ShowingResultsFor_Label);
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(purchases_DataGridView, sales_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Category_TextBox);

            purchases_DataGridView.RowsAdded += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, purchases_DataGridView); };
            purchases_DataGridView.RowsRemoved += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, purchases_DataGridView); };

            sales_DataGridView.RowsAdded += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, sales_DataGridView); };
            sales_DataGridView.RowsRemoved += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, sales_DataGridView); };
        }
        private void SetAccessibleDescriptions()
        {
            CategoryName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            WarningCategoryName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ForPurchase_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ForSale_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Methods
        private void LoadCategories()
        {
            foreach (Category category in MainMenu_Form.Instance.CategoryPurchaseList)
            {
                purchases_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(purchases_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                sales_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(sales_DataGridView);
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
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }
        private void Categories_Form_Shown(object sender, EventArgs e)
        {
            purchases_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Category_TextBox.Text.Trim();

            if (Purchase_RadioButton.Checked)
            {
                MainMenu_Form.Instance.CategoryPurchaseList.Add(new Category(name));
                int newRowIndex = purchases_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.Instance.CategorySaleList.Add(new Category(name));
                int newRowIndex = sales_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added category '{name}'");

            Category_TextBox.Text = "";
            Category_TextBox.Focus();
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Purchase_RadioButton.Checked)
            {
                CloseAllPanels(null, null);
                Controls.Add(purchases_DataGridView);
                Controls.Remove(sales_DataGridView);
                purchases_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = purchases_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategoryPurchases;
                CenterSelectedDataGridView();
                VaidateCategoryTextBox();
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Sale_RadioButton.Checked)
            {
                CloseAllPanels(null, null);
                Controls.Add(sales_DataGridView);
                Controls.Remove(purchases_DataGridView);
                sales_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = sales_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                CenterSelectedDataGridView();
                VaidateCategoryTextBox();
            }
        }
        private void Category_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (AddCategory_Button.Enabled)
                {
                    AddCategory_Button.PerformClick();
                }
            }
        }
        private void Category_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
            VaidateCategoryTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Tools.SearchSelectedDataGridView(Search_TextBox))
            {
                LabelManager.ShowShowingResultsLabel(ShowingResultsFor_Label, Search_TextBox.Text.Trim(), this);
            }
            else
            {
                Controls.Remove(ShowingResultsFor_Label);
            }
        }
        private void ForPurchase_Label_Click(object sender, EventArgs e)
        {
            Purchase_RadioButton.Checked = true;
        }
        private void ForSale_Label_Click(object sender, EventArgs e)
        {
            Sale_RadioButton.Checked = true;
        }

        // DataGridView properties
        public enum Columns
        {
            CategoryName,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.CategoryName, "Category" },
        };
        private Guna2DataGridView purchases_DataGridView, sales_DataGridView;
        private const byte topForDataGridView = 250;

        // DataGridView getters
        public Guna2DataGridView Purchases_DataGridView
        {
            get => purchases_DataGridView;
            set => purchases_DataGridView = value;
        }
        public Guna2DataGridView Sales_DataGridView
        {
            get => sales_DataGridView;
            set => sales_DataGridView = value;
        }

        // DataGridView methods
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.SelectedDataGridView == null) { return; }
            MainMenu_Form.Instance.SelectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.SelectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.SelectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            purchases_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(purchases_DataGridView, "purchases_DataGridView", size, ColumnHeaders);
            purchases_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            purchases_DataGridView.Location = new Point((ClientSize.Width - purchases_DataGridView.Width) / 2, topForDataGridView);
            purchases_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;

            sales_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(sales_DataGridView, "sales_DataGridView", size, ColumnHeaders);
            sales_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            sales_DataGridView.Location = new Point((ClientSize.Width - sales_DataGridView.Width) / 2, topForDataGridView);
            sales_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;
            Theme.CustomizeScrollBar(sales_DataGridView);
        }

        // Validate category name
        public void VaidateCategoryTextBox()
        {
            // Get list
            List<Category> categories;
            if (Sale_RadioButton.Checked)
            {
                categories = MainMenu_Form.Instance.CategorySaleList;
            }
            else
            {
                categories = MainMenu_Form.Instance.CategoryPurchaseList;
            }

            bool exists = categories.Any(category => string.Equals(category.Name, Category_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                AddCategory_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(Category_TextBox);
                ShowCategoryWarning();
            }
            else
            {
                if (Category_TextBox.Text != "")
                {
                    AddCategory_Button.Enabled = true;
                }
                CustomControls.SetGTextBoxToValid(Category_TextBox);
                HideCategoryWarning();
            }
        }
        private void ShowCategoryWarning()
        {
            WarningCategoryName_PictureBox.Visible = true;
            WarningCategoryName_Label.Visible = true;
            AddCategory_Button.Enabled = false;
            AddCategory_Button.Tag = false;
        }
        private void HideCategoryWarning()
        {
            WarningCategoryName_PictureBox.Visible = false;
            WarningCategoryName_Label.Visible = false;

            if (Category_TextBox.Text != "")
            {
                AddCategory_Button.Enabled = true;
                AddCategory_Button.Tag = true;
            }
        }

        // Methods
        private void ValidateInputs()
        {
            if (AddCategory_Button.Tag is bool and true)
            {
                AddCategory_Button.Enabled = !string.IsNullOrWhiteSpace(Category_TextBox.Text);
            }
        }
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            MainMenu_Form.CloseRightClickPanels();
        }
    }
}