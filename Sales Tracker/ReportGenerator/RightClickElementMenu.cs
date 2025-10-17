using Guna.UI2.WinForms;
using Sales_Tracker.ReportGenerator.Menus;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Manages the right-click context menu for report elements in the layout designer.
    /// </summary>
    internal static class RightClickElementMenu
    {
        // Properties
        public static Guna2Panel Panel { get; private set; }
        private static Guna2Button Delete_Button;
        private static Guna2Button Duplicate_Button;
        private static Guna2Button SelectAll_Button;
        private static Guna2Button BringToFront_Button;
        private static Guna2Button SendToBack_Button;

        // Constructor
        public static void ConstructRightClickElementMenu()
        {
            float scale = DpiHelper.GetRelativeDpiScale();
            int scaledButtonHeight = (int)(CustomControls.PanelButtonHeight * scale);
            int scaledSpaceForPanel = (int)(CustomControls.SpaceForPanel * scale);

            int calculatedHeight = 5 * scaledButtonHeight + scaledSpaceForPanel;

            Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth, calculatedHeight),
                "rightClickElement_Panel");

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)Panel.Controls[0];

            // Duplicate button
            Duplicate_Button = CustomControls.ConstructBtnForMenu("Duplicate", CustomControls.PanelBtnWidth, flowPanel);
            Duplicate_Button.Click += DuplicateElement;
            CustomControls.ConstructKeyShortcut("Ctrl+D", Duplicate_Button);

            // Select All button
            SelectAll_Button = CustomControls.ConstructBtnForMenu("Select all", CustomControls.PanelBtnWidth, flowPanel);
            SelectAll_Button.Click += SelectAllElements;
            CustomControls.ConstructKeyShortcut("Ctrl+A", SelectAll_Button);

            // Bring to Front button
            BringToFront_Button = CustomControls.ConstructBtnForMenu("Bring to front", CustomControls.PanelBtnWidth, flowPanel);
            BringToFront_Button.Click += BringToFront;

            // Send to Back button
            SendToBack_Button = CustomControls.ConstructBtnForMenu("Send to back", CustomControls.PanelBtnWidth, flowPanel);
            SendToBack_Button.Click += SendToBack;

            // Delete button
            Delete_Button = CustomControls.ConstructBtnForMenu("Delete", CustomControls.PanelBtnWidth, flowPanel);
            Delete_Button.ForeColor = CustomColors.AccentRed;
            Delete_Button.Click += DeleteElement;
            CustomControls.ConstructKeyShortcut("Del", Delete_Button);
        }

        // Event handlers
        private static void DuplicateElement(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance.DuplicateSelectedWithUndo();
            Hide();
        }
        private static void DeleteElement(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance.DeleteSelectedWithUndo();
            Hide();
        }
        private static void SelectAllElements(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance.SelectAllElements();
            Hide();
        }
        private static void BringToFront(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance.BringElementToFront();
            Hide();
        }
        private static void SendToBack(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance.SendElementToBack();
            Hide();
        }

        // Public methods
        /// <summary>
        /// Shows the right-click menu at the specified location.
        /// </summary>
        public static void Show(Point location, Form parentForm, bool hasSelection)
        {
            if (Panel == null)
            {
                ConstructRightClickElementMenu();
            }

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)Panel.Controls[0];
            flowPanel.Controls.Clear();

            // Add buttons based on selection state
            if (hasSelection)
            {
                flowPanel.Controls.Add(SelectAll_Button);
                flowPanel.Controls.Add(Duplicate_Button);
                flowPanel.Controls.Add(BringToFront_Button);
                flowPanel.Controls.Add(SendToBack_Button);
                flowPanel.Controls.Add(Delete_Button);
            }
            else
            {
                flowPanel.Controls.Add(SelectAll_Button);
            }

            CustomControls.SetRightClickMenuHeight(Panel);

            // Adjust position to keep menu within form boundaries
            Point adjustedLocation = location;

            // Check right boundary
            if (adjustedLocation.X + Panel.Width > parentForm.ClientSize.Width)
            {
                adjustedLocation.X = parentForm.ClientSize.Width - Panel.Width;
            }

            // Check bottom boundary
            if (adjustedLocation.Y + Panel.Height > parentForm.ClientSize.Height)
            {
                adjustedLocation.Y = parentForm.ClientSize.Height - Panel.Height;
            }

            // Ensure menu doesn't go off left or top edges
            if (adjustedLocation.X < 0)
            {
                adjustedLocation.X = 0;
            }

            if (adjustedLocation.Y < 0)
            {
                adjustedLocation.Y = 0;
            }

            // Position the menu
            Panel.Location = adjustedLocation;

            // Add to parent form
            parentForm.Controls.Add(Panel);
            Panel.BringToFront();
        }

        /// <summary>
        /// Hides the right-click menu.
        /// </summary>
        public static void Hide()
        {
            Panel?.Parent?.Controls.Remove(Panel);
        }
    }
}