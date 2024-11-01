using Guna.UI2.WinForms;
using System.Collections;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages keyboard shortcuts and navigation for menu controls.
    /// </summary>
    public static class MenuKeyShortcutManager
    {
        // Properties
        private static Guna2Panel _selectedPanel;
        private static Guna2Button buttonThatOpenedCascadingMenu;

        // Getter and setter
        public static Guna2Panel SelectedPanel
        {
            get => _selectedPanel;
            set => _selectedPanel = value;
        }

        // Main methods
        /// <summary>
        /// Manages navigation between menu buttons using arrow keys, tab, and enter.
        /// </summary>
        /// <remarks>
        /// Supports the following keyboard actions:
        /// - Down/Tab: Moves selection to next button
        /// - Up: Moves selection to previous button
        /// - Right: Opens cascading menu if available
        /// - Enter: Triggers the selected button's click event
        /// 
        /// - When reaching the end of the menu, selection wraps around to the beginning and vice versa.
        /// </remarks>
        public static void HandlePanelkeyDown(Guna2Panel panel, Keys e)
        {
            Guna2Panel activePanel = _selectedPanel ?? panel;

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)activePanel.Controls[0];
            IList results = flowPanel.Controls;

            if (results.Count == 0) { return; }

            (Guna2Button button, int buttonIndex) = GetSelectedButtonInMenu(results);

            if (button == null)
            {
                SelectFirstButton(results);
                return;
            }

            switch (e)
            {
                case Keys.Up:
                    HandleKeyUp(results, button, buttonIndex);
                    break;
                case Keys.Down:
                case Keys.Tab:
                    HandleKeyDown(results, button, buttonIndex);
                    break;
                case Keys.Left:
                    HandleKeyLeft(button, panel);
                    break;
                case Keys.Right:
                    HandleKeyRight(button);
                    break;
                case Keys.Enter:
                    button.PerformClick();
                    break;
            }
        }
        private static void HandleKeyUp(IList results, Guna2Button button, int buttonIndex)
        {
            UnselectMenuButton(button);

            // If it's not the first button
            if (buttonIndex > 0)
            {
                for (int j = buttonIndex - 1; j >= 0; j--)
                {
                    if (results[j] is Guna2Button prevBtn)
                    {
                        SelectMenuButton(prevBtn);
                        break;
                    }
                }
            }
            else
            {
                // Select the last button
                for (int j = results.Count - 1; j >= 0; j--)
                {
                    if (results[j] is Guna2Button lastBtn)
                    {
                        SelectMenuButton(lastBtn);
                        break;
                    }
                }
            }
        }
        private static void HandleKeyDown(IList results, Guna2Button button, int buttonIndex)
        {
            UnselectMenuButton(button);

            // If it's not the last button
            if (buttonIndex < results.Count - 1)
            {
                // Find the next button
                for (int j = buttonIndex + 1; j < results.Count; j++)
                {
                    if (results[j] is Guna2Button nextBtn)
                    {
                        SelectMenuButton(nextBtn);
                        break;
                    }
                }
            }
            else
            {
                SelectFirstButton(results);
            }
        }
        private static void HandleKeyLeft(Guna2Button button, Guna2Panel panel)
        {
            if (_selectedPanel != null)
            {
                _selectedPanel = null;
                UnselectMenuButton(button);

                FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
                IList results = flowPanel.Controls;

                if (results.Count == 0) { return; }

                SelectMenuButton(buttonThatOpenedCascadingMenu);
                buttonThatOpenedCascadingMenu = null;
                CascadingMenu.RemoveCascadingMenus();
            }
        }
        private static void HandleKeyRight(Guna2Button button)
        {
            if (button.Tag is Guna2Panel panel)
            {
                buttonThatOpenedCascadingMenu = button;
                _selectedPanel = panel;
                UnselectMenuButton(button);

                FlowLayoutPanel flowPanel = (FlowLayoutPanel)panel.Controls[0];
                IList results = flowPanel.Controls;

                if (results.Count == 0) { return; }

                SelectFirstButton(results);
            }
        }

        // Helper methods
        private static (Guna2Button?, int) GetSelectedButtonInMenu(IList results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i] is Guna2Button btn && IsMenuButtonSelected(btn))
                {
                    return (btn, i);
                }
            }
            return (null, 0);
        }
        private static void SelectMenuButton(Guna2Button button)
        {
            button.BorderThickness = 1;

            if (button.Tag is Guna2Panel)
            {
                button.PerformClick();  // Open the cascading menu
            }
        }
        private static void SelectFirstButton(IList results)
        {
            if (results[0] is Guna2Button firstBtn)
            {
                SelectMenuButton(firstBtn);
            }
        }
        private static void UnselectMenuButton(Guna2Button button)
        {
            button.BorderThickness = 0;

            if (!CascadingMenu.IsThisACascadingMenu(_selectedPanel))
            {
                CascadingMenu.RemoveCascadingMenus();
            }
        }
        private static bool IsMenuButtonSelected(Guna2Button button)
        {
            return button.BorderThickness == 1;
        }
    }
}