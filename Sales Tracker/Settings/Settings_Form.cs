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
        private CancellationTokenSource _translationCts = new();

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
            LoadingPanel.CancelRequested += OnTranslationCancelled;
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
            LoadingPanel.CancelRequested -= OnTranslationCancelled;

            // Cancel any ongoing translation
            _translationCts?.Cancel();
            _translationCts?.Dispose();

            CustomControls.CloseAllPanels();
        }
        private void OnTranslationCancelled(object sender, EventArgs e)
        {
            _translationCts?.Cancel();
            LoadingPanel.HideLoadingScreen(this);
            CustomMessageBox.Show("Translation Cancelled", "The language translation was cancelled.",
                CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
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
                await ApplyChanges();
            }
        }
        private async void Ok_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            SetButtonsEnabled(false);

            bool success = await ApplyChanges();

            SetButtonsEnabled(true);

            if (success)
            {
                Close();
            }
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private async void Apply_Button_Click(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
            SetButtonsEnabled(false);

            bool success = await ApplyChanges();

            SetButtonsEnabled(true);

            if (success && HasLanguageChanged())
            {
                CustomMessageBox.Show("Translation Complete", "Language has been successfully updated.",
                    CustomMessageBoxIcon.Success, CustomMessageBoxButtons.Ok);
            }
        }

        // Methods
        private void SetButtonsEnabled(bool enabled)
        {
            Ok_Button.Enabled = enabled;
            Apply_Button.Enabled = enabled;
            Cancel_Button.Enabled = enabled;
            ResetToDefault_Button.Enabled = enabled;

            General_Button.Enabled = enabled;
            Security_Button.Enabled = enabled;
            Updates_Button.Enabled = enabled;
        }

        /// <summary>
        /// Applies all setting changes, including language translation if the language was changed.
        /// Returns true if all changes were successfully applied, false otherwise.
        /// </summary>
        private async Task<bool> ApplyChanges()
        {
            try
            {
                bool success = await UserSettings.SaveUserSettingsAsync();

                if (!success)
                {
                    return false; // Settings save was cancelled or failed
                }

                if (HasLanguageChanged())
                {
                    // Dispose of previous cancellation token source
                    _translationCts?.Cancel();
                    _translationCts?.Dispose();

                    // Create a new cancellation token source
                    _translationCts = new CancellationTokenSource();

                    bool languageSuccess = await UpdateLanguageAsync();
                    if (!languageSuccess)
                    {
                        return false; // Language update was cancelled or failed
                    }

                    Security_Form.Instance.CenterEncryptControls();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Error applying settings changes: {ex.Message}");
                return false;
            }
        }
        private async Task<bool> UpdateLanguageAsync()
        {
            try
            {
                LoadingPanel.ShowLoadingScreen(this, "Translating application to new language...", true, _translationCts);

                try
                {
                    string currentLanguage = General_Form.Instance.Language_TextBox.Text;
                    bool success = await LanguageManager.UpdateApplicationLanguage(currentLanguage, _translationCts.Token);

                    if (success && !_translationCts.Token.IsCancellationRequested)
                    {
                        _originalLanguage = General_Form.Instance.Language_TextBox.Text;
                        UpdateLanguage();
                        LoadingPanel.HideLoadingScreen(this);
                        return true;
                    }
                    else
                    {
                        LoadingPanel.HideLoadingScreen(this);
                        return false;  // Translation was cancelled or failed
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Write(1, "Language translation was cancelled");
                    LoadingPanel.HideLoadingScreen(this);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoadingPanel.HideLoadingScreen(this);
                Log.Error_GetTranslation(ex.Message);
                return false;
            }
        }
        private bool HasLanguageChanged()
        {
            string currentLanguage = General_Form.Instance.Language_TextBox.Text;
            return !string.Equals(_originalLanguage, currentLanguage, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Updates the application language setting and logs the change.
        /// </summary>
        private static void UpdateLanguage()
        {
            Properties.Settings.Default.Language = General_Form.Instance.Language_TextBox.Text;

            // Remove previous messages that mention language changes
            string message = "Changed the language to";
            MainMenu_Form.SettingsThatHaveChangedInFile.RemoveAll(x => x.Contains(message));

            // Add the new language change message
            string fullMessage = $"{message} {Properties.Settings.Default.Language}";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.SettingsThatHaveChangedInFile, 2, fullMessage);
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
            FormBack_Panel.Controls.Add(form);
            form.BringToFront();

            // Save
            selectedButton = button;
        }
    }
}