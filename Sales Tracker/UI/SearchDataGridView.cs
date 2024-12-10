using Sales_Tracker.DataClasses;
using System.Text;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides advanced search functionality for DataGridView controls with support for exact phrases,
    /// exclusions, required terms, and fuzzy matching.
    /// </summary>
    public static class SearchDataGridView
    {
        /// <summary>
        /// Filters a DataGridView row based on advanced search criteria.
        /// - "exact phrase" for exact phrase matching
        /// - -term for excluding terms
        /// - +term for required terms
        /// - Regular terms for fuzzy matching
        /// </summary>
        /// <returns>True if the row matches the search criteria; otherwise, false.</returns>
        public static bool FilterRowByAdvancedSearch(DataGridViewRow row, string searchText, SearchOptions searchOptions = null)
        {
            ArgumentNullException.ThrowIfNull(row);

            SearchOptions options = searchOptions ?? new SearchOptions();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            List<SearchTerm> searchTerms;
            try
            {
                searchTerms = ParseSearchTerms(searchText);
            }
            catch (ArgumentException)
            {
                // If parsing fails due to invalid search terms, treat as no match
                return false;
            }

            List<string> searchableContent = GetSearchableContent(row);

            return searchTerms.All(term =>
            {
                if (term.IsExactPhrase)
                {
                    return searchableContent.Any(content =>
                    {
                        return content.Contains(term.Text, options.GetStringComparison());
                    });
                }

                if (term.IsExclusion)
                {
                    return !searchableContent.Any(content =>
                    {
                        return content.Contains(term.Text, options.GetStringComparison());
                    });
                }

                if (term.IsRequired)
                {
                    return searchableContent.Any(content =>
                    {
                        return content.Contains(term.Text, options.GetStringComparison());
                    });
                }

                return searchableContent.Any(content =>
                {
                    return IsFuzzyMatch(content, term.Text, options.GetMaxLevenshteinDistance());
                });
            });
        }

        private static List<SearchTerm> ParseSearchTerms(string searchText)
        {
            List<SearchTerm> terms = [];
            StringBuilder currentTerm = new();
            bool inQuotes = false;

            for (int i = 0; i < searchText.Length; i++)
            {
                char c = searchText[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    if (!inQuotes && currentTerm.Length > 0)
                    {
                        terms.Add(SearchTerm.Create(currentTerm.ToString(), isExactPhrase: true));
                        currentTerm.Clear();
                    }
                }
                else if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (currentTerm.Length > 0)
                    {
                        AddSearchTerm(terms, currentTerm.ToString());
                        currentTerm.Clear();
                    }
                }
                else
                {
                    currentTerm.Append(c);
                }
            }

            if (currentTerm.Length > 0)
            {
                AddSearchTerm(terms, currentTerm.ToString());
            }

            return terms;
        }
        private static void AddSearchTerm(List<SearchTerm> terms, string term)
        {
            if (term.Length == 1 && (term[0] == '+' || term[0] == '-'))
            {
                return; // Skip single character modifiers
            }

            try
            {
                if (term.StartsWith('-'))
                {
                    string termText = term[1..];
                    if (!string.IsNullOrWhiteSpace(termText))
                    {
                        terms.Add(SearchTerm.Create(termText, isExclusion: true));
                    }
                }
                else if (term.StartsWith('+'))
                {
                    string termText = term[1..];
                    if (!string.IsNullOrWhiteSpace(termText))
                    {
                        terms.Add(SearchTerm.Create(termText, isRequired: true));
                    }
                }
                else
                {
                    terms.Add(SearchTerm.Create(term));
                }
            }
            catch (ArgumentException)
            {
                // Skip invalid terms
            }
        }
        private static List<string> GetSearchableContent(DataGridViewRow row)
        {
            List<string> content = [];

            // Add visible cell values
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Value != null)
                {
                    content.Add(cell.Value.ToString());
                }
            }

            // Add tag data if available
            if (row.Tag is (List<string> items, TagData))
            {
                int itemsToTake = items.Count;
                if (items.Count > 0 && items[^1].StartsWith("Receipt:"))
                {
                    itemsToTake--;
                }

                for (int i = 0; i < itemsToTake; i++)
                {
                    string[] parts = items[i].Split(',');
                    for (int j = 0; j < Math.Min(4, parts.Length); j++)
                    {
                        content.Add(parts[j]);
                    }
                }
            }

            return content;
        }
        private static bool IsFuzzyMatch(string source, string target, int maxDistance)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                return false;
            }

            // Normalize strings
            string normalizedSource = source.ToLowerInvariant().Trim();
            string normalizedTarget = target.ToLowerInvariant().Trim();

            // Exact match check
            if (normalizedSource.Contains(normalizedTarget))
            {
                return true;
            }

            // If target is longer than source, no need to check
            if (normalizedTarget.Length > normalizedSource.Length + maxDistance)
            {
                return false;
            }

            // Check each word in source
            string[] sourceWords = normalizedSource.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] targetWords = normalizedTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string targetWord in targetWords)
            {
                bool wordMatched = false;
                foreach (string sourceWord in sourceWords)
                {
                    // Skip words with too much length difference
                    if (Math.Abs(sourceWord.Length - targetWord.Length) > maxDistance)
                    {
                        continue;
                    }

                    int distance = CalculateLevenshteinDistance(sourceWord, targetWord);
                    if (distance <= maxDistance)
                    {
                        wordMatched = true;
                        break;
                    }
                }

                if (!wordMatched)
                {
                    return false;
                }
            }

            return true;
        }
        private static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            }

            if (string.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            // Create matrix
            int[,] matrix = new int[source.Length + 1, target.Length + 1];

            // Initialize first row and column
            for (int i = 0; i <= source.Length; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                matrix[0, j] = j;
            }

            // Fill matrix
            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int substitutionCost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,     // Deletion
                            matrix[i, j - 1] + 1),    // Insertion
                        matrix[i - 1, j - 1] + substitutionCost  // Substitution
                    );

                    // Transposition check (for swapped characters)
                    if (i > 1 && j > 1 &&
                        source[i - 1] == target[j - 2] &&
                        source[i - 2] == target[j - 1])
                    {
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + 1);
                    }
                }
            }

            return matrix[source.Length, target.Length];
        }

        public sealed class SearchOptions
        {
            // Properties
            private readonly StringComparison _stringComparison;
            private readonly byte _maxLevenshteinDistance;

            public SearchOptions()
            {
                _stringComparison = StringComparison.OrdinalIgnoreCase;
                _maxLevenshteinDistance = 2;
            }
            public StringComparison GetStringComparison() => _stringComparison;
            public int GetMaxLevenshteinDistance() => _maxLevenshteinDistance;
        }

        private sealed class SearchTerm
        {
            // Properties
            private readonly string _text;
            private readonly SearchTermType _type;

            private SearchTerm(string text, SearchTermType type)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentException("Search term cannot be empty or whitespace.", nameof(text));
                }

                if (type.HasFlag(SearchTermType.Exclusion) && type.HasFlag(SearchTermType.Required))
                {
                    throw new ArgumentException("Search term cannot be both excluded and required.");
                }

                _text = text.Trim();
                _type = type;
            }

            // Getters
            public string Text => _text;
            public bool IsExactPhrase => _type.HasFlag(SearchTermType.ExactPhrase);
            public bool IsExclusion => _type.HasFlag(SearchTermType.Exclusion);
            public bool IsRequired => _type.HasFlag(SearchTermType.Required);

            public static SearchTerm Create(string text, bool isExactPhrase = false, bool isExclusion = false, bool isRequired = false)
            {
                SearchTermType type = SearchTermType.Normal;

                if (isExactPhrase)
                {
                    type |= SearchTermType.ExactPhrase;
                }

                if (isExclusion)
                {
                    type |= SearchTermType.Exclusion;
                }

                if (isRequired)
                {
                    type |= SearchTermType.Required;
                }

                return new SearchTerm(text, type);
            }

            [Flags]
            private enum SearchTermType
            {
                Normal = 0,
                ExactPhrase = 1,
                Exclusion = 2,
                Required = 4
            }
        }
    }
}