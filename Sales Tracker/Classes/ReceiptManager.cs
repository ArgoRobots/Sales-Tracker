using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker.Classes
{
    public class ReceiptManager
    {
        public static void ExportSelectedReceipts(Guna2DataGridView dataGridView)
        {
            // Select directory
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string destinationPath = dialog.SelectedPath;
            int selectedRowCount = dataGridView.SelectedRows.Count;

            if (selectedRowCount > 1)
            {
                // Create a new folder for multiple files
                string name = LanguageManager.TranslateSingleString("Argo Sales Tracker receipts for");
                string newFolderPath = Path.Combine(destinationPath, $"{name} {Directories.CompanyName} - "
                    + DateTime.Now.ToString("yyyyMMddHHmmss"));

                Directory.CreateDirectory(newFolderPath);
                destinationPath = newFolderPath;
            }

            bool isAnyReceiptExported = false, doAllRowsHaveReceipt = true;

            // Iterate through selected rows and copy files
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                if (!row.Visible) { continue; }

                string receipt = DataGridViewManager.GetFilePathFromRowTag(row.Tag);
                if (receipt == "")
                {
                    doAllRowsHaveReceipt = false;
                    continue;
                }

                receipt = ProcessReceiptTextFromRowTag(receipt);

                if (!File.Exists(receipt))
                {
                    Log.Error_FileDoesNotExist(receipt);
                    continue;
                }

                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(receipt));
                Directories.CopyFile(receipt, destinationFilePath);
                isAnyReceiptExported = true;
            }

            if (isAnyReceiptExported)
            {
                string message = "Receipts exported successfully";

                if (!doAllRowsHaveReceipt) { message += " Note: Not all the selected rows contain a receipt."; }

                CustomMessageBox.Show(
                    "Receipts exported", message,
                    CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
            }
        }
        public static string ProcessReceiptTextFromRowTag(string receipt)
        {
            return receipt.Replace(ReadOnlyVariables.CompanyName_text, Directories.CompanyName)
                .Replace(ReadOnlyVariables.Receipt_text, "");
        }
        public static string? GetReceiptPathFromRow(DataGridViewRow row)
        {
            if (row.Tag is (List<string> items, _))
            {
                // Handle multiple items case
                string lastItem = items.LastOrDefault() ?? string.Empty;
                if (lastItem.StartsWith(ReadOnlyVariables.Receipt_text))
                {
                    return ProcessReceiptTextFromRowTag(lastItem);
                }
            }
            else if (row.Tag is (string tagString, _))
            {
                // Handle single item case
                return ProcessReceiptTextFromRowTag(tagString);
            }

            return null;
        }
    }
}