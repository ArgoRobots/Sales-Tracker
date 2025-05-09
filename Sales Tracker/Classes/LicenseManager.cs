using System.Text;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles license key validation and management.
    /// </summary>
    public class LicenseManager
    {
        private const string ValidationUrl = "https://argorobots.com/validate_license.php";
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public LicenseManager()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Validates a license key with the server.
        /// </summary>
        /// <returns>True if the license is valid, false otherwise</returns>
        public async Task<bool> ValidateKeyAsync(string licenseKey = null)
        {
            licenseKey ??= Properties.Settings.Default.LicenseKey;
            if (licenseKey == "")
            {
                return false;
            }

            try
            {
                // Check if we have internet connectivity
                if (!IsInternetAvailable())
                {
                    // If no internet, check the saved license status
                    return Properties.Settings.Default.LicenseActivated;
                }

                // Prepare the content to send
                StringContent content = new(
                    JsonSerializer.Serialize(new { license_key = licenseKey }),
                    Encoding.UTF8,
                    "application/json");

                // Send the request to the validation endpoint
                HttpResponseMessage response = await _httpClient.PostAsync(ValidationUrl, content);

                // Read the response and parse it
                string jsonResponse = await response.Content.ReadAsStringAsync();
                LicenseResponse? result = JsonSerializer.Deserialize<LicenseResponse>(jsonResponse, _jsonOptions);

                // Check if the license is valid
                if (result != null && result.Success)
                {
                    // Set license to valid
                    Properties.Settings.Default.LicenseActivated = true;
                    Properties.Settings.Default.LicenseKey = licenseKey;
                    Properties.Settings.Default.Save();

                    // If the license is already activated on another device
                    if (result.Activated && !string.IsNullOrEmpty(result.ActivationDate))
                    {
                        ShowAlreadyActivatedMessage(result.ActivationDate);
                    }

                    return true;
                }
                else
                {
                    // Set license to invalid
                    Properties.Settings.Default.LicenseActivated = false;
                    Properties.Settings.Default.Save();

                    if (result != null && !string.IsNullOrEmpty(result.Message))
                    {
                        CustomMessageBox.Show("License Error", result.Message,
                            CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                CustomMessageBox.Show("License Error", $"Error validating license: {ex.Message}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);

                // If we can't validate, default to the stored setting
                return Properties.Settings.Default.LicenseActivated;
            }
        }

        /// <summary>
        /// Checks if the application can connect to the internet.
        /// </summary>
        /// <returns>True if internet is available, false otherwise</returns>
        private static bool IsInternetAvailable()
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                HttpResponseMessage response = client.GetAsync("https://argorobots.com").Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shows a message if the license is already activated on another device.
        /// </summary>
        private static void ShowAlreadyActivatedMessage(string activationDate)
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("License Already Active",
            $"This license key appears to be in use on another device (activated on {activationDate}) Would you like to activate it on this computer instead? This will deactivate the license on the other device.",
                 CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                // The server already activated the license for this device 
                CustomMessageBox.Show("Activation Successful",
                    "License has been successfully transferred to this device.",
                    CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
            }
            else
            {
                // Reset
                Properties.Settings.Default.LicenseActivated = false;
                Properties.Settings.Default.LicenseKey = "";
                Properties.Settings.Default.Save();

                CustomMessageBox.Show("Activation Canceled",
                    "License will remain active on the original device.",
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
        }

        /// <summary>
        /// Class to deserialize the license validation response.
        /// </summary>
        private class LicenseResponse
        {
            public bool Success { get; set; }
            public bool Activated { get; set; }
            public string Message { get; set; }
            public string ActivationDate { get; set; }
        }
    }
}