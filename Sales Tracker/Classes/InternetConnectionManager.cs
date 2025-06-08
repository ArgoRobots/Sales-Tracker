using System.Net.NetworkInformation;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Utility class for checking internet connectivity and displaying appropriate messages.
    /// </summary>
    public static class InternetConnectionManager
    {
        private static readonly string[] testHosts = [
            "8.8.8.8",        // Google DNS
            "1.1.1.1",        // Cloudflare DNS  
            "208.67.222.222"  // OpenDNS
        ];

        /// <summary>
        /// Checks if internet connection is available by pinging reliable hosts.
        /// </summary>
        private static async Task<bool> IsInternetAvailableAsync(int timeoutMs = 3000)
        {
            try
            {
                // First check network availability
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    return false;
                }

                // Test multiple hosts for reliability
                IEnumerable<Task<bool>> pingTasks = testHosts.Select(host => PingHostAsync(host, timeoutMs));
                bool[] results = await Task.WhenAll(pingTasks);

                // Return true if at least one ping succeeds
                return results.Any(success => success);
            }
            catch (Exception ex)
            {
                Log.Write(1, $"Internet connectivity check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pings a specific host to test connectivity.
        /// </summary>
        private static async Task<bool> PingHostAsync(string host, int timeoutMs)
        {
            try
            {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(host, timeoutMs);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shows a message box when no internet connection is detected.
        /// </summary>
        private static CustomMessageBoxResult ShowNoInternetMessage(string operationName = "this operation", bool showRetryOption = true)
        {
            string title = "No Internet Connection";
            string message = $"An internet connection is required for {operationName}. Please check your connection and try again";

            CustomMessageBoxButtons buttons = showRetryOption
                ? CustomMessageBoxButtons.RetryCancel
                : CustomMessageBoxButtons.Ok;

            return CustomMessageBox.Show(title, message, CustomMessageBoxIcon.Exclamation, buttons);
        }

        /// <summary>
        /// Checks internet connectivity and shows appropriate message if unavailable.
        /// </summary>
        public static async Task<bool> CheckInternetAndShowMessageAsync(string operationName = "this operation", bool showRetryOption = true)
        {
            while (true)
            {
                if (await IsInternetAvailableAsync())
                {
                    return true;
                }

                CustomMessageBoxResult result = ShowNoInternetMessage(operationName, showRetryOption);

                if (result != CustomMessageBoxResult.Retry)
                {
                    return false;
                }
            }
        }
    }
}