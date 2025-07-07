using Guna.UI2.WinForms.Enums;

namespace Sales_Tracker.Theme
{
    /// <summary>
    /// Manages the custom colors, providing different color settings for dark and light themes.
    /// </summary>
    public static class CustomColors
    {
        // Read-only accent colors that don't change with theme
        public static readonly Color AccentBlue = Color.FromArgb(25, 117, 197),
            AccentRed = Color.FromArgb(238, 89, 81),
            PastelBlue = Color.FromArgb(102, 153, 204),
            PastelGreen = Color.FromArgb(153, 204, 102);

        public static Color ControlBack { get; private set; }
        public static Color ControlDisabledBack { get; private set; }
        public static Color ControlBorder { get; private set; }
        public static Color ControlUncheckedBorder { get; private set; }
        public static Color ControlPanelBorder { get; private set; }
        public static Color Text { get; private set; }
        public static Color GrayText { get; private set; }
        public static Color LinkColor { get; private set; }
        public static Color DebugText { get; private set; }
        public static Color MouseHover { get; private set; }
        public static Color PanelBtn { get; private set; }
        public static Color PanelBtnHover { get; private set; }
        public static Color MainBackground { get; private set; }
        public static Color HeaderBackground { get; private set; }
        public static Color ToolbarBackground { get; private set; }
        public static Color ContentPanelBackground { get; private set; }
        public static DataGridViewPresetThemes DataGridViewTheme { get; private set; }
        public static Color AccentGreen { get; private set; }
        public static Color ReturnedItemBackground { get; private set; }
        public static Color ReturnedItemSelection { get; private set; }
        public static Color ReturnedItemText { get; private set; }

        /// <summary>
        /// Sets all color values based on the current theme. These colors are used for UI controls throughout the application.
        /// </summary>
        public static void SetColors()
        {
            if (ThemeManager.IsDarkTheme())
            {
                SetDarkThemeColors();
            }
            else
            {
                SetLightThemeColors();
            }
        }
        private static void SetDarkThemeColors()
        {
            // Control colors
            ControlBack = Color.FromArgb(62, 62, 66);
            ControlDisabledBack = Color.Gray;
            ControlBorder = Color.FromArgb(130, 130, 130);
            ControlUncheckedBorder = Color.FromArgb(125, 137, 149);
            ControlPanelBorder = Color.FromArgb(110, 110, 110);

            // Text colors
            Text = Color.White;
            GrayText = Color.FromArgb(160, 160, 160);
            LinkColor = Color.FromArgb(71, 157, 250);
            DebugText = Color.FromArgb(0, 255, 255);

            // File colors
            MouseHover = Color.FromArgb(77, 77, 77);

            // Panel colors
            PanelBtn = Color.FromArgb(62, 62, 66);
            PanelBtnHover = Color.FromArgb(90, 90, 94);

            // Background colors
            MainBackground = Color.FromArgb(40, 40, 40);
            HeaderBackground = Color.FromArgb(30, 30, 30);
            ToolbarBackground = Color.FromArgb(20, 20, 20);
            ContentPanelBackground = Color.FromArgb(50, 50, 50);

            // DataGridView colors
            DataGridViewTheme = DataGridViewPresetThemes.Dark;
            ReturnedItemBackground = Color.FromArgb(80, 45, 45);
            ReturnedItemSelection = Color.FromArgb(100, 60, 60);
            ReturnedItemText = Color.FromArgb(255, 180, 180);

            // Accent colors
            AccentGreen = Color.FromArgb(168, 233, 203);
        }
        private static void SetLightThemeColors()
        {
            // Control colors
            ControlBack = Color.FromArgb(220, 220, 220);
            ControlDisabledBack = Color.LightGray;
            ControlBorder = Color.FromArgb(150, 150, 150);
            ControlUncheckedBorder = Color.FromArgb(125, 137, 149);
            ControlPanelBorder = Color.FromArgb(50, 50, 50);

            // Text colors
            Text = Color.Black;
            GrayText = Color.Gray;
            LinkColor = Color.FromArgb(71, 157, 250);
            DebugText = Color.FromArgb(0, 100, 150);

            // File colors
            MouseHover = Color.FromArgb(229, 243, 255);

            // Panel colors
            PanelBtn = Color.FromArgb(246, 246, 246);
            PanelBtnHover = Color.FromArgb(214, 214, 214);

            // Background colors
            MainBackground = Color.FromArgb(240, 240, 240);
            HeaderBackground = Color.FromArgb(250, 250, 250);
            ToolbarBackground = Color.FromArgb(204, 204, 204);
            ContentPanelBackground = Color.FromArgb(250, 250, 250);

            // DataGridView colors
            DataGridViewTheme = DataGridViewPresetThemes.White;
            ReturnedItemBackground = Color.FromArgb(255, 245, 245);
            ReturnedItemSelection = Color.FromArgb(240, 220, 220);
            ReturnedItemText = Color.FromArgb(140, 70, 70);

            // Accent colors
            AccentGreen = Color.FromArgb(60, 160, 120);
        }
    }
}