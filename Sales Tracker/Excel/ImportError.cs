namespace Sales_Tracker.Excel
{
    public class ImportError
    {
        public string TransactionId { get; set; }
        public string FieldName { get; set; }
        public string InvalidValue { get; set; }
        public int RowNumber { get; set; }
        public string WorksheetName { get; set; }
    }
}