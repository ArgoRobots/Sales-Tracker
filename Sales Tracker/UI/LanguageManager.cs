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
    /// and font size adjustments post-translation. Implements request batching and rate limiting.
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

        // Rate limiting and batching
        private static readonly int MAX_BATCH_SIZE = 25; // Microsoft Translator supports up to 100, but we'll be conservative
        private static readonly int MAX_REQUESTS_PER_MINUTE = 100;
        private static readonly int REQUEST_DELAY_MS = 50; // Delay between batches
        private static readonly SemaphoreSlim rateLimiter = new(1, 1);
        private static readonly Queue<DateTime> requestTimestamps = new();

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
        /// This is the synchronous version for backward compatibility.
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

            // Use only cached translations
            TranslateAllTextInControlFromCache(control, targetLanguageAbbreviation, ref controlsTranslated,
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

            try
            {
                // First, ensure all English text is cached
                Log.Write(2, "Caching English text...");
                foreach (Control control in controlsList)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CacheAllEnglishTextInControl(control);
                }
                SaveEnglishCacheToFile();

                // If target language is English, just apply from cache
                if (targetLanguageAbbreviation == "en")
                {
                    Log.Write(0, "Target language is English, applying from cache...");
                    foreach (Control form in controlsList)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (form.IsHandleCreated)
                        {
                            form.BeginInvoke(new Action(() =>
                            {
                                int controlsTranslated = 0;
                                int charactersTranslated = 0;
                                int cacheHits = 0;
                                int totalTranslations = 0;

                                TranslateAllTextInControlFromCache(form, targetLanguageAbbreviation,
                                    ref controlsTranslated, ref charactersTranslated, ref cacheHits, ref totalTranslations);
                            }));
                        }
                    }

                    // Wait a bit for UI updates to complete
                    await Task.Delay(100, cancellationToken);
                }
                else
                {
                    // Collect all texts from all controls that need translation
                    Dictionary<string, string> allTextsToTranslate = [];
                    foreach (Control control in controlsList)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        CollectTextsToTranslate(control, targetLanguageAbbreviation, allTextsToTranslate);
                    }

                    // Batch translate all texts
                    Dictionary<string, string> translations = [];
                    if (allTextsToTranslate.Count > 0)
                    {
                        Log.Write(0, "Starting batch translation...");
                        translations = await BatchTranslateTexts(allTextsToTranslate, targetLanguageAbbreviation, cancellationToken).ConfigureAwait(false);
                        Log.Write(0, $"Batch translation completed with {translations.Count} translations");
                    }

                    // Apply translations to all controls on UI thread
                    // Use a single UI update for all controls
                    List<Task> updateTasks = [];

                    foreach (Control form in controlsList)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (form.IsHandleCreated)
                        {
                            TaskCompletionSource<bool> tcs = new();

                            form.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    int controlsTranslated = 0;
                                    int charactersTranslated = 0;
                                    int cacheHits = 0;
                                    int totalTranslations = 0;

                                    ApplyTranslationsToControl(form, targetLanguageAbbreviation, translations,
                                        ref controlsTranslated, ref charactersTranslated, ref cacheHits, ref totalTranslations);

                                    tcs.SetResult(true);
                                }
                                catch (Exception ex)
                                {
                                    tcs.SetException(ex);
                                }
                            }));

                            updateTasks.Add(tcs.Task);
                        }
                    }

                    // Wait for all UI updates to complete
                    await Task.WhenAll(updateTasks);
                }

                SaveCacheToFile();

                // Final UI updates
                if (Tools.IsFormOpen<Log_Form>() && Log_Form.Instance.IsHandleCreated)
                {
                    Log_Form.Instance.BeginInvoke(new Action(() =>
                    {
                        Log_Form.Instance.RefreshLogColoring();
                    }));
                }

                if (MainMenu_Form.Instance.IsHandleCreated)
                {
                    MainMenu_Form.Instance.BeginInvoke(new Action(() =>
                    {
                        MainMenu_Form.Instance.CenterAndResizeControls();
                    }));
                }

                Log.Write(2, "Completed translation of all application forms");
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Translates all text in control using only cached translations (no API calls).
        /// </summary>
        private static void TranslateAllTextInControlFromCache(Control control, string targetLanguageAbbreviation,
            ref int controlsTranslated, ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            CacheControlBounds(control);

            string controlKey = GetControlKey(control);

            switch (control)
            {
                case Form form:
                    string translatedFormText = GetCachedTranslation(targetLanguageAbbreviation, controlKey, form.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    form.InvokeIfRequired(() => form.Text = translatedFormText);
                    break;

                case LinkLabel linkLabel:
                    TranslateLinkLabelFromCache(linkLabel, targetLanguageAbbreviation,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    AdjustLabelSizeAndPosition(linkLabel);
                    break;

                case Label label:
                    string translatedLabelText = GetCachedTranslation(targetLanguageAbbreviation, controlKey, label.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    label.InvokeIfRequired(() =>
                    {
                        label.Text = translatedLabelText;
                        AdjustLabelSizeAndPosition(label);
                    });
                    break;

                case Guna2Button guna2Button:
                    string translatedButtonText = GetCachedTranslation(targetLanguageAbbreviation, controlKey, guna2Button.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    guna2Button.InvokeIfRequired(() =>
                    {
                        AdjustButtonFontSize(guna2Button, translatedButtonText);
                    });
                    break;

                case RichTextBox textBox:
                    textBox.Text = GetCachedTranslation(targetLanguageAbbreviation, controlKey, textBox.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2TextBox guna2TextBox:
                    guna2TextBox.Text = GetCachedTranslation(targetLanguageAbbreviation, controlKey, guna2TextBox.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    string placeholderKey = $"{controlKey}_{placeholder_text}";
                    guna2TextBox.PlaceholderText = GetCachedTranslation(targetLanguageAbbreviation, placeholderKey, guna2TextBox.PlaceholderText,
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
                            translatedItems.Add(GetCachedTranslation(targetLanguageAbbreviation, itemKey, guna2ComboBox.Items[i].ToString(),
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
                    gunaChart.Title.Text = GetCachedTranslation(targetLanguageAbbreviation, $"{controlKey}_{title_text}", gunaChart.Title.Text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        if (column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell)
                        {
                            string translatedHeaderText = GetCachedTranslation(targetLanguageAbbreviation, columnKey, imageHeaderCell.HeaderText,
                                ref charactersTranslated, ref cacheHits, ref totalTranslations);
                            imageHeaderCell.HeaderText = translatedHeaderText;
                        }
                        else
                        {
                            column.HeaderText = GetCachedTranslation(targetLanguageAbbreviation, columnKey, column.HeaderText,
                                ref charactersTranslated, ref cacheHits, ref totalTranslations);
                        }
                    }
                    gunaDataGridView.Refresh();
                    break;
            }

            controlsTranslated++;

            foreach (Control childControl in control.Controls)
            {
                TranslateAllTextInControlFromCache(childControl, targetLanguageAbbreviation,
                    ref controlsTranslated, ref charactersTranslated, ref cacheHits, ref totalTranslations);
            }
        }
        private static string GetCachedTranslation(string targetLanguageAbbreviation, string controlKey, string originalText,
            ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            if (string.IsNullOrEmpty(originalText))
                return originalText;

            totalTranslations++;

            // For English, return the English cache or original text
            if (targetLanguageAbbreviation == "en")
            {
                if (EnglishCache.TryGetValue(controlKey, out string englishText))
                {
                    cacheHits++;
                    return englishText;
                }
                return originalText;
            }

            // For other languages, check translation cache
            if (TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(controlKey, out string cachedTranslation))
            {
                cacheHits++;
                charactersTranslated += cachedTranslation.Length;
                return cachedTranslation;
            }

            // If no cached translation, return original
            return originalText;
        }
        private static void TranslateLinkLabelFromCache(LinkLabel linkLabel, string targetLanguageAbbreviation,
            ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            string fullText = linkLabel.Text.Replace("\r\n", "\n");
            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string controlKeyBefore = GetControlKey(linkLabel, before_text);
                string controlKeyLink = GetControlKey(linkLabel, link_text);
                string controlKeyAfter = GetControlKey(linkLabel, after_text);

                string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                string linkText = fullText.Substring(linkStart, linkLength).Trim();
                string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                string translatedTextBefore = GetCachedTranslation(targetLanguageAbbreviation, controlKeyBefore, textBeforeLink,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedLink = GetCachedTranslation(targetLanguageAbbreviation, controlKeyLink, linkText,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedTextAfter = GetCachedTranslation(targetLanguageAbbreviation, controlKeyAfter, textAfterLink,
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);

                bool hasNewLineBefore = fullText.Substring(0, linkStart).EndsWith('\n');
                string finalText = (hasNewLineBefore ? translatedTextBefore + "\n" : translatedTextBefore) + " " +
                    translatedLink + " " + translatedTextAfter;

                linkLabel.Text = finalText;
                linkLabel.LinkArea = new LinkArea(translatedTextBefore.Length + 1, translatedLink.Length + 1);
            }
            else
            {
                string controlKeyFull = GetControlKey(linkLabel, full_text);
                string translatedFullText = GetCachedTranslation(targetLanguageAbbreviation, controlKeyFull, fullText.Trim(),
                    ref charactersTranslated, ref cacheHits, ref totalTranslations);
                linkLabel.Text = translatedFullText;
            }
        }

        /// <summary>
        /// Collects all texts that need translation from a control and its children.
        /// </summary>
        private static void CollectTextsToTranslate(Control control, string targetLanguageAbbreviation, Dictionary<string, string> textsToTranslate)
        {
            if (!CanControlTranslate(control)) return;

            string controlKey = GetControlKey(control);
            bool canCache = CanControlCache(control);

            // Check if already cached
            if (targetLanguageAbbreviation != "en" && canCache &&
                TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.ContainsKey(controlKey))
            {
                // Already cached, skip
                return;
            }

            // Add texts based on control type
            switch (control)
            {
                case Form form:
                    if (!string.IsNullOrEmpty(form.Text))
                        AddTextToTranslate(controlKey, form.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    break;

                case LinkLabel linkLabel:
                    CollectLinkLabelTexts(linkLabel, targetLanguageAbbreviation, textsToTranslate);
                    break;

                case Label label:
                    if (!string.IsNullOrEmpty(label.Text))
                        AddTextToTranslate(controlKey, label.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    break;

                case Guna2Button guna2Button:
                    if (!string.IsNullOrEmpty(guna2Button.Text))
                        AddTextToTranslate(controlKey, guna2Button.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    break;

                case RichTextBox textBox:
                    if (!string.IsNullOrEmpty(textBox.Text))
                        AddTextToTranslate(controlKey, textBox.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    break;

                case Guna2TextBox guna2TextBox:
                    if (!string.IsNullOrEmpty(guna2TextBox.Text))
                        AddTextToTranslate(controlKey, guna2TextBox.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    if (!string.IsNullOrEmpty(guna2TextBox.PlaceholderText))
                    {
                        string placeholderKey = $"{controlKey}_{placeholder_text}";
                        AddTextToTranslate(placeholderKey, guna2TextBox.PlaceholderText, targetLanguageAbbreviation, canCache, textsToTranslate);
                    }
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            AddTextToTranslate(itemKey, guna2ComboBox.Items[i].ToString(), targetLanguageAbbreviation, canCache, textsToTranslate);
                        }
                    }
                    break;

                case GunaChart gunaChart:
                    AddTextToTranslate($"{controlKey}_{title_text}", gunaChart.Title.Text, targetLanguageAbbreviation, canCache, textsToTranslate);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        string headerText = column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell
                            ? imageHeaderCell.HeaderText
                            : column.HeaderText;
                        AddTextToTranslate(columnKey, headerText, targetLanguageAbbreviation, canCache, textsToTranslate);
                    }
                    break;
            }

            // Recursively collect from child controls
            foreach (Control childControl in control.Controls)
            {
                CollectTextsToTranslate(childControl, targetLanguageAbbreviation, textsToTranslate);
            }
        }
        private static void CollectLinkLabelTexts(LinkLabel linkLabel, string targetLanguageAbbreviation, Dictionary<string, string> textsToTranslate)
        {
            if (!CanControlTranslate(linkLabel)) return;

            bool canCache = CanControlCache(linkLabel);
            string fullText = linkLabel.Text.Replace("\r\n", "\n");
            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string linkText = fullText.Substring(linkStart, linkLength).Trim();
                string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                AddTextToTranslate(GetControlKey(linkLabel, before_text), textBeforeLink, targetLanguageAbbreviation, canCache, textsToTranslate);
                AddTextToTranslate(GetControlKey(linkLabel, link_text), linkText, targetLanguageAbbreviation, canCache, textsToTranslate);
                AddTextToTranslate(GetControlKey(linkLabel, after_text), textAfterLink, targetLanguageAbbreviation, canCache, textsToTranslate);
            }
            else
            {
                AddTextToTranslate(GetControlKey(linkLabel, full_text), fullText.Trim(), targetLanguageAbbreviation, canCache, textsToTranslate);
            }
        }
        private static void AddTextToTranslate(string key, string text, string targetLanguageAbbreviation, bool canCache, Dictionary<string, string> textsToTranslate)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Check if already cached
            if (targetLanguageAbbreviation != "en" && canCache &&
                TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.ContainsKey(key))
            {
                return;
            }

            // Get English text
            string englishText = canCache && EnglishCache.TryGetValue(key, out string? value) ? value : text;

            if (targetLanguageAbbreviation == "en")
            {
                return;
            }

            if (!string.IsNullOrEmpty(englishText))
            {
                textsToTranslate[key] = englishText;
            }
        }

        /// <summary>
        /// Batch translates multiple texts with rate limiting.
        /// </summary>
        private static async Task<Dictionary<string, string>> BatchTranslateTexts(Dictionary<string, string> textsToTranslate,
            string targetLanguageAbbreviation, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> results = [];
            List<Dictionary<string, string>> batches = CreateBatches(textsToTranslate, MAX_BATCH_SIZE);

            Log.Write(0, $"Translating {textsToTranslate.Count} texts in {batches.Count} batches");

            for (int i = 0; i < batches.Count; i++)
            {
                Dictionary<string, string> batch = batches[i];
                cancellationToken.ThrowIfCancellationRequested();

                await WaitForRateLimit(cancellationToken).ConfigureAwait(false);

                try
                {
                    Log.Write(0, $"Translating batch {i + 1} of {batches.Count} ({batch.Count} texts)");
                    Dictionary<string, string> batchResults = await TranslateBatch(batch, targetLanguageAbbreviation, cancellationToken).ConfigureAwait(false);

                    foreach (KeyValuePair<string, string> kvp in batchResults)
                    {
                        results[kvp.Key] = kvp.Value;

                        // Cache the translation
                        if (!TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlDict))
                        {
                            controlDict = [];
                            TranslationCache[targetLanguageAbbreviation] = controlDict;
                        }
                        controlDict[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error_GetTranslation($"Batch translation failed: {ex.Message}");
                    // Return original texts for failed batch
                    foreach (KeyValuePair<string, string> kvp in batch)
                    {
                        results[kvp.Key] = kvp.Value;
                    }
                }

                // Small delay between batches
                if (i < batches.Count - 1)
                {
                    await Task.Delay(REQUEST_DELAY_MS, cancellationToken).ConfigureAwait(false);
                }
            }

            SaveCacheToFile();
            return results;
        }
        private static List<Dictionary<string, string>> CreateBatches(Dictionary<string, string> items, int batchSize)
        {
            List<Dictionary<string, string>> batches = [];
            Dictionary<string, string> currentBatch = [];

            foreach (KeyValuePair<string, string> kvp in items)
            {
                currentBatch[kvp.Key] = kvp.Value;
                if (currentBatch.Count >= batchSize)
                {
                    batches.Add(currentBatch);
                    currentBatch = [];
                }
            }

            if (currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }
        private static async Task WaitForRateLimit(CancellationToken cancellationToken = default)
        {
            await rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Remove old timestamps
                DateTime cutoff = DateTime.UtcNow.AddMinutes(-1);
                while (requestTimestamps.Count > 0 && requestTimestamps.Peek() < cutoff)
                {
                    requestTimestamps.Dequeue();
                }

                // Wait if we're at the limit
                while (requestTimestamps.Count >= MAX_REQUESTS_PER_MINUTE)
                {
                    DateTime oldestRequest = requestTimestamps.Peek();
                    TimeSpan waitTime = oldestRequest.AddMinutes(1) - DateTime.UtcNow;
                    if (waitTime > TimeSpan.Zero)
                    {
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                    }
                    requestTimestamps.Dequeue();
                }

                // Add current request timestamp
                requestTimestamps.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                rateLimiter.Release();
            }
        }
        private static async Task<Dictionary<string, string>> TranslateBatch(Dictionary<string, string> batch,
            string targetLanguageAbbreviation, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> results = [];
            List<string> keys = batch.Keys.ToList();
            List<string> texts = batch.Values.ToList();

            // Create request body with all texts
            var body = texts.Select(text => new { Text = text }).ToArray();
            StringContent requestContent = new(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{translationEndpoint}&to={targetLanguageAbbreviation}", requestContent, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new Exception($"{response.StatusCode}: {response.ReasonPhrase}. Details: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            dynamic result = JsonConvert.DeserializeObject(responseBody);

            // Map results back to keys
            for (int i = 0; i < keys.Count; i++)
            {
                string translatedText = result[i].translations[0].text;
                results[keys[i]] = translatedText;
            }

            return results;
        }

        /// <summary>
        /// Applies translated texts to controls.
        /// </summary>
        private static void ApplyTranslationsToControl(Control control, string targetLanguageAbbreviation,
            Dictionary<string, string> translations, ref int controlsTranslated, ref int charactersTranslated,
            ref int cacheHits, ref int totalTranslations)
        {
            CacheControlBounds(control);

            string controlKey = GetControlKey(control);

            switch (control)
            {
                case Form form:
                    ApplyTranslation(form, controlKey, translations, targetLanguageAbbreviation, (f, text) => f.Text = text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case LinkLabel linkLabel:
                    ApplyLinkLabelTranslation(linkLabel, translations, targetLanguageAbbreviation,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    AdjustLabelSizeAndPosition(linkLabel);
                    break;

                case Label label:
                    ApplyTranslation(label, controlKey, translations, targetLanguageAbbreviation, (l, text) =>
                    {
                        l.Text = text;
                        AdjustLabelSizeAndPosition(l);
                    }, ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2Button guna2Button:
                    ApplyTranslation(guna2Button, controlKey, translations, targetLanguageAbbreviation, (b, text) =>
                    {
                        AdjustButtonFontSize(b, text);
                    }, ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case RichTextBox textBox:
                    ApplyTranslation(textBox, controlKey, translations, targetLanguageAbbreviation, (t, text) => t.Text = text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2TextBox guna2TextBox:
                    ApplyTranslation(guna2TextBox, controlKey, translations, targetLanguageAbbreviation, (t, text) => t.Text = text,
                        ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    string placeholderKey = $"{controlKey}_{placeholder_text}";
                    ApplyTranslation(guna2TextBox, placeholderKey, translations, targetLanguageAbbreviation,
                        (t, text) => t.PlaceholderText = text, ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        int selectedIndex = guna2ComboBox.SelectedIndex;
                        List<object> translatedItems = [];

                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            string translatedText = GetTranslatedText(itemKey, translations, targetLanguageAbbreviation,
                                guna2ComboBox.Items[i].ToString(), ref charactersTranslated, ref cacheHits, ref totalTranslations);
                            translatedItems.Add(translatedText);
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
                    ApplyTranslation(gunaChart, $"{controlKey}_{title_text}", translations, targetLanguageAbbreviation,
                        (c, text) => c.Title.Text = text, ref charactersTranslated, ref cacheHits, ref totalTranslations);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        string translatedText = GetTranslatedText(columnKey, translations, targetLanguageAbbreviation,
                            column.HeaderText, ref charactersTranslated, ref cacheHits, ref totalTranslations);

                        if (column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell)
                        {
                            imageHeaderCell.HeaderText = translatedText;
                        }
                        else
                        {
                            column.HeaderText = translatedText;
                        }
                    }
                    gunaDataGridView.Refresh();
                    break;
            }

            controlsTranslated++;

            // Recursively apply to child controls
            foreach (Control childControl in control.Controls)
            {
                ApplyTranslationsToControl(childControl, targetLanguageAbbreviation, translations,
                    ref controlsTranslated, ref charactersTranslated, ref cacheHits, ref totalTranslations);
            }
        }
        private static void ApplyTranslation<T>(T control, string key, Dictionary<string, string> translations,
            string targetLanguageAbbreviation, Action<T, string> applyAction, ref int charactersTranslated,
            ref int cacheHits, ref int totalTranslations) where T : Control
        {
            string translatedText = GetTranslatedText(key, translations, targetLanguageAbbreviation,
                "", ref charactersTranslated, ref cacheHits, ref totalTranslations);

            if (!string.IsNullOrEmpty(translatedText))
            {
                applyAction(control, translatedText);
            }
        }
        private static string GetTranslatedText(string key, Dictionary<string, string> translations,
            string targetLanguageAbbreviation, string fallbackText, ref int charactersTranslated,
            ref int cacheHits, ref int totalTranslations)
        {
            totalTranslations++;

            // Check translations dictionary first
            if (translations.TryGetValue(key, out string translatedText))
            {
                charactersTranslated += translatedText.Length;
                return translatedText;
            }

            // Check cache
            if (TranslationCache.TryGetValue(targetLanguageAbbreviation, out Dictionary<string, string>? controlTranslations) &&
                controlTranslations.TryGetValue(key, out string cachedTranslation))
            {
                cacheHits++;
                return cachedTranslation;
            }

            // Check English cache for "en" language
            if (targetLanguageAbbreviation == "en" && EnglishCache.TryGetValue(key, out string englishText))
            {
                return englishText;
            }

            return fallbackText;
        }
        private static void ApplyLinkLabelTranslation(LinkLabel linkLabel, Dictionary<string, string> translations,
            string targetLanguageAbbreviation, ref int charactersTranslated, ref int cacheHits, ref int totalTranslations)
        {
            string fullText = linkLabel.Text.Replace("\r\n", "\n");
            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string controlKeyBefore = GetControlKey(linkLabel, before_text);
                string controlKeyLink = GetControlKey(linkLabel, link_text);
                string controlKeyAfter = GetControlKey(linkLabel, after_text);

                string translatedTextBefore = GetTranslatedText(controlKeyBefore, translations, targetLanguageAbbreviation,
                    "", ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedLink = GetTranslatedText(controlKeyLink, translations, targetLanguageAbbreviation,
                    "", ref charactersTranslated, ref cacheHits, ref totalTranslations);
                string translatedTextAfter = GetTranslatedText(controlKeyAfter, translations, targetLanguageAbbreviation,
                    "", ref charactersTranslated, ref cacheHits, ref totalTranslations);

                bool hasNewLineBefore = fullText.Substring(0, linkStart).EndsWith('\n');
                string finalText = (hasNewLineBefore ? translatedTextBefore + "\n" : translatedTextBefore) + " " +
                    translatedLink + " " + translatedTextAfter;

                linkLabel.Text = finalText;
                linkLabel.LinkArea = new LinkArea(translatedTextBefore.Length + 1, translatedLink.Length + 1);
            }
            else
            {
                string controlKeyFull = GetControlKey(linkLabel, full_text);
                string translatedFullText = GetTranslatedText(controlKeyFull, translations, targetLanguageAbbreviation,
                    fullText.Trim(), ref charactersTranslated, ref cacheHits, ref totalTranslations);
                linkLabel.Text = translatedFullText;
            }
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
                // Use batch translation for single strings
                Dictionary<string, string> textsToTranslate = new() { { cacheKey, text } };
                Task<Dictionary<string, string>> task = BatchTranslateTexts(textsToTranslate, targetLanguageAbbreviation);

                Dictionary<string, string> results = Task.Run(async () => await task).GetAwaiter().GetResult();

                if (results.TryGetValue(cacheKey, out string translatedText))
                {
                    return translatedText;
                }
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation(ex.Message);
            }

            return text;
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
        /// <returns>True if any text was cached, otherwise False.</returns>
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

                case RichTextBox textBox:
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        EnglishCache[controlKey] = textBox.Text;
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
            try
            {
                string jsonContent = JsonConvert.SerializeObject(TranslationCache, Formatting.Indented);
                if (!Directory.Exists(Directories.Cache_dir))
                {
                    Directories.CreateDirectory(Directories.Cache_dir, false);
                }
                Directories.WriteTextToFile(Directories.TranslationsCache_file, jsonContent);
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Failed to save translation cache: {ex.Message}");
            }
        }
        private static void SaveEnglishCacheToFile()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(EnglishCache, Formatting.Indented);
                if (!Directory.Exists(Directories.Cache_dir))
                {
                    Directories.CreateDirectory(Directories.Cache_dir, false);
                }
                Directories.WriteTextToFile(Directories.EnglishTexts_file, jsonContent);
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Failed to save English cache: {ex.Message}");
            }
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