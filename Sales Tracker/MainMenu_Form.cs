using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings;
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
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        private static readonly string noteTextKey = "note", rowTagKey = "RowTag", itemsKey = "Items", purchaseDataKey = "PurchaseData", tagKey = "Tag";
        public static readonly string emptyCell = "-", multipleItems_text = "Multiple items", receipt_text = "receipt:", show_text = "show", companyName_text = "CompanyName";
        private readonly byte spaceForRightClickPanel = 30;
        public DateTime fromDate, toDate;
        private static string _currencySymbol;
        private static bool _isFullVersion = true;

        // Getters and setters
        public static MainMenu_Form Instance
        {
            get => _instance;
        }
        public static List<string> ThingsThatHaveChangedInFile
        {
            get => _thingsThatHaveChangedInFile;
            private set => _thingsThatHaveChangedInFile = value;
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

            UI.ConstructControls();
            InitiateSearchTimer();
            SearchBox.ConstructSearchBox();
            CurrencySymbol = Currency.GetSymbol(Properties.Settings.Default.Currency);

            SetCompanyLabel();

            isProgramLoading = true;
            LoadData();
            UpdateTheme();
            isProgramLoading = false;

            HideShowingResultsForLabel();
        }
        public void LoadData()
        {
            LoadCategoriesFromFile(Directories.CategoryPurchases_file, categoryPurchaseList);
            LoadCategoriesFromFile(Directories.CategorySales_file, categorySaleList);

            accountantList = Directories.ReadAllLinesInFile(Directories.Accountants_file).ToList();
            companyList = Directories.ReadAllLinesInFile(Directories.Companies_file).ToList();

            if (Purchases_DataGridView == null)
            {
                Size size = new(1300, 350);
                Purchases_DataGridView = new Guna2DataGridView();
                InitializeDataGridView(Purchases_DataGridView, size, PurchaseColumnHeaders);
                Purchases_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;

                Sales_DataGridView = new Guna2DataGridView();
                InitializeDataGridView(Sales_DataGridView, size, SalesColumnHeaders);
                Sales_DataGridView.Tag = DataGridViewTag.SaleOrPurchase;
            }

            AddRowsFromFile(Purchases_DataGridView, SelectedOption.Purchases);
            AddRowsFromFile(Sales_DataGridView, SelectedOption.Sales);

            // Load images into column headers
            DataGridViewColumn chargedDifferenceColumn = Purchases_DataGridView.Columns[Column.ChargedDifference.ToString()];
            string existingHeaderText = chargedDifferenceColumn.HeaderText;
            string messageBoxText = "Having a charged difference is common and is usually due to taxes, duties, bank fees, exchange rate differences, or political and tax variations across countries.";
            chargedDifferenceColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);

            DataGridViewColumn totalColumn = Sales_DataGridView.Columns[Column.Total.ToString()];
            existingHeaderText = totalColumn.HeaderText;
            messageBoxText = "The revenue excludes shipping, taxes, and fees.";
            totalColumn.HeaderCell = new DataGridViewImageHeaderCell(Resources.HelpGray, existingHeaderText, messageBoxText);

            AddTimeRangesIntoComboBox();
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
            string filePath = GetFilePathForDataGridView(selected);

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
                        // If the tagObject is a list of items and TagData
                        if (tagObject.TryGetValue(itemsKey, out object? itemsElement) && itemsElement is JsonElement itemsJsonElement && itemsJsonElement.ValueKind == JsonValueKind.Array)
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
                        else if (tagObject.TryGetValue(tagKey, out object? tagStringElement) && tagObject.TryGetValue(purchaseDataKey, out object? purchaseData1Element))
                        {
                            string? tagString = tagStringElement?.ToString();
                            TagData? purchaseData1 = JsonSerializer.Deserialize<TagData>(purchaseData1Element?.ToString());

                            if (tagString != null && purchaseData1 != null)
                            {
                                dataGridView.Rows[rowIndex].Tag = (tagString, purchaseData1);
                            }
                        }
                        // If the tagObject is a string
                        else if (value is JsonElement stringElement && stringElement.ValueKind == JsonValueKind.Object)
                        {
                            if (stringElement.TryGetProperty(tagKey, out JsonElement tagProperty) && tagProperty.ValueKind == JsonValueKind.String)
                            {
                                string singleTag = tagProperty.GetString();
                                dataGridView.Rows[rowIndex].Tag = singleTag;
                            }
                        }
                    }
                }

                // Set the cell tag for the last cell if note_text key exists
                if (rowData.TryGetValue(noteTextKey, out object noteValue))
                {
                    DataGridViewCell lastCell = dataGridView.Rows[rowIndex].Cells[dataGridView.Columns.Count - 1];
                    lastCell.Value = show_text;
                    lastCell.Tag = noteValue;
                    AddUnderlineToCell(lastCell);
                }
            }
        }
        public void LoadCharts()
        {
            double total;
            if (Selected == SelectedOption.Sales)
            {
                total = LoadChart.LoadTotalsIntoChart(Sales_DataGridView, Totals_Chart, LineGraph_ToggleSwitch.Checked);
                Totals_Chart.Title.Text = $"Total revenue: {CurrencySymbol}{total:N2}";

                LoadChart.LoadDistributionIntoChart(Sales_DataGridView, Distribution_Chart);
                Distribution_Chart.Title.Text = "Distribution of revenue";
            }
            else
            {
                total = LoadChart.LoadTotalsIntoChart(Purchases_DataGridView, Totals_Chart, LineGraph_ToggleSwitch.Checked);
                Totals_Chart.Title.Text = $"Total expenses: {CurrencySymbol}{total:N2}";

                LoadChart.LoadDistributionIntoChart(Purchases_DataGridView, Distribution_Chart);
                Distribution_Chart.Title.Text = "Distribution of expenses";
            }
            total = LoadChart.LoadProfitsIntoChart(Sales_DataGridView, Purchases_DataGridView, Profits_Chart, LineGraph_ToggleSwitch.Checked);
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
                Sales_DataGridView,
                Purchases_DataGridView,
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

        // Form event handlers
        private void MainMenu_Form_Load(object sender, EventArgs e)
        {
            Sales_Button.PerformClick();
            SortTheDataGridViewByDate();
        }
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
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
            UI.CloseAllPanels(null, null);
            ResizeControls();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Enter)
            {
                Guna2Panel[] panels = [
                    UI.fileMenu,
                    UI.helpMenu,
                    UI.accountMenu,
                    UI.ControlDropDown_Panel,
                    rightClickDataGridView_Panel
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

            if (Height > 1400)
            {
                Totals_Chart.Height = 500;
            }
            else if (Height > 1000)
            {
                Totals_Chart.Height = 400;
            }
            else
            {
                Totals_Chart.Height = 300;
            }

            int chartWidth = Width / 3 - 35;
            int chartHeight = Totals_Chart.Height;

            // Set chart on the left
            Totals_Chart.Width = chartWidth;
            Totals_Chart.Left = (Width - 3 * chartWidth - 40) / 2 - UI.spaceToOffsetFormNotCenter;

            // Set chart in the middle
            Distribution_Chart.Size = new Size(chartWidth, chartHeight);
            Distribution_Chart.Left = Totals_Chart.Right + 20;

            // Set chart on the right
            Profits_Chart.Size = new Size(chartWidth, chartHeight);
            Profits_Chart.Left = Distribution_Chart.Right + 20;

            // Set chart on the left
            selectedDataGridView.Size = new Size(Width - 65, Height - MainTop_Panel.Height - Top_Panel.Height - Totals_Chart.Height - Totals_Chart.Top - 15);
            selectedDataGridView.Location = new Point((Width - selectedDataGridView.Width) / 2 - UI.spaceToOffsetFormNotCenter, Height - MainTop_Panel.Height - Top_Panel.Height - selectedDataGridView.Height);

            Total_Panel.Location = new Point(selectedDataGridView.Left, selectedDataGridView.Top + selectedDataGridView.Height);
            Total_Panel.Width = selectedDataGridView.Width;

            if (Width < 1500 + Edit_Button.Left + Edit_Button.Width)
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
                int statChartWidth = Width / 3 - 30;
                int statchartHeight = Height / 3 - 30;

                // Set chart on the left
                countriesOfOrigin_Chart.Size = new Size(statChartWidth, statchartHeight);
                countriesOfOrigin_Chart.Left = 20;

                // Set chart in the middle
                companiesOfOrigin_Chart.Size = new Size(statChartWidth, statchartHeight);
                companiesOfOrigin_Chart.Left = (Width / 2) - (Distribution_Chart.Width / 2) - UI.spaceToOffsetFormNotCenter;

                // Set chart on the right
                countriesOfDestination_Chart.Size = new Size(statChartWidth, statchartHeight);
                countriesOfDestination_Chart.Left = Width - Totals_Chart.Width - 35;

                // Set chart on the left
                accountants_Chart.Size = new Size(statChartWidth, statchartHeight);
                accountants_Chart.Left = 20;

                // Calculate the available space between the top row and the bottom of the form
                int topRowBottom = Math.Max(countriesOfOrigin_Chart.Bottom, Math.Max(companiesOfOrigin_Chart.Bottom, countriesOfDestination_Chart.Bottom));
                int availableHeight = Height - topRowBottom - 30;

                // Center the accountants_Chart in the remaining space
                accountants_Chart.Left = 20;
                accountants_Chart.Top = topRowBottom + (availableHeight - chartHeight) / 2;
            }

            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2 - UI.spaceToOffsetFormNotCenter, Height - messagePanel.Height - 80);
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
                    return true;  // Cancel the closing
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
                UI.fileMenu.Location = new Point(File_Button.Left, Top_Panel.Height);
                Controls.Add(UI.fileMenu);
                UI.fileMenu.BringToFront();
                Focus();
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
            Purchases_DataGridView.ColumnWidthChanged -= DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Purchases;
            selectedDataGridView = Purchases_DataGridView;
            Controls.Add(Purchases_DataGridView);
            ResizeControls();
            Controls.Remove(Sales_DataGridView);
            ApplyFilters();
            LoadCharts();
            UpdateTotals();

            UnselectButtons();
            SelectButton(Purchases_Button);

            Purchases_DataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            AlignTotalLabels();
            Search_TextBox.PlaceholderText = "Search for purchases";
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Sales_DataGridView.ColumnWidthChanged -= DataGridView_ColumnWidthChanged;

            AddMainControls();
            Selected = SelectedOption.Sales;
            selectedDataGridView = Sales_DataGridView;
            Controls.Add(Sales_DataGridView);
            ResizeControls();
            Controls.Remove(Purchases_DataGridView);
            ApplyFilters();
            LoadCharts();
            UpdateTotals();

            UnselectButtons();
            SelectButton(Sales_Button);

            Sales_DataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
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
            LoadCharts();
        }
        private void Edit_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            UI.Rename_TextBox.Text = CompanyName_Label.Text;
            UI.Rename_TextBox.Font = CompanyName_Label.Font;
            Controls.Add(UI.Rename_TextBox);
            UI.Rename_TextBox.Location = new Point(CompanyName_Label.Left, CompanyName_Label.Top + CompanyName_Label.Parent.Top - 1);
            UI.Rename_TextBox.Size = new Size(300, CompanyName_Label.Height + 2);
            UI.Rename_TextBox.Focus();
            UI.Rename_TextBox.SelectAll();
            UI.Rename_TextBox.BringToFront();
            MainTop_Panel.Controls.Remove(Edit_Button);
            MainTop_Panel.Controls.Remove(CompanyName_Label);
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
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
        private void ApplyFilters()
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
                FilterDataGridViewByDateRange(selectedDataGridView);
            }

            bool filterExists = interval != TimeInterval.AllTime ||
                !string.IsNullOrEmpty(Search_TextBox.Text) ||
                !comboBoxEnabled;

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
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
            }

            if (filterExists)
            {
                ShowShowingResultsForLabel();
            }
            else
            {
                HideShowingResultsForLabel();
            }
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

        // Company label
        public void RenameCompany()
        {
            if (!Controls.Contains(UI.Rename_TextBox))
            {
                return;
            }

            // If the company name already exists in this directory
            string parentDir = Directory.GetParent(Directories.ArgoCompany_file).FullName;
            string filePath = parentDir + @"\" + UI.Rename_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension;
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
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.OkCancel);

                if (result == CustomMessageBoxResult.Ok)
                {
                    UI.Rename_TextBox.Text = suggestedCompanyName;
                }
                else { return; }
            }

            Controls.Remove(UI.Rename_TextBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);

            // If the name did not change
            if (UI.Rename_TextBox.Text == CompanyName_Label.Text)
            {
                return;
            }

            CompanyName_Label.Text = UI.Rename_TextBox.Text;
            ArgoCompany.Rename(UI.Rename_TextBox.Text);
            UI.Rename_TextBox.Text = "";
            MoveEditButton();
            ResizeControls();

            Text = $"Argo Sales Tracker - {Directories.CompanyName}";

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"Renamed program: {CompanyName_Label.Text}");
        }
        private void SetCompanyLabel()
        {
            CompanyName_Label.Text = Directories.CompanyName;
            MoveEditButton();
        }
        private void MoveEditButton()
        {
            Edit_Button.Left = CompanyName_Label.Left + CompanyName_Label.Width + 5;
        }

        // Search DataGridView
        public void ShowShowingResultsForLabel()
        {
            string text = "Showing results for";

            // Append search text if available
            if (!string.IsNullOrEmpty(Search_TextBox.Text))
            {
                text += $" '{Search_TextBox.Text}'";
            }

            // Handle ComboBox case
            if (Filter_ComboBox.Enabled && !string.IsNullOrEmpty(Filter_ComboBox.Text) && Filter_ComboBox.Text != timeIntervals[0].displayString)
            {
                if (!string.IsNullOrEmpty(Search_TextBox.Text))
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
                if (!string.IsNullOrEmpty(Search_TextBox.Text))
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
            ShowingResultsFor_Label.Location = new Point(
                (Width - ShowingResultsFor_Label.Width) / 2 - UI.spaceToOffsetFormNotCenter,
                MainTop_Panel.Bottom + (Distribution_Chart.Top - MainTop_Panel.Bottom - ShowingResultsFor_Label.Height) / 2);

            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
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

        // DataGridView
        public bool isProgramLoading;
        public SelectedOption Selected;
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
            ItemsInPurchase
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
            { Column.Name, "Buyer name" },
            { Column.Product, "Product name" },
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
            { Column.Name, "Customer name" },
            { Column.Product, "Product name" },
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
        public Guna2DataGridView Purchases_DataGridView, Sales_DataGridView, selectedDataGridView;
        private DataGridViewRow removedRow;
        private Control controlRightClickPanelWasAddedTo;
        private bool doNotDeleteRows;
        public DataGridViewRow selectedRowInMainMenu;
        private readonly byte rowHeight = 35, columnHeaderHeight = 60;
        public void InitializeDataGridView<TEnum>(Guna2DataGridView dataGridView, Size size, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad = null) where TEnum : Enum
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
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = CustomColors.text;
            dataGridView.Theme = CustomColors.dataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.controlBack;
            dataGridView.Size = size;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView.ScrollBars = ScrollBars.Vertical;

            dataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
            dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            dataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView.MouseDown += DataGridView_MouseDown;
            dataGridView.MouseUp += DataGridView_MouseUp;
            dataGridView.KeyDown += DataGridView_KeyDown;
            dataGridView.CellMouseClick += DataGridView_CellMouseClick;
            dataGridView.CellMouseMove += DataGridView_CellMouseMove;
            dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;

            LoadColumnsInDataGridView(dataGridView, columnHeaders, columnsToLoad);
            Theme.UpdateDataGridViewHeaderTheme(dataGridView);
        }
        public void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (doNotDeleteRows)
            {
                e.Cancel = true;
                return;
            }

            removedRow = e.Row;

            string type = "", columnName = "";
            byte logIndex = 0;

            switch (Selected)
            {
                case SelectedOption.Purchases:
                    type = "purchase";
                    columnName = Column.Product.ToString();
                    logIndex = 2;
                    break;

                case SelectedOption.Sales:
                    type = "sale";
                    columnName = Column.Product.ToString();
                    logIndex = 2;
                    break;

                case SelectedOption.ProductPurchases:
                    type = "product for purchase";
                    columnName = Products_Form.Column.ProductName.ToString();
                    logIndex = 3;

                    // Remove product from list
                    categoryPurchaseList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == e.Row.Cells[columnName].Value?.ToString())));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case SelectedOption.ProductSales:
                    type = "product for sale";
                    columnName = Products_Form.Column.ProductName.ToString();
                    logIndex = 3;

                    // Remove product from list
                    categorySaleList.ForEach(c => c.ProductList.Remove(c.ProductList.FirstOrDefault(p => p.Name == e.Row.Cells[columnName].Value?.ToString())));

                    // In case the product name that is being deleted is in the TextBox
                    Products_Form.Instance.ValidateProductNameTextBox();
                    break;

                case SelectedOption.CategoryPurchases:
                    type = "Category for purchase";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;

                    // Remove category from list
                    categoryPurchaseList.Remove(categoryPurchaseList.FirstOrDefault(c => c.Name == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case SelectedOption.CategorySales:
                    type = "Category for sale";
                    columnName = Categories_Form.Columns.CategoryName.ToString();
                    logIndex = 3;

                    // Remove category from list
                    categorySaleList.Remove(categorySaleList.FirstOrDefault(c => c.Name == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the category name that is being deleted is in the TextBox
                    Categories_Form.Instance.VaidateCategoryTextBox();
                    break;

                case SelectedOption.Accountants:
                    type = "Accountant";
                    columnName = Accountants_Form.Columns.AccountantName.ToString();
                    logIndex = 2;

                    // Remove accountant from list
                    accountantList.Remove(accountantList.FirstOrDefault(a => a == e.Row.Cells[columnName].Value?.ToString()));

                    // In case the accountant name that is being deleted is in the TextBox
                    Accountants_Form.Instance.VaidateAccountantTextBox();
                    break;

                case SelectedOption.Companies:
                    type = "Companies";
                    columnName = Companies_Form.Columns.Company.ToString();
                    logIndex = 2;

                    // Remove accountant from list
                    companyList.Remove(companyList.FirstOrDefault(a => a == e.Row.Cells[columnName].Value?.ToString()));
                    break;

                case SelectedOption.ItemsInPurchase:
                    columnName = Column.Product.ToString();
                    string name1 = e.Row.Cells[columnName].Value?.ToString();
                    columnName = Column.Category.ToString();
                    string purchase = e.Row.Cells[columnName].Value?.ToString();
                    List<string> tagList = (List<string>)selectedRowInMainMenu.Tag;

                    byte index = 1;
                    if (tagList.Last().StartsWith(receipt_text))
                    {
                        index = 2;
                    }

                    string selected;
                    if (Selected == SelectedOption.Purchases)
                    {
                        selected = "purchase";
                    }
                    else
                    {
                        selected = "sale";
                    }

                    if (tagList.Count == index)
                    {
                        CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", $"Deleting the last item will also delete the {selected}.", CustomMessageBoxIcon.None, CustomMessageBoxButtons.OkCancel);

                        if (result != CustomMessageBoxResult.Ok)
                        {
                            e.Cancel = true;
                            return;
                        }
                        itemsInPurchase_Form.Close();
                        e.Cancel = true;
                        Log.Write(2, $"Deleted item '{name1}' in {selected} '{purchase}'");
                        return;
                    }

                    // Remove the row from the tag
                    tagList.RemoveAt(e.Row.Index);

                    Log.Write(2, $"Deleted item '{name1}' in {selected} '{purchase}'");
                    break;
            }
            string name = e.Row.Cells[columnName].Value?.ToString();

            Log.Write(logIndex, $"Deleted {type} '{name}'");
        }
        public void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            AlignTotalLabels();
        }
        public void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            DataGridViewRowChanged();

            // Remove receipt from file
            if (Selected is SelectedOption.Purchases or SelectedOption.Sales && removedRow?.Tag != null)
            {
                string tagValue = "";

                if (removedRow.Tag is (List<string> tagList, TagData))
                {
                    tagValue = tagList[^1];
                }
                else if (removedRow.Tag is (string tagString, TagData))
                {
                    tagValue = tagString;
                }
                Directories.DeleteFile(tagValue);

                removedRow = null;
            }
        }
        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isProgramLoading) { return; }
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{Selected} list");
            DataGridViewRowChanged();
        }
        public void DataGridViewRowChanged()
        {
            if (Selected == SelectedOption.Purchases || Selected == SelectedOption.Sales)
            {
                UpdateTotals();
                LoadCharts();
                SaveDataGridViewToFileAsJson();
            }
            else if (Selected == SelectedOption.CategoryPurchases || Selected == SelectedOption.CategorySales ||
                Selected == SelectedOption.ProductPurchases || Selected == SelectedOption.ProductSales)
            {
                SaveCategoriesToFile(Selected);
            }
            else if (Selected == SelectedOption.Accountants || Selected == SelectedOption.Companies)
            {
                SaveDataGridViewToFile();
            }
        }
        public void DataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            UI.CloseAllPanels(null, null);

            Guna2DataGridView grid = (Guna2DataGridView)sender;
            DataGridView.HitTestInfo info = grid.HitTest(e.X, e.Y);

            if (Selected is SelectedOption.Purchases or SelectedOption.Sales && info.RowIndex != -1)
            {
                selectedRowInMainMenu = grid.Rows[info.RowIndex];
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
        public void DataGridView_MouseUp(object sender, MouseEventArgs e)
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

                FlowLayoutPanel flowPanel = (FlowLayoutPanel)rightClickDataGridView_Panel.Controls[0];

                // Add move button
                if (Selected == SelectedOption.CategoryPurchases)
                {
                    ConfigureMoveButton("Move category to sales");
                }
                else if (Selected == SelectedOption.CategorySales)
                {
                    ConfigureMoveButton("Move category to purchases");
                }
                else if (Selected == SelectedOption.ProductPurchases)
                {
                    ConfigureMoveButton("Move product to sales");
                }
                else if (Selected == SelectedOption.ProductSales)
                {
                    ConfigureMoveButton("Move product to purchases");
                }
                else
                {
                    flowPanel.Controls.Remove(rightClickDataGridView_MoveBtn);
                }

                flowPanel.Controls.Remove(rightClickDataGridView_ShowItemsBtn);
                flowPanel.Controls.Remove(rightClickDataGridView_ExportReceiptBtn);

                if (grid.SelectedRows[0].Tag is (List<string> tagList, TagData))
                {
                    ShowShowItemsBtn(flowPanel, 1);

                    // Check if the last item starts with "receipt:"
                    string lastItem = tagList[^1];
                    if (lastItem.StartsWith(receipt_text))
                    {
                        lastItem = lastItem.Substring(8).Replace(companyName_text, Directories.CompanyName);

                        if (File.Exists(lastItem))
                        {
                            ShowExportReceiptBtn(flowPanel, 2);
                        }
                    }
                }
                else if (grid.SelectedRows[0].Tag is (string, TagData))
                {
                    ShowExportReceiptBtn(flowPanel, 1);
                }

                // Adjust the panel height based on the number of controls
                int controlCount = flowPanel.Controls.Count;
                rightClickDataGridView_Panel.Height = controlCount * UI.panelButtonHeight + 10;
                flowPanel.Height = controlCount * UI.panelButtonHeight;

                Control controlSender = (Control)sender;
                controlRightClickPanelWasAddedTo = controlSender.Parent;
                Form parentForm = grid.FindForm();
                int formHeight = parentForm.ClientSize.Height;
                int formWidth = parentForm.ClientSize.Width;
                byte padding = 5;

                // Calculate the horizontal position
                bool tooFarRight = false;
                if (selectedDataGridView.Left + rightClickDataGridView_Panel.Width + e.X - spaceForRightClickPanel + padding > formWidth)
                {
                    rightClickDataGridView_Panel.Left = formWidth - rightClickDataGridView_Panel.Width - padding;
                    tooFarRight = true;
                }
                else
                {
                    rightClickDataGridView_Panel.Left = selectedDataGridView.Left + e.X - spaceForRightClickPanel;
                }

                // Calculate the vertical position
                int verticalOffset = grid.FirstDisplayedScrollingRowIndex * grid.Rows[0].Height;
                int rowTop = (info.RowIndex + 1) * grid.Rows[0].Height - verticalOffset + selectedDataGridView.Top + columnHeaderHeight;

                if (rowTop + rightClickDataGridView_Panel.Height > formHeight + padding)
                {
                    rightClickDataGridView_Panel.Top = formHeight - rightClickDataGridView_Panel.Height - padding;
                    if (!tooFarRight)
                    {
                        rightClickDataGridView_Panel.Left += spaceForRightClickPanel;
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
        public void DataGridView_KeyDown(object sender, KeyEventArgs e)
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
        private void DataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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
        private void DataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
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
        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns.Count - 1)
            {
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null && cell.Value.ToString() == show_text)
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

            if (cell.Value != null && cell.Value.ToString() == show_text)
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
        public void DataGridViewRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            DataGridViewRowChanged();
            DataGridViewRow row;

            if (e.RowIndex >= 0 && e.RowIndex < selectedDataGridView.RowCount)
            {
                row = selectedDataGridView.Rows[e.RowIndex];
            }
            else
            {
                Log.Error_RowIsOutOfRange();
                return;
            }

            // Perform sorting based on the current sorted column and direction
            if (selectedDataGridView.SortedColumn != null)
            {
                SortOrder sortOrder = selectedDataGridView.SortOrder;
                DataGridViewColumn sortedColumn = selectedDataGridView.SortedColumn;
                ListSortDirection direction = (sortOrder == SortOrder.Ascending) ?
                                              ListSortDirection.Ascending : ListSortDirection.Descending;
                selectedDataGridView.Sort(sortedColumn, direction);
            }

            if (Selected is SelectedOption.Purchases or SelectedOption.Sales)
            {
                SortDataGridView();
            }

            // Calculate the middle index
            int visibleRowCount = selectedDataGridView.DisplayedRowCount(true);
            int middleIndex = Math.Max(0, row.Index - (visibleRowCount / 2) + 1);

            // Ensure the row at middleIndex is visible
            if (middleIndex >= 0 && middleIndex < selectedDataGridView.RowCount && selectedDataGridView.Rows[middleIndex].Visible)
            {
                selectedDataGridView.FirstDisplayedScrollingRowIndex = middleIndex;
            }

            // Select the added row
            UnselectAllRowsInCurrentDataGridView();
            selectedDataGridView.Rows[row.Index].Selected = true;
        }
        public static void LoadColumnsInDataGridView<TEnum>(Guna2DataGridView dataGridView, Dictionary<TEnum, string> columnHeaders, List<TEnum>? columnsToLoad = null) where TEnum : Enum
        {
            columnsToLoad ??= Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

            foreach (TEnum column in columnsToLoad)
            {
                if (columnHeaders.TryGetValue(column, out string? value))
                {
                    dataGridView.Columns.Add(column.ToString(), value);
                }
            }
            Theme.UpdateDataGridViewHeaderTheme(dataGridView);
        }
        private void ConfigureMoveButton(string buttonText)
        {
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)rightClickDataGridView_Panel.Controls[0];

            // Ensure the Move button is the second control
            flowPanel.Controls.Add(rightClickDataGridView_MoveBtn);
            flowPanel.Controls.SetChildIndex(rightClickDataGridView_MoveBtn, 1);

            rightClickDataGridView_MoveBtn.Text = buttonText;
        }
        private void UpdateTotals()
        {
            if (isProgramLoading || Selected != SelectedOption.Purchases && Selected != SelectedOption.Sales)
            {
                return;
            }

            if (!DoDataGridViewsHaveVisibleRows(selectedDataGridView))
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

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
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
        private void AlignTotalLabels()
        {
            string quantityColumn = Column.Quantity.ToString();
            string taxColumn = Column.Tax.ToString();
            string shippingColumn = Column.Shipping.ToString();
            string feeColumn = Column.Fee.ToString();
            string chargedDifference = Column.ChargedDifference.ToString();
            string totalPriceColumn = Column.Total.ToString();

            Quantity_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = selectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = selectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = selectedDataGridView.Columns[shippingColumn].Width;

            PaymentFee_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[feeColumn].Index, -1, true).Left;
            PaymentFee_Label.Width = selectedDataGridView.Columns[feeColumn].Width;

            if (selectedDataGridView == Purchases_DataGridView)
            {
                Total_Panel.Controls.Add(ChargedDifference_Label);
                ChargedDifference_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[chargedDifference].Index, -1, true).Left;
                ChargedDifference_Label.Width = selectedDataGridView.Columns[chargedDifference].Width;
            }
            else
            {
                Total_Panel.Controls.Remove(ChargedDifference_Label);
            }

            Price_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = selectedDataGridView.Columns[totalPriceColumn].Width;
        }
        private static string GetFilePathForDataGridView(SelectedOption selected)
        {
            return selected switch
            {
                SelectedOption.Purchases => Directories.Purchases_file,
                SelectedOption.Sales => Directories.Sales_file,
                SelectedOption.CategoryPurchases => Directories.CategoryPurchases_file,
                SelectedOption.CategorySales => Directories.CategorySales_file,
                SelectedOption.ProductPurchases => Directories.CategoryPurchases_file,
                SelectedOption.ProductSales => Directories.CategorySales_file,
                SelectedOption.Accountants => Directories.Accountants_file,
                SelectedOption.Companies => Directories.Companies_file,
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
        public static bool DoDataGridViewsHaveVisibleRows(params DataGridView[] dataGridViews)
        {
            foreach (DataGridView dataGrid in dataGridViews)
            {
                if (dataGrid.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dataGrid.Rows)
                    {
                        if (row.Visible)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void SortTheDataGridViewByDate()
        {
            string dateColumnHeader = SalesColumnHeaders[Column.Date];
            Sales_DataGridView.Sort(Sales_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);

            dateColumnHeader = PurchaseColumnHeaders[Column.Date];
            Purchases_DataGridView.Sort(Purchases_DataGridView.Columns[dateColumnHeader], ListSortDirection.Ascending);
        }
        public static void SortTheDataGridViewByFirstColumn(params DataGridView[] dataGridViews)
        {
            foreach (DataGridView dataGrid in dataGridViews)
            {
                if (dataGrid.Columns.Count > 0)
                {
                    dataGrid.Sort(dataGrid.Columns[0], ListSortDirection.Ascending);
                }
            }
        }
        public void UpdateRow()
        {
            isProgramLoading = true;

            List<string> items = null;

            // Check if Tag is a ValueTuple
            if (selectedRowInMainMenu.Tag is (List<string> itemList, TagData))
            {
                items = itemList;
            }
            else if (selectedRowInMainMenu.Tag is List<string> list)
            {
                items = list;
            }

            if (items == null)
            {
                // Handle case where items are null (Optional)
                throw new InvalidOperationException("Tag does not contain a valid list of items.");
            }

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

            string categoryName = isCategoryNameConsistent ? firstCategoryName : emptyCell;
            string country = isCountryConsistent ? firstCountry : emptyCell;
            string company = isCompanyConsistent ? firstCompany : emptyCell;

            selectedRowInMainMenu.Cells[Column.Category.ToString()].Value = categoryName;
            selectedRowInMainMenu.Cells[Column.Country.ToString()].Value = country;
            selectedRowInMainMenu.Cells[Column.Company.ToString()].Value = company;
            selectedRowInMainMenu.Cells[Column.Quantity.ToString()].Value = items.Count - 1;

            // Update charged difference
            if (Selected == SelectedOption.Purchases)
            {
                int quantity = int.Parse(selectedRowInMainMenu.Cells[Column.Quantity.ToString()].Value.ToString());
                decimal shipping = decimal.Parse(selectedRowInMainMenu.Cells[Column.Shipping.ToString()].Value.ToString());
                decimal tax = decimal.Parse(selectedRowInMainMenu.Cells[Column.Tax.ToString()].Value.ToString());
                decimal totalPrice = quantity * pricePerUnit + shipping + tax;
                selectedRowInMainMenu.Cells[Column.ChargedDifference.ToString()].Value = Convert.ToDecimal(selectedRowInMainMenu.Cells[Column.Total.ToString()].Value) - totalPrice;
            }

            isProgramLoading = false;
        }
        public static void AddNoteToCell(int newRowIndex, string note)
        {
            Guna2DataGridView selectedDataGridView = Instance.selectedDataGridView;
            DataGridViewCell cell = selectedDataGridView.Rows[newRowIndex].Cells[^1];
            cell.Tag = note;
            AddUnderlineToCell(cell);
        }
        private static void AddUnderlineToCell(DataGridViewCell cell)
        {
            cell.Style.Font = new Font(cell.DataGridView.DefaultCellStyle.Font, FontStyle.Underline);
        }

        // Save to file for DataGridView
        public void SaveDataGridViewToFileAsJson()
        {
            string filePath = GetFilePathForDataGridView(Selected);
            List<Dictionary<string, object>> rowsData = new();

            // Collect data from the DataGridView
            foreach (DataGridViewRow row in selectedDataGridView.Rows)
            {
                Dictionary<string, object> rowData = new();

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value?.ToString() == show_text && cell.Tag != null)
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

                rowsData.Add(rowData);
            }

            // Serialize to JSON and write to file
            string json = JsonSerializer.Serialize(rowsData, jsonOptions);
            Directories.WriteTextToFile(filePath, json);

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{Selected} list");
        }
        public void SaveCategoriesToFile(SelectedOption option)
        {
            if (isProgramLoading)
            {
                return;
            }

            string filePath = GetFilePathForDataGridView(option);

            List<Category> categoryList;
            if (option == SelectedOption.CategoryPurchases || option == SelectedOption.ProductPurchases)
            {
                categoryList = categoryPurchaseList;
            }
            else
            {
                categoryList = categorySaleList;
            }

            string json = JsonSerializer.Serialize(categoryList, jsonOptions);
            Directories.WriteTextToFile(filePath, json);

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{Selected} list");
        }
        private void SaveDataGridViewToFile()
        {
            string filePath = GetFilePathForDataGridView(Selected);
            List<string> linesInDataGridView = [];

            // Write all the rows in the DataGridView to file
            for (int i = 0; i < selectedDataGridView.Rows.Count; i++)
            {
                DataGridViewRow row = selectedDataGridView.Rows[i];
                List<string> cellValues = [];

                foreach (DataGridViewCell cell in row.Cells)
                {
                    cellValues.Add(cell.Value?.ToString());
                }

                // Add the row tag
                if (row.Tag != null)
                {
                    cellValues.Add(row.Tag?.ToString());
                }

                string line = string.Join(",", cellValues);
                linesInDataGridView.Add(line);
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, $"{Selected} list");
        }

        // Right click DataGridView row
        public Guna2Panel rightClickDataGridView_Panel;
        private Guna2Button rightClickDataGridView_MoveBtn, rightClickDataGridView_ExportReceiptBtn, rightClickDataGridView_ShowItemsBtn;
        public Guna2Button rightClickDataGridView_DeleteBtn;
        public void ConstructRightClickDataGridViewRowMenu()
        {
            rightClickDataGridView_Panel = UI.ConstructPanelForMenu(new Size(UI.panelWidth, 5 * UI.panelButtonHeight + UI.spaceForPanel));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)rightClickDataGridView_Panel.Controls[0];

            Guna2Button menuBtn = UI.ConstructBtnForMenu("Modify", UI.panelBtnWidth, false, flowPanel);
            menuBtn.Click += ModifyRow;

            rightClickDataGridView_MoveBtn = UI.ConstructBtnForMenu("Move", UI.panelBtnWidth, false, flowPanel);
            rightClickDataGridView_MoveBtn.Click += MoveRow;

            rightClickDataGridView_ExportReceiptBtn = UI.ConstructBtnForMenu("Export receipt", UI.panelBtnWidth, false, flowPanel);
            rightClickDataGridView_ExportReceiptBtn.Click += ExportReceipt;

            rightClickDataGridView_ShowItemsBtn = UI.ConstructBtnForMenu("Show items", UI.panelBtnWidth, false, flowPanel);
            rightClickDataGridView_ShowItemsBtn.Click += ShowItems;

            rightClickDataGridView_DeleteBtn = UI.ConstructBtnForMenu("Delete", UI.panelBtnWidth, false, flowPanel);
            rightClickDataGridView_DeleteBtn.ForeColor = CustomColors.accent_red;
            rightClickDataGridView_DeleteBtn.Click += DeleteRow;

            UI.ConstructKeyShortcut("Del", rightClickDataGridView_DeleteBtn);
        }
        private void ModifyRow(object sender, EventArgs e)
        {
            CloseRightClickPanels();
            if (selectedDataGridView.SelectedRows.Count > 1)
            {
                CustomMessageBox.Show("Argo Sales Tracker", "You can only select one row to modify.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                return;
            }

            ModifyRow_Form modifyRow_Form = new(selectedDataGridView.SelectedRows[0]);
            modifyRow_Form.ShowDialog();
        }
        private void MoveRow(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            DataGridViewRow selectedRow = selectedDataGridView.SelectedRows[0];
            int selectedIndex = selectedRow.Index;

            // Save the current scroll position
            int scrollPosition = selectedDataGridView.FirstDisplayedScrollingRowIndex;

            if (Selected == SelectedOption.CategoryPurchases)
            {
                Categories_Form.Instance.Purchases_DataGridView.Rows.Remove(selectedRow);
                Categories_Form.Instance.Sales_DataGridView.Rows.Add(selectedRow);
            }
            else if (Selected == SelectedOption.CategorySales)
            {
                Categories_Form.Instance.Sales_DataGridView.Rows.Remove(selectedRow);
                Categories_Form.Instance.Purchases_DataGridView.Rows.Add(selectedRow);
            }
            else if (Selected == SelectedOption.ProductPurchases)
            {
                Products_Form.Instance.Purchases_DataGridView.Rows.Remove(selectedRow);
                Products_Form.Instance.Sales_DataGridView.Rows.Add(selectedRow);
            }
            else if (Selected == SelectedOption.ProductSales)
            {
                Products_Form.Instance.Sales_DataGridView.Rows.Remove(selectedRow);
                Products_Form.Instance.Purchases_DataGridView.Rows.Add(selectedRow);
            }

            // Select a new row
            if (selectedIndex < selectedDataGridView.Rows.Count)
            {
                selectedDataGridView.Rows[selectedIndex].Selected = true;
            }
            else if (selectedDataGridView.Rows.Count > 0)
            {
                selectedDataGridView.Rows[^1].Selected = true;
            }

            // Restore the scroll position
            selectedDataGridView.FirstDisplayedScrollingRowIndex = scrollPosition;
        }
        private void ExportReceipt(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            DataGridViewRow selectedRow = selectedDataGridView.SelectedRows[0];
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
        private ItemsInPurchase_Form itemsInPurchase_Form;
        private void ShowItems(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            itemsInPurchase_Form = new ItemsInPurchase_Form(selectedDataGridView.SelectedRows[0]);
            itemsInPurchase_Form.ShowDialog();
        }
        private void DeleteRow(object sender, EventArgs e)
        {
            CloseRightClickPanels();

            int index = selectedDataGridView.SelectedRows[^1].Index;

            // Delete all selected rows
            foreach (DataGridViewRow item in selectedDataGridView.SelectedRows)
            {
                DataGridView_UserDeletingRow(selectedDataGridView, new(item));
                selectedDataGridView.Rows.Remove(item);
            }

            // Select the row under the row that was just deleted
            if (selectedDataGridView.Rows.Count != 0)
            {
                // If the deleted row was not the last one, select the next row
                if (index < selectedDataGridView.Rows.Count)
                {
                    selectedDataGridView.Rows[index].Selected = true;
                }
                else  // If the deleted row was the last one, select the new last row
                {
                    selectedDataGridView.Rows[^1].Selected = true;
                }
            }
        }

        // Methods for right click DataGridView row
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
            path = path.Replace(companyName_text, Directories.CompanyName)
                       .Replace(receipt_text, "");

            return File.Exists(path) ? path : "";
        }
        private void ShowShowItemsBtn(FlowLayoutPanel flowPanel, int index)
        {
            flowPanel.Controls.Add(rightClickDataGridView_ShowItemsBtn);
            flowPanel.Controls.SetChildIndex(rightClickDataGridView_ShowItemsBtn, index);
        }
        private void ShowExportReceiptBtn(FlowLayoutPanel flowPanel, int index)
        {
            flowPanel.Controls.Add(rightClickDataGridView_ExportReceiptBtn);
            flowPanel.Controls.SetChildIndex(rightClickDataGridView_ExportReceiptBtn, index);
        }

        // Statistics menu
        private List<Control> GetMainControlsList()
        {
            return [
                Sales_DataGridView,
                Purchases_DataGridView,
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
        private List<Control> statisticsControls;
        public GunaChart countriesOfOrigin_Chart, companiesOfOrigin_Chart, countriesOfDestination_Chart, accountants_Chart;
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

            statisticsControls =
            [
                countriesOfOrigin_Chart,
                companiesOfOrigin_Chart,
                countriesOfDestination_Chart,
                accountants_Chart
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
            LoadChart.LoadCountriesOfOriginForProductsIntoChart(Purchases_DataGridView, countriesOfOrigin_Chart);
            LoadChart.LoadCompaniesOfOriginForProductsIntoChart(Purchases_DataGridView, companiesOfOrigin_Chart);
            LoadChart.LoadCountriesOfDestinationForProductsIntoChart(Sales_DataGridView, countriesOfDestination_Chart);
            LoadChart.LoadAccountantsIntoChart([Purchases_DataGridView, Sales_DataGridView], accountants_Chart);
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
                Image = Resources.CloseGray,
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
        private void MessagePanelClose(object sender, EventArgs e)
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

        // Receipts
        public static (string, bool) SaveReceiptInFile(string receiptFilePath)
        {
            string newReceiptsDir = Directories.Receipts_dir.Replace(companyName_text, Directories.CompanyName);
            string newFilePath = newReceiptsDir + Path.GetFileName(receiptFilePath);

            if (File.Exists(newFilePath))
            {
                // Get a new name for the file
                string name = Path.GetFileNameWithoutExtension(receiptFilePath);
                List<string> fileNames = Directories.GetListOfAllFilesWithoutExtensionInDirectory(newReceiptsDir);

                string suggestedThingName = Tools.AddNumberForAStringThatAlreadyExists(name, fileNames);

                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"Rename receipt",
                    $"Do you want to rename '{name}' to '{suggestedThingName}'? There is already a receipt with the same name.",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.OkCancel);

                if (result == CustomMessageBoxResult.Ok)
                {
                    newFilePath = newReceiptsDir + suggestedThingName + Path.GetExtension(receiptFilePath);
                }
                else { return ("", false); }
            }

            // Save receipt
            bool saved = Directories.CopyFile(receiptFilePath.Replace(receipt_text, ""), newFilePath);
            string newPath = receipt_text + newFilePath.Replace(Directories.CompanyName, companyName_text);
            return (newPath, saved);
        }
        public static void RemoveReceiptFromFile(DataGridViewRow row)
        {
            string filePath = row.Tag.ToString();

            if (File.Exists(filePath))
            {
                Directories.DeleteFile(filePath);
            }
            row.Tag = null;
        }
        public static void AddReceiptToTag(DataGridViewRow row, string filePath)
        {
            if (row.Tag is List<string> tagList)
            {
                tagList[^1] = filePath;
            }
            else
            {
                row.Tag = filePath;
            }
        }
        public static bool CheckIfReceiptExists(string receiptFilePath)
        {
            if (!File.Exists(receiptFilePath.Replace(receipt_text, "")))
            {
                string message = $"The receipt you selected no longer exists";
                CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return false;
            }
            return true;
        }

        // Misc.
        public bool IsPurchasesSelected()
        {
            return selectedDataGridView == Purchases_DataGridView;
        }
        public void CloseRightClickPanels()
        {
            controlRightClickPanelWasAddedTo?.Controls.Remove(rightClickDataGridView_Panel);
            doNotDeleteRows = false;
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}