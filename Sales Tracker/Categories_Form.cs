﻿using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.GridView;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.ComponentModel;

namespace Sales_Tracker
{
    public partial class Categories_Form : BaseForm
    {
        // Properties
        private static Categories_Form _instance;
        private readonly MainMenu_Form.SelectedOption _oldOption;
        private readonly int _topForDataGridView;

        // Getters
        public static Categories_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile { get; } = [];

        // Init.
        public Categories_Form() : this(false) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public Categories_Form(bool checkPurchaseRadioButton)
        {
            InitializeComponent();
            _instance = this;

            _oldOption = MainMenu_Form.Instance.Selected;
            _topForDataGridView = ShowingResultsFor_Label.Bottom + 20;
            ConstructDataGridViews();
            LoadCategories();
            CheckRadioButton(checkPurchaseRadioButton);
            UpdateTheme();
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LabelManager.ShowTotalLabel(Total_Label, selectedDataGridView);
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Purchase_DataGridView, Sale_DataGridView);
            AddEventHandlersToTextBoxes();

            PanelCloseFilter panelCloseFilter = new(this, ClosePanels, TextBoxManager.RightClickTextBox_Panel, RightClickDataGridViewRowMenu.Panel);
            Application.AddMessageFilter(panelCloseFilter);

            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Category_TextBox);
            TextBoxManager.Attach(Search_TextBox);

            Purchase_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, Purchase_DataGridView);
            Purchase_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, Purchase_DataGridView);

            Sale_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, Sale_DataGridView);
            Sale_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, Sale_DataGridView);
        }
        private void SetAccessibleDescriptions()
        {
            CategoryName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            WarningCategoryName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
            ForPurchase_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ForSale_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
        }

        // Methods
        private void LoadCategories()
        {
            foreach (Category category in MainMenu_Form.Instance.CategoryPurchaseList)
            {
                Purchase_DataGridView.Rows.Add(category.Name);
            }
            DataGridViewManager.ScrollToTopOfDataGridView(Purchase_DataGridView);

            foreach (Category category in MainMenu_Form.Instance.CategorySaleList)
            {
                Sale_DataGridView.Rows.Add(category.Name);
            }
            DataGridViewManager.ScrollToTopOfDataGridView(Sale_DataGridView);
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
        private void Categories_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = _oldOption;
        }
        private void Categories_Form_Shown(object sender, EventArgs e)
        {
            Purchase_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCategory_Button_Click(object sender, EventArgs e)
        {
            string name = Category_TextBox.Text.Trim();

            if (Purchase_RadioButton.Checked)
            {
                MainMenu_Form.Instance.CategoryPurchaseList.Add(new Category(name));
                int newRowIndex = Purchase_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            else
            {
                MainMenu_Form.Instance.CategorySaleList.Add(new Category(name));
                int newRowIndex = Sale_DataGridView.Rows.Add(name);
                DataGridViewManager.DataGridViewRowsAdded(selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            string message = $"Added category '{name}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(ThingsThatHaveChangedInFile, 3, message);

            Category_TextBox.Text = "";
            Category_TextBox.Focus();
        }
        private void Purchase_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Purchase_RadioButton.Checked)
            {
                Purchase_DataGridView.Visible = true;
                Sale_DataGridView.Visible = false;
                Purchase_DataGridView.ClearSelection();
                selectedDataGridView = Purchase_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategoryPurchases;
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, Purchase_DataGridView);
            }
        }
        private void Sale_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Sale_RadioButton.Checked)
            {
                Sale_DataGridView.Visible = true;
                Purchase_DataGridView.Visible = false;
                Sale_DataGridView.ClearSelection();
                selectedDataGridView = Sale_DataGridView;
                MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.CategorySales;
                VaidateCategoryTextBox();
                LabelManager.ShowTotalLabel(Total_Label, Sale_DataGridView);
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
                LabelManager.ShowLabelWithBaseText(ShowingResultsFor_Label, Search_TextBox.Text.Trim());
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
        private Guna2DataGridView selectedDataGridView;

        // DataGridView getters
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guna2DataGridView Purchase_DataGridView { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guna2DataGridView Sale_DataGridView { get; set; }

        // DataGridView methods
        private void ConstructDataGridViews()
        {
            Purchase_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(Purchase_DataGridView, "purchases_DataGridView", ColumnHeaders, null, this);
            Purchase_DataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - _topForDataGridView - 70);
            Purchase_DataGridView.Location = new Point((ClientSize.Width - Purchase_DataGridView.Width) / 2, _topForDataGridView);
            Purchase_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            Purchase_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;

            Sale_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(Sale_DataGridView, "sales_DataGridView", ColumnHeaders, null, this);
            Sale_DataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - _topForDataGridView - 70);
            Sale_DataGridView.Location = new Point((ClientSize.Width - Sale_DataGridView.Width) / 2, _topForDataGridView);
            Sale_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            Sale_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Category;
            ThemeManager.CustomizeScrollBar(Sale_DataGridView);
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
        private void ClosePanels()
        {
            TextBoxManager.HideRightClickPanel();
            RightClickDataGridViewRowMenu.Hide();
        }
    }
}