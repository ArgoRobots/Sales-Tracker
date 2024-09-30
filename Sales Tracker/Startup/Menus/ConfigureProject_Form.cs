using Sales_Tracker.Classes;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;

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

            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            LoadingPanel.ShowBlankLoadingPanel(this);

            UpdateTheme();
            LanguageManager.UpdateLanguage(this);
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
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(ProjectName_TextBox);
            TextBoxManager.Attach(Directory_TextBox);
        }

        // Form event handlers
        private void ConfigureProject_form_Load(object sender, EventArgs e)
        {
            // Set default name. Choose a name that doesn't already exist in the directory
            if (!Directory.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName") &&
                !File.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName" + ArgoFiles.ArgoCompanyFileExtension))
            {
                ProjectName_TextBox.Text = "CompanyName";
            }
            else
            {
                int count = 2;
                while (true)
                {
                    if (!Directory.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName (" + count + ")") &&
                        !File.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName (" + count + ")" + ArgoFiles.ArgoCompanyFileExtension))
                    {
                        ProjectName_TextBox.Text = "CompanyName (" + count + ")";
                        break;
                    }
                    count++;
                }
            }

            // Set default file location
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
        private void ConfigureProject_Form_Shown(object sender, EventArgs e)
        {
            ProjectName_TextBox.Focus();
            ProjectName_TextBox.SelectionStart = ProjectName_TextBox.Text.Length;
            ProjectName_TextBox.SelectionLength = 0;

            AddEventHandlersToTextBoxes();

            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ConfigureProject_form_Click(object sender, EventArgs e)
        {
            ConfigureNewCompany_Label.Focus();  // This deselects any TextBox
        }

        // Event handlers
        private void Back_Button_Click(object sender, EventArgs e)
        {
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.formGetStarted);
        }
        private void Create_Button_Click(object sender, EventArgs e)
        {
            // Set main directory
            selectedDirectory = Directory_TextBox.Text;

            if (Directory_TextBox.Text == "")
            {
                Directory_TextBox.Focus();
                CustomMessageBox.Show("Argo Sales Tracker", "Select a directory to create the project", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            if (File.Exists(selectedDirectory + @"\" + ProjectName_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension))
            {
                CustomMessageBox.Show("Argo Sales Tracker", "A project with this name already exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            projectName = ProjectName_TextBox.Text;

            // Hide current form. Don't close it or both forms will close
            Parent.Hide();

            Directories.SetDirectories(selectedDirectory, projectName);
            ArgoCompany.InitThings();

            // Create directories and files
            Directories.CreateDirectory(Directories.TempCompany_dir, true);

            Directories.CreateDirectory(Directories.Logs_dir, false);
            Directories.CreateDirectory(Directories.Receipts_dir, false);
            Directories.CreateFile(Directories.Purchases_file);
            Directories.CreateFile(Directories.Sales_file);
            Directories.CreateFile(Directories.CategorySales_file);
            Directories.CreateFile(Directories.CategoryPurchases_file);
            Directories.CreateFile(Directories.Accountants_file);
            Directories.CreateFile(Directories.Companies_file);
            ArgoCompany.InitCacheFiles();

            // Set recently opened projects
            DataFileManager.AppendValue(DataFileManager.GlobalAppDataSettings.RecentProjects, Directories.ArgoCompany_file);

            // Set default currency
            DataFileManager.SetValue(DataFileManager.AppDataSettings.DefaultCurrencyType, Currency_ComboBox.Text);

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
            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
                selectedDirectory = Directory_TextBox.Text;
            }
            // Save
            Properties.Settings.Default.ProjectDirectory = selectedDirectory;
            Properties.Settings.Default.Save();
        }
        private void TextBoxProjectName_TextChanged(object sender, EventArgs e)
        {
            if (@"/\#%&*|;".Any(ProjectName_TextBox.Text.Contains) || ProjectName_TextBox.Text == "")
            {
                Create_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(ProjectName_TextBox);
                WarningName_PictureBox.Visible = true;
                WarningName_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                CustomControls.SetGTextBoxToValid(ProjectName_TextBox);
                WarningName_PictureBox.Visible = false;
                WarningName_Label.Visible = false;
            }
        }
        private void Directory_textBox_TextChanged(object sender, EventArgs e)
        {
            if ("/#%&*|;".Any(Directory_TextBox.Text.Contains) || Directory_TextBox.Text == "" || !Directory_TextBox.Text.Contains('\\'))
            {
                Create_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(Directory_TextBox);
                WarningDir_PictureBox.Visible = true;
                WarningDir_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                CustomControls.SetGTextBoxToValid(Directory_TextBox);
                WarningDir_PictureBox.Visible = false;
                WarningDir_Label.Visible = false;
            }
            // Save
            Properties.Settings.Default.ProjectDirectory = Directory_TextBox.Text;
            Properties.Settings.Default.Save();
        }
    }
}