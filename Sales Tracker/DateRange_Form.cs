using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class DateRange_Form : BaseForm
    {
        // Properties
        private static DateRange_Form _instance;
        private Dictionary<TimeSpan, Guna2CustomRadioButton> _timeSpanOptions;

        // Getters and setters
        public static DateRange_Form Instance => _instance;

        // Init.
        public DateRange_Form()
        {
            InitializeComponent();
            _instance = this;

            UpdateTheme();
            SetAccessibleDescriptions();
            InitializeTimeSpanOptions();
            LanguageManager.UpdateLanguageForControl(this);

            ResetControls();
            SetCustomRangeControls();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        public void ResetControls()
        {
            AllTime_RadioButton.Checked = true;
            From_DateTimePicker.Value = MainMenu_Form.Instance.SortFromDate ?? GetOldestDate();
            To_DateTimePicker.Value = MainMenu_Form.Instance.SortToDate ?? DateTime.Now;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            Bottom_Separator.FillColor = CustomColors.ControlBorder;
            Main_Panel.BorderColor = CustomColors.ControlPanelBorder;
            ThemeManager.MakeGButtonBluePrimary(Apply_Button);
            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
        }
        private void SetAccessibleDescriptions()
        {
            From_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            To_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
        }
        private void InitializeTimeSpanOptions()
        {
            _timeSpanOptions = new()
            {
                { TimeSpan.MaxValue, AllTime_RadioButton },
                { TimeSpan.FromDays(1), Last24Hours_RadioButton },
                { TimeSpan.FromDays(2), Last48Hours_RadioButton },
                { TimeSpan.FromDays(3), Last3Days_RadioButton },
                { TimeSpan.FromDays(5), Last5Days_RadioButton },
                { TimeSpan.FromDays(10), Last10Days_RadioButton },
                { TimeSpan.FromDays(30), Last30Days_RadioButton },
                { TimeSpan.FromDays(100), Last100Days_RadioButton },
                { TimeSpan.FromDays(365), LastYear_RadioButton },
                { TimeSpan.FromDays(365 * 2), Last2Years_RadioButton },
                { TimeSpan.FromDays(365 * 5), Last5Years_RadioButton }
            };
        }

        // Form event handlers
        private void DateRange_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            MainMenu_Form.Instance.CloseDateRangePanel();
            ResetControls();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            if (Custom_RadioButton.Checked)
            {
                // Validate date range
                if (From_DateTimePicker.Value > To_DateTimePicker.Value)
                {
                    HandleInvalidDateRange();
                    return;
                }

                MainMenu_Form.Instance.SortTimeSpan = null;
                MainMenu_Form.Instance.SortFromDate = From_DateTimePicker.Value;
                MainMenu_Form.Instance.SortToDate = To_DateTimePicker.Value;
            }
            else
            {
                MainMenu_Form.Instance.SortFromDate = null;
                MainMenu_Form.Instance.SortToDate = null;
                MainMenu_Form.Instance.SortTimeSpan = GetSelectedTimeSpan();
            }
            MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
            MainMenu_Form.Instance.CloseDateRangePanel();
        }
        private void Custom_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetCustomRangeControls();
        }

        // Label event handlers
        private void AllTime_Label_Click(object sender, EventArgs e)
        {
            AllTime_RadioButton.Checked = !AllTime_RadioButton.Checked;
        }
        private void Last24Hours_Label_Click(object sender, EventArgs e)
        {
            Last24Hours_RadioButton.Checked = !Last24Hours_RadioButton.Checked;
        }
        private void Last48Hours_Label_Click(object sender, EventArgs e)
        {
            Last48Hours_RadioButton.Checked = !Last48Hours_RadioButton.Checked;
        }
        private void Last3Days_Label_Click(object sender, EventArgs e)
        {
            Last3Days_RadioButton.Checked = !Last3Days_RadioButton.Checked;
        }
        private void Last5Days_Label_Click(object sender, EventArgs e)
        {
            Last5Days_RadioButton.Checked = !Last5Days_RadioButton.Checked;
        }
        private void Last10Days_Label_Click(object sender, EventArgs e)
        {
            Last10Days_RadioButton.Checked = !Last10Days_RadioButton.Checked;
        }
        private void Last30Days_Label_Click(object sender, EventArgs e)
        {
            Last30Days_RadioButton.Checked = !Last30Days_RadioButton.Checked;
        }
        private void Last100Days_Label_Click(object sender, EventArgs e)
        {
            Last100Days_RadioButton.Checked = !Last100Days_RadioButton.Checked;
        }
        private void LastYear_Label_Click(object sender, EventArgs e)
        {
            LastYear_RadioButton.Checked = !LastYear_RadioButton.Checked;
        }
        private void Last2Years_Label_Click(object sender, EventArgs e)
        {
            Last2Years_RadioButton.Checked = !Last2Years_RadioButton.Checked;
        }
        private void Last5Years_Label_Click(object sender, EventArgs e)
        {
            Last5Years_RadioButton.Checked = !Last5Years_RadioButton.Checked;
        }
        private void Custom_Label_Click(object sender, EventArgs e)
        {
            Custom_RadioButton.Checked = !Custom_RadioButton.Checked;
        }

        // DateTimePicker methods
        private static DateTime GetOldestDate()
        {
            DateTime oldestDate = DateTime.Now;  // Default to today if there are no rows

            // Check if there are rows in both DataGridViews
            if (MainMenu_Form.Instance.Sale_DataGridView.Rows.Count > 0
                || MainMenu_Form.Instance.Purchase_DataGridView.Rows.Count > 0)
            {
                oldestDate = new[]
                {
                    GetOldestDateFromDataGridView(MainMenu_Form.Instance.Sale_DataGridView),
                    GetOldestDateFromDataGridView(MainMenu_Form.Instance.Purchase_DataGridView)
                }.Min();
            }
            return oldestDate;
        }
        private static DateTime GetOldestDateFromDataGridView(DataGridView dataGridView)
        {
            DateTime oldestDate = DateTime.MaxValue;
            string dateColumn = MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Date];

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (DateTime.TryParse(row.Cells[dateColumn].Value.ToString(), out DateTime rowDate))
                {
                    if (rowDate < oldestDate)
                    {
                        oldestDate = rowDate;
                    }
                }
            }
            return oldestDate;
        }
        private void HandleInvalidDateRange()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Invalid Date Range",
                "The 'From' date cannot be later than the 'To' date.\nWould you like to swap the dates automatically?",
                CustomMessageBoxIcon.Question,
                CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                // Swap the dates
                (To_DateTimePicker.Value, From_DateTimePicker.Value) = (From_DateTimePicker.Value, To_DateTimePicker.Value);

                // Now apply with corrected dates
                MainMenu_Form.Instance.SortTimeSpan = null;
                MainMenu_Form.Instance.SortFromDate = From_DateTimePicker.Value;
                MainMenu_Form.Instance.SortToDate = To_DateTimePicker.Value;
                MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
                MainMenu_Form.Instance.CloseDateRangePanel();
            }
        }

        // Methods
        private TimeSpan? GetSelectedTimeSpan()
        {
            return _timeSpanOptions.FirstOrDefault(kvp => kvp.Value.Checked).Key;
        }
        private void SetCustomRangeControls()
        {
            foreach (Control control in GetCustomRangeControls())
            {
                if (Custom_RadioButton.Checked)
                {
                    Main_Panel.Controls.Add(control);
                }
                else
                {
                    Main_Panel.Controls.Remove(control);
                }
            }

            Control bottomControl = Custom_RadioButton.Checked ? To_DateTimePicker : Custom_RadioButton;
            Main_Panel.Height = bottomControl.Bottom + Main_Panel.Height - Bottom_Separator.Bottom + 50;
        }
        public List<Control> GetCustomRangeControls()
        {
            return [
                From_Label,
                From_DateTimePicker,
                To_Label,
                To_DateTimePicker
            ];
        }
    }
}