using Guna.UI2.WinForms;
using Microsoft.VisualBasic.FileIO;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Passwords;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;

namespace Sales_Tracker.Startup.Menus
{
    public partial class GetStarted_Form : Form
    {
        // Properties
        private static GetStarted_Form _instance;
        private readonly Dictionary<string, FileSystemWatcher> fileWatchers;

        // Getters
        public static GetStarted_Form Instance => _instance;

        // Init.
        public GetStarted_Form()
        {
            InitializeComponent();
            _instance = this;
            fileWatchers = [];

            LoadingPanel.InitBlankLoadingPanel();
            LoadingPanel.InitLoadingPanel();

            CustomColors.SetColors();
            Directories.SetUniversalDirectories();
            Directories.EnsureAppDataDirectoriesExist();
            CustomControls.ConstructRightClickRename();

            SetAccessibleDescriptions();
            LanguageManager.InitLanguageManager();
            LanguageManager.UpdateLanguageForControl(this);

            ConstructRightClickOpenRecentMenu();
            LoadListOfRecentProjects();
            SetFlowLayoutPanel();
            SetTheme();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void SetFlowLayoutPanel()
        {
            // Enable vertical scroll
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Maximum = 0;
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Enabled = false;
            OpenRecent_FlowLayoutPanel.HorizontalScroll.Visible = false;
        }
        private void SetTheme()
        {
            Theme.SetThemeForForm(this);
            Theme.UpdateThemeForPanel([_rightClickOpenRecent_Panel]);
            rightClickOpenRecent_DeleteBtn.ForeColor = CustomColors.AccentRed;

            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                CreateCompany_Button.Image = Resources.CreateFileWhite;
                OpenCompany_Button.Image = Resources.OpenFolderWhite;
            }
            else
            {
                CreateCompany_Button.Image = Resources.CreateFileBlack;
                OpenCompany_Button.Image = Resources.OpenFolderBlack;
            }
        }
        private void SetAccessibleDescriptions()
        {
            ArgoSalesTracker_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
        }

        // Form event handlers
        private void GetStarted_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            ArgoCompany.RecoverUnsavedWork();
        }

        // Recent projects
        private void LoadListOfRecentProjects()
        {
            List<string> validProjectDirs = ArgoCompany.GetValidRecentProjectPaths(false);

            if (validProjectDirs.Count == 0)
            {
                LabelManager.AddNoRecentlyOpenedCompanies(OpenRecent_FlowLayoutPanel, CalculateButtonWidth());
                return;
            }

            foreach (string projectDir in validProjectDirs)
            {
                // Construct button
                Guna2Button btn = new()
                {
                    BackColor = CustomColors.ControlBack,
                    FillColor = CustomColors.ControlBack,
                    Size = new Size(CalculateButtonWidth(), 60),
                    Text = Path.GetFileNameWithoutExtension(projectDir),
                    Font = new Font("Segoe UI", 11),
                    Tag = projectDir,
                    AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate
                };
                btn.MouseDown += Btn_MouseDown;
                btn.MouseUp += Btn_MouseUp;
                OpenRecent_FlowLayoutPanel.Controls.Add(btn);

                // Initialize file watcher for the directory
                string directory = Path.GetDirectoryName(projectDir);
                InitializeFileWatcher(directory);
            }
        }
        private void Btn_MouseDown(object? sender, MouseEventArgs e)
        {
            Controls.Remove(_rightClickOpenRecent_Panel);

            if (e.Button != MouseButtons.Left) { return; }
            Guna2Button button = (Guna2Button)sender;

            string projectName = Path.GetFileNameWithoutExtension(button.Tag.ToString());
            if (!ArgoCompany.OnlyAllowOneInstanceOfAProject(projectName))
            {
                ArgoCompany.ApplicationMutex?.Dispose();  // Reset
                return;
            }

            // Save new ProjectDirectory
            string newDir = Directory.GetParent(button.Tag.ToString()).FullName;

            Directories.SetDirectories(newDir, projectName);
            ArgoCompany.InitThings();

            if (!PasswordManager.EnterPassword())
            {
                ArgoCompany.ApplicationMutex?.Dispose();  // Reset
                return;
            }

            string filePath = newDir + @"\" + projectName + ArgoFiles.ArgoCompanyFileExtension;
            DataFileManager.AppendValue(GlobalAppDataSettings.RecentProjects, filePath);

            List<string> listOfDirectories = Directories.GetListOfAllDirectoryNamesInDirectory(Directories.AppData_dir);
            Directories.ImportArgoTarFile(Directories.ArgoCompany_file, Directories.AppData_dir, Directories.ImportType.ArgoCompany, listOfDirectories, false);
            DataFileManager.SetValue(AppDataSettings.ChangesMade, false.ToString());

            ShowMainMenu();
        }
        private void Btn_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Guna2Button button = (Guna2Button)sender;

                // Position and show the right click panel
                _rightClickOpenRecent_Panel.Location = new Point(e.X, OpenRecent_FlowLayoutPanel.Top + button.Top + button.Height);
                _rightClickOpenRecent_Panel.Tag = button;
                Controls.Add(_rightClickOpenRecent_Panel);
                _rightClickOpenRecent_Panel.BringToFront();
            }
        }
        private int CalculateButtonWidth()
        {
            if (OpenRecent_FlowLayoutPanel.VerticalScroll.Visible)
            {
                return OpenRecent_FlowLayoutPanel.Width - SystemInformation.VerticalScrollBarWidth - 8;
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
                foreach (Control control in OpenRecent_FlowLayoutPanel.Controls)
                {
                    if (control is Guna2Button btn)
                    {
                        if (btn.Tag.ToString() == e.FullPath)
                        {
                            itemsToRemove.Add(btn);
                        }
                    }
                }

                foreach (Guna2Button item in itemsToRemove)
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

        // Event handlers
        private void CreateNewCompany_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            Startup_Form.Instance.SwitchMainForm(Startup_Form.Instance.FormConfigureProject);
        }
        private void OpenCompany_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            ArgoCompany.OpenCompany();
        }
        private void OpenRecent_FlowLayoutPanel_Resize(object sender, EventArgs e)
        {
            foreach (Control control in OpenRecent_FlowLayoutPanel.Controls)
            {
                if (control is Guna2Button)
                {
                    control.Width = CalculateButtonWidth();
                }
            }

            // Prevents the horizontal scrollbar from flashing in some cases
            OpenRecent_FlowLayoutPanel.PerformLayout();
        }

        // Right click open recent menu
        private static Guna2Panel _rightClickOpenRecent_Panel;
        public static Guna2Panel RightClickOpenRecent_Panel => _rightClickOpenRecent_Panel;
        private Guna2Button rightClickOpenRecent_DeleteBtn;
        private void ConstructRightClickOpenRecentMenu()
        {
            _rightClickOpenRecent_Panel = CustomControls.ConstructPanelForMenu(new Size(CustomControls.PanelWidth, 4 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel), "rightClickOpenRecent_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickOpenRecent_Panel.Controls[0];

            Guna2Button menuBtn = CustomControls.ConstructBtnForMenu("Show in folder", CustomControls.PanelBtnWidth, false, flowPanel);
            menuBtn.Click += ShowInFolder;

            menuBtn = CustomControls.ConstructBtnForMenu("Rename company", CustomControls.PanelBtnWidth, false, flowPanel);
            menuBtn.Click += Rename;

            menuBtn = CustomControls.ConstructBtnForMenu("Hide", CustomControls.PanelBtnWidth, false, flowPanel);
            menuBtn.Click += Hide;

            rightClickOpenRecent_DeleteBtn = CustomControls.ConstructBtnForMenu("Delete in folder", CustomControls.PanelBtnWidth, false, flowPanel);
            rightClickOpenRecent_DeleteBtn.Click += DeleteInFolder;

            LanguageManager.UpdateLanguageForControl(RightClickOpenRecent_Panel);
        }
        private void ShowInFolder(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (OpenRecent_FlowLayoutPanel.Controls.OfType<Guna2Button>().FirstOrDefault() is Guna2Button btn)
            {
                string projectFile = btn.Tag.ToString();
                if (File.Exists(projectFile))
                {
                    Tools.ShowFileInFolder(projectFile);
                }
            }
        }
        private void Rename(object sender, EventArgs e)
        {
            Controls.Remove(_rightClickOpenRecent_Panel);

            Guna2Button button = (Guna2Button)_rightClickOpenRecent_Panel.Tag;

            CustomControls.Rename_TextBox.Text = button.Text;
            CustomControls.Rename_TextBox.Location = new Point(OpenRecent_FlowLayoutPanel.Left + button.Left + 1, OpenRecent_FlowLayoutPanel.Top + button.Top);
            CustomControls.Rename_TextBox.Size = new Size(button.Width, button.Height);
            CustomControls.Rename_TextBox.Font = button.Font;
            Controls.Add(CustomControls.Rename_TextBox);
            CustomControls.Rename_TextBox.Focus();
            CustomControls.Rename_TextBox.SelectAll();
            CustomControls.Rename_TextBox.BringToFront();
        }
        private void Hide(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (_rightClickOpenRecent_Panel.Tag is Guna2Button btn)
            {
                string projectDir = btn.Tag.ToString();

                // Remove from recent project in file
                string? value = DataFileManager.GetValue(GlobalAppDataSettings.RecentProjects);
                if (value != null)
                {
                    List<string> projectDirs = value.Split(',').ToList();
                    projectDirs.RemoveAll(dir => dir == projectDir);
                    DataFileManager.SetValue(GlobalAppDataSettings.RecentProjects, string.Join(",", projectDirs));
                }

                // Remove the button from the FlowLayoutPanel
                OpenRecent_FlowLayoutPanel.Controls.Remove(btn);
            }
        }
        private void DeleteInFolder(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (_rightClickOpenRecent_Panel.Tag is Guna2Button btn)
            {
                string projectDir = btn.Tag.ToString();
                if (File.Exists(projectDir))
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show(
                        "Delete company",
                        $"Are you sure you want to delete this company? It will be moved to the recycle bin.",
                        CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                    if (result == CustomMessageBoxResult.Yes)
                    {
                        FileSystem.DeleteFile(
                        projectDir,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);

                        OpenRecent_FlowLayoutPanel.Controls.Remove(btn);
                    }
                }
                else { Hide(null, null); }
            }
        }

        // Methods
        public void RenameCompany()
        {
            if (!Controls.Contains(CustomControls.Rename_TextBox))
            {
                return;
            }
            Controls.Remove(CustomControls.Rename_TextBox);

            Guna2Button button = (Guna2Button)_rightClickOpenRecent_Panel.Tag;

            // If the name did not change
            if (CustomControls.Rename_TextBox.Text == button.Text)
            {
                return;
            }

            button.Text = CustomControls.Rename_TextBox.Text;

            string projectDir = button.Tag.ToString();
            string newProjectDir = Path.Combine(Path.GetDirectoryName(projectDir), CustomControls.Rename_TextBox.Text + ArgoFiles.ArgoCompanyFileExtension);

            if (Directories.MoveFile(projectDir, newProjectDir))
            {
                button.Tag = newProjectDir;
            }

            CustomControls.Rename_TextBox.Clear();
        }
        public void ShowMainMenu()
        {
            // Hide current form. Don't close it or both forms will close
            Parent?.Hide();

            // Add event to close FormStartup when FormMainMenu is closed
            MainMenu_Form FormMainMenu = new();
            FormMainMenu.FormClosed += (_, _) => Startup_Form.Instance.Close();

            MainMenu_Form.UpdateMainMenuFormText(FormMainMenu);
            FormMainMenu.Show();
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            Controls.Remove(_rightClickOpenRecent_Panel);
            CustomControls.Rename();
        }
    }
}