using Sales_Tracker.Charts;
using Sales_Tracker.ReportGenerator.Elements;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// First step in report generation - allows users to select charts, transactions, and apply filters.
    /// </summary>
    public partial class ReportDataSelection_Form : Form
    {
        // Properties
        private static ReportDataSelection_Form _instance;
        private int _initialFormWidth;
        private int _initialLeftPanelWidth;
        private int _initialRightPanelWidth;
        private CustomCheckListBox ChartSelection_CheckedListBox;
        private readonly List<MainMenu_Form.ChartDataType> _chartTypeOrder = [];

        /// <summary>
        /// Gets the parent report generator form.
        /// </summary>
        public ReportGenerator_Form ParentReportForm { get; private set; }

        /// <summary>
        /// Gets the current report configuration.
        /// </summary>
        private ReportConfiguration? ReportConfig => ParentReportForm?.CurrentReportConfiguration;

        /// <summary>
        /// Indicates if the form is currently being loaded/updated programmatically.
        /// </summary>
        private bool _isUpdating;

        // Getters
        public static ReportDataSelection_Form Instance => _instance;

        // Init.
        public ReportDataSelection_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            _instance = this;
            ParentReportForm = parentForm;

            // This fixes a bug. I have a similar setup on ReportPreviewExport_Form but it doesn't have this issue. I'm not sure why.
            Filters_GroupBox.Anchor = AnchorStyles.None;
            Filters_GroupBox.Height = Right_Panel.Height - Right_Panel.Padding.Bottom - Filters_GroupBox.Top;
            Filters_GroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            InitChartSelectionControl();
            SetupChartSelection();
            SetupFilterControls();
            SetupTemplates();
            StoreInitialSizes();
        }
        private void InitChartSelectionControl()
        {
            ChartSelection_CheckedListBox = new()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            ChartSelection_CheckedListBox.SetBounds(
                ChartSelection_GroupBox.Padding.Left + 10,  // Left
                ChartSelection_GroupBox.Padding.Top + ChartSelection_GroupBox.CustomBorderThickness.Top + 10,  // Top (allowing for group box title)
                ChartSelection_GroupBox.ClientSize.Width - (ChartSelection_GroupBox.Padding.Horizontal + 20),  // Width
                ChartSelection_GroupBox.ClientSize.Height - (ChartSelection_GroupBox.Padding.Vertical + 120)   // Height
            );

            ThemeManager.CustomizeScrollBar(ChartSelection_CheckedListBox);
            ChartSelection_CheckedListBox.ItemCheck += new ItemCheckEventHandler(ChartSelection_CheckedListBox_ItemCheck);

            ChartSelection_GroupBox.Controls.Add(ChartSelection_CheckedListBox);
        }
        private void SetupChartSelection()
        {
            // Overview Section
            ChartSelection_CheckedListBox.AddSection("Overview");
            AddChartItem(MainMenu_Form.ChartDataType.TotalExpensesVsSales, TranslatedChartTitles.SalesVsExpenses);
            AddChartItem(MainMenu_Form.ChartDataType.Profits, TranslatedChartTitles.TotalProfits);
            AddChartItem(MainMenu_Form.ChartDataType.TotalTransactions, TranslatedChartTitles.TotalTransactions);
            AddChartItem(MainMenu_Form.ChartDataType.AverageTransactionValue, TranslatedChartTitles.AverageTransactionValue);

            // Financial Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Financial");
            AddChartItem(MainMenu_Form.ChartDataType.TotalSales, TranslatedChartTitles.TotalRevenue);
            AddChartItem(MainMenu_Form.ChartDataType.TotalPurchases, TranslatedChartTitles.TotalExpenses);
            AddChartItem(MainMenu_Form.ChartDataType.DistributionOfSales, TranslatedChartTitles.RevenueDistribution);
            AddChartItem(MainMenu_Form.ChartDataType.DistributionOfPurchases, TranslatedChartTitles.ExpensesDistribution);

            // Geographic Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Geographic Analysis");
            AddChartItem(MainMenu_Form.ChartDataType.WorldMap, TranslatedChartTitles.WorldMap);
            AddChartItem(MainMenu_Form.ChartDataType.CountriesOfOrigin, TranslatedChartTitles.CountriesOfOrigin);
            AddChartItem(MainMenu_Form.ChartDataType.CountriesOfDestination, TranslatedChartTitles.CountriesOfDestination);
            AddChartItem(MainMenu_Form.ChartDataType.CompaniesOfOrigin, TranslatedChartTitles.CompaniesOfOrigin);

            // Performance Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Performance");
            AddChartItem(MainMenu_Form.ChartDataType.GrowthRates, TranslatedChartTitles.GrowthRates);

            // Operational Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Operational");
            AddChartItem(MainMenu_Form.ChartDataType.Accountants, TranslatedChartTitles.AccountantsTransactions);
            AddChartItem(MainMenu_Form.ChartDataType.AverageShippingCosts, TranslatedChartTitles.AverageShippingCosts);

            // Returns Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Returns Analysis");
            AddChartItem(MainMenu_Form.ChartDataType.ReturnsOverTime, TranslatedChartTitles.ReturnsOverTime);
            AddChartItem(MainMenu_Form.ChartDataType.ReturnReasons, TranslatedChartTitles.ReturnReasons);
            AddChartItem(MainMenu_Form.ChartDataType.ReturnFinancialImpact, TranslatedChartTitles.ReturnFinancialImpact);
            AddChartItem(MainMenu_Form.ChartDataType.ReturnsByCategory, TranslatedChartTitles.ReturnsByCategory);
            AddChartItem(MainMenu_Form.ChartDataType.ReturnsByProduct, TranslatedChartTitles.ReturnsByProduct);
            AddChartItem(MainMenu_Form.ChartDataType.PurchaseVsSaleReturns, TranslatedChartTitles.PurchaseVsSaleReturns);

            // Lost Products Section
            ChartSelection_CheckedListBox.AddSpacer(10);
            ChartSelection_CheckedListBox.AddSection("Lost Products");
            AddChartItem(MainMenu_Form.ChartDataType.LossesOverTime, TranslatedChartTitles.LossesOverTime);
            AddChartItem(MainMenu_Form.ChartDataType.LossReasons, TranslatedChartTitles.LossReasons);
            AddChartItem(MainMenu_Form.ChartDataType.LossFinancialImpact, TranslatedChartTitles.LossFinancialImpact);
            AddChartItem(MainMenu_Form.ChartDataType.LossesByCategory, TranslatedChartTitles.LossesByCategory);
            AddChartItem(MainMenu_Form.ChartDataType.LossesByProduct, TranslatedChartTitles.LossesByProduct);
            AddChartItem(MainMenu_Form.ChartDataType.PurchaseVsSaleLosses, TranslatedChartTitles.PurchaseVsSaleLosses);
        }
        private void AddChartItem(MainMenu_Form.ChartDataType chartType, string displayName)
        {
            _chartTypeOrder.Add(chartType);
            ChartSelection_CheckedListBox.Add(displayName, false);
        }
        private void SetupFilterControls()
        {
            // Setup date range
            StartDate_DateTimePicker.Value = DateTime.Now.AddMonths(-1);
            EndDate_DateTimePicker.Value = DateTime.Now;

            // Setup transaction type
            TransactionType_ComboBox.Items.Clear();
            TransactionType_ComboBox.Items.Add("Sales");
            TransactionType_ComboBox.Items.Add("Purchases");
            TransactionType_ComboBox.Items.Add("Both");
            TransactionType_ComboBox.SelectedIndex = 2;  // Both by default

            // Setup includes
            IncludeReturns_CheckBox.Checked = true;
            IncludeLosses_CheckBox.Checked = true;
        }
        private void SetupTemplates()
        {
            Template_ComboBox.Items.Clear();

            // Add templates from ReportTemplates class
            List<string> templates = ReportTemplates.GetAvailableTemplates();
            foreach (string templateName in templates)
            {
                Template_ComboBox.Items.Add(templateName);
            }

            // Select "Custom Report" by default
            Template_ComboBox.SelectedIndex = 0;
        }
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialLeftPanelWidth = Left_Panel.Width;
            _initialRightPanelWidth = Right_Panel.Width;
        }

        // Helper methods
        private MainMenu_Form.ChartDataType GetChartTypeFromIndex(int index)
        {
            if (index >= 0 && index < _chartTypeOrder.Count)
            {
                return _chartTypeOrder[index];
            }
            return MainMenu_Form.ChartDataType.TotalSales;
        }
        private List<MainMenu_Form.ChartDataType> GetSelectedChartTypes()
        {
            List<MainMenu_Form.ChartDataType> selectedCharts = [];

            for (int i = 0; i < ChartSelection_CheckedListBox.CheckedIndices.Count; i++)
            {
                int checkedIndex = ChartSelection_CheckedListBox.CheckedIndices[i];
                selectedCharts.Add(GetChartTypeFromIndex(checkedIndex));
            }

            return selectedCharts;
        }
        private TransactionType GetSelectedTransactionType()
        {
            return TransactionType_ComboBox.SelectedIndex switch
            {
                0 => TransactionType.Sales,
                1 => TransactionType.Purchases,
                2 => TransactionType.Both,
                _ => TransactionType.Both
            };
        }
        public void SwitchToCustomTemplate()
        {
            if (Template_ComboBox.SelectedIndex > 0)
            {
                PerformUpdate(() =>
                {
                    Template_ComboBox.SelectedIndex = 0;
                    if (ReportConfig != null)
                    {
                        ReportConfig.Title = ReportTemplates.TemplateNames.Custom;
                    }
                });
            }
        }

        // Form event handlers
        private void ReportDataSelection_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && !Visible)
            {
                OnStepDeactivated();
            }
        }
        private void ReportDataSelection_Form_Resize(object sender, EventArgs e)
        {
            if (_initialFormWidth == 0) { return; }

            // Calculate the form's width change ratio
            float widthRatio = (float)Width / _initialFormWidth;

            // Calculate new panel widths while maintaining proportion
            Left_Panel.Width = (int)(_initialLeftPanelWidth * widthRatio);
            Right_Panel.Width = (int)(_initialRightPanelWidth * widthRatio);

            // Position the right panel
            Right_Panel.Left = Left_Panel.Width;
        }

        // Event handlers
        private void SelectAll_Button_Click(object sender, EventArgs e)
        {
            if (_isUpdating) { return; }

            PerformUpdate(() =>
            {
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, true);
                }

                // Update the report configuration with current selections
                if (ReportConfig != null)
                {
                    ReportConfig.Filters.SelectedChartTypes = GetSelectedChartTypes();
                    UpdateElementsFromChartSelection();
                }

                NotifyParentValidationChanged();
            });
        }
        private void SelectNone_Button_Click(object sender, EventArgs e)
        {
            if (_isUpdating) { return; }

            PerformUpdate(() =>
            {
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, false);
                }

                // Update the report configuration with current selections
                if (ReportConfig != null)
                {
                    ReportConfig.Filters.SelectedChartTypes = GetSelectedChartTypes();
                    UpdateElementsFromChartSelection();
                }

                NotifyParentValidationChanged();
            });
        }
        private void ChartSelection_CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_isUpdating)
            {
                // If a non-custom template is selected and user makes changes, switch to custom
                if (Template_ComboBox.SelectedIndex > 0)
                {
                    PerformUpdate(() =>
                    {
                        Template_ComboBox.SelectedIndex = 0;  // Switch to Custom Report
                        if (ReportConfig != null)
                        {
                            ReportConfig.Title = ReportTemplates.TemplateNames.Custom;
                        }
                    });
                }

                // Update the report configuration with current selections
                if (ReportConfig != null)
                {
                    // Update the selected chart types in filters
                    ReportConfig.Filters.SelectedChartTypes = GetSelectedChartTypes();

                    UpdateElementsFromChartSelection();
                }

                NotifyParentValidationChanged();
            }
        }
        private void Template_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                string templateName = Template_ComboBox.SelectedItem?.ToString();

                if (Template_ComboBox.SelectedIndex == 0)
                {
                    // Custom Report selected - don't apply any template
                    if (ReportConfig != null)
                    {
                        ReportConfig.Title = ReportTemplates.TemplateNames.Custom;
                    }
                }
                else
                {
                    ApplyTemplate(templateName);
                }
            }
        }
        private void DateRange_Changed(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                SwitchToCustomTemplate();
                ValidateDateRange();
                NotifyParentValidationChanged();
            }
        }
        private void FilterChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                SwitchToCustomTemplate();
                NotifyParentValidationChanged();
            }
        }
        private void ReportTitle_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                NotifyParentValidationChanged();
            }
        }
        private void ValidateDateRange()
        {
            if (StartDate_DateTimePicker.Value >= EndDate_DateTimePicker.Value)
            {
                // Visual indication of invalid date range
                StartDate_DateTimePicker.BorderColor = Color.Red;
                EndDate_DateTimePicker.BorderColor = Color.Red;
            }
            else
            {
                // Reset to normal colors
                StartDate_DateTimePicker.BorderColor = CustomColors.ControlBorder;
                EndDate_DateTimePicker.BorderColor = CustomColors.ControlBorder;
            }
        }
        private void IncludeLosses_Label_Click(object sender, EventArgs e)
        {
            IncludeLosses_CheckBox.Checked = !IncludeLosses_CheckBox.Checked;
        }
        private void IncludeReturns_Label_Click(object sender, EventArgs e)
        {
            IncludeReturns_CheckBox.Checked = !IncludeReturns_CheckBox.Checked;
        }

        // Event handler helper methods
        private void ApplyTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName)) { return; }

            _isUpdating = true;
            try
            {
                ReportConfiguration template = ReportTemplates.CreateFromTemplate(templateName);

                // Apply the template configuration to the parent's report config
                if (ReportConfig != null && ParentReportForm != null)
                {
                    // Clear existing elements
                    ReportConfig.Elements.Clear();

                    // Copy all template elements
                    foreach (BaseElement element in template.Elements)
                    {
                        ReportConfig.AddElement(element.Clone());
                    }

                    // Copy page settings
                    ReportConfig.PageSize = template.PageSize;
                    ReportConfig.PageOrientation = template.PageOrientation;
                    ReportConfig.ShowHeader = template.ShowHeader;
                    ReportConfig.ShowFooter = template.ShowFooter;
                    ReportConfig.ShowPageNumbers = template.ShowPageNumbers;
                    ReportConfig.BackgroundColor = template.BackgroundColor;
                    ReportConfig.PageMargins = template.PageMargins;

                    // Copy filters from template
                    ReportConfig.Filters.SelectedChartTypes = [.. template.Filters.SelectedChartTypes];
                    ReportConfig.Filters.TransactionType = template.Filters.TransactionType;
                    ReportConfig.Filters.StartDate = template.Filters.StartDate;
                    ReportConfig.Filters.EndDate = template.Filters.EndDate;
                    ReportConfig.Filters.IncludeReturns = template.Filters.IncludeReturns;
                    ReportConfig.Filters.IncludeLosses = template.Filters.IncludeLosses;

                    // Copy template metadata
                    ReportConfig.Title = template.Title;
                    ReportConfig.Description = template.Description;
                }

                // Apply the template settings to the form controls
                PerformUpdate(() =>
                {
                    // Update chart selection
                    for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                    {
                        MainMenu_Form.ChartDataType chartType = GetChartTypeFromIndex(i);
                        bool isSelected = template.Filters.SelectedChartTypes.Contains(chartType);
                        ChartSelection_CheckedListBox.SetItemChecked(i, isSelected);
                    }

                    // Update report title
                    ReportTitle_TextBox.Text = template.Title;

                    // Update transaction type
                    TransactionType_ComboBox.SelectedIndex = (int)template.Filters.TransactionType;

                    // Update date range
                    StartDate_DateTimePicker.Value = template.Filters.StartDate ?? DateTime.Now.AddMonths(-1);

                    EndDate_DateTimePicker.Value = template.Filters.EndDate ?? DateTime.Now;

                    // Update includes
                    IncludeReturns_CheckBox.Checked = template.Filters.IncludeReturns;
                    IncludeLosses_CheckBox.Checked = template.Filters.IncludeLosses;
                });

                // Force the layout designer to refresh
                ParentReportForm?.OnChildFormValidationChanged();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Updates the ReportConfig.Elements collection based on current chart selections.
        /// </summary>
        private void UpdateElementsFromChartSelection()
        {
            if (ReportConfig == null) { return; }

            List<MainMenu_Form.ChartDataType> selectedCharts = GetSelectedChartTypes();
            ReportConfig.Elements.Clear();

            const int chartWidth = 350;
            const int chartHeight = 250;
            const int spacing = 20;
            const int startX = 50;
            const int startY = 50;
            const int columns = 2;

            int row = 0, col = 0;

            foreach (MainMenu_Form.ChartDataType chartType in selectedCharts)
            {
                int x = startX + (col * (chartWidth + spacing));
                int y = startY + (row * (chartHeight + spacing));

                ChartElement newChartElement = new()
                {
                    ChartType = chartType,
                    Bounds = new Rectangle(x, y, chartWidth, chartHeight)
                };
                ReportConfig.AddElement(newChartElement);

                col++;
                if (col >= columns)
                {
                    col = 0;
                    row++;
                }
            }
        }

        // Form implementation methods
        public bool IsValidForNextStep()
        {
            bool hasChartsSelected = ChartSelection_CheckedListBox.CheckedItems.Count > 0;
            bool hasValidDateRange = StartDate_DateTimePicker.Value < EndDate_DateTimePicker.Value;
            bool hasTitle = !string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text);

            return hasChartsSelected && hasValidDateRange && hasTitle;
        }
        public bool ValidateStep()
        {
            if (ChartSelection_CheckedListBox.CheckedItems.Count == 0)
            {
                CustomMessageBox.Show(
                    "Please select at least one chart to include in your report.",
                    "No Charts Selected",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            // Validate date range
            if (StartDate_DateTimePicker.Value >= EndDate_DateTimePicker.Value)
            {
                CustomMessageBox.Show(
                    "Invalid Date Range",
                    "Start date must be before end date.",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            // Validate report title
            if (string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text))
            {
                CustomMessageBox.Show(
                    "Missing Report Title",
                    "Please enter a report title.",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                ReportTitle_TextBox.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when the form becomes inactive (user navigates away from this step).
        /// </summary>
        public void OnStepDeactivated()
        {
            if (ReportConfig == null) { return; }

            // Update filters
            ReportConfig.Filters.StartDate = StartDate_DateTimePicker.Value;
            ReportConfig.Filters.EndDate = EndDate_DateTimePicker.Value;
            ReportConfig.Filters.TransactionType = GetSelectedTransactionType();
            ReportConfig.Filters.SelectedChartTypes = GetSelectedChartTypes();
            ReportConfig.Filters.IncludeReturns = IncludeReturns_CheckBox.Checked;
            ReportConfig.Filters.IncludeLosses = IncludeLosses_CheckBox.Checked;

            // Update template
            if (Template_ComboBox.SelectedIndex > 0)
            {
                ReportConfig.Title = Template_ComboBox.SelectedItem?.ToString();
            }

            // Update title if provided
            if (!string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text))
            {
                ReportConfig.Title = ReportTitle_TextBox.Text;
            }
        }

        // Helper Methods for Base Functionality
        /// <summary>
        /// Notifies the parent form that validation state has changed
        /// </summary>
        private void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }

        /// <summary>
        /// Safely updates UI controls without triggering events.
        /// </summary>
        private void PerformUpdate(Action updateAction)
        {
            if (updateAction == null) { return; }

            _isUpdating = true;
            try
            {
                updateAction();
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}