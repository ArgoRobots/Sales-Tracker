using Sales_Tracker.DataClasses;

namespace Sales_Tracker.Classes
{
    public class ReceiptsManager
    {
        public static (string, bool) SaveReceiptInFile(string receiptFilePath)
        {
            // Replace the company name with companyName_text
            string[] pathParts = Directories.Receipts_dir.Split(Path.DirectorySeparatorChar);
            if (pathParts[7] == ReadOnlyVariables.CompanyName_text)
            {
                pathParts[7] = Directories.CompanyName;
            }
            string newReceiptsDir = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts);

            string newFilePath = newReceiptsDir + Path.GetFileName(receiptFilePath);

            if (File.Exists(newFilePath))
            {
                // Get a new name for the file
                string name = Path.GetFileNameWithoutExtension(receiptFilePath);
                List<string> fileNames = Directories.GetListOfAllFilesWithoutExtensionInDirectory(newReceiptsDir);

                string suggestedThingName = Tools.AddNumberForAStringThatAlreadyExists(name, fileNames);

                CustomMessageBoxResult result = CustomMessageBox.Show($"Rename receipt",
                    $"Do you want to rename '{name}' to '{suggestedThingName}'? There is already a receipt with the same name.",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.OkCancel);

                if (result == CustomMessageBoxResult.Ok)
                {
                    newFilePath = newReceiptsDir + suggestedThingName + Path.GetExtension(receiptFilePath);
                }
                else { return ("", false); }
            }

            // Save receipt
            bool saved = Directories.CopyFile(receiptFilePath.Replace(ReadOnlyVariables.Receipt_text, ""), newFilePath);

            // Replace the companyName_text with companyName_textcompany name
            pathParts = newFilePath.Split(Path.DirectorySeparatorChar);
            if (pathParts[7] == Directories.CompanyName)
            {
                pathParts[7] = ReadOnlyVariables.CompanyName_text;
            }
            string newPath = ReadOnlyVariables.Receipt_text + string.Join(Path.DirectorySeparatorChar.ToString(), pathParts);

            return (newPath, saved);
        }
        public static void RemoveReceiptFromFile(DataGridViewRow row)
        {
            string filePath = row.Tag.ToString();

            if (File.Exists(filePath))
            {
                Directories.DeleteFile(filePath);
            }
            row.Tag = null;
        }
        public static void AddReceiptToTag(DataGridViewRow row, string filePath)
        {
            if (row.Tag is List<string> tagList)
            {
                tagList[^1] = filePath;
            }
            else
            {
                row.Tag = filePath;
            }
        }
        public static bool CheckIfReceiptExists(string receiptFilePath)
        {
            if (!File.Exists(receiptFilePath.Replace(ReadOnlyVariables.Receipt_text, "")))
            {
                CustomMessageBox.Show("Argo Sales Tracker", $"The receipt you selected no longer exists", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return false;
            }
            return true;
        }
    }
}