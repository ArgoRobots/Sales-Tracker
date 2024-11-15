using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ImportSpreadsheet;
using Sales_Tracker.Properties;
using Sales_Tracker.Startup.Menus;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages the construction and customization of various UI controls, including menus, buttons, panels, and validation styling. 
    /// This class helps streamline UI setup and ensures consistency across different control types.
    /// </summary>
    internal static class CustomControls
    {
        public static void ConstructControls()
        {
            CascadingMenu.Init();

            // Main menu controls
            ConstructRecentlyOpenedMenu();
            ConstructFileMenu();
            ConstructHelpMenu();
            ConstructAccountMenu();
            ContructControlsDropDownButton();
            ConstructControlsDropDownMenu();

            // Other controls
            DataGridViewManager.ConstructRightClickRowMenu();
            RightClickGunaChartMenu.ConstructRightClickGunaChartMenu();

            // Set language
            LanguageManager.UpdateLanguageForControl(_fileMenu);
            LanguageManager.UpdateLanguageForControl(_recentlyOpenedMenu);
            LanguageManager.UpdateLanguageForControl(_helpMenu);
            LanguageManager.UpdateLanguageForControl(_accountMenu);
            LanguageManager.UpdateLanguageForControl(_controlsDropDown_Button);
            LanguageManager.UpdateLanguageForControl(_controlDropDown_Panel);
            LanguageManager.UpdateLanguageForControl(DataGridViewManager.RightClickDataGridView_Panel);
        }

        // Properties
        private static readonly byte _panelButtonHeight = 35, spaceForSeperator = 11, _spaceForPanel = 10, _spaceBetweenControls = 8, offsetForKeyboardShortcutOrArrow = 15;
        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbersAndDecimal,
            OnlyNumbers,
            OnlyLetters,
            None
        }
        private static readonly int _panelWidth = 350;
        private static readonly int _panelBtnWidth = 340;

        // Getters
        public static int PanelWidth => _panelWidth;
        public static int PanelBtnWidth => _panelBtnWidth;
        public static byte PanelButtonHeight => _panelButtonHeight;
        public static byte SpaceForPanel => _spaceForPanel;
        public static byte SpaceBetweenControls => _spaceBetweenControls;

        // Construct things for menus
        public static Guna2Panel ConstructPanelForMenu(Size size, string name)
        {
            Guna2Panel panel = new()
            {
                BorderStyle = DashStyle.Solid,
                BorderColor = CustomColors.ControlPanelBorder,
                BorderThickness = 1,
                BorderRadius = 4,
                FillColor = CustomColors.PanelBtn,
                Size = size,
                Name = name
            };

            int half = _spaceForPanel / 2;
            FlowLayoutPanel flowLayoutPanel = new()
            {
                BackColor = CustomColors.PanelBtn,
                Size = new Size(size.Width - _spaceForPanel, size.Height - _spaceForPanel),
                Location = new Point(half, half)
            };
            panel.Controls.Add(flowLayoutPanel);
            return panel;
        }
        public static Guna2Separator ConstructSeperator(int width, Control control)
        {
            Guna2Separator seperator = new()
            {
                FillColor = CustomColors.ControlBorder,
                BackColor = CustomColors.PanelBtn,
                Size = new Size(width, 1),
                Margin = new Padding(0, 5, 0, 5)
            };
            control.Controls.Add(seperator);
            return seperator;
        }
        public static Guna2Button ConstructBtnForMenu(string text, int width, bool closeAllPanels, Control control)
        {
            Guna2Button menuBtn = new()
            {
                Size = new Size(width, _panelButtonHeight),
                FillColor = CustomColors.PanelBtn,
                ForeColor = CustomColors.Text,
                TextAlign = HorizontalAlignment.Left,
                Font = new Font("Segoe UI", 10),
                Text = text,
                Name = text.Replace(" ", "").Replace("...", "") + "_Button",
                Margin = new Padding(0),
                BorderColor = CustomColors.ControlBorder
            };
            menuBtn.HoverState.BorderColor = CustomColors.ControlBorder;
            menuBtn.HoverState.FillColor = CustomColors.PanelBtnHover;
            menuBtn.PressedColor = CustomColors.PanelBtnHover;
            if (closeAllPanels)
            {
                menuBtn.Click += CloseAllPanels;
            }

            menuBtn.MouseEnter += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                Label label = btn.Controls.OfType<Label>().FirstOrDefault();

                if (label != null)
                {
                    label.BackColor = CustomColors.PanelBtnHover;
                }
                btn.BorderThickness = 1;
            };
            menuBtn.MouseLeave += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                Label label = btn.Controls.OfType<Label>().FirstOrDefault();

                if (label != null)
                {
                    label.BackColor = CustomColors.PanelBtn;
                }
                btn.BorderThickness = 0;
            };

            control.Controls.Add(menuBtn);
            return menuBtn;
        }
        public static void ConstructKeyShortcut(string text, Control control)
        {
            Label KeyShortcut = new()
            {
                ForeColor = CustomColors.Text,
                BackColor = CustomColors.PanelBtn,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true,
                Top = 1,
                Name = text.Replace(" ", "") + "_Label",
                Text = text,
                AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter,
                Anchor = AnchorStyles.Top
            };

            KeyShortcut.MouseEnter += (sender, e) =>
            {
                Label label = (Label)sender;
                label.BackColor = CustomColors.PanelBtnHover;
                Guna2Button btn = (Guna2Button)label.Parent;
                btn.FillColor = CustomColors.PanelBtnHover;
                btn.BorderThickness = 1;
            };
            KeyShortcut.MouseLeave += (sender, e) =>
            {
                Label label = (Label)sender;
                label.BackColor = CustomColors.PanelBtn;
                Guna2Button btn = (Guna2Button)label.Parent;
                btn.FillColor = CustomColors.PanelBtn;
                btn.BorderThickness = 0;
            };
            KeyShortcut.MouseUp += (sender, e) =>
            {
                Label label = (Label)sender;
                Guna2Button btn = (Guna2Button)label.Parent;
                btn.PerformClick();
            };
            control.Controls.Add(KeyShortcut);

            // The .AutoSize property does not work until the control has been added
            KeyShortcut.Left = control.Width - KeyShortcut.Width - offsetForKeyboardShortcutOrArrow;
        }

        // fileMenu
        private static Guna2Panel _fileMenu, _recentlyOpenedMenu;
        private static Guna2Button _openRecentCompany_Button;
        public static Guna2Panel FileMenu
        {
            get => _fileMenu;
            set => _fileMenu = value;
        }
        public static Guna2Panel RecentlyOpenedMenu
        {
            get => _recentlyOpenedMenu;
            set => _recentlyOpenedMenu = value;
        }
        public static Guna2Button OpenRecentCompany_Button
        {
            get => _openRecentCompany_Button;
            set => _openRecentCompany_Button = value;
        }
        private static void ConstructFileMenu()
        {
            _fileMenu = ConstructPanelForMenu(new Size(_panelWidth, 9 * _panelButtonHeight + spaceForSeperator * 2 + _spaceForPanel), "fileMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_fileMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("New company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Process.Start(Application.ExecutablePath, "autoClickButton");
            };

            menuBtn = ConstructBtnForMenu("Open company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.OpenProjectWhenAProgramIsAlreadyOpen();
            };

            menuBtn = ConstructBtnForMenu("Open recent company", _panelBtnWidth, false, flowPanel);
            menuBtn.ImageSize = new Size(11, 11);
            menuBtn.ImageOffset = new Point((menuBtn.Width - menuBtn.ImageSize.Width - offsetForKeyboardShortcutOrArrow) - (menuBtn.Width / 2), 0);
            menuBtn.Tag = _recentlyOpenedMenu;
            Theme.SetRightArrowImageBasedOnTheme(menuBtn);
            menuBtn.Click += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                SetRecentlyOpenedMenu();
                CascadingMenu.OpenMenu();
            };
            menuBtn.MouseEnter += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                SetRecentlyOpenedMenu();
            };
            menuBtn.MouseLeave += CascadingMenu.CloseMenu;
            _openRecentCompany_Button = menuBtn;

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Save", _panelBtnWidth, true, flowPanel);
            menuBtn.Name = "Save";
            menuBtn.Click += (_, _) =>
            {
                SaveAll();
            };
            ConstructKeyShortcut("Ctrl+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Save as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.SaveAs();
            };
            ConstructKeyShortcut("Ctrl+Shift+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Export as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Export_Form());
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);

            menuBtn = ConstructBtnForMenu("Export receipts", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Receipts_Form());
            };

            menuBtn = ConstructBtnForMenu("Import spreadsheet", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                if (ShouldShowTutorial())
                {
                    Tools.OpenForm(new Setup_Form());
                }
                else
                {
                    Tools.OpenForm(new ImportSpreadsheet_Form());
                }
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show company in folder", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.ShowFileInFolder(Directories.ArgoCompany_file);
            };
        }
        private static void ConstructRecentlyOpenedMenu()
        {
            _recentlyOpenedMenu = ConstructPanelForMenu(new Size(_panelWidth, 100), "recentlyOpenedMenu_Panel");
            _recentlyOpenedMenu.MouseEnter += (_, _) => { CascadingMenu.KeepMenuOpen(); };
        }
        private static void SetRecentlyOpenedMenu()
        {
            if (MainMenu_Form.Instance.Controls.Contains(_recentlyOpenedMenu))
            {
                CascadingMenu.KeepMenuOpen();
                return;
            }

            FlowLayoutPanel flowPanel = _recentlyOpenedMenu.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            flowPanel.Controls.Clear();

            List<string> validProjectDirs = ArgoCompany.GetValidRecentProjectPaths(true);

            if (validProjectDirs.Count == 0)
            {
                LabelManager.AddNoRecentlyOpenedCompanies(flowPanel, _panelBtnWidth);
            }
            else
            {
                // Construct buttons
                foreach (string projectDir in validProjectDirs)
                {
                    string text = Path.GetFileNameWithoutExtension(projectDir);
                    Guna2Button menuBtn = ConstructBtnForMenu(text, _panelBtnWidth, true, flowPanel);
                    menuBtn.Tag = projectDir;
                    menuBtn.MouseEnter += (_, _) => { CascadingMenu.KeepMenuOpen(); };
                    menuBtn.Click += (sender, e) =>
                    {
                        Guna2Button button = (Guna2Button)sender;
                        string path = button.Tag.ToString();
                        ArgoCompany.OpenProject(path);
                    };
                }
            }

            SetRightClickMenuHeight(_recentlyOpenedMenu);

            _recentlyOpenedMenu.Location = new Point(_fileMenu.Right,
                _fileMenu.Top + _panelButtonHeight * 2);

            MainMenu_Form.Instance.Controls.Add(_recentlyOpenedMenu);
            _recentlyOpenedMenu.BringToFront();
        }
        public static bool ShouldShowTutorial()
        {
            string value = DataFileManager.GetValue(DataFileManager.GlobalAppDataSettings.ImportSpreadsheetTutorial);
            return bool.TryParse(value, out bool boolResult) && boolResult;
        }
        public static void SaveAll()
        {
            MainMenu_Form.Instance.Saved_Label.Visible = true;

            if (ArgoCompany.AreAnyChangesMade())
            {
                MainMenu_Form.Instance.Saved_Label.ForeColor = CustomColors.AccentGreen;
                MainMenu_Form.Instance.Saved_Label.Text = "Saving...";
                ArgoCompany.SaveAll();
                MainMenu_Form.Instance.Saved_Label.Text = "Saved";
            }
            else if (Tools.IsFormOpen(typeof(MainMenu_Form)))
            {
                MainMenu_Form.Instance.Saved_Label.ForeColor = CustomColors.Text;
                MainMenu_Form.Instance.Saved_Label.Text = "No changes found";
            }

            System.Windows.Forms.Timer timer = new()
            {
                Interval = 3000
            };
            timer.Tick += (_, _) =>
            {
                MainMenu_Form.Instance.Saved_Label.Visible = false;
                timer.Stop();
            };
            timer.Start();
        }

        // helpMenu
        private static Guna2Panel _helpMenu;
        public static Guna2Panel HelpMenu
        {
            get => _helpMenu;
            set => _helpMenu = value;
        }
        private static void ConstructHelpMenu()
        {
            _helpMenu = ConstructPanelForMenu(new Size(_panelWidth, 7 * _panelButtonHeight + spaceForSeperator * 2 + _spaceForPanel), "helpMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_helpMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Support", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Forums", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Contact us", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show logs", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Log_Form());
            };
            ConstructKeyShortcut("Ctrl+L", menuBtn);

            menuBtn = ConstructBtnForMenu("Clear cache", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.ClearCache();
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("What's new", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("About", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };
        }

        // accountMenu
        private static Guna2Panel _accountMenu;
        public static Guna2Panel AccountMenu
        {
            get => _accountMenu;
            set => _accountMenu = value;
        }
        private static void ConstructAccountMenu()
        {
            AccountMenu = ConstructPanelForMenu(new Size(_panelWidth, 4 * _panelButtonHeight + spaceForSeperator + _spaceForPanel), "accountMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)AccountMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Argo account", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };

            menuBtn = ConstructBtnForMenu("Settings", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                MainMenu_Form.Instance.OpenSettingsMenu();
            };

            menuBtn = ConstructBtnForMenu("Share feedback", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Sign out", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("");
            };
        }

        // Control dropdown
        private static Guna2Button _controlsDropDown_Button;
        private static Guna2Panel _controlDropDown_Panel;
        public static Guna2Button ControlsDropDown_Button => _controlsDropDown_Button;
        public static Guna2Panel ControlDropDown_Panel => _controlDropDown_Panel;
        private static void ContructControlsDropDownButton()
        {
            _controlsDropDown_Button = new Guna2Button
            {
                BackColor = Color.Transparent,
                BorderColor = CustomColors.ControlBorder,
                FillColor = CustomColors.PanelBtn,
                ForeColor = CustomColors.Text,
                BorderRadius = 3,
                BorderThickness = 2,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                Image = Resources.DownArrowFullGray,
                ImageAlign = HorizontalAlignment.Right,
                ImageSize = new Size(8, 8),
                Size = new Size(225, 60),
                TabIndex = 13,
                Text = "Controls",
                TextOffset = new Point(-10, 0)
            };
            _controlsDropDown_Button.Click += ControlsDropDownButton_Click;
        }
        private static void ControlsDropDownButton_Click(object sender, EventArgs e)
        {
            if (MainMenu_Form.Instance.Controls.Contains(_controlDropDown_Panel))
            {
                MainMenu_Form.Instance.Controls.Remove(_controlDropDown_Panel);
            }
            else
            {
                CloseAllPanels(null, null);

                _controlDropDown_Panel.Location = new Point(
                    _controlsDropDown_Button.Right - _controlDropDown_Panel.Width,
                    MainMenu_Form.Instance.MainTop_Panel.Top + MainMenu_Form.Instance.MainTop_Panel.Height);

                MainMenu_Form.Instance.Controls.Add(_controlDropDown_Panel);
                _controlDropDown_Panel.BringToFront();
                _controlDropDown_Panel.Focus();
            }
        }
        private static void ConstructControlsDropDownMenu()
        {
            int btnWidth = 290;
            byte btnHeight = 50;

            _controlDropDown_Panel = ConstructPanelForMenu(new Size(300, btnHeight * 6 + 15 + spaceForSeperator * 5), "controlDropDown_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_controlDropDown_Panel.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageAccountants_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Accountants_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCompanies_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Companies_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCategories_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Categories_Form(true));
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageProducts_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Products_Form(true));
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Sales_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new AddSale_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Purchases_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new AddPurchase_Form());
            };
        }

        // Init hover effect for Guna2ImageButton
        public static void InitHoverEffectForImageButton(Guna2ImageButton control, bool forSearchBox)
        {
            Color hoverColor = CustomColors.FileHover;
            Color defaultColor = forSearchBox ? CustomColors.ControlBack : CustomColors.MainBackground;

            control.Tag = (hoverColor, defaultColor);

            control.MouseEnter += Control_MouseEnter;
            control.MouseLeave += Control_MouseLeave;
        }
        private static void Control_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Guna2ImageButton button && button.Tag is ValueTuple<Color, Color> colors)
            {
                button.BackColor = colors.Item1;  // hoverColor
            }
        }
        private static void Control_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2ImageButton button && button.Tag is ValueTuple<Color, Color> colors)
            {
                button.BackColor = colors.Item2;  // defaultColor
            }
        }

        // Rename
        private static Guna2TextBox _rename_TextBox;
        public static Guna2TextBox Rename_TextBox
        {
            get => _rename_TextBox;
            set => _rename_TextBox = value;
        }
        public static void ConstructRightClickRename()
        {
            _rename_TextBox = new Guna2TextBox
            {
                Font = new Font("Segoe UI", 10),
                Top = 1,
                MaxLength = 30,
                ForeColor = CustomColors.Text,
                FillColor = CustomColors.ControlBack,
                BorderStyle = DashStyle.Solid,
                TextOffset = new Point(-3, 0),
                BorderThickness = 1,
                ShortcutsEnabled = false
            };
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                _rename_TextBox.BorderColor = Color.White;
                _rename_TextBox.HoverState.BorderColor = Color.White;
                _rename_TextBox.FocusedState.BorderColor = Color.White;
            }
            else
            {
                _rename_TextBox.BorderColor = Color.Black;
                _rename_TextBox.HoverState.BorderColor = Color.Black;
                _rename_TextBox.FocusedState.BorderColor = Color.Black;
            }

            _rename_TextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                {
                    Rename();
                }
            };
            TextBoxManager.Attach(_rename_TextBox);
        }

        // Other methods
        public static void SetRightClickMenuHeight(Guna2Panel panel)
        {
            FlowLayoutPanel flowPanel = panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            int controlCount = flowPanel.Controls.Count;

            panel.Height = controlCount * PanelButtonHeight + SpaceForPanel;
            flowPanel.Height = controlCount * PanelButtonHeight;
        }

        // Validity
        public static void SetGTextBoxToValid(Guna2TextBox textBox)
        {
            textBox.BorderColor = CustomColors.ControlBorder;
            textBox.FocusedState.BorderColor = CustomColors.AccentBlue;
        }
        public static void SetGTextBoxToInvalid(Guna2TextBox textBox)
        {
            textBox.BorderColor = Color.Red;
            textBox.FocusedState.BorderColor = Color.Red;
        }

        // Close all panels
        public static void Rename()
        {
            if (Tools.IsFormOpen(typeof(MainMenu_Form)))
            {
                MainMenu_Form.Instance.RenameCompany();
            }
            else if (Tools.IsFormOpen(typeof(GetStarted_Form)))
            {
                GetStarted_Form.Instance.RenameCompany();
            }
        }
        public static void CloseAllPanels(object sender, EventArgs? e)
        {
            Rename();

            if (MainMenu_Form.Instance == null) { return; }

            MainMenu_Form.Instance.Controls.Remove(_fileMenu);
            MainMenu_Form.Instance.Controls.Remove(_recentlyOpenedMenu);
            MainMenu_Form.Instance.Controls.Remove(_helpMenu);
            MainMenu_Form.Instance.Controls.Remove(AccountMenu);
            MainMenu_Form.Instance.CloseDateRangePanel();
            MenuKeyShortcutManager.SelectedPanel = null;
            DeselectAllMenuButtons(_fileMenu);
            DeselectAllMenuButtons(_recentlyOpenedMenu);
            DeselectAllMenuButtons(_helpMenu);
            DeselectAllMenuButtons(AccountMenu);

            MainMenu_Form.Instance.File_Button.Image = Resources.FileGray;
            MainMenu_Form.Instance.Help_Button.Image = Resources.HelpGray;
            MainMenu_Form.Instance.Account_Button.Image = Resources.ProfileGray;
            MainMenu_Form.Instance.Controls.Remove(_controlDropDown_Panel);
            MainMenu_Form.Instance.ClosePanels();
        }
        private static void DeselectAllMenuButtons(Guna2Panel panel)
        {
            foreach (Control control in panel.Controls[0].Controls)
            {
                if (control is Guna2Button button && button.BorderThickness == 1)
                {
                    button.BorderThickness = 0;
                }
            }
        }
    }
}