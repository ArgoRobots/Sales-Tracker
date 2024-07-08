namespace Sales_Tracker.Classes
{    /// <summary>
     /// Manages file operations for application settings, supporting read, write, and append functionalities.
     /// </summary>
    public static class DataFileManager
    {
        // Enums for categorizing settings
        public enum AppDataSettings
        {
            RPTutorial,
            RecentProjects
        }
        public enum BuildMachinesSettings
        {
            MachineController,
        }

        // Dictionary to hold settings loaded from files, with file path as key.
        private static readonly Dictionary<string, Dictionary<string, string>> fileSettings = [];

        public static byte MaxValueForRecentProjects { get; set; } = 10;

        /// <summary>
        /// Ensures the settings for a given file path are loaded into memory.
        /// </summary>
        /// <returns>A dictionary containing the settings for the specified file.</returns>
        private static Dictionary<string, string> EnsureSettingsLoaded(string filePath)
        {
            if (!fileSettings.TryGetValue(filePath, out Dictionary<string, string>? settings))
            {
                settings = ReadDataFile(filePath) ?? [];
                fileSettings[filePath] = settings;
            }
            return settings;
        }

        /// <summary>
        /// Reads settings from a specified file and returns them as a dictionary.
        /// </summary>
        /// <returns>A dictionary with the settings read from the file.</returns>
        private static Dictionary<string, string> ReadDataFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            return Directories.ReadAllLinesInFile(filePath)
                        .Select(line => line.Split([':'], 2))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets a value for a given key in the settings file.
        /// </summary>
        public static void SetValue<TEnum>(string filePath, TEnum key, string value) where TEnum : Enum
        {
            Dictionary<string, string> settings = EnsureSettingsLoaded(filePath);
            string keyString = Enum.GetName(typeof(TEnum), key);
            settings[keyString] = value;
            Save(filePath);
        }

        /// <summary>
        /// Appends a value to a setting, optionally maintaining a maximum number of values.
        /// </summary>
        public static void AppendValue<TEnum>(string filePath, TEnum key, string appendValue, int maxValue) where TEnum : Enum
        {
            Dictionary<string, string> settings = EnsureSettingsLoaded(filePath);
            string keyString = Enum.GetName(typeof(TEnum), key);

            // Attempt to get the current value
            string value = settings.GetValueOrDefault(keyString, "");

            // Split into list
            List<string> valuesList = value.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();

            // Append new value
            valuesList.Add(appendValue);

            // Ensure the total count does not exceed maxValue
            if (valuesList.Count > maxValue)
            {
                valuesList.RemoveAt(0);
            }
            settings[keyString] = string.Join(",", valuesList);

            Save(filePath);
        }

        /// <summary>
        /// Generic method to get values for any enum type key.
        /// </summary>
        public static string GetValue<TEnum>(string filePath, TEnum key) where TEnum : Enum
        {
            string keyString = Enum.GetName(typeof(TEnum), key);
            EnsureSettingsLoaded(filePath);

            if (keyString != null && fileSettings[filePath].TryGetValue(keyString, out string value))
            {
                return value;
            }

            return null;
        }

        public static void Save(string filePath)
        {
            if (!fileSettings.TryGetValue(filePath, out Dictionary<string, string>? settings))
            {
                return; // No changes to save for this file
            }

            string[] lines = settings.Select(kv => $"{kv.Key}:{kv.Value}").ToArray();
            Directories.WriteLinesToFile(filePath, lines);
        }
    }
}