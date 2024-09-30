using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class DateRange_Form : Form
    {
        // Init.
        public DateRange_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);
            InitializeDatePickers();
            Theme.SetThemeForForm(this);
            LanguageManager.UpdateLanguageForForm(this);
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
            MainMenu_Form.Instance.fromDate = From_DateTimePicker.Value;
            MainMenu_Form.Instance.toDate = To_DateTimePicker.Value;

            // Check if the current values of the DateTimePickers match the default values
            bool isDefaultRange = From_DateTimePicker.Value.Date == GetOldestDate().Date &&
                                  To_DateTimePicker.Value.Date == DateTime.Now.Date;

            MainMenu_Form.Instance.Filter_ComboBox.Enabled = isDefaultRange;
            MainMenu_Form.Instance.SortDataGridView();
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
            From_DateTimePicker.Value = GetOldestDate();
            To_DateTimePicker.Value = DateTime.Now;
        }
        private static DateTime GetOldestDate()
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
    }
}