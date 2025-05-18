using Microsoft.Win32;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Theme
{
    /// <summary>
    /// Detect Windows theme changes.
    /// </summary>
    internal class RegistryWatcher(RegistryHive hive, string keyPath)
    {
        private readonly RegistryHive _hive = hive;
        private readonly string _keyPath = keyPath;
        private readonly ManualResetEvent _stopEvent = new(false);
        private Thread _thread;

        public event EventHandler RegChanged;

        public void Start()
        {
            _stopEvent.Reset();
            _thread = new Thread(WatchForChanges)
            {
                IsBackground = true,
                Name = "RegistryWatcherThread"
            };
            _thread.Start();
        }
        public void Stop()
        {
            _stopEvent.Set();
            _thread?.Join(1000);
        }
        private void WatchForChanges()
        {
            object previousValue = null;
            bool firstRun = true;

            while (!_stopEvent.WaitOne(1000))
            {
                try
                {
                    using RegistryKey key = RegistryKey.OpenBaseKey(_hive, RegistryView.Default).OpenSubKey(_keyPath, false);
                    if (key == null) { continue; }

                    // Read the specific value we care about
                    object currentValue = key.GetValue("AppsUseLightTheme");

                    // Only trigger the event if the value has changed or this is the first check
                    if (firstRun || !Equals(currentValue, previousValue))
                    {
                        firstRun = false;
                        previousValue = currentValue;
                        RegChanged?.Invoke(this, EventArgs.Empty);
                        Log.Write(1, $"Windows theme changed to: {(currentValue?.ToString() == "0" ? "Dark" : "Light")}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(0, $"Registry watcher specific error: {ex.Message}");
                }
            }
        }
    }
}