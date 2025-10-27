using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator.Menus
{
    /// <summary>
    /// Dialog for saving a custom report template.
    /// </summary>
    public partial class SaveTemplate_Form : Form
    {
        private bool _isUpdating;

        public string TemplateName { get; private set; }

        public SaveTemplate_Form()
        {
            InitializeComponent();

            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        private void UpdateTheme()
        {
            ThemeManager.MakeGButtonBluePrimary(Save_Button);
            ThemeManager.SetThemeForForm(this);
        }

        public void AlignControlsAfterLanguageChange()
        {
            Title_Label.Left = (ClientSize.Width - Title_Label.Width) / 2;
        }

        private void SaveTemplate_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            TemplateName_TextBox.Focus();
        }

        private void Save_Button_Click(object sender, EventArgs e)
        {
            if (_isUpdating) { return; }

            string templateName = TemplateName_TextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(templateName))
            {
                CustomMessageBox.Show(
                    "Invalid Template Name",
                    "Please enter a name for the template.",
                    CustomMessageBoxIcon.Warning,
                    CustomMessageBoxButtons.OK);
                return;
            }

            // Check if template already exists
            if (CustomTemplateStorage.TemplateExists(templateName))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    "Template Exists",
                    $"A template named '{templateName}' already exists.\nDo you want to overwrite it?",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return;
                }
            }

            TemplateName = templateName;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TemplateName_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Save_Button_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                Cancel_Button_Click(sender, e);
            }
        }
    }
}
