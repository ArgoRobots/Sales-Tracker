using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using System.IO;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public class LanguageManager
    {
        private static readonly HttpClient httpClient = new();
        private static readonly string apiKey = "4e5f9ad96540482591a49028553e146c";
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
        private static Dictionary<string, Dictionary<string, string>> translationCache;  // language -> text -> translation

        public static void InitLanguageManager()
        {
            // Initialize translationCache with an empty dictionary
            translationCache = new Dictionary<string, Dictionary<string, string>>();

            if (File.Exists(Directories.Translations_file))
            {
                string cacheContent = File.ReadAllText(Directories.Translations_file);

                if (!string.IsNullOrWhiteSpace(cacheContent))
                {
                    Dictionary<string, Dictionary<string, string>>? deserialized =
                        JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(cacheContent);
                    if (deserialized != null)
                    {
                        translationCache = deserialized;
                    }
                }
            }
        }
        /// <summary>
        /// Translates text. It uses cache if available, or gets it from Microsoft Translator.
        /// </summary>
        public static string? TranslateText(string text, string targetLanguage)
        {
            if (text == "") { return null; }

            // Check if translation is already cached
            if (translationCache.TryGetValue(targetLanguage, out Dictionary<string, string>? translations) &&
                translations.TryGetValue(text, out string? cachedTranslation))
            {
                return cachedTranslation;
            }

            try
            {
                // Translation not found, call the API
                var body = new[] { new { Text = text } };
                StringContent requestContent = new(Newtonsoft.Json.JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");

                // Add necessary headers for the API request
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "canadacentral");

                HttpResponseMessage response = httpClient.PostAsync($"{translationEndpoint}&to={targetLanguage}", requestContent).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error_GetTranslation($"{response.StatusCode}: {response.ReasonPhrase}.");
                    return null;
                }

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                string translatedText = result[0].translations[0].text;

                // Cache the translation
                if (!translationCache.TryGetValue(targetLanguage, out Dictionary<string, string>? value))
                {
                    value = new Dictionary<string, string>();
                    translationCache[targetLanguage] = value;
                }

                value[text] = translatedText;

                SaveCacheToFile();

                return translatedText;
            }
            catch
            {
                Log.Error_GetTranslation("");
            }

            return null;
        }
        private static void SaveCacheToFile()
        {
            string jsonContent = JsonSerializer.Serialize(translationCache, MainMenu_Form.jsonOptions);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            File.WriteAllText(Directories.Translations_file, jsonContent);
        }

        /// <summary>
        /// Update all controls' text with the translated language
        /// </summary>
        public static void UpdateLanguage(Control control, string targetLanguage)
        {
            foreach (Control ctrl in control.Controls)
            {
                switch (ctrl)
                {
                    case Guna2Button guna2Button:
                    case Label label:
                        ctrl.Text = TranslateText(ctrl.Text, targetLanguage);
                        break;

                    case Guna2TextBox guna2TextBox:
                        guna2TextBox.Text = TranslateText(guna2TextBox.Text, targetLanguage);
                        guna2TextBox.PlaceholderText = TranslateText(guna2TextBox.PlaceholderText, targetLanguage);
                        break;

                    case Guna2ComboBox guna2ComboBox:
                        if (guna2ComboBox.DataSource == null)
                        {
                            // Handle the case where DataSource is not set (modifying Items directly)
                            int selectedIndex = guna2ComboBox.SelectedIndex;

                            List<object> translatedItems = new();
                            foreach (object item in guna2ComboBox.Items)
                            {
                                translatedItems.Add(TranslateText(item.ToString(), targetLanguage));
                            }

                            guna2ComboBox.Items.Clear();
                            guna2ComboBox.Items.AddRange(translatedItems.ToArray());

                            // Restore SelectedIndex
                            if (selectedIndex >= 0 && selectedIndex < guna2ComboBox.Items.Count)
                            {
                                guna2ComboBox.SelectedIndex = selectedIndex;
                            }
                        }
                        break;

                    case GunaChart gunaChart:
                        gunaChart.Title.Text = TranslateText(gunaChart.Title.Text, targetLanguage);
                        break;

                    case Guna2DataGridView gunaDataGridView:
                        foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                        {
                            column.HeaderText = TranslateText(column.HeaderText, targetLanguage);
                        }
                        break;
                }

                // Recursively update child controls
                if (ctrl.HasChildren)
                {
                    UpdateLanguage(ctrl, targetLanguage);
                }
            }
        }
        public static void UpdateLanguage(Control control)
        {
            string fullLanguageName = Properties.Settings.Default.Language;

            string languageAbbreviation = GetLanguages()
                .FirstOrDefault(l => l.Key == fullLanguageName).Value;

            // If the language abbreviation is found, run the async method with the abbreviation
            if (!string.IsNullOrEmpty(languageAbbreviation))
            {
                UpdateLanguage(control, languageAbbreviation);
            }
            else
            {
                Log.Error_GetTranslation($"Language '{fullLanguageName}' not found in the list.");
            }
        }
        public static List<KeyValuePair<string, string>> GetLanguages()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new("English", "en"),        // North America, UK, Australia
                new("French", "fr"),         // France, Canada, Belgium
                new("German", "de"),         // Germany, Austria
                new("Italian", "it"),        // Italy
                new("Spanish", "es"),        // Spain, Latin America
                new("Portuguese", "pt"),     // Portugal, Brazil
                new("Dutch", "nl"),          // Netherlands, Belgium
                new("Swedish", "sv"),        // Sweden
                new("Norwegian", "no"),      // Norway
                new("Danish", "da"),         // Denmark
                new("Finnish", "fi"),        // Finland
                new("Greek", "el"),          // Greece
                new("Russian", "ru"),        // Russia, Eastern Europe
                new("Turkish", "tr"),        // Turkey
                new("Japanese", "ja"),       // Japan
                new("Korean", "ko"),         // South Korea
                new("Chinese (Simplified)", "zh-Hans"),  // Mainland China
                new("Chinese (Traditional)", "zh-Hant")  // Taiwan, Hong Kong
            };
        }
    }
}