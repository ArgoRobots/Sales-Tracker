using Google.Apis.Auth.OAuth2;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public class GoogleCredentialsManager
    {
        /// <summary>
        /// Gets Google credentials from environment variables with the appropriate scopes
        /// </summary>
        public static GoogleCredential GetCredentialsFromEnvironment()
        {
            try
            {
                string projectId = DotEnv.Get("GOOGLE_PROJECT_ID");
                string privateKey = DotEnv.Get("GOOGLE_PRIVATE_KEY")?.Replace("\\n", "\n");
                string clientEmail = DotEnv.Get("GOOGLE_CLIENT_EMAIL");

                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(clientEmail))
                {
                    throw new Exception("Missing required Google credentials in .env file");
                }

                // Create credentials JSON from environment variables
                var credentialJson = new
                {
                    type = DotEnv.Get("GOOGLE_TYPE"),
                    project_id = projectId,
                    private_key_id = DotEnv.Get("GOOGLE_PRIVATE_KEY_ID"),
                    private_key = privateKey,
                    client_email = clientEmail,
                    client_id = DotEnv.Get("GOOGLE_CLIENT_ID"),
                    auth_uri = DotEnv.Get("GOOGLE_AUTH_URI"),
                    token_uri = DotEnv.Get("GOOGLE_TOKEN_URI"),
                    auth_provider_x509_cert_url = DotEnv.Get("GOOGLE_AUTH_PROVIDER_CERT_URL"),
                    client_x509_cert_url = DotEnv.Get("GOOGLE_CLIENT_CERT_URL"),
                    universe_domain = DotEnv.Get("GOOGLE_UNIVERSE_DOMAIN")
                };

                // Convert to JSON string
                string json = JsonSerializer.Serialize(credentialJson);

                // Create credential from JSON string with only needed scopes for Sheets and Drive
                GoogleCredential credential = GoogleCredential.FromJson(json)
                    .CreateScoped(
                        [
                            "https://www.googleapis.com/auth/spreadsheets",
                            "https://www.googleapis.com/auth/drive"
                        ]
                    );

                return credential;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    "Credentials Error",
                    $"Failed to load Google credentials from environment: {ex.Message}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok
                );
                throw;
            }
        }
    }
}