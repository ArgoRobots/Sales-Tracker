namespace Sales_Tracker.Classes
{
    class UserSettings
    {
        public static void SaveUserSettings()
        {
            Properties.Settings.Default.ColorTheme = "";
            Properties.Settings.Default.Save();
        }
    }
}