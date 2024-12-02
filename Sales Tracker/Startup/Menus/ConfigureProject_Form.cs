using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;
using System.IO;

namespace Sales_Tracker.Startup.Menus
{
    public partial class ConfigureProject_Form : Form
    {
        // Properties
        private static ConfigureProject_Form _instance;
        public string selectedDirectory, projectName;

        // Getters
        public static ConfigureProject_Form Instance => _instance;

        // Init.
        public ConfigureProject_Form()
        {
            InitializeComponent();
            _instance = this;

            SearchBox.ConstructSearchBox();
            UpdateTheme();
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            SetDefaultProjectDirectory();
            SetDefaultTextInTextBoxes();
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        public void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
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
            ProjectName_TextBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Directory_TextBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Currency_TextBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 200;

            TextBoxManager.Attach(ProjectName_TextBox);
            ProjectName_TextBox.TextChanged += (_, _) => ValidateInputs();

            TextBoxManager.Attach(Directory_TextBox);
            Directory_TextBox.TextChanged += (_, _) => ValidateInputs();

            TextBoxManager.Attach(Currency_TextBox);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(Currency.GetCurrencyTypesList());
            SearchBox.Attach(Currency_TextBox, this, () => searchResult, searchBoxMaxHeight, false, false, false, false);
            Currency_TextBox.TextChanged += (_, _) => ValidateInputs();
        }
        private void SetDefaultTextInTextBoxes()
        {
            string defaultName = "CompanyName";
            List<string> existingNames = [];

            string[] directories = Directory.GetDirectories(Properties.Settings.Default.ProjectDirectory);
            string[] files = Directory.GetFiles(Properties.Settings.Default.ProjectDirectory, "*" + ArgoFiles.ArgoCompanyFileExtension);

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

            ProjectName_TextBox.Text = defaultName;

            // Set default currency
            Currency_TextBox.Text = "CAD";
        }
        private void SetDefaultProjectDirectory()
        {
            if (Properties.Settings.Default.ProjectDirectory == "")
            {
                Properties.Settings.Default.ProjectDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
                Directory_TextBox.Text = Properties.Settings.Default.ProjectDirectory;
            }
            else
            {
                Directory_TextBox.Text = Properties.Settings.Default.ProjectDirectory;
            }
        }

        // Form event handlers
        private void ConfigureProject_Form_Shown(object sender, EventArgs e)
        {
            ProjectName_TextBox.Focus();

            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ConfigureProject_Form_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ConfigureNewCompany_Label.Focus();  // This deselects any TextBox
        }

        // Event handlers
        private void Back_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.formGetStarted);
        }
        private void Create_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Set main directory
            selectedDirectory = Directory_TextBox.Text;

            if (File.Exists(selectedDirectory + @"\" + ProjectName_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension))
            {
                Directory_TextBox.Focus();
                CustomMessageBox.Show("Project already exists", "A project with this name already exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            projectName = ProjectName_TextBox.Text;

            // Hide current form. Don't close it or both forms will close
            Parent.Hide();

            Directories.SetDirectories(selectedDirectory, projectName);
            ArgoCompany.InitThings();

            // Create directories and files
            Directories.CreateDirectory(Directories.TempCompany_dir, true);

            if (!Directory.Exists(Directories.Logs_dir))
            {
                Directories.CreateDirectory(Directories.Logs_dir, false);
            }
            Directories.CreateDirectory(Directories.Receipts_dir, false);
            Directories.CreateFile(Directories.Purchases_file);
            Directories.CreateFile(Directories.Sales_file);
            Directories.CreateFile(Directories.CategorySales_file);
            Directories.CreateFile(Directories.CategoryPurchases_file);
            Directories.CreateFile(Directories.Accountants_file);
            Directories.CreateFile(Directories.Companies_file);

            // Set recently opened projects
            DataFileManager.AppendValue(DataFileManager.GlobalAppDataSettings.RecentProjects, Directories.ArgoCompany_file);

            // Set default currency
            DataFileManager.SetValue(DataFileManager.AppDataSettings.DefaultCurrencyType, Currency_TextBox.Text);

            ArgoCompany.SaveAll();

            ArgoCompany.CreateMutex(projectName);

            // Add event to close FormStartup when FormMainMenu is closed
            MainMenu_Form FormMainMenu = new();
            FormMainMenu.FormClosed += (s, args) => Startup_Form.Instance.Close();

            MainMenu_Form.UpdateMainMenuFormText(FormMainMenu);
            FormMainMenu.Show();
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
                selectedDirectory = Directory_TextBox.Text;
            }

            // Save so it loads back in when the program is restarted
            Properties.Settings.Default.ProjectDirectory = selectedDirectory;
            Properties.Settings.Default.Save();
        }
        private void TextBoxProjectName_TextChanged(object sender, EventArgs e)
        {
            string invalidChars = "/\\#%&*|;";

            if (string.IsNullOrEmpty(ProjectName_TextBox.Text))
            {
                CustomControls.SetGTextBoxToInvalid(ProjectName_TextBox);
                ShowWarningForProjectName();
                WarningName_Label.Text = "Project name cannot be empty";
            }
            else if (invalidChars.Any(ProjectName_TextBox.Text.Contains))
            {
                CustomControls.SetGTextBoxToInvalid(ProjectName_TextBox);
                ShowWarningForProjectName();
                WarningName_Label.Text = "Project name contains invalid characters";
            }
            else
            {
                CustomControls.SetGTextBoxToValid(ProjectName_TextBox);
                HideWarningForProjectName();
            }
        }
        private void Directory_textBox_TextChanged(object sender, EventArgs e)
        {
            string invalidChars = "/#%&*|;";

            if (string.IsNullOrEmpty(Directory_TextBox.Text))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory();
                WarningDir_Label.Text = "Directory cannot be empty";
            }
            else if (invalidChars.Any(Directory_TextBox.Text.Contains))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory();
                WarningDir_Label.Text = "Directory contains invalid characters";
            }
            else if (!Directory_TextBox.Text.Contains('\\'))
            {
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                ShowWarningForDirectory();
                WarningDir_Label.Text = "Directory must contain a backslash (\\)";
            }
            else
            {
                CustomControls.SetGTextBoxToValid(Directory_TextBox);
                HideWarningForDirectory();
            }
        }

        // Warning labels
        private void ShowWarningForDirectory()
        {
            WarningDir_PictureBox.Visible = true;
            WarningDir_Label.Visible = true;
        }
        private void HideWarningForDirectory()
        {
            WarningDir_PictureBox.Visible = false;
            WarningDir_Label.Visible = false;
        }
        private void ShowWarningForProjectName()
        {
            WarningName_PictureBox.Visible = true;
            WarningName_Label.Visible = true;
        }
        private void HideWarningForProjectName()
        {
            WarningName_PictureBox.Visible = false;
            WarningName_Label.Visible = false;
        }

        // Methods
        private void ValidateInputs()
        {
            Create_Button.Enabled = ProjectName_TextBox.BorderColor != Color.Red &&
                Directory_TextBox.BorderColor != Color.Red &&
                Currency_TextBox.BorderColor != Color.Red;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}