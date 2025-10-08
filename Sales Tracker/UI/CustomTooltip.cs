using Guna.UI2.WinForms;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides custom tooltip functionality using Guna2HtmlToolTip.
    /// </summary>
    public static class CustomTooltip
    {
        // Properties - one tooltip per form
        private static readonly Dictionary<Form, Guna2HtmlToolTip> _tooltips = [];
        private static readonly Dictionary<Control, (string title, string message)> _tooltipData = [];

        // Methods
        /// <summary>
        /// Gets or creates the tooltip instance for a specific form.
        /// </summary>
        private static Guna2HtmlToolTip GetTooltipForForm(Form form)
        {
            if (!_tooltips.TryGetValue(form, out Guna2HtmlToolTip tooltip))
            {
                tooltip = new Guna2HtmlToolTip
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

                _tooltips[form] = tooltip;

                // Clean up when form is disposed
                form.Disposed += (_, _) =>
                {
                    if (_tooltips.TryGetValue(form, out Guna2HtmlToolTip t))
                    {
                        t.Dispose();
                        _tooltips.Remove(form);
                    }
                };
            }
            return tooltip;
        }

        /// <summary>
        /// Sets or updates a tooltip for a control with optional warning formatting.
        /// Only shows tooltips if enabled in application settings.
        /// </summary>
        public static void SetToolTip(Control control, string title, string message)
        {
            // Find the parent form
            Form parentForm = control.FindForm();
            if (parentForm == null) { return; }

            title = LanguageManager.TranslateString(title);
            message = LanguageManager.TranslateString(message);

            Guna2HtmlToolTip tooltip = GetTooltipForForm(parentForm);

            // Store the tooltip data for this control
            _tooltipData[control] = (title, message);

            tooltip.ToolTipTitle = title;
            tooltip.SetToolTip(control, message);
        }

        /// <summary>
        /// Updates all existing tooltips to match the current theme colors.
        /// </summary>
        public static void UpdateAllToolTipThemes()
        {
            foreach (Guna2HtmlToolTip tooltip in _tooltips.Values)
            {
                tooltip.TitleForeColor = CustomColors.Text;
                tooltip.BackColor = CustomColors.ControlBack;
                tooltip.ForeColor = CustomColors.Text;
                tooltip.BorderColor = CustomColors.ControlPanelBorder;
            }
        }
    }
}