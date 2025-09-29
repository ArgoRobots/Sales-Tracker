using Sales_Tracker.Charts;
using Sales_Tracker.Language;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// First step in report generation - allows users to select charts, transactions, and apply filters.
    /// </summary>
    public partial class ReportDataSelection_Form : Form
    {
        // Properties
        /// <summary>
        /// Gets the parent report generator form.
        /// </summary>
        public ReportGenerator_Form ParentReportForm { get; private set; }

        /// <summary>
        /// Gets the current report configuration.
        /// </summary>
        protected ReportConfiguration? ReportConfig => ParentReportForm?.CurrentReportConfiguration;

        /// <summary>
        /// Indicates if the form is currently being loaded/updated programmatically.
        /// </summary>
        protected bool IsUpdating { get; private set; }

        // Init.
        public ReportDataSelection_Form(ReportGenerator_Form parentForm)
        {
            ParentReportForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            InitializeComponent();
            InitializeChildForm();
        }
        protected virtual void InitializeChildForm()
        {
            SetupChartSelection();
            SetupFilterControls();
            SetupTemplates();
            LoadDefaultValues();
        }
        private void SetupChartSelection()
        {
            // Populate chart selection with all available chart types
            ChartSelection_CheckedListBox.Items.Clear();

            foreach (ChartDataType chartType in Enum.GetValues<ChartDataType>())
            {
                string displayName = TranslatedChartTitles.GetChartDisplayName(chartType);
                ChartSelection_CheckedListBox.Items.Add(displayName, false);
            }
        }
        private void SetupFilterControls()
        {
            // Setup date range
            StartDate_DateTimePicker.Value = DateTime.Now.AddMonths(-1);
            EndDate_DateTimePicker.Value = DateTime.Now;

            // Setup transaction type
            TransactionType_ComboBox.Items.Clear();
            TransactionType_ComboBox.Items.Add(LanguageManager.TranslateString("Sales"));
            TransactionType_ComboBox.Items.Add(LanguageManager.TranslateString("Purchases"));
            TransactionType_ComboBox.Items.Add(LanguageManager.TranslateString("Both"));
            TransactionType_ComboBox.SelectedIndex = 2;  // Both by default

            // Setup includes
            IncludeReturns_CheckBox.Checked = true;
            IncludeLosses_CheckBox.Checked = true;
        }
        private void SetupTemplates()
        {
            // Add predefined templates
            Template_ComboBox.Items.Clear();
            Template_ComboBox.Items.Add(LanguageManager.TranslateString("Custom Report"));
            Template_ComboBox.Items.Add(LanguageManager.TranslateString("Monthly Sales Report"));
            Template_ComboBox.Items.Add(LanguageManager.TranslateString("Financial Overview"));
            Template_ComboBox.Items.Add(LanguageManager.TranslateString("Performance Analysis"));
            Template_ComboBox.SelectedIndex = 0;  // Custom by default
        }
        private void LoadDefaultValues()
        {
            // Select some commonly used charts by default
            ChartDataType[] defaultCharts =
            [
                ChartDataType.TotalSales,
                ChartDataType.DistributionOfSales,
                ChartDataType.TotalExpensesVsSales
            ];

            for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
            {
                ChartDataType chartType = GetChartTypeFromIndex(i);
                if (defaultCharts.Contains(chartType))
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, true);
                }
            }
        }

        // Helper methods
        private static ChartDataType GetChartTypeFromIndex(int index)
        {
            ChartDataType[] chartTypes = Enum.GetValues<ChartDataType>();
            return index >= 0 && index < chartTypes.Length ? chartTypes[index] : ChartDataType.TotalSales;
        }
        private List<ChartDataType> GetSelectedChartTypes()
        {
            List<ChartDataType> selectedCharts = [];

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

        // Form event handlers
        private void ReportDataSelection_Form_Shown(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                OnStepActivated();
            }
        }
        private void ReportDataSelection_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                if (Visible)
                {
                    OnStepActivated();
                }
                else
                {
                    OnStepDeactivated();
                }
            }
        }

        // Event handlers
        private void SelectAll_Button_Click(object sender, EventArgs e)
        {
            if (IsUpdating) { return; }

            PerformUpdate(() =>
            {
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, true);
                }
                UpdateSelectedChartsDisplay();
                NotifyParentValidationChanged();
            });
        }
        private void SelectNone_Button_Click(object sender, EventArgs e)
        {
            if (IsUpdating) { return; }

            PerformUpdate(() =>
            {
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, false);
                }

                NotifyParentValidationChanged();
            });
        }
        private void ChartSelection_CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!IsUpdating)
            {
                // Check if the window handle has been created before using BeginInvoke
                if (IsHandleCreated && Visible)
                {
                    // Use BeginInvoke to ensure the checked state is updated before validation
                    BeginInvoke(new Action(() =>
                    {
                        NotifyParentValidationChanged();
                    }));
                }
            }
        }
        private void Template_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsUpdating && Template_ComboBox.SelectedIndex > 0)
            {
                ApplyTemplate(Template_ComboBox.SelectedItem?.ToString());
            }
        }
        private void DateRange_Changed(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                ValidateDateRange();
                NotifyParentValidationChanged();
            }
        }
        private void FilterChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                NotifyParentValidationChanged();
            }
        }
        private void ReportTitle_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
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
                StartDate_DateTimePicker.BorderColor = Color.FromArgb(213, 218, 223);
                EndDate_DateTimePicker.BorderColor = Color.FromArgb(213, 218, 223);
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

        // Template application
        private void ApplyTemplate(string templateName)
        {
            // Apply predefined template settings
            switch (templateName)
            {
                case "Monthly Sales Report":
                    ApplyMonthlySalesTemplate();
                    break;
                case "Financial Overview":
                    ApplyFinancialOverviewTemplate();
                    break;
                case "Performance Analysis":
                    ApplyPerformanceAnalysisTemplate();
                    break;
            }
        }
        private void ApplyMonthlySalesTemplate()
        {
            PerformUpdate(() =>
            {
                ChartDataType[] templateCharts =
                [
                    ChartDataType.TotalSales,
                    ChartDataType.DistributionOfSales,
                    ChartDataType.GrowthRates,
                    ChartDataType.AverageOrderValue
                ];

                ApplyChartTemplate(templateCharts);
                ReportTitle_TextBox.Text = "Monthly Sales Report";
                TransactionType_ComboBox.SelectedIndex = 0;  // Sales only

                // Set date range to last month
                DateTime now = DateTime.Now;
                StartDate_DateTimePicker.Value = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                EndDate_DateTimePicker.Value = new DateTime(now.Year, now.Month, 1).AddDays(-1);
            });
        }
        private void ApplyFinancialOverviewTemplate()
        {
            PerformUpdate(() =>
            {
                ChartDataType[] templateCharts =
                [
                    ChartDataType.TotalSales,
                    ChartDataType.TotalPurchases,
                    ChartDataType.TotalExpensesVsSales,
                    ChartDataType.TotalProfits
                ];

                ApplyChartTemplate(templateCharts);
                ReportTitle_TextBox.Text = "Financial Overview";
                TransactionType_ComboBox.SelectedIndex = 2;  // Both

                // Set date range to last quarter
                StartDate_DateTimePicker.Value = DateTime.Now.AddMonths(-3);
                EndDate_DateTimePicker.Value = DateTime.Now;
            });
        }
        private void ApplyPerformanceAnalysisTemplate()
        {
            PerformUpdate(() =>
            {
                ChartDataType[] templateCharts =
                [
                    ChartDataType.GrowthRates,
                    ChartDataType.AverageOrderValue,
                    ChartDataType.TotalTransactions,
                    ChartDataType.ReturnsOverTime
                ];

                ApplyChartTemplate(templateCharts);
                ReportTitle_TextBox.Text = "Performance Analysis";
                TransactionType_ComboBox.SelectedIndex = 2;  // Both

                // Set date range to last 6 months
                StartDate_DateTimePicker.Value = DateTime.Now.AddMonths(-6);
                EndDate_DateTimePicker.Value = DateTime.Now;
            });
        }
        private void ApplyChartTemplate(ChartDataType[] templateCharts)
        {
            // Uncheck all first
            for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
            {
                ChartSelection_CheckedListBox.SetItemChecked(i, false);
            }

            // Check template charts
            for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
            {
                ChartDataType chartType = GetChartTypeFromIndex(i);
                if (templateCharts.Contains(chartType))
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, true);
                }
            }
        }

        // Form implementation methods
        public virtual bool IsValidForNextStep()
        {
            // Must have at least one chart selected
            bool hasChartsSelected = ChartSelection_CheckedListBox.CheckedItems.Count > 0;

            // Must have valid date range
            bool hasValidDateRange = StartDate_DateTimePicker.Value < EndDate_DateTimePicker.Value;

            // Must have report title
            bool hasTitle = !string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text);

            return hasChartsSelected && hasValidDateRange && hasTitle;
        }
        public virtual bool ValidateStep()
        {
            if (ChartSelection_CheckedListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.TranslateString("Please select at least one chart to include in your report."),
                    LanguageManager.TranslateString("No Charts Selected"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            // Validate date range
            if (StartDate_DateTimePicker.Value >= EndDate_DateTimePicker.Value)
            {
                MessageBox.Show(
                    LanguageManager.TranslateString("Start date must be before end date."),
                    LanguageManager.TranslateString("Invalid Date Range"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            // Validate report title
            if (string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text))
            {
                MessageBox.Show(
                    LanguageManager.TranslateString("Please enter a report title."),
                    LanguageManager.TranslateString("Missing Report Title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                ReportTitle_TextBox.Focus();
                return false;
            }

            return true;
        }
        public virtual void UpdateReportConfiguration()
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
                ReportConfig.TemplateName = Template_ComboBox.SelectedItem?.ToString();
            }

            // Update title if provided
            if (!string.IsNullOrWhiteSpace(ReportTitle_TextBox.Text))
            {
                ReportConfig.Title = ReportTitle_TextBox.Text;
            }

            ReportConfig.LastModified = DateTime.Now;
        }
        public virtual void LoadFromReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            PerformUpdate(() =>
            {
                // Load filters
                if (ReportConfig.Filters.StartDate.HasValue)
                {
                    StartDate_DateTimePicker.Value = ReportConfig.Filters.StartDate.Value;
                }

                if (ReportConfig.Filters.EndDate.HasValue)
                {
                    EndDate_DateTimePicker.Value = ReportConfig.Filters.EndDate.Value;
                }

                TransactionType_ComboBox.SelectedIndex = (int)ReportConfig.Filters.TransactionType;
                IncludeReturns_CheckBox.Checked = ReportConfig.Filters.IncludeReturns;
                IncludeLosses_CheckBox.Checked = ReportConfig.Filters.IncludeLosses;

                // Load selected charts
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartDataType chartType = GetChartTypeFromIndex(i);
                    bool isSelected = ReportConfig.Filters.SelectedChartTypes.Contains(chartType);
                    ChartSelection_CheckedListBox.SetItemChecked(i, isSelected);
                }

                // Load title
                ReportTitle_TextBox.Text = ReportConfig.Title;

                // Load template
                if (!string.IsNullOrEmpty(ReportConfig.TemplateName))
                {
                    int templateIndex = Template_ComboBox.Items.IndexOf(ReportConfig.TemplateName);
                    if (templateIndex >= 0)
                        Template_ComboBox.SelectedIndex = templateIndex;
                }
            });
        }
        public virtual void ResetForm()
        {
            PerformUpdate(() =>
            {
                // Uncheck all charts
                for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, false);
                }

                // Reset to defaults
                LoadDefaultValues();
                SetupFilterControls();

                ReportTitle_TextBox.Text = "Sales Report";
                Template_ComboBox.SelectedIndex = 0;
            });
        }

        /// <summary>
        /// Called when the form becomes active (user navigates to this step).
        /// </summary>
        public virtual void OnStepActivated()
        {
            LoadFromReportConfiguration();
            NotifyParentValidationChanged();
        }

        /// <summary>
        /// Called when the form becomes inactive (user navigates away from this step).
        /// </summary>
        public virtual void OnStepDeactivated()
        {
            UpdateReportConfiguration();
        }

        // Helper Methods for Base Functionality

        /// <summary>
        /// Notifies the parent form that validation state has changed
        /// </summary>
        protected void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }

        /// <summary>
        /// Safely updates UI controls without triggering events.
        /// </summary>
        /// <param name="updateAction">Action to perform during update</param>
        protected void PerformUpdate(Action updateAction)
        {
            if (updateAction == null) { return; }

            IsUpdating = true;
            try
            {
                updateAction();
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}