using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Graphs;
using Sales_Tracker.Properties;
using static Sales_Tracker.Classes.Theme;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init
        public static MainMenu_Form? Instance { get; private set; }
        public MainMenu_Form()
        {
            InitializeComponent();
            Instance = this;

            UI.ConstructControls();
            SearchBox.ConstructSearchBox();

            ConstructDataGridViews();
            SetCompanyLabel();
            isDataGridViewLoading = true;
            LoadProducts();
            LoadSales();
            LoadPurchases();
            isDataGridViewLoading = false;
            Sales_Button.PerformClick();
            LoadDataFromSetting();
            AlignTotalLabels();
            AddTimeRangesIntoComboBox();
            UpdateTheme();
        }
        private void SetCompanyLabel()
        {
            CompanyName_Label.Text = Directories.companyName;
            MoveEditButton();
        }
        private void MoveEditButton()
        {
            Edit_Button.Left = CompanyName_Label.Left + CompanyName_Label.Width + 5;
        }
        private void LoadProducts()
        {
            LoadProductsFromFile(Directories.productPurchases_file, productCategoryPurchaseList);
            LoadProductsFromFile(Directories.productSales_file, productCategorySaleList);
        }
        private static void LoadProductsFromFile(string filePath, List<Category> categoryList)
        {
            string[] lines = Directories.ReadAllLinesInFile(filePath);
            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                if (fields.Length == 4)
                {
                    string categoryName = fields[2];
                    Category category = categoryList.FirstOrDefault(c => c.Name == categoryName);

                    if (category == null)
                    {
                        category = new Category(categoryName);
                        categoryList.Add(category);
                    }

                    category.AddProduct(new Product(fields[0], fields[2]));
                }
            }
        }
        public void LoadSales()
        {
            selectedDataGridView = Sales_DataGridView;
            LoadColumnsInDataGridView(selectedDataGridView, SalesColumnHeaders);
            Selected = Options.Sales;
            AddRowsFromFile();
            UpdateTotals();
            AlignTotalLabels();
        }
        public void LoadPurchases()
        {
            selectedDataGridView = Purchases_DataGridView;
            LoadColumnsInDataGridView(selectedDataGridView, PurchaseColumnHeaders);
            Selected = Options.Purchases;
            AddRowsFromFile();
            UpdateTotals();
            AlignTotalLabels();
        }
        public void LoadGraphs()
        {
            double total;
            if (Selected == Options.Sales)
            {
                total = Bar.LoadTotalsIntoChart(Sales_DataGridView, Bar_GunaChart, LineGraph_ToggleSwitch.Checked);
                Bar_Label.Text = $"Total revenue: {total:C}";
                Pie.LoadDistributionIntoChart(Sales_DataGridView, Pie_GunaChart);
                Pie_Label.Text = "Distribution of revenue";
            }
            else
            {
                total = Bar.LoadTotalsIntoChart(Purchases_DataGridView, Bar_GunaChart, LineGraph_ToggleSwitch.Checked);
                Bar_Label.Text = $"Total expenses: {total:C}";
                Pie.LoadDistributionIntoChart(Purchases_DataGridView, Pie_GunaChart);
                Pie_Label.Text = "Distribution of expenses";
            }
            total = Bar.LoadProfitsIntoChart(Sales_DataGridView, Purchases_DataGridView, Bar2_GunaChart, LineGraph_ToggleSwitch.Checked);
            Bar2_Label.Text = $"Total profits: {total:C}";
        }
        private bool DoNotUpdateTheme;
        private void LoadDataFromSetting()
        {
            if (CurrentTheme == ThemeType.Dark)
            {
                DoNotUpdateTheme = true;
                DarkMode_ToggleSwitch.Checked = true;
                DoNotUpdateTheme = false;
            }
        }
        private void UpdateTheme()
        {
            CustomColors.SetColors();
            Theme.SetThemeForForm(this);
            if (CurrentTheme == ThemeType.Dark)
            {
                Edit_Button.Image = Resources.EditWhite;
            }
            else
            {
                Edit_Button.Image = Resources.EditBlack;
            }

            MainTop_Panel.FillColor = CustomColors.background4;
            Edit_Button.FillColor = CustomColors.background4;
            Top_Panel.BackColor = CustomColors.background3;
            File_Button.FillColor = CustomColors.background3;
            Save_Button.FillColor = CustomColors.background3;
            Help_Button.FillColor = CustomColors.background3;
        }

        // Form
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            ResizeControls();
        }
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            // Ensure the charts are rendered
            // BeginInvoke ensures that the chart update logic runs on the main UI thread after all pending UI events have been processed.
            BeginInvoke(() =>
            {
                Bar_GunaChart.Invalidate();
                Bar_GunaChart.Refresh();
                Pie_GunaChart.Invalidate();
                Pie_GunaChart.Refresh();
                Bar2_GunaChart.Invalidate();
                Bar2_GunaChart.Refresh();
            });
            Log.Write(2, "Argo Sales Tracker has finished starting");
        }
        private void ResizeControls()
        {
            if (Height > 1000)
            {
                Bar_GunaChart.Height = 350;
            }
            else if (Height > 800)
            {
                Bar_GunaChart.Height = 300;
            }
            else
            {
                Bar_GunaChart.Height = 200;
            }

            if (Width > 1300)
            {
                selectedDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            }
            else
            {
                selectedDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            Bar_GunaChart.Width = Width / 3 - 30;
            Bar_GunaChart.Left = 20;
            Bar_Label.Left = Bar_GunaChart.Left;

            Pie_GunaChart.Size = new Size(Bar_GunaChart.Width, Bar_GunaChart.Height);
            Pie_GunaChart.Left = (Width / 2) - (Pie_GunaChart.Width / 2) - 8;
            Pie_Label.Left = Pie_GunaChart.Left;

            Bar2_GunaChart.Size = new Size(Bar_GunaChart.Width, Bar_GunaChart.Height);
            Bar2_GunaChart.Left = Width - Bar_GunaChart.Width - 35;
            Bar2_Label.Left = Bar2_GunaChart.Left;

            selectedDataGridView.Size = new Size(Width - 55, Height - MainTop_Panel.Height - Top_Panel.Height - Bar_GunaChart.Height - Bar_GunaChart.Top - 15);
            selectedDataGridView.Location = new Point((Width - selectedDataGridView.Width) / 2 - 7, Height - MainTop_Panel.Height - Top_Panel.Height - selectedDataGridView.Height);

            Total_Panel.Location = new Point(selectedDataGridView.Left, selectedDataGridView.Top + selectedDataGridView.Height);
            Total_Panel.Width = selectedDataGridView.Width;

            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
            }
        }
        private void MainMenu_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Write(2, "Closing Argo Studio");

            UI.CloseAllPanels(null, null);

            // Save logs in file
            if (Directory.Exists(Directories.logs_dir))
            {
                DateTime time = DateTime.Now;
                int count = 0;
                string directory;

                while (true)
                {
                    if (count == 0)
                    {
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + ArgoFiles.TxtFileExtension;
                    }
                    else
                    {
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + "-" + count + ArgoFiles.TxtFileExtension;
                    }
                    if (!Directory.Exists(directory))
                    {
                        Directories.WriteTextToFile(directory, Log.logText);
                        break;
                    }
                    count++;
                }
            }

            if (ArgoCompany.AreAnyChangesMade())
            {
                if (AskUserToSaveBeforeClosing())
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Delete hidden directory
            Directories.DeleteDirectory(Directories.tempCompany_dir, true);
        }

        /// <summary>
        /// Asks the user to save any changes.
        /// </summary>
        /// <returns>Returns true if the user cancels. Returns false if the user saves.</returns>
        private static bool AskUserToSaveBeforeClosing()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "Save changes to the following items?", CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

            switch (result)
            {
                case CustomMessageBoxResult.Save:
                    ArgoCompany.SaveAll();
                    break;
                case CustomMessageBoxResult.DontSave:
                    // Do nothing so the temp directory is deleted
                    break;
                case CustomMessageBoxResult.Cancel:
                    // Cancel close
                    return true;
                default:  // If the CustomMessageBox was closed
                    return true;
            }

            return false;
        }

        // Keyboard shortcuts
        private void MainMenu_form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        if (e.Shift)  // Save as
                        {
                            ArgoCompany.SaveAs();
                        }
                        else  // Save
                        {
                            UI.SaveAll();
                        }
                        break;

                    case Keys.E:  // Export
                        new Export_Form().ShowDialog();
                        break;

                    case Keys.L:  // Open logs
                        OpenLogs();
                        break;
                }
            }
            else if (e.Alt & e.KeyCode == Keys.F4)  // Close program
            {
                if (AskUserToSaveBeforeClosing())
                {
                    e.Handled = true;
                }
            }
        }


        // TOP BAR
        // Don't initiate these yet because it resets every time a program is loaded
        // File
        private void File_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.fileMenu))
            {
                Controls.Remove(UI.fileMenu);
                File_Button.Image = Resources.FileGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                File_Button.Image = Resources.FileWhite;
                UI.fileMenu.Location = new Point(File_Button.Left, 30);
                Controls.Add(UI.fileMenu);
                UI.fileMenu.BringToFront();
            }
        }

        // Save btn
        private void Save_Button_Click(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            UI.SaveAll();
        }
        private void Save_Button_MouseDown(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveWhite;
        }
        private void Save_Button_MouseUp(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveGray;
        }

        // Help
        private void Help_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.helpMenu))
            {
                Controls.Remove(UI.helpMenu);
                Help_Button.Image = Resources.HelpGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                Help_Button.Image = Resources.HelpWhite;
                UI.helpMenu.Location = new Point(Help_Button.Left - UI.helpMenu.Width + Help_Button.Width, 30);
                Controls.Add(UI.helpMenu);
                UI.helpMenu.BringToFront();
            }
        }


        // Controls
        private void Purchases_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Selected = Options.Purchases;
            selectedDataGridView = Purchases_DataGridView;
            Controls.Add(Purchases_DataGridView);
            ResizeControls();
            Controls.Remove(Sales_DataGridView);
            LoadGraphs();
            UpdateTotals();
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Selected = Options.Sales;
            selectedDataGridView = Sales_DataGridView;
            Controls.Add(Sales_DataGridView);
            ResizeControls();
            Controls.Remove(Purchases_DataGridView);
            LoadGraphs();
            UpdateTotals();
        }
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new AddPurchase_Form().ShowDialog();
        }
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new AddSale_Form().ShowDialog();
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {
            new Products_Form().ShowDialog();
        }
        private void ManageCategories_Button_Click(object sender, EventArgs e)
        {
            new Categories_Form().ShowDialog();
        }
        public List<Category> productCategorySaleList = [];
        public List<Category> productCategoryPurchaseList = [];
        public List<string> GetProductCategorySaleNames()
        {
            return productCategorySaleList.Select(s => s.Name).ToList();
        }
        public List<string> GetProductCategoryPurchaseNames()
        {
            return productCategoryPurchaseList.Select(p => p.Name).ToList();
        }
        public List<string> GetProductSaleNames()
        {
            List<string> productNames = [];

            foreach (Category category in productCategorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    productNames.Add(product.Name);
                }
            }
            return productNames;
        }
        public List<string> GetProductPurchaseNames()
        {
            List<string> productNames = [];

            foreach (Category category in productCategoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    productNames.Add(product.Name);
                }
            }
            return productNames;
        }
        public static void AddProductToCategoryByName(List<Category> categoryList, string categoryName, Product product)
        {
            Category category = GetCategoryByName(categoryList, categoryName);
            category.AddProduct(product);
        }
        public static Category? GetCategoryByName(List<Category> categoryList, string categoryName)
        {
            foreach (Category category in categoryList)
            {
                if (category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    return category;
                }
            }
            return null;
        }
        public static string GetCategoryNameByProductName(List<Category> categoryList, string productName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                    {
                        return category.Name;
                    }
                }
            }
            return null;
        }
        private void DarkMode_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (DoNotUpdateTheme) { return; }

            CloseAllPanels(null, null);

            if (DarkMode_ToggleSwitch.Checked)
            {
                CurrentTheme = ThemeType.Dark;
            }
            else
            {
                CurrentTheme = ThemeType.Light;
            }
            UpdateTheme();
        }
        private void LineGraph_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            LoadGraphs();
        }
        private void Edit_Button_Click(object sender, EventArgs e)
        {
            UI.rename_textBox.Text = CompanyName_Label.Text;
            UI.rename_textBox.Location = new Point(CompanyName_Label.Left, CompanyName_Label.Top + CompanyName_Label.Parent.Top - 1);
            UI.rename_textBox.Size = new Size(200, CompanyName_Label.Height + 2);
            UI.rename_textBox.Font = CompanyName_Label.Font;
            Controls.Add(UI.rename_textBox);
            UI.rename_textBox.Focus();
            UI.rename_textBox.SelectAll();
            UI.rename_textBox.BringToFront();
            MainTop_Panel.Controls.Remove(Edit_Button);
            MainTop_Panel.Controls.Remove(CompanyName_Label);
        }
        public void RenameCompany()
        {
            if (!Controls.Contains(UI.rename_textBox))
            {
                return;
            }
            Controls.Remove(UI.rename_textBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);

            if (UI.rename_textBox.Text == CompanyName_Label.Text)
            {
                return;
            }
            CompanyName_Label.Text = UI.rename_textBox.Text;

            string newDir = Directories.argoCompany_dir + "\\" + UI.rename_textBox.Text + ArgoFiles.ArgoCompanyFileExtension;
            Directories.MoveFile(Directories.argoCompany_file, newDir);
            Directories.argoCompany_file = newDir;

            newDir = Directories.appData_dir + UI.rename_textBox.Text;
            Directories.RenameFolder(Directories.tempCompany_dir, newDir);
            Directories.tempCompany_dir = newDir;

            UI.rename_textBox.Text = "";

            MoveEditButton();
        }


        // Filter_ComboBox
        private enum TimeInterval
        {
            AllTime,
            Hours24,
            Hours48,
            Days5,
            Days10,
            Days30,
            Days100,
            Year1,
            Years2,
            Years3,
            Years5,
            Years10
        }
        private readonly List<(TimeInterval interval, string displayString, TimeSpan timeSpan)> timeIntervals =
        [
            (TimeInterval.AllTime, "All time", TimeSpan.MaxValue),
            (TimeInterval.Hours24, "24 hours", TimeSpan.FromHours(24)),
            (TimeInterval.Hours48, "48 hours", TimeSpan.FromHours(48)),
            (TimeInterval.Days5, "5 days", TimeSpan.FromDays(5)),
            (TimeInterval.Days10, "10 days", TimeSpan.FromDays(10)),
            (TimeInterval.Days30, "30 days", TimeSpan.FromDays(30)),
            (TimeInterval.Days100, "100 days", TimeSpan.FromDays(100)),
            (TimeInterval.Year1, "1 year", TimeSpan.FromDays(365)),
            (TimeInterval.Years2, "2 years", TimeSpan.FromDays(365 * 2)),
            (TimeInterval.Years3, "3 years", TimeSpan.FromDays(365 * 3)),
            (TimeInterval.Years5, "5 years", TimeSpan.FromDays(365 * 5)),
            (TimeInterval.Years10, "10 years", TimeSpan.FromDays(365 * 10))
        ];
        private void AddTimeRangesIntoComboBox()
        {
            Filter_ComboBox.Items.AddRange(timeIntervals.Select(ti => ti.displayString).ToArray());
            Filter_ComboBox.SelectedIndex = 0;
        }
        private void Filter_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            FilterDataGridViewByDate();
            LoadGraphs();
        }
        private void FilterDataGridViewByDate()
        {
            string filter = Filter_ComboBox.SelectedItem.ToString();
            var selectedInterval = timeIntervals.FirstOrDefault(ti => ti.displayString == filter);

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
            {
                DateTime rowDate = DateTime.Parse(row.Cells[SalesColumnHeaders[SalesColumns.Date]].Value.ToString());
                bool isVisible = selectedInterval.interval == TimeInterval.AllTime || rowDate >= DateTime.Now - selectedInterval.timeSpan;
                row.Visible = isVisible;
            }
        }


        // DataGridView
        public bool isDataGridViewLoading;
        public Options Selected;
        public enum Options
        {
            Purchases,
            Sales,
            ProductPurchases,
            ProductSales,
            CategoryPurchases,
            CategorySales
        }
        public enum PurchaseColumns
        {
            PurchaseID,
            BuyerName,
            Product,
            Category,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            TotalExpenses
        }
        public enum SalesColumns
        {
            SalesID,
            CustomerName,
            Product,
            Category,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            TotalRevenue
        }
        public readonly Dictionary<PurchaseColumns, string> PurchaseColumnHeaders = new()
        {
            { PurchaseColumns.PurchaseID, "Purchase ID" },
            { PurchaseColumns.BuyerName, "Buyer name" },
            { PurchaseColumns.Product, "Product name" },
            { PurchaseColumns.Category, "Category" },
            { PurchaseColumns.Date, "Date" },
            { PurchaseColumns.Quantity, "Quantity" },
            { PurchaseColumns.PricePerUnit, "Price per unit" },
            { PurchaseColumns.Shipping, "Shipping" },
            { PurchaseColumns.Tax, "Tax" },
            { PurchaseColumns.TotalExpenses, "Total expenses" }
        };
        public readonly Dictionary<SalesColumns, string> SalesColumnHeaders = new()
        {
            { SalesColumns.SalesID, "Sales ID" },
            { SalesColumns.CustomerName, "Customer name" },
            { SalesColumns.Product, "Product name" },
            { SalesColumns.Category, "Category" },
            { SalesColumns.Date, "Date" },
            { SalesColumns.Quantity, "Quantity" },
            { SalesColumns.PricePerUnit, "Price per unit" },
            { SalesColumns.Shipping, "Shipping" },
            { SalesColumns.Tax, "Tax" },
            { SalesColumns.TotalRevenue, "Total revenue" }
        };
        public enum DataGridViewTags
        {
            SaleOrPurchase,
            AddCategory,
            AddProduct,
        }
        public readonly string CategoryColumn = "Category";
        public Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        public Guna2DataGridView selectedDataGridView;
        private bool doNotDeleteRows = false;
        private void ConstructDataGridViews()
        {
            Size size = new(1300, 350);
            Purchases_DataGridView = new Guna2DataGridView();
            InitializeDataGridView(Purchases_DataGridView, size);
            Purchases_DataGridView.Tag = DataGridViewTags.SaleOrPurchase;

            Sales_DataGridView = new Guna2DataGridView();
            InitializeDataGridView(Sales_DataGridView, size);
            Sales_DataGridView.Tag = DataGridViewTags.SaleOrPurchase;
        }
        private readonly byte rowHeight = 25, columnHeaderHeight = 30;
        public void InitializeDataGridView(Guna2DataGridView dataGridView, Size size)
        {
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
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dataGridView.Theme = CustomColors.dataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.controlBack;
            dataGridView.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            dataGridView.Size = size;

            dataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            dataGridView.RowsAdded += DataGridView_RowsAdded;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
            dataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView.MouseDown += DataGridView_MouseDown;
            dataGridView.MouseUp += DataGridView_MouseUp;
            dataGridView.KeyDown += DataGridView_KeyDown;

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.HeaderCell.Style.BackColor = CustomColors.background2;
                column.HeaderCell.Style.SelectionBackColor = CustomColors.background2;
            }
        }
        private void DataGridView_UserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
        {
            if (doNotDeleteRows)
            {
                e.Cancel = true;
            }
            string type = "", columnName = "";
            byte logIndex = 0;

            switch (Selected)
            {
                case Options.Purchases:
                    type = "purchase";
                    columnName = PurchaseColumns.Product.ToString();
                    logIndex = 2;
                    break;
                case Options.Sales:
                    type = "sale";
                    columnName = SalesColumns.Product.ToString();
                    logIndex = 2;
                    break;
                case Options.ProductPurchases:
                    type = "product for purchase";
                    columnName = Products_Form.Columns.ProductName.ToString();
                    logIndex = 3;
                    break;
                case Options.ProductSales:
                    type = "product for sale";
                    columnName = Products_Form.Columns.ProductName.ToString();
                    logIndex = 3;
                    break;
                case Options.CategoryPurchases:
                    type = "Category for purchase";
                    columnName = CategoryColumn;
                    logIndex = 3;
                    break;
                case Options.CategorySales:
                    type = "Category for sale";
                    columnName = CategoryColumn;
                    logIndex = 3;
                    break;
            }
            string name = e.Row.Cells[columnName].Value?.ToString();

            Log.Write(logIndex, $"Deleted {type} '{name}'");
        }
        public void DataGridView_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
        {
            AlignTotalLabels();
        }
        private void AddRowsFromFile()
        {
            string filePath = GetFilePathForDataGridView(Selected);

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            // Add all rows to dataGridView_list
            selectedDataGridView.Rows.Clear();

            string[] lines = Directories.ReadAllLinesInFile(filePath);
            foreach (string line in lines)
            {
                string[] cellValues = line.Split(',');
                selectedDataGridView.Rows.Add(cellValues);
            }
        }
        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridViewRowChanged();
        }
        private void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            DataGridViewRowChanged();
        }
        private void DataGridViewRowChanged()
        {
            SaveDataGridViewToFile(selectedDataGridView, Selected);

            if (Selected == Options.Sales || Selected == Options.Purchases)
            {
                UpdateTotals();
                LoadGraphs();
            }
        }
        private void DataGridView_MouseDown(object? sender, MouseEventArgs e)
        {
            UI.CloseAllPanels(null, null);

            if (e.Button == MouseButtons.Right)
            {
                Guna2DataGridView grid = (Guna2DataGridView)sender;

                // The right click button does not select rows by default, so implement it here
                // If it is not currently selected, unselect others
                DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);
                if (info.RowIndex == -1)
                {
                    return;
                }
                UnselectAllRowsInCurrentDataGridView();

                // Select current row
                grid.Rows[info.RowIndex].Selected = true;
                grid.Focus();
            }

            // Set color
            selectedDataGridView.RowsDefaultCellStyle.SelectionBackColor = CustomColors.fileSelected;
        }
        Control controlRightClickPanelWasAddedTo;
        private void DataGridView_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Guna2DataGridView grid = (Guna2DataGridView)sender;
                DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);

                // Calculate the horizontal position
                bool tooFarRight = false;
                if (selectedDataGridView.Left + rightClickDataGridView_Panel.Width + e.X - rowHeight > Width)
                {
                    rightClickDataGridView_Panel.Left = Width - rightClickDataGridView_Panel.Width - 17;
                    tooFarRight = true;
                }
                else
                {
                    rightClickDataGridView_Panel.Left = selectedDataGridView.Left + e.X - rowHeight;
                }

                // Calculate the vertical position
                int verticalOffset = grid.FirstDisplayedScrollingRowIndex * grid.Rows[0].Height;
                int rowTop = (info.RowIndex + 1) * grid.Rows[0].Height - verticalOffset + selectedDataGridView.Top + columnHeaderHeight;

                if (rowTop + rightClickDataGridView_Panel.Height > Height)
                {
                    rightClickDataGridView_Panel.Top = Height - rightClickDataGridView_Panel.Height - 2;
                    if (!tooFarRight)
                    {
                        rightClickDataGridView_Panel.Left += 30;
                    }
                }
                else
                {
                    rightClickDataGridView_Panel.Top = rowTop;
                }

                Control controlSender = (Control)sender;
                controlRightClickPanelWasAddedTo = controlSender.Parent;
                controlRightClickPanelWasAddedTo.Controls.Add(rightClickDataGridView_Panel);
                rightClickDataGridView_Panel.BringToFront();
            }
        }
        private void DataGridView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                CloseRightClickPanels();

                string message;
                if (selectedDataGridView.SelectedRows.Count == 1)
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
                    doNotDeleteRows = true;
                    UnselectAllRowsInCurrentDataGridView();
                }
            }
        }
        public void SaveDataGridViewToFile(Guna2DataGridView dataGridView, Options option)
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string filePath = GetFilePathForDataGridView(option);

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            List<string> linesInDataGridView = [];

            // Write all the rows in the DataGridView to file
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView.Rows[i];
                List<string> cellValues = [];

                foreach (DataGridViewCell cell in row.Cells)
                {
                    cellValues.Add(cell.Value?.ToString() ?? string.Empty);
                }

                string line = string.Join(",", cellValues);  // Join cell values with a comma
                linesInDataGridView.Add(line);
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);
            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, $"{Selected} list");
        }
        public static void LoadColumnsInDataGridView<TEnum>(Guna2DataGridView dataGridView, Dictionary<TEnum, string> columnHeaders) where TEnum : Enum
        {
            foreach (var column in Enum.GetValues(typeof(TEnum)))
            {
                dataGridView.Columns.Add(column.ToString(), columnHeaders[(TEnum)column]);
            }
            Theme.UpdateDataGridViewHeaderTheme(dataGridView);
        }
        private void UpdateTotals()
        {
            if (isDataGridViewLoading || Selected == Options.ProductPurchases || Selected == Options.ProductSales)
            {
                return;
            }

            int totalQuantity = 0;
            decimal totalPrice = 0;
            decimal totalTax = 0;
            decimal totalShipping = 0;

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
            {
                if (Selected == Options.Purchases)
                {
                    totalQuantity += Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                    totalPrice += Convert.ToDecimal(row.Cells[PurchaseColumns.TotalExpenses.ToString()].Value);
                    totalTax += Convert.ToDecimal(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                    totalShipping += Convert.ToDecimal(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                }
                else
                {
                    totalQuantity += Convert.ToInt32(row.Cells[SalesColumns.Quantity.ToString()].Value);
                    totalPrice += Convert.ToDecimal(row.Cells[SalesColumns.TotalRevenue.ToString()].Value);
                    totalTax += Convert.ToDecimal(row.Cells[SalesColumns.Tax.ToString()].Value);
                    totalShipping += Convert.ToDecimal(row.Cells[SalesColumns.Shipping.ToString()].Value);
                }
            }

            Quantity_Label.Text = $"Quantity: {totalQuantity}";
            Tax_Label.Text = $"Tax: {totalTax:C}";
            Shipping_Label.Text = $"Shipping: {totalShipping:C}";
            Price_Label.Text = $"Price: {totalPrice:C}";
        }
        private void AlignTotalLabels()
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string quantityColumn, taxColumn, shippingColumn, totalPriceColumn;

            if (Selected == Options.Purchases)
            {
                quantityColumn = PurchaseColumns.Quantity.ToString();
                taxColumn = PurchaseColumns.Tax.ToString();
                shippingColumn = PurchaseColumns.Shipping.ToString();
                totalPriceColumn = PurchaseColumns.TotalExpenses.ToString();
            }
            else
            {
                quantityColumn = SalesColumns.Quantity.ToString();
                taxColumn = SalesColumns.Tax.ToString();
                shippingColumn = SalesColumns.Shipping.ToString();
                totalPriceColumn = SalesColumns.TotalRevenue.ToString();
            }

            Quantity_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = selectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = selectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = selectedDataGridView.Columns[shippingColumn].Width;

            Price_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = selectedDataGridView.Columns[totalPriceColumn].Width;
        }
        private string GetFilePathForDataGridView(Options option)
        {
            return option switch
            {
                Options.Purchases => Directories.purchases_file,
                Options.Sales => Directories.sales_file,
                Options.ProductPurchases => Directories.productPurchases_file,
                Options.ProductSales => Directories.productSales_file,
                Options.CategoryPurchases => Directories.categoryPurchases_file,
                Options.CategorySales => Directories.categorySales_file,
                _ => ""
            };
        }
        private void UnselectAllRowsInCurrentDataGridView()
        {
            foreach (DataGridViewRow row in selectedDataGridView.Rows)
            {
                row.Selected = false;
            }
        }


        // Right click DataGridView row menu
        private Guna2Panel rightClickDataGridView_Panel;
        public void ConstructRightClickDataGridViewRowMenu()
        {
            rightClickDataGridView_Panel = UI.ConstructPanelForMenu(new Size(250, 3 * 22 + 10));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)rightClickDataGridView_Panel.Controls[0];

            rightClickDataGridView_Panel.BringToFront();

            Guna2Button menuBtn = UI.ConstructBtnForMenu("Modify", 240, false, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                CloseRightClickPanels();
                if (selectedDataGridView.SelectedRows.Count > 1)
                {
                    CustomMessageBox.Show("Argo Studio", "You can only select one row to modify.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                ModifyRow_Form ModifyRow_form = new(selectedDataGridView.Rows[0]);
                ModifyRow_form.ShowDialog();
            };

            menuBtn = UI.ConstructBtnForMenu("Duplicate", 240, false, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                CloseRightClickPanels();

                if (selectedDataGridView.Rows.Count == 0)
                {
                    CustomMessageBox.Show("Argo Studio", "Select a row to duplicate.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                int index = selectedDataGridView.SelectedRows[0].Index;

                // Duplicate row
                DataGridViewRow selectedRow = CloneWithValues(selectedDataGridView.SelectedRows[0]);
                selectedDataGridView.Rows.Add(selectedRow);

                // The new row is at the bottom by default, so move it up until it's below the row that was duplicated
                for (int i = selectedDataGridView.Rows.Count - 1; i > index; i--)
                {
                    // Use tuple to swap values
                    (selectedDataGridView.Rows[i].Cells[0].Value, selectedDataGridView.Rows[i - 1].Cells[0].Value) = (selectedDataGridView.Rows[i - 1].Cells[0].Value, selectedDataGridView.Rows[i].Cells[0].Value);
                }

                // Select the new row
                selectedDataGridView.Rows[index].Cells[0].Selected = false;
                selectedDataGridView.Rows[index + 1].Cells[0].Selected = true;
            };
            menuBtn = UI.ConstructBtnForMenu("Delete", 240, false, flowPanel);
            menuBtn.ForeColor = CustomColors.accent_red;
            menuBtn.Click += (sender, e) =>
            {
                CloseRightClickPanels();

                if (selectedDataGridView.Rows.Count == 0)
                {
                    CustomMessageBox.Show("Argo Studio", "Select a row to delete.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                int index = selectedDataGridView.SelectedRows[selectedDataGridView.SelectedRows.Count - 1].Index;

                // Delete all selected rows
                foreach (DataGridViewRow item in selectedDataGridView.SelectedRows)
                {
                    selectedDataGridView.Rows.Remove(item);
                }

                // If no rows are automatically selected again, select the row under the row that was just deleted
                if (selectedDataGridView.Rows.Count != 0)
                {
                    // If the deleted row was not at the bottom
                    if (index > selectedDataGridView.SelectedRows.Count)
                    {
                        selectedDataGridView.Rows[index - 1].Selected = true;
                    }
                    else
                    // Select the bottom row
                    {
                        selectedDataGridView.Rows[^1].Selected = true;
                    }
                }
            };
            UI.ConstructKeyShortcut("Del", menuBtn);
        }
        private static DataGridViewRow CloneWithValues(DataGridViewRow row)
        {
            DataGridViewRow clonedRow = (DataGridViewRow)row.Clone();
            for (Int32 index = 0; index < row.Cells.Count; index++)
            {
                clonedRow.Cells[index].Value = row.Cells[index].Value;
            }
            return clonedRow;
        }

        // Message panel
        public Guna2Panel messagePanel;
        public void ConstructMessage_Panel()
        {
            messagePanel = new Guna2Panel
            {
                Size = new Size(500, 150),
                FillColor = CustomColors.mainBackground,
                BackColor = Color.Transparent,
                BorderThickness = 1,
                BorderRadius = 5,
                BorderColor = CustomColors.controlPanelBorder
            };

            Label messageLabel = new()
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(10, 10),
                Size = new Size(480, 85),
                Name = "label",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = CustomColors.text
            };
            messageLabel.TextChanged += (sender, e) =>
            {
                if (messageLabel.Text != "")
                {
                    messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
                    Controls.Add(messagePanel);
                    messagePanel.BringToFront();
                    // Restart timer
                    MessagePanel_timer.Enabled = false;
                    MessagePanel_timer.Enabled = true;
                }
            };
            messagePanel.Controls.Add(messageLabel);

            Guna2Button gBtn = new()
            {
                Font = new Font("Segoe UI", 11),
                Text = "Ok",
                Size = new Size(120, 35),
                Location = new Point(190, 100),
                FillColor = Color.FromArgb(58, 153, 236),  // Blue
                ForeColor = Color.White
            };
            gBtn.Click += MessagePanelClose;
            messagePanel.Controls.Add(gBtn);

            PictureBox picture = new()
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, 10),
                Size = new Size(15, 15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Image = Resources.CloseGrey,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            picture.Click += MessagePanelClose;
            messagePanel.Controls.Add(picture);
        }
        private void SetMessage(string text)
        {
            Label label = (Label)messagePanel.Controls.Find("label", false).FirstOrDefault();
            label.Text = text;
        }
        private void MessagePanelClose(object? sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }
        private void MessagePanelTimer_Tick(object sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }


        private Log_Form LogForm;
        public void OpenLogs()
        {
            if (!Tools.IsFormOpen(typeof(Log_Form)))
            {
                LogForm = new Log_Form();
                LogForm.Show();
            }
            else
            {
                LogForm.BringToFront();
            }
        }

        public void CloseRightClickPanels()
        {
            controlRightClickPanelWasAddedTo?.Controls.Remove(rightClickDataGridView_Panel);
            doNotDeleteRows = false;
        }
        private void CloseAllPanels(object? sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}