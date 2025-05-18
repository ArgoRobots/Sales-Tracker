using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Sales_Tracker
{
    public partial class Log_Form : Form
    {
        // Properties
        private static Log_Form _instance;

        // Getters
        public static Log_Form Instance => _instance;

        // Init
        public Log_Form()
        {
            InitializeComponent();
            _instance = this;

            ThemeManager.SetThemeForForm(this);
            LanguageManager.UpdateLanguageForControl(this);

            // Hide caret
            RichTextBox.MouseDown += (_, _) =>
            {
                HideCaret(RichTextBox.Handle);
            };

            // Select "Enable autoscroll"
            AutoScroll_ComboBox.SelectedIndex = 0;

            AnimateButtons();

            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        public void AnimateButtons()
        {
            CustomControls.AnimateButtons([Clear_Button], Properties.Settings.Default.AnimateButtons);
        }

        // Form event handlers
        private void Log_form_Load(object sender, EventArgs e)
        {
            RichTextBox.Text = Log.LogText;
        }
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
            RichTextBox.Clear();
        }
        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            // Set autoscroll
            if (AutoScroll_ComboBox.Text == "Enable autoscroll")
            {
                RichTextBox.SelectionStart = RichTextBox.Text.Length;
                RichTextBox.ScrollToCaret();
            }

            // Set the time to gray
            // https://stackoverflow.com/questions/74134680/how-to-select-text-between-two-characters-in-a-richtextbox
            MatchCollection matches = MyRegex().Matches(RichTextBox.Text);
            foreach (Match m in matches.Cast<Match>())
            {
                RichTextBox.SelectionStart = m.Index;
                RichTextBox.SelectionLength = m.Length;
                RichTextBox.SelectionColor = CustomColors.GrayText;
            }

            // Set colors
            string text = "[Error]";
            int start = 0;
            int end = RichTextBox.Text.LastIndexOf(text);
            while (start < end)
            {
                RichTextBox.Find(text, start, RichTextBox.TextLength, RichTextBoxFinds.None);
                RichTextBox.SelectionColor = CustomColors.AccentRed;
                start = RichTextBox.Text.IndexOf(text, start) + 1;
            }

            text = "[Debug]";
            start = 0;
            end = RichTextBox.Text.LastIndexOf(text);
            while (start < end)
            {
                RichTextBox.Find(text, start, RichTextBox.TextLength, RichTextBoxFinds.None);
                RichTextBox.SelectionColor = Color.Aqua;
                start = RichTextBox.Text.IndexOf(text, start) + 1;
            }

            text = "[General]";
            start = 0;
            end = RichTextBox.Text.LastIndexOf(text);
            while (start < end)
            {
                RichTextBox.Find(text, start, RichTextBox.TextLength, RichTextBoxFinds.None);
                RichTextBox.SelectionColor = Color.Aqua;
                start = RichTextBox.Text.IndexOf(text, start) + 1;
            }

            text = "[Product manager]";
            start = 0;
            end = RichTextBox.Text.LastIndexOf(text);
            while (start < end)
            {
                RichTextBox.Find(text, start, RichTextBox.TextLength, RichTextBoxFinds.None);
                RichTextBox.SelectionColor = Color.Aqua;
                start = RichTextBox.Text.IndexOf(text, start) + 1;
            }

            // Remove selection
            RichTextBox.SelectionLength = 0;
        }

        // Caret
        [LibraryImport("user32.dll", EntryPoint = "HideCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool HideCaret(IntPtr hWnd);

        [GeneratedRegex("<*\\d+:\\d+:\\d+:\\d+>*", RegexOptions.Multiline)]
        private static partial Regex MyRegex();
    }
}