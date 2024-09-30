namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// This is used as a second Tag property.
    /// </summary>
    public static class AccessibleDescriptionStrings
    {
        // Properties
        private static readonly string doNotTranslate = "do not translate",
            doNotCache = "do not cache",
            alignLeftCenter = "left center align",
            alignRightCenter = "right center align";

        // Getters
        public static string DoNotTranslate => doNotTranslate;
        public static string DoNotCacheText => doNotCache;
        public static string AlignRightCenter => alignRightCenter;
        public static string AlignLeftCenter => alignLeftCenter;
    }
}