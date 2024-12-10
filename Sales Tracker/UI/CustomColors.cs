using Guna.UI2.WinForms.Enums;
using Sales_Tracker.Classes;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages the custom colors, providing different color settings for dark and light themes.
    /// </summary>
    public static class CustomColors
    {
        // Control colors
        private static Color _controlBack, _controlDisabledBack, _controlBorder, _controlUncheckedBorder, _controlPanelBorder, _controlSelectedBorder;

        // Text colors
        private static Color _text, _grayText, _linkColor;

        // File colors
        private static Color _fileHover;

        // Panel colors
        private static Color _panelBtn, _panelBtnHover;

        // Background colors
        private static Color _mainBackground, _background2, _background3, _background4;

        // DataGridView theme
        private static DataGridViewPresetThemes _dataGridViewTheme;

        // Accent colors
        private static Color _accentGreen;

        // Read-only accent colors that don't change with theme
        public static readonly Color AccentBlue = Color.FromArgb(25, 117, 197),
            AccentRed = Color.FromArgb(238, 89, 81),
            PastelBlue = Color.FromArgb(102, 153, 204),
            PastelGreen = Color.FromArgb(153, 204, 102);

        // Control Properties
        public static Color ControlBack => _controlBack;
        public static Color ControlDisabledBack => _controlDisabledBack;
        public static Color ControlBorder => _controlBorder;
        public static Color ControlUncheckedBorder => _controlUncheckedBorder;
        public static Color ControlPanelBorder => _controlPanelBorder;
        public static Color ControlSelectedBorder => _controlSelectedBorder;

        // Text Properties
        public static Color Text => _text;
        public static Color GrayText => _grayText;
        public static Color LinkColor => _linkColor;

        // File Properties
        public static Color FileHover => _fileHover;

        // Panel Properties
        public static Color PanelBtn => _panelBtn;
        public static Color PanelBtnHover => _panelBtnHover;

        // Background Properties
        public static Color MainBackground => _mainBackground;
        public static Color Background2 => _background2;
        public static Color Background3 => _background3;
        public static Color Background4 => _background4;

        // Theme Property
        public static DataGridViewPresetThemes DataGridViewTheme => _dataGridViewTheme;

        public static Color AccentGreen => _accentGreen;

        /// <summary>
        /// Sets all color values based on the current theme (Dark or Light).
        /// </summary>
        public static void SetColors()
        {
            Theme.MakeSureThemeIsNotWindows();

            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
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
            _controlBack = Color.FromArgb(62, 62, 66);
            _controlDisabledBack = Color.Gray;
            _controlBorder = Color.FromArgb(130, 130, 130);
            _controlUncheckedBorder = Color.FromArgb(125, 137, 149);
            _controlPanelBorder = Color.FromArgb(110, 110, 110);
            _controlSelectedBorder = Color.FromArgb(153, 209, 255);

            // Text colors
            _text = Color.White;
            _grayText = Color.FromArgb(160, 160, 160);
            _linkColor = Color.FromArgb(71, 157, 250);

            // File colors
            _fileHover = Color.FromArgb(77, 77, 77);

            // Panel colors
            _panelBtn = Color.FromArgb(62, 62, 66);
            _panelBtnHover = Color.FromArgb(90, 90, 94);

            // Background colors
            _mainBackground = Color.FromArgb(40, 40, 40);
            _background2 = Color.FromArgb(30, 30, 30);
            _background3 = Color.FromArgb(20, 20, 20);
            _background4 = Color.FromArgb(50, 50, 50);

            // Theme
            _dataGridViewTheme = DataGridViewPresetThemes.Dark;

            // Accent colors
            _accentGreen = Color.FromArgb(168, 233, 203);
        }
        private static void SetLightThemeColors()
        {
            // Control colors
            _controlBack = Color.FromArgb(220, 220, 220);
            _controlDisabledBack = Color.LightGray;
            _controlBorder = Color.FromArgb(150, 150, 150);
            _controlUncheckedBorder = Color.FromArgb(125, 137, 149);
            _controlPanelBorder = Color.FromArgb(50, 50, 50);
            _controlSelectedBorder = Color.FromArgb(153, 209, 255);

            // Text colors
            _text = Color.Black;
            _grayText = Color.Gray;
            _linkColor = Color.FromArgb(71, 157, 250);

            // File colors
            _fileHover = Color.FromArgb(229, 243, 255);

            // Panel colors
            _panelBtn = Color.FromArgb(246, 246, 246);
            _panelBtnHover = Color.FromArgb(214, 214, 214);

            // Background colors
            _mainBackground = Color.FromArgb(240, 240, 240);
            _background2 = Color.FromArgb(250, 250, 250);
            _background3 = Color.FromArgb(204, 204, 204);
            _background4 = Color.FromArgb(250, 250, 250);

            // Theme
            _dataGridViewTheme = DataGridViewPresetThemes.White;

            // Accent colors
            _accentGreen = Color.FromArgb(73, 186, 142);
        }
    }
}