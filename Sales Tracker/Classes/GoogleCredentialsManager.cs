using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudResourceManager.v1;
using Google.Apis.CloudResourceManager.v1.Data;
using Google.Apis.Iam.v1;
using Google.Apis.Iam.v1.Data;
using Google.Apis.Services;
using Google.Apis.ServiceUsage.v1;
using Google.Apis.ServiceUsage.v1.Data;
using System.Text;

namespace Sales_Tracker.Classes
{
    public class GoogleCredentialsManager
    {
        private const string PROJECT_ID = "argp-1653327700137";
        private const string SERVICE_ACCOUNT_NAME = "argo-sales-tracker";
        private static readonly string[] REQUIRED_APIS =
        [
            "sheets.googleapis.com",
            "drive.googleapis.com"
        ];

        public static async Task CreateCredentialsIfNeeded()
        {
            string credentialsPath = Path.Combine(Directories.GoogleCredentials_file);
            if (File.Exists(credentialsPath))
            {
                return;
            }

            try
            {
                GoogleCredential userCredential;

                // Load from your credentials file
                using (FileStream stream = new(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    userCredential = GoogleCredential.FromStream(stream)
                        .CreateScoped(
                        [
                            "https://www.googleapis.com/auth/spreadsheets",
                            "https://www.googleapis.com/auth/drive"
                        ]);
                }

                // Create new project if it doesn't exist
                using CloudResourceManagerService resourceManagerService = new(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = "Sales Tracker"
                });

                // Check if project exists
                try
                {
                    await resourceManagerService.Projects.Get(PROJECT_ID).ExecuteAsync();
                }
                catch (GoogleApiException)
                {
                    // Create project if it doesn't exist
                    Project project = new()
                    {
                        ProjectId = PROJECT_ID,
                        Name = "Sales Tracker"
                    };
                    await resourceManagerService.Projects.Create(project).ExecuteAsync();
                }

                // Enable required APIs
                ServiceUsageService serviceUsageService = new(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = "Sales Tracker"
                });

                foreach (string api in REQUIRED_APIS)
                {
                    string serviceName = $"projects/{PROJECT_ID}/services/{api}";

                    EnableServiceRequest enableRequest = new();

                    await serviceUsageService.Services
                        .Enable(enableRequest, serviceName)
                        .ExecuteAsync();
                }

                // Create service account
                IamService iamService = new(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredential
                });

                CreateServiceAccountRequest createRequest = new()
                {
                    AccountId = SERVICE_ACCOUNT_NAME,
                    ServiceAccount = new ServiceAccount
                    {
                        DisplayName = "Sales Tracker Service Account"
                    }
                };

                ServiceAccount serviceAccount = await iamService.Projects.ServiceAccounts
                     .Create(createRequest, $"projects/{PROJECT_ID}")
                     .ExecuteAsync();

                // Create key for service account
                CreateServiceAccountKeyRequest keyRequest = new();
                ServiceAccountKey key = await iamService.Projects.ServiceAccounts.Keys
                    .Create(keyRequest, serviceAccount.Name)
                    .ExecuteAsync();

                // Save the private key as googleCredentials.json
                string privateKeyData = Encoding.UTF8.GetString(
                    Convert.FromBase64String(key.PrivateKeyData)
                );

                Directory.CreateDirectory(Directories.AppData_dir);
                await File.WriteAllTextAsync(credentialsPath, privateKeyData);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    "Credentials Creation Error",
                    $"Failed to create Google credentials: {ex.Message}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok
                );
                throw;
            }
        }
    }
}