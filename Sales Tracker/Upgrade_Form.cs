using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Upgrade_Form : Form
    {
        // Init.
        public Upgrade_Form()
        {
            InitializeComponent();
            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Benifits_Panel.BackColor = CustomColors.Background2;
            Theme.MakeGButtonBluePrimary(Upgrade_Button);

            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                Square_ImageButton.Image = Resources.SquareLogoWhite;
            }
            else
            {
                Square_ImageButton.Image = Resources.SquareLogoBlack;
            }
        }

        // Form event handlers
        private void Upgrade_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Upgrade_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("");
        }
        private void LearnMore_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("");
        }
    }
}