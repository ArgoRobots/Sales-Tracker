using Guna.Charts.WinForms;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages mouse click detection for GunaChart controls and invokes specific actions
    /// based on left-click or right-click events.
    /// </summary>
    public static class MouseClickChartManager
    {
        private static GunaChart[] _charts;
        private static Action<GunaChart> _onLeftClick;
        private static Action<GunaChart, Point> _onRightClick;

        /// <summary>
        /// Initializes the click manager for specified GunaChart controls and assigns
        /// actions to be called on left and right mouse clicks.
        /// </summary>
        public static void Initialize(GunaChart[] charts, Action<GunaChart> onLeftClick, Action<GunaChart, Point> onRightClick)
        {
            _charts = charts;
            _onLeftClick = onLeftClick;
            _onRightClick = onRightClick;

            Application.AddMessageFilter(new CustomMessageFilter());
        }

        /// <summary>
        /// Custom IMessageFilter implementation that detects mouse clicks and determines
        /// whether a left or right click occurred on a GunaChart control.
        /// </summary>
        private class CustomMessageFilter : IMessageFilter
        {
            /// <summary>
            /// Filters Windows messages before they are dispatched to controls.
            /// Detects left and right mouse button down events and invokes the appropriate action.
            /// </summary>
            /// <returns>True if the message was handled and should not be passed to other controls; otherwise, false.</returns>
            public bool PreFilterMessage(ref Message m)
            {
                // Detect left or right mouse button down events
                if (m.Msg == 0x0201 || m.Msg == 0x0204)  // 0x0201 is WM_LBUTTONDOWN, 0x0204 is WM_RBUTTONDOWN
                {
                    bool isLeftClick = (m.Msg == 0x0201);
                    Point mousePosition = Control.MousePosition;

                    // Check if the click happened on any of the charts
                    foreach (GunaChart chart in _charts)
                    {
                        Point localMousePosition = chart.Parent.PointToClient(mousePosition);

                        // Check if the click is within the bounds of the chart
                        if (chart.Bounds.Contains(localMousePosition))
                        {
                            if (isLeftClick)
                            {
                                // Trigger the left click action with chart
                                _onLeftClick?.Invoke(chart);
                            }
                            else
                            {
                                // Trigger the right click action with chart and mouse position
                                _onRightClick?.Invoke(chart, mousePosition);
                            }
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}