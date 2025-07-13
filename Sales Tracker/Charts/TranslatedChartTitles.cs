using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    /// <summary>
    /// Provides translated chart titles for LiveCharts.
    /// </summary>
    public static class TranslatedChartTitles
    {
        private static string Translate(string str) => LanguageManager.TranslateString(str);

        // Main Revenue and Expense Charts
        public static string TotalRevenue => Translate("Total revenue");
        public static string TotalExpenses => Translate("Total expenses");
        public static string RevenueDistribution => Translate("Distribution of revenue");
        public static string ExpensesDistribution => Translate("Distribution of expenses");
        public static string TotalProfits => Translate("Total profits");

        // Geographic Analysis Charts
        public static string CountriesOfOrigin => Translate("Countries of origin for purchased products");
        public static string CompaniesOfOrigin => Translate("Companies of origin for purchased products");
        public static string CountriesOfDestination => Translate("Countries of destination for sold products");
        public static string WorldMap => Translate("Global Transaction Map");

        // Operational Charts
        public static string AccountantsTransactions => Translate("Transactions managed by accountants");

        // Comparative Analysis Charts
        public static string SalesVsExpenses => Translate("Total expenses vs. total sales");
        public static string AverageTransactionValue => Translate("Average transaction value");
        public static string TotalTransactions => Translate("Total transactions");
        public static string AverageShippingCosts => Translate("Average shipping costs");

        // Performance and Growth Charts
        public static string GrowthRates => Translate("Expenses and revenue growth rates");

        // Returns Analysis Charts
        public static string ReturnsOverTime => Translate("Returns Over Time");
        public static string ReturnReasons => Translate("Return Reasons Distribution");
        public static string ReturnFinancialImpact => Translate("Financial Impact of Returns");
        public static string ReturnsByCategory => Translate("Returns by Category");
        public static string ReturnsByProduct => Translate("Returns by Product");
        public static string PurchaseVsSaleReturns => Translate("Purchase vs Sale Returns");
    }
}