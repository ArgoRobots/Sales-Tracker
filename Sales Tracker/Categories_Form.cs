using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

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
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            ConstructTotalLabel();
            LoadCategories();
            CheckRadioButton(checkPurchaseRadioButton);
            CenterSelectedDataGridView();
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
            MainMenu_Form.SortTheDataGridViewByFirstColumnAndSelectFirstRow(Purchases_DataGridView, Sales_DataGridView);
            AddEventHandlersToTextBoxes();
            SetTotalLabel();
        }
        private void AddEventHandlersToTextBoxes()
        {
            Category_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            TextBoxManager.Attach(Category_TextBox);

            Purchases_DataGridView.RowsAdded += (sender, e) => { SetTotalLabel(); };
            Purchases_DataGridView.RowsRemoved += (sender, e) => { SetTotalLabel(); };

            Sales_DataGridView.RowsAdded += (sender, e) => { SetTotalLabel(); };
            Sales_DataGridView.RowsRemoved += (sender, e) => { SetTotalLabel(); };
        }

        // Methods
        private void LoadCategories()
        {
            foreach (Category category in MainMenu_Form.Instance.categoryPurchaseList)
            {
                Purchases_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(Purchases_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.categorySaleList)
            {
                Sales_DataGridView.Rows.Add(category.Name);
            }
            Tools.ScrollToTopOfDataGridView(Sales_DataGridView);
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
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }
        private void Categories_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Category_TextBox.Text.Trim();

            if (Purchase_RadioButton.Checked)
            {
                MainMenu_Form.Instance.categoryPurchaseList.Add(new Category(name));
                int newRowIndex = Purchases_DataGridView.Rows.Add(name);
                MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.Instance.categorySaleList.Add(new Category(name));
                int newRowIndex = Sales_DataGridView.Rows.Add(name);
                MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
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
                Controls.Add(Purchases_DataGridView);
                Controls.Remove(Sales_DataGridView);
                MainMenu_Form.Instance.selectedDataGridView = Purchases_DataGridView;
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
                Controls.Add(Sales_DataGridView);
                Controls.Remove(Purchases_DataGridView);
                MainMenu_Form.Instance.selectedDataGridView = Sales_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                CenterSelectedDataGridView();
                VaidateCategoryTextBox();
            }
        }
        private void Category_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
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
                ShowShowingResultsForLabel(Search_TextBox.Text.Trim());
            }
            else
            {
                HideShowingResultsForLabel();
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

        // DataGridView
        public enum Columns
        {
            CategoryName,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.CategoryName, "Category" },
        };
        public Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        private const byte topForDataGridView = 250;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Purchases_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Purchases_DataGridView, size, ColumnHeaders);
            Purchases_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Purchases_DataGridView.Location = new Point((ClientSize.Width - Purchases_DataGridView.Width) / 2, topForDataGridView);
            Purchases_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;

            Sales_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Sales_DataGridView, size, ColumnHeaders);
            Sales_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Sales_DataGridView.Location = new Point((ClientSize.Width - Sales_DataGridView.Width) / 2, topForDataGridView);
            Sales_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;
            Theme.CustomizeScrollBar(Sales_DataGridView);
        }

        // Label
        private Label totalLabel;
        private void ConstructTotalLabel()
        {
            totalLabel = new Label
            {
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                ForeColor = CustomColors.text,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(totalLabel);
        }
        private void SetTotalLabel()
        {
            totalLabel.Text = $"Total: {MainMenu_Form.Instance.selectedDataGridView.Rows.Count}";
            totalLabel.Location = new Point(MainMenu_Form.Instance.selectedDataGridView.Right - totalLabel.Width, MainMenu_Form.Instance.selectedDataGridView.Bottom + 10);
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

            bool exists = categories.Any(category => string.Equals(category.Name, Category_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                AddCategory_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Category_TextBox);
                ShowCategoryWarning();
            }
            else
            {
                if (Category_TextBox.Text != "")
                {
                    AddCategory_Button.Enabled = true;
                }
                UI.SetGTextBoxToValid(Category_TextBox);
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

        // SearchingFor_Label
        private void ShowShowingResultsForLabel(string text)
        {
            ShowingResultsFor_Label.Text = $"Showing results for: {text}";
            ShowingResultsFor_Label.Left = (ClientSize.Width - ShowingResultsFor_Label.Width) / 2 - 8;
            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
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
            MainMenu_Form.Instance.CloseRightClickPanels();
        }
    }
}