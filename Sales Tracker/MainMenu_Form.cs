using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Graphs;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings;
using System.Text.Json;
using static Sales_Tracker.Classes.Theme;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init.
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
            LoadCategories();
            LoadSalesAndPurchases();
            LoadAccountants();
            LoadCompanies();
            isDataGridViewLoading = false;
            Sales_Button.PerformClick();
            AddTimeRangesIntoComboBox();
            UpdateTheme();
        }
        private void LoadCategories()
        {
            LoadCategoriesFromFile(Directories.categoryPurchases_file, categoryPurchaseList);
            LoadCategoriesFromFile(Directories.categorySales_file, categorySaleList);
        }
        private static void LoadCategoriesFromFile(string filePath, List<Category> categoryList)
        {
            string json = Directories.ReadAllTextInFile(filePath);
            if (string.IsNullOrWhiteSpace(json)) { return; }

            List<Category>? loadedCategories = JsonSerializer.Deserialize<List<Category>>(json);

            if (loadedCategories != null)
            {
                categoryList.AddRange(loadedCategories);
            }
        }
        private void LoadAccountants()
        {
            accountantList = Directories.ReadAllLinesInFile(Directories.accountants_file).ToList();
        }
        private void LoadCompanies()
        {
            companyList = Directories.ReadAllLinesInFile(Directories.companies_file).ToList();
        }
        public void LoadSalesAndPurchases()
        {
            LoadColumnsInDataGridView(Sales_DataGridView, SalesColumnHeaders);
            AddRowsFromFile(Sales_DataGridView, Options.Sales);

            LoadColumnsInDataGridView(Purchases_DataGridView, PurchaseColumnHeaders);
            AddRowsFromFile(Purchases_DataGridView, Options.Purchases);
        }
        private static void AddRowsFromFile(Guna2DataGridView dataGridView, Options selected)
        {
            string filePath = GetFilePathForDataGridView(selected);

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            // Add all rows to dataGridView_list
            dataGridView.Rows.Clear();

            string[] lines = Directories.ReadAllLinesInFile(filePath);
            foreach (string line in lines)
            {
                string[] cellValues = line.Split(',');
                dataGridView.Rows.Add(cellValues);
            }
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
        public void UpdateTheme()
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
            Account_Button.FillColor = CustomColors.background3;
        }

        // Form event handlers
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
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            ResizeControls();
        }
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

        // Resize controls
        private bool wasControlsDropDownAdded = false;
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

            if (Width < 1000 + Edit_Button.Left + Edit_Button.Width)
            {
                AddControlsDropDown();
                wasControlsDropDownAdded = true;
            }
            else if (wasControlsDropDownAdded)
            {
                RemoveControlsDropDown();
                wasControlsDropDownAdded = false;
            }

            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
            }
        }
        private void AddControlsDropDown()
        {
            UI.controlsDropDown_Button.Location = new Point(MainTop_Panel.Width - UI.controlsDropDown_Button.Width - 11, (MainTop_Panel.Height - UI.controlsDropDown_Button.Height) / 2);
            MainTop_Panel.Controls.Add(UI.controlsDropDown_Button);
            MainTop_Panel.Controls.Remove(ManageAccountants_Button);
            MainTop_Panel.Controls.Remove(ManageCategories_Button);
            MainTop_Panel.Controls.Remove(ManageCompanies_Button);
            MainTop_Panel.Controls.Remove(ManageProducts_Button);
            MainTop_Panel.Controls.Remove(AddSale_Button);
            MainTop_Panel.Controls.Remove(AddPurchase_Button);
        }
        private void RemoveControlsDropDown()
        {
            MainTop_Panel.Controls.Remove(UI.controlsDropDown_Button);

            int buttonTop = (MainTop_Panel.Height - ManageAccountants_Button.Height) / 2;
            int buttonWidthPlusSpace = ManageAccountants_Button.Width + 8;
            ManageAccountants_Button.Location = new Point(MainTop_Panel.Width - buttonWidthPlusSpace - 5, buttonTop);
            ManageCategories_Button.Location = new Point(ManageAccountants_Button.Left - buttonWidthPlusSpace, buttonTop);
            ManageCompanies_Button.Location = new Point(ManageCategories_Button.Left - buttonWidthPlusSpace, buttonTop);
            ManageProducts_Button.Location = new Point(ManageCompanies_Button.Left - buttonWidthPlusSpace, buttonTop);
            AddSale_Button.Location = new Point(ManageProducts_Button.Left - buttonWidthPlusSpace, buttonTop);
            AddPurchase_Button.Location = new Point(AddSale_Button.Left - buttonWidthPlusSpace, buttonTop);

            MainTop_Panel.Controls.Add(ManageAccountants_Button);
            MainTop_Panel.Controls.Add(ManageCategories_Button);
            MainTop_Panel.Controls.Add(ManageCompanies_Button);
            MainTop_Panel.Controls.Add(ManageProducts_Button);
            MainTop_Panel.Controls.Add(AddSale_Button);
            MainTop_Panel.Controls.Add(AddPurchase_Button);
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


        // Event handlers - top bar
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
                UI.helpMenu.Location = new Point(Help_Button.Left - UI.helpMenu.Width + Help_Button.Width, Top_Panel.Height);
                Controls.Add(UI.helpMenu);
                UI.helpMenu.BringToFront();
            }
        }
        private void Account_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.accountMenu))
            {
                Controls.Remove(UI.accountMenu);
                Account_Button.Image = Resources.ProfileGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                Account_Button.Image = Resources.ProfileWhite;
                UI.accountMenu.Location = new Point(Account_Button.Left - UI.accountMenu.Width + Account_Button.Width, Top_Panel.Height);
                Controls.Add(UI.accountMenu);
                UI.accountMenu.BringToFront();
            }
        }

        // Event handlers
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
            Purchases_DataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
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
            Sales_DataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
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
        private void ManageAccountants_Button_Click(object sender, EventArgs e)
        {
            new Accountants_Form().ShowDialog();
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {
            new Products_Form(false).ShowDialog();
        }
        private void ManageCompanies_Button_Click(object sender, EventArgs e)
        {
            new Companies_Form().ShowDialog();
        }
        private void ManageCategories_Button_Click(object sender, EventArgs e)
        {
            new Categories_Form(false).ShowDialog();
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

        // Company label
        public void RenameCompany()
        {
            if (!Controls.Contains(UI.rename_textBox))
            {
                return;
            }
            Controls.Remove(UI.rename_textBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);

            // If the name did not change
            if (UI.rename_textBox.Text == CompanyName_Label.Text)
            {
                return;
            }

            CompanyName_Label.Text = UI.rename_textBox.Text;
            ArgoCompany.Rename(UI.rename_textBox.Text);
            UI.rename_textBox.Text = "";
            MoveEditButton();
            ResizeControls();
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


        // Lists
        public List<Category> categorySaleList = [];
        public List<Category> categoryPurchaseList = [];
        public List<string> accountantList = [];
        public List<string> companyList = [];
        public List<string> GetProductCategorySaleNames()
        {
            return categorySaleList.Select(s => s.Name).ToList();
        }
        public List<string> GetProductCategoryPurchaseNames()
        {
            return categoryPurchaseList.Select(p => p.Name).ToList();
        }
        public List<string> GetProductSaleNames()
        {
            List<string> productNames = [];

            foreach (Category category in categorySaleList)
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

            foreach (Category category in categoryPurchaseList)
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
            foreach (Category category in categoryList)
            {
                if (category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    category.AddProduct(product);
                    return;
                }
            }
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
            return "null";
        }
        public static string GetCountryProductNameIsFrom(List<Category> categoryList, string productName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                    {
                        return product.CountryOfOrigin;
                    }
                }
            }
            return "null";
        }
        public static string GetCompanyProductNameIsFrom(List<Category> categoryList, string productName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                    {
                        return product.CompanyOfOrigin;
                    }
                }
            }
            return "null";
        }
        public static bool IsProductInCategory(string productName, string productCategory, List<Category> categories)
        {
            foreach (Category category in categories)
            {
                if (category.Name == productCategory)
                {
                    foreach (Product product in category.ProductList)
                    {
                        if (product.Name == productName)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void SaveCategoriesToFile(Options option)
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string filePath = GetFilePathForDataGridView(option);

            if (option == Options.Accountants)
            {
                Directories.WriteLinesToFile(filePath, accountantList);
                return;
            }

            List<Category> categoryList;
            if (option == Options.CategoryPurchases || option == Options.ProductPurchases)
            {
                categoryList = categoryPurchaseList;
            }
            else
            {
                categoryList = categorySaleList;
            }

            string json = JsonSerializer.Serialize(categoryList);
            Directories.WriteTextToFile(filePath, json);

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, $"{Selected} list");
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
            CategorySales,
            Accountants,
            Companies
        }
        public enum PurchaseColumns
        {
            PurchaseID,
            BuyerName,
            Product,
            Category,
            Country,
            Company,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            Fee,
            TotalExpenses
        }
        public enum SalesColumns
        {
            SalesID,
            CustomerName,
            Product,
            Category,
            Country,
            Company,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            Fee,
            TotalRevenue
        }
        public readonly Dictionary<PurchaseColumns, string> PurchaseColumnHeaders = new()
        {
            { PurchaseColumns.PurchaseID, "Purchase ID" },
            { PurchaseColumns.BuyerName, "Buyer name" },
            { PurchaseColumns.Product, "Product name" },
            { PurchaseColumns.Category, "Category" },
            { PurchaseColumns.Country, "Country of origin" },
            { PurchaseColumns.Company, "Company of origin" },
            { PurchaseColumns.Date, "Date" },
            { PurchaseColumns.Quantity, "Quantity" },
            { PurchaseColumns.PricePerUnit, "Price per unit" },
            { PurchaseColumns.Shipping, "Shipping" },
            { PurchaseColumns.Tax, "Tax" },
            { PurchaseColumns.Fee, "Payment fee" },
            { PurchaseColumns.TotalExpenses, "Total expenses" }
        };
        public readonly Dictionary<SalesColumns, string> SalesColumnHeaders = new()
        {
            { SalesColumns.SalesID, "Sales ID" },
            { SalesColumns.CustomerName, "Customer name" },
            { SalesColumns.Product, "Product name" },
            { SalesColumns.Category, "Category" },
            { SalesColumns.Country, "Country of destination" },
            { SalesColumns.Company, "Company of origin" },
            { SalesColumns.Date, "Date" },
            { SalesColumns.Quantity, "Quantity" },
            { SalesColumns.PricePerUnit, "Price per unit" },
            { SalesColumns.Shipping, "Shipping" },
            { SalesColumns.Tax, "Tax" },
            { SalesColumns.Fee, "Payment fee" },
            { SalesColumns.TotalRevenue, "Total revenue" }
        };
        public enum DataGridViewTags
        {
            SaleOrPurchase,
            Category,
            Product,
            Accountant
        }
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
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView.Theme = CustomColors.dataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.controlBack;
            dataGridView.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            dataGridView.Size = size;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

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

                    // Remove product from list
                    categoryPurchaseList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == e.Row.Cells[columnName].Value?.ToString())));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case Options.ProductSales:
                    type = "product for sale";
                    columnName = Products_Form.Columns.ProductName.ToString();
                    logIndex = 3;

                    // Remove product from list
                    categorySaleList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == e.Row.Cells[columnName].Value?.ToString())));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case Options.CategoryPurchases:
                    type = "Category for purchase";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;

                    // Remove category from list
                    categoryPurchaseList.Remove(categoryPurchaseList.FirstOrDefault(c => c.Name == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case Options.CategorySales:
                    type = "Category for sale";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;

                    // Remove category from list
                    categorySaleList.Remove(categorySaleList.FirstOrDefault(c => c.Name == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case Options.Accountants:
                    type = "Accountant";
                    columnName = Accountants_Form.Columns.AccountantName.ToString();
                    logIndex = 3;

                    // Remove accountant from list
                    accountantList.Remove(accountantList.FirstOrDefault(a => a == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the accountant name that is being deleted is in the TextBox
                    Accountants_Form.Instance.VaidateAccountantTextBox();
                    break;
            }
            string name = e.Row.Cells[columnName].Value?.ToString();

            Log.Write(logIndex, $"Deleted {type} '{name}'");
        }
        public void DataGridView_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
        {
            AlignTotalLabels();
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
            if (isDataGridViewLoading)
            {
                return;
            }

            if (Selected == Options.Sales || Selected == Options.Purchases)
            {
                UpdateTotals();
                LoadGraphs();
                SaveDataGridViewToFile();
            }
            else if (Selected == Options.Companies)
            {
                SaveDataGridViewToFile();
            }
            else
            {
                SaveCategoriesToFile(Selected);
            }
        }
        public void SaveDataGridViewToFile()
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string filePath = GetFilePathForDataGridView(Selected);
            List<string> linesInDataGridView = [];

            // Write all the rows in the DataGridView to file
            for (int i = 0; i < selectedDataGridView.Rows.Count; i++)
            {
                DataGridViewRow row = selectedDataGridView.Rows[i];
                List<string> cellValues = [];

                foreach (DataGridViewCell cell in row.Cells)
                {
                    cellValues.Add(cell.Value?.ToString() ?? "");
                }

                string line = string.Join(",", cellValues);  // Join cell values with a comma
                linesInDataGridView.Add(line);
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);
            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, $"{Selected} list");
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
                Control controlSender = (Control)sender;
                controlRightClickPanelWasAddedTo = controlSender.Parent;

                // Calculate the horizontal position
                bool tooFarRight = false;
                if (selectedDataGridView.Left + rightClickDataGridView_Panel.Width + e.X + 17 > controlRightClickPanelWasAddedTo.Width)
                {
                    rightClickDataGridView_Panel.Left = controlRightClickPanelWasAddedTo.Width - rightClickDataGridView_Panel.Width - 17;
                    tooFarRight = true;
                }
                else
                {
                    rightClickDataGridView_Panel.Left = selectedDataGridView.Left + e.X;
                }

                // Calculate the vertical position
                int verticalOffset = grid.FirstDisplayedScrollingRowIndex * grid.Rows[0].Height;
                int rowTop = (info.RowIndex + 1) * grid.Rows[0].Height - verticalOffset + selectedDataGridView.Top + columnHeaderHeight;

                if (rowTop + rightClickDataGridView_Panel.Height > controlRightClickPanelWasAddedTo.Height - 17)
                {
                    rightClickDataGridView_Panel.Top = controlRightClickPanelWasAddedTo.Height - rightClickDataGridView_Panel.Height - 41;
                    if (!tooFarRight)
                    {
                        rightClickDataGridView_Panel.Left += 30;
                    }
                }
                else
                {
                    rightClickDataGridView_Panel.Top = rowTop;
                }

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
            if (isDataGridViewLoading || Selected != Options.Purchases & Selected != Options.Sales)
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
        private static string GetFilePathForDataGridView(Options selected)
        {
            return selected switch
            {
                Options.Purchases => Directories.purchases_file,
                Options.Sales => Directories.sales_file,
                Options.CategoryPurchases => Directories.categoryPurchases_file,
                Options.CategorySales => Directories.categorySales_file,
                Options.ProductPurchases => Directories.categoryPurchases_file,
                Options.ProductSales => Directories.categorySales_file,
                Options.Accountants => Directories.accountants_file,
                Options.Companies => Directories.companies_file,
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
            rightClickDataGridView_Panel = UI.ConstructPanelForMenu(new Size(250, 5 * 22 + 10));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)rightClickDataGridView_Panel.Controls[0];

            rightClickDataGridView_Panel.BringToFront();

            Guna2Button menuBtn = UI.ConstructBtnForMenu("Modify", 240, false, flowPanel);
            menuBtn.Click += ModifyRow;

            menuBtn = UI.ConstructBtnForMenu("Duplicate", 240, false, flowPanel);
            menuBtn.Click += DuplicateRow;

            menuBtn = UI.ConstructBtnForMenu("Move up", 240, false, flowPanel);
            menuBtn.Click += MoveRowUp;

            menuBtn = UI.ConstructBtnForMenu("Move down", 240, false, flowPanel);
            menuBtn.Click += MoveRowDown;

            menuBtn = UI.ConstructBtnForMenu("Delete", 240, false, flowPanel);
            menuBtn.ForeColor = CustomColors.accent_red;
            menuBtn.Click += DeleteRow;

            UI.ConstructKeyShortcut("Del", menuBtn);
        }
        private void ModifyRow(object? sender, EventArgs e)
        {
            CloseRightClickPanels();
            if (selectedDataGridView.SelectedRows.Count > 1)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "You can only select one row to modify.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            ModifyRow_Form ModifyRow_form = new(selectedDataGridView.SelectedRows[0]);
            ModifyRow_form.ShowDialog();
        }
        private void DuplicateRow(object? sender, EventArgs e)
        {
            CloseRightClickPanels();

            if (selectedDataGridView.Rows.Count == 0 || selectedDataGridView.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show("Argo Studio", "Select a row to duplicate.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            int index = selectedDataGridView.SelectedRows[0].Index;

            // Duplicate row
            DataGridViewRow selectedRow = CloneRowWithValues(selectedDataGridView.SelectedRows[0]);
            selectedDataGridView.Rows.Insert(index + 1, selectedRow);

            // Select the new row
            selectedDataGridView.ClearSelection();
            selectedDataGridView.Rows[index + 1].Selected = true;

            SaveDataGridViewToFile();
        }
        private static DataGridViewRow CloneRowWithValues(DataGridViewRow row)
        {
            DataGridViewRow clonedRow = (DataGridViewRow)row.Clone();
            for (int i = 0; i < row.Cells.Count; i++)
            {
                clonedRow.Cells[i].Value = row.Cells[i].Value;
            }
            return clonedRow;
        }
        private void MoveRowUp(object sender, EventArgs e)
        {
            CloseRightClickPanels();

            if (selectedDataGridView.Rows.Count == 0)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "Select a row to move up.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            int rowIndex = selectedDataGridView.SelectedCells[0].OwningRow.Index;

            if (rowIndex == 0)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "Cannot move the first row up.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }
            isDataGridViewLoading = true;

            DataGridViewRow selectedRow = selectedDataGridView.Rows[rowIndex];
            selectedDataGridView.Rows.Remove(selectedRow);
            selectedDataGridView.Rows.Insert(rowIndex - 1, selectedRow);

            // Reselect
            selectedDataGridView.ClearSelection();
            selectedDataGridView.Rows[rowIndex - 1].Selected = true;

            // Save
            SaveDataGridViewToFile();
            isDataGridViewLoading = false;
        }
        private void MoveRowDown(object sender, EventArgs e)
        {
            CloseRightClickPanels();

            if (selectedDataGridView.Rows.Count == 0)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "Select a row to move down.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            int rowIndex = selectedDataGridView.SelectedCells[0].OwningRow.Index;

            if (rowIndex == selectedDataGridView.Rows.Count - 1)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "Cannot move the last row down.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }
            isDataGridViewLoading = true;

            DataGridViewRow selectedRow = selectedDataGridView.Rows[rowIndex];
            selectedDataGridView.Rows.Remove(selectedRow);
            selectedDataGridView.Rows.Insert(rowIndex + 1, selectedRow);

            // Reselect
            selectedDataGridView.ClearSelection();
            selectedDataGridView.Rows[rowIndex + 1].Selected = true;

            // Save
            SaveDataGridViewToFile();
            isDataGridViewLoading = false;
        }
        private void DeleteRow(object? sender, EventArgs e)
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
                // Trigger the UserDeletingRow event manually
                var args = new DataGridViewRowCancelEventArgs(item);
                DataGridView_UserDeletingRow(selectedDataGridView, args);

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
                else  // Select the bottom row
                {
                    selectedDataGridView.Rows[^1].Selected = true;
                }
            }
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

        // Settings
        private Settings_Form SettingsForm;
        public void OpenSettingsMenu()
        {
            if (!Tools.IsFormOpen(typeof(Settings_Form)))
            {
                SettingsForm = new Settings_Form();
                SettingsForm.Show();
            }
            else { SettingsForm.BringToFront(); }
        }

        // Logs
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

        // Misc.
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