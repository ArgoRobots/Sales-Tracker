using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator.Menus
{
    /// <summary>
    /// Form for managing custom report templates.
    /// </summary>
    public partial class CustomTemplateManager_Form : Form
    {
        private List<string> _templateNames = [];
        public string SelectedTemplateName { get; private set; }

        public CustomTemplateManager_Form()
        {
            InitializeComponent();

            UpdateTheme();
            LoadTemplates();
            UpdateButtonStates();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        private void UpdateTheme()
        {
            ThemeManager.MakeGButtonBluePrimary(Load_Button);
            ThemeManager.SetThemeForForm(this);
            ThemeManager.CustomizeScrollBar(Templates_ListBox);
        }

        public void AlignControlsAfterLanguageChange()
        {
            Title_Label.Left = (ClientSize.Width - Title_Label.Width) / 2;
        }

        private void LoadTemplates()
        {
            _templateNames = CustomTemplateStorage.GetCustomTemplateNames();

            Templates_ListBox.Items.Clear();
            foreach (string templateName in _templateNames)
            {
                Templates_ListBox.Items.Add(templateName);
            }

            if (_templateNames.Count == 0)
            {
                NoTemplates_Label.Visible = true;
            }
            else
            {
                NoTemplates_Label.Visible = false;
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = Templates_ListBox.SelectedIndex >= 0;
            Load_Button.Enabled = hasSelection;
            Delete_Button.Enabled = hasSelection;
        }

        private void CustomTemplateManager_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        private void Templates_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void Templates_ListBox_DoubleClick(object sender, EventArgs e)
        {
            if (Templates_ListBox.SelectedIndex >= 0)
            {
                Load_Button_Click(sender, e);
            }
        }

        private void Load_Button_Click(object sender, EventArgs e)
        {
            if (Templates_ListBox.SelectedIndex < 0)
            {
                return;
            }

            SelectedTemplateName = _templateNames[Templates_ListBox.SelectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Delete_Button_Click(object sender, EventArgs e)
        {
            if (Templates_ListBox.SelectedIndex < 0)
            {
                return;
            }

            string templateName = _templateNames[Templates_ListBox.SelectedIndex];

            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Delete Template",
                $"Are you sure you want to delete the template '{templateName}'?\nThis action cannot be undone.",
                CustomMessageBoxIcon.Question,
                CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                if (CustomTemplateStorage.DeleteTemplate(templateName))
                {
                    LoadTemplates();
                    UpdateButtonStates();
                }
                else
                {
                    CustomMessageBox.Show(
                        "Delete Failed",
                        $"Failed to delete template '{templateName}'.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.OK);
                }
            }
        }

        private void Close_Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
