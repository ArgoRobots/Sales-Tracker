using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ImportSpreadsheet;
using Sales_Tracker.Properties;
using Sales_Tracker.Settings;
using Sales_Tracker.Startup;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages the construction and customization of various UI controls, including menus, buttons, panels, and validation styling. 
    /// This class helps streamline UI setup and ensures consistency across different control types.
    /// </summary>
    internal static class CustomControls
    {
        /// <summary>
        /// Constructs various UI controls used throughout the application, including FileMenu, HelpMenu, and more.
        /// </summary>
        public static void ConstructControls()
        {
            CascadingMenu.Init();

            // Main menu controls
            ConstructRecentlyOpenedMenu();
            ConstructFileMenu();
            ConstructHelpMenu();
            ContructControlsDropDownButton();
            ConstructControlsDropDownMenu();

            // Other controls
            DataGridViewManager.ConstructRightClickRowMenu();
            RightClickGunaChartMenu.ConstructRightClickGunaChartMenu();

            // Set language
            LanguageManager.UpdateLanguageForControl(RecentlyOpenedMenu);
            LanguageManager.UpdateLanguageForControl(FileMenu);
            LanguageManager.UpdateLanguageForControl(HelpMenu);
            LanguageManager.UpdateLanguageForControl(ControlsDropDown_Button);
            LanguageManager.UpdateLanguageForControl(ControlDropDown_Panel);
            LanguageManager.UpdateLanguageForControl(DataGridViewManager.RightClickDataGridView_Panel);
            LanguageManager.UpdateLanguageForControl(RightClickGunaChartMenu.RightClickGunaChart_Panel);
        }

        // Properties
        private static readonly byte spaceForSeperator = 11, offsetForKeyboardShortcutOrArrow = 15;
        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbersAndDecimal,
            OnlyNumbers,
            OnlyLetters,
            None
        }

        // Getters
        public static short PanelWidth { get; } = 350;
        public static short PanelBtnWidth { get; } = 340;
        public static byte PanelButtonHeight { get; } = 35;
        public static byte SpaceForPanel { get; } = 10;
        public static byte SpaceBetweenControls { get; } = 8;

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

            int half = SpaceForPanel / 2;
            FlowLayoutPanel flowLayoutPanel = new()
            {
                BackColor = CustomColors.PanelBtn,
                Size = new Size(size.Width - SpaceForPanel, size.Height - SpaceForPanel),
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
                Size = new Size(width, PanelButtonHeight),
                FillColor = CustomColors.PanelBtn,
                ForeColor = CustomColors.Text,
                TextAlign = HorizontalAlignment.Left,
                Font = new Font("Segoe UI", 10),
                Text = LanguageManager.TranslateString(text),
                Name = FormatControlName(text, "_Button"),
                Margin = new Padding(0),
                BorderColor = CustomColors.ControlBorder,
                PressedColor = CustomColors.PanelBtnHover,
                HoverState = {
                    FillColor = CustomColors.PanelBtnHover,
                    BorderColor = CustomColors.ControlBorder
                },
            };
            if (closeAllPanels)
            {
                menuBtn.Click += (sender, e) => CloseAllPanels();
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
                Name = FormatControlName(text, "_Label"),
                Text = text,
                AccessibleDescription = AccessibleDescriptionManager.AlignRight,
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
        public static Guna2Panel FileMenu { get; set; }
        public static Guna2Panel RecentlyOpenedMenu { get; set; }
        public static Guna2Button OpenRecentCompany_Button { get; set; }
        private static void ConstructFileMenu()
        {
            FileMenu = ConstructPanelForMenu(new Size(PanelWidth, 9 * PanelButtonHeight + spaceForSeperator * 2 + SpaceForPanel), "fileMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)FileMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Create new company", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Startup_Form(["autoClickButton"]));
            };

            menuBtn = ConstructBtnForMenu("Open company", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.OpenCompanyWhenACompanyIsAlreadyOpen();
            };

            menuBtn = ConstructBtnForMenu("Open recent company", PanelBtnWidth, false, flowPanel);
            menuBtn.ImageSize = new Size(11, 11);
            menuBtn.ImageOffset = new Point((menuBtn.Width - menuBtn.ImageSize.Width - offsetForKeyboardShortcutOrArrow) - (menuBtn.Width / 2), 0);
            menuBtn.Tag = RecentlyOpenedMenu;
            ThemeManager.SetRightArrowImageBasedOnTheme(menuBtn);
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
            OpenRecentCompany_Button = menuBtn;

            ConstructSeperator(PanelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Save", PanelBtnWidth, true, flowPanel);
            menuBtn.Name = "Save";
            menuBtn.Click += (_, _) =>
            {
                SaveAll();
            };
            ConstructKeyShortcut("Ctrl+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Save as...", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.SaveAs();
            };
            ConstructKeyShortcut("Ctrl+Shift+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Export as...", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Export_Form());
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);

            menuBtn = ConstructBtnForMenu("Export receipts", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Receipts_Form());
            };

            menuBtn = ConstructBtnForMenu("Import spreadsheet", PanelBtnWidth, true, flowPanel);
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

            ConstructSeperator(PanelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show company in folder", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.ShowFileInFolder(Directories.ArgoCompany_file);
            };
        }
        private static void ConstructRecentlyOpenedMenu()
        {
            RecentlyOpenedMenu = ConstructPanelForMenu(new Size(PanelWidth, 100), "recentlyOpenedMenu_Panel");
            RecentlyOpenedMenu.MouseEnter += (_, _) => CascadingMenu.KeepMenuOpen();
        }
        private static void SetRecentlyOpenedMenu()
        {
            if (MainMenu_Form.Instance.Controls.Contains(RecentlyOpenedMenu))
            {
                CascadingMenu.KeepMenuOpen();
                return;
            }

            FlowLayoutPanel flowPanel = RecentlyOpenedMenu.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            flowPanel.Controls.Clear();

            List<string> validCompanyDirs = ArgoCompany.GetValidRecentCompanyPaths(true);

            if (validCompanyDirs.Count == 0)
            {
                LabelManager.AddNoRecentlyOpenedCompanies(flowPanel, PanelBtnWidth);
            }
            else
            {
                // Construct buttons
                foreach (string companyDir in validCompanyDirs)
                {
                    string text = Path.GetFileNameWithoutExtension(companyDir);
                    Guna2Button menuBtn = ConstructBtnForMenu(text, PanelBtnWidth, true, flowPanel);
                    menuBtn.Tag = companyDir;
                    menuBtn.MouseEnter += (_, _) => CascadingMenu.KeepMenuOpen();
                    menuBtn.Click += (sender, e) =>
                    {
                        Guna2Button button = (Guna2Button)sender;
                        string path = button.Tag.ToString();
                        ArgoCompany.OpenCompanyWhenACompanyIsAlreadyOpenFromPath(path);
                    };
                }
            }

            SetRightClickMenuHeight(RecentlyOpenedMenu);

            RecentlyOpenedMenu.Location = new Point(FileMenu.Right,
                FileMenu.Top + PanelButtonHeight * 2);

            MainMenu_Form.Instance.Controls.Add(RecentlyOpenedMenu);
            RecentlyOpenedMenu.BringToFront();
        }
        public static bool ShouldShowTutorial()
        {
            string value = DataFileManager.GetValue(GlobalAppDataSettings.ImportSpreadsheetTutorial);
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
            else
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
        public static Guna2Panel HelpMenu { get; set; }
        private static void ConstructHelpMenu()
        {
            HelpMenu = ConstructPanelForMenu(new Size(PanelWidth, 8 * PanelButtonHeight + spaceForSeperator + SpaceForPanel), "helpMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)HelpMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Settings", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Settings_Form());
            };

            menuBtn = ConstructBtnForMenu("What's new", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("https://argorobots.com/whats-new/index.html");
            };

            menuBtn = ConstructBtnForMenu("Documentaion", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("https://argorobots.com/documentation/index.html");
            };

            menuBtn = ConstructBtnForMenu("Show logs", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Log_Form());
            };
            ConstructKeyShortcut("Ctrl+L", menuBtn);

            ConstructSeperator(PanelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Contact us", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("https://argorobots.com/contact-us/index.php");
            };

            menuBtn = ConstructBtnForMenu("About", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("https://argorobots.com/about-us/index.html");
            };

            menuBtn = ConstructBtnForMenu("Share feedback", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenLink("https://argorobots.com/contact-us/index.php");
            };

            menuBtn = ConstructBtnForMenu("Clear cache", PanelBtnWidth, true, flowPanel);
            menuBtn.Click += (_, _) =>
            {
                ArgoCompany.ClearCache();
            };
        }

        // Control dropdown
        public static Guna2Button ControlsDropDown_Button { get; private set; }
        public static Guna2Panel ControlDropDown_Panel { get; private set; }
        private static void ContructControlsDropDownButton()
        {
            ControlsDropDown_Button = new Guna2Button
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
                TextOffset = new Point(-10, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            ControlsDropDown_Button.Click += ControlsDropDownButton_Click;
        }
        private static void ControlsDropDownButton_Click(object sender, EventArgs e)
        {
            if (MainMenu_Form.Instance.Controls.Contains(ControlDropDown_Panel))
            {
                MainMenu_Form.Instance.Controls.Remove(ControlDropDown_Panel);
            }
            else
            {
                CloseAllPanels();

                ControlDropDown_Panel.Location = new Point(
                    ControlsDropDown_Button.Right - ControlDropDown_Panel.Width,
                    MainMenu_Form.Instance.MainTop_Panel.Top + MainMenu_Form.Instance.MainTop_Panel.Height);

                MainMenu_Form.Instance.Controls.Add(ControlDropDown_Panel);
                ControlDropDown_Panel.BringToFront();
                ControlDropDown_Panel.Focus();
            }
        }
        private static void ConstructControlsDropDownMenu()
        {
            int btnWidth = 290;
            byte btnHeight = 50;

            ControlDropDown_Panel = ConstructPanelForMenu(new Size(300, btnHeight * 6 + 15 + spaceForSeperator * 5), "controlDropDown_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)ControlDropDown_Panel.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Accountants_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Accountants_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Companies_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Companies_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Categories_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Categories_Form(true));
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Products_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new Products_Form(true));
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.AddSale_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new AddSale_Form());
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.AddPurchase_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += (_, _) =>
            {
                Tools.OpenForm(new AddPurchase_Form());
            };
        }

        // Init hover effect for Guna2ImageButton
        public static void InitHoverEffectForImageButton(Guna2ImageButton control)
        {
            control.MouseEnter += Control_MouseEnter;
            control.MouseLeave += Control_MouseLeave;
        }
        private static void Control_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Guna2ImageButton button)
            {
                button.BackColor = CustomColors.MouseHover;
            }
        }
        private static void Control_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2ImageButton button)
            {
                button.BackColor = CustomColors.MainBackground;
            }
        }

        // Rename
        public static Guna2TextBox Rename_TextBox { get; set; }
        public static void ConstructRightClickRename()
        {
            Rename_TextBox = new Guna2TextBox
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
            if (ThemeManager.IsDarkTheme())
            {
                Rename_TextBox.BorderColor = Color.White;
                Rename_TextBox.HoverState.BorderColor = Color.White;
                Rename_TextBox.FocusedState.BorderColor = Color.White;
            }
            else
            {
                Rename_TextBox.BorderColor = Color.Black;
                Rename_TextBox.HoverState.BorderColor = Color.Black;
                Rename_TextBox.FocusedState.BorderColor = Color.Black;
            }

            Rename_TextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                {
                    Rename();

                    // Remove Windows "ding" noise when user presses enter
                    e.SuppressKeyPress = true;
                }
            };
            TextBoxManager.Attach(Rename_TextBox);
        }

        // Other methods
        public static void SetRightClickMenuHeight(Guna2Panel panel)
        {
            FlowLayoutPanel flowPanel = panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            int controlCount = flowPanel.Controls.Cast<Control>().Count(c => c.Visible);

            panel.Height = controlCount * PanelButtonHeight + SpaceForPanel;
            flowPanel.Height = controlCount * PanelButtonHeight;
        }
        private static string FormatControlName(string text, string suffix)
        {
            // Capitalize first letter of each word
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string titleCaseText = textInfo.ToTitleCase(text);

            // Remove spaces and ellipses
            string cleanText = titleCaseText.Replace(" ", "").Replace("...", "");

            // Add suffix
            return cleanText + suffix;
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
        public static bool IsGTextBoxInvalid(Guna2TextBox textBox)
        {
            return textBox.BorderColor == Color.Red;
        }

        // Animate buttons
        public static void AnimateButtons(IEnumerable<Guna2Button> buttons, bool animate)
        {
            foreach (Guna2Button button in buttons)
            {
                button.Animated = animate;
            }
        }

        // Close all panels
        public static void Rename()
        {
            if (Tools.IsFormOpen<MainMenu_Form>())
            {
                MainMenu_Form.Instance.RenameCompany();
            }
            else if (Tools.IsFormOpen<GetStarted_Form>())
            {
                GetStarted_Form.Instance.RenameCompany();
            }
        }
        public static void CloseAllPanels()
        {
            Rename();
            SearchBox.CloseSearchBox();
            TextBoxManager.RightClickTextBox_Panel.Parent?.Controls.Remove(TextBoxManager.RightClickTextBox_Panel);

            if (MainMenu_Form.Instance == null) { return; }

            MainMenu_Form.Instance.Controls.Remove(FileMenu);
            MainMenu_Form.Instance.Controls.Remove(RecentlyOpenedMenu);
            MainMenu_Form.Instance.Controls.Remove(HelpMenu);
            MainMenu_Form.Instance.CloseDateRangePanel();
            MenuKeyShortcutManager.SelectedPanel = null;
            DeselectAllMenuButtons(FileMenu);
            DeselectAllMenuButtons(RecentlyOpenedMenu);
            DeselectAllMenuButtons(HelpMenu);

            MainMenu_Form.Instance.File_Button.Image = Resources.FileGray;
            MainMenu_Form.Instance.Help_Button.Image = Resources.HelpGray;
            MainMenu_Form.Instance.Controls.Remove(ControlDropDown_Panel);
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