using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.ImportSpreadsheet;
using Sales_Tracker.Passwords;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Settings;
using Sales_Tracker.Settings.Menus;
using Sales_Tracker.Startup;
using Sales_Tracker.Startup.Menus;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Utility class for generating translation JSON files for all application forms and controls.
    /// This class is intended for admin use only and should not be included in the production build.
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

                    // ReturnProduct
                    typeof(ReturnProduct_Form),
                    typeof(UndoReturn_Form),

                    // Settings
                    typeof(General_Form),
                    typeof(Security_Form),
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
                    typeof(Updates_Form),
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
                controlsToTranslate.AddRange(DateRange_Form.Instance.GetCustomRangeControls());

                // Create output directory
                string outputDirectory = Path.Combine(Directories.Downloads_dir, "Sales Tracker Translations");
                Directory.CreateDirectory(outputDirectory);

                // Collect all texts to translate (English source)
                progressForm.UpdateLanguage("English", 0);
                progressForm.UpdateTranslationProgress(0, "Collecting UI control texts...");

                // Collect UI control texts
                Dictionary<string, string> sourceTexts = CollectTextsToTranslate(controlsToTranslate);

                // Get all TranslateString calls from source code
                progressForm.UpdateTranslationProgress(sourceTexts.Count, "Scanning source code for TranslateString calls...");
                Dictionary<string, string> translateStringCalls = CollectAllTranslateStringCalls();

                // Get all CustomMessageBox calls from source code
                progressForm.UpdateTranslationProgress(sourceTexts.Count + translateStringCalls.Count, "Scanning source code for CustomMessageBox calls...");
                Dictionary<string, string> messageBoxStrings = CollectAllCustomMessageBoxCalls();

                // Get all Log.Write calls from source code
                progressForm.UpdateTranslationProgress(sourceTexts.Count + translateStringCalls.Count + messageBoxStrings.Count, "Scanning source code for Log.Write calls...");
                Dictionary<string, string> logWriteStrings = CollectAllLogWriteCalls();

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

                foreach (KeyValuePair<string, string> kvp in logWriteStrings)
                {
                    if (!sourceTexts.ContainsKey(kvp.Key))
                    {
                        sourceTexts[kvp.Key] = kvp.Value;
                    }
                }

                Log.WriteWithFormat(1, "Total texts collected: {0} ({1} from TranslateString calls, {2} from CustomMessageBox calls, {3} from Log.Write/WriteWithFormat calls)", sourceTexts.Count, translateStringCalls.Count, messageBoxStrings.Count, logWriteStrings.Count);

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

                    Log.WriteWithFormat(1, "Completed translations for {0} ({1} texts)", language.Key, translations.Count);
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

            Log.WriteWithFormat(1, "Optimized texts: {0} -> {1} (saved {2} unnecessary translations)", sourceTexts.Count, optimizedTexts.Count, sourceTexts.Count - optimizedTexts.Count);
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

            Log.WriteWithFormat(1, "Starting translation for {0}: {1} texts in {2} batches", targetLanguageAbbreviation, textsToTranslate.Count, batches.Count);

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
                        Log.WriteWithFormat(1, "Completed batch {0}/{1} ({2} characters, {3} total this hour)", i + 1, batches.Count, batchCharacters, _charactersUsedThisHour);
                        break;  // Success, exit retry loop
                    }
                    catch (HttpRequestException ex) when ((ex.Message.Contains("429") || ex.Message.Contains("rate")) && retryCount < maxRetries)
                    {
                        retryCount++;
                        int backoffDelay = (int)Math.Pow(2, retryCount) * 5000;  // 10s, 20s, 40s
                        Log.WriteWithFormat(1, "Rate limit hit on batch {0}, attempt {1}/{2}. Waiting {3}s before retry...", i + 1, retryCount, maxRetries, backoffDelay / 1000);

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
                            Log.WriteWithFormat(1, "Using original texts for batch {0} after {1} failed attempts", i + 1, maxRetries);
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

            Log.WriteWithFormat(1, "Created {0} batches from {1} items", batches.Count, items.Count);
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
                        Log.WriteWithFormat(1, "Hourly character limit would be exceeded. Waiting {0} minutes until reset...", waitTime.TotalMinutes.ToString("F1"));
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
                        Log.WriteWithFormat(1, "Request rate limit reached. Waiting {0} seconds...", waitTime.TotalSeconds.ToString("F0"));
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
                    if (ShouldSkipFile(file)) { continue; }

                    string content = File.ReadAllText(file);
                    ExtractTranslateStringCalls(content, collectedStrings, Path.GetFileName(file));
                }

                Log.WriteWithFormat(1, "Auto-collected {0} TranslateString calls from source code", collectedStrings.Count);
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error collecting TranslateString calls: {0}", ex.Message);
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
                    if (ShouldSkipFile(file)) { continue; }

                    string content = File.ReadAllText(file);
                    ExtractCustomMessageBoxCalls(content, messageBoxStrings, Path.GetFileName(file));
                }

                Log.WriteWithFormat(1, "Auto-collected {0} CustomMessageBox calls from source code", messageBoxStrings.Count);
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error collecting CustomMessageBox calls: {0}", ex.Message);
            }

            return messageBoxStrings;
        }

        /// <summary>
        /// Finds the project directory containing source files.
        /// </summary>
        private static string? FindProjectDirectory()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Go up directories to find source files
            while (currentDirectory != null)
            {
                // Look for .cs files or .csproj files to identify project root
                if (Directory.GetFiles(currentDirectory, "*.cs", SearchOption.TopDirectoryOnly).Length != 0
                    || Directory.GetFiles(currentDirectory, "*.csproj", SearchOption.TopDirectoryOnly).Length != 0)
                {
                    return currentDirectory;
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }

        /// <summary>
        /// Determines if a file should be skipped during scanning.
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
        /// Extracts LanguageManager.TranslateString call strings from source code content.
        /// </summary>
        private static void ExtractTranslateStringCalls(string content, Dictionary<string, string> collectedStrings, string fileName)
        {
            // Pattern 1: Direct LanguageManager.TranslateString calls
            // LanguageManager.TranslateString("text with \n newlines and \" quotes")
            string simplePattern = @"LanguageManager\.TranslateString\s*\(\s*""((?:[^""\\]|\\.)*)""";
            ExtractMatches(content, simplePattern, collectedStrings, fileName, false);

            // Pattern 2: String interpolation
            // LanguageManager.TranslateString($"text with \n and {variable}")
            string interpolationPattern = @"LanguageManager\.TranslateString\s*\(\s*\$""((?:[^""\\]|\\.)*)""";
            ExtractMatches(content, interpolationPattern, collectedStrings, fileName, false);

            // Pattern 3: Verbatim strings
            // LanguageManager.TranslateString(@"text with actual newlines and "" quotes")
            string verbatimPattern = @"LanguageManager\.TranslateString\s*\(\s*@""((?:[^""]|"""")*)""";
            ExtractMatches(content, verbatimPattern, collectedStrings, fileName, true);

            // Pattern 4: Multi-line regular strings
            string multilinePattern = @"LanguageManager\.TranslateString\s*\(\s*""((?:[^""\\]|\\.|[\r\n])*?)""";
            ExtractMatches(content, multilinePattern, collectedStrings, fileName, false);

            // Pattern 5: Constants or readonly strings assigned to TranslateString calls
            ExtractVariableReferences(content, collectedStrings, fileName);

            // Translate("text")
            string translateSimplePattern = @"Translate\s*\(\s*""((?:[^""\\]|\\.)*)""";
            ExtractMatches(content, translateSimplePattern, collectedStrings, fileName, false);

            // Pattern 6: Translate() wrapper - string interpolation
            // Translate($"text with {variable}")
            string translateInterpolationPattern = @"Translate\s*\(\s*\$""((?:[^""\\]|\\.)*)""";
            ExtractMatches(content, translateInterpolationPattern, collectedStrings, fileName, false);

            // Pattern 7: Translate() wrapper - verbatim strings
            // Translate(@"text with actual newlines")
            string translateVerbatimPattern = @"Translate\s*\(\s*@""((?:[^""]|"""")*)""";
            ExtractMatches(content, translateVerbatimPattern, collectedStrings, fileName, true);

            // Pattern 8: Translate() wrapper - multi-line strings
            string translateMultilinePattern = @"Translate\s*\(\s*""((?:[^""\\]|\\.|[\r\n])*?)""";
            ExtractMatches(content, translateMultilinePattern, collectedStrings, fileName, false);

            // Pattern 9: Constants or readonly strings assigned to TranslateString calls
            ExtractVariableReferences(content, collectedStrings, fileName);
        }

        /// <summary>
        /// Extracts CustomMessageBox.Show and CustomMessageBox.ShowWithFormat call strings from source code content.
        /// </summary>
        private static void ExtractCustomMessageBoxCalls(string content, Dictionary<string, string> messageBoxStrings, string fileName)
        {
            // Pattern 1: Basic CustomMessageBox.Show calls
            string pattern = @"CustomMessageBox\.Show\s*\(\s*""((?:[^""\\]|\\.)*?)""\s*,\s*""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractMessageBoxMatches(content, pattern, messageBoxStrings, fileName, false);

            // Pattern 2: String interpolation
            string interpolationPattern = @"CustomMessageBox\.Show\s*\(\s*""((?:[^""\\]|\\.)*?)""\s*,\s*\$""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractMessageBoxMatches(content, interpolationPattern, messageBoxStrings, fileName, false);

            // Pattern 3: Multi-line strings
            string multiLinePattern = @"CustomMessageBox\.Show\s*\(\s*@""((?:[^""]|"""")*?)""\s*,\s*@""((?:[^""]|"""")*?)""\s*,";
            ExtractMessageBoxMatches(content, multiLinePattern, messageBoxStrings, fileName, true);

            // Pattern 4: Basic CustomMessageBox.ShowWithFormat calls
            string showWithFormatPattern = @"CustomMessageBox\.ShowWithFormat\s*\(\s*""((?:[^""\\]|\\.)*?)""\s*,\s*""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractMessageBoxMatches(content, showWithFormatPattern, messageBoxStrings, fileName, false);

            // Pattern 5: String interpolation for ShowWithFormat
            string showWithFormatInterpolationPattern = @"CustomMessageBox\.ShowWithFormat\s*\(\s*""((?:[^""\\]|\\.)*?)""\s*,\s*\$""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractMessageBoxMatches(content, showWithFormatInterpolationPattern, messageBoxStrings, fileName, false);

            // Pattern 6: Multi-line strings for ShowWithFormat
            string showWithFormatMultiLinePattern = @"CustomMessageBox\.ShowWithFormat\s*\(\s*@""((?:[^""]|"""")*?)""\s*,\s*@""((?:[^""]|"""")*?)""\s*,";
            ExtractMessageBoxMatches(content, showWithFormatMultiLinePattern, messageBoxStrings, fileName, true);

            // Pattern 7: Mixed patterns - Show with interpolated title, ShowWithFormat with regular message
            string showWithFormatMixedPattern1 = @"CustomMessageBox\.ShowWithFormat\s*\(\s*\$""((?:[^""\\]|\\.)*?)""\s*,\s*""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractMessageBoxMatches(content, showWithFormatMixedPattern1, messageBoxStrings, fileName, false);

            // Pattern 8: Mixed patterns - Show with regular title, ShowWithFormat with verbatim message
            string showWithFormatMixedPattern2 = @"CustomMessageBox\.ShowWithFormat\s*\(\s*""((?:[^""\\]|\\.)*?)""\s*,\s*@""((?:[^""]|"""")*?)""\s*,";
            ExtractMessageBoxMatches(content, showWithFormatMixedPattern2, messageBoxStrings, fileName, true);
        }

        /// <summary>
        /// Extracts matches from regex pattern with improved handling for escaped characters and newlines.
        /// </summary>
        private static void ExtractMatches(string content, string pattern, Dictionary<string, string> collectedStrings, string fileName, bool isVerbatim)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string rawText = match.Groups[1].Value;

                    // Process strings based on type
                    string processedText = isVerbatim
                        ? ProcessVerbatimString(rawText).Trim()
                        : ProcessEscapedCharacters(rawText).Trim();

                    // Skip empty strings
                    if (string.IsNullOrWhiteSpace(processedText)) { continue; }

                    // Generate key and add to collection
                    string key = LanguageManager.GetStringKey(processedText);
                    if (!collectedStrings.ContainsKey(key))
                    {
                        collectedStrings[key] = processedText;
                        Log.WriteWithFormat(2, "Found TranslateString: '{0}' -> Key: '{1}' in {2}", processedText, key, fileName);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts CustomMessageBox matches and adds both title and message to collection with proper string handling.
        /// </summary>
        private static void ExtractMessageBoxMatches(string content, string pattern, Dictionary<string, string> messageBoxStrings, string fileName, bool isVerbatim)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string title = match.Groups[1].Value.Trim();
                    string message = match.Groups[2].Value.Trim();

                    // Process strings based on type
                    if (isVerbatim)
                    {
                        title = ProcessVerbatimString(title);
                        message = ProcessVerbatimString(message);
                    }
                    else
                    {
                        title = ProcessEscapedCharacters(title);
                        message = ProcessEscapedCharacters(message);
                    }

                    // Process title
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        string titleKey = LanguageManager.GetStringKey(title);
                        if (!messageBoxStrings.ContainsKey(titleKey))
                        {
                            messageBoxStrings[titleKey] = title;
                            Log.WriteWithFormat(2, "Found CustomMessageBox title: '{0}' -> Key: '{1}' in {2}", title, titleKey, fileName);
                        }
                    }

                    // Process message
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string messageKey = LanguageManager.GetStringKey(message);
                        if (!messageBoxStrings.ContainsKey(messageKey))
                        {
                            messageBoxStrings[messageKey] = message;
                            Log.WriteWithFormat(2, "Found CustomMessageBox message: '{0}' -> Key: '{1}' in {2}", message, messageKey, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes escaped characters in regular (non-verbatim) strings.
        /// </summary>
        private static string ProcessEscapedCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder result = new();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\' && i + 1 < input.Length)
                {
                    char nextChar = input[i + 1];
                    switch (nextChar)
                    {
                        case 'n':
                            result.Append('\n');
                            i++;  // Skip the next character
                            break;
                        case 'r':
                            result.Append('\r');
                            i++;
                            break;
                        case 't':
                            result.Append('\t');
                            i++;
                            break;
                        case '\\':
                            result.Append('\\');
                            i++;
                            break;
                        case '"':
                            result.Append('"');
                            i++;
                            break;
                        case '\'':
                            result.Append('\'');
                            i++;
                            break;
                        default:
                            // For unknown escape sequences, keep them as-is
                            result.Append(input[i]);
                            break;
                    }
                }
                else
                {
                    result.Append(input[i]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Processes verbatim strings (replace doubled quotes with single quotes).
        /// </summary>
        private static string ProcessVerbatimString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // In verbatim strings, "" becomes "
            return input.Replace("\"\"", "\"");
        }

        /// <summary>
        /// Attempts to find variable references passed to TranslateString.
        /// </summary>
        private static void ExtractVariableReferences(string content, Dictionary<string, string> collectedStrings, string fileName)
        {
            // Look for patterns like:
            // const string SomeText = "value";
            // static readonly string SomeText = "value";
            // string someVar = "value";
            string variablePattern = @"(?:const\s+string|static\s+readonly\s+string|string)\s+(\w+)\s*=\s*""((?:[^""\\]|\\.)*)""(?:\s*;|\s*,)";
            MatchCollection variableMatches = Regex.Matches(content, variablePattern, RegexOptions.Multiline | RegexOptions.Singleline);

            Dictionary<string, string> variables = [];
            foreach (Match match in variableMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    string varName = match.Groups[1].Value;
                    string varValue = ProcessEscapedCharacters(match.Groups[2].Value);
                    variables[varName] = varValue;
                }
            }

            // Also look for verbatim string variables
            string verbatimVariablePattern = @"(?:const\s+string|static\s+readonly\s+string|string)\s+(\w+)\s*=\s*@""((?:[^""]|"""")*)""(?:\s*;|\s*,)";
            MatchCollection verbatimVariableMatches = Regex.Matches(content, verbatimVariablePattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in verbatimVariableMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    string varName = match.Groups[1].Value;
                    string varValue = ProcessVerbatimString(match.Groups[2].Value);
                    variables[varName] = varValue;
                }
            }

            // Now find TranslateString calls using these variables
            foreach (KeyValuePair<string, string> variable in variables)
            {
                string usagePattern = $@"LanguageManager\.TranslateString\s*\(\s*{variable.Key}\s*\)";
                if (Regex.IsMatch(content, usagePattern))
                {
                    string key = LanguageManager.GetStringKey(variable.Value);
                    if (!collectedStrings.ContainsKey(key))
                    {
                        collectedStrings[key] = variable.Value;
                        Log.WriteWithFormat(2, "Found TranslateString variable: '{0}' ({1}) -> Key: '{2}' in {3}", variable.Value, variable.Key, key, fileName);
                    }
                }
            }
        }

        /// <summary>
        /// Automatically scans source files for Log.Write calls and extracts text strings.
        /// </summary>
        private static Dictionary<string, string> CollectAllLogWriteCalls()
        {
            Dictionary<string, string> logWriteStrings = [];

            try
            {
                string projectDirectory = FindProjectDirectory();

                if (projectDirectory == null)
                {
                    Log.Write(1, "Could not find source directory for Log.Write collection");
                    return logWriteStrings;
                }

                // Search all .cs files for Log.Write calls
                string[] csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                foreach (string file in csFiles)
                {
                    if (ShouldSkipFile(file)) { continue; }

                    string content = File.ReadAllText(file);
                    ExtractLogWriteCalls(content, logWriteStrings, Path.GetFileName(file));
                }

                Log.WriteWithFormat(1, "Auto-collected {0} Log.Write/WriteWithFormat calls from source code", logWriteStrings.Count);
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error collecting Log.Write/WriteWithFormat calls: {0}", ex.Message);
            }

            return logWriteStrings;
        }

        /// <summary>
        /// Extracts Log.Write call strings from source code content.
        /// </summary>
        private static void ExtractLogWriteCalls(string content, Dictionary<string, string> logWriteStrings, string fileName)
        {
            // ============ LOG.WRITE PATTERNS ============

            // Pattern 1: Basic Log.Write calls
            // Log.Write(1, "message text")
            string pattern = @"Log\.Write\s*\(\s*\d+\s*,\s*""((?:[^""\\]|\\.)*?)""";
            ExtractLogMatches(content, pattern, logWriteStrings, fileName, false);

            // Pattern 2: String interpolation
            // Log.Write(1, $"message with {variable}")
            string interpolationPattern = @"Log\.Write\s*\(\s*\d+\s*,\s*\$""((?:[^""\\]|\\.)*?)""";
            ExtractLogMatchesWithInterpolation(content, interpolationPattern, logWriteStrings, fileName);

            // Pattern 3: Verbatim strings
            // Log.Write(1, @"message with actual newlines")
            string verbatimPattern = @"Log\.Write\s*\(\s*\d+\s*,\s*@""((?:[^""]|"""")*)""";
            ExtractLogMatches(content, verbatimPattern, logWriteStrings, fileName, true);

            // Pattern 4: Multi-line strings
            string multilinePattern = @"Log\.Write\s*\(\s*\d+\s*,\s*""((?:[^""\\]|\\.|[\r\n])*?)""";
            ExtractLogMatches(content, multilinePattern, logWriteStrings, fileName, false);

            // Pattern 5: String concatenation with + operator
            // Log.Write(1, "message part 1" + variable + "message part 2")
            ExtractLogConcatenationMatches(content, logWriteStrings, fileName);

            // Pattern 6: Variable references
            ExtractLogVariableReferences(content, logWriteStrings, fileName);

            // ============ LOG.WRITEWITHFORMAT PATTERNS ============

            // Pattern 7: Basic Log.WriteWithFormat calls
            // Log.WriteWithFormat(1, "message template {0}", variable)
            string writeWithFormatPattern = @"Log\.WriteWithFormat\s*\(\s*\d+\s*,\s*""((?:[^""\\]|\\.)*?)""\s*,";
            ExtractLogWriteWithFormatMatches(content, writeWithFormatPattern, logWriteStrings, fileName, false);

            // Pattern 8: Verbatim strings for WriteWithFormat
            // Log.WriteWithFormat(1, @"message template {0}", variable)
            string writeWithFormatVerbatimPattern = @"Log\.WriteWithFormat\s*\(\s*\d+\s*,\s*@""((?:[^""]|"""")*?)""\s*,";
            ExtractLogWriteWithFormatMatches(content, writeWithFormatVerbatimPattern, logWriteStrings, fileName, true);

            // Pattern 9: Multi-line strings for WriteWithFormat
            string writeWithFormatMultilinePattern = @"Log\.WriteWithFormat\s*\(\s*\d+\s*,\s*""((?:[^""\\]|\\.|[\r\n])*?)""\s*,";
            ExtractLogWriteWithFormatMatches(content, writeWithFormatMultilinePattern, logWriteStrings, fileName, false);
        }

        /// <summary>
        /// Extracts Log.WriteWithFormat template strings.
        /// </summary>
        private static void ExtractLogWriteWithFormatMatches(string content, string pattern, Dictionary<string, string> logWriteStrings, string fileName, bool isVerbatim)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string template = match.Groups[1].Value.Trim();

                    // Process strings based on type
                    if (isVerbatim)
                    {
                        template = ProcessVerbatimString(template);
                    }
                    else
                    {
                        template = ProcessEscapedCharacters(template);
                    }

                    // Process template
                    if (!string.IsNullOrWhiteSpace(template))
                    {
                        string templateKey = LanguageManager.GetStringKey(template);
                        if (!logWriteStrings.ContainsKey(templateKey))
                        {
                            logWriteStrings[templateKey] = template;
                            Log.WriteWithFormat(2, "Found Log.WriteWithFormat template: '{0}' -> Key: '{1}' in {2}", template, templateKey, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts Log.Write matches with string interpolation, removing interpolation placeholders.
        /// </summary>
        private static void ExtractLogMatchesWithInterpolation(string content, string pattern, Dictionary<string, string> logWriteStrings, string fileName)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string message = match.Groups[1].Value.Trim();
                    message = ProcessEscapedCharacters(message);

                    // Remove interpolation placeholders {variable} and replace with generic placeholder
                    message = InterpolationPattern().Replace(message, "{0}");

                    // Process message
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string messageKey = LanguageManager.GetStringKey(message);
                        if (!logWriteStrings.ContainsKey(messageKey))
                        {
                            logWriteStrings[messageKey] = message;
                            Log.WriteWithFormat(2, "Found Log.Write interpolated message: '{0}' -> Key: '{1}' in {2}", message, messageKey, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts Log.Write matches with string concatenation, capturing only the string literal parts.
        /// </summary>
        private static void ExtractLogConcatenationMatches(string content, Dictionary<string, string> logWriteStrings, string fileName)
        {
            // Look for Log.Write calls that contain string concatenation
            string fullCallPattern = @"Log\.Write\s*\(\s*\d+\s*,\s*([^;)]+)\s*\)";
            MatchCollection matches = Regex.Matches(content, fullCallPattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string fullExpression = match.Groups[1].Value.Trim();

                    // Extract all string literals from the concatenation
                    string stringLiteralPattern = @"""((?:[^""\\]|\\.)*)""";
                    MatchCollection stringMatches = Regex.Matches(fullExpression, stringLiteralPattern);

                    foreach (Match stringMatch in stringMatches)
                    {
                        if (stringMatch.Groups.Count >= 2)
                        {
                            string stringLiteral = stringMatch.Groups[1].Value.Trim();
                            stringLiteral = ProcessEscapedCharacters(stringLiteral);

                            // Only process non-empty strings that aren't just whitespace
                            if (!string.IsNullOrWhiteSpace(stringLiteral) && stringLiteral.Length > 2)
                            {
                                string messageKey = LanguageManager.GetStringKey(stringLiteral);
                                if (!logWriteStrings.ContainsKey(messageKey))
                                {
                                    logWriteStrings[messageKey] = stringLiteral;
                                    Log.WriteWithFormat(2, "Found Log.Write concatenated string: '{0}' -> Key: '{1}' in {2}", stringLiteral, messageKey, fileName);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts Log.Write matches and adds the message text to collection with proper string handling.
        /// </summary>
        private static void ExtractLogMatches(string content, string pattern, Dictionary<string, string> logWriteStrings, string fileName, bool isVerbatim)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    string message = match.Groups[1].Value.Trim();

                    // Process strings based on type
                    if (isVerbatim)
                    {
                        message = ProcessVerbatimString(message);
                    }
                    else
                    {
                        message = ProcessEscapedCharacters(message);
                    }

                    // Process message
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string messageKey = LanguageManager.GetStringKey(message);
                        if (!logWriteStrings.ContainsKey(messageKey))
                        {
                            logWriteStrings[messageKey] = message;
                            Log.WriteWithFormat(2, "Found Log.Write message: '{0}' -> Key: '{1}' in {2}", message, messageKey, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to find variable references passed to Log.Write calls.
        /// </summary>
        private static void ExtractLogVariableReferences(string content, Dictionary<string, string> logWriteStrings, string fileName)
        {
            // Look for patterns like:
            // const string SomeText = "value";
            // static readonly string SomeText = "value";
            // string someVar = "value";
            string variablePattern = @"(?:const\s+string|static\s+readonly\s+string|string)\s+(\w+)\s*=\s*""((?:[^""\\]|\\.)*)""(?:\s*;|\s*,)";
            MatchCollection variableMatches = Regex.Matches(content, variablePattern, RegexOptions.Multiline | RegexOptions.Singleline);

            Dictionary<string, string> variables = [];
            foreach (Match match in variableMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    string varName = match.Groups[1].Value;
                    string varValue = ProcessEscapedCharacters(match.Groups[2].Value);
                    variables[varName] = varValue;
                }
            }

            // Also look for verbatim string variables
            string verbatimVariablePattern = @"(?:const\s+string|static\s+readonly\s+string|string)\s+(\w+)\s*=\s*@""((?:[^""]|"""")*)""(?:\s*;|\s*,)";
            MatchCollection verbatimVariableMatches = Regex.Matches(content, verbatimVariablePattern, RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in verbatimVariableMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    string varName = match.Groups[1].Value;
                    string varValue = ProcessVerbatimString(match.Groups[2].Value);
                    variables[varName] = varValue;
                }
            }

            // Now find Log.Write calls using these variables
            foreach (KeyValuePair<string, string> variable in variables)
            {
                string usagePattern = $@"Log\.Write\s*\(\s*\d+\s*,\s*{variable.Key}\s*\)";
                if (Regex.IsMatch(content, usagePattern))
                {
                    string key = LanguageManager.GetStringKey(variable.Value);
                    if (!logWriteStrings.ContainsKey(key))
                    {
                        logWriteStrings[key] = variable.Value;
                        Log.WriteWithFormat(2, "Found Log.Write variable: '{0}' ({1}) -> Key: '{2}' in {3}", variable.Value, variable.Key, key, fileName);
                    }
                }
            }
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
                Log.WriteWithFormat(1, "Generated translation file for {0} at {1}", languageCode, outputPath);
            }
            catch (Exception ex)
            {
                Log.Error_GetTranslation($"Failed to generate translation file for {languageCode}: {ex.Message}");
            }
        }
    }
}