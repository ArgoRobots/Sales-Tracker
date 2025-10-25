using Google.Apis.Auth.OAuth2;
using Sales_Tracker.Encryption;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public class GoogleCredentialsManager
    {
        /// <summary>
        /// Gets Google credentials from environment variables with the appropriate scopes.
        /// </summary>
        public static GoogleCredential GetCredentialsFromEnvironment()
        {
            try
            {
                string type = DotEnv.Get("GOOGLE_TYPE");
                string project_id = DotEnv.Get("GOOGLE_PROJECT_ID");
                string private_key_id = DotEnv.Get("GOOGLE_PRIVATE_KEY_ID");
                string private_key = DotEnv.Get("GOOGLE_PRIVATE_KEY")?.Replace("\\n", "\n");
                string client_email = DotEnv.Get("GOOGLE_CLIENT_EMAIL");
                string client_id = DotEnv.Get("GOOGLE_CLIENT_ID");
                string auth_uri = DotEnv.Get("GOOGLE_AUTH_URI");
                string token_uri = DotEnv.Get("GOOGLE_TOKEN_URI");
                string auth_provider_x509_cert_url = DotEnv.Get("GOOGLE_AUTH_PROVIDER_CERT_URL");
                string client_x509_cert_url = DotEnv.Get("GOOGLE_CLIENT_CERT_URL");
                string universe_domain = DotEnv.Get("GOOGLE_UNIVERSE_DOMAIN");

                if (string.IsNullOrEmpty(project_id) ||
                    string.IsNullOrEmpty(private_key) ||
                    string.IsNullOrEmpty(client_email) ||
                    string.IsNullOrEmpty(type) ||
                    string.IsNullOrEmpty(private_key_id) ||
                    string.IsNullOrEmpty(client_id) ||
                    string.IsNullOrEmpty(auth_uri) ||
                    string.IsNullOrEmpty(token_uri) ||
                    string.IsNullOrEmpty(auth_provider_x509_cert_url) ||
                    string.IsNullOrEmpty(client_x509_cert_url) ||
                    string.IsNullOrEmpty(universe_domain))
                {
                    throw new Exception("Missing required Google credentials in .env file");
                }

                // Create credentials JSON from environment variables
                var credentialJson = (
                    type,
                    project_id,
                    private_key_id,
                    private_key,
                    client_email,
                    client_id,
                    auth_uri,
                    token_uri,
                    auth_provider_x509_cert_url,
                    client_x509_cert_url,
                    universe_domain
                );

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
            catch
            {
                CustomMessageBox.Show(
                    "Credentials Error",
                    $"Failed to load Google credentials from environment",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok
                );
                throw;
            }
        }
    }
}