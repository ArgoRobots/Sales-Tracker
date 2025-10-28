using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.GridView;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Customers_Form : BaseForm
    {
        private static Customers_Form _instance;
        private static bool _isProgramLoading;
        private readonly MainMenu_Form.SelectedOption _oldOption;
        private readonly int _topForDataGridView;

        // Getters
        public static List<string> ThingsThatHaveChangedInFile { get; } = [];
        public static Customers_Form Instance => _instance;

        public Customers_Form() : this(false) { }
        public Customers_Form(bool loadExisting)
        {
            InitializeComponent();
            _instance = this;

            _oldOption = MainMenu_Form.Instance.Selected;
            _topForDataGridView = ShowingResultsFor_Label.Bottom + 20;
            AddSearchBoxEvents();

            _isProgramLoading = true;
            ConstructDataGridView();
            LoadCustomers();
            _isProgramLoading = false;

            ValidateInputs(null, null);
            ThemeManager.SetThemeForForm(this);
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            ShowingResultsFor_Label.Visible = false;
            LanguageManager.UpdateLanguageForControl(this);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(_customers_DataGridView);
            AddEventHandlersToTextBoxes();

            PanelCloseFilter panelCloseFilter = new(this, ClosePanels,
                TextBoxManager.RightClickTextBox_Panel,
                SearchBox.SearchResultBoxContainer,
                RightClickDataGridViewRowMenu.Panel);

            Application.AddMessageFilter(panelCloseFilter);

            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(CustomerID_TextBox);
            
            TextBoxValidation.OnlyAllowLetters(FirstName_TextBox);
            FirstName_TextBox.TextChanged += ValidateInputs;
            TextBoxManager.Attach(FirstName_TextBox);
            
            TextBoxValidation.OnlyAllowLetters(LastName_TextBox);
            LastName_TextBox.TextChanged += ValidateInputs;
            TextBoxManager.Attach(LastName_TextBox);
            
            TextBoxValidation.ValidateEmail(Email_TextBox);
            Email_TextBox.TextChanged += ValidateInputs;
            Email_TextBox.Leave += Email_TextBox_Leave;
            TextBoxManager.Attach(Email_TextBox);
            
            TextBoxValidation.ValidatePhoneNumber(PhoneNumber_TextBox);
            PhoneNumber_TextBox.TextChanged += ValidateInputs;
            TextBoxManager.Attach(PhoneNumber_TextBox);
            
            TextBoxManager.Attach(Address_TextBox);
            TextBoxManager.Attach(Notes_TextBox);
            TextBoxManager.Attach(Search_TextBox);

            _customers_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _customers_DataGridView);
            _customers_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, _customers_DataGridView);
        }

        private void AddSearchBoxEvents()
        {
            Search_TextBox.TextChanged += (_, _) =>
            {
                string searchText = Search_TextBox.Text.Trim();
                bool hasVisibleRows = false;

                foreach (DataGridViewRow row in _customers_DataGridView.Rows)
                {
                    row.Visible = SearchDataGridView.FilterRowByAdvancedSearch(row, searchText);
                    if (row.Visible) hasVisibleRows = true;
                }

                if (hasVisibleRows && !string.IsNullOrEmpty(searchText))
                {
                    LabelManager.ShowLabelWithBaseText(ShowingResultsFor_Label, searchText);
                }
                else
                {
                    ShowingResultsFor_Label.Visible = false;
                }

                DataGridViewManager.UpdateRowColors(_customers_DataGridView);
                LabelManager.ShowTotalLabel(Total_Label, _customers_DataGridView);
            };
        }

        private void SetAccessibleDescriptions()
        {
            CustomerID_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            FirstName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            LastName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            Email_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            PhoneNumber_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            Address_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            Notes_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            WarningCustomerName_Label.AccessibleDescription = AccessibleDescriptionManager.AlignLeft;
            Total_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotCache;
        }

        // Form event handlers
        private void Customers_Form_Shown(object sender, EventArgs e)
        {
            _customers_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void Email_TextBox_Leave(object sender, EventArgs e)
        {
            string email = Email_TextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }
            
            // Check if email is valid
            if (!email.Contains('@'))
            {
                CustomMessageBox.Show("Invalid email",
                    "Please enter a valid email address.",
                    CustomMessageBoxIcon.Warning,
                    CustomMessageBoxButtons.OK);
                Email_TextBox.Focus();
            }
        }

        private void AddCustomer_Button_Click(object sender, EventArgs e)
        {
            string customerID = CustomerID_TextBox.Text.Trim();
            if (customerID == "")
            {
                customerID = ReadOnlyVariables.EmptyCell;
            }

            // Validate email before proceeding
            string email = Email_TextBox.Text.Trim();
            if (!email.Contains('@'))
            {
                CustomMessageBox.Show("Invalid email",
                    "Please enter a valid email address.",
                    CustomMessageBoxIcon.Warning,
                    CustomMessageBoxButtons.OK);
                Email_TextBox.Focus();
                return;
            }

            // Check for duplicate email
            if (IsEmailDuplicate(email))
            {
                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat("Duplicate email",
                    "Email '{0}' already exists. Add customer anyway?",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.YesNo,
                    email);
        
                if (result != CustomMessageBoxResult.Yes)
                {
                    Email_TextBox.Focus();
                    return;
                }
            }

            // Check if customer ID already exists
            if (customerID != ReadOnlyVariables.EmptyCell &&
                DataGridViewManager.DoesValueExistInDataGridView(_customers_DataGridView, Column.CustomerID.ToString(), customerID))
            {
                CustomMessageBoxResult result = CustomMessageBox.ShowWithFormat("Customer already exists",
                    "The customer #{0} already exists. Would you like to add this customer anyway?",
                    CustomMessageBoxIcon.Question,
                    CustomMessageBoxButtons.YesNo,
                    customerID);

                if (result != CustomMessageBoxResult.Yes) return;
            }

            // Combine first and last name
            string firstName = FirstName_TextBox.Text.Trim();
            string lastName = LastName_TextBox.Text.Trim();
            string fullName = $"{firstName} {lastName}".Trim();

            // Create new customer
            Customer customer = new(
                customerID,
                fullName,
                email,
                PhoneNumber_TextBox.Text.Trim(),
                Address_TextBox.Text.Trim())
            {
                Notes = Notes_TextBox.Text.Trim()
            };

            // Add to list
            MainMenu_Form.Instance.CustomerList.Add(customer);

            // Add to DataGridView
            int newRowIndex = _customers_DataGridView.Rows.Add(
                customer.CustomerID,
                customer.Name,
                customer.Email,
                customer.PhoneNumber,
                customer.Address,
                customer.CurrentPaymentStatus.ToString(),
                customer.OutstandingBalance,
                customer.IsBanned,  
                customer.RentalHistory.Count,
                customer.LastRentalDate?.ToString("yyyy-MM-dd") ?? "-");

            _customers_DataGridView.Rows[newRowIndex].Tag = customer;
            DataGridViewManager.DataGridViewRowsAdded(_customers_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            // Save
            SaveCustomersToFile();

            string message = $"Added customer '{customer.Name}'";
            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(ThingsThatHaveChangedInFile, 4, message);

            // Clear inputs
            FirstName_TextBox.Clear();
            LastName_TextBox.Clear();
            CustomerID_TextBox.Clear();
            Email_TextBox.Clear();
            PhoneNumber_TextBox.Clear();
            Address_TextBox.Clear();
            Notes_TextBox.Clear();
        }

        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Clear();
        }

        // DataGridView properties
        public enum Column
        {
            CustomerID,
            CustomerName,
            Email,
            PhoneNumber,
            Address,
            PaymentStatus,
            OutstandingBalance,
            IsBanned,
            TotalRentals,
            LastRentalDate
        }

        public static readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.CustomerID, "Customer ID" },
            { Column.CustomerName, "Customer name" },
            { Column.Email, "Email" },
            { Column.PhoneNumber, "Phone number" },
            { Column.Address, "Address" },
            { Column.PaymentStatus, "Payment status" },
            { Column.OutstandingBalance, "Outstanding balance" },
            { Column.IsBanned, "Banned" },
            { Column.TotalRentals, "Total rentals" },
            { Column.LastRentalDate, "Last rental date" }
        };

        private Guna2DataGridView _customers_DataGridView;
        public Guna2DataGridView Customers_DataGridView => _customers_DataGridView;

        // DataGridView methods
        private void ConstructDataGridView()
        {
            _customers_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_customers_DataGridView, "customers_DataGridView", ColumnHeaders, null, this);
            _customers_DataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - _topForDataGridView - 70);
            _customers_DataGridView.Location = new Point((ClientSize.Width - _customers_DataGridView.Width) / 2, _topForDataGridView);
            _customers_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            _customers_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Customer;
            _customers_DataGridView.CellFormatting += DataGridView_CellFormatting;
            
            _customers_DataGridView.Columns[Column.IsBanned.ToString()].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _customers_DataGridView.Columns[Column.IsBanned.ToString()].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            // Color code payment status
            if (grid.Columns[e.ColumnIndex].Name == Column.PaymentStatus.ToString())
            {
                if (e.Value?.ToString() == "Current")
                {
                    e.CellStyle.ForeColor = CustomColors.AccentGreen;
                }
                else if (e.Value?.ToString() == "Overdue")
                {
                    e.CellStyle.ForeColor = Color.Orange;
                }
                else if (e.Value?.ToString() == "Delinquent")
                {
                    e.CellStyle.ForeColor = CustomColors.AccentRed;
                }
            }

            if (grid.Columns[e.ColumnIndex].Name == Column.IsBanned.ToString())
            {
                if (e.Value is bool isBanned)
                {
                    if (isBanned)
                    {
                        e.Value = "✓";
                        e.CellStyle.ForeColor = CustomColors.AccentGreen;
                    }
                    else
                    {
                        e.Value = "✗";
                        e.CellStyle.ForeColor = CustomColors.AccentRed;
                    }
                    
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    e.FormattingApplied = true;
                }
            }
        }

        private void LoadCustomers()
        {
            foreach (Customer customer in MainMenu_Form.Instance.CustomerList)
            {
                int rowIndex = _customers_DataGridView.Rows.Add(
                    customer.CustomerID,
                    customer.Name,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.Address,
                    customer.CurrentPaymentStatus.ToString(),
                    customer.OutstandingBalance,
                    customer.IsBanned,  
                    customer.RentalHistory.Count,
                    customer.LastRentalDate?.ToString("yyyy-MM-dd") ?? "-");

                _customers_DataGridView.Rows[rowIndex].Tag = customer;
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_customers_DataGridView);
        }

        // Methods
        private bool IsEmailDuplicate(string email)
        {
            return MainMenu_Form.Instance.CustomerList.Any(c => 
                c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(FirstName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(LastName_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Email_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PhoneNumber_TextBox.Text);

            // Check if email is valid 
            bool emailValid = Email_TextBox.Text.Trim().Contains('@');
            AddCustomer_Button.Enabled = allFieldsFilled && emailValid;
        }

        private static void SaveCustomersToFile()
        {
            MainMenu_Form.Instance.SaveCustomersToFile();
        }

        private void ClosePanels()
        {
            SearchBox.Close();
            TextBoxManager.HideRightClickPanel();
            RightClickDataGridViewRowMenu.Hide();
        }
    }
}