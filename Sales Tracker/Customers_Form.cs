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
        private Label _emailError_Label;
        private CountryCode _selectedCountryCode;

        // Getters
        public static List<string> ThingsThatHaveChangedInFile { get; } = [];
        public static Customers_Form Instance => _instance;

        public Customers_Form()
        {
            InitializeComponent();
            _instance = this;

            _oldOption = MainMenu_Form.Instance.Selected;
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
            ConstructEmailErrorLabel();
            InitializeCountryCodeSearchBox();

            PanelCloseFilter panelCloseFilter = new(this, ClosePanels,
                TextBoxManager.RightClickTextBox_Panel,
                SearchBox.SearchResultBoxContainer,
                RightClickDataGridViewRowMenu.Panel);

            Application.AddMessageFilter(panelCloseFilter);

            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void ConstructEmailErrorLabel()
        {
            _emailError_Label = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = CustomColors.AccentRed,
                Text = "Please enter a valid email address",
                Visible = false,
                Location = new Point(Email_TextBox.Left, Email_TextBox.Bottom + 3),
                Name = "EmailError_Label"
            };
            Controls.Add(_emailError_Label);
        }
        private void InitializeCountryCodeSearchBox()
        {
            // Attach SearchBox with country codes and flags
            float scale = DpiHelper.GetRelativeDpiScale();
            int searchBoxMaxHeight = (int)(255 * scale);
            SearchBox.Attach(CountryCode_TextBox, this, CountryCode.GetCountryCodeSearchResults, searchBoxMaxHeight, false, true, true, false);

            // Wire up event handler for text changes
            CountryCode_TextBox.TextChanged += CountryCode_TextBox_TextChanged;
            TextBoxManager.Attach(CountryCode_TextBox);
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
            Email_TextBox.Enter += Email_TextBox_Enter;
            TextBoxManager.Attach(Email_TextBox);

            PhoneNumber_TextBox.TextChanged += PhoneNumber_TextBox_TextChanged;
            PhoneNumber_TextBox.TextChanged += ValidateInputs;
            TextBoxValidation.ValidatePhoneNumber(PhoneNumber_TextBox);
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
        private void Email_TextBox_Enter(object sender, EventArgs e)
        {
            // Hide error label and reset border when user enters the text box to start editing
            _emailError_Label.Visible = false;
            Email_TextBox.BorderColor = CustomColors.ControlBorder;
        }
        private void Email_TextBox_Leave(object sender, EventArgs e)
        {
            string email = Email_TextBox.Text.Trim();

            // Only show error and red border if field has content and is invalid
            if (!string.IsNullOrWhiteSpace(email) && !TextBoxValidation.IsValidEmail(email))
            {
                _emailError_Label.Visible = true;
                Email_TextBox.BorderColor = CustomColors.AccentRed;
            }
            else
            {
                _emailError_Label.Visible = false;
                Email_TextBox.BorderColor = CustomColors.ControlBorder;
            }
        }

        private void CountryCode_TextBox_TextChanged(object sender, EventArgs e)
        {
            // Parse the country code from the search result text
            CountryCode? selectedCountry = CountryCode.GetCountryCodeFromText(CountryCode_TextBox.Text);
            if (selectedCountry != null)
            {
                _selectedCountryCode = selectedCountry;

                // Reformat the existing phone number with the new country format
                if (!string.IsNullOrWhiteSpace(PhoneNumber_TextBox.Text))
                {
                    FormatPhoneNumber();
                }
            }
        }
        private void PhoneNumber_TextBox_TextChanged(object sender, EventArgs e)
        {
            FormatPhoneNumber();
        }
        private void AddCustomer_Button_Click(object sender, EventArgs e)
        {
            string customerID = CustomerID_TextBox.Text.Trim();
            if (customerID == "")
            {
                customerID = ReadOnlyVariables.EmptyCell;
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

                if (result != CustomMessageBoxResult.Yes) { return; }
            }

            // Get first and last name
            string firstName = FirstName_TextBox.Text.Trim();
            string lastName = LastName_TextBox.Text.Trim();

            // Create new customer
            string phoneNumber = PhoneNumber_TextBox.Text.Trim();
            if (_selectedCountryCode != null && !string.IsNullOrWhiteSpace(phoneNumber))
            {
                phoneNumber = $"{_selectedCountryCode.Code} {phoneNumber}";
            }

            Customer customer = new(
                customerID,
                firstName,
                lastName,
                Email_TextBox.Text,
                phoneNumber,
                Address_TextBox.Text.Trim())
            {
                Notes = Notes_TextBox.Text.Trim()
            };

            // Add to list
            MainMenu_Form.Instance.CustomerList.Add(customer);

            // Add to DataGridView
            int newRowIndex = _customers_DataGridView.Rows.Add(
                customer.CustomerID,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.Address,
                customer.CurrentPaymentStatus.ToString(),
                customer.OutstandingBalance,
                customer.IsBanned,
                customer.RentalRecords.Count,
                customer.LastRentalDate?.ToString("yyyy-MM-dd") ?? "-");

            _customers_DataGridView.Rows[newRowIndex].Tag = customer;
            DataGridViewManager.DataGridViewRowsAdded(_customers_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            // Save
            MainMenu_Form.Instance.SaveCustomersToFile();

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
            FirstName,
            LastName,
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
            { Column.FirstName, "First name" },
            { Column.LastName, "Last name" },
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
            int topForDataGridView = ShowingResultsFor_Label.Bottom + 20;

            _customers_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(_customers_DataGridView, "customers_DataGridView", ColumnHeaders, null, this);
            _customers_DataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            _customers_DataGridView.Location = new Point((ClientSize.Width - _customers_DataGridView.Width) / 2, topForDataGridView);
            _customers_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            _customers_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Customer;
            _customers_DataGridView.CellFormatting += DataGridView_CellFormatting;
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
                    customer.FirstName,
                    customer.LastName,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.Address,
                    customer.CurrentPaymentStatus.ToString(),
                    customer.OutstandingBalance,
                    customer.IsBanned,
                    customer.RentalRecords.Count,
                    customer.LastRentalDate?.ToString("yyyy-MM-dd") ?? "-");

                _customers_DataGridView.Rows[rowIndex].Tag = customer;
            }
            DataGridViewManager.ScrollToTopOfDataGridView(_customers_DataGridView);
        }

        // Methods
        private void FormatPhoneNumber()
        {
            if (_selectedCountryCode == null)
            {
                return;
            }

            int cursorPosition = PhoneNumber_TextBox.SelectionStart;
            string currentText = PhoneNumber_TextBox.Text;

            // Extract only digits from current text
            string digitsOnly = new(currentText.Where(char.IsDigit).ToArray());

            // Count maximum allowed digits based on format (count 'X' characters)
            int maxDigits = _selectedCountryCode.Format.Count(c => c == 'X');

            // Limit digits to the format's maximum
            if (digitsOnly.Length > maxDigits)
            {
                digitsOnly = digitsOnly.Substring(0, maxDigits);
            }

            // Format the phone number
            string formattedNumber = _selectedCountryCode.FormatPhoneNumber(digitsOnly);

            if (currentText != formattedNumber)
            {
                PhoneNumber_TextBox.TextChanged -= PhoneNumber_TextBox_TextChanged;
                PhoneNumber_TextBox.Text = formattedNumber;

                // Adjust cursor position
                int newCursorPosition = Math.Min(cursorPosition + (formattedNumber.Length - currentText.Length), formattedNumber.Length);
                PhoneNumber_TextBox.SelectionStart = Math.Max(0, newCursorPosition);

                PhoneNumber_TextBox.TextChanged += PhoneNumber_TextBox_TextChanged;
            }
        }
        private static bool IsEmailDuplicate(string email)
        {
            return MainMenu_Form.Instance.CustomerList.Any(c =>
                c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            // Only require first and last name
            bool requiredFieldsFilled = !string.IsNullOrWhiteSpace(FirstName_TextBox.Text) &&
                                        !string.IsNullOrWhiteSpace(LastName_TextBox.Text);

            // If email is provided, it must be valid
            bool emailValid = true;
            if (!string.IsNullOrWhiteSpace(Email_TextBox.Text))
            {
                emailValid = TextBoxValidation.IsValidEmail(Email_TextBox.Text.Trim());
            }

            AddCustomer_Button.Enabled = requiredFieldsFilled && emailValid;
        }
        private void ClosePanels()
        {
            SearchBox.Close();
            TextBoxManager.HideRightClickPanel();
            RightClickDataGridViewRowMenu.Hide();
        }
    }
}