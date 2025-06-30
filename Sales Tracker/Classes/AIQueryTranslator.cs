using Newtonsoft.Json;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.Text;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Translates natural language queries into structured search parameters for the SearchDataGridView functionality.
    /// </summary>
    public class AIQueryTranslator
    {
        // Properties
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly DataGridViewSchemaInfo _schemaInfo;

        // Getter
        public int LastTokenUsage { get; private set; }

        // Methods
        public AIQueryTranslator(string apiKey, string apiEndpoint = "https://api.openai.com/v1/chat/completions")
        {
            _apiKey = apiKey;
            _apiEndpoint = apiEndpoint;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Initialize schema information - this helps the AI understand your data structure
            _schemaInfo = BuildSchemaInfo();
        }

        /// <summary>
        /// Translates a natural language query into structured search parameters.
        /// </summary>
        /// <param name="naturalLanguageQuery">The user's search query in natural language</param>
        /// <returns>A structured search query that can be used with SearchDataGridView</returns>
        public async Task<string> TranslateQueryAsync(string naturalLanguageQuery)
        {
            try
            {
                // Only process if there's an actual query
                if (string.IsNullOrWhiteSpace(naturalLanguageQuery))
                {
                    return "";
                }

                // Create the prompt for the AI
                string prompt = BuildPrompt(naturalLanguageQuery);

                // Send request to AI API
                string response = await SendApiRequestAsync(prompt);

                // Extract the formatted search query
                string formattedQuery = ParseResponse(response);

                return formattedQuery;
            }
            catch (Exception ex)
            {
                Log.Error_Translation(ex.Message);

                // Return the original query as fallback
                return naturalLanguageQuery;
            }
        }
        private string BuildPrompt(string naturalLanguageQuery)
        {
            // Create a detailed prompt that explains the search system and what we need
            StringBuilder promptBuilder = new();

            promptBuilder.AppendLine("You are a search query translator for a sales tracking application. Your role is to convert natural language queries into structured search syntax.");
            promptBuilder.AppendLine("\nThe application supports the following search syntax:");
            promptBuilder.AppendLine("- Regular terms for fuzzy matching");
            promptBuilder.AppendLine("- \"exact phrase\" for exact phrase matching");
            promptBuilder.AppendLine("- -term for excluding terms");
            promptBuilder.AppendLine("- +term for required terms");
            promptBuilder.AppendLine("- Field:value for field-specific searches (e.g., Company:AliExpress)");
            promptBuilder.AppendLine("- Field:>value for greater than comparisons (e.g., Discount:>10)");
            promptBuilder.AppendLine("- Field:<value for less than comparisons (e.g., Total:<50)");
            promptBuilder.AppendLine("- Use OR operator with pipe character '|' for multiple options (e.g., Country:France|Germany|Italy)");

            // Add information about the data schema
            promptBuilder.AppendLine("\nData includes the following fields:");

            foreach (FieldInfo field in _schemaInfo.Fields)
            {
                promptBuilder.AppendLine($"- {field.Name}: {field.Description}");

                // Include full country list for the Country field
                if (field.Name == "Country" && field.CommonValues != null && field.CommonValues.Count > 0)
                {
                    promptBuilder.AppendLine("  When the user refers to a country, map it to one of these exact country names:");
                    // Include the first 10 countries to give an idea
                    promptBuilder.AppendLine($"\n{string.Join(", ", field.CommonValues)}");
                    promptBuilder.AppendLine("\n For example, 'America' and 'USA' should be mapped to 'United States', 'UK' should be mapped to 'United Kingdom'");
                }
            }

            // Regional Groupings for OR conditions
            promptBuilder.AppendLine("\nREGIONAL GROUPINGS INSTRUCTIONS:");
            promptBuilder.AppendLine("When a user refers to a region or continent, translate it to an OR condition of all relevant countries.");
            promptBuilder.AppendLine("Examples of regional groupings:");
            promptBuilder.AppendLine("1. Europe: Use 'Country:France|Germany|United Kingdom|Italy|Spain|Netherlands|Sweden|Switzerland|Norway|Finland|Belgium|Denmark|Austria|Portugal|Greece|Ireland'");
            promptBuilder.AppendLine("2. Asia: Use 'Country:China|Japan|South Korea|India|Taiwan|Singapore|Vietnam|Thailand|Malaysia|Indonesia|Philippines'");
            promptBuilder.AppendLine("3. North America: Use 'Country:United States|Canada|Mexico'");
            promptBuilder.AppendLine("4. South America: Use 'Country:Brazil|Argentina|Colombia|Chile|Peru|Ecuador|Venezuela'");
            promptBuilder.AppendLine("5. Africa: Use 'Country:South Africa|Nigeria|Egypt|Kenya|Morocco|Ghana|Ethiopia|Tanzania'");
            promptBuilder.AppendLine("6. Middle East: Use 'Country:United Arab Emirates|Saudi Arabia|Israel|Turkey|Qatar|Kuwait|Bahrain|Oman'");
            promptBuilder.AppendLine("7. Oceania: Use 'Country:Australia|New Zealand|Fiji|Papua New Guinea'");

            // Explanation of "high" and "low" value thresholds
            promptBuilder.AppendLine("\nThresholds for qualitative terms:");
            promptBuilder.AppendLine("- \"high discount\" means discount values > 5");
            promptBuilder.AppendLine("- \"low price\" means price values < 50");
            promptBuilder.AppendLine("- \"expensive\" means total cost > 200");
            promptBuilder.AppendLine("- \"cheap\" means total cost < 50");

            // Add rules for using country fields
            promptBuilder.AppendLine("\nIMPORTANT RULES FOR COUNTRY FIELDS:");
            promptBuilder.AppendLine("1. Use 'Country of origin' for queries about where products come from");
            promptBuilder.AppendLine("2. Use 'Country of destination' for queries about where products are sent to");
            promptBuilder.AppendLine("3. If the query mentions 'purchases' or 'orders', use 'Country of origin'");
            promptBuilder.AppendLine("4. If the query mentions 'sales', use 'Country of destination'");
            promptBuilder.AppendLine("5. If unsure, default to 'Country of origin'");

            // Add examples to guide the AI
            promptBuilder.AppendLine("\nExamples:");

            promptBuilder.AppendLine("Query: \"products from america\"");
            promptBuilder.AppendLine("Translation: \"+Country of origin:United States\"");

            promptBuilder.AppendLine("\nQuery: \"sales to UK last month\"");
            promptBuilder.AppendLine("Translation: \"+Country of destination:United Kingdom +Date:>2023-12-01\"");

            promptBuilder.AppendLine("\nQuery: \"aliexpress orders with high discount\"");
            promptBuilder.AppendLine("Translation: \"+Company of origin:aliexpress +Discount:>5\"");

            promptBuilder.AppendLine("\nQuery: \"expensive purchases from last month\"");
            promptBuilder.AppendLine("Translation: \"+Total:>200 +Date:>2023-12-01\"");

            promptBuilder.AppendLine("\nQuery: \"sales with free shipping\"");
            promptBuilder.AppendLine("Translation: \"+Shipping:0\"");

            promptBuilder.AppendLine("\nQuery: \"orders from china with discount greater than 10\"");
            promptBuilder.AppendLine("Translation: \"+Country of origin:China +Discount:>10\"");

            promptBuilder.AppendLine("\nQuery: \"orders from europe\"");
            promptBuilder.AppendLine("Translation: \"+Country of origin:France|Germany|United Kingdom|Italy|Spain|Netherlands|Sweden|Switzerland|Norway|Finland|Belgium|Denmark|Austria|Portugal|Greece|Ireland\"");

            promptBuilder.AppendLine("\nQuery: \"sales to asian countries last month\"");
            promptBuilder.AppendLine("Translation: \"+Country of destination:China|Japan|South Korea|India|Taiwan|Singapore|Vietnam|Thailand|Malaysia|Indonesia|Philippines +Date:>2023-12-01\"");

            // Finally, add the user's query
            promptBuilder.AppendLine("\nUser Query: \"" + naturalLanguageQuery + "\"");
            promptBuilder.AppendLine("\nTranslation:");

            // Emphasize output format
            promptBuilder.AppendLine("\nIMPORTANT: Return ONLY the structured search query without any explanation or additional text.");
            promptBuilder.AppendLine("Use the exact field names as provided above and ensure proper syntax with + for required terms.");
            promptBuilder.AppendLine("Remember to use the pipe character '|' for OR conditions when translating regional terms.");

            return promptBuilder.ToString();
        }
        private async Task<string> SendApiRequestAsync(string prompt)
        {
            try
            {
                // Create the API request payload
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a search query translator." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 150
                };

                StringContent content = new(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Send the request
                HttpResponseMessage response = await _httpClient.PostAsync(_apiEndpoint, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                // Parse the response
                dynamic? responseObject = JsonConvert.DeserializeObject<dynamic>(responseBody);

                // Extract token usage
                if (responseObject?.usage != null)
                {
                    LastTokenUsage = responseObject.usage.total_tokens;
                    Log.Write(2, $"Token usage for query translation: {LastTokenUsage}");
                }
                else
                {
                    LastTokenUsage = 0;
                }

                return responseObject.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                Log.Error_TranslationAPIRequestFailed(ex.Message);
                LastTokenUsage = 0;
                throw;
            }
        }
        private static string ParseResponse(string response)
        {
            // Extract just the query part (strip explanations)
            string[] lines = response.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            // Look for the first line that contains a likely search query
            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // Heuristics to identify the actual translation:
                // - Contains search syntax characters (+, -, :, >, <)
                // - Not too long
                // - Doesn't contain full sentences
                if ((trimmed.Contains('+') || trimmed.Contains('-') ||
                     trimmed.Contains(':') || trimmed.Contains('>') ||
                     trimmed.Contains('<')) &&
                    trimmed.Length < 100 &&
                    !trimmed.EndsWith('.'))
                {
                    return trimmed;
                }
            }

            // If we can't find a good translation, return a cleaned-up version of the first non-empty line
            return lines.Length > 0 ? lines[0].Trim() : "";
        }
        private static DataGridViewSchemaInfo BuildSchemaInfo()
        {
            // Create schema information based on your actual data structure
            // This helps the AI understand what fields exist and typical values
            return new()
            {
                Fields =
                [
                    new FieldInfo
                    {
                        Name = "Order #",
                        Description = "Unique identifier for purchases",
                        Type = FieldType.String
                    },
                    new FieldInfo
                    {
                        Name = "Sale #",
                        Description = "Unique identifier for sales",
                        Type = FieldType.String
                    },
                    new FieldInfo
                    {
                        Name = "Accountant",
                        Description = "Name of the accountant handling the transaction",
                        Type = FieldType.String,
                    },
                    new FieldInfo
                    {
                        Name = "Product",
                        Description = "Name of the product",
                        Type = FieldType.String
                    },
                    new FieldInfo
                    {
                        Name = "Category",
                        Description = "Product category",
                        Type = FieldType.String,
                    },
                    new FieldInfo
                    {
                        Name = "Country of origin",
                        Description = "Country of origin or destination",
                        Type = FieldType.String,
                        CommonValues = GetAllCountryNames()
                    },
                    new FieldInfo
                    {
                        Name = "Country of destination",
                        Description = "Country where the product is sent to (for sales)",
                        Type = FieldType.String,
                        CommonValues = GetAllCountryNames()
                    },
                    new FieldInfo
                    {
                        Name = "Company of origin",
                        Description = "Company name",
                        Type = FieldType.String,
                    },
                    new FieldInfo
                    {
                        Name = "Date",
                        Description = "Transaction date",
                        Type = FieldType.Date
                    },
                    new FieldInfo
                    {
                        Name = "TotalItems",
                        Description = "Number of items in the transaction",
                        Type = FieldType.Number,
                    },
                    new FieldInfo
                    {
                        Name = "PricePerUnit",
                        Description = "Price per unit of product",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "Shipping",
                        Description = "Shipping cost",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "Tax",
                        Description = "Tax amount",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "Fee",
                        Description = "Additional fees",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "Discount",
                        Description = "Discount amount",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "ChargedDifference",
                        Description = "Difference between expected and actual charged amount",
                        Type = FieldType.Currency,
                    },
                    new FieldInfo
                    {
                        Name = "Total",
                        Description = "Total amount for the transaction (revenue for sales, expenses for purchases)",
                        Type = FieldType.Currency,
                    }
                ]
            };
        }
        private static List<string> GetAllCountryNames()
        {
            return Country.CountrySearchResults
                .Where(c => c.DisplayName != SearchBox.AddLine && c.DisplayName != "Unkown")
                .Select(c => c.DisplayName)
                .OrderBy(s => s)
                .ToList();
        }
    }

    /// <summary>
    /// Supporting classes for the AI Query Translator.
    /// </summary>
    public class DataGridViewSchemaInfo
    {
        public List<FieldInfo> Fields { get; set; } = [];
    }
    public class FieldInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public FieldType Type { get; set; }
        public List<string> CommonValues { get; set; } = [];
    }
    public enum FieldType
    {
        String,
        Number,
        Currency,
        Date,
        Boolean
    }
}