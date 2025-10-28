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
        // Properties
        private List<string> _templateNames = [];
        public string SelectedTemplateName { get; private set; }

        // Init.
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
            ThemeManager.UpdateDataGridViewTheme(Templates_DataGridView);
        }
        private void LoadTemplates()
        {
            _templateNames = CustomTemplateStorage.GetCustomTemplateNames();

            Templates_DataGridView.Rows.Clear();
            foreach (string templateName in _templateNames)
            {
                Templates_DataGridView.Rows.Add(templateName);
            }

            if (_templateNames.Count == 0)
            {
                NoTemplates_Label.Visible = true;
            }
            else
            {
                NoTemplates_Label.Visible = false;
            }
            NoTemplates_Label.BackColor = CustomColors.ControlBack;
            NoTemplates_Label.ForeColor = CustomColors.Text;
        }
        private void UpdateButtonStates()
        {
            bool hasSelection = Templates_DataGridView.SelectedRows.Count > 0;
            Load_Button.Enabled = hasSelection;
            Delete_Button.Enabled = hasSelection;
            Rename_Button.Enabled = hasSelection;
        }

        // Form event handlers
        private void CustomTemplateManager_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Templates_DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }
        private void Templates_DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Load_Button_Click(sender, e);
            }
        }
        private void Load_Button_Click(object sender, EventArgs e)
        {
            if (Templates_DataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            int selectedIndex = Templates_DataGridView.SelectedRows[0].Index;
            SelectedTemplateName = _templateNames[selectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        }
        private void Delete_Button_Click(object sender, EventArgs e)
        {
            if (Templates_DataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            int selectedIndex = Templates_DataGridView.SelectedRows[0].Index;
            string templateName = _templateNames[selectedIndex];

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
                    ReportDataSelection_Form.Instance.RefreshTemplates();
                }
                else
                {
                    CustomMessageBox.Show(
                        "Delete Failed",
                        $"Failed to delete template '{templateName}'.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok);
                }
            }
        }
        private void Rename_Button_Click(object sender, EventArgs e)
        {
            if (Templates_DataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            int selectedIndex = Templates_DataGridView.SelectedRows[0].Index;
            string oldTemplateName = _templateNames[selectedIndex];

            // Show SaveTemplate_Form dialog for new name
            SaveTemplate_Form renameForm = new()
            {
                CurrentTemplateName = oldTemplateName
            };

            if (renameForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string newTemplateName = renameForm.TemplateName;

            // Check if user entered empty name or same name
            if (string.IsNullOrWhiteSpace(newTemplateName) || newTemplateName == oldTemplateName)
            {
                return;
            }

            // Check if a template with the new name already exists
            if (_templateNames.Contains(newTemplateName))
            {
                CustomMessageBox.Show(
                    "Template Exists",
                    $"A template named '{newTemplateName}' already exists.\nPlease choose a different name.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
                return;
            }

            // Rename the template
            if (CustomTemplateStorage.RenameTemplate(oldTemplateName, newTemplateName))
            {
                LoadTemplates();
                UpdateButtonStates();
                ReportDataSelection_Form.Instance.RefreshTemplates();

                // Select the renamed template
                int index = _templateNames.IndexOf(newTemplateName);
                if (index >= 0)
                {
                    Templates_DataGridView.ClearSelection();
                    Templates_DataGridView.Rows[index].Selected = true;
                }
            }
            else
            {
                CustomMessageBox.Show(
                    "Rename Failed",
                    $"Failed to rename template '{oldTemplateName}' to '{newTemplateName}'.",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok);
            }
        }
        private void Close_Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
