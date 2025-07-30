using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Tracker.UI;
using System.Security.Cryptography;
using System.Text;

namespace Sales_Tracker.Classes
{
    public enum DataPointType
    {
        Export,
        OpenAI,
        OpenExchangeRates,
        GoogleSheets,
        Session,
        Error
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

    public class GeoLocationData
    {
        public string Country { get; set; } = "Unknown";
        public string CountryCode { get; set; } = "Unknown";
        public string Region { get; set; } = "Unknown";
        public string City { get; set; } = "Unknown";
        public string Timezone { get; set; } = "Unknown";
        public string ISP { get; set; } = "Unknown";
        public bool IsVPN { get; set; } = false;
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
    }

    public static class ApiConfig
    {
        public static string? ApiKey => DotEnv.Get("UPLOAD_API_KEY");
        public static string UserAgent => $"ArgoSalesTracker/{Tools.GetVersionNumber()}";
        public static string ServerUrl => "https://argorobots.com/upload_data.php";
    }

    public static class GeoLocationService
    {
        private static GeoLocationData? _cachedLocation;
        private static DateTime _lastFetched = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(12);
        private static readonly string[] GeoApiEndpoints = [
            "http://ip-api.com/json/?fields=status,country,countryCode,region,city,timezone,isp,proxy,lat,lon",
            "https://ipapi.co/json/",
            "https://ipinfo.io/json"
        ];

        public static async Task<GeoLocationData> GetLocationAsync()
        {
            // Return cached data if still valid
            if (_cachedLocation != null && (DateTime.Now - _lastFetched) < CacheExpiry)
            {
                return _cachedLocation;
            }

            // Try multiple geo-location services for reliability
            foreach (string endpoint in GeoApiEndpoints)
            {
                try
                {
                    using HttpClient client = new();
                    client.Timeout = TimeSpan.FromSeconds(8);
                    client.DefaultRequestHeaders.Add("User-Agent", ApiConfig.UserAgent);

                    string response = await client.GetStringAsync(endpoint);
                    dynamic? locationData = JsonConvert.DeserializeObject<dynamic>(response);

                    GeoLocationData geoData = new();

                    // Parse different API response formats
                    if (endpoint.Contains("ip-api.com"))
                    {
                        if (locationData?.status == "success")
                        {
                            geoData = ParseIpApiResponse(locationData);
                        }
                    }
                    else if (endpoint.Contains("ipapi.co"))
                    {
                        geoData = ParseIpApiCoResponse(locationData);
                    }
                    else if (endpoint.Contains("ipinfo.io"))
                    {
                        geoData = ParseIpInfoResponse(locationData);
                    }

                    if (geoData.Country != "Unknown")
                    {
                        _cachedLocation = geoData;
                        _lastFetched = DateTime.Now;
                        return _cachedLocation;
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteWithFormat(1, "Geo-location service {0} failed: {1}", endpoint, ex.Message);
                    continue;  // Try next service
                }
            }

            Log.Error_AnonymousDataCollection("All geo-location services failed, using default location");
            _cachedLocation = new GeoLocationData();
            return _cachedLocation;
        }
        private static GeoLocationData ParseIpApiResponse(dynamic data)
        {
            return new GeoLocationData
            {
                Country = data.country?.ToString() ?? "Unknown",
                CountryCode = data.countryCode?.ToString() ?? "Unknown",
                Region = data.region?.ToString() ?? "Unknown",
                City = data.city?.ToString() ?? "Unknown",
                Timezone = data.timezone?.ToString() ?? "Unknown",
                ISP = data.isp?.ToString() ?? "Unknown",
                IsVPN = data.proxy == true,
                Latitude = (double)(data.lat ?? 0.0),
                Longitude = (double)(data.lon ?? 0.0)
            };
        }
        private static GeoLocationData ParseIpApiCoResponse(dynamic data)
        {
            return new GeoLocationData
            {
                Country = data.country_name?.ToString() ?? "Unknown",
                CountryCode = data.country_code?.ToString() ?? "Unknown",
                Region = data.region?.ToString() ?? "Unknown",
                City = data.city?.ToString() ?? "Unknown",
                Timezone = data.timezone?.ToString() ?? "Unknown",
                ISP = data.org?.ToString() ?? "Unknown",
                IsVPN = false,  // ipapi.co doesn't provide VPN detection in free tier
                Latitude = (double)(data.latitude ?? 0.0),
                Longitude = (double)(data.longitude ?? 0.0)
            };
        }
        private static GeoLocationData ParseIpInfoResponse(dynamic data)
        {
            string[]? location = data.loc?.ToString()?.Split(',');

            return new GeoLocationData
            {
                Country = data.country?.ToString() ?? "Unknown",
                CountryCode = data.country?.ToString() ?? "Unknown",
                Region = data.region?.ToString() ?? "Unknown",
                City = data.city?.ToString() ?? "Unknown",
                Timezone = data.timezone?.ToString() ?? "Unknown",
                ISP = data.org?.ToString() ?? "Unknown",
                IsVPN = false,  // ipinfo.io doesn't provide VPN detection in free tier
                Latitude = location?.Length >= 2 && double.TryParse(location[0], out double lat) ? lat : 0.0,
                Longitude = location?.Length >= 2 && double.TryParse(location[1], out double lon) ? lon : 0.0
            };
        }
        public static string HashIP(string ip, string salt = "AnonymousTracker2025")
        {
            try
            {
                byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip + salt));
                return Convert.ToBase64String(hashedBytes)[..16];  // Truncate for storage
            }
            catch
            {
                return "HashFailed";
            }
        }
        public static async Task<string> GetHashedIPAsync()
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("User-Agent", ApiConfig.UserAgent);
                string ip = await client.GetStringAsync("https://api.ipify.org");
                return HashIP(ip.Trim());
            }
            catch
            {
                return "IPUnavailable";
            }
        }
    }

    /// <summary>
    /// Manages collection and storage of anonymous data points for analytics purposes with geo-location tracking.
    /// </summary>
    public static class AnonymousDataManager
    {
        private static GeoLocationData? _sessionLocation;
        private static DateTime? _sessionStartTime;
        private static string? _sessionHashedIP;

        /// <summary>
        /// Initializes the session with geo-location data.
        /// </summary>
        public static async Task InitializeSessionAsync()
        {
            try
            {
                _sessionLocation = await GeoLocationService.GetLocationAsync();
                _sessionHashedIP = await GeoLocationService.GetHashedIPAsync();
            }
            catch (Exception ex)
            {
                Log.Error_AnonymousDataCollection($"Failed to initialize session geo-location: {ex.Message}");
                _sessionLocation = new GeoLocationData();
                _sessionHashedIP = "InitFailed";
            }
        }
        private static Dictionary<string, object> GetBaseDataPoint(DataPointType dataType)
        {
            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["dataType"] = dataType.ToString(),
                ["country"] = _sessionLocation?.Country ?? "Unknown",
                ["countryCode"] = _sessionLocation?.CountryCode ?? "Unknown",
                ["region"] = _sessionLocation?.Region ?? "Unknown",
                ["city"] = _sessionLocation?.City ?? "Unknown",
                ["timezone"] = _sessionLocation?.Timezone ?? "Unknown",
                ["isp"] = _sessionLocation?.ISP ?? "Unknown"
            };

            if (_sessionLocation?.IsVPN == true)
            {
                dataPoint["isVPN"] = true;
            }

            if (!string.IsNullOrEmpty(_sessionHashedIP) && _sessionHashedIP != "IPUnavailable")
            {
                dataPoint["hashedIP"] = _sessionHashedIP;
            }

            if (_sessionLocation?.Latitude != 0.0 || _sessionLocation?.Longitude != 0.0)
            {
                dataPoint["latitude"] = _sessionLocation?.Latitude ?? 0.0;
                dataPoint["longitude"] = _sessionLocation?.Longitude ?? 0.0;
            }

            return dataPoint;
        }

        /// <summary>
        /// Adds export operation data to the anonymous data log.
        /// </summary>
        public static void AddExportData(Dictionary<ExportDataField, object> data)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await AddExportDataAsync(data);
                }
                catch (Exception ex)
                {
                    Log.Error_AnonymousDataCollection($"Failed to track export data: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Adds export operation data to the anonymous data log.
        /// </summary>
        public static async Task AddExportDataAsync(Dictionary<ExportDataField, object> data)
        {
            if (_sessionLocation == null)
            {
                await InitializeSessionAsync();
            }

            ExportType exportType = (ExportType)data[ExportDataField.ExportType];
            long durationMs = (long)data[ExportDataField.DurationMS];

            Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.Export);
            dataPoint["ExportType"] = exportType.ToString();
            dataPoint["DurationMS"] = Tools.FormatDuration(durationMs);
            dataPoint["FileSize"] = data[ExportDataField.FileSize];

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds OpenAI API usage data to the anonymous data log.
        /// </summary>
        public static void AddOpenAIUsageData(string model, long durationMs, int tokensUsed)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await AddOpenAIUsageDataAsync(model, durationMs, tokensUsed);
                }
                catch (Exception ex)
                {
                    Log.Error_AnonymousDataCollection($"Failed to track OpenAI usage: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Adds OpenAI API usage data to the anonymous data log.
        /// </summary>
        public static async Task AddOpenAIUsageDataAsync(string model, long durationMs, int tokensUsed)
        {
            if (_sessionLocation == null)
            {
                await InitializeSessionAsync();
            }

            Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.OpenAI);
            dataPoint["Model"] = model;
            dataPoint["DurationMS"] = durationMs;
            dataPoint["TokensUsed"] = tokensUsed;

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds Open Exchange Rates API usage data to the anonymous data log.
        /// </summary>
        public static void AddOpenExchangeRatesData(long durationMs)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await AddOpenExchangeRatesDataAsync(durationMs);
                }
                catch (Exception ex)
                {
                    Log.Error_AnonymousDataCollection($"Failed to track exchange rates usage: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Adds Open Exchange Rates API usage data to the anonymous data log.
        /// </summary>
        public static async Task AddOpenExchangeRatesDataAsync(long durationMs)
        {
            if (_sessionLocation == null)
            {
                await InitializeSessionAsync();
            }

            Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.OpenExchangeRates);
            dataPoint["DurationMS"] = durationMs;

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Adds error occurrence data to the anonymous data log.
        /// </summary>
        public static void AddErrorData(string errorCode, string errorCategory, int lineNumber)
        {
            _ = Task.Run(async () =>
            {
                if (_sessionLocation == null)
                {
                    await InitializeSessionAsync();
                }

                Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.Error);
                dataPoint["ErrorCode"] = errorCode;
                dataPoint["ErrorCategory"] = errorCategory;
                dataPoint["LineNumber"] = lineNumber;

                AppendToDataFile(dataPoint, false);  // Don't log errors to prevent recursion
            });
        }
        public static void TrackSessionStart()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    _sessionStartTime = DateTime.Now;

                    if (_sessionLocation == null)
                    {
                        await InitializeSessionAsync();
                    }

                    Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.Session);
                    dataPoint["action"] = "SessionStart";
                    dataPoint["userAgent"] = Environment.OSVersion.ToString();
                    dataPoint["appVersion"] = Tools.GetVersionNumber();

                    AppendToDataFile(dataPoint);
                }
                catch (Exception ex)
                {
                    Log.Error_AnonymousDataCollection($"Failed to track session start: {ex.Message}");
                }
            });
        }
        public static void TrackSessionEnd()
        {
            if (_sessionStartTime == null) { return; }

            TimeSpan duration = DateTime.Now - _sessionStartTime.Value;

            Dictionary<string, object> dataPoint = GetBaseDataPoint(DataPointType.Session);
            dataPoint["action"] = "SessionEnd";
            dataPoint["duration"] = duration.TotalSeconds;

            AppendToDataFile(dataPoint);
        }

        /// <summary>
        /// Appends a data point to the anonymous data cache file.
        /// </summary>
        private static void AppendToDataFile(Dictionary<string, object> dataPoint, bool logErrors = true)
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
                if (logErrors)
                {
                    Log.Error_AnonymousDataCollection($"Failed to log anonymous data: {ex.Message}");
                }
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
                ["geoLocationEnabled"] = true,
                ["dataPoints"] = new JObject
                {
                    ["Export"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Export.ToString())),
                    ["OpenAI"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.OpenAI.ToString())),
                    ["OpenExchangeRates"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.OpenExchangeRates.ToString())),
                    ["GoogleSheets"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.GoogleSheets.ToString())),
                    ["Session"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Session.ToString())),
                    ["Error"] = new JArray(allDataPoints.Where(d => d["dataType"]?.ToString() == DataPointType.Error.ToString()))
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
        public static void ClearUserData()
        {
            Directories.DeleteFile(Directories.AnonymousUserData_file);
            LanguageManager.TranslationCache = [];
            _sessionLocation = null;
            _sessionHashedIP = null;
        }

        /// <summary>
        /// Gets the size of the anonymous data cache file in bytes.
        /// </summary>
        public static long GetUserDataCacheSize()
        {
            if (File.Exists(Directories.AnonymousUserData_file))
            {
                FileInfo fileInfo = new(Directories.AnonymousUserData_file);
                return fileInfo.Length;
            }
            return 0;
        }
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