using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Settings
{
    public partial class Settings_Form : Form
    {
        // Properties
        private static Settings_Form _instance;
        private readonly Form FormGeneral = new General_Form();
        private readonly Form FormSecurity = new Security_Form();
        private readonly Form FormUpdates = new Updates_Form();
        private bool _isFormClosing = false;

        // Getters and setters
        public static Settings_Form Instance => _instance;
        public bool IsFormClosing
        {
            get => _isFormClosing;
            set => _isFormClosing = value;
        }

        // Init.
        public Settings_Form()
        {
            InitializeComponent();
            _instance = this;

            UpdateTheme();
            General_Button.PerformClick();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.MakeGButtonBluePrimary(Ok_Button);
            Theme.MakeGButtonBlueSecondary(Cancel_Button);
            Theme.MakeGButtonBlueSecondary(Apply_Button);
        }

        // Form event handlers
        private void Settings_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Settings_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            SearchBox.CloseSearchBox();
            _isFormClosing = true;
        }

        // Left menu buttons
        private Guna2Button selectedButton;
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
            SearchBox.CloseSearchBox();

            CustomMessageBoxResult result = CustomMessageBox.Show("Reset settings", "All settings will be reset to default.", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);
            if (result == CustomMessageBoxResult.Ok)
            {
                UserSettings.ResetAllToDefault();
                ApplyChanges(true);
            }
        }
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
            ApplyChanges(false);
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
            ApplyChanges(true);
        }
        private void ApplyChanges(bool includeGeneralForm)
        {
            UpdateColorTheme();
            UserSettings.SaveUserSettings(includeGeneralForm);
            Security_Form.Instance.CenterEncryptControls();
        }
        private void UpdateColorTheme()
        {
            // If the theme did not change
            if (General_Form.Instance.ColorTheme_ComboBox.Text == Theme.CurrentTheme.ToString())
            {
                return;
            }

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

            MainMenu_Form.Instance.LoadOrRefreshMainCharts();

            List<Guna2Panel> listOfPanels = MainMenu_Form.GetMenus();

            foreach (Guna2Panel guna2Panel in listOfPanels)
            {
                guna2Panel.FillColor = CustomColors.PanelBtn;
                guna2Panel.BorderColor = CustomColors.ControlPanelBorder;

                if (guna2Panel.Controls[0] is FlowLayoutPanel flowLayoutPanel)
                {
                    flowLayoutPanel.BackColor = CustomColors.MainBackground;
                }
            }

            Theme.UpdateThemeForPanel(listOfPanels);
            Theme.SetRightArrowImageBasedOnTheme(CustomControls.OpenRecentCompany_Button);

            // Update other controls
            Theme.SetThemeForControl([CustomControls.ControlsDropDown_Button, MainMenu_Form.TimeRangePanel]);

            DataGridViewManager.RightClickDataGridView_DeleteBtn.ForeColor = CustomColors.AccentRed;

            // Set the border to white or black, depending on the theme
            CustomControls.Rename_TextBox.HoverState.BorderColor = CustomColors.Text;
            CustomControls.Rename_TextBox.FocusedState.BorderColor = CustomColors.Text;
            CustomControls.Rename_TextBox.BorderColor = CustomColors.Text;

            SearchBox.SearchResultBoxContainer.FillColor = CustomColors.ControlBack;
            SearchBox.SearchResultBox.FillColor = CustomColors.ControlBack;
        }

        // Misc.
        private void SwitchForm(Form form, object btnSender)
        {
            SearchBox.CloseSearchBox();
            Guna2Button btn = (Guna2Button)btnSender;

            // If btn is already selected
            if (btn.FillColor == CustomColors.AccentBlue)
            {
                return;
            }

            // Unselect button
            if (selectedButton != null)
            {
                selectedButton.FillColor = CustomColors.ControlBack;
                if (Theme.CurrentTheme == Theme.ThemeType.Dark)
                {
                    selectedButton.ForeColor = Color.White;
                }
                else
                {
                    selectedButton.ForeColor = Color.Black;
                }
            }

            // Select new button
            btn.FillColor = CustomColors.AccentBlue;
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
            selectedButton = btn;
        }
    }
}