namespace Sales_Tracker.Excel
{
    public class ConversionResult
    {
        public decimal Value { get; set; }
        public bool IsValid { get; set; }
        public ExcelSheetManager.InvalidValueAction Action { get; set; }
    }
}