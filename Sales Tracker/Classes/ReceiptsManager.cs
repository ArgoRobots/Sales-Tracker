using Sales_Tracker.DataClasses;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Manages operations related to receipts in the application, including saving, removing,
    /// and checking the existence of receipt files.
    /// </summary>
    public class ReceiptsManager
    {
        /// <summary>
        /// Saves a receipt file to a specified location, handling name conflicts by renaming the file to avoid overwriting existing receipts.
        /// </summary>
        /// <returns>A tuple containing the new path of the saved receipt and a boolean indicating if the save operation was successful.</returns>
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

        /// <summary>
        /// Removes a receipt from a specified DataGridViewRow tag, deleting the file from the filesystem if it exists.
        /// </summary>
        public static void RemoveReceiptFromFile(DataGridViewRow row)
        {
            string filePath = row.Tag.ToString();

            if (File.Exists(filePath))
            {
                Directories.DeleteFile(filePath);
            }
            row.Tag = null;
        }

        /// <summary>
        /// Associates a receipt file path with a DataGridView row's tag, either updating an existing
        /// tag list or setting a new file path.
        /// </summary>
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

        /// <summary>
        /// Checks if a specified receipt file exists in the filesystem and shows a message if it does not.
        /// </summary>
        /// <returns>True if the receipt exists or if the user did not select a receipt; otherwise, false.</returns>
        public static bool CheckIfReceiptExists(string receiptFilePath)
        {
            if (receiptFilePath != null && !File.Exists(receiptFilePath.Replace(ReadOnlyVariables.Receipt_text, "")))
            {
                CustomMessageBox.Show("Receipt does not exist", $"The receipt you selected no longer exists", CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.Ok);
                return false;
            }
            return true;
        }
    }
}