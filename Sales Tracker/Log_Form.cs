﻿using Sales_Tracker.Classes;
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
        private Dictionary<string, Color> translatedLogLevelColors;

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
            Log_RichTextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;

            InitializeTranslatedLogLevels();

            LanguageManager.UpdateLanguageForControl(this);

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
            translatedLogLevelColors = new Dictionary<string, Color>
            {
                { "[" + LanguageManager.TranslateString("Error") +"]", CustomColors.AccentRed },
                { "[" +  LanguageManager.TranslateString("Debug") +"]", CustomColors.DebugText },
                { "[" +  LanguageManager.TranslateString("General") +"]", CustomColors.DebugText },
                { "[" +  LanguageManager.TranslateString("Product manager") +"]", CustomColors.DebugText }
            };
        }
        public void AnimateButtons()
        {
            CustomControls.AnimateButtons([Clear_Button], Properties.Settings.Default.AnimateButtons);
        }
        public void RefreshLogColoring()
        {
            // Reinitialize translated log levels and reapply coloring
            InitializeTranslatedLogLevels();
            RichTextBox_TextChanged(Log_RichTextBox, EventArgs.Empty);
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
            // Temporarily disable redrawing to improve performance
            Log_RichTextBox.SuspendLayout();

            try
            {
                // Store current selection
                int originalStart = Log_RichTextBox.SelectionStart;
                int originalLength = Log_RichTextBox.SelectionLength;

                // Set the time to gray
                MatchCollection matches = MyRegex().Matches(Log_RichTextBox.Text);
                foreach (Match m in matches.Cast<Match>())
                {
                    Log_RichTextBox.SelectionStart = m.Index;
                    Log_RichTextBox.SelectionLength = m.Length;
                    Log_RichTextBox.SelectionColor = CustomColors.GrayText;
                }

                // Apply colors for each translated log level
                if (translatedLogLevelColors != null)
                {
                    foreach (KeyValuePair<string, Color> logLevel in translatedLogLevelColors)
                    {
                        ApplyColorToLogLevel(logLevel.Key, logLevel.Value);
                    }
                }

                // Restore original selection
                Log_RichTextBox.SelectionStart = originalStart;
                Log_RichTextBox.SelectionLength = originalLength;
            }
            finally
            {
                Log_RichTextBox.ResumeLayout();
            }
        }

        // Methods
        private void ApplyColorToLogLevel(string logLevelText, Color color)
        {
            if (string.IsNullOrEmpty(logLevelText)) return;

            int start = 0;
            int lastIndex = Log_RichTextBox.Text.LastIndexOf(logLevelText);

            while (start <= lastIndex)
            {
                int index = Log_RichTextBox.Text.IndexOf(logLevelText, start);
                if (index == -1) break;

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

        [GeneratedRegex("<*\\d+:\\d+:\\d+:\\d+>*", RegexOptions.Multiline)]
        private static partial Regex MyRegex();
    }
}