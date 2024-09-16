using Sales_Tracker.Classes;

namespace Sales_Tracker.ImportSpreadsheets
{
    public partial class Tutorial_Form : Form
    {
        // Init.
        public Tutorial_Form()
        {
            InitializeComponent();

            Theme.SetThemeForForm(this);
            VideoPlayer.LoadVideo(WebBrowser, "https://www.youtube.com/watch?v=5aCbWqKl-wU");

            LoadingPanel.ShowBlankLoadingPanel(this);
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