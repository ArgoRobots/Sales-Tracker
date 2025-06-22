using System.Runtime.CompilerServices;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Provides logging functionality with different log levels and error handling mechanisms.
    /// </summary>
    internal class Log
    {
        // Properties
        public static string LogText { get; set; }

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
        private static void Error(
            string message, string link,
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
        public static void Error_DirectoryAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-cmr45a: Directory already exists:" +
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
            Error("Error-h88tzd: Failed to write to file:" +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }
        public static void Error_ReadFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-h88tzd: Failed to read the file:" +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }
        public static void Error_Save(
            string info, string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Failed to save. {info}." +
                $"\n'{filePath}'.",
                "",
                lineNumber);
        }

        // DataGridView errors
        public static void Error_DataGridViewCellIsEmpty(
            string dataGridViewName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Cell is empty in DataGridView:'{dataGridViewName}'.",
                "",
                lineNumber);
        }
        public static void Error_RowIsOutOfRange(
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-5knt54: Row is out of range.",
                "",
                lineNumber);
        }
        public static void Error_CannotProcessReceiptColumn(
            string transactionID,
           [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Cannot process receipt column for transaction {transactionID}",
                "",
                lineNumber);
        }

        // Encryption errors
        public static void Error_InitEncryptionHelper(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error initializing EncryptionHelper. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Encryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error during encryption. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Decryption(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error during decryption. {info}.",
                "",
                lineNumber);
        }

        // API errors
        public static void Error_GetExchangeRate(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error getting exchange rates. {info}.",
                "",
                lineNumber);
        }
        public static void Error_TranslationAPIRequestFailed(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: API request failed: {info}.",
                "",
                lineNumber);
        }
        public static void Error_EnhancingSearch(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: Error enhancing search: {info}.",
                "",
                lineNumber);
        }
        public static void Error_ExportingChart(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: Failed to export chart: {info}.",
                "",
                lineNumber);
        }

        // Language translation errors
        public static void Error_EnglishCacheDoesNotExist(
            string controlKey,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error getting the English cache with the value: {controlKey}",
                "",
                lineNumber);
        }
        public static void Error_GetTranslation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Error getting the translation. {info}.",
                "",
                lineNumber);
        }
        public static void Error_Translation(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: AI Query Translation failed: {info}.",
                "",
                lineNumber);
        }

        // Anonymous usage data errors
        public static void Error_AnonymousDataCollection(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: Error collecting anonymous usage data. {info}.",
                "",
                lineNumber);
        }

        // File association errors
        public static void Error_RegisterFileAssociations(
            string info,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: Error registering file associations: {info}.",
                "",
                lineNumber);
        }

        // ENV
        public static void Error_ENVFileNotFound(
            string fileName,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: {fileName} file not found relative to solution.",
                "",
                lineNumber);
        }
        public static void Error_ENVKeyNotFound(
            string key,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-7kp3xz: ENV key {key} not found.",
                "",
                lineNumber);
        }
    }
}