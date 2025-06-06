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
    /// Utility class for generating translation JSON files for all application forms and controls.
    /// This class is intended for admin use only and should not be included in the production build.
    /// </summary>
    public static class TranslationGenerator
    {
        // API Translation properties
        private static readonly HttpClient httpClient = new();
        private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";

        // Rate limiting and batching
        private static readonly int maxBatchSize = 25; // Microsoft Translator supports up to 100, but we'll be conservative
        private static readonly int maxRequestsPerMinute = 100;
        private static readonly int requestDelay_MS = 100; // Delay between batches
        private static readonly SemaphoreSlim rateLimiter = new(1, 1);
        private static readonly Queue<DateTime> requestTimestamps = new();

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
        /// Initialize the HTTP client with API credentials
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

                List<Control> controlsToTranslate = [];

                // List of form types to process
                Type[] formTypes =
                [
                    typeof(MainMenu_Form),
                    typeof(Settings_Form),
                    typeof(General_Form),
                    typeof(Security_Form),
                    typeof(Updates_Form),
                    typeof(Log_Form)
                ];

                List<Form> openForms = Application.OpenForms.Cast<Form>().ToList();

                // Process each form type
                foreach (Type formType in formTypes)
                {
                    if (progressForm.IsCancelled) { return; }

                    Form formInstance = null;

                    // Check if the form is already open
                    formInstance = openForms.FirstOrDefault(f => f.GetType() == formType);

                    // If not open, create a new instance using reflection
                    if (formInstance == null)
                    {
                        try
                        {
                            formInstance = (Form)Activator.CreateInstance(formType);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(0, $"Failed to create instance of {formType.Name}: {ex.Message}");
                            continue;
                        }
                    }

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

                List<KeyValuePair<string, string>> languages = LanguageManager.GetLanguages();

                // Initialize progress form with totals
                progressForm.Initialize(languages.Count, sourceTexts.Count);

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
        /// Batch translates multiple texts with rate limiting and progress updates.
        /// </summary>
        private static async Task<Dictionary<string, string>> BatchTranslateTexts(Dictionary<string, string> textsToTranslate,
            string targetLanguageAbbreviation, TranslationProgress_Form progressForm, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> results = [];
            List<Dictionary<string, string>> batches = CreateBatches(textsToTranslate, maxBatchSize);

            for (int i = 0; i < batches.Count; i++)
            {
                Dictionary<string, string> batch = batches[i];
                cancellationToken.ThrowIfCancellationRequested();

                await WaitForRateLimit(cancellationToken).ConfigureAwait(false);

                try
                {
                    progressForm.UpdateBatchProgress(i, batches.Count, batch.Count);
                    Dictionary<string, string> batchResults = await TranslateBatch(batch, targetLanguageAbbreviation, cancellationToken).ConfigureAwait(false);

                    foreach (KeyValuePair<string, string> kvp in batchResults)
                    {
                        results[kvp.Key] = kvp.Value;
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
                    await Task.Delay(requestDelay_MS, cancellationToken).ConfigureAwait(false);
                }
            }

            return results;
        }

        /// <summary>
        /// Creates batches of texts for translation.
        /// </summary>
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

        /// <summary>
        /// Implements rate limiting for API calls.
        /// </summary>
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
                while (requestTimestamps.Count >= maxRequestsPerMinute)
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