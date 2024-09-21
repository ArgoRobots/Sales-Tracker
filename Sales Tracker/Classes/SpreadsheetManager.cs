using ClosedXML.Excel;

namespace Sales_Tracker.Classes
{
    internal class SpreadsheetManager
    {
        // Import spreadsheet methods
        public static bool ImportAccountantsData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            foreach (IXLRow row in rowsToProcess)
            {
                string accountantName = row.Cell(1).GetValue<string>();
                if (MainMenu_Form.Instance.accountantList.Any(name => name.Equals(accountantName, StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBox.Show("Argo Sales Tracker",
                        $"The accountant {accountantName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    MainMenu_Form.Instance.accountantList.Add(accountantName);
                    wasSomethingImported = true;
                }
            }
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.accountantList, MainMenu_Form.SelectedOption.Accountants);
            return wasSomethingImported;
        }
        public static bool ImportCompaniesData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            foreach (IXLRow row in rowsToProcess)
            {
                string companyName = row.Cell(1).GetValue<string>();
                if (MainMenu_Form.Instance.companyList.Any(name => name.Equals(companyName, StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBox.Show("Argo Sales Tracker",
                        $"The company {companyName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    MainMenu_Form.Instance.companyList.Add(companyName);
                    wasSomethingImported = true;
                }
            }
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.companyList, MainMenu_Form.SelectedOption.Companies);
            return wasSomethingImported;
        }
        public static bool ImportProductsData(IXLWorksheet worksheet, bool purchase, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            List<Category> list;
            if (purchase)
            {
                list = MainMenu_Form.Instance.categoryPurchaseList;
            }
            else
            {
                list = MainMenu_Form.Instance.categorySaleList;
            }

            // Read product data from the worksheet and add it to the category purchase list
            foreach (IXLRow row in rowsToProcess)
            {
                string productId = row.Cell(1).GetValue<string>();
                string productName = row.Cell(2).GetValue<string>();
                string categoryName = row.Cell(3).GetValue<string>();
                string countryOfOrigin = row.Cell(4).GetValue<string>();
                string companyOfOrigin = row.Cell(5).GetValue<string>();

                List<string> unitedStatesVariants = new()
                {
                    "US", "USA", "U.S.", "U.S.A.", "United States of America", "America", "U.S. of A.", "The States"
                };

                // Check if the country exists or if it's a variant of "United States"
                if (unitedStatesVariants.Any(variant => variant.Equals(countryOfOrigin, StringComparison.OrdinalIgnoreCase)))
                {
                    // Convert any variant to "United States"
                    countryOfOrigin = "United States";
                }
                else
                {
                    bool countryExists = Country.countries.Any(c => c.Name.Equals(countryOfOrigin, StringComparison.OrdinalIgnoreCase));

                    if (!countryExists)
                    {
                        CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                            $"Country '{countryOfOrigin}' does not exist in the system. Please check the tutorial for more information. Do you want to skip this product and continue?",
                            CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                        if (result == CustomMessageBoxResult.Yes)
                        {
                            continue;
                        }
                    }
                }

                // Check if the company exists in companyList
                bool companyExists = MainMenu_Form.Instance.companyList.Contains(companyOfOrigin);
                if (!companyExists)
                {
                    MainMenu_Form.Instance.companyList.Add(companyOfOrigin);
                }

                // Find or create the category
                Category category = list.FirstOrDefault(c => c.Name == categoryName);

                if (category == null)
                {
                    category = new Category { Name = categoryName };
                    list.Add(category);
                }

                // Create the product and add it to the category's ProductList
                Product product = new()
                {
                    ProductID = productId,
                    Name = productName,
                    CountryOfOrigin = countryOfOrigin,
                    CompanyOfOrigin = companyOfOrigin
                };
                if (category.ProductList.Any(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase)))
                {
                    string type = purchase ? "purchase" : "sale";

                    CustomMessageBox.Show("Argo Sales Tracker",
                    $"The product for {type} '{productName}' already exists and will not be imported",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    category.ProductList.Add(product);
                    wasSomethingImported = true;
                }

                MainMenu_Form.Instance.SaveCategoriesToFile(purchase
                    ? MainMenu_Form.SelectedOption.CategoryPurchases
                    : MainMenu_Form.SelectedOption.CategorySales);
            }
            return wasSomethingImported;
        }
        public static (bool, bool) ImportPurchaseData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;
            int newRowIndex = -1;

            foreach (IXLRow row in rowsToProcess)
            {
                string purchaseNumber = row.Cell(1).GetValue<string>();

                if (purchaseNumber != MainMenu_Form.emptyCell && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), purchaseNumber))
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                      $"The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                      CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                DataGridViewRow newRow = (DataGridViewRow)MainMenu_Form.Instance.Purchases_DataGridView.RowTemplate.Clone();
                newRow.CreateCells(MainMenu_Form.Instance.Purchases_DataGridView);

                if (!ImportCells(row, newRow)) { return (false, wasSomethingImported); }

                if (MainMenu_Form.Instance.Purchases_DataGridView.InvokeRequired)
                {
                    MainMenu_Form.Instance.Purchases_DataGridView.Invoke(new Action(() =>
                    {
                        newRowIndex = MainMenu_Form.Instance.Purchases_DataGridView.Rows.Add(newRow);
                    }));
                }
                else
                {
                    newRowIndex = MainMenu_Form.Instance.Purchases_DataGridView.Rows.Add(newRow);
                }

                wasSomethingImported = true;
                MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.Purchases_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            return (true, wasSomethingImported);
        }
        public static (bool, bool) ImportSalesData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;
            int newRowIndex = -1;

            foreach (IXLRow row in rowsToProcess)
            {
                string saleNumber = row.Cell(1).GetValue<string>();

                if (saleNumber != MainMenu_Form.emptyCell && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), saleNumber))
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                      $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?",
                      CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                DataGridViewRow newRow = (DataGridViewRow)MainMenu_Form.Instance.Sales_DataGridView.RowTemplate.Clone();
                newRow.CreateCells(MainMenu_Form.Instance.Sales_DataGridView);

                if (!ImportCells(row, newRow)) { return (false, wasSomethingImported); }

                if (MainMenu_Form.Instance.Sales_DataGridView.InvokeRequired)
                {
                    MainMenu_Form.Instance.Sales_DataGridView.Invoke(new Action(() =>
                    {
                        newRowIndex = MainMenu_Form.Instance.Sales_DataGridView.Rows.Add(newRow);
                    }));
                }
                else
                {
                    newRowIndex = MainMenu_Form.Instance.Sales_DataGridView.Rows.Add(newRow);
                }

                wasSomethingImported = true;
                MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.Sales_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            return (true, wasSomethingImported);
        }

        /// <summary>
        /// Imports data into a DataGridViewRow.
        /// </summary>
        /// <returns>True if the cells are imported successfully. False if the exchange rate was not retrieved.</returns>
        private static bool ImportCells(IXLRow row, DataGridViewRow newRow)
        {
            TagData tagData = new();

            for (int i = 0; i < row.Cells().Count() - 1; i++)
            {
                string value = row.Cell(i + 1).GetValue<string>();

                if (i >= 8 && i <= 14)
                {
                    decimal decimalValue = ConvertStringToDecimal(value);

                    switch (i)
                    {
                        case 8:
                            tagData.PricePerUnitUSD = decimalValue;
                            break;
                        case 9:
                            tagData.ShippingUSD = decimalValue;
                            break;
                        case 10:
                            tagData.TaxUSD = decimalValue;
                            break;
                        case 11:
                            tagData.FeeUSD = decimalValue;
                            break;
                        case 12:
                            tagData.DiscountUSD = decimalValue;
                            break;
                        case 13:
                            tagData.ChargedDifferenceUSD = decimalValue;
                            break;
                        case 14:
                            tagData.TotalUSD = decimalValue;
                            break;
                    }

                    string date = row.Cell(7).GetValue<string>();
                    decimal exchangeRateToDefault = Currency.GetExchangeRate("USD", Properties.Settings.Default.Currency, date, false);
                    if (exchangeRateToDefault == -1) { return false; }

                    newRow.Cells[i].Value = (decimalValue * exchangeRateToDefault).ToString("N2");
                }
                else
                {
                    newRow.Cells[i].Value = value;
                }
            }
            newRow.Tag = tagData;
            return true;
        }

        // Export spreadsheet methods
        public static void ExportSpreadsheet(string filePath)
        {
            filePath = Directories.GetNewFileNameIfItAlreadyExists(filePath);

            using XLWorkbook workbook = new();

            IXLWorksheet purchaseWorksheet = workbook.Worksheets.Add("Purchases");
            AddTransactionToWorksheet(purchaseWorksheet, MainMenu_Form.Instance.Purchases_DataGridView);

            IXLWorksheet salesWorksheet = workbook.Worksheets.Add("Sales");
            AddTransactionToWorksheet(salesWorksheet, MainMenu_Form.Instance.Sales_DataGridView);

            IXLWorksheet purchaseProductsWorksheet = workbook.Worksheets.Add("Purchase products");
            AddProductsToWorksheet(purchaseProductsWorksheet, MainMenu_Form.Instance.categoryPurchaseList);

            IXLWorksheet saleProductsWorksheet = workbook.Worksheets.Add("Sale products");
            AddProductsToWorksheet(saleProductsWorksheet, MainMenu_Form.Instance.categorySaleList);

            IXLWorksheet accountantsWorksheet = workbook.Worksheets.Add("Accountants");
            AddAccountantsToWorksheet(accountantsWorksheet);

            IXLWorksheet companiesWorksheet = workbook.Worksheets.Add("Companies");
            AddCompaniesToWorksheet(companiesWorksheet);

            // Save the file
            workbook.SaveAs(filePath);
        }
        private static void AddTransactionToWorksheet(IXLWorksheet worksheet, DataGridView dataGridView)
        {
            // Add headers and format them
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                IXLCell cell = worksheet.Cell(1, i + 1);
                cell.Value = dataGridView.Columns[i].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            int receiptCellIndex = dataGridView.Columns.Count + 1;

            // Add header for the receipt column
            worksheet.Cell(1, receiptCellIndex).Value = "Receipt";
            worksheet.Cell(1, receiptCellIndex).Style.Font.Bold = true;
            worksheet.Cell(1, receiptCellIndex).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Add message
            int messageCellIndex = receiptCellIndex + 2;
            worksheet.Cell(1, messageCellIndex).Value = "All prices in USD";
            worksheet.Cell(1, messageCellIndex).Style.Font.Bold = true;

            // Extract TagData and receipt information
            string receiptFileName = MainMenu_Form.emptyCell;

            int currentRow = 2;
            int rowForReceipt = 2;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // Handle receipts and adding new rows
                if (row.Tag is (List<string> tagList, TagData tagData1) && tagList.Count > 0)
                {
                    // Is there a receipt
                    byte receiptOffset = 0;
                    string receipt = tagList[^1];
                    if (receipt.StartsWith(MainMenu_Form.receipt_text))
                    {
                        receiptOffset = 1;
                        receiptFileName = Path.GetFileName(receipt);
                    }
                    else
                    {
                        worksheet.Cell(currentRow, receiptCellIndex).Value = MainMenu_Form.emptyCell;
                    }

                    AddRowToWorksheet(worksheet, row, currentRow, tagData1);

                    // Add additional rows if they exist in the tagList
                    for (int j = 0; j < tagList.Count - receiptOffset; j++)
                    {
                        currentRow++;
                        string[] values = tagList[j].Split(',');

                        AddItemRowToWorksheet(worksheet, values, currentRow);
                    }
                }
                else if (row.Tag is (string tagString, TagData tagData2))
                {
                    AddRowToWorksheet(worksheet, row, currentRow, tagData2);
                    receiptFileName = Path.GetFileName(tagString);
                }

                // Add receipt to the last cell
                worksheet.Cell(rowForReceipt, receiptCellIndex).Value = receiptFileName;

                currentRow++;
                rowForReceipt = currentRow;  // This ensures that the receipt is not added to the bottom of a transaction with multiple items
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddRowToWorksheet(IXLWorksheet worksheet, DataGridViewRow row, int currentRow, TagData tagData)
        {
            for (int i = 0; i < row.Cells.Count - 1; i++)  // Do not add the note in the last cell here
            {
                IXLCell excelCell = worksheet.Cell(currentRow, i + 1);

                if (tagData != null && i >= 8 && i <= 14)
                {
                    decimal usdValue = 0;
                    switch (i)
                    {
                        case 8:
                            usdValue = tagData.PricePerUnitUSD;
                            break;
                        case 9:
                            usdValue = tagData.ShippingUSD;
                            break;
                        case 10:
                            usdValue = tagData.TaxUSD;
                            break;
                        case 11:
                            usdValue = tagData.FeeUSD;
                            break;
                        case 12:
                            usdValue = tagData.DiscountUSD;
                            break;
                        case 13:
                            usdValue = tagData.ChargedDifferenceUSD;
                            break;
                        case 14:
                            usdValue = tagData.TotalUSD;
                            break;
                    }
                    excelCell.Value = usdValue.ToString();
                }
                else
                {
                    string? cellValue = row.Cells[i].Value?.ToString();
                    excelCell.Value = cellValue;
                }

                string? cellValue1 = row.Cells[i].Value?.ToString();
                excelCell.Value = cellValue1;
            }

            // Set the note in the last cell
            DataGridViewCell lastCell = row.Cells[^1];
            if (lastCell.Value.ToString() == MainMenu_Form.show_text && lastCell.Tag != null)
            {
                lastCell.Value = lastCell.Tag.ToString();
            }
        }
        private static void AddItemRowToWorksheet(IXLWorksheet worksheet, string[] row, int currentRow)
        {
            int dateColumnIndex = 4;

            for (int i = 0; i < row.Length - 1; i++) // Skip the total value with - 1
            {
                // Shift the data one column to the right after the date column
                int columnIndex = i < dateColumnIndex ? i : i + 1;

                IXLCell excelCell = worksheet.Cell(currentRow, columnIndex + 3);

                string? cellValue = row[i]?.ToString();
                excelCell.Value = cellValue;
            }
        }
        private static void AddAccountantsToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Accountant name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string accountant in MainMenu_Form.Instance.accountantList)
            {
                worksheet.Cell(currentRow, 1).Value = accountant;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddCompaniesToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Company name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string company in MainMenu_Form.Instance.companyList)
            {
                worksheet.Cell(currentRow, 1).Value = company;
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

        // Other methods
        public static decimal ConvertStringToDecimal(string value)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                CustomMessageBox.Show("Argo Sales Tracker", $"Cannot import because a money value is not in the correct format: {value}", CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return -1;
            }
        }
    }
}