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
    internal class CustomControls
    {
        public static void ConstructControls()
        {
            // Main menu controls
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
            LanguageManager.UpdateLanguageForControl(_helpMenu);
            LanguageManager.UpdateLanguageForControl(_accountMenu);
            LanguageManager.UpdateLanguageForControl(_controlsDropDown_Button);
            LanguageManager.UpdateLanguageForControl(_controlDropDown_Panel);
            LanguageManager.UpdateLanguageForControl(DataGridViewManager.RightClickDataGridView_Panel);
        }

        // Properties
        private static readonly byte _panelButtonHeight = 35, spaceForSeperator = 10, _spaceForPanel = 10, _spaceBetweenControls = 8;
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
                BorderColor = CustomColors.controlPanelBorder,
                BorderThickness = 1,
                BorderRadius = 4,
                FillColor = CustomColors.panelBtn,
                Size = size,
                Name = name
            };

            int half = _spaceForPanel / 2;
            FlowLayoutPanel flowLayoutPanel = new()
            {
                BackColor = CustomColors.panelBtn,
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
                FillColor = CustomColors.controlBorder,
                BackColor = CustomColors.panelBtn,
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
                FillColor = CustomColors.panelBtn,
                ForeColor = CustomColors.text,
                TextAlign = HorizontalAlignment.Left,
                Font = new Font("Segoe UI", 10),
                Text = text,
                Name = text.Replace(" ", "").Replace("...", "") + "_Button",
                Margin = new Padding(0),
                BorderColor = CustomColors.controlBorder
            };
            menuBtn.HoverState.BorderColor = CustomColors.controlBorder;
            menuBtn.HoverState.FillColor = CustomColors.panelBtnHover;
            menuBtn.PressedColor = CustomColors.panelBtnHover;
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
                    label.BackColor = CustomColors.panelBtnHover;
                }
                btn.BorderThickness = 1;
            };
            menuBtn.MouseLeave += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                Label label = btn.Controls.OfType<Label>().FirstOrDefault();

                if (label != null)
                {
                    label.BackColor = CustomColors.panelBtn;
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
                ForeColor = CustomColors.text,
                BackColor = CustomColors.panelBtn,
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
                label.BackColor = CustomColors.panelBtnHover;
                Guna2Button btn = (Guna2Button)label.Parent;
                btn.FillColor = CustomColors.panelBtnHover;
                btn.BorderThickness = 1;
            };
            KeyShortcut.MouseLeave += (sender, e) =>
            {
                Label label = (Label)sender;
                label.BackColor = CustomColors.panelBtn;
                Guna2Button btn = (Guna2Button)label.Parent;
                btn.FillColor = CustomColors.panelBtn;
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
            KeyShortcut.Left = control.Width - KeyShortcut.Width - 15;
        }

        // fileMenu
        private static Guna2Panel _fileMenu;
        public static Guna2Panel FileMenu
        {
            get => _fileMenu;
            set => _fileMenu = value;
        }
        private static void ConstructFileMenu()
        {
            _fileMenu = ConstructPanelForMenu(new Size(_panelWidth, 8 * _panelButtonHeight + spaceForSeperator * 2 + _spaceForPanel), "fileMenu_Panel");
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_fileMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("New company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Process.Start(Application.ExecutablePath, "autoClickButton");
            };

            menuBtn = ConstructBtnForMenu("Open company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                ArgoCompany.OpenProjectWhenAProgramIsAlreadyOpen();
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Save", _panelBtnWidth, true, flowPanel);
            menuBtn.Name = "Save";
            menuBtn.Click += delegate
            {
                SaveAll();
            };
            ConstructKeyShortcut("Ctrl+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Save as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                ArgoCompany.SaveAs();
            };
            ConstructKeyShortcut("Ctrl+Shift+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Export as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                new Export_Form().ShowDialog();
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);

            menuBtn = ConstructBtnForMenu("Export receipts", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                new Receipts_Form().ShowDialog();
            };

            menuBtn = ConstructBtnForMenu("Import spreadsheet", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                if (ShouldShowTutorial())
                {
                    new Setup_Form().ShowDialog();
                }
                else
                {
                    new ImportSpreadsheet_Form().ShowDialog();
                }
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show company in folder", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Tools.ShowFileInFolder(Directories.ArgoCompany_file);
            };
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
                MainMenu_Form.Instance.Saved_Label.ForeColor = CustomColors.accent_green;
                MainMenu_Form.Instance.Saved_Label.Text = "Saving...";
                ArgoCompany.SaveAll();
                MainMenu_Form.Instance.Saved_Label.Text = "Saved";
            }
            else if (Tools.IsFormOpen(typeof(MainMenu_Form)))
            {
                MainMenu_Form.Instance.Saved_Label.ForeColor = CustomColors.text;
                MainMenu_Form.Instance.Saved_Label.Text = "No changed found";
            }

            System.Windows.Forms.Timer timer = new()
            {
                Interval = 3000
            };
            timer.Tick += delegate
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
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Forums", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Contact us", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show logs", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                MainMenu_Form.Instance.OpenLogs();
            };
            ConstructKeyShortcut("Ctrl+L", menuBtn);

            menuBtn = ConstructBtnForMenu("Clear cache", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                ArgoCompany.ClearCache();
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("What's new", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("About", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
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
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };

            menuBtn = ConstructBtnForMenu("Settings", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                MainMenu_Form.Instance.OpenSettingsMenu();
            };

            menuBtn = ConstructBtnForMenu("Share feedback", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Sign out", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += delegate
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
                BorderColor = CustomColors.controlBorder,
                FillColor = CustomColors.panelBtn,
                ForeColor = CustomColors.text,
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
            menuBtn.Click += delegate
            {
                new Accountants_Form().ShowDialog();
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCompanies_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += delegate
            {
                new Companies_Form().ShowDialog();
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCategories_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += delegate
            {
                new Categories_Form(true).ShowDialog();
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageProducts_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += delegate
            {
                new Products_Form(true).ShowDialog();
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Sales_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += delegate
            {
                new AddSale_Form().ShowDialog();
            };

            ConstructSeperator(btnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Purchases_Button.Text, btnWidth, true, flowPanel);
            menuBtn.Height = btnHeight;
            menuBtn.Click += delegate
            {
                new AddPurchase_Form().ShowDialog();
            };
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
                ForeColor = CustomColors.text,
                FillColor = CustomColors.controlBack,
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

        // Validity
        public static void SetGTextBoxToValid(Guna2TextBox textBox)
        {
            textBox.BorderColor = CustomColors.controlBorder;
            textBox.FocusedState.BorderColor = CustomColors.accent_blue;
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
            MainMenu_Form.Instance.Controls.Remove(_helpMenu);
            MainMenu_Form.Instance.Controls.Remove(AccountMenu);
            MainMenu_Form.Instance.CloseDateRangePanel();
            DeselectAllMenuButtons(_fileMenu);
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