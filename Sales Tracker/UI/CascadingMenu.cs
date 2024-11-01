using Guna.UI2.WinForms;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.UI
{
    public static class CascadingMenu
    {
        private static Guna2Panel menuToHide;
        private static readonly Timer HideMenu_timer = new();

        public static void Init()
        {
            HideMenu_timer.Interval = 800;
            HideMenu_timer.Tick += HideMenu_timer_Tick;
        }
        private static void HideMenu_timer_Tick(object sender, EventArgs e)
        {
            menuToHide.Parent?.Controls.Remove(menuToHide);
            HideMenu_timer.Enabled = false;
        }
        public static void OpenMenu()
        {
            HideMenu_timer.Enabled = false;
        }
        public static void CloseMenu(object sender, EventArgs e)
        {
            Guna2Button btn = (Guna2Button)sender;
            menuToHide = (Guna2Panel)btn.Tag;
            HideMenu_timer.Enabled = false;
            HideMenu_timer.Enabled = true;
        }
        public static void KeepMenuOpen()
        {
            HideMenu_timer.Enabled = false;
        }
    }
}