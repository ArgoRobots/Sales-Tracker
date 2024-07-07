using static Sales_Tracker.Classes.Theme;

namespace Sales_Tracker.Classes
{
    static internal class CustomColors
    {
        // Control colors
        public static Color
            controlBack,
            controlDisabledBack,
            controlBorder,
            controlUncheckedBorder,
            controlPanelBorder,
            text,
            fileHover,
            fileSelected,
            controlSelectedBorder,
            commandMouseDown,
            commandHidden,
            panelBtn,
            panelBtnHover,
            grayText,
            linkColor;

        // Background colors
        public static Color mainBackground, background2, background3, background4;

        // Accent colors
        public static readonly Color
            accent_blue = Color.FromArgb(25, 117, 197),
            accent_red = Color.FromArgb(238, 89, 81),
            accent_green = Color.FromArgb(168, 233, 203);

        public static Guna.UI2.WinForms.Enums.DataGridViewPresetThemes dataGridViewTheme;

        public static void SetColors()
        {
            if (Theme.CurrentTheme == Theme.ThemeType.Windows)
            {
                int value = (int)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);

                if (value == 0)
                {
                    Theme.CurrentTheme = Theme.ThemeType.Dark;
                }
                else if (value == 1)
                {
                    Theme.CurrentTheme = Theme.ThemeType.Light;
                }
            }

            if (CurrentTheme == ThemeType.Dark)
            {
                controlBack = Color.FromArgb(62, 62, 66);
                controlDisabledBack = Color.Gray;
                controlBorder = Color.FromArgb(130, 130, 130);
                controlUncheckedBorder = Color.FromArgb(125, 137, 149);
                controlPanelBorder = Color.FromArgb(110, 110, 110);
                text = Color.White;
                fileHover = Color.FromArgb(77, 77, 77);
                fileSelected = Color.FromArgb(119, 119, 119);
                controlSelectedBorder = Color.FromArgb(153, 209, 255);
                commandMouseDown = Color.FromArgb(50, 50, 50);
                commandHidden = Color.FromArgb(20, 20, 20);
                panelBtn = Color.FromArgb(62, 62, 66);
                panelBtnHover = Color.FromArgb(90, 90, 94);
                dataGridViewTheme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Dark;
                grayText = Color.FromArgb(160, 160, 160);
                linkColor = Color.FromArgb(71, 157, 250);

                mainBackground = Color.FromArgb(40, 40, 40);
                background2 = Color.FromArgb(30, 30, 30);
                background3 = Color.FromArgb(20, 20, 20);
                background4 = Color.FromArgb(50, 50, 50);
            }
            else // Light
            {
                controlBack = Color.FromArgb(243, 243, 243);
                controlDisabledBack = Color.LightGray;
                controlBorder = Color.FromArgb(214, 214, 214);
                controlUncheckedBorder = Color.FromArgb(125, 137, 149);
                controlPanelBorder = Color.FromArgb(50, 50, 50);
                text = Color.Black;
                fileHover = Color.FromArgb(229, 243, 255);
                fileSelected = Color.FromArgb(204, 232, 255);
                controlSelectedBorder = Color.FromArgb(153, 209, 255);
                commandMouseDown = Color.FromArgb(225, 225, 225);
                commandHidden = Color.LightGray;
                panelBtn = Color.FromArgb(246, 246, 246);
                panelBtnHover = Color.FromArgb(214, 214, 214);
                dataGridViewTheme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.LightGrid;
                grayText = Color.Gray;
                linkColor = Color.FromArgb(71, 157, 250);

                mainBackground = Color.White;
                background2 = Color.FromArgb(250, 250, 250);
                background3 = Color.FromArgb(204, 204, 204);
                background4 = Color.FromArgb(242, 242, 242);
            }
        }
    }
}