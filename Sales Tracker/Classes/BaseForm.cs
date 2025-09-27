using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    public partial class BaseForm : Form
    {
        private const int WM_ACTIVATE = 0x0006;
        private const int WA_INACTIVE = 0;

        protected override void WndProc(ref Message m)
        {
            // If Form is being deactivated 
            if (m.Msg == WM_ACTIVATE)
            {
                if (m.WParam.ToInt32() == WA_INACTIVE)
                {
                    if (MainMenu_Form.Instance != null &&
                        !MainMenu_Form.Instance.Controls.Contains(MainMenu_Form.DateRangePanel))
                    {
                        CustomControls.CloseAllPanels();
                    }

                    // Close other panels not called in CloseAllPanels()
                    GetStarted_Form.Instance.Controls.Remove(GetStarted_Form.RightClickOpenRecent_Panel);
                }
            }
            base.WndProc(ref m);
        }
    }
}