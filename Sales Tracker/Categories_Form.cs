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
        private static readonly List<string> _thingsThatHaveChangedInFile = [];

        // Getters and setters
        public static Categories_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;

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
            LabelManager.ShowTotalLabel(Total_Label, MainMenu_Form.Instance.SelectedDataGridView);
            Controls.Remove(ShowingResultsFor_Label);
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(_purchases_DataGridView, _sales_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Category_TextBox);

            _purchases_DataGridView.RowsAdded += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, _purchases_DataGridView); };
            _purchases_DataGridView.RowsRemoved += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, _purchases_DataGridView); };

            _sales_DataGridView.RowsAdded += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, _sales_DataGridView); };
            _sales_DataGridView.RowsRemoved += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, _sales_DataGridView); };
        }
        private void SetAccessibleDescriptions()
        {
            CategoryName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            WarningCategoryName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
            ForPurchase_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ForSale_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Methods
        private void LoadCategories()
        {
            foreach (Category category in MainMenu_Form.Instance.CategoryPurchaseList)
            {
                _purchases_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(_purchases_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                _sales_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(_sales_DataGridView);
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
            _purchases_DataGridView.ClearSelection();
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
                int newRowIndex = _purchases_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.Instance.CategorySaleList.Add(new Category(name));
                int newRowIndex = _sales_DataGridView.Rows.Add(name);
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
                Controls.Add(_purchases_DataGridView);
                Controls.Remove(_sales_DataGridView);
                _purchases_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = _purchases_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategoryPurchases;
                CenterSelectedDataGridView();
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, _purchases_DataGridView);
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Sale_RadioButton.Checked)
            {
                CloseAllPanels(null, null);
                Controls.Add(_sales_DataGridView);
                Controls.Remove(_purchases_DataGridView);
                _sales_DataGridView.ClearSelection();
                MainMenu_Form.Instance.SelectedDataGridView = _sales_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                CenterSelectedDataGridView();
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, _sales_DataGridView);
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
        private Guna2DataGridView _purchases_DataGridView, _sales_DataGridView;
        private const byte topForDataGridView = 250;

        // DataGridView getters
        public Guna2DataGridView Purchases_DataGridView
        {
            get => _purchases_DataGridView;
            set => _purchases_DataGridView = value;
        }
        public Guna2DataGridView Sales_DataGridView
        {
            get => _sales_DataGridView;
            set => _sales_DataGridView = value;
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

            _purchases_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_purchases_DataGridView, "purchases_DataGridView", size, ColumnHeaders);
            _purchases_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _purchases_DataGridView.Location = new Point((ClientSize.Width - _purchases_DataGridView.Width) / 2, topForDataGridView);
            _purchases_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;

            _sales_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_sales_DataGridView, "sales_DataGridView", size, ColumnHeaders);
            _sales_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _sales_DataGridView.Location = new Point((ClientSize.Width - _sales_DataGridView.Width) / 2, topForDataGridView);
            _sales_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;
            Theme.CustomizeScrollBar(_sales_DataGridView);
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
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}