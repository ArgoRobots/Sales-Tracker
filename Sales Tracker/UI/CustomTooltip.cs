using Guna.UI2.WinForms;
namespace Sales_Tracker.UI
{
    public static class CustomTooltip
    {
        // Properties
        private static readonly Dictionary<Control, Guna2HtmlToolTip> tooltips = [];
        private static Control? lastControl = null;
        private static Guna2HtmlToolTip? activeTooltip = null;

        // Methods
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

            // Add mouse enter/leave handlers to manage tooltip visibility
            tooltip.Popup += (s, e) =>
            {
                if (activeTooltip != null && activeTooltip != tooltip)
                {
                    // Get the active form as the owner window
                    Form owner = Form.ActiveForm;
                    if (owner != null)
                    {
                        activeTooltip.Hide(owner);
                    }
                }
                activeTooltip = tooltip;
            };

            return tooltip;
        }
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
            activeTooltip = null;
            lastControl = null;
        }
        public static void SetToolTip(Control control, string title, string message)
        {
            if (!tooltips.TryGetValue(control, out Guna2HtmlToolTip? tooltip))
            {
                tooltip = CreateTooltip();
                tooltips[control] = tooltip;

                // Add mouse enter handler to the control
                control.MouseEnter += (s, e) =>
                {
                    if (lastControl != control && lastControl != null)
                    {
                        if (tooltips.TryGetValue(lastControl, out Guna2HtmlToolTip? lastTooltip))
                        {
                            lastTooltip.Hide(lastControl);
                        }
                    }
                    lastControl = control;
                };

                // Add mouse leave handler to the control
                control.MouseLeave += (s, e) =>
                {
                    if (tooltips.TryGetValue(control, out Guna2HtmlToolTip? currentTooltip))
                    {
                        // Add a small delay before hiding to prevent flickering
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