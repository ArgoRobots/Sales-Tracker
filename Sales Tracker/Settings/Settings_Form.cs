using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings
{
    public partial class Settings_Form : Form
    {
        // Properties
        private readonly Form FormGeneral = new General_Form();
        private readonly Form FormSecurity = new Security_Form();
        private readonly Form FormUpdates = new Updates_Form();

        // Init.
        public Settings_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            UpdateTheme();
            General_Button.PerformClick();
        }
        private void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private void Settings_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Left menu buttons
        private Guna2Button btnSelected;

        private void GeneralButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormGeneral, sender);
        }
        private void SecurityButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormSecurity, sender);
        }
        private void UpdatesButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormUpdates, sender);
        }

        // Bottom buttons
        private void ResetToDefault_Button_Click(object sender, EventArgs e)
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "All user settings will be reset to default.", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);
            if (result == CustomMessageBoxResult.Ok)
            {
                UserSettings.ResetAllToDefault();
                ApplyChanges();
            }
        }
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }
        private void ApplyChanges()
        {
            UpdateColorTheme();
            UserSettings.SaveUserSettings();
        }
        private void UpdateColorTheme()
        {
            if (General_Form.Instance.ColorTheme_ComboBox.Text != Theme.CurrentTheme.ToString())
            {
                if (General_Form.Instance.ColorTheme_ComboBox.Text == Theme.ThemeType.Dark.ToString())
                {
                    Theme.CurrentTheme = Theme.ThemeType.Dark;
                }
                else if (General_Form.Instance.ColorTheme_ComboBox.Text == Theme.ThemeType.Light.ToString())
                {
                    Theme.CurrentTheme = Theme.ThemeType.Light;
                }
                else
                {
                    Theme.CurrentTheme = Theme.ThemeType.Windows;
                }

                CustomColors.SetColors();

                General_Form.Instance.UpdateTheme();
                Security_Form.Instance.UpdateTheme();
                Updates_Form.Instance.UpdateTheme();
                MainMenu_Form.Instance.UpdateTheme();
                UpdateTheme();

                MainMenu_Form.Instance.LoadCharts();

                List<Guna2Panel> listOfMenus = [
                    CustomControls.fileMenu,
                    CustomControls.helpMenu,
                    CustomControls.accountMenu,
                    CustomControls.ControlDropDown_Panel,
                    MainMenu_Form.Instance.rightClickDataGridView_Panel,
                    GetStarted_Form.Instance.rightClickOpenRecent_Panel];

                Theme.UpdateThemeForPanel(listOfMenus);

                MainMenu_Form.Instance.rightClickDataGridView_DeleteBtn.ForeColor = CustomColors.accent_red;

                if (Theme.CurrentTheme == Theme.ThemeType.Dark)
                {
                    CustomControls.Rename_TextBox.HoverState.BorderColor = Color.White;
                    CustomControls.Rename_TextBox.FocusedState.BorderColor = Color.White;
                    CustomControls.Rename_TextBox.BorderColor = Color.White;
                }
                else
                {
                    CustomControls.Rename_TextBox.HoverState.BorderColor = Color.Black;
                    CustomControls.Rename_TextBox.FocusedState.BorderColor = Color.Black;
                    CustomControls.Rename_TextBox.BorderColor = Color.Black;
                }

                SearchBox.SearchResultBoxContainer.FillColor = CustomColors.controlBack;
                SearchBox.SearchResultBox.FillColor = CustomColors.controlBack;
            }
        }

        // Misc.
        private void SwitchForm(Form form, object btnSender)
        {
            CustomControls.CloseAllPanels(null, null);
            Guna2Button btn = (Guna2Button)btnSender;

            // If btn is not already selected
            if (btn.FillColor != CustomColors.accent_blue)
            {
                // Unselect button
                if (btnSelected != null)
                {
                    btnSelected.FillColor = CustomColors.controlBack;
                    if (Theme.CurrentTheme == Theme.ThemeType.Dark)
                    {
                        btnSelected.ForeColor = Color.White;
                    }
                    else
                    {
                        btnSelected.ForeColor = Color.Black;
                    }
                }

                // Select new button
                btn.FillColor = CustomColors.accent_blue;
                btn.ForeColor = Color.White;
                form.BringToFront();

                // Show form
                form.TopLevel = false;
                form.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                form.Dock = DockStyle.Fill;
                form.Visible = true;
                FormBack_Panel.Controls.Clear();
                FormBack_Panel.Controls.Add(form);
                form.BringToFront();

                // Save
                btnSelected = btn;
            }
        }
    }
}