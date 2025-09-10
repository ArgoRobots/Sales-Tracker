using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Helper class for working with Excel worksheets using DataGridView column names.
    /// </summary>
    public static partial class ExcelColumnHelper
    {
        // Methods
        /// <summary>
        /// Performs flexible matching for column headers - extended to handle all form types.
        /// </summary>
        private static bool IsFlexibleMatch(string excelHeader, string standardHeader, Enum columnType)
        {
            // Remove common punctuation and extra spaces
            string cleanExcel = excelHeader.Replace("/", " ").Replace("-", " ").Replace("_", " ");
            string cleanStandard = standardHeader.Replace("/", " ").Replace("-", " ").Replace("_", " ");

            // Normalize multiple spaces to single space
            cleanExcel = CleanExcel().Replace(cleanExcel, " ").Trim().ToLowerInvariant();
            cleanStandard = CleanStandard().Replace(cleanStandard, " ").Trim().ToLowerInvariant();

            // Handle MainMenu_Form.Column types (existing logic)
            if (columnType is MainMenu_Form.Column mainMenuColumn)
            {
                return mainMenuColumn switch
                {
                    MainMenu_Form.Column.Product => IsProductMatch(cleanExcel),
                    MainMenu_Form.Column.Category => IsCategoryMatch(cleanExcel),
                    MainMenu_Form.Column.Country => IsCountryMatch(cleanExcel),
                    MainMenu_Form.Column.Company => IsCompanyMatch(cleanExcel),
                    MainMenu_Form.Column.Date => IsDateMatch(cleanExcel),
                    MainMenu_Form.Column.TotalItems => IsTotalItemsMatch(cleanExcel),
                    MainMenu_Form.Column.PricePerUnit => IsPricePerUnitMatch(cleanExcel),
                    MainMenu_Form.Column.ID => IsIdMatch(cleanExcel),
                    MainMenu_Form.Column.Note => IsNoteMatch(cleanExcel),
                    MainMenu_Form.Column.HasReceipt => IsReceiptMatch(cleanExcel),
                    _ => IsGenericMatch(cleanExcel, cleanStandard)
                };
            }

            // Handle Accountants_Form.Column types
            if (columnType is Accountants_Form.Column accountantColumn)
            {
                return accountantColumn switch
                {
                    Accountants_Form.Column.AccountantName => IsAccountantNameMatch(cleanExcel),
                    _ => IsGenericMatch(cleanExcel, cleanStandard)
                };
            }

            // Handle Companies_Form.Column types
            if (columnType is Companies_Form.Column companyColumn)
            {
                return companyColumn switch
                {
                    Companies_Form.Column.Company => IsCompanyNameMatch(cleanExcel),
                    _ => IsGenericMatch(cleanExcel, cleanStandard)
                };
            }

            // Handle Products_Form.Column types
            if (columnType is Products_Form.Column productColumn)
            {
                return productColumn switch
                {
                    Products_Form.Column.ProductID => IsProductIdMatch(cleanExcel),
                    Products_Form.Column.ProductName => IsProductNameMatch(cleanExcel),
                    Products_Form.Column.ProductCategory => IsCategoryMatch(cleanExcel),
                    Products_Form.Column.CountryOfOrigin => IsCountryMatch(cleanExcel),
                    Products_Form.Column.CompanyOfOrigin => IsCompanyMatch(cleanExcel),
                    _ => IsGenericMatch(cleanExcel, cleanStandard)
                };
            }

            // Fallback to generic matching for unknown types
            return IsGenericMatch(cleanExcel, cleanStandard);
        }
        private static bool IsCompanyNameMatch(string excel)
        {
            return excel.Contains("company") ||
                   excel == "company" || excel == "companies" ||
                   excel == "name" || excel == "company name" ||
                   excel.Contains("business") || excel.Contains("organization") ||
                   excel.Contains("corp") || excel.Contains("corporation");
        }
        private static bool IsDateMatch(string excel)
        {
            return excel.Contains("date");
        }
        private static bool IsTotalItemsMatch(string excel)
        {
            return excel.Contains("total") && excel.Contains("item") ||
                   excel == "quantity" || excel == "qty" || excel.Contains("amount");
        }
        private static bool IsPricePerUnitMatch(string excel)
        {
            return (excel.Contains("price") && excel.Contains("unit")) ||
                   excel == "unit price" || excel == "price" || excel.Contains("cost per unit");
        }
        private static bool IsIdMatch(string excel)
        {
            return excel.Contains("id") || excel.Contains("number") || excel.Contains('#') ||
                   excel == "transaction id" || excel == "order" || excel == "sale" || excel == "purchase";
        }
        private static bool IsNoteMatch(string excel)
        {
            return excel.Contains("note") || excel == "notes" || excel == "comment" ||
                   excel == "comments" || excel == "remarks";
        }
        private static bool IsReceiptMatch(string excel)
        {
            return excel.Contains("receipt") || excel.Contains("attachment") ||
                   excel.Contains("file") || excel == "receipt";
        }
        private static bool IsAccountantNameMatch(string excel)
        {
            return excel.Contains("accountant") ||
                   excel == "accountant" || excel == "accountants" ||
                   excel == "name" || excel == "accountant name" ||
                   excel == "cpa" || excel == "bookkeeper";
        }
        private static bool IsProductIdMatch(string excel)
        {
            return excel.Contains("id") || excel.Contains("product") ||
                   excel == "product id" || excel == "productid" ||
                   excel.Contains("sku") || excel.Contains("code") ||
                   excel == "id" || excel == "#";
        }
        private static bool IsProductNameMatch(string excel)
        {
            return excel.Contains("product") || excel.Contains("service") ||
                   excel == "product" || excel == "products" ||
                   excel == "service" || excel == "services" ||
                   excel == "name" || excel == "product name" ||
                   excel == "item" || excel == "items";
        }
        private static bool IsProductMatch(string excel)
        {
            return excel.Contains("product") || excel.Contains("service") ||
                   excel == "product" || excel == "products" || excel == "service" || excel == "services";
        }
        private static bool IsCategoryMatch(string excel)
        {
            return excel == "category" || excel == "categories" || excel.Contains("category");
        }
        private static bool IsCountryMatch(string excel)
        {
            return excel.Contains("country") || excel.Contains("origin") || excel.Contains("destination");
        }
        private static bool IsCompanyMatch(string excel)
        {
            return excel.Contains("company") || excel.Contains("origin") || excel.Contains("manufacturer") ||
                   excel.Contains("supplier") || excel.Contains("vendor");
        }
        private static bool IsGenericMatch(string cleanExcel, string cleanStandard)
        {
            // Direct match
            if (cleanExcel == cleanStandard)
            {
                return true;
            }

            // One contains the other
            if (cleanExcel.Contains(cleanStandard) || cleanStandard.Contains(cleanExcel))
            {
                return true;
            }

            // Handle "name" suffix variations
            if (cleanExcel.EndsWith(" name") && cleanStandard == cleanExcel.Substring(0, cleanExcel.Length - 5))
            {
                return true;
            }

            if (cleanStandard.EndsWith(" name") && cleanExcel == cleanStandard.Substring(0, cleanStandard.Length - 5))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a cell value by column type using standard column headers.
        /// </summary>
        public static string GetCellValue(IXLRow row, Enum column)
        {
            IXLWorksheet worksheet = row.Worksheet;
            IXLRow headerRow = worksheet.RowsUsed().First();

            // Find the column index that matches our enum
            int columnIndex = FindColumnIndex(headerRow, column);

            if (columnIndex == -1)
            {
                return "";  // Column not found
            }

            // Get the cell value at that column index
            IXLCell cell = row.Cell(columnIndex);
            return cell.GetValue<string>()?.Trim() ?? "";
        }

        /// <summary>
        /// Finds the column index for a given enum column in the header row using display text.
        /// </summary>
        private static int FindColumnIndex(IXLRow headerRow, Enum column, Dictionary<Enum, string> columnHeaders = null)
        {
            string standardHeader = columnHeaders?.TryGetValue(column, out string displayText) == true
                ? displayText
                : column.ToString();

            IXLCells headerCells = headerRow.CellsUsed();

            int columnIndex = 1;  // Excel columns are 1-based

            foreach (IXLCell headerCell in headerCells)
            {
                string excelHeader = headerCell.GetValue<string>()?.Trim();

                if (string.IsNullOrEmpty(excelHeader))
                {
                    columnIndex++;
                    continue;
                }

                // First try exact match (case-insensitive)
                if (string.Equals(excelHeader, standardHeader, StringComparison.OrdinalIgnoreCase))
                {
                    return columnIndex;
                }

                if (IsFlexibleMatch(excelHeader, standardHeader, column))
                {
                    return columnIndex;
                }

                columnIndex++;
            }

            return -1;  // Column not found
        }

        /// <summary>
        /// Checks if a column exists by column type using flexible matching.
        /// </summary>
        public static bool HasColumn(IXLWorksheet worksheet, Enum column, Dictionary<Enum, string> columnHeaders)
        {
            if (!worksheet.RowsUsed().Any())
            {
                return false;
            }

            // Get the standard header text from the dictionary
            if (!columnHeaders.TryGetValue(column, out string standardHeader))
            {
                return false;
            }

            List<string> columnsInWorksheet = GetColumnNamesInWorksheet(worksheet);

            // Check each column in the worksheet
            foreach (string excelHeader in columnsInWorksheet)
            {
                if (string.IsNullOrWhiteSpace(excelHeader))
                {
                    continue;
                }

                // First try exact match (case-insensitive)
                if (string.Equals(excelHeader, standardHeader, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (IsFlexibleMatch(excelHeader, standardHeader, column))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets column header names from worksheet's first row.
        /// </summary>
        private static List<string> GetColumnNamesInWorksheet(IXLWorksheet worksheet)
        {
            IXLCells columnCells = worksheet.RowsUsed().First().CellsUsed();
            List<string> columnNames = [];

            foreach (IXLCell? cell in columnCells)
            {
                string cellText = cell.GetValue<string>()?.Trim();
                if (!string.IsNullOrEmpty(cellText))
                {
                    columnNames.Add(cellText);
                }
            }

            return columnNames;
        }

        /// <summary>
        /// Checks if worksheet has any column headers.
        /// </summary>
        public static bool ContainsAnyColumns(IXLWorksheet worksheet)
        {
            return GetColumnNamesInWorksheet(worksheet).Count > 0;
        }

        /// <summary>
        /// Validates that required columns exist for simple list import (accountants/companies).
        /// </summary>
        public static ValidationResult ValidateSimpleListColumns(IXLWorksheet worksheet, Enum column, Dictionary<Enum, string> columnHeaders)
        {
            if (!HasColumn(worksheet, column, columnHeaders))
            {
                string displayText = columnHeaders.TryGetValue(column, out string text) ? text : column.ToString();
                return new ValidationResult([displayText]);
            }

            return new ValidationResult([]);
        }

        /// <summary>
        /// Validates that required columns exist for product import.
        /// </summary>
        public static ValidationResult ValidateProductColumns(IXLWorksheet worksheet)
        {
            List<Products_Form.Column> requiredColumns =
            [
                Products_Form.Column.ProductName,
                Products_Form.Column.ProductCategory
            ];

            List<string> missingColumns = [];
            Dictionary<Enum, string> columnHeaders = Products_Form.ColumnHeaders
               .ToDictionary(kvp => (Enum)kvp.Key, kvp => kvp.Value);

            foreach (Products_Form.Column column in requiredColumns)
            {
                if (!HasColumn(worksheet, column, columnHeaders))
                {
                    string displayText = columnHeaders.TryGetValue(column, out string text) ? text : column.ToString();
                    missingColumns.Add(displayText);
                }
            }

            return new ValidationResult(missingColumns);
        }

        /// <summary>
        /// Validates that required columns exist for transaction import.
        /// </summary>
        public static ValidationResult ValidateTransactionColumns(IXLWorksheet worksheet, bool isPurchase)
        {
            List<MainMenu_Form.Column> requiredColumns =
            [
                MainMenu_Form.Column.ID,
                MainMenu_Form.Column.Product,
                MainMenu_Form.Column.Category,
                MainMenu_Form.Column.Date,
                MainMenu_Form.Column.TotalItems,
                MainMenu_Form.Column.PricePerUnit
            ];

            List<string> missingColumns = [];

            Dictionary<Enum, string> columnHeaders = isPurchase
                ? MainMenu_Form.Instance.PurchaseColumnHeaders.ToDictionary(kvp => (Enum)kvp.Key, kvp => kvp.Value)
                : MainMenu_Form.Instance.SalesColumnHeaders.ToDictionary(kvp => (Enum)kvp.Key, kvp => kvp.Value);

            foreach (MainMenu_Form.Column column in requiredColumns)
            {
                if (!HasColumn(worksheet, column, columnHeaders))
                {
                    string displayText = columnHeaders.TryGetValue(column, out string text) ? text : column.ToString();
                    missingColumns.Add(displayText);
                }
            }

            return new ValidationResult(missingColumns);
        }

        public class ValidationResult(List<string> missingColumns)
        {
            public List<string> MissingColumns { get; } = missingColumns;
            public bool IsValid { get => MissingColumns.Count == 0; }
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex CleanStandard();

        [GeneratedRegex(@"\s+")]
        private static partial Regex CleanExcel();
    }
}