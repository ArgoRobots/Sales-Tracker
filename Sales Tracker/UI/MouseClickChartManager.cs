using Guna.Charts.WinForms;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages mouse click detection for GunaChart controls and invokes specific actions
    /// based on left-click or right-click events.
    /// </summary>
    public static class MouseClickChartManager
    {
        private static GunaChart[] charts;
        private static Action<GunaChart> onLeftClick;
        private static Action<GunaChart, Point> onRightClick;

        /// <summary>
        /// Initializes the click manager for specified GunaChart controls and assigns
        /// actions to be called on left and right mouse clicks.
        /// </summary>
        public static void Initialize(GunaChart[] charts, Action<GunaChart> onLeftClick, Action<GunaChart, Point> onRightClick)
        {
            MouseClickChartManager.charts = charts;
            MouseClickChartManager.onLeftClick = onLeftClick;
            MouseClickChartManager.onRightClick = onRightClick;

            Application.AddMessageFilter(new CustomMessageFilter());
        }

        /// <summary>
        /// Custom IMessageFilter implementation that detects mouse clicks and determines
        /// whether a left or right click occurred on a GunaChart control.
        /// </summary>
        public class CustomMessageFilter : IMessageFilter
        {
            /// <summary>
            /// Filters Windows messages before they are dispatched to controls.
            /// Detects left and right mouse button down events and invokes the appropriate action.
            /// </summary>
            public bool PreFilterMessage(ref Message m)
            {
                // Detect left or right mouse button down events
                if (m.Msg == 0x0201 || m.Msg == 0x0204)  // 0x0201 is WM_LBUTTONDOWN, 0x0204 is WM_RBUTTONDOWN
                {
                    bool isRightClick = m.Msg == 0x0204;
                    Point mousePosition = Control.MousePosition;

                    // Create a list of controls
                    List<Control> controlsList =
                    [
                        CustomControls.FileMenu,
                        CustomControls.HelpMenu
                    ];
                    Control mainPanel = DateRange_Form.Instance?.Main_Panel;
                    if (mainPanel != null && MainMenu_Form.Instance.Controls.Contains(mainPanel))
                    {
                        controlsList.Add(mainPanel);
                    }

                    // Ignore the click if it happened within a control
                    foreach (Control control in controlsList)
                    {
                        if (control == null || control.IsDisposed)
                        {
                            continue;
                        }

                        Point localMousePosition = control.PointToClient(mousePosition);

                        // Check if the mouse click was within the bounds of the control
                        if (control.ClientRectangle.Contains(localMousePosition))
                        {
                            return false;
                        }
                    }

                    // Check if the click happened on any of the charts
                    foreach (GunaChart chart in charts)
                    {
                        if (chart.Parent == null)
                        {
                            // Skip this chart if it has no parent (if the statistics charts are not shown)
                            continue;
                        }

                        Point localMousePosition = chart.Parent.PointToClient(mousePosition);

                        // Check if the click is within the bounds of the chart
                        if (chart.Bounds.Contains(localMousePosition))
                        {
                            // Trigger the left click action with chart
                            onLeftClick?.Invoke(chart);

                            if (isRightClick)
                            {
                                // Trigger the right click action with chart and mouse position
                                onRightClick?.Invoke(chart, mousePosition);
                            }
                            break;
                        }
                    }
                }
                return false;
            }
        }
    }
}