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
            NetSparkleUpdateManager.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            CustomColors.SetColors();
            LoadingPanel.InitBlankLoadingPanel();
            LoadingPanel.InitLoadingPanel();

            Directories.SetUniversalDirectories();
            Directories.EnsureAppDataDirectoriesExist();
            ArgoCompany.InitCacheFiles();
            EncryptionManager.Initialize();
            DotEnv.Load();
            PasswordManager.Password = EncryptionManager.GetPasswordFromFile(Directories.ArgoCompany_file, EncryptionManager.AesKey, EncryptionManager.AesIV);
            LanguageManager.InitLanguageManager();

            TextBoxManager.ConstructRightClickTextBoxMenu();
            SearchBox.ConstructSearchBox();
            CustomControls.ConstructRightClickRename();

            _ = Task.Run(EnsureDefaultLanguageTranslationsAsync);

            ThemeChangeDetector.StartListeningForThemeChanges();
            RegisterFileAssociationOnFirstRun();
        }

        /// <summary>
        /// Ensures that translations for the default language are available.
        /// Downloads them if the translations file is missing or doesn't contain the required language.
        /// </summary>
        private static async Task EnsureDefaultLanguageTranslationsAsync()
        {
            try
            {
                string defaultLanguage = Properties.Settings.Default.Language;

                // If default language is English, no need to download translations
                if (string.IsNullOrEmpty(defaultLanguage) || defaultLanguage.Equals("English", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Get the language abbreviation for the default language
                List<KeyValuePair<string, string>> languages = LanguageManager.GetLanguages();
                KeyValuePair<string, string> languageInfo = languages.FirstOrDefault(l => l.Key.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(languageInfo.Value))
                {
                    Log.Write(1, $"Unknown default language: {defaultLanguage}");
                    return;
                }

                string languageAbbreviation = languageInfo.Value;

                // Check if translations are already available for this language
                bool translationsAvailable = LanguageManager.TranslationCache != null &&
                                             LanguageManager.TranslationCache.ContainsKey(languageAbbreviation) &&
                                             LanguageManager.TranslationCache[languageAbbreviation].Count > 0;

                if (!translationsAvailable)
                {
                    Log.Write(1, $"Translations file missing or empty for default language '{defaultLanguage}'. Downloading...");

                    // Download the language file for the default language
                    bool downloadSuccess = await LanguageManager.DownloadAndMergeLanguageJson(defaultLanguage);

                    // Apply the translations to any open forms if the startup forms are still shown
                    if (MainMenu_Form.Instance == null)
                    {
                        foreach (Form form in Application.OpenForms)
                        {
                            LanguageManager.UpdateLanguageForControl(form);
                        }
                    }

                    if (downloadSuccess)
                    {
                        Log.Write(1, $"Successfully downloaded translations for default language '{defaultLanguage}'");
                    }
                    else
                    {
                        Log.Write(1, $"Failed to download translations for default language '{defaultLanguage}'. App will use original text.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error ensuring default language translations: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers Argo file associations if they have not already been registered.
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
                // If the command is invalid
                if (args.Length == 0 || !File.Exists(args[0]))
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

        /// <summary>
        /// Auto-opens the most recent company after an update if the flag is set.
        /// </summary>
        /// <returns>True if a company was automatically opened, false otherwise</returns>
        public static bool TryAutoOpenRecentCompanyAfterUpdate()
        {
            try
            {
                // Check if we should auto-open the most recent company
                string? autoOpenFlag = DataFileManager.GetValue(GlobalAppDataSettings.AutoOpenRecentAfterUpdate);
                if (!bool.TryParse(autoOpenFlag, out bool shouldAutoOpen) || !shouldAutoOpen)
                {
                    return false;
                }

                // Clear the flag
                DataFileManager.SetValue(GlobalAppDataSettings.AutoOpenRecentAfterUpdate, bool.FalseString);

                // Get the most recent company
                List<string> recentCompanies = ArgoCompany.GetValidRecentCompanyPaths(excludeCurrentCompany: false);
                if (recentCompanies.Count == 0)
                {
                    return false;
                }

                string mostRecentCompany = recentCompanies[0];

                return TryOpenCompanyFromCommandLine([mostRecentCompany]);
            }
            catch
            {
                Log.Write(0, $"Error reopening the company after an update");
                return false;
            }
        }
    }
}