using System.Runtime.CompilerServices;

namespace Sales_Tracker.Classes
{
    internal class Log
    {
        public static string logText;

        /// <summary>
        ///  0 = [Error], 1 = [Debug], 2 = [General], 3 = [Product manager]
        /// </summary>
        public static void Write(byte index, string text)
        {
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
            }
            newText += text + "\n";
            logText += newText;

            if (Tools.IsFormOpen(typeof(Log_Form)))
            {
                if (Log_Form.Instance.RichTextBox.InvokeRequired)
                {
                    Log_Form.Instance.RichTextBox.Invoke(new Action(() =>
                    {
                        Log_Form.Instance.RichTextBox.AppendText(newText);
                    }));
                }
                else { Log_Form.Instance.RichTextBox.AppendText(newText); }
            }
        }
        private static void Error(
            string message,
            string link,
            bool showMessageBox,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            // Add link
            if (link != "")
            {
                message += "\n\n";
                CustomMessageBoxVariables.LinkStart = message.Length;
                string text = "More information";
                message += text;
                CustomMessageBoxVariables.Link = link;
                CustomMessageBoxVariables.LinkLength = text.Length;
            }

            // Add debug info
            if (Properties.Settings.Default.ShowDebugInfo)
            {
                message += "\n\nDebug info:";
                message += $"\nLine: '{lineNumber}'.";
                message += $"\nCaller: '{caller}'.";
            }

            // Show error
            if (showMessageBox)
            {
                CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }

            // Log error
            Write(0, message);
        }

        // File errors
        public static void Error_FileDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-3vknm9: File does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_DirectoryDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-tq45ek: Directory does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_FileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-djrr3r: File already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_DirectoryAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-cmr45a: Directory already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_SourceDirectoryDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-hx9rxy: The source directory does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_DestinationDirectoryAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error($"Error-9e8gu8: The destination directory already exists '{filePath}'.",
                  "https://www.google.com",
                  false,
                  lineNumber,
                  caller);
        }
        public static void Error_SourceFileDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-gpm3u8: The source file does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_DestinationFileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-8g8we7: The destination file already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_TheSourceAndDestinationAreTheSame(
            string source,
            string destination,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-h88tzd: The source and destination files are the same." +
                $"\nSource:'{source}'." +
                $"\nDestination: '{destination}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_FailedToWriteToFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-h88tzd: Failed to write to file:" +
                $"\nSource:'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void Error_FailedToReadFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-h88tzd: Failed to read the file:" +
                $"\nSource:'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
    }
}