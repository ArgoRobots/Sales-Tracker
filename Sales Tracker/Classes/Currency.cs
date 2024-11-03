using System.Text.Json;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Static class responsible for handling currency-related operations including currency types,
    /// symbols, and exchange rate calculations. Supports multiple international currencies and
    /// provides real-time exchange rate data through the OpenExchangeRates API.
    /// </summary>
    internal static class Currency
    {
        /// <summary>
        /// Enumeration of supported currency types, ordered by relative western geographic location.
        /// Each entry represents a national or regional currency with its ISO 4217 three-letter code.
        /// https://www.iso.org/iso-4217-currency-codes.html
        /// </summary>
        public enum CurrencyTypes
        {
            USD,    // United States Dollar
            CAD,    // Canadian Dollar
            EUR,    // Euro
            BRL,    // Brazilian Real
            DKK,    // Danish Krone
            NOK,    // Norwegian Krone
            SEK,    // Swedish Krona
            ISK,    // Icelandic Króna
            PLN,    // Polish Złoty
            CZK,    // Czech Koruna
            HUF,    // Hungarian Forint
            ALL,    // Albanian Lek
            RON,    // Romanian Leu
            BGN,    // Bulgarian Lev
            RSD,    // Serbian Dinar
            MKD,    // Macedonian Denar
            BAM,    // Bosnia and Herzegovina Convertible Mark
            UAH,    // Ukrainian Hryvnia
            BYN,    // Belarusian Ruble
            RUB,    // Russian Ruble
            TRY,    // Turkish Lira
            JPY,    // Japanese Yen
            KRW,    // South Korean Won
            CNY,    // Chinese Yuan Renminbi
            TWD     // Taiwan Dollar
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
        /// Gets a list of all supported currency type names.
        /// </summary>
        /// <returns>List of currency type names as strings.</returns>
        public static List<string> GetCurrencyTypesList()
        {
            return Enum.GetNames(typeof(CurrencyTypes)).ToList();
        }

        /// <summary>
        /// Gets the symbol for the default currency type set in application settings.
        /// </summary>
        /// <returns>Currency symbol as a string.</returns>
        public static string GetSymbol()
        {
            string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);

            if (Enum.TryParse(currency, out CurrencyTypes currencyType))
            {
                if (CurrencySymbols.TryGetValue(currencyType, out string symbol))
                {
                    return symbol;
                }
            }
            throw new ArgumentException("Invalid currency type");
        }

        /// <summary>
        /// Retrieves the exchange rate between two currencies for a specific date using the OpenExchangeRates API.
        /// </summary>
        /// <returns>
        /// The exchange rate as a decimal value. Returns
        /// 1 if source and target currencies are the same, or
        /// -1 if an error occurs and the rate cannot be retrieved
        /// </returns>
        /// <remarks>
        /// Requires an internet connection to access the OpenExchangeRates API.
        /// </remarks>
        public static decimal GetExchangeRate(string sourceCurrency, string targetCurrency, string date, bool showErrorMessage = true)
        {
            if (sourceCurrency == targetCurrency) { return 1; }

            // Your API key
            string appId = "beb7bee9c266473297d93c2132da637f";
            string url = $"https://openexchangerates.org/api/historical/{date}.json?app_id={appId}";

            try
            {
                using HttpClient httpClient = new();
                HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement root = jsonDocument.RootElement;

                // Access the "rates" property and get the value for source and target currencies
                if (root.TryGetProperty("rates", out JsonElement rates))
                {
                    if (rates.TryGetProperty(sourceCurrency, out JsonElement sourceRateElement) && rates.TryGetProperty(targetCurrency, out JsonElement targetRateElement))
                    {
                        decimal sourceRate = sourceRateElement.GetDecimal();
                        decimal targetRate = targetRateElement.GetDecimal();

                        // Calculate the exchange rate from sourceCurrency to targetCurrency
                        return targetRate / sourceRate;
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
    }
}