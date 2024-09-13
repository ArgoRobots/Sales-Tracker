using ClosedXML.Excel;
using System.Linq;

namespace Sales_Tracker.Classes
{
    internal class SpreadsheetManager
    {
        // Import spreadsheet methods
        public static void ImportAccountantsData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();

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
                }
            }
            MainMenu_Form.Instance.SaveListToFile(MainMenu_Form.Instance.accountantList);
        }
        public static void ImportCompaniesData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();

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
                }
            }
            MainMenu_Form.Instance.SaveListToFile(MainMenu_Form.Instance.companyList);
        }
        public static void ImportProductsData(IXLWorksheet worksheet, bool purchase, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();

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
                Category category = list
                    .FirstOrDefault(c => c.Name == categoryName);

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
                    CustomMessageBox.Show("Argo Sales Tracker",
                        $"The product {productName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    category.ProductList.Add(product);
                }

                if (purchase)
                {
                    MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategoryPurchases);
                }
                else
                {
                    MainMenu_Form.Instance.SaveCategoriesToFile(MainMenu_Form.SelectedOption.CategorySales);
                }
            }
        }
        public static void ImportPurchaseData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            MainMenu_Form.Instance.Purchases_DataGridView.Rows.Clear();

            foreach (IXLRow row in rowsToProcess)
            {
                DataGridViewRow newRow = new();
                for (int i = 0; i < row.Cells().Count(); i++)
                {
                    newRow.Cells[i].Value = row.Cell(i + 1).GetValue<string>();
                }
                MainMenu_Form.Instance.Purchases_DataGridView.Rows.Add(newRow);
            }
        }
        public static void ImportSalesData(IXLWorksheet worksheet, bool skipHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = skipHeader ? worksheet.RowsUsed().Skip(1) : worksheet.RowsUsed();
            MainMenu_Form.Instance.Sales_DataGridView.Rows.Clear();

            foreach (IXLRow row in rowsToProcess)
            {
                DataGridViewRow newRow = new();
                for (int i = 0; i < row.Cells().Count(); i++)
                {
                    newRow.Cells[i].Value = row.Cell(i + 1).GetValue<string>();
                }
                MainMenu_Form.Instance.Sales_DataGridView.Rows.Add(newRow);
            }
        }

        // Export spreadsheet methods
        public static void ExportSpreadsheet(string filePath)
        {
            filePath = Directories.GetNewFileNameIfItAlreadyExists(filePath);

            using XLWorkbook workbook = new();

            IXLWorksheet purchaseWorksheet = workbook.Worksheets.Add("Purchases");
            AddDataToWorksheet(purchaseWorksheet, MainMenu_Form.Instance.Purchases_DataGridView);

            IXLWorksheet salesWorksheet = workbook.Worksheets.Add("Sales");
            AddDataToWorksheet(salesWorksheet, MainMenu_Form.Instance.Sales_DataGridView);

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
        private static void AddDataToWorksheet(IXLWorksheet worksheet, DataGridView dataGridView)
        {
            // Add headers and format them
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                IXLCell cell = worksheet.Cell(1, i + 1);
                cell.Value = dataGridView.Columns[i].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            int lastCellIndex = dataGridView.Columns.Count + 1;

            // Add header for the receipt column
            worksheet.Cell(1, lastCellIndex).Value = "Receipt";
            worksheet.Cell(1, lastCellIndex).Style.Font.Bold = true;
            worksheet.Cell(1, lastCellIndex).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // Iterate over the cells in the row to fill the data
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    string? cellValue = row.Cells[i].Value?.ToString();
                    IXLCell excelCell = worksheet.Cell(currentRow, i + 1);

                    // Check if the last cell has "show"
                    if (cellValue == MainMenu_Form.show_text && row.Cells[i].Tag != null)
                    {
                        excelCell.Value = row.Cells[i].Tag.ToString();
                    }
                    else
                    {
                        excelCell.Value = cellValue;
                    }
                }

                // Handle receipts and adding new rows
                if (row.Tag is (List<string> tagList, TagData) && tagList.Count > 0)
                {
                    // Is there a receipt
                    byte receiptOffset = 0;
                    string lastItem = tagList[^1];
                    if (lastItem.StartsWith(MainMenu_Form.receipt_text))
                    {
                        receiptOffset = 1;
                        worksheet.Cell(currentRow, lastCellIndex).Value = lastItem;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, lastCellIndex).Value = MainMenu_Form.emptyCell;
                    }

                    // Add additional rows if they exist in the tagList
                    for (int j = 0; j < tagList.Count - receiptOffset; j++)
                    {
                        currentRow++;
                        string[] values = tagList[j].Split(',');
                        for (int k = 0; k < values.Length; k++)
                        {
                            worksheet.Cell(currentRow, k + 3).Value = values[k];
                        }
                    }
                }
                else if (row.Tag is (string tagString, TagData))
                {
                    string fileName = Path.GetFileName(tagString);
                    worksheet.Cell(currentRow, lastCellIndex).Value = fileName;
                }
                else if (row.Tag is string)
                {
                    string fileName = Path.GetFileName(row.Tag.ToString());
                    worksheet.Cell(currentRow, lastCellIndex).Value = fileName;
                }
                else
                {
                    worksheet.Cell(currentRow, lastCellIndex).Value = MainMenu_Form.emptyCell;
                }

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
    }
}