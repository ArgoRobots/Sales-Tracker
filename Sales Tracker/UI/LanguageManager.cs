using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using System.Text;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages language translation and caching for user interface controls.
    /// Uses Microsoft Translator API to translate text and maintain language consistency across controls.
    /// Provides caching to optimize translation efficiency and ensures correct text alignment
    /// and font size adjustments post-translation.
    /// </summary>
    public class LanguageManager
    {
        // Properties
        private static readonly HttpClient httpClient = new();
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
        private static readonly string placeholder_text = "Placeholder", item_text = "Item", title_text = "Title", column_text = "Column",
            before_text = "before", link_text = "link", after_text = "after", full_text = "full";
        private static readonly Dictionary<string, Rectangle> controlBoundsCache = [];
        private static readonly Dictionary<Control, float> originalFontSizes = [];
        private static readonly Dictionary<string, Size> formSizeCache = [];

        // Getters and setters
        public static Dictionary<string, Dictionary<string, string>> TranslationCache { get; set; }
        public static Dictionary<string, string> EnglishCache { get; set; }

        // Init.
        public static void InitLanguageManager()
        {
            TranslationCache = [];
            EnglishCache = [];

            // Load translation cache
            if (File.Exists(Directories.TranslationsCache_file))
            {
                string cacheContent = File.ReadAllText(Directories.TranslationsCache_file);

                if (!string.IsNullOrWhiteSpace(cacheContent))
                {
                    Dictionary<string, Dictionary<string, string>>? deserialized =
                         JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(cacheContent);

                    if (deserialized != null)
                    {
                        TranslationCache = deserialized;
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
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(englishContent);

                    if (deserializedEnglish != null)
                    {
                        EnglishCache = deserializedEnglish;
                    }
                }
            }

            string APIKey = DotEnv.Get("MICROSOFT_TRANSLATOR_API_KEY");

            // Add headers for the API request
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIKey);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "canadacentral");
        }

        // Main methods
        /// <summary>
        /// Update the controls text with the translated language. It also updates all the child controls.
        /// </summary>
        public static void UpdateLanguageForControl(Control control, bool cacheControl = true)
        {
            string targetLanguageAbbreviation = GetDefaultLanguageAbbreviation();
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int controlsTranslated = 0;
            int charactersTranslated = 0;
            int cacheHits = 0;
            int totalTranslations = 0;

            bool cachedEnglish = CacheAllEnglishTextInControl(control);

            // Ensure all the English text in this Form has been cached
            if (cacheControl)
            {
                if (cachedEnglish)
                {
                    SaveEnglishCacheToFile();
                }
                SaveCacheToFile();
            }

            TranslateAllTextInControl(control, targetLanguageAbbreviation, ref controlsTranslated,
                ref charactersTranslated, ref cacheHits, ref totalTranslations);

            if (cacheControl)
            {
                // Calculate duration and cache hit percentage
                long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                long duration = endTime - startTime;
                double cacheHitPercentage = totalTranslations > 0 ? (double)cacheHits / totalTranslations * 100 : 0;

                // Log the language data
                Dictionary<LanguageDataField, object> languageData = new()
                {
                    [LanguageDataField.TargetLanguage] = targetLanguageAbbreviation,
                    [LanguageDataField.DurationMS] = duration,
                    [LanguageDataField.CharactersTranslated] = charactersTranslated,
                    [LanguageDataField.ControlsTranslated] = controlsTranslated,
                    [LanguageDataField.CacheHitPercentage] = cacheHitPercentage
                };
                AnonymousDataManager.AddMicrosoftTranslatorData(languageData);
            }
        }

        /// <summary>
        /// Translates all application forms and controls when the language is changed in settings.
        /// </summary>
        public static async Task TranslateAllApplicationFormsAsync(bool includeGeneralForm, CancellationToken cancellationToken = default)
        {
            string targetLanguageAbbreviation = GetDefaultLanguageAbbreviation();
            if (targetLanguageAbbreviation == null) { return; }

            Log.Write(2, "Starting translation of all application forms...");

            List<Control> controlsList = [MainMenu_Form.Instance];

            if (includeGeneralForm)
            {
                controlsList.AddRange(
                [
                    Settings_Form.Instance,
                    General_Form.Instance,
                    Security_Form.Instance,
                    Updates_Form.Instance
                ]);
            }
            if (Tools.IsFormOpen<Log_Form>())
            {
                controlsList.Add(Log_Form.Instance);
            }

            // Add UI panels
            List<Control> panelsList = MainMenu_Form.GetMenus().Cast<Control>().ToList();
            controlsList.AddRange(panelsList);

            // Add other controls
            controlsList.Add(CustomControls.ControlsDropDown_Button);

            List<Task> translationTasks = [];

            foreach (Control form in controlsList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                translationTasks.Add(Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    form?.InvokeIfRequired(() =>
                    {
                        UpdateLanguageForControl(form, false);
                    });
                }, cancellationToken));
            }

            // Wait for all translation tasks to complete
            await Task.WhenAll(translationTasks);

            SaveEnglishCacheToFile();
            SaveCacheToFile();
            MainMenu_Form.Instance.CenterAndResizeControls();

            Log.Write(2, "Completed translation of all application forms");
        }
        private static void TranslateAllTextInControl(Control control, string targetLanguageAbbreviation,
            ref int controlsTranslated, ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            CacheControlBounds(control);

            string controlKey = GetControlKey(control);

            switch (control)
            {
                case Form form:
                    string translatedFormText = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, form, form.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);

                    form.InvokeIfRequired(() =>
                    {
                        form.Text = translatedFormText;
                    });
                    break;

                case LinkLabel linkLabel:
                    TranslateLinkLabel(linkLabel, targetLanguageAbbreviation,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    AdjustLabelSizeAndPosition(linkLabel);
                    break;

                case Label label:
                    string translatedLabelText = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, label, label.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);

                    label.InvokeIfRequired(() =>
                    {
                        label.Text = translatedLabelText;
                        AdjustLabelSizeAndPosition(label);
                    });
                    break;

                case Guna2Button guna2Button:
                    string translatedButtonText = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, guna2Button, guna2Button.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);

                    guna2Button.InvokeIfRequired(() =>
                    {
                        AdjustButtonFontSize(guna2Button, translatedButtonText);
                    });
                    break;

                case Guna2TextBox guna2TextBox:
                    guna2TextBox.Text = TranslateAndCacheText(targetLanguageAbbreviation, controlKey, control, guna2TextBox.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    string placeholderKey = $"{controlKey}_{placeholder_text}";
                    guna2TextBox.PlaceholderText = TranslateAndCacheText(targetLanguageAbbreviation, placeholderKey, control, guna2TextBox.PlaceholderText,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        int selectedIndex = guna2ComboBox.SelectedIndex;

                        List<object> translatedItems = [];
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            translatedItems.Add(TranslateAndCacheText(targetLanguageAbbreviation, itemKey, control, guna2ComboBox.Items[i].ToString(),
                                ref charactersTranslated, ref cacheHits, ref totalTranslations));
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
                    gunaChart.Title.Text = TranslateAndCacheText(targetLanguageAbbreviation, $"{controlKey}_{title_text}", control, gunaChart.Title.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";

                        // Check if the column uses DataGridViewImageHeaderCell
                        if (column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell)
                        {
                            // Translate the HeaderText property
                            string translatedHeaderText = TranslateAndCacheText(targetLanguageAbbreviation, columnKey, control, imageHeaderCell.HeaderText,
                                ref charactersTranslated, ref cacheHits, ref totalTranslations);
                            imageHeaderCell.HeaderText = translatedHeaderText;

                            // Show the updated text
                            gunaDataGridView.Refresh();
                        }
                        else
                        {
                            // Translate regular column headers
                            column.HeaderText = TranslateAndCacheText(targetLanguageAbbreviation, columnKey, control, column.HeaderText,
                                ref charactersTranslated, ref cacheHits, ref totalTranslations);
                        }
                    }
                    break;
            }

            controlsTranslated++;

            foreach (Control childControl in control.Controls)
            {
                TranslateAllTextInControl(childControl, targetLanguageAbbreviation,
                    ref controlsTranslated, ref charactersTranslated, ref cacheHits, ref totalTranslations);
            }
        }

        /// <summary>
        /// Translates the text in a control to the default language using the cache or Microsoft Translator API.
        /// </summary>
        private static string? TranslateAndCacheText(string targetLanguageAbbreviation, string controlKey, Control control, string text,
            ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (!CanControlTranslate(control)) { return text; }

            bool canCache = CanControlCache(control);
            totalTranslations++;

            // Get the cached translation for this control if it already exists
            if (targetLanguageAbbreviation != "en" && canCache &&
                TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(controlKey, out string cachedTranslation))
            {
                cacheHits++;
                return cachedTranslation;
            }

            // Get the cached English for this control so it can be translated without back-translation errors
            string englishText = EnglishCache.TryGetValue(controlKey, out string? value) ? value : null;
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
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
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
                dynamic result = JsonConvert.DeserializeObject(responseBody);
                string translatedText = result[0].translations[0].text;

                // Update character count
                charactersTranslated += englishText.Length;

                // Cache the translation
                if (canCache)
                {
                    if (!TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlDict))
                    {
                        controlDict = [];
                        TranslationCache[targetLanguageAbbreviation] = controlDict;
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

        /// <summary>
        /// Translates a string to the default language using the cache or Microsoft Translator API.
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
            string cacheKey = $"single_string_{text}";

            // Check if we have this translation cached
            if (TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(cacheKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            // Cache the English version if it's not already cached
            if (!EnglishCache.ContainsKey(cacheKey))
            {
                EnglishCache[cacheKey] = text;
                SaveEnglishCacheToFile();
            }

            try
            {
                var body = new[] { new { Text = text } };
                StringContent requestContent = new(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = httpClient
                    .PostAsync($"{translationEndpoint}&to={targetLanguageAbbreviation}", requestContent)
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error_GetTranslation($"{response.StatusCode}: {response.ReasonPhrase}.");
                    return text;
                }

                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic result = JsonConvert.DeserializeObject(responseBody);
                string translatedText = result[0].translations[0].text;

                // Cache the translation
                if (!TranslationCache.TryGetValue(targetLanguageAbbreviation, out controlTranslations))
                {
                    controlTranslations = [];
                    TranslationCache[targetLanguageAbbreviation] = controlTranslations;
                }

                if (!string.IsNullOrEmpty(translatedText))
                {
                    controlTranslations[cacheKey] = translatedText;
                    SaveCacheToFile();
                }

                return translatedText;
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation(ex.Message);
                return text;
            }
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

        /// <summary>
        /// Adjusts label position based on original bounds and current form size.
        /// Accounts for form resizing by calculating scale factors.
        /// </summary>
        private static void AdjustLabelPosition(Label label, Rectangle originalBounds)
        {
            // Find the top-level form containing this label
            Form parentForm = label.FindForm();
            if (parentForm == null) { return; }

            string formKey = GetFormKey(parentForm);

            // Get the original form size when bounds were cached
            if (!formSizeCache.TryGetValue(formKey, out Size originalFormSize))
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
            // For left alignment or anchored left, no adjustment needed as the form handles it
        }
        private static bool CanControlCache(Control control)
        {
            return control.AccessibleDescription != AccessibleDescriptionManager.DoNotCache;
        }
        private static bool CanControlTranslate(Control control)
        {
            return control.AccessibleDescription != AccessibleDescriptionManager.DoNotTranslate;
        }
        private static void TranslateLinkLabel(LinkLabel linkLabel, string targetLanguageAbbreviation,
            ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
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
                string translatedTextBefore = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyBefore, linkLabel, textBeforeLink,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedLink = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyLink, linkLabel, linkText,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedTextAfter = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyAfter, linkLabel, textAfterLink,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);

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
                string translatedFullText = TranslateAndCacheText(targetLanguageAbbreviation, controlKeyFull, linkLabel, fullText.Trim(),
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);

                // Set the translated text
                linkLabel.Text = translatedFullText;
            }
        }
        public static void AdjustButtonFontSize(Guna2Button button, string text)
        {
            button.InvokeIfRequired(() =>
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
            });
        }

        // Cache things
        /// <summary>
        /// Saves all the text in a Control to englishCache.
        /// </summary>
        /// <returns>True if any text was caches, otherwise False.</returns>
        private static bool CacheAllEnglishTextInControl(Control control)
        {
            if (!CanControlTranslate(control)) { return false; }

            string controlKey = GetControlKey(control);

            if (EnglishCache.ContainsKey(controlKey))
            {
                return false;  // This control has already been cached
            }

            switch (control)
            {
                case Form form:
                    if (!string.IsNullOrEmpty(form.Text))
                    {
                        EnglishCache[controlKey] = form.Text;
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
                            EnglishCache[GetControlKey(linkLabel, before_text)] = textBeforeLink;
                            EnglishCache[GetControlKey(linkLabel, link_text)] = linkText;
                            EnglishCache[GetControlKey(linkLabel, after_text)] = textAfterLink;
                        }
                        else
                        {
                            // If there's no link, cache the full text
                            EnglishCache[GetControlKey(linkLabel, full_text)] = fullText.Trim();
                        }
                    }
                    break;

                case Label label:
                    if (!string.IsNullOrEmpty(label.Text))
                    {
                        EnglishCache[controlKey] = label.Text;
                    }
                    break;

                case Guna2Button guna2Button:
                    if (!string.IsNullOrEmpty(guna2Button.Text))
                    {
                        EnglishCache[controlKey] = guna2Button.Text;
                    }
                    break;

                case Guna2TextBox guna2TextBox:
                    if (!string.IsNullOrEmpty(guna2TextBox.Text))
                    {
                        EnglishCache[controlKey] = guna2TextBox.Text;
                    }
                    if (!string.IsNullOrEmpty(guna2TextBox.PlaceholderText))
                    {
                        string placeholderKey = $"{controlKey}_{placeholder_text}";
                        EnglishCache[placeholderKey] = guna2TextBox.PlaceholderText;
                    }
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            EnglishCache[itemKey] = guna2ComboBox.Items[i].ToString();
                        }
                    }
                    break;

                case GunaChart gunaChart:
                    EnglishCache[$"{controlKey}_{title_text}"] = gunaChart.Title.Text;
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";

                        // Check if the column uses DataGridViewImageHeaderCell
                        if (column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell)
                        {
                            // Cache the HeaderText for DataGridViewImageHeaderCell
                            EnglishCache[columnKey] = imageHeaderCell.HeaderText;
                        }
                        else
                        {
                            // Cache the regular DataGridViewColumn HeaderText
                            EnglishCache[columnKey] = column.HeaderText;
                        }
                    }
                    break;
            }

            // Recursively update child controls
            foreach (Control childControl in control.Controls)
            {
                CacheAllEnglishTextInControl(childControl);
            }

            return true;
        }

        /// <summary>
        /// Caches control bounds and form size for labels to enable proper positioning after form resize.
        /// </summary>
        private static void CacheControlBounds(Control control)
        {
            if (control is Label)
            {
                string controlKey = GetControlKey(control);
                if (!controlBoundsCache.ContainsKey(controlKey))
                {
                    controlBoundsCache[controlKey] = control.Bounds;

                    // Also cache the form size when we first cache this control
                    Form parentForm = control.FindForm();
                    if (parentForm != null)
                    {
                        string formKey = GetFormKey(parentForm);
                        if (!formSizeCache.ContainsKey(formKey))
                        {
                            formSizeCache[formKey] = parentForm.ClientSize;
                        }
                    }
                }
            }
        }

        // Save cache to file
        private static void SaveCacheToFile()
        {
            string jsonContent = JsonConvert.SerializeObject(TranslationCache, Formatting.Indented);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            Directories.WriteTextToFile(Directories.TranslationsCache_file, jsonContent);
        }
        private static void SaveEnglishCacheToFile()
        {
            string jsonContent = JsonConvert.SerializeObject(EnglishCache, Formatting.Indented);
            if (!Directory.Exists(Directories.Cache_dir))
            {
                Directories.CreateDirectory(Directories.Cache_dir, false);
            }
            Directories.WriteTextToFile(Directories.EnglishTexts_file, jsonContent);
        }

        // Misc. methods
        public static List<SearchResult> GetLanguageSearchResults()
        {
            // Get all languages from existing method
            List<KeyValuePair<string, string>> allLanguages = GetLanguages();

            // Define priority languages that should appear at the top
            HashSet<string> priorityLanguages =
            [
                "English",
                "French",
                "German",
                "Italian",
            ];

            List<SearchResult> searchResults = [];

            // Add priority languages first
            foreach (string lang in priorityLanguages)
            {
                KeyValuePair<string, string> language = allLanguages.First(l => l.Key == lang);
                searchResults.Add(new SearchResult(language.Key, null, 0));
            }

            // Add line
            searchResults.Add(new SearchResult(SearchBox.addLine, null, 0));

            // Add remaining languages in alphabetical order
            IOrderedEnumerable<KeyValuePair<string, string>> remainingLanguages = allLanguages
                .Where(l => !priorityLanguages.Contains(l.Key))
                .OrderBy(l => l.Key);

            foreach (KeyValuePair<string, string> language in remainingLanguages)
            {
                searchResults.Add(new SearchResult(language.Key, null, 0));
            }

            return searchResults;
        }
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

        /// <summary>
        /// Gets a unique key for a form to cache its size.
        /// </summary>
        private static string GetFormKey(Form form)
        {
            return $"Form_{form.Name ?? form.GetType().Name}";
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
        public static List<string> GetLanguageNames()
        {
            List<KeyValuePair<string, string>> languages = GetLanguages();
            return languages.Select(kvp => kvp.Key).ToList();
        }
        public static string? GetDefaultLanguageAbbreviation()
        {
            string fullLanguageName = Properties.Settings.Default.Language;
            string languageAbbreviation = GetLanguages().FirstOrDefault(l => l.Key == fullLanguageName).Value;

            if (string.IsNullOrEmpty(languageAbbreviation))
            {
                Log.Error_GetTranslation($"Language '{fullLanguageName}' not found in the list.");
                return null;
            }
            else
            {
                return languageAbbreviation;
            }
        }
    }
}