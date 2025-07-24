using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : BaseForm
    {
        // Properties
        private readonly string _originalButtonText;
        private bool _updateReadyForRestart = false;

        // Init.
        public Updates_Form()
        {
            InitializeComponent();

            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            InitializeUpdateManager();
            _originalButtonText = CheckForUpdates_Button.Text;
        }
        private void InitializeUpdateManager()
        {
            // Subscribe to events
            NetSparkleUpdateManager.UpdateCheckStarted += OnUpdateCheckStarted;
            NetSparkleUpdateManager.UpdateCheckCompleted += OnUpdateCheckCompleted;
            NetSparkleUpdateManager.UpdateDownloadStarted += OnUpdateDownloadStarted;
            NetSparkleUpdateManager.UpdateDownloadCompleted += OnUpdateDownloadCompleted;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(CheckForUpdates_Button);
        }

        // Form event handlers
        private void Updates_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            UpdateButtonState();
            UpdateLastCheckLabel();
        }
        private void Updates_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from events
            NetSparkleUpdateManager.UpdateCheckStarted -= OnUpdateCheckStarted;
            NetSparkleUpdateManager.UpdateCheckCompleted -= OnUpdateCheckCompleted;
            NetSparkleUpdateManager.UpdateDownloadStarted -= OnUpdateDownloadStarted;
            NetSparkleUpdateManager.UpdateDownloadCompleted -= OnUpdateDownloadCompleted;
        }

        // Event handlers
        private async void CheckForUpdates_Button_Click(object sender, EventArgs e)
        {
            if (NetSparkleUpdateManager.IsChecking || NetSparkleUpdateManager.IsUpdating)
            {
                return;
            }

            if (_updateReadyForRestart)
            {
                // Update is ready, show restart options
                ShowRestartDialog();
                return;
            }

            if (NetSparkleUpdateManager.IsUpdateAvailable)
            {
                // User clicked the "update to V.x.x now" button
                Log.Write(2, "User clicked update button - starting update process");
                Log.Write(2, $"Update status: {NetSparkleUpdateManager.GetUpdateStatus()}");

                try
                {
                    bool started = await NetSparkleUpdateManager.StartUpdateAsync();
                    Log.Write(2, $"StartUpdateAsync returned: {started}");

                    if (!started)
                    {
                        CustomMessageBox.Show(
                            "Update Error",
                            "Failed to start the update process. Please try again or check the logs for more details.",
                            CustomMessageBoxIcon.Error,
                            CustomMessageBoxButtons.Ok);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(0, $"Error in CheckForUpdates_Button_Click: {ex.Message}");
                    CustomMessageBox.Show(
                        "Update Error",
                        $"An error occurred while starting the update: {ex.Message}",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok);
                }
            }
            else
            {
                // User clicked "Check for Updates"
                await NetSparkleUpdateManager.CheckForUpdatesAsync();
            }
        }

        // Update Manager Event Handlers
        private void OnUpdateCheckStarted(object sender, UpdateCheckStartedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateCheckStarted(sender, e));
                return;
            }

            CheckForUpdates_Button.Text = LanguageManager.TranslateString("Checking for updates...");
            CheckForUpdates_Button.Enabled = false;

            UpdateStatusLabel(LanguageManager.TranslateString("Checking for updates..."), CustomColors.Text);
        }
        private void OnUpdateCheckCompleted(object sender, UpdateCheckCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateCheckCompleted(sender, e));
                return;
            }

            UpdateLastCheckTime();

            if (!string.IsNullOrEmpty(e.Error))
            {
                // Error occurred
                ResetButtonState();
                UpdateStatusLabel("Error checking for updates", CustomColors.AccentRed);

                CustomMessageBox.Show(
                    "Update Check Failed",
                    $"Error checking for updates: {e.Error}",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.IsUpdateAvailable)
            {
                // Update is available
                SetUpdateButtonText(e.AvailableVersion);
                CheckForUpdates_Button.Enabled = true;
                CheckForUpdates_Button.FillColor = CustomColors.AccentBlue;

                // Update status label to show available version
                string statusText = string.IsNullOrEmpty(e.AvailableVersion)
                    ? "Update available"
                    : $"Version {e.AvailableVersion} is available";
                UpdateStatusLabel(statusText, CustomColors.AccentBlue);

                // Show notification
                string availableVersionText = string.IsNullOrEmpty(e.AvailableVersion)
                    ? "a new version"
                    : $"version {e.AvailableVersion}";

                CustomMessageBox.Show(
                    "Update Available",
                    $"Great news! {availableVersionText} is available.\n\n" +
                    $"Current version: {e.CurrentVersion}\n" +
                    $"Click the update button to download and install the latest version.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.ShowNoUpdateMessage)
            {
                // No update available
                ResetButtonState();
                UpdateStatusLabel($"Up to date (v{e.CurrentVersion})", CustomColors.AccentGreen);

                CustomMessageBox.Show(
                    "No Updates",
                    $"You are already using the latest version (v{e.CurrentVersion}).",
                    CustomMessageBoxIcon.Success,
                    CustomMessageBoxButtons.Ok);
            }
            else
            {
                ResetButtonState();
                UpdateStatusLabel($"Up to date (v{e.CurrentVersion})", CustomColors.AccentGreen);
            }
        }
        private void OnUpdateDownloadStarted(object sender, UpdateDownloadStartedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateDownloadStarted(sender, e));
                return;
            }

            CheckForUpdates_Button.Text = LanguageManager.TranslateString("Installing update...");
            CheckForUpdates_Button.Enabled = false;

            // Update status label
            string statusText = string.IsNullOrEmpty(e.Version)
                ? "Downloading update..."
                : $"Downloading V.{e.Version}...";
            UpdateStatusLabel(statusText, CustomColors.AccentBlue);
        }
        private void OnUpdateDownloadCompleted(object sender, UpdateDownloadCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateDownloadCompleted(sender, e));
                return;
            }

            if (!e.Success)
            {
                // Download/Installation failed
                ResetButtonState();
                UpdateStatusLabel("Update failed", CustomColors.AccentRed);

                CustomMessageBox.Show(
                    "Update Error",
                    $"Failed to install update: {e.Error ?? "Unknown error"}\n\n" +
                    $"Please try again later or contact support if the problem persists.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.RequiresRestart)
            {
                // Download and installation completed successfully
                HandleUpdateComplete();
            }
            else
            {
                ResetButtonState();
                UpdateStatusLabel("Update completed", CustomColors.AccentGreen);
            }
        }

        // Handle update completion
        private void HandleUpdateComplete()
        {
            _updateReadyForRestart = true;

            // Update button to show restart option
            CheckForUpdates_Button.Text = LanguageManager.TranslateString("Restart to apply update");
            CheckForUpdates_Button.Enabled = true;

            UpdateStatusLabel("Update ready - restart required", CustomColors.AccentGreen);

            // Show success message
            CustomMessageBox.Show(
                "Update Downloaded Successfully",
                "The update has been downloaded and is ready to install.\n\n" +
                "Click 'Restart to apply update', or restart the application manually when convenient.",
                CustomMessageBoxIcon.Success,
                CustomMessageBoxButtons.Ok);
        }
        private static void ShowRestartDialog()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Apply Update",
                "The update is ready to be applied.\n\n" +
                "Would you like to restart the application now to apply the update?\n" +
                "Your work will be automatically saved before restarting.",
                CustomMessageBoxIcon.Question,
                CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                // Apply update and restart
                NetSparkleUpdateManager.ApplyUpdateAndRestart();
            }
            // If No, just leave the button as "Restart to apply update"
        }
        private void UpdateButtonState()
        {
            if (_updateReadyForRestart)
            {
                CheckForUpdates_Button.Text = LanguageManager.TranslateString("Restart to apply update");
                CheckForUpdates_Button.Enabled = true;
                CheckForUpdates_Button.FillColor = CustomColors.AccentGreen;
                UpdateStatusLabel("Update ready - restart required", CustomColors.AccentGreen);
            }
            else if (NetSparkleUpdateManager.IsUpdateAvailable)
            {
                SetUpdateButtonText(NetSparkleUpdateManager.AvailableVersion);
                CheckForUpdates_Button.Enabled = !NetSparkleUpdateManager.IsChecking && !NetSparkleUpdateManager.IsUpdating;

                string statusText = string.IsNullOrEmpty(NetSparkleUpdateManager.AvailableVersion)
                    ? "Update available"
                    : $"Version {NetSparkleUpdateManager.AvailableVersion} is available";
                UpdateStatusLabel(statusText, CustomColors.AccentBlue);
            }
            else
            {
                ResetButtonState();
                UpdateStatusLabel($"Up to date (V.{Tools.GetVersionNumber()})", CustomColors.AccentGreen);
            }
        }
        private void ResetButtonState()
        {
            _updateReadyForRestart = false;
            CheckForUpdates_Button.Text = LanguageManager.TranslateString(_originalButtonText);
            CheckForUpdates_Button.Enabled = true;
            CheckForUpdates_Button.FillColor = CustomColors.AccentBlue;
        }
        private void SetUpdateButtonText(string? availableVersion)
        {
            if (string.IsNullOrEmpty(availableVersion) || availableVersion == "Update Available")
            {
                CheckForUpdates_Button.Text = LanguageManager.TranslateString("Download update");
            }
            else
            {
                CheckForUpdates_Button.Text = LanguageManager.TranslateString("Update to V.") + availableVersion;
            }
        }
        private void UpdateStatusLabel(string text, Color color)
        {
            Status_Label.Text = LanguageManager.TranslateString(text);
            Status_Label.ForeColor = color;
            Status_Label.Left = (Width - Status_Label.Width) / 2;
        }

        // Methods for handling last check label
        private void UpdateLastCheckTime()
        {
            try
            {
                // Store current time as last check time
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DataFileManager.SetValue(GlobalAppDataSettings.LastUpdateCheck, currentTime);

                UpdateLastCheckLabel();
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error updating last check time: {ex.Message}");
            }
        }
        private void UpdateLastCheckLabel()
        {
            try
            {
                // Get the last check time from settings
                string? lastCheckTime = DataFileManager.GetValue(GlobalAppDataSettings.LastUpdateCheck);

                if (!string.IsNullOrEmpty(lastCheckTime) && DateTime.TryParse(lastCheckTime, out DateTime lastCheck))
                {
                    // Format the time for display
                    string displayText = FormatLastCheckTime(lastCheck);

                    if (LastCheck_Label != null)
                    {
                        UpdateLastCheckLabelText(LanguageManager.TranslateString("Last checked") + ": " + displayText);
                    }
                }
                else
                {
                    // No previous check time found
                    if (LastCheck_Label != null)
                    {
                        UpdateLastCheckLabelText(LanguageManager.TranslateString("Last checked: Never"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error updating last check label: {ex.Message}");
                if (LastCheck_Label != null)
                {
                    UpdateLastCheckLabelText(LanguageManager.TranslateString("Last checked: Unknown"));
                }
            }
        }
        private static string FormatLastCheckTime(DateTime lastCheck)
        {
            DateTime now = DateTime.Now;
            TimeSpan difference = now - lastCheck;

            // If it's today, show "Today, HH:mm AM/PM"
            if (lastCheck.Date == now.Date)
            {
                string today = LanguageManager.TranslateString("Today");
                return $"{today}, {lastCheck:h:mm tt}";
            }
            // If it's yesterday, show "Yesterday, HH:mm AM/PM"
            else if (lastCheck.Date == now.Date.AddDays(-1))
            {
                string yesterday = LanguageManager.TranslateString("Yesterday");
                return $"{yesterday}, {lastCheck:h:mm tt}";
            }
            // If it's within the last week, show day name and time
            else if (difference.TotalDays < 7)
            {
                return $"{lastCheck:dddd, h:mm tt}";
            }
            // If it's within the current year, show month/day and time
            else if (lastCheck.Year == now.Year)
            {
                return $"{lastCheck:MMM d, h:mm tt}";
            }
            // Otherwise show full date and time
            else
            {
                return $"{lastCheck:MMM d, yyyy h:mm tt}";
            }
        }
        private void UpdateLastCheckLabelText(string text)
        {
            LastCheck_Label.Text = text;
            LastCheck_Label.Left = (Width - LastCheck_Label.Width) / 2;
        }
    }
}