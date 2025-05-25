using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.Theme;
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

        // Getters and setters
        public static Settings_Form Instance => _instance;

        // Init.
        public Settings_Form()
        {
            InitializeComponent();
            _instance = this;

            UpdateTheme();
            General_Button.PerformClick();
            AnimateButtons();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBluePrimary(Ok_Button);
            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
            ThemeManager.MakeGButtonBlueSecondary(Apply_Button);
        }
        public void AnimateButtons()
        {
            IEnumerable<Guna2Button> buttons =
            [
               General_Button,
               Security_Button,
               Updates_Button,
               ResetToDefault_Button,
               Apply_Button,
            ];
            CustomControls.AnimateButtons(buttons, Properties.Settings.Default.AnimateButtons);
        }

        // Form event handlers
        private void Settings_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Settings_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            CustomControls.CloseAllPanels();
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
            CustomControls.CloseAllPanels();

            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Reset settings", "All settings will be reset to default.",
                CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);

            if (result == CustomMessageBoxResult.Ok)
            {
                UserSettings.ResetAllToDefault();
                ApplyChanges(true);
            }
        }
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            ApplyChanges(false);
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            ApplyChanges(true);
        }
        private static void ApplyChanges(bool includeGeneralForm)
        {
            UpdateColorTheme();
            UserSettings.SaveUserSettings(includeGeneralForm);
            Security_Form.Instance.CenterEncryptControls();  // In case the language changes
        }
        private static void UpdateColorTheme()
        {
            string selectedTheme = General_Form.Instance.ColorTheme_ComboBox.Text;

            // If the theme did not change
            if (selectedTheme == ThemeManager.CurrentTheme.ToString())
            {
                return;
            }

            if (selectedTheme == ThemeManager.ThemeType.Dark.ToString())
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Dark;
            }
            else if (selectedTheme == ThemeManager.ThemeType.Light.ToString())
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Light;
            }
            else // Windows theme
            {
                ThemeManager.CurrentTheme = ThemeManager.ThemeType.Windows;
            }

            CustomColors.SetColors();
            FormThemeManager.UpdateAllForms();
            ThemeManager.UpdateOtherControls();
            MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, $"Changed the 'color theme' setting to {selectedTheme}");
        }

        // Misc.
        private void SwitchForm(Form form, object btnSender)
        {
            CustomControls.CloseAllPanels();
            Guna2Button button = (Guna2Button)btnSender;

            // If button is already selected
            if (button.FillColor == CustomColors.AccentBlue)
            {
                return;
            }

            // Unselect button
            if (selectedButton != null)
            {
                selectedButton.FillColor = CustomColors.ControlBack;
                if (ThemeManager.IsDarkTheme())
                {
                    selectedButton.ForeColor = Color.White;
                }
                else
                {
                    selectedButton.ForeColor = Color.Black;
                }
            }

            // Select new button
            button.FillColor = CustomColors.AccentBlue;
            button.ForeColor = Color.White;
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
            selectedButton = button;
        }
    }
}