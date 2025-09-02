using Guna.UI2.WinForms;
using Sales_Tracker.AnonymousData;
using System.Diagnostics;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Extension to the existing search functionality to add AI capabilities.
    /// </summary>
    public static class AISearchExtensions
    {
        // Properties
        private static AIQueryTranslator _queryTranslator;
        private static string _originalQuery = "";
        private static string _translatedQuery = "";

        // Getter
        public static bool IsUsingAIQuery { get; private set; } = false;

        // Init.
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
                _originalQuery = naturalLanguageQuery;

                Stopwatch stopwatch = Stopwatch.StartNew();
                _translatedQuery = await _queryTranslator.TranslateQueryAsync(naturalLanguageQuery);
                stopwatch.Stop();

                // Track the API usage data
                AnonymousDataManager.AddOpenAIUsageData(AIQueryTranslator.Model, stopwatch.ElapsedMilliseconds, _queryTranslator.LastTokenUsage);

                IsUsingAIQuery = true;
                Log.WriteWithFormat(2, "AI translated '{0}' to '{1}' (Used {2} tokens)", naturalLanguageQuery, _translatedQuery, _queryTranslator.LastTokenUsage);
                MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
            }
            catch (Exception ex)
            {
                Log.Error_EnhancingSearch(ex.Message);
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
    }
}