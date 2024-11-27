using System.Text.Json;

namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Contains a set of read-only variables used for various UI and data-related text and formatting values.
    /// </summary>
    public class ReadOnlyVariables
    {
        // Properties
        private static readonly string _emptyCell = "-",
            _multipleItems_text = "Multiple items",
            _receipt_text = "receipt:",
            _show_text = "show",
            _companyName_text = "CompanyName",
            _noData_text = "No data";

        private static readonly byte _offsetRightClickPanel = 30;
        private static readonly byte _paddingRightClickPanel = 5;

        // Getters
        public static string EmptyCell => _emptyCell;
        public static string MultipleItems_text => _multipleItems_text;
        public static string Receipt_text => _receipt_text;
        public static string Show_text => _show_text;
        public static string CompanyName_text => _companyName_text;
        public static string NoData_text => _noData_text;
        public static byte OffsetRightClickPanel => _offsetRightClickPanel;
        public static byte PaddingRightClickPanel => _paddingRightClickPanel;
    }
}