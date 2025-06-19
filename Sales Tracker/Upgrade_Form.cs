using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Upgrade_Form : Form
    {
        // Init.
        public Upgrade_Form()
        {
            InitializeComponent();
            ConstructIEnterKeyPanel();
            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            CenterControls();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void CenterControls()
        {
            int CenterHorizontally(Control control) => (ClientSize.Width - Benifits_Panel.Width - control.Width) / 2;

            // Center controls
            UpgradeTitle_Label.Left = CenterHorizontally(UpgradeTitle_Label);
            UpgradeSubTitle_Label.Left = CenterHorizontally(UpgradeSubTitle_Label);
            DollarAmount_Label.Left = CenterHorizontally(DollarAmount_Label);
            Upgrade_Button.Left = CenterHorizontally(Upgrade_Button);
            Upgrade_Button.Left = CenterHorizontally(EnterKey_Button);

            StripeLogo_ImageButton.Left = CenterHorizontally(StripeLogo_ImageButton);
            PayPalLogo_ImageButton.Left = StripeLogo_ImageButton.Left - PayPalLogo_ImageButton.Width - CustomControls.SpaceBetweenControls;
            Square_ImageButton.Left = StripeLogo_ImageButton.Right + CustomControls.SpaceBetweenControls;
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            Benifits_Panel.BackColor = CustomColors.HeaderBackground;
            ThemeManager.MakeGButtonBluePrimary(Upgrade_Button);
            ThemeManager.MakeGButtonBlueSecondary(EnterKey_Button);

            // For EnterKey_Panel
            ThemeManager.MakeGButtonBluePrimary(verifyLicense_Button);
            backButton.FillColor = CustomColors.MainBackground;

            if (ThemeManager.IsDarkTheme())
            {
                Square_ImageButton.Image = Resources.SquareLogoWhite;
                backButton.Image = Resources.BackArrowWhite;
            }
            else
            {
                Square_ImageButton.Image = Resources.SquareLogoBlack;
                backButton.Image = Resources.BackArrowBlack;
            }
        }

        // Form event handlers
        private void Upgrade_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Upgrade_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("https://argorobots.com/upgrade/index.html");
        }
        private void EnterKey_Button_Click(object sender, EventArgs e)
        {
            EnterKey_Panel.Visible = true;
            EnterKey_Panel.BringToFront();
        }
        private void LearnMore_Button_Click(object sender, EventArgs e)
        {
            Tools.OpenLink("https://argorobots.com/documentation/index.html#version-comparison");
        }

        // EnterKey_Panel
        private Panel EnterKey_Panel;
        private Guna2Button verifyLicense_Button, backButton;
        private Label errorLabel;
        private Guna2TextBox license_TextBox;
        private void ConstructIEnterKeyPanel()
        {
            EnterKey_Panel = new()
            {
                BackColor = CustomColors.MainBackground,
                Visible = false,
                Dock = DockStyle.Fill,
                Size = ClientSize
            };
            Controls.Add(EnterKey_Panel);

            Label title_Label = new()
            {
                ForeColor = CustomColors.Text,
                AutoSize = true,
                Top = 22,
                Font = new Font("Segoe UI", 16),
                Text = "Enter your license",
                Name = "EnterYourLicense_Label",
                Anchor = AnchorStyles.Top
            };
            EnterKey_Panel.Controls.Add(title_Label);

            license_TextBox = new()
            {
                Anchor = AnchorStyles.Top,
                Size = new Size(350, 50),
                PlaceholderText = "Enter license"
            };
            TextBoxManager.Attach(license_TextBox);
            EnterKey_Panel.Controls.Add(license_TextBox);

            verifyLicense_Button = new()
            {
                Anchor = AnchorStyles.Top,
                Size = new Size(350, 50),
                Text = "Verify license",
                Font = new Font("Segoe UI", 10),
                BorderRadius = 2,
                BorderThickness = 1,
                Name = "VerifyLicense_Button",
            };
            verifyLicense_Button.Click += VerifyLicense_Button_Click;
            EnterKey_Panel.Controls.Add(verifyLicense_Button);

            backButton = new()
            {
                Size = new Size(50, 45),
                ImageSize = new Size(40, 40),
                Location = new Point(title_Label.Top, title_Label.Top)
            };
            backButton.Click += BackButton_Click;
            EnterKey_Panel.Controls.Add((backButton));

            // Set locations
            license_TextBox.Location = new Point((
                EnterKey_Panel.Width - license_TextBox.Width) / 2,
                ((EnterKey_Panel.Height - license_TextBox.Height - CustomControls.SpaceBetweenControls - verifyLicense_Button.Height)) / 2);

            verifyLicense_Button.Location = new Point(license_TextBox.Left, license_TextBox.Bottom + CustomControls.SpaceBetweenControls);

            title_Label.Left = (EnterKey_Panel.Width - title_Label.Width) / 2;
        }
        private async void VerifyLicense_Button_Click(object sender, EventArgs e)
        {
            string key = license_TextBox.Text.Trim().ToUpper();
            LicenseManager licenseManager = new();
            bool iskeyValid = await licenseManager.ValidateKeyAsync(key);

            if (iskeyValid)
            {
                SetLicenseValid();
                Log.Write(2, "Enabled full version!");
            }
            else { SetLicenseInvalid(); }
        }
        private void SetLicenseValid()
        {
            Panel animationPanel = new()
            {
                BackColor = CustomColors.MainBackground,
                Dock = DockStyle.Fill
            };
            EnterKey_Panel.Controls.Add(animationPanel);
            animationPanel.BringToFront();

            Guna2CircleProgressBar progressCircle = new()
            {
                Size = new Size(100, 100),
                Anchor = AnchorStyles.Top,
                FillColor = CustomColors.ControlBack,
                BackColor = CustomColors.MainBackground,
                ProgressColor = CustomColors.AccentGreen
            };
            animationPanel.Controls.Add(progressCircle);

            Label successLabel = new()
            {
                AutoSize = true,
                Anchor = AnchorStyles.Top,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = CustomColors.MainBackground,
                ForeColor = CustomColors.Text,
                Text = LanguageManager.TranslateString("License Verified Successfully!"),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            animationPanel.Controls.Add(successLabel);

            Guna2Button closeButton = new()
            {
                Size = verifyLicense_Button.Size,
                Anchor = AnchorStyles.Top,
                Text = LanguageManager.TranslateString("Close"),
                Font = new Font("Segoe UI", 10),
                BorderRadius = 2,
                BorderThickness = 1,
                Visible = false
            };
            ThemeManager.MakeGButtonBluePrimary(closeButton);
            animationPanel.Controls.Add(closeButton);

            closeButton.Click += (_, _) => Close();

            // Center progressCircle
            progressCircle.Location = new Point(
                (animationPanel.Width - progressCircle.Width) / 2,
                (animationPanel.Height - progressCircle.Height) / 2
            );

            // Animate the progress circle
            async Task AnimateSuccess()
            {
                for (int i = 0; i <= 100; i += 2)
                {
                    progressCircle.Value = i;
                    await Task.Delay(10);
                }
                await Task.Delay(500);

                MainMenu_Form.RemoveUpgradeButtonIfFullVersion();

                progressCircle.Visible = false;
                successLabel.Visible = true;
                closeButton.Visible = true;

                // Center the two controls
                int totalHeight = successLabel.Height + closeButton.Height + 20;
                int startY = (animationPanel.Height - totalHeight) / 2;

                successLabel.Location = new Point(
                    (animationPanel.Width - successLabel.Width) / 2,
                    startY
                );
                closeButton.Location = new Point(
                    (animationPanel.Width - closeButton.Width) / 2,
                    startY + successLabel.Height + 20
                );
            }

            _ = AnimateSuccess();
        }
        private void SetLicenseInvalid()
        {
            // Create error label if it doesn't exist
            if (errorLabel == null)
            {
                errorLabel = new()
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = CustomColors.AccentRed,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Visible = false
                };
                EnterKey_Panel.Controls.Add(errorLabel);
            }

            // Set label properties
            errorLabel.Text = LanguageManager.TranslateString("Invalid license key");
            errorLabel.Location = new Point(
                (EnterKey_Panel.Width - errorLabel.Width) / 2,
                verifyLicense_Button.Bottom + 10
            );
            errorLabel.Visible = true;

            // Disable button
            verifyLicense_Button.Enabled = false;
            verifyLicense_Button.Text = LanguageManager.TranslateString("Invalid License");

            _ = ShowErrorAndShake();

            // Reset to allow user to try again
            verifyLicense_Button.Enabled = true;
            errorLabel.Visible = false;
        }
        private async Task ShowErrorAndShake()
        {
            int originalX = verifyLicense_Button.Left;
            for (int i = 0; i < 6; i++)
            {
                verifyLicense_Button.Left = originalX + (i % 2 == 0 ? -5 : 5);
                await Task.Delay(50);
            }
            verifyLicense_Button.Left = originalX;

            // Wait before hiding error message
            await Task.Delay(3000);

            // Hide error message and reset button
            errorLabel.Visible = false;
            verifyLicense_Button.Enabled = true;
            verifyLicense_Button.Text = LanguageManager.TranslateString("Verify License");
        }
        private void BackButton_Click(object sender, EventArgs e)
        {
            EnterKey_Panel.Visible = false;
        }
    }
}