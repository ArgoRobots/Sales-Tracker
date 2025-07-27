using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.UI;
using System.Diagnostics;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages application updates using the NetSparkle updater framework.
    /// Handles checking for updates, downloading, and silent installation.
    /// </summary>
    public static class NetSparkleUpdateManager
    {
        // Events for UI updates
        public static event EventHandler<UpdateCheckStartedEventArgs>? UpdateCheckStarted;
        public static event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;
        public static event EventHandler<UpdateDownloadStartedEventArgs>? UpdateDownloadStarted;
        public static event EventHandler<UpdateDownloadCompletedEventArgs>? UpdateDownloadCompleted;

        // Private static fields
        private static SparkleUpdater? _sparkle;
        private static bool _isUpdating = false;
        private static bool _updateAvailable = false;
        private static string? _availableVersion;
        private static string? _installerPath;
        private const string APP_CAST_URL = "https://argorobots.com/update.xml";

        // Installer arguments discovered by running: & ".\Argo Sales Tracker Installer V.1.0.4.exe" /?
        // in the directory where the exe is located.
        // These arguements are used for silent installation without user interaction.
        private const string SILENT_INSTALL_ARGUMENT = "/exenoui /norestart";

        // Public properties
        public static bool IsUpdating => _isUpdating;
        public static string? AvailableVersion
        {
            get
            {
                if (!_updateAvailable) { return null; }
                return !string.IsNullOrEmpty(_availableVersion) ? _availableVersion : "Unknown";
            }
        }

        // Init.
        public static void Initialize()
        {
            try
            {
                _sparkle = new SparkleUpdater(
                    APP_CAST_URL,
                    new DSAChecker(SecurityMode.UseIfPossible)
                )
                {
                    // Configure for silent operation
                    UserInteractionMode = UserInteractionMode.NotSilent,
                    RelaunchAfterUpdate = false,
                    CustomInstallerArguments = SILENT_INSTALL_ARGUMENT
                };

                // Subscribe to events with correct signatures
                _sparkle.UpdateCheckStarted += OnUpdateCheckStarted;
                _sparkle.UpdateCheckFinished += OnUpdateCheckFinished;
                _sparkle.DownloadStarted += OnDownloadStarted;
                _sparkle.DownloadFinished += OnDownloadFinished;
                _sparkle.DownloadHadError += OnDownloadHadError;
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error initializing NetSparkleUpdateManager: {ex.Message}");
                Log.Write(0, $"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Check for updates asynchronously.
        /// </summary>
        /// <returns>True if update is available, false otherwise</returns>
        public static async Task<bool> CheckForUpdatesAsync()
        {
            if (_isUpdating || _sparkle == null)
            {
                Log.Write(1, $"Cannot check for updates: IsUpdating={_isUpdating}, Sparkle={_sparkle != null}");
                return false;
            }

            try
            {
                // NetSparkle handles the async checking, events will fire
                UpdateInfo? updateInfo = await _sparkle.CheckForUpdatesQuietly();

                // The events will handle setting _updateAvailable
                return _updateAvailable;
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error checking for updates: {ex.Message}");
                Log.Write(0, $"Stack trace: {ex.StackTrace}");

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    Error = ex.Message
                });

                return false;
            }
        }

        /// <summary>
        /// Check for updates in the background and show Updates_Form if an update is available.
        /// </summary>
        public static void CheckForUpdates()
        {
            Task.Run(async () =>
            {
                bool updateAvailable = await CheckForUpdatesAsync();

                if (updateAvailable)
                {
                    // Use Invoke to ensure UI operations happen on the main thread
                    MainMenu_Form.Instance?.Invoke(() =>
                    {
                        Tools.OpenForm(new Updates_Form());
                    });
                }
            });
        }

        /// <summary>
        /// Start downloading and installing the available update.
        /// </summary>
        /// <returns>True if download started successfully</returns>
        public static async Task<bool> StartUpdateAsync()
        {
            if (!_updateAvailable || _isUpdating || _sparkle == null)
            {
                Log.Write(2, $"Cannot start update: UpdateAvailable={_updateAvailable}, IsUpdating={_isUpdating}, Sparkle={_sparkle != null}");
                return false;
            }

            try
            {
                _isUpdating = true;

                AppCastItem updateItem = _sparkle.LatestAppCastItems[0];
                Log.Write(2, $"Starting download for version {updateItem.Version}");

                await DownloadUpdate(updateItem);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error starting update: {ex.Message}");
                Log.Write(0, $"Stack trace: {ex.StackTrace}");
                _isUpdating = false;

                UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
                {
                    Success = false,
                    Error = ex.Message
                });

                return false;
            }
        }
        private static async Task DownloadUpdate(AppCastItem updateItem)
        {
            try
            {
                // Trigger download started event
                UpdateDownloadStarted?.Invoke(null, new UpdateDownloadStartedEventArgs(updateItem.Version ?? "Unknown"));

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromMinutes(10);  // 10 minute timeout for large files

                // Get the download URL
                string downloadUrl = updateItem.DownloadLink;
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new InvalidOperationException("No download URL available");
                }

                // Create temp directory
                string tempDir = Path.Combine(Path.GetTempPath(), "SalesTrackerUpdate");
                Directory.CreateDirectory(tempDir);

                string fileName = $"Argo Sales Tracker Installer V.{updateItem.Version}.exe";
                string filePath = Path.Combine(tempDir, fileName);

                // Download the file
                using (HttpResponseMessage response = await client.GetAsync(downloadUrl))
                {
                    response.EnsureSuccessStatusCode();

                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    Log.Write(2, $"Download completed. File size: {Tools.ConvertBytesToReadableSize(fileBytes.Length)}");
                }

                // Verify the file exists
                if (!File.Exists(filePath))
                {
                    Log.Write(0, $"Download failed: file does not exist");

                    CustomMessageBox.Show(
                    "Download Error",
                    "The update failed to download, please try again or contact support. You can also donwload it directly from agorobots.com",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
                }

                _installerPath = filePath;

                // Trigger download completed event
                UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
                {
                    Success = true,
                    RequiresRestart = true,
                    IsSilentInstall = true
                });
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Download failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Apply the downloaded update and restart the application.
        /// </summary>
        public static void ApplyUpdateAndRestart()
        {
            try
            {
                Log.Write(2, "Applying update and restarting application");

                // Save any pending work before restart
                CustomControls.SaveAll();

                // Set a flag to indicate we should auto-open the most recent company
                DataFileManager.SetValue(GlobalAppDataSettings.AutoOpenRecentAfterUpdate, bool.TrueString);

                if (!string.IsNullOrEmpty(_installerPath) && File.Exists(_installerPath))
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = _installerPath,
                        Arguments = SILENT_INSTALL_ARGUMENT,
                        UseShellExecute = true,
                        Verb = "runas",  // Run as administrator
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    Process installerProcess = Process.Start(startInfo);

                    if (installerProcess == null)
                    {
                        Log.Write(0, "Failed to start installer process - Process.Start returned null");

                        // Show error message to user
                        CustomMessageBox.Show(
                            "Update Error",
                            "Failed to start the installer process. Please run the installer manually from:\n" + _installerPath,
                            CustomMessageBoxIcon.Error,
                            CustomMessageBoxButtons.Ok);
                        return;
                    }

                    // Exit the current application immediately for silent install
                    Log.Write(2, "Exiting application to allow installer to complete");
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error applying update and restarting: {ex.Message}");

                // Show error to user
                CustomMessageBox.Show(
                    "Update Error",
                    $"An error occurred during the update process." +
                    "Please run the installer manually from:\n" + _installerPath,
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
        }

        // NetSparkle event handlers with actual correct signatures
        private static void OnUpdateCheckStarted(object sender)
        {
            UpdateCheckStarted?.Invoke(null, new UpdateCheckStartedEventArgs());
        }
        private static void OnUpdateCheckFinished(object sender, UpdateStatus status)
        {
            if (status == UpdateStatus.UpdateAvailable)
            {
                _updateAvailable = true;

                // Try to get the available version from the sparkle updater
                string? availableVersion = null;
                try
                {
                    if (_sparkle != null && _sparkle.LatestAppCastItems != null && _sparkle.LatestAppCastItems.Count > 0)
                    {
                        AppCastItem latestItem = _sparkle.LatestAppCastItems[0];
                        availableVersion = latestItem.Version;
                        _availableVersion = availableVersion;

                        string currentVersion = Tools.GetVersionNumber();

                        // Skip update if versions are identical
                        if (currentVersion == availableVersion)
                        {
                            _updateAvailable = false;
                            _availableVersion = null;

                            UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                            {
                                IsUpdateAvailable = false,
                                CurrentVersion = currentVersion
                            });
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(1, $"Could not extract version from AppCast: {ex.Message}");
                }

                Log.Write(2, "Update is available");

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = true,
                    AvailableVersion = availableVersion ?? _availableVersion ?? "New Version Available",
                    CurrentVersion = Tools.GetVersionNumber(),
                    UpdateInfo = new UpdateInfoResult
                    {
                        CurrentVersion = Tools.GetVersionNumber(),
                        AvailableVersion = availableVersion ?? _availableVersion ?? "New Version Available",
                        DownloadUrl = APP_CAST_URL
                    }
                });
            }
            else if (status == UpdateStatus.UpdateNotAvailable)
            {
                _updateAvailable = false;
                _availableVersion = null;

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    CurrentVersion = Tools.GetVersionNumber()
                });
            }
            else
            {
                // Handle other statuses (UserSkipped, CouldNotDetermine, etc.)
                _updateAvailable = false;
                _availableVersion = null;

                string errorMessage = status switch
                {
                    UpdateStatus.CouldNotDetermine => "Could not determine if an update is available",
                    UpdateStatus.UserSkipped => "Update check was skipped",
                    _ => $"Update check returned status: {status}"
                };

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    Error = errorMessage,
                    CurrentVersion = Tools.GetVersionNumber()
                });
            }
        }
        private static void OnDownloadStarted(AppCastItem item, string path)
        {
            _availableVersion = item.Version;
            Log.Write(2, $"Download started for version: {item.Version}");

            UpdateDownloadStarted?.Invoke(null, new UpdateDownloadStartedEventArgs(item.Version ?? "Unknown"));
        }
        private static void OnDownloadFinished(AppCastItem item, string path)
        {
            _isUpdating = false;
            _installerPath = path;  // Store the installer path from NetSparkle
            Log.Write(2, $"Download finished for version: {item.Version}, path: {path}");

            UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
            {
                Success = true,
                RequiresRestart = true,
                IsSilentInstall = true
            });
        }
        private static void OnDownloadHadError(AppCastItem item, string? path, Exception exception)
        {
            _isUpdating = false;
            Log.Write(0, $"Download error for version {item.Version}: {exception.Message}");

            UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
            {
                Success = false,
                Error = exception.Message
            });
        }
    }

    // Event argument classes
    public class UpdateCheckStartedEventArgs : EventArgs { }

    public class UpdateCheckCompletedEventArgs : EventArgs
    {
        public bool IsUpdateAvailable { get; set; }
        public string? AvailableVersion { get; set; }
        public string? CurrentVersion { get; set; }
        public string? Error { get; set; }
        public UpdateInfoResult? UpdateInfo { get; set; }
    }

    public class UpdateDownloadStartedEventArgs(string version) : EventArgs
    {
        public string Version { get; } = version;
    }

    public class UpdateDownloadCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public bool RequiresRestart { get; set; }
        public bool IsSilentInstall { get; set; } = false;
    }

    public class UpdateInfoResult
    {
        public string? CurrentVersion { get; set; }
        public string? AvailableVersion { get; set; }
        public string? DownloadUrl { get; set; }
    }
}