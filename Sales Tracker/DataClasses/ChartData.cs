namespace Sales_Tracker.DataClasses
{
    public record ChartData
    {
        // Properties
        private readonly double _total;
        private readonly Dictionary<string, double> _data;

        // Getters
        public double GetTotal() => _total;
        public Dictionary<string, double> GetData() => _data;

        // Constructor
        public ChartData(double total, Dictionary<string, double> data)
        {
            _total = total;
            _data = new Dictionary<string, double>(data ?? []);  // Defensive copy
        }

        // Static factory method for creating empty ChartData
        public static ChartData Empty => new(0, []);
    }

    public record SalesExpensesChartData
    {
        // Properties
        private readonly Dictionary<string, double> _salesData;
        private readonly Dictionary<string, double> _expensesData;
        private readonly IReadOnlyList<string> _dateOrder;

        // Getter
        public IReadOnlyList<string> GetDateOrder() => _dateOrder;

        // Constructor
        public SalesExpensesChartData(
            Dictionary<string, double> salesData,
            Dictionary<string, double> expensesData,
            IEnumerable<string> dateOrder)
        {
            _salesData = new Dictionary<string, double>(salesData ?? []);
            _expensesData = new Dictionary<string, double>(expensesData ?? []);
            _dateOrder = (dateOrder?.ToList() ?? []).AsReadOnly();
        }

        // Static factory method for creating empty data
        public static SalesExpensesChartData Empty => new([], [], []);

        // Query methods
        public double GetSalesForDate(string date) =>
            _salesData.TryGetValue(date, out double value) ? value : 0;
        public double GetExpensesForDate(string date) =>
            _expensesData.TryGetValue(date, out double value) ? value : 0;
    }
}