﻿namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages file operations for application settings, supporting read, write, and append functionalities.
    /// </summary>
    public static class DataFileManager
    {
        // Enums for categorizing settings
        public enum GlobalAppDataSettings
        {
            ImportSpreadsheetTutorial,  // bool
            RecentProjects  // string[]
        }

        public enum AppDataSettings
        {
            ChangesMade,  // bool
            DefaultCurrencyType  // string
        }

        // Dictionary to hold settings loaded from files, with file path as key.
        private static readonly Dictionary<string, Dictionary<string, string>> data = new();

        /// <summary>
        /// Determines the file path based on the enum type.
        /// </summary>
        private static string GetFilePath<TEnum>(string? filePath = null) where TEnum : Enum
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            return typeof(TEnum) switch
            {
                _ when typeof(TEnum) == typeof(GlobalAppDataSettings) => Directories.GlobalAppDataSettings_file,
                _ when typeof(TEnum) == typeof(AppDataSettings) => Directories.AppDataSettings_file,
                _ => throw new ArgumentException("Unsupported enum type", nameof(TEnum))
            };
        }

        /// <summary>
        /// Ensures the values for a given file path are loaded into memory.
        /// </summary>
        /// <returns>A dictionary containing the values for the specified file.</returns>
        private static Dictionary<string, string> EnsureSettingsLoaded(string filePath)
        {
            if (!data.TryGetValue(filePath, out Dictionary<string, string>? values))
            {
                values = ReadDataFile(filePath) ?? new Dictionary<string, string>();
                data[filePath] = values;
            }
            return values;
        }

        /// <summary>
        /// Reads settings from a specified file and returns them as a dictionary.
        /// </summary>
        /// <returns>A dictionary with the settings read from the file.</returns>
        private static Dictionary<string, string>? ReadDataFile(string filePath)
        {
            if (!File.Exists(filePath)) { return null; }

            return Directories.ReadAllLinesInFile(filePath)
                        .Select(line => line.Split([':'], 2))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets a value for a given key in the settings file.
        /// </summary>
        /// <param name="filePath">Optional file path. If not provided, it will use the default based on the enum type.</param>
        public static void SetValue<TEnum>(TEnum key, string value, string? filePath = null) where TEnum : Enum
        {
            string finalFilePath = GetFilePath<TEnum>(filePath);
            Dictionary<string, string> values = EnsureSettingsLoaded(finalFilePath);
            string? keyString = Enum.GetName(typeof(TEnum), key);

            if (keyString != null)
            {
                values[keyString] = value;
                Save(finalFilePath);
            }
            else
            {
                throw new ArgumentException("Invalid enum key", nameof(key));
            }
        }

        /// <summary>
        /// Appends a value to a setting. If the value already exists, it is moved to the front of the list.
        /// </summary>
        /// <param name="filePath">Optional file path. If not provided, it will use the default based on the enum type.</param>
        public static void AppendValue<TEnum>(TEnum key, string appendValue, string? filePath = null) where TEnum : Enum
        {
            string finalFilePath = GetFilePath<TEnum>(filePath);
            Dictionary<string, string> settings = EnsureSettingsLoaded(finalFilePath);
            string? keyString = Enum.GetName(typeof(TEnum), key) ?? throw new ArgumentException("Invalid enum key", nameof(key));
            byte maxValue = GetMaxValueForSetting(key);

            // Attempt to get the current value
            if (!settings.TryGetValue(keyString, out string? value))
            {
                value = "";
            }

            // Split into list
            List<string> valuesList = value.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();

            // Remove all instances of the value
            valuesList.RemoveAll(val => val == appendValue);

            // Add the value to the end
            valuesList.Add(appendValue);

            // Ensure the total count does not exceed maxValue
            if (valuesList.Count > maxValue)
            {
                valuesList.RemoveAt(0);
            }

            settings[keyString] = string.Join(",", valuesList);

            Save(finalFilePath);
        }

        /// <summary>
        /// Gets the maximum value for a given setting key.
        /// </summary>
        private static byte GetMaxValueForSetting<TEnum>(TEnum key) where TEnum : Enum
        {
            return key switch
            {
                GlobalAppDataSettings.RecentProjects => 10,
                _ => throw new ArgumentException("Unsupported setting key", nameof(key)),
            };
        }

        /// <summary>
        /// Generic method to get values for any enum type key.
        /// </summary>
        /// <param name="filePath">Optional file path. If not provided, it will use the default based on the enum type.</param>
        public static string? GetValue<TEnum>(TEnum key, string? filePath = null) where TEnum : Enum
        {
            string finalFilePath = GetFilePath<TEnum>(filePath);
            string? keyString = Enum.GetName(typeof(TEnum), key);
            EnsureSettingsLoaded(finalFilePath);

            if (keyString != null && data[finalFilePath].TryGetValue(keyString, out string? value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Saves the current settings to the specified file if any changes were made.
        /// </summary>
        /// <param name="filePath">The file path where the settings will be saved.</param>
        private static void Save(string filePath)
        {
            if (!data.TryGetValue(filePath, out Dictionary<string, string>? values))
            {
                return;  // No changes to save for this file
            }

            string[] lines = values.Select(kv => $"{kv.Key}:{kv.Value}").ToArray();
            Directories.WriteLinesToFile(filePath, lines);
        }
    }
}