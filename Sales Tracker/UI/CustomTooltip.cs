using Guna.UI2.WinForms;
using Sales_Tracker.Theme;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides custom tooltip functionality using Guna2HtmlToolTip.
    /// </summary>
    public static class CustomTooltip
    {
        // Properties
        private static readonly Dictionary<Control, Guna2HtmlToolTip> _tooltips = [];

        // Methods
        /// <summary>
        /// Creates and configures a new Guna2HtmlToolTip instance.
        /// </summary>
        private static Guna2HtmlToolTip CreateTooltip()
        {
            Guna2HtmlToolTip tooltip = new()
            {
                TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                TitleForeColor = CustomColors.Text,
                BackColor = CustomColors.ControlBack,
                ForeColor = CustomColors.Text,
                BorderColor = CustomColors.ControlPanelBorder,
                Font = new Font("Segoe UI", 10),
                InitialDelay = 500,
                ReshowDelay = 100
            };

            // Only show the tooltip if the setting is true
            tooltip.Popup += (_, e) => e.Cancel = !Properties.Settings.Default.ShowTooltips;

            return tooltip;
        }

        /// <summary>
        /// Sets or updates a tooltip for a control with optional warning formatting.
        /// Only shows tooltips if enabled in application settings.
        /// </summary>
        public static void SetToolTip(Control control, string title, string message)
        {
            title = LanguageManager.TranslateString(title);
            message = LanguageManager.TranslateString(message);

            if (!_tooltips.TryGetValue(control, out Guna2HtmlToolTip tooltip))
            {
                tooltip = CreateTooltip();
                _tooltips[control] = tooltip;
            }

            tooltip.ToolTipTitle = title;
            tooltip.SetToolTip(control, message);
        }
    }
}