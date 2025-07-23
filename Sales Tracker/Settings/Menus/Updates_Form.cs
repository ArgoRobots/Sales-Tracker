using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings.Menus
{
    public partial class Updates_Form : BaseForm
    {
        // Properties
        private readonly string _originalButtonText;

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
            // Subscribe to static events
            AutoUpdateManager.UpdateCheckStarted += OnUpdateCheckStarted;
            AutoUpdateManager.UpdateCheckCompleted += OnUpdateCheckCompleted;
            AutoUpdateManager.UpdateDownloadStarted += OnUpdateDownloadStarted;
            AutoUpdateManager.UpdateDownloadCompleted += OnUpdateDownloadCompleted;
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
            // Unsubscribe from static events
            AutoUpdateManager.UpdateCheckStarted -= OnUpdateCheckStarted;
            AutoUpdateManager.UpdateCheckCompleted -= OnUpdateCheckCompleted;
            AutoUpdateManager.UpdateDownloadStarted -= OnUpdateDownloadStarted;
            AutoUpdateManager.UpdateDownloadCompleted -= OnUpdateDownloadCompleted;
        }

        // Event handlers
        private async void CheckForUpdates_Button_Click(object sender, EventArgs e)
        {
            if (AutoUpdateManager.IsChecking || AutoUpdateManager.IsUpdating)
            {
                return;
            }

            if (AutoUpdateManager.IsUpdateAvailable && CheckForUpdates_Button.Text.Contains("update to"))
            {
                // User clicked the "update to V.x.x now" button
                await AutoUpdateManager.StartUpdateAsync();
            }
            else
            {
                // User clicked "Check for Updates"
                await AutoUpdateManager.CheckForUpdatesAsync();
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

            CheckForUpdates_Button.Text = "Checking for updates...";
            CheckForUpdates_Button.Enabled = false;
        }
        private void OnUpdateCheckCompleted(object sender, UpdateCheckCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateCheckCompleted(sender, e));
                return;
            }

            // Update the last check time regardless of result
            UpdateLastCheckTime();

            if (!string.IsNullOrEmpty(e.Error))
            {
                // Error occurred
                ResetButtonState();
                CustomMessageBox.Show(
                    "Update Check Failed",
                    $"Error checking for updates: {e.Error}",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.IsUpdateAvailable)
            {
                // Update is available
                CheckForUpdates_Button.Text = $"Update to v{e.AvailableVersion} now";
                CheckForUpdates_Button.Enabled = true;
                CheckForUpdates_Button.FillColor = CustomColors.AccentBlue;

                // Show notification
                CustomMessageBox.Show(
                    "Update Available",
                    $"A new version {e.AvailableVersion} is available.\nCurrent version: {e.CurrentVersion}\n\nClick the update button to download and install.",
                    CustomMessageBoxIcon.Info,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.ShowNoUpdateMessage)
            {
                // No update available
                ResetButtonState();
                CustomMessageBox.Show(
                    "No Updates",
                    "You are using the latest version.",
                    CustomMessageBoxIcon.Success,
                    CustomMessageBoxButtons.Ok);
            }
            else
            {
                ResetButtonState();
            }
        }
        private void OnUpdateDownloadStarted(object sender, UpdateDownloadStartedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateDownloadStarted(sender, e));
                return;
            }

            CheckForUpdates_Button.Text = "Updating...";
            CheckForUpdates_Button.Enabled = false;
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
                // Download failed
                ResetButtonState();
                CustomMessageBox.Show(
                    "Update Error",
                    $"Failed to download update: {e.Error ?? "Unknown error"}",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
            else if (e.RequiresRestart)
            {
                // Download completed - show restart dialog
                ShowUpdateCompleteDialog();
            }
            else
            {
                ResetButtonState();
            }
        }
        private void ShowUpdateCompleteDialog()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Update Ready",
                "The update has been downloaded and will finish installing the next time the program is opened.\n\nWould you like to restart the application now?",
                CustomMessageBoxIcon.Success,
                CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                // Restart now
                CustomControls.SaveAll();
                Application.Exit();
            }
            else
            {
                // Restart later
                ResetButtonState();
            }
        }
        private void UpdateButtonState()
        {
            if (AutoUpdateManager.IsUpdateAvailable)
            {
                CheckForUpdates_Button.Text = $"Update to v{AutoUpdateManager.AvailableVersion} now";
                CheckForUpdates_Button.Enabled = !AutoUpdateManager.IsChecking && !AutoUpdateManager.IsUpdating;

                UpToDate_Label.Visible = false;
            }
            else
            {
                ResetButtonState();
                UpToDate_Label.Visible = true;
            }
        }
        private void ResetButtonState()
        {
            CheckForUpdates_Button.Text = _originalButtonText;
            CheckForUpdates_Button.Enabled = true;
            CheckForUpdates_Button.FillColor = CustomColors.AccentBlue;
        }

        // Methods for handling last check label
        private void UpdateLastCheckTime()
        {
            try
            {
                // Store current time as last check time
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DataFileManager.SetValue(GlobalAppDataSettings.LastUpdateCheck, currentTime);

                // Update the label
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