using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Sales_Tracker.UI;
using System.Reflection;

namespace Sales_Tracker.Classes
{
    public static class NetSparkleUpdateManager
    {
        // Events for UI updates
        public static event EventHandler<UpdateCheckStartedEventArgs>? UpdateCheckStarted;
        public static event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;
        public static event EventHandler<UpdateDownloadStartedEventArgs>? UpdateDownloadStarted;
        public static event EventHandler<UpdateDownloadCompletedEventArgs>? UpdateDownloadCompleted;

        // Private static fields
        private static SparkleUpdater? _sparkle;
        private static bool _isChecking = false;
        private static bool _isUpdating = false;
        private static bool _updateAvailable = false;
        private static string? _availableVersion;
        private const string APP_CAST_URL = "https://argorobots.com/update.xml";

        // Public properties
        public static bool IsChecking => _isChecking;
        public static bool IsUpdating => _isUpdating;
        public static bool IsUpdateAvailable => _updateAvailable;
        public static string? AvailableVersion => _availableVersion ?? (_updateAvailable ? "Update Available" : null);

        // Init.
        public static void Initialize()
        {
            try
            {
                // Create Sparkle updater - using no signature checker for now
                // You can add signature verification later if needed
                _sparkle = new SparkleUpdater(
                    APP_CAST_URL,
                    new DSAChecker(SecurityMode.Unsafe) // Use unsafe mode for testing
                )
                {
                    // Configure for silent operation
                    UserInteractionMode = UserInteractionMode.NotSilent,
                    RelaunchAfterUpdate = false,
                    CustomInstallerArguments = "/quiet"  // Pass /quiet to Advanced Installer
                };

                // Subscribe to events with correct signatures
                _sparkle.UpdateCheckStarted += OnUpdateCheckStarted;
                _sparkle.UpdateCheckFinished += OnUpdateCheckFinished;
                _sparkle.DownloadStarted += OnDownloadStarted;
                _sparkle.DownloadFinished += OnDownloadFinished;
                _sparkle.DownloadHadError += OnDownloadHadError;

                Log.Write(2, "NetSparkleUpdateManager initialized successfully");
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
        public static async Task<bool> CheckForUpdatesAsync(bool showNoUpdateMessage = true)
        {
            if (_isChecking || _isUpdating || _sparkle == null)
            {
                Log.Write(1, $"Cannot check for updates: IsChecking={_isChecking}, IsUpdating={_isUpdating}, Sparkle={_sparkle != null}");
                return false;
            }

            try
            {
                _isChecking = true;

                Log.Write(2, $"Checking for updates from: {APP_CAST_URL}");
                Log.Write(2, $"Current version: {GetCurrentVersion()}");

                // NetSparkle handles the async checking, events will fire
                var updateInfo = await _sparkle.CheckForUpdatesQuietly();

                Log.Write(2, $"Update check result: {updateInfo?.Status}");

                // The events will handle setting _updateAvailable, but we can also check here
                if (updateInfo != null)
                {
                    Log.Write(2, $"Update info status: {updateInfo.Status}");
                    if (updateInfo.Updates != null && updateInfo.Updates.Count > 0)
                    {
                        Log.Write(2, $"Available updates count: {updateInfo.Updates.Count}");
                        foreach (var update in updateInfo.Updates)
                        {
                            Log.Write(2, $"Available version: {update.Version}");
                        }
                    }
                }

                return _updateAvailable;
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error checking for updates: {ex.Message}");
                Log.Write(0, $"Stack trace: {ex.StackTrace}");
                _isChecking = false;

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    Error = ex.Message,
                    ShowNoUpdateMessage = showNoUpdateMessage
                });

                return false;
            }
        }

        /// <summary>
        /// Check for updates in the background.
        /// </summary>
        public static void CheckForUpdates(bool showNoUpdateMessage = true)
        {
            Task.Run(async () =>
            {
                try
                {
                    await CheckForUpdatesAsync(showNoUpdateMessage);
                }
                catch (Exception ex)
                {
                    Log.Write(0, $"Background update check failed: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Start downloading and installing the available update.
        /// </summary>
        /// <returns>True if download started successfully</returns>
        public static async Task<bool> StartUpdateAsync()
        {
            if (!_updateAvailable || _isUpdating || _isChecking || _sparkle == null)
            {
                Log.Write(1, $"Cannot start update: UpdateAvailable={_updateAvailable}, IsUpdating={_isUpdating}, IsChecking={_isChecking}, Sparkle={_sparkle != null}");
                return false;
            }

            try
            {
                _isUpdating = true;

                Log.Write(2, "Starting update download and installation");

                // Start the download process
                // NetSparkle will handle downloading and running the installer with /quiet
                await _sparkle.CheckForUpdatesAtUserRequest();

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

                // For NetSparkle, the installer has already run silently
                // Just exit the application
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error applying update and restarting: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the current installed version
        /// </summary>
        public static string GetCurrentVersion()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Debug method to test update checking manually
        /// </summary>
        public static async Task<string> DebugUpdateCheck()
        {
            try
            {
                Log.Write(2, "=== DEBUG UPDATE CHECK ===");
                Log.Write(2, $"APP_CAST_URL: {APP_CAST_URL}");
                Log.Write(2, $"Current Version: {GetCurrentVersion()}");

                // Try to download the XML directly
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                string xmlContent = await client.GetStringAsync(APP_CAST_URL);
                Log.Write(2, $"XML Content Length: {xmlContent.Length}");
                Log.Write(2, $"XML Content: {xmlContent}");

                return xmlContent;
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Debug check failed: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        // NetSparkle event handlers with actual correct signatures
        private static void OnUpdateCheckStarted(object sender)
        {
            Log.Write(2, "Update check started");
            UpdateCheckStarted?.Invoke(null, new UpdateCheckStartedEventArgs());
        }
        private static void OnUpdateCheckFinished(object sender, UpdateStatus status)
        {
            _isChecking = false;

            Log.Write(2, $"Update check finished with status: {status}");

            if (status == UpdateStatus.UpdateAvailable)
            {
                _updateAvailable = true;
                Log.Write(2, "Update is available");

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = true,
                    AvailableVersion = _availableVersion ?? "New Version Available",
                    CurrentVersion = GetCurrentVersion(),
                    UpdateInfo = new UpdateInfoResult
                    {
                        CurrentVersion = GetCurrentVersion(),
                        AvailableVersion = _availableVersion ?? "New Version Available",
                        DownloadUrl = APP_CAST_URL
                    }
                });
            }
            else if (status == UpdateStatus.UpdateNotAvailable)
            {
                _updateAvailable = false;
                _availableVersion = null;
                Log.Write(2, "No update available");

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    ShowNoUpdateMessage = true,
                    CurrentVersion = GetCurrentVersion()
                });
            }
            else
            {
                // Handle other statuses (UserSkipped, CouldNotDetermine, etc.)
                _updateAvailable = false;
                _availableVersion = null;
                Log.Write(1, $"Update check completed with status: {status}");

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
                    ShowNoUpdateMessage = false,
                    CurrentVersion = GetCurrentVersion()
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
            Log.Write(2, $"Download finished for version: {item.Version}");

            UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
            {
                Success = true,
                RequiresRestart = true,
                IsSilentInstall = true  // NetSparkle runs installer with /quiet
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

    // Event argument classes - keeping same for compatibility
    public class UpdateCheckStartedEventArgs : EventArgs { }

    public class UpdateCheckCompletedEventArgs : EventArgs
    {
        public bool IsUpdateAvailable { get; set; }
        public string? AvailableVersion { get; set; }
        public string? CurrentVersion { get; set; }
        public string? Error { get; set; }
        public bool ShowNoUpdateMessage { get; set; } = true;
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

    // Update info class for external use
    public class UpdateInfoResult
    {
        public string? CurrentVersion { get; set; }
        public string? AvailableVersion { get; set; }
        public string? DownloadUrl { get; set; }
    }
}