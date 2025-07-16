using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    public partial class BaseForm : Form
    {
        private const int WM_ACTIVATE = 0x0006;
        private const int WA_INACTIVE = 0;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ACTIVATE)
            {
                if (m.WParam.ToInt32() == WA_INACTIVE)
                {
                    // Form is being deactivated (and TimeRangePanel is not open)
                    if (MainMenu_Form.Instance != null &&
                        !MainMenu_Form.Instance.Controls.Contains(MainMenu_Form.TimeRangePanel))
                    {
                        CustomControls.CloseAllPanels();
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
