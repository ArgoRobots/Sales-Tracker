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
        public Categories_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            ConstructDataGridViews();
            LoadCategories();
            CheckRadioButton(checkPurchaseRadioButton);
            CenterDataGridView();
            SetTheme();
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LabelManager.ShowTotalLabel(Total_Label, selectedDataGridView);
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(_purchase_DataGridView, _sale_DataGridView);
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Category_TextBox);
            TextBoxManager.Attach(Search_TextBox);

            _purchase_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);
            _purchase_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);

            _sale_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);
            _sale_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);
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
        private void SetTheme()
        {
            Theme.SetThemeForForm(this);
        }

        // Methods
        private void LoadCategories()
        {
            foreach (Category category in MainMenu_Form.Instance.CategoryPurchaseList)
            {
                _purchase_DataGridView.Rows.Add(category.Name);
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_purchase_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                _sale_DataGridView.Rows.Add(category.Name);
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_sale_DataGridView);
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
            CenterDataGridView();
        }
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
        }
        private void Categories_Form_Shown(object sender, EventArgs e)
        {
            _purchase_DataGridView.ClearSelection();
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
                int newRowIndex = _purchase_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.Instance.CategorySaleList.Add(new Category(name));
                int newRowIndex = _sale_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
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
                _purchase_DataGridView.Visible = true;
                _sale_DataGridView.Visible = false;
                _purchase_DataGridView.ClearSelection();
                selectedDataGridView = _purchase_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategoryPurchases;
                CenterDataGridView();
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, _purchase_DataGridView);
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Sale_RadioButton.Checked)
            {
                CloseAllPanels(null, null);
                _sale_DataGridView.Visible = true;
                _purchase_DataGridView.Visible = false;
                _sale_DataGridView.ClearSelection();
                selectedDataGridView = _sale_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                CenterDataGridView();
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, _sale_DataGridView);
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
            if (DataGridViewManager.SearchSelectedDataGridViewAndUpdateRowColors(selectedDataGridView, Search_TextBox))
            {
                LabelManager.ShowShowingResultsLabel(ShowingResultsFor_Label, Search_TextBox.Text.Trim(), this);
            }
            else
            {
                ShowingResultsFor_Label.Visible = false;
            }
            LabelManager.ShowTotalLabel(Total_Label, selectedDataGridView);
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Text = "";
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
        public enum Column
        {
            CategoryName,
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.CategoryName, "Category" },
        };
        private Guna2DataGridView _purchase_DataGridView, _sale_DataGridView, selectedDataGridView;
        private const byte topForDataGridView = 250;

        // DataGridView getters
        public Guna2DataGridView Purchase_DataGridView
        {
            get => _purchase_DataGridView;
            set => _purchase_DataGridView = value;
        }
        public Guna2DataGridView Sale_DataGridView
        {
            get => _sale_DataGridView;
            set => _sale_DataGridView = value;
        }

        // DataGridView methods
        private void CenterDataGridView()
        {
            if (selectedDataGridView == null) { return; }
            selectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            selectedDataGridView.Location = new Point((ClientSize.Width - selectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            _purchase_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_purchase_DataGridView, "purchases_DataGridView", size, ColumnHeaders, null, this);
            _purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _purchase_DataGridView.Location = new Point((ClientSize.Width - _purchase_DataGridView.Width) / 2, topForDataGridView);
            _purchase_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;

            _sale_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_sale_DataGridView, "sales_DataGridView", size, ColumnHeaders, null, this);
            _sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            _sale_DataGridView.Location = new Point((ClientSize.Width - _sale_DataGridView.Width) / 2, topForDataGridView);
            _sale_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;
            Theme.CustomizeScrollBar(_sale_DataGridView);
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
            CustomControls.CloseAllPanels(null, null);
        }
    }
}