using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ImportSpreadsheet;
using Sales_Tracker.Passwords;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.Startup;
using Sales_Tracker.Startup.Menus;
using System.Text;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Utility class for generating translation JSON files for all application forms and controls.
    /// This class is intended for admin use only and should not be included in the production build.
    /// </summary>
    public static class TranslationGenerator
    {
        // API Translation properties
        private static readonly HttpClient httpClient = new();
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";

        // Improved rate limiting and batching
        private static readonly byte maxBatchSize = 10;
        private static readonly byte maxRequestsPerMinute = 60; // Reduced from 100 to be safe
        private static readonly byte requestDelay_MS = 100;
        private static readonly int maxCharactersPerRequest = 45000; // Safety buffer below 50,000 limit
        private static readonly int maxCharactersPerHour = 2000000; // F0 tier limit
        private static readonly SemaphoreSlim rateLimiter = new(1, 1);
        private static readonly Queue<DateTime> requestTimestamps = new();
        private static int charactersUsedThisHour = 0;
        private static DateTime hourlyResetTime = DateTime.UtcNow.AddHours(1);

        // Constants
        private static readonly string placeholder_text = "Placeholder",
            item_text = "Item",
            title_text = "Title",
            column_text = "Column",
            before_text = "before",
            link_text = "link",
            after_text = "after",
            full_text = "full";

        /// <summary>
        /// Initialize the HTTP client with API credentials.
        /// </summary>
        static TranslationGenerator()
        {
            string APIKey = DotEnv.Get("MICROSOFT_TRANSLATOR_API_KEY");

            // Add headers for the API request
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIKey);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "canadacentral");
        }

        /// <summary>
        /// Generates translation JSON files for all supported languages based on the current application's text.
        /// Shows a modern progress dialog with overall progress tracking.
        /// </summary>
        public static async Task GenerateAllLanguageTranslationFiles()
        {
            TranslationProgress_Form progressForm = new();

            try
            {
                // Show progress form as modal dialog
                progressForm.Show();
                progressForm.BringToFront();

                // List of form types to process
                Type[] formTypes =
                [
                    // ImportSpreadsheet
                    typeof(ImportSpreadsheet_Form),
                    typeof(Setup_Form),
                    typeof(Tutorial_Form),

                    // Passwords
                    typeof(ImportSpreadsheet_Form),
                    typeof(EnterPassword_Form),
                    typeof(PasswordManager_Form),

                    // Settings
                    typeof(General_Form),
                    typeof(Security_Form),
                    typeof(Updates_Form),
                    typeof(Settings_Form),

                    // Startup
                    typeof(ConfigureProject_Form),
                    typeof(GetStarted_Form),
                    typeof(Startup_Form),

                    // Main
                    typeof(Accountants_Form),
                    typeof(AddPurchase_Form),
                    typeof(AddSale_Form),
                    typeof(Categories_Form),
                    typeof(Companies_Form),
                    typeof(CustomMessage_Form),
                    typeof(DateRange_Form),
                    typeof(EULA_Form),
                    typeof(Export_Form),
                    typeof(ItemsInTransaction_Form),
                    typeof(Loading_Form),
                    typeof(Log_Form),
                    typeof(MainMenu_Form),
                    typeof(ModifyRow_Form),
                    typeof(Products_Form),
                    typeof(Receipts_Form),
                    typeof(Upgrade_Form),
                    typeof(Welcome_Form),
                ];

                List<Form> openForms = Application.OpenForms.Cast<Form>().ToList();
                List<Control> controlsToTranslate = [];

                // Process each form type
                foreach (Type formType in formTypes)
                {
                    if (progressForm.IsCancelled) { return; }

                    // Check if the form is already open or create a new instance
                    Form formInstance = openForms.FirstOrDefault(f => f.GetType() == formType)
                        ?? (Form)Activator.CreateInstance(formType);

                    controlsToTranslate.Add(formInstance);
                }

                // Add UI panels
                controlsToTranslate.AddRange(MainMenu_Form.GetMenus().Cast<Control>());

                // Add other controls
                controlsToTranslate.Add(CustomControls.ControlsDropDown_Button);

                // Create output directory
                string outputDirectory = Path.Combine(Directories.Downloads_dir, "Sales Tracker Translations");
                Directory.CreateDirectory(outputDirectory);

                // Collect all texts to translate (English source)
                progressForm.UpdateLanguage("English", 0);
                progressForm.UpdateTranslationProgress(0, "Collecting source texts...");

                Dictionary<string, string> sourceTexts = CollectTextsToTranslate(controlsToTranslate);

                // Pre-filter and optimize texts
                sourceTexts = FilterAndOptimizeTexts(sourceTexts);

                List<KeyValuePair<string, string>> languages = LanguageManager.GetLanguages();

                // Initialize progress form with totals
                progressForm.Initialize(languages.Count, sourceTexts.Count);

                // Reset hourly tracking
                charactersUsedThisHour = 0;
                hourlyResetTime = DateTime.UtcNow.AddHours(1);

                // Process each language
                for (int langIndex = 0; langIndex < languages.Count; langIndex++)
                {
                    KeyValuePair<string, string> language = languages[langIndex];

                    if (progressForm.IsCancelled)
                    {
                        progressForm.Close();
                        return;
                    }

                    progressForm.UpdateLanguage(language.Key, langIndex);

                    Dictionary<string, string> translations;

                    if (language.Value == "en")
                    {
                        // For English, just use the source texts
                        progressForm.UpdateTranslationProgress(sourceTexts.Count, "Using source texts for English...");
                        translations = sourceTexts;
                    }
                    else
                    {
                        // For other languages, translate using API
                        translations = await BatchTranslateTexts(sourceTexts, language.Value, progressForm, progressForm.CancellationToken);

                        if (progressForm.IsCancelled)
                        {
                            progressForm.Close();
                            return;
                        }
                    }

                    // Generate the JSON file
                    progressForm.UpdateTranslationProgress(sourceTexts.Count, $"Saving {language.Key} translations to file...");
                    string outputPath = Path.Combine(outputDirectory, $"{language.Value}.json");
                    await GenerateLanguageJsonFile(translations, outputPath, language.Value);

                    Log.Write(1, $"Completed translations for {language.Key} ({translations.Count} texts)");
                }

                progressForm.CompleteProgress();
                Log.Write(1, "Translation generation complete.");
            }
            catch (OperationCanceledException)
            {
                Log.Write(1, "Translation generation was cancelled by user.");
                progressForm.Close();
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation($"Translation generation failed: {ex.Message}");
                progressForm.Close();
                throw;
            }
        }

        /// <summary>
        /// Pre-filters and optimizes texts to reduce API usage.
        /// </summary>
        private static Dictionary<string, string> FilterAndOptimizeTexts(Dictionary<string, string> sourceTexts)
        {
            Dictionary<string, string> optimizedTexts = [];

            foreach (KeyValuePair<string, string> kvp in sourceTexts)
            {
                string text = kvp.Value?.Trim();

                // Skip empty, null, or very short texts
                if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
                    continue;

                // Skip numbers only
                if (text.All(char.IsDigit))
                    continue;

                // Skip single characters
                if (text.Length == 1)
                    continue;

                optimizedTexts[kvp.Key] = text;
            }

            Log.Write(1, $"Optimized texts: {sourceTexts.Count} -> {optimizedTexts.Count} (saved {sourceTexts.Count - optimizedTexts.Count} unnecessary translations)");
            return optimizedTexts;
        }

        /// <summary>
        /// Batch translates multiple texts with enhanced rate limiting and progress updates.
        /// </summary>
        private static async Task<Dictionary<string, string>> BatchTranslateTexts(Dictionary<string, string> textsToTranslate,
            string targetLanguageAbbreviation, TranslationProgress_Form progressForm, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> results = [];
            List<Dictionary<string, string>> batches = CreateBatches(textsToTranslate, maxBatchSize);

            Log.Write(1, $"Starting translation for {targetLanguageAbbreviation}: {textsToTranslate.Count} texts in {batches.Count} batches");

            for (int i = 0; i < batches.Count; i++)
            {
                Dictionary<string, string> batch = batches[i];
                cancellationToken.ThrowIfCancellationRequested();

                // Calculate characters in this batch
                int batchCharacters = batch.Values.Sum(text => text.Length);

                await WaitForRateLimit(batchCharacters, cancellationToken).ConfigureAwait(false);

                int retryCount = 0;
                const int maxRetries = 3;

                while (retryCount <= maxRetries)
                {
                    try
                    {
                        progressForm.UpdateBatchProgress(i, batches.Count, batch.Count);
                        Dictionary<string, string> batchResults = await TranslateBatch(batch, targetLanguageAbbreviation, cancellationToken).ConfigureAwait(false);

                        foreach (KeyValuePair<string, string> kvp in batchResults)
                        {
                            results[kvp.Key] = kvp.Value;
                        }

                        // Log progress
                        Log.Write(1, $"Completed batch {i + 1}/{batches.Count} ({batchCharacters} characters, {charactersUsedThisHour} total this hour)");
                        break; // Success, exit retry loop
                    }
                    catch (HttpRequestException ex) when ((ex.Message.Contains("429") || ex.Message.Contains("rate")) && retryCount < maxRetries)
                    {
                        retryCount++;
                        int backoffDelay = (int)Math.Pow(2, retryCount) * 5000; // 10s, 20s, 40s
                        Log.Write(1, $"Rate limit hit on batch {i + 1}, attempt {retryCount}/{maxRetries}. Waiting {backoffDelay / 1000}s before retry...");

                        await Task.Delay(backoffDelay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log.Error_GetTranslation($"Batch translation failed on attempt {retryCount + 1}: {ex.Message}");

                        if (retryCount >= maxRetries)
                        {
                            // Return original texts for failed batch after all retries
                            foreach (KeyValuePair<string, string> kvp in batch)
                            {
                                results[kvp.Key] = kvp.Value;
                            }
                            Log.Write(1, $"Using original texts for batch {i + 1} after {maxRetries} failed attempts");
                            break;
                        }

                        retryCount++;
                        await Task.Delay(2000 * retryCount, cancellationToken).ConfigureAwait(false);
                    }
                }

                // Small delay between batches
                if (i < batches.Count - 1)
                {
                    await Task.Delay(requestDelay_MS, cancellationToken).ConfigureAwait(false);
                }
            }

            return results;
        }

        /// <summary>
        /// Creates batches of texts for translation with character limit consideration.
        /// </summary>
        private static List<Dictionary<string, string>> CreateBatches(Dictionary<string, string> items, int batchSize)
        {
            List<Dictionary<string, string>> batches = [];
            Dictionary<string, string> currentBatch = [];
            int currentBatchCharacters = 0;

            foreach (KeyValuePair<string, string> kvp in items)
            {
                int textLength = kvp.Value.Length;

                // Start new batch if adding this text would exceed limits
                if ((currentBatch.Count >= batchSize) ||
                    (currentBatchCharacters + textLength > maxCharactersPerRequest))
                {
                    if (currentBatch.Count > 0)
                    {
                        batches.Add(currentBatch);
                        currentBatch = [];
                        currentBatchCharacters = 0;
                    }
                }

                currentBatch[kvp.Key] = kvp.Value;
                currentBatchCharacters += textLength;
            }

            if (currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
            }

            Log.Write(1, $"Created {batches.Count} batches from {items.Count} items");
            return batches;
        }

        /// <summary>
        /// Enhanced rate limiting with character tracking.
        /// </summary>
        private static async Task WaitForRateLimit(int charactersInBatch, CancellationToken cancellationToken = default)
        {
            await rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                DateTime now = DateTime.UtcNow;

                // Reset hourly character counter if needed
                if (now >= hourlyResetTime)
                {
                    charactersUsedThisHour = 0;
                    hourlyResetTime = now.AddHours(1);
                    Log.Write(1, "Hourly character limit reset");
                }

                // Check if this batch would exceed hourly character limit
                if (charactersUsedThisHour + charactersInBatch > maxCharactersPerHour)
                {
                    TimeSpan waitTime = hourlyResetTime - now;
                    if (waitTime > TimeSpan.Zero)
                    {
                        Log.Write(1, $"Hourly character limit would be exceeded. Waiting {waitTime.TotalMinutes:F1} minutes until reset...");
                        // Wait until the hour resets
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                        charactersUsedThisHour = 0;
                        hourlyResetTime = DateTime.UtcNow.AddHours(1);
                    }
                }

                // Remove old request timestamps (older than 1 minute)
                DateTime cutoff = now.AddMinutes(-1);
                while (requestTimestamps.Count > 0 && requestTimestamps.Peek() < cutoff)
                {
                    requestTimestamps.Dequeue();
                }

                // Wait if we're at the request limit
                while (requestTimestamps.Count >= maxRequestsPerMinute)
                {
                    DateTime oldestRequest = requestTimestamps.Peek();
                    TimeSpan waitTime = oldestRequest.AddMinutes(1) - now;
                    if (waitTime > TimeSpan.Zero)
                    {
                        Log.Write(1, $"Request rate limit reached. Waiting {waitTime.TotalSeconds:F0} seconds...");
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                    }
                    requestTimestamps.Dequeue();
                }

                // Add current request timestamp and update character usage
                requestTimestamps.Enqueue(now);
                charactersUsedThisHour += charactersInBatch;
            }
            finally
            {
                rateLimiter.Release();
            }
        }

        /// <summary>
        /// Translates a single batch using Microsoft Translator API.
        /// </summary>
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
                throw new HttpRequestException($"{response.StatusCode}: {response.ReasonPhrase}. Details: {errorContent}");
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
        /// Collects all translatable texts from the given controls.
        /// </summary>
        private static Dictionary<string, string> CollectTextsToTranslate(List<Control> controls)
        {
            Dictionary<string, string> textsToTranslate = [];

            foreach (Control control in controls)
            {
                CollectTextsRecursively(control, textsToTranslate);
            }

            return textsToTranslate;
        }

        /// <summary>
        /// Recursively collects translatable texts from a control and its children.
        /// </summary>
        private static void CollectTextsRecursively(Control control, Dictionary<string, string> textsToTranslate)
        {
            // Skip controls that should not be cached
            if (control.AccessibleDescription == AccessibleDescriptionManager.DoNotCache)
            {
                return;
            }

            string controlKey = LanguageManager.GetControlKey(control);

            // Collect texts based on control type
            switch (control)
            {
                case Form form:
                    AddTextToTranslate(textsToTranslate, controlKey, form.Text);
                    break;

                case LinkLabel linkLabel:
                    CollectLinkLabelTexts(linkLabel, textsToTranslate);
                    break;

                case Label label:
                    AddTextToTranslate(textsToTranslate, controlKey, label.Text);
                    break;

                case Guna2Button guna2Button:
                    AddTextToTranslate(textsToTranslate, controlKey, guna2Button.Text);
                    break;

                case RichTextBox textBox:
                    AddTextToTranslate(textsToTranslate, controlKey, textBox.Text);
                    break;

                case Guna2TextBox guna2TextBox:
                    AddTextToTranslate(textsToTranslate, controlKey, guna2TextBox.Text);
                    AddTextToTranslate(textsToTranslate, $"{controlKey}_{placeholder_text}", guna2TextBox.PlaceholderText);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{item_text}_{i}";
                            AddTextToTranslate(textsToTranslate, itemKey, guna2ComboBox.Items[i].ToString());
                        }
                    break;

                case GunaChart gunaChart:
                    AddTextToTranslate(textsToTranslate, $"{controlKey}_{title_text}", gunaChart.Title.Text);
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{column_text}_{column.Name}";
                        string headerText = column.HeaderCell is DataGridViewImageHeaderCell imageHeaderCell
                            ? imageHeaderCell.HeaderText
                            : column.HeaderText;
                        AddTextToTranslate(textsToTranslate, columnKey, headerText);
                    }
                    break;
            }

            // Recursively collect from child controls
            foreach (Control childControl in control.Controls)
            {
                CollectTextsRecursively(childControl, textsToTranslate);
            }
        }

        /// <summary>
        /// Collects texts for LinkLabel with special handling for link areas.
        /// </summary>
        private static void CollectLinkLabelTexts(LinkLabel linkLabel, Dictionary<string, string> textsToTranslate)
        {
            string fullText = linkLabel.Text.Replace("\r\n", "\n");
            int linkStart = linkLabel.LinkArea.Start;
            int linkLength = linkLabel.LinkArea.Length;

            if (linkLength > 0)
            {
                string linkText = fullText.Substring(linkStart, linkLength).Trim();
                string textBeforeLink = fullText.Substring(0, linkStart).Trim();
                string textAfterLink = fullText.Substring(linkStart + linkLength).Trim();

                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, before_text), textBeforeLink);
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, link_text), linkText);
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, after_text), textAfterLink);
            }
            else
            {
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, full_text), fullText.Trim());
            }
        }

        /// <summary>
        /// Adds a text to the translation dictionary if it's not empty.
        /// </summary>
        private static void AddTextToTranslate(Dictionary<string, string> textsToTranslate, string key, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                textsToTranslate[key] = text;
            }
        }

        /// <summary>
        /// Generates a JSON file for a specific language with the collected texts.
        /// </summary>
        private static async Task GenerateLanguageJsonFile(Dictionary<string, string> translations, string outputPath, string languageCode)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(translations, Formatting.Indented);
                await File.WriteAllTextAsync(outputPath, jsonContent);
                Log.Write(1, $"Generated translation file for {languageCode} at {outputPath}");
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation($"Failed to generate translation file for {languageCode}: {ex.Message}");
            }
        }
    }
}