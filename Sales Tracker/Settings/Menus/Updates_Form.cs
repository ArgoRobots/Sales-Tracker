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
            _originalButtonText = Updates_Button.Text;
        }
        private void InitializeUpdateManager()
        {
            // Subscribe to events
            NetSparkleUpdateManager.UpdateDownloadStarted += OnUpdateDownloadStarted;
            NetSparkleUpdateManager.UpdateDownloadCompleted += OnUpdateDownloadCompleted;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Updates_Button);
        }

        // Form event handlers
        private void Updates_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            UpdateButtonState();
        }
        private void Updates_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from events
            NetSparkleUpdateManager.UpdateDownloadStarted -= OnUpdateDownloadStarted;
            NetSparkleUpdateManager.UpdateDownloadCompleted -= OnUpdateDownloadCompleted;
        }

        // Event handlers
        private async void Update_Button_Click(object sender, EventArgs e)
        {
            if (NetSparkleUpdateManager.IsChecking)
            {
                return;
            }

            if (_updateReadyForRestart)
            {
                NetSparkleUpdateManager.ApplyUpdateAndRestart();
                return;
            }

            bool started = await NetSparkleUpdateManager.StartUpdateAsync();

            if (!started)
            {
                CustomMessageBox.Show(
                    "Update Error",
                    "Failed to start the update process. Please try again or check the logs for more details.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
        }

        // Update Manager Event Handlers
        private void OnUpdateDownloadStarted(object sender, UpdateDownloadStartedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnUpdateDownloadStarted(sender, e));
                return;
            }

            Updates_Button.Text = LanguageManager.TranslateString("Installing update...");
            Updates_Button.Enabled = false;

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
            Updates_Button.Text = LanguageManager.TranslateString("Restart to apply update");
            Updates_Button.Enabled = true;

            UpdateStatusLabel("Update ready - restart required", CustomColors.AccentGreen);

            // Show success message
            CustomMessageBox.Show(
                "Update Downloaded Successfully",
                "The update has been downloaded and is ready to install.\n\n" +
                "Click 'Restart to apply update', or restart the application manually when convenient.",
                CustomMessageBoxIcon.Success,
                CustomMessageBoxButtons.Ok);
        }
        private void UpdateButtonState()
        {
            if (_updateReadyForRestart)
            {
                Updates_Button.Text = LanguageManager.TranslateString("Restart to apply update");
                Updates_Button.Enabled = true;
                Updates_Button.FillColor = CustomColors.AccentGreen;
                UpdateStatusLabel("Update ready - restart required", CustomColors.AccentGreen);
            }
            else
            {
                SetUpdateButtonText(NetSparkleUpdateManager.AvailableVersion);
                Updates_Button.Enabled = !NetSparkleUpdateManager.IsChecking && !NetSparkleUpdateManager.IsUpdating;

                string statusText = string.IsNullOrEmpty(NetSparkleUpdateManager.AvailableVersion)
                    ? "Update available"
                    : $"Version {NetSparkleUpdateManager.AvailableVersion} is available";
                UpdateStatusLabel(statusText, CustomColors.AccentBlue);
            }
        }
        private void ResetButtonState()
        {
            _updateReadyForRestart = false;
            Updates_Button.Text = LanguageManager.TranslateString(_originalButtonText);
            Updates_Button.Enabled = true;
            Updates_Button.FillColor = CustomColors.AccentBlue;
        }
        private void SetUpdateButtonText(string? availableVersion)
        {
            if (string.IsNullOrEmpty(availableVersion) || availableVersion == "Update Available")
            {
                Updates_Button.Text = LanguageManager.TranslateString("Download update");
            }
            else
            {
                Updates_Button.Text = LanguageManager.TranslateString("Update to V.") + availableVersion;
            }
        }
        private void UpdateStatusLabel(string text, Color color)
        {
            Status_Label.Text = LanguageManager.TranslateString(text);
            Status_Label.ForeColor = color;
            Status_Label.Left = (Width - Status_Label.Width) / 2;
        }
    }
}