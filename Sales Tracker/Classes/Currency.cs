using System.Text.Json;

namespace Sales_Tracker.Classes
{
    internal static class Currency
    {
        // Ordered by how western the country is
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
        public static List<string> GetCurrencyTypesList()
        {
            return Enum.GetNames(typeof(CurrencyTypes)).ToList();
        }
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
        /// The date must be in yyyy-mm-dd format.
        /// </summary>
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
                    CustomMessageBox.Show("Argo Sales Tracker",
                        "It looks like you're not connected to the internet. Please check your connection and try again. A connection is needed to get the currency exchange rates",
                        CustomMessageBoxIcon.Exclamation,
                        CustomMessageBoxButtons.Ok);
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