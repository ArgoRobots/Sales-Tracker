using Guna.UI2.WinForms;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// This prevents the screen from flashing white while the Form is loading.
    /// This is especially useful when dark theme is enabled and there are a lot of things to load.
    /// </summary>
    public class LoadingPanel
    {
        private static Panel blankLoadingPanel;
        public static Panel BlankLoadingPanelInstance
        {
            get { return blankLoadingPanel; }
        }
        public static void InitBlankLoadingPanel()
        {
            blankLoadingPanel = new Panel
            {
                BackColor = CustomColors.mainBackground
            };
        }
        public static void ShowBlankLoadingPanel(Control control)
        {
            if (blankLoadingPanel.InvokeRequired)
            {
                blankLoadingPanel.Invoke(new Action(show));
            }
            else
            {
                show();
            }

            void show()
            {
                blankLoadingPanel.Size = control.Size;
                control.Controls.Add(blankLoadingPanel);
                blankLoadingPanel.BringToFront();
            }
        }
        public static void HideBlankLoadingPanel(Control control)
        {
            control.Controls.Remove(blankLoadingPanel);
        }

        private static Panel loadingPanel;
        public static Panel LoadingPanelInstance
        {
            get { return blankLoadingPanel; }
        }
        public static void InitLoadingPanel()
        {
            if (loadingPanel != null) { return; }

            loadingPanel = new Panel
            {
                BackColor = CustomColors.mainBackground
            };
            Guna2WinProgressIndicator progressIndicator = new()
            {
                AutoStart = true,
                ProgressColor = CustomColors.accent_blue,
            };

            loadingPanel.Controls.Add(progressIndicator);
        }
        public static void ShowLoadingScreen(Control control)
        {
            loadingPanel.Size = control.Size;
            control.Controls.Add(loadingPanel);

            Guna2WinProgressIndicator progressIndicator = (Guna2WinProgressIndicator)loadingPanel.Controls[0];
            progressIndicator.Location = new Point((loadingPanel.Width - progressIndicator.Width) / 2, (loadingPanel.Height - progressIndicator.Height) / 2);

            loadingPanel.BringToFront();
        }
        public static void HideLoadingScreen(Control control)
        {
            control.Controls.Remove(loadingPanel);
        }
    }
}