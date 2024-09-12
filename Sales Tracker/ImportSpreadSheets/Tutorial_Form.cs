using Sales_Tracker.Classes;

namespace Sales_Tracker.ImportSpreadsheets
{
    public partial class Tutorial_Form : Form
    {
        // Init.
        public Tutorial_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);
            Theme.SetThemeForForm(this);

            LoadVideo();
        }
        private void LoadVideo()
        {
            // https://stackoverflow.com/questions/73795000/how-do-i-display-a-youtube-video-in-webviewer#73795057

            string url = "https://www.youtube.com/watch?v=5aCbWqKl-wU";
            string html = "<html style='width: 100%; height: 100%; margin: 0; padding: 0;'><head>";
            html += "<meta content='IE=Edge' http-equiv='X-UA-Compatible'/>";
            html += "</head><body style='width: 100%; height: 100%; margin: 0; padding: 0;'>";
            html += "<iframe id='video' src='https://www.youtube.com/embed/{0}' style=\"padding: 0px; width: 100%; height: 100%; border: none; display: block;\"></iframe>";
            html += "</body></html>";
            WebBrowser.DocumentText = string.Format(html, url.Split('=')[1]);
        }

        // Form event handlers
        private void Tutorial_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Next_Button_Click(object sender, EventArgs e)
        {
            DataFileManager.SetValue(Directories.AppDataConfig_file, DataFileManager.GlobalAppDataSettings.ImportSpreadsheetTutorial, bool.FalseString);
            DataFileManager.Save(Directories.AppDataConfig_file);

            Setup_Form.Instance.FormImport.BringToFront();
        }
        private void YouTube_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("https://www.youtube.com/channel/UCNvyzuUPinKr6wZojmQTUuA");
        }
        private void Documentation_LinkLabel_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("");
        }
    }
}