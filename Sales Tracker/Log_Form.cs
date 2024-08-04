using Sales_Tracker.Classes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Sales_Tracker
{
    public partial class Log_Form : Form
    {
        // Init
        public static Log_Form Instance { get; private set; }
        public Log_Form()
        {
            InitializeComponent();
            Instance = this;

            LoadingPanel.ShowLoadingPanel(this);
            Theme.SetThemeForForm(this);

            // Hide caret
            RichTextBox.MouseDown += (sender, e) =>
            {
                HideCaret(RichTextBox.Handle);
            };

            // Select "Enable autoscroll"
            AutoScroll_ComboBox.SelectedIndex = 0;
        }

        // Form event handlers
        private void Log_form_Load(object sender, EventArgs e)
        {
            RichTextBox.Text = Log.logText;
        }
        private void Log_form_Shown(object sender, EventArgs e)
        {
            BtnClear.Focus();  // Remove the caret (blinking text cursor)

            LoadingPanel.HideLoadingPanel(this);
        }

        // Event handlers
        private void RichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Tools.OpenLink(e.LinkText);
        }

        // Caret
        [LibraryImport("user32.dll", EntryPoint = "HideCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool HideCaret(IntPtr hWnd);

        // Controls
        private void BtnClear_Click(object sender, EventArgs e)
        {
            RichTextBox.Clear();
        }
        [GeneratedRegex("<*\\d+:\\d+:\\d+:\\d+>*", RegexOptions.Multiline)]
        private static partial Regex MyRegex();
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
                RichTextBox.SelectionColor = CustomColors.grayText;
            }

            // Set colors
            string text = "[Error]";
            int start = 0;
            int end = RichTextBox.Text.LastIndexOf(text);
            while (start < end)
            {
                RichTextBox.Find(text, start, RichTextBox.TextLength, RichTextBoxFinds.None);
                RichTextBox.SelectionColor = CustomColors.accent_red;
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
    }
}