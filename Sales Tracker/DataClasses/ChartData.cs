namespace Sales_Tracker.DataClasses
{
    public record ChartData
    {
        // Private setters for encapsulation
        public double Total { get; private init; }
        public Dictionary<string, double> Data { get; private init; }

        // Constructor to ensure proper initialization
        public ChartData(double total, Dictionary<string, double> data)
        {
            Total = total;
            Data = new Dictionary<string, double>(data ?? []); // Defensive copy
        }

        // Static factory method for creating empty ChartData
        public static ChartData Empty => new(0, []);

        // Method to safely access the data dictionary
        public IReadOnlyDictionary<string, double> GetData() => Data;

        // Optional: Add methods to manipulate or query the data if needed
        public bool HasData => Data.Count > 0;

        public double GetValueForDate(string date) =>
            Data.TryGetValue(date, out double value) ? value : 0;
    }

    public record SalesExpensesChartData
    {
        // Private setters for encapsulation
        public double TotalSales { get; private init; }
        public double TotalExpenses { get; private init; }
        public Dictionary<string, double> SalesData { get; private init; }
        public Dictionary<string, double> ExpensesData { get; private init; }
        public IReadOnlyList<string> DateOrder { get; private init; }

        // Constructor to ensure proper initialization
        public SalesExpensesChartData(
            double totalSales,
            double totalExpenses,
            Dictionary<string, double> salesData,
            Dictionary<string, double> expensesData,
            IEnumerable<string> dateOrder)
        {
            TotalSales = totalSales;
            TotalExpenses = totalExpenses;
            SalesData = new Dictionary<string, double>(salesData ?? []);
            ExpensesData = new Dictionary<string, double>(expensesData ?? []);
            DateOrder = (dateOrder?.ToList() ?? []).AsReadOnly();
        }

        // Static factory method for creating empty data
        public static SalesExpensesChartData Empty => new(0, 0, [], [], []);

        // Helper properties
        public double GrandTotal => TotalSales + TotalExpenses;
        public bool HasData => SalesData.Count > 0 || ExpensesData.Count > 0;

        // Safe data access methods
        public IReadOnlyDictionary<string, double> GetSalesData() => SalesData;
        public IReadOnlyDictionary<string, double> GetExpensesData() => ExpensesData;

        public double GetSalesForDate(string date) =>
            SalesData.TryGetValue(date, out double value) ? value : 0;

        public double GetExpensesForDate(string date) =>
            ExpensesData.TryGetValue(date, out double value) ? value : 0;
    }
}