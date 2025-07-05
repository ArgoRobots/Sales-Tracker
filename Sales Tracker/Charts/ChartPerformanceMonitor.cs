using Guna.Charts.WinForms;
using Sales_Tracker.Classes;
using System.Diagnostics;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Chart performance monitoring that logs chart timing if enabled.
    /// </summary>
    public static class ChartPerformanceMonitor
    {
        private static readonly Dictionary<string, Stopwatch> _activeTimers = [];

        /// <summary>
        /// Gets whether performance monitoring is currently enabled.
        /// </summary>
        public static bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// Enables or disables performance monitoring.
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }
        public static IDisposable TimeChartOperation(string chartName, int recordCount = 0)
        {
            if (!IsEnabled) { return new NoOpTimer(); }

            string operationKey = $"{chartName}_{Guid.NewGuid():N}";
            return new ChartTimer(operationKey, chartName, recordCount);
        }
        public static IDisposable TimeOperation(string operationName, string context = "")
        {
            if (!IsEnabled) { return new NoOpTimer(); }

            string operationKey = $"{operationName}_{Guid.NewGuid():N}";
            return new SilentTimer(operationKey, operationName, context);
        }

        /// <summary>
        /// Timer for chart operations.
        /// </summary>
        private class ChartTimer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _chartName;
            private readonly int _recordCount;
            private bool _disposed = false;

            public ChartTimer(string timerKey, string chartName, int recordCount)
            {
                _chartName = chartName;
                _recordCount = recordCount;
                _stopwatch = Stopwatch.StartNew();
                _activeTimers[timerKey] = _stopwatch;
            }
            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    long elapsedMs = _stopwatch.ElapsedMilliseconds;

                    string displayName = string.IsNullOrEmpty(_chartName) ? "Chart" : _chartName;

                    // Log chart operations
                    string logMessage = $"[CHART] {displayName}: {elapsedMs} ms";
                    if (_recordCount > 0)
                    {
                        logMessage += $" ({_recordCount} records)";
                    }

                    Log.Write(1, logMessage);

                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Timer for non-chart operations.
        /// </summary>
        private class SilentTimer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _operationName, _context;
            private bool _disposed = false;

            public SilentTimer(string timerKey, string operationName, string context)
            {
                _operationName = operationName;
                _context = context;
                _stopwatch = Stopwatch.StartNew();
                _activeTimers[timerKey] = _stopwatch;
            }
            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    long elapsedMs = _stopwatch.ElapsedMilliseconds;

                    string displayName = string.IsNullOrEmpty(_context) ? _operationName : $"{_operationName} ({_context})";
                    Log.Write(1, $"[PERF] {displayName}: {elapsedMs} ms");

                    _disposed = true;
                }
            }
        }
        private class NoOpTimer : IDisposable
        {
            public void Dispose() { }
        }
    }

    public static class ChartUpdateManager
    {
        public static void UpdateChartWithRendering(GunaChart chart, Action<GunaChart> updateAction = null)
        {
            // Execute the update action if provided
            updateAction?.Invoke(chart);

            if (ChartPerformanceMonitor.IsEnabled)
            {
                Stopwatch renderTimer = Stopwatch.StartNew();
                string chartName = chart.Name;

                // Track when rendering actually completes
                void OnApplicationIdle(object sender, EventArgs e)
                {
                    Application.Idle -= OnApplicationIdle;
                    renderTimer.Stop();
                    Log.Write(1, $"[CHART RENDER] {chartName}: {renderTimer.ElapsedMilliseconds} ms");
                }

                Application.Idle += OnApplicationIdle;
            }

            chart.Update();
        }
    }
}