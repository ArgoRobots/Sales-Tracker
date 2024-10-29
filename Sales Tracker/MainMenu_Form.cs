using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;
using System.Collections;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Proprties
        private static MainMenu_Form _instance;
        private static readonly List<string> _thingsThatHaveChangedInFile = [];
        private static readonly List<string> _settingsThatHaveChangedInFile = [];
        private static string _currencySymbol;
        private static bool _isFullVersion = true;
        private static bool _isProgramLoading;
        private static readonly string noteTextKey = "note", rowTagKey = "RowTag", itemsKey = "Items", purchaseDataKey = "PurchaseData", tagKey = "Tag";

        // Getters and setters
        public static MainMenu_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;
        public static List<string> SettingsThatHaveChangedInFile => _settingsThatHaveChangedInFile;
        public static string CurrencySymbol
        {
            get => _currencySymbol;
            set => _currencySymbol = value;
        }
        public static bool IsFullVersion
        {
            get => _isFullVersion;
            set => _isFullVersion = value;
        }
        public static bool IsProgramLoading
        {
            get => _isProgramLoading;
            set => _isProgramLoading = value;
        }

        // Init.
        public MainMenu_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            CustomControls.ConstructControls();
            InitiateSearchTimer();

            CurrencySymbol = Currency.GetSymbol();

            SetCompanyLabel();

            _isProgramLoading = true;
            LoadData();
            LoadColumnHeader();
            UpdateTheme();
            _isProgramLoading = false;

            Sales_Button.PerformClick();
            SortTheDataGridViewByDate();

            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);

            SetToolTips();
            HideShowingResultsForLabel();
            AlignTotalLabels();
            UpdateTotals();
            MouseClickChartManager.InitCharts([Totals_Chart, Distribution_Chart, Profits_Chart]);
            InitTimeRangePanel();
        }
        private void SetToolTips()
        {
            CustomTooltip.SetToolTip(File_Button, "", "File");
            CustomTooltip.SetToolTip(Save_Button, "", "Save");
            CustomTooltip.SetToolTip(Help_Button, "", "Help");
            CustomTooltip.SetToolTip(Account_Button, "", "Account");
        }
        public void ResetData()
        {
            _categoryPurchaseList.Clear();
            _categorySaleList.Clear();

            _accountantList.Clear();
            _companyList.Clear();

            _purchase_DataGridView.Rows.Clear();
            _sale_DataGridView.Rows.Clear();

            Search_TextBox.Text = "";
            _sortFromDate = default;
            _sortToDate = default;
        }
        public void LoadData()
        {
            LoadCategoriesFromFile(Directories.CategoryPurchases_file, _categoryPurchaseList);
            LoadCategoriesFromFile(Directories.CategorySales_file, _categorySaleList);

            _accountantList = Directories.ReadAllLinesInFile(Directories.Accountants_file).ToList();
            _companyList = Directories.ReadAllLinesInFile(Directories.Companies_file).ToList();

            if (_purchase_DataGridView == null)
            {
                Size size = new(1300, 350);
                _purchase_DataGridView = new();
                DataGridViewManager.InitializeDataGridView(_purchase_DataGridView, "purchases_DataGridView", size, PurchaseColumnHeaders, null, this);
                _purchase_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;

                _sale_DataGridView = new();
                DataGridViewManager.InitializeDataGridView(_sale_DataGridView, "sales_DataGridView", size, SalesColumnHeaders, null, this);
                _sale_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;
                Theme.CustomizeScrollBar(_sale_DataGridView);
            }

            AddRowsFromFile(_purchase_DataGridView, SelectedOption.Purchases);
            AddRowsFromFile(_sale_DataGridView, SelectedOption.Sales);
        }
        private void LoadColumnHeader()
        {
            DataGridViewColumn chargedDifferenceColumn = _purchase_DataGridView.Columns[Column.ChargedDifference.ToString()];
            string existingHeaderText = chargedDifferenceColumn.HeaderText;
            string messageBoxText = "Having a charged difference is common and is usually due to taxes, duties, bank fees, exchange rate differences, or political and tax variations across countries.";
            chargedDifferenceColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);

            DataGridViewColumn totalColumn = _sale_DataGridView.Columns[Column.Total.ToString()];
            existingHeaderText = totalColumn.HeaderText;
            messageBoxText = "The revenue excludes shipping, taxes, and fees.";
            totalColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);
        }
        private static void LoadCategoriesFromFile(string filePath, List<Category> categoryList)
        {
            string json = Directories.ReadAllTextInFile(filePath);
            if (string.IsNullOrWhiteSpace(json)) { return; }

            List<Category>? loadedCategories = JsonConvert.DeserializeObject<List<Category>>(json);

            if (loadedCategories != null)
            {
                categoryList.AddRange(loadedCategories);
            }
        }
        private static void AddRowsFromFile(Guna2DataGridView dataGridView, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.CompanyName);
                return;
            }

            // Read the JSON content from the file
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            List<Dictionary<string, object>>? rowsData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            foreach (Dictionary<string, object> rowData in rowsData)
            {
                // Extract cell values
                List<string?> cellValuesList = [];
                foreach (KeyValuePair<string, object> kv in rowData)
                {
                    if (kv.Key != rowTagKey && kv.Key != noteTextKey)
                    {
                        cellValuesList.Add(kv.Value?.ToString());
                    }
                }
                string?[] cellValues = cellValuesList.ToArray();

                // Add the row values to the DataGridView
                int rowIndex = dataGridView.Rows.Add(cellValues);

                // Set the row tag
                if (rowData.TryGetValue(rowTagKey, out object value) && value is JObject jsonObject)
                {
                    Dictionary<string, object>? tagObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonObject.ToString());

                    if (tagObject != null)
                    {
                        // If the tagObject is a list of strings and TagData
                        if (tagObject.TryGetValue(itemsKey, out object? itemsElement) && itemsElement is JArray itemsArray)
                        {
                            List<string> itemList = itemsArray.Select(e => e.ToString()).ToList();

                            if (tagObject.TryGetValue(purchaseDataKey, out object? purchaseDataElement) && purchaseDataElement is JObject purchaseDataObject)
                            {
                                TagData? purchaseData = JsonConvert.DeserializeObject<TagData>(purchaseDataObject.ToString());

                                if (itemList != null && purchaseData != null)
                                {
                                    dataGridView.Rows[rowIndex].Tag = (itemList, purchaseData);
                                }
                            }
                        }
                        // If the tagObject is a string and TagData
                        else if (tagObject.TryGetValue(tagKey, out object? tagStringElement) && tagObject.TryGetValue(purchaseDataKey, out object? purchaseData1Element))
                        {
                            string? tagString = tagStringElement?.ToString();
                            TagData? purchaseData1 = JsonConvert.DeserializeObject<TagData>(purchaseData1Element?.ToString());

                            if (tagString != null && purchaseData1 != null)
                            {
                                dataGridView.Rows[rowIndex].Tag = (tagString, purchaseData1);
                            }
                        }
                        // If the tagObject is a TagData
                        else if (rowData.TryGetValue(rowTagKey, out object value1) && value1 is JObject jsonObject1)
                        {
                            // Try to deserialize the JObject directly into a TagData object
                            TagData? tagData = JsonConvert.DeserializeObject<TagData>(jsonObject1.ToString());

                            if (tagData != null)
                            {
                                dataGridView.Rows[rowIndex].Tag = tagData;
                            }
                        }
                    }
                }

                // Set the cell tag for the last cell if note_text key exists
                if (rowData.TryGetValue(noteTextKey, out object noteValue))
                {
                    DataGridViewCell lastCell = dataGridView.Rows[rowIndex].Cells[dataGridView.Columns.Count - 1];
                    lastCell.Value = ReadOnlyVariables.Show_text;
                    lastCell.Tag = noteValue;
                    DataGridViewManager.AddUnderlineToCell(lastCell);
                }
            }
        }
        public void LoadCharts(bool onlyLoadForLineCharts = false)
        {
            double total;
            bool isLine = LineGraph_ToggleSwitch.Checked;

            if (Selected == SelectedOption.Sales)
            {
                total = LoadChart.LoadTotalsIntoChart(_sale_DataGridView, Totals_Chart, isLine);
                Totals_Chart.Title.Text = $"Total revenue: {CurrencySymbol}{total:N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_sale_DataGridView, Distribution_Chart);
                    Distribution_Chart.Title.Text = "Distribution of revenue";
                }
            }
            else
            {
                total = LoadChart.LoadTotalsIntoChart(_purchase_DataGridView, Totals_Chart, isLine);
                Totals_Chart.Title.Text = $"Total expenses: {CurrencySymbol}{total:N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_purchase_DataGridView, Distribution_Chart);
                    Distribution_Chart.Title.Text = "Distribution of expenses";
                }
            }
            total = LoadChart.LoadProfitsIntoChart(_sale_DataGridView, _purchase_DataGridView, Profits_Chart, isLine);
            Profits_Chart.Title.Text = $"Total profits: {CurrencySymbol}{total:N2}";

            LanguageManager.UpdateLanguageForControl(Profits_Chart);
            LanguageManager.UpdateLanguageForControl(Totals_Chart);
            LanguageManager.UpdateLanguageForControl(Distribution_Chart);
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
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

            ShowingResultsFor_Label.ForeColor = CustomColors.text;
            ReselectedButton();

            Theme.SetThemeForControl([
                _sale_DataGridView,
                _purchase_DataGridView,
                Totals_Chart,
                Distribution_Chart,
                Profits_Chart,
                Total_Panel,
                countriesOfOrigin_Chart,
                companiesOfOrigin_Chart,
                countriesOfDestination_Chart,
                accountants_Chart
            ]);
        }
        public void ReselectedButton()
        {
            if (Purchases_Button.BorderThickness == 2)
            {
                Purchases_Button.BorderColor = CustomColors.accent_blue;
            }
            else if (Sales_Button.BorderThickness == 2)
            {
                Sales_Button.BorderColor = CustomColors.accent_blue;
            }
            else if (Statistics_Button.BorderThickness == 2)
            {
                Statistics_Button.BorderColor = CustomColors.accent_blue;
            }
        }
        private void SetAccessibleDescriptions()
        {
            CompanyName_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
            Profits_Chart.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Totals_Chart.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Quantity_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Shipping_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Tax_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            PaymentFee_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            ChargedDifference_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Price_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Form event handlers
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            _sale_DataGridView.ClearSelection();

            LoadingPanel.HideBlankLoadingPanel(this);

            // This is a bug with Guna Charts V.1.0.8. It has been fixed in V.1.0.9, but this version is broken.
            // So this can probably be removed in V.1.1.0.
            // This does not solve the problem, but it makes it better.
            BeginInvoke(() =>
            {
                Totals_Chart.Invalidate();
                Totals_Chart.Refresh();
                Distribution_Chart.Invalidate();
                Distribution_Chart.Refresh();
                Profits_Chart.Invalidate();
                Profits_Chart.Refresh();
            });

            Log.Write(2, "Argo Sales Tracker has finished starting");
        }
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            if (_instance == null) { return; }
            CustomControls.CloseAllPanels(null, null);
            CenterAndResizeControls();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Enter)
            {
                Guna2Panel[] panels = [
                    CustomControls.FileMenu,
                    CustomControls.HelpMenu,
                    CustomControls.AccountMenu,
                    CustomControls.ControlDropDown_Panel,
                    DataGridViewManager.RightClickDataGridView_Panel
                ];

                foreach (Guna2Panel panel in panels)
                {
                    if (Controls.Contains(panel))
                    {
                        HandlePanelkeyDown(panel, keyData);
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private static void HandlePanelkeyDown(Guna2Panel panel, Keys e)
        {
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
            IList results = flowPanel.Controls;

            if (results.Count == 0)
            {
                return;
            }

            // Select the next result
            bool isResultSelected = false;
            if (e is Keys.Down or Keys.Tab)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] is not Guna2Button) { continue; }

                    Guna2Button btn = (Guna2Button)results[i];

                    // Find the result that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the last one
                        if (i < results.Count - 1)
                        {
                            // Find the next button
                            for (int j = i + 1; j < results.Count; j++)
                            {
                                if (results[j] is Guna2Button nextBtn)
                                {
                                    nextBtn.BorderThickness = 1;
                                    isResultSelected = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Select the first button
                            Guna2Button firstBtn = (Guna2Button)results[0];
                            firstBtn.BorderThickness = 1;
                        }
                        break;
                    }
                }
            }
            else if (e is Keys.Up)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] is not Guna2Button) { continue; }

                    Guna2Button btn = (Guna2Button)results[i];

                    // Find the result that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the first one
                        if (i > 0)
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                if (results[j] is Guna2Button prevBtn)
                                {
                                    prevBtn.BorderThickness = 1;
                                    isResultSelected = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Select the last button
                            for (int j = results.Count - 1; j >= 0; j--)
                            {
                                if (results[j] is Guna2Button lastBtn)
                                {
                                    lastBtn.BorderThickness = 1;
                                    isResultSelected = true;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else if (e is Keys.Enter)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i] is Guna2Button btn && btn.BorderThickness == 1)
                    {
                        btn.PerformClick();
                        isResultSelected = true;
                        break;
                    }
                }
            }

            if (!isResultSelected)
            {
                // Select the first button
                Guna2Button firstBtn = (Guna2Button)results[0];
                firstBtn.BorderThickness = 1;
            }
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
                            CustomControls.SaveAll();
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
            Log.Write(2, "Closing Argo Sales Tracker");

            CustomControls.CloseAllPanels(null, null);

            // Save logs in file
            if (Directory.Exists(Directories.Logs_dir))
            {
                DateTime time = DateTime.Now;
                int count = 0;
                string directory;

                while (true)
                {
                    if (count == 0)
                    {
                        directory = $@"{Directories.Logs_dir}\{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}{ArgoFiles.TxtFileExtension}";
                    }
                    else
                    {
                        directory = $@"{Directories.Logs_dir}\{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}-{count}{ArgoFiles.TxtFileExtension}";
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
                    Log.Write(2, "Close canceled");
                    return;
                }
            }

            // Delete hidden directory
            Directories.DeleteDirectory(Directories.TempCompany_dir, true);
        }

        // Center and resize controls
        private bool wasControlsDropDownAdded;
        private void CenterAndResizeControls()
        {
            if (_isProgramLoading) { return; }

            if (ClientSize.Height > 1400)
            {
                Totals_Chart.Height = 500;
            }
            else if (ClientSize.Height > 1000)
            {
                Totals_Chart.Height = 400;
            }
            else
            {
                Totals_Chart.Height = 300;
            }

            byte spaceBetweenCharts = 20, chartWidthOffset = 35;

            int chartWidth = ClientSize.Width / 3 - chartWidthOffset;
            int chartHeight = Totals_Chart.Height;

            // Calculate X positions for charts
            int leftX = (ClientSize.Width - 3 * chartWidth - spaceBetweenCharts * 2) / 2;
            int middleX = leftX + chartWidth + spaceBetweenCharts;
            int rightX = middleX + chartWidth + spaceBetweenCharts;

            // Set positions
            Totals_Chart.Width = chartWidth;
            Totals_Chart.Left = leftX;

            Distribution_Chart.Size = new Size(chartWidth, chartHeight);
            Distribution_Chart.Left = middleX;

            Profits_Chart.Size = new Size(chartWidth, chartHeight);
            Profits_Chart.Left = rightX;

            _selectedDataGridView.Size = new Size(ClientSize.Width - 65, ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - Totals_Chart.Height - Totals_Chart.Top - 15);
            _selectedDataGridView.Location = new Point((ClientSize.Width - _selectedDataGridView.Width) / 2, ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - _selectedDataGridView.Height);

            Total_Panel.Location = new Point(_selectedDataGridView.Left, _selectedDataGridView.Top + _selectedDataGridView.Height);
            Total_Panel.Width = _selectedDataGridView.Width;

            if (ClientSize.Width < 1500 + Edit_Button.Left + Edit_Button.Width)
            {
                AddControlsDropDown();
                wasControlsDropDownAdded = true;
            }
            else if (wasControlsDropDownAdded)
            {
                RemoveControlsDropDown();
                wasControlsDropDownAdded = false;
            }

            if (Selected == SelectedOption.Statistics)
            {
                int availableHeight = ClientSize.Height - Purchases_Button.Bottom;

                int statChartHeight = availableHeight / 2 - 50;
                Size chartSize = new(chartWidth, statChartHeight);

                // Calculate the space between the charts
                int totalChartHeight = statChartHeight * 2;
                int spaceBetweenRows = (availableHeight - totalChartHeight) / 4;

                // Calculate Y positions
                int topRowY = Purchases_Button.Bottom + spaceBetweenRows;
                int bottomRowY = topRowY + statChartHeight + spaceBetweenRows;

                // Set positions for top row charts
                SetChartPosition(countriesOfOrigin_Chart, chartSize, leftX, topRowY);
                SetChartPosition(companiesOfOrigin_Chart, chartSize, middleX, topRowY);
                SetChartPosition(countriesOfDestination_Chart, chartSize, rightX, topRowY);

                // Set positions for bottom row charts
                SetChartPosition(accountants_Chart, chartSize, leftX, bottomRowY);
                SetChartPosition(salesVsExpenses_Chart, chartSize, middleX, bottomRowY);
                SetChartPosition(averageOrderValue_Chart, chartSize, rightX, bottomRowY);
            }
        }
        private static void SetChartPosition(GunaChart chart, Size size, int left, int top)
        {
            chart.Size = size;
            chart.Location = new Point(left, top);

            Label label = chart.Controls.OfType<Label>().FirstOrDefault(label => label.Tag.ToString() == ReadOnlyVariables.NoData_text);
            if (label != null)
            {
                CenterLabelInControl(label, chart);
            }
        }
        private void AddControlsDropDown()
        {
            CustomControls.ControlsDropDown_Button.Location = new Point(
                MainTop_Panel.Width - CustomControls.ControlsDropDown_Button.Width - 11,
                (MainTop_Panel.Height - CustomControls.ControlsDropDown_Button.Height) / 2);

            MainTop_Panel.Controls.Add(CustomControls.ControlsDropDown_Button);

            foreach (Control button in GetMainTopButtons())
            {
                button.Visible = false;
            };
        }
        private void RemoveControlsDropDown()
        {
            MainTop_Panel.Controls.Remove(CustomControls.ControlsDropDown_Button);

            foreach (Control button in GetMainTopButtons())
            {
                button.Visible = true;
            };
        }
        private Control[] GetMainTopButtons()
        {
            return [
                ManageAccountants_Button, ManageCategories_Button, ManageCompanies_Button,
                ManageProducts_Button, AddSale_Button, AddPurchase_Button];
        }

        /// <summary>
        /// Asks the user to save any changes.
        /// </summary>
        /// <returns>Returns true if the user cancels. Returns false if the user saves.</returns>
        private static bool AskUserToSaveBeforeClosing()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                "Save changes to the following items?",
                CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

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
                    // Cancel close
                    return true;
            }

            return false;
        }

        // Event handlers - top bar
        private void File_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(CustomControls.FileMenu))
            {
                Controls.Remove(CustomControls.FileMenu);
                File_Button.Image = Resources.FileGray;
            }
            else
            {
                CustomControls.CloseAllPanels(null, null);
                File_Button.Image = Resources.FileWhite;
                CustomControls.FileMenu.Location = new Point(File_Button.Left, Top_Panel.Height);
                Controls.Add(CustomControls.FileMenu);
                CustomControls.FileMenu.BringToFront();
                Focus();
            }
        }
        private void Save_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
            CustomControls.SaveAll();
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
            if (Controls.Contains(CustomControls.HelpMenu))
            {
                Controls.Remove(CustomControls.HelpMenu);
                Help_Button.Image = Resources.HelpGray;
            }
            else
            {
                CustomControls.CloseAllPanels(null, null);
                Help_Button.Image = Resources.HelpWhite;
                CustomControls.HelpMenu.Location = new Point(Help_Button.Left - CustomControls.HelpMenu.Width + Help_Button.Width, Top_Panel.Height);
                Controls.Add(CustomControls.HelpMenu);
                CustomControls.HelpMenu.BringToFront();
            }
        }
        private void Account_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(CustomControls.AccountMenu))
            {
                Controls.Remove(CustomControls.AccountMenu);
                Account_Button.Image = Resources.ProfileGray;
            }
            else
            {
                CustomControls.CloseAllPanels(null, null);
                Account_Button.Image = Resources.ProfileWhite;
                CustomControls.AccountMenu.Location = new Point(Account_Button.Left - CustomControls.AccountMenu.Width + Account_Button.Width, Top_Panel.Height);
                Controls.Add(CustomControls.AccountMenu);
                CustomControls.AccountMenu.BringToFront();
            }
        }

        // Event handlers
        private void Purchases_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            _purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Purchases;
            _selectedDataGridView = _purchase_DataGridView;
            _purchase_DataGridView.Visible = true;
            _sale_DataGridView.Visible = false;
            LanguageManager.UpdateLanguageForControl(Purchase_DataGridView);
            CenterAndResizeControls();
            RefreshDataGridView();

            UnselectButtons();
            SelectButton(Purchases_Button);

            _purchase_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            Search_TextBox.PlaceholderText = "Search for purchases";
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            _sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Sales;
            _selectedDataGridView = _sale_DataGridView;
            _sale_DataGridView.Visible = true;
            _purchase_DataGridView.Visible = false;
            CenterAndResizeControls();
            RefreshDataGridView();

            UnselectButtons();
            SelectButton(Sales_Button);

            _sale_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            Search_TextBox.PlaceholderText = "Search for sales";
        }
        private void Statistics_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ConstructControlsForStatistics();

            AddStatisticsControls();
            CenterAndResizeControls();

            UnselectButtons();
            SelectButton(Statistics_Button);
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
            CloseAllPanels(null, null);
            new Accountants_Form().ShowDialog();
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new Products_Form(true).ShowDialog();
        }
        private void ManageCompanies_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new Companies_Form().ShowDialog();
        }
        private void ManageCategories_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new Categories_Form(true).ShowDialog();
        }
        private void LineGraph_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            LoadCharts(true);
        }
        private void Edit_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CustomControls.Rename_TextBox.Text = CompanyName_Label.Text;
            CustomControls.Rename_TextBox.Font = CompanyName_Label.Font;
            Controls.Add(CustomControls.Rename_TextBox);
            CustomControls.Rename_TextBox.Location = new Point(CompanyName_Label.Left, CompanyName_Label.Top + CompanyName_Label.Parent.Top - 1);
            CustomControls.Rename_TextBox.Size = new Size(300, CompanyName_Label.Height + 2);
            CustomControls.Rename_TextBox.Focus();
            CustomControls.Rename_TextBox.SelectAll();
            CustomControls.Rename_TextBox.BringToFront();
            MainTop_Panel.Controls.Remove(Edit_Button);
            MainTop_Panel.Controls.Remove(CompanyName_Label);
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isProgramLoading) { return; }

            if (!timerRunning)
            {
                timerRunning = true;
                searchTimer.Start();
            }
        }

        // TimeRange
        private static Guna2Panel _timeRangePanel;
        public static Guna2Panel TimeRangePanel => _timeRangePanel;
        private static void InitTimeRangePanel()
        {
            DateRange_Form dateRange_Form = new();
            _timeRangePanel = dateRange_Form.Main_Panel;
            _timeRangePanel.BorderColor = CustomColors.controlPanelBorder;
            _timeRangePanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
        private void TimeRange_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(_timeRangePanel))
            {
                CloseDateRangePanel();
            }
            else
            {
                CloseAllPanels(null, null);

                // Set the location for the panel
                _timeRangePanel.Location = new Point(
                    TimeRange_Button.Right - _timeRangePanel.Width,
                    TimeRange_Button.Bottom);

                Controls.Add(_timeRangePanel);
                _timeRangePanel.BringToFront();
            }
        }
        public void CloseDateRangePanel()
        {
            Controls.Remove(_timeRangePanel);
        }

        // Timer for loading the charts
        private Timer searchTimer;
        private bool timerRunning = false;
        private void InitiateSearchTimer()
        {
            searchTimer = new()
            {
                Interval = 300
            };
            searchTimer.Tick += SearchTimer_Tick;
        }
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            timerRunning = false;

            SortDataGridView();
        }

        // Methods for Event handlers
        private static void SelectButton(Guna2Button button)
        {
            button.BorderThickness = 2;
            button.BorderColor = CustomColors.accent_blue;
        }
        private void UnselectButtons()
        {
            Purchases_Button.BorderThickness = 1;
            Sales_Button.BorderThickness = 1;
            Statistics_Button.BorderThickness = 1;
            Purchases_Button.BorderColor = CustomColors.controlBorder;
            Sales_Button.BorderColor = CustomColors.controlBorder;
            Statistics_Button.BorderColor = CustomColors.controlBorder;
        }

        /// <summary>
        /// If there is no data, then it adds a Label to the control.
        /// </summary>
        /// <returns>True if there is any data, False if there is no data.</returns>
        public static bool ManageNoDataLabelOnControl(bool hasData, Control control)
        {
            string text = ReadOnlyVariables.NoData_text;
            Label existingLabel = control.Controls.OfType<Label>().FirstOrDefault(label => label.Tag.ToString() == text);

            if (!hasData)
            {
                string textWithoutWhitespace = string.Concat(text.Where(c => !char.IsWhiteSpace(c)));

                // If there's no data and the label doesn't exist, create and add it
                if (existingLabel == null)
                {
                    Label label = new()
                    {
                        Font = new Font("Segoe UI", 12),
                        ForeColor = CustomColors.text,
                        Text = text,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Tag = text,
                        Anchor = AnchorStyles.Top,
                        Name = $"{textWithoutWhitespace}_Label"  // This is needed for the language translation
                    };

                    control.Controls.Add(label);
                    CenterLabelInControl(label, control);

                    control.Resize += delegate { CenterLabelInControl(label, control); };

                    label.BringToFront();
                }
                return false;
            }
            else
            {
                // If there's data and the label exists, remove it
                if (existingLabel != null)
                {
                    control.Controls.Remove(existingLabel);
                    existingLabel.Dispose();
                }
                return true;
            }
        }
        private static void CenterLabelInControl(Label label, Control parent)
        {
            if (label != null && parent != null)
            {
                label.Location = new Point((parent.Width - label.Width) / 2, (parent.Height - label.Height) / 2);
            }
        }

        // Company label
        public void RenameCompany()
        {
            if (!Controls.Contains(CustomControls.Rename_TextBox))
            {
                return;
            }

            // If the company name already exists in this directory
            string parentDir = Directory.GetParent(Directories.ArgoCompany_file).FullName;
            string filePath = parentDir + @"\" + CustomControls.Rename_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension;
            string fileName = Path.GetFileName(filePath);
            List<string> files = Directory.GetFiles(parentDir).ToList();

            bool fileExists = files.Any(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (fileExists)
            {
                // Get a new name for the file
                string name = Path.GetFileNameWithoutExtension(filePath);
                List<string> fileNames = Directories.GetListOfAllFilesWithoutExtensionInDirectory(parentDir);

                string suggestedCompanyName = Tools.AddNumberForAStringThatAlreadyExists(name, fileNames);

                CustomMessageBoxResult result = CustomMessageBox.Show($"Rename company",
                    $"Do you want to rename '{name}' to '{suggestedCompanyName}'? There is already a company with the same name.",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.OkCancel);

                if (result == CustomMessageBoxResult.Ok)
                {
                    CustomControls.Rename_TextBox.Text = suggestedCompanyName;
                }
                else { return; }
            }

            Controls.Remove(CustomControls.Rename_TextBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);

            // If the name did not change
            if (CustomControls.Rename_TextBox.Text == CompanyName_Label.Text)
            {
                return;
            }

            CompanyName_Label.Text = CustomControls.Rename_TextBox.Text;
            ArgoCompany.Rename(CustomControls.Rename_TextBox.Text);
            CustomControls.Rename_TextBox.Text = "";
            MoveEditButton();
            CenterAndResizeControls();

            UpdateMainMenuFormText(this);
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"Renamed program: {CompanyName_Label.Text}");
        }
        public void SetCompanyLabel()
        {
            CompanyName_Label.Text = Directories.CompanyName;
            MoveEditButton();
        }
        private void MoveEditButton()
        {
            Edit_Button.Left = CompanyName_Label.Left + CompanyName_Label.Width + 5;
        }

        // Search DataGridView properties
        private DateTime? _sortFromDate = null, _sortToDate = null;
        private TimeSpan? _sortTimeSpan = TimeSpan.MaxValue;

        // Search DataGridView getters
        public DateTime? SortFromDate
        {
            get => _sortFromDate;
            set => _sortFromDate = value;
        }
        public DateTime? SortToDate
        {
            get => _sortToDate;
            set => _sortToDate = value;
        }
        public TimeSpan? SortTimeSpan
        {
            get => _sortTimeSpan;
            set => _sortTimeSpan = value;
        }

        // Search DataGridView methods
        public void SortDataGridView()
        {
            ApplyFilters();
            LoadCharts();
            UpdateTotals();
        }
        private void ApplyFilters()
        {
            if (_sortFromDate != null && _sortToDate != null)
            {
                FilterDataGridViewByDateRange(_selectedDataGridView);
            }

            bool filterExists = _sortTimeSpan != TimeSpan.MaxValue ||
                !string.IsNullOrEmpty(Search_TextBox.Text.Trim());

            bool hasVisibleRows = false;

            if (filterExists)
            {
                ShowShowingResultsForLabel();

                foreach (DataGridViewRow row in _selectedDataGridView.Rows)
                {
                    DateTime rowDate = DateTime.Parse(row.Cells[SalesColumnHeaders[Column.Date]].Value.ToString());

                    bool isVisibleByDate = (_sortTimeSpan == TimeSpan.MaxValue ||
                        rowDate >= DateTime.Now - _sortTimeSpan);

                    bool isVisibleBySearch = row.Cells.Cast<DataGridViewCell>()
                        .Any(cell => cell.Value != null &&
                            cell.Value.ToString().Contains(Search_TextBox.Text, StringComparison.OrdinalIgnoreCase));

                    // Row is visible only if it matches both the date filter and the search filter
                    row.Visible = isVisibleByDate && isVisibleBySearch;

                    if (row.Visible)
                    {
                        hasVisibleRows = true;
                    }
                }
            }
            else
            {
                HideShowingResultsForLabel();

                foreach (DataGridViewRow row in _selectedDataGridView.Rows)
                {
                    row.Visible = true;
                    hasVisibleRows = true;
                }
            }

            ManageNoDataLabelOnControl(hasVisibleRows, _selectedDataGridView);
        }
        private void FilterDataGridViewByDateRange(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DateTime rowDate = DateTime.Parse(row.Cells[PurchaseColumnHeaders[Column.Date]].Value.ToString());
                bool isVisible = rowDate >= _sortFromDate && rowDate <= _sortToDate;
                row.Visible = isVisible;
            }
        }
        private void ShowShowingResultsForLabel()
        {
            string text = "Showing results for";
            string searchText = Search_TextBox.Text.Trim();

            // Append search text if available
            if (!string.IsNullOrEmpty(searchText))
            {
                text += $" '{searchText}'";
            }

            // Handle time span case
            if (_sortTimeSpan != null && _sortTimeSpan != TimeSpan.MaxValue)
            {
                string timeSpanText = GetTimeSpanText(_sortTimeSpan.Value);

                if (!string.IsNullOrEmpty(searchText))
                {
                    text += $"\nin the last {timeSpanText}";
                }
                else
                {
                    text = $"Showing results for\nthe last {timeSpanText}";
                }
            }

            // Handle custom date range case
            if (_sortFromDate != null && _sortToDate != null)
            {
                string fromDate = Tools.FormatDate((DateTime)_sortFromDate);
                string toDate = Tools.FormatDate((DateTime)_sortToDate);

                if (!string.IsNullOrEmpty(searchText))
                {
                    text += $"\nfrom {fromDate} to {toDate}";
                }
                else
                {
                    text = $"Showing results from\n{fromDate} to {toDate}";
                }
            }

            // Update label text and location
            ShowingResultsFor_Label.Text = text;
            ShowingResultsFor_Label.Location = new Point(
                (ClientSize.Width - ShowingResultsFor_Label.Width) / 2,
                MainTop_Panel.Bottom + (Distribution_Chart.Top - MainTop_Panel.Bottom - ShowingResultsFor_Label.Height) / 2);

            ShowingResultsFor_Label.Visible = true;
        }
        /// <summary>
        /// Helper function to convert TimeSpan into human-readable text
        /// </summary>
        private static string? GetTimeSpanText(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.MaxValue) { return "All Time"; }
            if (timeSpan == TimeSpan.FromDays(1)) { return "1 day"; }
            if (timeSpan == TimeSpan.FromDays(2)) { return "2 days"; }
            if (timeSpan == TimeSpan.FromDays(3)) { return "3 days"; }
            if (timeSpan == TimeSpan.FromDays(5)) { return "5 days"; }
            if (timeSpan == TimeSpan.FromDays(10)) { return "10 days"; }
            if (timeSpan == TimeSpan.FromDays(30)) { return "30 days"; }
            if (timeSpan == TimeSpan.FromDays(100)) { return "100 days"; }
            if (timeSpan == TimeSpan.FromDays(365)) { return "1 year"; }
            if (timeSpan == TimeSpan.FromDays(365 * 2)) { return "2 years"; }
            if (timeSpan == TimeSpan.FromDays(365 * 5)) { return "5 years"; }

            return null;
        }
        public void HideShowingResultsForLabel()
        {
            ShowingResultsFor_Label.Visible = false;
        }
        private void SortTheDataGridViewByDate()
        {
            string dateColumnHeader = SalesColumnHeaders[Column.Date];
            _sale_DataGridView.Sort(_sale_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);

            dateColumnHeader = PurchaseColumnHeaders[Column.Date];
            _purchase_DataGridView.Sort(_purchase_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);
        }

        // List properties
        private readonly List<Category> _categorySaleList = [];
        private readonly List<Category> _categoryPurchaseList = [];
        private List<string> _accountantList = [];
        private List<string> _companyList = [];

        // List getters
        public List<Category> CategorySaleList => _categorySaleList;
        public List<Category> CategoryPurchaseList => _categoryPurchaseList;
        public List<string> AccountantList => _accountantList;
        public List<string> CompanyList => _companyList;

        // List methods
        public List<string> GetCategorySaleNames()
        {
            return _categorySaleList.Select(s => s.Name).ToList();
        }
        public List<string> GetCategoryPurchaseNames()
        {
            return _categoryPurchaseList.Select(p => p.Name).ToList();
        }
        public List<string> GetCategoryAndProductSaleNames()
        {
            List<string> names = [];

            foreach (Category category in _categorySaleList)
            {
                foreach (Product product in category.ProductList)
                {
                    names.Add($"{category.Name} > {product.Name}");
                }
            }
            return names;
        }
        public List<string> GetCategoryAndProductPurchaseNames()
        {
            List<string> names = [];

            foreach (Category category in _categoryPurchaseList)
            {
                foreach (Product product in category.ProductList)
                {
                    names.Add($"{category.Name} > {product.Name}");
                }
            }
            return names;
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
        public static string? GetCountryProductIsFrom(List<Category> categoryList, string productName)
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
            return null;
        }
        public static string? GetCompanyProductIsFrom(List<Category> categoryList, string productName)
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
            return null;
        }
        public static string? GetCategoryNameProductIsFrom(List<Category> categoryList, string productName)
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
        public static bool DoesProductExist(string productName, List<Category> categories)
        {
            foreach (Category category in categories)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name == productName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static Category? GetCategoryCategoryNameIsFrom(List<Category> categoryList, string categoryName)
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

        // DataGridView properties
        public SelectedOption Selected;
        private Guna2DataGridView _purchase_DataGridView, _sale_DataGridView, _selectedDataGridView;
        public enum SelectedOption
        {
            Purchases,
            Sales,
            ProductPurchases,
            ProductSales,
            CategoryPurchases,
            CategorySales,
            Accountants,
            Receipts,
            Companies,
            Statistics,
            ItemsInPurchase,
            ItemsInSale
        }
        public enum Column
        {
            OrderNumber,
            Name,
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
            Discount,
            ChargedDifference,
            Total,
            Note
        }
        public readonly Dictionary<Column, string> PurchaseColumnHeaders = new()
        {
            { Column.OrderNumber, "Order #" },
            { Column.Name, "Accountant" },
            { Column.Product, "Product" },
            { Column.Category, "Category" },
            { Column.Country, "Country of origin" },
            { Column.Company, "Company of origin" },
            { Column.Date, "Date" },
            { Column.Quantity, "Quantity" },
            { Column.PricePerUnit, "Price per unit" },
            { Column.Shipping, "Shipping" },
            { Column.Tax, "Tax" },
            { Column.Fee, "Fee" },
            { Column.Discount, "Discount" },
            { Column.ChargedDifference, "Charged difference" },
            { Column.Total, "Total expenses" },
            { Column.Note, "Notes" }
        };
        public readonly Dictionary<Column, string> SalesColumnHeaders = new()
        {
            { Column.OrderNumber, "Sale #" },
            { Column.Name, "Accountant" },
            { Column.Product, "Product" },
            { Column.Category, "Category" },
            { Column.Country, "Country of destination" },
            { Column.Company, "Company of origin" },
            { Column.Date, "Date" },
            { Column.Quantity, "Quantity" },
            { Column.PricePerUnit, "Price per unit" },
            { Column.Shipping, "Shipping" },
            { Column.Tax, "Tax" },
            { Column.Fee, "Fee" },
            { Column.Discount, "Discount" },
            { Column.ChargedDifference, "Charged difference" },
            { Column.Total, "Total revenue" },
            { Column.Note, "Notes" }
        };
        public enum DataGridViewTag
        {
            SaleOrPurchase,
            Category,
            Company,
            Product,
            Accountant,
            ItemsInPurchase
        }

        // DataGridView getters and setters
        public Guna2DataGridView Purchase_DataGridView => _purchase_DataGridView;
        public Guna2DataGridView Sale_DataGridView => _sale_DataGridView;
        public Guna2DataGridView SelectedDataGridView
        {
            get => _selectedDataGridView;
            set => _selectedDataGridView = value;
        }

        // Total labels
        public void AlignTotalLabels()
        {
            string quantityColumn = Column.Quantity.ToString();
            string taxColumn = Column.Tax.ToString();
            string shippingColumn = Column.Shipping.ToString();
            string feeColumn = Column.Fee.ToString();
            string chargedDifference = Column.ChargedDifference.ToString();
            string totalPriceColumn = Column.Total.ToString();

            Quantity_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = _selectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = _selectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = _selectedDataGridView.Columns[shippingColumn].Width;

            PaymentFee_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[feeColumn].Index, -1, true).Left;
            PaymentFee_Label.Width = _selectedDataGridView.Columns[feeColumn].Width;

            if (_selectedDataGridView == _purchase_DataGridView)
            {
                Total_Panel.Controls.Add(ChargedDifference_Label);
                ChargedDifference_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[chargedDifference].Index, -1, true).Left;
                ChargedDifference_Label.Width = _selectedDataGridView.Columns[chargedDifference].Width;
            }
            else
            {
                Total_Panel.Controls.Remove(ChargedDifference_Label);
            }

            Price_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = _selectedDataGridView.Columns[totalPriceColumn].Width;
        }
        public void UpdateTotals()
        {
            if (Selected != SelectedOption.Purchases && Selected != SelectedOption.Sales)
            {
                return;
            }

            if (!DataGridViewManager.HasVisibleRows(_selectedDataGridView))
            {
                Total_Panel.Visible = false;
            }
            else
            {
                Total_Panel.Visible = true;
            }

            int totalQuantity = 0;
            decimal totalTax = 0;
            decimal totalShipping = 0;
            decimal fee = 0;
            decimal chargedDifference = 0;
            decimal totalPrice = 0;

            foreach (DataGridViewRow row in _selectedDataGridView.Rows)
            {
                if (!row.Visible) { continue; }

                totalQuantity += Convert.ToInt32(row.Cells[Column.Quantity.ToString()].Value);
                totalTax += Convert.ToDecimal(row.Cells[Column.Tax.ToString()].Value);
                totalShipping += Convert.ToDecimal(row.Cells[Column.Shipping.ToString()].Value);
                fee += Convert.ToDecimal(row.Cells[Column.Fee.ToString()].Value);
                if (Selected == SelectedOption.Purchases)
                {
                    chargedDifference += Convert.ToDecimal(row.Cells[Column.ChargedDifference.ToString()].Value);
                }
                totalPrice += Convert.ToDecimal(row.Cells[Column.Total.ToString()].Value);
            }

            Quantity_Label.Text = totalQuantity.ToString();
            Tax_Label.Text = totalTax.ToString("C");
            Shipping_Label.Text = totalShipping.ToString("C");
            PaymentFee_Label.Text = fee.ToString("C");
            ChargedDifference_Label.Text = chargedDifference.ToString("C");
            Price_Label.Text = totalPrice.ToString("C");
        }

        // Save to file
        public static void SaveDataGridViewToFileAsJson(Guna2DataGridView dataGridView, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);
            List<Dictionary<string, object>> rowsData = [];

            // Collect data from the DataGridView
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                Dictionary<string, object> rowData = [];

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value?.ToString() == ReadOnlyVariables.Show_text && cell.Tag != null)
                    {
                        rowData[noteTextKey] = cell.Tag;
                    }
                    else
                    {
                        rowData[cell.OwningColumn.Name] = cell.Value;
                    }
                }

                // Add the row tag
                if (row.Tag is (List<string> tagList, TagData purchaseData))
                {
                    rowData[rowTagKey] = new
                    {
                        Items = tagList,
                        PurchaseData = purchaseData
                    };
                }
                else if (row.Tag is (string tagString, TagData purchaseData1))
                {
                    rowData[rowTagKey] = new
                    {
                        Tag = tagString,
                        PurchaseData = purchaseData1
                    };
                }
                else if (row.Tag is TagData purchaseData2)
                {
                    rowData[rowTagKey] = new
                    {
                        PurchaseData = purchaseData2
                    };
                }

                rowsData.Add(rowData);
            }

            string json = JsonConvert.SerializeObject(rowsData, Formatting.Indented);
            Directories.WriteTextToFile(filePath, json);
        }
        public void SaveCategoriesToFile(SelectedOption option)
        {
            if (_isProgramLoading)
            {
                return;
            }

            string filePath = DataGridViewManager.GetFilePathForDataGridView(option);

            List<Category> categoryList;
            if (option == SelectedOption.CategoryPurchases || option == SelectedOption.ProductPurchases)
            {
                categoryList = _categoryPurchaseList;
            }
            else
            {
                categoryList = _categorySaleList;
            }

            string json = JsonConvert.SerializeObject(categoryList, Formatting.Indented);
            Directories.WriteTextToFile(filePath, json);
        }
        public static void SaveDataGridViewToFile(Guna2DataGridView dataGridView, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);
            List<string> linesInDataGridView = [];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string cellValue = row.Cells[0].Value.ToString();
                    linesInDataGridView.Add(cellValue);
                }
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);
        }
        public static void SaveListToFile(List<string> list, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);

            Directories.WriteLinesToFile(filePath, list);
        }

        // Statistics menu properties
        private List<GunaChart> statisticsCharts;
        private GunaChart countriesOfOrigin_Chart, companiesOfOrigin_Chart, countriesOfDestination_Chart,
            accountants_Chart, salesVsExpenses_Chart, averageOrderValue_Chart;

        // Statistics menu methods
        private List<Control> GetMainControlsList()
        {
            return [
                _sale_DataGridView,
                _purchase_DataGridView,
                Totals_Chart,
                Distribution_Chart,
                Profits_Chart,
                Total_Panel
            ];
        }
        private void AddMainControls()
        {
            if (Selected != SelectedOption.Statistics)
            {
                return;
            }

            foreach (Control control in GetMainControlsList())
            {
                control.Visible = true;
            }
            foreach (Control control in statisticsCharts)
            {
                control.Visible = false;
            }

            CenterAndResizeControls();
        }

        /// <summary>
        /// Constructs statistics charts if they have not been constructed already.
        /// </summary>
        private void ConstructControlsForStatistics()
        {
            if (countriesOfOrigin_Chart != null)
            {
                return;
            }

            countriesOfOrigin_Chart = ConstructStatisticsChart(250, "Countries of origin for purchased products", "countriesOfOrigin_Chart");
            companiesOfOrigin_Chart = ConstructStatisticsChart(250, "Companies of origin for purchased products", "companiesOfOrigin_Chart");
            countriesOfDestination_Chart = ConstructStatisticsChart(250, "Countries of destination for sold products", "countriesOfDestination_Chart");
            accountants_Chart = ConstructStatisticsChart(800, "Transactions managed by accountants", "accountants_Chart");
            salesVsExpenses_Chart = ConstructStatisticsChart(800, "Total sales vs. total expenses", "salesVsExpenses_Chart");
            averageOrderValue_Chart = ConstructStatisticsChart(800, "Average order value", "averageOrderValue_Chart");

            statisticsCharts =
            [
                countriesOfOrigin_Chart,
                companiesOfOrigin_Chart,
                countriesOfDestination_Chart,
                accountants_Chart,
                salesVsExpenses_Chart,
                averageOrderValue_Chart
            ];

            MouseClickChartManager.InitCharts(statisticsCharts.ToArray());
        }
        private GunaChart ConstructStatisticsChart(int top, string title, string name)
        {
            GunaChart gunaChart = new()
            {
                Top = top,
                Name = name,  // This is needed for the language translation
                Height = 500
            };

            gunaChart.ApplyConfig(ChartColors.Config(), CustomColors.background4);
            LoadChart.ConfigureChartForPie(gunaChart);
            gunaChart.Title.Text = title;
            gunaChart.Title.Display = true;
            gunaChart.Title.Font = new ChartFont("Segoe UI", 20, ChartFontStyle.Bold);
            gunaChart.Legend.LabelFont = new ChartFont("Segoe UI", 18);
            gunaChart.Tooltips.TitleFont = new ChartFont("Segoe UI", 18, ChartFontStyle.Bold);
            gunaChart.Tooltips.BodyFont = new ChartFont("Segoe UI", 18);
            gunaChart.XAxes.Ticks.Font = new("Segoe UI", 18);
            gunaChart.YAxes.Ticks.Font = new("Segoe UI", 18);
            Controls.Add(gunaChart);

            return gunaChart;
        }
        private void AddStatisticsControls()
        {
            if (Selected == SelectedOption.Statistics)
            {
                return;
            }
            Selected = SelectedOption.Statistics;

            foreach (Control control in statisticsCharts)
            {
                control.Visible = true;
            }
            foreach (Control control in GetMainControlsList())
            {
                control.Visible = false;
            }

            UpdateStatisticsCharts();
            CenterAndResizeControls();
        }
        private void UpdateStatisticsCharts()
        {
            bool isLineChart = LineGraph_ToggleSwitch.Checked;

            LoadChart.LoadCountriesOfOriginForProductsIntoChart(_purchase_DataGridView, countriesOfOrigin_Chart);
            LoadChart.LoadCompaniesOfOriginForProductsIntoChart(_purchase_DataGridView, companiesOfOrigin_Chart);
            LoadChart.LoadCountriesOfDestinationForProductsIntoChart(_sale_DataGridView, countriesOfDestination_Chart);
            LoadChart.LoadAccountantsIntoChart([_purchase_DataGridView, _sale_DataGridView], accountants_Chart);
            LoadChart.LoadSalesVsExpensesChart(_purchase_DataGridView, _sale_DataGridView, salesVsExpenses_Chart, isLineChart);
            LoadChart.LoadAverageOrderValueChart(_sale_DataGridView, averageOrderValue_Chart, isLineChart);

            LanguageManager.UpdateLanguageForControl(countriesOfOrigin_Chart);
            LanguageManager.UpdateLanguageForControl(companiesOfOrigin_Chart);
            LanguageManager.UpdateLanguageForControl(countriesOfDestination_Chart);
            LanguageManager.UpdateLanguageForControl(accountants_Chart);
            LanguageManager.UpdateLanguageForControl(salesVsExpenses_Chart);
            LanguageManager.UpdateLanguageForControl(averageOrderValue_Chart);
        }

        // Settings
        private Settings_Form settingsForm;
        public void OpenSettingsMenu()
        {
            if (!Tools.IsFormOpen(typeof(Settings_Form)))
            {
                settingsForm = new Settings_Form();
                settingsForm.Show();
            }
            else { settingsForm.BringToFront(); }
        }

        // Logs
        private Log_Form logForm;
        public void OpenLogs()
        {
            if (!Tools.IsFormOpen(typeof(Log_Form)))
            {
                logForm = new Log_Form();
                logForm.Show();
            }
            else
            {
                logForm.BringToFront();
            }
        }

        // Misc.
        public static List<Guna2Panel> GetMenus()
        {
            return [
                CustomControls.FileMenu,
                CustomControls.HelpMenu,
                CustomControls.AccountMenu,
                CustomControls.ControlDropDown_Panel,
                GetStarted_Form.RightClickOpenRecent_Panel,
                DataGridViewManager.RightClickDataGridView_Panel
            ];
        }
        public void RefreshDataGridView()
        {
            ApplyFilters();
            LoadCharts();
            UpdateTotals();
            _selectedDataGridView.ClearSelection();
        }
        public static void UpdateMainMenuFormText(Form instance)
        {
            instance.Text = $"Argo Sales Tracker {Tools.GetVersionNumber()} - {Directories.CompanyName}";
        }
        public void ClosePanels()
        {
            DataGridViewManager.ControlRightClickPanelWasAddedTo?.Controls.Remove(DataGridViewManager.RightClickDataGridView_Panel);
            Controls.Remove(RightClickGunaChartMenu.RightClickGunaChart_Panel);
            DataGridViewManager.DoNotDeleteRows = false;
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}