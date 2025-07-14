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
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Utility class for generating translation JSON files for all application forms and controls.
    /// This class is intended for admin use only and should not be included in the production build.
    /// Now features fully automated string collection - no manual maintenance required!
    /// </summary>
    public static partial class TranslationGenerator
    {
        // API Translation properties
        private static readonly HttpClient _httpClient = new();
        private static readonly string _translationEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";

        // Rate limiting and batching
        private static readonly byte _maxBatchSize = 10;
        private static readonly byte _maxRequestsPerMinute = 60;
        private static readonly byte _requestDelay_MS = 100;
        private static readonly int _maxCharactersPerRequest = 45000;  // Safety buffer below 50,000 limit
        private static readonly int _maxCharactersPerHour = 2000000;   // F0 tier limit
        private static readonly SemaphoreSlim _rateLimiter = new(1, 1);
        private static readonly Queue<DateTime> _requestTimestamps = new();
        private static int _charactersUsedThisHour = 0;
        private static DateTime _hourlyResetTime = DateTime.UtcNow.AddHours(1);

        // Constants
        private static readonly string _placeholder_text = "Placeholder",
            _item_text = "Item",
            _column_text = "Column",
            _before_text = "before",
            _link_text = "link",
            _after_text = "after",
            _full_text = "full";

        [GeneratedRegex(@"\{[^}]+\}")]
        private static partial Regex InterpolationPattern();

        [GeneratedRegex(@"[^\w]")]
        private static partial Regex NonWordCharacters();

        /// <summary>
        /// Initialize the HTTP client with API credentials.
        /// </summary>
        static TranslationGenerator()
        {
            string APIKey = DotEnv.Get("MICROSOFT_TRANSLATOR_API_KEY");

            // Add headers for the API request
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIKey);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "canadacentral");
        }

        /// <summary>
        /// Generates translation JSON files for all supported languages based on the current application's text.
        /// Shows a progress dialog with progress tracking.
        /// Features fully automated string collection from source code.
        /// </summary>
        public static async Task GenerateAllLanguageTranslationFiles()
        {
            // Check for internet connection
            if (!await InternetConnectionManager.CheckInternetAndShowMessageAsync("generating translations", true))
            {
                Log.Write(1, "Translation generation cancelled - no internet connection");
                return;
            }

            TranslationProgress_Form progressForm = new();
            Tools.OpenForm(progressForm);

            try
            {
                // List of form types to process
                Type[] formTypes =
                [
                    // ImportSpreadsheet
                    typeof(ImportSpreadsheet_Form),
                    typeof(Setup_Form),
                    typeof(Tutorial_Form),

                    // Passwords
                    typeof(AddPassword_Form),
                    typeof(EnterPassword_Form),
                    typeof(PasswordManager_Form),

                    // Settings
                    typeof(General_Form),
                    typeof(Security_Form),
                    typeof(Updates_Form),
                    typeof(Settings_Form),

                    // Startup
                    typeof(ConfigureCompany_Form),
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
                controlsToTranslate.AddRange(MainMenu_Form.Instance.GetAnalyticsControls());
                controlsToTranslate.AddRange(MainMenu_Form.Instance.GetMainControls());

                // Create output directory
                string outputDirectory = Path.Combine(Directories.Downloads_dir, "Sales Tracker Translations");
                Directory.CreateDirectory(outputDirectory);

                // Collect all texts to translate (English source)
                progressForm.UpdateLanguage("English", 0);
                progressForm.UpdateTranslationProgress(0, "Collecting UI control texts...");

                // Collect UI control texts
                Dictionary<string, string> sourceTexts = CollectTextsToTranslate(controlsToTranslate);

                // AUTO-COLLECT: Get all TranslateString calls from source code
                progressForm.UpdateTranslationProgress(sourceTexts.Count, "Scanning source code for TranslateString calls...");
                Dictionary<string, string> translateStringCalls = CollectAllTranslateStringCalls();

                // AUTO-COLLECT: Get all CustomMessageBox calls from source code
                progressForm.UpdateTranslationProgress(sourceTexts.Count + translateStringCalls.Count, "Scanning source code for CustomMessageBox calls...");
                Dictionary<string, string> messageBoxStrings = CollectAllCustomMessageBoxCalls();

                // Merge all collections
                foreach (KeyValuePair<string, string> kvp in translateStringCalls)
                {
                    if (!sourceTexts.ContainsKey(kvp.Key))
                    {
                        sourceTexts[kvp.Key] = kvp.Value;
                    }
                }

                foreach (KeyValuePair<string, string> kvp in messageBoxStrings)
                {
                    if (!sourceTexts.ContainsKey(kvp.Key))
                    {
                        sourceTexts[kvp.Key] = kvp.Value;
                    }
                }

                Log.Write(1, $"Total texts collected: {sourceTexts.Count} ({translateStringCalls.Count} from TranslateString calls, {messageBoxStrings.Count} from CustomMessageBox calls)");

                // Pre-filter and optimize texts
                sourceTexts = FilterAndOptimizeTexts(sourceTexts);

                List<KeyValuePair<string, string>> languages = LanguageManager.GetLanguages();

                // Initialize progress form with totals
                progressForm.Initialize(languages.Count, sourceTexts.Count);

                // Reset hourly tracking
                _charactersUsedThisHour = 0;
                _hourlyResetTime = DateTime.UtcNow.AddHours(1);

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
                Log.Write(1, "Translation generation complete - fully automated!");
            }
            catch (OperationCanceledException)
            {
                Log.Write(1, "Translation generation was cancelled by user");
                progressForm.Close();
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation($"Translation generation failed: {ex.Message}");
                progressForm.Close();
            }
        }

        // API Translation Methods

        /// <summary>
        /// Pre-filters and optimizes texts to reduce API usage.
        /// </summary>
        private static Dictionary<string, string> FilterAndOptimizeTexts(Dictionary<string, string> sourceTexts)
        {
            Dictionary<string, string> optimizedTexts = [];

            foreach (KeyValuePair<string, string> kvp in sourceTexts)
            {
                string text = kvp.Value?.Trim();

                // Skip null, empty, very short texts, and numbers
                if (string.IsNullOrWhiteSpace(text) || text.Length < 2 || text.All(char.IsDigit))
                {
                    continue;
                }

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
            List<Dictionary<string, string>> batches = CreateBatches(textsToTranslate, _maxBatchSize);

            Log.Write(1, $"Starting translation for {targetLanguageAbbreviation}: {textsToTranslate.Count} texts in {batches.Count} batches");

            for (int i = 0; i < batches.Count; i++)
            {
                Dictionary<string, string> batch = batches[i];
                cancellationToken.ThrowIfCancellationRequested();

                // Calculate characters in this batch
                int batchCharacters = batch.Values.Sum(text => text.Length);

                await WaitForRateLimit(batchCharacters, cancellationToken).ConfigureAwait(false);

                int retryCount = 0;
                const byte maxRetries = 3;

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
                        Log.Write(1, $"Completed batch {i + 1}/{batches.Count} ({batchCharacters} characters, {_charactersUsedThisHour} total this hour)");
                        break;  // Success, exit retry loop
                    }
                    catch (HttpRequestException ex) when ((ex.Message.Contains("429") || ex.Message.Contains("rate")) && retryCount < maxRetries)
                    {
                        retryCount++;
                        int backoffDelay = (int)Math.Pow(2, retryCount) * 5000;  // 10s, 20s, 40s
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
                    await Task.Delay(_requestDelay_MS, cancellationToken).ConfigureAwait(false);
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
                    (currentBatchCharacters + textLength > _maxCharactersPerRequest))
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
            await _rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                DateTime now = DateTime.UtcNow;

                // Reset hourly character counter if needed
                if (now >= _hourlyResetTime)
                {
                    _charactersUsedThisHour = 0;
                    _hourlyResetTime = now.AddHours(1);
                    Log.Write(1, "Hourly character limit reset");
                }

                // Check if this batch would exceed hourly character limit
                if (_charactersUsedThisHour + charactersInBatch > _maxCharactersPerHour)
                {
                    TimeSpan waitTime = _hourlyResetTime - now;
                    if (waitTime > TimeSpan.Zero)
                    {
                        Log.Write(1, $"Hourly character limit would be exceeded. Waiting {waitTime.TotalMinutes:F1} minutes until reset...");
                        // Wait until the hour resets
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                        _charactersUsedThisHour = 0;
                        _hourlyResetTime = DateTime.UtcNow.AddHours(1);
                    }
                }

                // Remove old request timestamps (older than 1 minute)
                DateTime cutoff = now.AddMinutes(-1);
                while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < cutoff)
                {
                    _requestTimestamps.Dequeue();
                }

                // Wait if we're at the request limit
                while (_requestTimestamps.Count >= _maxRequestsPerMinute)
                {
                    DateTime oldestRequest = _requestTimestamps.Peek();
                    TimeSpan waitTime = oldestRequest.AddMinutes(1) - now;
                    if (waitTime > TimeSpan.Zero)
                    {
                        Log.Write(1, $"Request rate limit reached. Waiting {waitTime.TotalSeconds:F0} seconds...");
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                    }
                    _requestTimestamps.Dequeue();
                }

                // Add current request timestamp and update character usage
                _requestTimestamps.Enqueue(now);
                _charactersUsedThisHour += charactersInBatch;
            }
            finally
            {
                _rateLimiter.Release();
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

            HttpResponseMessage response = await _httpClient.PostAsync($"{_translationEndpoint}&to={targetLanguageAbbreviation}", requestContent, cancellationToken).ConfigureAwait(false);

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

        // UI Control Text Collection

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
            if (control.AccessibleDescription == AccessibleDescriptionManager.DoNotCache
                || control.AccessibleDescription == AccessibleDescriptionManager.DoNotTranslate)
            {
                return;
            }

            string controlKey = LanguageManager.GetControlKey(control);

            // Collect texts based on control type
            switch (control)
            {
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
                    AddTextToTranslate(textsToTranslate, $"{controlKey}_{_placeholder_text}", guna2TextBox.PlaceholderText);
                    break;

                case Guna2ComboBox guna2ComboBox:
                    if (guna2ComboBox.DataSource == null)
                    {
                        for (int i = 0; i < guna2ComboBox.Items.Count; i++)
                        {
                            string itemKey = $"{controlKey}_{_item_text}_{i}";
                            AddTextToTranslate(textsToTranslate, itemKey, guna2ComboBox.Items[i].ToString());
                        }
                    }
                    break;

                case Guna2DataGridView gunaDataGridView:
                    foreach (DataGridViewColumn column in gunaDataGridView.Columns)
                    {
                        string columnKey = $"{controlKey}_{_column_text}_{column.Name}";
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

                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, _before_text), textBeforeLink);
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, _link_text), linkText);
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, _after_text), textAfterLink);
            }
            else
            {
                AddTextToTranslate(textsToTranslate, LanguageManager.GetControlKey(linkLabel, _full_text), fullText.Trim());
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

        // Automated Source Code Scanning

        /// <summary>
        /// Automatically scans source files for LanguageManager.TranslateString calls and extracts string literals.
        /// This completely replaces the need for manual string collection.
        /// </summary>
        private static Dictionary<string, string> CollectAllTranslateStringCalls()
        {
            Dictionary<string, string> collectedStrings = [];

            try
            {
                string projectDirectory = FindProjectDirectory();

                if (projectDirectory == null)
                {
                    Log.Write(1, "Could not find source directory for TranslateString collection");
                    return collectedStrings;
                }

                // Search all .cs files for LanguageManager.TranslateString calls
                string[] csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                foreach (string file in csFiles)
                {
                    if (ShouldSkipFile(file)) continue;

                    string content = File.ReadAllText(file);
                    ExtractTranslateStringCalls(content, collectedStrings, Path.GetFileName(file));
                }

                Log.Write(1, $"Auto-collected {collectedStrings.Count} TranslateString calls from source code");
            }
            catch (Exception ex)
            {
                Log.Write(1, $"Error collecting TranslateString calls: {ex.Message}");
            }

            return collectedStrings;
        }

        /// <summary>
        /// Automatically scans source files for CustomMessageBox.Show calls and extracts title/message strings.
        /// </summary>
        private static Dictionary<string, string> CollectAllCustomMessageBoxCalls()
        {
            Dictionary<string, string> messageBoxStrings = [];

            try
            {
                string projectDirectory = FindProjectDirectory();

                if (projectDirectory == null)
                {
                    Log.Write(1, "Could not find source directory for CustomMessageBox collection");
                    return messageBoxStrings;
                }

                // Search all .cs files for CustomMessageBox.Show calls
                string[] csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                foreach (string file in csFiles)
                {
                    if (ShouldSkipFile(file)) continue;

                    string content = File.ReadAllText(file);
                    ExtractCustomMessageBoxCalls(content, messageBoxStrings, Path.GetFileName(file));
                }

                Log.Write(1, $"Auto-collected {messageBoxStrings.Count} CustomMessageBox calls from source code");
            }
            catch (Exception ex)
            {
                Log.Write(1, $"Error collecting CustomMessageBox calls: {ex.Message}");
            }

            return messageBoxStrings;
        }

        /// <summary>
        /// Finds the project directory containing source files
        /// </summary>
        private static string? FindProjectDirectory()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Go up directories to find source files
            while (currentDirectory != null)
            {
                // Look for .cs files or .csproj files to identify project root
                if (Directory.GetFiles(currentDirectory, "*.cs", SearchOption.TopDirectoryOnly).Length != 0 ||
                    Directory.GetFiles(currentDirectory, "*.csproj", SearchOption.TopDirectoryOnly).Length != 0)
                {
                    return currentDirectory;
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }

        /// <summary>
        /// Determines if a file should be skipped during scanning
        /// </summary>
        private static bool ShouldSkipFile(string filePath)
        {
            return filePath.Contains("bin\\") ||
                   filePath.Contains("obj\\") ||
                   filePath.Contains("\\Debug\\") ||
                   filePath.Contains("\\Release\\") ||
                   Path.GetFileName(filePath) == "TranslationGenerator.cs";  // Skip self to avoid recursion
        }

        /// <summary>
        /// Extracts LanguageManager.TranslateString call strings from source code content
        /// </summary>
        private static void ExtractTranslateStringCalls(string content, Dictionary<string, string> collectedStrings, string fileName)
        {
            // Pattern 1: Simple string literals
            // LanguageManager.TranslateString("text")
            string simplePattern = @"LanguageManager\.TranslateString\s*\(\s*""([^""]*)""\s*\)";
            ExtractMatches(content, simplePattern, collectedStrings, fileName);

            // Pattern 2: String interpolation (basic)
            // LanguageManager.TranslateString($"text")
            string interpolationPattern = @"LanguageManager\.TranslateString\s*\(\s*\$""([^""]*)""\s*\)";
            ExtractMatches(content, interpolationPattern, collectedStrings, fileName, true);

            // Pattern 3: Multi-line strings with @
            // LanguageManager.TranslateString(@"text")
            string verbatimPattern = @"LanguageManager\.TranslateString\s*\(\s*@""([^""]*)""\s*\)";
            ExtractMatches(content, verbatimPattern, collectedStrings, fileName);

            // Pattern 4: Constants or readonly strings assigned to TranslateString calls
            ExtractVariableReferences(content, collectedStrings, fileName);
        }

        /// <summary>
        /// Extracts CustomMessageBox.Show call strings from source code content
        /// </summary>
        private static void ExtractCustomMessageBoxCalls(string content, Dictionary<string, string> messageBoxStrings, string fileName)
        {
            // Pattern 1: Basic CustomMessageBox.Show calls
            // CustomMessageBox.Show("title", "message", icon, buttons)
            string pattern = @"CustomMessageBox\.Show\s*\(\s*""([^""]*)""\s*,\s*""([^""]*)""\s*,";
            ExtractMessageBoxMatches(content, pattern, messageBoxStrings, fileName);

            // Pattern 2: String interpolation
            // CustomMessageBox.Show("title", $"message", icon, buttons)
            string interpolationPattern = @"CustomMessageBox\.Show\s*\(\s*""([^""]*)""\s*,\s*\$""([^""]*)""\s*,";
            ExtractMessageBoxMatches(content, interpolationPattern, messageBoxStrings, fileName, true);

            // Pattern 3: Multi-line strings
            string multiLinePattern = @"CustomMessageBox\.Show\s*\(\s*@""([^""]*)""\s*,\s*@""([^""]*)""\s*,";
            ExtractMessageBoxMatches(content, multiLinePattern, messageBoxStrings, fileName);
        }

        /// <summary>
        /// Extracts matches from regex pattern and adds to collection
        /// </summary>
        private static void ExtractMatches(string content, string pattern, Dictionary<string, string> collectedStrings,
            string fileName, bool isInterpolated = false)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string text = match.Groups[1].Value.Trim();

                    // Skip empty strings
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    // Clean up interpolated strings (remove variable placeholders for key generation)
                    string cleanText = isInterpolated ? CleanInterpolatedString(text) : text;

                    // Generate key and add to collection
                    string key = GetStringKey(cleanText);
                    if (!collectedStrings.ContainsKey(key))
                    {
                        collectedStrings[key] = cleanText;
                        Log.Write(2, $"Found TranslateString: '{cleanText}' in {fileName}");
                    }
                }
            }
        }

        /// <summary>
        /// Extracts CustomMessageBox matches and adds both title and message to collection
        /// </summary>
        private static void ExtractMessageBoxMatches(string content, string pattern, Dictionary<string, string> messageBoxStrings,
            string fileName, bool isInterpolated = false)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string title = match.Groups[1].Value.Trim();
                    string message = match.Groups[2].Value.Trim();

                    // Process title
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        string cleanTitle = isInterpolated ? CleanInterpolatedString(title) : title;
                        string titleKey = GetStringKey(cleanTitle);
                        if (!messageBoxStrings.ContainsKey(titleKey))
                        {
                            messageBoxStrings[titleKey] = cleanTitle;
                            Log.Write(2, $"Found CustomMessageBox title: '{cleanTitle}' in {fileName}");
                        }
                    }

                    // Process message
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string cleanMessage = isInterpolated ? CleanInterpolatedString(message) : message;
                        string messageKey = GetStringKey(cleanMessage);
                        if (!messageBoxStrings.ContainsKey(messageKey))
                        {
                            messageBoxStrings[messageKey] = cleanMessage;
                            Log.Write(2, $"Found CustomMessageBox message: '{cleanMessage}' in {fileName}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleans interpolated strings by removing variable placeholders
        /// </summary>
        private static string CleanInterpolatedString(string interpolatedText)
        {
            // Remove common interpolation patterns like {variable}, {property}, etc.
            string cleaned = InterpolationPattern().Replace(interpolatedText, "[VAR]");

            // Remove [VAR] placeholders for key generation (but keep original for display)
            return cleaned.Replace("[VAR]", "").Trim();
        }

        /// <summary>
        /// Attempts to find variable references passed to TranslateString
        /// </summary>
        private static void ExtractVariableReferences(string content, Dictionary<string, string> collectedStrings, string fileName)
        {
            // Look for patterns like:
            // const string SomeText = "value";
            // static readonly string SomeText = "value";
            // string someVar = "value";
            string variablePattern = @"(?:const\s+string|static\s+readonly\s+string|string)\s+(\w+)\s*=\s*""([^""]*)""\s*;";
            MatchCollection variableMatches = Regex.Matches(content, variablePattern);

            Dictionary<string, string> variables = [];
            foreach (Match match in variableMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    string varName = match.Groups[1].Value;
                    string varValue = match.Groups[2].Value;
                    variables[varName] = varValue;
                }
            }

            // Now find TranslateString calls using these variables
            foreach (KeyValuePair<string, string> variable in variables)
            {
                string usagePattern = $@"LanguageManager\.TranslateString\s*\(\s*{variable.Key}\s*\)";
                if (Regex.IsMatch(content, usagePattern))
                {
                    string key = GetStringKey(variable.Value);
                    if (!collectedStrings.ContainsKey(key))
                    {
                        collectedStrings[key] = variable.Value;
                        Log.Write(2, $"Found TranslateString variable: '{variable.Value}' ({variable.Key}) in {fileName}");
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to get string key (same as LanguageManager.GetStringKey)
        /// </summary>
        private static string GetStringKey(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Remove common formatting and clean up
            string cleanText = text
                .Replace("\\n", " ")
                .Replace("\\r", "")
                .Replace("\\t", " ")
                .Trim();

            // Capitalize first letter of each word
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string titleCaseText = textInfo.ToTitleCase(cleanText.ToLower());

            // Remove spaces, punctuation, and special characters for key
            string finalText = NonWordCharacters().Replace(titleCaseText, "");

            return $"single_string_{finalText}";
        }

        // File Generation

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