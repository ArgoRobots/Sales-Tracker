using Sales_Tracker.UI;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Defines the categories of errors that can occur in the application.
    /// </summary>
    public enum ErrorCategory
    {
        General,
        File,
        DataGridView,
        Encryption,
        API,
        Translation,
        AnonymousData,
        FileAssociation,
        Environment
    }

    /// <summary>
    /// Provides logging functionality with different log levels and error handling mechanisms.
    /// </summary>
    internal partial class Log
    {
        // Properties
        public static string LogText { get; set; }

        [GeneratedRegex(@"Error-[a-zA-Z0-9]+")]
        private static partial Regex ErrorCodeRegex();

        // Save logs
        public static void SaveLogs()
        {
            Directories.CreateDirectory(Directories.Logs_dir);

            DateTime time = DateTime.Now;
            int count = 0;
            string directory;

            while (true)
            {
                if (count == 0)
                {
                    directory = $@"{Directories.Logs_dir}\{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}{ArgoFiles.TxtFileExtension}";
                }
                else
                {
                    directory = $@"{Directories.Logs_dir}\{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}-{count}{ArgoFiles.TxtFileExtension}";
                }

                if (!File.Exists(directory))
                {
                    Directories.WriteTextToFile(directory, LogText);
                    break;
                }
                count++;
            }

            CleanupOldLogFiles();
        }
        private static void CleanupOldLogFiles()
        {
            byte maxFiles = 30;

            try
            {
                List<FileInfo> logFiles = Directory.GetFiles(Directories.Logs_dir, $"*{ArgoFiles.TxtFileExtension}")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (logFiles.Count > maxFiles)
                {
                    // Remove the oldest files
                    foreach (FileInfo? file in logFiles.Skip(maxFiles))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            LogText += $"Failed to delete old log file {file.FullName}: {ex.Message}\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogText += $"Error during log cleanup: {ex.Message}\n";
            }
        }

        // Writing to log methods
        /// <summary>
        /// Writes a log entry with a specified log level and message.
        /// Log level index: 0 = [Error], 1 = [Debug], 2 = [General], 3 = [Product manager], 4 = [Password manager].
        /// </summary>
        public static void Write(byte index, string text)
        {
            if (!Properties.Settings.Default.ShowDebugInfo && index == 1)
            {
                return;
            }

            string newText = "<" + Tools.FormatTime(DateTime.Now) + "> ";

            switch (index)
            {
                case 0:
                    newText += "[" + LanguageManager.TranslateString("Error") + "] ";
                    break;

                case 1:
                    newText += "[" + LanguageManager.TranslateString("Debug") + "] ";
                    break;

                case 2:
                    newText += "[" + LanguageManager.TranslateString("General") + "] ";
                    break;

                case 3:
                    newText += "[" + LanguageManager.TranslateString("Product manager") + "] ";
                    break;

                case 4:
                    newText += "[" + LanguageManager.TranslateString("Password manager") + "] ";
                    break;
            }
            newText += text + "\n";
            LogText += newText;

            if (Tools.IsFormOpen<Log_Form>())
            {
                Log_Form.Instance.Log_RichTextBox.InvokeIfRequired(() =>
                    Log_Form.Instance.Log_RichTextBox.AppendText(newText)
                );
            }
        }

        /// <summary>
        /// Writes a log entry with formatting support for translation.
        /// Log level index: 0 = [Error], 1 = [Debug], 2 = [General], 3 = [Product manager], 4 = [Password manager].
        /// </summary>
        public static void WriteWithFormat(byte index, string messageTemplate, params object[] args)
        {
            if (!Properties.Settings.Default.ShowDebugInfo && index == 1)
            {
                return;
            }

            string translatedMessage = LanguageManager.TranslateString(messageTemplate);

            // If translation was found and has placeholders, use it
            string finalMessage;
            if (translatedMessage != messageTemplate && translatedMessage.Contains('{'))
            {
                finalMessage = string.Format(translatedMessage, args);
            }
            else
            {
                // Fall back to original template
                finalMessage = string.Format(messageTemplate, args);
            }

            Write(index, finalMessage);
        }

        /// <summary>
        /// Extracts error code from error message (e.g., "Error-3vknm9" from the message)
        /// </summary>
        private static string ExtractErrorCode(string message)
        {
            Match match = ErrorCodeRegex().Match(message);
            return match.Success ? match.Value : "Unknown";
        }

        private static void Error(
            string message,
            string link,
            ErrorCategory category,
            [CallerLineNumber] int lineNumber = 0)
        {
            // Add link
            if (link != "")
            {
                message += "\nMore information: " + link;
                CustomMessageBoxVariables.LinkStart = message.Length - link.Length;
                CustomMessageBoxVariables.Link = link;
                CustomMessageBoxVariables.LinkLength = link.Length;
            }

            // Prepare debug info
            string debugInfo = "\nDebug info:";
            debugInfo += $"\nLine: '{lineNumber}'.";
            debugInfo += $"\nStack trace\n: '{Environment.StackTrace}'.";

            // Log error with debug info
            Write(0, message + debugInfo);

            // Add to anonymous data collection
            try
            {
                string errorCode = ExtractErrorCode(message);
                AnonymousDataManager.AddErrorData(errorCode, category.ToString(), lineNumber);
            }
            catch
            {
                // Silently fail to avoid recursive error logging
            }

            CustomMessageBox.Show("Error", message, CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
        }

        // File errors
        public static void Error_FileDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-3vknm9: File does not exist:" +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_FileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-djrr3r: File already exists:" +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_DestinationFileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-8g8we7: The destination file already exists:" +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_TheSourceAndDestinationAreTheSame(
            string source, string destination,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-h88tzd: The source and destination files are the same." +
                $"\nSource: '{source}'." +
                $"\nDestination: '{destination}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_WriteToFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-w7f3k2: Failed to write to file:" +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_ReadFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-r9d5m1: Failed to read the file:" +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_Save(
            string info, string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s4v8n6: Failed to save. {info}." +
                $"\n'{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }

        // DataGridView errors
        public static void Error_DataGridViewCellIsEmpty(
            string dataGridViewName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-d3g7c4: Cell is empty in DataGridView:'{dataGridViewName}'.",
                "",
                ErrorCategory.DataGridView,
                lineNumber);
        }
        public static void Error_RowIsOutOfRange(
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-r6w9o2: Row is out of range.",
                "",
                ErrorCategory.DataGridView,
                lineNumber);
        }

        // Encryption errors
        public static void Error_InitEncryptionHelper(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-i2e5h8: Error initializing EncryptionHelper. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }
        public static void Error_Encryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-e7n9c1: Error during encryption. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }
        public static void Error_Decryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-d4c8r3: Error during decryption. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }

        // API errors
        public static void Error_GetExchangeRate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-g8x2r5: Error getting exchange rates. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_TranslationAPIRequestFailed(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t5a7p9: API request failed: {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_EnhancingSearch(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-e3n6s1: Error enhancing search: {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_ExportingChart(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-x4p8c2: Failed to export chart: {info}.",
                "",
                ErrorCategory.General,
                lineNumber);
        }

        // Language translation errors
        public static void Error_GetTranslation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-g9t3l7: Error getting the translation. {info}.",
                "",
                ErrorCategory.Translation,
                lineNumber);
        }
        public static void Error_Translation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t2l8n4: AI Query Translation failed: {info}.",
                "",
                ErrorCategory.Translation,
                lineNumber);
        }
        public static void Error_SaveTranslationCache(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t7c4s2: Failed to save translation cache. {info}.",
                "",
                ErrorCategory.Translation,
                lineNumber);
        }

        // Anonymous usage data errors
        public static void Error_AnonymousDataCollection(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-a6d5c9: Error collecting anonymous usage data. {info}.",
                "",
                ErrorCategory.AnonymousData,
                lineNumber);
        }

        // File association errors
        public static void Error_RegisterFileAssociations(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-r7f2a8: Error registering file associations: {info}.",
                "",
                ErrorCategory.FileAssociation,
                lineNumber);
        }

        // ENV errors
        public static void Error_ENVFileNotFound(
            string fileName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-v3f6n1: ENV file '{fileName}' not found relative to solution.",
                "",
                ErrorCategory.Environment,
                lineNumber);
        }
        public static void Error_ENVKeyNotFound(
            string key,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-v8k4n5: ENV key {key} not found.",
                "",
                ErrorCategory.Environment,
                lineNumber);
        }

        // Auto update errors
        public static void Error_InitializeUpdateManager(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-u1i8m3: Error initializing NetSparkleUpdateManager. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_CheckForUpdates(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-u2c7k9: Error checking for updates. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_StartUpdate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-u3s5n2: Error starting update. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_DownloadUpdate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-u4d6w8: Error downloading update. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }
        public static void Error_ApplyUpdate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-u5a4r1: Error applying update and restarting. {info}.",
                "",
                ErrorCategory.API,
                lineNumber);
        }

        // Secrets Manager errors
        public static void Error_CreateEncryptedSecretsFile(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s1c9e4: Error creating encrypted secrets file. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }
        public static void Error_SecretsFileNotFound(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s2f7n8: Encrypted secrets file not found: '{filePath}'.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_DecryptSecretsFile(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s3d2r6: Failed to decrypt secrets file. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }
        public static void Error_LoadEncryptedSecrets(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s4l5k1: Error loading encrypted secrets. {info}.",
                "",
                ErrorCategory.Encryption,
                lineNumber);
        }

        // Application Startup errors
        public static void Error_EnsureDefaultLanguageTranslations(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-a1l7t9: Error ensuring default language translations. {info}.",
                "",
                ErrorCategory.Translation,
                lineNumber);
        }
        public static void Error_OpenCompanyFromCommandLine(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-a2o5c8: Error opening company from command line. {info}.",
                "",
                ErrorCategory.File,
                lineNumber);
        }
        public static void Error_AutoOpenRecentCompanyAfterUpdate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-a3u6r4: Error auto-opening recent company after update. {info}.",
                "",
                ErrorCategory.File,
                lineNumber);
        }

        // Theme Change Detector error
        public static void Error_StartThemeListener(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t1s8l5: Error starting theme listener. {info}.",
                "",
                ErrorCategory.General,
                lineNumber);
        }

        // Registry Watcher error
        public static void Error_RegistryWatcher(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-r9w3k7: Registry watcher error. {info}.",
                "",
                ErrorCategory.General,
                lineNumber);
        }
    }
}