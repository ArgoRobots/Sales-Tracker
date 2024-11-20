using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles the import and export of data to and from Excel spreadsheets, including data for 
    /// accountants, companies, products, purchases, and sales in the application.
    /// </summary>
    internal class ExcelSheetManager
    {
        // Import spreadsheet methods
        public static bool ImportAccountantsData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            foreach (IXLRow row in rowsToProcess)
            {
                string accountantName = row.Cell(1).GetValue<string>();
                if (MainMenu_Form.Instance.AccountantList.Any(name => name.Equals(accountantName, StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBox.Show(
                        "Accountant already exists",
                        $"The accountant {accountantName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    MainMenu_Form.Instance.AccountantList.Add(accountantName);
                    wasSomethingImported = true;
                }
            }
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.AccountantList, MainMenu_Form.SelectedOption.Accountants);
            return wasSomethingImported;
        }
        public static bool ImportCompaniesData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            foreach (IXLRow row in rowsToProcess)
            {
                string companyName = row.Cell(1).GetValue<string>();
                if (MainMenu_Form.Instance.CompanyList.Any(name => name.Equals(companyName, StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBox.Show(
                        "Company already exists",
                        $"The company {companyName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    MainMenu_Form.Instance.CompanyList.Add(companyName);
                    wasSomethingImported = true;
                }
            }
            MainMenu_Form.SaveListToFile(MainMenu_Form.Instance.CompanyList, MainMenu_Form.SelectedOption.Companies);
            return wasSomethingImported;
        }
        public static bool ImportProductsData(IXLWorksheet worksheet, bool purchase, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            bool wasSomethingImported = false;

            List<Category> list;
            if (purchase)
            {
                list = MainMenu_Form.Instance.CategoryPurchaseList;
            }
            else
            {
                list = MainMenu_Form.Instance.CategorySaleList;
            }

            // Read product data from the worksheet and add it to the category purchase list
            foreach (IXLRow row in rowsToProcess)
            {
                string productId = row.Cell(1).GetValue<string>();
                string productName = row.Cell(2).GetValue<string>();
                string categoryName = row.Cell(3).GetValue<string>();
                string countryOfOrigin = row.Cell(4).GetValue<string>();
                string companyOfOrigin = row.Cell(5).GetValue<string>();

                List<string> unitedStatesVariants =
                [
                    "US", "USA", "U.S.", "U.S.A.", "United States of America", "America", "U.S. of A.", "The States"
                ];

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
                        CustomMessageBoxResult result = CustomMessageBox.Show(
                            "Country does not exist",
                            $"Country '{countryOfOrigin}' does not exist in the system. Please check the tutorial for more information. Do you want to skip this product and continue?",
                            CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                        if (result == CustomMessageBoxResult.Yes)
                        {
                            continue;
                        }
                    }
                }

                // Check if the company exists in companyList
                bool companyExists = MainMenu_Form.Instance.CompanyList.Contains(companyOfOrigin);
                if (!companyExists)
                {
                    MainMenu_Form.Instance.CompanyList.Add(companyOfOrigin);
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

                    CustomMessageBox.Show(
                        "Product already exists",
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

                if (purchaseNumber == "") { continue; }

                // Check if this row's purchase number already exists
                if (purchaseNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.Column.ID.ToString(), purchaseNumber))
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show(
                        "Purchase # already exists",
                        $"The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                // Create a new row
                DataGridViewRow newRow = (DataGridViewRow)MainMenu_Form.Instance.Purchase_DataGridView.RowTemplate.Clone();
                newRow.CreateCells(MainMenu_Form.Instance.Purchase_DataGridView);

                if (!ImportTransaction(row, newRow)) { return (false, wasSomethingImported); }

                ImportItemsInTransaction(row, newRow);

                // Add the row to the DataGridView
                if (MainMenu_Form.Instance.Purchase_DataGridView.InvokeRequired)
                {
                    MainMenu_Form.Instance.Purchase_DataGridView.Invoke(new Action(() =>
                    {
                        newRowIndex = MainMenu_Form.Instance.Purchase_DataGridView.Rows.Add(newRow);
                    }));
                }
                else
                {
                    newRowIndex = MainMenu_Form.Instance.Purchase_DataGridView.Rows.Add(newRow);
                }

                FormatNoteCell(newRow);

                wasSomethingImported = true;
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.Purchase_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
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

                if (saleNumber == "") { continue; }

                // Check if this row's sales number already exists
                if (saleNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Column.ID.ToString(), saleNumber))
                {
                    CustomMessageBoxResult result = CustomMessageBox.Show(
                        "Sale # already exists",
                        $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                // Create a new row
                DataGridViewRow newRow = (DataGridViewRow)MainMenu_Form.Instance.Sale_DataGridView.RowTemplate.Clone();
                newRow.CreateCells(MainMenu_Form.Instance.Sale_DataGridView);

                if (!ImportTransaction(row, newRow)) { return (false, wasSomethingImported); }

                ImportItemsInTransaction(row, newRow);

                // Add the row to the DataGridView
                if (MainMenu_Form.Instance.Sale_DataGridView.InvokeRequired)
                {
                    MainMenu_Form.Instance.Sale_DataGridView.Invoke(new Action(() =>
                    {
                        newRowIndex = MainMenu_Form.Instance.Sale_DataGridView.Rows.Add(newRow);
                    }));
                }
                else
                {
                    newRowIndex = MainMenu_Form.Instance.Sale_DataGridView.Rows.Add(newRow);
                }

                FormatNoteCell(newRow);

                wasSomethingImported = true;
                DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.Sale_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }
            return (true, wasSomethingImported);
        }
        /// <summary>
        /// This needs to be done after the row has been added to a DataGridView.
        /// </summary>
        private static void FormatNoteCell(DataGridViewRow row)
        {
            DataGridViewCell lastCell = row.Cells[^1];
            DataGridViewManager.AddUnderlineToCell(lastCell);
        }

        /// <summary>
        /// Imports data into a DataGridViewRow.
        /// </summary>
        /// <returns>True if the cells are imported successfully. False if the exchange rate was not retrieved.</returns>
        private static bool ImportTransaction(IXLRow row, DataGridViewRow newRow)
        {
            TagData tagData = new();

            // Get exchange rate
            string date = row.Cell(7).GetValue<string>();
            string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            decimal exchangeRateToDefault = Currency.GetExchangeRate("USD", currency, date, false);
            if (exchangeRateToDefault == -1) { return false; }

            for (int i = 0; i < row.Cells().Count() - 2; i++)  // Do not add the note in the last cell yet
            {
                string value = row.Cell(i + 1).GetValue<string>();

                if (i >= 8 && i <= 14)
                {
                    decimal decimalValue = ConvertStringToDecimal(value);
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
                                tagData.PricePerUnitUSD = decimalValue;
                            }
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
                            tagData.ChargedOrCreditedUSD = decimalValue;
                            break;
                    }

                    newRow.Cells[i].Value = useEmpty
                        ? ReadOnlyVariables.EmptyCell
                        : (decimalValue * exchangeRateToDefault).ToString("N2");
                }
                else
                {
                    newRow.Cells[i].Value = value;
                }
            }

            // Set the note in the last cell
            DataGridViewCell lastCell = newRow.Cells[^1];
            IXLCell lastExcelCell = row.Cell(row.Cells().Count() - 1);
            string lastExcelCellValue = lastExcelCell.Value.ToString();

            if (lastExcelCellValue == ReadOnlyVariables.EmptyCell)
            {
                lastCell.Value = ReadOnlyVariables.EmptyCell;
            }
            else
            {
                lastCell.Value = ReadOnlyVariables.Show_text;
                lastCell.Tag = lastExcelCellValue;
            }

            // Save
            newRow.Tag = tagData;
            return true;
        }
        private static void ImportItemsInTransaction(IXLRow row, DataGridViewRow transaction)
        {
            TagData tagData = (TagData)transaction.Tag;
            List<string> items = [];

            while (true)
            {
                IXLRow nextRow = row.RowBelow();

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
                    decimal quantity = ConvertStringToDecimal(nextRow.Cell(8).Value.ToString());
                    decimal pricePerUnit = ConvertStringToDecimal(nextRow.Cell(9).Value.ToString());

                    string item = string.Join(",",
                        productName,
                        categoryName,
                        currentCountry,
                        currentCompany,
                        quantity.ToString(),
                        pricePerUnit.ToString("N2"),
                        (quantity * pricePerUnit).ToString("N2")
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
        }

        // Export spreadsheet methods
        public static void ExportSpreadsheet(string filePath)
        {
            filePath = Directories.GetNewFileNameIfItAlreadyExists(filePath);

            using XLWorkbook workbook = new();

            IXLWorksheet purchaseWorksheet = workbook.Worksheets.Add("Purchases");
            AddTransactionToWorksheet(purchaseWorksheet, MainMenu_Form.Instance.Purchase_DataGridView);

            IXLWorksheet salesWorksheet = workbook.Worksheets.Add("Sales");
            AddTransactionToWorksheet(salesWorksheet, MainMenu_Form.Instance.Sale_DataGridView);

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
            string receiptFileName = ReadOnlyVariables.EmptyCell;

            int currentRow = 2;
            int rowForReceipt = 2;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // Handle receipts and adding new rows
                if (row.Tag is (List<string> itemList, TagData tagData1) && itemList.Count > 0)
                {
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

                    AddRowToWorksheet(worksheet, row, currentRow, tagData1);

                    // Add additional rows if they exist in itemList
                    for (int j = 0; j < itemList.Count - receiptOffset; j++)
                    {
                        currentRow++;
                        string[] values = itemList[j].Split(',');

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
            for (int i = 0; i < row.Cells.Count - 1; i++)  // Do not add the note in the last cell yet
            {
                IXLCell excelCell = worksheet.Cell(currentRow, i + 1);

                if (tagData != null && i >= 8 && i <= 14)
                {
                    decimal usdValue = 0;
                    bool useEmpty = false;

                    switch (i)
                    {
                        case 8:
                            if (tagData.PricePerUnitUSD == 0)
                            {
                                useEmpty = true;
                            }
                            else
                            {
                                usdValue = tagData.PricePerUnitUSD;
                            }
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
                            usdValue = tagData.ChargedOrCreditedUSD;
                            break;
                    }
                    excelCell.Value = useEmpty ? ReadOnlyVariables.EmptyCell : usdValue.ToString("N5");
                }
                else
                {
                    string? cellValue = row.Cells[i].Value?.ToString();
                    excelCell.Value = cellValue;
                }
            }

            // Set the note in the last cell
            DataGridViewCell lastCell = row.Cells[^1];
            string lastCellValue = lastCell.Value?.ToString();
            IXLCell lastExcelCell = worksheet.Cell(currentRow, row.Cells.Count);

            if (lastCellValue == ReadOnlyVariables.EmptyCell)
            {
                lastExcelCell.Value = ReadOnlyVariables.EmptyCell;
            }
            else if (lastCellValue == ReadOnlyVariables.Show_text && lastCell.Tag != null)
            {
                lastExcelCell.Value = lastCell.Tag.ToString();
            }
        }
        private static void AddItemRowToWorksheet(IXLWorksheet worksheet, string[] row, int currentRow)
        {
            int dateColumnIndex = 4;

            for (int i = 0; i < row.Length - 1; i++)  // Skip the total value with - 1
            {
                // Shift the data one column to the right after the date column
                int columnIndex = i < dateColumnIndex ? i : i + 1;

                IXLCell excelCell = worksheet.Cell(currentRow, columnIndex + 3);

                string? cellValue = row[i]?.ToString();
                excelCell.Value = cellValue;
            }
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
        public static void ExportChartToExcel(Dictionary<string, double> data, string filePath, eChartType chartType, string chartTitle, string column1Text, string column2Text)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateSingleString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            string translatedChartTitle = LanguageManager.TranslateSingleString(chartTitle);

            // Add headers
            worksheet.Cells["A1"].Value = LanguageManager.TranslateSingleString(column1Text);
            worksheet.Cells["B1"].Value = LanguageManager.TranslateSingleString(column2Text);

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
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                row++;
            }

            // Create chart
            ExcelChart chart = worksheet.Drawings.AddChart(translatedChartTitle, chartType);
            chart.SetPosition(0, 0, 3, 0);
            chart.SetSize(800, 400);

            // Configure chart
            ExcelChartSerie series = chart.Series.Add(worksheet.Cells[$"B2:B{row - 1}"], worksheet.Cells[$"A2:A{row - 1}"]);
            chart.Title.Text = translatedChartTitle;
            chart.Legend.Remove();

            worksheet.Columns[1, 2].AutoFit();
            package.SaveAs(new FileInfo(filePath));
        }
        public static void ExportMultiDataSetChartToExcel(Dictionary<string, Dictionary<string, double>> data, string filePath, eChartType chartType, string chartTitle)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateSingleString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Get all series names
            List<string> seriesNames = data.First().Value.Keys.ToList();

            // Add headers
            worksheet.Cells[1, 1].Value = LanguageManager.TranslateSingleString("Date");
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
                    worksheet.Cells[row, i + 2].Style.Numberformat.Format = "#,##0.00";
                }
                row++;
            }

            // Create chart
            ExcelChart chart = worksheet.Drawings.AddChart(chartTitle, chartType);
            chart.SetPosition(0, 0, 3, 0);
            chart.SetSize(800, 400);

            // Add series to chart
            for (int i = 0; i < seriesNames.Count; i++)
            {
                ExcelChartSerie series = chart.Series.Add(
                    worksheet.Cells[2, i + 2, row - 1, i + 2], // Y values
                    worksheet.Cells[2, 1, row - 1, 1]          // X values
                );
                series.Header = seriesNames[i];
            }

            chart.Title.Text = chartTitle;
            chart.Legend.Position = eLegendPosition.Top;

            worksheet.Columns.AutoFit();

            package.SaveAs(new FileInfo(filePath));
        }

        // Other methods
        public static decimal ConvertStringToDecimal(string value)
        {
            if (value == ReadOnlyVariables.EmptyCell) { return 0; }

            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                CustomMessageBox.Show(
                    "Cannot import",
                    $"Cannot import because a money value is not in the correct format: {value}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return -1;
            }
        }
    }
}