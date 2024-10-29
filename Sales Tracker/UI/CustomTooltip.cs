using Guna.UI2.WinForms;
namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides custom tooltip functionality using Guna2HtmlToolTip.
    /// </summary>
    public static class CustomTooltip
    {
        // Properties
        private static readonly Dictionary<Control, Guna2HtmlToolTip> tooltips = [];
        private static Control? lastControl = null;

        // Methods
        /// <summary>
        /// Creates and configures a new Guna2HtmlToolTip instance.
        /// </summary>
        private static Guna2HtmlToolTip CreateTooltip()
        {
            Guna2HtmlToolTip tooltip = new()
            {
                // Basic settings
                TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                TitleForeColor = CustomColors.text,
                BackColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlPanelBorder,
                Font = new Font("Segoe UI", 10),

                // Animation settings
                UseAnimation = true,
                UseFading = true,

                // Timing settings
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100
            };

            // Only show the tooltip if the setting is true
            tooltip.Popup += (s, e) =>
            {
                if (!Properties.Settings.Default.ShowTooltips)
                {
                    e.Cancel = true;
                    return;
                }
            };

            return tooltip;
        }

        /// <summary>
        /// Sets or updates a tooltip for a control with optional warning formatting.
        /// Only shows tooltips if enabled in application settings.
        /// </summary>
        public static void SetToolTip(Control control, string title, string message)
        {
            if (!tooltips.TryGetValue(control, out Guna2HtmlToolTip? tooltip))
            {
                tooltip = CreateTooltip();
                tooltips[control] = tooltip;

                control.MouseEnter += Control_MouseEnter;
                control.MouseLeave += Control_MouseLeave;
            }

            tooltip.ToolTipTitle = title;
            tooltip.SetToolTip(control, message);
        }
        private static void Control_MouseEnter(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            // Hide the previous tooltip if a new one is shown before it closes naturally
            if (lastControl != control && lastControl != null && tooltips.TryGetValue(lastControl, out Guna2HtmlToolTip? lastTooltip))
            {
                lastTooltip.Hide(lastControl);
            }
            lastControl = control;
        }
        private static void Control_MouseLeave(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            if (!tooltips.TryGetValue(control, out Guna2HtmlToolTip tooltip))
            {
                return;
            }

            // Add delay to prevent flickering when moving mouse quickly
            Task.Delay(50).ContinueWith(_ =>
            {
                control.BeginInvoke(() =>
                {
                    if (!control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition)))
                    {
                        tooltip.Hide(control);
                        if (lastControl == control)
                        {
                            lastControl = null;
                        }
                    }
                });
            });
        }
    }
}