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
            if (Properties.Settings.Default.ShowDebugInfo && index == 1)
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

            // Add debug info to message box
            if (Properties.Settings.Default.ShowDebugInfo)
            {
                message += debugInfo;
            }

            // Show error
            if (showMessageBox)
            {
                CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
            }
        }

        // File errors
        public static void Error_FileDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-3vknm9: File does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_DirectoryDoesNotExist(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-tq45ek: Directory does not exist:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_FileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-djrr3r: File already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_DirectoryAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-cmr45a: Directory already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_DestinationFileAlreadyExists(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-8g8we7: The destination file already exists:" +
                $"\n'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_TheSourceAndDestinationAreTheSame(
            string source,
            string destination,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-h88tzd: The source and destination files are the same." +
                $"\nSource:'{source}'." +
                $"\nDestination: '{destination}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_FailedToWriteToFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-h88tzd: Failed to write to file:" +
                $"\nSource:'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_FailedToReadFile(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-h88tzd: Failed to read the file:" +
                $"\nSource:'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_FailedToSave(
            string filePath,
            [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-5knt54: Failed to save:" +
                $"\nSource:'{filePath}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }

        // DataGridView errors
        public static void Error_RowIsEmpty(
          string dataGridViewName,
          [CallerLineNumber] int lineNumber = 0)
        {
            Error($"Error-5knt54: Row is empty in DataGridView:'{dataGridViewName}'.",
                "https://www.google.com",
                false,
                lineNumber);
        }
        public static void Error_RowIsOutOfRange(
          [CallerLineNumber] int lineNumber = 0)
        {
            Error("Error-5knt54: Row is out of range",
                "https://www.google.com",
                false,
                lineNumber);
        }
    }
}