using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker.Startup
{
    public partial class GetStarted_Form : Form
    {
        // Properties
        private readonly Dictionary<string, FileSystemWatcher> fileWatchers;

        // Init.
        public static GetStarted_Form Instance { get; private set; }
        public GetStarted_Form()
        {
            InitializeComponent();
            Instance = this;
            fileWatchers = [];

            CustomColors.SetColors();
            Directories.SetUniversalDirectories();
            Directories.InitDataFile();
            LoadListOfRecentProjects();
            SetFlowLayoutPanel();
            Theme.SetThemeForForm(this);
        }
        private void SetFlowLayoutPanel()
        {
            // Enable vertical scroll
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Maximum = 0;
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Visible = false;
            OpenRecent_FlowLayoutPanel.AutoScroll = true;
        }

        // Recent projects
        private void LoadListOfRecentProjects()
        {
            string value = DataFileManager.GetValue(Directories.appDataCongig_file, DataFileManager.AppDataSettings.RecentProjects);
            if (value == null)
            {
                return;
            }

            string[] projectDirs = value.Split([',']);
            Array.Reverse(projectDirs);  // Reverse the array so it loads in the correct order

            // Remove duplicates and filter valid directories
            List<string> validProjectDirs = projectDirs.Distinct().Where(File.Exists).ToList();

            foreach (string projectDir in validProjectDirs)
            {
                // Construct button
                Guna2Button gBtn = new()
                {
                    BackColor = CustomColors.controlBack,
                    FillColor = CustomColors.controlBack,
                    Size = new Size(CalculateButtonWidth(validProjectDirs.Count), 40),
                    Text = Path.GetFileNameWithoutExtension(projectDir),
                    Font = new Font("Segoe UI", 11),
                    Tag = projectDir
                };
                gBtn.Click += (sender, e) =>
                {
                    Guna2Button Gbtn = (Guna2Button)sender;

                    string projectName = Path.GetFileNameWithoutExtension(Gbtn.Tag.ToString());
                    if (!ArgoCompany.OnlyAllowOneInstanceOfAProject(projectName))
                    {
                        return;
                    }

                    // Save new ProjectDirectory
                    Properties.Settings.Default.ProjectDirectory = Directory.GetParent(Gbtn.Tag.ToString()).FullName;
                    Properties.Settings.Default.Save();

                    Directories.SetDirectoriesAndInit(Properties.Settings.Default.ProjectDirectory, projectName);

                    List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.appData_dir);
                    Directories.ImportArgoTarFile(Directories.argoCompany_file, Directories.appData_dir, "Argo copmany", listOfDirectories, false);

                    ShowMainMenu();
                };
                OpenRecent_FlowLayoutPanel.Controls.Add(gBtn);

                // Initialize file watcher for the directory
                string directory = Path.GetDirectoryName(projectDir);
                InitializeFileWatcher(directory);
            }
        }
        private int CalculateButtonWidth(int count)
        {
            if (count > 7)
            {
                return OpenRecent_FlowLayoutPanel.Width - SystemInformation.VerticalScrollBarWidth - 5;
            }
            else
            {
                return OpenRecent_FlowLayoutPanel.Width - 8;
            }
        }
        private void InitializeFileWatcher(string directory)
        {
            if (Directory.Exists(directory) && !fileWatchers.ContainsKey(directory))
            {
                FileSystemWatcher fileWatcher = new()
                {
                    Path = directory,
                    NotifyFilter = NotifyFilters.FileName
                };
                fileWatcher.Deleted += FileDeleted;
                fileWatcher.EnableRaisingEvents = true;

                fileWatchers[directory] = fileWatcher;
            }
        }
        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                List<Guna2Button> itemsToRemove = [];
                foreach (var control in OpenRecent_FlowLayoutPanel.Controls)
                {
                    if (control is Guna2Button btn)
                    {
                        if (btn.Tag.ToString() == e.FullPath)
                        {
                            itemsToRemove.Add(btn);
                        }
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    OpenRecent_FlowLayoutPanel.Controls.Remove(item);
                }

                // Remove the watcher if no more files are being watched in this directory
                string? directory = Path.GetDirectoryName(e.FullPath);
                if (directory != null)
                {
                    bool directoryStillInUse = OpenRecent_FlowLayoutPanel.Controls.OfType<Guna2Button>()
                        .Any(btn => Path.GetDirectoryName(btn.Tag.ToString()) == directory);

                    if (!directoryStillInUse && fileWatchers.TryGetValue(directory, out FileSystemWatcher? value))
                    {
                        value.Dispose();
                        fileWatchers.Remove(directory);
                    }
                }
            });
        }

        // Form event handlers
        private void GetStarted_Form_Shown(object sender, EventArgs e)
        {
            ArgoCompany.CheckForUnsavedWork();
        }

        // Event handlers
        private void CreateNewCompany_Click(object sender, EventArgs e)
        {
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.FormConfigureProject);
        }
        private void OpenCompany_Button_Click(object sender, EventArgs e)
        {
            ArgoCompany.Open();
        }

        // Methods   
        public void ShowMainMenu()
        {
            // Hide current form. Don't close it or both forms will close
            Parent?.Hide();

            // Add event to close FormStartup when FormMainMenu is closed
            Form FormMainMenu = new MainMenu_Form();
            FormMainMenu.FormClosed += (s, args) => Startup_Form.Instance.Close();

            FormMainMenu.Show();
        }
    }
}