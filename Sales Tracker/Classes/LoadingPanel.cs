namespace Sales_Tracker.Classes
{
    /// <summary>
    /// This prevents the screen from flashing white while the Form is loading.
    /// This is especially useful when dark theme is enabled and there are a lot of things to load.
    /// </summary>
    internal class LoadingPanel
    {
        private static Panel loadingPanel;

        public static void InitLoadingPanel()
        {
            loadingPanel = new Panel
            {
                BackColor = CustomColors.mainBackground
            };
        }
        public static void ShowLoadingPanel(Control controls)
        {
            loadingPanel.Size = controls.ClientSize;
            controls.Controls.Add(loadingPanel);
            loadingPanel.BringToFront();
        }
        public static void HideLoadingPanel(Control controls)
        {
            controls.Controls.Remove(loadingPanel);
        }
    }
}