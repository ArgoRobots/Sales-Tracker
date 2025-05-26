namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Provides functionality to load environment variables from a .env file (development) 
    /// or encrypted secrets file (production) and access them throughout the application.
    /// </summary>
    public static class DotEnv
    {
        private static readonly Dictionary<string, string> _envVars = [];

        /// <summary>
        /// Loads environment variables from either .env file (development) or encrypted secrets (production).
        /// Automatically creates encrypted secrets file (development).
        /// </summary>
        public static void Load()
        {
            // Check if we have a .env file for development
            string envFilePath = FindFileRelativeToSolution(".env");

            if (envFilePath != null)
            {
                // Development mode - use .env file
                LoadFromEnvFile(envFilePath);

                ProductionSecretsManager.CreateEncryptedSecretsFile(envFilePath);
            }
            else if (File.Exists(Directories.SecretsFilePath))
            {
                // Production mode - use encrypted secrets
                LoadFromEncryptedSecrets();
            }
            else
            {
                Log.Error_ENVFileNotFound($".env or {Path.GetFileName(Directories.SecretsFilePath)}");
            }
        }
        private static void LoadFromEncryptedSecrets()
        {
            try
            {
                Dictionary<string, string> secrets = ProductionSecretsManager.LoadEncryptedSecrets();

                foreach (KeyValuePair<string, string> kvp in secrets)
                {
                    _envVars[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Failed to load encrypted secrets: {ex.Message}");
            }
        }
        private static void LoadFromEnvFile(string envFilePath)
        {
            foreach (string line in File.ReadAllLines(envFilePath))
            {
                string[] parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2) { continue; }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                // Remove quotes if present
                if (value.StartsWith('\"') && value.EndsWith('\"'))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                _envVars[key] = value;
            }
        }

        /// <summary>
        /// Retrieves the value of an environment variable by its key.
        /// </summary>
        public static string? Get(string key)
        {
            if (_envVars.TryGetValue(key, out string? value))
            {
                return value;
            }

            Log.Error_ENVKeyNotFound(key);
            return null;
        }

        /// <summary>
        /// Locates a file by searching up through the directory hierarchy from the current directory.
        /// </summary>
        private static string? FindFileRelativeToSolution(string fileName)
        {
            string currentDir = Directory.GetCurrentDirectory();

            // Navigate up until we find either the solution file or the .env file
            while (!string.IsNullOrEmpty(currentDir))
            {
                // Check if the .env file exists in the current directory
                string envFilePath = Path.Combine(currentDir, fileName);
                if (File.Exists(envFilePath))
                {
                    return envFilePath;
                }

                // Check if any .sln file exists in the current directory
                if (Directory.GetFiles(currentDir, "*.sln").Length != 0)
                {
                    // We found the solution directory, check for .env file
                    envFilePath = Path.Combine(currentDir, fileName);
                    if (File.Exists(envFilePath))
                    {
                        return envFilePath;
                    }
                    else
                    {
                        // .env doesn't exist in solution directory
                        return null;
                    }
                }

                // Move up one directory
                DirectoryInfo? parentDir = Directory.GetParent(currentDir);
                if (parentDir == null)
                {
                    break;  // We've reached the root directory
                }

                currentDir = parentDir.FullName;
            }

            return null;  // File not found
        }
    }
}