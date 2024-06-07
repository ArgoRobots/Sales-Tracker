using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
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

            SetPropertiesForDataGridView();
            SetCompanyLabel();
            UpdateTheme();
        }
        private void SetPropertiesForDataGridView()
        {
            Items_DataGridView.ReadOnly = true;
            Items_DataGridView.AllowUserToAddRows = false;
            Items_DataGridView.AllowUserToResizeRows = false;
            Items_DataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            Items_DataGridView.RowTemplate.Height = 25;
            Items_DataGridView.ColumnHeadersHeight = 30;
            Items_DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // Disable header height resizing
            Items_DataGridView.RowTemplate.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            Items_DataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            Items_DataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            Items_DataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            Items_DataGridView.Theme = CustomColors.dataGridViewTheme;
        }

        private void SetCompanyLabel()
        {
            CompanyName_Label.Text = Directories.companyName;
            Edit_Button.Left = CompanyName_Label.Left + CompanyName_Label.Width;
        }
        public void UpdateTheme()
        {
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

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
            if (Controls.Contains(messagePanel))
            {
                messagePanel.Location = new Point((Width - messagePanel.Width) / 2, Height - messagePanel.Height - 80);
            }
        }
        private void MainMenu_form_ResizeBegin(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
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
            Directories.DeleteDirectory(Directories.company_dir, true);
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
                    case Keys.S:  // Save
                        if (e.Shift)
                        {
                            ArgoCompany.SaveAll();
                        }
                        else  // Save as
                        {
                            ArgoCompany.SaveAs();
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

        // Cascading menus
        Guna2Panel menuToHide;
        private void HideMenu_timer_Tick(object sender, EventArgs e)
        {
            menuToHide.Parent?.Controls.Remove(menuToHide);
            HideMenu_timer.Enabled = false;
        }
        public void OpenMenu()
        {
            HideMenu_timer.Enabled = false;
        }
        public void CloseMenu(object sender, EventArgs e)
        {
            Guna2Button btn = (Guna2Button)sender;
            menuToHide = (Guna2Panel)btn.Tag;
            HideMenu_timer.Enabled = false;
            HideMenu_timer.Enabled = true;
        }
        public void KeepMenuOpen(object sender, EventArgs e)
        {
            HideMenu_timer.Enabled = false;
        }


        // TOP BAR
        // Don't initiate these yet because it resets every time a program is loaded
        public Products_Form formProducts;

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
            Guna2Button saveBtn = (Guna2Button)UI.fileMenu.Controls[0].Controls.Find("Save", false).FirstOrDefault();
            saveBtn.PerformClick();
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
            LoadPurchases();
        }
        private void Sales_Button_Click(object sender, EventArgs e)
        {
            LoadSales();
        }
        private void ManageProducts_Button_Click(object sender, EventArgs e)
        {

        }
        private void DarkMode_ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void TimeRange_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void Edit_Button_Click(object sender, EventArgs e)
        {

        }


        // DataGridView
        private bool isDataGridViewLoading;
        private enum Options
        {
            Purchases,
            Sales
        }
        private Options Selected;
        private enum PurchaseColumns
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
        private enum SalesColumns
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
        private readonly Dictionary<PurchaseColumns, string> PurchaseColumnHeaders = new()
        {
            { PurchaseColumns.PurchaseID, "Purchase ID" },
            { PurchaseColumns.BuyerName, "Buyer Name" },
            { PurchaseColumns.ItemName, "Item Name" },
            { PurchaseColumns.Date, "Date" },
            { PurchaseColumns.Quantity, "Quantity" },
            { PurchaseColumns.PricePerUnit, "Price Per Unit" },
            { PurchaseColumns.Shipping, "Shipping" },
            { PurchaseColumns.Tax, "Tax" },
            { PurchaseColumns.TotalPrice, "Total Price" }
        };

        private readonly Dictionary<SalesColumns, string> SalesColumnHeaders = new()
        {
            { SalesColumns.SalesID, "Sales ID" },
            { SalesColumns.CustomerName, "Customer Name" },
            { SalesColumns.ItemName, "Item Name" },
            { SalesColumns.Date, "Date" },
            { SalesColumns.Quantity, "Quantity" },
            { SalesColumns.PricePerUnit, "Price Per Unit" },
            { SalesColumns.Shipping, "Shipping" },
            { SalesColumns.Tax, "Tax" },
            { SalesColumns.TotalPrice, "Total Price" }
        };

        private void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
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
            Items_DataGridView.Rows.Clear();

            string[] lines = Directories.ReadAllLinesInFile(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                Items_DataGridView.Rows.Add(lines[i]);
            }
        }
        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SaveDataGridViewToFile();
        }
        private void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            SaveDataGridViewToFile();
        }
        private void SaveDataGridViewToFile()
        {
            string filePath = GetFilePathForDataGridView();

            if (!File.Exists(filePath))
            {
                Log.Error_FailedToWriteToFile(Directories.companyName);
                return;
            }

            List<string> linesInDataGridView = [];

            // Write all the rows in the dataGridView_list in file
            for (int i = 0; i < Items_DataGridView.Rows.Count; i++)
            {
                linesInDataGridView.Add(Items_DataGridView.Rows[i].Cells[0].Value.ToString());
            }
            Directories.WriteLinesToFile(filePath, linesInDataGridView);
            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, $"{Selected} list");
        }
        private void LoadPurchases()
        {
            isDataGridViewLoading = true;
            Items_DataGridView.Columns.Clear();
            Items_DataGridView.Rows.Clear();

            foreach (var column in Enum.GetValues(typeof(PurchaseColumns)))
            {
                Items_DataGridView.Columns.Add(column.ToString(), PurchaseColumnHeaders[(PurchaseColumns)column]);
            }

            Items_DataGridView.Rows.Add("P001", "Laptop", "2", "1000", "2000", "200", "50");
            Selected = Options.Purchases;

            AddRowsFromFile();
            FilterData();
            UpdateTotals();
            isDataGridViewLoading = false;
            AlignTotalLabels();
        }
        private void LoadSales()
        {
            isDataGridViewLoading = true;
            Items_DataGridView.Columns.Clear();
            Items_DataGridView.Rows.Clear();

            foreach (var column in Enum.GetValues(typeof(SalesColumns)))
            {
                Items_DataGridView.Columns.Add(column.ToString(), SalesColumnHeaders[(SalesColumns)column]);
            }

            Items_DataGridView.Rows.Add("S001", "John Doe", "Laptop", "1", "1200", "1200", "120", "30");
            Selected = Options.Sales;

            AddRowsFromFile();
            FilterData();
            UpdateTotals();
            AlignTotalLabels();
            isDataGridViewLoading = false;
        }
        private void UpdateTotals()
        {
            int totalQuantity = 0;
            decimal totalPrice = 0;
            decimal totalTax = 0;
            decimal totalShipping = 0;

            foreach (DataGridViewRow row in Items_DataGridView.Rows)
            {
                if (Selected == Options.Purchases)
                {
                    totalQuantity += Convert.ToInt32(row.Cells[PurchaseColumns.Quantity.ToString()].Value);
                    totalPrice += Convert.ToDecimal(row.Cells[PurchaseColumns.TotalPrice.ToString()].Value);
                    totalTax += Convert.ToDecimal(row.Cells[PurchaseColumns.Tax.ToString()].Value);
                    totalShipping += Convert.ToDecimal(row.Cells[PurchaseColumns.Shipping.ToString()].Value);
                }
                else if (Selected == Options.Sales)
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

            Quantity_Label.Left = Items_DataGridView.GetCellDisplayRectangle(Items_DataGridView.Columns[quantityColumn].Index, -1, true).Left;
            Quantity_Label.Width = Items_DataGridView.Columns[quantityColumn].Width;

            Tax_Label.Left = Items_DataGridView.GetCellDisplayRectangle(Items_DataGridView.Columns[taxColumn].Index, -1, true).Left;
            Tax_Label.Width = Items_DataGridView.Columns[taxColumn].Width;

            Shipping_Label.Left = Items_DataGridView.GetCellDisplayRectangle(Items_DataGridView.Columns[shippingColumn].Index, -1, true).Left;
            Shipping_Label.Width = Items_DataGridView.Columns[shippingColumn].Width;

            Price_Label.Left = Items_DataGridView.GetCellDisplayRectangle(Items_DataGridView.Columns[totalPriceColumn].Index, -1, true).Left;
            Price_Label.Width = Items_DataGridView.Columns[totalPriceColumn].Width;
        }
        private void FilterData()
        {
        }
        private string GetFilePathForDataGridView()
        {
            if (Selected == Options.Purchases)
            {
                return Directories.purchases_file;
            }
            else
            {
                return Directories.sales_file;
            }
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
        private void CloseAllPanels(object sender, EventArgs e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}