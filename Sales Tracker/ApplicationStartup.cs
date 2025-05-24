using Microsoft.Win32;
using Sales_Tracker.Classes;
using Sales_Tracker.Passwords;
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

            CustomColors.SetColors();

            TextBoxManager.ConstructRightClickTextBoxMenu();
            SearchBox.ConstructSearchBox();

            LoadingPanel.InitBlankLoadingPanel();
            LoadingPanel.InitLoadingPanel();

            Directories.SetUniversalDirectories();
            Directories.EnsureAppDataDirectoriesExist();
            CustomControls.ConstructRightClickRename();

            DotEnv.Load();
            LanguageManager.InitLanguageManager();

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
                string subKey = @"Software\ArgoSalesTracker";
                bool hasRegistered = false;

                using RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey, false);
                if (key != null)
                {
                    hasRegistered = key.GetValue("AssociationsRegistered", false) as bool? ?? false;
                }

                if (!hasRegistered)
                {
                    ArgoFiles.RegisterFileIcon(ArgoFiles.ArgoCompanyFileExtension, Properties.Resources.ArgoColor, 0);

                    using RegistryKey writeKey = Registry.CurrentUser.CreateSubKey(subKey);
                    writeKey?.SetValue("AssociationsRegistered", true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error registering file associations: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles opening a company directly from command line arguments.
        /// </summary>
        /// <returns>True if file was successfully opened, false otherwise</returns>
        public static bool TryOpenCompanyFromCommandLine(string[] args)
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

                // Open the company file directly
                string filePath = args[0];
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                string projectName = Path.GetFileNameWithoutExtension(fileName);

                if (!ArgoCompany.OnlyAllowOneInstanceOfACompany(projectName))
                {
                    return false;
                }

                // Open the company
                Directories.SetDirectories(directory, projectName);
                ArgoCompany.InitThings();

                if (!PasswordManager.EnterPassword())
                {
                    return false;
                }

                // Save to recent files and import
                DataFileManager.AppendValue(GlobalAppDataSettings.RecentCompanies, filePath);
                List<string> dirNames = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.AppData_dir);
                Directories.ImportArgoTarFile(filePath, Directories.AppData_dir, Directories.ImportType.ArgoCompany, dirNames, false);
                DataFileManager.SetValue(AppDataSettings.ChangesMade, false.ToString());

                MainMenu_Form form = new();
                form.Show();

                return true;
            }
            catch (Exception ex)
            {
                // Show a regular MessageBox in case CustomMessageBox is unavailable
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}