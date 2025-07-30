using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Sales_Tracker
{
    public partial class Log_Form : BaseForm
    {
        // Properties
        private static Log_Form _instance;
        private Dictionary<string, Color> _translatedLogLevelColors;

        // Getters
        public static Log_Form Instance => _instance;

        // Init
        public Log_Form()
        {
            InitializeComponent();
            _instance = this;

            ThemeManager.SetThemeForForm(this);
            ThemeManager.CustomizeScrollBar(Log_RichTextBox);

            Log_RichTextBox.Text = Log.LogText;
            Log_RichTextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;

            InitializeTranslatedLogLevels();
            LanguageManager.UpdateLanguageForControl(this);
            Log_RichTextBox.Text = TranslateExistingLogText(Log_RichTextBox.Text);

            // Hide caret
            Log_RichTextBox.MouseDown += (_, _) =>
            {
                HideCaret(Log_RichTextBox.Handle);
            };

            AnimateButtons();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void InitializeTranslatedLogLevels()
        {
            _translatedLogLevelColors = new Dictionary<string, Color>
            {
                { Log.GetTranslatedFormattedCategory(LogCategory.Error), CustomColors.AccentRed },
                { Log.GetTranslatedFormattedCategory(LogCategory.Debug), CustomColors.DebugText },
                { Log.GetTranslatedFormattedCategory(LogCategory.General), CustomColors.DebugText },
                { Log.GetTranslatedFormattedCategory(LogCategory.ProductManager), CustomColors.DebugText },
                { Log.GetTranslatedFormattedCategory(LogCategory.PasswordManager), CustomColors.DebugText }
            };
        }
        private static string TranslateExistingLogText(string logText)
        {
            if (string.IsNullOrEmpty(logText))
            {
                return logText;
            }

            if (Properties.Settings.Default.Language == "English")
            {
                return logText;  // No translation needed for English
            }

            // Match log entries: <timestamp> [LogLevel] message
            Regex logEntryPattern = LogEntryRegex();

            // Dictionary of English log levels to their translated versions
            Dictionary<string, string> logLevelTranslations = new()
            {
                ["[Error]"] = Log.GetTranslatedFormattedCategory(LogCategory.Error),
                ["[Debug]"] = Log.GetTranslatedFormattedCategory(LogCategory.Debug),
                ["[General]"] = Log.GetTranslatedFormattedCategory(LogCategory.General),
                ["[Product manager]"] = Log.GetTranslatedFormattedCategory(LogCategory.ProductManager),
                ["[Password manager]"] = Log.GetTranslatedFormattedCategory(LogCategory.PasswordManager)
            };

            MatchCollection matches = logEntryPattern.Matches(logText);
            string result = logText;

            foreach (Match match in matches)
            {
                string timestamp = match.Groups[1].Value;
                string logLevel = match.Groups[2].Value;
                string message = match.Groups[3].Value;

                // Translate the log level
                string translatedLogLevel = logLevel;
                if (logLevelTranslations.TryGetValue(logLevel, out string translatedLevel))
                {
                    translatedLogLevel = translatedLevel;
                }

                // For existing messages, try basic translation but don't expect placeholders
                string translatedMessage = LanguageManager.TranslateString(message);

                string replacement = $"{timestamp} {translatedLogLevel} {translatedMessage}";
                result = result.Replace(match.Value, replacement);
            }

            return result;
        }
        public void AnimateButtons()
        {
            CustomControls.AnimateButtons([Clear_Button], Properties.Settings.Default.AnimateButtons);
        }
        public void RefreshLogColoring()
        {
            InitializeTranslatedLogLevels();
            ApplyColorsToLogs();
        }

        // Form event handlers
        private void Log_form_Shown(object sender, EventArgs e)
        {
            Clear_Button.Focus();  // Remove the caret (blinking text cursor)
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void RichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Tools.OpenLink(e.LinkText);
        }
        private void ClearButton_Click(object sender, EventArgs e)
        {
            Log_RichTextBox.Clear();
        }
        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            ApplyColorsToLogs();
        }

        // Methods
        private void ApplyColorsToLogs()
        {
            // Temporarily disable redrawing to improve performance
            Log_RichTextBox.SuspendLayout();

            // Store current selection
            int originalStart = Log_RichTextBox.SelectionStart;
            int originalLength = Log_RichTextBox.SelectionLength;

            // Set the time to gray
            MatchCollection matches = TimeRegex().Matches(Log_RichTextBox.Text);
            foreach (Match m in matches.Cast<Match>())
            {
                Log_RichTextBox.SelectionStart = m.Index;
                Log_RichTextBox.SelectionLength = m.Length;
                Log_RichTextBox.SelectionColor = CustomColors.GrayText;
            }

            // Apply colors for each translated log level
            if (_translatedLogLevelColors != null)
            {
                foreach (KeyValuePair<string, Color> logLevel in _translatedLogLevelColors)
                {
                    ApplyColorToLogLevel(logLevel.Key, logLevel.Value);
                }
            }

            // Restore original selection
            Log_RichTextBox.SelectionStart = originalStart;
            Log_RichTextBox.SelectionLength = originalLength;

            Log_RichTextBox.ResumeLayout();
        }
        private void ApplyColorToLogLevel(string logLevelText, Color color)
        {
            if (string.IsNullOrEmpty(logLevelText)) { return; }

            int start = 0;
            int lastIndex = Log_RichTextBox.Text.LastIndexOf(logLevelText);

            while (start <= lastIndex)
            {
                int index = Log_RichTextBox.Text.IndexOf(logLevelText, start);
                if (index == -1) { break; }

                Log_RichTextBox.SelectionStart = index;
                Log_RichTextBox.SelectionLength = logLevelText.Length;
                Log_RichTextBox.SelectionColor = color;

                start = index + 1;
            }
        }

        // Caret
        [LibraryImport("user32.dll", EntryPoint = "HideCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool HideCaret(IntPtr hWnd);

        // Regex for timestamp format: <14:20:07.98>
        [GeneratedRegex(@"<\d+:\d+:\d+\.\d+>", RegexOptions.Multiline)]
        private static partial Regex TimeRegex();

        // Regex for log entry format: <timestamp> [LogLevel] message
        [GeneratedRegex(@"^(<\d+:\d+:\d+\.\d+>)\s*(\[[^\]]+\])\s*(.*)$", RegexOptions.Multiline)]
        private static partial Regex LogEntryRegex();
    }
}