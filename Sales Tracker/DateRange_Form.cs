using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class DateRange_Form : Form
    {
        public DateRange_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowBlankLoadingPanel(this);
            InitializeDatePickers();
            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private void DateRange_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Reset_Button_Click(object sender, EventArgs e)
        {
            SetDatePickers();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            FilterDataGridViews();

            MainMenu_Form.Instance.fromDate = From_DateTimePicker.Value;
            MainMenu_Form.Instance.toDate = To_DateTimePicker.Value;

            MainMenu_Form.Instance.Filter_ComboBox.Enabled = false;
            MainMenu_Form.Instance.ShowShowingResultsForLabel();

            Close();
        }

        // Methods
        private void InitializeDatePickers()
        {
            if (MainMenu_Form.Instance.fromDate == DateTime.MinValue)
            {
                SetDatePickers();
            }
            else
            {
                From_DateTimePicker.Value = MainMenu_Form.Instance.fromDate;
                To_DateTimePicker.Value = MainMenu_Form.Instance.toDate;
            }
        }
        private void SetDatePickers()
        {
            SetFromDateTimePickerToOldestDate();
            To_DateTimePicker.Value = DateTime.Now;
        }
        private void SetFromDateTimePickerToOldestDate()
        {
            DateTime oldestDate = DateTime.Now;  // Default to today if no rows are found

            // Check if there are rows in both DataGridViews
            if (MainMenu_Form.Instance.Sales_DataGridView.Rows.Count > 0 || MainMenu_Form.Instance.Purchases_DataGridView.Rows.Count > 0)
            {
                oldestDate = new[]
                {
                    GetOldestDateFromDataGridView(MainMenu_Form.Instance.Sales_DataGridView),
                    GetOldestDateFromDataGridView(MainMenu_Form.Instance.Purchases_DataGridView)
                }.Min();
            }

            From_DateTimePicker.Value = oldestDate;
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
        private void FilterDataGridViews()
        {
            FilterDataGridViewByDateRange(MainMenu_Form.Instance.Sales_DataGridView);
            FilterDataGridViewByDateRange(MainMenu_Form.Instance.Purchases_DataGridView);
        }
        private void FilterDataGridViewByDateRange(DataGridView dataGridView)
        {
            DateTime fromDate = From_DateTimePicker.Value;
            DateTime toDate = To_DateTimePicker.Value;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DateTime rowDate = DateTime.Parse(row.Cells[MainMenu_Form.Instance.PurchaseColumnHeaders[MainMenu_Form.Column.Date]].Value.ToString());
                bool isVisible = rowDate >= fromDate && rowDate <= toDate;
                row.Visible = isVisible;
            }
        }
    }
}