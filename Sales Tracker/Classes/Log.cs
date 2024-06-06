using System.Runtime.CompilerServices;

namespace Sales_Tracker.Classes
{
    internal class Log
    {
        public static string logText;

        /// <summary>
        ///  0 = [Error], 1 = [Debug], 2 = [General], 3 = [Machine Programmer], 4 = [Robot Programmer]
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
                    newText += "[Machine Programmer] ";
                    break;

                case 4:
                    newText += "[Robot Programmer] ";
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
                CustomMessageBox.Show("Argo Studio", message, CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
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



        // MachineProgrammer
        public static void FailedToLoadVariables(
           string appName,
           string filePath,
           [CallerLineNumber] int lineNumber = 0,
           [CallerMemberName] string caller = null)
        {
            Error($"Error-7vbn2y: Failed to load variables in app '{appName}' because file does not exist:" +
                $"\n'{filePath}.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void FailedToLoadEvents(
           string appName,
           string filePath,
           [CallerLineNumber] int lineNumber = 0,
           [CallerMemberName] string caller = null)
        {
            Error($"Error-jt56yr: Failed to load events in app '{appName}' because file does not exist:" +
                $"\n'{filePath}.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void FailedToSortAListOfNamesBecauseOfNullOrWhitespace(
         string filePath,
         [CallerLineNumber] int lineNumber = 0,
         [CallerMemberName] string caller = null)
        {
            Error("Error-gyymw9: Failed to sort a list of names because the first line in the file:" +
                $"\n'{filePath}' is null or whitespace.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }


        // MainControls_form errors
        public static void FailedToLoadProgram(
            string selectedProgramName,
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error($"Error-ypum7h: Failed to load program '{selectedProgramName}' because file does not exist:" +
                $"\n'{filePath}.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void FailedToDeleteProgram(
            string selectedLoadedProgramName,
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error($"Error-96s70p: Failed to delete program '{selectedLoadedProgramName}' because file does not exist:" +
                $"\n'{filePath}.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }
        public static void FailedToSaveProgram(
            string selectedLoadedProgramName,
            string filePath,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error("Error-t3skfz: Failed to save program '" + selectedLoadedProgramName + "' because file does not exist:" +
                $"\n'{filePath}.",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }


        // Serial com
        public static void SerialComReceivedIncorrectFormat(
            string serialPortName,
            string robotName,
            string data,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error($"Error-wucgn3: Serial port '{serialPortName} ({robotName})' received a string in an incorrect format:" +
                $"\n'{data}'",
                "https://www.google.com",
                false,
                lineNumber,
                caller);
        }

        public static void SerialComFailedToConnect(
            string serialPortName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Error($"Error-something: Serial port '{serialPortName}' failed to connect.",
                "https://www.google.com",
                true,
                lineNumber,
                caller);
        }
    }
}