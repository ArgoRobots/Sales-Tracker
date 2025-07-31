using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings.Menus;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages language translation and caching for user interface controls.
    /// Downloads pre-translated JSON files from the server and applies them to UI controls.
    /// Provides caching for offline use and ensures correct text alignment and font size adjustments.
    /// </summary>
    public partial class LanguageManager
    {
        // Properties
        private static readonly HttpClient _httpClient = new();
        private static readonly Dictionary<string, Rectangle> _controlBoundsCache = [];
        private static readonly Dictionary<Control, float> _originalFontSizes = [];
        private static readonly Dictionary<string, Size> _formSizeCache = [];

        // Constants
        private static readonly string
            _placeholder_text = "Placeholder",
            _item_text = "Item",
            _column_text = "Column",
            _before_text = "before",
            _link_text = "link",
            _after_text = "after",
            _full_text = "full";

        [GeneratedRegex(@"[^\w{}]")]
        private static partial Regex NonWordCharacters();

        // Getters and setters
        public static Dictionary<string, Dictionary<string, string>> TranslationCache { get; set; }
        public static Dictionary<string, string> EnglishCache { get; set; }

        // Init.
        /// <summary>
        /// Initializes the LanguageManager by loading cached translation data from the translations file.
        /// </summary>
        public static void InitLanguageManager()
        {
            TranslationCache = [];
            EnglishCache = [];

            // Load non-English translation cache
            if (File.Exists(Directories.Translations_file))
            {
                string cacheContent = File.ReadAllText(Directories.Translations_file);

                if (!string.IsNullOrWhiteSpace(cacheContent))
                {
                    dynamic? combinedCache = JsonConvert.DeserializeObject<dynamic>(cacheContent);

                    if (combinedCache?.TranslationCache != null)
                    {
                        TranslationCache = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
                            combinedCache.TranslationCache.ToString());
                    }
                }
            }

            // Load English translations from separate file into separate cache
            if (File.Exists(Directories.English_file))
            {
                string englishContent = File.ReadAllText(Directories.English_file);

                if (!string.IsNullOrWhiteSpace(englishContent))
                {
                    Dictionary<string, string> englishTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(englishContent);

                    if (englishTranslations != null && englishTranslations.Count > 0)
                    {
                        EnglishCache = englishTranslations;
                    }
                }
            }
        }

        /// <summary>
        /// Downloads and merges language JSON for the specified language.
        /// </summary>
        /// <returns>True if successful, false if failed or skipped</returns>
        public static async Task<bool> DownloadAndMergeLanguageJson(string languageName, CancellationToken cancellationToken = default)
        {
            string languageAbbreviation = GetLanguages().FirstOrDefault(l => l.Key == languageName).Value;

            if (string.IsNullOrEmpty(languageAbbreviation))
            {
                Log.WriteWithFormat(1, "Skipping download for language: {0}", languageName);
                return false;
            }

            // Check if language already exists in cache (or English cache for English)
            if (languageAbbreviation == "en")
            {
                if (EnglishCache.Count > 0)
                {
                    Log.WriteWithFormat(1, "Found English language in cache");
                    return true;
                }
            }
            else if (TranslationCache.ContainsKey(languageAbbreviation))
            {
                Log.WriteWithFormat(1, "Found language '{0}' in cache", languageName);
                return true;  // Consider cached language as success
            }

            // Check for internet connection
            if (!await InternetConnectionManager.CheckInternetAndShowMessageAsync("translating language", true))
            {
                Log.Write(1, "Translating language cancelled - no internet connection");
                return false;
            }

            try
            {
                string downloadUrl = $"https://argorobots.com/resources/downloads/languages/{languageAbbreviation}.json";

                Log.WriteWithFormat(1, "Downloading language file from: {0}", downloadUrl);

                HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error_GetTranslation($"Failed to download language file. Status: {response.StatusCode}");
                    return false;
                }

                string jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Dictionary<string, string> downloadedTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                if (downloadedTranslations == null || downloadedTranslations.Count == 0)
                {
                    Log.Write(1, "Downloaded translations are empty or invalid");
                    return false;
                }

                // Handle English separately
                if (languageAbbreviation == "en")
                {
                    // Save to dedicated English file
                    Directories.WriteTextToFile(Directories.English_file, jsonContent);
                    Log.WriteWithFormat(1, "Successfully saved English translations to {0}", Directories.English_file);

                    // Merge into English cache
                    foreach (KeyValuePair<string, string> kvp in downloadedTranslations)
                    {
                        EnglishCache[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Merge downloaded translations with existing cache for other languages
                    if (!TranslationCache.TryGetValue(languageAbbreviation, out Dictionary<string, string> existingTranslations))
                    {
                        existingTranslations = [];
                        TranslationCache[languageAbbreviation] = existingTranslations;
                    }

                    // Merge translations, giving priority to downloaded translations
                    foreach (KeyValuePair<string, string> kvp in downloadedTranslations)
                    {
                        existingTranslations[kvp.Key] = kvp.Value;
                    }
                }

                SaveCacheToFile();
                Log.WriteWithFormat(1, "Successfully merged translations for {0}", languageName);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation($"Failed to download language file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the application's language translation by downloading and merging the language JSON.
        /// </summary>
        /// <returns>True if translation was successful, false if failed (e.g., no internet)</returns>
        public static async Task<bool> UpdateApplicationLanguage(string targetLanguageName, CancellationToken cancellationToken = default)
        {
            bool downloadSuccess = await DownloadAndMergeLanguageJson(targetLanguageName, cancellationToken);

            if (downloadSuccess)
            {
                await ApplyTranslations(targetLanguageName, cancellationToken);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the controls text with the cached translated language. It also updates all the child controls.
        /// </summary>
        public static void UpdateLanguageForControl(Control control)
        {
            TranslateAllTextInControlFromCache(control, GetDefaultLanguageAbbreviation());
        }

        /// <summary>
        /// Applies cached translations to all application forms and controls.
        /// </summary>
        private static async Task ApplyTranslations(string targetLanguageName, CancellationToken cancellationToken = default)
        {
            string targetLanguageAbbreviation = GetDefaultLanguageAbbreviation(targetLanguageName);
            if (targetLanguageAbbreviation == null) { return; }

            Log.WriteWithFormat(2, "Translating application language to {0}...", targetLanguageName);

            List<Control> controlsList = [MainMenu_Form.Instance];

            // Add all currently open forms
            foreach (Form openForm in Application.OpenForms.Cast<Form>())
            {
                if (openForm.IsDisposed || openForm.Disposing)
                {
                    continue;
                }
                controlsList.Add(openForm);
            }

            // Add UI panels
            List<Control> panelsList = MainMenu_Form.GetMenus().Cast<Control>().ToList();
            controlsList.AddRange(panelsList);

            // Add other controls
            controlsList.Add(CustomControls.ControlsDropDown_Button);
            controlsList.AddRange(MainMenu_Form.Instance.GetAnalyticsControls());
            controlsList.AddRange(MainMenu_Form.Instance.GetMainControls());

            // Apply translations to all controls on UI thread
            List<Task> updateTasks = [];

            foreach (Control control in controlsList)
            {
                if (control == null) { continue; }

                cancellationToken.ThrowIfCancellationRequested();

                Tools.EnsureHandleCreated(control);

                TaskCompletionSource<bool> tcs = new();

                control.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        TranslateAllTextInControlFromCache(control, targetLanguageAbbreviation);

                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }));

                updateTasks.Add(tcs.Task);
            }

            // Wait for all UI updates to complete
            await Task.WhenAll(updateTasks);

            SaveCacheToFile();

            // Final UI updates
            if (Tools.IsFormOpen<Log_Form>() && Log_Form.Instance.IsHandleCreated)
            {
                Log_Form.Instance.BeginInvoke(new Action(Log_Form.Instance.SetLogColoringAndTranslate));
            }

            if (Tools.IsFormOpen<General_Form>() && General_Form.Instance.IsHandleCreated)
            {
                General_Form.Instance.BeginInvoke(new Action(General_Form.Instance.PopulateThemeComboBox));
            }

            MainMenu_Form.Instance.BeginInvoke(new Action(MainMenu_Form.Instance.CenterAndResizeControls));
            MainMenu_Form.Instance.BeginInvoke(new Action(MainMenu_Form.Instance.RefreshDataGridViewAndCharts));
        }

        /// <summary>
        /// Translates all text in control using only cached translations.
        /// </summary>
        private static void TranslateAllTextInControlFromCache(Control control, string targetLanguageAbbreviation)
        {
            CacheControlBounds(control);
            string controlKey = GetControlKey(control);

            foreach (Control childControl in control.Controls)
            {
                TranslateAllTextInControlFromCache(childControl, targetLanguageAbbreviation);
            }

            if (control.AccessibleDescription == AccessibleDescriptionManager.DoNotTranslate)
            {
                return;
            }

            switch (control)
            {
                case LinkLabel linkLabel:
                    TranslateLinkLabelFromCache(linkLabel, targetLanguageAbbreviation);
                    AdjustLabelPosition(linkLabel);
                    break;

                case Label label:
                    string translatedLabelText = GetCachedTranslation(targetLanguageAbbreviation, controlKey, label.Text);
                    label.InvokeIfRequired(() =>
                    {
                        label.Text = translatedLabelText;
                        AdjustLabelPosition(label);
                    });
                    break;

                case Guna2Button guna2Button:
                    string translatedButtonText = GetCachedTranslation(targetLanguageAbbreviation, controlKey, guna2Button.Text);
                    guna2Button.InvokeIfRequired(() =>
                    {
                        AdjustButtonFontSize(guna2Button, translatedButtonText);
                    });
                    break;

                case RichTextBox textBox:
                    textBox.Text = GetCachedTranslation(targetLanguageAbbreviation, controlKey, textBox.Text);
                    break;

                case Guna2TextBox guna2TextBox:
                    guna2TextBox.Text = GetCachedTranslation(targetLanguageAbbreviation, controlKey, guna2TextBox.Text);
                    string placeholderKey = $"{controlKey}_{_placeholder_text}";
                    guna2TextBox.PlaceholderText = GetCachedTranslation(targetLanguageAbbreviation, placeholderKey, guna2TextBox.PlaceholderText);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        int selectedIndex = guna2ComboBox.SelectedIndex;
                        List<object> translatedItems = [];
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{_item_text}_{i}";
                            translatedItems.Add(GetCachedTranslation(targetLanguageAbbreviation, itemKey, guna2ComboBox.Items[i].ToString()));
                        }
                        guna2ComboBox.Items.Clear();
                        guna2ComboBox.Items.AddRange(translatedItems.ToArray());
                        if (selectedIndex >= 0 && selectedIndex < guna2ComboBox.Items.Count)
                        {
                            guna2ComboBox.SelectedIndex = selectedIndex;
                        }
                    }
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{_column_text}_{column.Name}";
                        if (column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell)
                        {
                            string translatedHeaderText = GetCachedTranslation(targetLanguageAbbreviation, columnKey, imageHeaderCell.HeaderText);
                            imageHeaderCell.HeaderText = translatedHeaderText;
                        }
                        else
                        {
                            column.HeaderText = GetCachedTranslation(targetLanguageAbbreviation, columnKey, column.HeaderText);
                        }
                    }
                    gunaDataGridView.Refresh();
                    break;
            }
        }
        public static string GetCachedTranslation(string targetLanguageAbbreviation, string controlKey, string originalText)
        {
            if (string.IsNullOrEmpty(originalText))
            {
                return originalText;
            }

            // Handle English separately
            if (targetLanguageAbbreviation == "en")
            {
                // Try control-specific translation first
                if (EnglishCache.TryGetValue(controlKey, out string cachedTranslation))
                {
                    return cachedTranslation;
                }

                // Try string-based translation
                string stringKey = GetStringKey(originalText);
                if (EnglishCache.TryGetValue(stringKey, out string stringTranslation))
                {
                    return stringTranslation;
                }
            }
            else
            {
                // Handle other languages
                if (TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations))
                {
                    // Try control-specific translation first
                    if (controlTranslations.TryGetValue(controlKey, out string cachedTranslation))
                    {
                        return cachedTranslation;
                    }

                    // Try string-based translation
                    string stringKey = GetStringKey(originalText);
                    if (controlTranslations.TryGetValue(stringKey, out string stringTranslation))
                    {
                        return stringTranslation;
                    }
                }
            }

            // If no cached translation is found, return original
            return originalText;
        }
        private static void TranslateLinkLabelFromCache(LinkLabel linkLabel, string targetLanguageAbbreviation)
        {
            string fullText = linkLabel.Text.Replace("\r\n", "\n");
            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string controlKeyBefore = GetControlKey(linkLabel, _before_text);
                string controlKeyLink = GetControlKey(linkLabel, _link_text);
                string controlKeyAfter = GetControlKey(linkLabel, _after_text);

                string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                string linkText = fullText.Substring(linkStart, linkLength).Trim();
                string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                string translatedTextBefore = GetCachedTranslation(targetLanguageAbbreviation, controlKeyBefore, textBeforeLink);
                string translatedLink = GetCachedTranslation(targetLanguageAbbreviation, controlKeyLink, linkText);
                string translatedTextAfter = GetCachedTranslation(targetLanguageAbbreviation, controlKeyAfter, textAfterLink);

                bool hasNewLineBefore = fullText.Substring(0, linkStart).EndsWith('\n');
                string finalText = (hasNewLineBefore ? translatedTextBefore + "\n" : translatedTextBefore) + " " +
                    translatedLink + " " + translatedTextAfter;

                linkLabel.Text = finalText;
                linkLabel.LinkArea = new LinkArea(translatedTextBefore.Length + 1, translatedLink.Length + 1);
            }
            else
            {
                string controlKeyFull = GetControlKey(linkLabel, _full_text);
                string translatedFullText = GetCachedTranslation(targetLanguageAbbreviation, controlKeyFull, fullText.Trim());
                linkLabel.Text = translatedFullText;
            }
        }

        /// <summary>
        /// Translates a string using cached translations. 
        /// The string also needs to be added to TranslationGenerator.CollectStringsToTranslate().
        /// </summary>
        public static string TranslateString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string targetLanguageAbbreviation = GetDefaultLanguageAbbreviation();
            if (targetLanguageAbbreviation == "en")
            {
                return text;
            }

            // Use the text itself as the cache key
            string cacheKey = GetStringKey(text);

            // Check if we have this translation cached
            if (TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(cacheKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            // If no cached translation available, return original text
            return text;
        }

        // Helper methods for UI adjustments
        /// <summary>
        /// Adjusts label position based on original bounds and current form size.
        /// Accounts for form resizing by calculating scale factors.
        /// </summary>
        private static void AdjustLabelPosition(Label label)
        {
            string controlKey = GetControlKey(label);

            if (!_controlBoundsCache.TryGetValue(controlKey, out Rectangle originalBounds))
            {
                return;
            }

            // Find the top-level form containing this label
            Form parentForm = label.FindForm();
            if (parentForm == null) { return; }

            string formKey = GetFormKey(parentForm);

            // Get the original form size when bounds were cached
            if (!_formSizeCache.TryGetValue(formKey, out Size originalFormSize))
            {
                return;
            }

            // Calculate scaling factors based on form size changes
            double scaleX = (double)parentForm.ClientSize.Width / originalFormSize.Width;
            double scaleY = (double)parentForm.ClientSize.Height / originalFormSize.Height;

            // Scale the original bounds to match current form size
            Rectangle scaledBounds = new(
                (int)(originalBounds.X * scaleX),
                (int)(originalBounds.Y * scaleY),
                (int)(originalBounds.Width * scaleX),
                (int)(originalBounds.Height * scaleY)
            );

            // Apply positioning based on alignment
            if (label.AccessibleDescription == AccessibleDescriptionManager.AlignRight)
            {
                label.Left = scaledBounds.Right - label.Width;
            }
            else if (label.AccessibleDescription != AccessibleDescriptionManager.AlignLeft
                && !label.Anchor.HasFlag(AnchorStyles.Left))
            {
                // Center alignment
                int scaledCenterX = scaledBounds.Left + scaledBounds.Width / 2;
                label.Left = scaledCenterX - label.Width / 2;
            }
            // For left alignment or anchored left, no adjustment is needed as the form handles it
        }
        public static void AdjustButtonFontSize(Guna2Button button, string text)
        {
            if (!_originalFontSizes.ContainsKey(button))
            {
                _originalFontSizes[button] = button.Font.Size;
            }

            float originalFontSize = _originalFontSizes[button];
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
        private static void CacheControlBounds(Control control)
        {
            if (control is not Label)
            {
                return;
            }

            // Skip caching for controls marked as DoNotCache
            if (control.AccessibleDescription == AccessibleDescriptionManager.DoNotCache)
            {
                return;
            }

            string controlKey = GetControlKey(control);
            if (!_controlBoundsCache.ContainsKey(controlKey))
            {
                _controlBoundsCache[controlKey] = control.Bounds;

                // Also cache the form size
                Form parentForm = control.FindForm();
                if (parentForm != null)
                {
                    string formKey = GetFormKey(parentForm);
                    if (!_formSizeCache.ContainsKey(formKey))
                    {
                        _formSizeCache[formKey] = parentForm.ClientSize;
                    }
                }
            }
        }
        private static void SaveCacheToFile()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(TranslationCache, Formatting.Indented);
                Directories.WriteTextToFile(Directories.Translations_file, jsonContent);
            }
            catch (Exception ex)
            {
                Log.Error_SaveTranslationCache(ex.Message);
            }
        }

        // Misc. methods
        public static List<SearchResult> GetLanguageSearchResults()
        {
            List<KeyValuePair<string, string>> allLanguages = GetLanguages();
            List<SearchResult> searchResults = [];

            // Priority languages that appear at the top
            HashSet<string> priorityLanguages =
            [
                "English",
                "French",
                "German",
                "Italian"
            ];

            // Add priority languages first
            foreach (string lang in priorityLanguages)
            {
                KeyValuePair<string, string> language = allLanguages.First(l => l.Key == lang);
                searchResults.Add(new SearchResult(language.Key, null, 0));
            }

            // Add line
            searchResults.Add(new SearchResult(SearchBox.AddLine, null, 0));

            // Add remaining languages
            IEnumerable<KeyValuePair<string, string>> remainingLanguages = allLanguages
                .Where(l => !priorityLanguages.Contains(l.Key));

            foreach (KeyValuePair<string, string> language in remainingLanguages)
            {
                searchResults.Add(new SearchResult(language.Key, null, 0));
            }

            return searchResults;
        }

        /// <summary>
        /// Gets a list of supported languages with their corresponding ISO language codes, sorted alphabetically.
        /// </summary>
        public static List<KeyValuePair<string, string>> GetLanguages()
        {
            return
            [
                new("Albanian", "sq"),          // Albania
                new("Basque", "eu"),            // Basque Country
                new("Belarusian", "be"),        // Belarus
                new("Bosnian", "bs"),           // Bosnia and Herzegovina
                new("Bulgarian", "bg"),         // Bulgaria
                new("Catalan", "ca"),           // Catalonia
                new("Chinese (Simplified)", "zh-Hans"),  // Mainland China
                new("Chinese (Traditional)", "zh-Hant"), // Taiwan, Hong Kong
                new("Croatian", "hr"),          // Croatia
                new("Czech", "cs"),             // Czech Republic
                new("Danish", "da"),            // Denmark
                new("Dutch", "nl"),             // Netherlands, Belgium
                new("English", "en"),           // North America, UK, Australia
                new("Estonian", "et"),          // Estonia
                new("Finnish", "fi"),           // Finland
                new("French", "fr"),            // France, Canada, Belgium
                new("Galician", "gl"),          // Galicia
                new("German", "de"),            // Germany, Austria
                new("Greek", "el"),             // Greece
                new("Hungarian", "hu"),         // Hungary
                new("Icelandic", "is"),         // Iceland
                new("Irish", "ga"),             // Ireland
                new("Italian", "it"),           // Italy
                new("Japanese", "ja"),          // Japan
                new("Korean", "ko"),            // South Korea
                new("Latvian", "lv"),           // Latvia
                new("Lithuanian", "lt"),        // Lithuania
                new("Luxembourgish", "lb"),     // Luxembourg
                new("Macedonian", "mk"),        // North Macedonia
                new("Maltese", "mt"),           // Malta
                new("Norwegian", "no"),         // Norway
                new("Polish", "pl"),            // Poland
                new("Portuguese", "pt"),        // Portugal, Brazil
                new("Romanian", "ro"),          // Romania
                new("Russian", "ru"),           // Russia, Eastern Europe
                new("Serbian", "sr"),           // Serbia
                new("Slovak", "sk"),            // Slovakia
                new("Slovenian", "sl"),         // Slovenia
                new("Spanish", "es"),           // Spain, Latin America
                new("Swedish", "sv"),           // Sweden
                new("Turkish", "tr"),           // Turkey
                new("Ukrainian", "uk"),         // Ukraine
            ];
        }
        public static string? GetDefaultLanguageAbbreviation(string targetLanguageName = null)
        {
            targetLanguageName ??= Properties.Settings.Default.Language;

            string languageAbbreviation = GetLanguages().FirstOrDefault(l => l.Key == targetLanguageName).Value;

            if (string.IsNullOrEmpty(languageAbbreviation))
            {
                Log.Error_GetTranslation($"Language '{targetLanguageName}' not found in the list.");
                return null;
            }
            else
            {
                return languageAbbreviation;
            }
        }

        // Get keys
        /// <summary>
        /// Gets a unique key for a control.
        /// </summary>
        public static string GetControlKey(Control control, string section = null)
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

        /// <summary>
        /// Gets a unique key for a form to cache its size.
        /// </summary>
        private static string GetFormKey(Form form)
        {
            return $"Form_{form.Name ?? form.GetType().Name}";
        }

        /// <summary>
        /// Gets a unique key for a string using itself.
        /// </summary>
        public static string GetStringKey(string text)
        {
            // Capitalize first letter of each word
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string titleCaseText = textInfo.ToTitleCase(text.ToLower());

            // Remove spaces, punctuation, and special characters for key
            string finalText = NonWordCharacters().Replace(titleCaseText, "");
            return $"single_string_{finalText}";
        }
    }
}