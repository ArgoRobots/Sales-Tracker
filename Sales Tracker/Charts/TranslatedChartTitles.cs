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

        // Chart Descriptions for Tooltips/Help
        public static class Descriptions
        {
            public static string TotalRevenue => Translate("Shows total revenue over time from sales transactions");
            public static string TotalExpenses => Translate("Shows total expenses over time from purchase transactions");
            public static string RevenueDistribution => Translate("Breakdown of revenue by category, including taxes, shipping, and fees");
            public static string ExpensesDistribution => Translate("Breakdown of expenses by category, including taxes, shipping, and fees");
            public static string TotalProfits => Translate("Net profit calculated as revenue minus expenses over time");

            public static string CountriesOfOrigin => Translate("Distribution of purchased products by country of origin");
            public static string CompaniesOfOrigin => Translate("Distribution of purchased products by company of origin");
            public static string CountriesOfDestination => Translate("Distribution of sold products by destination country");

            public static string AccountantsTransactions => Translate("Number of transactions managed by each accountant");

            public static string SalesVsExpenses => Translate("Comparison of total sales and expenses over time");
            public static string AverageTransactionValue => Translate("Average value of purchase and sale transactions over time");
            public static string TotalTransactions => Translate("Number of purchase and sale transactions over time");
            public static string AverageShippingCosts => Translate("Average shipping costs for purchases and sales over time");

            public static string GrowthRates => Translate("Percentage growth rates for expenses and revenue over time");

            public static string ReturnsOverTime => Translate("Number of returned transactions over time");
            public static string ReturnReasons => Translate("Distribution of return transactions by reason");
            public static string ReturnFinancialImpact => Translate("Financial value of returned transactions over time");
            public static string ReturnsByCategory => Translate("Distribution of returns by product category");
            public static string ReturnsByProduct => Translate("Distribution of returns by specific product");
            public static string PurchaseVsSaleReturns => Translate("Comparison of purchase returns vs sale returns");
        }

        // Data Labels and Units
        public static class DataLabels
        {
            public static string Revenue => Translate("Revenue");
            public static string Expenses => Translate("Expenses");
            public static string Profits => Translate("Profits");
            public static string Date => Translate("Date");
            public static string Category => Translate("Category");
            public static string Country => Translate("Country");
            public static string Company => Translate("Company");
            public static string Product => Translate("Product");
            public static string Accountant => Translate("Accountant");
            public static string Quantity => Translate("Quantity");
            public static string Percentage => Translate("Percentage");
            public static string Count => Translate("Count");
            public static string Value => Translate("Value");
            public static string Average => Translate("Average");
            public static string Total => Translate("Total");
            public static string Growth => Translate("Growth");
            public static string Returns => Translate("Returns");
        }
    }
}