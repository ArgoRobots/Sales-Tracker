using Guna.Charts.WinForms;
using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Manages mouse click detection for GunaChart controls and invokes specific actions.
    /// </summary>
    public static class MouseClickChartManager
    {
        private static readonly HashSet<GunaChart> registeredCharts = [];
        private static Action<GunaChart> onLeftClick;
        private static Action<GunaChart, Point> onRightClick;
        private static CustomMessageFilter messageFilter;

        /// <summary>
        /// Initializes the click manager for specified GunaChart controls and assigns actions to be called on left and right mouse clicks.
        /// </summary>
        public static void InitCharts(GunaChart[] charts)
        {
            static void leftClickAction(GunaChart statisticsControls) => CustomControls.CloseAllPanels(null, null);
            Initialize(charts, leftClickAction, RightClickGunaChartMenu.ShowMenu);
        }
        private static void Initialize(GunaChart[] charts, Action<GunaChart> onLeftClick, Action<GunaChart, Point> onRightClick)
        {
            // Add new charts to the collection
            foreach (GunaChart chart in charts)
            {
                registeredCharts.Add(chart);
            }

            MouseClickChartManager.onLeftClick = onLeftClick;
            MouseClickChartManager.onRightClick = onRightClick;

            // Initialize message filter if not already done
            if (messageFilter == null)
            {
                messageFilter = new CustomMessageFilter();
                Application.AddMessageFilter(messageFilter);
            }
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
                        CustomControls.RecentlyOpenedMenu,
                        CustomControls.HelpMenu,
                        CustomControls.ControlDropDown_Panel
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
                    foreach (GunaChart chart in registeredCharts)
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