using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    public static class RightClickGunaChartMenu
    {
        // Properties
        private static Guna2Panel _rightClickGunaChart_Panel;
        private static readonly string exportBtn_text = "ExportBtn";

        // Getter
        public static Guna2Panel RightClickGunaChart_Panel => _rightClickGunaChart_Panel;

        // Right click menu methods
        public static void ConstructRightClickGunaChartMenu()
        {
            Guna2Panel panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth - 50, 4 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickGunaChart_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
            int newBtnWidth = CustomControls.PanelBtnWidth - 50;

            Guna2Button button = CustomControls.ConstructBtnForMenu("Reset zoom", newBtnWidth, true, flowPanel);
            button.Click += ResetZoom;

            button = CustomControls.ConstructBtnForMenu("Save image", newBtnWidth, true, flowPanel);
            button.Click += SaveImage;

            // Export buttons are only visible when chart has data
            button = CustomControls.ConstructBtnForMenu("Export to Microsoft Excel", newBtnWidth, true, flowPanel);
            button.Click += ExportToMicrosoftExcel;
            button.Tag = exportBtn_text;

            button = CustomControls.ConstructBtnForMenu("Export to Google Sheets", newBtnWidth, true, flowPanel);
            button.Click += ExportToGoogleSheets;
            button.Tag = exportBtn_text;

            _rightClickGunaChart_Panel = panel;
        }
        private static void ResetZoom(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            chart.ResetZoom();
        }
        private static void SaveImage(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            dialog.FileName = $"{Directories.CompanyName} {chart.Title.Text.ToLower()} {date}";
            dialog.DefaultExt = ArgoFiles.PngFileExtension;
            dialog.Filter = $"PNG Image|*{ArgoFiles.PngFileExtension}";
            dialog.Title = "Save Chart Image";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                chart.Export(dialog.FileName);
            }
        }
        private static void ExportToMicrosoftExcel(object sender, EventArgs e)
        {
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            string directory = "";

            using SaveFileDialog dialog = new();
            string date = Tools.FormatDate(DateTime.Now);
            string name = chart.Title.Text.Split(':')[0];
            dialog.FileName = $"{Directories.CompanyName} {name} {date}";
            dialog.DefaultExt = ArgoFiles.XlsxFileExtension;
            dialog.Filter = $"XLSX spreadsheet|*{ArgoFiles.XlsxFileExtension}";
            dialog.Title = "Export Chart to XLSX";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                directory = dialog.FileName;
            }

            Guna2DataGridView activeDataGridView = MainMenu_Form.Instance.Sale_DataGridView.Visible
                ? MainMenu_Form.Instance.Sale_DataGridView
                : MainMenu_Form.Instance.Purchase_DataGridView;

            bool isLine = MainMenu_Form.Instance.LineGraph_ToggleSwitch.Checked;

            switch (chart.Tag)
            {
                case MainMenu_Form.ChartDataType.TotalRevenue:
                    LoadChart.LoadTotalsIntoChart(activeDataGridView, MainMenu_Form.Instance.Totals_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.DistributionOfRevenue:
                    LoadChart.LoadDistributionIntoChart(activeDataGridView, MainMenu_Form.Instance.Distribution_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalProfits:
                    LoadChart.LoadProfitsIntoChart(MainMenu_Form.Instance.Profits_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfOrigin:
                    LoadChart.LoadCountriesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CountriesOfOrigin_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CompaniesOfOrigin:
                    LoadChart.LoadCompaniesOfOriginForProductsIntoChart(MainMenu_Form.Instance.CompaniesOfOrigin_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.CountriesOfDestination:
                    LoadChart.LoadCountriesOfDestinationForProductsIntoChart(MainMenu_Form.Instance.CountriesOfDestination_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.Accountants:
                    LoadChart.LoadAccountantsIntoChart(MainMenu_Form.Instance.Accountants_Chart, PieChartGrouping.Unlimited, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.TotalExpensesVsSales:
                    LoadChart.LoadSalesVsExpensesChart(MainMenu_Form.Instance.SalesVsExpenses_Chart, isLine, true, directory);
                    break;

                case MainMenu_Form.ChartDataType.AverageOrderValue:
                    LoadChart.LoadAverageOrderValueChart(MainMenu_Form.Instance.AverageOrderValue_Chart, isLine, true, directory);
                    break;
            }
        }
        private static void ExportToGoogleSheets(object sender, EventArgs e)
        {

        }

        // Other methods
        public static void ShowMenu(GunaChart chart, Point mousePosition)
        {
            Form form = chart.FindForm();
            _rightClickGunaChart_Panel.Tag = chart;
            Point localMousePosition = form.PointToClient(mousePosition);
            int formWidth = form.ClientSize.Width;
            int formHeight = form.ClientSize.Height;
            byte offset = ReadOnlyVariables.OffsetRightClickPanel;
            byte padding = ReadOnlyVariables.PaddingRightClickPanel;

            // Show/hide export buttons based on chart data
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickGunaChart_Panel.Controls[0];

            foreach (Control control in flowPanel.Controls)
            {
                if (control is Guna2Button btn && btn.Tag?.ToString() == exportBtn_text)
                {
                    btn.Visible = chart.DatasetCount > 0;
                }
            }
            CustomControls.SetRightClickMenuHeight(_rightClickGunaChart_Panel);

            // Calculate the horizontal position
            bool tooFarRight = false;
            if (_rightClickGunaChart_Panel.Width + localMousePosition.X - offset + padding > formWidth)
            {
                _rightClickGunaChart_Panel.Left = formWidth - _rightClickGunaChart_Panel.Width - padding;
                tooFarRight = true;
            }
            else
            {
                _rightClickGunaChart_Panel.Left = localMousePosition.X - offset;
            }

            // Calculate the vertical position
            if (localMousePosition.Y + _rightClickGunaChart_Panel.Height + padding > formHeight)
            {
                _rightClickGunaChart_Panel.Top = formHeight - _rightClickGunaChart_Panel.Height - padding;
                if (!tooFarRight)
                {
                    _rightClickGunaChart_Panel.Left += offset;
                }
            }
            else
            {
                _rightClickGunaChart_Panel.Top = localMousePosition.Y;
            }

            form.Controls.Add(_rightClickGunaChart_Panel);
            _rightClickGunaChart_Panel.BringToFront();
        }
    }
}