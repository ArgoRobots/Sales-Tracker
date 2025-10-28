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
            int selectedCount = Templates_DataGridView.SelectedRows.Count;

            // Load and Rename only work with a single selection
            Load_Button.Enabled = selectedCount == 1;
            Rename_Button.Enabled = selectedCount == 1;

            // Delete works with one or more selections
            Delete_Button.Enabled = selectedCount > 0;
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

            // Get all selected template names
            List<string> selectedTemplateNames = [];
            foreach (DataGridViewRow row in Templates_DataGridView.SelectedRows)
            {
                selectedTemplateNames.Add(_templateNames[row.Index]);
            }

            // Confirm deletion
            string message;
            if (selectedTemplateNames.Count == 1)
            {
                message = $"Are you sure you want to delete the template '{selectedTemplateNames[0]}'?\nThis action cannot be undone.";
            }
            else
            {
                message = $"Are you sure you want to delete {selectedTemplateNames.Count} templates?\nThis action cannot be undone.";
            }

            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Delete Template" + (selectedTemplateNames.Count > 1 ? "s" : ""),
                message,
                CustomMessageBoxIcon.Question,
                CustomMessageBoxButtons.YesNo);

            if (result == CustomMessageBoxResult.Yes)
            {
                int successCount = 0;
                int failCount = 0;

                foreach (string templateName in selectedTemplateNames)
                {
                    if (CustomTemplateStorage.DeleteTemplate(templateName))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                LoadTemplates();
                UpdateButtonStates();
                ReportDataSelection_Form.Instance.RefreshTemplates();

                // Show error message if any deletions failed
                if (failCount > 0)
                {
                    CustomMessageBox.Show(
                        "Delete Failed",
                        $"Failed to delete {failCount} template{(failCount > 1 ? "s" : "")}.",
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
                CurrentTemplateName = oldTemplateName,
                IsRenameMode = true
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
