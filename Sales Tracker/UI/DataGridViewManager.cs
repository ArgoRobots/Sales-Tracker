using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Theme;
using System.ComponentModel;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages the setup, interactions, and event handling for DataGridView components in the.
    /// This class includes methods for initializing DataGridViews, handling row and cell events, 
    /// and customizing DataGridView behavior, such as right-click context menus and cell style updates.
    /// </summary>
    public class DataGridViewManager
    {
        // Properties
        private static DataGridViewRow _removedRow;
        private static readonly string _deleteAction = LanguageManager.TranslateString("deleted");
        private static readonly string _moveAction = LanguageManager.TranslateString("moved");
        private static DataGridViewCell _currentlyHoveredNoteCell;
        private static bool _isMouseDown;
        private static int RowHeight => (int)(35 * DpiHelper.GetRelativeDpiScale());
        private static int ColumnHeaderHeight => (int)(60 * DpiHelper.GetRelativeDpiScale());

        // Getters
        public static bool DoNotDeleteRows { get; set; }
        public static DataGridViewRow SelectedRowInMainMenu { get; set; }

        // Construct DataGridView
        public static void InitializeDataGridView<TEnum>(Guna2DataGridView dataGridView, string name, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad, Control parent) where TEnum : Enum
        {
            dataGridView.Name = name;
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView.ColumnHeadersHeight = ColumnHeaderHeight;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.RowTemplate.Height = RowHeight;
            dataGridView.RowTemplate.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = CustomColors.Text;
            dataGridView.Theme = CustomColors.DataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.ControlBack;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView.ScrollBars = ScrollBars.Vertical;

            dataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            dataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
            dataGridView.MouseDown += DataGridView_MouseDown;
            dataGridView.MouseMove += DataGridView_MouseMove;
            dataGridView.MouseUp += DataGridView_MouseUp;
            dataGridView.KeyDown += DataGridView_KeyDown;
            dataGridView.CellMouseClick += DataGridView_CellMouseClick;
            dataGridView.CellMouseMove += DataGridView_CellMouseMove;
            dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;
            dataGridView.ColumnHeaderMouseClick += DataGridView_ColumnHeaderMouseClick;
            dataGridView.SortCompare += DataGridView_SortCompare;

            LoadColumns(dataGridView, columnHeaders, columnsToLoad);
            ThemeManager.UpdateDataGridViewHeaderTheme(dataGridView);
            parent.Controls.Add(dataGridView);
        }

        // DataGridView event handlers
        public static void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }
            MainMenu_Form.Instance.AlignTotalLabels();
        }
        public static void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            if (DoNotDeleteRows)
            {
                e.Cancel = true;
                return;
            }

            _removedRow = e.Row;

            switch (MainMenu_Form.Instance.Selected)
            {
                case MainMenu_Form.SelectedOption.Purchases:
                    HandlePurchasesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.Sales:
                    HandleSalesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.ProductPurchases:
                    HandleProductPurchasesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.ProductSales:
                    HandleProductSalesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.CategoryPurchases:
                    HandleCategoryPurchasesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.CategorySales:
                    HandleCategorySalesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.Accountants:
                    HandleAccountantsDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.Companies:
                    HandleCompaniesDeletion(e);
                    break;

                case MainMenu_Form.SelectedOption.ItemsInPurchase:
                case MainMenu_Form.SelectedOption.ItemsInSale:
                    HandleItemsDeletion(sender, e);
                    break;
            }
        }
        public static void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            DataGridViewRowChanged((Guna2DataGridView)sender, MainMenu_Form.Instance.Selected);

            // Remove receipt from file
            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales && _removedRow?.Tag != null)
            {
                string tagValue = "";

                if (_removedRow.Tag is (List<string> tagList, TagData))
                {
                    tagValue = ReceiptManager.ProcessReceiptTextFromRowTag(tagList[^1]);
                }
                else if (_removedRow.Tag is (string tagString, TagData))
                {
                    tagValue = ReceiptManager.ProcessReceiptTextFromRowTag(tagString);
                }

                if (tagValue != "")
                {
                    Directories.DeleteFile(tagValue);
                }

                _removedRow = null;
            }
        }
        public static void DataGridViewRowChanged(Guna2DataGridView dataGridView, MainMenu_Form.SelectedOption selected)
        {
            if (selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.UpdateTotalLabels();
                MainMenu_Form.Instance.LoadOrRefreshMainCharts();
                MainMenu_Form.SaveDataGridViewToFileAsJson(dataGridView, selected);
            }
            else if (selected is MainMenu_Form.SelectedOption.CategoryPurchases or MainMenu_Form.SelectedOption.CategorySales or
               MainMenu_Form.SelectedOption.ProductPurchases or MainMenu_Form.SelectedOption.ProductSales)
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(selected);
            }
            else if (selected is MainMenu_Form.SelectedOption.Accountants or MainMenu_Form.SelectedOption.Companies)
            {
                MainMenu_Form.SaveDataGridViewToFile(dataGridView, selected);
            }
        }
        private static void DataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDown = true;
        }
        private static void DataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                CustomControls.CloseAllPanels();
            }
        }
        private static void DataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            Guna2DataGridView grid = (Guna2DataGridView)sender;

            CustomControls.CloseAllPanels();

            SelectRowAndDeselectAllOthers(grid, e);

            if (!IsValidRightClick(grid, e))
            {
                return;
            }

            DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);
            if (info.Type != DataGridViewHitTestType.Cell)
            {
                return;
            }

            ConfigureRightClickDataGridViewMenuButtons(grid);
            PositionRightClickDataGridViewMenu(grid, e, info);
        }
        private static void DataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                MainMenu_Form.Instance.ClosePanels();

                Guna2DataGridView grid = (Guna2DataGridView)sender;

                string message;
                if (grid.SelectedRows.Count == 1)
                {
                    message = "Are you sure you want to delete this row?";
                }
                else
                {
                    message = "Are you sure you want to delete the selected rows?";
                }
                CustomMessageBoxResult result = CustomMessageBox.Show("Delete rows", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    DoNotDeleteRows = true;
                    UnselectAllRowsInCurrentDataGridView(grid);
                }
            }
        }
        private static void DataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (IsNoteCell(e, dataGridView))
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string idCell = dataGridView.Rows[e.RowIndex].Cells[ReadOnlyVariables.ID_column].Value.ToString();

                if (IsClickOnText(cell, out Rectangle hitbox))
                {
                    Point mousePos = dataGridView.PointToClient(Cursor.Position);
                    if (hitbox.Contains(mousePos))
                    {
                        AddUnderlineToCell(cell);
                        string type = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases ? "purchase" : "sale";

                        CustomMessageBox.ShowWithFormat("Note for {0} {1}",
                            cell.Tag?.ToString(),
                            CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok,
                            type, idCell);
                    }
                }
            }
        }
        private static void DataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (IsNoteCell(e, dataGridView))
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                _currentlyHoveredNoteCell = cell;  // Track the current note cell

                if (IsClickOnText(cell, out Rectangle hitbox))
                {
                    // Draw a rectangle around the hitbox for debugging
                    //using (Graphics g = dataGridView.CreateGraphics())
                    //{
                    //    g.DrawRectangle(Pens.Red, hitbox);
                    //}

                    Point mousePos = dataGridView.PointToClient(Cursor.Position);
                    UpdateCellStyleBasedOnMousePosition(cell, hitbox, mousePos);
                }
            }
            else
            {
                // If we're not in a note cell, clear any previously hovered note cell
                if (_currentlyHoveredNoteCell != null)
                {
                    RestoreNoteCellUnderline(_currentlyHoveredNoteCell);
                    _currentlyHoveredNoteCell = null;
                }
            }
        }
        private static void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            // Always restore underline for any currently tracked note cell
            if (_currentlyHoveredNoteCell != null)
            {
                RestoreNoteCellUnderline(_currentlyHoveredNoteCell);
                _currentlyHoveredNoteCell = null;
            }

            // Fallback: also check the cell being left (original logic)
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.ColumnIndex < dataGridView.Columns.Count)
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null && cell.Value.ToString() == ReadOnlyVariables.Show_text && cell.Tag is string)
                {
                    RestoreNoteCellUnderline(cell);
                }
            }
        }
        private static void DataGridView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateAlternatingRowColors((DataGridView)sender);
        }
        private static void DataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            // Check if this is a money column
            if (IsMoneyColumn(((DataGridView)sender).Columns[e.Column.Index].Name))
            {
                // Parse values as decimals for proper numeric comparison
                if (decimal.TryParse(e.CellValue1?.ToString().Replace(MainMenu_Form.CurrencySymbol, "").Replace(",", ""), out decimal num1) &&
                    decimal.TryParse(e.CellValue2?.ToString().Replace(MainMenu_Form.CurrencySymbol, "").Replace(",", ""), out decimal num2))
                {
                    e.SortResult = num1.CompareTo(num2);
                    e.Handled = true;
                }
            }
        }
        private static readonly HashSet<string> MoneyColumns =
        [
            ReadOnlyVariables.PricePerUnit_column,
            ReadOnlyVariables.Shipping_column,
            ReadOnlyVariables.Tax_column,
            ReadOnlyVariables.Fee_column,
            ReadOnlyVariables.Discount_column,
            ReadOnlyVariables.ChargedDifference_column,
            ReadOnlyVariables.Total_column
        ];
        private static bool IsMoneyColumn(string columnName)
        {
            return MoneyColumns.Contains(columnName);
        }

        // Methods for DataGridView_UserDeletingRow
        private static void HandlePurchasesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "purchase";
            string columnName = ReadOnlyVariables.ID_column;
            string name = e.Row.Cells[columnName].Value?.ToString();

            string message = $"Deleted {type} '{name}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleSalesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "sale";
            string columnName = ReadOnlyVariables.ID_column;
            string name = e.Row.Cells[columnName].Value?.ToString();

            string message = $"Deleted {type} '{name}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleProductPurchasesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "product";
            string columnName = Products_Form.Column.ProductName.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (IsThisBeingUsedByDataGridView(type, ReadOnlyVariables.Product_column, valueBeingRemoved, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            // Remove product from list
            MainMenu_Form.Instance.CategoryPurchaseList.ForEach(c =>
                c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == valueBeingRemoved)));

            // In case the product name that is being deleted is in the TextBox
            Products_Form.Instance.ValidateProductNameTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleProductSalesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "product";
            string columnName = Products_Form.Column.ProductName.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (IsThisBeingUsedByDataGridView(type, ReadOnlyVariables.Product_column, valueBeingRemoved, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            // Remove product from list
            MainMenu_Form.Instance.CategorySaleList.ForEach(c =>
                c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == valueBeingRemoved)));

            // In case the product name that is being deleted is in the TextBox
            Products_Form.Instance.ValidateProductNameTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleCategoryPurchasesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "category";
            string columnName = Categories_Form.Column.CategoryName.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (!CanCategoryBeMovedOrDeleted(valueBeingRemoved, MainMenu_Form.Instance.CategoryPurchaseList, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            // Remove category from list
            MainMenu_Form.Instance.CategoryPurchaseList.Remove(
                MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == valueBeingRemoved));

            // In case the category name that is being deleted is in the TextBox
            Categories_Form.Instance.VaidateCategoryTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleCategorySalesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "category";
            string columnName = Categories_Form.Column.CategoryName.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (!CanCategoryBeMovedOrDeleted(valueBeingRemoved, MainMenu_Form.Instance.CategorySaleList, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            // Remove category from list
            MainMenu_Form.Instance.CategorySaleList.Remove(
                MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == valueBeingRemoved));

            // In case the category name that is being deleted is in the TextBox
            Categories_Form.Instance.VaidateCategoryTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleAccountantsDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "accountant";
            string columnName = Accountants_Form.Column.AccountantName.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (IsThisBeingUsedByDataGridView(type, ReadOnlyVariables.Accountant_column, valueBeingRemoved, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            if (MainMenu_Form.SelectedAccountant == valueBeingRemoved)
            {
                CustomMessageBox.Show(
                    "Cannot delete accountant",
                    "You cannot delete the accountant you are currently signed in as.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.Ok);

                e.Cancel = true;
                return;
            }

            if (MainMenu_Form.Instance.AccountantList.Count == 1)
            {
                CustomMessageBox.Show(
                    "Cannot delete accountant",
                    "You must have at least one accountant.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.Ok);

                e.Cancel = true;
                return;
            }

            // Remove accountant from list
            MainMenu_Form.Instance.AccountantList.Remove(valueBeingRemoved);

            // In case the accountant name that is being deleted is in the TextBox
            Accountants_Form.Instance.VaidateAccountantTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleCompaniesDeletion(DataGridViewRowCancelEventArgs e)
        {
            string type = "company";
            string columnName = Companies_Form.Column.Company.ToString();
            string valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

            if (IsThisBeingUsedByDataGridView(type, ReadOnlyVariables.Company_column, valueBeingRemoved, _deleteAction))
            {
                e.Cancel = true;
                return;
            }

            // Remove company from list
            MainMenu_Form.Instance.CompanyList.Remove(valueBeingRemoved);

            // In case the company name that is being deleted is in the TextBox
            Companies_Form.Instance.ValidateCompanyTextBox();

            string message = $"Deleted {type} '{valueBeingRemoved}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }
        private static void HandleItemsDeletion(object sender, DataGridViewRowCancelEventArgs e)
        {
            string columnName = ReadOnlyVariables.Product_column;
            string productName = e.Row.Cells[columnName].Value?.ToString();

            if (!(SelectedRowInMainMenu.Tag is (List<string> itemList, TagData)))
            {
                return;
            }

            string transactionType = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInPurchase
                ? "purchase" : "sale";

            if (e.Row.DataGridView.Rows.Count == 1)
            {
                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                    "Delete the {0}",
                    "Deleting the last item will also delete the {0}.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.OkCancel,
                    transactionType);

                if (result != CustomMessageBoxResult.Ok)
                {
                    e.Cancel = true;
                    return;
                }

                Tools.CloseOpenForm<ItemsInTransaction_Form>();
                Guna2DataGridView grid = (Guna2DataGridView)sender;
                grid.Rows.Remove(SelectedRowInMainMenu);
            }
            else
            {
                // Remove the row from the tag
                itemList.RemoveAt(e.Row.Index);
            }

            string message = $"Deleted item '{productName}' in {transactionType}";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
        }

        // Methods for DataGridView for event handlers
        private static void RestoreNoteCellUnderline(DataGridViewCell cell)
        {
            if (cell != null && cell.Value != null && cell.Value.ToString() == ReadOnlyVariables.Show_text)
            {
                AddUnderlineToCell(cell);
            }
        }
        private static bool IsNoteCell(DataGridViewCellMouseEventArgs e, DataGridView dataGridView)
        {
            if (e?.RowIndex < 0) { return false; }

            DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            return cell.Value?.ToString() == ReadOnlyVariables.Show_text && cell.Tag is string;
        }
        private static bool IsClickOnText(DataGridViewCell cell, out Rectangle hitbox)
        {
            hitbox = Rectangle.Empty;

            if (cell.Value != null && cell.Value.ToString() == ReadOnlyVariables.Show_text)
            {
                Size textSize = TextRenderer.MeasureText(cell.Value.ToString(), cell.DataGridView.Font);
                byte padding = 3;
                textSize.Width += 10 + padding + padding;
                textSize.Height += padding + padding;

                Rectangle cellRect = cell.DataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
                int relativeX = cellRect.X + cell.InheritedStyle.Padding.Left - padding;
                int relativeY = cellRect.Y + (cell.Size.Height - textSize.Height) / 2;
                hitbox = new Rectangle(relativeX, relativeY, textSize.Width, textSize.Height);

                return true;
            }
            return false;
        }
        private static void UpdateCellStyleBasedOnMousePosition(DataGridViewCell cell, Rectangle hitbox, Point mousePos)
        {
            if (hitbox.Contains(mousePos))
            {
                RemoveUnderlineFromCell(cell);
            }
            else
            {
                AddUnderlineToCell(cell);
            }
        }

        // Methods for DataGridView for MouseUp
        private static bool IsValidRightClick(Guna2DataGridView grid, MouseEventArgs e)
        {
            return e.Button == MouseButtons.Right && grid.Rows.Count > 0;
        }
        private static void SelectRowAndDeselectAllOthers(Guna2DataGridView grid, MouseEventArgs e)
        {
            DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);
            if (info.RowIndex == -1)
            {
                return;
            }
            if (!grid.Rows[info.RowIndex].Selected)
            {
                UnselectAllRowsInCurrentDataGridView(grid);
            }

            // Select current row
            grid.Rows[info.RowIndex].Selected = true;
            grid.Focus();
        }
        private static void ConfigureRightClickDataGridViewMenuButtons(Guna2DataGridView grid)
        {
            FlowLayoutPanel flowPanel = RightClickDataGridView_Panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            RightClickDataGridView_Panel.Tag = grid;  // This is used in the button event handlers

            // First, hide all buttons
            foreach (Control control in flowPanel.Controls)
            {
                control.Visible = false;
            }

            MainMenu_Form.SelectedOption selectedOption = MainMenu_Form.Instance.Selected;
            bool isSingleRowSelected = grid.SelectedRows.Count == 1;
            bool isPurchasesOrSales = selectedOption == MainMenu_Form.SelectedOption.Purchases ||
                                      selectedOption == MainMenu_Form.SelectedOption.Sales;
            bool isTransactionView = selectedOption == MainMenu_Form.SelectedOption.ItemsInPurchase ||
                                     selectedOption == MainMenu_Form.SelectedOption.ItemsInSale;
            int currentIndex = 0;

            // Add ModifyBtn
            if (isSingleRowSelected)
            {
                _rightClickDataGridView_ModifyBtn.Visible = true;
                flowPanel.Controls.SetChildIndex(_rightClickDataGridView_ModifyBtn, currentIndex++);
            }

            // Add MoveBtn
            if (selectedOption == MainMenu_Form.SelectedOption.CategoryPurchases)
            {
                string text = LanguageManager.TranslateString("Move category to sales");
                _rightClickDataGridView_MoveBtn.Visible = true;
                _rightClickDataGridView_MoveBtn.Text = text;
                flowPanel.Controls.SetChildIndex(_rightClickDataGridView_MoveBtn, currentIndex++);
            }
            else if (selectedOption == MainMenu_Form.SelectedOption.CategorySales)
            {
                string text = LanguageManager.TranslateString("Move category to purchases");
                _rightClickDataGridView_MoveBtn.Visible = true;
                _rightClickDataGridView_MoveBtn.Text = text;
                flowPanel.Controls.SetChildIndex(_rightClickDataGridView_MoveBtn, currentIndex++);
            }

            if (isPurchasesOrSales)
            {
                // Add ShowItemsBtn
                if (isSingleRowSelected && grid.SelectedRows[0].Tag is (List<string>, TagData))
                {
                    _rightClickDataGridView_ShowItemsBtn.Visible = true;
                    flowPanel.Controls.SetChildIndex(_rightClickDataGridView_ShowItemsBtn, currentIndex++);
                }

                // Add ExportReceiptBtn
                if (AnySelectedRowHasReceipt(grid))
                {
                    _rightClickDataGridView_ExportReceiptBtn.Visible = true;
                    flowPanel.Controls.SetChildIndex(_rightClickDataGridView_ExportReceiptBtn, currentIndex++);
                }
            }

            if (isPurchasesOrSales || isTransactionView)
            {
                // Add ReturnBtn and/or UndoReturnBtn based on current return status
                if (isSingleRowSelected)
                {
                    DataGridViewRow selectedRow;

                    // Check if we're in the items view of a transaction
                    if (selectedOption == MainMenu_Form.SelectedOption.ItemsInPurchase ||
                        selectedOption == MainMenu_Form.SelectedOption.ItemsInSale)
                    {
                        // Use the main transaction row for return status checks
                        selectedRow = SelectedRowInMainMenu;
                    }
                    else
                    {
                        // Normal case - we're in the main purchases/sales view
                        selectedRow = grid.SelectedRows[0];
                    }

                    bool isFullyReturned = ReturnManager.IsTransactionFullyReturned(selectedRow);
                    bool isPartiallyReturned = ReturnManager.IsTransactionPartiallyReturned(selectedRow);

                    if (isFullyReturned)
                    {
                        // Fully returned - only show undo button
                        _rightClickDataGridView_UndoReturnBtn.Visible = true;
                        _rightClickDataGridView_UndoReturnBtn.Text = LanguageManager.TranslateString("Undo return");
                        flowPanel.Controls.SetChildIndex(_rightClickDataGridView_UndoReturnBtn, currentIndex++);
                    }
                    else if (isPartiallyReturned)
                    {
                        // Partially returned - show both buttons
                        _rightClickDataGridView_ReturnBtn.Visible = true;
                        _rightClickDataGridView_ReturnBtn.Text = LanguageManager.TranslateString("Return more items");
                        flowPanel.Controls.SetChildIndex(_rightClickDataGridView_ReturnBtn, currentIndex++);

                        _rightClickDataGridView_UndoReturnBtn.Visible = true;
                        _rightClickDataGridView_UndoReturnBtn.Text = LanguageManager.TranslateString("Undo partial return");
                        flowPanel.Controls.SetChildIndex(_rightClickDataGridView_UndoReturnBtn, currentIndex++);
                    }
                    else
                    {
                        // Not returned - only show return button
                        _rightClickDataGridView_ReturnBtn.Visible = true;
                        _rightClickDataGridView_ReturnBtn.Text = LanguageManager.TranslateString("Return product");
                        flowPanel.Controls.SetChildIndex(_rightClickDataGridView_ReturnBtn, currentIndex++);
                    }
                }
            }

            // Add DeleteBtn
            RightClickDataGridView_DeleteBtn.Visible = true;
            flowPanel.Controls.SetChildIndex(RightClickDataGridView_DeleteBtn, currentIndex);
        }
        private static bool AnySelectedRowHasReceipt(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                if (row.Tag is (List<string> tagList, TagData) && IsLastItemAReceipt(tagList[^1]))
                {
                    return true;
                }
                else if (row.Tag is (string item, TagData) && IsLastItemAReceipt(item))
                {
                    return true;
                }
            }
            return false;
        }
        private static void PositionRightClickDataGridViewMenu(Guna2DataGridView grid, MouseEventArgs e, DataGridView.HitTestInfo info)
        {
            Form parentForm = grid.FindForm();
            int formHeight = parentForm.ClientSize.Height;
            int formWidth = parentForm.ClientSize.Width;

            CustomControls.SetRightClickMenuHeight(RightClickDataGridView_Panel);

            SetHorizontalPosition(grid, e, formWidth);
            SetVerticalPosition(grid, info, formHeight);

            grid.Parent.Controls.Add(RightClickDataGridView_Panel);
            RightClickDataGridView_Panel.BringToFront();
        }
        private static void SetHorizontalPosition(Guna2DataGridView grid, MouseEventArgs e, int formWidth)
        {
            if (grid.Left + RightClickDataGridView_Panel.Width + e.X - ReadOnlyVariables.OffsetRightClickPanel + ReadOnlyVariables.PaddingRightClickPanel > formWidth)
            {
                RightClickDataGridView_Panel.Left = formWidth - RightClickDataGridView_Panel.Width - ReadOnlyVariables.PaddingRightClickPanel;
            }
            else
            {
                RightClickDataGridView_Panel.Left = grid.Left + e.X - ReadOnlyVariables.OffsetRightClickPanel;
            }
        }
        private static void SetVerticalPosition(Guna2DataGridView grid, DataGridView.HitTestInfo info, int formHeight)
        {
            int rowHeight = grid.Rows[0].Height;
            int headerHeight = grid.ColumnHeadersHeight;

            // Calculate scroll offset considering only visible rows
            int scrollOffset = 0;
            for (int i = 0; i < grid.FirstDisplayedScrollingRowIndex; i++)
            {
                if (grid.Rows[i].Visible)
                {
                    scrollOffset += rowHeight;
                }
            }

            // Calculate position up to target row, counting only visible rows
            int visibleRowsBeforeTarget = 0;
            for (int i = 0; i < info.RowIndex; i++)
            {
                if (grid.Rows[i].Visible)
                {
                    visibleRowsBeforeTarget++;
                }
            }

            int rowTop = headerHeight + ((visibleRowsBeforeTarget + 1) * rowHeight) - scrollOffset + grid.Top;

            if (rowTop + RightClickDataGridView_Panel.Height > formHeight - ReadOnlyVariables.PaddingRightClickPanel)
            {
                RightClickDataGridView_Panel.Top = formHeight - RightClickDataGridView_Panel.Height - ReadOnlyVariables.PaddingRightClickPanel;
            }
            else
            {
                RightClickDataGridView_Panel.Top = rowTop;
            }
        }

        // Methods for DataGridView
        private static bool IsLastItemAReceipt(string lastItem)
        {
            if (lastItem.StartsWith(ReadOnlyVariables.Receipt_text))
            {
                lastItem = ReceiptManager.ProcessReceiptTextFromRowTag(lastItem);
                return File.Exists(lastItem);
            }
            return false;
        }
        public static void DataGridViewRowsAdded(Guna2DataGridView grid, DataGridViewRowsAddedEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            DataGridViewRowChanged(grid, MainMenu_Form.Instance.Selected);
            DataGridViewRow row;

            if (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count)
            {
                row = grid.Rows[e.RowIndex];
            }
            else
            {
                Log.Error_RowIsOutOfRange();
                return;
            }

            SortDataGridViewByCurrentDirection(grid);

            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
            }

            // Calculate the middle index
            int visibleRowCount = grid.DisplayedRowCount(true);
            int middleIndex = Math.Max(0, row.Index - (visibleRowCount / 2) + 1);

            // Ensure the row at middleIndex is visible
            if (middleIndex >= 0 && middleIndex < grid.RowCount && grid.Rows[middleIndex].Visible)
            {
                grid.FirstDisplayedScrollingRowIndex = middleIndex;
            }

            // Select the added row
            UnselectAllRowsInCurrentDataGridView(grid);
            grid.Rows[row.Index].Selected = true;
        }
        private static void LoadColumns<TEnum>(Guna2DataGridView dataGridView, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad = null) where TEnum : Enum
        {
            foreach (KeyValuePair<TEnum, string> columnHeader in columnHeaders)
            {
                if (columnsToLoad != null && !columnsToLoad.Contains(columnHeader.Key))
                {
                    continue;
                }

                DataGridViewTextBoxColumn column = new()
                {
                    HeaderText = columnHeader.Value,
                    Name = columnHeader.Key.ToString()
                };
                dataGridView.Columns.Add(column);
            }
        }
        public static string GetFilePathForDataGridView(MainMenu_Form.SelectedOption selected)
        {
            return selected switch
            {
                MainMenu_Form.SelectedOption.Purchases => Directories.Purchases_file,
                MainMenu_Form.SelectedOption.Sales => Directories.Sales_file,
                MainMenu_Form.SelectedOption.CategoryPurchases => Directories.CategoryPurchases_file,
                MainMenu_Form.SelectedOption.CategorySales => Directories.CategorySales_file,
                MainMenu_Form.SelectedOption.ProductPurchases => Directories.CategoryPurchases_file,
                MainMenu_Form.SelectedOption.ProductSales => Directories.CategorySales_file,
                MainMenu_Form.SelectedOption.Accountants => Directories.Accountants_file,
                MainMenu_Form.SelectedOption.Companies => Directories.Companies_file,
                _ => ""
            };
        }
        private static void UnselectAllRowsInCurrentDataGridView(Guna2DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                row.Selected = false;
            }
        }
        public static bool DoesValueExistInDataGridView(Guna2DataGridView dataGridView, string column, string purchaseID)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (purchaseID == row.Cells[column].Value.ToString())
                {
                    return true;
                }
            }
            return false;
        }
        public static void SortFirstColumnAndSelectFirstRow(params Guna2DataGridView[] dataGridViews)
        {
            foreach (DataGridView dataGrid in dataGridViews)
            {
                if (dataGrid.Columns.Count > 0)
                {
                    dataGrid.Sort(dataGrid.Columns[0], ListSortDirection.Ascending);

                    // Select the first row
                    if (dataGrid.Rows.Count > 0)
                    {
                        dataGrid.ClearSelection();
                        dataGrid.Rows[0].Selected = true;
                    }
                }

                UpdateAlternatingRowColors(dataGrid);
            }
        }
        public static void UpdateRowWithMultipleItems(DataGridViewRow selectedRow)
        {
            List<string> items = selectedRow.Tag is (List<string> itemList, TagData) ? itemList : [];

            if (items.Count <= 1) { return; }

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;
            decimal itemsTotal = 0;
            int totalQuantity = 0;

            bool shouldSkipLastItem = items[^1].StartsWith(ReadOnlyVariables.Receipt_text);

            // Calculate total for all items
            for (int i = 0; i < (shouldSkipLastItem ? items.Count - 1 : items.Count); i++)
            {
                string[] itemDetails = items[i].Split(',');

                string currentCategoryName = itemDetails[1];
                string currentCountry = itemDetails[2];
                string currentCompany = itemDetails[3];
                int quantity = Convert.ToInt32(itemDetails[4]);
                decimal pricePerUnit = decimal.Parse(itemDetails[5]);
                itemsTotal += quantity * pricePerUnit;
                totalQuantity += quantity;

                // Check consistency for category, country, company
                if (firstCategoryName == null) { firstCategoryName = currentCategoryName; }
                else if (isCategoryNameConsistent && firstCategoryName != currentCategoryName) { isCategoryNameConsistent = false; }

                if (firstCountry == null) { firstCountry = currentCountry; }
                else if (isCountryConsistent && firstCountry != currentCountry) { isCountryConsistent = false; }

                if (firstCompany == null) { firstCompany = currentCompany; }
                else if (isCompanyConsistent && firstCompany != currentCompany) { isCompanyConsistent = false; }
            }

            // Update category, country, company fields
            string categoryName = isCategoryNameConsistent ? firstCategoryName : ReadOnlyVariables.EmptyCell;
            string country = isCountryConsistent ? firstCountry : ReadOnlyVariables.EmptyCell;
            string company = isCompanyConsistent ? firstCompany : ReadOnlyVariables.EmptyCell;

            selectedRow.Cells[ReadOnlyVariables.Category_column].Value = categoryName;
            selectedRow.Cells[ReadOnlyVariables.Country_column].Value = country;
            selectedRow.Cells[ReadOnlyVariables.Company_column].Value = company;
            selectedRow.Cells[ReadOnlyVariables.TotalItems_column].Value = totalQuantity;

            // Update charged difference
            decimal shipping = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Shipping_column].Value.ToString());
            decimal tax = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Tax_column].Value.ToString());
            decimal fee = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Fee_column].Value.ToString());
            decimal discount = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Discount_column].Value.ToString());

            bool isSale = selectedRow.DataGridView == MainMenu_Form.Instance.Sale_DataGridView;
            decimal expectedTotal;

            if (isSale)
            {
                expectedTotal = itemsTotal - shipping - discount;
            }
            else
            {
                expectedTotal = itemsTotal + shipping + tax + fee - discount;
            }

            // Calculate charged difference: Actual Total - Expected Total
            decimal actualTotal = Convert.ToDecimal(selectedRow.Cells[ReadOnlyVariables.Total_column].Value);
            decimal chargedDifference = actualTotal - expectedTotal;

            selectedRow.Cells[ReadOnlyVariables.ChargedDifference_column].Value = chargedDifference.ToString("N2");
        }
        public static void UpdateRowWithNoItems(DataGridViewRow selectedRow)
        {
            // Update charged difference
            int quantity = int.Parse(selectedRow.Cells[ReadOnlyVariables.TotalItems_column].Value.ToString());
            decimal pricePerUnit = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.PricePerUnit_column].Value.ToString());
            decimal shipping = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Shipping_column].Value.ToString());
            decimal tax = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Tax_column].Value.ToString());
            decimal fee = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Fee_column].Value.ToString());
            decimal discount = decimal.Parse(selectedRow.Cells[ReadOnlyVariables.Discount_column].Value.ToString());

            bool isSale = selectedRow.DataGridView == MainMenu_Form.Instance.Sale_DataGridView;
            decimal expectedTotal;

            if (isSale)
            {
                expectedTotal = quantity * pricePerUnit - discount;
            }
            else
            {
                expectedTotal = quantity * pricePerUnit + shipping + tax + fee - discount;
            }

            // Calculate charged difference: Actual Total - Expected Total
            decimal actualTotal = Convert.ToDecimal(selectedRow.Cells[ReadOnlyVariables.Total_column].Value);
            decimal chargedDifference = actualTotal - expectedTotal;

            selectedRow.Cells[ReadOnlyVariables.ChargedDifference_column].Value = chargedDifference.ToString("N2");
        }
        public static void AddNoteToCell(Guna2DataGridView grid, int newRowIndex, string note)
        {
            DataGridViewCell cell = grid.Rows[newRowIndex].Cells[^1];
            cell.Tag = note;
            AddUnderlineToCell(cell);
        }
        public static void AddUnderlineToCell(DataGridViewCell cell)
        {
            cell.Style.Font = new Font(cell.DataGridView.DefaultCellStyle.Font, FontStyle.Underline);
        }
        public static void RemoveUnderlineFromCell(DataGridViewCell cell)
        {
            // Remove underline by resetting the font without FontStyle.Underline
            if (cell.Style.Font != null)
            {
                cell.Style.Font = new Font(cell.Style.Font, cell.Style.Font.Style & ~FontStyle.Underline);
            }
            else
            {
                cell.Style.Font = new Font(cell.DataGridView.DefaultCellStyle.Font, cell.DataGridView.DefaultCellStyle.Font.Style & ~FontStyle.Underline);
            }
        }
        public static bool HasVisibleRows(params Guna2DataGridView[] dataGridViews)
        {
            foreach (DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Visible)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool HasVisibleRowsForReturn(params Guna2DataGridView[] dataGridViews)
        {
            foreach (DataGridView dataGridView in dataGridViews)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Visible && ReturnManager.IsTransactionReturned(row))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Updates row appearance for returns in ItemsInTransaction forms.
        /// Individual returned items will be displayed in red.
        /// </summary>
        public static void UpdateItemRowAppearanceForReturns(Guna2DataGridView itemsGrid, DataGridViewRow mainTransactionRow)
        {
            if (mainTransactionRow.Tag is not (List<string>, TagData))
            {
                return;
            }

            // Apply return styling to individual item rows
            for (int i = 0; i < itemsGrid.Rows.Count; i++)
            {
                DataGridViewRow itemRow = itemsGrid.Rows[i];
                bool isItemReturned = ReturnManager.IsItemReturned(mainTransactionRow, i);

                if (isItemReturned)
                {
                    // Mark returned items in red
                    itemRow.DefaultCellStyle.BackColor = CustomColors.ReturnedItemBackground;
                    itemRow.DefaultCellStyle.SelectionBackColor = CustomColors.ReturnedItemSelection;
                    itemRow.DefaultCellStyle.ForeColor = CustomColors.ReturnedItemText;
                }
                else
                {
                    // Reset to default colors for non-returned items
                    itemRow.DefaultCellStyle.BackColor = Color.Empty;
                    itemRow.DefaultCellStyle.SelectionBackColor = Color.Empty;
                    itemRow.DefaultCellStyle.ForeColor = Color.Empty;
                }
            }
        }
        private static void SortDataGridViewByCurrentDirection(DataGridView dataGridView)
        {
            if (dataGridView.SortedColumn == null)
            {
                return;
            }

            SortOrder sortOrder = dataGridView.SortOrder;
            DataGridViewColumn sortedColumn = dataGridView.SortedColumn;
            ListSortDirection direction = (sortOrder == SortOrder.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;
            dataGridView.Sort(sortedColumn, direction);
        }
        public static void UpdateAlternatingRowColors(DataGridView dataGridView)
        {
            int visibleRowIndex = 0;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Visible) { continue; }

                // Check return status for proper color application
                bool isFullyReturned = ReturnManager.IsTransactionFullyReturned(row);
                bool isPartiallyReturned = ReturnManager.IsTransactionPartiallyReturned(row);

                if (isFullyReturned)
                {
                    // Fully returned - red background
                    ReturnManager.UpdateRowAppearanceForReturn(row, true, false);
                }
                else if (isPartiallyReturned)
                {
                    // Partially returned - orange background
                    ReturnManager.UpdateRowAppearanceForReturn(row, false, true);
                }
                else
                {
                    // Not returned - apply alternating colors
                    row.DefaultCellStyle.BackColor = (visibleRowIndex % 2 == 0)
                        ? dataGridView.DefaultCellStyle.BackColor
                        : dataGridView.AlternatingRowsDefaultCellStyle.BackColor;

                    // Reset other styling
                    row.DefaultCellStyle.SelectionBackColor = Color.Empty;
                    row.DefaultCellStyle.ForeColor = Color.Empty;
                }

                visibleRowIndex++;
            }
        }
        public static void ScrollToTopOfDataGridView(Guna2DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = 0;
            }
        }

        /// <summary>
        /// Searches a DataGridView for the text in the search_TextBox.
        /// </summary>
        /// <returns>True if the searching label should be shown, or false if it should not be shown.</returns>
        public static bool SearchSelectedDataGridViewAndUpdateRowColors(Guna2DataGridView grid, Guna2TextBox search_TextBox)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                bool isVisible = row.Cells.Cast<DataGridViewCell>()
                                          .Any(cell => cell.Value != null && cell.Value.ToString()
                                          .Contains(search_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                row.Visible = isVisible;
            }

            UpdateAlternatingRowColors(grid);

            return !string.IsNullOrEmpty(search_TextBox.Text.Trim());
        }

        // Other methods
        /// <summary>
        /// Shows a MessageBox if the row is being used by another row.
        /// </summary>
        /// <returns>True if it's being used by another row; Otherwise, false.</returns>
        private static bool IsThisBeingUsedByDataGridView(string type, string columnName, string value, string action)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
            {
                if (row.Cells[columnName].Value.ToString() == value)
                {
                    ShowInUseMessage(type, action);
                    return true;
                }

                // Skip if we are not checking a product name
                if (columnName != ReadOnlyVariables.Product_column) { continue; }

                // Skip if transaction does not have multiple items
                if (row.Tag is not (List<string> items, TagData)) { continue; }

                // Do not check receipt if present
                int itemsToCheck = items[^1].StartsWith(ReadOnlyVariables.Receipt_text)
                    ? items.Count - 1
                    : items.Count;

                // Check each product name until match found
                for (int i = 0; i < itemsToCheck; i++)
                {
                    if (items[i].Split(',')[0] == value)
                    {
                        ShowInUseMessage(type, action);
                        return true;
                    }
                }
            }

            HandleValueDeletion(type, value);
            return false;
        }
        private static void ShowInUseMessage(string type, string action)
        {
            CustomMessageBox.ShowWithFormat(
                "Cannot be {0}",
                "This {1} is being used by a transaction and cannot be {0}.",
                CustomMessageBoxIcon.Info,
                CustomMessageBoxButtons.Ok,
                action, type);
        }

        /// <summary>
        /// Determines whether a category can be moved or deleted by checking if it contains any products.
        /// Shows a message box if the action cannot be performed.
        /// </summary>
        /// <returns>
        /// true if the category can be moved or deleted (has no products);
        /// false if the category contains products, in which case displays an error message.
        /// </returns>
        private static bool CanCategoryBeMovedOrDeleted(string categoryName, List<Category> categoryList, string action)
        {
            if (MainMenu_Form.DoesCategoryHaveProducts(categoryName, categoryList))
            {
                CustomMessageBox.ShowWithFormat(
                    "Cannot {0} category",
                    "Cannot {0} category '{1}' because it contains products",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok,
                    action, action, categoryName);
                return false;
            }
            return true;
        }

        // Validate TextBoxes in other forms
        private static void HandleValueDeletion(string type, string value)
        {
            if (Tools.IsFormOpen<AddPurchase_Form>() &&
                Application.OpenForms[nameof(AddPurchase_Form)] is AddPurchase_Form purchaseForm)
            {
                switch (type)
                {
                    case "product":
                        ValidateFormTextBox(purchaseForm.ProductName_TextBox, value);
                        break;
                    case "company":
                        ValidateProductPathCompany(purchaseForm.ProductName_TextBox, value);
                        break;
                    case "category":
                        ValidateProductPathCategory(purchaseForm.ProductName_TextBox, value);
                        break;
                }
            }

            if (Tools.IsFormOpen<AddSale_Form>() &&
                Application.OpenForms[nameof(AddSale_Form)] is AddSale_Form saleForm)
            {
                switch (type)
                {
                    case "product":
                        ValidateFormTextBox(saleForm.ProductName_TextBox, value);
                        break;
                    case "company":
                        ValidateProductPathCompany(saleForm.ProductName_TextBox, value);
                        break;
                    case "category":
                        ValidateProductPathCategory(saleForm.ProductName_TextBox, value);
                        break;
                }
            }
        }
        private static void ValidateFormTextBox(Guna2TextBox textBox, string value)
        {
            if (textBox.Text.Contains(value))
            {
                textBox.Text = "";
            }
        }
        private static void ValidateProductPathCompany(Guna2TextBox textBox, string company)
        {
            if (textBox.Text.StartsWith($"{company} >"))
            {
                textBox.Text = textBox.Text;
            }
        }
        private static void ValidateProductPathCategory(Guna2TextBox textBox, string category)
        {
            if (textBox.Text.Contains($"> {category} >"))
            {
                textBox.Text = textBox.Text;
            }
        }

        private static Guna2Button _rightClickDataGridView_ModifyBtn, _rightClickDataGridView_MoveBtn,
            _rightClickDataGridView_ExportReceiptBtn, _rightClickDataGridView_ShowItemsBtn,
            _rightClickDataGridView_ReturnBtn, _rightClickDataGridView_UndoReturnBtn;

        // Right click row getters
        public static Guna2Panel RightClickDataGridView_Panel { get; private set; }
        public static Guna2Button RightClickDataGridView_DeleteBtn { get; private set; }

        // Right click row methods
        public static void ConstructRightClickRowMenu()
        {
            RightClickDataGridView_Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth, 5 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickDataGridView_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)RightClickDataGridView_Panel.Controls[0];

            _rightClickDataGridView_ModifyBtn = CustomControls.ConstructBtnForMenu("Modify", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_ModifyBtn.Click += ModifyRow;

            _rightClickDataGridView_MoveBtn = CustomControls.ConstructBtnForMenu("Move", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_MoveBtn.Click += MoveRows;

            _rightClickDataGridView_ExportReceiptBtn = CustomControls.ConstructBtnForMenu("Export receipt", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_ExportReceiptBtn.Click += ExportReceipt;

            _rightClickDataGridView_ShowItemsBtn = CustomControls.ConstructBtnForMenu("Show items", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_ShowItemsBtn.Click += ShowItems;

            _rightClickDataGridView_ReturnBtn = CustomControls.ConstructBtnForMenu("Return product", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_ReturnBtn.Click += ReturnProduct;

            _rightClickDataGridView_UndoReturnBtn = CustomControls.ConstructBtnForMenu("Undo return", CustomControls.PanelBtnWidth, true, flowPanel);
            _rightClickDataGridView_UndoReturnBtn.Click += UndoReturnProduct;

            RightClickDataGridView_DeleteBtn = CustomControls.ConstructBtnForMenu("Delete", CustomControls.PanelBtnWidth, true, flowPanel);
            RightClickDataGridView_DeleteBtn.ForeColor = CustomColors.AccentRed;
            RightClickDataGridView_DeleteBtn.Click += DeleteRow;
        }
        private static void ModifyRow(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            Tools.OpenForm(new ModifyRow_Form(grid.SelectedRows[0]));
        }
        private static void MoveRows(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            List<DataGridViewRow> selectedRows = grid.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (selectedRows.Count == 0) { return; }

            // Save scroll position
            int scrollPosition = grid.FirstDisplayedScrollingRowIndex;
            int firstSelectedIndex = selectedRows[0].Index;

            // Move rows based on current selection
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategoryPurchases)
            {
                MoveCategoryRows(selectedRows, true);
            }
            else if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales)
            {
                MoveCategoryRows(selectedRows, false);
            }

            // Restore selection and scroll position
            RestoreSelectionAndScroll(grid, firstSelectedIndex, scrollPosition);
        }
        private static void MoveCategoryRows(List<DataGridViewRow> rowsToMove, bool fromPurchaseToSale)
        {
            Guna2DataGridView sourceGrid = fromPurchaseToSale
                ? Categories_Form.Instance.Purchase_DataGridView
                : Categories_Form.Instance.Sale_DataGridView;

            Guna2DataGridView targetGrid = fromPurchaseToSale
                ? Categories_Form.Instance.Sale_DataGridView
                : Categories_Form.Instance.Purchase_DataGridView;

            List<Category> sourceList = fromPurchaseToSale
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            List<Category> targetList = fromPurchaseToSale
                ? MainMenu_Form.Instance.CategorySaleList
                : MainMenu_Form.Instance.CategoryPurchaseList;

            MainMenu_Form.IsProgramLoading = true;
            int successfulMoves = 0;

            foreach (DataGridViewRow row in rowsToMove)
            {
                string categoryName = row.Cells[0].Value.ToString();
                Category? category = MainMenu_Form.GetCategoryCategoryNameIsFrom(sourceList, categoryName);

                // Check if category is being used in transactions
                if (IsCategoryBeingUsedInTransactions(categoryName))
                {
                    ShowInUseMessage("category", _moveAction);
                    continue;
                }

                // Check if category has products
                if (category != null && category.ProductList.Count > 0)
                {

                    // Prepare products list for message box
                    string allProductsList = string.Join("\n", category.ProductList.Select(p => $"• {p.Name} ({p.CompanyOfOrigin})"));

                    // Ask user if they want to move products
                    CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                        "Move category with {0} products",
                        "The category '{1}' contains the following products:\n\n{2}\n\nDo you want to move all these products to the {3} category?",
                        CustomMessageBoxIcon.Question,
                        CustomMessageBoxButtons.OkCancel,
                        category.ProductList.Count, categoryName, allProductsList, fromPurchaseToSale ? "Sales" : "Purchases");

                    if (result != CustomMessageBoxResult.Ok)
                    {
                        continue;
                    }

                    // Move all products to the target list
                    List<Category> targetCategories = fromPurchaseToSale
                        ? MainMenu_Form.Instance.CategorySaleList
                        : MainMenu_Form.Instance.CategoryPurchaseList;

                    // Find or create a category in the target list
                    Category targetCategory = targetCategories.FirstOrDefault(c => c.Name == categoryName);
                    if (targetCategory == null)
                    {
                        targetCategory = new Category { Name = categoryName };
                        targetCategories.Add(targetCategory);
                    }

                    // Open Products_Form if it exists
                    Products_Form? productsForm = Tools.IsFormOpen<Products_Form>()
                        ? (Products_Form)Application.OpenForms[nameof(Products_Form)]
                        : null;

                    // Determine which grids to update based on current view
                    Guna2DataGridView sourceProductGrid = fromPurchaseToSale
                        ? productsForm?.Purchase_DataGridView
                        : productsForm?.Sale_DataGridView;

                    Guna2DataGridView targetProductGrid = fromPurchaseToSale
                        ? productsForm?.Sale_DataGridView
                        : productsForm?.Purchase_DataGridView;

                    // Move products
                    foreach (Product product in category.ProductList.ToList())
                    {
                        category.ProductList.Remove(product);
                        targetCategory.AddProduct(product);

                        // Update Products_Form if open
                        if (productsForm != null && sourceProductGrid != null && targetProductGrid != null)
                        {
                            // Find and remove the product row from source grid
                            DataGridViewRow? productRowToRemove = sourceProductGrid.Rows
                                .Cast<DataGridViewRow>()
                                .FirstOrDefault(r =>
                                    r.Cells[Products_Form.Column.ProductName.ToString()].Value.ToString() == product.Name &&
                                    r.Cells[Products_Form.Column.CompanyOfOrigin.ToString()].Value.ToString() == product.CompanyOfOrigin
                                );

                            if (productRowToRemove != null)
                            {
                                sourceProductGrid.Rows.Remove(productRowToRemove);
                            }

                            // Add product to the target grid
                            targetProductGrid.Rows.Add(
                                product.ProductID,
                                product.Name,
                                targetCategory.Name,
                                product.CountryOfOrigin,
                                product.CompanyOfOrigin
                            );
                        }
                    }
                }

                // Move the category row
                sourceGrid.Rows.Remove(row);
                targetGrid.Rows.Add(row);

                // Update category lists
                sourceList.Remove(category);
                targetList.Add(category);

                successfulMoves++;
            }

            if (successfulMoves > 0)
            {
                string direction = fromPurchaseToSale ? "Sales" : "Purchases";
                string message = successfulMoves == 1
                    ? $"Moved {successfulMoves} category to {direction}"
                    : $"Moved {successfulMoves} categories to {direction}";
                CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, message);
            }

            // Update UI
            LabelManager.ShowTotalLabel(Categories_Form.Instance.Total_Label, sourceGrid);
            SortDataGridViewByCurrentDirection(targetGrid);

            // Save changes to file
            MainMenu_Form.Instance.SaveCategoriesToFile(
                fromPurchaseToSale ? MainMenu_Form.SelectedOption.CategoryPurchases : MainMenu_Form.SelectedOption.CategorySales
            );
            MainMenu_Form.Instance.SaveCategoriesToFile(
                fromPurchaseToSale ? MainMenu_Form.SelectedOption.CategorySales : MainMenu_Form.SelectedOption.CategoryPurchases
            );

            MainMenu_Form.IsProgramLoading = false;
        }
        private static bool IsCategoryBeingUsedInTransactions(string categoryName)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.GetAllRows())
            {
                if (row.Cells[ReadOnlyVariables.Category_column].Value?.ToString() == categoryName)
                {
                    return true;
                }

                // Check if transaction has multiple items
                if (row.Tag is not (List<string> items, TagData))
                    continue;

                // Do not check receipt if present
                int itemsToCheck = items[^1].StartsWith(ReadOnlyVariables.Receipt_text)
                    ? items.Count - 1
                    : items.Count;

                // Check each item's category
                for (int i = 0; i < itemsToCheck; i++)
                {
                    string[] itemDetails = items[i].Split(',');
                    if (itemDetails.Length > 1 && itemDetails[1] == categoryName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void RestoreSelectionAndScroll(DataGridView gridView, int previousIndex, int scrollPosition)
        {
            gridView.ClearSelection();

            // Select the row at the previous index or the last row if the previous index is now out of bounds
            int newRowIndex = previousIndex < gridView.Rows.Count - 1 ? previousIndex : gridView.Rows.Count - 1;
            if (newRowIndex >= 0 && newRowIndex < gridView.Rows.Count)
            {
                gridView.Rows[newRowIndex].Selected = true;
            }

            // Restore scroll position
            if (scrollPosition != 0 && scrollPosition < gridView.Rows.Count)
            {
                gridView.FirstDisplayedScrollingRowIndex = scrollPosition;
            }
        }
        private static void ExportReceipt(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            ReceiptManager.ExportSelectedReceipts(grid);
        }
        private static void ShowItems(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            Tools.OpenForm(new ItemsInTransaction_Form(grid.SelectedRows[0]));
        }
        private static void ReturnProduct(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            if (grid.SelectedRows.Count != 1) { return; }

            DataGridViewRow selectedRow;
            bool isPurchase;

            // Check if we're in the items view of a transaction
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInPurchase ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInSale)
            {
                // Use the main transaction row, not the individual item row
                selectedRow = SelectedRowInMainMenu;
                isPurchase = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInPurchase;
            }
            else
            {
                // Normal case - we're in the main purchases/sales view
                selectedRow = grid.SelectedRows[0];
                isPurchase = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases;
            }

            using ReturnProduct_Form returnForm = new(selectedRow, isPurchase);
            if (returnForm.ShowDialog() == DialogResult.OK)
            {
                CustomControls.CloseAllPanels();

                // Refresh ItemsInTransaction_Form if it's open
                if (Tools.IsFormOpen<ItemsInTransaction_Form>() &&
                    Application.OpenForms[nameof(ItemsInTransaction_Form)] is ItemsInTransaction_Form itemsForm)
                {
                    itemsForm.RefreshItemReturnStatus();
                }
            }
        }
        private static void UndoReturnProduct(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            if (grid.SelectedRows.Count != 1) { return; }

            DataGridViewRow selectedRow;

            // Check if we're in the items view of a transaction
            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInPurchase ||
                MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInSale)
            {
                // Use the main transaction row, not the individual item row
                selectedRow = SelectedRowInMainMenu;
            }
            else
            {
                // Normal case - we're in the main purchases/sales view
                selectedRow = grid.SelectedRows[0];
            }

            using UndoReturn_Form undoForm = new(selectedRow);
            if (undoForm.ShowDialog() == DialogResult.OK)
            {
                CustomControls.CloseAllPanels();

                // Refresh ItemsInTransaction_Form if it's open
                if (Tools.IsFormOpen<ItemsInTransaction_Form>() &&
                    Application.OpenForms[nameof(ItemsInTransaction_Form)] is ItemsInTransaction_Form itemsForm)
                {
                    itemsForm.RefreshItemReturnStatus();
                }
            }
        }
        private static void DeleteRow(object sender, EventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)RightClickDataGridView_Panel.Tag;
            int index = grid.SelectedRows[^1].Index;

            // Delete all selected rows
            foreach (DataGridViewRow item in grid.SelectedRows)
            {
                DataGridViewRowCancelEventArgs eventArgs = new(item);
                DataGridView_UserDeletingRow(grid, eventArgs);
                if (item.Index == -1) { continue; }  // This can happen if ItemsInTransaction_Form is closed in DataGridView_UserDeletingRow()

                if (!eventArgs.Cancel)
                {
                    grid.Rows.Remove(item);
                }
            }

            // Select the row under the row that was just deleted
            if (grid.Rows.Count != 0)
            {
                // If the deleted row was not the last one, select the next row
                if (index < grid.Rows.Count)
                {
                    grid.Rows[index].Selected = true;
                }
                else  // If the deleted row was the last one, select the new last row
                {
                    grid.Rows[^1].Selected = true;
                }
            }
        }

        // Methods for right click row
        public static string GetFilePathFromRowTag(object tag)
        {
            if (tag is (List<string> tagList, TagData) && tagList[^1].Contains('\\'))
            {
                return ProcessDirectoryFromString(tagList[^1]);
            }
            else if (tag is (string tagString, TagData))
            {
                return ProcessDirectoryFromString(tagString);
            }
            else if (tag is string tagString2)
            {
                return ProcessDirectoryFromString(tagString2);
            }
            return "";
        }
        private static string ProcessDirectoryFromString(string path)
        {
            string[] pathParts = path.Split(Path.DirectorySeparatorChar);
            if (pathParts[7] == ReadOnlyVariables.CompanyName_text)
            {
                pathParts[7] = Directories.CompanyName;
            }
            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts);

            newPath = newPath.Replace(ReadOnlyVariables.Receipt_text, "");

            return File.Exists(newPath) ? newPath : "";
        }
    }
}