using Sales_Tracker.Classes;

namespace Sales_Tracker.Startup.Menus
{
    public partial class ConfigureProject_Form : Form
    {
        // Properties
        public string selectedDirectory, projectName;

        // Init.
        public static ConfigureProject_Form Instance { get; private set; }
        public ConfigureProject_Form()
        {
            InitializeComponent();
            Instance = this;

            Theme.SetThemeForForm(this);
        }

        // Form event handlers
        private void ConfigureProject_form_Load(object sender, EventArgs e)
        {
            // Set default name. Choose a name that doesn't already exist in the directory
            if (!Directory.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName") &&
                !File.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName" + ArgoFiles.ArgoCompanyFileExtension))
            {
                ProjectName_textBox.Text = "CompanyName";
            }
            else
            {
                int count = 2;
                while (true)
                {
                    if (!Directory.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName (" + count + ")") &&
                        !File.Exists(Properties.Settings.Default.ProjectDirectory + @"\CompanyName (" + count + ")" + ArgoFiles.ArgoCompanyFileExtension))
                    {
                        ProjectName_textBox.Text = "CompanyName (" + count + ")";
                        break;
                    }
                    count++;
                }
            }

            // Set default file location
            if (Properties.Settings.Default.ProjectDirectory == "")
            {
                Properties.Settings.Default.ProjectDirectory = Directories.desktop_dir;
                Properties.Settings.Default.Save();
                Directory_textBox.Text = Properties.Settings.Default.ProjectDirectory;
            }
            else
            {
                Directory_textBox.Text = Properties.Settings.Default.ProjectDirectory;
            }
        }
        private void ConfigureProject_Form_Shown(object sender, EventArgs e)
        {
            ProjectName_textBox.Focus();
            ProjectName_textBox.SelectionStart = ProjectName_textBox.Text.Length;
            ProjectName_textBox.SelectionLength = 0;
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
            selectedDirectory = Directory_textBox.Text;

            if (Directory_textBox.Text == "")
            {
                Directory_textBox.Focus();
                CustomMessageBox.Show("Argo Sales Tracker", "Select a directory to create the project", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            if (File.Exists(selectedDirectory + @"\" + ProjectName_textBox.Text + ArgoFiles.ArgoCompanyFileExtension))
            {
                CustomMessageBox.Show("Argo Sales Tracker", "A project with this name already exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            projectName = ProjectName_textBox.Text;
            // Hide current form. Don't close it or both forms will close
            Parent.Hide();

            Directories.SetDirectoriesAndInit(selectedDirectory, projectName);

            // Create directories and files
            Directories.CreateDirectory(Directories.tempCompany_dir, true);

            Directories.CreateDirectory(Directories.logs_dir, false);
            Directories.CreateDirectory(Directories.receipts_dir, false);
            Directories.CreateFile(Directories.purchases_file);
            Directories.CreateFile(Directories.sales_file);
            Directories.CreateFile(Directories.categorySales_file);
            Directories.CreateFile(Directories.categoryPurchases_file);
            Directories.CreateFile(Directories.accountants_file);
            Directories.CreateFile(Directories.companies_file);
            ArgoCompany.SaveAll();

            // Save recently opened projects
            DataFileManager.AppendValue(Directories.appDataCongig_file, DataFileManager.AppDataSettings.RecentProjects, Directories.argoCompany_file, DataFileManager.MaxValueForRecentProjects);
            DataFileManager.Save(Directories.appDataCongig_file);

            ArgoCompany.CreateMutex(projectName);

            // Add event to close FormStartup when FormMainMenu is closed
            Form FormMainMenu = new MainMenu_Form();
            FormMainMenu.FormClosed += (s, args) => Startup_Form.Instance.Close();

            FormMainMenu.Show();
        }
        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_textBox.Text = dialog.SelectedPath + @"\";
                selectedDirectory = Directory_textBox.Text;
            }
            // Save
            Properties.Settings.Default.ProjectDirectory = selectedDirectory;
            Properties.Settings.Default.Save();
        }


        private void TextBoxProjectName_TextChanged(object sender, EventArgs e)
        {
            if (@"/\#%&*|;".Any(ProjectName_textBox.Text.Contains) || ProjectName_textBox.Text == "")
            {
                Create_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(ProjectName_textBox);
                WarningName_pictureBox.Visible = true;
                WarningName_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                UI.SetGTextBoxToValid(ProjectName_textBox);
                WarningName_pictureBox.Visible = false;
                WarningName_Label.Visible = false;
            }
        }
        private void Directory_textBox_TextChanged(object sender, EventArgs e)
        {
            // TEMP NOTE: the backround color of WarningDir_pictureBox is always white even in dark mode because the backround is not transparent. It needs to be a .png

            if ("/#%&*|;".Any(Directory_textBox.Text.Contains) || Directory_textBox.Text == "" || !Directory_textBox.Text.Contains('\\'))
            {
                Create_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Directory_textBox);
                WarningDir_pictureBox.Visible = true;
                WarningDir_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                UI.SetGTextBoxToValid(Directory_textBox);
                WarningDir_pictureBox.Visible = false;
                WarningDir_Label.Visible = false;
            }
            // Save
            Properties.Settings.Default.ProjectDirectory = Directory_textBox.Text;
            Properties.Settings.Default.Save();
        }
    }
}