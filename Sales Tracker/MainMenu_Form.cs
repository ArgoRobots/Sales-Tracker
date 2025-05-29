using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Proprties
        private static MainMenu_Form _instance;
        private static readonly List<string> _thingsThatHaveChangedInFile = [], _settingsThatHaveChangedInFile = [];
        private static string _currencySymbol;
        private static bool _isProgramLoading;
        public static readonly string noteTextKey = "note", rowTagKey = "RowTag", itemsKey = "Items", purchaseDataKey = "PurchaseData", tagKey = "Tag";

        // Getters and setters
        public static MainMenu_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;
        public static List<string> SettingsThatHaveChangedInFile => _settingsThatHaveChangedInFile;
        public static string CurrencySymbol
        {
            get => _currencySymbol;
            set => _currencySymbol = value;
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

            _isProgramLoading = true;
            CurrencySymbol = Currency.GetSymbol();

            CustomControls.ConstructControls();
            ConstructMainCharts();
            ConstructControlsForAnalytics();
            InitiateSearchTimer();
            SetCompanyLabel();
            LoadData();
            LoadCustomColumnHeaders();
            UpdateTheme();
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            SetToolTips();
            HideShowingResultsForLabel();
            InitTimeRangePanel();
            InitChartTags();
            AddEventHandlersToTextBoxes();
            AnimateButtons();
            InitializeAISearch();
            RemoveUpgradeButtonIfFullVersion();
            UpdateMainMenuFormText();
            LoadingPanel.ShowBlankLoadingPanel(this);
            IncludeFreeShipping_CheckBox.Checked = Properties.Settings.Default.IncludeFreeShipping;
        }
        private void ConstructMainCharts()
        {
            _purchaseTotals_Chart = ConstructMainChart("purchaseTotals_Chart");
            _purchaseDistribution_Chart = ConstructMainChart("purchaseDistribution_Chart");
            _saleTotals_Chart = ConstructMainChart("saleTotals_Chart");
            _saleDistribution_Chart = ConstructMainChart("saleDistribution_Chart");
            _profits_Chart = ConstructMainChart("profits_Chart");

            LoadChart.ConfigureChartForPie(_purchaseDistribution_Chart);
            LoadChart.ConfigureChartForPie(_saleDistribution_Chart);

            // Set initial visibility - only show sale charts by default
            _purchaseTotals_Chart.Visible = false;
            _purchaseDistribution_Chart.Visible = false;
            _saleTotals_Chart.Visible = true;
            _saleDistribution_Chart.Visible = true;
            _profits_Chart.Visible = true;

            _purchaseTotals_Chart.Tag = ChartDataType.TotalRevenue;
            _purchaseDistribution_Chart.Tag = ChartDataType.DistributionOfRevenue;
            _saleTotals_Chart.Tag = ChartDataType.TotalRevenue;
            _saleDistribution_Chart.Tag = ChartDataType.DistributionOfRevenue;
            _profits_Chart.Tag = ChartDataType.TotalProfits;

            MouseClickChartManager.InitCharts([
                _purchaseTotals_Chart, _purchaseDistribution_Chart,
                _saleTotals_Chart, _saleDistribution_Chart, _profits_Chart
            ]);
        }
        private GunaChart ConstructMainChart(string name)
        {
            GunaChart chart = new()
            {
                Name = name
            };

            chart.ApplyConfig(ChartColors.Config(), CustomColors.ContentPanelBackground);
            chart.Title.Display = true;
            chart.Title.Font = new ChartFont("Segoe UI", 20, ChartFontStyle.Bold);
            chart.Legend.LabelFont = new ChartFont("Segoe UI", 18);
            chart.Tooltips.TitleFont = new ChartFont("Segoe UI", 18, ChartFontStyle.Bold);
            chart.Tooltips.BodyFont = new ChartFont("Segoe UI", 18);
            chart.XAxes.Ticks.Font = new("Segoe UI", 18);
            chart.YAxes.Ticks.Font = new("Segoe UI", 18);
            chart.Top = 219;

            Controls.Add(chart);
            return chart;
        }
        private void SetToolTips()
        {
            CustomTooltip.SetToolTip(File_Button, "", "File");
            CustomTooltip.SetToolTip(Save_Button, "", "Save");
            CustomTooltip.SetToolTip(Upgrade_Button, "", "Upgrade to full version");
            CustomTooltip.SetToolTip(Help_Button, "", "Help");
        }
        public void ResetData()
        {
            _categoryPurchaseList.Clear();
            _categorySaleList.Clear();

            _accountantList.Clear();
            _companyList.Clear();

            _purchase_DataGridView.Rows.Clear();
            _sale_DataGridView.Rows.Clear();

            Search_TextBox.Clear();
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
            }

            SetHasReceiptColumnVisibilty();

            AddRowsFromFile(_purchase_DataGridView, SelectedOption.Purchases);
            AddRowsFromFile(_sale_DataGridView, SelectedOption.Sales);
        }
        private void LoadCustomColumnHeaders()
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
        public void LoadOrRefreshMainCharts(bool onlyLoadForLineCharts = false)
        {
            bool isLine = LineGraph_ToggleSwitch.Checked;

            if (_sale_DataGridView.Visible)
            {
                // Load sale charts
                ChartData totalsData = LoadChart.LoadTotalsIntoChart(_sale_DataGridView, _saleTotals_Chart, isLine);
                _saleTotals_Chart.Title.Text = $"Total revenue: {CurrencySymbol}{totalsData.GetTotal():N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_sale_DataGridView, _saleDistribution_Chart, PieChartGrouping.Top12);
                    _saleDistribution_Chart.Title.Text = "Distribution of revenue";
                }

                LanguageManager.UpdateLanguageForControl(_saleTotals_Chart, true);
                LanguageManager.UpdateLanguageForControl(_saleDistribution_Chart, true);
            }
            else if (_purchase_DataGridView.Visible)
            {
                // Load purchase charts
                ChartData totalsData = LoadChart.LoadTotalsIntoChart(_purchase_DataGridView, _purchaseTotals_Chart, isLine);
                _purchaseTotals_Chart.Title.Text = $"Total expenses: {CurrencySymbol}{totalsData.GetTotal():N2}";

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(_purchase_DataGridView, _purchaseDistribution_Chart, PieChartGrouping.Top12);
                    _purchaseDistribution_Chart.Title.Text = "Distribution of expenses";
                }

                LanguageManager.UpdateLanguageForControl(_purchaseTotals_Chart, true);
                LanguageManager.UpdateLanguageForControl(_purchaseDistribution_Chart, true);
            }

            // Always load profits chart (it combines both)
            ChartData profitsData = LoadChart.LoadProfitsIntoChart(_profits_Chart, isLine);
            _profits_Chart.Title.Text = $"Total profits: {CurrencySymbol}{profitsData.GetTotal():N2}";
            LanguageManager.UpdateLanguageForControl(_profits_Chart, true);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            if (ThemeManager.IsDarkTheme())
            {
                Edit_Button.Image = Resources.EditWhite;
            }
            else
            {
                Edit_Button.Image = Resources.EditBlack;
            }

            MainTop_Panel.FillColor = CustomColors.ContentPanelBackground;
            Edit_Button.FillColor = CustomColors.ContentPanelBackground;
            Top_Panel.BackColor = CustomColors.ToolbarBackground;
            File_Button.FillColor = CustomColors.ToolbarBackground;
            Save_Button.FillColor = CustomColors.ToolbarBackground;
            Upgrade_Button.FillColor = CustomColors.ToolbarBackground;
            Help_Button.FillColor = CustomColors.ToolbarBackground;

            ShowingResultsFor_Label.ForeColor = CustomColors.Text;
            ReselectButton();

            ThemeManager.SetThemeForControl([
                _purchaseTotals_Chart,
                _purchaseDistribution_Chart,
                _saleTotals_Chart,
                _saleDistribution_Chart,
                _profits_Chart,
                Total_Panel
            ]);
        }
        private void ReselectButton()
        {
            if (Purchases_Button.BorderThickness == 2)
            {
                Purchases_Button.BorderColor = CustomColors.AccentBlue;
            }
            else if (Sales_Button.BorderThickness == 2)
            {
                Sales_Button.BorderColor = CustomColors.AccentBlue;
            }
            else if (Analytics_Button.BorderThickness == 2)
            {
                Analytics_Button.BorderColor = CustomColors.AccentBlue;
            }
        }
        private void SetAccessibleDescriptions()
        {
            CompanyName_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Quantity_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Shipping_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Tax_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Fee_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Discount_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            ChargedDifference_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            Price_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            LineGraph_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;
        }
        private void InitChartTags()
        {
            // Main charts
            _purchaseTotals_Chart.Tag = ChartDataType.TotalRevenue;
            _purchaseDistribution_Chart.Tag = ChartDataType.DistributionOfRevenue;
            _saleTotals_Chart.Tag = ChartDataType.TotalRevenue;
            _saleDistribution_Chart.Tag = ChartDataType.DistributionOfRevenue;
            _profits_Chart.Tag = ChartDataType.TotalProfits;

            // Analytics charts
            _countriesOfOrigin_Chart.Tag = ChartDataType.CountriesOfOrigin;
            _companiesOfOrigin_Chart.Tag = ChartDataType.CompaniesOfOrigin;
            _countriesOfDestination_Chart.Tag = ChartDataType.CountriesOfDestination;
            _accountants_Chart.Tag = ChartDataType.Accountants;
            _growthRates_Chart.Tag = ChartDataType.GrowthRates;
            _salesVsExpenses_Chart.Tag = ChartDataType.TotalExpensesVsSales;
            _totalTransactions_Chart.Tag = ChartDataType.TotalTransactions;
            _averageTransactionValue_Chart.Tag = ChartDataType.AverageOrderValue;
            _averageShippingCosts_Chart.Tag = ChartDataType.AverageShippingCosts;
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Search_TextBox);
        }
        public static void RemoveUpgradeButtonIfFullVersion()
        {
            if (Properties.Settings.Default.LicenseActivated)
            {
                Instance.Top_Panel.Controls.Remove(Instance.Upgrade_Button);
            }
        }
        public void AnimateButtons()
        {
            IEnumerable<Guna2Button> buttons =
            [
               Purchases_Button,
               Sales_Button,
               Analytics_Button,
               AddPurchase_Button,
               AddSale_Button,
               ManageProducts_Button,
               ManageCategories_Button,
               ManageCompanies_Button,
               ManageAccountants_Button,
               TimeRange_Button,
            ];
            CustomControls.AnimateButtons(buttons, Properties.Settings.Default.AnimateButtons);
        }
        private static void InitializeAISearch()
        {
            if (Properties.Settings.Default.AISearchEnabled)
            {
                string _chatGptApiKey = DotEnv.Get("CHATGPT_API_KEY");
                if (!string.IsNullOrEmpty(_chatGptApiKey))
                {
                    AISearchExtensions.InitializeAISearch(_chatGptApiKey);
                }
                else
                {
                    Log.Write(1, "AI Search disabled: No API key found");
                }
            }
        }

        // Add rows from file
        private static void AddRowsFromFile(Guna2DataGridView dataGridView, SelectedOption selected)
        {
            string filePath = DataGridViewManager.GetFilePathForDataGridView(selected);
            if (!ValidateFile(filePath))
            {
                return;
            }

            List<Dictionary<string, object>>? rowsData = DeserializeJsonFile(filePath);
            if (rowsData == null)
            {
                return;
            }

            foreach (Dictionary<string, object> rowData in rowsData)
            {
                ProcessRow(dataGridView, rowData);
            }

            bool hasVisibleRows = false;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                hasVisibleRows = true;
            }
            LabelManager.ManageNoDataLabelOnControl(hasVisibleRows, dataGridView);
        }
        private static bool ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error_WriteToFile(Directories.CompanyName);
                return false;
            }
            return true;
        }
        private static List<Dictionary<string, object>>? DeserializeJsonFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
        }
        private static void ProcessRow(Guna2DataGridView dataGridView, Dictionary<string, object> rowData)
        {
            string?[] cellValues = ExtractCellValues(rowData);
            int rowIndex = dataGridView.Rows.Add(cellValues);

            ProcessRowTag(dataGridView, rowData, rowIndex);
            ProcessNoteText(dataGridView, rowData, rowIndex);
            ProcessHasReceipt(dataGridView, rowIndex);
        }
        private static string?[] ExtractCellValues(Dictionary<string, object> rowData)
        {
            return rowData
                .Where(kv => kv.Key != rowTagKey && kv.Key != noteTextKey)
                .Select(kv => kv.Value?.ToString())
                .ToArray();
        }
        private static void ProcessRowTag(Guna2DataGridView dataGridView, Dictionary<string, object> rowData, int rowIndex)
        {
            if (!rowData.TryGetValue(rowTagKey, out object value) || value is not JObject jsonObject)
            {
                return;
            }

            Dictionary<string, object>? tagObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonObject.ToString());
            if (tagObject == null)
            {
                return;
            }

            if (TryProcessItemsAndPurchaseData(tagObject, out (List<string>, TagData)? rowTag))
            {
                dataGridView.Rows[rowIndex].Tag = rowTag;
            }
            else if (TryProcessTagStringAndPurchaseData(tagObject, out (string, TagData)? stringPurchaseTag))
            {
                dataGridView.Rows[rowIndex].Tag = stringPurchaseTag;
            }
            else
            {
                TryProcessTagData(jsonObject, dataGridView.Rows[rowIndex]);
            }
        }
        private static bool TryProcessItemsAndPurchaseData(Dictionary<string, object> tagObject, out (List<string>, TagData)? rowTag)
        {
            rowTag = null;

            if (!tagObject.TryGetValue(itemsKey, out object? itemsElement) || itemsElement is not JArray itemsArray)
            {
                return false;
            }

            if (!tagObject.TryGetValue(purchaseDataKey, out object? purchaseDataElement) || purchaseDataElement is not JObject purchaseDataObject)
            {
                return false;
            }

            List<string> itemList = itemsArray.Select(e => e.ToString()).ToList();
            TagData? purchaseData = JsonConvert.DeserializeObject<TagData>(purchaseDataObject.ToString());

            if (purchaseData != null)
            {
                rowTag = (itemList, purchaseData);
                return true;
            }

            return false;
        }
        private static bool TryProcessTagStringAndPurchaseData(Dictionary<string, object> tagObject, out (string, TagData)? tag)
        {
            tag = null;

            if (!tagObject.TryGetValue(tagKey, out object? tagStringElement) ||
                !tagObject.TryGetValue(purchaseDataKey, out object? purchaseDataElement))
            {
                return false;
            }

            string? tagString = tagStringElement?.ToString();
            TagData? purchaseData = JsonConvert.DeserializeObject<TagData>(purchaseDataElement?.ToString());

            if (tagString != null && purchaseData != null)
            {
                tag = (tagString, purchaseData);
                return true;
            }

            return false;
        }
        private static void TryProcessTagData(JObject jsonObject, DataGridViewRow row)
        {
            TagData? tagData = JsonConvert.DeserializeObject<TagData>(jsonObject.ToString());
            if (tagData != null)
            {
                row.Tag = tagData;
            }
        }
        private static void ProcessNoteText(Guna2DataGridView dataGridView, Dictionary<string, object> rowData, int rowIndex)
        {
            if (!rowData.TryGetValue(noteTextKey, out object noteValue))
            {
                return;
            }

            DataGridViewCell noteCell = dataGridView.Rows[rowIndex].Cells[Column.Note.ToString()];
            noteCell.Value = ReadOnlyVariables.Show_text;
            noteCell.Tag = noteValue;
            DataGridViewManager.AddUnderlineToCell(noteCell);
        }

        // "Has receipt" column
        private static void ProcessHasReceipt(Guna2DataGridView dataGridView, int rowIndex)
        {
            DataGridViewRow row = dataGridView.Rows[rowIndex];
            string? receipt = ReceiptManager.GetReceiptPathFromRow(row);

            if (receipt == null)
            {
                string transactionID = row.Cells[Column.ID.ToString()].Value.ToString();
                Log.Error_CannotProcessReceiptColumn(transactionID);
                return;
            }

            SetHasReceiptColumn(row, receipt);
        }
        public static void SetHasReceiptColumn(DataGridViewRow row, string receipt)
        {
            if (!Properties.Settings.Default.ShowHasReceiptColumn) { return; }

            string newReceipt = ReceiptManager.ProcessReceiptTextFromRowTag(receipt);

            DataGridViewCell lastCell = row.Cells[row.DataGridView.Columns.Count - 1];
            SetReceiptStatusSymbol(lastCell, newReceipt);
        }
        private static void SetReceiptStatusSymbol(DataGridViewCell cell, string processedReceipt)
        {
            if (!string.IsNullOrEmpty(processedReceipt) && File.Exists(processedReceipt))
            {
                cell.Value = "✓";
                cell.Style.ForeColor = CustomColors.AccentGreen;
            }
            else
            {
                cell.Value = "✗";
                cell.Style.ForeColor = CustomColors.AccentRed;
            }

            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public void SetHasReceiptColumnVisibilty()
        {
            bool showReceipts = Properties.Settings.Default.ShowHasReceiptColumn;
            Purchase_DataGridView.Columns[Column.HasReceipt.ToString()].Visible = showReceipts;
            Sale_DataGridView.Columns[Column.HasReceipt.ToString()].Visible = showReceipts;

            if (!showReceipts) { return; }

            foreach (DataGridViewRow row in GetAllRows())
            {
                DataGridViewCell lastCell = row.Cells[row.DataGridView.Columns.Count - 1];
                string? receiptPath = ReceiptManager.GetReceiptPathFromRow(row);
                SetReceiptStatusSymbol(lastCell, receiptPath);
            }
        }

        // Form event handlers
        private void MainMenu_Form_Load(object sender, EventArgs e)
        {
            Sales_Button.PerformClick();
            _sale_DataGridView.ClearSelection();

            _isProgramLoading = false;

            SortTheDataGridViewByDate();
            AlignTotalLabels();
            UpdateTotalLabels();
            CenterAndResizeControls();
        }
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            Log.Write(2, "Argo Sales Tracker has finished starting");

            // Check if the user has accepted the EULA
            string eulaAccepted = DataFileManager.GetValue(GlobalAppDataSettings.EULAAccepted);
            if (string.IsNullOrEmpty(eulaAccepted) || !bool.TryParse(eulaAccepted, out bool eulaAcceptedBool) || !eulaAcceptedBool)
            {
                // The user hasn't accepted the EULA yet, show the EULA form
                EULA_Form eulaForm = new();
                if (eulaForm.ShowDialog() != DialogResult.OK)
                {
                    // User declined the EULA, the application will exit in the EULA form's Decline_Button_Click method
                    return;
                }
            }

            // Check if the welcome form should be shown (original code)
            string value = DataFileManager.GetValue(GlobalAppDataSettings.ShowWelcomeForm);
            if (bool.TryParse(value, out bool boolResult) && boolResult)
            {
                new Welcome_Form().ShowDialog();
            }
        }
        private void MainMenu_Form_ResizeBegin(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            if (_instance == null) { return; }
            CenterAndResizeControls();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData is Keys.Up or Keys.Down or Keys.Right or Keys.Left or Keys.Enter)
            {
                foreach (Guna2Panel panel in GetMenus())
                {
                    if (Controls.Contains(panel))
                    {
                        MenuKeyShortcutManager.HandlePanelKeyDown(panel, keyData);
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
                        Tools.OpenForm(new Export_Form());
                        break;

                    case Keys.L:  // Open logs
                        Tools.OpenForm(new Log_Form());
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
            CustomControls.CloseAllPanels();
            Log.Write(2, "Closing Argo Sales Tracker");

            if (ArgoCompany.AreAnyChangesMade())
            {
                if (AskUserToSaveBeforeClosing())
                {
                    e.Cancel = true;
                    Log.Write(2, "Close canceled");
                    return;
                }
            }

            Log.SaveLogs();
            ThemeChangeDetector.StopListeningForThemeChanges();
            ArgoCompany.ApplicationMutex?.Dispose();
            Directories.DeleteDirectory(Directories.TempCompany_dir, true);
            Environment.Exit(0);  // Make sure all processes are fully closed
        }

        // Center and resize controls
        private bool wasControlsDropDownAdded;
        public void CenterAndResizeControls()
        {
            if (_isProgramLoading) { return; }

            if (ClientSize.Height > 1400)
            {
                SetMainChartsHeight(500);
            }
            else if (ClientSize.Height > 1000)
            {
                SetMainChartsHeight(400);
            }
            else
            {
                SetMainChartsHeight(300);
            }

            byte spaceBetweenCharts = 20, chartWidthOffset = 35;

            // Handle dropdown menu for narrow windows
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

            if (Selected == SelectedOption.Analytics)
            {
                // Calculate chart dimensions
                int chartWidth = ClientSize.Width / 3 - chartWidthOffset;
                int availableHeight = ClientSize.Height - Purchases_Button.Bottom;
                int statChartHeight = availableHeight / 3 - 20 - 20;  // 3 rows with 20px spacing vertically, 20px for checkbox on bottom
                Size chartSize = new(chartWidth, statChartHeight);

                // Calculate total width needed for all charts
                int totalWidth = (chartWidth * 3) + (spaceBetweenCharts * 2);

                // Calculate left margin to center all charts
                int leftMargin = (ClientSize.Width - totalWidth) / 2;

                // Calculate X positions for 3 columns
                int firstX = leftMargin;
                int secondX = firstX + chartWidth + spaceBetweenCharts;
                int thirdX = secondX + chartWidth + spaceBetweenCharts;

                // Calculate vertical spacing
                int totalChartHeight = statChartHeight * 3;
                int spaceBetweenRows = (availableHeight - totalChartHeight) / 4;

                // Calculate Y positions for 3 rows
                int topRowY = Purchases_Button.Bottom + spaceBetweenRows;
                int middleRowY = topRowY + statChartHeight + spaceBetweenRows - 10;  // 10px to make space for checkbox on bottom
                int bottomRowY = middleRowY + statChartHeight + spaceBetweenRows - 10;

                // Set positions for top row charts
                SetChartPosition(_countriesOfOrigin_Chart, chartSize, firstX, topRowY);
                SetChartPosition(_countriesOfDestination_Chart, chartSize, secondX, topRowY);
                SetChartPosition(_companiesOfOrigin_Chart, chartSize, thirdX, topRowY);

                // Set positions for middle row charts
                SetChartPosition(_accountants_Chart, chartSize, firstX, middleRowY);
                SetChartPosition(_growthRates_Chart, chartSize, secondX, middleRowY);
                SetChartPosition(_salesVsExpenses_Chart, chartSize, thirdX, middleRowY);

                // Set positions for bottom row charts
                SetChartPosition(_totalTransactions_Chart, chartSize, firstX, bottomRowY);
                SetChartPosition(_averageTransactionValue_Chart, chartSize, secondX, bottomRowY);
                SetChartPosition(_averageShippingCosts_Chart, chartSize, thirdX, bottomRowY);

                // Set position for free shipping checkbox
                _includeFreeShipping_CheckBox.Location = new Point(_averageShippingCosts_Chart.Left, _averageShippingCosts_Chart.Bottom + 8);
                _includeFreeShipping_Label.Location = new Point(_includeFreeShipping_CheckBox.Right - 2, _includeFreeShipping_CheckBox.Top - 9);
            }
            else
            {
                // Regular view layout for main charts
                int chartWidth = ClientSize.Width / 3 - chartWidthOffset;

                // Get current chart height (already set above)
                int chartHeight = _saleTotals_Chart.Height;

                // Calculate X positions for charts
                int leftX = (ClientSize.Width - 3 * chartWidth - spaceBetweenCharts * 2) / 2;
                int middleX = leftX + chartWidth + spaceBetweenCharts;
                int rightX = middleX + chartWidth + spaceBetweenCharts;

                // Position the currently visible charts
                GunaChart totalsChart = _sale_DataGridView.Visible ? _saleTotals_Chart : _purchaseTotals_Chart;
                GunaChart distributionChart = _sale_DataGridView.Visible ? _saleDistribution_Chart : _purchaseDistribution_Chart;

                // Set positions for visible charts
                totalsChart.Size = new Size(chartWidth, chartHeight);
                totalsChart.Left = leftX;

                distributionChart.Size = new Size(chartWidth, chartHeight);
                distributionChart.Left = middleX;

                _profits_Chart.Size = new Size(chartWidth, chartHeight);
                _profits_Chart.Left = rightX;

                // Position DataGridView and Total Panel
                _selectedDataGridView.Size = new Size(ClientSize.Width - 65,
                    ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - chartHeight - totalsChart.Top - 15);
                _selectedDataGridView.Location = new Point((ClientSize.Width - _selectedDataGridView.Width) / 2,
                    ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - _selectedDataGridView.Height);

                Total_Panel.Location = new Point(_selectedDataGridView.Left, _selectedDataGridView.Top + _selectedDataGridView.Height);
                Total_Panel.Width = _selectedDataGridView.Width;
            }
        }
        private void SetMainChartsHeight(int height)
        {
            _purchaseTotals_Chart.Height = height;
            _purchaseDistribution_Chart.Height = height;
            _saleTotals_Chart.Height = height;
            _saleDistribution_Chart.Height = height;
            _profits_Chart.Height = height;
        }

        private static void SetChartPosition(GunaChart chart, Size size, int left, int top)
        {
            chart.Size = size;
            chart.Location = new Point(left, top);
            LabelManager.CenterNoDataLabelInControl(chart);
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
            }
            ;
        }
        private void RemoveControlsDropDown()
        {
            MainTop_Panel.Controls.Remove(CustomControls.ControlsDropDown_Button);

            foreach (Control button in GetMainTopButtons())
            {
                button.Visible = true;
            }
            ;
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
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Save changes",
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
            if (ToggleMenu(CustomControls.FileMenu, File_Button, Resources.FileGray, Resources.FileWhite, false))
            {
                Controls.Remove(CustomControls.RecentlyOpenedMenu);
            }
        }
        private void Save_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
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
        private void Upgrade_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            Tools.OpenForm(new Upgrade_Form());
        }
        private void Help_Button_Click(object sender, EventArgs e)
        {
            ToggleMenu(CustomControls.HelpMenu, Help_Button, Resources.HelpGray, Resources.HelpWhite, true);
        }

        /// <summary>
        /// Toggles the visibility of a menu and updates the button's image accordingly.
        /// </summary>
        /// <returns>
        /// True if the menu was closed; False if the menu was opened.
        /// </returns>
        private bool ToggleMenu(Control menu, Guna2Button button, Image grayImage, Image whiteImage, bool alignRight)
        {
            if (Controls.Contains(menu))
            {
                Controls.Remove(menu);
                button.Image = grayImage;
                return true;
            }
            else
            {
                CustomControls.CloseAllPanels();
                button.Image = whiteImage;

                // Calculate X position based on alignment
                int xPosition = alignRight ?
                    button.Left - menu.Width + button.Width :
                    button.Left;

                menu.Location = new Point(xPosition, Top_Panel.Height);
                Controls.Add(menu);
                menu.BringToFront();
                return false;
            }
        }

        // Event handlers
        private void Purchases_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (Selected == SelectedOption.Purchases) { return; }

            _purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            Selected = SelectedOption.Purchases;
            ShowMainControls();
            _selectedDataGridView = _purchase_DataGridView;
            _purchase_DataGridView.Visible = true;
            _sale_DataGridView.Visible = false;

            // Show purchase charts, hide sale charts
            _purchaseTotals_Chart.Visible = true;
            _purchaseDistribution_Chart.Visible = true;
            _saleTotals_Chart.Visible = false;
            _saleDistribution_Chart.Visible = false;

            CenterAndResizeControls();

            // Only refresh if the purchase charts haven't been loaded yet
            if (_purchaseTotals_Chart.Datasets.Count == 0)
            {
                RefreshDataGridViewAndCharts();
            }

            _purchase_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            UpdateTotalLabels();
            SelectButton(Purchases_Button);
            Search_TextBox.PlaceholderText = LanguageManager.TranslateString("Search for purchases");
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (Selected == SelectedOption.Sales) { return; }

            _sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            Selected = SelectedOption.Sales;
            ShowMainControls();
            _selectedDataGridView = _sale_DataGridView;
            _sale_DataGridView.Visible = true;
            _purchase_DataGridView.Visible = false;

            // Show sale charts, hide purchase charts
            _saleTotals_Chart.Visible = true;
            _saleDistribution_Chart.Visible = true;
            _purchaseTotals_Chart.Visible = false;
            _purchaseDistribution_Chart.Visible = false;

            CenterAndResizeControls();

            // Only refresh if the sale charts haven't been loaded yet
            if (_saleTotals_Chart.Datasets.Count == 0)
            {
                RefreshDataGridViewAndCharts();
            }

            _sale_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            UpdateTotalLabels();
            SelectButton(Sales_Button);
            Search_TextBox.PlaceholderText = LanguageManager.TranslateString("Search for sales");
        }
        private void Analytics_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (Selected == SelectedOption.Analytics) { return; }

            Selected = SelectedOption.Analytics;
            ShowAnalyticsControls();
            CenterAndResizeControls();

            SelectButton(Analytics_Button);
        }
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new AddPurchase_Form());
        }
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new AddSale_Form());
        }
        private void ManageAccountants_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new Accountants_Form());
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new Products_Form(true));
        }
        private void ManageCompanies_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new Companies_Form());
        }
        private void ManageCategories_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Tools.OpenForm(new Categories_Form(true));
        }
        private void LineGraph_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            LoadOrRefreshMainCharts(true);
            LoadOrRefreshAnalyticsCharts(true);
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
        private async void Search_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isProgramLoading) { return; }

            bool isAIQuery = Properties.Settings.Default.AISearchEnabled
                && Properties.Settings.Default.LicenseActivated
                && Search_TextBox.Text.StartsWith('!');

            // Start timer for regular searches
            if (!timerRunning && !isAIQuery)
            {
                timerRunning = true;
                searchTimer.Start();
            }

            // Process AI search on Enter key
            else if (e.KeyCode == Keys.Enter && isAIQuery)
            {
                ShowingResultsFor_Label.Text = "AI search in progress...";
                CenterShowingResultsLabel();
                await Search_TextBox.EnhanceSearchAsync();
            }
            else if (isAIQuery)
            {
                ShowingResultsFor_Label.Text = "Press enter to begin AI search";
                CenterShowingResultsLabel();
            }
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Search_TextBox.Text == "")
            {
                AISearchExtensions.ResetQuery();

                // Start timer for regular searches
                if (!timerRunning)
                {
                    timerRunning = true;
                    searchTimer.Start();
                }
            }
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Clear();
        }

        // TimeRange
        private static Guna2Panel _timeRangePanel;
        public static Guna2Panel TimeRangePanel => _timeRangePanel;
        private static void InitTimeRangePanel()
        {
            DateRange_Form dateRange_Form = new();
            _timeRangePanel = dateRange_Form.Main_Panel;
            _timeRangePanel.BorderColor = CustomColors.ControlPanelBorder;
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
            RefreshDataGridViewAndCharts();
        }

        // Methods for Event handlers
        private void SelectButton(Guna2Button button)
        {
            UnselectButtons();
            button.BorderThickness = 2;
            button.BorderColor = CustomColors.AccentBlue;
        }
        private void UnselectButtons()
        {
            Purchases_Button.BorderThickness = 1;
            Sales_Button.BorderThickness = 1;
            Analytics_Button.BorderThickness = 1;
            Purchases_Button.BorderColor = CustomColors.ControlBorder;
            Sales_Button.BorderColor = CustomColors.ControlBorder;
            Analytics_Button.BorderColor = CustomColors.ControlBorder;
        }

        // Company label
        public void RenameCompany()
        {
            if (!Controls.Contains(CustomControls.Rename_TextBox))
            {
                return;
            }

            // If the name did not change
            if (CustomControls.Rename_TextBox.Text == CompanyName_Label.Text)
            {
                CloseRenameCompany();
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

                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"Rename company",
                    $"Do you want to rename '{name}' to '{suggestedCompanyName}'? There is already a company with the same name.",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);

                if (result == CustomMessageBoxResult.Ok)
                {
                    CustomControls.Rename_TextBox.Text = suggestedCompanyName;
                }
                else { return; }
            }

            CloseRenameCompany();

            CompanyName_Label.Text = CustomControls.Rename_TextBox.Text;
            ArgoCompany.Rename(CustomControls.Rename_TextBox.Text);
            CustomControls.Rename_TextBox.Clear();
            MoveEditButton();
            CenterAndResizeControls();
            UpdateMainMenuFormText();

            string message = $"Renamed program to: {CompanyName_Label.Text}";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(_thingsThatHaveChangedInFile, 3, message);
        }
        private void CloseRenameCompany()
        {
            Controls.Remove(CustomControls.Rename_TextBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);
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
        private static readonly Dictionary<TimeSpan, string> TimeSpanMappings = new()
        {
            { TimeSpan.MaxValue, "All Time" },
            { TimeSpan.FromDays(1), "1 day" },
            { TimeSpan.FromDays(2), "2 days" },
            { TimeSpan.FromDays(3), "3 days" },
            { TimeSpan.FromDays(5), "5 days" },
            { TimeSpan.FromDays(10), "10 days" },
            { TimeSpan.FromDays(30), "30 days" },
            { TimeSpan.FromDays(100), "100 days" },
            { TimeSpan.FromDays(365), "1 year" },
            { TimeSpan.FromDays(365 * 2), "2 years" },
            { TimeSpan.FromDays(365 * 5), "5 years" }
        };

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
        public void RefreshDataGridViewAndCharts()
        {
            ApplyFiltersToDataGridView();

            if (Selected == SelectedOption.Analytics)
            {
                LoadOrRefreshAnalyticsCharts(false);
            }
            else
            {
                LoadOrRefreshMainCharts();
                _selectedDataGridView.ClearSelection();
            }
        }
        private void ApplyFiltersToDataGridView()
        {
            if (_sortFromDate != null && _sortToDate != null)
            {
                FilterDataGridViewByDateRange(_selectedDataGridView);
            }

            // Get the effective search text, which might be AI-translated
            string displayedSearchText = Search_TextBox.Text.Trim();
            string effectiveSearchText = AISearchExtensions.GetEffectiveSearchQuery(displayedSearchText);

            bool filterExists = _sortTimeSpan != TimeSpan.MaxValue ||
                !string.IsNullOrEmpty(effectiveSearchText);

            bool hasVisibleRows = false;

            if (filterExists)
            {
                ShowShowingResultsForLabel();

                foreach (DataGridViewRow row in _selectedDataGridView.Rows)
                {
                    DateTime rowDate = DateTime.Parse(row.Cells[SalesColumnHeaders[Column.Date]].Value.ToString());

                    bool isVisibleByDate = (_sortTimeSpan == TimeSpan.MaxValue ||
                        rowDate >= DateTime.Now - _sortTimeSpan);

                    bool isVisibleBySearch = string.IsNullOrEmpty(effectiveSearchText) ||
                        SearchDataGridView.FilterRowByAdvancedSearch(row, effectiveSearchText);

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

            DataGridViewManager.UpdateAlternatingRowColors(_selectedDataGridView);
            LabelManager.ManageNoDataLabelOnControl(hasVisibleRows, _selectedDataGridView);
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

            // Get the appropriate display text for the search
            string displayedSearchText = Search_TextBox.Text.Trim();
            string searchDisplay = AISearchExtensions.GetDisplayQuery(displayedSearchText);

            // Append search text if available
            if (!string.IsNullOrEmpty(searchDisplay))
            {
                // Remove the ! prefix if using AI search
                if (displayedSearchText.StartsWith('!') && AISearchExtensions.IsUsingAIQuery)
                {
                    text += $" '{searchDisplay}'";
                }
                else
                {
                    text += $" '{searchDisplay}'";
                }
            }

            // Handle time span case
            if (_sortTimeSpan != null && _sortTimeSpan != TimeSpan.MaxValue)
            {
                string timeSpanText = GetTimeSpanText(_sortTimeSpan.Value);

                if (!string.IsNullOrEmpty(searchDisplay))
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

                if (!string.IsNullOrEmpty(searchDisplay))
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
            CenterShowingResultsLabel();
            ShowingResultsFor_Label.Visible = true;
        }
        private void CenterShowingResultsLabel()
        {
            GunaChart visibleDistributionChart = _sale_DataGridView.Visible ? _saleDistribution_Chart : _purchaseDistribution_Chart;

            ShowingResultsFor_Label.Location = new Point(
                (ClientSize.Width - ShowingResultsFor_Label.Width) / 2,
                MainTop_Panel.Bottom + (visibleDistributionChart.Top - MainTop_Panel.Bottom - ShowingResultsFor_Label.Height) / 2);
        }

        /// <summary>
        /// Helper function to convert TimeSpan into human-readable text
        /// </summary>
        private static string? GetTimeSpanText(TimeSpan timeSpan)
        {
            return TimeSpanMappings.TryGetValue(timeSpan, out string? text) ? text : null;
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
        public static string? GetCountryProductIsFrom(List<Category> categoryList, string productName, string companyName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                        product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return product.CountryOfOrigin;
                    }
                }
            }
            return null;
        }
        public static string? GetCompanyProductIsFrom(List<Category> categoryList, string productName, string companyName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                        product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return product.CompanyOfOrigin;
                    }
                }
            }
            return null;
        }
        public static string? GetCategoryNameProductIsFrom(List<Category> categoryList, string productName, string companyName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                        product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return category.Name;
                    }
                }
            }
            return null;
        }
        public static bool DoesProductExistInCategory(string productName, string companyName, List<Category> categories, string categoryName)
        {
            foreach (Category category in categories)
            {
                if (category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Product product in category.ProductList)
                    {
                        if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                           product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    // Return since we found and checked the category
                    return false;
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
        public static Category? GetCategoryProductNameIsFrom(List<Category> categoryList, string productName, string companyName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                        product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return category;
                    }
                }
            }
            return null;
        }
        public static Product? GetProductProductNameIsFrom(List<Category> categoryList, string productName, string companyName)
        {
            foreach (Category category in categoryList)
            {
                foreach (Product product in category.ProductList)
                {
                    if (product.Name.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                        product.CompanyOfOrigin.Equals(companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return product;
                    }
                }
            }
            return null;
        }
        public static bool DoesCategoryHaveProducts(string categoryName, List<Category> sourceList)
        {
            Category? category = GetCategoryCategoryNameIsFrom(sourceList, categoryName);
            return category != null && category.ProductList.Count > 0;
        }
        /// <summary>
        /// Gets a list of formatted product names for items available for sale.
        /// Format: "CompanyOfOrigin > CategoryName > ProductName"
        /// </summary>
        public List<string> GetProductSaleNames() => GetFormattedProductNames(_categorySaleList);
        /// <summary>
        /// Gets a list of formatted product names for items available for purchase.
        /// Format: "CompanyOfOrigin > CategoryName > ProductName"
        /// </summary>
        public List<string> GetProductPurchaseNames() => GetFormattedProductNames(_categoryPurchaseList);
        /// <summary>
        /// Helper method to format product names from a category list.
        /// </summary>
        private static List<string> GetFormattedProductNames(List<Category> categories)
        {
            List<string> names = [];
            foreach (Category category in categories)
            {
                foreach (Product product in category.ProductList)
                {
                    names.Add($"{product.CompanyOfOrigin} > {category.Name} > {product.Name}");
                }
            }
            return names;
        }

        // DataGridView properties
        public SelectedOption Selected;
        private Guna2DataGridView _purchase_DataGridView, _sale_DataGridView, _selectedDataGridView;
        private GunaChart _purchaseTotals_Chart, _purchaseDistribution_Chart;
        private GunaChart _saleTotals_Chart, _saleDistribution_Chart;
        private GunaChart _profits_Chart;

        // DataGridView getters
        public Guna2DataGridView Purchase_DataGridView => _purchase_DataGridView;
        public Guna2DataGridView Sale_DataGridView => _sale_DataGridView;
        public Guna2DataGridView SelectedDataGridView => _selectedDataGridView;
        public GunaChart Profits_Chart => _profits_Chart;

        // DataGridView enums
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
            Analytics,
            ItemsInPurchase,
            ItemsInSale
        }
        public enum Column
        {
            ID,
            Accountant,
            Product,
            Category,
            Country,
            Company,
            Date,
            TotalItems,
            PricePerUnit,
            Shipping,
            Tax,
            Fee,
            Discount,
            ChargedDifference,
            Total,
            Note,
            HasReceipt
        }
        public readonly Dictionary<Column, string> PurchaseColumnHeaders = new()
        {
            { Column.ID, "Order #" },
            { Column.Accountant, "Accountant" },
            { Column.Product, "Product" },
            { Column.Category, "Category" },
            { Column.Country, "Country of origin" },
            { Column.Company, "Company of origin" },
            { Column.Date, "Date" },
            { Column.TotalItems, "Total items" },
            { Column.PricePerUnit, "Price per unit" },
            { Column.Shipping, "Shipping" },
            { Column.Tax, "Tax" },
            { Column.Fee, "Fee" },
            { Column.Discount, "Discount" },
            { Column.ChargedDifference, "Charged difference" },
            { Column.Total, "Total expenses" },
            { Column.Note, "Notes" },
            { Column.HasReceipt, "Has receipt" }
        };
        public readonly Dictionary<Column, string> SalesColumnHeaders = new()
        {
            { Column.ID, "Sale #" },
            { Column.Accountant, "Accountant" },
            { Column.Product, "Product" },
            { Column.Category, "Category" },
            { Column.Country, "Country of destination" },
            { Column.Company, "Company of origin" },
            { Column.Date, "Date" },
            { Column.TotalItems, "Total items" },
            { Column.PricePerUnit, "Price per unit" },
            { Column.Shipping, "Shipping" },
            { Column.Tax, "Tax" },
            { Column.Fee, "Fee" },
            { Column.Discount, "Discount" },
            { Column.ChargedDifference, "Charged difference" },
            { Column.Total, "Total revenue" },
            { Column.Note, "Notes" },
            { Column.HasReceipt, "Has receipt" }
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

        // DataGridView methods
        public IEnumerable<DataGridViewRow> GetAllRows()
        {
            return Purchase_DataGridView.Rows.Cast<DataGridViewRow>()
                .Concat(Sale_DataGridView.Rows.Cast<DataGridViewRow>());
        }
        public GunaChart GetTotalsChart()
        {
            return _sale_DataGridView.Visible ? _saleTotals_Chart : _purchaseTotals_Chart;
        }
        public GunaChart GetDistributionChart()
        {
            return _sale_DataGridView.Visible ? _saleDistribution_Chart : _purchaseDistribution_Chart;
        }

        // Total labels
        public void AlignTotalLabels()
        {
            string quantityColumn = Column.TotalItems.ToString();
            string taxColumn = Column.Tax.ToString();
            string shippingColumn = Column.Shipping.ToString();
            string feeColumn = Column.Fee.ToString();
            string discountColumn = Column.Discount.ToString();
            string chargedDifference = Column.ChargedDifference.ToString();
            string totalPriceColumn = Column.Total.ToString();

            Quantity_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = _selectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = _selectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = _selectedDataGridView.Columns[shippingColumn].Width;

            Fee_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[feeColumn].Index, -1, true).Left;
            Fee_Label.Width = _selectedDataGridView.Columns[feeColumn].Width;

            Discount_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[discountColumn].Index, -1, true).Left;
            Discount_Label.Width = _selectedDataGridView.Columns[feeColumn].Width;

            if (_selectedDataGridView == _purchase_DataGridView)
            {
                ChargedDifference_Label.Visible = true;
                ChargedDifference_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[chargedDifference].Index, -1, true).Left;
                ChargedDifference_Label.Width = _selectedDataGridView.Columns[chargedDifference].Width;
            }
            else
            {
                ChargedDifference_Label.Visible = false;
            }

            Price_Label.Left = _selectedDataGridView.GetCellDisplayRectangle(_selectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = _selectedDataGridView.Columns[totalPriceColumn].Width;
        }
        public void UpdateTotalLabels()
        {
            if (Selected != SelectedOption.Purchases && Selected != SelectedOption.Sales)
            {
                return;
            }

            Total_Panel.Visible = DataGridViewManager.HasVisibleRows(_selectedDataGridView);

            int totalVisibleRows = 0,
                totalQuantity = 0;

            decimal totalTax = 0,
                totalShipping = 0,
                fee = 0,
                discount = 0,
                chargedDifference = 0,
                totalPrice = 0;

            foreach (DataGridViewRow row in _selectedDataGridView.Rows)
            {
                if (!row.Visible) { continue; }
                totalVisibleRows++;

                totalQuantity += Convert.ToInt32(row.Cells[Column.TotalItems.ToString()].Value);
                totalTax += Convert.ToDecimal(row.Cells[Column.Tax.ToString()].Value);
                totalShipping += Convert.ToDecimal(row.Cells[Column.Shipping.ToString()].Value);
                fee += Convert.ToDecimal(row.Cells[Column.Fee.ToString()].Value);
                discount += Convert.ToDecimal(row.Cells[Column.Discount.ToString()].Value);
                if (Selected == SelectedOption.Purchases)
                {
                    chargedDifference += Convert.ToDecimal(row.Cells[Column.ChargedDifference.ToString()].Value);
                }
                totalPrice += Convert.ToDecimal(row.Cells[Column.Total.ToString()].Value);
            }

            LabelManager.ShowTotalsWithTransactions(Total_Label, _selectedDataGridView);
            Quantity_Label.Text = totalQuantity.ToString();
            Tax_Label.Text = totalTax.ToString("C");
            Shipping_Label.Text = totalShipping.ToString("C");
            Fee_Label.Text = fee.ToString("C");
            Discount_Label.Text = discount.ToString("C");
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

                // Iterate through all cells except the last one ("Has receipt" column)
                for (int i = 0; i < row.Cells.Count - 1; i++)
                {
                    DataGridViewCell cell = row.Cells[i];
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

        // Chart properties
        private List<Control> analyticsControls;
        private GunaChart _countriesOfOrigin_Chart, _countriesOfDestination_Chart, _companiesOfOrigin_Chart, _accountants_Chart,
            _growthRates_Chart, _salesVsExpenses_Chart, _averageTransactionValue_Chart, _totalTransactions_Chart, _averageShippingCosts_Chart;
        private Guna2CustomCheckBox _includeFreeShipping_CheckBox;
        private Label _includeFreeShipping_Label;

        public enum ChartDataType
        {
            TotalRevenue,
            DistributionOfRevenue,
            TotalProfits,
            CountriesOfOrigin,
            CompaniesOfOrigin,
            CountriesOfDestination,
            Accountants,
            TotalExpensesVsSales,
            AverageOrderValue,
            TotalTransactions,
            AverageShippingCosts,
            GrowthRates
        }
        public GunaChart CountriesOfOrigin_Chart => _countriesOfOrigin_Chart;
        public GunaChart CountriesOfDestination_Chart => _countriesOfDestination_Chart;
        public GunaChart CompaniesOfOrigin_Chart => _companiesOfOrigin_Chart;
        public GunaChart Accountants_Chart => _accountants_Chart;
        public GunaChart GrowthRates_Chart => _growthRates_Chart;
        public GunaChart SalesVsExpenses_Chart => _salesVsExpenses_Chart;
        public GunaChart AverageTransactionValue_Chart => _averageTransactionValue_Chart;
        public GunaChart TotalTransactions_Chart => _totalTransactions_Chart;
        public GunaChart AverageShippingCosts_Chart => _averageShippingCosts_Chart;
        public Guna2CustomCheckBox IncludeFreeShipping_CheckBox => _includeFreeShipping_CheckBox;

        // Analytics charts methods
        private List<Control> GetMainControlsList()
        {
            return [
                _sale_DataGridView,
                _purchase_DataGridView,
                _purchaseTotals_Chart,
                _purchaseDistribution_Chart,
                _saleTotals_Chart,
                _saleDistribution_Chart,
                _profits_Chart,
                Total_Panel
            ];
        }
        private void ShowMainControls()
        {
            if (analyticsControls == null) { return; }

            foreach (Control control in GetMainControlsList())
            {
                control.Visible = true;
            }
            foreach (Control control in analyticsControls)
            {
                control.Visible = false;
            }
        }

        /// <summary>
        /// Constructs analytics charts if they have not been constructed already.
        /// </summary>
        private void ConstructControlsForAnalytics()
        {
            _countriesOfOrigin_Chart = ConstructAnalyticsChart("countriesOfOrigin_Chart");
            _companiesOfOrigin_Chart = ConstructAnalyticsChart("companiesOfOrigin_Chart");
            _countriesOfDestination_Chart = ConstructAnalyticsChart("countriesOfDestination_Chart");
            _accountants_Chart = ConstructAnalyticsChart("accountants_Chart");
            _salesVsExpenses_Chart = ConstructAnalyticsChart("salesVsExpenses_Chart");
            _averageTransactionValue_Chart = ConstructAnalyticsChart("averageOrderValue_Chart");
            _totalTransactions_Chart = ConstructAnalyticsChart("totalTransactions_Chart");
            _averageShippingCosts_Chart = ConstructAnalyticsChart("averageShippingCosts_Chart");
            _growthRates_Chart = ConstructAnalyticsChart("growthRates_Chart");

            _includeFreeShipping_CheckBox = new Guna2CustomCheckBox
            {
                Size = new Size(20, 20)
            };
            _includeFreeShipping_CheckBox.CheckedChanged += IncludeFreeShippingCheckBox_CheckedChanged;
            Controls.Add(_includeFreeShipping_CheckBox);

            _includeFreeShipping_Label = new Label
            {
                Text = "Include transactions with free shipping",
                Name = "IncludeFreeShipping_Label",  // This is needed for language translation
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(5),
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _includeFreeShipping_Label.Click += IncludeFreeShipping_Label_Click;
            Controls.Add(_includeFreeShipping_Label);

            analyticsControls =
            [
                _countriesOfOrigin_Chart,
                _companiesOfOrigin_Chart,
                _countriesOfDestination_Chart,
                _accountants_Chart,
                _salesVsExpenses_Chart,
                _averageTransactionValue_Chart,
                _totalTransactions_Chart,
                _averageShippingCosts_Chart,
                _growthRates_Chart,
                _includeFreeShipping_CheckBox,
                _includeFreeShipping_Label
            ];

            MouseClickChartManager.InitCharts(analyticsControls.OfType<GunaChart>().ToArray());
        }
        private void IncludeFreeShipping_Label_Click(object? sender, EventArgs e)
        {
            IncludeFreeShipping_CheckBox.Checked = !IncludeFreeShipping_CheckBox.Checked;
        }
        private void IncludeFreeShippingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isLineChart = LineGraph_ToggleSwitch.Checked;
            bool zeroShipping = IncludeFreeShipping_CheckBox.Checked;

            UserSettings.UpdateSetting("Include free shipping in chart", Properties.Settings.Default.IncludeFreeShipping, zeroShipping,
             value => Properties.Settings.Default.IncludeFreeShipping = value);

            LoadChart.LoadAverageShippingCostsChart(_averageShippingCosts_Chart, isLineChart, includeZeroShipping: zeroShipping);
        }
        private GunaChart ConstructAnalyticsChart(string name)
        {
            GunaChart gunaChart = new()
            {
                Name = name,  // This is needed for the language translation
                Height = 490
            };

            gunaChart.ApplyConfig(ChartColors.Config(), CustomColors.ContentPanelBackground);
            LoadChart.ConfigureChartForPie(gunaChart);
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
        private void ShowAnalyticsControls()
        {
            LoadOrRefreshAnalyticsCharts(false);

            foreach (Control control in analyticsControls)
            {
                control.Visible = true;
            }
            foreach (Control control in GetMainControlsList())
            {
                control.Visible = false;
            }
        }
        public void LoadOrRefreshAnalyticsCharts(bool onlyRefreshForLineCharts)
        {
            bool isLineChart = LineGraph_ToggleSwitch.Checked;

            if (!onlyRefreshForLineCharts)
            {
                LoadChart.LoadCountriesOfOriginForProductsIntoChart(_countriesOfOrigin_Chart, PieChartGrouping.Top8);
                _countriesOfOrigin_Chart.Title.Text = TranslatedChartTitles.CountriesOfOrigin;
                LanguageManager.UpdateLanguageForControl(_countriesOfOrigin_Chart);

                LoadChart.LoadCompaniesOfOriginForProductsIntoChart(_companiesOfOrigin_Chart, PieChartGrouping.Top8);
                _companiesOfOrigin_Chart.Title.Text = TranslatedChartTitles.CompaniesOfOrigin;
                LanguageManager.UpdateLanguageForControl(_companiesOfOrigin_Chart);

                LoadChart.LoadCountriesOfDestinationForProductsIntoChart(_countriesOfDestination_Chart, PieChartGrouping.Top8);
                _countriesOfDestination_Chart.Title.Text = TranslatedChartTitles.CountriesOfDestination;
                LanguageManager.UpdateLanguageForControl(_countriesOfDestination_Chart);

                LoadChart.LoadAccountantsIntoChart(_accountants_Chart, PieChartGrouping.Top8);
                _accountants_Chart.Title.Text = TranslatedChartTitles.AccountantsTransactions;
                LanguageManager.UpdateLanguageForControl(_accountants_Chart);

                LoadChart.LoadGrowthRateChart(_growthRates_Chart);
                _growthRates_Chart.Title.Text = TranslatedChartTitles.GrowthRates;
                LanguageManager.UpdateLanguageForControl(_growthRates_Chart);
            }

            LoadChart.LoadSalesVsExpensesChart(_salesVsExpenses_Chart, isLineChart);
            _salesVsExpenses_Chart.Title.Text = TranslatedChartTitles.SalesVsExpenses;
            LanguageManager.UpdateLanguageForControl(_salesVsExpenses_Chart);

            LoadChart.LoadTotalTransactionsChart(_totalTransactions_Chart, isLineChart);
            _totalTransactions_Chart.Title.Text = TranslatedChartTitles.TotalTransactions;
            LanguageManager.UpdateLanguageForControl(_totalTransactions_Chart);

            LoadChart.LoadAverageTransactionValueChart(_averageTransactionValue_Chart, isLineChart);
            _averageTransactionValue_Chart.Title.Text = TranslatedChartTitles.AverageTransactionValue;
            LanguageManager.UpdateLanguageForControl(_averageTransactionValue_Chart);

            LoadChart.LoadAverageShippingCostsChart(_averageShippingCosts_Chart, isLineChart, includeZeroShipping: IncludeFreeShipping_CheckBox.Checked);
            _averageShippingCosts_Chart.Title.Text = TranslatedChartTitles.AverageShippingCosts;
            LanguageManager.UpdateLanguageForControl(_averageShippingCosts_Chart);
        }

        // Misc.
        public static List<Guna2Panel> GetMenus()
        {
            return new List<Guna2Panel>
            {
                CustomControls.FileMenu,
                CustomControls.RecentlyOpenedMenu,
                CustomControls.HelpMenu,
                CustomControls.ControlDropDown_Panel,
                GetStarted_Form.RightClickOpenRecent_Panel,
                DataGridViewManager.RightClickDataGridView_Panel,
                RightClickGunaChartMenu.RightClickGunaChart_Panel
            }.Where(panel => panel != null).ToList();
        }
        private void UpdateMainMenuFormText()
        {
            Text = $"Argo Sales Tracker {Tools.GetVersionNumber()} - {Directories.CompanyName}";
        }
        public void ClosePanels()
        {
            DataGridViewManager.RightClickDataGridView_Panel.Parent?.Controls.Remove(DataGridViewManager.RightClickDataGridView_Panel);
            Controls.Remove(RightClickGunaChartMenu.RightClickGunaChart_Panel);
            DataGridViewManager.DoNotDeleteRows = false;
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}