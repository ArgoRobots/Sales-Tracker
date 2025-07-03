using Sales_Tracker.Classes;
using System.Diagnostics;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Simplified chart performance monitoring that only logs essential chart timing.
    /// </summary>
    public static class ChartPerformanceMonitor
    {
        private static readonly Dictionary<string, ChartPerformanceData> _performanceData = [];
        private static readonly Dictionary<string, Stopwatch> _activeTimers = [];

        private class ChartPerformanceData
        {
            public string OperationName { get; set; }
            public long TotalExecutions { get; set; }
            public long TotalMilliseconds { get; set; }
            public long MinMilliseconds { get; set; } = long.MaxValue;
            public long MaxMilliseconds { get; set; }
            public double AverageMilliseconds => TotalExecutions > 0 ? (double)TotalMilliseconds / TotalExecutions : 0;
            public DateTime LastExecution { get; set; }
        }

        /// <summary>
        /// Times a chart operation and logs only if it's a main chart operation.
        /// </summary>
        public static IDisposable TimeChartOperation(string chartName, string chartType = "", int recordCount = 0)
        {
            if (!IsEnabled) { return new NoOpTimer(); }

            string operationKey = $"{chartType}_{chartName}_{Guid.NewGuid():N}";
            return new ChartTimer(operationKey, chartName, chartType, recordCount);
        }

        /// <summary>
        /// Times any operation but only logs if it takes longer than threshold.
        /// </summary>
        public static IDisposable TimeOperation(string operationName, string context = "", int thresholdMs = 100)
        {
            if (!IsEnabled) { return new NoOpTimer(); }

            string operationKey = $"{operationName}_{Guid.NewGuid():N}";
            return new SilentTimer(operationKey, operationName, context, thresholdMs);
        }

        /// <summary>
        /// Logs performance statistics for all monitored chart operations.
        /// </summary>
        public static void LogPerformanceStatistics()
        {
            if (!IsEnabled || _performanceData.Count == 0) { return; }

            Log.Write(2, "=== CHART PERFORMANCE SUMMARY ===");

            List<ChartPerformanceData> sortedData = _performanceData.Values
                .Where(d => d.OperationName.Contains("Chart"))
                .OrderByDescending(d => d.AverageMilliseconds)
                .ToList();

            foreach (ChartPerformanceData data in sortedData)
            {
                Log.Write(2, $"[CHART PERF] {data.OperationName}: {data.AverageMilliseconds:F0}ms avg ({data.TotalExecutions} times, max: {data.MaxMilliseconds}ms)");
            }
        }

        /// <summary>
        /// Enables or disables performance monitoring.
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        /// <summary>
        /// Gets whether performance monitoring is currently enabled.
        /// </summary>
        public static bool IsEnabled { get; private set; } = true;

        private static void UpdatePerformanceData(string operationName, long elapsedMs)
        {
            if (!_performanceData.TryGetValue(operationName, out ChartPerformanceData? data))
            {
                data = new ChartPerformanceData { OperationName = operationName };
                _performanceData[operationName] = data;
            }

            data.TotalExecutions++;
            data.TotalMilliseconds += elapsedMs;
            data.MinMilliseconds = Math.Min(data.MinMilliseconds, elapsedMs);
            data.MaxMilliseconds = Math.Max(data.MaxMilliseconds, elapsedMs);
            data.LastExecution = DateTime.Now;
        }

        /// <summary>
        /// Timer for chart operations - always logs.
        /// </summary>
        private class ChartTimer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _chartName;
            private readonly string _chartType;
            private readonly int _recordCount;
            private bool _disposed = false;

            public ChartTimer(string timerKey, string chartName, string chartType, int recordCount)
            {
                _chartName = chartName;
                _chartType = chartType;
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

                    string displayName = string.IsNullOrEmpty(_chartType) ? _chartName : $"{_chartType} ({_chartName})";

                    // Always log chart operations
                    string logMessage = $"[CHART] {displayName}: {elapsedMs}ms";
                    if (_recordCount > 0)
                    {
                        logMessage += $" ({_recordCount} records)";
                    }

                    Log.Write(1, logMessage);

                    UpdatePerformanceData(displayName, elapsedMs);
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Timer for non-chart operations - only logs if over threshold.
        /// </summary>
        private class SilentTimer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _operationName;
            private readonly string _context;
            private readonly int _thresholdMs;
            private bool _disposed = false;

            public SilentTimer(string timerKey, string operationName, string context, int thresholdMs)
            {
                _operationName = operationName;
                _context = context;
                _thresholdMs = thresholdMs;
                _stopwatch = Stopwatch.StartNew();
                _activeTimers[timerKey] = _stopwatch;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    long elapsedMs = _stopwatch.ElapsedMilliseconds;

                    // Only log if over threshold
                    if (elapsedMs >= _thresholdMs)
                    {
                        string displayName = string.IsNullOrEmpty(_context) ? _operationName : $"{_operationName} ({_context})";
                        Log.Write(2, $"[PERF] {displayName}: {elapsedMs}ms");
                        UpdatePerformanceData(displayName, elapsedMs);
                    }

                    _disposed = true;
                }
            }
        }

        private class NoOpTimer : IDisposable
        {
            public void Dispose() { }
        }
    }
}