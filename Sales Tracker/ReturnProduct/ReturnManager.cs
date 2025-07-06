using Sales_Tracker.DataClasses;

namespace Sales_Tracker.ReturnProduct
{
    public static class ReturnManager
    {
        private static readonly Color ReturnedRowBackColor = Color.FromArgb(250, 235, 235);

        /// <summary>
        /// Processes a product return for the specified transaction row.
        /// </summary>
        public static void ProcessReturn(DataGridViewRow row, string reason, string additionalNotes, string returnedBy)
        {
            if (row.Tag is TagData tagData)
            {
                // Single item transaction
                tagData.IsReturned = true;
                tagData.ReturnDate = DateTime.Now;
                tagData.ReturnReason = string.IsNullOrEmpty(additionalNotes) ? reason : $"{reason} - {additionalNotes}";
                tagData.ReturnedBy = returnedBy;
            }
            else if (row.Tag is (List<string>, TagData purchaseData))
            {
                // Multiple items transaction
                purchaseData.IsReturned = true;
                purchaseData.ReturnDate = DateTime.Now;
                purchaseData.ReturnReason = string.IsNullOrEmpty(additionalNotes) ? reason : $"{reason} - {additionalNotes}";
                purchaseData.ReturnedBy = returnedBy;
            }
            else if (row.Tag is (string, TagData receiptData))
            {
                // Single item with receipt
                receiptData.IsReturned = true;
                receiptData.ReturnDate = DateTime.Now;
                receiptData.ReturnReason = string.IsNullOrEmpty(additionalNotes) ? reason : $"{reason} - {additionalNotes}";
                receiptData.ReturnedBy = returnedBy;
            }

            // Update row appearance
            UpdateRowAppearanceForReturn(row, true);

            // Log the return
            string transactionId = row.Cells[ReadOnlyVariables.ID_column].Value?.ToString() ?? "Unknown";
            string productName = row.Cells[ReadOnlyVariables.Product_column].Value?.ToString() ?? "Unknown";
            string logMessage = $"Returned {(MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases ? "purchase" : "sale")} " +
                              $"'{transactionId}' - {productName}. Reason: {reason}";

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, logMessage);

            SaveReturnChanges();
            MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
        }

        /// <summary>
        /// Undoes a product return for the specified transaction row.
        /// </summary>
        public static void UndoReturn(DataGridViewRow row, string undoReason)
        {
            if (row.Tag is TagData tagData)
            {
                tagData.IsReturned = false;
                tagData.ReturnDate = null;
                tagData.ReturnReason = null;
                tagData.ReturnedBy = null;
            }
            else if (row.Tag is (List<string>, TagData purchaseData))
            {
                purchaseData.IsReturned = false;
                purchaseData.ReturnDate = null;
                purchaseData.ReturnReason = null;
                purchaseData.ReturnedBy = null;
            }
            else if (row.Tag is (string, TagData receiptData))
            {
                receiptData.IsReturned = false;
                receiptData.ReturnDate = null;
                receiptData.ReturnReason = null;
                receiptData.ReturnedBy = null;
            }

            // Update row appearance
            UpdateRowAppearanceForReturn(row, false);

            // Log the undo
            string transactionId = row.Cells[ReadOnlyVariables.ID_column].Value?.ToString() ?? "Unknown";
            string productName = row.Cells[ReadOnlyVariables.Product_column].Value?.ToString() ?? "Unknown";
            string logMessage = $"Undid return for {(MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases ? "purchase" : "sale")} " +
                              $"'{transactionId}' - {productName}. Reason: {undoReason}";

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(MainMenu_Form.ThingsThatHaveChangedInFile, 2, logMessage);

            // Save changes
            SaveReturnChanges();

            // Refresh UI
            MainMenu_Form.Instance.RefreshDataGridViewAndCharts();
        }

        /// <summary>
        /// Checks if a transaction is returned.
        /// </summary>
        public static bool IsTransactionReturned(DataGridViewRow row)
        {
            if (row.Tag is TagData tagData)
            {
                return tagData.IsReturned;
            }
            else if (row.Tag is (List<string>, TagData purchaseData))
            {
                return purchaseData.IsReturned;
            }
            else if (row.Tag is (string, TagData receiptData))
            {
                return receiptData.IsReturned;
            }
            return false;
        }

        /// <summary>
        /// Gets return information for a transaction.
        /// </summary>
        public static (DateTime? returnDate, string returnReason, string returnedBy) GetReturnInfo(DataGridViewRow row)
        {
            TagData tagData = null;

            if (row.Tag is TagData data)
            {
                tagData = data;
            }
            else if (row.Tag is (List<string>, TagData purchaseData))
            {
                tagData = purchaseData;
            }
            else if (row.Tag is (string, TagData receiptData))
            {
                tagData = receiptData;
            }

            if (tagData != null)
            {
                return (tagData.ReturnDate, tagData.ReturnReason, tagData.ReturnedBy);
            }

            return (null, null, null);
        }

        /// <summary>
        /// Updates the visual appearance of a row based on its return status.
        /// </summary>
        public static void UpdateRowAppearanceForReturn(DataGridViewRow row, bool isReturned)
        {
            if (isReturned)
            {
                row.DefaultCellStyle.BackColor = ReturnedRowBackColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 200, 255);
            }
            else
            {
                // Reset to default colors - you may need to adjust these based on your theme
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(94, 148, 255);
            }
        }

        /// <summary>
        /// Calculates totals excluding returned transactions.
        /// </summary>
        public static decimal CalculateTotalExcludingReturns(DataGridView dataGridView, string totalColumn)
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!IsTransactionReturned(row))
                {
                    if (decimal.TryParse(row.Cells[totalColumn].Value?.ToString(), out decimal value))
                    {
                        total += value;
                    }
                }
            }
            return total;
        }
        private static void SaveReturnChanges()
        {
            // Save both purchase and sale data
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
            MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
        }
    }
}