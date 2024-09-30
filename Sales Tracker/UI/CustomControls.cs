using Guna.UI2.WinForms;
using Sales_Tracker.ImportSpreadsheet;
using Sales_Tracker.Properties;
using Sales_Tracker.Startup.Menus;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using Sales_Tracker.Classes;

namespace Sales_Tracker.UI
{
    internal class CustomControls
    {
        public static void ConstructControls()
        {
            // Main menu controls
            ConstructFileMenu();
            ConstructHelpMenu();
            ConstructProfileMenu();
            ContructControlsDropDownButton();
            ConstructControlsDropDownMenu();
            DataGridViewManager.ConstructRightRowMenu();
        }

        // Properties
        private static readonly byte _panelButtonHeight = 35, _spaceForSeperator = 10, _spaceForPanel = 10, _spaceBetweenControls = 8;
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
        public static byte SpaceForSeperator => _spaceForSeperator;
        public static byte SpaceForPanel => _spaceForPanel;
        public static byte SpaceBetweenControls => _spaceBetweenControls;

        // Construct things for menus
        public static Guna2Panel ConstructPanelForMenu(Size size)
        {
            Guna2Panel panel = new()
            {
                BorderStyle = DashStyle.Solid,
                BorderColor = CustomColors.controlPanelBorder,
                BorderThickness = 1,
                BorderRadius = 4,
                FillColor = CustomColors.panelBtn,
                Size = size
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
                Label label = (Label)btn.Controls.Find("label", false).FirstOrDefault();
                if (label != null)
                {
                    label.BackColor = CustomColors.panelBtnHover;
                }
                btn.BorderThickness = 1;
            };
            menuBtn.MouseLeave += (sender, e) =>
            {
                Guna2Button btn = (Guna2Button)sender;
                Label label = (Label)btn.Controls.Find("label", false).FirstOrDefault();
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
                Name = "label",
                Text = text
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
        public static Guna2Panel fileMenu;
        private static void ConstructFileMenu()
        {
            fileMenu = ConstructPanelForMenu(new Size(_panelWidth, 8 * _panelButtonHeight + _spaceForSeperator * 2 + _spaceForPanel));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)fileMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("New company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Process.Start(Application.ExecutablePath, "autoClickButton");
            };

            menuBtn = ConstructBtnForMenu("Open company", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                ArgoCompany.OpenProjectWhenAProgramIsAlreadyOpen();
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Save", _panelBtnWidth, true, flowPanel);
            menuBtn.Name = "Save";
            menuBtn.Click += (sender, e) =>
            {
                SaveAll();
            };
            ConstructKeyShortcut("Ctrl+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Save as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                ArgoCompany.SaveAs();
            };
            ConstructKeyShortcut("Ctrl+Shift+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Export as...", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                new Export_Form().ShowDialog();
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);

            menuBtn = ConstructBtnForMenu("Export receipts", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                new Receipts_Form().ShowDialog();
            };

            menuBtn = ConstructBtnForMenu("Import spreadsheet", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
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
            menuBtn.Click += (sender, e) =>
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
            timer.Tick += (sender, e) =>
            {
                MainMenu_Form.Instance.Saved_Label.Visible = false;
                timer.Stop();
            };
            timer.Start();
        }

        // helpMenu
        public static Guna2Panel helpMenu;
        private static void ConstructHelpMenu()
        {
            helpMenu = ConstructPanelForMenu(new Size(_panelWidth, 7 * _panelButtonHeight + _spaceForSeperator * 2 + _spaceForPanel));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)helpMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Support", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Forums", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Contact us", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show logs", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                MainMenu_Form.Instance.OpenLogs();
            };
            ConstructKeyShortcut("Ctrl+L", menuBtn);

            menuBtn = ConstructBtnForMenu("Clear cache", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                ArgoCompany.ClearCache();
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("What's new", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("About", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
        }

        // accountMenu
        public static Guna2Panel accountMenu;
        private static void ConstructProfileMenu()
        {
            accountMenu = ConstructPanelForMenu(new Size(_panelWidth, 4 * _panelButtonHeight + _spaceForSeperator + _spaceForPanel));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)accountMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Argo account", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };

            menuBtn = ConstructBtnForMenu("Settings", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                MainMenu_Form.Instance.OpenSettingsMenu();
            };

            menuBtn = ConstructBtnForMenu("Share feedback", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(_panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Sign out", _panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
        }

        // Control dropdown
        public static Guna2Button controlsDropDown_Button;
        private static void ContructControlsDropDownButton()
        {
            controlsDropDown_Button = new Guna2Button
            {
                BackColor = Color.Transparent,
                BorderColor = CustomColors.controlBorder,
                FillColor = CustomColors.panelBtn,
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
            controlsDropDown_Button.Click += (sender, e) =>
            {
                if (MainMenu_Form.Instance.Controls.Contains(fileMenu))
                {
                    MainMenu_Form.Instance.Controls.Remove(ControlDropDown_Panel);
                }
                else
                {
                    CloseAllPanels(null, null);
                    ControlDropDown_Panel.Location = new Point(controlsDropDown_Button.Right - ControlDropDown_Panel.Width, MainMenu_Form.Instance.MainTop_Panel.Top + MainMenu_Form.Instance.MainTop_Panel.Height);
                    MainMenu_Form.Instance.Controls.Add(ControlDropDown_Panel);
                    ControlDropDown_Panel.BringToFront();
                    ControlDropDown_Panel.Focus();
                }
            };
        }
        public static Guna2Panel ControlDropDown_Panel;
        private static void ConstructControlsDropDownMenu()
        {
            ControlDropDown_Panel = ConstructPanelForMenu(new Size(300, 50 * 6 + 15 + _spaceForSeperator * 5));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)ControlDropDown_Panel.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageAccountants_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageAccountants_Button.PerformClick();
            };

            ConstructSeperator(290, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageAccountants_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageAccountants_Button.PerformClick();
            };

            ConstructSeperator(290, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCompanies_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageCompanies_Button.PerformClick();
            };

            ConstructSeperator(290, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageProducts_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageProducts_Button.PerformClick();
            };

            ConstructSeperator(290, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Sales_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.AddSale_Button.PerformClick();
            };

            ConstructSeperator(290, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Purchases_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(290, 50);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.AddPurchase_Button.PerformClick();
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

            MainMenu_Form.Instance.Controls.Remove(fileMenu);
            MainMenu_Form.Instance.Controls.Remove(helpMenu);
            MainMenu_Form.Instance.Controls.Remove(accountMenu);
            DeselectAllMenuButtons(fileMenu);
            DeselectAllMenuButtons(helpMenu);
            DeselectAllMenuButtons(accountMenu);

            MainMenu_Form.Instance.File_Button.Image = Resources.FileGray;
            MainMenu_Form.Instance.Help_Button.Image = Resources.HelpGray;
            MainMenu_Form.Instance.Account_Button.Image = Resources.ProfileGray;
            MainMenu_Form.Instance.Controls.Remove(ControlDropDown_Panel);
            MainMenu_Form.CloseRightClickPanels();
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