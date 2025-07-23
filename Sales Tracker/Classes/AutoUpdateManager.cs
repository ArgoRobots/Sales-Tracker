using AutoUpdaterDotNET;

namespace Sales_Tracker.Classes
{
    public static class AutoUpdateManager
    {
        // Events for UI updates
        public static event EventHandler<UpdateCheckStartedEventArgs>? UpdateCheckStarted;
        public static event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;
        public static event EventHandler<UpdateDownloadStartedEventArgs>? UpdateDownloadStarted;
        public static event EventHandler<UpdateDownloadCompletedEventArgs>? UpdateDownloadCompleted;

        // Private static fields
        private static UpdateInfoEventArgs? _updateInfo;
        private static bool _isChecking = false;
        private static bool _isUpdating = false;
        private const string UPDATE_XML_URL = "https://argorobots.com/update.xml";

        // Public properties
        public static bool IsChecking => _isChecking;
        public static bool IsUpdating => _isUpdating;
        public static bool IsUpdateAvailable => _updateInfo != null;
        public static string? AvailableVersion => _updateInfo?.CurrentVersion?.ToString();

        // Init.
        public static void Initialize()
        {
            // Configure AutoUpdater settings
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.Mandatory = false;
            AutoUpdater.UpdateMode = Mode.Normal;
            AutoUpdater.ReportErrors = true;

            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;

            // Set icon if available
            try
            {
                AutoUpdater.Icon = Properties.Resources.ArgoColor.ToBitmap();
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error setting AutoUpdater icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Check for updates asynchronously.
        /// </summary>
        /// <returns>True if update is available, false otherwise</returns>
        public static async Task<bool> CheckForUpdatesAsync(bool showNoUpdateMessage = true)
        {
            if (_isChecking || _isUpdating)
            {
                return false;
            }

            try
            {
                _isChecking = true;
                UpdateCheckStarted?.Invoke(null, new UpdateCheckStartedEventArgs());

                // Check for updates
                TaskCompletionSource<bool> tcs = new();

                // Set up one-time event handler to complete the task
                void Handler(object? s, UpdateCheckCompletedEventArgs e)
                {
                    UpdateCheckCompleted -= Handler;
                    tcs.SetResult(e.IsUpdateAvailable);
                }

                UpdateCheckCompleted += Handler;

                // Start the check
                await Task.Run(() =>
                {
                    AutoUpdater.Start(UPDATE_XML_URL);
                });

                // Wait for completion
                bool result = await tcs.Task;
                return result;
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error checking for updates: {ex.Message}");
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
            // Start the check in the background, don't wait for result
            Task.Run(async () =>
            {
                try
                {
                    await CheckForUpdatesAsync(showNoUpdateMessage);
                }
                catch (Exception ex)
                {
                    Log.Error_WriteToFile($"Background update check failed: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Start downloading and installing the available update
        /// </summary>
        /// <returns>True if download started successfully</returns>
        public static async Task<bool> StartUpdateAsync()
        {
            if (_updateInfo == null || _isUpdating || _isChecking)
            {
                return false;
            }

            try
            {
                _isUpdating = true;
                string versionString = _updateInfo.CurrentVersion?.ToString() ?? "Unknown";
                UpdateDownloadStarted?.Invoke(null, new UpdateDownloadStartedEventArgs(versionString));

                // Start the download and installation process
                bool success = await Task.Run(() =>
                {
                    try
                    {
                        return AutoUpdater.DownloadUpdate(_updateInfo);
                    }
                    catch (Exception ex)
                    {
                        Log.Error_WriteToFile($"Error downloading update: {ex.Message}");
                        return false;
                    }
                });

                if (!success)
                {
                    _isUpdating = false;
                    UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
                    {
                        Success = false,
                        Error = "Failed to start download"
                    });
                }

                return success;
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error starting update: {ex.Message}");
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
        /// Get update information if available.
        /// </summary>
        public static UpdateInfo? GetUpdateInfo()
        {
            if (_updateInfo == null)
            {
                return null;
            }

            return new UpdateInfo
            {
                CurrentVersion = _updateInfo.CurrentVersion?.ToString() ?? "Unknown",
                InstalledVersion = _updateInfo.InstalledVersion?.ToString() ?? "Unknown",
                ChangelogURL = _updateInfo.ChangelogURL,
                DownloadURL = _updateInfo.DownloadURL
            };
        }


        // AutoUpdater event handlers
        private static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            _isChecking = false;

            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    _updateInfo = args;

                    UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                    {
                        IsUpdateAvailable = true,
                        AvailableVersion = args.CurrentVersion?.ToString() ?? "Unknown",
                        CurrentVersion = args.InstalledVersion?.ToString() ?? "Unknown",
                        UpdateInfo = GetUpdateInfo()
                    });
                }
                else
                {
                    _updateInfo = null;

                    UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                    {
                        IsUpdateAvailable = false,
                        ShowNoUpdateMessage = true
                    });
                }
            }
            else
            {
                _updateInfo = null;

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    Error = args.Error.Message
                });
            }
        }
        private static void AutoUpdater_ApplicationExitEvent()
        {
            _isUpdating = false;

            UpdateDownloadCompleted?.Invoke(null, new UpdateDownloadCompletedEventArgs
            {
                Success = true,
                RequiresRestart = true
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
        public bool ShowNoUpdateMessage { get; set; } = true;
        public UpdateInfo? UpdateInfo { get; set; }
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
    }

    // Update info class for external use
    public class UpdateInfo
    {
        public string? CurrentVersion { get; set; }
        public string? InstalledVersion { get; set; }
        public string? ChangelogURL { get; set; }
        public string? DownloadURL { get; set; }
    }
}