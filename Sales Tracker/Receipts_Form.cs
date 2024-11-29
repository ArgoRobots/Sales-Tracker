﻿using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.ComponentModel;

namespace Sales_Tracker
{
    public partial class Receipts_Form : Form
    {
        // Properties
        private DateTime oldestDate;
        private readonly MainMenu_Form.SelectedOption oldOption;

        // Init.
        public Receipts_Form()
        {
            InitializeComponent();

            oldOption = MainMenu_Form.Instance.Selected;

            MainMenu_Form.IsProgramLoading = true;
            DataGridViewManager.InitializeDataGridView(Receipts_DataGridView, "Receipts_DataGridView", Receipts_DataGridView.Size, ColumnHeaders, null, this);
            Receipts_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Receipts;
            AddAllReceiptsAndGetOldestDate();
            Receipts_DataGridView.SelectionChanged += Receipts_DataGridView_SelectionChanged;

            if (Receipts_DataGridView.Rows.Count > 0)
            {
                From_DateTimePicker.Value = oldestDate;
                To_DateTimePicker.Value = DateTime.Now;
            }
            MainMenu_Form.IsProgramLoading = false;

            SetTheme();
            LabelManager.ShowTotalLabel(Total_Label, Receipts_DataGridView, true);
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Receipts_DataGridView);

            CenterControls();
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void Receipts_DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            ExportSelected_Button.Enabled = Receipts_DataGridView.SelectedRows.Count > 0;
        }
        private void SetAccessibleDescriptions()
        {
            Search_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            FilterByDate_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            From_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            To_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            IncludePurchaseReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            IncludeSaleReceipts_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }
        private void SetTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(ExportSelected_Button);
            Theme.MakeGButtonBlueSecondary(ClearFilters_Button);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Search_TextBox);
        }

        // Form event handlers
        private void Receipts_Form_Shown(object sender, EventArgs e)
        {
            CenterControls();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Receipts_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
        }

        // Event handlers
        private void ClearFilters_Button_Click(object sender, EventArgs e)
        {
            Search_TextBox.Text = "";
            From_DateTimePicker.Value = oldestDate;
            To_DateTimePicker.Value = DateTime.Now;
        }
        private void ExportSelected_Button_Click(object sender, EventArgs e)
        {
            ReceiptManager.ExportSelectedReceipts(Receipts_DataGridView);
        }
        private void FilterByDate_Label_Click(object sender, EventArgs e)
        {
            FilterByDate_CheckBox.Checked = !FilterByDate_CheckBox.Checked;
        }
        private void IncludeSaleReceipts_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AddAllReceiptsAndGetOldestDate();
        }
        private void IncludeSaleReceipts_Label_Click(object sender, EventArgs e)
        {
            IncludeSaleReceipts_CheckBox.Checked = !IncludeSaleReceipts_CheckBox.Checked;
        }
        private void IncludePurchaseReceipts_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AddAllReceiptsAndGetOldestDate();
        }
        private void IncludePurchaseReceipts_Label_Click(object sender, EventArgs e)
        {
            IncludePurchaseReceipts_CheckBox.Checked = !IncludePurchaseReceipts_CheckBox.Checked;
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Text = "";
        }

        // DataGridView
        public enum Column
        {
            Type,
            ID,
            Product,
            Category,
            Company,
            Date,
            Total
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.Type, "Type" },
            { Column.ID, "Transaction ID" },
            { Column.Product, "Product name" },
            { Column.Category, "Category" },
            { Column.Company, "Company" },
            { Column.Date, "Date" },
            { Column.Total, "Total revenue" }
        };

        // Methods
        /// <summary>
        /// Center the checkboxes between form left and DateTimePicker, and search controls between DateTimePicker and form right.
        /// </summary>
        private void CenterControls()
        {
            // Center checkboxes between form left and DateTimePicker
            int leftEdge = 0;
            int rightEdge = From_DateTimePicker.Left;
            int centerX = leftEdge + (rightEdge - leftEdge) / 2;
            int spacing = IncludePurchaseReceipts_Label.Left - IncludePurchaseReceipts_CheckBox.Right;

            // Calculate total widths of the checkboxes and labels
            int purchaseTotalWidth = IncludePurchaseReceipts_CheckBox.Width + spacing + IncludePurchaseReceipts_Label.Width;
            int saleTotalWidth = IncludeSaleReceipts_CheckBox.Width + spacing + IncludeSaleReceipts_Label.Width;
            int maxTotalWidth = Math.Max(purchaseTotalWidth, saleTotalWidth);

            // Compute the left position to center the checkbox controls
            int controlsLeft = centerX - (maxTotalWidth / 2);

            // Position the checkbox controls
            IncludePurchaseReceipts_CheckBox.Left = controlsLeft;
            IncludePurchaseReceipts_Label.Left = IncludePurchaseReceipts_CheckBox.Right + spacing;
            IncludeSaleReceipts_CheckBox.Left = controlsLeft;
            IncludeSaleReceipts_Label.Left = IncludeSaleReceipts_CheckBox.Right + spacing;

            // Center search controls between DateTimePicker right and form right
            int searchLeftEdge = From_DateTimePicker.Right;
            int searchRightEdge = ClientSize.Width;
            int searchCenterX = searchLeftEdge + (searchRightEdge - searchLeftEdge) / 2;
            int searchSpacing = Search_TextBox.Left - Search_Label.Right;

            // Calculate total width of search controls
            int searchTotalWidth = Search_Label.Width + searchSpacing + Search_TextBox.Width;

            // Compute the left position to center the search controls
            int searchControlsLeft = searchCenterX - (searchTotalWidth / 2);

            // Position the search controls
            Search_Label.Left = searchControlsLeft;
            Search_TextBox.Left = Search_Label.Right + searchSpacing;
        }
        private void AddAllReceiptsAndGetOldestDate()
        {
            // Save the current sort order
            DataGridViewColumn sortedColumn = Receipts_DataGridView.SortedColumn;
            ListSortDirection sortDirection = ListSortDirection.Ascending;
            if (Receipts_DataGridView.SortOrder == SortOrder.Descending)
            {
                sortDirection = ListSortDirection.Descending;
            }

            Receipts_DataGridView.Rows.Clear();
            oldestDate = default;

            if (IncludeSaleReceipts_CheckBox.Checked)
            {
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Sale_DataGridView, "Sale");
            }

            if (IncludePurchaseReceipts_CheckBox.Checked)
            {
                AddReceiptsFromDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, "Purchase");
            }

            DataGridViewManager.ScrollToTopOfDataGridView(Receipts_DataGridView);

            // Restore the previous sort order
            if (sortedColumn != null)
            {
                Receipts_DataGridView.Sort(sortedColumn, sortDirection);
            }

            FilterDataGridView(null, null);
        }
        private void AddReceiptsFromDataGridView(Guna2DataGridView sourceDataGridView, string type)
        {
            string translatedType = LanguageManager.TranslateSingleString(type);

            foreach (DataGridViewRow row in sourceDataGridView.Rows)
            {
                if (row.Tag == null)
                {
                    continue;
                }

                // Add receipt filepath to row tag
                string receipt = "";
                if (row.Tag is (string dir, TagData))
                {
                    receipt = ReceiptManager.ProcessReceiptTextFromRowTag(dir);
                }
                else if (row.Tag is (List<string> items, TagData))
                {
                    receipt = items[^1];
                    receipt = ReceiptManager.ProcessReceiptTextFromRowTag(receipt);
                }

                if (!File.Exists(receipt))
                {
                    continue;
                }

                Receipts_DataGridView.Rows.Add(
                    translatedType,
                    row.Cells[MainMenu_Form.Column.ID.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Product.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Category.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Company.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString(),
                    row.Cells[MainMenu_Form.Column.Total.ToString()].Value.ToString());

                Receipts_DataGridView.Rows[^1].Tag = receipt;

                // Get oldest date
                DateTime currentDate = DateTime.Parse(row.Cells[MainMenu_Form.Column.Date.ToString()].Value.ToString());
                if (oldestDate == default || oldestDate > currentDate)
                {
                    oldestDate = currentDate;
                }
            }
        }
        private void FilterDataGridView(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in Receipts_DataGridView.Rows)
            {
                bool visible = DataGridViewManager.FilterRowBySearchTerms(row, Search_TextBox.Text.Trim());

                if (FilterByDate_CheckBox.Checked)
                {
                    DateTime date = DateTime.Parse(row.Cells[Column.Date.ToString()].Value.ToString());
                    if (date < From_DateTimePicker.Value || date > To_DateTimePicker.Value)
                    {
                        visible = false;
                    }
                }

                row.Visible = visible;
            }

            DataGridViewManager.UpdateAlternatingRowColors(Receipts_DataGridView);
            LabelManager.ShowTotalLabel(Total_Label, Receipts_DataGridView, true);
        }
    }
}