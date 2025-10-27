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
        // Properties
        public string TemplateName { get; private set; }
        public string CurrentTemplateName { get; set; }
        public bool IsUpdate { get; private set; }

        // Init.
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

        // Form event handlers
        private void SaveTemplate_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);

            // If there's a current template, show the update option
            if (!string.IsNullOrEmpty(CurrentTemplateName))
            {
                UpdateExisting_RadioButton.Visible = true;
                SaveAsNew_RadioButton.Visible = true;
                UpdateExisting_RadioButton.Text = $"Update existing template: '{CurrentTemplateName}'";
                UpdateExisting_RadioButton.Checked = true;
                SaveAsNew_RadioButton.Checked = false;
                TemplateName_TextBox.Text = CurrentTemplateName;
                TemplateName_TextBox.Enabled = false;
            }
            else
            {
                UpdateExisting_RadioButton.Visible = false;
                SaveAsNew_RadioButton.Visible = false;
                TemplateName_TextBox.Enabled = true;
            }

            TemplateName_TextBox.Focus();
        }

        // Event handlers
        private void UpdateExisting_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateExisting_RadioButton.Checked)
            {
                TemplateName_TextBox.Text = CurrentTemplateName;
                TemplateName_TextBox.Enabled = false;
            }
        }
        private void SaveAsNew_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (SaveAsNew_RadioButton.Checked)
            {
                TemplateName_TextBox.Text = "";
                TemplateName_TextBox.Enabled = true;
                TemplateName_TextBox.Focus();
            }
        }
        private void Save_Button_Click(object sender, EventArgs e)
        {
            // Check if we're updating an existing template
            if (UpdateExisting_RadioButton.Visible && UpdateExisting_RadioButton.Checked)
            {
                IsUpdate = true;
                TemplateName = CurrentTemplateName;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            // Saving as new template
            IsUpdate = false;
            string templateName = TemplateName_TextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(templateName))
            {
                CustomMessageBox.Show(
                    "Invalid Template Name",
                    "Please enter a name for the template.",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok);
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
