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

        // Getter
        public static Guna2Panel RightClickGunaChart_Panel => _rightClickGunaChart_Panel;

        // Methods
        public static void ConstructRightClickGunaChartMenu()
        {
            Guna2Panel panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth - 50, 2 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickGunaChart_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
            int newBtnWidth = CustomControls.PanelBtnWidth - 50;

            Guna2Button button = CustomControls.ConstructBtnForMenu("Reset zoom", newBtnWidth, false, flowPanel);
            button.Click += ResetZoom;

            button = CustomControls.ConstructBtnForMenu("Save image", newBtnWidth, false, flowPanel);
            button.Click += SaveImage;

            _rightClickGunaChart_Panel = panel;
        }
        private static void ResetZoom(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
            //GunaChart chart = (GunaChart)rightClickGunaChart_Panel.Tag;
            // Wait until the next update for Guna Charts to implement this
            //chart.ResetZoom();
        }
        private static void SaveImage(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
            GunaChart chart = (GunaChart)_rightClickGunaChart_Panel.Tag;
            string filepath;

            // Select folder
            using (Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string date = Tools.FormatDate(DateTime.Now);
                    filepath = Path.Combine(
                        dialog.SelectedPath,
                        $"{chart.Title.Text}-{date}{ArgoFiles.PngFileExtension}"
                    );
                }
                else
                {
                    return;
                }
            }

            MessageBox.Show(filepath);
            // Wait until the next update for Guna Charts to implement this
            //chart.Export();
        }
        public static void ShowMenu(GunaChart chart, Point mousePosition)
        {
            Form form = chart.FindForm();
            Point localMousePosition = form.PointToClient(mousePosition);
            int formWidth = form.ClientSize.Width;
            int formHeight = form.ClientSize.Height;
            byte offset = ReadOnlyVariables.OffsetRightClickPanel;
            byte padding = ReadOnlyVariables.PaddingRightClickPanel;

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
            _rightClickGunaChart_Panel.Tag = chart;
        }
    }
}