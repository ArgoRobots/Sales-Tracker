using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Sales_Tracker.ImportSpreadsheet
{
    public partial class Tutorial_Form : Form
    {
        // Init.
        public Tutorial_Form()
        {
            InitializeComponent();

            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(Next_Button);
        }

        // Form event handlers
        private void Tutorial_Form_Shown(object sender, EventArgs e)
        {
            VideoPlayer.LoadVideo(WebBrowser, "https://www.youtube.com/watch?v=5aCbWqKl-wU");
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Next_Button_Click(object sender, EventArgs e)
        {
            DataFileManager.SetValue(DataFileManager.GlobalAppDataSettings.ImportSpreadsheetTutorial, bool.FalseString);

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