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
        Session
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

    public static class ApiConfig
    {
        public static string? ApiKey => DotEnv.Get("UPLOAD_API_KEY");
        public static string UserAgent => "ArgoSalesTracker/1.0";
        public static string ServerUrl => "https://argorobots.com/upload_data.php";
    }

    /// <summary>
    /// Manages collection and storage of anonymous data points for analytics purposes.
    /// </summary>
    public static class AnonymousDataManager
    {
        // Methods to add data points
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

        private static DateTime? _sessionStartTime;
        public static void TrackSessionStart()
        {
            _sessionStartTime = DateTime.Now;
        }
        public static void TrackSessionEnd()
        {
            if (_sessionStartTime == null) { return; }

            TimeSpan duration = DateTime.Now - _sessionStartTime.Value;

            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = DataPointType.Session.ToString(),
                ["duration"] = duration.TotalSeconds
            };

            AppendToDataFile(dataPoint);
        }

        // General methods
        /// <summary>
        /// Appends a data point to the anonymous data cache file.
        /// </summary>
        private static void AppendToDataFile(Dictionary<string, object> dataPoint)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(Directories.AnonymousUserData_file);
                Directories.CreateDirectory(directory);

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
        /// Exports the anonymous data in an organized format by data type.
        /// </summary>
        public static void ExportOrganizedData(string outputFilePath, bool indent)
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
                    ["GoogleSheets"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.GoogleSheets.ToString())),
                    ["Session"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Session.ToString()))
                }
            };

            // Write data to file
            Formatting format = indent ? Formatting.Indented : Formatting.None;
            string organizedJson = JsonConvert.SerializeObject(organizedData, format);
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
            if (!File.Exists(Directories.AnonymousUserData_file)) { return; }

            string tempFile = "";
            try
            {
                // Check if API key is available
                string? apiKey = ApiConfig.ApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    Log.Error_AnonymousDataCollection("Upload API key not found in environment variables");
                    return;
                }

                tempFile = Path.Combine(Path.GetTempPath(), "organized_anonymous_data.json");
                ExportOrganizedData(tempFile, false);

                using HttpClient client = new();

                // Set authentication headers
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                client.DefaultRequestHeaders.Add("User-Agent", ApiConfig.UserAgent);

                // Properly manage the file stream
                using FileStream fileStream = File.OpenRead(tempFile);
                using StreamContent streamContent = new(fileStream);
                using MultipartFormDataContent form = new()
                {
                    { streamContent, "file", "anonymous_data.json" }
                };

                HttpResponseMessage response = await client.PostAsync(ApiConfig.ServerUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Log.Write(1, "Successfully uploaded anonymous data");
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Log.Error_AnonymousDataCollection($"Upload failed with status {response.StatusCode}: {error}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error_AnonymousDataCollection($"Network error during upload: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Log.Error_AnonymousDataCollection($"Upload timeout: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error_AnonymousDataCollection($"Upload failed: {ex.Message}");
            }
            finally
            {
                // Clean up - file handles are now closed
                Directories.DeleteFile(tempFile);
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