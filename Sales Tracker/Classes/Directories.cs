using Sales_Tracker.Passwords;
using System.Formats.Tar;
using System.IO.Compression;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Static class responsible for managing file system operations and directory structures.
    /// Handles file paths, directory creation/deletion, and file operations including encryption and compression.
    /// </summary>
    public static class Directories
    {
        // Directories and files
        private static string _companyName, _tempCompany_dir, _argoCompany_dir, _argoCompany_file, _appData_dir,
            _globalAppDataSettingsCache_file, _appDataSettings_file, _purchases_file, _sales_file, _categoryPurchases_file,
            _categorySales_file, _accountants_file, _companies_file, _receipts_dir, _logsCache_dir, _desktop_dir, _cache_dir,
            _translationsCache_file, _englishTexts_file, _anonymousUserDataCache_file, _exchangeRateCache_file, _secretsFilePath;

        // Getters and setters
        public static string CompanyName => _companyName;
        public static string TempCompany_dir => _tempCompany_dir;
        public static string ArgoCompany_dir => _argoCompany_dir;
        public static string ArgoCompany_file => _argoCompany_file;
        public static string AppData_dir => _appData_dir;
        public static string AppDataSettings_file => _appDataSettings_file;
        public static string Purchases_file => _purchases_file;
        public static string Sales_file => _sales_file;
        public static string CategoryPurchases_file => _categoryPurchases_file;
        public static string CategorySales_file => _categorySales_file;
        public static string Accountants_file => _accountants_file;
        public static string Companies_file => _companies_file;
        public static string Receipts_dir => _receipts_dir;
        public static string Logs_dir => _logsCache_dir;
        public static string Desktop_dir => _desktop_dir;
        public static string Cache_dir
        {
            get => _cache_dir;
            set => _cache_dir = value;
        }
        public static string TranslationsCache_file
        {
            get => _translationsCache_file;
            set => _translationsCache_file = value;
        }
        public static string GlobalAppDataSettingsCache_file => _globalAppDataSettingsCache_file;
        public static string AnonymousUserDataCache_file => _anonymousUserDataCache_file;
        public static string ExchangeRateCache_file => _exchangeRateCache_file;
        public static string EnglishTexts_file
        {
            get => _englishTexts_file;
            set => _englishTexts_file = value;
        }
        public static string SecretsFilePath => _secretsFilePath;

        // Methods
        /// <summary>
        /// Initializes directory paths for a specific project.
        /// </summary>
        public static void SetDirectories(string projectDir, string project_name)
        {
            Properties.Settings.Default.ProjectDirectory = projectDir;
            Properties.Settings.Default.Save();

            if (!projectDir.EndsWith('\\'))
            {
                projectDir += "\\";
            }

            _companyName = project_name;
            _tempCompany_dir = _appData_dir + project_name + @"\";

            _argoCompany_dir = projectDir;
            _argoCompany_file = projectDir + project_name + ArgoFiles.ArgoCompanyFileExtension;

            // Main files
            _purchases_file = _tempCompany_dir + "purchases" + ArgoFiles.TxtFileExtension;
            _sales_file = _tempCompany_dir + "sales" + ArgoFiles.TxtFileExtension;
            _categoryPurchases_file = _tempCompany_dir + "categoryPurchases" + ArgoFiles.JsonFileExtension;
            _categorySales_file = _tempCompany_dir + "categorySales" + ArgoFiles.JsonFileExtension;
            _accountants_file = _tempCompany_dir + "accountants" + ArgoFiles.TxtFileExtension;
            _companies_file = _tempCompany_dir + "companies" + ArgoFiles.TxtFileExtension;
            _receipts_dir = _appData_dir + project_name + @"\receipts\";

            // Misc.
            _appDataSettings_file = _tempCompany_dir + "appSettings" + ArgoFiles.TxtFileExtension;
        }
        public static void SetUniversalDirectories()
        {
            // App data
            _appData_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Argo\Argo Sales Tracker\";

            // Cache
            _cache_dir = _appData_dir + "cache-" + ArgoCompany.GetUniqueCompanyIdentifier("Argo Sales Tracker") + @"\";
            _translationsCache_file = _cache_dir + "translations" + ArgoFiles.JsonFileExtension;
            _globalAppDataSettingsCache_file = _cache_dir + "globalSettings" + ArgoFiles.TxtFileExtension;
            _logsCache_dir = _cache_dir + @"logs\";
            _anonymousUserDataCache_file = _cache_dir + "anonymousUserData" + ArgoFiles.JsonFileExtension;
            _exchangeRateCache_file = _cache_dir + "exchangeRatesCache" + ArgoFiles.JsonFileExtension;

            // Other
            _englishTexts_file = _appData_dir + "english" + ArgoFiles.JsonFileExtension;
            _desktop_dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _secretsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.info");
        }
        public static void EnsureAppDataDirectoriesExist()
        {
            if (!Directory.Exists(_appData_dir))
            {
                CreateDirectory(_appData_dir, false);
            }
        }

        // Directories
        /// <summary>
        /// Creates a new directory with optional hidden attribute.
        /// </summary>
        /// <returns>True if directory was created successfully, false if it already exists</returns>
        public static bool CreateDirectory(string directory, bool hidden)
        {
            if (Directory.Exists(directory))
            {
                Log.Error_DirectoryAlreadyExists(directory);
                return false;
            }

            DirectoryInfo di = Directory.CreateDirectory(directory);
            if (hidden)
            {
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            return true;
        }

        /// <summary>
        /// Recursively copies a directory and its contents to a new location.
        /// </summary>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool overWrite)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories

            // Get information about the source directory
            DirectoryInfo dir = new(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
            {
                Log.Error_FileDoesNotExist(dir.FullName);
            }

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                File.Copy(file.FullName, targetFilePath, overWrite);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true, overWrite);
                }
            }
        }

        /// <summary>
        /// Deletes a directory and optionally its contents.
        /// </summary>
        /// <returns>True if directory was deleted successfully</returns>
        public static bool DeleteDirectory(string directory, bool recursive)
        {
            if (!Directory.Exists(directory))
            {
                Log.Error_DirectoryDoesNotExist(directory);
                return false;
            }

            Directory.Delete(directory, recursive);
            return true;
        }

        /// <summary>
        /// Rename folders without having to provide the full path for the new name.
        /// </summary>
        public static void RenameFolder(string oldFolderPath, string newFolderName)
        {
            DirectoryInfo directoryInfo = new(oldFolderPath);
            string newFolderPath = Path.Combine(directoryInfo.Parent.FullName, newFolderName);

            Directory.Move(oldFolderPath, newFolderPath);
        }

        /// <summary>
        /// Sets the Hidden attribute for the specified directory.
        /// </summary>
        public static void MakeDirectoryHidden(string directory)
        {
            DirectoryInfo folder = new(directory);

            if (folder.Exists)
            {
                folder.Attributes |= FileAttributes.Hidden;
            }
        }

        /// <summary>
        /// Returns the total size in bytes of all files within a directory and its subdirectories.
        /// </summary>
        public static long CalculateDirectorySize(string directoryPath)
        {
            long size = 0;
            DirectoryInfo dirInfo = new(directoryPath);

            // Add size of files
            foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }

            return size;
        }

        // Files
        public static bool CreateFile(string directory)
        {
            if (File.Exists(directory))
            {
                Log.Error_FileAlreadyExists(directory);
                return false;
            }

            File.Create(directory).Close();
            return true;
        }
        public static bool CopyFile(string source, string destination, bool overwrite = false)
        {
            if (!File.Exists(source))
            {
                Log.Error_FileDoesNotExist(source);
                return false;
            }
            if (File.Exists(destination) && !overwrite)
            {
                Log.Error_DestinationFileAlreadyExists(destination);
                return false;
            }

            File.Copy(source, destination);
            return true;
        }
        public static bool DeleteFile(string file)
        {
            if (!File.Exists(file))
            {
                Log.Error_FileDoesNotExist(file);
                return false;
            }

            File.Delete(file);
            return true;
        }
        /// <summary>
        /// Moves a file. Gives an option to rename the file.
        /// </summary>
        public static bool MoveFile(string sourceFileName, string destinationFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                Log.Error_FileDoesNotExist(destinationFileName);
                return false;
            }
            if (File.Exists(destinationFileName))
            {
                Log.Error_DestinationFileAlreadyExists(sourceFileName);
                return false;
            }
            if (sourceFileName == destinationFileName)
            {
                Log.Error_TheSourceAndDestinationAreTheSame(sourceFileName, destinationFileName);
                return false;
            }

            File.Move(sourceFileName, destinationFileName);
            return true;
        }

        // Write to file
        /// <summary>
        /// Writes multiple lines to a file, creating the file if it doesn't exist.
        /// </summary>
        public static void WriteLinesToFile(string filePath, IEnumerable<string> lines)
        {
            if (!File.Exists(filePath))
            {
                CreateFile(filePath);
            }
            try
            {
                using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using StreamWriter writer = new(fs);
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }
            catch
            {
                Log.Error_WriteToFile(filePath);
            }
        }

        /// <summary>
        /// Writes text to a file, creating the file if it doesn't exist.
        /// </summary>
        public static void WriteTextToFile(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                CreateFile(filePath);
            }
            try
            {
                using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using StreamWriter writer = new(fs);
                writer.Write(content);
            }
            catch
            {
                Log.Error_WriteToFile(filePath);
            }
        }

        // Read file
        /// <summary>
        /// Reads all lines from the specified file.
        /// </summary>
        /// <returns>An array of strings from the file.</returns>
        public static string[] ReadAllLinesInFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error_FileDoesNotExist(filePath);
                return [];
            }

            List<string> lines = [];
            try
            {
                using StreamReader reader = new(filePath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            catch
            {
                Log.Error_ReadFile(filePath);
            }

            return lines.ToArray();
        }
        /// <summary>
        /// Reads all text from the specified file.
        /// </summary>
        /// <returns>The text content of the file as a single string.</returns>
        public static string ReadAllTextInFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error_FileDoesNotExist(filePath);
                return "";
            }

            try
            {
                return File.ReadAllText(filePath);
            }
            catch
            {
                Log.Error_ReadFile(filePath);
                return "";
            }
        }

        // Tar files
        /// <summary>
        /// Creates an encrypted Argo Tar file from a directory.
        /// Handles both encrypted and unencrypted file creation based on application settings.
        /// </summary>
        public static void CreateArgoTarFileFromDirectory(string sourceDirectory, string destinationFile, bool overwrite)
        {
            try
            {
                if (!overwrite)
                {
                    destinationFile = GetNewFileNameIfItAlreadyExists(destinationFile);
                }

                // Use MemoryStream to securely hold the tar data, avoiding the security risk of writing it to a temporary file
                using MemoryStream tarMemoryStream = new();

                // Create the tar file in memory
                TarFile.CreateFromDirectory(sourceDirectory, tarMemoryStream, true);

                tarMemoryStream.Seek(0, SeekOrigin.Begin);

                if (Properties.Settings.Default.EncryptFiles)
                {
                    // Encrypt the tar data in memory
                    MemoryStream encryptedMemoryStream = EncryptionManager.EncryptStream(tarMemoryStream, EncryptionManager.AesKey, EncryptionManager.AesIV);
                    encryptedMemoryStream.Seek(0, SeekOrigin.Begin);

                    // Write the encrypted data to the destination file
                    using (FileStream destFileStream = new(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        encryptedMemoryStream.CopyTo(destFileStream);
                    }

                    // Encrypt the password
                    string encryptedPassword = EncryptionManager.EncryptString(EncryptionManager.passwordTag + PasswordManager.Password, EncryptionManager.AesKey, EncryptionManager.AesIV);

                    // Create and append the footer
                    string footer = Environment.NewLine + EncryptionManager.encryptedTag + EncryptionManager.encryptedValue + Environment.NewLine + encryptedPassword;
                    File.AppendAllText(destinationFile, footer);

                    Log.Write(2, $"File created and encrypted successfully: {destinationFile}");
                }
                else
                {
                    // Write the unencrypted data to the destination file
                    using (FileStream destFileStream = new(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        tarMemoryStream.CopyTo(destFileStream);
                    }

                    // Encrypt the password
                    string encryptedPassword = EncryptionManager.EncryptString(EncryptionManager.passwordTag + PasswordManager.Password, EncryptionManager.AesKey, EncryptionManager.AesIV);

                    // Create and append the footer with the encrypted password
                    string footer = Environment.NewLine + EncryptionManager.encryptedTag + Environment.NewLine + encryptedPassword;
                    File.AppendAllText(destinationFile, footer);

                    Log.Write(2, $"File created successfully: {destinationFile}");
                }
            }
            catch (Exception ex)
            {
                string info = $"Error during tar creation or encryption: {ex.Message}";
                Log.Error_Save(info, destinationFile);
            }
        }
        /// <summary>
        /// Imports an Argo Tar file into a directory.
        /// </summary>
        /// <returns> The file name without the (num) and the extension. </returns>
        public enum ImportType
        {
            ArgoCompany
        }
        public static string ImportArgoTarFile(string sourceFile, string destinationDirectory, ImportType importType, List<string> listOfThingNames, bool askUserToRename)
        {
            string thingName = Path.GetFileNameWithoutExtension(sourceFile);
            string tempDir = destinationDirectory + ArgoCompany.GetUniqueCompanyIdentifier(AppData_dir);
            string decryptedTempFile = null;
            string extractedDir;

            try
            {
                // Check if the file is encrypted and decrypt if necessary
                using (FileStream fs = new(sourceFile, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new(fs))
                {
                    List<string> lines = [];
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }

                    if (lines[^2].Split(':')[1] == EncryptionManager.encryptedValue)
                    {
                        decryptedTempFile = Path.GetTempFileName();
                        EncryptionManager.DecryptAndWriteToFile(sourceFile, decryptedTempFile, EncryptionManager.AesKey, EncryptionManager.AesIV);
                        sourceFile = decryptedTempFile;
                    }
                }

                extractedDir = GetTopDirectoryFromTarFile(sourceFile);

                // Check if the thing already exists
                if (listOfThingNames.Contains(thingName))
                {
                    string suggestedThingName = Tools.AddNumberForAStringThatAlreadyExists(thingName, listOfThingNames);

                    CustomMessageBoxResult? result = null;
                    if (askUserToRename)
                    {
                        result = CustomMessageBox.Show(
                            $"Rename {importType}",
                            $"Do you want to rename '{thingName}' to '{suggestedThingName}'? There is already a {importType} with the same name.",
                            CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);
                    }
                    if (result == CustomMessageBoxResult.Yes || !askUserToRename)
                    {
                        // Extract the tab to a temp folder because there is already a thing with the same name, that way the thing can be renamed
                        CreateDirectory(tempDir, true);
                        TarFile.ExtractToDirectory(sourceFile, tempDir, false);

                        // Rename thing in file and move it out of the temp directory
                        Directory.Move(tempDir + "\\" + thingName, destinationDirectory + suggestedThingName);
                        DeleteDirectory(tempDir, true);

                        thingName = suggestedThingName;
                        extractedDir = suggestedThingName;
                    }
                    else { return ""; }
                }
                else
                {
                    // Extract normally
                    TarFile.ExtractToDirectory(sourceFile, destinationDirectory, false);
                }

                string newDestinationDirectory = destinationDirectory + thingName;

                // If the .ArgoProject file was renamed
                if (thingName != extractedDir)
                {
                    RenameFolder(destinationDirectory + extractedDir, newDestinationDirectory);
                }

                MakeDirectoryHidden(newDestinationDirectory);
            }
            finally
            {
                if (decryptedTempFile != null && File.Exists(decryptedTempFile))
                {
                    File.Delete(decryptedTempFile);
                }
            }

            return thingName;
        }
        /// <summary>
        /// Reads a TAR file and returns the name of the top-most directory or file.
        /// </summary>
        /// <returns>The name of the top-most directory found in the TAR file, or null if no directories are found.</returns>
        public static string GetTopDirectoryFromTarFile(string tarFilePath)
        {
            foreach (string line in File.ReadLines(tarFilePath))
            {
                if (line.Contains("path="))
                {
                    int pathStartIndex = line.IndexOf("path=") + 5;
                    int pathEndIndex = line.IndexOf('/', pathStartIndex);
                    if (pathEndIndex != -1)
                    {
                        return line.Substring(pathStartIndex, pathEndIndex - pathStartIndex);
                    }
                }
            }
            throw new Exception("Path not found in the provided tar file string.");
        }

        /// <summary>
        /// Creates a backup of the current company project by saving all changes and creating a compressed archive.
        /// Uses consistent naming convention to handle duplicates by appending " (2)", " (3)", etc.
        /// </summary>
        public static void CreateBackup(string destinationDirectory)
        {
            ArgoCompany.SaveAll();

            string projectName = new DirectoryInfo(destinationDirectory).Name;
            string backupDir = Path.GetDirectoryName(destinationDirectory);
            string fileExtension = ArgoFiles.ArgoCompanyFileExtension;

            // Get list of existing backup files (without extensions)
            List<string> existingBackups = GetListOfAllFilesWithoutExtensionInDirectory(backupDir);

            // Generate unique name if needed using consistent naming method
            string uniqueName = projectName;
            if (existingBackups.Contains(projectName))
            {
                uniqueName = Tools.AddNumberForAStringThatAlreadyExists(projectName, existingBackups);
            }

            // Construct paths using the unique name
            string uniqueBasePath = Path.Combine(backupDir, uniqueName);
            string tempDirPath = uniqueBasePath;
            string tempFilePath = Path.Combine(tempDirPath, projectName + fileExtension);
            string finalZipPath = uniqueBasePath + ArgoFiles.ZipExtension;

            // Create initial backup file
            CopyFile(ArgoCompany_file, uniqueBasePath + fileExtension);

            // Create temporary directory for zip processing
            CreateDirectory(tempDirPath, true);

            // Move backup file into temporary directory with correct name
            MoveFile(uniqueBasePath + fileExtension, tempFilePath);

            // Create zip archive from temporary directory
            ZipFile.CreateFromDirectory(tempDirPath, finalZipPath);

            Log.Write(4, $"Backed up '{uniqueName}'");

            // Clean up temporary directory and files
            if (Directory.Exists(tempDirPath))
            {
                DeleteDirectory(tempDirPath, true);
            }
        }
        /// <summary>
        /// Returns a list of all the files in the directory. Also remove the file extension.
        /// </summary>
        public static List<string> GetListOfAllFilesWithoutExtensionInDirectory(string directory)
        {
            return Directory.GetFiles(directory)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name != null)
                .Cast<string>()
                .ToList();
        }
        /// <summary>
        /// Returns a list of all the directories in the specified directory.
        /// </summary>
        public static List<string> GetListOfAllDirectoriesInDirectory(string directory)
        {
            return Directory.GetDirectories(directory).ToList();
        }
        /// <summary>
        /// Returns a list of all the directory names in the specified directory.
        /// </summary>
        public static List<string> GetListOfAllDirectoryNamesInDirectory(string directory)
        {
            return Directory.GetDirectories(directory)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Cast<string>()
                .ToList();
        }

        /// <summary>
        /// Generates a new unique file name when a file already exists.
        /// Appends a number to the filename to make it unique.
        /// </summary>
        /// <returns>New unique file name</returns>
        public static string GetNewFileNameIfItAlreadyExists(string filePath)
        {
            // Check if the file already exists and get a new name if necessary
            if (File.Exists(filePath))
            {
                string destinationDirectory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);

                List<string> filesList = GetListOfAllFilesWithoutExtensionInDirectory(destinationDirectory);
                fileNameWithoutExtension = Tools.AddNumberForAStringThatAlreadyExists(fileNameWithoutExtension, filesList);
                return Path.Combine(destinationDirectory, fileNameWithoutExtension + fileExtension);
            }
            return filePath;
        }
    }
}