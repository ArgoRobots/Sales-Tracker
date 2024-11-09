using Guna.UI2.WinForms;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// This prevents the screen from flashing white while the Form is loading.
    /// This is especially useful when dark theme is enabled and there are a lot of things to load.
    /// </summary>
    public class LoadingPanel
    {
        // Properties
        private static Panel _blankLoadingPanel;
        private static Panel _loadingPanel;

        // Getters
        public static Panel BlankLoadingPanelInstance => _blankLoadingPanel;
        public static Panel LoadingPanelInstance => _loadingPanel;

        // blankLoadingPanel
        public static void InitBlankLoadingPanel()
        {
            _blankLoadingPanel = new Panel
            {
                BackColor = CustomColors.MainBackground
            };
        }
        public static void ShowBlankLoadingPanel(Control control)
        {
            if (_blankLoadingPanel.InvokeRequired)
            {
                _blankLoadingPanel.Invoke(new Action(show));
            }
            else
            {
                show();
            }

            void show()
            {
                _blankLoadingPanel.Size = control.Size;
                control.Controls.Add(_blankLoadingPanel);
                _blankLoadingPanel.BringToFront();
            }
        }
        public static void HideBlankLoadingPanel(Control control)
        {
            control.Controls.Remove(_blankLoadingPanel);
        }

        // loadingPanel
        public static void InitLoadingPanel()
        {
            if (_loadingPanel != null) { return; }

            _loadingPanel = new Panel
            {
                BackColor = CustomColors.MainBackground
            };
            Guna2WinProgressIndicator progressIndicator = new()
            {
                AutoStart = true,
                ProgressColor = CustomColors.AccentBlue,
            };

            _loadingPanel.Controls.Add(progressIndicator);
        }
        public static void ShowLoadingScreen(Control control)
        {
            _loadingPanel.Size = control.Size;
            control.Controls.Add(_loadingPanel);

            Guna2WinProgressIndicator progressIndicator = (Guna2WinProgressIndicator)_loadingPanel.Controls[0];
            progressIndicator.Location = new Point((_loadingPanel.Width - progressIndicator.Width) / 2, (_loadingPanel.Height - progressIndicator.Height) / 2);

            _loadingPanel.BringToFront();
        }
        public static void HideLoadingScreen(Control control)
        {
            control.Controls.Remove(_loadingPanel);
        }
    }
}