using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Settings.Menus;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.Settings
{
    public partial class Settings_Form : BaseForm
    {
        private readonly Form FormGeneral = new General_Form();
        private readonly Form FormSecurity = new Security_Form();
        private readonly Form FormUpdates = new Updates_Form();

        // Init.
        public static Settings_Form Instance { get; private set; }
        public Settings_Form()
        {
            InitializeComponent();
            Instance = this;

            UpdateTheme();

            // Add forms
            FormGeneral.TopLevel = false;
            FormGeneral.Dock = DockStyle.Fill;
            FormGeneral.Visible = true;
            FormBack_Panel.Controls.Add(FormGeneral);

            FormSecurity.TopLevel = false;
            FormSecurity.Dock = DockStyle.Fill;
            FormSecurity.Visible = true;
            FormBack_Panel.Controls.Add(FormSecurity);

            FormUpdates.TopLevel = false;
            FormUpdates.Dock = DockStyle.Fill;
            FormUpdates.Visible = true;
            FormBack_Panel.Controls.Add(FormUpdates);

            General_Button.PerformClick();
        }
        private void UpdateTheme()
        {
            Theme.SetThemeForForm(this);
        }


        // Left menu buttons
        private Guna2Button btnSelected;

        private void GeneralButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormGeneral, sender);
        }
        private void SecurityButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormSecurity, sender);
        }
        private void UpdatesButton_Click(object sender, EventArgs e)
        {
            SwitchForm(FormUpdates, sender);
        }


        // Bottom buttons
        private void ResetToDefault_Button_Click(object sender, EventArgs e)
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "All user settings will be reset to default.", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);
            if (result == CustomMessageBoxResult.Ok)
            {
                UserSettings.ResetAllToDefault();
                Theme.MakeSureThemeIsNotWindows();
                ApplyChanges();
            }
        }
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Apply_Button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }
        private void ApplyChanges()
        {
            UpdateColorTheme();
            UserSettings.SaveUserSettings();
        }
        private void UpdateColorTheme()
        {
            if (General_Form.Instance.ColorTheme_ComboBox.Text != Theme.CurrentTheme.ToString())
            {
                if (General_Form.Instance.ColorTheme_ComboBox.Text == Theme.ThemeType.Dark.ToString())
                {
                    Theme.CurrentTheme = Theme.ThemeType.Dark;
                }
                else if (General_Form.Instance.ColorTheme_ComboBox.Text == Theme.ThemeType.Light.ToString())
                {
                    Theme.CurrentTheme = Theme.ThemeType.Light;
                }
                else
                {
                    Theme.CurrentTheme = Theme.ThemeType.Windows;
                }

                CustomColors.SetColors();

                General_Form.Instance.UpdateTheme();
                Security_Form.Instance.UpdateTheme();
                Updates_Form.Instance.UpdateTheme();
                MainMenu_Form.Instance.UpdateTheme();
                UpdateTheme();

                List<Guna2Panel> listOfMenus = [
                    UI.fileMenu,
                    UI.helpMenu,
                    UI.accountMenu,
                    UI.ControlDropDown_Panel,
                    MainMenu_Form.Instance.rightClickDataGridView_Panel];

                foreach (Guna2Panel guna2Panel in listOfMenus)
                {
                    guna2Panel.FillColor = CustomColors.panelBtn;
                    guna2Panel.BorderColor = CustomColors.controlPanelBorder;

                    Control.ControlCollection list;
                    FlowLayoutPanel flowPanel = guna2Panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                    if (flowPanel != null)
                    {
                        flowPanel.BackColor = CustomColors.panelBtn;
                        list = flowPanel.Controls;
                    }
                    else
                    {
                        list = guna2Panel.Controls;
                    }

                    foreach (Control control in list)
                    {
                        switch (control)
                        {
                            case Guna2Separator guna2Separator:
                                guna2Separator.FillColor = CustomColors.controlPanelBorder;
                                guna2Separator.BackColor = CustomColors.panelBtn;
                                break;

                            case Guna2Button guna2Button:
                                guna2Button.FillColor = CustomColors.panelBtn;
                                guna2Button.ForeColor = CustomColors.text;
                                guna2Button.BorderColor = CustomColors.controlBorder;
                                guna2Button.HoverState.BorderColor = CustomColors.controlBorder;
                                guna2Button.HoverState.FillColor = CustomColors.panelBtnHover;
                                guna2Button.PressedColor = CustomColors.panelBtnHover;

                                foreach (Label label in guna2Button.Controls)
                                {
                                    label.ForeColor = CustomColors.text;
                                    label.BackColor = CustomColors.panelBtn;
                                }
                                break;
                        }
                    }
                }

                MainMenu_Form.Instance.rightClickDataGridView_DeleteBtn.ForeColor = CustomColors.accent_red;

                if (Theme.CurrentTheme == Theme.ThemeType.Dark)
                {
                    UI.rename_textBox.HoverState.BorderColor = Color.White;
                    UI.rename_textBox.FocusedState.BorderColor = Color.White;
                    UI.rename_textBox.BorderColor = Color.White;
                }
                else
                {
                    UI.rename_textBox.HoverState.BorderColor = Color.Black;
                    UI.rename_textBox.FocusedState.BorderColor = Color.Black;
                    UI.rename_textBox.BorderColor = Color.Black;
                }

                SearchBox.SearchResultBoxContainer.FillColor = CustomColors.controlBack;
                SearchBox.SearchResultBox.FillColor = CustomColors.controlBack;
            }
        }

        // Misc.
        private void SwitchForm(Form form, object btnSender)
        {
            UI.CloseAllPanels(null, null);
            Guna2Button btn = (Guna2Button)btnSender;

            // If btn is not already selected
            if (btn.FillColor != CustomColors.accent_blue)
            {
                // Unselect button
                if (btnSelected != null)
                {
                    btnSelected.FillColor = CustomColors.controlBack;
                    if (Theme.CurrentTheme == Theme.ThemeType.Dark)
                    {
                        btnSelected.ForeColor = Color.White;
                    }
                    else
                    {
                        btnSelected.ForeColor = Color.Black;
                    }
                }

                // Select new button
                btn.FillColor = CustomColors.accent_blue;
                btn.ForeColor = Color.White;
                form.BringToFront();

                // Save
                btnSelected = btn;
            }
        }
    }
}