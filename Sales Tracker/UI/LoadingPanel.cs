using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides a loading animation panel for better UX during data-intensive operations.
    /// This prevents the screen from flashing white while the Form is loading, especially in dark theme scenarios.
    /// </summary>
    public class LoadingPanel
    {
        // Properties
        private static Label messageLabel;
        private static Timer animationTimer;
        private static Guna2WinProgressIndicator progressIndicator;

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

            progressIndicator = new Guna2WinProgressIndicator
            {
                ProgressColor = CustomColors.AccentBlue
            };

            messageLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Text = "Loading...",
                AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create a timer to ensure smooth animation
            animationTimer = new Timer
            {
                Interval = 16, // ~60 FPS
                Enabled = false
            };
            animationTimer.Tick += AnimationTimer_Tick;

            LoadingPanelInstance.Controls.Add(progressIndicator);
            LoadingPanelInstance.Controls.Add(messageLabel);
        }
        private static void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Force the progress indicator to update, even if the UI thread is busy
            progressIndicator?.Invalidate();
            Application.DoEvents();
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
            LoadingPanelInstance.Dock = DockStyle.Fill;

            progressIndicator.Location = new Point(
                (LoadingPanelInstance.Width - progressIndicator.Width) / 2,
                (LoadingPanelInstance.Height - progressIndicator.Height) / 2
            );

            messageLabel.Text = translatedMessage;
            messageLabel.Location = new Point(
                (LoadingPanelInstance.Width - messageLabel.Width) / 2,
                progressIndicator.Top - progressIndicator.Height - 20
            );
            messageLabel.ForeColor = CustomColors.Text;

            LoadingPanelInstance.BringToFront();
            animationTimer.Start();
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
            animationTimer.Stop();
            progressIndicator.Stop();

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