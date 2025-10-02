namespace Sales_Tracker.Classes
{
    // Thread-safe UI extensions
    public static class UIExtensions
    {
        /// <summary>
        /// Executes an action on the UI thread if required, otherwise it executes immediately.
        /// </summary>
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Executes a function on the UI thread if required and returns the result, otherwise it executes immediately.
        /// </summary>
        /// <returns>The result of the function execution.</returns>
        public static T InvokeIfRequired<T>(this Control control, Func<T> func)
        {
            if (control.InvokeRequired)
            {
                return control.Invoke(func);
            }
            else
            {
                return func();
            }
        }

        /// <summary>
        /// Extension method to disable control scrolling and forward to parent panel.
        /// </summary>
        public static void DisableScrollAndForwardToPanel(this Control control)
        {
            void handler(object? sender, MouseEventArgs e)
            {
                // Find parent scrollable panel
                Panel? panel = control.Parent as Panel;
                while (panel != null && !panel.AutoScroll)
                {
                    panel = panel.Parent as Panel;
                }

                if (panel != null)
                {
                    // Forward scroll to panel
                    int newValue = panel.VerticalScroll.Value - e.Delta;
                    panel.VerticalScroll.Value = Math.Clamp(newValue,
                        panel.VerticalScroll.Minimum,
                        panel.VerticalScroll.Maximum);
                    panel.PerformLayout();

                    // Mark as handled
                    if (e is HandledMouseEventArgs args)
                    {
                        args.Handled = true;
                    }
                }
            }

            // Attach to main control and all children (some Guna controls have children controls)
            control.MouseWheel += handler;
            foreach (Control child in control.Controls)
            {
                child.MouseWheel += handler;
            }
        }
    }
}