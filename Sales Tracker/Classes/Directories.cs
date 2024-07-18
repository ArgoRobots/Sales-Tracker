using System.Formats.Tar;
using System.IO.Compression;

namespace Sales_Tracker.Classes
{
    static class Directories
    {
        // Directories
        public static string companyName, tempCompany_dir, argoCompany_dir, argoCompany_file, appData_dir, appDataCongig_file, purchases_file,
          sales_file, categoryPurchases_file, categorySales_file, accountants_file, companies_file, receipts_dir, logs_dir, desktop_dir;

        public static void SetDirectoriesFor(string projectDir, string project_name)
        {
            if (!projectDir.EndsWith('\\'))
            {
                projectDir += "\\";
            }

            companyName = project_name;
            tempCompany_dir = appData_dir + project_name + @"\";

            argoCompany_dir = projectDir;
            argoCompany_file = projectDir + project_name + ArgoFiles.ArgoCompanyFileExtension;

            purchases_file = tempCompany_dir + "purchases" + ArgoFiles.TxtFileExtension;
            sales_file = tempCompany_dir + "sales" + ArgoFiles.TxtFileExtension;
            categoryPurchases_file = tempCompany_dir + "categoryPurchases" + ArgoFiles.JsonFileExtension;
            categorySales_file = tempCompany_dir + "categorySales" + ArgoFiles.JsonFileExtension;
            accountants_file = tempCompany_dir + "accountants" + ArgoFiles.TxtFileExtension;
            companies_file = tempCompany_dir + "companies" + ArgoFiles.TxtFileExtension;
            receipts_dir = tempCompany_dir + @"receipts\";

            // Logs
            logs_dir = tempCompany_dir + @"logs\";
        }
        public static void SetUniversalDirectories()
        {
            // App data
            appData_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Argo\Argo Sales Tracker\";
            appDataCongig_file = appData_dir + "ArgoSalesTracker.config";

            // Other
            desktop_dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        public static void InitDataFile()
        {
            if (!Directory.Exists(appData_dir))
            {
                CreateDirectory(appData_dir, false);
            }
            if (!File.Exists(appDataCongig_file))
            {
                CreateFile(appDataCongig_file);

                DataFileManager.SetValue(appDataCongig_file, DataFileManager.AppDataSettings.RPTutorial, "true");
                DataFileManager.Save(appDataCongig_file);
            }
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
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

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
            DirectoryInfo directoryInfo = new DirectoryInfo(oldFolderPath);
            string newFolderPath = Path.Combine(directoryInfo.Parent.FullName, newFolderName);

            Directory.Move(oldFolderPath, newFolderPath);
        }
        public static void MakeDirectoryHidden(string directory)
        {
            DirectoryInfo folder = new DirectoryInfo(directory);

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
        /// Creates an Argo Tar file from a directory.
        /// </summary>
        public static void CreateArgoTarFileFromDirectory(string sourceDirectory, string destinationDirectory)
        {
            // This method ensures that the Argo file is not deleted,
            // preventing the file from being moved on the desktop screen.

            // Create a temporary file
            string tempFile = Path.GetTempPath() + Path.GetFileName(destinationDirectory);

            try
            {
                // Create the tar file in the temporary location
                TarFile.CreateFromDirectory(sourceDirectory, tempFile, true);

                // Overwrite the existing file without deleting it
                using FileStream tempFileStream = new(tempFile, FileMode.Open, FileAccess.Read);
                using FileStream destFileStream = new(destinationDirectory, FileMode.Create, FileAccess.Write);
                tempFileStream.CopyTo(destFileStream);
            }
            catch
            {
                Log.Error_FailedToSave(destinationDirectory);
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>
        /// Imports an Argo Tar file into a directory.
        /// </summary>
        /// <returns> The file name wihtout the (num) and the extension. </returns>
        public static string ImportArgoTarFile(string sourceFile, string destinationDirectory, string thingBeingImported, List<string> listOfThingNames, bool askUserToRename)
        {
            string thingName = Path.GetFileNameWithoutExtension(sourceFile);
            string tempDir = destinationDirectory + "\\" + ArgoCompany.GetUniqueProjectIdentifier(appData_dir);
            string extractedDir = GetTopDirectoryFromTarFile(sourceFile);

            // Check if the thing already exists
            if (listOfThingNames.Contains(thingName))
            {
                string suggestedThingName = Tools.AddNumberForAStringThatAlreadyExists(thingName, listOfThingNames);

                CustomMessageBoxResult? result = null;
                if (askUserToRename)
                {
                    result = CustomMessageBox.Show(
                        $"Rename {thingBeingImported}",
                        $"Do you want to rename '{thingName}' to '{suggestedThingName}'? There is already a {thingBeingImported} with the same name.",
                        CustomMessageBoxIcon.Question,
                        CustomMessageBoxButtons.YesNo);
                }
                if (result == CustomMessageBoxResult.Yes || !askUserToRename)
                {
                    // Extract the tab to a temp folder becuase there is already a thing with the same name,
                    // that way the thing can be renamed.
                    CreateDirectory(tempDir, true);
                    TarFile.ExtractToDirectory(sourceFile, tempDir, false);

                    // Rename thing in file and move it out of the temp directory
                    Directory.Move(tempDir + "\\" + thingName, destinationDirectory + "\\" + suggestedThingName);
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

            // If the .ArgoPoject file was renamed
            if (thingName != extractedDir)
            {
                RenameFolder(destinationDirectory + "\\" + extractedDir, destinationDirectory + "\\" + thingName);
            }

            MakeDirectoryHidden(destinationDirectory + "\\" + thingName);

            return thingName;
        }
        /// <summary>
        /// Reads a TAR file and returns the name of the top-most directory or file.
        /// </summary>
        /// <returns>The name of the top-most directory found in the TAR file, or null if no directories are found.</returns>
        public static string GetTopDirectoryFromTarFile(string sourceFile)
        {
            using FileStream fs = new(sourceFile, FileMode.Open);
            using TarReader reader = new(fs);
            TarEntry entry;
            while ((entry = reader.GetNextEntry()) != null)
            {
                if (entry.EntryType == TarEntryType.Directory)
                {
                    string[] pathSegments = entry.Name.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
                    if (pathSegments.Length > 0)
                    {
                        return pathSegments[0];
                    }
                }
            }

            return "";
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
            CopyFile(argoCompany_file, tarName + fileExtension);

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