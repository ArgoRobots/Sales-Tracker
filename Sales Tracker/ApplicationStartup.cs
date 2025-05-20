using Microsoft.Win32;
using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    /// <summary>
    /// Handles application startup tasks including file associations and opening files from command line.
    /// </summary>
    public static class ApplicationStartup
    {
        /// <summary>
        /// This method performs essential startup tasks including:
        /// - Setting up visual styles and high DPI mode
        /// - Starting the theme change detection
        /// - Registering file associations (only on first run)
        /// </summary>
        public static void InitializeApplication()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            ThemeChangeDetector.StartListeningForThemeChanges();
            RegisterFileAssociationOnFirstRun();
        }

        /// <summary>
        /// Registers file associations only on first run of the application.
        /// </summary>
        private static void RegisterFileAssociationOnFirstRun()
        {
            try
            {
                // Force re-registration for testing
                bool forceRegistration = true;  // Set to true for testing, then change back to false
                bool hasRegistered = false;

                if (!forceRegistration)
                {
                    using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\ArgoSalesTracker", false);
                    if (key != null)
                    {
                        hasRegistered = key.GetValue("AssociationsRegistered", false) as bool? ?? false;
                    }
                }

                if (forceRegistration || !hasRegistered)
                {
                    // Get application path for file association
                    string applicationPath = Application.ExecutablePath;

                    ArgoFiles.RegisterFileIcon(ArgoFiles.ArgoCompanyFileExtension, Properties.Resources.ArgoColor, 0);

                    using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ArgoSalesTracker");
                    key?.SetValue("AssociationsRegistered", true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error registering file associations: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles opening a file directly from command line arguments.
        /// </summary>
        /// <returns>True if file was successfully opened, false otherwise</returns>
        public static bool TryOpenFileFromCommandLine(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    return false;
                }

                if (!File.Exists(args[0]))
                {
                    return false;
                }

                string extension = Path.GetExtension(args[0]);

                if (!extension.Equals(ArgoFiles.ArgoCompanyFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Initialize directories
                Directories.SetUniversalDirectories();
                Directories.EnsureAppDataDirectoriesExist();
                DotEnv.Load();

                LanguageManager.InitLanguageManager();

                // Open the company file directly
                string filePath = args[0];
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                string projectName = Path.GetFileNameWithoutExtension(fileName);

                // Use the non-UI version of the method here
                //if (!ArgoCompany.OnlyAllowOneInstanceOfACompany(projectName))
                //{
                //    return false;
                //}

                // Open the company
                Directories.SetDirectories(directory, projectName);
                ArgoCompany.InitThings();

                // Skip the password step for now, or use a non-UI version if needed
                /*
                if (!PasswordManager.EnterPasswordNonUI())
                {
                    ArgoCompany.ApplicationMutex?.Dispose();  // Reset
                    return false;
                }
                */

                // Save to recent files and import
                DataFileManager.AppendValue(GlobalAppDataSettings.RecentCompanies, filePath);
                List<string> dirNames = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.AppData_dir);
                Directories.ImportArgoTarFile(filePath, Directories.AppData_dir, Directories.ImportType.ArgoCompany, dirNames, false);
                DataFileManager.SetValue(AppDataSettings.ChangesMade, false.ToString());

                CustomColors.SetColors();
                CustomControls.ConstructRightClickRename();
                LoadingPanel.InitBlankLoadingPanel();
                LoadingPanel.InitLoadingPanel();

                // Create and show MainMenu
                Application.Run(new MainMenu_Form());

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}