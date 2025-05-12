namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// This is used as a second Tag property.
    /// Manages the combination and parsing of multiple AccessibleDescriptionStrings.
    /// Uses a delimiter-based approach to combine and separate multiple tags.
    /// </summary>
    public static class AccessibleDescriptionManager
    {
        // Properties
        private static readonly string doNotTranslate = "do not translate",
            doNotCache = "do not cache",
            alignLeft = "left center align",
            alignRight = "right center align";

        // Getters
        public static string DoNotTranslate => doNotTranslate;
        public static string DoNotCache => doNotCache;
        public static string AlignRight => alignRight;
        public static string AlignLeft => alignLeft;

        private const string Delimiter = "|";

        /// <summary>
        /// Combines multiple AccessibleDescriptionStrings into a single string.
        /// </summary>
        /// <param name="descriptions">Array of AccessibleDescriptionStrings to combine.</param>
        /// <returns>Combined string with delimiter separation.</returns>
        public static string Combine(params string[] descriptions)
        {
            if (descriptions == null || descriptions.Length == 0)
            {
                return "";
            }

            return string.Join(Delimiter, descriptions.Where(d => !string.IsNullOrWhiteSpace(d)));
        }

        /// <summary>
        /// Checks if the combined AccessibleDescription string contains a specific tag.
        /// </summary>
        /// <param name="combinedDescription">The combined AccessibleDescription string to check.</param>
        /// <param name="tagToCheck">The specific tag to look for.</param>
        /// <returns>True if the tag is present, false otherwise.</returns>
        public static bool HasTag(Control control, string tagToCheck)
        {
            if (string.IsNullOrWhiteSpace(control.AccessibleDescription) || string.IsNullOrWhiteSpace(tagToCheck))
            {
                return false;
            }

            string[] tags = control.AccessibleDescription.Split(Delimiter);
            return tags.Contains(tagToCheck);
        }
    }
}