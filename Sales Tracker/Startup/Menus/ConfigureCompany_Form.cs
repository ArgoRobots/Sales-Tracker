using LiveChartsCore.SkiaSharpView.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.Startup.Menus
{
    public partial class ConfigureCompany_Form : BaseForm
    {
        // Properties
        private static Action _validationCallback;
        private static bool _isProgramLoading;

        // Init.
        public ConfigureCompany_Form()
        {
            InitializeComponent();

            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            _isProgramLoading = true;
            SetDefaultCompanyDirectory();
            SetDefaultTextInTextBoxes();
            AddEventHandlersToTextBoxes();
            _isProgramLoading = false;
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);
            if (ThemeManager.IsDarkTheme())
            {
                ThreeDots_Button.Image = Resources.ThreeDotsWhite;
            }
            else
            {
                ThreeDots_Button.Image = Resources.ThreeDotsBlack;
            }
        }
        private void SetAccessibleDescriptions()
        {
            CompanyName_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            Directory_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
            Currency_TextBox.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 200;

            TextBoxManager.Attach(CompanyName_TextBox);
            TextBoxManager.Attach(Directory_TextBox);
            TextBoxManager.Attach(Currency_TextBox);

            // Set the validation callback before attaching SearchBox
            _validationCallback = ValidateInputs;
            SearchBox.SetValidationCallback(_validationCallback);

            SearchBox.Attach(Currency_TextBox, this, Currency.GetSearchResults, searchBoxMaxHeight, false, false, false, false);
        }
        private void SetDefaultTextInTextBoxes()
        {
            string defaultName = "CompanyName";
            List<string> existingNames = [];
            string[] directories = Directory.GetDirectories(Properties.Settings.Default.CompanyDirectory);
            string[] files = Directory.GetFiles(Properties.Settings.Default.CompanyDirectory, "*" + ArgoFiles.ArgoCompanyFileExtension);

            foreach (string dir in directories)
            {
                existingNames.Add(Path.GetFileName(dir));
            }
            foreach (string file in files)
            {
                existingNames.Add(Path.GetFileNameWithoutExtension(file));
            }
            if (existingNames.Contains(defaultName))
            {
                defaultName = Tools.AddNumberForAStringThatAlreadyExists(defaultName, existingNames);
            }

            CompanyName_TextBox.Text = defaultName;

            // Set default currency
            Currency_TextBox.Text = "CAD";
        }
        private void SetDefaultCompanyDirectory()
        {
            if (Properties.Settings.Default.CompanyDirectory == "")
            {
                Properties.Settings.Default.CompanyDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
                Directory_TextBox.Text = Properties.Settings.Default.CompanyDirectory;
            }
            else
            {
                Directory_TextBox.Text = Properties.Settings.Default.CompanyDirectory;
            }
        }

        // Form event handlers
        private void ConfigureCompany_Form_Shown(object sender, EventArgs e)
        {
            CompanyName_TextBox.Focus();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ConfigureCompany_Form_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ConfigureNewCompany_Label.Focus();  // This deselects any TextBox
        }

        // Event handlers
        private void Back_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.FormGetStarted);
        }
        private void Create_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (File.Exists(Directory_TextBox.Text + @"\" + CompanyName_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension))
            {
                Directory_TextBox.Focus();
                CustomMessageBox.Show("Company already exists", "A company with this name already exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            string tempDir = Directories.TempCompany_dir;

            Directories.SetDirectories(Directory_TextBox.Text, CompanyName_TextBox.Text);

            // Create directories and files
            Directories.CreateDirectory(Directories.TempCompany_dir, true);
            Directories.CreateDirectory(Directories.Logs_dir);
            Directories.CreateDirectory(Directories.Receipts_dir);
            Directories.CreateFile(Directories.Purchases_file);
            Directories.CreateFile(Directories.Sales_file);
            Directories.CreateFile(Directories.CategorySales_file);
            Directories.CreateFile(Directories.CategoryPurchases_file);
            Directories.CreateFile(Directories.Accountants_file);
            Directories.CreateFile(Directories.Companies_file);

            // Set recently opened companies
            DataFileManager.AppendValue(GlobalAppDataSettings.RecentCompanies, Directories.ArgoCompany_file);

            // Set default currency
            DataFileManager.SetValue(AppDataSettings.DefaultCurrencyType, Currency_TextBox.Text);

            ArgoCompany.SaveAll();
            ArgoCompany.CreateMutex(CompanyName_TextBox.Text);

            Startup_Form.CanExitApp = false;
            Startup_Form.Instance.Close();

            // The user is starting Argo Sales Tracker
            if (MainMenu_Form.Instance == null)
            {
                MainMenu_Form FormMainMenu = new();
                FormMainMenu.Show();
            }
            else  // The user is creating a new company from the "New company" button
            {
                Directories.DeleteDirectory(tempDir, true);
                MainMenu_Form.Instance.ResetData();

                // Reset controls
                MainMenu_Form.Instance.SetCompanyLabel();
                MainMenu_Form.Instance.UpdateTotalLabels();
                MainMenu_Form.Instance.HideShowingResultsForLabel();
                MainMenu_Form.Instance.UpdateMainMenuFormText();

                // Clear charts
                foreach (Control chart in MainMenu_Form.Instance.GetAllCharts())
                {
                    switch (chart)
                    {
                        case CartesianChart cartesianChart:
                            LoadChart.ClearChart(cartesianChart);
                            LabelManager.ManageNoDataLabelOnControl(false, cartesianChart);
                            break;
                        case PieChart pieChart:
                            LoadChart.ClearPieChart(pieChart);
                            LabelManager.ManageNoDataLabelOnControl(false, pieChart);
                            break;
                    }
                }

                Tools.CloseAllOpenForms();
            }
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
            }

            // Save so it loads back in when the program is restarted
            Properties.Settings.Default.CompanyDirectory = Directory_TextBox.Text;
            Properties.Settings.Default.Save();
        }
        private void CompanyName_TextChanged(object sender, EventArgs e)
        {
            string invalidChars = "/\\#%&*|;";

            if (string.IsNullOrEmpty(CompanyName_TextBox.Text))
            {
                CustomControls.SetGTextBoxToInvalid(CompanyName_TextBox);
                ShowWarningForCompanyName("Company name cannot be empty");
            }
            else if (invalidChars.Any(CompanyName_TextBox.Text.Contains))
            {
                CustomControls.SetGTextBoxToInvalid(CompanyName_TextBox);
                ShowWarningForCompanyName("Company name contains invalid characters");
            }
            else
            {
                CustomControls.SetGTextBoxToValid(CompanyName_TextBox);
                HideWarningForCompanyName();
            }

            ValidateInputs();
        }
        private void Directory_textBox_TextChanged(object sender, EventArgs e)
        {
            string invalidChars = "/#%&*|;";

            if (string.IsNullOrEmpty(Directory_TextBox.Text))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory("Directory cannot be empty");
            }
            else if (invalidChars.Any(Directory_TextBox.Text.Contains))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory("Directory contains invalid characters");
            }
            else if (!Directory_TextBox.Text.Contains('\\'))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory("Directory must contain a backslash (\\)");
            }
            else if (!Directory.Exists(Directory_TextBox.Text))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory("Directory does not exist");
            }
            else
            {
                CustomControls.SetGTextBoxToValid(Directory_TextBox);
                HideWarningForDirectory();
            }

            ValidateInputs();
        }

        // Warning labels
        private void ShowWarningForDirectory(string text)
        {
            WarningDir_Label.Text = LanguageManager.TranslateString(text);
            WarningDir_PictureBox.Visible = true;
            WarningDir_Label.Visible = true;
        }
        private void HideWarningForDirectory()
        {
            WarningDir_PictureBox.Visible = false;
            WarningDir_Label.Visible = false;
        }
        private void ShowWarningForCompanyName(string text)
        {
            WarningName_Label.Text = LanguageManager.TranslateString(text);
            WarningName_PictureBox.Visible = true;
            WarningName_Label.Visible = true;
        }
        private void HideWarningForCompanyName()
        {
            WarningName_PictureBox.Visible = false;
            WarningName_Label.Visible = false;
        }

        // Methods
        private void ValidateInputs()
        {
            if (_isProgramLoading) { return; }

            Create_Button.Enabled = CustomControls.IsGTextBoxInvalid(CompanyName_TextBox)
                && CustomControls.IsGTextBoxInvalid(Directory_TextBox)
                && !string.IsNullOrWhiteSpace(Currency_TextBox.Text) && Currency_TextBox.Tag?.ToString() != "0";
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels();
        }
    }
}