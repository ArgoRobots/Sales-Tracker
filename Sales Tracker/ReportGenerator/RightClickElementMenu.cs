using Guna.UI2.WinForms;
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
        public static Guna2Panel RightClickElement_Panel { get; private set; }
        public static Guna2Button RightClickElement_DeleteBtn { get; private set; }
        public static Guna2Button RightClickElement_DuplicateBtn { get; private set; }
        public static Guna2Button RightClickElement_SelectAllBtn { get; private set; }

        // Constructor
        public static void ConstructRightClickElementMenu()
        {
            float scale = DpiHelper.GetRelativeDpiScale();
            int scaledButtonHeight = (int)(CustomControls.PanelButtonHeight * scale);
            int scaledSpaceForPanel = (int)(CustomControls.SpaceForPanel * scale);

            int calculatedHeight = 3 * scaledButtonHeight + scaledSpaceForPanel;

            RightClickElement_Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth, calculatedHeight),
                "rightClickElement_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)RightClickElement_Panel.Controls[0];

            // Duplicate button
            RightClickElement_DuplicateBtn = CustomControls.ConstructBtnForMenu(
                "Duplicate",
                CustomControls.PanelBtnWidth,
                true,
                flowPanel
            );
            RightClickElement_DuplicateBtn.Click += DuplicateElement;
            CustomControls.ConstructKeyShortcut("Ctrl+D", RightClickElement_DuplicateBtn);

            // Select All button
            RightClickElement_SelectAllBtn = CustomControls.ConstructBtnForMenu(
                "Select all",
                CustomControls.PanelBtnWidth,
                true,
                flowPanel
            );
            RightClickElement_SelectAllBtn.Click += SelectAllElements;
            CustomControls.ConstructKeyShortcut("Ctrl+A", RightClickElement_SelectAllBtn);

            // Delete button
            RightClickElement_DeleteBtn = CustomControls.ConstructBtnForMenu(
                "Delete",
                CustomControls.PanelBtnWidth,
                true,
                flowPanel
            );
            RightClickElement_DeleteBtn.ForeColor = CustomColors.AccentRed;
            RightClickElement_DeleteBtn.Click += DeleteElement;
            CustomControls.ConstructKeyShortcut("Del", RightClickElement_DeleteBtn);
        }

        // Event handlers
        private static void DuplicateElement(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance?.DuplicateSelected();
            HideMenu();
        }
        private static void DeleteElement(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance?.DeleteSelected();
            HideMenu();
        }
        private static void SelectAllElements(object sender, EventArgs e)
        {
            ReportLayoutDesigner_Form.Instance?.SelectAllElements();
            HideMenu();
        }

        /// <summary>
        /// Shows the right-click menu at the specified location.
        /// </summary>
        public static void ShowMenu(Point location, Form parentForm, bool hasSelection)
        {
            if (RightClickElement_Panel == null)
            {
                ConstructRightClickElementMenu();
            }

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)RightClickElement_Panel.Controls[0];
            flowPanel.Controls.Clear();

            // Add buttons based on selection state
            if (hasSelection)
            {
                flowPanel.Controls.Add(RightClickElement_DuplicateBtn);
                flowPanel.Controls.Add(RightClickElement_DeleteBtn);
                flowPanel.Controls.Add(RightClickElement_SelectAllBtn);
            }
            else
            {
                flowPanel.Controls.Add(RightClickElement_SelectAllBtn);
            }

            CustomControls.SetRightClickMenuHeight(RightClickElement_Panel);

            // Adjust position to keep menu within form boundaries
            Point adjustedLocation = location;

            // Check right boundary
            if (adjustedLocation.X + RightClickElement_Panel.Width > parentForm.ClientSize.Width)
            {
                adjustedLocation.X = parentForm.ClientSize.Width - RightClickElement_Panel.Width;
            }

            // Check bottom boundary
            if (adjustedLocation.Y + RightClickElement_Panel.Height > parentForm.ClientSize.Height)
            {
                adjustedLocation.Y = parentForm.ClientSize.Height - RightClickElement_Panel.Height;
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
            RightClickElement_Panel.Location = adjustedLocation;

            // Add to parent form
            parentForm.Controls.Add(RightClickElement_Panel);
            RightClickElement_Panel.BringToFront();
        }

        /// <summary>
        /// Hides the right-click menu.
        /// </summary>
        public static void HideMenu()
        {
            RightClickElement_Panel?.Parent?.Controls.Remove(RightClickElement_Panel);
        }
    }
}