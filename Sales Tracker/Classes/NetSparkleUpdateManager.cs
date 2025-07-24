using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
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
        private static bool _isChecking = false;
        private static bool _isUpdating = false;
        private static bool _updateAvailable = false;
        private static string? _availableVersion;
        private static string? _installerPath;
        private const string APP_CAST_URL = "https://argorobots.com/update.xml";

        // Installer argument discovered by running: & ".\Argo Sales Tracker Installer V.1.0.4.exe" /?
        // in the directory where the exe is located
        // /exenoui - launches the EXE setup without UI (silent installation)
        // This is the correct argument for installer generated with Advanced Installer
        private const string SILENT_INSTALL_ARGUMENT = "/exenoui";

        // Public properties
        public static bool IsChecking => _isChecking;
        public static bool IsUpdating => _isUpdating;
        public static bool IsUpdateAvailable => _updateAvailable;
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

                // NetSparkle handles the async checking, events will fire
                UpdateInfo? updateInfo = await _sparkle.CheckForUpdatesQuietly();

                // The events will handle setting _updateAvailable, but we can also check here
                if (updateInfo != null)
                {
                    if (updateInfo.Updates != null && updateInfo.Updates.Count > 0)
                    {
                        Log.Write(1, $"Available updates count: {updateInfo.Updates.Count}");
                        foreach (AppCastItem update in updateInfo.Updates)
                        {
                            Log.Write(1, $"Available version: {update.Version}");
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
                Log.Write(2, $"Cannot start update: UpdateAvailable={_updateAvailable}, IsUpdating={_isUpdating}, IsChecking={_isChecking}, Sparkle={_sparkle != null}");
                return false;
            }

            try
            {
                _isUpdating = true;

                AppCastItem updateItem = _sparkle.LatestAppCastItems[0];
                Log.Write(2, $"Starting download for version: {updateItem.Version}");

                await DownloadUpdateManually(updateItem);
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
        /// Manual download approach as fallback.
        /// </summary>
        private static async Task DownloadUpdateManually(AppCastItem updateItem)
        {
            try
            {
                Log.Write(2, $"Starting manual download from: {updateItem.DownloadLink}");

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

                // FORCE the correct filename based on version - don't rely on URL or headers
                string fileName = $"Argo Sales Tracker Installer V.{updateItem.Version}.exe";
                string filePath = Path.Combine(tempDir, fileName);

                Log.Write(2, $"Downloading to: {filePath}");
                Log.Write(2, $"Forced filename to: {fileName}");

                // Download the file
                using (var response = await client.GetAsync(downloadUrl))
                {
                    response.EnsureSuccessStatusCode();

                    // Log response headers for debugging
                    Log.Write(2, $"Response Content-Type: {response.Content.Headers.ContentType}");
                    if (response.Content.Headers.ContentDisposition != null)
                    {
                        Log.Write(2, $"Response Content-Disposition: {response.Content.Headers.ContentDisposition}");
                    }

                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    Log.Write(2, $"Download completed. File size: {fileBytes.Length} bytes");
                }

                // Verify the file exists and has the correct name
                if (!File.Exists(filePath))
                {
                    throw new InvalidOperationException($"Downloaded file not found at expected path: {filePath}");
                }

                Log.Write(2, $"File successfully saved as: {fileName}");

                // Store the installer path for later use
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
                Log.Write(0, $"Manual download failed: {ex.Message}");
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
                Log.Write(2, $"Installer path: {_installerPath}");

                // Save any pending work before restart
                CustomControls.SaveAll();

                // If we have a stored installer path, start it with the correct parameters
                if (!string.IsNullOrEmpty(_installerPath) && File.Exists(_installerPath))
                {
                    Log.Write(2, $"Starting installer: {_installerPath}");
                    Log.Write(2, $"File exists check: {File.Exists(_installerPath)}");
                    Log.Write(2, $"File size: {new FileInfo(_installerPath).Length} bytes");

                    ProcessStartInfo startInfo = new()
                    {
                        FileName = _installerPath,
                        Arguments = SILENT_INSTALL_ARGUMENT,
                        UseShellExecute = true,
                        Verb = "runas",  // Run as administrator
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    Log.Write(2, $"Process start info - FileName: {startInfo.FileName}");
                    Log.Write(2, $"Process start info - Arguments: {startInfo.Arguments}");
                    Log.Write(2, $"Process start info - UseShellExecute: {startInfo.UseShellExecute}");
                    Log.Write(2, $"Process start info - Verb: {startInfo.Verb}");

                    try
                    {
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

                        Log.Write(2, $"Installer process started with ID: {installerProcess.Id}");

                        // Exit the current application immediately for silent install
                        Log.Write(2, "Exiting application to allow installer to complete");
                        Application.Exit();
                    }
                    catch (Exception processEx)
                    {
                        Log.Write(0, $"Exception starting installer process: {processEx.Message}");
                        Log.Write(0, $"Process exception stack trace: {processEx.StackTrace}");

                        // Try without elevation as fallback
                        Log.Write(1, "Trying to start installer without elevation...");
                        try
                        {
                            ProcessStartInfo fallbackStartInfo = new()
                            {
                                FileName = _installerPath,
                                Arguments = SILENT_INSTALL_ARGUMENT,
                                UseShellExecute = true,
                                CreateNoWindow = false
                            };

                            Process fallbackProcess = Process.Start(fallbackStartInfo);
                            if (fallbackProcess != null)
                            {
                                Log.Write(2, "Installer started without elevation, exiting application");
                                Application.Exit();
                            }
                            else
                            {
                                CustomMessageBox.Show(
                                    "Update Error",
                                    "Failed to start installer even without elevation",
                                    CustomMessageBoxIcon.Error,
                                    CustomMessageBoxButtons.Ok);
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            Log.Write(0, $"Fallback installer start also failed: {fallbackEx.Message}");

                            // Show error message to user with manual instructions
                            CustomMessageBox.Show(
                                "Update Error",
                                $"Could not start the installer automatically.\n\n" +
                                $"Please run the installer manually:\n{_installerPath}\n\n" +
                                $"Error: {processEx.Message}",
                                CustomMessageBoxIcon.Error,
                                CustomMessageBoxButtons.Ok);
                        }
                    }
                }
                else
                {
                    string message = string.IsNullOrEmpty(_installerPath)
                        ? "No installer path found"
                        : $"Installer file not found: {_installerPath}";

                    Log.Write(1, message);

                    // Show error message to user
                    CustomMessageBox.Show(
                        "Update Error",
                        "The installer file could not be found. Please download the update manually from the website.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok);

                    // Fallback: try to restart the current application
                    try
                    {
                        ProcessStartInfo startInfo = new()
                        {
                            FileName = Application.ExecutablePath,
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                        Application.Exit();
                    }
                    catch (Exception restartEx)
                    {
                        Log.Write(0, $"Failed to restart current application: {restartEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error applying update and restarting: {ex.Message}");
                Log.Write(0, $"Stack trace: {ex.StackTrace}");

                // Show error to user
                CustomMessageBox.Show(
                    "Update Error",
                    $"An error occurred during the update process: {ex.Message}\n\n" +
                    $"Installer location: {_installerPath}\n\n" +
                    $"Please try running the installer manually.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
        }

        /// <summary>
        /// Get current update status for debugging.
        /// </summary>
        public static string GetUpdateStatus()
        {
            return $"IsChecking: {IsChecking}, IsUpdating: {IsUpdating}, " +
                   $"IsUpdateAvailable: {IsUpdateAvailable}, AvailableVersion: {AvailableVersion}, " +
                   $"CurrentVersion: {Tools.GetVersionNumber()}, InstallerPath: {_installerPath}";
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

                // Try to get the available version from the sparkle updater
                string? availableVersion = null;
                try
                {
                    if (_sparkle != null && _sparkle.LatestAppCastItems != null && _sparkle.LatestAppCastItems.Count > 0)
                    {
                        AppCastItem latestItem = _sparkle.LatestAppCastItems[0];
                        availableVersion = latestItem.Version;
                        _availableVersion = availableVersion;
                        Log.Write(2, $"Available version from AppCast: {availableVersion}");
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
                Log.Write(2, "No update available");

                UpdateCheckCompleted?.Invoke(null, new UpdateCheckCompletedEventArgs
                {
                    IsUpdateAvailable = false,
                    ShowNoUpdateMessage = true,
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
                    ShowNoUpdateMessage = false,
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

    public class UpdateInfoResult
    {
        public string? CurrentVersion { get; set; }
        public string? AvailableVersion { get; set; }
        public string? DownloadUrl { get; set; }
    }
}