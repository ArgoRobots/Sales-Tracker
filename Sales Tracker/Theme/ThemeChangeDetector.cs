using Microsoft.Win32;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Theme
{
    internal class ThemeChangeDetector
    {
        private static bool _isListeningForThemeChanges = false;
        private static RegistryKey _personalizeKey = null;
        private static RegistryWatcher _watcher;

        public static void StartListeningForThemeChanges()
        {
            if (_isListeningForThemeChanges)
            {
                return;
            }

            try
            {
                _personalizeKey = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    false);

                if (_personalizeKey != null)
                {
                    _personalizeKey = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                        true);

                    if (_personalizeKey != null)
                    {
                        // Create a registry watcher
                        _watcher = new RegistryWatcher(
                            RegistryHive.CurrentUser,
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                        _watcher.RegChanged += OnWindowsThemeChanged;
                        _watcher.Start();
                        _isListeningForThemeChanges = true;
                        Log.Write(1, "Started listening for Windows theme changes");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error starting theme listener: {ex.Message}");
            }
        }
        private static void OnWindowsThemeChanged(object sender, EventArgs e)
        {
            if (ThemeManager.CurrentTheme == ThemeManager.ThemeType.Windows)
            {
                CustomColors.SetColors();
                FormThemeManager.UpdateAllForms();
                ThemeManager.UpdateOtherControls();
            }
        }
        public static void StopListeningForThemeChanges()
        {
            if (_isListeningForThemeChanges)
            {
                Log.Write(1, "Stopped listening for Windows theme changes");
                _isListeningForThemeChanges = false;
                _personalizeKey?.Close();
                _watcher?.Stop();
            }
        }
    }
}
