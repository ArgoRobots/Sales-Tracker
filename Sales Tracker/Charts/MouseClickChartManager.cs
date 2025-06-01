using Guna.Charts.WinForms;
using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Manages mouse click detection for GunaChart controls and invokes specific actions, because unfortunately Guna Charts does not support this.
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
            static void leftClickAction(GunaChart statisticsControls) => CustomControls.CloseAllPanels();
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
                if (m.Msg != 0x0201 && m.Msg != 0x0204)
                {
                    return false;
                }

                bool isRightClick = m.Msg == 0x0204;
                Point mousePosition = Control.MousePosition;

                // Check if any other form is over the click position
                if (IsAnotherFormAtPosition(mousePosition))
                {
                    return false;
                }

                // Create a list of controls
                List<Control> controlsList =
                [
                    CustomControls.FileMenu,
                    CustomControls.RecentlyOpenedMenu,
                    CustomControls.HelpMenu,
                    CustomControls.ControlDropDown_Panel,
                    RightClickGunaChartMenu.RightClickGunaChart_Panel
                ];
                Control mainPanel = DateRange_Form.Instance?.Main_Panel;
                if (mainPanel != null && MainMenu_Form.Instance.Controls.Contains(mainPanel))
                {
                    controlsList.Add(mainPanel);
                }

                // Ignore the click if it was over any of the controls
                foreach (Control control in controlsList)
                {
                    if (control.Parent == null)
                    {
                        continue;
                    }

                    // Check if the mouse click was within the bounds of the control
                    Point localMousePosition = control.PointToClient(mousePosition);
                    if (control.ClientRectangle.Contains(localMousePosition))
                    {
                        return false;
                    }
                }

                // Check if the click happened on any of the charts
                foreach (GunaChart chart in registeredCharts)
                {
                    if (!chart.Visible)
                    {
                        // For the analyticss charts if they are not shown
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
                return false;
            }

            /// <summary>
            /// Checks if there is another form at the specified position
            /// </summary>
            private static bool IsAnotherFormAtPosition(Point screenPosition)
            {
                // If there's only one form in the application, then there can't be another form on top
                if (Application.OpenForms.Count <= 1)
                {
                    return false;
                }

                // Check if any other visible form contains the position
                foreach (Form form in Application.OpenForms)
                {
                    if (form != MainMenu_Form.Instance && form.Visible)
                    {
                        // Check if the form contains the screen position
                        Point clientPoint = form.PointToClient(screenPosition);
                        if (form.ClientRectangle.Contains(clientPoint))
                        {
                            return true;  // Another form is at this position
                        }
                    }
                }

                return false;
            }
        }
    }
}