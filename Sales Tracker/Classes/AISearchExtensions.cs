using Guna.UI2.WinForms;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Extension to the existing search functionality to add AI capabilities.
    /// </summary>
    public static class AISearchExtensions
    {
        private static AIQueryTranslator _queryTranslator;

        // Store the original query and its translation
        private static string _originalQuery = "";
        private static string _translatedQuery = "";

        // Flag to indicate if we're currently using an AI-translated query
        public static bool IsUsingAIQuery { get; private set; } = false;

        public static void InitializeAISearch(string apiKey)
        {
            _queryTranslator = new AIQueryTranslator(apiKey);
            Log.Write(2, "AI Search functionality initialized");
        }

        /// <summary>
        /// Enhances the search box with AI query translation without replacing the input text.
        /// </summary>
        public static async Task EnhanceSearchAsync(this Guna2TextBox searchBox)
        {
            try
            {
                string userQuery = searchBox.Text;
                string naturalLanguageQuery = userQuery.Substring(1).Trim();

                // Store the original query (for display in the results label)
                _originalQuery = naturalLanguageQuery;

                // Translate the query
                _translatedQuery = await _queryTranslator.TranslateQueryAsync(naturalLanguageQuery);
                IsUsingAIQuery = true;

                Log.Write(2, $"AI translated '{naturalLanguageQuery}' to '{_translatedQuery}'");
                TriggerSearchWithTranslatedQuery();

            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error enhancing search: {ex.Message}");
                IsUsingAIQuery = false;
            }
        }
        public static void ResetQuery()
        {
            // Reset the AI query flag if the user is doing a normal search
            IsUsingAIQuery = false;
            _originalQuery = "";
            _translatedQuery = "";
        }

        /// <summary>
        /// Returns the translated query if using AI search, otherwise returns the original query.
        /// </summary>
        public static string GetEffectiveSearchQuery(string displayedQuery)
        {
            if (IsUsingAIQuery && displayedQuery.StartsWith('!'))
            {
                return _translatedQuery;
            }

            return displayedQuery;
        }

        /// <summary>
        /// Returns the query that should be displayed in the "Showing results for" label.
        /// </summary>
        public static string GetDisplayQuery(string displayedQuery)
        {
            if (IsUsingAIQuery && displayedQuery.StartsWith('!'))
            {
                return _originalQuery;
            }

            return displayedQuery;
        }
        private static void TriggerSearchWithTranslatedQuery()
        {
            // Get the MainMenu_Form instance
            MainMenu_Form mainForm = MainMenu_Form.Instance;
            if (mainForm == null)
            {
                Log.Write(0, "Could not find MainMenu_Form instance to trigger search.");
                return;
            }

            // Trigger the search directly
            mainForm.RefreshDataGridViewAndCharts();
        }
    }
}