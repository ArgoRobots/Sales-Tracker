using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using System.Runtime.InteropServices;

namespace Sales_Tracker.Classes
{
    internal static partial class Theme
    {
        public enum ThemeType
        {
            Light,
            Dark,
            Windows
        }
        public static ThemeType CurrentTheme
        {
            get
            {
                if (Enum.TryParse(Properties.Settings.Default.ColorTheme, out ThemeType theme))
                {
                    return theme;
                }
                else
                {
                    // Default to Windows if parsing fails
                    return ThemeType.Windows;
                }
            }
            set
            {
                Properties.Settings.Default.ColorTheme = value.ToString();
                Properties.Settings.Default.Save();
            }
        }
        public static void SetThemeForControl(List<Control> list)
        {
            foreach (Control control in list)
            {
                bool setCurrentControl = false;
                if (control.HasChildren)
                {
                    // Recursively loop through the child controls
                    List<Control> childList = new();
                    foreach (Control item in control.Controls)
                    {
                        childList.Add(item);
                    }
                    SetThemeForControl(childList);
                    setCurrentControl = true;
                }
                if (!control.HasChildren || setCurrentControl)
                {
                    switch (control)
                    {
                        case LinkLabel linkLabel:
                            linkLabel.LinkColor = CustomColors.accent_blue;
                            linkLabel.VisitedLinkColor = CustomColors.accent_blue;
                            linkLabel.ActiveLinkColor = CustomColors.accent_blue;
                            linkLabel.ForeColor = CustomColors.text;
                            linkLabel.LinkBehavior = LinkBehavior.HoverUnderline;
                            break;

                        case Label label:
                            label.ForeColor = CustomColors.text;
                            break;

                        case PictureBox pictureBox:
                            pictureBox.BackColor = CustomColors.mainBackground;
                            break;

                        case RichTextBox richTextBox:
                            richTextBox.BackColor = CustomColors.mainBackground;
                            richTextBox.ForeColor = CustomColors.text;
                            break;

                        case FlowLayoutPanel flowLayoutPanel:
                            flowLayoutPanel.BackColor = CustomColors.mainBackground;
                            CustomizeScrollBar(flowLayoutPanel);
                            break;

                        case Guna2Button guna2Button:
                            guna2Button.FillColor = CustomColors.controlBack;
                            guna2Button.ForeColor = CustomColors.text;
                            guna2Button.BorderColor = CustomColors.controlBorder;
                            guna2Button.DisabledState.FillColor = CustomColors.controlDisabledBack;
                            guna2Button.HoverState.BorderColor = CustomColors.accent_blue;
                            guna2Button.GotFocus += Guna2Button_GotFocus;
                            guna2Button.LostFocus += Guna2Button_LostFocus;
                            break;

                        case Guna2TextBox guna2TextBox:
                            guna2TextBox.FillColor = CustomColors.controlBack;
                            guna2TextBox.HoverState.FillColor = CustomColors.controlBack;
                            guna2TextBox.DisabledState.FillColor = CustomColors.controlDisabledBack;
                            guna2TextBox.DisabledState.BorderColor = CustomColors.controlBorder;
                            guna2TextBox.ForeColor = CustomColors.text;
                            guna2TextBox.BorderColor = CustomColors.controlBorder;
                            guna2TextBox.HoverState.BorderColor = CustomColors.accent_blue;
                            guna2TextBox.FocusedState.BorderColor = CustomColors.accent_blue;
                            guna2TextBox.FocusedState.FillColor = CustomColors.controlBack;
                            break;

                        case Guna2CheckBox guna2CheckBox:
                            guna2CheckBox.ForeColor = CustomColors.text;
                            guna2CheckBox.CheckedState.BorderColor = CustomColors.accent_blue;
                            guna2CheckBox.CheckedState.FillColor = CustomColors.accent_blue;
                            guna2CheckBox.UncheckedState.BorderColor = CustomColors.controlUncheckedBorder;
                            guna2CheckBox.UncheckedState.FillColor = CustomColors.controlBack;
                            break;

                        case Guna2RadioButton guna2RadioButton:
                            guna2RadioButton.ForeColor = CustomColors.text;
                            guna2RadioButton.CheckedState.BorderColor = CustomColors.accent_blue;
                            guna2RadioButton.CheckedState.FillColor = CustomColors.accent_blue;
                            guna2RadioButton.UncheckedState.BorderColor = CustomColors.controlUncheckedBorder;
                            guna2RadioButton.UncheckedState.FillColor = CustomColors.controlBack;
                            break;

                        case Guna2Panel guna2Panel:
                            guna2Panel.FillColor = CustomColors.mainBackground;
                            break;

                        case Panel panel:
                            panel.BackColor = CustomColors.mainBackground;
                            break;

                        case Guna2TrackBar guna2TrackBar:
                            guna2TrackBar.ThumbColor = CustomColors.accent_blue;
                            guna2TrackBar.BackColor = CustomColors.mainBackground;
                            break;

                        case Guna2NumericUpDown guna2NumericUpDown:
                            guna2NumericUpDown.FillColor = CustomColors.controlBack;
                            guna2NumericUpDown.ForeColor = CustomColors.text;
                            guna2NumericUpDown.BorderColor = CustomColors.controlBorder;
                            guna2NumericUpDown.UpDownButtonFillColor = CustomColors.controlBack;
                            guna2NumericUpDown.UpDownButtonForeColor = CustomColors.text;
                            guna2NumericUpDown.UpDownButtonBorderVisible = true;
                            break;

                        case Guna2ComboBox guna2ComboBox:
                            guna2ComboBox.FillColor = CustomColors.controlBack;
                            guna2ComboBox.ForeColor = CustomColors.text;
                            guna2ComboBox.BorderColor = CustomColors.controlBorder;
                            guna2ComboBox.HoverState.BorderColor = CustomColors.accent_blue;
                            guna2ComboBox.FocusedState.BorderColor = CustomColors.accent_blue;
                            guna2ComboBox.ItemsAppearance.BackColor = CustomColors.controlBack;
                            break;

                        case Guna2DataGridView guna2DataGridView:
                            guna2DataGridView.Theme = CustomColors.dataGridViewTheme;
                            guna2DataGridView.BackgroundColor = CustomColors.controlBack;

                            UpdateDataGridViewHeaderTheme(guna2DataGridView);
                            CustomizeScrollBar(guna2DataGridView);
                            guna2DataGridView.ClearSelection();
                            break;

                        case Guna2CircleButton guna2CircleButton:
                            guna2CircleButton.FillColor = CustomColors.controlBack;
                            guna2CircleButton.HoverState.FillColor = CustomColors.controlBack;
                            guna2CircleButton.HoverState.BorderColor = CustomColors.controlBorder;
                            break;

                        case Guna2ToggleSwitch guna2ToggleSwitch:
                            guna2ToggleSwitch.CheckedState.FillColor = CustomColors.accent_blue;
                            guna2ToggleSwitch.UncheckedState.FillColor = CustomColors.controlBack;
                            break;

                        case Guna2DateTimePicker guna2DateTimePicker:
                            guna2DateTimePicker.FillColor = CustomColors.controlBack;
                            guna2DateTimePicker.ForeColor = CustomColors.text;
                            guna2DateTimePicker.BorderColor = CustomColors.controlBorder;
                            guna2DateTimePicker.HoverState.BorderColor = CustomColors.accent_blue;
                            break;

                        case GunaChart gunaChart:
                            if (CurrentTheme == ThemeType.Dark)
                            {
                                gunaChart.ApplyConfig(Dark.Config(), CustomColors.background4);
                            }
                            else
                            {
                                gunaChart.ApplyConfig(Light.Config(), Color.White);
                            }

                            if (gunaChart.Datasets.Count > 0)
                            {
                                if (gunaChart.Datasets[0] is GunaBarDataset)
                                {
                                    LoadChart.ConfigureChartForBar(gunaChart);
                                }
                                else if (gunaChart.Datasets[0] is GunaLineDataset)
                                {
                                    LoadChart.ConfigureChartForLine(gunaChart);
                                }
                                else if (gunaChart.Datasets[0] is GunaPieDataset)
                                {
                                    LoadChart.ConfigureChartForPie(gunaChart);
                                }
                            }

                            gunaChart.Title.Font = new ChartFont("Segoe UI", 20, ChartFontStyle.Bold);
                            gunaChart.Legend.LabelFont = new ChartFont("Segoe UI", 16);
                            gunaChart.Tooltips.TitleFont = new ChartFont("Segoe UI", 16, ChartFontStyle.Bold);
                            gunaChart.Tooltips.BodyFont = new ChartFont("Segoe UI", 16);
                            gunaChart.XAxes.Ticks.Font = new ChartFont("Segoe UI", 16);
                            break;
                    }
                }
            }
        }
        public static void UpdateThemeForPanel(List<Guna2Panel> listOfMenus)
        {
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
        }
        public static void UpdateDataGridViewHeaderTheme(Guna2DataGridView dataGridView)
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.HeaderCell.Style.BackColor = CustomColors.background2;
                column.HeaderCell.Style.SelectionBackColor = CustomColors.background2;
            }
        }
        private static void Guna2Button_GotFocus(object sender, EventArgs e)
        {
            Guna2Button button = (Guna2Button)sender;

            // Change the button's appearance when it receives focus
            button.BorderColor = CustomColors.accent_blue;
        }
        private static void Guna2Button_LostFocus(object sender, EventArgs e)
        {
            Guna2Button button = (Guna2Button)sender;

            // Revert the button's appearance when it loses focus
            if (button.BorderThickness == 1)
            {
                button.BorderColor = CustomColors.controlBorder;
            }
        }
        public static void CustomizeScrollBar(Control control)
        {
            Guna2VScrollBar vScrollBar = new()
            {
                FillColor = CustomColors.mainBackground,
                ThumbColor = Color.Gray,
                BorderColor = CustomColors.controlPanelBorder,
                ThumbSize = 40
            };
            vScrollBar.Scroll += (sender, e) =>
            {
                Guna2VScrollBar bar = (Guna2VScrollBar)sender;
                bar.ThumbSize = 40;
            };
            control.Controls.Add(vScrollBar);
            vScrollBar.BringToFront();
            vScrollBar.BindingContainer = control;
        }
        public static void SetThemeForForm(Form form)
        {
            form.BackColor = CustomColors.mainBackground;

            List<Control> list = new();
            foreach (Control item in form.Controls)
            {
                list.Add(item);
            }

            SetThemeForControl(list);
            UseImmersiveDarkMode(form.Handle, CurrentTheme == ThemeType.Dark);
        }
        public static void MakeSureThemeIsNotWindows()
        {
            if (CurrentTheme == ThemeType.Windows)
            {
                int? value = (int?)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);

                if (value == 0)
                {
                    CurrentTheme = ThemeType.Dark;
                }
                else if (value == 1)
                {
                    CurrentTheme = ThemeType.Light;
                }
            }
        }

        // Set the header to dark
        // https://stackoverflow.com/questions/57124243/winforms-dark-title-bar-on-windows-10

        [LibraryImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
        private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (Environment.OSVersion.Version.Build >= 18985)
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }
            return false;
        }
    }
}