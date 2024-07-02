using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    internal class Currency
    {
        public enum CurrencyTypes
        {
            CAD,
            USD,
            EUR,
            AUD
        }

        private static readonly HttpClient httpClient = new();

        /// <summary>
        /// The date must be in yyyy-mm-dd format.
        /// </summary>
        public static decimal GetExchangeRate(string sourceCurrency, string targetCurrency, string date)
        {
            // Your API key
            string appId = "beb7bee9c266473297d93c2132da637f";
            string url = $"https://openexchangerates.org/api/historical/{date}.json?app_id={appId}";

            try
            {
                string response = httpClient.GetStringAsync(url).Result;
                using JsonDocument jsonDocument = JsonDocument.Parse(response);
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
            catch (Exception ex)
            {
                Log.Write(0, $"Exception: {ex.Message}");
                return -1;
            }
        }
    }
}