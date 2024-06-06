using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Startup
{
    public partial class GetStarted_Form : Form
    {
        // Init.
        public static GetStarted_Form Instance { get; set; }
        public GetStarted_Form()
        {
            InitializeComponent();
            Instance = this;

            CustomColors.SetColors();
            Directories.SetUniversalDirectories();
            LoadListOfRecentProjects();

            // Enable vertical scroll
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Maximum = 0;
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Visible = false;
            OpenRecent_FlowLayoutPanel.AutoScroll = true;

            UpdateTheme();
        }
        private void UpdateTheme()
        {
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

            }
        }
        private void LoadListOfRecentProjects()
        {
            string value = DataFileManager.GetValue(Directories.appDataCongig_file, DataFileManager.AppDataSettings.RecentProjects);
            if (value == null)
            {
                return;
            }

            string[] projectDirs = value.Split(new char[] { ',' });
            Array.Reverse(projectDirs);  // Reverse the array so it loads in the correct order

            foreach (string projectDir in projectDirs)
            {
                bool alreadyInPanel = OpenRecent_FlowLayoutPanel.Controls.OfType<Guna2Button>().Any(btn => btn.Tag.ToString() == projectDir);

                if (File.Exists(projectDir) && !alreadyInPanel)
                {
                    // Construct button
                    Guna2Button gBtn = new()
                    {
                        BackColor = CustomColors.controlBack,
                        FillColor = CustomColors.controlBack,
                        Size = new Size(250, 40),
                        Text = Path.GetFileNameWithoutExtension(projectDir),
                        Font = new Font("Segoe UI", 11),
                        Tag = projectDir
                    };
                    gBtn.Click += (sender, e) =>
                    {
                        Guna2Button Gbtn = (Guna2Button)sender;

                        string projectName = Path.GetFileNameWithoutExtension(Gbtn.Tag.ToString());
                        if (!ArgoProject.OnlyAllowOneInstanceOfAProject(projectName))
                        {
                            return;
                        }

                        // Save new ProjectDirectory
                        Sales_Tracker.Properties.Settings.Default.ProjectDirectory = Directory.GetParent(Gbtn.Tag.ToString()).FullName;
                        Sales_Tracker.Properties.Settings.Default.Save();

                        Directories.SetDirectoriesFor(Sales_Tracker.Properties.Settings.Default.ProjectDirectory, projectName);
                        Directories.InitDataFile();

                        List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.appData_dir);
                        Directories.ImportArgoTarFile(Directories.argoProject_file, Directories.appData_dir, "Argo project", listOfDirectories, false);

                        ShowMainMenu();
                    };
                    OpenRecent_FlowLayoutPanel.Controls.Add(gBtn);
                }
            }
        }
        private void GetStarted_Form_Shown(object sender, EventArgs e)
        {
            ArgoProject.CheckForUnsavedWork();
        }

        // Event handlers
        private void CreateNewProject_Click(object sender, EventArgs e)
        {
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.FormConfigureProject);
        }


        // Open project     
        private void OpenProject_Button_Click(object sender, EventArgs e)
        {
            ArgoProject.OpenProject();
        }
        public void ShowMainMenu()
        {
            // Hide current form. Don't close it or both forms will close
            Parent?.Hide();

            // Add event to close FormStartup when FormMainMenu is closed
            Form FormMainMenu = new Sales_Tracker.MainMenu_Form();
            FormMainMenu.FormClosed += (s, args) => Startup_Form.Instance.Close();

            FormMainMenu.Show();
        }
    }
}