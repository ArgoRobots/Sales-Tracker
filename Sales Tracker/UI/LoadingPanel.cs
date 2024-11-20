using Guna.UI2.WinForms;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// This prevents the screen from flashing white while the Form is loading.
    /// This is especially useful when dark theme is enabled and there are a lot of things to load.
    /// </summary>
    public class LoadingPanel
    {
        private static Panel _blankLoadingPanel;
        private static Panel _loadingPanel;
        private static Label _messageLabel;

        public static Panel BlankLoadingPanelInstance => _blankLoadingPanel;
        public static Panel LoadingPanelInstance => _loadingPanel;

        public static void InitBlankLoadingPanel()
        {
            _blankLoadingPanel = new Panel
            {
                BackColor = CustomColors.MainBackground
            };
        }
        public static void InitLoadingPanel()
        {
            _loadingPanel = new Panel
            {
                BackColor = CustomColors.MainBackground
            };

            Guna2WinProgressIndicator progressIndicator = new()
            {
                AutoStart = true,
                ProgressColor = CustomColors.AccentBlue,
            };

            _messageLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Text = "Loading...",
                TextAlign = ContentAlignment.MiddleCenter
            };

            _loadingPanel.Controls.Add(progressIndicator);
            _loadingPanel.Controls.Add(_messageLabel);
        }
        /// <summary>
        /// Displays a loading screen with a progress indicator and a message over a specified control.
        /// </summary>
        /// <remarks>
        /// The message is translated into the current language using <see cref="LanguageManager.TranslateSingleString"/>.
        /// </remarks>
        public static void ShowLoadingScreen(Control control, string message)
        {
            string translatedMessage = LanguageManager.TranslateSingleString(message);

            _loadingPanel.Size = control.Size;
            control.Controls.Add(_loadingPanel);

            Guna2WinProgressIndicator progressIndicator = (Guna2WinProgressIndicator)_loadingPanel.Controls[0];
            progressIndicator.Location = new Point(
                (_loadingPanel.Width - progressIndicator.Width) / 2,
                (_loadingPanel.Height - progressIndicator.Height) / 2
            );

            _messageLabel.Text = translatedMessage;
            _messageLabel.Location = new Point(
                (_loadingPanel.Width - _messageLabel.Width) / 2,
                progressIndicator.Top - progressIndicator.Height - 20
            );
            _messageLabel.ForeColor = CustomColors.Text;

            _loadingPanel.BringToFront();
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
        public static void HideLoadingScreen(Control control)
        {
            control.Controls.Remove(_loadingPanel);
        }
        public static void HideBlankLoadingPanel(Control control)
        {
            control.Controls.Remove(_blankLoadingPanel);
        }
    }
}