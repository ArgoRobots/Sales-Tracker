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

                // Calculate dynamic threshold for "expensive" items based on current data
                decimal? expensiveThreshold = null;
                try
                {
                    var selectedDataGridView = MainMenu_Form.Instance?.SelectedDataGridView;
                    if (selectedDataGridView != null)
                    {
                        expensiveThreshold = PercentileCalculator.GetExpensiveItemThreshold(selectedDataGridView);
                        Log.WriteWithFormat(2, "Calculated dynamic expensive threshold: {0:F2}", expensiveThreshold.Value);
                    }
                }
                catch (Exception thresholdEx)
                {
                    Log.WriteWithFormat(1, "Failed to calculate dynamic threshold, using default: {0}", thresholdEx.Message);
                    expensiveThreshold = null; // Will use default 200 in translator
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                _translatedQuery = await _queryTranslator.TranslateQueryAsync(naturalLanguageQuery, expensiveThreshold);
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