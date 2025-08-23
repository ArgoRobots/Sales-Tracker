using Guna.UI2.WinForms;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.ComponentModel;
using System.Reflection;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Admin mode can only be enabled by directly setting it to true here
        public static bool IsAdminMode { get; } = true && Tools.IsRunningInVisualStudio();

        // Properties
        private static MainMenu_Form _instance;
        public static readonly string _noteTextKey = "note", _rowTagKey = "RowTag", _itemsKey = "Items", _purchaseDataKey = "PurchaseData", _tagKey = "Tag";
        private static int _chartTop, _analyticChartTop;
        private static Guna2Button _upgrade_Button;

        // Getters and setters
        public static MainMenu_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile { get; } = [];
        public static List<string> SettingsThatHaveChangedInFile { get; } = [];
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string CurrencySymbol { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool IsProgramLoading { get; set; }
        public static string SelectedAccountant { get; set; }

        // Init.
        public MainMenu_Form()
        {
            InitializeComponent();
            _instance = this;

            IsProgramLoading = true;
            CurrencySymbol = Currency.GetSymbol();

            CustomControls.ConstructControls();
            ConstructMainCharts();
            ConstructControlsForAnalytics();
            InitiateSearchTimer();
            SetCompanyLabel();
            LoadData();
            LoadCustomColumnHeaders();
            CreateUpgradeButtonIfNeeded();
            CompanyLogo.SetCompanyLogo();
            UpdateTheme();
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            SetToolTips();
            HideShowingResultsForLabel();
            ConstructTimeRangePanel();
            InitChartTags();
            AddEventHandlersToTextBoxes();
            AnimateButtons();
            AnimateCharts();
            InitializeAISearch();
            UpdateMainMenuFormText();
            ScaleButtonImages();
            _ = AnonymousDataManager.TryUploadDataOnStartupAsync();
            AnonymousDataManager.TrackSessionStart();
            NetSparkleUpdateManager.CheckForUpdates();

            _chartTop = Purchases_Button.Bottom + 20;
            _analyticChartTop = _analyticsTabButtons_Panel.Bottom + 20;
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void ConstructMainCharts()
        {
            PurchaseTotals_Chart = ConstructMainChart("purchaseTotals_Chart", true);
            PurchaseDistribution_Chart = ConstructMainChart("purchaseDistribution_Chart", false);
            SaleTotals_Chart = ConstructMainChart("saleTotals_Chart", true);
            SaleDistribution_Chart = ConstructMainChart("saleDistribution_Chart", false);
            Profits_Chart = ConstructMainChart("profits_Chart", true);

            LoadChart.ConfigureChartForPie(PurchaseDistribution_Chart as PieChart);
            LoadChart.ConfigureChartForPie(SaleDistribution_Chart as PieChart);

            MouseClickChartManager.InitCharts([
                PurchaseTotals_Chart, PurchaseDistribution_Chart,
                SaleTotals_Chart, SaleDistribution_Chart, Profits_Chart
            ]);
        }
        private Chart ConstructMainChart(string name, bool isCartesian)
        {
            Chart chart;

            if (isCartesian)
            {
                CartesianChart cartesianChart = new()
                {
                    Name = name,
                    Title = CreateChartTitle("")
                };

                chart = cartesianChart;
            }
            else
            {
                PieChart pieChart = new()
                {
                    Name = name,
                    Title = CreateChartTitle("")
                };

                chart = pieChart;
            }

            // Enable double buffering
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(chart, true, null);

            Controls.Add(chart);
            return chart;
        }
        private void SetToolTips()
        {
            CustomTooltip.SetToolTip(File_Button, "", "File");
            CustomTooltip.SetToolTip(Save_Button, "", "Save");
            CustomTooltip.SetToolTip(Help_Button, "", "Help");
        }
        public void ResetData()
        {
            CategoryPurchaseList.Clear();
            CategorySaleList.Clear();

            AccountantList.Clear();
            CompanyList.Clear();

            Purchase_DataGridView.Rows.Clear();
            Sale_DataGridView.Rows.Clear();

            Search_TextBox.Clear();
            SortFromDate = default;
            SortToDate = default;
        }
        public void LoadData()
        {
            LoadCategoriesFromFile(Directories.CategoryPurchases_file, CategoryPurchaseList);
            LoadCategoriesFromFile(Directories.CategorySales_file, CategorySaleList);

            AccountantList = Directories.ReadAllLinesInFile(Directories.Accountants_file).ToList();
            CompanyList = Directories.ReadAllLinesInFile(Directories.Companies_file).ToList();

            if (Purchase_DataGridView == null)
            {
                Size size = new(1300, 350);
                Purchase_DataGridView = new();
                DataGridViewManager.InitializeDataGridView(Purchase_DataGridView, "purchases_DataGridView", size, PurchaseColumnHeaders, null, this);
                Purchase_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;

                Sale_DataGridView = new();
                DataGridViewManager.InitializeDataGridView(Sale_DataGridView, "sales_DataGridView", size, SalesColumnHeaders, null, this);
                Sale_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;
            }

            SetHasReceiptColumnVisibilty();

            AddRowsFromFile(Purchase_DataGridView, SelectedOption.Purchases);
            AddRowsFromFile(Sale_DataGridView, SelectedOption.Sales);
        }
        private void LoadCustomColumnHeaders()
        {
            DataGridViewColumn chargedDifferenceColumn = Purchase_DataGridView.Columns[Column.ChargedDifference.ToString()];
            string existingHeaderText = chargedDifferenceColumn.HeaderText;
            string messageBoxText = "Having a charged difference is common and is usually due to taxes, duties, bank fees, exchange rate differences, or politicy variations across countries.";
            chargedDifferenceColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);

            DataGridViewColumn totalColumn = Sale_DataGridView.Columns[Column.Total.ToString()];
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
            bool isLine = LineChart_ToggleSwitch.Checked;

            // Load purchase charts
            if (Selected == SelectedOption.Analytics || Purchase_DataGridView.Visible)
            {
                ChartData totalsData = LoadChart.LoadTotalsIntoChart(Purchase_DataGridView, PurchaseTotals_Chart as CartesianChart, isLine);
                string translatedExpenses = LanguageManager.TranslateString("Total expenses");
                SetChartTitle(PurchaseTotals_Chart, $"{translatedExpenses}: {CurrencySymbol}{totalsData.Total:N2}");

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(Purchase_DataGridView, PurchaseDistribution_Chart as PieChart, PieChartGrouping.Top12);
                    SetChartTitle(PurchaseDistribution_Chart, TranslatedChartTitles.ExpensesDistribution);
                }
            }

            // Load sale charts
            if (Selected == SelectedOption.Analytics || Sale_DataGridView.Visible)
            {
                ChartData totalsData = LoadChart.LoadTotalsIntoChart(Sale_DataGridView, SaleTotals_Chart as CartesianChart, isLine);
                string translatedRevenue = LanguageManager.TranslateString("Total revenue");
                SetChartTitle(SaleTotals_Chart, $"{translatedRevenue}: {CurrencySymbol}{totalsData.Total:N2}");

                if (!onlyLoadForLineCharts)
                {
                    LoadChart.LoadDistributionIntoChart(Sale_DataGridView, SaleDistribution_Chart as PieChart, PieChartGrouping.Top12);
                    SetChartTitle(SaleDistribution_Chart, TranslatedChartTitles.RevenueDistribution);
                }
            }

            // Always load profits chart
            ChartData profitsData = LoadChart.LoadProfitsIntoChart(Profits_Chart as CartesianChart, isLine);
            SetProfitsChartTitle(profitsData.Total);
        }
        public static void SetChartTitle(Chart chart, string title)
        {
            chart.Title = CreateChartTitle(title);
        }
        private static LabelVisual CreateChartTitle(string text) => new()
        {
            Text = text,
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = ChartColors.CreateSolidColorPaint(ChartColors.ToSKColor(CustomColors.Text))
        };
        private void SetProfitsChartTitle(double total)
        {
            string translatedPrefix = LanguageManager.TranslateString("Total profits");
            SetChartTitle(Profits_Chart, $"{translatedPrefix}: {CurrencySymbol}{total:N2}");
        }
        public void UpdateChartCurrencyFormats()
        {
            List<Control> mainCharts = [PurchaseTotals_Chart, SaleTotals_Chart, Profits_Chart];
            List<Control> analyticsCharts = [SalesVsExpenses_Chart, AverageTransactionValue_Chart, AverageShippingCosts_Chart];

            // Combine charts based on current view
            List<Control> chartsToUpdate = [.. mainCharts];
            if (Selected == SelectedOption.Analytics)
            {
                chartsToUpdate.AddRange(analyticsCharts);
            }

            // Update currency formats for all relevant charts
            foreach (Control chart in chartsToUpdate)
            {
                if (chart is CartesianChart cartesianChart)
                {
                    LoadChart.ApplyCurrencyFormatToChart(cartesianChart);
                    cartesianChart.Invalidate();
                }
            }
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
            if (_upgrade_Button != null)
            {
                _upgrade_Button.FillColor = CustomColors.ToolbarBackground;
            }
            Help_Button.FillColor = CustomColors.ToolbarBackground;

            ReselectButton();
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
            LineChart_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRight;

            // Chart titles are saved in cache using a string, not the control
            PurchaseTotals_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            PurchaseDistribution_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            SaleTotals_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            SaleDistribution_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            Profits_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;

            CountriesOfOrigin_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            CountriesOfDestination_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            CompaniesOfOrigin_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            WorldMap_GeoMap.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            Accountants_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            GrowthRates_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            SalesVsExpenses_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            AverageTransactionValue_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            TotalTransactions_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            AverageShippingCosts_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            ReturnsOverTime_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            ReturnReasons_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            ReturnFinancialImpact_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            ReturnsByCategory_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            ReturnsByProduct_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            PurchaseVsSaleReturns_Chart.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
        }
        private void InitChartTags()
        {
            // Main charts
            PurchaseTotals_Chart.Tag = ChartDataType.TotalPurchases;
            PurchaseDistribution_Chart.Tag = ChartDataType.DistributionOfPurchases;
            SaleTotals_Chart.Tag = ChartDataType.TotalSales;
            SaleDistribution_Chart.Tag = ChartDataType.DistributionOfSales;
            Profits_Chart.Tag = ChartDataType.TotalProfits;

            // Analytics charts
            CountriesOfOrigin_Chart.Tag = ChartDataType.CountriesOfOrigin;
            CompaniesOfOrigin_Chart.Tag = ChartDataType.CompaniesOfOrigin;
            CountriesOfDestination_Chart.Tag = ChartDataType.CountriesOfDestination;
            WorldMap_GeoMap.Tag = ChartDataType.WorldMap;
            Accountants_Chart.Tag = ChartDataType.Accountants;
            GrowthRates_Chart.Tag = ChartDataType.GrowthRates;
            SalesVsExpenses_Chart.Tag = ChartDataType.TotalExpensesVsSales;
            TotalTransactions_Chart.Tag = ChartDataType.TotalTransactions;
            AverageTransactionValue_Chart.Tag = ChartDataType.AverageOrderValue;
            AverageShippingCosts_Chart.Tag = ChartDataType.AverageShippingCosts;
            ReturnsOverTime_Chart.Tag = ChartDataType.ReturnsOverTime;
            ReturnReasons_Chart.Tag = ChartDataType.ReturnReasons;
            ReturnFinancialImpact_Chart.Tag = ChartDataType.ReturnFinancialImpact;
            ReturnsByCategory_Chart.Tag = ChartDataType.ReturnsByCategory;
            ReturnsByProduct_Chart.Tag = ChartDataType.ReturnsByProduct;
            PurchaseVsSaleReturns_Chart.Tag = ChartDataType.PurchaseVsSaleReturns;
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Search_TextBox);
        }
        public void CreateUpgradeButtonIfNeeded()
        {
            if (!Properties.Settings.Default.LicenseActivated)
            {
                float scale = DpiHelper.GetRelativeDpiScale();
                int height = Top_Panel.Height;
                int scaledImageSize = (int)(32 * scale);

                _upgrade_Button = new Guna2Button
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Image = Resources.Upgrade,
                    ImageSize = new Size(scaledImageSize, scaledImageSize),
                    Size = new Size(height, height),
                    Left = Help_Button.Left - height
                };
                _upgrade_Button.Click += Upgrade_Button_Click;
                Top_Panel.Controls.Add(_upgrade_Button);
                CustomTooltip.SetToolTip(_upgrade_Button, "", "Upgrade to full version");
            }
        }
        public void RemoveUpgradeButton()
        {
            Top_Panel.Controls.Remove(_upgrade_Button);
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
               Products_Button,
               Categories_Button,
               Companies_Button,
               Accountants_Button,
               TimeRange_Button,
            ];
            CustomControls.AnimateButtons(buttons);
        }
        public void AnimateCharts()
        {
            bool enableAnimations = Properties.Settings.Default.AnimateCharts;

            // Apply to all charts
            foreach (Control chart in GetAllCharts())
            {
                switch (chart)
                {
                    case CartesianChart cartesianChart:
                        cartesianChart.EasingFunction = enableAnimations ? LiveChartsCore.EasingFunctions.QuadraticOut : null;
                        break;
                    case PieChart pieChart:
                        pieChart.EasingFunction = enableAnimations ? LiveChartsCore.EasingFunctions.QuadraticOut : null;
                        break;
                }
            }
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
        private void ScaleButtonImages()
        {
            DpiHelper.ScaleImageSize(File_Button);
            DpiHelper.ScaleImageSize(Save_Button);
            DpiHelper.ScaleImageSize(Help_Button);
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

            bool hasVisibleRows = DataGridViewManager.HasVisibleRows(dataGridView);
            LabelManager.ManageNoDataLabelOnControl(hasVisibleRows, dataGridView);

            ApplyReturnAppearancesToAllRows(dataGridView);
        }
        private static void ApplyReturnAppearancesToAllRows(Guna2DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                bool isFullyReturned = ReturnManager.IsTransactionFullyReturned(row);
                bool isPartiallyReturned = ReturnManager.IsTransactionPartiallyReturned(row);

                if (isFullyReturned || isPartiallyReturned)
                {
                    ReturnManager.UpdateRowAppearanceForReturn(row, isFullyReturned, isPartiallyReturned);
                }
            }
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
            // Cast to object[] to match the expected parameter type
            int rowIndex = dataGridView.Rows.Add(cellValues.Cast<object>().ToArray());

            ProcessRowTag(dataGridView, rowData, rowIndex);
            ProcessNoteText(dataGridView, rowData, rowIndex);
            ProcessHasReceipt(dataGridView, rowIndex);
        }
        private static string?[] ExtractCellValues(Dictionary<string, object> rowData)
        {
            return rowData
                .Where(kv => kv.Key != _rowTagKey && kv.Key != _noteTextKey)
                .Select(kv => kv.Value?.ToString())
                .ToArray();
        }
        private static void ProcessRowTag(Guna2DataGridView dataGridView, Dictionary<string, object> rowData, int rowIndex)
        {
            if (!rowData.TryGetValue(_rowTagKey, out object value) || value is not JObject jsonObject)
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

            if (!tagObject.TryGetValue(_itemsKey, out object? itemsElement) || itemsElement is not JArray itemsArray)
            {
                return false;
            }

            if (!tagObject.TryGetValue(_purchaseDataKey, out object? purchaseDataElement) || purchaseDataElement is not JObject purchaseDataObject)
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

            if (!tagObject.TryGetValue(_tagKey, out object? tagStringElement) ||
                !tagObject.TryGetValue(_purchaseDataKey, out object? purchaseDataElement))
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
            if (!rowData.TryGetValue(_noteTextKey, out object noteValue))
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

            SetHasReceiptColumn(row, receipt);
        }
        public static void SetHasReceiptColumn(DataGridViewRow row, string? receipt)
        {
            if (!Properties.Settings.Default.ShowHasReceiptColumn) { return; }

            string newReceipt = receipt != null ? ReceiptManager.ProcessReceiptTextFromRowTag(receipt) : null;
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
            Sale_DataGridView.ClearSelection();

            IsProgramLoading = false;

            SortTheDataGridViewByDate();
            CenterAndResizeControls();

            // Total_Panel only needs to be set once
            Total_Panel.Location = new Point(SelectedDataGridView.Left, SelectedDataGridView.Top + SelectedDataGridView.Height);
            Total_Panel.Width = SelectedDataGridView.Width;
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
                    // User declined the EULA, the application will exit in EULA_Form.Decline_Button_Click()
                    return;
                }
            }

            // Check if the welcome form should be shown
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

            CompanyLogo.Cleanup();
            AnonymousDataManager.TrackSessionEnd();
            ThemeChangeDetector.StopListeningForThemeChanges();
            Log.SaveLogs();
            ArgoCompany.ApplicationMutex?.Dispose();
            Directories.DeleteDirectory(Directories.TempCompany_dir, true);
            Environment.Exit(0);  // Make sure all processes are fully closed
        }

        // Center and resize controls
        public void CenterAndResizeControls()
        {
            if (IsProgramLoading) { return; }

            byte spaceBetweenCharts = 20, chartWidthOffset = 35;

            // Handle dropdown menu for narrow windows
            if (ClientSize.Width < 1500 + Edit_Button.Left + Edit_Button.Width)
            {
                AddControlsDropDown();
            }
            else if (MainTop_Panel.Controls.Contains(CustomControls.ControlsDropDown_Button))
            {
                RemoveControlsDropDown();
            }

            if (Search_TextBox.Text != "")
            {
                CenterShowingResultsLabel();
            }

            if (Selected == SelectedOption.Analytics)
            {
                LayoutChartsForTab(_selectedTabKey, spaceBetweenCharts);
            }
            else
            {
                int chartHeight = ClientSize.Height switch
                {
                    > 1400 => 500,
                    > 1000 => 400,
                    _ => 300
                };
                SetMainChartsHeight(chartHeight);

                // Regular view layout for main charts
                int chartWidth = ClientSize.Width / 3 - chartWidthOffset;

                // Calculate X positions for charts
                int leftX = (ClientSize.Width - 3 * chartWidth - spaceBetweenCharts * 2) / 2;
                int middleX = leftX + chartWidth + spaceBetweenCharts;
                int rightX = middleX + chartWidth + spaceBetweenCharts;

                // Position the currently visible charts
                Control totalsChart = Sale_DataGridView.Visible ? SaleTotals_Chart : PurchaseTotals_Chart;
                Control distributionChart = Sale_DataGridView.Visible ? SaleDistribution_Chart : PurchaseDistribution_Chart;

                SetChartPosition(totalsChart, new Size(chartWidth, chartHeight), leftX, _chartTop);
                SetChartPosition(distributionChart, new Size(chartWidth, chartHeight), middleX, _chartTop);
                SetChartPosition(Profits_Chart, new Size(chartWidth, chartHeight), rightX, _chartTop);

                // Position DataGridView
                SelectedDataGridView.Size = new Size(ClientSize.Width - 65,
                    ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - chartHeight - totalsChart.Top + 35);
                SelectedDataGridView.Location = new Point((ClientSize.Width - SelectedDataGridView.Width) / 2,
                    ClientSize.Height - MainTop_Panel.Height - Top_Panel.Height - SelectedDataGridView.Height + 50);
            }
        }
        private void LayoutChartsForTab(AnalyticsTab tabKey, int spacing)
        {
            int startY = _analyticChartTop;
            int availableHeight = ClientSize.Height - startY - 10;
            int availableWidth = ClientSize.Width - 80;
            const int maxChartWidth = 800;
            const int maxChartHeight = 550;

            List<Control> charts = _tabControls[tabKey];

            switch (tabKey)
            {
                case AnalyticsTab.Overview:
                    // 2x2 grid for overview
                    if (charts.Count >= 4)
                    {
                        int chartWidth = Math.Min(maxChartWidth, (availableWidth - spacing) / 2);
                        int chartHeight = Math.Min(maxChartHeight, (availableHeight - spacing) / 2);

                        int startX = (ClientSize.Width - (chartWidth * 2 + spacing)) / 2;

                        SetChartPosition(charts[0], new Size(chartWidth, chartHeight), startX, startY);
                        SetChartPosition(charts[1], new Size(chartWidth, chartHeight), startX + chartWidth + spacing, startY);
                        SetChartPosition(charts[2], new Size(chartWidth, chartHeight), startX, startY + chartHeight + spacing);
                        SetChartPosition(charts[3], new Size(chartWidth, chartHeight), startX + chartWidth + spacing, startY + chartHeight + spacing);
                    }
                    break;

                case AnalyticsTab.Geographic:
                    // Position controls at the top
                    WorldMapControls_Panel.Location = new Point((
                        ClientSize.Width - WorldMapControls_Panel.Width) / 2,
                        _analyticsTabButtons_Panel.Bottom + 10);

                    if (charts.Count >= 4)
                    {
                        Control geoMap = charts[0];

                        // GeoMap takes top 60% of space
                        int geoMapY = WorldMapControls_Panel.Bottom + 10;
                        int geoMapHeight = (int)((availableHeight - WorldMapControls_Panel.Height - 20) * 0.6);
                        int geoMapWidth = availableWidth;

                        SetChartPosition(geoMap, new Size(geoMapWidth, geoMapHeight),
                            (ClientSize.Width - geoMapWidth) / 2, geoMapY);

                        // Pie charts take remaining space with max constraints
                        List<Control> pieCharts = charts.Skip(1).Take(3).ToList();
                        if (pieCharts.Count == 3)
                        {
                            int smallChartWidth = Math.Min(maxChartWidth, (availableWidth - (2 * spacing)) / 3);
                            int smallChartHeight = Math.Min(maxChartHeight, availableHeight - geoMapHeight - WorldMapControls_Panel.Height - 30);
                            int smallChartsY = geoMapY + geoMapHeight + spacing;
                            int smallChartsStartX = (ClientSize.Width - (smallChartWidth * 3 + spacing * 2)) / 2;

                            for (int i = 0; i < pieCharts.Count; i++)
                            {
                                int x = smallChartsStartX + (smallChartWidth + spacing) * i;
                                SetChartPosition(pieCharts[i], new Size(smallChartWidth, smallChartHeight), x, smallChartsY);
                            }
                        }
                    }
                    break;

                case AnalyticsTab.Financial:
                    // 2x2 grid for financial
                    if (charts.Count >= 4)
                    {
                        int chartWidth = Math.Min(maxChartWidth, (availableWidth - spacing) / 2);
                        int chartHeight = Math.Min(maxChartHeight, (availableHeight - spacing) / 2);

                        int startX = (ClientSize.Width - (chartWidth * 2 + spacing)) / 2;

                        for (int i = 0; i < 4; i++)
                        {
                            int x = startX + (chartWidth + spacing) * (i % 2);
                            int y = startY + (chartHeight + spacing) * (i / 2);
                            SetChartPosition(charts[i], new Size(chartWidth, chartHeight), x, y);
                        }
                    }
                    break;

                case AnalyticsTab.Performance:
                    // 3 charts in a row
                    if (charts.Count >= 3)
                    {
                        int chartWidth = Math.Min(maxChartWidth, (availableWidth - (2 * spacing)) / 3);
                        int chartHeight = Math.Min(maxChartHeight, availableHeight);

                        int startX = (ClientSize.Width - (chartWidth * 3 + spacing * 2)) / 2;

                        for (int i = 0; i < 3; i++)
                        {
                            int x = startX + (chartWidth + spacing) * i;
                            SetChartPosition(charts[i], new Size(chartWidth, chartHeight), x, startY);
                        }
                    }
                    break;

                case AnalyticsTab.Operational:
                    // 2 charts side by side
                    if (charts.Count >= 2)
                    {
                        int chartWidth = Math.Min(maxChartWidth, (availableWidth - spacing) / 2);
                        int chartHeight = Math.Min(maxChartHeight, availableHeight - 50);  // Leave space for controls

                        int startX = (ClientSize.Width - (chartWidth * 2 + spacing)) / 2;

                        SetChartPosition(charts[0], new Size(chartWidth, chartHeight), startX, startY);
                        SetChartPosition(charts[1], new Size(chartWidth, chartHeight), startX + chartWidth + spacing, startY);

                        // Position controls below the shipping chart
                        int shippingChartX = startX + chartWidth + spacing;
                        int controlsY = startY + chartHeight + 20;

                        IncludeFreeShipping_CheckBox.Location = new Point(shippingChartX, controlsY);

                        int labelX = IncludeFreeShipping_CheckBox.Right - 2;
                        int labelY = IncludeFreeShipping_CheckBox.Top + (IncludeFreeShipping_CheckBox.Height / 2) - (_includeFreeShipping_Label.Height / 2);
                        _includeFreeShipping_Label.Location = new Point(labelX, labelY);
                    }
                    break;

                case AnalyticsTab.Returns:
                    // 2x3 grid layout
                    if (charts.Count >= 6)
                    {
                        int chartWidth = Math.Min(maxChartWidth, (availableWidth - (2 * spacing)) / 3);
                        int chartHeight = Math.Min(maxChartHeight, (availableHeight - spacing) / 2);

                        int startX = (ClientSize.Width - (chartWidth * 3 + spacing * 2)) / 2;

                        // Top row - 3 charts
                        SetChartPosition(charts[0], new Size(chartWidth, chartHeight), startX, startY);
                        SetChartPosition(charts[1], new Size(chartWidth, chartHeight), startX + chartWidth + spacing, startY);
                        SetChartPosition(charts[2], new Size(chartWidth, chartHeight), startX + (chartWidth + spacing) * 2, startY);

                        // Bottom row - 3 charts
                        SetChartPosition(charts[3], new Size(chartWidth, chartHeight), startX, startY + chartHeight + spacing);
                        SetChartPosition(charts[4], new Size(chartWidth, chartHeight), startX + chartWidth + spacing, startY + chartHeight + spacing);
                        SetChartPosition(charts[5], new Size(chartWidth, chartHeight), startX + (chartWidth + spacing) * 2, startY + chartHeight + spacing);
                    }
                    break;
            }
        }
        private void SetMainChartsHeight(int height)
        {
            PurchaseTotals_Chart.Height = height;
            PurchaseDistribution_Chart.Height = height;
            SaleTotals_Chart.Height = height;
            SaleDistribution_Chart.Height = height;
            Profits_Chart.Height = height;
        }
        private static void SetChartPosition(Control chart, Size size, int left, int top)
        {
            chart.Size = size;
            chart.Location = new Point(left, top);

            if (chart is PieChart pieChart)
            {
                pieChart.LegendPosition = LoadChart.GetLegendPosition(pieChart);
            }
        }
        private void AddControlsDropDown()
        {
            if (!MainTop_Panel.Controls.Contains(CustomControls.ControlsDropDown_Button))
            {
                CustomControls.ControlsDropDown_Button.Location = new Point(
                    MainTop_Panel.Width - CustomControls.ControlsDropDown_Button.Width - 11,
                    (MainTop_Panel.Height - CustomControls.ControlsDropDown_Button.Height) / 2);

                MainTop_Panel.Controls.Add(CustomControls.ControlsDropDown_Button);

                foreach (Control button in GetMainTopButtons())
                {
                    button.Visible = false;
                }
            }
        }
        private void RemoveControlsDropDown()
        {
            if (MainTop_Panel.Controls.Contains(CustomControls.ControlsDropDown_Button))
            {
                MainTop_Panel.Controls.Remove(CustomControls.ControlsDropDown_Button);

                foreach (Control button in GetMainTopButtons())
                {
                    button.Visible = true;
                }
            }
        }
        private Control[] GetMainTopButtons()
        {
            return [
                Accountants_Button, Categories_Button, Companies_Button,
                Products_Button, AddSale_Button, AddPurchase_Button];
        }

        /// <summary>
        /// Asks the user to save any changes.
        /// </summary>
        /// <returns>Returns true if the user cancels. Returns false if the user saves.</returns>
        private static bool AskUserToSaveBeforeClosing()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Save changes", "Save changes to the following items?",
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
        /// <returns>True if the menu was closed; False if the menu was opened.</returns>
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

                // Calculate X position based on left or right alignment
                int xPosition = alignRight
                    ? button.Left - menu.Width + button.Width
                    : button.Left;

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

            Purchase_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            Selected = SelectedOption.Purchases;
            ShowMainControls();
            SelectedDataGridView = Purchase_DataGridView;
            Purchase_DataGridView.Visible = true;
            Sale_DataGridView.Visible = false;

            // Show purchase charts, hide sale charts
            PurchaseTotals_Chart.Visible = true;
            PurchaseDistribution_Chart.Visible = true;
            SaleTotals_Chart.Visible = false;
            SaleDistribution_Chart.Visible = false;

            CenterAndResizeControls();
            RefreshDataGridViewAndCharts();

            Purchase_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            UpdateTotalLabels();
            SelectButton(Purchases_Button);
            Search_TextBox.PlaceholderText = LanguageManager.TranslateString("Search for purchases");
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (Selected == SelectedOption.Sales) { return; }

            Sale_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;

            Selected = SelectedOption.Sales;
            ShowMainControls();
            SelectedDataGridView = Sale_DataGridView;
            Sale_DataGridView.Visible = true;
            Purchase_DataGridView.Visible = false;

            // Show sale charts, hide purchase charts
            SaleTotals_Chart.Visible = true;
            SaleDistribution_Chart.Visible = true;
            PurchaseTotals_Chart.Visible = false;
            PurchaseDistribution_Chart.Visible = false;

            CenterAndResizeControls();
            RefreshDataGridViewAndCharts();

            Sale_DataGridView.ColumnWidthChanged += DataGridViewManager.DataGridView_ColumnWidthChanged;
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
            LoadOrRefreshAnalyticsCharts();

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
        private void LineChart_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            LoadOrRefreshMainCharts(true);
            LoadOrRefreshAnalyticsCharts(true);
            SetAllAnalyticTabsAsNotLoaded();
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
            if (IsProgramLoading) { return; }

            bool isAIQuery = Properties.Settings.Default.AISearchEnabled
                && Properties.Settings.Default.LicenseActivated
                && Search_TextBox.Text.StartsWith('!');

            // Process AI search only on Enter key
            if (e.KeyCode == Keys.Enter && isAIQuery)
            {
                // Check for internet connection
                if (!await InternetConnectionManager.CheckInternetAndShowMessageAsync("AI search", true))
                {
                    Log.Write(1, "AI search cancelled - no internet connection");
                    return;
                }

                ShowingResultsFor_Label.Text = LanguageManager.TranslateString("AI search in progress...");
                CenterShowingResultsLabel();
                await Search_TextBox.EnhanceSearchAsync();
            }
            // Show AI search prompt for AI queries (but don't start regular search)
            else if (isAIQuery)
            {
                ShowingResultsFor_Label.Text = LanguageManager.TranslateString("Press enter to begin AI search");
                CenterShowingResultsLabel();
            }
            // Start timer for regular searches (only if not an AI query)
            else if (!timerRunning)
            {
                timerRunning = true;
                searchTimer.Start();
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
        public static Guna2Panel TimeRangePanel { get; private set; }
        private static void ConstructTimeRangePanel()
        {
            DateRange_Form dateRange_Form = new();
            TimeRangePanel = dateRange_Form.Main_Panel;
        }
        private void TimeRange_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(TimeRangePanel))
            {
                CloseDateRangePanel();
            }
            else
            {
                CloseAllPanels(null, null);

                // Set the location for the panel
                TimeRangePanel.Location = new Point(
                    TimeRange_Button.Right - TimeRangePanel.Width,
                    TimeRange_Button.Bottom);

                Controls.Add(TimeRangePanel);
                TimeRangePanel.BringToFront();
            }
        }
        public void CloseDateRangePanel()
        {
            Controls.Remove(TimeRangePanel);
        }

        // Search timer
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

                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                    "Rename company",
                    "Do you want to rename '{0}' to '{1}'? There is already a company with the same name.",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.OkCancel,
                    name, suggestedCompanyName);

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
            SetEditButtonLocation();
            CenterAndResizeControls();
            UpdateMainMenuFormText();

            string message = $"Renamed program to: {CompanyName_Label.Text}";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(ThingsThatHaveChangedInFile, 3, message);
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
            SetEditButtonLocation();
        }
        public void SetEditButtonLocation()
        {
            Edit_Button.Left = CompanyName_Label.Right + 5;
        }

        // Search DataGridView getters and setters
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime? SortFromDate { get; set; } = null;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime? SortToDate { get; set; } = null;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan? SortTimeSpan { get; set; } = TimeSpan.MaxValue;

        // Search DataGridView methods
        public void RefreshDataGridViewAndCharts()
        {
            if (Selected == SelectedOption.Analytics)
            {
                ApplyFiltersToDataGridView(Purchase_DataGridView);
                ApplyFiltersToDataGridView(Sale_DataGridView);
                LoadOrRefreshAnalyticsCharts();
            }
            else
            {
                ApplyFiltersToDataGridView(SelectedDataGridView);
                LoadOrRefreshMainCharts();
                SelectedDataGridView.ClearSelection();
            }
        }
        private void ApplyFiltersToDataGridView(Guna2DataGridView dataGridView)
        {
            // Suspend layout updates during filtering for better performance
            dataGridView.SuspendLayout();

            bool usingCustomDateRange = SortFromDate != null && SortToDate != null;
            string displayedSearchText = Search_TextBox.Text.Trim();
            string effectiveSearchText = AISearchExtensions.GetEffectiveSearchQuery(displayedSearchText);

            bool filterExists = (SortTimeSpan != null && SortTimeSpan != TimeSpan.MaxValue) ||
                usingCustomDateRange ||
                !string.IsNullOrEmpty(effectiveSearchText);

            if (filterExists)
            {
                ShowShowingResultsForLabel();

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    // Try to parse the date from the correct column depending on which filter is used
                    if (!DateTime.TryParse(row.Cells[SalesColumnHeaders[Column.Date]].Value.ToString(), out DateTime rowDate) &&
                        !DateTime.TryParse(row.Cells[PurchaseColumnHeaders[Column.Date]].Value.ToString(), out rowDate))
                    {
                        row.Visible = false;
                        continue;
                    }

                    bool isVisibleByDate = true;

                    if (usingCustomDateRange)
                    {
                        isVisibleByDate = rowDate >= SortFromDate && rowDate <= SortToDate;
                    }
                    else if (SortTimeSpan != null && SortTimeSpan != TimeSpan.MaxValue)
                    {
                        isVisibleByDate = rowDate >= DateTime.Now - SortTimeSpan;
                    }

                    bool isVisibleBySearch = string.IsNullOrEmpty(effectiveSearchText) ||
                        SearchDataGridView.FilterRowByAdvancedSearch(row, effectiveSearchText);

                    row.Visible = isVisibleByDate && isVisibleBySearch;
                }
            }
            else
            {
                HideShowingResultsForLabel();

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Visible = true;
                }
            }

            bool hasVisibleRows = DataGridViewManager.HasVisibleRows(dataGridView);

            LabelManager.ManageNoDataLabelOnControl(hasVisibleRows, dataGridView);
            DataGridViewManager.UpdateAlternatingRowColors(dataGridView);
            UpdateTotalLabels();

            dataGridView.ResumeLayout(true);
        }
        private void ShowShowingResultsForLabel()
        {
            string text = BuildShowingResultsText();
            ShowingResultsFor_Label.Text = text;
            CenterShowingResultsLabel();
        }
        private string BuildShowingResultsText()
        {
            string displayedSearchText = Search_TextBox.Text.Trim();
            string searchDisplay = AISearchExtensions.GetDisplayQuery(displayedSearchText);

            // Helper methods to check if filters are active
            bool hasSearchFilter = !string.IsNullOrEmpty(searchDisplay);
            bool hasTimeSpanFilter = SortTimeSpan != null && SortTimeSpan != TimeSpan.MaxValue;
            bool hasDateRangeFilter = SortFromDate != null && SortToDate != null;

            // Base case: no search, no time filter, no date range
            if (!hasSearchFilter && !hasTimeSpanFilter && !hasDateRangeFilter)
            {
                return LanguageManager.TranslateString("Showing all results");
            }

            // Case 1: Search term only
            if (hasSearchFilter && !hasTimeSpanFilter && !hasDateRangeFilter)
            {
                string template = LanguageManager.TranslateString("Showing results for '{0}'");
                return string.Format(template, searchDisplay);
            }

            // Case 2: Time span only
            if (!hasSearchFilter && hasTimeSpanFilter && !hasDateRangeFilter)
            {
                string timeSpanText = GetTimeSpanText(SortTimeSpan!.Value);
                string template = LanguageManager.TranslateString("Showing results for\nthe last {0}");
                return string.Format(template, timeSpanText);
            }

            // Case 3: Custom date range only
            if (!hasSearchFilter && !hasTimeSpanFilter && hasDateRangeFilter)
            {
                string fromDate = Tools.FormatDate(SortFromDate!.Value);
                string toDate = Tools.FormatDate(SortToDate!.Value);
                string template = LanguageManager.TranslateString("Showing results from\n{0} to {1}");
                return string.Format(template, fromDate, toDate);
            }

            // Case 4: Search term + time span
            if (hasSearchFilter && hasTimeSpanFilter && !hasDateRangeFilter)
            {
                string timeSpanText = GetTimeSpanText(SortTimeSpan!.Value);
                string template = LanguageManager.TranslateString("Showing results for '{0}'\nin the last {1}");
                return string.Format(template, searchDisplay, timeSpanText);
            }

            // Case 5: Search term + custom date range
            if (hasSearchFilter && !hasTimeSpanFilter && hasDateRangeFilter)
            {
                string fromDate = Tools.FormatDate((DateTime)SortFromDate!);
                string toDate = Tools.FormatDate((DateTime)SortToDate!);
                string template = LanguageManager.TranslateString("Showing results for '{0}'\nfrom {1} to {2}");
                return string.Format(template, searchDisplay, fromDate, toDate);
            }

            // Fallback
            return LanguageManager.TranslateString("Showing results");
        }
        private static string GetTimeSpanText(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 365)
            {
                int years = (int)(timeSpan.TotalDays / 365);
                return years == 1
                    ? LanguageManager.TranslateString("year")
                    : string.Format(LanguageManager.TranslateString("{0} years"), years);
            }
            else
            {
                int days = (int)timeSpan.TotalDays;
                return days == 1
                    ? LanguageManager.TranslateString("day")
                    : string.Format(LanguageManager.TranslateString("{0} days"), days);
            }
        }
        private void CenterShowingResultsLabel()
        {
            bool visible = ShowingResultsFor_Label.Right < LineChart_Label.Left - 5;
            ShowingResultsFor_Label.Visible = visible;

            if (visible)
            {
                ShowingResultsFor_Label.Location = new Point(
                    (ClientSize.Width - ShowingResultsFor_Label.Width) / 2,
                    MainTop_Panel.Bottom + (_chartTop - MainTop_Panel.Bottom - ShowingResultsFor_Label.Height) / 2);
            }
        }
        public void HideShowingResultsForLabel()
        {
            ShowingResultsFor_Label.Visible = false;
        }
        private void SortTheDataGridViewByDate()
        {
            string dateColumnHeader = SalesColumnHeaders[Column.Date];
            Sale_DataGridView.Sort(Sale_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);

            dateColumnHeader = PurchaseColumnHeaders[Column.Date];
            Purchase_DataGridView.Sort(Purchase_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);
        }

        // List getters
        public List<Category> CategorySaleList { get; } = [];
        public List<Category> CategoryPurchaseList { get; } = [];
        public List<string> AccountantList { get; private set; } = [];
        public List<string> CompanyList { get; private set; } = [];

        // List methods
        public List<string> GetCategorySaleNames()
        {
            return CategorySaleList.Select(s => s.Name).ToList();
        }
        public List<string> GetCategoryPurchaseNames()
        {
            return CategoryPurchaseList.Select(p => p.Name).ToList();
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
        public List<string> GetProductSaleNames() => GetFormattedProductNames(CategorySaleList);

        /// <summary>
        /// Gets a list of formatted product names for items available for purchase.
        /// Format: "CompanyOfOrigin > CategoryName > ProductName"
        /// </summary>
        public List<string> GetProductPurchaseNames() => GetFormattedProductNames(CategoryPurchaseList);

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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedOption Selected { get; set; }

        // DataGridView getters
        public Guna2DataGridView Purchase_DataGridView { get; private set; }
        public Guna2DataGridView Sale_DataGridView { get; private set; }
        public Guna2DataGridView SelectedDataGridView { get; private set; }
        public Chart Profits_Chart { get; private set; }

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

            Quantity_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = SelectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = SelectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = SelectedDataGridView.Columns[shippingColumn].Width;

            Fee_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[feeColumn].Index, -1, true).Left;
            Fee_Label.Width = SelectedDataGridView.Columns[feeColumn].Width;

            Discount_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[discountColumn].Index, -1, true).Left;
            Discount_Label.Width = SelectedDataGridView.Columns[feeColumn].Width;

            ChargedDifference_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[chargedDifference].Index, -1, true).Left;
            ChargedDifference_Label.Width = SelectedDataGridView.Columns[chargedDifference].Width;

            Price_Label.Left = SelectedDataGridView.GetCellDisplayRectangle(SelectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = SelectedDataGridView.Columns[totalPriceColumn].Width;
        }
        public void UpdateTotalLabels()
        {
            if (Selected != SelectedOption.Purchases && Selected != SelectedOption.Sales)
            {
                return;
            }

            Total_Panel.Visible = DataGridViewManager.HasVisibleRows(SelectedDataGridView);

            int totalQuantity = 0;

            decimal totalTax = 0,
                totalShipping = 0,
                fee = 0,
                discount = 0,
                chargedDifference = 0,
                totalPrice = 0;

            foreach (DataGridViewRow row in SelectedDataGridView.Rows)
            {
                if (!LoadChart.IsRowValid(row)) { continue; }

                totalQuantity += Convert.ToInt32(row.Cells[Column.TotalItems.ToString()].Value);
                totalTax += Convert.ToDecimal(row.Cells[Column.Tax.ToString()].Value);
                totalShipping += Convert.ToDecimal(row.Cells[Column.Shipping.ToString()].Value);
                fee += Convert.ToDecimal(row.Cells[Column.Fee.ToString()].Value);
                discount += Convert.ToDecimal(row.Cells[Column.Discount.ToString()].Value);
                chargedDifference += Convert.ToDecimal(row.Cells[Column.ChargedDifference.ToString()].Value);
                totalPrice += Convert.ToDecimal(row.Cells[Column.Total.ToString()].Value);
            }

            LabelManager.ShowTotalsWithTransactions(Total_Label, SelectedDataGridView);
            Quantity_Label.Text = totalQuantity.ToString();
            Tax_Label.Text = $"{CurrencySymbol}{totalTax:N2}";
            Shipping_Label.Text = $"{CurrencySymbol}{totalShipping:N2}";
            Fee_Label.Text = $"{CurrencySymbol}{fee:N2}";
            Discount_Label.Text = $"{CurrencySymbol}{discount:N2}";
            ChargedDifference_Label.Text = $"{CurrencySymbol}{chargedDifference:N2}";
            Price_Label.Text = $"{CurrencySymbol}{totalPrice:N2}";
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
                        rowData[_noteTextKey] = cell.Tag;
                    }
                    else
                    {
                        rowData[cell.OwningColumn.Name] = cell.Value;
                    }
                }

                // Add the row tag
                if (row.Tag is (List<string> tagList, TagData purchaseData))
                {
                    rowData[_rowTagKey] = new
                    {
                        Items = tagList,
                        PurchaseData = purchaseData
                    };
                }
                else if (row.Tag is (string tagString, TagData purchaseData1))
                {
                    rowData[_rowTagKey] = new
                    {
                        Tag = tagString,
                        PurchaseData = purchaseData1
                    };
                }
                else if (row.Tag is TagData purchaseData2)
                {
                    rowData[_rowTagKey] = new
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
            if (IsProgramLoading) { return; }

            string filePath = DataGridViewManager.GetFilePathForDataGridView(option);

            List<Category> categoryList;
            if (option == SelectedOption.CategoryPurchases || option == SelectedOption.ProductPurchases)
            {
                categoryList = CategoryPurchaseList;
            }
            else
            {
                categoryList = CategorySaleList;
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

        // Analytic chart properties
        private List<Control> _analyticsControls;
        private Label _includeFreeShipping_Label;
        private Guna2Panel _analyticsTabButtons_Panel;
        private List<Guna2Button> _tabButtons;
        private AnalyticsTab _selectedTabKey = AnalyticsTab.Overview;
        private readonly Dictionary<AnalyticsTab, bool> _tabChartsLoaded = [];
        private readonly Dictionary<AnalyticsTab, List<Control>> _tabControls = [];

        public enum AnalyticsTab
        {
            Overview,
            Geographic,
            Financial,
            Performance,
            Operational,
            Returns
        }
        public enum ChartDataType
        {
            TotalSales,
            TotalPurchases,
            DistributionOfSales,
            DistributionOfPurchases,
            TotalProfits,
            CountriesOfOrigin,
            CompaniesOfOrigin,
            CountriesOfDestination,
            Accountants,
            TotalExpensesVsSales,
            AverageOrderValue,
            TotalTransactions,
            AverageShippingCosts,
            GrowthRates,
            ReturnsOverTime,
            ReturnReasons,
            ReturnFinancialImpact,
            ReturnsByCategory,
            ReturnsByProduct,
            PurchaseVsSaleReturns,
            WorldMap
        }
        public enum GeoMapDataType
        {
            Combined,
            PurchasesOnly,
            SalesOnly
        }
        public Chart CountriesOfOrigin_Chart { get; private set; }
        public Chart CountriesOfDestination_Chart { get; private set; }
        public Chart CompaniesOfOrigin_Chart { get; private set; }
        public Chart Accountants_Chart { get; private set; }
        public Chart GrowthRates_Chart { get; private set; }
        public Chart SalesVsExpenses_Chart { get; private set; }
        public Chart AverageTransactionValue_Chart { get; private set; }
        public Chart TotalTransactions_Chart { get; private set; }
        public Chart AverageShippingCosts_Chart { get; private set; }
        public Guna2CustomCheckBox IncludeFreeShipping_CheckBox { get; private set; }
        public Chart ReturnsOverTime_Chart { get; private set; }
        public Chart ReturnReasons_Chart { get; private set; }
        public Chart ReturnFinancialImpact_Chart { get; private set; }
        public Chart ReturnsByCategory_Chart { get; private set; }
        public Chart ReturnsByProduct_Chart { get; private set; }
        public Chart PurchaseVsSaleReturns_Chart { get; private set; }
        public Chart PurchaseTotals_Chart { get; private set; }
        public Chart PurchaseDistribution_Chart { get; private set; }
        public Chart SaleTotals_Chart { get; private set; }
        public Chart SaleDistribution_Chart { get; private set; }

        // GeoMap properties
        public GeoMap WorldMap_GeoMap { get; private set; }
        public Guna2Panel WorldMapControls_Panel { get; private set; }
        public Guna2CustomRadioButton CombinedData_RadioButton { get; private set; }
        public Guna2CustomRadioButton PurchasesOnly_RadioButton { get; private set; }
        public Guna2CustomRadioButton SalesOnly_RadioButton { get; private set; }
        private Label _worldMapDataType_Label;
        private Label _combinedData_Label;
        private Label _purchasesOnly_Label;
        private Label _salesOnly_Label;

        // Analytic chart methods
        public List<Control> GetMainControls()
        {
            return [
                Sale_DataGridView,
                Purchase_DataGridView,
                PurchaseTotals_Chart,
                PurchaseDistribution_Chart,
                SaleTotals_Chart,
                SaleDistribution_Chart,
                Profits_Chart,
                Total_Panel
            ];
        }
        public List<Control> GetAnalyticsControls() => _analyticsControls;
        public IEnumerable<Control> GetAllCharts()
        {
            return [PurchaseTotals_Chart,
                PurchaseDistribution_Chart,
                SaleTotals_Chart,
                SaleDistribution_Chart,
                Profits_Chart,
                CountriesOfOrigin_Chart,
                CompaniesOfOrigin_Chart,
                CountriesOfDestination_Chart,
                WorldMap_GeoMap,
                Accountants_Chart,
                SalesVsExpenses_Chart,
                AverageTransactionValue_Chart,
                TotalTransactions_Chart,
                AverageShippingCosts_Chart,
                GrowthRates_Chart,
                ReturnsOverTime_Chart,
                ReturnReasons_Chart,
                ReturnFinancialImpact_Chart,
                ReturnsByCategory_Chart,
                ReturnsByProduct_Chart,
                PurchaseVsSaleReturns_Chart];
        }
        private void ShowMainControls()
        {
            if (_analyticsControls == null) { return; }

            foreach (Control control in GetMainControls())
            {
                control.Visible = true;
            }
            foreach (Control control in _analyticsControls)
            {
                control.Visible = false;
            }

            // Reset chart positions in case returning from analytics
            PurchaseTotals_Chart.Top = _analyticChartTop;
            PurchaseDistribution_Chart.Top = _analyticChartTop;
            SaleTotals_Chart.Top = _analyticChartTop;
            SaleDistribution_Chart.Top = _analyticChartTop;
            Profits_Chart.Top = _analyticChartTop;
        }
        private void ConstructControlsForAnalytics()
        {
            CountriesOfOrigin_Chart = ConstructAnalyticsChart("countriesOfOrigin_Chart", false);
            CompaniesOfOrigin_Chart = ConstructAnalyticsChart("companiesOfOrigin_Chart", false);
            CountriesOfDestination_Chart = ConstructAnalyticsChart("countriesOfDestination_Chart", false);
            Accountants_Chart = ConstructAnalyticsChart("accountants_Chart", false);
            SalesVsExpenses_Chart = ConstructAnalyticsChart("salesVsExpenses_Chart", true);
            AverageTransactionValue_Chart = ConstructAnalyticsChart("averageOrderValue_Chart", true);
            TotalTransactions_Chart = ConstructAnalyticsChart("totalTransactions_Chart", true);
            AverageShippingCosts_Chart = ConstructAnalyticsChart("averageShippingCosts_Chart", true);
            GrowthRates_Chart = ConstructAnalyticsChart("growthRates_Chart", true);
            ReturnsOverTime_Chart = ConstructAnalyticsChart("returnsOverTime_Chart", true);
            ReturnReasons_Chart = ConstructAnalyticsChart("returnReasons_Chart", false);
            ReturnFinancialImpact_Chart = ConstructAnalyticsChart("returnFinancialImpact_Chart", true);
            ReturnsByCategory_Chart = ConstructAnalyticsChart("returnsByCategory_Chart", false);
            ReturnsByProduct_Chart = ConstructAnalyticsChart("returnsByProduct_Chart", false);
            PurchaseVsSaleReturns_Chart = ConstructAnalyticsChart("purchaseVsSaleReturns_Chart", false);

            WorldMap_GeoMap = ConstructGeoMap();
            ConstructWorldMapDataControls();

            IncludeFreeShipping_CheckBox = new Guna2CustomCheckBox
            {
                Size = new Size(20, 20),
                Checked = Properties.Settings.Default.IncludeFreeShipping,
                Animated = true
            };
            IncludeFreeShipping_CheckBox.CheckedChanged += IncludeFreeShippingCheckBox_CheckedChanged;
            Controls.Add(IncludeFreeShipping_CheckBox);

            _includeFreeShipping_Label = new Label
            {
                Text = "Include transactions with free shipping",
                Name = "IncludeFreeShipping_Label",  // This is needed for the language translation
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(5),
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _includeFreeShipping_Label.Click += IncludeFreeShipping_Label_Click;
            Controls.Add(_includeFreeShipping_Label);

            CreateAnalyticsTabControl();
            OrganizeChartsIntoTabs();

            MouseClickChartManager.InitCharts(_analyticsControls.OfType<Chart>().ToArray());
        }
        private void CreateAnalyticsTabControl()
        {
            // Initialize the dictionaries with enum values
            foreach (AnalyticsTab tab in Enum.GetValues<AnalyticsTab>())
            {
                _tabChartsLoaded[tab] = false;
                _tabControls[tab] = [];
            }

            // Create a panel to hold all the tab buttons
            Guna2Panel tabButtonsPanel = new()
            {
                Height = 60,
                FillColor = CustomColors.ContentPanelBackground,
                BorderColor = CustomColors.ControlBorder,
                BorderThickness = 1,
                BorderRadius = 8,
                Anchor = AnchorStyles.Top
            };
            tabButtonsPanel.Click += (s, e) => { CloseAllPanels(null, null); };

            // Create tab buttons
            List<Guna2Button> tabButtons = [];

            Guna2Button overviewButton = CreateTabButton("Overview", AnalyticsTab.Overview, Resources.Overview);
            tabButtons.Add(overviewButton);

            Guna2Button geographicButton = CreateTabButton("Geographic", AnalyticsTab.Geographic, Resources.Earth);
            tabButtons.Add(geographicButton);

            Guna2Button financialButton = CreateTabButton("Financial", AnalyticsTab.Financial, Resources.Financial);
            tabButtons.Add(financialButton);

            Guna2Button performanceButton = CreateTabButton("Performance", AnalyticsTab.Performance, Resources.Performance);
            tabButtons.Add(performanceButton);

            Guna2Button operationalButton = CreateTabButton("Operational", AnalyticsTab.Operational, Resources.Gear);
            tabButtons.Add(operationalButton);

            Guna2Button returnsButton = CreateTabButton("Returns", AnalyticsTab.Returns, Resources.Return);
            tabButtons.Add(returnsButton);

            // Position buttons
            byte buttonWidth = 200, buttonSpacing = 12, startX = 10;

            for (int i = 0; i < tabButtons.Count; i++)
            {
                Guna2Button button = tabButtons[i];
                button.Location = new Point(i * (buttonWidth + buttonSpacing) + startX, 10);
                tabButtonsPanel.Controls.Add(button);
            }

            _analyticsTabButtons_Panel = tabButtonsPanel;
            _analyticsTabButtons_Panel.Size = new Size(tabButtons.Count * (buttonWidth + buttonSpacing) + startX, 65);
            _analyticsTabButtons_Panel.Location = new Point((Width - _analyticsTabButtons_Panel.Width) / 2, Purchases_Button.Bottom + 20);

            _tabButtons = tabButtons;

            ThemeManager.SetThemeForControls([tabButtonsPanel]);

            // Select the first tab by default
            SelectTabButton(AnalyticsTab.Overview);

            Controls.Add(tabButtonsPanel);
        }
        private Guna2Button CreateTabButton(string title, AnalyticsTab tabKey, Image icon)
        {
            Guna2Button button = new()
            {
                Name = $"{tabKey}Tab_Button",
                Text = title,
                Tag = tabKey,
                Image = icon,
                Size = new Size(200, 45),
                ImageOffset = new Point(-5, 0),
                ImageSize = new Size(25, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 6,
            };

            button.Click += TabButton_Click;
            return button;
        }
        private void TabButton_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (sender is Guna2Button clickedButton && clickedButton.Tag is AnalyticsTab tabKey)
            {
                SelectTabButton(tabKey);

                if (!_tabChartsLoaded[tabKey])
                {
                    LoadChartsForTab(tabKey);
                    _tabChartsLoaded[tabKey] = true;
                }

                ShowControlsForTab(tabKey);
                CenterAndResizeControls();
            }
        }
        private void SelectTabButton(AnalyticsTab selectedTabKey)
        {
            _selectedTabKey = selectedTabKey;

            foreach (Guna2Button button in _tabButtons)
            {
                if (button.Tag is AnalyticsTab buttonTab && buttonTab == selectedTabKey)
                {
                    // Selected state
                    button.FillColor = CustomColors.AccentBlue;
                    button.ForeColor = Color.White;
                }
                else
                {
                    // Unselected state
                    button.FillColor = Color.Transparent;
                    button.ForeColor = CustomColors.Text;
                }
            }
        }
        private void OrganizeChartsIntoTabs()
        {
            _tabControls[AnalyticsTab.Overview].AddRange(
            [
                SalesVsExpenses_Chart,
                Profits_Chart,
                TotalTransactions_Chart,
                AverageTransactionValue_Chart
            ]);

            _tabControls[AnalyticsTab.Geographic].AddRange(
            [
                WorldMap_GeoMap,
                CountriesOfOrigin_Chart,
                CountriesOfDestination_Chart,
                CompaniesOfOrigin_Chart,
                WorldMapControls_Panel
            ]);

            _tabControls[AnalyticsTab.Financial].AddRange(
            [
                SaleTotals_Chart,
                PurchaseTotals_Chart,
                SaleDistribution_Chart,
                PurchaseDistribution_Chart
            ]);

            _tabControls[AnalyticsTab.Performance].AddRange(
            [
                GrowthRates_Chart,
                AverageTransactionValue_Chart,
                TotalTransactions_Chart
            ]);

            _tabControls[AnalyticsTab.Operational].AddRange(
            [
                Accountants_Chart,
                AverageShippingCosts_Chart,
                IncludeFreeShipping_CheckBox,
                _includeFreeShipping_Label
            ]);

            _tabControls[AnalyticsTab.Returns].AddRange(
            [
                ReturnsOverTime_Chart,
                ReturnReasons_Chart,
                ReturnFinancialImpact_Chart,
                ReturnsByCategory_Chart,
                ReturnsByProduct_Chart,
                PurchaseVsSaleReturns_Chart
            ]);

            _analyticsControls =
            [
                CountriesOfOrigin_Chart,
                CompaniesOfOrigin_Chart,
                CountriesOfDestination_Chart,
                WorldMap_GeoMap,
                WorldMapControls_Panel,
                Accountants_Chart,
                SalesVsExpenses_Chart,
                AverageTransactionValue_Chart,
                TotalTransactions_Chart,
                AverageShippingCosts_Chart,
                GrowthRates_Chart,
                ReturnsOverTime_Chart,
                ReturnReasons_Chart,
                ReturnFinancialImpact_Chart,
                ReturnsByCategory_Chart,
                ReturnsByProduct_Chart,
                PurchaseVsSaleReturns_Chart,
                IncludeFreeShipping_CheckBox,
                _includeFreeShipping_Label,
                _analyticsTabButtons_Panel
            ];
        }
        private void LoadChartsForTab(AnalyticsTab tabKey)
        {
            bool isLine = LineChart_ToggleSwitch.Checked;

            switch (tabKey)
            {
                case AnalyticsTab.Overview:
                    LoadChart.LoadSalesVsExpensesChart(SalesVsExpenses_Chart as CartesianChart, isLine);
                    SetChartTitle(SalesVsExpenses_Chart, TranslatedChartTitles.SalesVsExpenses);

                    LoadChart.LoadTotalTransactionsChart(TotalTransactions_Chart as CartesianChart, isLine);
                    SetChartTitle(TotalTransactions_Chart, TranslatedChartTitles.TotalTransactions);

                    LoadChart.LoadAverageTransactionValueChart(AverageTransactionValue_Chart as CartesianChart, isLine);
                    SetChartTitle(AverageTransactionValue_Chart, TranslatedChartTitles.AverageTransactionValue);

                    ChartData profitsData = LoadChart.LoadProfitsIntoChart(Profits_Chart as CartesianChart, isLine);
                    SetProfitsChartTitle(profitsData.Total);
                    break;

                case AnalyticsTab.Geographic:
                    GeoMapDataType dataType = GetSelectedGeoMapDataType();
                    LoadChart.LoadWorldMapChart(WorldMap_GeoMap, dataType);

                    LoadChart.LoadCountriesOfOriginForProductsIntoChart(CountriesOfOrigin_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(CountriesOfOrigin_Chart, TranslatedChartTitles.CountriesOfOrigin);

                    LoadChart.LoadCountriesOfDestinationForProductsIntoChart(CountriesOfDestination_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(CountriesOfDestination_Chart, TranslatedChartTitles.CountriesOfDestination);

                    LoadChart.LoadCompaniesOfOriginForProductsIntoChart(CompaniesOfOrigin_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(CompaniesOfOrigin_Chart, TranslatedChartTitles.CompaniesOfOrigin);
                    break;

                case AnalyticsTab.Financial:
                    LoadOrRefreshMainCharts();
                    break;

                case AnalyticsTab.Performance:
                    LoadChart.LoadGrowthRateChart(GrowthRates_Chart as CartesianChart);
                    SetChartTitle(GrowthRates_Chart, TranslatedChartTitles.GrowthRates);

                    // Load shared charts if not already loaded
                    if (!_tabChartsLoaded[AnalyticsTab.Overview])
                    {
                        LoadChart.LoadAverageTransactionValueChart(AverageTransactionValue_Chart as CartesianChart, isLine);
                        SetChartTitle(AverageTransactionValue_Chart, TranslatedChartTitles.AverageTransactionValue);

                        LoadChart.LoadTotalTransactionsChart(TotalTransactions_Chart as CartesianChart, isLine);
                        SetChartTitle(TotalTransactions_Chart, TranslatedChartTitles.TotalTransactions);
                    }
                    break;

                case AnalyticsTab.Operational:
                    LoadChart.LoadAccountantsIntoChart(Accountants_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(Accountants_Chart, TranslatedChartTitles.AccountantsTransactions);

                    LoadChart.LoadAverageShippingCostsChart(AverageShippingCosts_Chart as CartesianChart, isLine,
                        includeZeroShipping: IncludeFreeShipping_CheckBox.Checked);
                    SetChartTitle(AverageShippingCosts_Chart, TranslatedChartTitles.AverageShippingCosts);
                    break;

                case AnalyticsTab.Returns:
                    LoadChart.LoadReturnsOverTimeChart(ReturnsOverTime_Chart as CartesianChart, isLine);
                    SetChartTitle(ReturnsOverTime_Chart, TranslatedChartTitles.ReturnsOverTime);

                    LoadChart.LoadReturnReasonsChart(ReturnReasons_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(ReturnReasons_Chart, TranslatedChartTitles.ReturnReasons);

                    LoadChart.LoadReturnFinancialImpactChart(ReturnFinancialImpact_Chart as CartesianChart, isLine);
                    SetChartTitle(ReturnFinancialImpact_Chart, TranslatedChartTitles.ReturnFinancialImpact);

                    LoadChart.LoadReturnsByCategoryChart(ReturnsByCategory_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(ReturnsByCategory_Chart, TranslatedChartTitles.ReturnsByCategory);

                    LoadChart.LoadReturnsByProductChart(ReturnsByProduct_Chart as PieChart, PieChartGrouping.Top8);
                    SetChartTitle(ReturnsByProduct_Chart, TranslatedChartTitles.ReturnsByProduct);

                    LoadChart.LoadPurchaseVsSaleReturnsChart(PurchaseVsSaleReturns_Chart as PieChart);
                    SetChartTitle(PurchaseVsSaleReturns_Chart, TranslatedChartTitles.PurchaseVsSaleReturns);
                    break;
            }
        }
        private void ShowControlsForTab(AnalyticsTab tabKey)
        {
            // Hide all analytics charts first
            foreach (List<Control> controlList in _tabControls.Values)
            {
                foreach (Control control in controlList)
                {
                    control.Visible = false;
                }
            }

            // Show controls for the selected tab
            foreach (Control control in _tabControls[tabKey])
            {
                control.Visible = true;
            }
        }
        private void IncludeFreeShipping_Label_Click(object? sender, EventArgs e)
        {
            IncludeFreeShipping_CheckBox.Checked = !IncludeFreeShipping_CheckBox.Checked;
        }
        private void IncludeFreeShippingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isLineChart = LineChart_ToggleSwitch.Checked;
            bool zeroShipping = IncludeFreeShipping_CheckBox.Checked;

            UserSettings.UpdateSetting("Include free shipping in chart", Properties.Settings.Default.IncludeFreeShipping, zeroShipping,
                value => Properties.Settings.Default.IncludeFreeShipping = value);

            LoadChart.LoadAverageShippingCostsChart(AverageShippingCosts_Chart as CartesianChart, isLineChart, includeZeroShipping: zeroShipping);
        }
        private Chart ConstructAnalyticsChart(string name, bool isCartesian)
        {
            Chart chart;

            if (isCartesian)
            {
                CartesianChart cartesianChart = new()
                {
                    Name = name,
                    Title = CreateChartTitle("")
                };

                chart = cartesianChart;
            }
            else
            {
                PieChart pieChart = new()
                {
                    Name = name,
                    Title = CreateChartTitle("")
                };

                LoadChart.ConfigureChartForPie(pieChart);
                chart = pieChart;
            }

            // Enable double buffering
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(chart, true, null);

            Controls.Add(chart);
            return chart;
        }
        private void ShowAnalyticsControls()
        {
            // Hide main controls
            foreach (Control control in GetMainControls())
            {
                control.Visible = false;
            }

            _analyticsTabButtons_Panel.Visible = true;

            // Load overview tab by default if not loaded
            if (!_tabChartsLoaded[AnalyticsTab.Overview])
            {
                LoadChartsForTab(AnalyticsTab.Overview);
                _tabChartsLoaded[AnalyticsTab.Overview] = true;
            }

            ShowControlsForTab(_selectedTabKey);
        }
        public void LoadOrRefreshAnalyticsCharts(bool onlyRefreshForLineCharts = false)
        {
            if (Selected != SelectedOption.Analytics) { return; }

            AnalyticsTab currentTabKey = _selectedTabKey;
            bool isLine = LineChart_ToggleSwitch.Checked;

            if (onlyRefreshForLineCharts)
            {
                // Only reload line/bar charts when toggle switches
                switch (currentTabKey)
                {
                    case AnalyticsTab.Overview:
                    case AnalyticsTab.Performance:
                        LoadChart.LoadSalesVsExpensesChart(SalesVsExpenses_Chart as CartesianChart, isLine);
                        LoadChart.LoadTotalTransactionsChart(TotalTransactions_Chart as CartesianChart, isLine);
                        LoadChart.LoadAverageTransactionValueChart(AverageTransactionValue_Chart as CartesianChart, isLine);

                        ChartData profitsData = LoadChart.LoadProfitsIntoChart(Profits_Chart as CartesianChart, isLine);
                        SetProfitsChartTitle(profitsData.Total);
                        break;

                    case AnalyticsTab.Geographic:
                        LoadChart.LoadWorldMapChart(WorldMap_GeoMap);
                        break;

                    case AnalyticsTab.Financial:
                        LoadOrRefreshMainCharts(true);
                        break;

                    case AnalyticsTab.Operational:
                        LoadChart.LoadAverageShippingCostsChart(AverageShippingCosts_Chart as CartesianChart, isLine,
                            includeZeroShipping: IncludeFreeShipping_CheckBox.Checked);
                        break;

                    case AnalyticsTab.Returns:
                        LoadChart.LoadReturnsOverTimeChart(ReturnsOverTime_Chart as CartesianChart, isLine);
                        LoadChart.LoadReturnFinancialImpactChart(ReturnFinancialImpact_Chart as CartesianChart, isLine);
                        break;
                }
            }
            else
            {
                // Full reload
                LoadChartsForTab(currentTabKey);
            }
        }
        public void SetAllAnalyticTabsAsNotLoaded()
        {
            foreach (AnalyticsTab tab in Enum.GetValues<AnalyticsTab>())
            {
                _tabChartsLoaded[tab] = false;
            }
        }
        public void ReloadCurrentAnalyticTab()
        {
            if (Selected == SelectedOption.Analytics)
            {
                LoadChartsForTab(_selectedTabKey);
                _tabChartsLoaded[_selectedTabKey] = true;
            }
        }

        // GeoMap methods
        private GeoMap ConstructGeoMap()
        {
            GeoMap geoMap = new()
            {
                Visible = false
            };

            // Enable double buffering
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(geoMap, true, null);

            Controls.Add(geoMap);
            return geoMap;
        }
        private void ConstructWorldMapDataControls()
        {
            const int radioButtonSize = 20;

            // Create main label
            _worldMapDataType_Label = new Label
            {
                Text = "Map Data Source:",
                Name = "WorldMapDataType_Label",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CustomColors.Text,
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _worldMapDataType_Label.Click += (s, e) => { CloseAllPanels(null, null); };
            LanguageManager.UpdateLanguageForControl(_worldMapDataType_Label);

            // Combined data option
            CombinedData_RadioButton = new Guna2CustomRadioButton
            {
                Size = new Size(radioButtonSize, radioButtonSize),
                Animated = true
            };

            _combinedData_Label = new Label
            {
                Text = "Purchases + sales",
                Name = "CombinedData_Label",
                AutoSize = true,
                Padding = new Padding(5),
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.Text,
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _combinedData_Label.Click += (s, e) =>
            {
                CombinedData_RadioButton.Checked = true;
                CloseAllPanels(null, null);
            };
            LanguageManager.UpdateLanguageForControl(_combinedData_Label);

            // Purchases only option
            PurchasesOnly_RadioButton = new Guna2CustomRadioButton
            {
                Size = new Size(radioButtonSize, radioButtonSize),
                Animated = true
            };

            _purchasesOnly_Label = new Label
            {
                Text = "Countries of origin",
                Name = "CountriesOfOrigin_Label",
                AutoSize = true,
                Padding = new Padding(5),
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.Text,
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _purchasesOnly_Label.Click += (s, e) =>
            {
                PurchasesOnly_RadioButton.Checked = true;
                CloseAllPanels(null, null);
            };
            LanguageManager.UpdateLanguageForControl(_purchasesOnly_Label);

            // Sales only option
            SalesOnly_RadioButton = new Guna2CustomRadioButton
            {
                Size = new Size(radioButtonSize, radioButtonSize),
                Animated = true
            };

            _salesOnly_Label = new Label
            {
                Text = "Countries of destination",
                Name = "CountriesOfDestination_Label",
                AutoSize = true,
                Padding = new Padding(5),
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.Text,
                AccessibleDescription = AccessibleDescriptionManager.AlignLeft
            };
            _salesOnly_Label.Click += (s, e) =>
            {
                SalesOnly_RadioButton.Checked = true;
                CloseAllPanels(null, null);
            };
            LanguageManager.UpdateLanguageForControl(_salesOnly_Label);

            // Create panel (size will be calculated in RecalculateWorldMapControlsLayout)
            WorldMapControls_Panel = new Guna2Panel
            {
                FillColor = Color.Transparent,
                BorderThickness = 0,
                Visible = false
            };
            WorldMapControls_Panel.Click += (s, e) => { CloseAllPanels(null, null); };

            // Add all controls to panel
            WorldMapControls_Panel.Controls.AddRange([
                _worldMapDataType_Label,
                CombinedData_RadioButton,
                _combinedData_Label,
                PurchasesOnly_RadioButton,
                _purchasesOnly_Label,
                SalesOnly_RadioButton,
                _salesOnly_Label
            ]);

            Controls.Add(WorldMapControls_Panel);
            CombinedData_RadioButton.Checked = true;  // Check it after the control is added

            // Set up event handlers
            SalesOnly_RadioButton.CheckedChanged += WorldMapDataType_CheckedChanged;
            CombinedData_RadioButton.CheckedChanged += WorldMapDataType_CheckedChanged;
            PurchasesOnly_RadioButton.CheckedChanged += WorldMapDataType_CheckedChanged;

            RecalculateWorldMapControlsLayout();
        }

        /// <summary>
        /// Recalculates and repositions the world map controls.
        /// This should be called after language changes or during initial construction.
        /// </summary>
        public void RecalculateWorldMapControlsLayout()
        {
            if (WorldMapControls_Panel == null || _worldMapDataType_Label == null)
            {
                return;
            }

            // Constants
            const int radioButtonSize = 20;
            const int radioLabelSpacing = -2;
            const int optionSpacing = 30;
            const int panelHeight = 55;
            int currentX = 0;

            // Recalculate main label position
            int mainLabelY = (panelHeight - _worldMapDataType_Label.PreferredHeight) / 2;
            _worldMapDataType_Label.Location = new Point(currentX, mainLabelY);
            currentX += _worldMapDataType_Label.PreferredWidth + optionSpacing;

            int radioButtonY = (panelHeight - radioButtonSize) / 2;
            int labelY = (panelHeight - _combinedData_Label.PreferredHeight) / 2;

            // Reposition Combined data option
            CombinedData_RadioButton.Location = new Point(currentX, radioButtonY);
            _combinedData_Label.Location = new Point(currentX + radioButtonSize + radioLabelSpacing, labelY);
            currentX += radioButtonSize + radioLabelSpacing + _combinedData_Label.PreferredWidth + optionSpacing;

            // Reposition Purchases only option
            PurchasesOnly_RadioButton.Location = new Point(currentX, radioButtonY);
            _purchasesOnly_Label.Location = new Point(currentX + radioButtonSize + radioLabelSpacing, labelY);
            currentX += radioButtonSize + radioLabelSpacing + _purchasesOnly_Label.PreferredWidth + optionSpacing;

            // Reposition Sales only option
            SalesOnly_RadioButton.Location = new Point(currentX, radioButtonY);
            _salesOnly_Label.Location = new Point(currentX + radioButtonSize + radioLabelSpacing, labelY);

            // Recalculate total panel width
            int totalWidth = currentX + radioButtonSize + radioLabelSpacing + _salesOnly_Label.PreferredWidth;

            // Update panel size
            WorldMapControls_Panel.Size = new Size(totalWidth, panelHeight);

            // Position the panel itself (both for initial construction and language updates)
            if (_analyticsTabButtons_Panel != null)
            {
                WorldMapControls_Panel.Location = new Point((ClientSize.Width - WorldMapControls_Panel.Width) / 2,
                    _analyticsTabButtons_Panel.Bottom + 10);
            }
        }
        private void WorldMapDataType_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (sender is Guna2CustomRadioButton radioButton && radioButton.Checked)
            {
                GeoMapDataType dataType = GetSelectedGeoMapDataType();
                LoadChart.LoadWorldMapChart(WorldMap_GeoMap, dataType);
            }
        }
        private GeoMapDataType GetSelectedGeoMapDataType()
        {
            if (PurchasesOnly_RadioButton.Checked)
            {
                return GeoMapDataType.PurchasesOnly;
            }
            else if (SalesOnly_RadioButton.Checked)
            {
                return GeoMapDataType.SalesOnly;
            }
            else
            {
                return GeoMapDataType.Combined;
            }
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
                CompanyLogo.CompanyLogoRightClick_Panel,
                GetStarted_Form.RightClickOpenRecent_Panel,
                DataGridViewManager.RightClickDataGridView_Panel,
                RightClickGunaChartMenu.RightClickGunaChart_Panel
            }.Where(panel => panel != null).ToList();
        }
        public void UpdateMainMenuFormText()
        {
            Text = $"Argo Sales Tracker {Tools.GetVersionNumber()} - {Directories.CompanyName}";
        }
        public void ClosePanels()
        {
            DataGridViewManager.RightClickDataGridView_Panel.Parent?.Controls.Remove(DataGridViewManager.RightClickDataGridView_Panel);
            Controls.Remove(RightClickGunaChartMenu.RightClickGunaChart_Panel);
            Controls.Remove(CompanyLogo.CompanyLogoRightClick_Panel);
            DataGridViewManager.DoNotDeleteRows = false;
        }
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}