using System.Text.Json;

namespace Sales_Tracker.Classes
{
    internal static class Currency
    {
        // Ordered by GDP
        public enum CurrencyTypes
        {
            USD,
            EUR,
            GBP,
            CAD,
            AUD
        }
        private static readonly Dictionary<CurrencyTypes, string> CurrencySymbols = new()
        {
            { CurrencyTypes.USD, "$" },
            { CurrencyTypes.EUR, "€" },
            { CurrencyTypes.GBP, "£" },
            { CurrencyTypes.CAD, "$" },
            { CurrencyTypes.AUD, "$" }
        };
        public static string GetSymbol(string currencyTypeString)
        {
            if (Enum.TryParse(currencyTypeString, out CurrencyTypes currencyType))
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
            // Your API key
            string appId = "beb7bee9c266473297d93c2132da637f";
            string url = $"https://openexchangerates.org/api/historical/{date}.json?app_id={appId}";

            try
            {
                using HttpClient httpClient = new();
                HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {

                }

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
                        Log.Write(0, $"{sourceCurrency} or {targetCurrency} rate not found.");
                        return -1;
                    }
                }
                else
                {
                    Log.Write(0, "rates not found.");
                    return -1;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Write(0, "The request timed out. Please try again later");
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
                Log.Write(0, $"Exception: {ex.Message}");
                return -1;
            }
        }
    }
}