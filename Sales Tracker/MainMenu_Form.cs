using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Graphs;
using Sales_Tracker.Properties;
using static Sales_Tracker.Classes.Theme;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : BaseForm
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init
        public static MainMenu_Form Instance { get; set; }
        public MainMenu_Form()
        {
            InitializeComponent();
            Instance = this;

            UI.ConstructControls();
            SearchBox.ConstructSearchBox();

            ConstructDataGridViews();
            SetCompanyLabel();
            LoadProducts();
            LoadSales();
            LoadPurchases();
            Sales_Button.PerformClick();
            LoadDataFromSetting();
            UpdateTheme();
        }
        private void SetCompanyLabel()
        {
            CompanyName_Label.Text = Directories.companyName;
            MoveEditButton();
        }
        private void MoveEditButton()
        {
            Edit_Button.Left = CompanyName_Label.Left + CompanyName_Label.Width + 5;
        }
        private void LoadProducts()
        {
            string[] lines = Directories.ReadAllLinesInFile(Directories.productPurchases_file);
            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                if (fields.Length >= 3)
                {
                    productPurchaseList.Add(new Product(fields[0], fields[1], fields[2]));
                }
            }

            lines = Directories.ReadAllLinesInFile(Directories.productSales_file);
            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                if (fields.Length >= 3)
                {
                    productSaleList.Add(new Product(fields[0], fields[1], fields[2]));
                }
            }
        }
        public void LoadSales()
        {
            selectedDataGridView = Sales_DataGridView;
            isDataGridViewLoading = true;

            LoadColumnsInDataGridView(selectedDataGridView, SalesColumnHeaders);
            Selected = Options.Sales;

            AddRowsFromFile();
            UpdateTotals();
            isDataGridViewLoading = false;
            AlignTotalLabels();
        }
        public void LoadPurchases()
        {
            selectedDataGridView = Purchases_DataGridView;
            isDataGridViewLoading = true;

            LoadColumnsInDataGridView(selectedDataGridView, PurchaseColumnHeaders);
            Selected = Options.Purchases;

            AddRowsFromFile();
            UpdateTotals();
            isDataGridViewLoading = false;
            AlignTotalLabels();
        }
        private void LoadGraphs()
        {
            double total;
            if (Selected == Options.Sales)
            {
                total = Bar.LoadTotalsIntoChart(Sales_DataGridView, Bar_GunaChart);
                Bar_Label.Text = $"Total revenue: ${total}";
                Pie.LoadDistributionIntoChart(Sales_DataGridView, Pie_GunaChart);
                Pie_Label.Text = "Distribution of revenue";
            }
            else
            {
                total = Bar.LoadTotalsIntoChart(Purchases_DataGridView, Bar_GunaChart);
                Bar_Label.Text = $"Total expenses ${total}";
                Pie.LoadDistributionIntoChart(Purchases_DataGridView, Pie_GunaChart);
                Pie_Label.Text = "Distribution of expenses";
            }
            total = Bar.LoadProfitsIntoChart(Sales_DataGridView, Purchases_DataGridView, Bar2_GunaChart);
            Bar2_Label.Text = $"Total profits: ${total}";
        }
        private bool DoNotUpdateTheme;
        private void LoadDataFromSetting()
        {
            if (CurrentTheme == ThemeType.Dark)
            {
                DoNotUpdateTheme = true;
                DarkMode_ToggleSwitch.Checked = true;
                DoNotUpdateTheme = false;
            }
        }
        private void UpdateTheme()
        {
            CustomColors.SetColors();
            Theme.SetThemeForForm(this);
            if (CurrentTheme == ThemeType.Dark)
            {
                Edit_Button.Image = Resources.EditWhite;
            }
            else
            {
                Edit_Button.Image = Resources.EditBlack;
            }

            MainTop_Panel.FillColor = CustomColors.background4;
            Edit_Button.FillColor = CustomColors.background4;
            Top_Panel.BackColor = CustomColors.background3;
            File_Button.FillColor = CustomColors.background3;
            Save_Button.FillColor = CustomColors.background3;
            Help_Button.FillColor = CustomColors.background3;
        }

        // Form
        private void MainMenu_form_Shown(object sender, EventArgs e)
        {
            Log.Write(2, "Argo Studio has finished starting");
        }
        private void MainMenu_form_Resize(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            ResizeControls();
        }
        private void ResizeControls()
        {
            Bar_GunaChart.Width = Width / 3 - 30;
            Bar_GunaChart.Left = 20;
            Bar_Label.Left = Bar_GunaChart.Left;

            Pie_GunaChart.Width = Bar_GunaChart.Width;
            Pie_GunaChart.Left = (Width / 2) - (Pie_GunaChart.Width / 2) - 8;
            Pie_Label.Left = Pie_GunaChart.Left;

            Bar2_GunaChart.Width = Bar_GunaChart.Width;
            Bar2_GunaChart.Left = Width - Bar_GunaChart.Width - 35;
            Bar2_Label.Left = Bar2_GunaChart.Left;

            selectedDataGridView.Width = Width - 50;
            selectedDataGridView.Left = 18;
            Total_Panel.Width = selectedDataGridView.Width;
            Total_Panel.Left = selectedDataGridView.Left;

            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
            }
        }
        private void MainMenu_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Write(2, "Closing Argo Studio");

            UI.CloseAllPanels(null, null);

            // Save logs in file
            if (Directory.Exists(Directories.logs_dir))
            {
                DateTime time = DateTime.Now;  // Get time
                int count = 0;
                string directory;

                while (true)
                {
                    if (count == 0)
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + ".txt";
                    else
                        directory = Directories.logs_dir + @"\" + time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + "-" + count + ".txt";

                    if (!Directory.Exists(directory))
                    {
                        Directories.WriteTextToFile(directory, Log.logText);
                        break;
                    }
                    count++;
                }
            }

            if (ArgoCompany.AreAnyChangesMade())
            {
                if (AskUserToSaveBeforeClosing()) { e.Cancel = true; return; }
            }

            // Delete hidden directory
            Directories.DeleteDirectory(Directories.tempCompany_dir, true);
        }

        /// <summary>
        /// Asks the user to save any changes.
        /// </summary>
        /// <returns>Returns true if the user cancels. Returns false if the user saves.</returns>
        private static bool AskUserToSaveBeforeClosing()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", "Save changes to the following items?", CustomMessageBoxIcon.None, CustomMessageBoxButtons.SaveDontSaveCancel);

            switch (result)
            {
                case CustomMessageBoxResult.Save:
                    ArgoCompany.SaveAll();
                    break;
                case CustomMessageBoxResult.DontSave:
                    // Do nothing so the temp directory is deleted
                    break;
                case CustomMessageBoxResult.Cancel:
                    // Cancel close
                    return true;
                default:  // If the CustomMessageBox was closed
                    return true;
            }

            return false;
        }

        // Keyboard shortcuts
        private void MainMenu_form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        if (e.Shift)  // Save as
                        {
                            ArgoCompany.SaveAs();
                        }
                        else  // Save
                        {
                            UI.SaveAll();
                        }
                        break;

                    case Keys.E:  // Export
                        new Export_Form().ShowDialog();
                        break;

                    case Keys.L:  // Open logs
                        OpenLogs();
                        break;
                }
            }
            else if (e.Alt & e.KeyCode == Keys.F4)  // Close program
            {
                if (AskUserToSaveBeforeClosing())
                {
                    e.Handled = true;
                }
            }
        }


        // TOP BAR
        // Don't initiate these yet because it resets every time a program is loaded
        public void SwitchMainForm(Form form, object btnSender)
        {
            Guna2Button btn = (Guna2Button)btnSender;
            // If the form is not already selected
            if (btn.FillColor != Color.FromArgb(15, 13, 74) & btn.FillColor != Color.Gray)
            {
                Main_Panel.Controls.Clear();
                form.TopLevel = false;
                form.Dock = DockStyle.Fill;
                Main_Panel.Controls.Add(form);
                form.Show();
            }
        }

        // File
        private void File_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.fileMenu))
            {
                Controls.Remove(UI.fileMenu);
                File_Button.Image = Resources.FileGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                File_Button.Image = Resources.FileWhite;
                UI.fileMenu.Location = new Point(File_Button.Left, 30);
                Controls.Add(UI.fileMenu);
                UI.fileMenu.BringToFront();
            }
        }

        // Save btn
        private void Save_Button_Click(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
            UI.SaveAll();
        }
        private void Save_Button_MouseDown(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveWhite;
        }
        private void Save_Button_MouseUp(object sender, MouseEventArgs e)
        {
            Save_Button.Image = Resources.SaveGray;
        }

        // Help
        private void Help_Button_Click(object sender, EventArgs e)
        {
            if (Controls.Contains(UI.helpMenu))
            {
                Controls.Remove(UI.helpMenu);
                Help_Button.Image = Resources.HelpGray;
            }
            else
            {
                UI.CloseAllPanels(null, null);
                Help_Button.Image = Resources.HelpWhite;
                UI.helpMenu.Location = new Point(Help_Button.Left - UI.helpMenu.Width + Help_Button.Width, 30);
                Controls.Add(UI.helpMenu);
                UI.helpMenu.BringToFront();
            }
        }


        // Controls
        private void Purchases_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            selectedDataGridView = Purchases_DataGridView;
            Main_Panel.Controls.Add(Purchases_DataGridView);
            CenterSelectedDataGridView();
            ResizeControls();
            Main_Panel.Controls.Remove(Sales_DataGridView);
            Selected = Options.Purchases;
            LoadGraphs();
            UpdateTotals();
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            selectedDataGridView = Sales_DataGridView;
            Main_Panel.Controls.Add(Sales_DataGridView);
            CenterSelectedDataGridView();
            ResizeControls();
            Main_Panel.Controls.Remove(Purchases_DataGridView);
            Selected = Options.Sales;
            LoadGraphs();
            UpdateTotals();
        }
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new AddPurchase_Form().ShowDialog();
        }
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            new AddSale_Form().ShowDialog();
        }
        public List<Product> productSaleList = [];
        public List<Product> productPurchaseList = [];
        public List<string> GetAllProductSaleNames()
        {
            return productSaleList.Select(s => s.ProductName).ToList();
        }
        public List<string> GetAllProductPurchaseNames()
        {
            return productPurchaseList.Select(p => p.ProductName).ToList();
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {
            new Products_Form().ShowDialog();
        }
        private void DarkMode_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (DoNotUpdateTheme) { return; }

            CloseAllPanels(null, null);

            if (CurrentTheme == ThemeType.Dark)
            {
                CurrentTheme = ThemeType.Light;
            }
            else
            {
                CurrentTheme = ThemeType.Dark;
            }
            UpdateTheme();
        }
        private void TimeRange_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
        }
        private void Edit_Button_Click(object sender, EventArgs e)
        {
            UI.rename_textBox.Text = CompanyName_Label.Text;
            UI.rename_textBox.Location = new Point(CompanyName_Label.Left, CompanyName_Label.Top + CompanyName_Label.Parent.Top - 1);
            UI.rename_textBox.Size = new Size(200, CompanyName_Label.Height + 2);
            UI.rename_textBox.Font = CompanyName_Label.Font;
            Controls.Add(UI.rename_textBox);
            UI.rename_textBox.Focus();
            UI.rename_textBox.SelectAll();
            UI.rename_textBox.BringToFront();
            MainTop_Panel.Controls.Remove(Edit_Button);
            MainTop_Panel.Controls.Remove(CompanyName_Label);
        }
        public void RenameCompany()
        {
            if (!Controls.Contains(UI.rename_textBox))
            {
                return;
            }
            Controls.Remove(UI.rename_textBox);
            MainTop_Panel.Controls.Add(Edit_Button);
            MainTop_Panel.Controls.Add(CompanyName_Label);

            if (UI.rename_textBox.Text == CompanyName_Label.Text)
            {
                return;
            }
            CompanyName_Label.Text = UI.rename_textBox.Text;

            string newDir = Directories.argoCompany_dir + "\\" + UI.rename_textBox.Text + ".ArgoCompany";
            Directories.MoveFile(Directories.argoCompany_file, newDir);
            Directories.argoCompany_file = newDir;

            newDir = Directories.appData_dir + UI.rename_textBox.Text;
            Directories.RenameFolder(Directories.tempCompany_dir, newDir);
            Directories.tempCompany_dir = newDir;

            UI.rename_textBox.Text = "";

            MoveEditButton();
        }


        // DataGridView
        public bool isDataGridViewLoading;
        public Options Selected;
        public enum Options
        {
            Purchases,
            Sales,
            ProductPurchases,
            ProductSales
        }
        public enum PurchaseColumns
        {
            PurchaseID,
            BuyerName,
            ItemName,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            TotalPrice
        }
        public enum SalesColumns
        {
            SalesID,
            CustomerName,
            ItemName,
            Date,
            Quantity,
            PricePerUnit,
            Shipping,
            Tax,
            TotalPrice
        }
        public readonly Dictionary<PurchaseColumns, string> PurchaseColumnHeaders = new()
        {
            { PurchaseColumns.PurchaseID, "Purchase ID" },
            { PurchaseColumns.BuyerName, "Buyer name" },
            { PurchaseColumns.ItemName, "Item name" },
            { PurchaseColumns.Date, "Date" },
            { PurchaseColumns.Quantity, "Quantity" },
            { PurchaseColumns.PricePerUnit, "Price per unit" },
            { PurchaseColumns.Shipping, "Shipping" },
            { PurchaseColumns.Tax, "Tax" },
            { PurchaseColumns.TotalPrice, "Total price" }
        };
        public readonly Dictionary<SalesColumns, string> SalesColumnHeaders = new()
        {
            { SalesColumns.SalesID, "Sales ID" },
            { SalesColumns.CustomerName, "Customer name" },
            { SalesColumns.ItemName, "Item name" },
            { SalesColumns.Date, "Date" },
            { SalesColumns.Quantity, "Quantity" },
            { SalesColumns.PricePerUnit, "Price per unit" },
            { SalesColumns.Shipping, "Shipping" },
            { SalesColumns.Tax, "Tax" },
            { SalesColumns.TotalPrice, "Total price" }
        };
        public Guna2DataGridView Purchases_DataGridView, Sales_DataGridView;
        public Guna2DataGridView selectedDataGridView;
        private void ConstructDataGridViews()
        {
            Purchases_DataGridView = new Guna2DataGridView();
            Size size = new(1300, 350);
            InitializeDataGridView(Purchases_DataGridView, size);

            Sales_DataGridView = new Guna2DataGridView();
            InitializeDataGridView(Sales_DataGridView, size);
        }
        public void InitializeDataGridView(Guna2DataGridView dataGridView, Size size)
        {
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView.ColumnHeadersHeight = 30;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.RowTemplate.Height = 25;
            dataGridView.RowTemplate.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dataGridView.Theme = CustomColors.dataGridViewTheme;
            dataGridView.BackgroundColor = CustomColors.controlBack;
            dataGridView.Anchor = AnchorStyles.Bottom;
            dataGridView.Size = size;
            dataGridView.Click += CloseAllPanels;
            dataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            dataGridView.RowsAdded += DataGridView_RowsAdded;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
            dataGridView.KeyDown += DataGridView_KeyDown;
        }
        public void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            AlignTotalLabels();
        }
        private void AddRowsFromFile()
        {
            string filePath = GetFilePathForDataGridView();

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            // Add all rows to dataGridView_list
            selectedDataGridView.Rows.Clear();

            string[] lines = Directories.ReadAllLinesInFile(filePath);
            foreach (string line in lines)
            {
                string[] cellValues = line.Split(',');
                selectedDataGridView.Rows.Add(cellValues);
            }
        }
        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SaveDataGridViewToFile();
            DataGridViewRowChanged();
        }
        private void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            SaveDataGridViewToFile();
            DataGridViewRowChanged();
        }
        private void DataGridViewRowChanged()
        {
            if (Selected == Options.Sales || Selected == Options.Purchases)
            {
                UpdateTotals();
                LoadGraphs();
            }
        }
        private void DataGridView_KeyDown(object? sender, KeyEventArgs e)
        {

        }
        private void SaveDataGridViewToFile()
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string filePath = GetFilePathForDataGridView();

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            List<string> linesInDataGridView = [];

            // Write all the rows in the DataGridView to file
            for (int i = 0; i < selectedDataGridView.Rows.Count; i++)
            {
                DataGridViewRow row = selectedDataGridView.Rows[i];
                List<string> cellValues = [];

                foreach (DataGridViewCell cell in row.Cells)
                {
                    cellValues.Add(cell.Value?.ToString() ?? string.Empty);
                }

                string line = string.Join(",", cellValues); // Join cell values with a comma
                linesInDataGridView.Add(line);
            }

            Directories.WriteLinesToFile(filePath, linesInDataGridView);
            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, $"{Selected} list");
        }
        public static void LoadColumnsInDataGridView<TEnum>(Guna2DataGridView dataGridView, Dictionary<TEnum, string> columnHeaders) where TEnum : Enum
        {
            foreach (var column in Enum.GetValues(typeof(TEnum)))
            {
                dataGridView.Columns.Add(column.ToString(), columnHeaders[(TEnum)column]);
            }

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.HeaderCell.Style.BackColor = CustomColors.background2;
                column.HeaderCell.Style.SelectionBackColor = CustomColors.background2;
            }
        }
        private void CenterSelectedDataGridView()
        {
            selectedDataGridView.Location = new Point((Width - selectedDataGridView.Width) / 2, Main_Panel.Height - selectedDataGridView.Height - 100);
            Total_Panel.Location = new Point(selectedDataGridView.Left, selectedDataGridView.Top + selectedDataGridView.Height + 10);
        }
        private void UpdateTotals()
        {
            if (isDataGridViewLoading || Selected == Options.ProductPurchases || Selected == Options.ProductSales)
            {
                return;
            }

            int totalQuantity = 0;
            decimal totalPrice = 0;
            decimal totalTax = 0;
            decimal totalShipping = 0;

            foreach (DataGridViewRow row in selectedDataGridView.Rows)
            {
                if (Selected == Options.Purchases)
                {
                    totalQuantity += Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                    totalPrice += Convert.ToDecimal(row.Cells[PurchaseColumns.TotalPrice.ToString()].Value);
                    totalTax += Convert.ToDecimal(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                    totalShipping += Convert.ToDecimal(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                }
                else
                {
                    totalQuantity += Convert.ToInt32(row.Cells[SalesColumns.Quantity.ToString()].Value);
                    totalPrice += Convert.ToDecimal(row.Cells[SalesColumns.TotalPrice.ToString()].Value);
                    totalTax += Convert.ToDecimal(row.Cells[SalesColumns.Tax.ToString()].Value);
                    totalShipping += Convert.ToDecimal(row.Cells[SalesColumns.Shipping.ToString()].Value);
                }
            }

            Quantity_Label.Text = $"Quantity: {totalQuantity}";
            Tax_Label.Text = $"Tax: {totalTax:C}";
            Shipping_Label.Text = $"Shipping: {totalShipping:C}";
            Price_Label.Text = $"Price: {totalPrice:C}";
        }
        private void AlignTotalLabels()
        {
            if (isDataGridViewLoading)
            {
                return;
            }

            string quantityColumn, taxColumn, shippingColumn, totalPriceColumn;

            if (Selected == Options.Purchases)
            {
                quantityColumn = PurchaseColumns.Quantity.ToString();
                taxColumn = PurchaseColumns.Tax.ToString();
                shippingColumn = PurchaseColumns.Shipping.ToString();
                totalPriceColumn = PurchaseColumns.TotalPrice.ToString();
            }
            else
            {
                quantityColumn = SalesColumns.Quantity.ToString();
                taxColumn = SalesColumns.Tax.ToString();
                shippingColumn = SalesColumns.Shipping.ToString();
                totalPriceColumn = SalesColumns.TotalPrice.ToString();
            }

            Quantity_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = selectedDataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = selectedDataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = selectedDataGridView.Columns[shippingColumn].Width;

            Price_Label.Left = selectedDataGridView.GetCellDisplayRectangle(selectedDataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = selectedDataGridView.Columns[totalPriceColumn].Width;
        }
        private string GetFilePathForDataGridView()
        {
            if (Selected == Options.Purchases)
            {
                return Directories.purchases_file;
            }
            else if (Selected == Options.Sales)
            {
                return Directories.sales_file;
            }
            else if (Selected == Options.ProductPurchases)
            {
                return Directories.productPurchases_file;
            }
            else if (Selected == Options.ProductSales)
            {
                return Directories.productSales_file;
            }
            return "";
        }


        // Message panel
        public Guna2Panel messagePanel;
        public void ConstructMessage_Panel()
        {
            messagePanel = new Guna2Panel
            {
                Size = new Size(500, 150),
                FillColor = CustomColors.mainBackground,
                BackColor = Color.Transparent,
                BorderThickness = 1,
                BorderRadius = 5,
                BorderColor = CustomColors.controlPanelBorder
            };

            Label messageLabel = new()
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(10, 10),
                Size = new Size(480, 85),
                Name = "label",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = CustomColors.text
            };
            messageLabel.TextChanged += (sender, e) =>
            {
                if (messageLabel.Text != "")
                {
                    messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
                    Controls.Add(messagePanel);
                    messagePanel.BringToFront();
                    // Restart timer
                    MessagePanel_timer.Enabled = false;
                    MessagePanel_timer.Enabled = true;
                }
            };
            messagePanel.Controls.Add(messageLabel);

            Guna2Button gBtn = new()
            {
                Font = new Font("Segoe UI", 11),
                Text = "Ok",
                Size = new Size(120, 35),
                Location = new Point(190, 100),
                FillColor = Color.FromArgb(58, 153, 236),  // Blue
                ForeColor = Color.White
            };
            gBtn.Click += MessagePanelClose;
            messagePanel.Controls.Add(gBtn);

            PictureBox picture = new()
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, 10),
                Size = new Size(15, 15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Image = Resources.CloseGrey,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            picture.Click += MessagePanelClose;
            messagePanel.Controls.Add(picture);
        }
        private void SetMessage(string text)
        {
            Label label = (Label)messagePanel.Controls.Find("label", false).FirstOrDefault();
            label.Text = text;
        }
        private void MessagePanelClose(object sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }
        private void MessagePanelTimer_Tick(object sender, EventArgs e)
        {
            Controls.Remove(messagePanel);
            MessagePanel_timer.Enabled = false;
            // Reset in case the next message is the same
            SetMessage("");
        }


        private Log_Form LogForm;
        public void OpenLogs()
        {
            if (!Tools.IsFormOpen(typeof(Log_Form)))
            {
                LogForm = new Log_Form();
                LogForm.Show();
            }
            else
            {
                LogForm.BringToFront();
            }
        }

        // Misc.
        private void CloseAllPanels(object? sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}