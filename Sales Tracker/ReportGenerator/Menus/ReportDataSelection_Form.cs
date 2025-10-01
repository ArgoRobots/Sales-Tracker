using Sales_Tracker.Charts;
using Sales_Tracker.Language;
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
            LoadDefaultValues();
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
            // Populate chart selection with all available chart types
            ChartSelection_CheckedListBox.Items.Clear();

            foreach (MainMenu_Form.ChartDataType chartType in Enum.GetValues<MainMenu_Form.ChartDataType>())
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
            Template_ComboBox.Items.Clear();

            // Add templates from ReportTemplates class
            List<string> templates = ReportTemplates.GetAvailableTemplates();
            foreach (string templateName in templates)
            {
                Template_ComboBox.Items.Add(LanguageManager.TranslateString(templateName));
            }

            // Select "Custom Report" by default
            Template_ComboBox.SelectedIndex = 0;
        }
        private void LoadDefaultValues()
        {
            // Select some commonly used charts by default
            MainMenu_Form.ChartDataType[] defaultCharts =
            [
                MainMenu_Form.ChartDataType.TotalSales,
                MainMenu_Form.ChartDataType.DistributionOfSales,
                MainMenu_Form.ChartDataType.TotalExpensesVsSales
            ];

            for (int i = 0; i < ChartSelection_CheckedListBox.Items.Count; i++)
            {
                MainMenu_Form.ChartDataType chartType = GetChartTypeFromIndex(i);
                if (defaultCharts.Contains(chartType))
                {
                    ChartSelection_CheckedListBox.SetItemChecked(i, true);
                }
            }
        }
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialLeftPanelWidth = Left_Panel.Width;
            _initialRightPanelWidth = Right_Panel.Width;
        }

        // Helper methods
        private static MainMenu_Form.ChartDataType GetChartTypeFromIndex(int index)
        {
            MainMenu_Form.ChartDataType[] chartTypes = Enum.GetValues<MainMenu_Form.ChartDataType>();
            return index >= 0 && index < chartTypes.Length ? chartTypes[index] : MainMenu_Form.ChartDataType.TotalSales;
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

        // Form event handlers
        private void ReportDataSelection_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
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

                NotifyParentValidationChanged();
            });
        }
        private void ChartSelection_CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_isUpdating)
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
            if (!_isUpdating && Template_ComboBox.SelectedIndex > 0)
            {
                ApplyTemplate(Template_ComboBox.SelectedItem?.ToString());
            }
        }
        private void DateRange_Changed(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                ValidateDateRange();
                NotifyParentValidationChanged();
            }
        }
        private void FilterChanged(object sender, EventArgs e)
        {
            if (!_isUpdating)
            {
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

        // Template application
        private void ApplyTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName)) { return; }

            // Create a new report configuration from the template
            ReportConfiguration template = ReportTemplates.CreateFromTemplate(templateName);

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
                if (template.Filters.StartDate.HasValue)
                    StartDate_DateTimePicker.Value = template.Filters.StartDate.Value;

                if (template.Filters.EndDate.HasValue)
                    EndDate_DateTimePicker.Value = template.Filters.EndDate.Value;

                // Update includes
                IncludeReturns_CheckBox.Checked = template.Filters.IncludeReturns;
                IncludeLosses_CheckBox.Checked = template.Filters.IncludeLosses;
            });
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
        public void UpdateReportConfiguration()
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
        }
        public void LoadFromReportConfiguration()
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
                    MainMenu_Form.ChartDataType chartType = GetChartTypeFromIndex(i);
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
                    {
                        Template_ComboBox.SelectedIndex = templateIndex;
                    }
                }
            });
        }

        /// <summary>
        /// Called when the form becomes active (user navigates to this step).
        /// </summary>
        public void OnStepActivated()
        {
            LoadFromReportConfiguration();

            // If template name is set in the configuration, select it in the combobox
            if (ReportConfig != null && !string.IsNullOrEmpty(ReportConfig.TemplateName))
            {
                int index = Template_ComboBox.Items.IndexOf(
                    LanguageManager.TranslateString(ReportConfig.TemplateName));
                if (index >= 0)
                {
                    Template_ComboBox.SelectedIndex = index;
                }
            }

            NotifyParentValidationChanged();
        }

        /// <summary>
        /// Called when the form becomes inactive (user navigates away from this step).
        /// </summary>
        public void OnStepDeactivated()
        {
            UpdateReportConfiguration();
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
        /// <param name="updateAction">Action to perform during update</param>
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