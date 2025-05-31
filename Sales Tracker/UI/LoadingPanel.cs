using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Theme;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// This prevents the screen from flashing white while the Form is loading.
    /// This is especially useful when dark theme is enabled and there are a lot of things to load.
    /// </summary>
    public class LoadingPanel
    {
        // Properties
        private static Label _messageLabel;

        // Getters and setters
        public static Panel BlankLoadingPanelInstance { get; private set; }
        public static Panel LoadingPanelInstance { get; private set; }

        public static void InitBlankLoadingPanel()
        {
            BlankLoadingPanelInstance = new Panel
            {
                BackColor = CustomColors.MainBackground
            };
        }
        public static void InitLoadingPanel()
        {
            LoadingPanelInstance = new Panel
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

            LoadingPanelInstance.Controls.Add(progressIndicator);
            LoadingPanelInstance.Controls.Add(_messageLabel);
        }

        /// <summary>
        /// Displays a loading screen with a progress indicator and a message over a specified control.
        /// </summary>
        /// <remarks>
        /// The message is translated into the current language using <see cref="LanguageManager.TranslateString"/>.
        /// </remarks>
        public static void ShowLoadingScreen(Control control, string message)
        {
            string translatedMessage = LanguageManager.TranslateString(message);

            LoadingPanelInstance.Size = control.Size;
            control.Controls.Add(LoadingPanelInstance);

            Guna2WinProgressIndicator progressIndicator = (Guna2WinProgressIndicator)LoadingPanelInstance.Controls[0];
            progressIndicator.Location = new Point(
                (LoadingPanelInstance.Width - progressIndicator.Width) / 2,
                (LoadingPanelInstance.Height - progressIndicator.Height) / 2
            );

            _messageLabel.Text = translatedMessage;
            _messageLabel.Location = new Point(
                (LoadingPanelInstance.Width - _messageLabel.Width) / 2,
                progressIndicator.Top - progressIndicator.Height - 20
            );
            _messageLabel.ForeColor = CustomColors.Text;

            LoadingPanelInstance.BringToFront();
        }
        public static void ShowBlankLoadingPanel(Control control)
        {
            BlankLoadingPanelInstance.InvokeIfRequired(() =>
            {
                BlankLoadingPanelInstance.Size = control.Size;
                control.Controls.Add(BlankLoadingPanelInstance);
                BlankLoadingPanelInstance.BringToFront();
            });
        }
        public static void HideLoadingScreen(Control control)
        {
            control.Controls.Remove(LoadingPanelInstance);
        }
        public static void HideBlankLoadingPanel(Control control)
        {
            control.Controls.Remove(BlankLoadingPanelInstance);
        }
        public static void UpdateTheme()
        {
            LoadingPanelInstance.BackColor = CustomColors.MainBackground;
            BlankLoadingPanelInstance.BackColor = CustomColors.MainBackground;
        }
    }
}