using ClosedXML.Excel;
using Guna.UI2.WinForms;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.Diagnostics;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles the import and export of data to and from .xlsx spreadsheets, including transactions
    /// and receipts. It also has immediate cancellation support and multi-currency import capabilities.
    /// </summary>
    internal class ExcelSheetManager
    {
        // Constants
        private const string _decimalFormatPattern = "#,##0.00";
        private const string _currencyFormatPattern = "\"$\"#,##0.00";
        private const string _numberFormatPattern = "0";

        // Rollback tracking classes
        public class ImportSession
        {
            public List<string> AddedAccountants { get; set; } = [];
            public List<string> AddedCompanies { get; set; } = [];
            public Dictionary<string, List<Product>> AddedProducts { get; set; } = [];
            public List<DataGridViewRow> AddedPurchaseRows { get; set; } = [];
            public List<DataGridViewRow> AddedSaleRows { get; set; } = [];
            public List<Category> AddedCategories { get; set; } = [];
            public bool IsCancelled { get; set; } = false;
            public bool HasChanges()
            {
                return AddedAccountants.Count > 0 || AddedCompanies.Count > 0 ||
                       AddedProducts.Count > 0 || AddedPurchaseRows.Count > 0 ||
                       AddedSaleRows.Count > 0 || AddedCategories.Count > 0;
            }
        }

        public enum InvalidValueAction
        {
            Skip,
            Cancel,
            Continue
        }
        public enum ImportTransactionResult
        {
            Success,
            Skip,
            Cancel,
            Failed
        }
        public class ConversionResult
        {
            public decimal Value { get; set; }
            public bool IsValid { get; set; }
            public InvalidValueAction Action { get; set; }
        }
        public class ImportError
        {
            public string TransactionId { get; set; }
            public string FieldName { get; set; }
            public string InvalidValue { get; set; }
            public int RowNumber { get; set; }
            public string WorksheetName { get; set; }
        }
        public class ImportSummary
        {
            public int AccountantsImported { get; set; }
            public int CompaniesImported { get; set; }
            public int PurchaseProductsImported { get; set; }
            public int SaleProductsImported { get; set; }
            public int PurchaseTransactionsImported { get; set; }
            public int SaleTransactionsImported { get; set; }
            public int ReceiptsImported { get; set; }
            public int SkippedRows { get; set; }
            public List<ImportError> Errors { get; set; } = [];
            public bool WasCancelled { get; set; }

            public int TotalSuccessfulImports => AccountantsImported + CompaniesImported +
                PurchaseProductsImported + SaleProductsImported + PurchaseTransactionsImported +
                SaleTransactionsImported + ReceiptsImported;

            public bool HasAnyImports => TotalSuccessfulImports > 0;
        }

        // Import data with rollback and cancellation support
        public static bool ImportAccountantsData(IXLWorksheet worksheet, bool includeHeader, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            return ImportSimpleListData(
                worksheet,
                includeHeader,
                MainMenu_Form.Instance.AccountantList,
                MainMenu_Form.SelectedOption.Accountants,
                "Accountant",
                session.AddedAccountants,
                session);
        }
        public static bool ImportCompaniesData(IXLWorksheet worksheet, bool includeHeader, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            return ImportSimpleListData(
                worksheet,
                includeHeader,
                MainMenu_Form.Instance.CompanyList,
                MainMenu_Form.SelectedOption.Companies,
                "Company",
                session.AddedCompanies,
                session);
        }

        /// <summary>
        /// Generic method to import simple list data like accountants or companies with immediate cancellation.
        /// </summary>
        private static bool ImportSimpleListData(
            IXLWorksheet worksheet,
            bool includeHeader,
            List<string> existingList,
            MainMenu_Form.SelectedOption optionType,
            string itemTypeName,
            List<string> addedItems,
            ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            HashSet<string> existingItems = new(existingList.Count);
            foreach (string item in existingList)
            {
                existingItems.Add(item.ToLowerInvariant());
            }

            HashSet<string> addedDuringImport = [];

            foreach (IXLRow row in rowsToProcess)
            {
                // Check for cancellation before processing each row
                if (session.IsCancelled == true) { break; }

                string itemName = row.Cell(1).GetValue<string>();
                string itemNameLower = itemName.ToLowerInvariant();

                if (existingItems.Contains(itemNameLower))
                {
                    CustomMessageBox.Show(
                        $"{itemTypeName} already exists",
                        $"The {itemTypeName.ToLowerInvariant()} {itemName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    existingList.Add(itemName);
                    addedDuringImport.Add(itemNameLower);
                    addedItems?.Add(itemName); // Track for rollback
                    wasSomethingImported = true;
                }
            }

            // Only save if not using session tracking (immediate save mode)
            if (addedItems == null)
            {
                MainMenu_Form.SaveListToFile(existingList, optionType);
            }

            return wasSomethingImported;
        }

        public static bool ImportProductsData(IXLWorksheet worksheet, bool purchase, bool includeHeader, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            List<Category> list = purchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            Dictionary<string, HashSet<string>> existingProducts = [];
            foreach (Category category in list)
            {
                existingProducts[category.Name] = [.. category.ProductList.Select(p => p.Name.ToLowerInvariant())];
            }

            Dictionary<string, HashSet<string>> addedDuringImport = [];

            // Read product data from the worksheet and add it to the category purchase list
            foreach (IXLRow row in rowsToProcess)
            {
                // Check for cancellation before processing each row
                if (session.IsCancelled == true) { break; }

                string productId = row.Cell(1).GetValue<string>();
                string productName = row.Cell(2).GetValue<string>();
                string categoryName = row.Cell(3).GetValue<string>();
                string countryOfOrigin = row.Cell(4).GetValue<string>();
                string companyOfOrigin = row.Cell(5).GetValue<string>();

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(categoryName))
                {
                    continue;
                }

                countryOfOrigin = Country.NormalizeCountryName(countryOfOrigin);

                if (!ValidateCountry(countryOfOrigin))
                {
                    continue;
                }

                EnsureCompanyExists(companyOfOrigin, session);

                // Find or create the category
                Category category = FindOrCreateCategory(
                    list,
                    categoryName,
                    existingProducts,
                    addedDuringImport,
                    session);

                string productNameLower = productName.ToLowerInvariant();

                // Check if product already exists
                if (ProductExists(
                    existingProducts,
                    categoryName,
                    productNameLower,
                    productName,
                    purchase))
                {
                    continue;
                }

                // Create the product and add it to the category's ProductList
                Product newProduct = AddProductToCategory(
                    category,
                    productId,
                    productName,
                    productNameLower,
                    countryOfOrigin,
                    companyOfOrigin,
                    addedDuringImport,
                    categoryName);

                // Track for rollback
                if (session != null)
                {
                    if (!session.AddedProducts.TryGetValue(categoryName, out List<Product>? value))
                    {
                        value = [];
                        session.AddedProducts[categoryName] = value;
                    }

                    value.Add(newProduct);
                }

                wasSomethingImported = true;
            }

            // Only save if not using session tracking (immediate save mode)
            if (session == null)
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(purchase
                    ? MainMenu_Form.SelectedOption.CategoryPurchases
                    : MainMenu_Form.SelectedOption.CategorySales);
            }

            return wasSomethingImported;
        }
        private static bool ValidateCountry(string countryName)
        {
            bool countryExists = Country.CountrySearchResults.Any(
                c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase));

            if (!countryExists)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    "Country does not exist",
                    $"Country '{countryName}' does not exist in the system. Please check the tutorial for more information. Do you want to skip this product and continue?",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                return result != CustomMessageBoxResult.Yes;
            }

            return true;
        }
        private static void EnsureCompanyExists(string companyName, ImportSession session)
        {
            if (!MainMenu_Form.Instance.CompanyList.Contains(companyName))
            {
                MainMenu_Form.Instance.CompanyList.Add(companyName);
                session.AddedCompanies.Add(companyName);  // Track for rollback
            }
        }
        private static Category FindOrCreateCategory(
            List<Category> list,
            string categoryName,
            Dictionary<string, HashSet<string>> existingProducts,
            Dictionary<string, HashSet<string>> addedDuringImport,
            ImportSession session)
        {
            Category category = list.FirstOrDefault(c => c.Name == categoryName);

            if (category == null)
            {
                category = new Category { Name = categoryName };
                list.Add(category);
                existingProducts[categoryName] = [];
                addedDuringImport[categoryName] = [];

                // Track for rollback
                session.AddedCategories.Add(category);
            }
            else if (!addedDuringImport.ContainsKey(categoryName))
            {
                addedDuringImport[categoryName] = [];
            }

            return category;
        }
        private static bool ProductExists(
            Dictionary<string, HashSet<string>> existingProducts,
            string categoryName,
            string productNameLower,
            string productName,
            bool purchase)
        {
            if (existingProducts.TryGetValue(categoryName, out HashSet<string> existingCategoryProducts) &&
                existingCategoryProducts.Contains(productNameLower))
            {
                string type = purchase ? "purchase" : "sale";

                CustomMessageBox.Show(
                    "Product already exists",
                    $"The product for {type} '{productName}' already exists and will not be imported",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);

                return true;
            }

            return false;
        }
        private static Product AddProductToCategory(
            Category category,
            string productId,
            string productName,
            string productNameLower,
            string countryOfOrigin,
            string companyOfOrigin,
            Dictionary<string, HashSet<string>> addedDuringImport,
            string categoryName)
        {
            Product product = new()
            {
                ProductID = productId,
                Name = productName,
                CountryOfOrigin = countryOfOrigin,
                CompanyOfOrigin = companyOfOrigin
            };

            category.ProductList.Add(product);

            // Track that we've added this product
            if (!addedDuringImport.TryGetValue(categoryName, out HashSet<string> productsSet))
            {
                productsSet = [];
                addedDuringImport[categoryName] = productsSet;
            }
            productsSet.Add(productNameLower);

            return product;
        }

        public static ImportSummary ImportPurchaseData(IXLWorksheet worksheet, bool includeHeader, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true)
            {
                return new ImportSummary { WasCancelled = true };
            }
            return ImportTransactionData(worksheet, includeHeader, true, sourceCurrency, session);
        }
        public static ImportSummary ImportSalesData(IXLWorksheet worksheet, bool includeHeader, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true)
            {
                return new ImportSummary { WasCancelled = true };
            }
            return ImportTransactionData(worksheet, includeHeader, false, sourceCurrency, session);
        }
        public static int ImportReceiptsData(IXLWorksheet worksheet, bool includeHeader, string receiptsFolderPath, bool isPurchase)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            int importedCount = 0;

            foreach (IXLRow row in rowsToProcess)
            {
                string transactionId = row.Cell(1).GetValue<string>();
                string receiptFileName = row.Cell(17).GetValue<string>();

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(transactionId) ||
                    string.IsNullOrWhiteSpace(receiptFileName) ||
                    string.IsNullOrWhiteSpace(receiptsFolderPath) ||
                    receiptFileName == ReadOnlyVariables.EmptyCell)
                {
                    continue;
                }

                string receiptFilePath = Path.Combine(receiptsFolderPath, receiptFileName);

                // Check if the receipt file exists
                if (!File.Exists(receiptFilePath))
                {
                    CustomMessageBox.Show(
                        "Receipt does not exist",
                         $"The receipt '{receiptFileName}' does not exist in the folder you selected. This receipt will not be added",
                         CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                    continue;
                }

                // Find the transaction in the correct DataGridView
                DataGridViewRow? targetRow = FindTransactionRow(transactionId, isPurchase);
                if (targetRow == null)
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    Log.Write(1, $"{transactionType} {transactionId} not found for receipt {receiptFileName}");
                    continue;
                }

                // Check if transaction already has a receipt
                if (TransactionHasReceipt(targetRow))
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    CustomMessageBoxResult result = CustomMessageBox.Show(
                        "Transaction already has receipt",
                        $"{transactionType} {transactionId} already has a receipt. Do you want to replace it?",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                // Copy the receipt file to the receipts directory
                (string newReceiptPath, bool saved) = ReceiptManager.SaveReceiptInFile(receiptFilePath);
                if (!saved)
                {
                    continue;
                }

                ReceiptManager.AddReceiptToTag(targetRow, newReceiptPath);
                MainMenu_Form.SetHasReceiptColumn(targetRow, newReceiptPath);

                importedCount++;
            }

            // Save the updated transaction data
            if (importedCount > 0)
            {
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
            }

            return importedCount;
        }
        private static DataGridViewRow? FindTransactionRow(string transactionId, bool isPurchase)
        {
            Guna2DataGridView targetDataGridView = isPurchase
                ? MainMenu_Form.Instance.Purchase_DataGridView
                : MainMenu_Form.Instance.Sale_DataGridView;

            foreach (DataGridViewRow row in targetDataGridView.Rows)
            {
                if (row.Cells[MainMenu_Form.Column.ID.ToString()].Value?.ToString() == transactionId)
                {
                    return row;
                }
            }

            return null;
        }
        private static bool TransactionHasReceipt(DataGridViewRow row)
        {
            if (row.Tag is (List<string> tagList, TagData))
            {
                return tagList[^1].StartsWith(ReadOnlyVariables.Receipt_text);
            }
            else if (row.Tag is (string tagString, TagData))
            {
                return !string.IsNullOrEmpty(tagString);
            }
            return false;
        }

        /// <summary>
        /// Helper method for importing purchase and sales data with source currency support and immediate cancellation support.
        /// Returns ImportSummary with actual counts instead of just boolean.
        /// </summary>
        private static ImportSummary ImportTransactionData(IXLWorksheet worksheet, bool includeHeader, bool isPurchase, string sourceCurrency, ImportSession session)
        {
            ImportSummary summary = new();

            if (session.IsCancelled == true)
            {
                summary.WasCancelled = true;
                return summary;
            }

            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            int newRowIndex = -1;
            int currentRowNumber = includeHeader ? 1 : 2;  // Track the actual Excel row number
            int successfulTransactions = 0;

            Guna2DataGridView targetGridView = isPurchase
                ? MainMenu_Form.Instance.Purchase_DataGridView
                : MainMenu_Form.Instance.Sale_DataGridView;

            // Get existing transaction numbers
            HashSet<string> existingTransactionNumbers = new(targetGridView.Rows.Count);
            string idColumn = MainMenu_Form.Column.ID.ToString();

            foreach (DataGridViewRow row in targetGridView.Rows)
            {
                if (row.Cells[idColumn].Value != null)
                {
                    existingTransactionNumbers.Add(row.Cells[idColumn].Value.ToString());
                }
            }

            HashSet<string> addedDuringImport = [];
            string itemType = isPurchase ? "Purchase" : "Sale";
            string worksheetName = isPurchase ? "Purchases" : "Sales";

            foreach (IXLRow row in rowsToProcess)
            {
                // Check for cancellation before processing each row
                if (session.IsCancelled == true)
                {
                    summary.WasCancelled = true;
                    break;
                }

                currentRowNumber++;  // Increment for each row we process
                string transactionNumber = row.Cell(1).GetValue<string>();

                if (string.IsNullOrEmpty(transactionNumber))
                {
                    summary.SkippedRows++;
                    continue;
                }

                // Check if this row's transaction number already exists
                if (transactionNumber != ReadOnlyVariables.EmptyCell)
                {
                    bool shouldContinue = CheckIfItemExists(
                        transactionNumber,
                        existingTransactionNumbers,
                        addedDuringImport,
                        itemType);

                    if (!shouldContinue)
                    {
                        summary.SkippedRows++;
                        continue;
                    }
                }

                // Create a new row
                DataGridViewRow newRow = (DataGridViewRow)targetGridView.RowTemplate.Clone();
                newRow.CreateCells(targetGridView);

                ImportTransactionResult importResult = ImportTransaction(row, newRow, currentRowNumber, worksheetName, sourceCurrency, session);

                switch (importResult)
                {
                    case ImportTransactionResult.Cancel:
                        session.IsCancelled = true;
                        summary.WasCancelled = true;
                        return summary;  // Cancel the import
                    case ImportTransactionResult.Skip:
                        summary.SkippedRows++;
                        continue;  // Skip this transaction, continue with next
                    case ImportTransactionResult.Failed:
                        // Add error to summary
                        summary.Errors.Add(new ImportError
                        {
                            TransactionId = transactionNumber,
                            RowNumber = currentRowNumber,
                            WorksheetName = worksheetName,
                            FieldName = "Transaction Import",
                            InvalidValue = "Technical failure during import"
                        });
                        return summary;  // Technical failure, stop import
                    case ImportTransactionResult.Success:
                        break;  // Continue processing this transaction
                }

                ImportTransactionResult itemsImportResult = ImportItemsInTransaction(row, newRow, currentRowNumber, worksheetName, sourceCurrency, session);

                switch (itemsImportResult)
                {
                    case ImportTransactionResult.Cancel:
                        session.IsCancelled = true;
                        summary.WasCancelled = true;
                        return summary;  // Cancel the import
                    case ImportTransactionResult.Skip:
                        summary.SkippedRows++;
                        continue;  // Skip this transaction, continue with next
                    case ImportTransactionResult.Failed:
                        summary.Errors.Add(new ImportError
                        {
                            TransactionId = transactionNumber,
                            RowNumber = currentRowNumber,
                            WorksheetName = worksheetName,
                            FieldName = "Items Import",
                            InvalidValue = "Technical failure during items import"
                        });
                        return summary;  // Technical failure, stop import
                    case ImportTransactionResult.Success:
                        break;  // Continue processing this transaction
                }

                // Add the row to the DataGridView
                targetGridView.InvokeIfRequired(() =>
                {
                    newRowIndex = targetGridView.Rows.Add(newRow);
                });

                FormatNoteCell(newRow);

                // Track for rollback
                if (isPurchase)
                {
                    session.AddedPurchaseRows.Add(newRow);
                }
                else
                {
                    session.AddedSaleRows.Add(newRow);
                }

                // Track that we've added this transaction number
                if (!string.IsNullOrEmpty(transactionNumber) && transactionNumber != ReadOnlyVariables.EmptyCell)
                {
                    addedDuringImport.Add(transactionNumber);
                }

                successfulTransactions++;
                DataGridViewManager.DataGridViewRowsAdded(targetGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            if (isPurchase)
            {
                summary.PurchaseTransactionsImported = successfulTransactions;
            }
            else
            {
                summary.SaleTransactionsImported = successfulTransactions;
            }

            // Update "Has Receipt" column for all imported rows
            if (summary.HasAnyImports)
            {
                MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();
            }

            return summary;
        }

        /// <summary>
        /// Validates if a product exists in the appropriate category list.
        /// </summary>
        /// <returns>InvalidValueAction indicating whether to continue, skip, or cancel</returns>
        private static InvalidValueAction ValidateProductExists(
            string productName,
            string categoryName,
            bool isPurchase,
            string transactionId,
            int rowNumber,
            string worksheetName)
        {
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(categoryName))
            {
                return InvalidValueAction.Continue;  // Empty values are handled elsewhere
            }

            List<Category> categoryList = isPurchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            // Find the category
            Category? category = categoryList.FirstOrDefault(c =>
                c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                // Category doesn't exist
                return ShowProductNotFoundError(
                    productName,
                    categoryName,
                    transactionId,
                    rowNumber,
                    worksheetName,
                    $"Category '{categoryName}' does not exist");
            }

            // Check if product exists in the category
            bool productExists = category.ProductList.Any(p =>
                p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

            if (!productExists)
            {
                // Product doesn't exist in the category
                return ShowProductNotFoundError(
                    productName,
                    categoryName,
                    transactionId,
                    rowNumber,
                    worksheetName,
                    $"Product '{productName}' does not exist in category '{categoryName}'");
            }

            return InvalidValueAction.Continue;
        }

        /// <summary>
        /// Shows error dialog when a product is not found during transaction import.
        /// </summary>
        private static InvalidValueAction ShowProductNotFoundError(
            string productName,
            string categoryName,
            string transactionId,
            int rowNumber,
            string worksheetName,
            string errorDescription)
        {
            string message = "Product not found during transaction import:\n\n" +
                             $"Worksheet: {worksheetName}\n" +
                             $"Row: {rowNumber}\n" +
                             $"Transaction ID: {transactionId}\n" +
                             $"Product: '{productName}'\n" +
                             $"Category: '{categoryName}'\n" +
                             $"Error: {errorDescription}\n\n" +
                             "The transaction cannot be imported because the product does not exist in the system. " +
                             "Please ensure the product is created before importing transactions that reference it.\n\n" +
                             "How would you like to proceed?";

            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Product Not Found - Transaction Import",
                message,
                CustomMessageBoxIcon.Error,
                CustomMessageBoxButtons.SkipCancel); // Only Skip or Cancel, no Retry for missing products

            return result switch
            {
                CustomMessageBoxResult.Skip => InvalidValueAction.Skip,
                CustomMessageBoxResult.Cancel => InvalidValueAction.Cancel,
                _ => InvalidValueAction.Cancel // Default to cancel for safety
            };
        }

        // Export spreadsheet methods
        private static ConversionResult ConvertStringToDecimalWithOptions(string value, ImportError errorContext)
        {
            if (value == ReadOnlyVariables.EmptyCell)
            {
                return new ConversionResult { Value = 0, IsValid = true, Action = InvalidValueAction.Continue };
            }

            string currentValue = value;

            while (true)
            {
                try
                {
                    decimal result = Convert.ToDecimal(currentValue);
                    return new ConversionResult
                    {
                        Value = Math.Round(result, 2, MidpointRounding.AwayFromZero),
                        IsValid = true,
                        Action = InvalidValueAction.Continue
                    };
                }
                catch
                {
                    errorContext.InvalidValue = currentValue;
                    CustomMessageBoxResult result = ShowDetailedImportError(errorContext);

                    switch (result)
                    {
                        case CustomMessageBoxResult.Skip:  // Skip transaction
                            return new ConversionResult { Value = 0, IsValid = false, Action = InvalidValueAction.Skip };
                        case CustomMessageBoxResult.Cancel: // Cancel import
                            return new ConversionResult { Value = 0, IsValid = false, Action = InvalidValueAction.Cancel };
                        case CustomMessageBoxResult.Retry:  // Retry with user input
                            //   currentValue = newValue;
                            break; // Continue the loop with the new value
                        default:  // This shouldn't happen, but treat as cancel
                            return new ConversionResult { Value = 0, IsValid = false, Action = InvalidValueAction.Cancel };
                    }
                }
            }
        }
        private static CustomMessageBoxResult ShowDetailedImportError(ImportError error)
        {
            string message = "Invalid value found during import:\n\n" +
                             $"Worksheet: {error.WorksheetName}\n" +
                             $"Row: {error.RowNumber}\n" +
                             $"Transaction ID: {error.TransactionId}\n" +
                             $"Field: {error.FieldName}\n" +
                             $"Invalid Value: '{error.InvalidValue}'\n\n" +
                             "This value cannot be converted to a valid monetary amount. How would you like to proceed?";

            return CustomMessageBox.Show(
                "Import Error - Invalid Monetary Value",
                message,
                CustomMessageBoxIcon.Error,
                CustomMessageBoxButtons.SkipRetryCancel);
        }

        /// <summary>
        /// Checks if an item already exists and asks the user if they want to add it anyway.
        /// </summary>
        /// <returns>True if the item should be added, false if it should be skipped</returns>
        private static bool CheckIfItemExists(
            string itemNumber,
            HashSet<string> existingItems,
            HashSet<string> addedDuringImport,
            string itemTypeName)
        {
            bool alreadyExistsInSystem = existingItems.Contains(itemNumber);
            bool alreadyAddedDuringImport = addedDuringImport.Contains(itemNumber);

            if (alreadyExistsInSystem)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"{itemTypeName} # already exists",
                    $"The {itemTypeName.ToLowerInvariant()} #{itemNumber} already exists. Would you like to add this {itemTypeName.ToLowerInvariant()} anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                return result == CustomMessageBoxResult.Yes;
            }
            else if (alreadyAddedDuringImport)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"Duplicate {itemTypeName} # in Spreadsheet",
                    $"The {itemTypeName.ToLowerInvariant()} #{itemNumber} appears multiple times in this spreadsheet. Would you like to add this duplicate anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                return result == CustomMessageBoxResult.Yes;
            }

            return true;
        }

        /// <summary>
        /// This needs to be done after the row has been added to a DataGridView.
        /// </summary>
        private static void FormatNoteCell(DataGridViewRow row)
        {
            int noteCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? row.Cells.Count - 2 : row.Cells.Count - 1;
            DataGridViewCell lastCell = row.Cells[noteCellIndex];

            // Only add underline if the cell has a note
            if (lastCell.Value?.ToString() == ReadOnlyVariables.Show_text && lastCell.Tag != null)
            {
                DataGridViewManager.AddUnderlineToCell(lastCell);
            }
        }

        /// <summary>
        /// Imports data into a DataGridViewRow with source currency support and immediate cancellation support.
        /// </summary>
        /// <returns>ImportTransactionResult indicating the result of the import operation.</returns>
        private static ImportTransactionResult ImportTransaction(IXLRow row, DataGridViewRow newRow, int rowNumber, string worksheetName, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

            TagData tagData = new();

            // Get exchange rate from source currency to default currency
            string date = row.Cell(7).GetValue<string>();
            string defaultCurrency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);

            decimal exchangeRateToDefault = Currency.GetExchangeRate(sourceCurrency, defaultCurrency, date, false);
            if (exchangeRateToDefault == -1) { return ImportTransactionResult.Failed; }

            // Get exchange rate from source currency to USD for TagData storage
            decimal exchangeRateToUSD = Currency.GetExchangeRate(sourceCurrency, "USD", date, false);
            if (exchangeRateToUSD == -1) { return ImportTransactionResult.Failed; }

            string transactionId = row.Cell(1).GetValue<string>();  // Get transaction ID for error reporting

            string productName = row.Cell(3).GetValue<string>();
            string categoryName = row.Cell(4).GetValue<string>();
            bool isPurchase = worksheetName.Equals("Purchases", StringComparison.OrdinalIgnoreCase);

            // Make sure the product exists
            InvalidValueAction productValidationResult = ValidateProductExists(
                productName,
                categoryName,
                isPurchase,
                transactionId,
                rowNumber,
                worksheetName);

            switch (productValidationResult)
            {
                case InvalidValueAction.Cancel:
                    return ImportTransactionResult.Cancel;
                case InvalidValueAction.Skip:
                    return ImportTransactionResult.Skip;
                case InvalidValueAction.Continue:
                    break; // Continue processing
            }

            int noteCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? newRow.Cells.Count - 2 : newRow.Cells.Count - 1;
            for (int i = 0; i < noteCellIndex; i++)
            {
                // Check for cancellation before processing each cell
                if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

                string value = row.Cell(i + 1).GetValue<string>();

                if (i >= 8 && i <= 14)
                {
                    // Get field name for better error reporting
                    string fieldName = i switch
                    {
                        8 => "Price Per Unit",
                        9 => "Shipping",
                        10 => "Tax",
                        11 => "Fee",
                        12 => "Discount",
                        13 => "Charged Difference",
                        14 => "Charged Or Credited",
                        _ => "Unknown"
                    };

                    // Create error context for better error reporting
                    ImportError errorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = fieldName,
                        RowNumber = rowNumber,
                        WorksheetName = worksheetName
                    };

                    ConversionResult conversionResult = ConvertStringToDecimalWithOptions(value, errorContext);

                    // Handle the user's choice
                    switch (conversionResult.Action)
                    {
                        case InvalidValueAction.Cancel:
                            return ImportTransactionResult.Cancel; // This will stop the import process

                        case InvalidValueAction.Skip:
                            return ImportTransactionResult.Skip; // This will skip this transaction but continue importing others

                        case InvalidValueAction.Continue:
                            // Process normally
                            break;
                    }

                    decimal sourceValue = conversionResult.Value;
                    bool useEmpty = false;

                    switch (i)
                    {
                        case 8:
                            if (value == ReadOnlyVariables.EmptyCell)
                            {
                                useEmpty = true;
                            }
                            else
                            {
                                // Store USD value in TagData
                                tagData.PricePerUnitUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            }
                            break;
                        case 9:
                            tagData.ShippingUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case 10:
                            tagData.TaxUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case 11:
                            tagData.FeeUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case 12:
                            tagData.DiscountUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case 13:
                            tagData.ChargedDifferenceUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case 14:
                            tagData.ChargedOrCreditedUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                    }

                    // Store the display value in default currency
                    newRow.Cells[i].Value = useEmpty
                        ? ReadOnlyVariables.EmptyCell
                        : Math.Round(sourceValue * exchangeRateToDefault, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    newRow.Cells[i].Value = value;
                }
            }

            // Store the original currency information in TagData
            tagData.OriginalCurrency = sourceCurrency;
            if (exchangeRateToUSD != 0)
            {
                tagData.OriginalPricePerUnit = tagData.PricePerUnitUSD > 0 ? Math.Round(tagData.PricePerUnitUSD / exchangeRateToUSD, 2) : 0;
                tagData.OriginalShipping = Math.Round(tagData.ShippingUSD / exchangeRateToUSD, 2);
                tagData.OriginalTax = Math.Round(tagData.TaxUSD / exchangeRateToUSD, 2);
                tagData.OriginalFee = Math.Round(tagData.FeeUSD / exchangeRateToUSD, 2);
                tagData.OriginalDiscount = Math.Round(tagData.DiscountUSD / exchangeRateToUSD, 2);
                tagData.OriginalChargedDifference = Math.Round(tagData.ChargedDifferenceUSD / exchangeRateToUSD, 2);
                tagData.OriginalChargedOrCredited = Math.Round(tagData.ChargedOrCreditedUSD / exchangeRateToUSD, 2);
            }

            // Set the note
            DataGridViewCell noteCell = newRow.Cells[noteCellIndex];
            IXLCell excelNoteCell = row.Cell(16);
            string excelNoteCellValue = excelNoteCell.GetValue<string>();

            if (string.IsNullOrWhiteSpace(excelNoteCellValue) || excelNoteCellValue == ReadOnlyVariables.EmptyCell)
            {
                noteCell.Value = ReadOnlyVariables.EmptyCell;
            }
            else
            {
                noteCell.Value = ReadOnlyVariables.Show_text;
                noteCell.Tag = excelNoteCellValue;
            }

            newRow.Tag = tagData;
            return ImportTransactionResult.Success;
        }
        private static ImportTransactionResult ImportItemsInTransaction(IXLRow row, DataGridViewRow transaction, int baseRowNumber, string worksheetName, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

            TagData tagData = (TagData)transaction.Tag;
            List<string> items = [];
            int currentRowOffset = 0;

            // Get exchange rates
            string date = transaction.Cells[6].Value?.ToString() ?? DateTime.Today.ToString("yyyy-MM-dd");
            decimal exchangeRateToUSD = Currency.GetExchangeRate(sourceCurrency, "USD", date, false);
            decimal exchangeRateToDefault = Currency.GetExchangeRate(sourceCurrency, DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType), date, false);

            if (exchangeRateToUSD == -1 || exchangeRateToDefault == -1)
            {
                return ImportTransactionResult.Failed;
            }

            bool isPurchase = worksheetName.Equals("Purchases", StringComparison.OrdinalIgnoreCase);

            while (true)
            {
                // Check for cancellation before processing each item
                if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

                IXLRow nextRow = row.RowBelow();
                currentRowOffset++;

                // Check if the row has any data
                if (nextRow.IsEmpty())
                {
                    break;
                }

                // Get data from every item in the transaction
                string number = nextRow.Cell(1).GetValue<string>();

                // Check if the next row has no number, indicating multiple items
                if (string.IsNullOrEmpty(number))
                {
                    string productName = nextRow.Cell(3).Value.ToString();
                    string categoryName = nextRow.Cell(4).Value.ToString();
                    string currentCountry = nextRow.Cell(5).Value.ToString();
                    string currentCompany = nextRow.Cell(6).Value.ToString();

                    // Make sure the product exists
                    string transactionId = transaction.Cells[0].Value?.ToString() ?? "Unknown";
                    InvalidValueAction productValidationResult = ValidateProductExists(
                        productName,
                        categoryName,
                        isPurchase,
                        transactionId,
                        baseRowNumber + currentRowOffset,
                        worksheetName);

                    switch (productValidationResult)
                    {
                        case InvalidValueAction.Cancel:
                            return ImportTransactionResult.Cancel;
                        case InvalidValueAction.Skip:
                            return ImportTransactionResult.Skip;
                        case InvalidValueAction.Continue:
                            break; // Continue processing
                    }

                    // Use enhanced error handling for quantity
                    ImportError quantityErrorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = "Item Quantity",
                        RowNumber = baseRowNumber + currentRowOffset,
                        WorksheetName = worksheetName
                    };

                    ConversionResult quantityResult = ConvertStringToDecimalWithOptions(
                        nextRow.Cell(8).Value.ToString(), quantityErrorContext);

                    if (quantityResult.Action == InvalidValueAction.Cancel)
                    {
                        return ImportTransactionResult.Cancel;
                    }
                    if (quantityResult.Action == InvalidValueAction.Skip)
                    {
                        return ImportTransactionResult.Skip;
                    }

                    // Use enhanced error handling for price per unit
                    ImportError priceErrorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = "Item Price Per Unit",
                        RowNumber = baseRowNumber + currentRowOffset,
                        WorksheetName = worksheetName
                    };

                    ConversionResult priceResult = ConvertStringToDecimalWithOptions(
                        nextRow.Cell(9).Value.ToString(), priceErrorContext);

                    if (priceResult.Action == InvalidValueAction.Cancel)
                    {
                        return ImportTransactionResult.Cancel;
                    }
                    if (priceResult.Action == InvalidValueAction.Skip)
                    {
                        return ImportTransactionResult.Skip;
                    }

                    decimal quantity = quantityResult.Value;
                    decimal sourcePricePerUnit = priceResult.Value;

                    // Convert prices to default currency for display and USD for storage
                    decimal defaultPricePerUnit = Math.Round(sourcePricePerUnit * exchangeRateToDefault, 2, MidpointRounding.AwayFromZero);
                    decimal usdPricePerUnit = Math.Round(sourcePricePerUnit * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);

                    string item = string.Join(",",
                        productName,
                        categoryName,
                        currentCountry,
                        currentCompany,
                        quantity.ToString(),
                        defaultPricePerUnit.ToString("F2"),  // Default currency for display
                        usdPricePerUnit.ToString("F2")       // USD for storage
                    );

                    items.Add(item);
                    row = nextRow;  // Move to the next row
                }
                else { break; }
            }

            // Save
            if (items.Count > 0)
            {
                transaction.Tag = (items, tagData);
            }

            return ImportTransactionResult.Success;
        }

        /// <summary>
        /// Rollback all changes made during the import session.
        /// </summary>
        public static void RollbackImportSession(ImportSession session)
        {
            if (session == null || !session.HasChanges())
            {
                return;
            }

            // Remove added accountants
            foreach (string accountant in session.AddedAccountants)
            {
                MainMenu_Form.Instance.AccountantList.Remove(accountant);
            }

            // Remove added companies
            foreach (string company in session.AddedCompanies)
            {
                MainMenu_Form.Instance.CompanyList.Remove(company);
            }

            // Remove added products
            foreach (KeyValuePair<string, List<Product>> categoryProducts in session.AddedProducts)
            {
                string categoryName = categoryProducts.Key;
                List<Product> productsToRemove = categoryProducts.Value;

                // Find the category in both purchase and sale lists
                Category? purchaseCategory = MainMenu_Form.Instance.CategoryPurchaseList
                    .FirstOrDefault(c => c.Name == categoryName);
                Category? saleCategory = MainMenu_Form.Instance.CategorySaleList
                    .FirstOrDefault(c => c.Name == categoryName);

                if (purchaseCategory != null)
                {
                    foreach (Product product in productsToRemove)
                    {
                        purchaseCategory.ProductList.Remove(product);
                    }
                }

                if (saleCategory != null)
                {
                    foreach (Product product in productsToRemove)
                    {
                        saleCategory.ProductList.Remove(product);
                    }
                }
            }

            // Remove added categories
            foreach (Category category in session.AddedCategories)
            {
                MainMenu_Form.Instance.CategoryPurchaseList.Remove(category);
                MainMenu_Form.Instance.CategorySaleList.Remove(category);
            }

            // Remove added purchase rows
            foreach (DataGridViewRow row in session.AddedPurchaseRows)
            {
                if (MainMenu_Form.Instance.Purchase_DataGridView.Rows.Contains(row))
                {
                    MainMenu_Form.Instance.Purchase_DataGridView.Rows.Remove(row);
                }
            }

            // Remove added sale rows
            foreach (DataGridViewRow row in session.AddedSaleRows)
            {
                if (MainMenu_Form.Instance.Sale_DataGridView.Rows.Contains(row))
                {
                    MainMenu_Form.Instance.Sale_DataGridView.Rows.Remove(row);
                }
            }

            // Save the reverted state
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.AccountantList, MainMenu_Form.SelectedOption.Accountants);
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.CompanyList, MainMenu_Form.SelectedOption.Companies);
            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategoryPurchases);
            MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategorySales);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
        }

        /// <summary>
        /// Commit all changes made during the import session.
        /// </summary>
        public static void CommitImportSession(ImportSession session)
        {
            if (session == null || !session.HasChanges())
            {
                return;
            }

            // Save all changes to files
            if (session.AddedAccountants.Count > 0)
            {
                MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.AccountantList, MainMenu_Form.SelectedOption.Accountants);
            }

            if (session.AddedCompanies.Count > 0)
            {
                MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.CompanyList, MainMenu_Form.SelectedOption.Companies);
            }

            if (session.AddedProducts.Count > 0 || session.AddedCategories.Count > 0)
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategoryPurchases);
                MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategorySales);
            }

            if (session.AddedPurchaseRows.Count > 0)
            {
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            }

            if (session.AddedSaleRows.Count > 0)
            {
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
            }
        }
        public static void ExportSpreadsheet(string filePath, string currency)
        {
            filePath = Directories.GetNewFileNameIfItAlreadyExists(filePath);

            // Parse currency code from the ComboBox format (e.g., "USD ($)" -> "USD")
            string currencyCode = ParseCurrencyCode(currency);
            string currencySymbol = Currency.GetSymbol(currencyCode);

            using XLWorkbook workbook = new();

            IXLWorksheet purchaseWorksheet = workbook.Worksheets.Add("Purchases");
            AddTransactionToWorksheet(purchaseWorksheet, MainMenu_Form.Instance.Purchase_DataGridView, currencyCode, currencySymbol);

            IXLWorksheet salesWorksheet = workbook.Worksheets.Add("Sales");
            AddTransactionToWorksheet(salesWorksheet, MainMenu_Form.Instance.Sale_DataGridView, currencyCode, currencySymbol);

            IXLWorksheet purchaseProductsWorksheet = workbook.Worksheets.Add("Purchase products");
            AddProductsToWorksheet(purchaseProductsWorksheet, MainMenu_Form.Instance.CategoryPurchaseList);

            IXLWorksheet saleProductsWorksheet = workbook.Worksheets.Add("Sale products");
            AddProductsToWorksheet(saleProductsWorksheet, MainMenu_Form.Instance.CategorySaleList);

            IXLWorksheet companiesWorksheet = workbook.Worksheets.Add("Companies");
            AddCompaniesToWorksheet(companiesWorksheet);

            IXLWorksheet accountantsWorksheet = workbook.Worksheets.Add("Accountants");
            AddAccountantsToWorksheet(accountantsWorksheet);

            // Save the file
            workbook.SaveAs(filePath);
        }
        private static string ParseCurrencyCode(string currencySelection)
        {
            // Extract currency code (first 3 characters)
            string[] parts = currencySelection.Split(' ');
            if (parts.Length > 0 && parts[0].Length >= 3)
            {
                return parts[0].Substring(0, 3).ToUpperInvariant();
            }

            return "USD";  // Default fallback
        }
        private static void AddTransactionToWorksheet(IXLWorksheet worksheet, DataGridView dataGridView, string targetCurrency, string currencySymbol)
        {
            // Add headers and format them
            int excelColumnIndex = 1;
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                // Skip the "Has Receipt" column
                if (dataGridView.Columns[i].Name == MainMenu_Form.Column.HasReceipt.ToString())
                {
                    continue;
                }

                IXLCell cell = worksheet.Cell(1, excelColumnIndex);
                cell.Value = dataGridView.Columns[i].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                excelColumnIndex++;
            }

            int receiptCellIndex = excelColumnIndex;

            // Add header for the receipt column
            worksheet.Cell(1, receiptCellIndex).Value = "Receipt";
            worksheet.Cell(1, receiptCellIndex).Style.Font.Bold = true;
            worksheet.Cell(1, receiptCellIndex).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Add currency message
            int messageCellIndex = receiptCellIndex + 2;
            string currencyMessage = targetCurrency == "USD" ? "All prices in USD" : $"All prices in {targetCurrency}";
            worksheet.Cell(1, messageCellIndex).Value = currencyMessage;
            worksheet.Cell(1, messageCellIndex).Style.Font.Bold = true;

            string currencyFormatPattern = $"\"{currencySymbol}\"#,##0.00";
            string receiptFileName = ReadOnlyVariables.EmptyCell;
            int currentRow = 2;
            int rowForReceipt = 2;

            // Add transactions
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                switch (row.Tag)
                {
                    case (List<string> itemList, TagData tagData) when itemList.Count > 0:
                        // Is there a receipt
                        byte receiptOffset = 0;
                        string receipt = itemList[^1];
                        if (receipt.StartsWith(ReadOnlyVariables.Receipt_text))
                        {
                            receiptOffset = 1;
                            receiptFileName = Path.GetFileName(receipt);
                        }
                        else
                        {
                            worksheet.Cell(currentRow, receiptCellIndex).Value = ReadOnlyVariables.EmptyCell;
                        }

                        AddRowToWorksheet(worksheet, row, currentRow, tagData, targetCurrency, currencyFormatPattern);

                        // Add items in transaction if they exist in itemList
                        for (int i = 0; i < itemList.Count - receiptOffset; i++)
                        {
                            currentRow++;
                            string[] values = itemList[i].Split(',');

                            AddItemRowToWorksheet(worksheet, values, currentRow, targetCurrency, currencyFormatPattern);
                        }
                        break;

                    case (string tagString, TagData tagData):
                        AddRowToWorksheet(worksheet, row, currentRow, tagData, targetCurrency, currencyFormatPattern);
                        receiptFileName = Path.GetFileName(tagString);
                        break;
                }

                // Add receipt to the last cell
                worksheet.Cell(rowForReceipt, receiptCellIndex).Value = receiptFileName;

                currentRow++;
                rowForReceipt = currentRow;  // This ensures that the receipt is not added to the bottom of a transaction with multiple items
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddRowToWorksheet(IXLWorksheet worksheet, DataGridViewRow row, int currentRow, TagData tagData, string targetCurrency, string currencyFormatPattern)
        {
            int excelColumnIndex = 1;

            // Get exchange rate from USD to target currency
            string transactionDate = row.Cells[6].Value?.ToString() ?? DateTime.Today.ToString("yyyy-MM-dd");
            decimal exchangeRate = GetExchangeRateForExport(transactionDate, targetCurrency);

            for (int i = 0; i < row.Cells.Count; i++)
            {
                // Skip the Notes column - it will be handled separately
                int notesColumnIndex = Properties.Settings.Default.ShowHasReceiptColumn
                    ? row.Cells.Count - 2
                    : row.Cells.Count - 1;

                if (i == notesColumnIndex)
                {
                    break;
                }

                IXLCell excelCell = worksheet.Cell(currentRow, excelColumnIndex);

                if (tagData != null && i >= 8 && i <= 14)
                {
                    (decimal usdValue, bool useEmpty) = i switch
                    {
                        8 => (tagData.PricePerUnitUSD, tagData.PricePerUnitUSD == 0),
                        9 => (tagData.ShippingUSD, false),
                        10 => (tagData.TaxUSD, false),
                        11 => (tagData.FeeUSD, false),
                        12 => (tagData.DiscountUSD, false),
                        13 => (tagData.ChargedDifferenceUSD, false),
                        14 => (tagData.ChargedOrCreditedUSD, false),
                        _ => (0, false)
                    };

                    if (useEmpty)
                    {
                        excelCell.Value = ReadOnlyVariables.EmptyCell;
                    }
                    else
                    {
                        // Convert from USD to target currency
                        decimal convertedValue = Math.Round(usdValue * exchangeRate, 2, MidpointRounding.AwayFromZero);
                        excelCell.Value = convertedValue;
                        excelCell.Style.NumberFormat.Format = currencyFormatPattern;
                    }
                }
                else
                {
                    // Handle other cell types
                    object? cellValue = row.Cells[i].Value;

                    // Special handling for quantity column (index 7) to ensure it's treated as numeric
                    if (i == 7)
                    {
                        if (cellValue != null && decimal.TryParse(cellValue.ToString(), out decimal quantityValue))
                        {
                            excelCell.Value = quantityValue;
                        }
                        else
                        {
                            excelCell.Value = cellValue?.ToString();
                        }
                        excelCell.Style.NumberFormat.Format = _numberFormatPattern;
                    }
                    else
                    {
                        excelCell.Value = cellValue?.ToString();
                    }
                }

                excelColumnIndex++;
            }

            // Handle the Notes column
            int notesCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? row.Cells.Count - 2 : row.Cells.Count - 1;
            DataGridViewCell notesCell = row.Cells[notesCellIndex];
            string? notesCellValue = notesCell.Value?.ToString();
            IXLCell notesExcelCell = worksheet.Cell(currentRow, excelColumnIndex);

            notesExcelCell.Value = notesCellValue == ReadOnlyVariables.EmptyCell
                ? ReadOnlyVariables.EmptyCell
                : (notesCellValue == ReadOnlyVariables.Show_text && notesCell.Tag != null)
                    ? notesCell.Tag.ToString()
                    : notesCellValue;
        }
        private static void AddItemRowToWorksheet(IXLWorksheet worksheet, string[] row, int currentRow, string targetCurrency, string currencyFormatPattern)
        {
            // Get transaction date from the main row for exchange rate calculation
            string transactionDate = worksheet.Cell(currentRow - GetItemOffsetForTransaction(worksheet, currentRow), 7).Value.ToString()
                ?? DateTime.Today.ToString("yyyy-MM-dd");
            decimal exchangeRate = GetExchangeRateForExport(transactionDate, targetCurrency);

            for (int i = 0; i < row.Length - 1; i++)  // Skip the total value with - 1
            {
                // Shift the data one column to the right after the date column
                int columnIndex = i < 4 ? i : i + 1;
                IXLCell excelCell = worksheet.Cell(currentRow, columnIndex + 3);

                string cellValue = row[i];

                // Check if this should be a numeric value (quantity, price, etc.)
                if (i == 4 || i == 5) // quantity and price columns
                {
                    if (decimal.TryParse(cellValue, out decimal numericValue))
                    {
                        if (i == 5)  // price column - convert from USD to target currency
                        {
                            decimal convertedPrice = Math.Round(numericValue * exchangeRate, 2, MidpointRounding.AwayFromZero);
                            excelCell.Value = convertedPrice;
                            excelCell.Style.NumberFormat.Format = currencyFormatPattern;
                        }
                        else // quantity column
                        {
                            excelCell.Value = numericValue;
                            excelCell.Style.NumberFormat.Format = _decimalFormatPattern;
                        }
                    }
                    else
                    {
                        excelCell.Value = cellValue;
                    }
                }
                else
                {
                    excelCell.Value = cellValue;
                }
            }
        }
        private static int GetItemOffsetForTransaction(IXLWorksheet worksheet, int currentRow)
        {
            // Look backwards to find how many item rows we've added for this transaction
            int offset = 0;
            for (int row = currentRow - 1; row >= 2; row--)
            {
                // If we find a row with a transaction ID (first column not empty), we've found the main transaction row
                if (!string.IsNullOrEmpty(worksheet.Cell(row, 1).Value.ToString()))
                {
                    break;
                }
                offset++;
            }
            return offset;
        }
        private static decimal GetExchangeRateForExport(string transactionDate, string targetCurrency)
        {
            if (targetCurrency == "USD")
            {
                return 1.0m;  // No conversion needed
            }

            try
            {
                // Parse date to ensure correct format
                if (DateTime.TryParse(transactionDate, out DateTime parsedDate))
                {
                    string formattedDate = parsedDate.ToString("yyyy-MM-dd");
                    decimal rate = Currency.GetExchangeRate("USD", targetCurrency, formattedDate, false);
                    return rate > 0 ? rate : 1.0m;  // Fallback to 1:1 if rate fetch fails
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the export
                Log.Write(1, $"Failed to get exchange rate for {targetCurrency}: {ex.Message}");
            }

            return 1.0m;  // Fallback to 1:1 ratio
        }
        private static void AddCompaniesToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Company name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string company in MainMenu_Form.Instance.CompanyList)
            {
                worksheet.Cell(currentRow, 1).Value = company;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddAccountantsToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Accountant name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string accountant in MainMenu_Form.Instance.AccountantList)
            {
                worksheet.Cell(currentRow, 1).Value = accountant;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddProductsToWorksheet(IXLWorksheet worksheet, List<Category> list)
        {
            worksheet.Cell(1, 1).Value = "Product ID";
            worksheet.Cell(1, 2).Value = "Product name";
            worksheet.Cell(1, 3).Value = "Category";
            worksheet.Cell(1, 4).Value = "Country of origin";
            worksheet.Cell(1, 5).Value = "Company of origin";

            // Format title cells
            for (int i = 1; i <= 5; i++)
            {
                worksheet.Cell(1, i).Style.Font.Bold = true;
                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            int currentRow = 2;
            foreach (Category category in list)
            {
                foreach (Product product in category.ProductList)
                {
                    worksheet.Cell(currentRow, 1).Value = product.ProductID;
                    worksheet.Cell(currentRow, 2).Value = product.Name;
                    worksheet.Cell(currentRow, 3).Value = category.Name;
                    worksheet.Cell(currentRow, 4).Value = product.CountryOfOrigin;
                    worksheet.Cell(currentRow, 5).Value = product.CompanyOfOrigin;
                    currentRow++;
                }
            }

            worksheet.Columns().AdjustToContents();
        }

        // Export charts to Microsoft Excel
        public static void ExportChartToExcel(Dictionary<string, double> data, string filePath, eChartType chartType, string chartTitle, string column1Text, string column2Text, bool isSpline = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ExcelPackage.License.SetNonCommercialPersonal("Argo");
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add headers
            worksheet.Cells["A1"].Value = LanguageManager.TranslateString(column1Text);
            worksheet.Cells["B1"].Value = LanguageManager.TranslateString(column2Text);

            // Format headers
            ExcelRange headerRange = worksheet.Cells["A1:B1"];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

            // Add data
            int row = 2;
            foreach (KeyValuePair<string, double> item in data.OrderBy(x => x.Key))
            {
                worksheet.Cells[row, 1].Value = item.Key;
                worksheet.Cells[row, 2].Value = item.Value;
                worksheet.Cells[row, 2].Style.Numberformat.Format = _currencyFormatPattern;
                row++;
            }

            ExcelChart chart = CreateChart(worksheet, chartTitle, chartType, false);

            // Configure chart
            ExcelChartSerie series = chart.Series.Add(worksheet.Cells[$"B2:B{row - 1}"], worksheet.Cells[$"A2:A{row - 1}"]);
            if (isSpline && chartType == eChartType.Line)
            {
                ((ExcelLineChartSerie)series).Smooth = true;
            }
            chart.Legend.Remove();

            worksheet.Columns[1, 2].AutoFit();
            package.SaveAs(new FileInfo(filePath));

            TrackChartExport(stopwatch, filePath, isSpline ? ExportType.GoogleSheetsChart : ExportType.ExcelSheetsChart);
        }
        public static void ExportMultiDataSetChartToExcel(Dictionary<string, Dictionary<string, double>> data, string filePath, eChartType chartType, string chartTitle, bool isSpline = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ExcelPackage.License.SetNonCommercialPersonal("Argo");
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Get all series names
            List<string> seriesNames = data.First().Value.Keys.ToList();

            // Add headers
            worksheet.Cells[1, 1].Value = LanguageManager.TranslateString("Date");
            for (int i = 0; i < seriesNames.Count; i++)
            {
                worksheet.Cells[1, i + 2].Value = seriesNames[i];
            }

            // Format headers
            ExcelRange headerRange = worksheet.Cells[1, 1, 1, seriesNames.Count + 1];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

            // Add data
            int row = 2;
            foreach (KeyValuePair<string, Dictionary<string, double>> dateEntry in data.OrderBy(x => x.Key))
            {
                worksheet.Cells[row, 1].Value = dateEntry.Key;

                for (int i = 0; i < seriesNames.Count; i++)
                {
                    worksheet.Cells[row, i + 2].Value = dateEntry.Value[seriesNames[i]];

                    // Use currency formatting for money values
                    worksheet.Cells[row, i + 2].Style.Numberformat.Format = _currencyFormatPattern;
                }
                row++;
            }

            // Create chart
            ExcelChart chart = CreateChart(worksheet, chartTitle, chartType, true);
            chart.Legend.Position = eLegendPosition.Top;

            // Add series to chart
            for (int i = 0; i < seriesNames.Count; i++)
            {
                ExcelChartSerie series = chart.Series.Add(
                    worksheet.Cells[2, i + 2, row - 1, i + 2], // Y values
                    worksheet.Cells[2, 1, row - 1, 1]          // X values
                );
                if (isSpline && chartType == eChartType.Line)
                {
                    ((ExcelLineChartSerie)series).Smooth = true;
                }
                series.Header = seriesNames[i];
            }

            worksheet.Columns.AutoFit();
            package.SaveAs(new FileInfo(filePath));

            TrackChartExport(stopwatch, filePath, isSpline ? ExportType.GoogleSheetsChart : ExportType.ExcelSheetsChart);
        }
        private static void TrackChartExport(Stopwatch stopwatch, string filePath, ExportType exportType)
        {
            stopwatch.Stop();
            string readableSize = "0 Bytes";

            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new(filePath);
                long fileSizeBytes = fileInfo.Length;
                readableSize = Tools.ConvertBytesToReadableSize(fileSizeBytes);
            }

            Dictionary<ExportDataField, object> exportData = new()
            {
                { ExportDataField.ExportType, exportType },
                { ExportDataField.DurationMS, stopwatch.ElapsedMilliseconds },
                { ExportDataField.FileSize, readableSize }
            };

            AnonymousDataManager.AddExportData(exportData);
        }

        /// <summary>
        /// Creates and configures an Excel chart with default position and size.
        /// </summary>
        /// <returns>The created Excel chart.</returns>
        private static ExcelChart CreateChart(ExcelWorksheet worksheet, string chartTitle, eChartType chartType, bool isMultiDataset)
        {
            ExcelChart chart = worksheet.Drawings.AddChart(chartTitle, chartType);
            chart.SetPosition(0, 0, isMultiDataset ? 4 : 3, 0);
            chart.SetSize(800, 400);
            chart.Title.Text = chartTitle;

            return chart;
        }
    }
}