using ClosedXML.Excel;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Helper class for working with Excel worksheets using DataGridView column names.
    /// </summary>
    public class ExcelColumnHelper
    {
        private readonly Dictionary<string, int> _columnMapping;

        public ExcelColumnHelper(IXLWorksheet worksheet)
        {
            _columnMapping = new(StringComparer.OrdinalIgnoreCase);

            if (worksheet.RowsUsed().Any())
            {
                BuildColumnMapping(worksheet);
            }
        }

        /// <summary>
        /// Builds the column mapping from header row.
        /// </summary>
        private void BuildColumnMapping(IXLWorksheet worksheet)
        {
            IXLRow headerRow = worksheet.RowsUsed().First();
            IXLCells usedColumns = headerRow.CellsUsed();

            foreach (IXLCell? cell in usedColumns)
            {
                string headerText = cell.GetValue<string>()?.Trim();
                if (!string.IsNullOrEmpty(headerText))
                {
                    _columnMapping[headerText] = cell.Address.ColumnNumber;
                }
            }
        }

        /// <summary>
        /// Gets a cell value by column header name.
        /// </summary>
        public string GetCellValue(IXLRow row, string columnHeader)
        {
            if (_columnMapping.TryGetValue(columnHeader, out int columnIndex))
            {
                return row.Cell(columnIndex).GetValue<string>();
            }
            return "";
        }

        /// <summary>
        /// Checks if a column exists in the worksheet.
        /// </summary>
        public bool HasColumn(string columnHeader)
        {
            return _columnMapping.ContainsKey(columnHeader);
        }

        /// <summary>
        /// Gets all available column headers.
        /// </summary>
        public IEnumerable<string> GetAvailableHeaders()
        {
            return _columnMapping.Keys;
        }

        /// <summary>
        /// Gets required columns for transaction import based on DataGridView structure.
        /// </summary>
        private static List<string> GetRequiredTransactionColumns(bool isPurchase)
        {
            // Determine which dictionary to use based on the DataGridView
            Dictionary<MainMenu_Form.Column, string> columnHeaders = isPurchase
                ? MainMenu_Form.Instance.PurchaseColumnHeaders
                : MainMenu_Form.Instance.SalesColumnHeaders;

            return
            [
                columnHeaders[MainMenu_Form.Column.ID],
                columnHeaders[MainMenu_Form.Column.Product],
                columnHeaders[MainMenu_Form.Column.Category],
                columnHeaders[MainMenu_Form.Column.Date],
                columnHeaders[MainMenu_Form.Column.TotalItems],
                columnHeaders[MainMenu_Form.Column.PricePerUnit]
            ];
        }

        /// <summary>
        /// Validates that required columns exist for simple list import (accountants/companies).
        /// </summary>
        public ValidationResult ValidateSimpleListColumns(string expectedColumnName)
        {
            List<string> missingColumns = HasColumn(expectedColumnName) ? [] : [expectedColumnName];

            return new ValidationResult
            {
                IsValid = missingColumns.Count == 0,
                MissingColumns = missingColumns,
                AvailableColumns = GetAvailableHeaders().ToList()
            };
        }

        /// <summary>
        /// Validates that required columns exist for product import.
        /// </summary>
        public ValidationResult ValidateProductColumns()
        {
            List<string> requiredColumns = ["Product Name", "Category"];
            List<string> missingColumns = requiredColumns.Where(col => !HasColumn(col)).ToList();

            return new ValidationResult
            {
                IsValid = missingColumns.Count == 0,
                MissingColumns = missingColumns,
                AvailableColumns = GetAvailableHeaders().ToList()
            };
        }

        /// <summary>
        /// Validates that required columns exist for transaction import.
        /// </summary>
        public ValidationResult ValidateTransactionColumns(bool isPurchase)
        {
            List<string> requiredColumns = GetRequiredTransactionColumns(isPurchase);
            List<string> missingColumns = requiredColumns.Where(col => !HasColumn(col)).ToList();

            return new ValidationResult
            {
                IsValid = missingColumns.Count == 0,
                MissingColumns = missingColumns,
                AvailableColumns = GetAvailableHeaders().ToList()
            };
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> MissingColumns { get; set; } = [];
            public List<string> AvailableColumns { get; set; } = [];
        }
    }
}