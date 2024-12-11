using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Charts;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;
using System.Runtime.InteropServices;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages dark and light mode themes, and custom colors.
    /// This class provides methods for setting theme attributes on various UI controls and forms, including custom scrollbars, button colors, and DataGridView header styling.
    /// The class also supports Windows system theme detection and immersive dark mode integration on supported systems.
    /// </summary>
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
            }
        }
        public static void SetThemeForControl(List<Control> list)
        {
            foreach (Control control in list)
            {
                if (control == null) { continue; }

                switch (control)
                {
                    case LinkLabel linkLabel:
                        linkLabel.LinkColor = CustomColors.AccentBlue;
                        linkLabel.VisitedLinkColor = CustomColors.AccentBlue;
                        linkLabel.ActiveLinkColor = CustomColors.AccentBlue;
                        linkLabel.ForeColor = CustomColors.Text;
                        linkLabel.LinkBehavior = LinkBehavior.HoverUnderline;
                        break;

                    case Label label:
                        label.ForeColor = CustomColors.Text;
                        break;

                    case PictureBox pictureBox:
                        pictureBox.BackColor = CustomColors.MainBackground;
                        break;

                    case RichTextBox richTextBox:
                        richTextBox.BackColor = CustomColors.MainBackground;
                        richTextBox.ForeColor = CustomColors.Text;
                        break;

                    case FlowLayoutPanel flowLayoutPanel:
                        flowLayoutPanel.BackColor = CustomColors.MainBackground;
                        CustomizeScrollBar(flowLayoutPanel);
                        break;

                    case Guna2Button guna2Button:
                        // Don't change the theme if the button is blue
                        if (IsButtonBlue(guna2Button))
                        {
                            continue;
                        }

                        guna2Button.FillColor = CustomColors.ControlBack;
                        guna2Button.ForeColor = CustomColors.Text;
                        guna2Button.BorderColor = CustomColors.ControlBorder;
                        guna2Button.DisabledState.FillColor = CustomColors.ControlDisabledBack;
                        guna2Button.HoverState.BorderColor = CustomColors.AccentBlue;
                        guna2Button.GotFocus += Guna2Button_GotFocus;
                        guna2Button.LostFocus += Guna2Button_LostFocus;
                        break;

                    case Guna2TextBox guna2TextBox:
                        guna2TextBox.FillColor = CustomColors.ControlBack;
                        guna2TextBox.HoverState.FillColor = CustomColors.ControlBack;
                        guna2TextBox.DisabledState.FillColor = CustomColors.ControlDisabledBack;
                        guna2TextBox.DisabledState.BorderColor = CustomColors.ControlBorder;
                        guna2TextBox.ForeColor = CustomColors.Text;
                        guna2TextBox.BorderColor = CustomColors.ControlBorder;
                        guna2TextBox.HoverState.BorderColor = CustomColors.AccentBlue;
                        guna2TextBox.FocusedState.BorderColor = CustomColors.AccentBlue;
                        guna2TextBox.FocusedState.FillColor = CustomColors.ControlBack;
                        break;

                    case Guna2CustomCheckBox guna2CustomCheckBox:
                        guna2CustomCheckBox.CheckedState.BorderColor = CustomColors.AccentBlue;
                        guna2CustomCheckBox.CheckedState.FillColor = CustomColors.AccentBlue;
                        guna2CustomCheckBox.UncheckedState.BorderColor = CustomColors.ControlUncheckedBorder;
                        guna2CustomCheckBox.UncheckedState.FillColor = CustomColors.ControlBack;
                        break;

                    case Guna2CustomRadioButton guna2CustomRadioButton:
                        guna2CustomRadioButton.CheckedState.BorderColor = CustomColors.AccentBlue;
                        guna2CustomRadioButton.CheckedState.FillColor = CustomColors.AccentBlue;
                        guna2CustomRadioButton.UncheckedState.BorderColor = CustomColors.ControlUncheckedBorder;
                        guna2CustomRadioButton.UncheckedState.FillColor = CustomColors.ControlBack;
                        break;

                    case Guna2Panel guna2Panel:
                        guna2Panel.FillColor = CustomColors.MainBackground;
                        break;

                    case Panel panel:
                        panel.BackColor = CustomColors.MainBackground;
                        break;

                    case Guna2TrackBar guna2TrackBar:
                        guna2TrackBar.ThumbColor = CustomColors.AccentBlue;
                        guna2TrackBar.BackColor = CustomColors.MainBackground;
                        break;

                    case Guna2NumericUpDown guna2NumericUpDown:
                        guna2NumericUpDown.FillColor = CustomColors.ControlBack;
                        guna2NumericUpDown.ForeColor = CustomColors.Text;
                        guna2NumericUpDown.BorderColor = CustomColors.ControlBorder;
                        guna2NumericUpDown.UpDownButtonFillColor = CustomColors.ControlBack;
                        guna2NumericUpDown.UpDownButtonForeColor = CustomColors.Text;
                        guna2NumericUpDown.UpDownButtonBorderVisible = true;
                        break;

                    case Guna2ComboBox guna2ComboBox:
                        guna2ComboBox.FillColor = CustomColors.ControlBack;
                        guna2ComboBox.ForeColor = CustomColors.Text;
                        guna2ComboBox.BorderColor = CustomColors.ControlBorder;
                        guna2ComboBox.HoverState.BorderColor = CustomColors.AccentBlue;
                        guna2ComboBox.FocusedState.BorderColor = CustomColors.AccentBlue;
                        guna2ComboBox.ItemsAppearance.BackColor = CustomColors.ControlBack;
                        break;

                    case Guna2DataGridView guna2DataGridView:
                        guna2DataGridView.Theme = CustomColors.DataGridViewTheme;
                        guna2DataGridView.BackgroundColor = CustomColors.ControlBack;

                        UpdateDataGridViewHeaderTheme(guna2DataGridView);
                        CustomizeScrollBar(guna2DataGridView);
                        guna2DataGridView.ClearSelection();
                        DataGridViewManager.UpdateAlternatingRowColors(guna2DataGridView);
                        break;

                    case Guna2CircleButton guna2CircleButton:
                        guna2CircleButton.FillColor = CustomColors.ControlBack;
                        guna2CircleButton.HoverState.FillColor = CustomColors.ControlBack;
                        guna2CircleButton.HoverState.BorderColor = CustomColors.ControlBorder;
                        break;

                    case Guna2ToggleSwitch guna2ToggleSwitch:
                        guna2ToggleSwitch.CheckedState.FillColor = CustomColors.AccentBlue;
                        guna2ToggleSwitch.UncheckedState.FillColor = CustomColors.ControlBack;
                        break;

                    case Guna2DateTimePicker guna2DateTimePicker:
                        guna2DateTimePicker.FillColor = CustomColors.ControlBack;
                        guna2DateTimePicker.ForeColor = CustomColors.Text;
                        guna2DateTimePicker.BorderColor = CustomColors.ControlBorder;
                        guna2DateTimePicker.HoverState.BorderColor = CustomColors.AccentBlue;
                        break;

                    case GunaChart gunaChart:
                        gunaChart.ApplyConfig(ChartColors.Config(), CustomColors.Background4);

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
                        gunaChart.Legend.LabelFont = new ChartFont("Segoe UI", 18);
                        gunaChart.Tooltips.TitleFont = new ChartFont("Segoe UI", 18, ChartFontStyle.Bold);
                        gunaChart.Tooltips.BodyFont = new ChartFont("Segoe UI", 18);
                        gunaChart.XAxes.Ticks.Font = new ChartFont("Segoe UI", 18);
                        gunaChart.YAxes.Ticks.Font = new ChartFont("Segoe UI", 18);
                        break;
                }

                // Recursively apply theming to child controls if any
                if (control.HasChildren)
                {
                    List<Control> childControls = [];
                    foreach (Control childControl in control.Controls)
                    {
                        childControls.Add(childControl);
                    }
                    SetThemeForControl(childControls);
                }
            }
        }
        public static void UpdateThemeForPanel(List<Guna2Panel> listOfMenus)
        {
            foreach (Guna2Panel guna2Panel in listOfMenus)
            {
                guna2Panel.FillColor = CustomColors.PanelBtn;
                guna2Panel.BorderColor = CustomColors.ControlPanelBorder;

                Control.ControlCollection list;
                FlowLayoutPanel flowPanel = guna2Panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                if (flowPanel != null)
                {
                    flowPanel.BackColor = CustomColors.PanelBtn;
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
                            guna2Separator.FillColor = CustomColors.ControlPanelBorder;
                            guna2Separator.BackColor = CustomColors.PanelBtn;
                            break;

                        case Guna2Button guna2Button:
                            guna2Button.FillColor = CustomColors.PanelBtn;
                            guna2Button.ForeColor = CustomColors.Text;
                            guna2Button.BorderColor = CustomColors.ControlBorder;
                            guna2Button.HoverState.BorderColor = CustomColors.ControlBorder;
                            guna2Button.HoverState.FillColor = CustomColors.PanelBtnHover;
                            guna2Button.PressedColor = CustomColors.PanelBtnHover;

                            foreach (Label label in guna2Button.Controls)
                            {
                                label.ForeColor = CustomColors.Text;
                                label.BackColor = CustomColors.PanelBtn;
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
                column.HeaderCell.Style.BackColor = CustomColors.Background2;
                column.HeaderCell.Style.SelectionBackColor = CustomColors.Background2;
            }
        }
        private static void Guna2Button_GotFocus(object sender, EventArgs e)
        {
            Guna2Button button = (Guna2Button)sender;
            button.BorderColor = CustomColors.AccentBlue;
        }
        private static void Guna2Button_LostFocus(object sender, EventArgs e)
        {
            Guna2Button button = (Guna2Button)sender;

            if (button.BorderThickness == 1 && !IsButtonBlue(button))
            {
                button.BorderColor = CustomColors.ControlBorder;
            }
        }
        public static void CustomizeScrollBar(Control control)
        {
            // Remove any existing Guna2VScrollBar
            List<Guna2VScrollBar> existingScrollBars = control.Controls.OfType<Guna2VScrollBar>().ToList();
            foreach (Guna2VScrollBar scrollBar in existingScrollBars)
            {
                control.Controls.Remove(scrollBar);
                scrollBar.Dispose();
            }

            // Add new scrollbar
            Guna2VScrollBar vScrollBar = new()
            {
                FillColor = CustomColors.MainBackground,
                ThumbColor = CustomColors.PanelBtnHover,
                BorderColor = CustomColors.ControlPanelBorder,
                ThumbSize = 40
            };
            vScrollBar.HoverState.ThumbColor = Color.Gray;
            control.Controls.Add(vScrollBar);
            vScrollBar.BringToFront();
            vScrollBar.BindingContainer = control;
        }
        public static void SetThemeForForm(Form form)
        {
            form.BackColor = CustomColors.MainBackground;

            List<Control> list = [];
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
                CurrentTheme = value == 0
                    ? ThemeType.Dark
                    : ThemeType.Light;
            }
        }

        // Set RightArrow image
        public static void SetRightArrowImageBasedOnTheme(Guna2Button button)
        {
            button.Image = CurrentTheme == ThemeType.Dark
                ? Resources.RightArrowWhite
                : Resources.RightArrowBlack;
        }

        // Make button blue
        public static void MakeGButtonBluePrimary(Guna2Button button)
        {
            button.BorderThickness = 0;
            button.FillColor = CustomColors.AccentBlue;
            button.ForeColor = Color.White;
            button.Font = new Font(button.Font, FontStyle.Bold);
        }
        public static void MakeGButtonBlueSecondary(Guna2Button button)
        {
            button.FillColor = Color.Transparent;
            button.BorderThickness = 2;
            button.BorderColor = CustomColors.AccentBlue;
            button.ForeColor = CustomColors.AccentBlue;
            button.Font = new Font(button.Font, FontStyle.Bold);
        }
        public static bool IsButtonBlue(Guna2Button button)
        {
            return button.FillColor == CustomColors.AccentBlue || button.ForeColor == CustomColors.AccentBlue;
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