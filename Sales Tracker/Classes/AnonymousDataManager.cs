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
        GoogleSheets
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

    /// <summary>
    /// Manages collection and storage of anonymous data points for analytics purposes.
    /// </summary>
    public static class AnonymousDataManager
    {
        /// <summary>
        /// Adds export operation data to the anonymous data log.
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
        /// Adds OpenAI API usage data to the anonymous data log.
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
        /// Adds Open Exchange Rates API usage data to the anonymous data log.
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
        /// Appends a data point to the anonymous data cache file.
        /// </summary>
        private static void AppendToDataFile(Dictionary<string, object> dataPoint)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(Directories.AnonymousUserData_file);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Add the data point to file
                string jsonLine = JsonConvert.SerializeObject(dataPoint, Formatting.None) + Environment.NewLine;
                File.AppendAllText(Directories.AnonymousUserData_file, jsonLine);
            }
            catch (Exception ex)
            {
                Log.Error_AnonymousDataCollection($"Failed to log anonymous data: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the cached anonymous data in an organized format by data type.
        /// </summary>
        /// <returns>Status message or the exported JSON if no file path provided</returns>
        public static void ExportOrganizedData(string outputFilePath)
        {
            if (string.IsNullOrEmpty(outputFilePath)) { return; }

            // Read all data points
            string[] lines = File.ReadAllLines(Directories.AnonymousUserData_file);
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
                    ["GoogleSheets"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.GoogleSheets.ToString()))
                }
            };

            // Write organized data to file or return as string
            string organizedJson = JsonConvert.SerializeObject(organizedData, Formatting.Indented);
            outputFilePath = Directories.GetNewFileNameIfItAlreadyExists(outputFilePath);
            File.WriteAllText(outputFilePath, organizedJson);
        }

        /// <summary>
        /// Clears the anonymous user data file.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static void ClearUserData()
        {
            Directories.DeleteFile(Directories.AnonymousUserData_file);
            LanguageManager.TranslationCache = [];
        }

        /// <summary>
        /// Gets the size of the anonymous data cache file in bytes.
        /// </summary>
        /// <returns>Size in bytes, or 0 if the file doesn't exist</returns>
        public static long GetUserDataCacheSize()
        {
            if (File.Exists(Directories.AnonymousUserData_file))
            {
                FileInfo fileInfo = new(Directories.AnonymousUserData_file);
                return fileInfo.Length;
            }
            return 0;
        }

        // Upload anonymous data
        private static async Task UploadAnonymousDataAsync()
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "organized_anonymous_data.json");
                ExportOrganizedData(tempFile);

                using HttpClient client = new();
                MultipartFormDataContent form = new()
                {
                    { new StreamContent(File.OpenRead(tempFile)), "file", "anonymous_data.json" }
                };

                string serverUrl = "https://argorobots.com/upload_data.php";
                HttpResponseMessage response = await client.PostAsync(serverUrl, form);
                string result = await response.Content.ReadAsStringAsync();
                Log.Write(1, "Uploaded anonymous data: " + result);
            }
            catch (Exception ex)
            {
                Log.Error_AnonymousDataCollection($"Upload failed: {ex.Message}");
            }
        }
        public static async Task TryUploadDataOnStartupAsync()
        {
            DateTime? lastUpload = GetLastUploadTime();
            if (lastUpload == null || (DateTime.Now - lastUpload.Value).TotalHours >= 24)
            {
                await UploadAnonymousDataAsync();
                SaveUploadTime(DateTime.Now);
            }
            else
            {
                Log.Write(1, $"Skipped anonymous data upload; last sent {lastUpload.Value}");
            }
        }
        private static DateTime? GetLastUploadTime()
        {
            string? val = DataFileManager.GetValue(GlobalAppDataSettings.UploadTime_AnonymousData);
            return DateTime.TryParse(val, out DateTime dt) ? dt : null;
        }
        private static void SaveUploadTime(DateTime time)
        {
            DataFileManager.SetValue(GlobalAppDataSettings.UploadTime_AnonymousData, time.ToString("o"));
        }
    }
}