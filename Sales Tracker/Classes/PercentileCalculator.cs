using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Provides methods for calculating percentile-based thresholds for filtering data.
    /// </summary>
    public static class PercentileCalculator
    {
        /// <summary>
        /// Calculates the percentile value from a list of decimal values.
        /// </summary>
        /// <param name="values">List of values to calculate percentile from</param>
        /// <param name="percentile">Percentile to calculate (0-100)</param>
        /// <returns>The value at the specified percentile</returns>
        public static decimal CalculatePercentile(List<decimal> values, double percentile)
        {
            if (values == null || values.Count == 0)
                return 0;

            if (percentile < 0 || percentile > 100)
                throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

            // Sort the values
            var sortedValues = values.OrderBy(v => v).ToList();

            // Calculate the index
            double index = (percentile / 100.0) * (sortedValues.Count - 1);
            int lowerIndex = (int)Math.Floor(index);
            int upperIndex = (int)Math.Ceiling(index);

            // If the index is a whole number, return that value
            if (lowerIndex == upperIndex)
                return sortedValues[lowerIndex];

            // Otherwise, interpolate between the two nearest values
            decimal lowerValue = sortedValues[lowerIndex];
            decimal upperValue = sortedValues[upperIndex];
            decimal fraction = (decimal)(index - lowerIndex);

            return lowerValue + (upperValue - lowerValue) * fraction;
        }

        /// <summary>
        /// Determines the appropriate percentile threshold based on the number of transactions.
        /// Uses a sliding scale: more transactions = smaller percentage of top items.
        /// </summary>
        /// <param name="transactionCount">Number of total transactions</param>
        /// <returns>Percentile value (0-100) where higher values mean more exclusive (top X%)</returns>
        public static double GetDynamicPercentileThreshold(int transactionCount)
        {
            if (transactionCount <= 0)
                return 95; // Default to top 5%

            // Sliding scale based on transaction count:
            // - Very few transactions (1-20): top 50% (percentile 50)
            // - Few transactions (21-50): top 25% (percentile 75)
            // - Medium transactions (51-200): top 10% (percentile 90)
            // - Many transactions (201-500): top 5% (percentile 95)
            // - Lots of transactions (500+): top 2% (percentile 98)

            if (transactionCount <= 20)
                return 50;  // Top 50% - with only 10 items, this gives us ~5 items
            else if (transactionCount <= 50)
                return 75;  // Top 25%
            else if (transactionCount <= 200)
                return 90;  // Top 10%
            else if (transactionCount <= 500)
                return 95;  // Top 5%
            else
                return 98;  // Top 2%
        }

        /// <summary>
        /// Gets all decimal values from a specific column in a DataGridView.
        /// </summary>
        /// <param name="dataGridView">The DataGridView to extract values from</param>
        /// <param name="columnName">Name of the column to extract values from</param>
        /// <returns>List of decimal values from the column</returns>
        public static List<decimal> GetColumnDecimalValues(DataGridView dataGridView, string columnName)
        {
            var values = new List<decimal>();

            if (dataGridView == null || string.IsNullOrWhiteSpace(columnName))
                return values;

            // Find the column
            DataGridViewColumn column = null;
            foreach (DataGridViewColumn col in dataGridView.Columns)
            {
                if (col.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase) ||
                    col.HeaderText.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    column = col;
                    break;
                }
            }

            if (column == null)
                return values;

            // Extract all decimal values from the column
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var cellValue = row.Cells[column.Index].Value;
                if (cellValue != null && decimal.TryParse(cellValue.ToString(), out decimal decimalValue))
                {
                    values.Add(decimalValue);
                }
            }

            return values;
        }

        /// <summary>
        /// Calculates the "expensive" threshold for a given dataset using dynamic percentile.
        /// </summary>
        /// <param name="dataGridView">The DataGridView containing the data</param>
        /// <param name="columnName">Name of the column to analyze (typically "Total")</param>
        /// <returns>The threshold value above which items are considered "expensive"</returns>
        public static decimal GetExpensiveItemThreshold(DataGridView dataGridView, string columnName = "Total")
        {
            var values = GetColumnDecimalValues(dataGridView, columnName);

            if (values.Count == 0)
                return 200; // Fallback to hardcoded default if no data

            // Determine the appropriate percentile based on data size
            double percentile = GetDynamicPercentileThreshold(values.Count);

            // Calculate and return the threshold
            return CalculatePercentile(values, percentile);
        }
    }
}
