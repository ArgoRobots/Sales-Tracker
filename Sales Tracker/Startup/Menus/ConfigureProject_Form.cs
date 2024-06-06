using Sales_Tracker.Classes;
using System;
using Sales_Tracker;

namespace Sales_Tracker.Startup.Menus
{
    public partial class ConfigureProject_Form : Form
    {
        // Init.
        public static ConfigureProject_Form Instance { get; set; }
        public ConfigureProject_Form()
        {
            InitializeComponent();
            Instance = this;

            // Set theme
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

            }
        }

        // Form
        private void ConfigureProject_form_Load(object sender, EventArgs e)
        {
            // Set default name. Choose a name that doesn't already exist in the directory
            if (!Directory.Exists(Sales_Tracker.Properties.Settings.Default.ProjectDirectory + @"\ArgoProject") &&
                !File.Exists(Sales_Tracker.Properties.Settings.Default.ProjectDirectory + @"\ArgoProject.ArgoProject"))
            {
                ProjectName_textBox.Text = "ArgoProject";
            }
            else
            {
                int count = 2;
                while (true)
                {
                    if (!Directory.Exists(Sales_Tracker.Properties.Settings.Default.ProjectDirectory + @"\ArgoProject (" + count + ")") &&
                        !File.Exists(Sales_Tracker.Properties.Settings.Default.ProjectDirectory + @"\ArgoProject (" + count + ").ArgoProject"))
                    {
                        ProjectName_textBox.Text = "ArgoProject (" + count + ")";
                        break;
                    }
                    count++;
                }
            }

            // Set default file location
            if (Sales_Tracker.Properties.Settings.Default.ProjectDirectory == "")
            {
                Sales_Tracker.Properties.Settings.Default.ProjectDirectory = Directories.desktop_dir;
                Sales_Tracker.Properties.Settings.Default.Save();
                Directory_textBox.Text = Sales_Tracker.Properties.Settings.Default.ProjectDirectory;
            }
            else
            {
                Directory_textBox.Text = Sales_Tracker.Properties.Settings.Default.ProjectDirectory;
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
            label1.Focus();  // This deselects any TextBox
        }


        // Back btn
        private void Back_Button_Click(object sender, EventArgs e)
        {
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.formGetStarted);
        }
        // Create btn
        public string selectedDirectory, projectName;
        private void Create_Button_Click(object sender, EventArgs e)
        {
            // Set main directory
            selectedDirectory = Directory_textBox.Text;

            if (Directory_textBox.Text == "")
            {
                Directory_textBox.Focus();
                CustomMessageBox.Show("Argo Studio", "Select a directory to create the project", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            if (File.Exists(selectedDirectory + @"\" + ProjectName_textBox.Text + ".ArgoProject"))
            {
                CustomMessageBox.Show("Argo Studio", "A project with this name already exists", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return;
            }

            projectName = ProjectName_textBox.Text;
            // Hide current form. Don't close it or both forms will close
            Parent.Hide();

            Directories.SetDirectoriesFor(selectedDirectory, projectName);
            Directories.InitDataFile();

            // Create directories and files
            Directories.CreateDirectory(Directories.project_dir, true);
            Directories.CreateDirectory(Directories.buildMachines_commands_dir, false);
            Directories.CreateDirectory(Directories.robotArms_programs_dir, false);
            Directories.CreateDirectory(Directories.logs_dir, false);
            ArgoProject.SaveAll();

            // Save recently opened projects
            DataFileManager.AppendValue(Directories.appDataCongig_file, DataFileManager.AppDataSettings.RecentProjects, Directories.argoProject_file, DataFileManager.MaxValueForRecentProjects);
            DataFileManager.Save(Directories.appDataCongig_file);

            ArgoProject.CreateMutex(projectName);

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
            Sales_Tracker.Properties.Settings.Default.ProjectDirectory = selectedDirectory;
            Sales_Tracker.Properties.Settings.Default.Save();
        }


        private void TextBoxProjectName_TextChanged(object sender, EventArgs e)
        {
            if (@"/\#%&*|;".Any(ProjectName_textBox.Text.Contains) || ProjectName_textBox.Text == "")
            {
                Create_Button.Enabled = false;
                ProjectName_textBox.BorderColor = Color.Red;
                ProjectName_textBox.FocusedState.BorderColor = Color.Red;
                WarningName_pictureBox.Visible = true;
                WarningName_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                ProjectName_textBox.BorderColor = CustomColors.controlBorder;
                ProjectName_textBox.FocusedState.BorderColor = CustomColors.accent_blue;
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
                Directory_textBox.BorderColor = Color.Red;
                Directory_textBox.FocusedState.BorderColor = Color.Red;
                WarningDir_pictureBox.Visible = true;
                WarningDir_Label.Visible = true;
            }
            else
            {
                Create_Button.Enabled = true;
                Directory_textBox.BorderColor = CustomColors.controlBorder;
                Directory_textBox.FocusedState.BorderColor = CustomColors.controlBorder;
                WarningDir_pictureBox.Visible = false;
                WarningDir_Label.Visible = false;
            }
            // Save
            Sales_Tracker.Properties.Settings.Default.ProjectDirectory = Directory_textBox.Text;
            Sales_Tracker.Properties.Settings.Default.Save();
        }
    }
}