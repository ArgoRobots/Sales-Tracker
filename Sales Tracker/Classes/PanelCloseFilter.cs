namespace Sales_Tracker.Classes
{
    public class PanelCloseFilter(Control panelToMonitor, Action closeAction) : IMessageFilter
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private readonly Control _panelToMonitor = panelToMonitor;
        private readonly Action _closeAction = closeAction;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN)
            {
                if (_panelToMonitor.Parent != null && _panelToMonitor.Visible)
                {
                    Point clickPoint = _panelToMonitor.Parent.PointToClient(Control.MousePosition);

                    if (!_panelToMonitor.Bounds.Contains(clickPoint))
                    {
                        _closeAction?.Invoke();
                    }
                }
            }
            return false;
        }
    }
}