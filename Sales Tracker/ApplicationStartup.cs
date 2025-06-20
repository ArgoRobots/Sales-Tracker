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
            LoadingPanel.InitBlankLoadingPanel();
            LoadingPanel.InitLoadingPanel();

            Directories.SetUniversalDirectories();
            ArgoCompany.InitCacheFiles();
            EncryptionManager.Initialize();
            DotEnv.Load();
            PasswordManager.Password = EncryptionManager.GetPasswordFromFile(Directories.ArgoCompany_file, EncryptionManager.AesKey, EncryptionManager.AesIV);
            LanguageManager.InitLanguageManager();

            TextBoxManager.ConstructRightClickTextBoxMenu();
            SearchBox.ConstructSearchBox();

            Directories.EnsureAppDataDirectoriesExist();
            CustomControls.ConstructRightClickRename();

            ThemeChangeDetector.StartListeningForThemeChanges();
            RegisterFileAssociationOnFirstRun();
        }

        /// <summary>
        /// Registers file associations if they have not already been registered.
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
                Log.Error_RegisterFileAssociations(ex.Message);
            }
        }

        /// <summary>
        /// Handles opening a company directly from command line arguments.
        /// </summary>
        /// <returns>True if file was successfully opened, otherwise false</returns>
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

                // Open the company file directly
                string filePath = args[0];
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                string companyName = Path.GetFileNameWithoutExtension(fileName);

                if (!ArgoCompany.OnlyAllowOneInstanceOfACompany(companyName))
                {
                    return false;
                }

                // Open the company
                Directories.SetDirectories(directory, companyName);

                if (!PasswordManager.EnterPassword())
                {
                    return false;
                }

                // Save recently opened companies
                DataFileManager.AppendValue(GlobalAppDataSettings.RecentCompanies, filePath);

                // Import company data
                List<string> dirNames = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.AppData_dir);
                Directories.ImportArgoTarFile(filePath, Directories.AppData_dir, dirNames, false);
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