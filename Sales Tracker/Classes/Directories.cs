using Sales_Tracker.Passwords;
using System.Formats.Tar;
using System.IO.Compression;

namespace Sales_Tracker.Classes
{
    public static class Directories
    {
        // Directories
        public static string CompanyName { get; private set; }
        public static string TempCompany_dir { get; private set; }
        public static string ArgoCompany_dir { get; private set; }
        public static string ArgoCompany_file { get; private set; }
        public static string AppData_dir { get; private set; }
        public static string AppDataConfig_file { get; private set; }
        public static string Purchases_file { get; private set; }
        public static string Sales_file { get; private set; }
        public static string CategoryPurchases_file { get; private set; }
        public static string CategorySales_file { get; private set; }
        public static string Accountants_file { get; private set; }
        public static string Companies_file { get; private set; }
        public static string Receipts_dir { get; private set; }
        public static string Logs_dir { get; private set; }
        public static string Info_file { get; private set; }
        public static string Desktop_dir { get; private set; }

        public static void SetDirectories(string projectDir, string project_name)
        {
            if (!projectDir.EndsWith('\\'))
            {
                projectDir += "\\";
            }

            CompanyName = project_name;
            TempCompany_dir = AppData_dir + project_name + @"\";

            ArgoCompany_dir = projectDir;
            ArgoCompany_file = projectDir + project_name + ArgoFiles.ArgoCompanyFileExtension;

            Purchases_file = TempCompany_dir + "purchases" + ArgoFiles.TxtFileExtension;
            Sales_file = TempCompany_dir + "sales" + ArgoFiles.TxtFileExtension;
            CategoryPurchases_file = TempCompany_dir + "categoryPurchases" + ArgoFiles.JsonFileExtension;
            CategorySales_file = TempCompany_dir + "categorySales" + ArgoFiles.JsonFileExtension;
            Accountants_file = TempCompany_dir + "accountants" + ArgoFiles.TxtFileExtension;
            Companies_file = TempCompany_dir + "companies" + ArgoFiles.TxtFileExtension;
            Receipts_dir = TempCompany_dir + @"receipts\";

            // Logs
            Logs_dir = TempCompany_dir + @"logs\";

            Info_file = TempCompany_dir + "info" + ArgoFiles.TxtFileExtension;
        }
        public static void SetUniversalDirectories()
        {
            // App data
            AppData_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Argo\Argo Sales Tracker\";
            AppDataConfig_file = AppData_dir + "ArgoSalesTracker" + ArgoFiles.TxtFileExtension;

            // Other
            Desktop_dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        // Directories
        /// <summary>
        /// Creates a directory.
        /// </summary>
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
        /// Copies an existing directory to a new directory.
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
        /// Deletes a directory. If recursive is true, then the subdirectories will also be deleted.
        /// </summary>
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
        public static void MakeDirectoryHidden(string directory)
        {
            DirectoryInfo folder = new(directory);

            if (folder.Exists)
            {
                folder.Attributes |= FileAttributes.Hidden;
            }
        }

        // Files
        /// <summary>
        /// Creates a file.
        /// </summary>
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
        /// <summary>
        /// Copies a file.
        /// </summary>
        public static bool CopyFile(string source, string destination)
        {
            if (!File.Exists(source))
            {
                Log.Error_FileDoesNotExist(destination);
                return false;
            }
            if (File.Exists(destination))
            {
                Log.Error_DestinationFileAlreadyExists(destination);
                return false;
            }

            File.Copy(source, destination);
            return true;
        }
        /// <summary>
        /// Deletes a file.
        /// </summary>
        public static bool DeleteFile(string directory)
        {
            if (!File.Exists(directory))
            {
                Log.Error_FileDoesNotExist(directory);
                return false;
            }

            File.Delete(directory);
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
        public static void WriteLinesToFile(string filePath, IEnumerable<string> lines)
        {
            if (!File.Exists(filePath))
            {
                Log.Error_FileDoesNotExist(filePath);
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
                Log.Error_FailedToWriteToFile(filePath);
            }
        }
        public static void WriteTextToFile(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                Log.Error_FileDoesNotExist(filePath);
            }
            try
            {
                using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using StreamWriter writer = new(fs);
                writer.Write(content);
            }
            catch
            {
                Log.Error_FailedToWriteToFile(filePath);
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
                Log.Error_FailedToReadFile(filePath);
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
                Log.Error_FailedToReadFile(filePath);
                return "";
            }
        }

        // Tar files
        /// <summary>
        /// Creates an encrypted Argo Tar file from a directory.
        /// </summary>
        public static void CreateArgoTarFileFromDirectory(string sourceDirectory, string destinationFile, bool overwrite)
        {
            try
            {
                string destinationDirectory = Path.GetDirectoryName(destinationFile);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationFile);
                string fileExtension = Path.GetExtension(destinationFile);

                // Check if the file already exists and get a new name if necessary
                if (File.Exists(destinationFile) && !overwrite)
                {
                    List<string> filesList = GetListOfAllFilesWithoutExtensionInDirectory(destinationDirectory);
                    fileNameWithoutExtension = Tools.AddNumberForAStringThatAlreadyExists(fileNameWithoutExtension, filesList);
                    destinationFile = Path.Combine(destinationDirectory, fileNameWithoutExtension + fileExtension);
                }

                // Use MemoryStream to securely hold the tar data, avoiding the security risk of writing it to a temporary file
                using MemoryStream tarMemoryStream = new();

                // Create the tar file in memory
                TarFile.CreateFromDirectory(sourceDirectory, tarMemoryStream, true);

                tarMemoryStream.Seek(0, SeekOrigin.Begin);

                if (Properties.Settings.Default.EncryptFiles)
                {
                    // Encrypt the tar data in memory
                    MemoryStream encryptedMemoryStream = EncryptionHelper.EncryptStream(tarMemoryStream, EncryptionHelper.AesKey, EncryptionHelper.AesIV);
                    encryptedMemoryStream.Seek(0, SeekOrigin.Begin);

                    // Write the encrypted data to the destination file
                    using (FileStream destFileStream = new(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        encryptedMemoryStream.CopyTo(destFileStream);
                    }

                    // Encrypt the password
                    string encryptedPassword = EncryptionHelper.EncryptString(EncryptionHelper.passwordTag + PasswordManager.Password, EncryptionHelper.AesKey, EncryptionHelper.AesIV);

                    // Create and append the footer
                    string footer = Environment.NewLine + EncryptionHelper.encryptedTag + EncryptionHelper.encryptionHeader + Environment.NewLine + encryptedPassword;
                    File.AppendAllText(destinationFile, footer);

                    Log.Write(2, $"File successfully created and encrypted: {destinationFile}");
                }
                else
                {
                    // Write the unencrypted data to the destination file
                    using (FileStream destFileStream = new(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        tarMemoryStream.CopyTo(destFileStream);
                    }

                    // Encrypt the password
                    string encryptedPassword = EncryptionHelper.EncryptString(EncryptionHelper.passwordTag + PasswordManager.Password, EncryptionHelper.AesKey, EncryptionHelper.AesIV);

                    // Create and append the footer with the encrypted password
                    string footer = Environment.NewLine + EncryptionHelper.encryptedTag + Environment.NewLine + encryptedPassword;
                    File.AppendAllText(destinationFile, footer);

                    Log.Write(2, $"File successfully created: {destinationFile}");
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error during tar creation or encryption: {ex.Message}");
                Log.Error_FailedToSave(destinationFile);
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
            string tempDir = destinationDirectory + ArgoCompany.GetUniqueProjectIdentifier(AppData_dir);
            string decryptedTempFile = null;
            string extractedDir;

            try
            {
                // Check if the file is encrypted and decrypt if necessary
                using (FileStream fs = new(sourceFile, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new(fs))
                {
                    List<string> lines = new();
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }

                    if (lines[^2].Split(':')[1] == EncryptionHelper.encryptionHeader)
                    {
                        decryptedTempFile = Path.GetTempFileName();
                        EncryptionHelper.DecryptAndWriteToFile(sourceFile, decryptedTempFile, EncryptionHelper.AesKey, EncryptionHelper.AesIV);
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
                            CustomMessageBoxIcon.Question,
                            CustomMessageBoxButtons.YesNo);
                    }
                    if (result == CustomMessageBoxResult.Yes || !askUserToRename)
                    {
                        // Extract the tab to a temp folder because there is already a thing with the same name,
                        // that way the thing can be renamed.
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
        /// This also saves all.
        /// </summary>
        public static void CreateBackup(string destinationDirectory, string fileExtension)
        {
            ArgoCompany.SaveAll();

            string tarName = destinationDirectory;
            string folderName = new DirectoryInfo(destinationDirectory).Name;
            string newFileExtension = fileExtension;

            if (fileExtension == ".ArgoProject")
            {
                newFileExtension = ".zip";
            }

            if (File.Exists(destinationDirectory + newFileExtension))
            {
                int count = 2;

                while (true)
                {
                    if (!File.Exists(destinationDirectory + "-" + count + newFileExtension))
                    {
                        tarName = destinationDirectory + "-" + count;
                        break;
                    }
                    count++;
                }
            }

            // Copy the directory to the new location, and rename it so the tar file will have the new name
            CopyFile(ArgoCompany_file, tarName + fileExtension);

            // Move the file into a temp folder so it can be zipped. Use tarName to make sure the folder name does not already exist
            CreateDirectory(tarName, true);
            MoveFile(tarName + fileExtension, tarName + "\\" + folderName + fileExtension);

            ZipFile.CreateFromDirectory(tarName, tarName + ".zip");

            DeleteDirectory(tarName, true);

            Log.Write(4, $"Backed up '{folderName}'");
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
    }
}