using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings;
using Sales_Tracker.UI;
using System.Collections;
using System.ComponentModel;
using System.Text.Json;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Proprties
        private static MainMenu_Form _instance;
        private static List<string> _thingsThatHaveChangedInFile = [];
        private static List<string> _settingsThatHaveChangedInFile = [];
        public DateTime fromDate, toDate;
        private static string _currencySymbol;
        private static bool _isFullVersion = true;
        private static readonly string noteTextKey = "note", rowTagKey = "RowTag", itemsKey = "Items", purchaseDataKey = "PurchaseData", tagKey = "Tag";

        // Getters and setters
        public static MainMenu_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile
        {
            get => _thingsThatHaveChangedInFile;
            private set => _thingsThatHaveChangedInFile = value;
        }
        public static List<string> SettingsThatHaveChangedInFile
        {
            get => _settingsThatHaveChangedInFile;
            private set => _settingsThatHaveChangedInFile = value;
        }
        public static bool IsFullVersion
        {
            get => _isFullVersion;
            set => _isFullVersion = value;
        }
        public static string CurrencySymbol
        {
            get => _currencySymbol;
            set => _currencySymbol = value;
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

            isProgramLoading = true;
            LoadData();
            LoadColumnHeader();
            UpdateTheme();
            isProgramLoading = false;

            Sales_Button.PerformClick();
            SortTheDataGridViewByDate();
            HideShowingResultsForLabel();

            SetDoNotTranslateControls();
            LanguageManager.UpdateLanguageForForm(this);
        }
        public void ResetData()
        {
            _categoryPurchaseList.Clear();
            _categorySaleList.Clear();

            _accountantList.Clear();
            _companyList.Clear();

            _purchases_DataGridView.Rows.Clear();
            _sales_DataGridView.Rows.Clear();

            Search_TextBox.Text = "";
            Filter_ComboBox.SelectedIndex = 0;
            Filter_ComboBox.Enabled = true;
            fromDate = default;
            toDate = default;
        }
        public void LoadData()
        {
            LoadCategoriesFromFile(Directories.CategoryPurchases_file, _categoryPurchaseList);
            LoadCategoriesFromFile(Directories.CategorySales_file, _categorySaleList);

            _accountantList = Directories.ReadAllLinesInFile(Directories.Accountants_file).ToList();
            _companyList = Directories.ReadAllLinesInFile(Directories.Companies_file).ToList();

            if (_purchases_DataGridView == null)
            {
                Size size = new(1300, 350);
                _purchases_DataGridView = new Guna2DataGridView();
                DataGridViewManager.InitializeDataGridView(_purchases_DataGridView, size, PurchaseColumnHeaders);
                _purchases_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;

                _sales_DataGridView = new Guna2DataGridView();
                DataGridViewManager.InitializeDataGridView(_sales_DataGridView, size, SalesColumnHeaders);
                _sales_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;
                Theme.CustomizeScrollBar(_sales_DataGridView);
            }

            AddRowsFromFile(_purchases_DataGridView, SelectedOption.Purchases);
            AddRowsFromFile(_sales_DataGridView, SelectedOption.Sales);

            AddTimeRangesIntoComboBox();
        }
        private void LoadColumnHeader()
        {
            DataGridViewColumn chargedDifferenceColumn = _purchases_DataGridView.Columns[Column.ChargedDifference.ToString()];
            string existingHeaderText = chargedDifferenceColumn.HeaderText;
            string messageBoxText = "Having a charged difference is common and is usually due to taxes, duties, bank fees, exchange rate differences, or political and tax variations across countries.";
            chargedDifferenceColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);

            DataGridViewColumn totalColumn = _sales_DataGridView.Columns[Column.Total.ToString()];
            existingHeaderText = totalColumn.HeaderText;
            messageBoxText = "The revenue excludes shipping, taxes, and fees.";
            totalColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);
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
        private void AddTimeRangesIntoComboBox()
        {
            Filter_ComboBox.Items.AddRange(timeIntervals.Select(ti => ti.displayString).ToArray());
            Filter_ComboBox.SelectedIndex = 0;
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

            List<Dictionary<string, object>>? rowsData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);

            foreach (Dictionary<string, object> rowData in rowsData)
            {
                // Extract cell values
                List<string?> cellValuesList = new();
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
                if (rowData.TryGetValue(rowTagKey, out object value) && value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    Dictionary<string, object>? tagObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());

                    if (tagObject != null)
                    {
                        // If the tagObject is a list of strings and TagData
                        if (tagObject.TryGetValue(itemsKey, out object? itemsElement) &&
                            itemsElement is JsonElement itemsJsonElement &&
                            itemsJsonElement.ValueKind == JsonValueKind.Array)
                        {
                            List<string?> itemList = itemsJsonElement.EnumerateArray().Select(e => e.GetString()).ToList();

                            if (tagObject.TryGetValue(purchaseDataKey, out object? purchaseDataElement) && purchaseDataElement is JsonElement purchaseDataJsonElement)
                            {
                                TagData? purchaseData = JsonSerializer.Deserialize<TagData>(purchaseDataJsonElement.GetRawText());

                                if (itemList != null && purchaseData != null)
                                {
                                    dataGridView.Rows[rowIndex].Tag = (itemList, purchaseData);
                                }
                            }
                        }
                        // If the tagObject is a string and TagData
                        else if (tagObject.TryGetValue(tagKey, out object? tagStringElement) &&
                            tagObject.TryGetValue(purchaseDataKey, out object? purchaseData1Element))
                        {
                            string? tagString = tagStringElement?.ToString();
                            TagData? purchaseData1 = JsonSerializer.Deserialize<TagData>(purchaseData1Element?.ToString());

                            if (tagString != null && purchaseData1 != null)
                            {
                                dataGridView.Rows[rowIndex].Tag = (tagString, purchaseData1);
                            }
                        }
                        // If the tagObject is a TagData
                        else if (rowData.TryGetValue(rowTagKey, out object value1) &&
                          value1 is JsonElement jsonElement1 &&
                          jsonElement1.ValueKind == JsonValueKind.Object)
                        {
                            // Try to deserialize the JsonElement directly into a TagData object
                            TagData? tagData = JsonSerializer.Deserialize<TagData>(jsonElement1.GetRawText());

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
                total = LoadChart.LoadTotalsIntoChart(_sales_DataGridView, Totals_Chart, isLine);
                Totals_Chart.Title.Text = $"Total revenue: {CurrencySymbol}{total:N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_sales_DataGridView, Distribution_Chart);
                    Distribution_Chart.Title.Text = "Distribution of revenue";
                }
            }
            else
            {
                total = LoadChart.LoadTotalsIntoChart(_purchases_DataGridView, Totals_Chart, isLine);
                Totals_Chart.Title.Text = $"Total expenses: {CurrencySymbol}{total:N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_purchases_DataGridView, Distribution_Chart);
                    Distribution_Chart.Title.Text = "Distribution of expenses";
                }
            }
            total = LoadChart.LoadProfitsIntoChart(_sales_DataGridView, _purchases_DataGridView, Profits_Chart, isLine);
            Profits_Chart.Title.Text = $"Total profits: {CurrencySymbol}{total:N2}";
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
                _sales_DataGridView,
                _purchases_DataGridView,
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
        private void SetDoNotTranslateControls()
        {
            CompanyName_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
        }

        // Form event handlers
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            _sales_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);

            // Ensure the charts are rendered
            // BeginInvoke ensures that it runs on the main UI thread after all pending UI events have been processed
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
            ResizeControls();
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
            Log.Write(2, "Closing Argo Studio");

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

        // Resize controls
        private bool wasControlsDropDownAdded;
        private void ResizeControls()
        {
            if (isProgramLoading) { return; }

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

                int statChartHeight = availableHeight / 2 - chartWidthOffset * 3;
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
        }
        private void AddControlsDropDown()
        {
            CustomControls.controlsDropDown_Button.Location = new Point(MainTop_Panel.Width - CustomControls.controlsDropDown_Button.Width - 11, (MainTop_Panel.Height - CustomControls.controlsDropDown_Button.Height) / 2);
            MainTop_Panel.Controls.Add(CustomControls.controlsDropDown_Button);
            MainTop_Panel.Controls.Remove(ManageAccountants_Button);
            MainTop_Panel.Controls.Remove(ManageCategories_Button);
            MainTop_Panel.Controls.Remove(ManageCompanies_Button);
            MainTop_Panel.Controls.Remove(ManageProducts_Button);
            MainTop_Panel.Controls.Remove(AddSale_Button);
            MainTop_Panel.Controls.Remove(AddPurchase_Button);
        }
        private void RemoveControlsDropDown()
        {
            MainTop_Panel.Controls.Remove(CustomControls.controlsDropDown_Button);

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
            _purchases_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Purchases;
            _selectedDataGridView = _purchases_DataGridView;
            Controls.Add(_purchases_DataGridView);
            Controls.Remove(_sales_DataGridView);
            ResizeControls();
            RefreshDataGridView();

            UnselectButtons();
            SelectButton(Purchases_Button);

            _purchases_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            Search_TextBox.PlaceholderText = "Search for purchases";
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            _sales_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Sales;
            _selectedDataGridView = _sales_DataGridView;
            Controls.Add(_sales_DataGridView);
            Controls.Remove(_purchases_DataGridView);
            ResizeControls();
            RefreshDataGridView();

            UnselectButtons();
            SelectButton(Sales_Button);

            _sales_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            Search_TextBox.PlaceholderText = "Search for sales";
        }
        private void Statistics_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ConstructControlsForStatistics();
            AddStatisticsControls();
            ResizeControls();

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
            if (isProgramLoading) { return; }

            if (!timerRunning)
            {
                timerRunning = true;
                searchTimer.Start();
            }
        }
        private void DateRange_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new DateRange_Form().ShowDialog();
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
        public void SortDataGridView()
        {
            ApplyFilters();
            LoadCharts();
            UpdateTotals();
        }
        public void ApplyFilters()
        {
            bool comboBoxEnabled = Filter_ComboBox.Enabled;
            TimeInterval interval = TimeInterval.AllTime;
            TimeSpan timeSpan = TimeSpan.MaxValue;

            if (comboBoxEnabled)
            {
                string filter = Filter_ComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(filter))
                {
                    (interval, _, timeSpan) = timeIntervals.FirstOrDefault(ti => ti.displayString == filter);
                }
            }
            else
            {
                FilterDataGridViewByDateRange(_selectedDataGridView);
            }

            bool filterExists = interval != TimeInterval.AllTime ||
                !string.IsNullOrEmpty(Search_TextBox.Text.Trim()) ||
                !comboBoxEnabled;

            bool hasVisibleRows = false;

            foreach (DataGridViewRow row in _selectedDataGridView.Rows)
            {
                DateTime rowDate = DateTime.Parse(row.Cells[SalesColumnHeaders[Column.Date]].Value.ToString());
                bool isVisibleByDate = comboBoxEnabled
                    ? (interval == TimeInterval.AllTime || rowDate >= DateTime.Now - timeSpan)
                    : row.Visible;

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

            if (filterExists)
            {
                ShowShowingResultsForLabel();
            }
            else
            {
                HideShowingResultsForLabel();
            }

            ManageNoDataLabelOnControl(hasVisibleRows, _selectedDataGridView, ReadOnlyVariables.NoResults_text);
        }
        private void FilterDataGridViewByDateRange(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DateTime rowDate = DateTime.Parse(row.Cells[PurchaseColumnHeaders[Column.Date]].Value.ToString());
                bool isVisible = rowDate >= fromDate && rowDate <= toDate;
                row.Visible = isVisible;
            }
        }

        /// <summary>
        /// If there is no data, then it adds a Label to the control.
        /// </summary>
        /// <returns>True if there is any data, False if there is no data.</returns>
        public static bool ManageNoDataLabelOnControl(bool hasData, Control control, string text)
        {
            Label existingLabel = control.Controls.OfType<Label>().FirstOrDefault(label => label.Tag.ToString() == text);

            if (!hasData)
            {
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
                        Tag = text
                    };

                    control.Controls.Add(label);
                    CenterLabelInControl(label, control);

                    control.Resize += (sender, e) => CenterLabelInControl(label, control);

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
            ResizeControls();

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

        // Search DataGridView
        private void ShowShowingResultsForLabel()
        {
            string text = "Showing results for";
            string searchText = Search_TextBox.Text.Trim();

            // Append search text if available
            if (!string.IsNullOrEmpty(searchText))
            {
                text += $" '{searchText}'";
            }

            // Handle ComboBox case
            if (Filter_ComboBox.Enabled && !string.IsNullOrEmpty(Filter_ComboBox.Text) && Filter_ComboBox.Text != timeIntervals[0].displayString)
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    text += $"\nin the last {Filter_ComboBox.Text}";
                }
                else
                {
                    text = $"Showing results for\nthe last {Filter_ComboBox.Text}";
                }
            }

            // Handle DateRange case
            if (!Filter_ComboBox.Enabled)
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    text += $"\nfrom {Tools.FormatDate(fromDate)} to {Tools.FormatDate(toDate)}";
                }
                else
                {
                    text = $"Showing results from\n{Tools.FormatDate(fromDate)} to {Tools.FormatDate(toDate)}";
                }
            }

            // Update label text and location
            ShowingResultsFor_Label.Text = text;

            // The control needs to be addded before the location is set, otherwise the control's size property is not calculated properly
            Controls.Add(ShowingResultsFor_Label);

            ShowingResultsFor_Label.Location = new Point(
                (ClientSize.Width - ShowingResultsFor_Label.Width) / 2,
                MainTop_Panel.Bottom + (Distribution_Chart.Top - MainTop_Panel.Bottom - ShowingResultsFor_Label.Height) / 2);

        }
        public void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
        }

        private void SortTheDataGridViewByDate()
        {
            string dateColumnHeader = SalesColumnHeaders[Column.Date];
            _sales_DataGridView.Sort(_sales_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);

            dateColumnHeader = PurchaseColumnHeaders[Column.Date];
            _purchases_DataGridView.Sort(_purchases_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);
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
        private void Filter_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isProgramLoading) { return; }

            CloseAllPanels(null, null);
            ApplyFilters();
            LoadCharts();
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
        public static string? GetCategoryProductIsFrom(List<Category> categoryList, string productName)
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

        // DataGridView properties
        public bool isProgramLoading;
        public SelectedOption Selected;
        private Guna2DataGridView _purchases_DataGridView, _sales_DataGridView, _selectedDataGridView;
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

        // DataGridView getters
        public Guna2DataGridView Purchases_DataGridView => _purchases_DataGridView;
        public Guna2DataGridView Sales_DataGridView => _sales_DataGridView;
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

            if (_selectedDataGridView == _purchases_DataGridView)
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
                Controls.Remove(Total_Panel);
            }
            else
            {
                Controls.Add(Total_Panel);
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
            List<Dictionary<string, object>> rowsData = new();

            // Collect data from the DataGridView
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                Dictionary<string, object> rowData = new();

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

            // Serialize to JSON and write to file
            string json = JsonSerializer.Serialize(rowsData, ReadOnlyVariables.JsonOptions);
            Directories.WriteTextToFile(filePath, json);

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{selected} list");
        }
        public void SaveCategoriesToFile(SelectedOption option)
        {
            if (isProgramLoading)
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

            string json = JsonSerializer.Serialize(categoryList, ReadOnlyVariables.JsonOptions);
            Directories.WriteTextToFile(filePath, json);

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{Selected} list");
        }
        public static void SaveDataGridViewToFile(Guna2DataGridView dataGridView, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);
            List<string> linesInDataGridView = new();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string cellValue = row.Cells[0].Value.ToString();
                    linesInDataGridView.Add(cellValue);
                }
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{selected} list");
        }
        public static void SaveListToFile(List<string> list, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);

            Directories.WriteLinesToFile(filePath, list);
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{selected} list");
        }

        // Statistics menu properties
        private List<Control> statisticsControls;
        private GunaChart countriesOfOrigin_Chart, companiesOfOrigin_Chart, countriesOfDestination_Chart,
            accountants_Chart, salesVsExpenses_Chart, averageOrderValue_Chart;

        // Statistics menu methods
        private List<Control> GetMainControlsList()
        {
            return [
                _sales_DataGridView,
                _purchases_DataGridView,
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
                Controls.Add(control);
            }
            foreach (Control control in statisticsControls)
            {
                Controls.Remove(control);
            }

            ResizeControls();
        }
        private void ConstructControlsForStatistics()
        {
            if (countriesOfOrigin_Chart != null)
            {
                return;
            }

            countriesOfOrigin_Chart = ConstructStatisticsChart(250, "Countries of origin for purchased products");
            companiesOfOrigin_Chart = ConstructStatisticsChart(250, "Companies of origin for purchased products");
            countriesOfDestination_Chart = ConstructStatisticsChart(250, "Countries of destination for sold products");
            accountants_Chart = ConstructStatisticsChart(800, "Transactions managed by accountants");
            salesVsExpenses_Chart = ConstructStatisticsChart(800, "Total sales vs. total expenses");
            averageOrderValue_Chart = ConstructStatisticsChart(800, "Average order value");

            statisticsControls =
            [
                countriesOfOrigin_Chart,
                companiesOfOrigin_Chart,
                countriesOfDestination_Chart,
                accountants_Chart,
                salesVsExpenses_Chart,
                averageOrderValue_Chart
            ];
        }
        private static GunaChart ConstructStatisticsChart(int top, string title)
        {
            GunaChart gunaChart = new()
            {
                Top = top,
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

            return gunaChart;
        }
        private void AddStatisticsControls()
        {
            if (Selected == SelectedOption.Statistics)
            {
                return;
            }
            Selected = SelectedOption.Statistics;

            foreach (Control control in statisticsControls)
            {
                Controls.Add(control);
            }
            foreach (Control control in GetMainControlsList())
            {
                Controls.Remove(control);
            }

            UpdateStatisticsCharts();
            ResizeControls();
        }
        private void UpdateStatisticsCharts()
        {
            bool isLineChart = LineGraph_ToggleSwitch.Checked;

            LoadChart.LoadCountriesOfOriginForProductsIntoChart(_purchases_DataGridView, countriesOfOrigin_Chart);
            LoadChart.LoadCompaniesOfOriginForProductsIntoChart(_purchases_DataGridView, companiesOfOrigin_Chart);
            LoadChart.LoadCountriesOfDestinationForProductsIntoChart(_sales_DataGridView, countriesOfDestination_Chart);
            LoadChart.LoadAccountantsIntoChart([_purchases_DataGridView, _sales_DataGridView], accountants_Chart);
            LoadChart.LoadSalesVsExpensesChart(_purchases_DataGridView, _sales_DataGridView, salesVsExpenses_Chart, isLineChart);
            LoadChart.LoadAverageOrderValueChart(_sales_DataGridView, averageOrderValue_Chart, isLineChart);
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
        public static void CloseRightClickPanels()
        {
            DataGridViewManager.ControlRightClickPanelWasAddedTo?.Controls.Remove(DataGridViewManager.RightClickDataGridView_Panel);
            DataGridViewManager.DoNotDeleteRows = false;
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}