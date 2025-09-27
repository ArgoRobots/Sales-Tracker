using Sales_Tracker.Language;

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
        public static string CountriesOfOrigin => Translate("Countries of origin for purchases");
        public static string CompaniesOfOrigin => Translate("Companies of origin for purchases");
        public static string CountriesOfDestination => Translate("Countries of destination for sales");

        // Operational Charts
        public static string AccountantsTransactions => Translate("Transactions managed by accountants");

        // Performance and Growth Charts
        public static string SalesVsExpenses => Translate("Total expenses vs. total revenue");
        public static string AverageTransactionValue => Translate("Average transaction value");
        public static string TotalTransactions => Translate("Total transactions");
        public static string AverageShippingCosts => Translate("Average shipping costs");
        public static string GrowthRates => Translate("Expenses and revenue growth rates");

        // Returns Analysis Charts
        public static string ReturnsOverTime => Translate("Returns over time");
        public static string ReturnReasons => Translate("Return reasons distribution");
        public static string ReturnFinancialImpact => Translate("Financial impact of returns");
        public static string ReturnsByCategory => Translate("Returns by category");
        public static string ReturnsByProduct => Translate("Returns by product");
        public static string PurchaseVsSaleReturns => Translate("Purchase vs. sale returns");

        // Losses Analysis Charts
        public static string LossesOverTime => LanguageManager.TranslateString("Losses Over Time");
        public static string LossReasons => LanguageManager.TranslateString("Loss Reasons");
        public static string LossFinancialImpact => LanguageManager.TranslateString("Loss Financial Impact");
        public static string LossesByCategory => LanguageManager.TranslateString("Losses by Category");
        public static string LossesByProduct => LanguageManager.TranslateString("Losses by Product");
        public static string PurchaseVsSaleLosses => LanguageManager.TranslateString("Purchase vs Sale Losses");
    }
}