using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Sales_Tracker.Classes
{
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
                    newText += "[Error] ";
                    break;

                case 1:
                    newText += "[Debug] ";
                    break;

                case 2:
                    newText += "[General] ";
                    break;

                case 3:
                    newText += "[Product manager] ";
                    break;

                case 4:
                    newText += "[Password manager] ";
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
        /// Extracts error code from error message (e.g., "Error-3vknm9" from the message)
        /// </summary>
        private static string ExtractErrorCode(string message)
        {
            Match match = ErrorCodeRegex().Match(message);
            return match.Success ? match.Value : "Unknown";
        }

        /// <summary>
        /// Determines error category based on the method name that called the error
        /// </summary>
        private static string DetermineErrorCategory([CallerMemberName] string callerMethod = "")
        {
            return callerMethod switch
            {
                string method when method.Contains("File") => "File",
                string method when method.Contains("DataGridView") => "DataGridView",
                string method when method.Contains("Encryption") => "Encryption",
                string method when method.Contains("API") || method.Contains("ExchangeRate") || method.Contains("Translation") => "API",
                string method when method.Contains("Language") || method.Contains("Translation") => "Translation",
                string method when method.Contains("AnonymousData") => "AnonymousData",
                string method when method.Contains("FileAssociation") => "FileAssociation",
                string method when method.Contains("ENV") => "Environment",
                _ => "General"
            };
        }

        private static void Error(
            string message, string link,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string callerMethod = "")
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
                string errorCategory = DetermineErrorCategory(callerMethod);
                AnonymousDataManager.AddErrorData(errorCode, errorCategory, lineNumber);
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
                lineNumber);
        }
        public static void Error_FileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-djrr3r: File already exists:" +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }
        public static void Error_DestinationFileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-8g8we7: The destination file already exists:" +
                $"\n'{filePath}'.",
                "",
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
                lineNumber);
        }
        public static void Error_WriteToFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-w7f3k2: Failed to write to file:" +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }
        public static void Error_ReadFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-r9d5m1: Failed to read the file:" +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }
        public static void Error_Save(
            string info, string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-s4v8n6: Failed to save. {info}." +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }

        // DataGridView errors
        public static void Error_DataGridViewCellIsEmpty(
            string dataGridViewName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-d3g7c4: Cell is empty in DataGridView:'{dataGridViewName}'.",
                "",
                lineNumber);
        }
        public static void Error_RowIsOutOfRange(
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-r6w9o2: Row is out of range.",
                "",
                lineNumber);
        }

        // Encryption errors
        public static void Error_InitEncryptionHelper(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-i2e5h8: Error initializing EncryptionHelper. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Encryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-e7n9c1: Error during encryption. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Decryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-d4c8r3: Error during decryption. {info}.",
                "",
                lineNumber);
        }

        // API errors
        public static void Error_GetExchangeRate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-g8x2r5: Error getting exchange rates. {info}.",
                "",
                lineNumber);
        }
        public static void Error_TranslationAPIRequestFailed(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t5a7p9: API request failed: {info}.",
                "",
                lineNumber);
        }
        public static void Error_EnhancingSearch(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-e3n6s1: Error enhancing search: {info}.",
                "",
                lineNumber);
        }
        public static void Error_ExportingChart(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-x4p8c2: Failed to export chart: {info}.",
                "",
                lineNumber);
        }

        // Language translation errors
        public static void Error_GetTranslation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-g9t3l7: Error getting the translation. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Translation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-t2l8n4: AI Query Translation failed: {info}.",
                "",
                lineNumber);
        }

        // Anonymous usage data errors
        public static void Error_AnonymousDataCollection(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-a6d5c9: Error collecting anonymous usage data. {info}.",
                "",
                lineNumber);
        }

        // File association errors
        public static void Error_RegisterFileAssociations(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-r7f2a8: Error registering file associations: {info}.",
                "",
                lineNumber);
        }

        // ENV
        public static void Error_ENVFileNotFound(
            string fileName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-v3f6n1: ENV file '{fileName}' not found relative to solution.",
                "",
                lineNumber);
        }
        public static void Error_ENVKeyNotFound(
            string key,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-v8k4n5: ENV key {key} not found.",
                "",
                lineNumber);
        }
    }
}