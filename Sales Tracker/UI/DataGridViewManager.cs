using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using System.ComponentModel;

namespace Sales_Tracker.UI
{
    public class DataGridViewManager
    {
        // Properties
        private static DataGridViewRow removedRow;
        private static Control _controlRightClickPanelWasAddedTo;
        private static bool _doNotDeleteRows;
        private static DataGridViewRow _selectedRowInMainMenu;
        private static readonly byte rowHeight = 35, columnHeaderHeight = 60;

        // Getters
        public static Control ControlRightClickPanelWasAddedTo
        {
            get => _controlRightClickPanelWasAddedTo;
            set => _controlRightClickPanelWasAddedTo = value;
        }
        public static bool DoNotDeleteRows
        {
            get => _doNotDeleteRows;
            set => _doNotDeleteRows = value;
        }
        public static DataGridViewRow SelectedRowInMainMenu
        {
            get => _selectedRowInMainMenu;
            set => _selectedRowInMainMenu = value;
        }

        // Construct DataGridView
        public static void InitializeDataGridView<TEnum>(Guna2DataGridView dataGridView, string name, Size size, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad = null) where TEnum : Enum
        {
            dataGridView.Name = name;
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView.ColumnHeadersHeight = columnHeaderHeight;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.RowTemplate.Height = rowHeight;
            dataGridView.RowTemplate.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = CustomColors.text;
            dataGridView.Theme = CustomColors.dataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.controlBack;
            dataGridView.Size = size;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView.ScrollBars = ScrollBars.Vertical;

            dataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
            dataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView.MouseDown += DataGridView_MouseDown;
            dataGridView.MouseUp += DataGridView_MouseUp;
            dataGridView.KeyDown += DataGridView_KeyDown;
            dataGridView.CellMouseClick += DataGridView_CellMouseClick;
            dataGridView.CellMouseMove += DataGridView_CellMouseMove;
            dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;

            LoadColumns(dataGridView, columnHeaders, columnsToLoad);
            Theme.UpdateDataGridViewHeaderTheme(dataGridView);
        }
        public static void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            if (_doNotDeleteRows)
            {
                e.Cancel = true;
                return;
            }

            removedRow = e.Row;

            string type = "", columnName = "", valueBeingRemoved;
            byte logIndex = 0;

            switch (MainMenu_Form.Instance.Selected)
            {
                case MainMenu_Form.SelectedOption.Purchases:
                    type = "purchase";
                    columnName = MainMenu_Form.Column.Product.ToString();
                    logIndex = 2;
                    break;

                case MainMenu_Form.SelectedOption.Sales:
                    type = "sale";
                    columnName = MainMenu_Form.Column.Product.ToString();
                    logIndex = 2;
                    break;

                case MainMenu_Form.SelectedOption.ProductPurchases:
                    type = "product for purchase";
                    columnName = Products_Form.Column.ProductName.ToString();
                    logIndex = 3;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, MainMenu_Form.Column.Product.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove product from list
                    MainMenu_Form.Instance.CategoryPurchaseList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == valueBeingRemoved)));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case MainMenu_Form.SelectedOption.ProductSales:
                    type = "product for sale";
                    columnName = Products_Form.Column.ProductName.ToString();
                    logIndex = 3;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, MainMenu_Form.Column.Product.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove product from list
                    MainMenu_Form.Instance.CategorySaleList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == valueBeingRemoved)));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case MainMenu_Form.SelectedOption.CategoryPurchases:
                    type = "category for purchase";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, MainMenu_Form.Column.Category.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove category from list
                    MainMenu_Form.Instance.CategoryPurchaseList.Remove(MainMenu_Form.Instance.CategoryPurchaseList.FirstOrDefault(c => c.Name == valueBeingRemoved));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case MainMenu_Form.SelectedOption.CategorySales:
                    type = "category for sale";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, MainMenu_Form.Column.Category.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove category from list
                    MainMenu_Form.Instance.CategorySaleList.Remove(MainMenu_Form.Instance.CategorySaleList.FirstOrDefault(c => c.Name == valueBeingRemoved));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case MainMenu_Form.SelectedOption.Accountants:
                    type = "accountant";
                    columnName = Accountants_Form.Columns.AccountantName.ToString();
                    logIndex = 2;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, Accountants_Form.Columns.AccountantName.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove accountant from list
                    MainMenu_Form.Instance.AccountantList.Remove(MainMenu_Form.Instance.AccountantList.FirstOrDefault(a => a == valueBeingRemoved));

                    // In case the accountant name that is being deleted is in the TextBox
                    Accountants_Form.Instance.VaidateAccountantTextBox();
                    break;

                case MainMenu_Form.SelectedOption.Companies:
                    type = "company";
                    columnName = Companies_Form.Columns.Company.ToString();
                    logIndex = 2;
                    valueBeingRemoved = e.Row.Cells[columnName].Value?.ToString();

                    if (IsThisBeingUsed(type, MainMenu_Form.Column.Company.ToString(), valueBeingRemoved))
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Remove company from list
                    MainMenu_Form.Instance.CompanyList.Remove(MainMenu_Form.Instance.CompanyList.FirstOrDefault(a => a == valueBeingRemoved));

                    // In case the company name that is being deleted is in the TextBox
                    Companies_Form.Instance.ValidateCompanyTextBox();
                    break;

                case MainMenu_Form.SelectedOption.ItemsInPurchase:
                case MainMenu_Form.SelectedOption.ItemsInSale:
                    columnName = MainMenu_Form.Column.Product.ToString();
                    string productName = e.Row.Cells[columnName].Value?.ToString();

                    if (_selectedRowInMainMenu.Tag is (List<string> itemList, TagData))
                    {
                        byte index = 1;
                        if (itemList.Last().StartsWith(ReadOnlyVariables.CompanyName_text))
                        {
                            index = 2;
                        }

                        string selected;
                        if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.ItemsInPurchase)
                        {
                            selected = "purchase";
                        }
                        else
                        {
                            selected = "sale";
                        }

                        if (itemList.Count == index)
                        {
                            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                                $"Deleting the last item will also delete the {selected}.",
                                CustomMessageBoxIcon.Info, CustomMessageBoxButtons.OkCancel);

                            if (result != CustomMessageBoxResult.Ok)
                            {
                                e.Cancel = true;
                                return;
                            }
                            itemsInPurchase_Form.Close();
                            Log.Write(2, $"Deleted item '{productName}' in {selected}");
                            return;
                        }

                        // Remove the row from the tag
                        itemList.RemoveAt(e.Row.Index);

                        Log.Write(2, $"Deleted item '{productName}' in {selected}");
                        return;
                    }
                    break;
            }
            string name = e.Row.Cells[columnName].Value?.ToString();

            Log.Write(logIndex, $"Deleted {type} '{name}'");
        }
        public static void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            MainMenu_Form.Instance.AlignTotalLabels();
        }
        public static void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            DataGridViewRowChanged(MainMenu_Form.Instance.SelectedDataGridView, MainMenu_Form.Instance.Selected);

            // Remove receipt from file
            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales && removedRow?.Tag != null)
            {
                string tagValue = "";

                if (removedRow.Tag is (List<string> tagList, TagData))
                {
                    tagValue = tagList[^1].Replace(ReadOnlyVariables.CompanyName_text, "").Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName);
                }
                else if (removedRow.Tag is (string tagString, TagData))
                {
                    tagValue = tagString.Replace(ReadOnlyVariables.CompanyName_text, "").Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName);
                }

                if (tagValue != "")
                {
                    Directories.DeleteFile(tagValue);
                }

                removedRow = null;
            }
        }
        public static void DataGridViewRowChanged(Guna2DataGridView dataGridView, MainMenu_Form.SelectedOption selected)
        {
            if (selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.UpdateTotals();
                MainMenu_Form.Instance.LoadCharts();
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
            CustomControls.CloseAllPanels(null, null);

            Guna2DataGridView grid = (Guna2DataGridView)sender;
            DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);

            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales && info.RowIndex != -1)
            {
                _selectedRowInMainMenu = grid.Rows[info.RowIndex];
            }

            if (e.Button == MouseButtons.Right && grid.Rows.Count > 0)
            {
                // The right click button does not select rows by default, so implement it here
                // If it is not currently selected, unselect others
                if (info.RowIndex == -1)
                {
                    return;
                }
                UnselectAllRowsInCurrentDataGridView();

                // Select current row
                grid.Rows[info.RowIndex].Selected = true;
                grid.Focus();
            }
        }
        private static void DataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            Guna2DataGridView grid = (Guna2DataGridView)sender;
            if (grid.SelectedRows.Count == 0) { return; }

            if (e.Button == MouseButtons.Right && grid.Rows.Count > 0)
            {
                DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);

                // If a row was not clicked
                if (info.Type != DataGridViewHitTestType.Cell)
                {
                    return;
                }

                FlowLayoutPanel flowPanel = _rightClickDataGridView_Panel.Controls
                    .OfType<FlowLayoutPanel>()
                    .FirstOrDefault();

                // Add move button
                if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategoryPurchases)
                {
                    ConfigureMoveButton("Move category to sales");
                }
                else if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales)
                {
                    ConfigureMoveButton("Move category to purchases");
                }
                else
                {
                    flowPanel.Controls.Remove(rightClickDataGridView_MoveBtn);
                }

                flowPanel.Controls.Remove(rightClickDataGridView_ModifyBtn);
                flowPanel.Controls.Remove(rightClickDataGridView_ShowItemsBtn);
                flowPanel.Controls.Remove(rightClickDataGridView_ExportReceiptBtn);

                if (MainMenu_Form.Instance.Selected is not MainMenu_Form.SelectedOption.ItemsInPurchase or MainMenu_Form.SelectedOption.ItemsInSale)
                {
                    AddButtonToFlowPanel(flowPanel, rightClickDataGridView_ModifyBtn, 1);
                }

                if (grid.SelectedRows[0].Tag is (List<string> tagList, TagData))
                {
                    AddButtonToFlowPanel(flowPanel, rightClickDataGridView_ShowItemsBtn, 2);

                    if (IsLastItemAReceipt(tagList[^1]))
                    {
                        AddButtonToFlowPanel(flowPanel, rightClickDataGridView_ExportReceiptBtn, 3);
                    }
                }
                else if (grid.SelectedRows[0].Tag is (string item, TagData))
                {
                    if (IsLastItemAReceipt(item))
                    {
                        AddButtonToFlowPanel(flowPanel, rightClickDataGridView_ExportReceiptBtn, 2);
                    }
                }

                flowPanel.Controls.SetChildIndex(_rightClickDataGridView_DeleteBtn, 4);

                // Adjust the panel height based on the number of controls
                int controlCount = flowPanel.Controls.Count;
                _rightClickDataGridView_Panel.Height = controlCount * CustomControls.PanelButtonHeight + 10;
                flowPanel.Height = controlCount * CustomControls.PanelButtonHeight;

                Control controlSender = (Control)sender;
                _controlRightClickPanelWasAddedTo = controlSender.Parent;
                Form parentForm = grid.FindForm();
                int formHeight = parentForm.ClientSize.Height;
                int formWidth = parentForm.ClientSize.Width;
                byte padding = 5;

                // Calculate the horizontal position
                bool tooFarRight = false;
                if (MainMenu_Form.Instance.SelectedDataGridView.Left + _rightClickDataGridView_Panel.Width + e.X - ReadOnlyVariables.OffsetRightClickPanel + padding > formWidth)
                {
                    _rightClickDataGridView_Panel.Left = formWidth - _rightClickDataGridView_Panel.Width - padding;
                    tooFarRight = true;
                }
                else
                {
                    _rightClickDataGridView_Panel.Left = MainMenu_Form.Instance.SelectedDataGridView.Left + e.X - ReadOnlyVariables.OffsetRightClickPanel;
                }

                // Calculate the vertical position
                int verticalOffset = grid.FirstDisplayedScrollingRowIndex * grid.Rows[0].Height;
                int rowTop = (info.RowIndex + 1) * grid.Rows[0].Height - verticalOffset + MainMenu_Form.Instance.SelectedDataGridView.Top + columnHeaderHeight;

                if (rowTop + _rightClickDataGridView_Panel.Height > formHeight + padding)
                {
                    _rightClickDataGridView_Panel.Top = formHeight - _rightClickDataGridView_Panel.Height - padding;
                    if (!tooFarRight)
                    {
                        _rightClickDataGridView_Panel.Left += ReadOnlyVariables.OffsetRightClickPanel;
                    }
                }
                else
                {
                    _rightClickDataGridView_Panel.Top = rowTop;
                }

                _controlRightClickPanelWasAddedTo.Controls.Add(_rightClickDataGridView_Panel);
                _rightClickDataGridView_Panel.BringToFront();
            }
        }
        private static void DataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                MainMenu_Form.Instance.ClosePanels();

                string message;
                if (MainMenu_Form.Instance.SelectedDataGridView.SelectedRows.Count == 1)
                {
                    message = "Are you sure you want to delete this row?";
                }
                else
                {
                    message = "Are you sure you want to delete the selected rows?";
                }
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    _doNotDeleteRows = true;
                    UnselectAllRowsInCurrentDataGridView();
                }
            }
        }
        private static void DataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (IsLastCellClicked(e, dataGridView))
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (IsClickOnText(cell, out Rectangle hitbox))
                {
                    Point mousePos = dataGridView.PointToClient(Cursor.Position);
                    if (hitbox.Contains(mousePos))
                    {
                        CustomMessageBox.Show("Note for purchase", cell.Tag.ToString(), CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    }
                }
            }
        }
        private static void DataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (IsLastCellClicked(e, dataGridView))
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
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
        }
        private static void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns.Count - 1)
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null && cell.Value.ToString() == ReadOnlyVariables.Show_text)
                {
                    ResetCellStyle(cell);
                }
            }
        }
        private static bool IsLastCellClicked(DataGridViewCellMouseEventArgs e, DataGridView dataGridView)
        {
            return e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns.Count - 1;
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
                cell.Style.ForeColor = CustomColors.accent_blue;
                cell.Style.SelectionForeColor = CustomColors.accent_blue;
            }
            else
            {
                ResetCellStyle(cell);
            }
        }
        private static void ResetCellStyle(DataGridViewCell cell)
        {
            cell.Style.ForeColor = CustomColors.text;
            cell.Style.SelectionForeColor = CustomColors.text;
        }

        // Methods for DataGridView
        /// <summary>
        /// Shows a MessageBox if the row is being used by another row.
        /// </summary>
        /// <returns>True if it's being used by another row.</returns>
        private static bool IsThisBeingUsed(string type, string columnName, string valueBeingRemoved)
        {
            foreach (DataGridViewRow row in GetAllRowsInMainMenu())
            {
                if (row.Cells[columnName].Value.ToString() == valueBeingRemoved)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", $"This {type} is being used and cannot be deleted", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    return true;
                }
            }
            return false;
        }
        private static bool IsLastItemAReceipt(string lastItem)
        {
            // Check if the last item starts with "receipt:"
            if (lastItem.StartsWith(ReadOnlyVariables.Receipt_text))
            {
                lastItem = lastItem.Substring(8).Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName);

                return File.Exists(lastItem);
            }
            return false;
        }
        public static void DataGridViewRowsAdded(Guna2DataGridView dataGridView, DataGridViewRowsAddedEventArgs e)
        {
            if (MainMenu_Form.IsProgramLoading) { return; }

            DataGridViewRowChanged(MainMenu_Form.Instance.SelectedDataGridView, MainMenu_Form.Instance.Selected);
            DataGridViewRow row;

            if (e.RowIndex >= 0 && e.RowIndex < dataGridView.Rows.Count)
            {
                row = dataGridView.Rows[e.RowIndex];
            }
            else
            {
                Log.Error_RowIsOutOfRange();
                return;
            }

            SortDataGridViewByCurrentDirection(dataGridView);

            if (MainMenu_Form.Instance.Selected is MainMenu_Form.SelectedOption.Purchases or MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.SortDataGridView();
            }

            // Calculate the middle index
            int visibleRowCount = dataGridView.DisplayedRowCount(true);
            int middleIndex = Math.Max(0, row.Index - (visibleRowCount / 2) + 1);

            // Ensure the row at middleIndex is visible
            if (middleIndex >= 0 && middleIndex < dataGridView.RowCount && dataGridView.Rows[middleIndex].Visible)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = middleIndex;
            }

            // Select the added row
            UnselectAllRowsInCurrentDataGridView();
            dataGridView.Rows[row.Index].Selected = true;
        }
        public static void LoadColumns<TEnum>(Guna2DataGridView dataGridView, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad = null) where TEnum : Enum
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
                    Name = columnHeader.Key.ToString()  // Set the Name property for the language translation
                };
                dataGridView.Columns.Add(column);
            }
        }
        private static void ConfigureMoveButton(string buttonText)
        {
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickDataGridView_Panel.Controls[0];

            // Ensure the Move button is the second control
            flowPanel.Controls.Add(rightClickDataGridView_MoveBtn);
            flowPanel.Controls.SetChildIndex(rightClickDataGridView_MoveBtn, 1);

            rightClickDataGridView_MoveBtn.Text = buttonText;
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
        private static void UnselectAllRowsInCurrentDataGridView()
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.SelectedDataGridView.Rows)
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
            }
        }
        public static void UpdateRowWithMultipleItems(DataGridViewRow selectedRow)
        {
            List<string> items = selectedRow.Tag is (List<string> itemList, TagData) ? itemList : [];

            if (items.Count <= 1) { return; }

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;
            decimal pricePerUnit = 0;

            foreach (string item in items)
            {
                string[] itemDetails = item.Split(',');

                if (itemDetails.Length < 7) { continue; }

                string currentCategoryName = itemDetails[1];
                string currentCountry = itemDetails[2];
                string currentCompany = itemDetails[3];
                pricePerUnit += decimal.Parse(itemDetails[5]);

                if (firstCategoryName == null) { firstCategoryName = currentCategoryName; }
                else if (isCategoryNameConsistent && firstCategoryName != currentCategoryName) { isCategoryNameConsistent = false; }

                if (firstCountry == null) { firstCountry = currentCountry; }
                else if (isCountryConsistent && firstCountry != currentCountry) { isCountryConsistent = false; }

                if (firstCompany == null) { firstCompany = currentCompany; }
                else if (isCompanyConsistent && firstCompany != currentCompany) { isCompanyConsistent = false; }
            }

            string categoryName = isCategoryNameConsistent ? firstCategoryName : ReadOnlyVariables.EmptyCell;
            string country = isCountryConsistent ? firstCountry : ReadOnlyVariables.EmptyCell;
            string company = isCompanyConsistent ? firstCompany : ReadOnlyVariables.EmptyCell;

            selectedRow.Cells[MainMenu_Form.Column.Category.ToString()].Value = categoryName;
            selectedRow.Cells[MainMenu_Form.Column.Country.ToString()].Value = country;
            selectedRow.Cells[MainMenu_Form.Column.Company.ToString()].Value = company;
            selectedRow.Cells[MainMenu_Form.Column.Quantity.ToString()].Value = items.Count - 1;

            // Update charged difference
            int quantity = int.Parse(selectedRow.Cells[MainMenu_Form.Column.Quantity.ToString()].Value.ToString());
            decimal shipping = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Shipping.ToString()].Value.ToString());
            decimal tax = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Tax.ToString()].Value.ToString());
            decimal totalPrice = quantity * pricePerUnit + shipping + tax;
            selectedRow.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = Convert.ToDecimal(selectedRow.Cells[MainMenu_Form.Column.Total.ToString()].Value) - totalPrice;

            selectedRow.Cells[MainMenu_Form.Column.Total.ToString()].Value = totalPrice;
        }
        public static void UpdateRowWithNoItems(DataGridViewRow selectedRow)
        {
            int quantity = int.Parse(selectedRow.Cells[MainMenu_Form.Column.Quantity.ToString()].Value.ToString());
            decimal pricePerUnit = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.PricePerUnit.ToString()].Value.ToString());
            decimal shipping = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Shipping.ToString()].Value.ToString());
            decimal tax = decimal.Parse(selectedRow.Cells[MainMenu_Form.Column.Tax.ToString()].Value.ToString());
            decimal totalPrice = quantity * pricePerUnit + shipping + tax;
            selectedRow.Cells[MainMenu_Form.Column.ChargedDifference.ToString()].Value = Convert.ToDecimal(selectedRow.Cells[MainMenu_Form.Column.Total.ToString()].Value) - totalPrice;
        }
        public static void UpdateAllRows(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                UpdateRowWithMultipleItems(row);
            }
        }
        public static void AddNoteToCell(int newRowIndex, string note)
        {
            DataGridViewCell cell = MainMenu_Form.Instance.SelectedDataGridView.Rows[newRowIndex].Cells[^1];
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
        public static List<DataGridViewRow> GetAllRowsInMainMenu()
        {
            List<DataGridViewRow> allRows = [];
            allRows.AddRange(MainMenu_Form.Instance.Purchase_DataGridView.Rows.Cast<DataGridViewRow>());
            allRows.AddRange(MainMenu_Form.Instance.Sale_DataGridView.Rows.Cast<DataGridViewRow>());
            return allRows;
        }
        private static void SortDataGridViewByCurrentDirection(DataGridView dataGridView)
        {
            if (dataGridView.SortedColumn == null)
            {
                return;
            }

            SortOrder sortOrder = dataGridView.SortOrder;
            DataGridViewColumn sortedColumn = dataGridView.SortedColumn;
            ListSortDirection direction = (sortOrder == SortOrder.Ascending) ?
                                          ListSortDirection.Ascending :
                                          ListSortDirection.Descending;
            dataGridView.Sort(sortedColumn, direction);
        }

        // Right click row properties
        private static Guna2Panel _rightClickDataGridView_Panel;
        private static Guna2Button rightClickDataGridView_ModifyBtn, rightClickDataGridView_MoveBtn, rightClickDataGridView_ExportReceiptBtn, rightClickDataGridView_ShowItemsBtn;
        private static Guna2Button _rightClickDataGridView_DeleteBtn;

        // Right click row getters
        public static Guna2Panel RightClickDataGridView_Panel => _rightClickDataGridView_Panel;
        public static Guna2Button RightClickDataGridView_DeleteBtn => _rightClickDataGridView_DeleteBtn;

        // Right click row methods
        public static void ConstructRightClickRowMenu()
        {
            _rightClickDataGridView_Panel = CustomControls.ConstructPanelForMenu(new Size(CustomControls.PanelWidth, 5 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel), "rightClickDataGridView_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickDataGridView_Panel.Controls[0];

            rightClickDataGridView_ModifyBtn = CustomControls.ConstructBtnForMenu("Modify", CustomControls.PanelBtnWidth, false, flowPanel);
            rightClickDataGridView_ModifyBtn.Click += ModifyRow;

            rightClickDataGridView_MoveBtn = CustomControls.ConstructBtnForMenu("Move", CustomControls.PanelBtnWidth, false, flowPanel);
            rightClickDataGridView_MoveBtn.Click += MoveRow;

            rightClickDataGridView_ExportReceiptBtn = CustomControls.ConstructBtnForMenu("Export receipt", CustomControls.PanelBtnWidth, false, flowPanel);
            rightClickDataGridView_ExportReceiptBtn.Click += ExportReceipt;

            rightClickDataGridView_ShowItemsBtn = CustomControls.ConstructBtnForMenu("Show items", CustomControls.PanelBtnWidth, false, flowPanel);
            rightClickDataGridView_ShowItemsBtn.Click += ShowItems;

            _rightClickDataGridView_DeleteBtn = CustomControls.ConstructBtnForMenu("Delete", CustomControls.PanelBtnWidth, false, flowPanel);
            _rightClickDataGridView_DeleteBtn.ForeColor = CustomColors.accent_red;
            _rightClickDataGridView_DeleteBtn.Click += DeleteRow;

            CustomControls.ConstructKeyShortcut("Del", _rightClickDataGridView_DeleteBtn);
        }
        private static void ModifyRow(object sender, EventArgs e)
        {
            MainMenu_Form.Instance.ClosePanels();
            if (MainMenu_Form.Instance.SelectedDataGridView.SelectedRows.Count > 1)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "You can only select one row to modify.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            ModifyRow_Form modifyRow_Form = new(MainMenu_Form.Instance.SelectedDataGridView.SelectedRows[0]);
            modifyRow_Form.ShowDialog();
        }
        private static void MoveRow(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);


            DataGridView selectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;
            DataGridViewRow selectedRow = selectedDataGridView.SelectedRows[0];
            int selectedIndex = selectedRow.Index;

            // Save the current scroll position
            int scrollPosition = selectedDataGridView.FirstDisplayedScrollingRowIndex;

            if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategoryPurchases)
            {
                MainMenu_Form.IsProgramLoading = true;
                Categories_Form.Instance.Purchase_DataGridView.Rows.Remove(selectedRow);
                Categories_Form.Instance.Sale_DataGridView.Rows.Add(selectedRow);
                MainMenu_Form.IsProgramLoading = false;

                Category category = MainMenu_Form.GetCategoryCategoryNameIsFrom(
                    MainMenu_Form.Instance.CategoryPurchaseList,
                    selectedRow.Cells[0].Value.ToString());

                MainMenu_Form.Instance.CategoryPurchaseList.Remove(category);
                MainMenu_Form.Instance.CategorySaleList.Add(category);

                LabelManager.ShowTotalLabel(Categories_Form.Instance.Total_Label, Categories_Form.Instance.Purchase_DataGridView);
                SortDataGridViewByCurrentDirection(Categories_Form.Instance.Sale_DataGridView);
            }
            else if (MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.CategorySales)
            {
                MainMenu_Form.IsProgramLoading = true;
                Categories_Form.Instance.Sale_DataGridView.Rows.Remove(selectedRow);
                Categories_Form.Instance.Purchase_DataGridView.Rows.Add(selectedRow);
                MainMenu_Form.IsProgramLoading = false;

                Category category = MainMenu_Form.GetCategoryCategoryNameIsFrom(
                    MainMenu_Form.Instance.CategorySaleList,
                    selectedRow.Cells[0].Value.ToString());

                MainMenu_Form.Instance.CategorySaleList.Remove(category);
                MainMenu_Form.Instance.CategoryPurchaseList.Add(category);

                LabelManager.ShowTotalLabel(Categories_Form.Instance.Total_Label, Categories_Form.Instance.Sale_DataGridView);
                SortDataGridViewByCurrentDirection(Categories_Form.Instance.Purchase_DataGridView);
            }

            // Select a new row
            int newRowIndex = selectedIndex < selectedDataGridView.Rows.Count - 1 ? selectedIndex : selectedIndex - 1;

            if (newRowIndex >= 0 && newRowIndex < selectedDataGridView.Rows.Count)
            {
                selectedDataGridView.ClearSelection();
                selectedDataGridView.Rows[newRowIndex].Selected = true;
                selectedDataGridView.CurrentCell = selectedDataGridView.Rows[newRowIndex].Cells[0]; // Optionally focus on the first cell
            }

            // Restore the scroll position
            if (scrollPosition != 0)
            {
                selectedDataGridView.FirstDisplayedScrollingRowIndex = scrollPosition;
            }

            MainMenu_Form.IsProgramLoading = false;
        }
        private static void ExportReceipt(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);

            DataGridViewRow selectedRow = MainMenu_Form.Instance.SelectedDataGridView.SelectedRows[0];
            string receiptFilePath = GetFilePathFromRowTag(selectedRow.Tag);

            if (!File.Exists(receiptFilePath))
            {
                CustomMessageBox.Show("Argo Sales Tracker", "The receipt no longer exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                Log.Error_FileDoesNotExist(receiptFilePath);
                return;
            }

            // Select directory
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string newFilepath = dialog.SelectedPath + @"\" + Path.GetFileName(receiptFilePath);
                newFilepath = Directories.GetNewFileNameIfItAlreadyExists(newFilepath);
                Directories.CopyFile(receiptFilePath, newFilepath);
            }
        }
        private static ItemsInTransaction_Form itemsInPurchase_Form;
        private static void ShowItems(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
            itemsInPurchase_Form = new ItemsInTransaction_Form(MainMenu_Form.Instance.SelectedDataGridView.SelectedRows[0]);
            itemsInPurchase_Form.ShowDialog();
        }
        private static void DeleteRow(object sender, EventArgs e)
        {
            MainMenu_Form.Instance.ClosePanels();

            int index = MainMenu_Form.Instance.SelectedDataGridView.SelectedRows[^1].Index;

            // Delete all selected rows
            foreach (DataGridViewRow item in MainMenu_Form.Instance.SelectedDataGridView.SelectedRows)
            {
                DataGridViewRowCancelEventArgs eventArgs = new(item);
                DataGridView_UserDeletingRow(MainMenu_Form.Instance.SelectedDataGridView, eventArgs);

                if (!eventArgs.Cancel)
                {
                    MainMenu_Form.Instance.SelectedDataGridView.Rows.Remove(item);
                }
            }

            // Select the row under the row that was just deleted
            if (MainMenu_Form.Instance.SelectedDataGridView.Rows.Count != 0)
            {
                // If the deleted row was not the last one, select the next row
                if (index < MainMenu_Form.Instance.SelectedDataGridView.Rows.Count)
                {
                    MainMenu_Form.Instance.SelectedDataGridView.Rows[index].Selected = true;
                }
                else  // If the deleted row was the last one, select the new last row
                {
                    MainMenu_Form.Instance.SelectedDataGridView.Rows[^1].Selected = true;
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
        private static void AddButtonToFlowPanel(FlowLayoutPanel flowPanel, Guna2Button button, int index)
        {
            flowPanel.Controls.Add(button);
            flowPanel.Controls.SetChildIndex(button, index);
        }
    }
}