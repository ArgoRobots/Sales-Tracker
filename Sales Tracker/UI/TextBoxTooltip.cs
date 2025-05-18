using Guna.UI2.WinForms;
using Sales_Tracker.Theme;

namespace Sales_Tracker.UI
{
    public static class TextBoxTooltip
    {
        private static readonly Dictionary<Control, Guna2HtmlToolTip> tooltips = [];
        private static readonly Dictionary<Control, bool> tooltipStates = [];

        public static void SetOverflowTooltip(Guna2TextBox textBox)
        {
            if (!tooltips.TryGetValue(textBox, out Guna2HtmlToolTip tooltip))
            {
                tooltip = CreateTooltip();
                tooltips[textBox] = tooltip;
                tooltipStates[textBox] = false;

                textBox.MouseHover += TextBox_MouseHover;
                textBox.MouseLeave += HandleMouseLeave;
                textBox.TextChanged += UpdateTooltip;
            }
        }
        private static Guna2HtmlToolTip CreateTooltip()
        {
            return new Guna2HtmlToolTip
            {
                TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                TitleForeColor = CustomColors.Text,
                BackColor = CustomColors.ControlBack,
                ForeColor = CustomColors.Text,
                BorderColor = CustomColors.ControlPanelBorder,
                Font = new Font("Segoe UI", 10),
                InitialDelay = 500,
                ReshowDelay = 100,
            };
        }
        private static void TextBox_MouseHover(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (IsTextOverflowing(textBox) && !tooltipStates[textBox])
            {
                tooltips[textBox].Show(textBox.Text, textBox);
                tooltipStates[textBox] = true;
            }
        }
        private static void HandleMouseLeave(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            tooltips[textBox].Hide(textBox);
            tooltipStates[textBox] = false;
        }
        private static void UpdateTooltip(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (IsTextOverflowing(textBox))
            {
                tooltips[textBox].SetToolTip(textBox, textBox.Text);
            }
            else
            {
                tooltips[textBox].Hide(textBox);
                tooltipStates[textBox] = false;
            }
        }
        private static bool IsTextOverflowing(Guna2TextBox textBox)
        {
            using Graphics g = textBox.CreateGraphics();
            float textWidth = g.MeasureString(textBox.Text, textBox.Font).Width;
            int availableWidth = textBox.ClientSize.Width - textBox.Padding.Horizontal - 10;
            return textWidth > availableWidth;
        }
    }
}