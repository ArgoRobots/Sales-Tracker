using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class AddPurchase_Form : Form
    {
        // Properties
        private static List<string> _thingsThatHaveChangedInFile = [];

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile
        {
            get => _thingsThatHaveChangedInFile;
            private set => _thingsThatHaveChangedInFile = value;
        }

        // Init.
        public AddPurchase_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            Date_DateTimePicker.Value = DateTime.Now;
            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            Currency_ComboBox.Text = Properties.Settings.Default.Currency;
            CheckIfProductsExist();
            CheckIfBuyersExist();
            Theme.SetThemeForForm(this);
            RemoveReceiptLabel();
            Charged_Label.Text = $"{MainMenu_Form.CurrencySymbol} charged ({Properties.Settings.Default.Currency})";
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(OrderNumber_TextBox);

            AccountantName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(AccountantName_TextBox);

            TextBoxManager.Attach(ProductName_TextBox);

            Quantity_TextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
            TextBoxManager.Attach(Quantity_TextBox);

            PricePerUnit_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(PricePerUnit_TextBox);

            Shipping_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(Shipping_TextBox);

            Tax_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(Tax_TextBox);

            PaymentFee_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(PaymentFee_TextBox);

            Discount_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(Discount_TextBox);

            Charged_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(Charged_TextBox);

            TextBoxManager.Attach(Notes_TextBox);
        }
        private void AddSearchBoxEvents()
        {
            byte searchBoxMaxHeight = 255;

            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList);
            SearchBox.Attach(AccountantName_TextBox, this, () => searchResult, searchBoxMaxHeight);
            AccountantName_TextBox.TextChanged += ValidateInputs;

            List<SearchResult> searchResult1 = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames());
            SearchBox.Attach(ProductName_TextBox, this, () => searchResult1, searchBoxMaxHeight);
            ProductName_TextBox.TextChanged += ValidateInputs;
        }

        // Form event handlers
        private void AddPurchase_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
            OrderNumber_TextBox.Focus();
        }

        // Event handlers
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (MainMenu_Form.Instance.Selected != MainMenu_Form.SelectedOption.Purchases)
            {
                MainMenu_Form.Instance.Purchases_Button.PerformClick();
            }
            MainMenu_Form.Instance.selectedDataGridView = MainMenu_Form.Instance.Purchases_DataGridView;

            if (panelsForMultipleProducts_List.Count == 0 || !MultipleItems_CheckBox.Checked)
            {
                if (!AddSinglePurchase()) { return; }
            }
            // When the user selects "multiple items in this order" but only adds one, treat it as one
            else if (panelsForMultipleProducts_List.Count == 1)
            {
                // Extract details from the single panel and populate the single purchase fields
                Guna2Panel singlePanel = panelsForMultipleProducts_List[0];
                ProductName_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault()).Text;
                Quantity_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.quantity.ToString(), false).FirstOrDefault()).Text;
                PricePerUnit_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.pricePerUnit.ToString(), false).FirstOrDefault()).Text;

                if (!AddSinglePurchase()) { return; }
            }
            else if (!AddMultiplePurchases())
            {
                return;
            }

            // Reset
            RemoveReceiptLabel();
            OrderNumber_TextBox.Text = "";
        }
        private void MultipleItems_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (MultipleItems_CheckBox.Checked)
            {
                if (AddButton == null)
                {
                    ConstructFlowPanel();
                    CosntructAddButton();
                    ConstructControlsForMultipleProducts();
                }
                SetControlsForMultipleProducts();
            }
            else
            {
                SetControlsForSingleProduct();
            }
        }
        private void WarningProduct_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Products_Form(true).ShowDialog();
            CheckIfProductsExist();
        }
        private void WarningBuyer_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Accountants_Form().ShowDialog();
            CheckIfBuyersExist();
        }
        private void Receipt_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Select file
            OpenFileDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                receiptFilePath = MainMenu_Form.receipt_text + dialog.FileName;
                ShowReceiptLabel(dialog.SafeFileName);
            }
        }
        private void RemoveReceipt_ImageButton_Click(object sender, EventArgs e)
        {
            RemoveReceiptLabel();
        }
        private void RemoveReceipt_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.fileHover;
        }
        private void RemoveReceipt_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.mainBackground;
        }
        private void MultipleItems_Label_Click(object sender, EventArgs e)
        {
            MultipleItems_CheckBox.Checked = !MultipleItems_CheckBox.Checked;
        }

        // Methods to add purchases
        private bool AddSinglePurchase()
        {
            string purchaseNumber = OrderNumber_TextBox.Text.Trim();

            // Check if purchase ID already exists
            if (purchaseNumber != MainMenu_Form.emptyCell && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), purchaseNumber))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                    $"The order #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            string buyerName = AccountantName_TextBox.Text;

            string[] items = ProductName_TextBox.Text.Split('>');
            string categoryName = items[0].Trim();
            string productName = items[1].Trim();

            string country = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, productName);
            string company = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, productName);
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal discount = decimal.Parse(Discount_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit + shipping + tax + fee - discount;
            string noteLabel = MainMenu_Form.emptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = MainMenu_Form.show_text;
            }

            // Round to 2 decimal places
            decimal amountCharged = decimal.Parse(Charged_TextBox.Text);
            totalPrice = Math.Round(totalPrice, 2);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                    $"Amount charged ({MainMenu_Form.CurrencySymbol}{amountCharged}) is not equal to the total price of the purchase (${totalPrice}). The difference will be accounted for.",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }

            // Convert to USD
            decimal exchangeRateToUSD = Currency.GetExchangeRate(Currency_ComboBox.Text, "USD", date);
            if (exchangeRateToUSD == -1) { return false; }

            decimal pricePerUnitUSD = pricePerUnit * exchangeRateToUSD;
            decimal shippingUSD = shipping * exchangeRateToUSD;
            decimal taxUSD = tax * exchangeRateToUSD;
            decimal feeUSD = fee * exchangeRateToUSD;
            decimal discountUSD = discount * exchangeRateToUSD;
            decimal chargedDifferenceUSD = chargedDifference * exchangeRateToUSD;
            decimal totalPriceUSD = totalPrice * exchangeRateToUSD;

            // Store the money values in the tag
            TagData purchaseData = new()
            {
                PricePerUnitUSD = pricePerUnitUSD,
                ShippingUSD = shippingUSD,
                TaxUSD = taxUSD,
                FeeUSD = feeUSD,
                DiscountUSD = discountUSD,
                ChargedDifferenceUSD = chargedDifferenceUSD,
                TotalUSD = totalPriceUSD,
                DefaultCurrencyType = Currency_ComboBox.Text
            };

            string newFilePath = "";
            if (!MainMenu_Form.CheckIfReceiptExists(receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = MainMenu_Form.SaveReceiptInFile(receiptFilePath);
                if (!saved)
                {
                    return false;
                }
            }

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                purchaseNumber,
                buyerName,
                productName,
                categoryName,
                country,
                company,
                date,
                quantity.ToString(),
                pricePerUnit.ToString("N2"),
                shipping.ToString("N2"),
                tax.ToString("N2"),
                fee.ToString("N2"),
                discount.ToString("N2"),
                chargedDifference.ToString("N2"),
                totalPrice.ToString("N2"),
                noteLabel
            );

            if (noteLabel == MainMenu_Form.show_text)
            {
                MainMenu_Form.AddNoteToCell(newRowIndex, note);
            }

            // Set the tag
            if (newFilePath != "")
            {
                MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = (newFilePath, purchaseData);
            }

            MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, categoryName);
            Log.Write(3, $"Added purchase '{categoryName}'");

            return true;
        }
        private bool AddMultiplePurchases()
        {
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;

            string purchaseNumber = OrderNumber_TextBox.Text.Trim();

            // Check if purchase ID already exists
            if (purchaseNumber != MainMenu_Form.emptyCell && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchases_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), purchaseNumber))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                    $"The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            string accountant = AccountantName_TextBox.Text;
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal discount = decimal.Parse(Discount_TextBox.Text);
            string noteLabel = MainMenu_Form.emptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = MainMenu_Form.show_text;
            }

            decimal exchangeRate = Currency.GetExchangeRate(Currency_ComboBox.Text, Properties.Settings.Default.Currency, date);
            if (exchangeRate == -1) { return false; }

            List<string> items = [];

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            decimal totalPrice = 0;
            int totalQuantity = 0;

            // Get exchange rate
            decimal exchangeRateToUSD = Currency.GetExchangeRate(Currency_ComboBox.Text, "USD", date);
            if (exchangeRateToUSD == -1) { return false; }

            foreach (Guna2Panel panel in panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string[] itemsInName = nameTextBox.Text.Split('>');
                string categoryName = itemsInName[0].Trim();
                string productName = itemsInName[1].Trim();

                string currentCountry = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, productName);
                string currentCompany = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, productName);

                if (firstCategoryName == null)
                {
                    firstCategoryName = categoryName;
                }
                else if (isCategoryNameConsistent && firstCategoryName != categoryName)
                {
                    isCategoryNameConsistent = false;
                }

                if (firstCountry == null)
                {
                    firstCountry = currentCountry;
                }
                else if (isCountryConsistent && firstCountry != currentCountry)
                {
                    isCountryConsistent = false;
                }

                if (firstCompany == null)
                {
                    firstCompany = currentCompany;
                }
                else if (isCompanyConsistent && firstCompany != currentCompany)
                {
                    isCompanyConsistent = false;
                }

                Guna2TextBox quantityTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.quantity.ToString(), false).FirstOrDefault();
                int quantity = int.Parse(quantityTextBox.Text);
                Guna2TextBox pricePerUnitTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.pricePerUnit.ToString(), false).FirstOrDefault();
                decimal pricePerUnit = decimal.Parse(pricePerUnitTextBox.Text);
                decimal pricePerUnitUSD = pricePerUnit * exchangeRateToUSD;
                totalPrice += quantity * pricePerUnit;
                totalQuantity += quantity;

                string item = string.Join(",",
                    productName,
                    categoryName,
                    currentCountry,
                    currentCompany,
                    quantity.ToString(),
                    pricePerUnit.ToString("N2"),
                    pricePerUnitUSD.ToString("N2")
                );

                items.Add(item);
            }

            // Convert currency
            shipping *= exchangeRate;
            tax *= exchangeRate;
            fee *= exchangeRate;
            discount *= exchangeRate;
            totalPrice *= exchangeRate;

            totalPrice += shipping + tax + fee - discount;
            totalPrice = Math.Round(totalPrice, 2);

            decimal amountCharged = decimal.Parse(Charged_TextBox.Text);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker",
                    $"Amount charged ({MainMenu_Form.CurrencySymbol}{amountCharged}) is not equal to the total price of the sale ({MainMenu_Form.CurrencySymbol}{totalPrice}). The difference will be accounted for.",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }
            totalPrice += chargedDifference;

            string newFilePath = "";
            if (!MainMenu_Form.CheckIfReceiptExists(receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = MainMenu_Form.SaveReceiptInFile(receiptFilePath);
                if (!saved)
                {
                    return false;
                }
            }

            string finalCategoryName = isCategoryNameConsistent ? firstCategoryName : MainMenu_Form.emptyCell;
            string finalCountry = isCountryConsistent ? firstCountry : MainMenu_Form.emptyCell;
            string finalCompany = isCompanyConsistent ? firstCompany : MainMenu_Form.emptyCell;

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                purchaseNumber,
                accountant,
                MainMenu_Form.multipleItems_text,
                finalCategoryName,
                finalCountry,
                finalCompany,
                date,
                totalQuantity.ToString(),
                MainMenu_Form.emptyCell,
                shipping.ToString("N2"),
                tax.ToString("N2"),
                fee.ToString("N2"),
                discount.ToString("N2"),
                chargedDifference.ToString("N2"),
                totalPrice.ToString("N2"),
                noteLabel
            );
            if (noteLabel == MainMenu_Form.show_text)
            {
                MainMenu_Form.AddNoteToCell(newRowIndex, note);
            }
            if (newFilePath != "")
            {
                items.Add(newFilePath);
            }

            // Calculate USD values
            decimal shippingUSD = shipping * exchangeRateToUSD;
            decimal taxUSD = tax * exchangeRateToUSD;
            decimal feeUSD = fee * exchangeRateToUSD;
            decimal discountUSD = discount * exchangeRateToUSD;
            decimal chargedDifferenceUSD = chargedDifference * exchangeRateToUSD;
            decimal totalPriceUSD = totalPrice * exchangeRateToUSD;

            // Store the money values in the tag
            TagData tagData = new()
            {
                ShippingUSD = shippingUSD,
                TaxUSD = taxUSD,
                FeeUSD = feeUSD,
                DiscountUSD = discountUSD,
                ChargedDifferenceUSD = chargedDifferenceUSD,
                TotalUSD = totalPriceUSD,
                DefaultCurrencyType = Currency_ComboBox.Text
            };

            // Set the tag
            MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = (items, tagData);

            MainMenu_Form.Instance.DataGridViewRowsAdded(MainMenu_Form.Instance.selectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, purchaseNumber);
            Log.Write(3, $"Added purchase '{purchaseNumber}' with '{totalQuantity}' items");

            return true;
        }

        // Receipts
        private string receiptFilePath;
        private void ShowReceiptLabel(string text)
        {
            SelectedReceipt_Label.Text = text;

            Controls.Add(SelectedReceipt_Label);
            Controls.Add(RemoveReceipt_ImageButton);
            SetReceiptLabelLocation();

            ValidateInputs(null, null);
        }
        private void SetReceiptLabelLocation()
        {
            if (!Controls.Contains(SelectedReceipt_Label))
            {
                return;
            }

            RemoveReceipt_ImageButton.Location = new Point(Receipt_Button.Right - RemoveReceipt_ImageButton.Width, Receipt_Button.Bottom + UI.spaceBetweenControls);
            SelectedReceipt_Label.Location = new Point(
                RemoveReceipt_ImageButton.Left - SelectedReceipt_Label.Width,
                RemoveReceipt_ImageButton.Top + (RemoveReceipt_ImageButton.Height - SelectedReceipt_Label.Height) / 2 - 1);
        }
        private void RemoveReceiptLabel()
        {
            Controls.Remove(SelectedReceipt_Label);
            Controls.Remove(RemoveReceipt_ImageButton);
            ValidateInputs(null, null);
        }

        // Methods for multiple items
        private List<Control> GetControlsForMultipleProducts()
        {
            return [ProductName_TextBox, ProductName_Label,
                Quantity_TextBox, Quantity_Label,
                PricePerUnit_TextBox, PricePerUnit_Label];
        }
        private readonly byte textBoxHeight = 48, circleButtonHeight = 38, extraSpaceForBottom = 210, spaceBetweenPanels = 10,
               initialHeightForPanel = 88, spaceOnSidesOfPanel = 100, flowPanelMargin = 6;
        private readonly short initialWidthForPanel = 673, maxFlowPanelHeight = 300;
        private void SetControlsForSingleProduct()
        {
            // Center controls
            Currency_ComboBox.Left = (ClientSize.Width - Currency_ComboBox.Width - UI.spaceBetweenControls -
                OrderNumber_TextBox.Width - UI.spaceBetweenControls -
                AccountantName_TextBox.Width - UI.spaceBetweenControls -
                ProductName_TextBox.Width - UI.spaceBetweenControls -
                Receipt_Button.Width) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + UI.spaceBetweenControls;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            AccountantName_TextBox.Left = OrderNumber_TextBox.Right + UI.spaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            ProductName_TextBox.Left = AccountantName_TextBox.Right + UI.spaceBetweenControls;
            ProductName_Label.Left = ProductName_TextBox.Left;
            Receipt_Button.Left = ProductName_TextBox.Right + UI.spaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width -
                Date_DateTimePicker.Width - UI.spaceBetweenControls -
                Quantity_TextBox.Width - UI.spaceBetweenControls -
                PricePerUnit_TextBox.Width - UI.spaceBetweenControls -
                Shipping_TextBox.Width - UI.spaceBetweenControls -
                Tax_TextBox.Width - UI.spaceBetweenControls -
                PaymentFee_TextBox.Width - UI.spaceBetweenControls -
                Charged_TextBox.Width - UI.spaceBetweenControls -
                Discount_TextBox.Width) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Quantity_TextBox.Left = Date_DateTimePicker.Right + UI.spaceBetweenControls;
            Quantity_Label.Left = Quantity_TextBox.Left;
            PricePerUnit_TextBox.Left = Quantity_TextBox.Right + UI.spaceBetweenControls;
            PricePerUnit_Label.Left = PricePerUnit_TextBox.Left;
            Shipping_TextBox.Left = PricePerUnit_TextBox.Right + UI.spaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + UI.spaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + UI.spaceBetweenControls;
            Fee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + UI.spaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            Charged_TextBox.Left = Discount_TextBox.Right + UI.spaceBetweenControls;
            Charged_Label.Left = Charged_TextBox.Left;

            // Add controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Add(control);
            }

            Controls.Remove(FlowPanel);
            Controls.Remove(AddButton);
            Height = 465;

            RelocateBuyerWarning();
            SetReceiptLabelLocation();

            if (Controls.Contains(WarningProduct_PictureBox))
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + UI.spaceBetweenControls);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + UI.spaceBetweenControls, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            Currency_ComboBox.Left = (ClientSize.Width -
                Currency_ComboBox.Width - UI.spaceBetweenControls -
                OrderNumber_TextBox.Width - UI.spaceBetweenControls -
                AccountantName_TextBox.Width - UI.spaceBetweenControls -
                Receipt_Button.Width) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + UI.spaceBetweenControls;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            AccountantName_TextBox.Left = OrderNumber_TextBox.Right + UI.spaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            Receipt_Button.Left = AccountantName_TextBox.Right + UI.spaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width -
                Date_DateTimePicker.Width - UI.spaceBetweenControls -
                Shipping_TextBox.Width - UI.spaceBetweenControls -
                Tax_TextBox.Width - UI.spaceBetweenControls -
                PaymentFee_TextBox.Width - UI.spaceBetweenControls -
                Charged_TextBox.Width - UI.spaceBetweenControls -
                Discount_TextBox.Width) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + UI.spaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + UI.spaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + UI.spaceBetweenControls;
            Fee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + UI.spaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            Charged_TextBox.Left = Discount_TextBox.Right + UI.spaceBetweenControls;
            Charged_Label.Left = Charged_TextBox.Left;

            // Remove controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Remove(control);
            }

            Controls.Add(FlowPanel);
            SetHeight();

            RelocateBuyerWarning();
            SetReceiptLabelLocation();

            if (Controls.Contains(WarningProduct_PictureBox))
            {
                WarningProduct_PictureBox.Location = new Point(AddButton.Left + UI.spaceBetweenControls, AddButton.Top - flowPanelMargin * 2);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + UI.spaceBetweenControls, WarningProduct_PictureBox.Top);
                Controls.Remove(AddButton);
            }
            else
            {
                Controls.Add(AddButton);
            }
        }
        private void RelocateBuyerWarning()
        {
            WarningBuyer_PictureBox.Location = new Point(AccountantName_TextBox.Left, AccountantName_TextBox.Bottom + UI.spaceBetweenControls);
            WarningBuyer_LinkLabel.Location = new Point(WarningBuyer_PictureBox.Right + UI.spaceBetweenControls, WarningBuyer_PictureBox.Top);
        }
        private readonly List<Guna2Panel> panelsForMultipleProducts_List = [];
        private enum TextBoxnames
        {
            name,
            quantity,
            pricePerUnit
        }
        private void ConstructControlsForMultipleProducts()
        {
            byte searchBoxMaxHeight = 150;

            Guna2Panel panel = new()
            {
                Size = new Size(initialWidthForPanel, initialHeightForPanel),
                FillColor = CustomColors.mainBackground
            };
            panelsForMultipleProducts_List.Add(panel);

            Guna2TextBox textBox;
            int left;

            // Product name
            textBox = CosntructTextBox(0, ProductName_TextBox.Width, TextBoxnames.name.ToString(), UI.KeyPressValidation.None, panel);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames());
            SearchBox.Attach(textBox, this, () => searchResult, searchBoxMaxHeight);
            AccountantName_TextBox.TextChanged += ValidateInputs;

            CosntructLabel(ProductName_Label.Text, 0, panel);

            // Quantity
            left = textBox.Right + UI.spaceBetweenControls;
            textBox = CosntructTextBox(left, Quantity_TextBox.Width, TextBoxnames.quantity.ToString(), UI.KeyPressValidation.OnlyNumbers, panel);
            CosntructLabel(Quantity_Label.Text, left, panel);

            // Price per unit
            left = textBox.Right + UI.spaceBetweenControls;
            textBox = CosntructTextBox(left, PricePerUnit_TextBox.Width, TextBoxnames.pricePerUnit.ToString(), UI.KeyPressValidation.OnlyNumbersAndDecimal, panel);
            CosntructLabel(PricePerUnit_Label.Text, left, panel);

            // Add minus button unless this is the first panel
            left = textBox.Right + UI.spaceBetweenControls;
            if (panelsForMultipleProducts_List.Count > 1)
            {
                CosntructMinusButton(new Point(left + UI.spaceBetweenControls, (textBoxHeight - circleButtonHeight) / 2 + textBox.Top), panel);
            }

            FlowPanel.SuspendLayout();
            FlowPanel.Controls.Add(panel);
            SetHeight();
            FlowPanel.ResumeLayout();
            FlowPanel.ScrollControlIntoView(panel);
        }
        private void CosntructLabel(string text, int left, Control parent)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = CustomColors.text,
                Left = left,
                AutoSize = true
            };
            label.Click += CloseAllPanels;
            parent.Controls.Add(label);
        }
        private Guna2TextBox CosntructTextBox(int left, int width, string name, UI.KeyPressValidation keyPressValidation, Control parent)
        {
            Guna2TextBox textBox = new()
            {
                Size = new Size(width, textBoxHeight),
                Name = name,
                Location = new Point(left, 28 + UI.spaceBetweenControls),
                FillColor = CustomColors.controlBack,
                BorderColor = CustomColors.controlBorder,
                ForeColor = CustomColors.text,
                ShortcutsEnabled = false
            };
            textBox.HoverState.BorderColor = CustomColors.accent_blue;
            textBox.FocusedState.BorderColor = CustomColors.accent_blue;
            textBox.FocusedState.FillColor = CustomColors.controlBack;

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case UI.KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case UI.KeyPressValidation.OnlyNumbersAndDecimal:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
                    break;
                case UI.KeyPressValidation.OnlyNumbers:
                    textBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case UI.KeyPressValidation.OnlyLetters:
                    textBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
                    break;
                case UI.KeyPressValidation.None:
                    break;
            }

            textBox.Click += CloseAllPanels;
            textBox.TextChanged += ValidateInputs;
            TextBoxManager.Attach(textBox);

            parent.Controls.Add(textBox);
            return textBox;
        }
        private void CosntructMinusButton(Point location, Control parent)
        {
            Guna2CircleButton circleBtn = new()
            {
                FillColor = CustomColors.mainBackground,
                BackColor = CustomColors.mainBackground,
                Location = location,
                Size = new Size(circleButtonHeight, circleButtonHeight),
                ImageSize = new Size(32, 32),
                PressedColor = CustomColors.controlBack
            };
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                circleBtn.Image = Resources.MinusWhite;
            }
            else
            {
                circleBtn.Image = Resources.MinusBlack;
            }
            circleBtn.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                RemovePanelForMultipleProducts(sender, e);
                ValidateInputs(null, null);
            };
            parent.Controls.Add(circleBtn);
        }
        private void RemovePanelForMultipleProducts(object sender, EventArgs e)
        {
            Guna2CircleButton button = (Guna2CircleButton)sender;
            Guna2Panel panel = (Guna2Panel)button.Parent;

            FlowPanel.Controls.Remove(panel);
            panelsForMultipleProducts_List.Remove(panel);
            FlowPanel.Height -= initialHeightForPanel + flowPanelMargin;
            SetHeight();
        }
        private Guna2CircleButton AddButton;
        private FlowLayoutPanel FlowPanel;
        private void ConstructFlowPanel()
        {
            int width = initialWidthForPanel + spaceOnSidesOfPanel;
            FlowPanel = new()
            {
                Anchor = AnchorStyles.Top,
                AutoScroll = false,
                Location = new Point((ClientSize.Width - width) / 2, 570),
                Size = new Size(width, 20 + UI.spaceBetweenControls + textBoxHeight),
                Padding = new Padding(spaceOnSidesOfPanel / 2, 0, spaceOnSidesOfPanel / 2, 0),
                Margin = new Padding(flowPanelMargin / 2, 0, flowPanelMargin / 2, 0),
                MaximumSize = new Size(width, maxFlowPanelHeight)
            };
            FlowPanel.Click += CloseAllPanels;
        }
        private void CosntructAddButton()
        {
            AddButton = new()
            {
                FillColor = CustomColors.mainBackground,
                BackColor = CustomColors.mainBackground,
                Location = new Point(0, 60),
                Size = new Size(circleButtonHeight, circleButtonHeight),
                Image = Resources.AddWhite,
                ImageSize = new Size(32, 32),
                Left = FlowPanel.Left + spaceOnSidesOfPanel / 2,
                PressedColor = CustomColors.controlBack
            };
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                AddButton.Image = Resources.AddWhite;
            }
            else
            {
                AddButton.Image = Resources.AddBlack;
            }
            AddButton.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                ConstructControlsForMultipleProducts();
                ValidateInputs(null, null);
            };
        }
        private void SetHeight()
        {
            int totalHeight = panelsForMultipleProducts_List.Sum(panel => panel.Height + flowPanelMargin);
            FlowPanel.Height = Math.Min(totalHeight + flowPanelMargin, maxFlowPanelHeight);
            FlowPanel.AutoScroll = totalHeight + flowPanelMargin > maxFlowPanelHeight;

            Height = FlowPanel.Bottom + extraSpaceForBottom;
            AddButton.Top = FlowPanel.Bottom + spaceBetweenPanels;
        }

        // Warning labels
        private void CheckIfProductsExist()
        {
            if (MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames().Count == 0)
            {
                ShowProductWarning();
            }
            else
            {
                HideProductWarning();
            }
        }
        private void ShowProductWarning()
        {
            Controls.Add(WarningProduct_PictureBox);
            Controls.Add(WarningProduct_LinkLabel);
        }
        private void HideProductWarning()
        {
            Controls.Remove(WarningProduct_PictureBox);
            Controls.Remove(WarningProduct_LinkLabel);
            Controls.Add(AddButton);
        }
        private void CheckIfBuyersExist()
        {
            if (MainMenu_Form.Instance.accountantList.Count == 0)
            {
                ShowBuyerWarning();
            }
            else
            {
                HideBuyerWarning();
            }
        }
        private void ShowBuyerWarning()
        {
            WarningBuyer_LinkLabel.Visible = true;
            WarningBuyer_PictureBox.Visible = true;
        }
        private void HideBuyerWarning()
        {
            WarningBuyer_LinkLabel.Visible = false;
            WarningBuyer_PictureBox.Visible = false;
        }

        // Misc.
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(OrderNumber_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(AccountantName_TextBox.Text) && AccountantName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PaymentFee_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Discount_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Charged_TextBox.Text);

            if (Properties.Settings.Default.PurchaseReceipts)
            {
                allFieldsFilled &= Controls.Contains(SelectedReceipt_Label);
            }

            bool allMultipleFieldsFilled = true;

            if (MultipleItems_CheckBox.Checked)
            {
                allMultipleFieldsFilled = panelsForMultipleProducts_List
                    .SelectMany(panel => panel.Controls.OfType<Guna2TextBox>())
                    .All(textBox => !string.IsNullOrWhiteSpace(textBox.Text));
            }
            else
            {
                allFieldsFilled &= !string.IsNullOrWhiteSpace(ProductName_TextBox.Text) && ProductName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Quantity_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PricePerUnit_TextBox.Text);
            }
            AddPurchase_Button.Enabled = allFieldsFilled && allMultipleFieldsFilled;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}