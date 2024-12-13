using Sales_Tracker.Passwords;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;
using System.Text;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Static class responsible for managing company data files, project state, and file operations.
    /// </summary>
    public static class ArgoCompany
    {
        private static Mutex? _applicationMutex = null;

        /// <summary>
        /// Static mutex used to ensure only one instance of a project can be open at a time.
        /// </summary>
        public static Mutex? ApplicationMutex
        {
            get => _applicationMutex;
            private set => _applicationMutex = value;
        }

        /// <summary>
        /// Initializes core components including encryption, cache files, and password management.
        /// Run this when the program is started.
        /// </summary>
        public static void InitThings()
        {
            EncryptionManager.Initialize();
            InitCacheFiles();
            PasswordManager.Password = EncryptionManager.GetPasswordFromFile(Directories.ArgoCompany_file, EncryptionManager.AesKey, EncryptionManager.AesIV);
        }

        /// <summary>
        /// Initializes cache directory and global application settings file.
        /// Creates necessary directories and files if they don't exist.
        /// </summary>
        public static void InitCacheFiles()
        {
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }

            if (!File.Exists(Directories.GlobalAppDataSettings_file))
            {
                Directories.CreateFile(Directories.GlobalAppDataSettings_file);

                DataFileManager.SetValue(GlobalAppDataSettings.ImportSpreadsheetTutorial, bool.TrueString);
            }
        }

        /// <summary>
        /// Saves all current company data to a tar file and resets change tracking.
        /// </summary>
        public static void SaveAll()
        {
            Directories.CreateArgoTarFileFromDirectory(Directories.TempCompany_dir, Directories.ArgoCompany_file, true);
            Log.Write(2, $"Saved '{Directories.CompanyName}'");
            ResetChanges();

            DataFileManager.SetValue(AppDataSettings.ChangesMade, false.ToString());
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Prompts user to select a location and saves the company data to a new location.
        /// </summary>
        public static void SaveAs()
        {
            using SaveFileDialog dialog = new();
            dialog.FileName = Directories.CompanyName;
            dialog.DefaultExt = ArgoFiles.ArgoCompanyFileExtension;
            dialog.Filter = $"Argo Company Files|*{ArgoFiles.ArgoCompanyFileExtension}";
            dialog.Title = "Save Company As";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Create a tar file from the temp directory directly to the new location
                Directories.CreateArgoTarFileFromDirectory(Directories.TempCompany_dir, dialog.FileName, true);
                Log.Write(2, $"Saved '{Path.GetFileNameWithoutExtension(dialog.FileName)}' to new location");
            }
        }

        /// <summary>
        /// Checks if any changes have been made across all forms in the application.
        /// </summary>
        /// <returns>True if any changes have been made, false otherwise.</returns>
        public static bool AreAnyChangesMade()
        {
            if (MainMenu_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                MainMenu_Form.SettingsThatHaveChangedInFile.Count > 0 ||
                Accountants_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                Categories_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                Companies_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                Products_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                AddSale_Form.ThingsThatHaveChangedInFile.Count > 0 ||
                AddPurchase_Form.ThingsThatHaveChangedInFile.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets all change tracking collections across all forms.
        /// </summary>
        public static void ResetChanges()
        {
            MainMenu_Form.ThingsThatHaveChangedInFile.Clear();
            MainMenu_Form.SettingsThatHaveChangedInFile.Clear();
            Accountants_Form.ThingsThatHaveChangedInFile.Clear();
            Categories_Form.ThingsThatHaveChangedInFile.Clear();
            Companies_Form.ThingsThatHaveChangedInFile.Clear();
            Products_Form.ThingsThatHaveChangedInFile.Clear();
            AddSale_Form.ThingsThatHaveChangedInFile.Clear();
            AddPurchase_Form.ThingsThatHaveChangedInFile.Clear();
        }

        /// <summary>
        /// Opens a dialog for selecting and opening an existing company file.
        /// </summary>
        public static void OpenCompany()
        {
            // Select file
            OpenFileDialog dialog = new()
            {
                Filter = $"Argo company (*{ArgoFiles.ArgoCompanyFileExtension})|*{ArgoFiles.ArgoCompanyFileExtension}"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!OnlyAllowOneInstanceOfAProject(Path.GetFileNameWithoutExtension(dialog.FileName)))
                {
                    _applicationMutex?.Dispose();  // Reset
                    return;
                }

                if (!Open(Directory.GetParent(dialog.FileName).FullName, dialog.FileName))
                {
                    return;
                }
                GetStarted_Form.Instance.ShowMainMenu();
            }
        }

        /// <summary>
        /// Opens a company file at the specified location.
        /// </summary>
        /// <returns>True if successfully opened, false otherwise</returns>
        private static bool Open(string filePath, string name)
        {
            Directories.SetDirectories(filePath, Path.GetFileNameWithoutExtension(name));
            InitThings();

            if (!PasswordManager.EnterPassword())
            {
                _applicationMutex?.Dispose();  // Reset
                return false;
            }

            // Save recently opened projects
            DataFileManager.AppendValue(GlobalAppDataSettings.RecentProjects, Directories.ArgoCompany_file);

            // Import company data
            List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.AppData_dir);
            Directories.ImportArgoTarFile(Directories.ArgoCompany_file, Directories.AppData_dir, Directories.ImportType.ArgoCompany, listOfDirectories, false);
            DataFileManager.SetValue(AppDataSettings.ChangesMade, false.ToString());

            return true;
        }

        /// <summary>
        /// Renames the company project and updates all associated files and directories.
        /// </summary>
        public static void Rename(string name)
        {
            string newFile = Directories.ArgoCompany_dir + name + ArgoFiles.ArgoCompanyFileExtension;
            string newDir = Directories.AppData_dir + name;

            // Rename in file
            Directories.MoveFile(Directories.ArgoCompany_file, newFile);
            Directories.RenameFolder(Directories.TempCompany_dir, newDir);

            Directories.SetDirectories(Directories.ArgoCompany_dir, name);

            // Update recently opened projects
            DataFileManager.AppendValue(GlobalAppDataSettings.RecentProjects, Directories.ArgoCompany_file);
        }

        /// <summary>
        /// Clears all cached data and displays the amount of space cleared.
        /// </summary>
        public static void ClearCache()
        {
            string directoryPath = Directories.Cache_dir;
            long totalSizeInBytes = 0;

            if (Directory.Exists(directoryPath))
            {
                totalSizeInBytes = Directories.CalculateDirectorySize(directoryPath);
                Directories.DeleteDirectory(directoryPath, true);
            }

            string totalSizeReadable = Tools.ConvertBytesToReadableSize(totalSizeInBytes);

            if (totalSizeInBytes > 0)
            {
                CustomMessageBox.Show("Cleared cache", $"Cleared {totalSizeReadable} of cached data", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
            else
            {
                CustomMessageBox.Show("No cache to clear", $"No cache to clear", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
        }

        /// <summary>
        /// Opens a new project when another project is already open. Handles saving current project changes if necessary.
        /// </summary>
        public static void OpenProjectWhenAProgramIsAlreadyOpen()
        {
            // Select file
            OpenFileDialog dialog = new()
            {
                Filter = $"Argo company (*{ArgoFiles.ArgoCompanyFileExtension})|*{ArgoFiles.ArgoCompanyFileExtension}"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenProject(dialog.FileName);
            }
        }

        /// <summary>
        /// Opens a new project from the specified file path when another project is already open.
        /// Handles saving current project changes if necessary.
        /// </summary>
        public static void OpenProject(string filePath)
        {
            // Validate the file path
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                CustomMessageBox.Show("Project does not exist", "The specified project file does not exist.", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return;
            }

            // If this project is already open
            if (Directories.ArgoCompany_file == filePath)
            {
                CustomMessageBox.Show("Project already open", "This project is already open", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return;
            }

            // Save current project
            if (AreAnyChangesMade())
            {
                string message = "Would you like to save your changes before opening a new project?";
                CustomMessageBoxResult result = CustomMessageBox.Show("Save changes", message, CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

                switch (result)
                {
                    case CustomMessageBoxResult.Save:
                        SaveAll();
                        break;
                    case CustomMessageBoxResult.DontSave:
                        // Do nothing so the hidden directory is deleted
                        break;
                    case CustomMessageBoxResult.Cancel:
                        return;
                    default:  // If the CustomMessageBox was closed
                        return;
                }
            }

            Directories.DeleteDirectory(Directories.TempCompany_dir, true);

            if (!Open(Directory.GetParent(filePath).FullName, filePath))
            {
                return;
            }

            MainMenu_Form.IsProgramLoading = true;
            MainMenu_Form.Instance.ResetData();
            MainMenu_Form.Instance.LoadData();
            MainMenu_Form.IsProgramLoading = false;

            MainMenu_Form.UpdateMainMenuFormText(MainMenu_Form.Instance);
            MainMenu_Form.Instance.SetCompanyLabel();

            MainMenu_Form.Instance.UpdateTotalLabels();
            MainMenu_Form.Instance.LoadOrRefreshMainCharts();
            MainMenu_Form.Instance.HideShowingResultsForLabel();

            bool hasVisibleRows = AreRowsVisible(MainMenu_Form.Instance.Purchase_DataGridView) &&
                                  AreRowsVisible(MainMenu_Form.Instance.Sale_DataGridView);

            LabelManager.ManageNoDataLabelOnControl(hasVisibleRows, MainMenu_Form.Instance.SelectedDataGridView);
        }

        /// <summary>
        /// Checks if any rows in the given DataGridView are visible.
        /// </summary>
        /// <returns>True if any rows are visible, false otherwise</returns>
        private static bool AreRowsVisible(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ensures only one instance of a project can be open at a time.
        /// </summary>
        /// <returns>True if this is the only instance, false if another instance exists</returns>
        public static bool OnlyAllowOneInstanceOfAProject(string projectFilePath)
        {
            if (!CreateMutex(projectFilePath))
            {
                CustomMessageBox.Show("Already open",
                    "This project is already open in another instance of Argo Sales Tracker",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                _applicationMutex?.Dispose();  // Reset
                return false;
            }
            return true;
        }

        /// <summary>
        /// Only allow one instance of a project to be open at a time
        /// </summary>
        /// <returns>
        /// True if a mutex is created. False if a mutex already exists.
        /// </returns>
        public static bool CreateMutex(string projectFilePath)
        {
            string uniqueMutexName = "Global\\MyApplication_" + GetUniqueProjectIdentifier(projectFilePath);
            _applicationMutex = new Mutex(initiallyOwned: true, name: uniqueMutexName, out bool createdNew);

            if (createdNew)
            {
                Log.Write(1, $"Created Mutex: {uniqueMutexName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates a unique identifier for a project based on its file path.
        /// </summary>
        /// <returns>Base64 encoded unique identifier</returns>
        public static string GetUniqueProjectIdentifier(string projectFilePath)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(projectFilePath));
        }

        /// <summary>
        /// This will prompt the user to recover any unsaved work.
        /// </summary>
        /// <returns>
        /// True if unsaved work was recovered. False if no work was recovered.
        /// </returns>
        public static void RecoverUnsavedWork()
        {
            List<string> projects = Directories.GetListOfAllDirectoriesInDirectory(Directories.AppData_dir);

            foreach (string project in projects)
            {
                if (project + @"\" == Directories.Cache_dir) { continue; }

                // Check if there are any changes
                string? value = DataFileManager.GetValue(AppDataSettings.ChangesMade, project + @"\info" + ArgoFiles.TxtFileExtension);
                if (bool.TryParse(value, out bool boolResult) && !boolResult)
                {
                    // Delete the temp folder
                    Directories.DeleteDirectory(project, true);
                    return;
                }

                CustomMessageBoxResult result = CustomMessageBox.Show("Unsaved work found",
                    $"Unsaved work was found. Would you like to recover it? {Path.GetFileName(project)}",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                if (result == CustomMessageBoxResult.Yes)
                {
                    CustomMessageBox.Show("Select folder",
                        $"You will be promted to select a folder to save the unsaved work.",
                        CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);

                    // Select folder
                    Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Directories.SetDirectories(dialog.SelectedPath, Path.GetFileNameWithoutExtension(project));

                        InitThings();
                        SaveAll();

                        // Delete the temp folder
                        Directories.DeleteDirectory(project, true);
                    }
                }
                else if (result == CustomMessageBoxResult.No)
                {
                    // Delete the temp folder
                    Directories.DeleteDirectory(project, true);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of valid recent project paths from the global application data settings.
        /// </summary>
        /// <returns>A list of valid project file paths.</returns>
        public static List<string> GetValidRecentProjectPaths(bool excludeCurrentCompany)
        {
            string? value = DataFileManager.GetValue(GlobalAppDataSettings.RecentProjects);
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }

            string[] projectPaths = value.Split(',');
            Array.Reverse(projectPaths);  // Reverse the array so it loads in the correct order

            string? currentCompanyPath = null;
            if (excludeCurrentCompany)
            {
                currentCompanyPath = Directories.ArgoCompany_file;
            }

            // Remove duplicates (case-insensitive), filter valid paths, and optionally exclude the current company
            List<string> validProjectPaths = projectPaths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(File.Exists)
                .Where(path => !excludeCurrentCompany || !string.Equals(path, currentCompanyPath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return validProjectPaths;
        }
    }
}