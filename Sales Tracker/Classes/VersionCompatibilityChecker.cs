using System.Formats.Tar;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles version comparison and compatibility checking for application files.
    /// </summary>
    public static class VersionCompatibilityChecker
    {
        /// <summary>
        /// Validates file version compatibility before proceeding with file operations.
        /// Call this before any file opening operation to ensure compatibility.
        /// This method extracts and reads the version entirely in memory without creating temporary files.
        /// </summary>
        /// <returns>True if compatible and should proceed, false if incompatible</returns>
        public static bool HandleFileVersionCompatibility(string filePath)
        {
            try
            {
                string fileVersion = GetVersionFromCompanyFile(filePath);
                string currentVersion = Tools.GetVersionNumber();

                if (!IsFileVersionCompatible(fileVersion, currentVersion))
                {
                    HandleIncompatibleVersion(
                        fileVersion ?? "Unknown",
                        currentVersion,
                        filePath
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(1, $"Error validating file version: {ex.Message}");
                return true;  // Default to compatible on error to avoid blocking access
            }
        }

        /// <summary>
        /// Extracts and reads the version information from a company file entirely in memory.
        /// Similar to GetAccountantList() method in PasswordManager, but for version data.
        /// </summary>
        private static string? GetVersionFromCompanyFile(string companyFilePath)
        {
            try
            {
                // Check if the company file exists
                if (!File.Exists(companyFilePath))
                {
                    Log.WriteWithFormat(1, "Company file not found: {0}", companyFilePath);
                    return null;
                }

                // Create a temporary directory for extraction
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directories.CreateDirectory(tempDir);

                string sourceFile = companyFilePath;
                string decryptedTempFile = null;

                try
                {
                    // Check if the file is encrypted and decrypt if necessary
                    using (FileStream fs = new(sourceFile, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new(fs))
                    {
                        // Read to the end to find encryption markers
                        string content = reader.ReadToEnd();
                        string[] lines = content.Split([Environment.NewLine], StringSplitOptions.None);

                        if (lines.Length >= 2 && lines[^2].Contains(EncryptionManager.encryptedTag) &&
                            lines[^2].Split(':')[1] == EncryptionManager.encryptedValue)
                        {
                            // File is encrypted, decrypt it first
                            decryptedTempFile = Path.GetTempFileName();
                            EncryptionManager.DecryptAndWriteToFile(sourceFile, decryptedTempFile, EncryptionManager.AesKey, EncryptionManager.AesIV);
                            sourceFile = decryptedTempFile;
                            Log.Write(1, "Decrypted company file for version extraction");
                        }
                    }

                    // Extract the tar file to temp directory
                    TarFile.ExtractToDirectory(sourceFile, tempDir, true);
                    Log.WriteWithFormat(1, "Extracted company file to temp directory for version check: {0}", tempDir);

                    // Find the company data file in the extracted directory
                    string[] dataFiles = Directory.GetFiles(tempDir, Directories.CompanyDataFileName, SearchOption.AllDirectories);

                    if (dataFiles.Length > 0)
                    {
                        string companyDataFile = dataFiles[0];
                        string version = DataFileManager.GetValue(AppDataSettings.AppVersion, companyDataFile);

                        Log.WriteWithFormat(1, "Found version in company file: {0}", version ?? "null");
                        return version;
                    }
                    else
                    {
                        Log.Write(1, "No company data file found in company archive - assuming older format");
                        return null;  // Older format without version info
                    }
                }
                finally
                {
                    // Clean up temporary files
                    if (decryptedTempFile != null && File.Exists(decryptedTempFile))
                    {
                        File.Delete(decryptedTempFile);
                    }

                    if (Directory.Exists(tempDir))
                    {
                        Directories.DeleteDirectory(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error extracting version from company file: {0}", ex.Message);
                return null;  // Default to compatible on error
            }
        }

        /// <summary>
        /// Checks if a file version is compatible with the current application version.
        /// </summary>
        /// <returns>True if compatible, false if file version is newer.</returns>
        private static bool IsFileVersionCompatible(string fileVersion, string currentVersion)
        {
            if (string.IsNullOrEmpty(fileVersion))
            {
                // If no version info in file, assume it's an older format and compatible
                return true;
            }

            if (string.IsNullOrEmpty(currentVersion))
            {
                Log.Write(1, "Current application version is null or empty");
                return false;
            }

            try
            {
                Version fileVer = new(fileVersion);
                Version currentVer = new(currentVersion);

                // File is compatible if its version is less than or equal to current version
                return fileVer <= currentVer;
            }
            catch (Exception ex)
            {
                Log.Write(1, $"Error parsing version strings. FileVersion: {fileVersion}, CurrentVersion: {currentVersion}, Error: {ex.Message}");
                // If we can't parse versions, assume compatible to avoid blocking access
                return true;
            }
        }

        /// <summary>
        /// Displays an incompatible version warning.
        /// </summary>
        private static void HandleIncompatibleVersion(string fileVersion, string currentVersion, string filePath)
        {
            string company = Path.GetFileNameWithoutExtension(filePath);

            CustomMessageBox.Show(
                "Incompatible File Version",
                $"Cannot Open This File\n\n" +
                $"The company '{company}' was created with a newer version of Argo Sales Tracker than what you currently have installed.\n\n" +

                $"📄 File was saved with: Version {fileVersion}\n" +
                $"💻 You currently have: Version {currentVersion}\n\n" +

                $"What does this mean?\n" +
                $"• Opening it could possibly damage or corrupt your data\n" +
                $"• Your file is completely safe - it just needs a newer version to open\n\n" +

                $"What should you do?\n" +
                $"• Download and install the latest version of Argo Sales Tracker\n" +
                $"• Then try opening this file again\n" +

                $"The application will now close as a safety precaution to protect your data.",
                CustomMessageBoxIcon.Exclamation,
                CustomMessageBoxButtons.Ok
            );

            Log.Write(1, $"File version incompatibility detected. File: {filePath}, FileVersion: {fileVersion}, CurrentVersion: {currentVersion}");
        }
    }
}