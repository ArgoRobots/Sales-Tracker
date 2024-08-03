using Sales_Tracker.Startup.Menus;
using System.Text;

namespace Sales_Tracker.Classes
{
    internal static class ArgoCompany
    {
        public static void InitThings()
        {
            EncryptionHelper.Initialize();
            InitDataFile();
        }
        public static void InitDataFile()
        {
            if (!Directory.Exists(Directories.appData_dir))
            {
                Directories.CreateDirectory(Directories.appData_dir, false);
            }
            if (!File.Exists(Directories.appDataCongig_file))
            {
                Directories.CreateFile(Directories.appDataCongig_file);

                DataFileManager.SetValue(Directories.appDataCongig_file, DataFileManager.GlobalAppDataSettings.RPTutorial, bool.TrueString);
                DataFileManager.Save(Directories.appDataCongig_file);
            }
        }

        public static void SaveAll()
        {
            Directories.CreateArgoTarFileFromDirectory(Directories.tempCompany_dir, Directories.argoCompany_file, true);
            Log.Write(2, $"Saved '{Directories.companyName}'");
            ResetChanges();
            CustomMessage_Form.AddChangesMadeToInfoFile(false);
        }
        public static void SaveAs()
        {
            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Save normally
                SaveAll();

                // Copy the project to a new location
                Directories.CopyFile(Directories.argoCompany_file, dialog.SelectedPath + Directories.companyName + ArgoFiles.ArgoCompanyFileExtension);
            }
        }
        public static bool AreAnyChangesMade()
        {
            if (MainMenu_Form.thingsThatHaveChangedInFile.Count > 0 ||
                Accountants_Form.thingsThatHaveChangedInFile.Count > 0 ||
                Categories_Form.thingsThatHaveChangedInFile.Count > 0 ||
                Companies_Form.thingsThatHaveChangedInFile.Count > 0 ||
                Products_Form.thingsThatHaveChangedInFile.Count > 0 ||
                AddSale_Form.thingsThatHaveChangedInFile.Count > 0 ||
                AddPurchase_Form.thingsThatHaveChangedInFile.Count > 0)
            {
                return true;
            }
            return false;
        }
        public static void ResetChanges()
        {
            MainMenu_Form.thingsThatHaveChangedInFile.Clear();
            Accountants_Form.thingsThatHaveChangedInFile.Clear();
            Categories_Form.thingsThatHaveChangedInFile.Clear();
            Companies_Form.thingsThatHaveChangedInFile.Clear();
            Products_Form.thingsThatHaveChangedInFile.Clear();
            AddSale_Form.thingsThatHaveChangedInFile.Clear();
            AddPurchase_Form.thingsThatHaveChangedInFile.Clear();
        }
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
                    return;
                }

                // Save new ProjectDirectory
                Properties.Settings.Default.ProjectDirectory = Directory.GetParent(dialog.FileName).FullName;
                Properties.Settings.Default.Save();

                Directories.SetDirectories(Properties.Settings.Default.ProjectDirectory, Path.GetFileNameWithoutExtension(dialog.FileName));
                InitThings();

                // Save recently opened projects
                DataFileManager.AppendValue(Directories.appDataCongig_file, DataFileManager.GlobalAppDataSettings.RecentProjects, Directories.argoCompany_file);
                DataFileManager.Save(Directories.appDataCongig_file);

                List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.appData_dir);
                Directories.ImportArgoTarFile(Directories.argoCompany_file, Directories.appData_dir, Directories.ImportType.ArgoCompany, listOfDirectories, false);
                DataFileManager.SetValue(Directories.info_file, DataFileManager.AppDataSettings.ChangesMade, false.ToString());

                GetStarted_Form.Instance.ShowMainMenu();
            }
        }
        public static void Rename(string name)
        {
            string newFile = Directories.argoCompany_dir + name + ArgoFiles.ArgoCompanyFileExtension;
            string newDir = Directories.appData_dir + name;

            // Rename in file
            Directories.MoveFile(Directories.argoCompany_file, newFile);
            Directories.RenameFolder(Directories.tempCompany_dir, newDir);

            Directories.SetDirectories(Directories.argoCompany_dir, name);

            // Update recently opened projects
            DataFileManager.AppendValue(Directories.appDataCongig_file, DataFileManager.GlobalAppDataSettings.RecentProjects, Directories.argoCompany_file);
            DataFileManager.Save(Directories.appDataCongig_file);
        }
        public static void OpenProjectWhenAProgramIsAlreadyOpen()
        {
            // Select file
            OpenFileDialog dialog = new()
            {
                Filter = $"Argo company (*{ArgoFiles.ArgoCompanyFileExtension})|*{ArgoFiles.ArgoCompanyFileExtension}"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // If this project is already open
                if (Directories.argoCompany_file == dialog.FileName)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", "This project is already open", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    return;
                }

                // Save current project
                if (AreAnyChangesMade())
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "Would you like to save your changes before opening a new project?", CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

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

                Directories.DeleteDirectory(Directories.tempCompany_dir, true);

                // Save new ProjectDirectory
                Properties.Settings.Default.ProjectDirectory = Directory.GetParent(dialog.FileName).FullName;
                Properties.Settings.Default.Save();

                Directories.SetDirectories(Properties.Settings.Default.ProjectDirectory, Path.GetFileNameWithoutExtension(dialog.FileName));
                InitThings();

                // Save recently opened projects
                DataFileManager.AppendValue(Directories.appDataCongig_file, DataFileManager.GlobalAppDataSettings.RecentProjects, Directories.argoCompany_file);
                DataFileManager.Save(Directories.appDataCongig_file);

                List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.appData_dir);
                Directories.ImportArgoTarFile(Directories.argoCompany_file, Directories.appData_dir, Directories.ImportType.ArgoCompany, listOfDirectories, false);
                DataFileManager.SetValue(Directories.info_file, DataFileManager.AppDataSettings.ChangesMade, false.ToString());
            }
        }

        private static Mutex? applicationMutex = null;
        public static bool OnlyAllowOneInstanceOfAProject(string projectFilePath)
        {
            if (!CreateMutex(projectFilePath))
            {
                CustomMessageBox.Show("Argo Sales Tracker", "This project is already open in another instance of Argo Studio", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                applicationMutex?.Dispose();
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
            applicationMutex = new Mutex(initiallyOwned: true, name: uniqueMutexName, out bool createdNew);

            if (createdNew)
            {
                Log.Write(1, $"Created Mutex: {uniqueMutexName}");
                return true;
            }
            return false;
        }
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
            List<string> projects = Directories.GetListOfAllDirectoriesInDirectory(Directories.appData_dir);

            foreach (string project in projects)
            {
                // Check if there are any changes
                string? value = DataFileManager.GetValue(project + @"\info" + ArgoFiles.TxtFileExtension, DataFileManager.AppDataSettings.ChangesMade);
                if (bool.TryParse(value, out bool boolResult) && !boolResult)
                {
                    // Delete the temp folder
                    Directories.DeleteDirectory(project, true);
                    return;
                }

                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", $"Unsaved work was found. Would you like to recover it? {Path.GetFileName(project)}", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);
                if (result == CustomMessageBoxResult.Yes)
                {
                    CustomMessageBox.Show("Argo Sales Tracker", $"You will be promted to select a folder to save the unsaved work.", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);

                    // Select folder
                    Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save new ProjectDirectory
                        Properties.Settings.Default.ProjectDirectory = dialog.SelectedPath;
                        Properties.Settings.Default.Save();

                        Directories.SetDirectories(Properties.Settings.Default.ProjectDirectory, Path.GetFileNameWithoutExtension(project));

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
    }
}