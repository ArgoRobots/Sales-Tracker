namespace Sales_Tracker.UI
{
    /// <summary>
    /// Message filter that intercepts mouse clicks globally and triggers panel close action.
    /// Replaces the need for attaching Click event handlers to every control.
    /// </summary>
    public class PanelCloseFilter : IMessageFilter
    {
        private readonly Control _panelToMonitor;
        private readonly Action _closeAction;

        /// <summary>
        /// Initializes a new instance of the PanelCloseFilter.
        /// </summary>
        /// <param name="panelToMonitor">The panel that should remain open when clicked.</param>
        /// <param name="closeAction">The action to invoke when clicking outside the panel.</param>
        public PanelCloseFilter(Control panelToMonitor, Action closeAction)
        {
            _panelToMonitor = panelToMonitor;
            _closeAction = closeAction;
        }

        /// <summary>
        /// Filters Windows messages before they are dispatched to controls.
        /// Detects left mouse button down events and closes panels if clicked outside.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            // Detect left mouse button down event (WM_LBUTTONDOWN = 0x0201)
            if (m.Msg != 0x0201)
            {
                return false;
            }

            Point mousePosition = Control.MousePosition;

            // Check if the panel is visible and has a parent
            if (_panelToMonitor?.Parent == null || !_panelToMonitor.Visible)
            {
                return false;
            }

            // Check if the click was inside the panel
            Point localMousePosition = _panelToMonitor.PointToClient(mousePosition);
            if (_panelToMonitor.ClientRectangle.Contains(localMousePosition))
            {
                // Click was inside the panel, don't close
                return false;
            }

            // Click was outside the panel, invoke close action
            _closeAction?.Invoke();
            return false;
        }
    }
}
