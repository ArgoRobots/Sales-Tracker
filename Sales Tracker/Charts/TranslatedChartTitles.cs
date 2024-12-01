using Sales_Tracker.UI;

namespace Sales_Tracker.Charts
{
    public static class TranslatedChartTitles
    {
        private static string Translate(string key) => LanguageManager.TranslateSingleString(key);

        public static string TotalRevenue => Translate("Total revenue");
        public static string TotalExpenses => Translate("Total expenses");
        public static string RevenueDistribution => Translate("Distribution of revenue");
        public static string ExpensesDistribution => Translate("Distribution of expenses");
        public static string TotalProfits => Translate("Total profits");
        public static string CountriesOfOrigin => Translate("Countries of origin for purchased products");
        public static string CompaniesOfOrigin => Translate("Companies of origin for purchased products");
        public static string CountriesOfDestination => Translate("Countries of destination for sold products");
        public static string AccountantsTransactions => Translate("Transactions managed by accountants");
        public static string SalesVsExpenses => Translate("Total expenses vs. total sales");
        public static string AverageTransactionValue => Translate("Average transaction value");
        public static string TotalTransactions => Translate("Total transactions");
        public static string AverageShippingCosts => Translate("Average shipping costs");
        public static string GrowthRates => Translate("Expenses and revenue growth rates");
    }
}