namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Provides functionality to load environment variables from a .env file and access them throughout the application.
    /// </summary>
    public static class DotEnv
    {
        private static readonly Dictionary<string, string> _envVars = [];

        /// <summary>
        /// Loads environment variables from a .env file into memory.
        /// </summary>
        public static void Load()
        {
            string envFileName = ".env";
            string envFilePath = FindFileRelativeToSolution(envFileName);

            if (envFilePath == null)
            {
                Log.Write(0, $"{envFileName} file not found relative to solution.");
                return;
            }

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
            if (_envVars.TryGetValue(key, out string value))
            {
                return value;
            }

            Log.Write(0, $"{key} not found.");
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
                DirectoryInfo parentDir = Directory.GetParent(currentDir);
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