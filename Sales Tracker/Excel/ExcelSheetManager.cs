using ClosedXML.Excel;
using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.GridView;

namespace Sales_Tracker.Excel
{
    /// <summary>
    /// Handles the import and export of data to and from .xlsx spreadsheets, including transactions
    /// and receipts. It also has immediate cancellation support and multi-currency import capabilities.
    /// </summary>
    public class ExcelSheetManager
    {
        // Constants
        private const string _numberFormatPattern = "0";

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

        // Import data with rollback and cancellation support
        public static bool ImportAccountantsData(IXLWorksheet worksheet, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            return ImportSimpleListData(
                worksheet,
                MainMenu_Form.Instance.AccountantList,
                MainMenu_Form.SelectedOption.Accountants,
                "Accountant",
                Accountants_Form.Column.AccountantName,
                session.AddedAccountants,
                session);
        }
        public static bool ImportCompaniesData(IXLWorksheet worksheet, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            return ImportSimpleListData(
                worksheet,
                MainMenu_Form.Instance.CompanyList,
                MainMenu_Form.SelectedOption.Companies,
                "Company",
                Companies_Form.Column.Company,
                session.AddedCompanies,
                session);
        }

        /// <summary>
        /// Helper method for importing simple list data like accountants or companies.
        /// </summary>
        private static bool ImportSimpleListData(
            IXLWorksheet worksheet,
            List<string> existingList,
            MainMenu_Form.SelectedOption optionType,
            string itemTypeName,
            Enum column,
            List<string> addedItems,
            ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateSimpleListColumns(worksheet, column, GetColumnHeadersForType(optionType));
            if (!validation.IsValid)
            {
                ShowColumnValidationError(itemTypeName, validation.MissingColumns);
                return false;
            }

            // Always skip header row (row 1) since we use it for column lookup
            IEnumerable<IXLRow> rowsToProcess = worksheet.RowsUsed().Skip(1);

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

                string itemName = ExcelColumnHelper.GetCellValue(row, column);
                if (string.IsNullOrWhiteSpace(itemName) || itemName == ReadOnlyVariables.EmptyCell)
                {
                    continue;
                }

                string itemNameLower = itemName.ToLowerInvariant();

                // If the item already exists
                if (existingItems.Contains(itemNameLower))
                {
                    // Check if user has already made a "to all" choice for this entity type
                    bool? existingChoice = itemTypeName switch
                    {
                        "Accountant" => session.UserChoices.DuplicateAccountantChoice,
                        "Company" => session.UserChoices.DuplicateCompanyChoice,
                        _ => null
                    };

                    bool shouldSkip = false;

                    if (existingChoice.HasValue)
                    {
                        shouldSkip = !existingChoice.Value;  // If choice is false, skip (don't import)
                    }
                    else
                    {
                        // Show dialog with "Yes to All" and "No to All" options
                        CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                            "{0} already exists",
                            "The {1} '{2}' already exists. Would you like to import it anyway?",
                            CustomMessageBoxIcon.Question,
                            CustomMessageBoxButtons.YesNoAll,
                            itemTypeName, itemTypeName.ToLowerInvariant(), itemName);

                        switch (result)
                        {
                            case CustomMessageBoxResult.Yes:
                                shouldSkip = false;  // Import this one
                                break;
                            case CustomMessageBoxResult.No:
                                shouldSkip = true;  // Skip this one
                                break;
                            case CustomMessageBoxResult.YesAll:
                                // Import all duplicates from now on
                                if (itemTypeName == "Accountant")
                                {
                                    session.UserChoices.DuplicateAccountantChoice = true;
                                }
                                else if (itemTypeName == "Company")
                                {
                                    session.UserChoices.DuplicateCompanyChoice = true;
                                }
                                shouldSkip = false;
                                break;
                            case CustomMessageBoxResult.NoAll:
                                // Skip all duplicates from now on
                                if (itemTypeName == "Accountant")
                                {
                                    session.UserChoices.DuplicateAccountantChoice = false;
                                }
                                else if (itemTypeName == "Company")
                                {
                                    session.UserChoices.DuplicateCompanyChoice = false;
                                }
                                shouldSkip = true;
                                break;
                            default:
                                shouldSkip = true;
                                break;
                        }
                    }

                    if (shouldSkip)
                    {
                        continue;
                    }
                }

                existingList.Add(itemName);
                addedDuringImport.Add(itemNameLower);
                addedItems?.Add(itemName);  // Track for rollback
                wasSomethingImported = true;
            }

            if (addedItems == null)
            {
                MainMenu_Form.SaveListToFile(existingList, optionType);
            }

            return wasSomethingImported;
        }
        private static Dictionary<Enum, string> GetColumnHeadersForType(MainMenu_Form.SelectedOption optionType)
        {
            return optionType switch
            {
                MainMenu_Form.SelectedOption.Accountants =>
                    Accountants_Form.ColumnHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                MainMenu_Form.SelectedOption.Companies =>
                    Companies_Form.ColumnHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                _ => []
            };
        }
        public static bool ImportProductsData(IXLWorksheet worksheet, bool isPurchase, ImportSession session)
        {
            if (session.IsCancelled == true) { return false; }

            // Create column helper and validate required columns
            ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateProductColumns(worksheet);

            if (!validation.IsValid)
            {
                ShowColumnValidationError("Products", validation.MissingColumns);
                return false;
            }

            // Always skip header row (row 1) since we use it for column lookup
            IEnumerable<IXLRow> rowsToProcess = worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            List<Category> list = isPurchase
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

                string productId = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.ProductID);
                string productName = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.ProductName);
                string categoryName = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.ProductCategory);
                string countryOfOrigin = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.CountryOfOrigin);
                string companyOfOrigin = ExcelColumnHelper.GetCellValue(row, Products_Form.Column.CompanyOfOrigin);

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(categoryName))
                {
                    continue;
                }

                countryOfOrigin = Country.NormalizeCountryName(countryOfOrigin);

                if (!ValidateCountry(countryOfOrigin, session))
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
                    isPurchase,
                    session))
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

            if (session == null)
            {
                MainMenu_Form.Instance.SaveCategoriesToFile(isPurchase
                    ? MainMenu_Form.SelectedOption.CategoryPurchases
                    : MainMenu_Form.SelectedOption.CategorySales);
            }

            return wasSomethingImported;
        }
        private static bool ValidateCountry(string countryName, ImportSession session)
        {
            bool countryExists = Country.CountrySearchResults.Any(
                c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase));

            if (!countryExists)
            {
                // Check if user has already made a "to all" choice
                if (session.UserChoices.CountryNotFoundChoice.HasValue)
                {
                    return session.UserChoices.CountryNotFoundChoice.Value;
                }

                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                    "Country does not exist",
                    "Country '{0}' does not exist in the system. Please check the documentation for more information. Do you want to skip this product and continue?",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.YesNoAll,
                    countryName);

                switch (result)
                {
                    case CustomMessageBoxResult.Yes:
                        return false;  // Skip this product
                    case CustomMessageBoxResult.No:
                        return true;  // Don't skip, continue with invalid country
                    case CustomMessageBoxResult.YesAll:
                        session.UserChoices.CountryNotFoundChoice = false;  // Skip all invalid countries
                        return false;
                    case CustomMessageBoxResult.NoAll:
                        session.UserChoices.CountryNotFoundChoice = true;  // Continue with all invalid countries
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }
        private static void EnsureCompanyExists(string companyName, ImportSession session)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return;
            }

            // Check if company already exists (case-insensitive)
            bool companyExists = MainMenu_Form.Instance.CompanyList.Any(c =>
                c.Equals(companyName, StringComparison.OrdinalIgnoreCase));

            if (!companyExists)
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
            bool purchase,
            ImportSession session)
        {
            if (existingProducts.TryGetValue(categoryName, out HashSet<string> existingCategoryProducts) &&
                existingCategoryProducts.Contains(productNameLower))
            {
                // Check if user has already made a "to all" choice for duplicate products
                if (session.UserChoices.DuplicateProductChoice.HasValue)
                {
                    return !session.UserChoices.DuplicateProductChoice.Value;  // If choice is false, return true (exists, don't import)
                }

                string type = purchase ? "purchase" : "sale";

                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                    "Product already exists",
                    "The product for {0} '{1}' already exists. Would you like to import it anyway?",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.YesNoAll,
                    type, productName);

                switch (result)
                {
                    case CustomMessageBoxResult.Yes:
                        return false;  // Don't skip, import this one
                    case CustomMessageBoxResult.No:
                        return true;  // Skip this one
                    case CustomMessageBoxResult.YesAll:
                        session.UserChoices.DuplicateProductChoice = true;  // Import all duplicates
                        return false;
                    case CustomMessageBoxResult.NoAll:
                        session.UserChoices.DuplicateProductChoice = false;  // Skip all duplicates
                        return true;
                    default:
                        return true;  // Default to skip
                }
            }

            return false;  // Product doesn't exist, proceed with import
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

        public static ImportSummary ImportPurchaseData(IXLWorksheet worksheet, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true)
            {
                return new ImportSummary { WasCancelled = true };
            }
            return ImportTransactionData(worksheet, true, sourceCurrency, session);
        }
        public static ImportSummary ImportSalesData(IXLWorksheet worksheet, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true)
            {
                return new ImportSummary { WasCancelled = true };
            }
            return ImportTransactionData(worksheet, false, sourceCurrency, session);
        }
        public static int ImportReceiptsData(IXLWorksheet worksheet, string receiptsFolderPath, bool isPurchase, ImportSession session = null)
        {
            // Always skip header row (row 1) since we use it for column lookup
            IEnumerable<IXLRow> rowsToProcess = worksheet.RowsUsed().Skip(1);

            int importedCount = 0;

            foreach (IXLRow row in rowsToProcess)
            {
                string transactionId = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ID);
                string receiptFileName = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Receipt);

                DataGridViewRow? targetRow = FindTransactionRow(transactionId, isPurchase);

                // Skip item rows (rows without transaction IDs)
                if (string.IsNullOrEmpty(transactionId) || transactionId == ReadOnlyVariables.EmptyCell)
                {
                    continue;
                }

                // Skip receipt if the transaction was skipped during import
                if (session != null && session.SkippedTransactionIds.Contains(transactionId))
                {
                    continue;
                }

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(transactionId) ||
                    string.IsNullOrWhiteSpace(receiptFileName) ||
                    string.IsNullOrWhiteSpace(receiptsFolderPath) ||
                    receiptFileName == ReadOnlyVariables.EmptyCell)
                {
                    DataGridViewCell noteCell = targetRow.Cells[ReadOnlyVariables.HasReceipt_column];
                    MainMenu_Form.SetReceiptCellToX(noteCell);
                    continue;
                }

                string receiptFilePath = Path.Combine(receiptsFolderPath, receiptFileName);

                // Check if the receipt file exists
                if (!File.Exists(receiptFilePath))
                {
                    CustomMessageBox.ShowWithFormat(
                        "Receipt does not exist",
                        "The receipt '{0}' does not exist in the folder you selected. This receipt will not be added.",
                        CustomMessageBoxIcon.Exclamation,
                        CustomMessageBoxButtons.Ok,
                        receiptFileName);
                    continue;
                }

                // Find the transaction in the correct DataGridView
                if (targetRow == null)
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    Log.WriteWithFormat(1, "{0} {1} not found for receipt {2}", transactionType, transactionId, receiptFileName);
                    continue;
                }

                // Check if transaction already has a receipt
                if (TransactionHasReceipt(targetRow))
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                        "Transaction already has receipt",
                        "{0} {1} already has a receipt. Do you want to replace it?",
                        CustomMessageBoxIcon.Question,
                        CustomMessageBoxButtons.YesNo,
                        transactionType, transactionId);

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
                if (row.Cells[ReadOnlyVariables.ID_column].Value?.ToString() == transactionId)
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
        private static ImportSummary ImportTransactionData(IXLWorksheet worksheet, bool isPurchase, string sourceCurrency, ImportSession session)
        {
            ImportSummary summary = new();

            if (session.IsCancelled == true)
            {
                summary.WasCancelled = true;
                return summary;
            }

            // Get the target DataGridView to use its column structure
            Guna2DataGridView targetGridView = isPurchase
                ? MainMenu_Form.Instance.Purchase_DataGridView
                : MainMenu_Form.Instance.Sale_DataGridView;

            ExcelColumnHelper.ValidationResult validation = ExcelColumnHelper.ValidateTransactionColumns(worksheet, isPurchase);

            if (!validation.IsValid)
            {
                ShowColumnValidationError("Transactions", validation.MissingColumns);
                summary.Errors.Add(new ImportError
                {
                    WorksheetName = isPurchase ? "Purchases" : "Sales",
                    FieldName = "Column Validation",
                    InvalidValue = $"Missing columns: {string.Join(", ", validation.MissingColumns)}"
                });
                return summary;
            }

            // Always skip header row (row 1) since we use it for column lookup
            IEnumerable<IXLRow> rowsToProcess = worksheet.RowsUsed().Skip(1);

            int newRowIndex = -1;
            int currentRowNumber = 2;  // Start at 2 since we're skipping header row (row 1)
            int successfulTransactions = 0;

            // Get existing transaction numbers
            HashSet<string> existingTransactionNumbers = new(targetGridView.Rows.Count);
            string idColumnHeader = targetGridView.Columns[0].HeaderText;

            foreach (DataGridViewRow row in targetGridView.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    existingTransactionNumbers.Add(row.Cells[0].Value.ToString());
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

                currentRowNumber++;
                string transactionNumber = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ID);

                // Check if this is an item row (part of a multi-item transaction)
                if (string.IsNullOrEmpty(transactionNumber) || transactionNumber == ReadOnlyVariables.EmptyCell)
                {
                    // This is an item row, not a main transaction row
                    // Item rows are processed by ImportItemsInTransaction, so we just skip them here
                    summary.ItemRowsProcessed++;
                    continue;
                }

                // Check if this row's transaction number already exists
                bool shouldContinue = CheckIfItemExists(
                    transactionNumber,
                    existingTransactionNumbers,
                    addedDuringImport,
                    itemType,
                    session);

                if (!shouldContinue)
                {
                    summary.SkippedRows++;
                    session.SkippedTransactionIds.Add(transactionNumber);
                    continue;
                }

                // Create a new row
                DataGridViewRow newRow = (DataGridViewRow)targetGridView.RowTemplate.Clone();
                newRow.CreateCells(targetGridView);

                // Add the row to the DataGridView right away to give it a OwningColumn.Name so it can be accessed by the column name
                targetGridView.InvokeIfRequired(() =>
                {
                    newRowIndex = targetGridView.Rows.Add(newRow);
                });

                ImportTransactionResult importResult = ImportTransaction(targetGridView, newRowIndex, row, newRow, currentRowNumber, worksheetName, sourceCurrency, session);

                switch (importResult)
                {
                    case ImportTransactionResult.Cancel:
                        RemoveRowFromDataGridView(targetGridView, newRowIndex);
                        session.IsCancelled = true;
                        summary.WasCancelled = true;
                        return summary;

                    case ImportTransactionResult.Skip:
                        RemoveRowFromDataGridView(targetGridView, newRowIndex);
                        summary.SkippedRows++;
                        session.SkippedTransactionIds.Add(transactionNumber);
                        continue;

                    case ImportTransactionResult.Failed:
                        RemoveRowFromDataGridView(targetGridView, newRowIndex);
                        summary.Errors.Add(new ImportError
                        {
                            TransactionId = transactionNumber,
                            RowNumber = currentRowNumber,
                            WorksheetName = worksheetName,
                            FieldName = "Transaction Import",
                            InvalidValue = "Technical failure during import"
                        });
                        return summary;

                    case ImportTransactionResult.Success:
                        break;
                }

                ImportTransactionResult itemsImportResult = ImportItemsInTransaction(row, newRow, currentRowNumber, worksheetName, sourceCurrency, session);

                switch (itemsImportResult)
                {
                    case ImportTransactionResult.Cancel:
                        session.IsCancelled = true;
                        summary.WasCancelled = true;
                        return summary;
                    case ImportTransactionResult.Skip:
                        summary.SkippedRows++;
                        session.SkippedTransactionIds.Add(transactionNumber);
                        continue;
                    case ImportTransactionResult.Failed:
                        summary.Errors.Add(new ImportError
                        {
                            TransactionId = transactionNumber,
                            RowNumber = currentRowNumber,
                            WorksheetName = worksheetName,
                            FieldName = "Items Import",
                            InvalidValue = "Technical failure during items import"
                        });
                        return summary;
                    case ImportTransactionResult.Success:
                        break;
                }

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

            return summary;
        }
        private static void RemoveRowFromDataGridView(DataGridView dataGridView, int rowIndex)
        {
            dataGridView.InvokeIfRequired(() =>
            {
                if (rowIndex >= 0 && rowIndex < dataGridView.Rows.Count)
                {
                    dataGridView.Rows.RemoveAt(rowIndex);
                }
            });
        }

        /// <summary>
        /// Validates if a product exists in the appropriate category list.
        /// </summary>
        /// <returns>InvalidValueAction indicating whether to continue, skip, or cancel.</returns>
        private static InvalidValueAction ValidateProductExists(
            string productName,
            string categoryName,
            bool isPurchase,
            string transactionId,
            int rowNumber,
            string worksheetName,
            ImportSession session)
        {
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(categoryName))
            {
                return InvalidValueAction.Continue;  // Empty values are handled elsewhere
            }

            // Skip validation for "Multiple items" placeholder
            if (productName.Equals("Multiple items", StringComparison.OrdinalIgnoreCase))
            {
                return InvalidValueAction.Continue;
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
                    $"Category '{categoryName}' does not exist",
                    session);
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
                  $"Product '{productName}' does not exist in category '{categoryName}'",
                  session);
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
            string errorDescription,
            ImportSession session)
        {
            // Check if user has already made a "to all" choice
            if (session.UserChoices.ProductNotFoundChoice.HasValue)
            {
                return session.UserChoices.ProductNotFoundChoice.Value
                    ? InvalidValueAction.Skip
                    : InvalidValueAction.Cancel;
            }

            CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                "Product Not Found - Transaction Import",
                "Product not found during transaction import:\n\n" +
                "Worksheet: {0}\n" +
                "Row: {1}\n" +
                "Transaction ID: {2}\n" +
                "Product: '{3}'\n" +
                "Category: '{4}'\n" +
                "Error: {5}\n\n" +
                "The transaction cannot be imported because the product does not exist in the system. " +
                "Please ensure the product is created before importing transactions that reference it.\n\n" +
                "How would you like to proceed?",
                CustomMessageBoxIcon.Error,
                CustomMessageBoxButtons.SkipCancel,
                worksheetName, rowNumber, transactionId, productName, categoryName, errorDescription);

            return result switch
            {
                CustomMessageBoxResult.Skip => InvalidValueAction.Skip,
                CustomMessageBoxResult.Cancel => InvalidValueAction.Cancel,
                _ => InvalidValueAction.Cancel
            };
        }

        private static void ShowColumnValidationError(string importType, List<string> missingColumns)
        {
            string missingColumnsText = string.Join(", ", missingColumns.Select(c => $"'{c}'"));

            CustomMessageBox.ShowWithFormat(
                "Missing Required Columns",
                "Cannot import {0} data because the following required columns are missing:\n\n" +
                "Missing: {1}\n\n" +
                "Please ensure your spreadsheet has the correct column headers.",
                CustomMessageBoxIcon.Error,
                CustomMessageBoxButtons.Ok,
                importType, missingColumnsText);
        }

        // Export spreadsheet methods
        public enum ImportReturnResult
        {
            Success,
            Skip,
            Cancel
        }
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
                        case CustomMessageBoxResult.Cancel:  // Cancel import
                            return new ConversionResult { Value = 0, IsValid = false, Action = InvalidValueAction.Cancel };
                        case CustomMessageBoxResult.Retry:  // Retry with user input
                            break;  // Continue the while loop - the user may update the spreadsheet with a new value
                        default:  // Treat as cancel
                            return new ConversionResult { Value = 0, IsValid = false, Action = InvalidValueAction.Cancel };
                    }
                }
            }
        }
        private static CustomMessageBoxResult ShowDetailedImportError(ImportError error)
        {
            return CustomMessageBox.ShowWithFormat(
                "Import Error - Invalid Monetary Value",
                "Invalid value found during import:\n\n" +
                "Worksheet: {0}\n" +
                "Row: {1}\n" +
                "Transaction ID: {2}\n" +
                "Field: {3}\n" +
                "Invalid Value: '{4}'\n\n" +
                "This value cannot be converted to a valid monetary amount. How would you like to proceed?",
                CustomMessageBoxIcon.Error,
                CustomMessageBoxButtons.SkipRetryCancel,
                error.WorksheetName, error.RowNumber, error.TransactionId, error.FieldName, error.InvalidValue);
        }

        /// <summary>
        /// Checks if an item already exists and asks the user if they want to add it anyway.
        /// </summary>
        /// <returns>True if the item should be added, false if it should be skipped.</returns>
        private static bool CheckIfItemExists(
            string itemNumber,
            HashSet<string> existingItems,
            HashSet<string> addedDuringImport,
            string itemTypeName,
            ImportSession session)
        {
            bool alreadyExistsInSystem = existingItems.Contains(itemNumber);
            bool alreadyAddedDuringImport = addedDuringImport.Contains(itemNumber);

            if (alreadyExistsInSystem || alreadyAddedDuringImport)
            {
                // Check if user has already made a "to all" choice
                if (session.UserChoices.DuplicateItemChoice.HasValue)
                {
                    return session.UserChoices.DuplicateItemChoice.Value;
                }

                CustomMessageBoxResult result;

                if (alreadyExistsInSystem)
                {
                    result = CustomMessageBox.ShowWithFormat(
                        "{0} # already exists",
                        "The {1} #{2} already exists. Would you like to add this {2} anyways?",
                        CustomMessageBoxIcon.Question,
                        CustomMessageBoxButtons.YesNoAll,
                        itemTypeName, itemTypeName.ToLowerInvariant(), itemNumber, itemTypeName.ToLowerInvariant());
                }
                else
                {
                    result = CustomMessageBox.ShowWithFormat(
                        "{0} # appears multiple times in this spreadsheet",
                        "The {1} #{2} appears multiple times in this spreadsheet. Would you like to add this duplicate anyways?",
                        CustomMessageBoxIcon.Question,
                        CustomMessageBoxButtons.YesNoAll,
                        itemTypeName, itemTypeName.ToLowerInvariant(), itemNumber);
                }

                switch (result)
                {
                    case CustomMessageBoxResult.Yes:
                        return true;
                    case CustomMessageBoxResult.No:
                        return false;
                    case CustomMessageBoxResult.YesAll:
                        session.UserChoices.DuplicateItemChoice = true;
                        return true;
                    case CustomMessageBoxResult.NoAll:
                        session.UserChoices.DuplicateItemChoice = false;
                        return false;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This needs to be done after the row has been added to a DataGridView.
        /// </summary>
        private static void FormatNoteCell(DataGridViewRow row)
        {
            DataGridViewCell lastCell = row.Cells[ReadOnlyVariables.Note_column];

            // Only add underline if the cell has a note
            if (lastCell.Value?.ToString() == ReadOnlyVariables.Show_text && lastCell.Tag != null)
            {
                DataGridViewManager.AddUnderlineToCell(lastCell);
            }
        }

        /// <summary>
        /// Imports data into a DataGridViewRow with source currency support and immediate cancellation support.
        /// </summary>
        private static ImportTransactionResult ImportTransaction(
            DataGridView targetGridView,
            int rowIndex,
            IXLRow row,
            DataGridViewRow transaction,
            int rowNumber,
            string worksheetName,
            string sourceCurrency,
            ImportSession session)
        {
            if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

            TagData tagData = new();

            // Get exchange rates
            string dateCellValue = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Date);
            string date = Tools.FormatDate(Tools.ParseDateOrToday(dateCellValue));

            string defaultCurrency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);

            decimal exchangeRateToDefault = Currency.GetExchangeRate(sourceCurrency, defaultCurrency, date, false);
            if (exchangeRateToDefault == -1) { return ImportTransactionResult.Failed; }

            decimal exchangeRateToUSD = Currency.GetExchangeRate(sourceCurrency, "USD", date, false);
            if (exchangeRateToUSD == -1) { return ImportTransactionResult.Failed; }

            string transactionId = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ID);
            string productName = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Product);
            string categoryName = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Category);
            bool isPurchase = worksheetName.Equals("Purchases", StringComparison.OrdinalIgnoreCase);

            // Validate product exists
            InvalidValueAction productValidationResult = ValidateProductExists(
                productName,
                categoryName,
                isPurchase,
                transactionId,
                rowNumber,
                worksheetName,
                session);

            switch (productValidationResult)
            {
                case InvalidValueAction.Cancel:
                    return ImportTransactionResult.Cancel;
                case InvalidValueAction.Skip:
                    return ImportTransactionResult.Skip;
                case InvalidValueAction.Continue:
                    break;
            }

            // Get country and company from product data (not from Excel sheet)
            List<Category> categoryList = isPurchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            string companyFromProduct = MainMenu_Form.GetCompanyProductIsFrom(categoryList, productName);
            string countryFromProduct = MainMenu_Form.GetCountryProductIsFrom(categoryList, productName);

            int noteCellIndex = targetGridView.Rows[rowIndex].Cells[ReadOnlyVariables.Note_column].ColumnIndex;

            // Add values to each cell in the transaction row
            for (int i = 0; i < noteCellIndex; i++)
            {
                if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

                // Skip the "Has Receipt" column
                if (targetGridView.Columns[i].Name == ReadOnlyVariables.HasReceipt_column)
                {
                    continue;
                }

                string columnHeaderText = targetGridView.Columns[i].HeaderText;
                MainMenu_Form.Column? columnType = GetColumnTypeFromHeader(columnHeaderText, isPurchase);

                if (columnType == null)
                {
                    // Log the error and cancel import
                    Log.WriteWithFormat(1, "Column mapping error in transaction import");

                    CustomMessageBox.ShowWithFormat(
                        "Column Mapping Error",
                        "Failed to map column '{0}' in {1}. The spreadsheet may have changed after it was selected. The import operation will be cancelled.",
                        CustomMessageBoxIcon.Error,
                        CustomMessageBoxButtons.Ok,
                        columnHeaderText, worksheetName);

                    return ImportTransactionResult.Cancel;
                }

                string value;

                // Handle Country and Company columns - get from product data instead of Excel
                if (columnType == MainMenu_Form.Column.Country)
                {
                    value = countryFromProduct;
                }
                else if (columnType == MainMenu_Form.Column.Company)
                {
                    value = companyFromProduct;
                }
                else
                {
                    // Get value from Excel for all other columns
                    value = ExcelColumnHelper.GetCellValue(row, columnType);
                }

                // Handle monetary fields using column type detection
                if (IsMonetaryColumn(columnHeaderText, isPurchase))
                {
                    ImportError errorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = columnHeaderText,
                        RowNumber = rowNumber,
                        WorksheetName = worksheetName
                    };

                    ConversionResult conversionResult = ConvertStringToDecimalWithOptions(value, errorContext);

                    switch (conversionResult.Action)
                    {
                        case InvalidValueAction.Cancel:
                            return ImportTransactionResult.Cancel;
                        case InvalidValueAction.Skip:
                            return ImportTransactionResult.Skip;
                        case InvalidValueAction.Continue:
                            break;
                    }

                    decimal sourceValue = conversionResult.Value;
                    bool useEmpty = false;

                    // Store USD values in TagData based on column type
                    switch (columnType)
                    {
                        case MainMenu_Form.Column.PricePerUnit:
                            if (value == ReadOnlyVariables.EmptyCell)
                            {
                                useEmpty = true;
                            }
                            else
                            {
                                tagData.PricePerUnitUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            }
                            break;
                        case MainMenu_Form.Column.Shipping:
                            tagData.ShippingUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case MainMenu_Form.Column.Tax:
                            tagData.TaxUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case MainMenu_Form.Column.Fee:
                            tagData.FeeUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case MainMenu_Form.Column.Discount:
                            tagData.DiscountUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case MainMenu_Form.Column.ChargedDifference:
                            tagData.ChargedDifferenceUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                        case MainMenu_Form.Column.Total:
                            tagData.ChargedOrCreditedUSD = Math.Round(sourceValue * exchangeRateToUSD, 2, MidpointRounding.AwayFromZero);
                            break;
                    }

                    // Store the display value in default currency
                    transaction.Cells[i].Value = useEmpty
                        ? ReadOnlyVariables.EmptyCell
                        : Math.Round(sourceValue * exchangeRateToDefault, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    // Handle non-monetary fields
                    transaction.Cells[i].Value = string.IsNullOrEmpty(value) ? ReadOnlyVariables.EmptyCell : value;
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

            // Process return data if return columns exist
            ImportReturnResult returnResult = ProcessReturnDataFromExcel(row, tagData, transactionId, rowNumber, worksheetName);
            switch (returnResult)
            {
                case ImportReturnResult.Cancel:
                    return ImportTransactionResult.Cancel;
                case ImportReturnResult.Skip:
                    return ImportTransactionResult.Skip;
                case ImportReturnResult.Success:
                    break;
            }

            // Handle notes
            DataGridViewCell noteCell = transaction.Cells[noteCellIndex];
            string excelNoteCellValue = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.Note);

            if (string.IsNullOrWhiteSpace(excelNoteCellValue) || excelNoteCellValue == ReadOnlyVariables.EmptyCell)
            {
                noteCell.Value = ReadOnlyVariables.EmptyCell;
            }
            else
            {
                noteCell.Value = ReadOnlyVariables.Show_text;
                noteCell.Tag = excelNoteCellValue;
            }

            transaction.Tag = tagData;
            return ImportTransactionResult.Success;
        }

        private static ImportReturnResult ProcessReturnDataFromExcel(IXLRow row, TagData tagData, string transactionId, int rowNumber, string worksheetName)
        {
            try
            {
                // Check if return columns exist in the worksheet
                bool hasReturnColumns = HasReturnColumns(row.Worksheet);
                if (!hasReturnColumns)
                {
                    // No return columns found, skip return processing
                    return ImportReturnResult.Success;
                }

                // Get return status
                string isReturnedValue = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.IsReturned);
                if (string.IsNullOrWhiteSpace(isReturnedValue) || isReturnedValue == ReadOnlyVariables.EmptyCell)
                {
                    // No return status specified
                    return ImportReturnResult.Success;
                }

                // Parse return status
                bool isReturned = false;
                bool isPartiallyReturned = false;

                switch (isReturnedValue.ToLowerInvariant().Trim())
                {
                    case "yes":
                    case "true":
                    case "1":
                    case "returned":
                        isReturned = true;
                        break;
                    case "partial":
                    case "partially":
                    case "partial return":
                        isPartiallyReturned = true;
                        break;
                    case "no":
                    case "false":
                    case "0":
                    case "":
                        // Not returned, leave defaults
                        break;
                    default:
                        // Invalid return status, show error
                        CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                            "Invalid Return Status",
                            "Invalid return status '{0}' found in row {1} of {2}. Expected values are: 'Yes', 'No', or 'Partial'.\n\nHow would you like to proceed?",
                            CustomMessageBoxIcon.Exclamation,
                            CustomMessageBoxButtons.SkipCancel,
                            isReturnedValue, rowNumber, worksheetName);

                        return result == CustomMessageBoxResult.Skip ? ImportReturnResult.Skip : ImportReturnResult.Cancel;
                }

                // If returned or partially returned, process additional return data
                if (isReturned || isPartiallyReturned)
                {
                    tagData.IsReturned = isReturned;
                    tagData.IsPartiallyReturned = isPartiallyReturned;

                    // Parse return date
                    string returnDateValue = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ReturnDate);
                    if (!string.IsNullOrWhiteSpace(returnDateValue) && returnDateValue != ReadOnlyVariables.EmptyCell)
                    {
                        if (DateTime.TryParse(returnDateValue, out DateTime returnDate))
                        {
                            tagData.ReturnDate = returnDate;
                        }
                        else
                        {
                            // Invalid date format, use current date and log warning
                            tagData.ReturnDate = DateTime.Now;
                            Log.WriteWithFormat(1, "Invalid return date '{0}' in transaction {1}, using current date", returnDateValue, transactionId);
                        }
                    }
                    else
                    {
                        // No return date specified, use current date
                        tagData.ReturnDate = DateTime.Now;
                    }

                    // Get return reason
                    string returnReason = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ReturnReason);
                    if (!string.IsNullOrWhiteSpace(returnReason) && returnReason != ReadOnlyVariables.EmptyCell)
                    {
                        tagData.ReturnReason = returnReason;
                    }
                    else
                    {
                        tagData.ReturnReason = "Imported return (no reason specified)";
                    }

                    // Get returned by
                    string returnedBy = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ReturnedBy);
                    if (!string.IsNullOrWhiteSpace(returnedBy) && returnedBy != ReadOnlyVariables.EmptyCell)
                    {
                        tagData.ReturnedBy = returnedBy;
                    }
                    else
                    {
                        tagData.ReturnedBy = "Import process";
                    }

                    // For partial returns, parse returned items (this will be handled later in multi-item import)
                    if (isPartiallyReturned)
                    {
                        string returnedItemsValue = ExcelColumnHelper.GetCellValue(row, MainMenu_Form.Column.ReturnedItems);
                        if (!string.IsNullOrWhiteSpace(returnedItemsValue) && returnedItemsValue != ReadOnlyVariables.EmptyCell)
                        {
                            // Store the returned items text to be processed later when we have the full item list
                            tagData.ReturnedItemsText = returnedItemsValue;
                        }
                    }
                }

                return ImportReturnResult.Success;
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error processing return data for transaction {0}: {1}", transactionId, ex.Message);

                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat(
                    "Return Data Import Error",
                    "An error occurred while processing return data for transaction {0} in row {1}:\n\n{2}\n\nHow would you like to proceed?",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.SkipCancel,
                    transactionId, rowNumber, ex.Message);

                return result == CustomMessageBoxResult.Skip ? ImportReturnResult.Skip : ImportReturnResult.Cancel;
            }
        }
        private static bool HasReturnColumns(IXLWorksheet worksheet)
        {
            Dictionary<Enum, string> columnHeaders = new()
            {
                { MainMenu_Form.Column.IsReturned, "Is Returned" },
                { MainMenu_Form.Column.ReturnDate, "Return Date" },
                { MainMenu_Form.Column.ReturnReason, "Return Reason" },
                { MainMenu_Form.Column.ReturnedBy, "Returned By" },
                { MainMenu_Form.Column.ReturnedItems, "Returned Items" }
            };

            // Check if at least one return column exists
            return columnHeaders.Any(kvp => ExcelColumnHelper.HasColumn(worksheet, kvp.Key, columnHeaders));
        }
        private static void ProcessReturnedItemsForMultiItemTransaction(List<string> items, TagData tagData)
        {
            if (string.IsNullOrWhiteSpace(tagData.ReturnedItemsText))
            {
                return;
            }

            tagData.ReturnedItems = [];

            try
            {
                // Parse returned items text - could be item names or indices
                string[] returnedItemsArray = tagData.ReturnedItemsText.Split([';', ','], StringSplitOptions.RemoveEmptyEntries);

                foreach (string returnedItemText in returnedItemsArray)
                {
                    string trimmedItem = returnedItemText.Trim();

                    // Try to parse as item index first
                    if (int.TryParse(trimmedItem, out int itemIndex) && itemIndex >= 0 && itemIndex < items.Count)
                    {
                        tagData.ReturnedItems.Add(itemIndex);
                        continue;
                    }

                    // Try to find by item name
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].StartsWith(ReadOnlyVariables.Receipt_text))
                        {
                            continue;  // Skip receipt entries
                        }

                        string[] itemDetails = items[i].Split(',');
                        if (itemDetails.Length > 0 &&
                            itemDetails[0].Equals(trimmedItem, StringComparison.OrdinalIgnoreCase))
                        {
                            tagData.ReturnedItems.Add(i);
                            break;
                        }
                    }
                }

                // Validate partial return consistency
                if (tagData.IsPartiallyReturned && tagData.ReturnedItems.Count == 0)
                {
                    // No valid returned items found for partial return, treat as not returned
                    tagData.IsPartiallyReturned = false;
                    tagData.ReturnDate = null;
                    tagData.ReturnReason = null;
                    tagData.ReturnedBy = null;

                    Log.WriteWithFormat(1, "No valid returned items found for partial return, treating transaction as not returned");
                }
                else if (tagData.IsPartiallyReturned && tagData.ReturnedItems.Count >= items.Count - (items[^1].StartsWith(ReadOnlyVariables.Receipt_text) ? 1 : 0))
                {
                    // All items are returned, change to full return
                    tagData.IsReturned = true;
                    tagData.IsPartiallyReturned = false;
                    tagData.ReturnedItems = null; // Clear since it's a full return
                }
            }
            catch (Exception ex)
            {
                Log.WriteWithFormat(1, "Error parsing returned items '{0}': {1}", tagData.ReturnedItemsText, ex.Message);

                // Clear partial return data due to parsing error
                tagData.IsPartiallyReturned = false;
                tagData.ReturnDate = null;
                tagData.ReturnReason = null;
                tagData.ReturnedBy = null;
                tagData.ReturnedItems = null;
            }
            finally
            {
                // Clean up the temporary text field
                tagData.ReturnedItemsText = null;
            }
        }

        /// <summary>
        /// Gets the column type from a header text.
        /// </summary>
        private static MainMenu_Form.Column? GetColumnTypeFromHeader(string headerText, bool isPurchase)
        {
            Dictionary<MainMenu_Form.Column, string> columnHeaders = isPurchase
                ? MainMenu_Form.Instance.PurchaseColumnHeaders
                : MainMenu_Form.Instance.SalesColumnHeaders;

            // First try exact match (case-insensitive) for performance
            foreach (KeyValuePair<MainMenu_Form.Column, string> kvp in columnHeaders)
            {
                if (string.Equals(kvp.Value, headerText, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            // Try flexible matching for each column type
            foreach (KeyValuePair<MainMenu_Form.Column, string> kvp in columnHeaders)
            {
                if (ExcelColumnHelper.IsFlexibleMatch(headerText, kvp.Key))
                {
                    return kvp.Key;
                }
            }

            // No match found
            return null;
        }

        /// <summary>
        /// Determines if a column contains monetary values based on header text and context.
        /// </summary>
        private static bool IsMonetaryColumn(string columnHeaderText, bool isPurchase)
        {
            // Get the appropriate column headers dictionary
            Dictionary<MainMenu_Form.Column, string> columnHeaders = isPurchase
                ? MainMenu_Form.Instance.PurchaseColumnHeaders
                : MainMenu_Form.Instance.SalesColumnHeaders;

            // Define which columns are monetary
            MainMenu_Form.Column[] monetaryColumnTypes =
            [
                MainMenu_Form.Column.PricePerUnit,
                MainMenu_Form.Column.Shipping,
                MainMenu_Form.Column.Tax,
                MainMenu_Form.Column.Fee,
                MainMenu_Form.Column.Discount,
                MainMenu_Form.Column.ChargedDifference,
                MainMenu_Form.Column.Total
            ];

            // Check if the given header text matches any of the monetary columns
            foreach (MainMenu_Form.Column columnType in monetaryColumnTypes)
            {
                if (columnHeaders.TryGetValue(columnType, out string standardHeader) &&
                    string.Equals(standardHeader, columnHeaderText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        private static ImportTransactionResult ImportItemsInTransaction(IXLRow row, DataGridViewRow transaction, int baseRowNumber, string worksheetName, string sourceCurrency, ImportSession session)
        {
            if (session.IsCancelled == true) { return ImportTransactionResult.Cancel; }

            TagData tagData = (TagData)transaction.Tag;
            List<string> items = [];
            int currentRowOffset = 0;

            // Get exchange rates
            string dateCellValue = transaction.Cells[6].Value?.ToString();
            string date = Tools.FormatDate(Tools.ParseDateOrToday(dateCellValue));

            decimal exchangeRateToUSD = Currency.GetExchangeRate(sourceCurrency, "USD", date, false);
            decimal exchangeRateToDefault = Currency.GetExchangeRate(sourceCurrency, DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType), date, false);

            if (exchangeRateToUSD == -1 || exchangeRateToDefault == -1)
            {
                return ImportTransactionResult.Failed;
            }

            bool isPurchase = worksheetName.Equals("Purchases", StringComparison.OrdinalIgnoreCase);
            List<Category> categoryList = isPurchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

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

                // Check if the next row has a transaction ID - if it doesn't, it's an item row
                string number = ExcelColumnHelper.GetCellValue(nextRow, MainMenu_Form.Column.ID);

                // Check if the next row has no number, indicating multiple items
                if (string.IsNullOrEmpty(number))
                {
                    string productName = ExcelColumnHelper.GetCellValue(nextRow, MainMenu_Form.Column.Product);
                    string categoryName = ExcelColumnHelper.GetCellValue(nextRow, MainMenu_Form.Column.Category);

                    // Get country and company from product data instead of Excel sheet
                    string currentCountry = "";
                    string currentCompany = "";

                    // Try to find the product in the category list to get country and company
                    Category? category = MainMenu_Form.GetCategoryCategoryNameIsFrom(categoryList, categoryName);
                    if (category != null)
                    {
                        Product? product = category.ProductList.FirstOrDefault(p =>
                            p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

                        if (product != null)
                        {
                            currentCountry = product.CountryOfOrigin ?? "";
                            currentCompany = product.CompanyOfOrigin ?? "";
                        }
                    }

                    // Validate product exists
                    string transactionId = transaction.Cells[0].Value?.ToString() ?? "Unknown";
                    InvalidValueAction productValidationResult = ValidateProductExists(
                        productName,
                        categoryName,
                        isPurchase,
                        transactionId,
                        baseRowNumber + currentRowOffset,
                        worksheetName,
                        session);

                    switch (productValidationResult)
                    {
                        case InvalidValueAction.Cancel:
                            return ImportTransactionResult.Cancel;
                        case InvalidValueAction.Skip:
                            return ImportTransactionResult.Skip;
                        case InvalidValueAction.Continue:
                            break;
                    }

                    // Get quantity and price using column types
                    string quantityValue = ExcelColumnHelper.GetCellValue(nextRow, MainMenu_Form.Column.TotalItems);
                    string priceValue = ExcelColumnHelper.GetCellValue(nextRow, MainMenu_Form.Column.PricePerUnit);

                    // Use error handling for quantity and price
                    ImportError quantityErrorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = "Item Quantity",
                        RowNumber = baseRowNumber + currentRowOffset,
                        WorksheetName = worksheetName
                    };

                    ConversionResult quantityResult = ConvertStringToDecimalWithOptions(quantityValue, quantityErrorContext);

                    if (quantityResult.Action == InvalidValueAction.Cancel)
                    {
                        return ImportTransactionResult.Cancel;
                    }
                    if (quantityResult.Action == InvalidValueAction.Skip)
                    {
                        return ImportTransactionResult.Skip;
                    }

                    ImportError priceErrorContext = new()
                    {
                        TransactionId = transactionId,
                        FieldName = "Item Price Per Unit",
                        RowNumber = baseRowNumber + currentRowOffset,
                        WorksheetName = worksheetName
                    };

                    ConversionResult priceResult = ConvertStringToDecimalWithOptions(priceValue, priceErrorContext);

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
                        defaultPricePerUnit.ToString("F2"),
                        usdPricePerUnit.ToString("F2")
                    );

                    items.Add(item);
                    row = nextRow;  // Move to the next row
                }
                else
                {
                    break;
                }
            }

            // After all items are processed, handle returned items for partial returns
            if (tagData.IsPartiallyReturned && !string.IsNullOrWhiteSpace(tagData.ReturnedItemsText))
            {
                ProcessReturnedItemsForMultiItemTransaction(items, tagData);
            }

            // Save updated tag data
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
                if (dataGridView.Columns[i].Name == ReadOnlyVariables.HasReceipt_column)
                {
                    continue;
                }

                // Skip the country and company colums because they already exist in the product sheets
                if (dataGridView.Columns[i].Name == ReadOnlyVariables.Country_column
                    || dataGridView.Columns[i].Name == ReadOnlyVariables.Company_column)
                {
                    continue;
                }

                IXLCell cell = worksheet.Cell(1, excelColumnIndex);
                cell.Value = dataGridView.Columns[i].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                excelColumnIndex++;
            }

            // Add return-related headers
            int returnColumnsStartIndex = excelColumnIndex;
            string[] returnHeaders = ["Is Returned", "Return Date", "Return Reason", "Returned By", "Returned Items"];

            foreach (string header in returnHeaders)
            {
                IXLCell cell = worksheet.Cell(1, excelColumnIndex);
                cell.Value = header;
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
            int currentRow = 2;

            // Add transactions
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                string receiptFileName = ReadOnlyVariables.EmptyCell;
                int rowForReceipt = currentRow;

                switch (row.Tag)
                {
                    case (List<string> itemList, TagData tagData) when itemList.Count > 0:
                        // Check if there's a receipt in the item list
                        byte receiptOffset = 0;
                        string receipt = itemList[^1];
                        if (receipt.StartsWith(ReadOnlyVariables.Receipt_text))
                        {
                            receiptOffset = 1;
                            receiptFileName = Path.GetFileName(receipt);
                        }

                        AddRowToWorksheet(worksheet, row, currentRow, tagData, targetCurrency, currencyFormatPattern, returnColumnsStartIndex);

                        // Add items in transaction if they exist in itemList
                        for (int i = 0; i < itemList.Count - receiptOffset; i++)
                        {
                            currentRow++;
                            string[] values = itemList[i].Split(',');

                            AddItemRowToWorksheet(worksheet, values, currentRow, targetCurrency, currencyFormatPattern);
                        }
                        break;

                    case (string tagString, TagData tagData):
                        AddRowToWorksheet(worksheet, row, currentRow, tagData, targetCurrency, currencyFormatPattern, returnColumnsStartIndex);

                        if (!string.IsNullOrEmpty(tagString))
                        {
                            receiptFileName = Path.GetFileName(tagString);
                        }
                        break;

                    case TagData tagData:
                        // Transaction with no items and no receipt
                        AddRowToWorksheet(worksheet, row, currentRow, tagData, targetCurrency, currencyFormatPattern, returnColumnsStartIndex);
                        break;
                }

                // Add receipt to the main transaction row (not item rows)
                worksheet.Cell(rowForReceipt, receiptCellIndex).Value = receiptFileName;

                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddRowToWorksheet(IXLWorksheet worksheet, DataGridViewRow row, int currentRow, TagData tagData, string targetCurrency, string currencyFormatPattern, int returnColumnsStartIndex)
        {
            int excelColumnIndex = 1;

            // Get exchange rate from USD to target currency
            string transactionDate = row.Cells[6].Value?.ToString() ?? Tools.FormatDate(DateTime.Today);
            decimal exchangeRate = GetExchangeRateForExport(transactionDate, targetCurrency);

            // Skip the Notes column - it will be handled separately
            int notesColumnIndex = row.Cells[ReadOnlyVariables.Note_column].ColumnIndex;

            // Skip country and company columns becausethey are already in the product data
            int countryColumnIndex = row.Cells[ReadOnlyVariables.Country_column].ColumnIndex;
            int companyColumnIndex = row.Cells[ReadOnlyVariables.Company_column].ColumnIndex;

            for (int i = 0; i < row.Cells.Count; i++)
            {
                if (i == notesColumnIndex)
                {
                    break;
                }
                if (i == countryColumnIndex || i == companyColumnIndex)
                {
                    continue;
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

                        // Check if this is the price per unit column (index 8) for multiple items
                        if (i == 8 && row.Cells[i].Value?.ToString() == ReadOnlyVariables.EmptyCell)
                        {
                            excelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
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
            DataGridViewCell notesCell = row.Cells[ReadOnlyVariables.Note_column];
            string? notesCellValue = notesCell.Value?.ToString();
            IXLCell notesExcelCell = worksheet.Cell(currentRow, excelColumnIndex);

            notesExcelCell.Value = notesCellValue == ReadOnlyVariables.EmptyCell
                ? ReadOnlyVariables.EmptyCell
                : (notesCellValue == ReadOnlyVariables.Show_text && notesCell.Tag != null)
                    ? notesCell.Tag.ToString()
                    : notesCellValue;

            // Add return data columns
            AddReturnDataToWorksheet(worksheet, currentRow, returnColumnsStartIndex, tagData, row);
        }
        private static void AddReturnDataToWorksheet(IXLWorksheet worksheet, int currentRow, int startColumnIndex, TagData tagData, DataGridViewRow row)
        {
            int columnIndex = startColumnIndex;

            if (tagData != null)
            {
                // Is Returned column
                IXLCell isReturnedCell = worksheet.Cell(currentRow, columnIndex++);
                if (tagData.IsReturned)
                {
                    isReturnedCell.Value = "Yes";
                    isReturnedCell.Style.Font.FontColor = XLColor.Red;
                    isReturnedCell.Style.Font.Bold = true;
                }
                else if (tagData.IsPartiallyReturned)
                {
                    isReturnedCell.Value = "Partial";
                    isReturnedCell.Style.Font.FontColor = XLColor.Orange;
                    isReturnedCell.Style.Font.Bold = true;
                }
                else
                {
                    isReturnedCell.Value = "No";
                }

                // Return Date column
                IXLCell returnDateCell = worksheet.Cell(currentRow, columnIndex++);
                if (tagData.ReturnDate.HasValue)
                {
                    returnDateCell.Value = tagData.ReturnDate.Value.ToString("yyyy-MM-dd");
                }
                else
                {
                    returnDateCell.Value = ReadOnlyVariables.EmptyCell;
                }

                // Return Reason column
                IXLCell returnReasonCell = worksheet.Cell(currentRow, columnIndex++);
                returnReasonCell.Value = string.IsNullOrEmpty(tagData.ReturnReason) ? ReadOnlyVariables.EmptyCell : tagData.ReturnReason;

                // Returned By column
                IXLCell returnedByCell = worksheet.Cell(currentRow, columnIndex++);
                returnedByCell.Value = string.IsNullOrEmpty(tagData.ReturnedBy) ? ReadOnlyVariables.EmptyCell : tagData.ReturnedBy;

                // Returned Items column (for partial returns)
                IXLCell returnedItemsCell = worksheet.Cell(currentRow, columnIndex++);
                if (tagData.IsPartiallyReturned && tagData.ReturnedItems != null && tagData.ReturnedItems.Count > 0)
                {
                    // Get the actual item names that were returned
                    List<string> returnedItemNames = GetReturnedItemNamesFromTransaction(row, tagData.ReturnedItems);
                    returnedItemsCell.Value = string.Join("; ", returnedItemNames);
                }
                else if (tagData.IsReturned)
                {
                    returnedItemsCell.Value = "All items";
                }
                else
                {
                    returnedItemsCell.Value = ReadOnlyVariables.EmptyCell;
                }
            }
            else
            {
                // No TagData, fill with empty values
                for (int i = 0; i < 5; i++)
                {
                    worksheet.Cell(currentRow, columnIndex++).Value = ReadOnlyVariables.EmptyCell;
                }
            }
        }
        private static List<string> GetReturnedItemNamesFromTransaction(DataGridViewRow row, List<int> returnedItemIndices)
        {
            List<string> itemNames = [];

            if (row.Tag is (List<string> items, TagData _))
            {
                foreach (int itemIndex in returnedItemIndices)
                {
                    if (itemIndex < items.Count && !items[itemIndex].StartsWith(ReadOnlyVariables.Receipt_text))
                    {
                        string[] itemDetails = items[itemIndex].Split(',');
                        if (itemDetails.Length > 0)
                        {
                            itemNames.Add(itemDetails[0]);  // Product name
                        }
                    }
                }
            }
            else
            {
                // Single item transaction
                string productName = row.Cells[ReadOnlyVariables.Product_column].Value?.ToString();
                if (!string.IsNullOrEmpty(productName))
                {
                    itemNames.Add(productName);
                }
            }

            return itemNames;
        }
        private static void AddItemRowToWorksheet(IXLWorksheet worksheet, string[] row, int currentRow, string targetCurrency, string currencyFormatPattern)
        {
            // Get transaction date from the main row for exchange rate calculation
            string transactionDate = worksheet.Cell(currentRow - GetItemOffsetForTransaction(worksheet, currentRow), 7).Value.ToString()
                ?? Tools.FormatDate(DateTime.Today);
            decimal exchangeRate = GetExchangeRateForExport(transactionDate, targetCurrency);

            for (int i = 0; i < row.Length - 1; i++)  // Skip the total value with - 1
            {
                // Skip country and company columns because they are already in the product data
                if (i == 2 || i == 3)
                {
                    continue;
                }

                // Shift the data one column to the left after the date column
                int columnIndex = i < 4 ? i : i - 1;
                IXLCell excelCell = worksheet.Cell(currentRow, columnIndex + 3);

                string cellValue = row[i];

                // Check if this should be a numeric value (quantity, price, etc.)
                if (i == 4 || i == 5)  // quantity and price columns
                {
                    if (decimal.TryParse(cellValue, out decimal numericValue))
                    {
                        if (i == 5)  // price column - convert from USD to target currency
                        {
                            decimal convertedPrice = Math.Round(numericValue * exchangeRate, 2, MidpointRounding.AwayFromZero);
                            excelCell.Value = convertedPrice;
                            excelCell.Style.NumberFormat.Format = currencyFormatPattern;
                        }
                        else  // quantity column
                        {
                            excelCell.Value = numericValue;
                            excelCell.Style.NumberFormat.Format = _numberFormatPattern;
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
                    string formattedDate = Tools.FormatDate(parsedDate);
                    decimal rate = Currency.GetExchangeRate("USD", targetCurrency, formattedDate, false);
                    return rate > 0 ? rate : 1.0m;  // Fallback to 1:1 if rate fetch fails
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the export
                Log.WriteWithFormat(1, "Failed to get exchange rate for {0}: {1}", targetCurrency, ex.Message);
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
    }
}