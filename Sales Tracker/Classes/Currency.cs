using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Static class responsible for handling currency-related operations including currency types,
    /// symbols, and exchange rate calculations. Supports multiple international currencies and
    /// provides real-time exchange rate data through the OpenExchangeRates API.
    /// </summary>
    public static class Currency
    {
        /// <summary>
        /// Enumeration of supported currency types, ordered by relative western geographic location.
        /// Each entry represents a national or regional currency with its ISO 4217 three-letter code.
        /// https://www.iso.org/iso-4217-currency-codes.html
        /// </summary>
        public enum CurrencyTypes
        {
            USD,  // United States Dollar
            CAD,  // Canadian Dollar
            EUR,  // Euro
            AUD,  // Australia
            BRL,  // Brazilian Real
            DKK,  // Danish Krone
            NOK,  // Norwegian Krone
            SEK,  // Swedish Krona
            ISK,  // Icelandic Króna
            PLN,  // Polish Złoty
            CZK,  // Czech Koruna
            HUF,  // Hungarian Forint
            ALL,  // Albanian Lek
            RON,  // Romanian Leu
            BGN,  // Bulgarian Lev
            RSD,  // Serbian Dinar
            MKD,  // Macedonian Denar
            BAM,  // Bosnia and Herzegovina Convertible Mark
            UAH,  // Ukrainian Hryvnia
            BYN,  // Belarusian Ruble
            RUB,  // Russian Ruble
            TRY,  // Turkish Lira
            JPY,  // Japanese Yen
            KRW,  // South Korean Won
            CNY,  // Chinese Yuan Renminbi
            TWD   // Taiwan Dollar
        }

        /// <summary>
        /// Dictionary mapping currency types to their corresponding symbols.
        /// Provides quick lookup of currency symbols for display purposes.
        /// </summary>
        private static readonly Dictionary<CurrencyTypes, string> CurrencySymbols = new()
        {
            { CurrencyTypes.USD, "$" },
            { CurrencyTypes.CAD, "$" },
            { CurrencyTypes.EUR, "€" },
            { CurrencyTypes.AUD, "$" },
            { CurrencyTypes.BRL, "R$" },
            { CurrencyTypes.DKK, "kr" },
            { CurrencyTypes.NOK, "kr" },
            { CurrencyTypes.SEK, "kr" },
            { CurrencyTypes.ISK, "kr" },
            { CurrencyTypes.PLN, "zł" },
            { CurrencyTypes.CZK, "Kč" },
            { CurrencyTypes.HUF, "Ft" },
            { CurrencyTypes.ALL, "L" },
            { CurrencyTypes.RON, "lei" },
            { CurrencyTypes.BGN, "лв" },
            { CurrencyTypes.RSD, "дин" },
            { CurrencyTypes.MKD, "ден" },
            { CurrencyTypes.BAM, "KM" },
            { CurrencyTypes.UAH, "₴" },
            { CurrencyTypes.BYN, "Br" },
            { CurrencyTypes.RUB, "₽" },
            { CurrencyTypes.TRY, "₺" },
            { CurrencyTypes.JPY, "¥" },
            { CurrencyTypes.KRW, "₩" },
            { CurrencyTypes.CNY, "¥" },
            { CurrencyTypes.TWD, "NT$" }
        };

        /// <summary>
        /// In-memory cache for exchange rates to minimize API calls.
        /// Key format: "date_sourceCurrency_targetCurrency" (e.g., "2023-04-01_USD_EUR").
        /// </summary>
        private static readonly Dictionary<string, decimal> _exchangeRateCache = [];

        /// <summary>
        /// Initializes the currency cache by loading saved exchange rates from disk.
        /// </summary>
        static Currency()
        {
            LoadExchangeRateCache();
        }

        /// <summary>
        /// Gets a list of all supported currency type names.
        /// </summary>
        /// <returns>List of currency type names as strings.</returns>
        public static List<string> GetCurrencyTypesList()
        {
            return Enum.GetNames<CurrencyTypes>().ToList();
        }

        /// <summary>
        /// Gets the symbol for the default currency type set in application settings.
        /// </summary>
        /// <returns>Currency symbol as a string.</returns>
        public static string GetSymbol()
        {
            string currency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);

            if (Enum.TryParse(currency, out CurrencyTypes currencyType))
            {
                if (CurrencySymbols.TryGetValue(currencyType, out string symbol))
                {
                    return symbol;
                }
            }
            throw new ArgumentException($"Invalid currency type: {currency}");
        }

        /// <summary>
        /// Retrieves the exchange rate between two currencies for a specific date using the OpenExchangeRates API.
        /// Uses a caching mechanism to minimize API calls.
        /// </summary>
        /// <returns>
        /// The exchange rate as a decimal value. Returns
        /// 1 if source and target currencies are the same, or
        /// -1 if an error occurs and the rate cannot be retrieved
        /// </returns>
        /// <remarks>
        /// Requires an internet connection to access the OpenExchangeRates API if data is not in cache.
        /// </remarks>
        public static decimal GetExchangeRate(string sourceCurrency, string targetCurrency, string date, bool showErrorMessage = true)
        {
            if (sourceCurrency == targetCurrency) { return 1; }

            // Check in-memory cache first
            string cacheKey = $"{date}_{sourceCurrency}_{targetCurrency}";
            if (_exchangeRateCache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                return cachedRate;
            }

            // Check if we have the inverse rate (e.g., USD to EUR instead of EUR to USD)
            string inverseCacheKey = $"{date}_{targetCurrency}_{sourceCurrency}";
            if (_exchangeRateCache.TryGetValue(inverseCacheKey, out decimal inverseRate) && inverseRate != 0)
            {
                decimal calculatedRate = 1 / inverseRate;
                _exchangeRateCache[cacheKey] = calculatedRate;  // Cache the calculated rate
                SaveExchangeRateCache();
                return calculatedRate;
            }

            // If not in cache, fetch from API
            string _openExchangeRateApiKey = DotEnv.Get("OPEN_EXCHANGE_RATE_API_KEY");
            string url = $"https://openexchangerates.org/api/historical/{date}.json?app_id={_openExchangeRateApiKey}";

            try
            {
                // Start tracking API call duration
                DateTime startTime = DateTime.Now;

                using HttpClient httpClient = new();
                HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();

                // Calculate API call duration
                long durationMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject responseJson = JObject.Parse(responseBody);

                // Track the API usage data
                AnonymousDataManager.AddOpenExchangeRatesData(durationMs);

                // Access the "rates" property and get the value for source and target currencies
                if (responseJson["rates"] != null)
                {
                    JObject rates = (JObject)responseJson["rates"];
                    if (rates[sourceCurrency] != null && rates[targetCurrency] != null)
                    {
                        decimal sourceRate = rates[sourceCurrency].Value<decimal>();
                        decimal targetRate = rates[targetCurrency].Value<decimal>();

                        // Calculate the exchange rate from sourceCurrency to targetCurrency
                        decimal exchangeRate = targetRate / sourceRate;

                        // Cache both the rate and its inverse
                        _exchangeRateCache[cacheKey] = exchangeRate;
                        _exchangeRateCache[inverseCacheKey] = 1 / exchangeRate;
                        SaveExchangeRateCache();

                        return exchangeRate;
                    }
                    else
                    {
                        Log.Error_GetExchangeRate($"{sourceCurrency} or {targetCurrency} rate not found");
                        return -1;
                    }
                }
                else
                {
                    Log.Error_GetExchangeRate("rates not found");
                    return -1;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Error_GetExchangeRate("The request timed out. Please try again later");
                return -1;
            }
            catch (HttpRequestException)
            {
                if (showErrorMessage)
                {
                    string message = "It looks like you're not connected to the internet. Please check your connection and try again. A connection is needed to get the currency exchange rates";
                    CustomMessageBox.Show("No connection", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error_GetExchangeRate(ex.Message);
                return -1;
            }
        }
        private static void LoadExchangeRateCache()
        {
            try
            {
                // If cache file exists, load it
                if (File.Exists(Directories.ExchangeRates_file))
                {
                    string json = File.ReadAllText(Directories.ExchangeRates_file);
                    Dictionary<string, decimal>? cacheData = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);

                    if (cacheData != null)
                    {
                        foreach (KeyValuePair<string, decimal> item in cacheData)
                        {
                            _exchangeRateCache[item.Key] = item.Value;
                        }
                        Log.Write(1, $"Loaded {cacheData.Count} exchange rates from cache");
                    }
                }
            }
            catch
            {
                Log.Error_ReadFile(Directories.ExchangeRates_file);
                _exchangeRateCache.Clear();
            }
        }
        private static void SaveExchangeRateCache()
        {
            try
            {
                // Create cache directory if it doesn't exist
                if (!Directory.Exists(Directories.Cache_dir))
                {
                    Directory.CreateDirectory(Directories.Cache_dir);
                }

                // Serialize the cache to JSON using a cached JsonSerializerSettings for better performance
                string json = JsonConvert.SerializeObject(_exchangeRateCache, Formatting.Indented);
                File.WriteAllText(Directories.ExchangeRates_file, json);
            }
            catch (Exception ex)
            {
                // If there's an error saving the cache, log it but continue
                Log.Error_WriteToFile($"Failed to save exchange rate cache: {ex.Message}");
            }
        }
    }
}