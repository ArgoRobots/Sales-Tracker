using ClosedXML.Excel;
using Sales_Tracker;
using Sales_Tracker.Classes;

namespace Argo_Studio.Classes
{
    internal class ExcelManager
    {
        public static void ExportToExcel(string filePath)
        {
            using XLWorkbook workbook = new();

            // Create worksheet
            IXLWorksheet worksheet = workbook.Worksheets.Add($"{Directories.CompanyName} sales data");

            // Add headers
            worksheet.Cell(1, 1).Value = "Product ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Country of Origin";
            worksheet.Cell(1, 4).Value = "Company of Origin";
            worksheet.Cell(1, 5).Value = "Category";

            int currentRow = 2;

            // Loop through your data and add rows
            foreach (Category category in MainMenu_Form.Instance.categorySaleList)
            {
                foreach (var product in category.ProductList)
                {
                    worksheet.Cell(currentRow, 1).Value = product.ProductID;
                    worksheet.Cell(currentRow, 2).Value = product.Name;
                    worksheet.Cell(currentRow, 3).Value = product.CountryOfOrigin;
                    worksheet.Cell(currentRow, 4).Value = product.CompanyOfOrigin;
                    worksheet.Cell(currentRow, 5).Value = category.Name;
                    currentRow++;
                }
            }

            // Create accountants worksheet
            IXLWorksheet accountantsWorksheet = workbook.Worksheets.Add("Accountants");

            // Add accountants headers
            accountantsWorksheet.Cell(1, 1).Value = "Accountant name";

            currentRow = 2;

            // Loop through your accountants and add rows
            foreach (string accountant in MainMenu_Form.Instance.accountantList)
            {
                accountantsWorksheet.Cell(currentRow, 1).Value = accountant;
                currentRow++;
            }

            // Create companies worksheet
            IXLWorksheet companiesWorksheet = workbook.Worksheets.Add("Companies");

            // Add companies headers
            companiesWorksheet.Cell(1, 1).Value = "Company name";

            currentRow = 2;

            // Loop through your companies and add rows
            foreach (string company in MainMenu_Form.Instance.companyList)
            {
                companiesWorksheet.Cell(currentRow, 1).Value = company;
                currentRow++;
            }

            // Save the file
            workbook.SaveAs(filePath);
        }
    }
}