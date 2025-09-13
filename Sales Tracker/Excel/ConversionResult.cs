namespace Sales_Tracker.Excel
{
    /// <summary>
    /// Holds the result of converting string values to decimal with validation status and action to take on invalid values.
    /// </summary
    public class ConversionResult
    {
        public decimal Value { get; set; }
        public bool IsValid { get; set; }
        public ExcelSheetManager.InvalidValueAction Action { get; set; }
    }
}