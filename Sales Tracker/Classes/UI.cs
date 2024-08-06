using Guna.UI2.WinForms;
using Sales_Tracker.Properties;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.Classes
{
    internal class UI
    {
        public static void ConstructControls()
        {
            // Main menu controls
            ConstructFileMenu();
            ConstructHelpMenu();
            ConstructProfileMenu();
            ConstructRightClickRename();
            ContructControlsDropDownButton();
            ConstructControlsDropDownMenu();
            MainMenu_Form.Instance.ConstructRightClickDataGridViewRowMenu();
            MainMenu_Form.Instance.ConstructMessage_Panel();
        }

        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbersAndDecimal,
            OnlyNumbers,
            OnlyLetters,
            None
        }

        // Construct things for menus
        public static readonly byte panelWidth = 250, panelBtnWidth = 240;
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
            FlowLayoutPanel flowLayoutPanel = new()
            {
                BackColor = CustomColors.panelBtn,
                Size = new Size(size.Width - 10, size.Height - 6),
                Location = new Point(5, 5)
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
                Size = new Size(width, 22),
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
            fileMenu = ConstructPanelForMenu(new Size(panelWidth, 6 * 22 + 10 + 10));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)fileMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("New company", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Process.Start(Application.ExecutablePath, "autoClickButton");
            };

            menuBtn = ConstructBtnForMenu("Open company", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                ArgoCompany.OpenProjectWhenAProgramIsAlreadyOpen();
            };

            ConstructSeperator(panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Save", panelBtnWidth, true, flowPanel);
            menuBtn.Name = "Save";
            menuBtn.Click += (sender, e) =>
            {
                SaveAll();
            };
            ConstructKeyShortcut("Ctrl+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Save as", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                ArgoCompany.SaveAs();
            };
            ConstructKeyShortcut("Ctrl+Shift+S", menuBtn);

            menuBtn = ConstructBtnForMenu("Export / make backup", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                new Export_Form().ShowDialog();
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);

            menuBtn = ConstructBtnForMenu("Export receipts", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                new Receipts_Form().ShowDialog();
            };
            ConstructKeyShortcut("Ctrl+E", menuBtn);
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
            helpMenu = ConstructPanelForMenu(new Size(panelWidth, 7 * 22 + 10 + 20));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)helpMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Support", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Forums", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("Contact us", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };

            ConstructSeperator(panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Show logs", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                MainMenu_Form.Instance.OpenLogs();
            };
            ConstructKeyShortcut("Ctrl+L", menuBtn);

            menuBtn = ConstructBtnForMenu("Clear cache", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Directories.DeleteFile(Directories.appDataConfig_file);
            };

            ConstructSeperator(panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("What's new", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
            menuBtn = ConstructBtnForMenu("About", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };
        }

        // accountMenu
        public static Guna2Panel accountMenu;
        private static void ConstructProfileMenu()
        {
            accountMenu = ConstructPanelForMenu(new Size(panelWidth, 4 * 22 + 10 + 10));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)accountMenu.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu("Argo account", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                Tools.OpenLink("");
            };

            menuBtn = ConstructBtnForMenu("Settings", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
                MainMenu_Form.Instance.OpenSettingsMenu();
            };

            menuBtn = ConstructBtnForMenu("Share feedback", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
            };

            ConstructSeperator(panelBtnWidth, flowPanel);

            menuBtn = ConstructBtnForMenu("Sign out", panelBtnWidth, true, flowPanel);
            menuBtn.Click += (sender, e) =>
            {
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
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Image = Resources.DownArrowFull,
                ImageAlign = HorizontalAlignment.Right,
                ImageSize = new Size(8, 8),
                Size = new Size(150, 40),
                TabIndex = 13,
                Text = "Controls",
                TextOffset = new Point(-10, 0)
            };
            controlsDropDown_Button.Click += (sender, e) =>
            {
                if (MainMenu_Form.Instance.Controls.Contains(UI.fileMenu))
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
            ControlDropDown_Panel = ConstructPanelForMenu(new Size(200, 6 * 30 + 15 + 50));
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)ControlDropDown_Panel.Controls[0];

            Guna2Button menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageAccountants_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageAccountants_Button.PerformClick();
            };

            ConstructSeperator(190, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageAccountants_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageAccountants_Button.PerformClick();
            };

            ConstructSeperator(190, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageCompanies_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageCompanies_Button.PerformClick();
            };

            ConstructSeperator(190, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.ManageProducts_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.ManageProducts_Button.PerformClick();
            };

            ConstructSeperator(190, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Sales_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.AddSale_Button.PerformClick();
            };

            ConstructSeperator(190, flowPanel);

            menuBtn = ConstructBtnForMenu(MainMenu_Form.Instance.Purchases_Button.Text, 0, true, flowPanel);
            menuBtn.Size = new Size(190, 30);
            menuBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                MainMenu_Form.Instance.AddPurchase_Button.PerformClick();
            };
        }

        // Rename
        public static Guna2TextBox rename_textBox;
        public static void ConstructRightClickRename()
        {
            rename_textBox = new Guna2TextBox
            {
                Font = new Font("Segoe UI", 10),
                Height = 23,
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
                rename_textBox.BorderColor = Color.White;
                rename_textBox.HoverState.BorderColor = Color.White;
                rename_textBox.FocusedState.BorderColor = Color.White;
            }
            else
            {
                rename_textBox.BorderColor = Color.Black;
                rename_textBox.HoverState.BorderColor = Color.Black;
                rename_textBox.FocusedState.BorderColor = Color.Black;
            }

            rename_textBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
                    MainMenu_Form.Instance.RenameCompany();
                }
            };
            rename_textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            rename_textBox.KeyDown += TextBox_KeyDown;
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

        // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X in Guna2TextBox
        public static void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && e.KeyCode is Keys.A or Keys.C or Keys.X or Keys.V)
            {
                e.IsInputKey = true;
            }
        }
        public static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (e.Control && e.KeyCode == Keys.A)
            {
                // Select all
                textBox.SelectionStart = 0;
                textBox.SelectionLength = textBox.Text.Length;
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                // Copy
                if (!string.IsNullOrEmpty(textBox.SelectedText))
                {
                    Clipboard.SetText(textBox.SelectedText);
                }
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Cut
                if (!string.IsNullOrEmpty(textBox.SelectedText))
                {
                    Clipboard.SetText(textBox.SelectedText);
                    textBox.SelectedText = "";
                }
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                // Paste
                if (Clipboard.ContainsText())
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = textBox.Text.Remove(selectionStart, textBox.SelectionLength)
                                    .Insert(selectionStart, Clipboard.GetText());
                    textBox.SelectionStart = selectionStart + Clipboard.GetText().Length;
                }
                e.SuppressKeyPress = true;
            }
        }

        // Close all panels
        public static void CloseAllPanels(object sender, EventArgs? e)
        {
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
            MainMenu_Form.Instance.RenameCompany();
            MainMenu_Form.Instance.CloseRightClickPanels();
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