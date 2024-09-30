using System.Text.Json;

namespace Sales_Tracker.DataClasses
{
    public class ReadOnlyVariables
    {
        // Properties
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        private static readonly string emptyCell = "-",
            multipleItems_text = "Multiple items",
            receipt_text = "receipt:",
            show_text = "show",
            companyName_text = "CompanyName",
            noData_text = "No data",
            noResults_text = "No results";

        private static readonly byte offsetRightClickPanel = 30;

        // Getters
        public static JsonSerializerOptions JsonOptions => jsonOptions;
        public static string EmptyCell => emptyCell;
        public static string MultipleItems_text => multipleItems_text;
        public static string Receipt_text => receipt_text;
        public static string Show_text => show_text;
        public static string CompanyName_text => companyName_text;
        public static string NoData_text => noData_text;
        public static string NoResults_text => noResults_text;
        public static byte OffsetRightClickPanel => offsetRightClickPanel;
    }
}