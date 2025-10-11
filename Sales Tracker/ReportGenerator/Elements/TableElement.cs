using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;
using System.Data;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Data selection options for the transaction table.
    /// </summary>
    public enum TableDataSelection
    {
        All,             // Show all transactions within report filters
        TopByAmount,     // Top N highest value transactions
        BottomByAmount,  // Bottom N lowest value transactions  
        ReturnsOnly,     // Only returned items
        LossesOnly       // Only lost items
    }

    /// <summary>
    /// Sort order for table data.
    /// </summary>
    public enum TableSortOrder
    {
        DateDescending,
        DateAscending,
        AmountDescending,
        AmountAscending
    }

    /// <summary>
    /// Transaction table element for displaying tabular data.
    /// </summary>
    public class TableElement : BaseElement
    {
        // Data selection properties (General tab)
        public TransactionType TransactionType { get; set; } = TransactionType.Both;
        public bool IncludeReturns { get; set; } = true;
        public bool IncludeLosses { get; set; } = true;
        public TableDataSelection DataSelection { get; set; } = TableDataSelection.All;
        public TableSortOrder SortOrder { get; set; } = TableSortOrder.DateDescending;
        public int MaxRows { get; set; } = 10;

        // Display properties (Style tab)
        public bool ShowHeaders { get; set; } = true;
        public bool AlternateRowColors { get; set; } = true;
        public bool ShowGridLines { get; set; } = true;
        public bool ShowTotalsRow { get; set; } = false;
        public bool AutoSizeColumns { get; set; } = true;
        public float FontSize { get; set; } = 8f;
        public string FontFamily { get; set; } = "Segoe UI";
        public int DataRowHeight { get; set; } = 20;
        public int HeaderRowHeight { get; set; } = 25;
        public int CellPadding { get; set; } = 3;

        // Colors (Style tab)
        public Color HeaderBackgroundColor { get; set; } = Color.FromArgb(94, 148, 255);
        public Color HeaderTextColor { get; set; } = Color.White;
        public Color GridLineColor { get; set; } = Color.LightGray;
        public Color BaseRowColor { get; set; } = Color.White;
        public Color AlternateRowColor { get; set; } = Color.FromArgb(248, 248, 248);

        // Column visibility (Columns tab)
        public bool ShowDateColumn { get; set; } = true;
        public bool ShowTransactionIdColumn { get; set; } = true;
        public bool ShowCustomerSupplierColumn { get; set; } = true;
        public bool ShowProductColumn { get; set; } = true;
        public bool ShowQuantityColumn { get; set; } = true;
        public bool ShowUnitPriceColumn { get; set; } = false;
        public bool ShowTotalColumn { get; set; } = true;
        public bool ShowStatusColumn { get; set; } = false;
        public bool ShowAccountantColumn { get; set; } = false;
        public bool ShowShippingColumn { get; set; } = false;

        // Internal data structure for transactions
        private class TransactionData
        {
            public DateTime Date { get; set; }
            public string TransactionId { get; set; }
            public string CustomerSupplier { get; set; }
            public string Product { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
            public string Status { get; set; }
            public string Accountant { get; set; }
            public decimal Shipping { get; set; }
            public bool IsReturn { get; set; }
            public bool IsLoss { get; set; }
            public TransactionType Type { get; set; }
        }

        public override ReportElementType GetElementType() => ReportElementType.TransactionTable;
        public override BaseElement Clone()
        {
            return new TableElement
            {
                TransactionType = TransactionType,
                IncludeReturns = IncludeReturns,
                IncludeLosses = IncludeLosses,
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName,
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                DataSelection = DataSelection,
                SortOrder = SortOrder,
                MaxRows = MaxRows,
                ShowHeaders = ShowHeaders,
                AlternateRowColors = AlternateRowColors,
                ShowGridLines = ShowGridLines,
                ShowTotalsRow = ShowTotalsRow,
                AutoSizeColumns = AutoSizeColumns,
                FontSize = FontSize,
                FontFamily = FontFamily,
                DataRowHeight = DataRowHeight,
                HeaderRowHeight = HeaderRowHeight,
                CellPadding = CellPadding,
                HeaderBackgroundColor = HeaderBackgroundColor,
                HeaderTextColor = HeaderTextColor,
                GridLineColor = GridLineColor,
                BaseRowColor = BaseRowColor,
                AlternateRowColor = AlternateRowColor,
                ShowDateColumn = ShowDateColumn,
                ShowTransactionIdColumn = ShowTransactionIdColumn,
                ShowCustomerSupplierColumn = ShowCustomerSupplierColumn,
                ShowProductColumn = ShowProductColumn,
                ShowQuantityColumn = ShowQuantityColumn,
                ShowUnitPriceColumn = ShowUnitPriceColumn,
                ShowTotalColumn = ShowTotalColumn,
                ShowStatusColumn = ShowStatusColumn,
                ShowAccountantColumn = ShowAccountantColumn,
                ShowShippingColumn = ShowShippingColumn
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                List<TransactionData> transactions = GetFilteredTransactions(config);

                if (transactions == null || transactions.Count == 0)
                {
                    RenderNoDataMessage(graphics);
                    return;
                }

                RenderTable(graphics, transactions);
            }
            catch (Exception ex)
            {
                RenderErrorMessage(graphics, $"Error: {ex.Message}");
            }
        }
        private List<TransactionData> GetFilteredTransactions(ReportConfiguration config)
        {
            List<TransactionData> allTransactions = [];

            // Get date range from filters
            DateTime startDate = config?.Filters?.StartDate ?? DateTime.MinValue;
            DateTime endDate = config?.Filters?.EndDate ?? DateTime.MaxValue;

            // Load sales transactions if needed
            if (TransactionType == TransactionType.Sales || TransactionType == TransactionType.Both)
            {
                DataGridView salesGrid = MainMenu_Form.Instance.Sale_DataGridView;
                allTransactions.AddRange(ExtractTransactionsFromGrid(salesGrid, startDate, endDate, TransactionType.Sales, IncludeReturns, IncludeLosses));
            }

            // Load purchase transactions if needed  
            if (TransactionType == TransactionType.Purchases || TransactionType == TransactionType.Both)
            {
                DataGridView purchaseGrid = MainMenu_Form.Instance.Purchase_DataGridView;
                allTransactions.AddRange(ExtractTransactionsFromGrid(purchaseGrid, startDate, endDate, TransactionType.Purchases, IncludeReturns, IncludeLosses));
            }

            // Sort transactions
            allTransactions = ApplyDataSelection(allTransactions);
            allTransactions = SortTransactions(allTransactions);

            // Limit to MaxRows
            if (allTransactions.Count > MaxRows)
            {
                allTransactions = allTransactions.Take(MaxRows).ToList();
            }

            return allTransactions;
        }
        private static List<TransactionData> ExtractTransactionsFromGrid(DataGridView grid, DateTime startDate, DateTime endDate, TransactionType type, bool includeReturns, bool includeLosses)
        {
            List<TransactionData> transactions = [];

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells[ReadOnlyVariables.Date_column].Value == null)
                {
                    continue;
                }

                if (!DateTime.TryParse(row.Cells[ReadOnlyVariables.Date_column].Value.ToString(), out DateTime date))
                {
                    continue;
                }

                if (date < startDate || date > endDate)
                {
                    continue;
                }

                decimal total = 0;
                if (row.Cells[ReadOnlyVariables.Total_column].Value != null)
                {
                    if (!decimal.TryParse(row.Cells[ReadOnlyVariables.Total_column].Value.ToString(), out total))
                    {
                        total = 0;
                    }
                }

                // Use the existing managers to check return and loss status
                bool isReturn = ReturnProduct.ReturnManager.IsTransactionReturned(row);
                bool isLoss = LostProduct.LostManager.IsTransactionLost(row);

                // Check filters for returns and losses
                if (isReturn && !includeReturns)
                {
                    continue;
                }
                if (isLoss && !includeLosses)
                {
                    continue;
                }

                // Determine status text
                string status = "Normal";
                if (isReturn && isLoss)
                {
                    status = "Return/Loss";
                }
                else if (isReturn)
                {
                    bool isPartialReturn = ReturnProduct.ReturnManager.IsTransactionPartiallyReturned(row);
                    status = isPartialReturn ? "Partial Return" : "Return";
                }
                else if (isLoss)
                {
                    bool isPartialLoss = LostProduct.LostManager.IsTransactionPartiallyLost(row);
                    status = isPartialLoss ? "Partial Loss" : "Loss";
                }

                TransactionData transaction = new()
                {
                    Date = date,
                    TransactionId = row.Cells[ReadOnlyVariables.ID_column].Value?.ToString() ?? "",
                    CustomerSupplier = row.Cells[ReadOnlyVariables.Company_column].Value?.ToString() ?? "",
                    Product = row.Cells[ReadOnlyVariables.Product_column].Value?.ToString() ?? "",
                    Quantity = ParseDecimal(row.Cells[ReadOnlyVariables.TotalItems_column].Value),
                    UnitPrice = ParseDecimal(row.Cells[ReadOnlyVariables.PricePerUnit_column].Value),
                    Total = Math.Abs(total),
                    Status = status,
                    Accountant = row.Cells[ReadOnlyVariables.Accountant_column].Value?.ToString() ?? "",
                    Shipping = ParseDecimal(row.Cells[ReadOnlyVariables.Shipping_column].Value),
                    IsReturn = isReturn,
                    IsLoss = isLoss,
                    Type = type
                };

                transactions.Add(transaction);
            }

            return transactions;
        }
        private static decimal ParseDecimal(object value)
        {
            if (value == null) { return 0; }

            if (decimal.TryParse(value.ToString(), out decimal result))
            {
                return result;
            }
            return 0;
        }
        private List<TransactionData> ApplyDataSelection(List<TransactionData> transactions)
        {
            return DataSelection switch
            {
                TableDataSelection.TopByAmount => transactions.OrderByDescending(t => t.Total).ToList(),
                TableDataSelection.BottomByAmount => transactions.OrderBy(t => t.Total).ToList(),
                TableDataSelection.ReturnsOnly => transactions.Where(t => t.IsReturn).ToList(),
                TableDataSelection.LossesOnly => transactions.Where(t => t.IsLoss).ToList(),
                _ => transactions
            };
        }
        private List<TransactionData> SortTransactions(List<TransactionData> transactions)
        {
            return SortOrder switch
            {
                TableSortOrder.DateAscending => transactions.OrderBy(t => t.Date).ToList(),
                TableSortOrder.DateDescending => transactions.OrderByDescending(t => t.Date).ToList(),
                TableSortOrder.AmountAscending => transactions.OrderBy(t => t.Total).ToList(),
                TableSortOrder.AmountDescending => transactions.OrderByDescending(t => t.Total).ToList(),
                _ => transactions
            };
        }
        private void RenderTable(Graphics graphics, List<TransactionData> transactions)
        {
            using Font headerFont = new(FontFamily, FontSize, FontStyle.Bold);
            using Font dataFont = new(FontFamily, FontSize);
            using SolidBrush headerBgBrush = new(HeaderBackgroundColor);
            using SolidBrush headerTextBrush = new(HeaderTextColor);
            using SolidBrush dataBrush = new(Color.Black);
            using SolidBrush alternateBrush = new(AlternateRowColor);
            using Pen gridPen = new(GridLineColor, 0.5f);

            // Draw base background for entire table bounds
            using (SolidBrush baseBgBrush = new(BaseRowColor))
            {
                graphics.FillRectangle(baseBgBrush, Bounds);
            }

            // Calculate column widths
            List<(string Header, float Width, bool Show, StringAlignment Align)> columns = GetColumnDefinitions(graphics, headerFont, dataFont, transactions);

            float totalWidth = columns.Where(c => c.Show).Sum(c => c.Width);
            float scaleFactor = Bounds.Width / totalWidth;

            // Scale columns to fit
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i] = (columns[i].Header, columns[i].Width * scaleFactor, columns[i].Show, columns[i].Align);
            }

            float currentY = Bounds.Y;
            float currentX = Bounds.X;

            // Draw header if enabled
            if (ShowHeaders)
            {
                graphics.FillRectangle(headerBgBrush, Bounds.X, currentY, Bounds.Width, HeaderRowHeight);

                currentX = Bounds.X;
                foreach ((string Header, float Width, bool Show, StringAlignment Align) in columns.Where(c => c.Show))
                {
                    RectangleF headerRect = new(currentX, currentY, Width, HeaderRowHeight);
                    DrawCellText(graphics, Header, headerRect, headerFont, headerTextBrush, StringAlignment.Center);

                    if (ShowGridLines)
                    {
                        graphics.DrawRectangle(gridPen, headerRect.X, headerRect.Y, headerRect.Width, headerRect.Height);
                    }

                    currentX += Width;
                }

                currentY += HeaderRowHeight;
            }

            // Draw data rows
            int rowIndex = 0;
            foreach (TransactionData transaction in transactions)
            {
                // Alternate row colors
                if (AlternateRowColors && rowIndex % 2 == 1)
                {
                    graphics.FillRectangle(alternateBrush, Bounds.X, currentY, Bounds.Width, DataRowHeight);
                }
                else
                {
                    using SolidBrush bgBrush = new(BaseRowColor);
                    graphics.FillRectangle(bgBrush, Bounds.X, currentY, Bounds.Width, DataRowHeight);
                }

                currentX = Bounds.X;
                int colIndex = 0;

                // Draw each column
                if (ShowDateColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.Date.ToShortDateString(), cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowTransactionIdColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.TransactionId, cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowCustomerSupplierColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.CustomerSupplier, cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowProductColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.Product, cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowQuantityColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.Quantity.ToString("N0"), cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowUnitPriceColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, $"${transaction.UnitPrice:N2}", cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowTotalColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, $"${transaction.Total:N2}", cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowStatusColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.Status, cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowAccountantColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, transaction.Accountant, cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }
                colIndex++;

                if (ShowShippingColumn && columns[colIndex].Show)
                {
                    RectangleF cellRect = new(currentX, currentY, columns[colIndex].Width, DataRowHeight);
                    DrawCellText(graphics, $"${transaction.Shipping:N2}", cellRect, dataFont, dataBrush, columns[colIndex].Align);
                    if (ShowGridLines) graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    currentX += columns[colIndex].Width;
                }

                currentY += DataRowHeight;
                rowIndex++;

                // Stop if we've exceeded the bounds
                if (currentY > Bounds.Bottom - DataRowHeight)
                    break;
            }

            // Draw totals row if enabled
            if (ShowTotalsRow && transactions.Count > 0)
            {
                using SolidBrush totalsBrush = new(Color.FromArgb(230, 230, 230));
                graphics.FillRectangle(totalsBrush, Bounds.X, currentY, Bounds.Width, DataRowHeight);

                currentX = Bounds.X;

                // Calculate totals
                decimal totalAmount = transactions.Sum(t => t.Total);
                int totalCount = transactions.Count;

                // Draw totals label in first visible column
                bool labelDrawn = false;
                foreach ((string Header, float Width, bool Show, StringAlignment Align) in columns.Where(c => c.Show))
                {
                    RectangleF cellRect = new(currentX, currentY, Width, DataRowHeight);

                    if (!labelDrawn)
                    {
                        DrawCellText(graphics, $"Totals ({totalCount} rows)", cellRect, headerFont, dataBrush, StringAlignment.Near);
                        labelDrawn = true;
                    }
                    else if (Header == "Total")
                    {
                        DrawCellText(graphics, $"${totalAmount:N2}", cellRect, headerFont, dataBrush, StringAlignment.Far);
                    }

                    if (ShowGridLines)
                        graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                    currentX += Width;
                }
            }

            // Draw outer border
            using Pen borderPen = new(Color.Black, 1);
            graphics.DrawRectangle(borderPen, Bounds);
        }
        private List<(string Header, float Width, bool Show, StringAlignment Align)> GetColumnDefinitions(
            Graphics graphics, Font headerFont, Font dataFont, List<TransactionData> transactions)
        {
            List<(string Header, float Width, bool Show, StringAlignment Align)> columns = [];

            // Define columns with minimum widths
            columns.Add(("Date", ShowDateColumn ? CalculateColumnWidth(graphics, "Date", "2024-12-31", headerFont, dataFont) : 0, ShowDateColumn, StringAlignment.Near));
            columns.Add(("ID", ShowTransactionIdColumn ? CalculateColumnWidth(graphics, "ID", "INV-12345", headerFont, dataFont) : 0, ShowTransactionIdColumn, StringAlignment.Near));
            columns.Add(("Customer/Supplier", ShowCustomerSupplierColumn ? CalculateColumnWidth(graphics, "Customer/Supplier", GetLongestText(transactions, t => t.CustomerSupplier), headerFont, dataFont) : 0, ShowCustomerSupplierColumn, StringAlignment.Near));
            columns.Add(("Product", ShowProductColumn ? CalculateColumnWidth(graphics, "Product", GetLongestText(transactions, t => t.Product), headerFont, dataFont) : 0, ShowProductColumn, StringAlignment.Near));
            columns.Add(("Qty", ShowQuantityColumn ? CalculateColumnWidth(graphics, "Qty", "9,999", headerFont, dataFont) : 0, ShowQuantityColumn, StringAlignment.Far));
            columns.Add(("Unit Price", ShowUnitPriceColumn ? CalculateColumnWidth(graphics, "Unit Price", "$9,999.99", headerFont, dataFont) : 0, ShowUnitPriceColumn, StringAlignment.Far));
            columns.Add(("Total", ShowTotalColumn ? CalculateColumnWidth(graphics, "Total", "$999,999.99", headerFont, dataFont) : 0, ShowTotalColumn, StringAlignment.Far));
            columns.Add(("Status", ShowStatusColumn ? CalculateColumnWidth(graphics, "Status", "Return", headerFont, dataFont) : 0, ShowStatusColumn, StringAlignment.Center));
            columns.Add(("Accountant", ShowAccountantColumn ? CalculateColumnWidth(graphics, "Accountant", GetLongestText(transactions, t => t.Accountant), headerFont, dataFont) : 0, ShowAccountantColumn, StringAlignment.Near));
            columns.Add(("Shipping", ShowShippingColumn ? CalculateColumnWidth(graphics, "Shipping", "$999.99", headerFont, dataFont) : 0, ShowShippingColumn, StringAlignment.Far));

            return columns;
        }
        private static string GetLongestText(List<TransactionData> transactions, Func<TransactionData, string> selector)
        {
            return transactions == null || transactions.Count == 0
                ? "Sample Text"
                : transactions.Select(selector).OrderByDescending(s => s?.Length ?? 0).FirstOrDefault() ?? "Sample Text";
        }
        private float CalculateColumnWidth(Graphics graphics, string header, string sampleData, Font headerFont, Font dataFont)
        {
            float headerWidth = graphics.MeasureString(header, headerFont).Width;
            float dataWidth = graphics.MeasureString(sampleData, dataFont).Width;
            return Math.Max(headerWidth, dataWidth) + (CellPadding * 2);  // Add padding
        }
        private void DrawCellText(Graphics graphics, string text, RectangleF cellRect, Font font, Brush brush, StringAlignment alignment)
        {
            StringFormat format = new()
            {
                Alignment = alignment,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.EllipsisCharacter
            };

            // Add padding
            RectangleF textRect = new(
                cellRect.X + CellPadding,
                cellRect.Y,
                cellRect.Width - (CellPadding * 2),
                cellRect.Height
            );

            graphics.DrawString(text, font, brush, textRect, format);
        }
        private void RenderNoDataMessage(Graphics graphics)
        {
            using Font font = new("Segoe UI", 10);
            using SolidBrush brush = new(Color.Gray);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString("No transactions to display", font, brush, Bounds, format);
        }
        private void RenderErrorMessage(Graphics graphics, string message)
        {
            using SolidBrush bgBrush = new(Color.FromArgb(255, 240, 240));
            using Pen borderPen = new(Color.Red, 1);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.DarkRed);

            graphics.FillRectangle(bgBrush, Bounds);
            graphics.DrawRectangle(borderPen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(message, font, textBrush, Bounds, format);
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.Lavender);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            // Draw mini table preview
            using Pen gridPen = new(Color.LightGray, 0.5f);
            float rowHeight = 20;
            float colWidth = Bounds.Width / 4;

            // Draw header row
            using SolidBrush headerBrush = new(HeaderBackgroundColor);
            RectangleF headerRect = new(Bounds.X, Bounds.Y, Bounds.Width, rowHeight);
            graphics.FillRectangle(headerBrush, headerRect);

            // Draw grid lines
            for (int i = 0; i < 3; i++)
            {
                float y = Bounds.Y + (i + 1) * rowHeight;
                if (y < Bounds.Bottom)
                    graphics.DrawLine(gridPen, Bounds.Left, y, Bounds.Right, y);
            }

            for (int i = 1; i < 4; i++)
            {
                float x = Bounds.X + i * colWidth;
                graphics.DrawLine(gridPen, x, Bounds.Y, x, Math.Min(Bounds.Y + rowHeight * 3, Bounds.Bottom));
            }

            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Black);
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            RectangleF labelRect = new(Bounds.X, Bounds.Y + Bounds.Height / 2 - 10, Bounds.Width, 20);
            graphics.DrawString(DisplayName ?? "Transaction Table", font, textBrush, labelRect, format);
        }

        // Override the property to indicate this element handles its own common controls
        public override bool HandlesOwnCommonControls => true;

        public static Panel General_Panel { get; private set; }
        public static Panel Style_Panel { get; private set; }
        public static Panel Columns_Panel { get; private set; }
        protected override int CreateElementSpecificControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Create panels for each tab (only once)
            General_Panel = CreateTabPanel(container, true);
            Style_Panel = CreateTabPanel(container, false);
            Columns_Panel = CreateTabPanel(container, false);

            container.Controls.Add(General_Panel);
            container.Controls.Add(Style_Panel);
            container.Controls.Add(Columns_Panel);

            // Create controls for each tab panel
            CreateGeneralTabControls(General_Panel, 0, onPropertyChanged);
            CreateStyleTabControls(Style_Panel, 0, onPropertyChanged);
            CreateColumnsTabControls(Columns_Panel, 0, onPropertyChanged);

            // Tab changed handler
            static void TabChangedHandler(int tabIndex)
            {
                General_Panel.Visible = tabIndex == 0;
                Style_Panel.Visible = tabIndex == 1;
                Columns_Panel.Visible = tabIndex == 2;
            }

            // Create tab buttons
            string[] tabNames = ["General", "Style", "Columns"];
            Panel tabPanel = CreateTabButtons(container, tabNames, TabChangedHandler);

            return yPosition + 45;
        }
        private static Panel CreateTabPanel(Panel container, bool visible) => new()
        {
            Location = new Point(0, 45),
            Size = new Size(container.Width, container.Height - 45),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true,
            Visible = visible
        };
        private void CreateGeneralTabControls(Panel panel, int yPosition, Action onPropertyChanged)
        {
            // Add common controls first
            CreateCommonPropertyControls(panel, this, yPosition, onPropertyChanged,
                out Dictionary<string, Action> commonUpdateActions);

            // Cache the common control update actions
            foreach (KeyValuePair<string, Action> kvp in commonUpdateActions)
            {
                CacheControl(kvp.Key, null, kvp.Value);
            }

            yPosition += ControlRowHeight * 5;

            // Data selection combo box
            AddPropertyLabel(panel, "Data:", yPosition);
            Guna2ComboBox dataCombo = AddPropertyComboBox(panel, DataSelection.ToString(), yPosition,
                Enum.GetNames<TableDataSelection>(),
                value =>
                {
                    DataSelection = Enum.Parse<TableDataSelection>(value);
                    onPropertyChanged();
                });
            CacheControl("DataSelection", dataCombo, () => dataCombo.SelectedItem = DataSelection.ToString());
            yPosition += ControlRowHeight;

            // Transaction type
            AddPropertyLabel(panel, "Type:", yPosition);
            Guna2ComboBox typeCombo = AddPropertyComboBox(panel, TransactionType.ToString(), yPosition,
                ["Sales", "Purchases", "Both"],
                value =>
                {
                    TransactionType = Enum.Parse<TransactionType>(value);
                    onPropertyChanged();
                });
            CacheControl("TransactionType", typeCombo, () => typeCombo.SelectedItem = TransactionType.ToString());
            yPosition += ControlRowHeight;

            // Include returns checkbox
            AddPropertyCheckBoxWithLabel(panel, "Include Returns", IncludeReturns, yPosition,
                value =>
                {
                    IncludeReturns = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox returnsCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("IncludeReturns", returnsCheck, () => returnsCheck.Checked = IncludeReturns);
            yPosition += CheckBoxRowHeight;

            // Include losses checkbox
            AddPropertyCheckBoxWithLabel(panel, "Include Losses", IncludeLosses, yPosition,
                value =>
                {
                    IncludeLosses = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox lossesCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("IncludeLosses", lossesCheck, () => lossesCheck.Checked = IncludeLosses);
            yPosition += CheckBoxRowHeight;

            // Sort order combo box
            AddPropertyLabel(panel, "Sort:", yPosition);
            Guna2ComboBox sortCombo = AddPropertyComboBox(panel, FormatSortOrder(SortOrder), yPosition,
                ["Date ↓", "Date ↑", "Amount ↓", "Amount ↑"],
                value =>
                {
                    SortOrder = ParseSortOrder(value);
                    onPropertyChanged();
                });
            CacheControl("SortOrder", sortCombo, () => sortCombo.SelectedItem = FormatSortOrder(SortOrder));
            yPosition += ControlRowHeight;

            // Max rows
            AddPropertyLabel(panel, "Max Rows:", yPosition);
            Guna2NumericUpDown numericUpDown = AddPropertyNumericUpDown(panel, MaxRows, yPosition, value =>
            {
                MaxRows = (int)value;
                onPropertyChanged();
            }, 1, 100);
            numericUpDown.Left = 110;
            CacheControl("MaxRows", numericUpDown, () => numericUpDown.Value = MaxRows);
            yPosition += ControlRowHeight;

            // Totals row
            AddPropertyCheckBoxWithLabel(panel, "Show Totals Row", ShowTotalsRow, yPosition,
                value =>
                {
                    ShowTotalsRow = value;
                    onPropertyChanged();
                });
            Guna2CustomCheckBox totalsCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowTotalsRow", totalsCheck, () => totalsCheck.Checked = ShowTotalsRow);
        }
        private void CreateStyleTabControls(Panel panel, int yPosition, Action onPropertyChanged)
        {
            // Font family
            AddPropertyLabel(panel, "Font:", yPosition);
            Guna2ComboBox fontCombo = AddPropertyComboBox(panel, FontFamily, yPosition,
                ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana"],
                value =>
                {
                    FontFamily = value;
                    onPropertyChanged();
                });
            CacheControl("FontFamily", fontCombo, () => fontCombo.SelectedItem = FontFamily);
            yPosition += ControlRowHeight;

            // Font size
            AddPropertyLabel(panel, "Font Size:", yPosition);
            Guna2NumericUpDown fontSizeNumeric = AddPropertyNumericUpDown(panel, (decimal)FontSize, yPosition, value =>
            {
                FontSize = (float)value;
                onPropertyChanged();
            }, 6, 14);
            fontSizeNumeric.Left = 150;
            CacheControl("FontSize", fontSizeNumeric, () => fontSizeNumeric.Value = (decimal)FontSize);
            yPosition += ControlRowHeight;

            // Row Height
            AddPropertyLabel(panel, "Row Height:", yPosition);
            Guna2NumericUpDown rowHeightNumeric = AddPropertyNumericUpDown(panel, DataRowHeight, yPosition, value =>
            {
                DataRowHeight = (int)value;
                onPropertyChanged();
            }, 15, 50);
            rowHeightNumeric.Left = 150;
            CacheControl("DataRowHeight", rowHeightNumeric, () => rowHeightNumeric.Value = DataRowHeight);
            yPosition += ControlRowHeight;

            // Header Row Height
            AddPropertyLabel(panel, "Header Height:", yPosition);
            Guna2NumericUpDown headerHeightNumeric = AddPropertyNumericUpDown(panel, HeaderRowHeight, yPosition, value =>
            {
                HeaderRowHeight = (int)value;
                onPropertyChanged();
            }, 20, 60);
            headerHeightNumeric.Left = 150;
            CacheControl("HeaderRowHeight", headerHeightNumeric, () => headerHeightNumeric.Value = HeaderRowHeight);
            yPosition += ControlRowHeight;

            // Cell Padding
            AddPropertyLabel(panel, "Cell Padding:", yPosition);
            Guna2NumericUpDown cellPaddingNumeric = AddPropertyNumericUpDown(panel, CellPadding, yPosition, value =>
            {
                CellPadding = (int)value;
                onPropertyChanged();
            }, 0, 20);
            cellPaddingNumeric.Left = 150;
            CacheControl("CellPadding", cellPaddingNumeric, () => cellPaddingNumeric.Value = CellPadding);
            yPosition += ControlRowHeight;

            // Display option checkboxes
            AddPropertyCheckBoxWithLabel(panel, "Show Headers", ShowHeaders, yPosition,
                value => { ShowHeaders = value; onPropertyChanged(); });
            Guna2CustomCheckBox headersCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowHeaders", headersCheck, () => headersCheck.Checked = ShowHeaders);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Alternate Row Colors", AlternateRowColors, yPosition,
                value => { AlternateRowColors = value; onPropertyChanged(); });
            Guna2CustomCheckBox alternateCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("AlternateRowColors", alternateCheck, () => alternateCheck.Checked = AlternateRowColors);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Show Grid Lines", ShowGridLines, yPosition,
                value => { ShowGridLines = value; onPropertyChanged(); });
            Guna2CustomCheckBox gridCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowGridLines", gridCheck, () => gridCheck.Checked = ShowGridLines);
        }
        private void CreateColumnsTabControls(Panel panel, int yPosition, Action onPropertyChanged)
        {
            // Add column visibility checkboxes
            AddPropertyCheckBoxWithLabel(panel, "Date", ShowDateColumn, yPosition,
                value => { ShowDateColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox dateCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowDateColumn", dateCheck, () => dateCheck.Checked = ShowDateColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Transaction ID", ShowTransactionIdColumn, yPosition,
                value => { ShowTransactionIdColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox idCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowTransactionIdColumn", idCheck, () => idCheck.Checked = ShowTransactionIdColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Customer/Supplier", ShowCustomerSupplierColumn, yPosition,
                value => { ShowCustomerSupplierColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox customerCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowCustomerSupplierColumn", customerCheck, () => customerCheck.Checked = ShowCustomerSupplierColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Product", ShowProductColumn, yPosition,
                value => { ShowProductColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox productCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowProductColumn", productCheck, () => productCheck.Checked = ShowProductColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Quantity", ShowQuantityColumn, yPosition,
                value => { ShowQuantityColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox quantityCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowQuantityColumn", quantityCheck, () => quantityCheck.Checked = ShowQuantityColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Unit Price", ShowUnitPriceColumn, yPosition,
                value => { ShowUnitPriceColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox unitPriceCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowUnitPriceColumn", unitPriceCheck, () => unitPriceCheck.Checked = ShowUnitPriceColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Total", ShowTotalColumn, yPosition,
                value => { ShowTotalColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox totalCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowTotalColumn", totalCheck, () => totalCheck.Checked = ShowTotalColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Status", ShowStatusColumn, yPosition,
                value => { ShowStatusColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox statusCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowStatusColumn", statusCheck, () => statusCheck.Checked = ShowStatusColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Accountant", ShowAccountantColumn, yPosition,
                value => { ShowAccountantColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox accountantCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowAccountantColumn", accountantCheck, () => accountantCheck.Checked = ShowAccountantColumn);
            yPosition += CheckBoxRowHeight;

            AddPropertyCheckBoxWithLabel(panel, "Shipping", ShowShippingColumn, yPosition,
                value => { ShowShippingColumn = value; onPropertyChanged(); });
            Guna2CustomCheckBox shippingCheck = panel.Controls.OfType<Guna2CustomCheckBox>().LastOrDefault();
            CacheControl("ShowShippingColumn", shippingCheck, () => shippingCheck.Checked = ShowShippingColumn);
        }

        // Helper methods
        private static string FormatSortOrder(TableSortOrder order)
        {
            return order switch
            {
                TableSortOrder.DateDescending => "Date ↓",
                TableSortOrder.DateAscending => "Date ↑",
                TableSortOrder.AmountDescending => "Amount ↓",
                TableSortOrder.AmountAscending => "Amount ↑",
                _ => "Date ↓"
            };
        }
        private static TableSortOrder ParseSortOrder(string formatted)
        {
            return formatted switch
            {
                "Date ↓" => TableSortOrder.DateDescending,
                "Date ↑" => TableSortOrder.DateAscending,
                "Amount ↓" => TableSortOrder.AmountDescending,
                "Amount ↑" => TableSortOrder.AmountAscending,
                _ => TableSortOrder.DateDescending
            };
        }
    }
}