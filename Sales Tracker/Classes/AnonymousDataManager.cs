using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    public enum DataPointType
    {
        Export,
        OpenAI,
        OpenExchangeRates,
        GoogleSheets,
        Language
    }
    public enum ExportDataField
    {
        ExportType,
        DurationMS,
        FileSize,
    }
    public enum ExportType
    {
        ExcelSheetsChart,
        GoogleSheetsChart,
        Backup,
        XLSX,
        Receipts
    }
    public enum LanguageDataField
    {
        TargetLanguage,
        DurationMS,
        CharactersTranslated,
        ControlsTranslated,
        CacheHitPercentage
    }

    /// <summary>
    /// Manages collection and storage of anonymous data points for analytics purposes.
    /// </summary>
    public static class AnonymousDataManager
    {
        /// <summary>
        /// Adds export operation data to the anonymous data log
        /// </summary>
        /// <param name="data">Dictionary containing export operation metadata</param>
        public static void AddExportData(Dictionary<ExportDataField, object> data)
        {
            ExportType exportType = (ExportType)data[ExportDataField.ExportType];
            long durationMs = (long)data[ExportDataField.DurationMS];

            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.Export.ToString(),
                ["ExportType"] = exportType.ToString(),
                ["DurationMS"] = Tools.FormatDuration(durationMs),
                ["FileSize"] = data[ExportDataField.FileSize]
            };

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds OpenAI API usage data to the anonymous data log
        /// </summary>
        /// <param name="model">The OpenAI model used</param>
        /// <param name="durationMs">Duration of the API call in milliseconds</param>
        /// <param name="tokensUsed">Number of tokens consumed in the operation</param>
        public static void AddOpenAIUsageData(string model, long durationMs, int tokensUsed)
        {
            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.OpenAI.ToString(),
                ["Model"] = model,
                ["DurationMS"] = durationMs,
                ["TokensUsed"] = tokensUsed
            };

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds Open Exchange Rates API usage data to the anonymous data log
        /// </summary>
        /// <param name="durationMs">Duration of the API call in milliseconds</param>
        public static void AddOpenExchangeRatesData(long durationMs)
        {
            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.OpenExchangeRates.ToString(),
                ["DurationMS"] = durationMs
            };

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds Google Sheets API usage data to the anonymous data log
        /// </summary>
        /// <param name="durationMs">Duration of the API call in milliseconds</param>
        /// <param name="operation">Type of operation performed (optional)</param>
        public static void AddGoogleSheetsData(long durationMs, string operation = null)
        {
            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.GoogleSheets.ToString(),
                ["DurationMS"] = durationMs
            };

            if (!string.IsNullOrEmpty(operation))
            {
                dataPoint["Operation"] = operation;
            }

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds language translation data to the anonymous data log
        /// </summary>
        /// <param name="data">Dictionary containing language translation metadata</param>
        public static void AddMicrosoftTranslatorData(Dictionary<LanguageDataField, object> data)
        {
            string targetLanguage = (string)data[LanguageDataField.TargetLanguage];
            long durationMs = (long)data[LanguageDataField.DurationMS];

            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.Language.ToString(),
                ["TargetLanguage"] = targetLanguage,
                ["DurationMS"] = Tools.FormatDuration(durationMs)
            };

            // Add optional fields if they exist
            if (data.TryGetValue(LanguageDataField.CharactersTranslated, out object? charsValue))
            {
                dataPoint["CharactersTranslated"] = charsValue;
            }

            if (data.TryGetValue(LanguageDataField.ControlsTranslated, out object? controlsValue))
            {
                dataPoint["ControlsTranslated"] = controlsValue;
            }

            if (data.TryGetValue(LanguageDataField.CacheHitPercentage, out object? cacheHitValue))
            {
                dataPoint["CacheHitPercentage"] = cacheHitValue;
            }

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Appends a data point to the anonymous data cache file
        /// </summary>
        private static void AppendToDataFile(Dictionary<string, object> dataPoint)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(Directories.AnonymousUserDataCache_file);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Add the data point to file
                string jsonLine = JsonConvert.SerializeObject(dataPoint, Formatting.None) + Environment.NewLine;
                File.AppendAllText(Directories.AnonymousUserDataCache_file, jsonLine);
            }
            catch (Exception ex)
            {
                Log.Error_AnonymousDataCollection($"Failed to log anonymous data: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the cached anonymous data in an organized format by data type
        /// </summary>
        /// <returns>Status message or the exported JSON if no file path provided</returns>
        public static void ExportOrganizedData(string outputFilePath)
        {
            if (string.IsNullOrEmpty(outputFilePath)) { return; }

            // Read all data points
            string[] lines = File.ReadAllLines(Directories.AnonymousUserDataCache_file);
            List<JObject> allDataPoints = [];

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        JObject dataPoint = JObject.Parse(line);
                        allDataPoints.Add(dataPoint);
                    }
                    catch { /* Skip invalid JSON lines */ }
                }
            }

            // Group data by type
            JObject organizedData = new()
            {
                ["exportTime"] = Tools.FormatDateTime(DateTime.Now),
                ["dataPoints"] = new JObject
                {
                    ["Export"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Export.ToString())),
                    ["OpenAI"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.OpenAI.ToString())),
                    ["OpenExchangeRates"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.OpenExchangeRates.ToString())),
                    ["GoogleSheets"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.GoogleSheets.ToString())),
                    ["Language"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Language.ToString()))
                }
            };

            // Write organized data to file or return as string
            string organizedJson = JsonConvert.SerializeObject(organizedData, Formatting.Indented);
            outputFilePath = Directories.GetNewFileNameIfItAlreadyExists(outputFilePath);
            File.WriteAllText(outputFilePath, organizedJson);
        }

        /// <summary>
        /// Clears the anonymous data cache file
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static void ClearDataCache()
        {
            Directories.DeleteFile(Directories.AnonymousUserDataCache_file);
            LanguageManager.TranslationCache = [];
            LanguageManager.EnglishCache = [];
        }

        /// <summary>
        /// Gets the size of the anonymous data cache file in bytes
        /// </summary>
        /// <returns>Size in bytes, or 0 if file doesn't exist</returns>
        public static long GetDataCacheSize()
        {
            if (File.Exists(Directories.AnonymousUserDataCache_file))
            {
                FileInfo fileInfo = new(Directories.AnonymousUserDataCache_file);
                return fileInfo.Length;
            }
            return 0;
        }
    }
}