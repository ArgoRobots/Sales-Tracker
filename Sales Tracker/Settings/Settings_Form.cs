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
        private string _originalLanguage;

        // Getters and setters
        public static Settings_Form Instance => _instance;

        // Init.
        public Settings_Form()
        {
            InitializeComponent();
            _instance = this;
            _originalLanguage = Properties.Settings.Default.Language;

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
        private async void ResetToDefault_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();

            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Reset settings", "All settings will be reset to default.",
                CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);

            if (result == CustomMessageBoxResult.Ok)
            {
                UserSettings.ResetAllToDefault();
                await ApplyChanges(true);
            }
        }
        private async void Ok_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            await ApplyChanges(false);
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private async void Apply_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            await ApplyChanges(true);
        }

        /// <summary>
        /// Applies all setting changes, including language translation if the language was changed.
        /// </summary>
        private async Task ApplyChanges(bool includeGeneralForm)
        {
            UserSettings.SaveUserSettings();

            if (HasLanguageChanged())
            {
                await UpdateLanguageAsync(includeGeneralForm);
                Security_Form.Instance.CenterEncryptControls();
            }
        }
        private async Task UpdateLanguageAsync(bool includeGeneralForm)
        {
            try
            {
                LoadingPanel.ShowLoadingScreen(this, "Translating application to new language...");

                using CancellationTokenSource cts = new();

                await Task.Run(async () =>
                {
                    try
                    {
                        await LanguageManager.TranslateAllApplicationFormsAsync(includeGeneralForm, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Write(1, "Language translation was cancelled");
                    }
                }, cts.Token).ConfigureAwait(false);

                _originalLanguage = General_Form.Instance.Language_TextBox.Text;
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation(ex.Message);
            }
            finally
            {
                this.InvokeIfRequired(() =>
                {
                    LoadingPanel.HideLoadingScreen(this);
                });
            }
        }
        private bool HasLanguageChanged()
        {
            string currentLanguage = General_Form.Instance.Language_TextBox.Text;
            return !string.Equals(_originalLanguage, currentLanguage, StringComparison.OrdinalIgnoreCase);
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