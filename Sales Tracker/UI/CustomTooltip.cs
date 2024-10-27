using Guna.UI2.WinForms;
namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides custom tooltip functionality.
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
        /// Cleans up all tooltips and resets the tooltip system.
        /// Call this when starting the application or clearing up resources.
        /// </summary>
        public static void InitializeTooltip()
        {
            foreach (Guna2HtmlToolTip tooltip in tooltips.Values)
            {
                Form owner = Form.ActiveForm;
                if (owner != null)
                {
                    tooltip.Hide(owner);
                }
                tooltip.Dispose();
            }
            tooltips.Clear();
            lastControl = null;
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

                // Handle mouse enter to manage tooltip visibility
                control.MouseEnter += (s, e) =>
                {
                    // Only show tooltip if enabled in settings
                    if (!Properties.Settings.Default.ShowTooltips) { return; }

                    // Hide the previous tooltip if a new one is shown before it closes naturally
                    if (lastControl != control && lastControl != null)
                    {
                        if (tooltips.TryGetValue(lastControl, out Guna2HtmlToolTip? lastTooltip))
                        {
                            lastTooltip.Hide(lastControl);
                        }
                    }
                    lastControl = control;
                };

                // Handle mouse leave with delayed hiding
                control.MouseLeave += (s, e) =>
                {
                    if (tooltips.TryGetValue(control, out Guna2HtmlToolTip? currentTooltip))
                    {
                        // Add delay to prevent flickering when moving mouse quickly
                        Task.Delay(50).ContinueWith(_ =>
                        {
                            control.BeginInvoke(() =>
                            {
                                if (!control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition)))
                                {
                                    currentTooltip.Hide(control);
                                    if (lastControl == control)
                                    {
                                        lastControl = null;
                                    }
                                }
                            });
                        });
                    }
                };
            }

            tooltip.ToolTipTitle = title;
            tooltip.SetToolTip(control, message);
        }
    }
}