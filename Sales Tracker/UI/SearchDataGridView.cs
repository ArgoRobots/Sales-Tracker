using Sales_Tracker.DataClasses;
using System.Text;
using System.Text.RegularExpressions;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides advanced search functionality for DataGridView controls with support for exact phrases,
    /// exclusions, required terms, and fuzzy matching.
    /// </summary>
    public static partial class SearchDataGridView
    {
        /// <summary>
        /// Filters a DataGridView row based on advanced search criteria.
        /// - "exact phrase" for exact phrase matching
        /// - -term for excluding terms
        /// - +term for required terms
        /// - Regular terms for fuzzy matching
        /// </summary>
        /// <returns>True if the row matches the search criteria, otherwise false.</returns>
        public static bool FilterRowByAdvancedSearch(DataGridViewRow row, string searchText, SearchOptions searchOptions = null)
        {
            ArgumentNullException.ThrowIfNull(row);

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            if (searchText.Contains(':'))
            {
                return FilterRowByStructuredSearch(row, searchText, searchOptions);
            }

            SearchOptions options = searchOptions ?? new SearchOptions();

            List<SearchTerm> searchTerms;
            try
            {
                searchTerms = ParseSearchTerms(searchText);
            }
            catch (ArgumentException)
            {
                // If parsing fails, treat it as no match
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

        // Normal search
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
                return;  // Skip single character modifiers
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

                    double similarity = CalculateNormalizedSimilarity(sourceWord, targetWord);
                    if (similarity >= 0.6)  // Using the threshold of 0.6
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
        private static double CalculateNormalizedSimilarity(string source, string target)
        {
            // Handle empty strings
            if (string.IsNullOrEmpty(source))
            {
                return string.IsNullOrEmpty(target) ? 1.0 : 0.0;
            }

            if (string.IsNullOrEmpty(target))
            {
                return 0.0;
            }

            int distance = CalculateLevenshteinDistance(source, target);
            int maxDistance = Math.Max(source.Length, target.Length);

            // Normalize to similarity (1.0 means identical, 0.0 means completely different)
            return 1.0 - ((double)distance / maxDistance);
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

        // AI search
        private static bool FilterRowByStructuredSearch(DataGridViewRow row, string searchText, SearchOptions searchOptions = null)
        {
            List<StructuredSearchTerm> structuredTerms = ParseStructuredSearchTerms(searchText);

            // Parse any remaining free-text terms that aren't in field:value format
            string remainingText = ExtractRemainingText(searchText, structuredTerms);
            List<SearchTerm> freeTextTerms = !string.IsNullOrWhiteSpace(remainingText)
                ? ParseSearchTerms(remainingText)
                : [];

            // If no terms found, consider it a match
            if (structuredTerms.Count == 0 && freeTextTerms.Count == 0)
            {
                return true;
            }

            List<string> searchableContent = GetSearchableContent(row);

            // Check structured field-specific terms
            bool structuredTermsMatch = structuredTerms.All(term =>
            {
                // Match against specific column/field
                DataGridViewColumn column = FindColumnByName(row.DataGridView, term.Field);
                if (column == null)
                {
                    // Field not found, can't be a match unless it's an exclusion
                    return term.IsExclusion;
                }

                // Get cell value
                object cellValue = row.Cells[column.Index].Value;
                if (cellValue == null)
                {
                    // No value means exclusion matches, required fails
                    return term.IsExclusion;
                }

                string stringValue = cellValue.ToString();

                // Handle OR conditions
                if (term.HasOrCondition)
                {
                    bool anyMatch = term.OrValues.Any(orValue =>
                    {
                        if (!string.IsNullOrEmpty(term.ComparisonOperator))
                        {
                            // Handle numeric comparisons with OR
                            if (IsNumericComparison(stringValue, orValue, term.ComparisonOperator, out bool comparisonResult))
                            {
                                return comparisonResult;
                            }

                            // Handle date comparisons with OR
                            if (IsDateField(column) && IsDateComparison(stringValue, orValue, term.ComparisonOperator, out bool dateComparisonResult))
                            {
                                return dateComparisonResult;
                            }
                        }

                        // Regular string comparison for OR values
                        StringComparison comparison = searchOptions?.GetStringComparison() ?? StringComparison.OrdinalIgnoreCase;
                        return stringValue.Contains(orValue, comparison);
                    });

                    // Flip result if it's an exclusion
                    return term.IsExclusion ? !anyMatch : anyMatch;
                }

                // Handle numeric comparisons
                if (!string.IsNullOrEmpty(term.ComparisonOperator))
                {
                    if (IsNumericComparison(stringValue, term.Value, term.ComparisonOperator, out bool comparisonResult))
                    {
                        // Flip result if it's an exclusion
                        return term.IsExclusion ? !comparisonResult : comparisonResult;
                    }
                }

                // Handle date comparisons
                if (IsDateField(column) && !string.IsNullOrEmpty(term.ComparisonOperator))
                {
                    if (IsDateComparison(stringValue, term.Value, term.ComparisonOperator, out bool dateComparisonResult))
                    {
                        return term.IsExclusion ? !dateComparisonResult : dateComparisonResult;
                    }
                }

                // Handle string matching with exact phrases
                StringComparison comparison = searchOptions?.GetStringComparison() ?? StringComparison.OrdinalIgnoreCase;
                bool isExactMatch = term.Value.StartsWith('\"') && term.Value.EndsWith('\"');

                if (isExactMatch)
                {
                    string exactValue = term.Value.Trim('"');
                    bool exactMatch = stringValue.Equals(exactValue, comparison);
                    return term.IsExclusion ? !exactMatch : exactMatch;
                }

                // Regular string contains
                bool match = stringValue.Contains(term.Value, comparison);
                return term.IsExclusion ? !match : match;
            });

            // Check free text terms against all content
            bool freeTextTermsMatch = freeTextTerms.All(term =>
            {
                if (term.IsExactPhrase)
                {
                    return searchableContent.Any(content =>
                        content.Contains(term.Text, searchOptions?.GetStringComparison() ?? StringComparison.OrdinalIgnoreCase));
                }

                if (term.IsExclusion)
                {
                    return !searchableContent.Any(content =>
                        content.Contains(term.Text, searchOptions?.GetStringComparison() ?? StringComparison.OrdinalIgnoreCase));
                }

                if (term.IsRequired)
                {
                    return searchableContent.Any(content =>
                        content.Contains(term.Text, searchOptions?.GetStringComparison() ?? StringComparison.OrdinalIgnoreCase));
                }

                // Fuzzy match for normal terms
                return searchableContent.Any(content =>
                    IsFuzzyMatch(content, term.Text, searchOptions?.GetMaxLevenshteinDistance() ?? 2));
            });

            // Both structured and free-text terms must match
            return structuredTermsMatch && freeTextTermsMatch;
        }
        private static string ExtractRemainingText(string searchText, List<StructuredSearchTerm> structuredTerms)
        {
            // Remove all structured terms from the original search text to get remaining free text
            string remainingText = searchText;

            foreach (StructuredSearchTerm term in structuredTerms)
            {
                string valuePattern = term.HasOrCondition
                    ? string.Join("\\|", term.OrValues.Select(v => Regex.Escape(v)))
                    : Regex.Escape(term.Value);

                string termPattern = $"{(term.IsRequired ? "\\+" : "")}{(term.IsExclusion ? "-" : "")}{Regex.Escape(term.Field)}:{Regex.Escape(term.ComparisonOperator)}{valuePattern}";
                remainingText = Regex.Replace(remainingText, termPattern, " ", RegexOptions.IgnoreCase);
            }

            return remainingText.Trim();
        }
        private static readonly string[]
            CountryAliases = ["Country of origin", "Country of destination"],
            CompanyAliases = ["Company of origin"],
            PriceAliases = ["Price per unit", "Total"],
            TotalAliases = ["Total"],
            DiscountAliases = ["Discount"],
            ShippingAliases = ["Shipping"],
            DateAliases = ["Date"];
        private static DataGridViewColumn? FindColumnByName(DataGridView dataGridView, string fieldName)
        {
            // Try to find column by name or header text (case insensitive)
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (column.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase) ||
                    column.HeaderText.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            // Handle special cases for field names that might be translated or have variations
            // This helps match AI-translated queries to the actual column names
            Dictionary<string, string[]> fieldAliases = new()
            {
                {"country", CountryAliases},
                {"company", CompanyAliases},
                {"price", PriceAliases},
                {"expensive", TotalAliases},
                {"cheap", TotalAliases},
                {"discount", DiscountAliases},
                {"shipping", ShippingAliases},
                {"date", DateAliases}
            };

            if (fieldAliases.TryGetValue(fieldName.ToLower(), out string[] aliases))
            {
                foreach (string alias in aliases)
                {
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        if (column.Name.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                            column.HeaderText.Equals(alias, StringComparison.OrdinalIgnoreCase))
                        {
                            return column;
                        }
                    }
                }
            }

            return null;
        }
        private static bool IsNumericComparison(string cellValue, string compareValue, string comparisonOperator, out bool result)
        {
            result = false;

            // Clean up the comparison value - remove any quote characters
            compareValue = compareValue.Trim('"', '\'');

            if (decimal.TryParse(cellValue, out decimal numericCellValue) &&
                decimal.TryParse(compareValue, out decimal numericCompareValue))
            {
                if (comparisonOperator == ">")
                {
                    result = numericCellValue > numericCompareValue;
                    return true;
                }
                else if (comparisonOperator == "<")
                {
                    result = numericCellValue < numericCompareValue;
                    return true;
                }
                else if (comparisonOperator == "=")
                {
                    result = numericCellValue == numericCompareValue;
                    return true;
                }
            }

            return false;
        }
        private static bool IsDateField(DataGridViewColumn column)
        {
            return column.Name.Equals("Date", StringComparison.OrdinalIgnoreCase) ||
                   column.HeaderText.Equals("Date", StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsDateComparison(string cellValue, string compareValue, string comparisonOperator, out bool result)
        {
            result = false;

            if (DateTime.TryParse(cellValue, out DateTime dateCell))
            {
                // Handle relative date values like "2025-12-01" from AI translations
                if (DateTime.TryParse(compareValue, out DateTime dateCompare))
                {
                    if (comparisonOperator == ">")
                    {
                        result = dateCell > dateCompare;
                        return true;
                    }
                    else if (comparisonOperator == "<")
                    {
                        result = dateCell < dateCompare;
                        return true;
                    }
                }

                // Handle special date keywords
                DateTime now = DateTime.Now;
                if (compareValue.Equals("today", StringComparison.OrdinalIgnoreCase))
                {
                    dateCompare = now.Date;
                }
                else if (compareValue.Equals("yesterday", StringComparison.OrdinalIgnoreCase))
                {
                    dateCompare = now.Date.AddDays(-1);
                }
                else if (compareValue.Equals("this month", StringComparison.OrdinalIgnoreCase) ||
                        compareValue.Equals("thismonth", StringComparison.OrdinalIgnoreCase))
                {
                    dateCompare = new DateTime(now.Year, now.Month, 1);
                }
                else if (compareValue.Equals("last month", StringComparison.OrdinalIgnoreCase) ||
                        compareValue.Equals("lastmonth", StringComparison.OrdinalIgnoreCase))
                {
                    dateCompare = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                }
                else
                {
                    // Couldn't parse as a date
                    return false;
                }

                if (comparisonOperator == ">")
                {
                    result = dateCell > dateCompare;
                    return true;
                }
                else if (comparisonOperator == "<")
                {
                    result = dateCell < dateCompare;
                    return true;
                }
            }

            return false;
        }
        [GeneratedRegex(@"([\+\-])?(\w+[\s\w]*?):([<>=]*)([^:]+?)(?=\s+[\+\-]?\w+:|\s*$)", RegexOptions.Compiled)]
        private static partial Regex StructuredSearchTermRegex();
        private static List<StructuredSearchTerm> ParseStructuredSearchTerms(string searchText)
        {
            List<StructuredSearchTerm> terms = [];

            // Match patterns like:
            // +Field:Value, -Field:Value, Field:Value
            // Field:>Value, Field:<Value
            // +Field:>Value, -Field:<Value, etc.
            // Field:Value1|Value2|Value3 (OR condition)
            MatchCollection matches = StructuredSearchTermRegex().Matches(searchText);

            foreach (Match match in matches)
            {
                string prefix = match.Groups[1].Value;
                string field = match.Groups[2].Value.Trim();
                string comparisonOp = match.Groups[3].Value;
                string value = match.Groups[4].Value;

                bool isRequired = prefix == "+";
                bool isExclusion = prefix == "-";

                terms.Add(new StructuredSearchTerm(field, value, isRequired, isExclusion, comparisonOp));
            }

            return terms;
        }

        private class StructuredSearchTerm
        {
            public string Field { get; }
            public string Value { get; }
            public bool IsRequired { get; }
            public bool IsExclusion { get; }
            public string ComparisonOperator { get; }
            public bool HasOrCondition { get; }
            public List<string> OrValues { get; }

            public StructuredSearchTerm(string field, string value, bool isRequired, bool isExclusion, string comparisonOperator)
            {
                Field = field;
                Value = value;
                IsRequired = isRequired;
                IsExclusion = isExclusion;
                ComparisonOperator = comparisonOperator;

                // Check if this is an OR condition (contains pipe character)
                HasOrCondition = value.Contains('|');
                if (HasOrCondition)
                {
                    // Split the value by pipe character to get individual values
                    OrValues = value.Split('|').Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToList();
                }
                else
                {
                    OrValues = [];
                }
            }
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

                Text = text.Trim();
                _type = type;
            }

            // Getters
            public string Text { get; }
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
                Normal,
                ExactPhrase,
                Exclusion,
                Required
            }
        }
    }
}