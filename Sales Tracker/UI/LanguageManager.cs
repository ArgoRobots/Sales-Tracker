using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using System.Text.Json;

namespace Sales_Tracker.UI
{
    public class LanguageManager
    {
        // Properties
        private static readonly HttpClient httpClient = new();
        private static readonly string apiKey = "4e5f9ad96540482591a49028553e146c";
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
        private static readonly string placeholder_text = "Placeholder", item_text = "Item", title_text = "Title", column_text = "Column",
            before_text = "before", link_text = "link", after_text = "after", full_text = "full";

        private static Dictionary<string, Dictionary<string, string>> translationCache;  // language -> controlKey -> translation
        private static Dictionary<string, string> englishCache;  // controlKey -> originalText
        private static readonly Dictionary<string, Rectangle> controlBoundsCache = [];
        private static readonly Dictionary<Control, float> originalFontSizes = [];

        // Init.
        public static void InitLanguageManager()
        {
            translationCache = [];
            englishCache = [];

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
        /// Update the controls text with the translated language. It also updates all the child controls.
        /// </summary>
        public static void UpdateLanguageForControl(Control control, bool cacheControlAgain = false)
        {
            string targetLanguageAbbreviation = GetDefaultLanguageAbbreviation();

            // Ensure all the English text in this Form has been cached
            if (CacheAllEnglishTextInForm(control, cacheControlAgain))
            {
                SaveEnglishCacheToFile();
            }

            TranslateAllTextInControl(control, targetLanguageAbbreviation);
            SaveCacheToFile();
        }
        public static void TranslateAllTextInControl(Control control, string targetLanguageAbbreviation)
        {
            CacheControlBounds(control);

            string controlKey = GetControlKey(control);

            switch (control)
            {
                case LinkLabel linkLabel:
                    TranslateLinkLabel(linkLabel, targetLanguageAbbreviation);
                    AdjustLabelSizeAndPosition(linkLabel);
                    break;

                case Label label:
                    label.Text = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, control, label.Text);
                    AdjustLabelSizeAndPosition(label);
                    break;

                case Guna2Button guna2Button:
                    string newText = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, control, guna2Button.Text);
                    AdjustButtonFontSize(guna2Button, newText);
                    break;

                case Guna2TextBox guna2TextBox:
                    guna2TextBox.Text = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, control, guna2TextBox.Text);
                    string placeholderKey = $"{controlKey}_{placeholder_text}";
                    guna2TextBox.PlaceholderText = TranslateAndCacheText(targetLanguageAbbreviation, placeholderKey, control, guna2TextBox.PlaceholderText);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        int selectedIndex = guna2ComboBox.SelectedIndex;

                        List<object> translatedItems = [];
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            translatedItems.Add(TranslateAndCacheText(targetLanguageAbbreviation, itemKey, control, guna2ComboBox.Items[i].ToString()));
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
                    gunaChart.Title.Text = TranslateAndCacheText(targetLanguageAbbreviation, $"{controlKey}_{title_text}", control, gunaChart.Title.Text);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        column.HeaderText = TranslateAndCacheText(targetLanguageAbbreviation, columnKey, control, column.HeaderText);
                    }
                    break;
            }

            // Recursively update child controls
            foreach (Control childControl in control.Controls)
            {
                TranslateAllTextInControl(childControl, targetLanguageAbbreviation);
            }
        }
        /// <summary>
        /// Translates text using cache or Microsoft Translator API.
        /// </summary>
        private static string? TranslateAndCacheText(string targetLanguageAbbreviation, string controlKey, Control control, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (!CanControlTranslate(control)) { return text; }

            bool canCache = CanControlCache(control);

            // Get the cached translation for this control if it already exists
            if (targetLanguageAbbreviation != "en" && canCache &&
                translationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(controlKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            // Get the cached English for this control so it can be translated without back-translation errors
            string englishText = englishCache.TryGetValue(controlKey, out string? value) ? value : null;
            if (canCache && englishText == null)
            {
                Log.Error_EnglishCacheDoesNotExist(controlKey);
                return null;
            }
            else if (targetLanguageAbbreviation == "en" && canCache)
            {
                return englishText;
            }
            else if (targetLanguageAbbreviation == "en")
            {
                return text;
            }
            else
            {
                englishText = text;
            }

            // Call the API to translate the text
            try
            {
                var body = new[] { new { Text = englishText } };
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
                    return englishText;  // Return original text if translation fails
                }

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                string translatedText = result[0].translations[0].text;

                // Cache the translation
                if (canCache)
                {
                    if (!translationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlDict))
                    {
                        controlDict = [];
                        translationCache[targetLanguageAbbreviation] = controlDict;
                    }

                    if (!string.IsNullOrEmpty(translatedText))
                    {
                        controlDict[controlKey] = translatedText;
                    }
                }

                return translatedText;
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation(ex.Message);
            }

            return englishText;  // Return original text if translation fails
        }

        // Methods
        private static void AdjustLabelSizeAndPosition(Label label)
        {
            string controlKey = GetControlKey(label);

            if (!controlBoundsCache.TryGetValue(controlKey, out Rectangle originalBounds))
            {
                return;
            }

            AdjustLabelPosition(label, originalBounds);
        }
        private static void AdjustLabelPosition(Label label, Rectangle originalBounds)
        {
            if (label.Anchor.HasFlag(AnchorStyles.Right))
            {
                label.Left = originalBounds.Right - label.Width;
            }
            else if (label.Anchor == AnchorStyles.Top || label.Anchor == AnchorStyles.Bottom)
            {
                if (label.AccessibleDescription == AccessibleDescriptionStrings.AlignRightCenter)
                {
                    label.Left = originalBounds.Right - label.Width;
                }
                else if (label.AccessibleDescription != AccessibleDescriptionStrings.AlignLeftCenter)
                {
                    // Center
                    int originalCenterX = originalBounds.Left + originalBounds.Width / 2;
                    label.Left = originalCenterX - label.Width / 2;
                }
                // If it is AlignLeftCenter, do nothing
            }
        }
        private static bool CanControlCache(Control control)
        {
            return control.AccessibleDescription != AccessibleDescriptionStrings.DoNotCache;
        }
        private static bool CanControlTranslate(Control control)
        {
            return control.AccessibleDescription != AccessibleDescriptionStrings.DoNotTranslate;
        }
        private static void TranslateLinkLabel(LinkLabel linkLabel, string targetLanguageAbbreviation)
        {
            // Normalize the fullText by replacing "\r\n" with "\n" to handle both cases
            string fullText = linkLabel.Text.Replace("\r\n", "\n");

            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string linkText = fullText.Substring(linkStart, linkLength).Trim();

                // Extract the text before and after the link
                string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                // Check if the original text contains a new line before the link
                bool hasNewLineBefore = fullText.Substring(0, linkStart).EndsWith('\n');

                // Generate proper control keys for each part
                string controlKeyBefore = GetControlKey(linkLabel, before_text);
                string controlKeyLink = GetControlKey(linkLabel, link_text);
                string controlKeyAfter = GetControlKey(linkLabel, after_text);

                // Translate the text
                string translatedTextBefore = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyBefore, linkLabel, textBeforeLink);
                string translatedLink = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyLink, linkLabel, linkText);
                string translatedTextAfter = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyAfter, linkLabel, textAfterLink);

                // Combine the translated text, adding back the new line before the link if necessary
                string finalText = (hasNewLineBefore ? translatedTextBefore + "\n" : translatedTextBefore) + " " +
                    translatedLink + " " + translatedTextAfter;

                // Set the translated text and preserve the link area
                linkLabel.Text = finalText;
                linkLabel.LinkArea = new LinkArea(translatedTextBefore.Length + 1, translatedLink.Length + 1);
            }
            else
            {
                // Translate and cache the entire text when no link is present
                string controlKeyFull = GetControlKey(linkLabel, full_text);
                string translatedFullText = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyFull, linkLabel, fullText.Trim());

                // Set the translated text
                linkLabel.Text = translatedFullText;
            }
        }
        public static void AdjustButtonFontSize(Guna2Button button, string text)
        {
            if (!originalFontSizes.ContainsKey(button))
            {
                originalFontSizes[button] = button.Font.Size;
            }

            float originalFontSize = originalFontSizes[button];
            float minFontSize = 3.0f;
            float maxFontSize = originalFontSize;

            while (maxFontSize - minFontSize > 0.5f)
            {
                float fontSize = (minFontSize + maxFontSize) / 2;
                button.Font = new Font(button.Font.FontFamily, fontSize, button.Font.Style);
                button.Text = text;

                button.PerformLayout();
                Size preferredSize = button.PreferredSize;

                if (preferredSize.Width <= button.Width && preferredSize.Height <= button.Height)
                {
                    minFontSize = fontSize;  // Try a larger font
                }
                else
                {
                    maxFontSize = fontSize;  // Try a smaller font
                }
            }

            // Set the final font and text
            float finalFontSize = Math.Max(minFontSize, 3.0f);
            button.Font = new Font(button.Font.FontFamily, finalFontSize, button.Font.Style);
            button.Text = text;
        }

        // Cache things
        /// <summary>
        /// Saves all the text in a Control to englishCache.
        /// </summary>
        /// <returns>True if any text was caches, otherwise False.</returns>
        private static bool CacheAllEnglishTextInForm(Control control, bool cacheControlAgain)
        {
            if (!CanControlTranslate(control)) { return false; }

            string controlKey = GetControlKey(control);

            if (englishCache.ContainsKey(controlKey) && !cacheControlAgain)
            {
                return false;  // This control has already been cached
            }

            switch (control)
            {
                case Form form:
                    if (!string.IsNullOrEmpty(form.Text))
                    {
                        englishCache[controlKey] = form.Text;
                    }
                    break;

                case LinkLabel linkLabel:
                    if (!string.IsNullOrEmpty(linkLabel.Text))
                    {
                        // Split the text into parts
                        string fullText = linkLabel.Text.Replace("\r\n", "\n");
                        int linkStart = linkLabel.LinkArea.Start;
                        int linkLength = linkLabel.LinkArea.Length;

                        if (linkLength > 0)
                        {
                            string linkText = fullText.Substring(linkStart, linkLength).Trim();

                            // Extract the text before and after the link
                            string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                            string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                            // Cache each part separately using the same logic as translation
                            englishCache[GetControlKey(linkLabel, before_text)] = textBeforeLink;
                            englishCache[GetControlKey(linkLabel, link_text)] = linkText;
                            englishCache[GetControlKey(linkLabel, after_text)] = textAfterLink;
                        }
                        else
                        {
                            // If there's no link, cache the full text
                            englishCache[GetControlKey(linkLabel, full_text)] = fullText.Trim();
                        }
                    }
                    break;

                case Label label:
                    if (!string.IsNullOrEmpty(label.Text))
                    {
                        englishCache[controlKey] = label.Text;
                    }
                    break;

                case Guna2Button guna2Button:
                    if (!string.IsNullOrEmpty(guna2Button.Text))
                    {
                        englishCache[controlKey] = guna2Button.Text;
                    }
                    break;

                case Guna2TextBox guna2TextBox:
                    if (!string.IsNullOrEmpty(guna2TextBox.Text))
                    {
                        englishCache[controlKey] = guna2TextBox.Text;
                    }
                    if (!string.IsNullOrEmpty(guna2TextBox.PlaceholderText))
                    {
                        string placeholderKey = $"{controlKey}_{placeholder_text}";
                        englishCache[placeholderKey] = guna2TextBox.PlaceholderText;
                    }
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            englishCache[itemKey] = guna2ComboBox.Items[i].ToString();
                        }
                    }
                    break;

                case GunaChart gunaChart:
                    englishCache[$"{controlKey}_{title_text}"] = gunaChart.Title.Text;
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        englishCache[columnKey] = column.HeaderText;
                    }
                    break;
            }

            // Recursively update child controls
            foreach (Control childControl in control.Controls)
            {
                CacheAllEnglishTextInForm(childControl, cacheControlAgain);
            }

            return true;
        }
        private static void CacheControlBounds(Control control)
        {
            if (control is Label)
            {
                string controlKey = GetControlKey(control);
                if (!controlBoundsCache.ContainsKey(controlKey))
                {
                    controlBoundsCache[controlKey] = control.Bounds;
                }
            }
        }

        // Save cache to file
        private static void SaveCacheToFile()
        {
            string jsonContent = JsonSerializer.Serialize(translationCache, ReadOnlyVariables.JsonOptions);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            Directories.WriteTextToFile(Directories.Translations_file, jsonContent);
        }
        private static void SaveEnglishCacheToFile()
        {
            string jsonContent = JsonSerializer.Serialize(englishCache, ReadOnlyVariables.JsonOptions);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            Directories.WriteTextToFile(Directories.EnglishTexts_file, jsonContent);
        }

        // Misc. methods
        private static string GetControlKey(Control control, string section = null)
        {
            List<string> parentNames = [];
            Control currentControl = control;

            // Loop through all parent controls and add their names to the list
            while (currentControl != null)
            {
                if (!string.IsNullOrEmpty(currentControl.Name))
                {
                    parentNames.Insert(0, currentControl.Name);  // Add the parent names in reverse order (top-down)
                }

                // Stop if we encounter a form that is not the current control (a form within a form)
                if (currentControl is Form && currentControl != control)
                {
                    break;
                }

                currentControl = currentControl.Parent;
            }

            // Join the parent names with a period
            string key = string.Join(".", parentNames);

            // Append section (before, link, after) if provided
            if (!string.IsNullOrEmpty(section))
            {
                key += $"_{section}";
            }

            return key;
        }
        public static List<KeyValuePair<string, string>> GetLanguages()
        {
            // Ordered by how western the country is
            return
            [
                new("English", "en"),           // North America, UK, Australia
                new("Irish", "ga"),             // Ireland
                new("French", "fr"),            // France, Canada, Belgium
                new("Spanish", "es"),           // Spain, Latin America
                new("Portuguese", "pt"),        // Portugal, Brazil
                new("Catalan", "ca"),           // Catalonia
                new("Basque", "eu"),            // Basque Country
                new("Galician", "gl"),          // Galicia
                new("Dutch", "nl"),             // Netherlands, Belgium
                new("German", "de"),            // Germany, Austria
                new("Luxembourgish", "lb"),     // Luxembourg
                new("Danish", "da"),            // Denmark
                new("Norwegian", "no"),         // Norway
                new("Swedish", "sv"),           // Sweden
                new("Icelandic", "is"),         // Iceland
                new("Finnish", "fi"),           // Finland
                new("Italian", "it"),           // Italy
                new("Maltese", "mt"),           // Malta
                new("Polish", "pl"),            // Poland
                new("Czech", "cs"),             // Czech Republic
                new("Slovak", "sk"),            // Slovakia
                new("Hungarian", "hu"),         // Hungary
                new("Slovenian", "sl"),         // Slovenia
                new("Croatian", "hr"),          // Croatia
                new("Greek", "el"),             // Greece
                new("Albanian", "sq"),          // Albania
                new("Romanian", "ro"),          // Romania
                new("Bulgarian", "bg"),         // Bulgaria
                new("Serbian", "sr"),           // Serbia
                new("Macedonian", "mk"),        // North Macedonia
                new("Bosnian", "bs"),           // Bosnia and Herzegovina
                new("Estonian", "et"),          // Estonia
                new("Latvian", "lv"),           // Latvia
                new("Lithuanian", "lt"),        // Lithuania
                new("Ukrainian", "uk"),         // Ukraine
                new("Belarusian", "be"),        // Belarus
                new("Russian", "ru"),           // Russia, Eastern Europe
                new("Turkish", "tr"),           // Turkey
                new("Japanese", "ja"),          // Japan
                new("Korean", "ko"),            // South Korea
                new("Chinese (Simplified)", "zh-Hans"),  // Mainland China
                new("Chinese (Traditional)", "zh-Hant")  // Taiwan, Hong Kong
            ];
        }
        public static List<string> GetLanguageNames()
        {
            List<KeyValuePair<string, string>> languages = GetLanguages();
            return languages.Select(kvp => kvp.Key).ToList();
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