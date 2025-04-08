using System.Text;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles license key validation and management
    /// </summary>
    public class LicenseManager
    {
        private const string ValidationUrl = "https://yourdomain.com/validate_license.php";
        private const string LicenseSettingsKey = "ArgoLicenseKey";
        public bool IsFullVersionEnabled { get; private set; }
        public string CurrentLicenseKey { get; private set; }

        // Constructor - loads license from settings and validates it
        public LicenseManager()
        {
            // Load license key from application settings
            CurrentLicenseKey = Properties.Settings.Default.LicenseKey ?? "";

            // Check if license is activated
            IsFullVersionEnabled = !string.IsNullOrEmpty(CurrentLicenseKey) &&
                                 Properties.Settings.Default.LicenseActivated;
        }

        /// <summary>
        /// Activates a license key
        /// </summary>
        /// <returns>True if activation successful, false otherwise</returns>
        public async Task<(bool Success, string Message)> ActivateLicenseAsync(string licenseKey)
        {
            try
            {
                // Format the license key (remove any extra whitespace, etc.)
                licenseKey = FormatLicenseKey(licenseKey);

                // Call the server to validate the license
                (bool Success, string Message) result = await ValidateLicenseWithServerAsync(licenseKey);

                if (result.Success)
                {
                    // Save license information
                    CurrentLicenseKey = licenseKey;
                    IsFullVersionEnabled = true;

                    // Save to application settings
                    Properties.Settings.Default.LicenseKey = licenseKey;
                    Properties.Settings.Default.LicenseActivated = true;
                    Properties.Settings.Default.Save();

                    return (true, "License activated successfully!");
                }
                else
                {
                    return (false, result.Message);
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats the license key (removes whitespace, ensures correct format)
        /// </summary>
        private static string FormatLicenseKey(string licenseKey)
        {
            return licenseKey.Replace(" ", "").Trim().ToUpper();
        }

        /// <summary>
        /// Validates the license key with the server
        /// </summary>
        private static async Task<(bool Success, string Message)> ValidateLicenseWithServerAsync(string licenseKey)
        {
            try
            {
                using HttpClient httpClient = new();
                // Create request data
                var requestData = new
                {
                    license_key = licenseKey
                };

                // Serialize to JSON
                string json = JsonSerializer.Serialize(requestData);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                // Send POST request to validation endpoint
                HttpResponseMessage response = await httpClient.PostAsync(ValidationUrl, content);

                // Read response
                string responseJson = await response.Content.ReadAsStringAsync();

                // Parse JSON response
                using JsonDocument document = JsonDocument.Parse(responseJson);
                JsonElement root = document.RootElement;

                bool success = root.GetProperty("success").GetBoolean();
                string message = root.GetProperty("message").GetString();

                return (success, message);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}. Please check your internet connection and try again.");
            }
        }
    }
}