using Sales_Tracker.Classes;

namespace Sales_Tracker.ImportSpreadSheets
{
    public partial class ImportSpreadSheets_Form : Form
    {
        // Init.
        public ImportSpreadSheets_Form()
        {
            InitializeComponent();
            Theme.SetThemeForForm(this);

            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        // Form event handlers
        private void ImportSpreadSheets_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
    }
}