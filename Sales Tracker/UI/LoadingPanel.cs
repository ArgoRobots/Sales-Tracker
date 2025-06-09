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
        private static Panel animationPanel;
        private static Guna2Button cancelButton;
        private static CancellationTokenSource cancellationTokenSource;
        private static Control currentControl;

        // Events
        public static event EventHandler CancelRequested;

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

            // Create a separate panel for the animation to run on its own thread
            animationPanel = new Panel
            {
                BackColor = Color.Transparent,
                Size = new Size(100, 100)
            };

            progressIndicator = new Guna2WinProgressIndicator
            {
                ProgressColor = CustomColors.AccentBlue,
                AnimationSpeed = 80
            };

            messageLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Text = "Loading...",
                AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create cancel button
            cancelButton = new Guna2Button
            {
                Text = "Cancel",
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };
            cancelButton.Click += CancelButton_Click;
            ThemeManager.MakeGButtonBlueSecondary(cancelButton);

            // Create a timer for smooth animation
            animationTimer = new Timer
            {
                Interval = 100,
                Enabled = false
            };
            animationTimer.Tick += AnimationTimer_Tick;

            animationPanel.Controls.Add(progressIndicator);
            LoadingPanelInstance.Controls.Add(animationPanel);
            LoadingPanelInstance.Controls.Add(messageLabel);
            LoadingPanelInstance.Controls.Add(cancelButton);
        }
        private static void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (progressIndicator != null && progressIndicator.IsHandleCreated)
            {
                progressIndicator.BeginInvoke(new Action(() =>
                {
                    progressIndicator.Invalidate();
                    progressIndicator.Update();
                }));
            }
        }
        private static void CancelButton_Click(object sender, EventArgs e)
        {
            // Cancel the operation
            cancellationTokenSource?.Cancel();

            // Hide the loading panel from the current control
            if (currentControl != null)
            {
                HideLoadingScreen(currentControl);
            }

            // Fire the cancel requested event for any additional handling
            CancelRequested?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Displays a loading screen with a progress indicator and a message over a specified control.
        /// </summary>
        public static void ShowLoadingScreen(Control control, string message, bool showCancelButton = false, CancellationTokenSource cancellationTokenSource = null)
        {
            string translatedMessage = LanguageManager.TranslateString(message);
            LoadingPanel.cancellationTokenSource = cancellationTokenSource;
            currentControl = control; // Store the current control

            LoadingPanelInstance.Size = control.Size;
            control.Controls.Add(LoadingPanelInstance);
            LoadingPanelInstance.Dock = DockStyle.Fill;

            // Position the animation panel
            animationPanel.Location = new Point(
                (LoadingPanelInstance.Width - animationPanel.Width) / 2,
                (LoadingPanelInstance.Height - animationPanel.Height) / 2
            );

            progressIndicator.Location = new Point(
                (animationPanel.Width - progressIndicator.Width) / 2,
                (animationPanel.Height - progressIndicator.Height) / 2
            );

            messageLabel.Text = translatedMessage;
            messageLabel.Location = new Point(
                (LoadingPanelInstance.Width - messageLabel.Width) / 2,
                animationPanel.Top - 70
            );
            messageLabel.ForeColor = CustomColors.Text;

            cancelButton.Visible = showCancelButton;
            if (showCancelButton)
            {
                cancelButton.Location = new Point(
                    (LoadingPanelInstance.Width - cancelButton.Width) / 2,
                    animationPanel.Bottom + 80
                );
            }

            LoadingPanelInstance.BringToFront();

            // Start the progress indicator animation
            progressIndicator.Start();
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
            currentControl = null;

            control.InvokeIfRequired(() =>
            {
                control.Controls.Remove(LoadingPanelInstance);
            });
        }
        public static void HideBlankLoadingPanel(Control control)
        {
            control.InvokeIfRequired(() =>
            {
                control.Controls.Remove(BlankLoadingPanelInstance);
            });
        }
        public static void UpdateTheme()
        {
            LoadingPanelInstance.BackColor = CustomColors.MainBackground;
            BlankLoadingPanelInstance.BackColor = CustomColors.MainBackground;
            progressIndicator.ProgressColor = CustomColors.AccentBlue;
            messageLabel.ForeColor = CustomColors.Text;
        }
    }
}