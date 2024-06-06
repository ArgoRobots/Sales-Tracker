namespace Sales_Tracker.Classes
{
    class UserSettings
    {
        public static void ResetAllToDefault()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
        }
    }
}