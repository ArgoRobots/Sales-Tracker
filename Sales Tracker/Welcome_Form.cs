﻿using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Welcome_Form : Form
    {
        // Init.
        public Welcome_Form()
        {
            InitializeComponent();

            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(Continue_Button);
            LanguageManager.UpdateLanguageForControl(this);
            DontShowAgain_Label.AccessibleDescription = AccessibleDescriptionManager.AlignRightCenter;
            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        // Form event handlers
        private void Welcome_Form_Shown(object sender, EventArgs e)
        {
            VideoPlayer.LoadVideo(WebBrowser, "https://www.youtube.com/watch?v=5aCbWqKl-wU");
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Continue_Button_Click(object sender, EventArgs e)
        {
            if (DontShowAgain_CheckBox.Checked)
            {
                DataFileManager.SetValue(GlobalAppDataSettings.ShowWelcomeForm, bool.FalseString);
            }
            Close();
        }
        private void YouTube_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("https://www.youtube.com/channel/UCNvyzuUPinKr6wZojmQTUuA");
        }

        private void Documentation_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink("");
        }
    }
}