using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Init
        public static MainMenu_Form Instance { get; set; }
        public MainMenu_Form()
        {
            InitializeComponent();
            Instance = this;

            UI.ConstructControls();
            SearchBox.ConstructSearchBox();
            UpdateTheme();
        }
        public void UpdateTheme()
        {
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

            }

            Top_Panel.BackColor = CustomColors.background3;
            MainTop_Panel.FillColor = CustomColors.background4;
            File_Button.FillColor = CustomColors.background3;
            Save_Button.FillColor = CustomColors.background3;
            Help_Button.FillColor = CustomColors.background3;
        }

        // Form
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            Log.Write(2, "Argo Studio has finished starting");
        }
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
            }
        }
        private void MainMenu_form_ResizeBegin(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
        }
        private void MainMenu_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Write(2, "Closing Argo Studio");

            UI.CloseAllPanels(null, null);

            // Save logs in file
            if (Directory.Exists(Directories.logs_dir))
            {
                DateTime time = DateTime.Now;  // Get time
                int count = 0;
                string directory;

                while (true)
                {
                    if (count == 0)
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + ".txt";
                    else
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + "-" + count + ".txt";

                    if (!Directory.Exists(directory))
                    {
                        Directories.WriteTextToFile(directory, Log.logText);
                        break;
                    }
                    count++;
                }
            }

            if (ArgoCompany.AreAnyChangesMade())
            {
                if (AskUserToSaveBeforeClosing()) { e.Cancel = true; return; }
            }

            // Delete hidden directory
            Directories.DeleteDirectory(Directories.project_dir, true);
        }

        /// <summary>
        /// Asks the user to save any changes.
        /// </summary>
        /// <returns>Returns true if the user cancels. Returns false if the user saves.</returns>
        private static bool AskUserToSaveBeforeClosing()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "Save changes to the following items?", CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

            switch (result)
            {
                case CustomMessageBoxResult.Save:
                    ArgoCompany.SaveAll();
                    break;
                case CustomMessageBoxResult.DontSave:
                    // Do nothing so the temp directory is deleted
                    break;
                case CustomMessageBoxResult.Cancel:
                    // Cancel close
                    return true;
                default:  // If the CustomMessageBox was closed
                    return true;
            }

            return false;
        }
        // Keyboard shortcuts
        private void MainMenu_form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:  // Save
                        if (e.Shift)
                        {
                            ArgoCompany.SaveAll();
                        }
                        else  // Save as
                        {
                            ArgoCompany.SaveAs();
                        }
                        break;

                    case Keys.E:  // Export
                        new Export_Form().ShowDialog();
                        break;

                    case Keys.L:  // Open logs
                        OpenLogs();
                        break;
                }
            }
            else if (e.Alt & e.KeyCode == Keys.F4)  // Close program
            {
                if (AskUserToSaveBeforeClosing())
                {
                    e.Handled = true;
                }
            }
        }

        // Cascading menus
        Guna2Panel menuToHide;
        private void HideMenu_timer_Tick(object sender, EventArgs e)
        {
            menuToHide.Parent?.Controls.Remove(menuToHide);
            HideMenu_timer.Enabled = false;
        }
        public void OpenMenu()
        {
            HideMenu_timer.Enabled = false;
        }
        public void CloseMenu(object sender, EventArgs e)
        {
            Guna2Button btn = (Guna2Button)sender;
            menuToHide = (Guna2Panel)btn.Tag;
            HideMenu_timer.Enabled = false;
            HideMenu_timer.Enabled = true;
        }
        public void KeepMenuOpen(object sender, EventArgs e)
        {
            HideMenu_timer.Enabled = false;
        }


        // TOP BAR
        // Don't initiate these yet because it resets every time a program is loaded
        public Products_Form formProducts;

        public void SwitchMainForm(Form form, object btnSender)
        {
            Guna2Button btn = (Guna2Button)btnSender;
            // If the form is not already selected
            if (btn.FillColor != Color.FromArgb(15, 13, 74) & btn.FillColor != Color.Gray)
            {
                Main_Panel.Controls.Clear();
                form.TopLevel = false;
                form.Dock = DockStyle.Fill;
                Main_Panel.Controls.Add(form);
                form.Show();
            }
        }

        // File
        private void File_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.fileMenu))
            {
                Controls.Remove(UI.fileMenu);
                File_Button.Image = Resources.FileGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                File_Button.Image = Resources.FileWhite;
                UI.fileMenu.Location = new Point(File_Button.Left, 30);
                Controls.Add(UI.fileMenu);
                UI.fileMenu.BringToFront();
            }
        }

        // Save btn
        private void Save_Button_Click(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            Guna2Button saveBtn = (Guna2Button)UI.fileMenu.Controls[0].Controls.Find("Save", false).FirstOrDefault();
            saveBtn.PerformClick();
        }
        private void Save_Button_MouseDown(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveWhite;
        }
        private void Save_Button_MouseUp(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveGray;
        }

        // Help
        private void Help_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.helpMenu))
            {
                Controls.Remove(UI.helpMenu);
                Help_Button.Image = Resources.HelpGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                Help_Button.Image = Resources.HelpWhite;
                UI.helpMenu.Location = new Point(Help_Button.Left - UI.helpMenu.Width + Help_Button.Width, 30);
                Controls.Add(UI.helpMenu);
                UI.helpMenu.BringToFront();
            }
        }


        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {

        }
        private void DarkMode_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {

        }

        // Message panel
        public Guna2Panel messagePanel;
        public void ConstructMessage_Panel()
        {
            messagePanel = new Guna2Panel
            {
                Size = new Size(500, 150),
                FillColor = CustomColors.mainBackground,
                BackColor = Color.Transparent,
                BorderThickness = 1,
                BorderRadius = 5,
                BorderColor = CustomColors.controlPanelBorder
            };

            Label messageLabel = new()
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(10, 10),
                Size = new Size(480, 85),
                Name = "label",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = CustomColors.text
            };
            messageLabel.TextChanged += (sender, e) =>
            {
                if (messageLabel.Text != "")
                {
                    messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
                    Controls.Add(messagePanel);
                    messagePanel.BringToFront();
                    // Restart timer
                    MessagePanel_timer.Enabled = false;
                    MessagePanel_timer.Enabled = true;
                }
            };
            messagePanel.Controls.Add(messageLabel);

            Guna2Button gBtn = new()
            {
                Font = new Font("Segoe UI", 11),
                Text = "Ok",
                Size = new Size(120, 35),
                Location = new Point(190, 100),
                FillColor = Color.FromArgb(58, 153, 236),  // Blue
                ForeColor = Color.White
            };
            gBtn.Click += MessagePanelClose;
            messagePanel.Controls.Add(gBtn);

            PictureBox picture = new()
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, 10),
                Size = new Size(15, 15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Image = Resources.CloseGrey,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            picture.Click += MessagePanelClose;
            messagePanel.Controls.Add(picture);
        }
        public void SetMessage(string text)
        {
            Label label = (Label)messagePanel.Controls.Find("label", false).FirstOrDefault();
            label.Text = text;
        }
        private void MessagePanelClose(object sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }
        private void MessagePanelTimer_Tick(object sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }


        private Log_Form LogForm;
        public void OpenLogs()
        {
            if (!Tools.IsFormOpen(typeof(Log_Form)))
            {
                LogForm = new Log_Form();
                LogForm.Show();
            }
            else
            {
                LogForm.BringToFront();
            }
        }

        private void CloseAllPanels(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}