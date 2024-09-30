using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public class LanguageManager
    {
        private static readonly HttpClient httpClient = new();
        private static readonly string apiKey = "4e5f9ad96540482591a49028553e146c";
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
        private static Dictionary<string, Dictionary<string, string>> translationCache;  // language -> controlKey -> translation
        private static Dictionary<string, string> englishCache;  // controlKey -> originalText
        private static readonly Dictionary<string, Rectangle> controlBoundsCache = new();

        public static void InitLanguageManager()
        {
            translationCache = new Dictionary<string, Dictionary<string, string>>();
            englishCache = new Dictionary<string, string>();

            // Load translation cache
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

            // Load English cache
            if (File.Exists(Directories.EnglishTexts_file))
            {
                string englishContent = File.ReadAllText(Directories.EnglishTexts_file);

                if (!string.IsNullOrWhiteSpace(englishContent))
                {
                    Dictionary<string, string>? deserializedEnglish =
                        JsonSerializer.Deserialize<Dictionary<string, string>>(englishContent);
                    if (deserializedEnglish != null)
                    {
                        englishCache = deserializedEnglish;
                    }
                }
            }

            // Add headers for the API request
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "canadacentral");
        }

        // Main methods
        /// <summary>
        /// Update all controls' text with the translated language.
        /// </summary>
        public static void UpdateLanguage(Control control, string targetLanguageAbbreviation = null)
        {
            targetLanguageAbbreviation ??= GetDefaultLanguageAbbreviation();

            // Restore original English texts to prevent back-translation errors
            RestoreEnglishTexts(control);

            // Ensure English texts have been cached before translation
            CacheEnglishTexts(control);
            CacheControlBounds(control);

            foreach (Control ctrl in control.Controls)
            {
                // Check if the control should be skipped
                if (ctrl.AccessibleDescription == AccessibleDescriptionStrings.DoNotTranslate)
                {
                    continue;
                }

                string controlKey = GetControlKey(ctrl);

                switch (ctrl)
                {
                    case Guna2Button guna2Button:
                        guna2Button.Text = TranslateText(ctrl.Text, targetLanguageAbbreviation, controlKey);
                        break;

                    case Label label:
                        label.Text = TranslateText(label.Text, targetLanguageAbbreviation, controlKey);
                        AdjustLabelSizeAndPosition(label);
                        break;

                    case Guna2TextBox guna2TextBox:
                        guna2TextBox.Text = TranslateText(guna2TextBox.Text, targetLanguageAbbreviation, controlKey);
                        guna2TextBox.PlaceholderText = TranslateText(guna2TextBox.PlaceholderText, targetLanguageAbbreviation, controlKey + "_Placeholder");
                        break;

                    case Guna2ComboBox guna2ComboBox:
                        if (guna2ComboBox.DataSource == null)
                        {
                            int selectedIndex = guna2ComboBox.SelectedIndex;

                            List<object> translatedItems = new();
                            for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                            {
                                string itemKey = $"{controlKey}_Item_{i}";
                                translatedItems.Add(TranslateText(guna2ComboBox.Items[i].ToString(), targetLanguageAbbreviation, itemKey));
                            }

                            guna2ComboBox.Items.Clear();
                            guna2ComboBox.Items.AddRange(translatedItems.ToArray());

                            if (selectedIndex >= 0 && selectedIndex < guna2ComboBox.Items.Count)
                            {
                                guna2ComboBox.SelectedIndex = selectedIndex;
                            }
                        }
                        break;

                    case GunaChart gunaChart:
                        gunaChart.Title.Text = TranslateText(gunaChart.Title.Text, targetLanguageAbbreviation, controlKey + "_Title");
                        break;

                    case Guna2DataGridView gunaDataGridView:
                        foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                        {
                            string columnKey = $"{controlKey}_Column_{column.Name}";
                            column.HeaderText = TranslateText(column.HeaderText, targetLanguageAbbreviation, columnKey);
                        }
                        break;
                }

                // Recursively update child controls
                if (ctrl.HasChildren)
                {
                    UpdateLanguage(ctrl, targetLanguageAbbreviation);
                }
            }

            SaveCacheToFile();
        }
        private static void AdjustLabelSizeAndPosition(Label label)
        {
            string controlKey = GetControlKey(label);

            if (!controlBoundsCache.TryGetValue(controlKey, out Rectangle originalBounds))
            {
                return;
            }

            // Measure the new size of the text
            Size textSize = TextRenderer.MeasureText(label.Text, label.Font);

            // Set the label's size
            label.Width = textSize.Width;
            label.Height = textSize.Height;

            // Adjust position
            AdjustLabelPosition(label, originalBounds);
        }

        private static void AdjustLabelPosition(Label label, Rectangle originalBounds)
        {
            if (label.Anchor.HasFlag(AnchorStyles.Right))
            {
                label.Left = originalBounds.Right - label.Width;
            }
            else if (label.Anchor == AnchorStyles.Top)
            {
                // If the control should be moved left


                // If the controls should be centered

            }
        }

        /// <summary>
        /// Translates text using cache or Microsoft Translator API.
        /// </summary>
        private static string? TranslateText(string text, string targetLanguageAbbreviation, string controlKey)
        {
            if (string.IsNullOrEmpty(text)) { return text; }

            // Get the cached translation for this control if it exists
            if (translationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(controlKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            try
            {
                // Translation not found, call the API
                var body = new[] { new { Text = text } };
                StringContent requestContent = new(
                    Newtonsoft.Json.JsonConvert.SerializeObject(body),
                    System.Text.Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = httpClient
                    .PostAsync($"{translationEndpoint}&to={targetLanguageAbbreviation}", requestContent)
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error_GetTranslation($"{response.StatusCode}: {response.ReasonPhrase}.");
                    return text;  // Return original text if translation fails
                }

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                string translatedText = result[0].translations[0].text;

                // Cache the translation
                if (!translationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlDict))
                {
                    controlDict = new();
                    translationCache[targetLanguageAbbreviation] = controlDict;
                }

                if (!string.IsNullOrEmpty(translatedText))
                {
                    controlDict[controlKey] = translatedText;
                }

                SaveCacheToFile();

                return translatedText;
            }
            catch
            {
                Log.Error_GetTranslation("");
            }

            return text;  // Return original text if exception occurs
        }

        // Save cache to file
        private static void SaveCacheToFile()
        {
            string jsonContent = JsonSerializer.Serialize(translationCache, MainMenu_Form.jsonOptions);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            File.WriteAllText(Directories.Translations_file, jsonContent);
        }
        private static void SaveEnglishCacheToFile()
        {
            string jsonContent = JsonSerializer.Serialize(englishCache, MainMenu_Form.jsonOptions);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            File.WriteAllText(Directories.EnglishTexts_file, jsonContent);
        }

        // Cache things
        private static void CacheEnglishTexts(Control control)
        {
            foreach (Control ctrl in control.Controls)
            {
                string controlKey = GetControlKey(ctrl);

                if (!englishCache.ContainsKey(controlKey))
                {
                    englishCache[controlKey] = ctrl.Text;

                    switch (ctrl)
                    {
                        case Guna2Button guna2Button:
                            englishCache[controlKey] = guna2Button.Text;
                            break;

                        case Label label:
                            englishCache[controlKey] = label.Text;
                            break;

                        case Guna2TextBox guna2TextBox:
                            englishCache[controlKey] = guna2TextBox.Text;
                            string placeholderKey = $"{controlKey}_Placeholder";
                            englishCache[placeholderKey] = guna2TextBox.PlaceholderText;
                            break;

                        case Guna2ComboBox guna2ComboBox:
                            if (guna2ComboBox.DataSource == null)
                            {
                                for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                                {
                                    string itemKey = $"{controlKey}_Item_{i}";
                                    englishCache[controlKey] = guna2ComboBox.Items[i].ToString();
                                }
                            }
                            break;

                        case GunaChart gunaChart:
                            englishCache[controlKey] = gunaChart.Title.Text;
                            break;

                        case Guna2DataGridView gunaDataGridView:
                            foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                            {
                                string columnKey = $"{controlKey}_Column_{column.Name}";
                                englishCache[columnKey] = column.HeaderText;
                            }
                            break;
                    }
                }

                if (ctrl.HasChildren)
                {
                    CacheEnglishTexts(ctrl);
                }
            }

            // Save the English texts to file
            SaveEnglishCacheToFile();
        }
        private static void CacheControlBounds(Control control)
        {
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl is Label)
                {
                    string controlKey = GetControlKey(ctrl);
                    if (!controlBoundsCache.ContainsKey(controlKey))
                    {
                        controlBoundsCache[controlKey] = ctrl.Bounds;
                    }

                    if (ctrl.HasChildren)
                    {
                        CacheControlBounds(ctrl);
                    }
                }
            }
        }

        // Misc. methods
        private static void RestoreEnglishTexts(Control control)
        {
            foreach (Control ctrl in control.Controls)
            {
                string controlKey = GetControlKey(ctrl);

                if (englishCache.TryGetValue(controlKey, out string originalText))
                {
                    try
                    {
                        ctrl.Text = originalText;
                    }
                    catch { }
                }

                if (ctrl is Guna2TextBox guna2TextBox)
                {
                    string placeholderKey = $"{controlKey}_Placeholder";
                    if (englishCache.TryGetValue(placeholderKey, out string originalPlaceholder))
                    {
                        guna2TextBox.PlaceholderText = originalPlaceholder;
                    }
                }

                if (ctrl.HasChildren)
                {
                    RestoreEnglishTexts(ctrl);
                }
            }
        }
        private static string GetControlKey(Control ctrl)
        {
            string formName = ctrl.FindForm()?.Name ?? "UnknownForm";
            string controlName = ctrl.Name ?? ctrl.GetHashCode().ToString();
            return $"{formName}.{controlName}";
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
        public static string? GetDefaultLanguageAbbreviation()
        {
            string fullLanguageName = Properties.Settings.Default.Language;

            string defaultLanguageAbbreviation = GetLanguages()
                 .FirstOrDefault(l => l.Key == fullLanguageName).Value;

            if (string.IsNullOrEmpty(defaultLanguageAbbreviation))
            {
                Log.Error_GetTranslation($"Language '{fullLanguageName}' not found in the list.");
                return null;
            }
            else
            {
                return defaultLanguageAbbreviation;
            }
        }
    }
}