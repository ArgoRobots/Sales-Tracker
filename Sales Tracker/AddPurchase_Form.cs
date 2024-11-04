using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class AddPurchase_Form : Form
    {
        // Properties
        private static readonly List<string> _thingsThatHaveChangedInFile = [];

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;

        // Init.
        public AddPurchase_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            AddEventHandlersToTextBoxes();
            Date_DateTimePicker.Value = DateTime.Now;
            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            Currency_ComboBox.Text = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            CheckIfProductsExist();
            CheckIfAccountantsExist();
            Theme.SetThemeForForm(this);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            RemoveReceiptLabel();
            Charged_Label.Text = $"{MainMenu_Form.CurrencySymbol} charged ({DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType)})";
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(OrderNumber_TextBox);

            AccountantName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(AccountantName_TextBox);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.AccountantList);
            SearchBox.Attach(AccountantName_TextBox, this, () => searchResult, searchBoxMaxHeight);
            AccountantName_TextBox.TextChanged += ValidateInputs;

            TextBoxManager.Attach(ProductName_TextBox);
            List<SearchResult> searchResult1 = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames());
            SearchBox.Attach(ProductName_TextBox, this, () => searchResult1, searchBoxMaxHeight, false, true);
            ProductName_TextBox.TextChanged += ValidateInputs;

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
        private void SetAccessibleDescriptions()
        {
            Label[] labelsToAlignLeftCenter =
            [
                MultipleItems_Label,
                Currency_Label,
                OrderNumber_Label,
                AccountantName_Label,
                ProductName_Label,
                Date_Label,
                Quantity_Label,
                PricePerUnit_Label,
                Shipping_Label,
                Tax_Label,
                Discount_Label,
                Charged_Label,
                WarningAccountant_LinkLabel,
                WarningProduct_LinkLabel
            ];

            foreach (Label label in labelsToAlignLeftCenter)
            {
                label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            }

            SelectedReceipt_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignRightCenter;
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
            MainMenu_Form.Instance.SelectedDataGridView = MainMenu_Form.Instance.Purchase_DataGridView;

            if (_panelsForMultipleProducts_List.Count == 0 || !MultipleItems_CheckBox.Checked)
            {
                if (!AddSinglePurchase()) { return; }
            }
            // When the user selects "multiple items in this order" but only adds one, treat it as one
            else if (_panelsForMultipleProducts_List.Count == 1)
            {
                // Extract details from the single panel and populate the single purchase fields
                Guna2Panel singlePanel = _panelsForMultipleProducts_List[0];
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
                if (_addButton == null)
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
            CheckIfAccountantsExist();
        }
        private void Receipt_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            // Select file
            OpenFileDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _receiptFilePath = ReadOnlyVariables.Receipt_text + dialog.FileName;
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
            if (purchaseNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), purchaseNumber))
            {
                string message = $"The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?";
                CustomMessageBoxResult result = CustomMessageBox.Show("Purchase # already exists", message, CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            string buyerName = AccountantName_TextBox.Text;

            string[] items = ProductName_TextBox.Text.Split('>');
            string categoryName = items[0].Trim();
            string productName = items[1].Trim();

            string country = MainMenu_Form.GetCountryProductIsFrom(MainMenu_Form.Instance.CategoryPurchaseList, productName);
            string company = MainMenu_Form.GetCompanyProductIsFrom(MainMenu_Form.Instance.CategoryPurchaseList, productName);
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal discount = decimal.Parse(Discount_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit;
            string noteLabel = ReadOnlyVariables.EmptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = ReadOnlyVariables.Show_text;
            }

            decimal exchangeRateToDefault = Currency.GetExchangeRate(Currency_ComboBox.Text, DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType), date);
            if (exchangeRateToDefault == -1) { return false; }

            // Convert currency
            pricePerUnit *= exchangeRateToDefault;
            shipping *= exchangeRateToDefault;
            tax *= exchangeRateToDefault;
            fee *= exchangeRateToDefault;
            discount *= exchangeRateToDefault;
            totalPrice *= exchangeRateToDefault;

            totalPrice += shipping + tax + fee - discount;
            totalPrice = Math.Round(totalPrice, 2);

            decimal amountCharged = decimal.Parse(Charged_TextBox.Text);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
                string message = $"Amount charged ({MainMenu_Form.CurrencySymbol}{amountCharged} {currency}) is not equal to the total price of the sale ({MainMenu_Form.CurrencySymbol}{totalPrice} {currency}). The difference will be accounted for.";
                CustomMessageBoxResult result = CustomMessageBox.Show("Amount charged is different", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

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
                TotalUSD = totalPriceUSD
            };

            string newFilePath = "";
            if (!ReceiptsManager.CheckIfReceiptExists(_receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = ReceiptsManager.SaveReceiptInFile(_receiptFilePath);
                if (!saved)
                {
                    return false;
                }
            }

            int newRowIndex = MainMenu_Form.Instance.SelectedDataGridView.Rows.Add(
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
                amountCharged.ToString("N2"),
                noteLabel
            );

            if (noteLabel == ReadOnlyVariables.Show_text)
            {
                DataGridViewManager.AddNoteToCell(newRowIndex, note);
            }

            // Set the tag
            if (newFilePath != "")
            {
                MainMenu_Form.Instance.SelectedDataGridView.Rows[newRowIndex].Tag = (newFilePath, purchaseData);
            }

            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            string logMessage = $"Added purchase '{purchaseNumber}'";
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, logMessage);
            Log.Write(3, logMessage);

            return true;
        }
        private bool AddMultiplePurchases()
        {
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;

            string purchaseNumber = OrderNumber_TextBox.Text.Trim();

            // Check if purchase ID already exists
            if (purchaseNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), purchaseNumber))
            {
                string message = $"The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?";
                CustomMessageBoxResult result = CustomMessageBox.Show("Purchase # already exists", message, CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

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
            string noteLabel = ReadOnlyVariables.EmptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = ReadOnlyVariables.Show_text;
            }

            decimal exchangeRateToDefault = Currency.GetExchangeRate(Currency_ComboBox.Text, DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType), date);
            if (exchangeRateToDefault == -1) { return false; }

            List<string> items = [];

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            decimal totalPrice = 0;
            int totalQuantity = 0;

            // Get exchange rate
            decimal exchangeRateToUSD = Currency.GetExchangeRate(Currency_ComboBox.Text, "USD", date);
            if (exchangeRateToUSD == -1) { return false; }

            foreach (Guna2Panel panel in _panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string[] itemsInName = nameTextBox.Text.Split('>');
                string categoryName = itemsInName[0].Trim();
                string productName = itemsInName[1].Trim();

                string currentCountry = MainMenu_Form.GetCountryProductIsFrom(MainMenu_Form.Instance.CategoryPurchaseList, productName);
                string currentCompany = MainMenu_Form.GetCompanyProductIsFrom(MainMenu_Form.Instance.CategoryPurchaseList, productName);

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
                    (pricePerUnit * exchangeRateToDefault).ToString("F2"),
                    pricePerUnitUSD.ToString("F2")
                );

                items.Add(item);
            }

            // Convert currency
            shipping *= exchangeRateToDefault;
            tax *= exchangeRateToDefault;
            fee *= exchangeRateToDefault;
            discount *= exchangeRateToDefault;
            totalPrice *= exchangeRateToDefault;

            totalPrice += shipping + tax + fee - discount;
            totalPrice = Math.Round(totalPrice, 2);

            decimal amountCharged = decimal.Parse(Charged_TextBox.Text);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
                string message = $"Amount charged ({MainMenu_Form.CurrencySymbol}{amountCharged} {currency}) is not equal to the total price of the sale ({MainMenu_Form.CurrencySymbol}{totalPrice} {currency}). The difference will be accounted for.";
                CustomMessageBoxResult result = CustomMessageBox.Show("Amount charged is different", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }
            totalPrice += chargedDifference;

            string newFilePath = "";
            if (!ReceiptsManager.CheckIfReceiptExists(_receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = ReceiptsManager.SaveReceiptInFile(_receiptFilePath);
                if (!saved)
                {
                    return false;
                }
            }

            string finalCategoryName = isCategoryNameConsistent ? firstCategoryName : ReadOnlyVariables.EmptyCell;
            string finalCountry = isCountryConsistent ? firstCountry : ReadOnlyVariables.EmptyCell;
            string finalCompany = isCompanyConsistent ? firstCompany : ReadOnlyVariables.EmptyCell;

            int newRowIndex = MainMenu_Form.Instance.SelectedDataGridView.Rows.Add(
                purchaseNumber,
                accountant,
                ReadOnlyVariables.MultipleItems_text,
                finalCategoryName,
                finalCountry,
                finalCompany,
                date,
                totalQuantity.ToString(),
                ReadOnlyVariables.EmptyCell,
                shipping.ToString("N2"),
                tax.ToString("N2"),
                fee.ToString("N2"),
                discount.ToString("N2"),
                chargedDifference.ToString("N2"),
                amountCharged.ToString("N2"),
                noteLabel
            );
            if (noteLabel == ReadOnlyVariables.Show_text)
            {
                DataGridViewManager.AddNoteToCell(newRowIndex, note);
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
                TotalUSD = totalPriceUSD
            };

            // Set the tag
            MainMenu_Form.Instance.SelectedDataGridView.Rows[newRowIndex].Tag = (items, tagData);

            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            string logMessage = $"Added purchase '{purchaseNumber}' with '{totalQuantity}' items";
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, logMessage);
            Log.Write(3, logMessage);

            return true;
        }

        // Receipts
        private string _receiptFilePath;
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

            RemoveReceipt_ImageButton.Location = new Point(Receipt_Button.Right - RemoveReceipt_ImageButton.Width, Receipt_Button.Bottom + CustomControls.SpaceBetweenControls);
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
            return [ProductName_TextBox, ProductName_Label, Quantity_TextBox, Quantity_Label, PricePerUnit_TextBox, PricePerUnit_Label];
        }
        private readonly byte _textBoxHeight = 48, _circleButtonHeight = 38, _extraSpaceForBottom = 210, spaceBetweenPanels = 10,
               _initialHeightForPanel = 88, _spaceOnSidesOfPanel = 100, _flowPanelMargin = 6;
        private readonly short _initialWidthForPanel = 673, _flowPanelMaxHeight = 300;
        private void SetControlsForSingleProduct()
        {
            // Center controls
            Currency_ComboBox.Left = (ClientSize.Width - Currency_ComboBox.Width - CustomControls.SpaceBetweenControls -
                OrderNumber_TextBox.Width - CustomControls.SpaceBetweenControls -
                AccountantName_TextBox.Width - CustomControls.SpaceBetweenControls -
                ProductName_TextBox.Width - CustomControls.SpaceBetweenControls -
                Receipt_Button.Width) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + CustomControls.SpaceBetweenControls;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            AccountantName_TextBox.Left = OrderNumber_TextBox.Right + CustomControls.SpaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            ProductName_TextBox.Left = AccountantName_TextBox.Right + CustomControls.SpaceBetweenControls;
            ProductName_Label.Left = ProductName_TextBox.Left;
            Receipt_Button.Left = ProductName_TextBox.Right + CustomControls.SpaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width -
                Date_DateTimePicker.Width - CustomControls.SpaceBetweenControls -
                Quantity_TextBox.Width - CustomControls.SpaceBetweenControls -
                PricePerUnit_TextBox.Width - CustomControls.SpaceBetweenControls -
                Shipping_TextBox.Width - CustomControls.SpaceBetweenControls -
                Tax_TextBox.Width - CustomControls.SpaceBetweenControls -
                PaymentFee_TextBox.Width - CustomControls.SpaceBetweenControls -
                Charged_TextBox.Width - CustomControls.SpaceBetweenControls -
                Discount_TextBox.Width) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Quantity_TextBox.Left = Date_DateTimePicker.Right + CustomControls.SpaceBetweenControls;
            Quantity_Label.Left = Quantity_TextBox.Left;
            PricePerUnit_TextBox.Left = Quantity_TextBox.Right + CustomControls.SpaceBetweenControls;
            PricePerUnit_Label.Left = PricePerUnit_TextBox.Left;
            Shipping_TextBox.Left = PricePerUnit_TextBox.Right + CustomControls.SpaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + CustomControls.SpaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + CustomControls.SpaceBetweenControls;
            Fee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + CustomControls.SpaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            Charged_TextBox.Left = Discount_TextBox.Right + CustomControls.SpaceBetweenControls;
            Charged_Label.Left = Charged_TextBox.Left;

            // Add controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Add(control);
            }

            _flowPanel.Visible = false;
            _addButton.Visible = false;
            Height = 465;

            RelocateBuyerWarning();
            SetReceiptLabelLocation();

            if (WarningProduct_PictureBox.Visible)
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + CustomControls.SpaceBetweenControls);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + CustomControls.SpaceBetweenControls, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            Currency_ComboBox.Left = (ClientSize.Width -
                Currency_ComboBox.Width - CustomControls.SpaceBetweenControls -
                OrderNumber_TextBox.Width - CustomControls.SpaceBetweenControls -
                AccountantName_TextBox.Width - CustomControls.SpaceBetweenControls -
                Receipt_Button.Width) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + CustomControls.SpaceBetweenControls;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            AccountantName_TextBox.Left = OrderNumber_TextBox.Right + CustomControls.SpaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            Receipt_Button.Left = AccountantName_TextBox.Right + CustomControls.SpaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width -
                Date_DateTimePicker.Width - CustomControls.SpaceBetweenControls -
                Shipping_TextBox.Width - CustomControls.SpaceBetweenControls -
                Tax_TextBox.Width - CustomControls.SpaceBetweenControls -
                PaymentFee_TextBox.Width - CustomControls.SpaceBetweenControls -
                Charged_TextBox.Width - CustomControls.SpaceBetweenControls -
                Discount_TextBox.Width) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + CustomControls.SpaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + CustomControls.SpaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + CustomControls.SpaceBetweenControls;
            Fee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + CustomControls.SpaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            Charged_TextBox.Left = Discount_TextBox.Right + CustomControls.SpaceBetweenControls;
            Charged_Label.Left = Charged_TextBox.Left;

            // Remove controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Remove(control);
            }

            _flowPanel.Visible = true;
            SetHeight();

            RelocateBuyerWarning();
            SetReceiptLabelLocation();

            if (WarningProduct_PictureBox.Visible)
            {
                WarningProduct_PictureBox.Location = new Point(_addButton.Left + CustomControls.SpaceBetweenControls, _addButton.Top - _flowPanelMargin * 2);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + CustomControls.SpaceBetweenControls, WarningProduct_PictureBox.Top);
                _addButton.Visible = false;
            }
            else
            {
                _addButton.Visible = true;
            }
        }
        private void RelocateBuyerWarning()
        {
            WarningAccountant_PictureBox.Location = new Point(AccountantName_TextBox.Left, AccountantName_TextBox.Bottom + CustomControls.SpaceBetweenControls);
            WarningAccountant_LinkLabel.Location = new Point(WarningAccountant_PictureBox.Right + CustomControls.SpaceBetweenControls, WarningAccountant_PictureBox.Top);
        }
        private readonly List<Guna2Panel> _panelsForMultipleProducts_List = [];
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
                Size = new Size(_initialWidthForPanel, _initialHeightForPanel),
                FillColor = CustomColors.mainBackground
            };
            _panelsForMultipleProducts_List.Add(panel);

            Guna2TextBox textBox;
            int left;

            // Product name
            textBox = CosntructTextBox(0, ProductName_TextBox.Width, TextBoxnames.name.ToString(), CustomControls.KeyPressValidation.None, panel);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames());
            SearchBox.Attach(textBox, this, () => searchResult, searchBoxMaxHeight, false, true);
            AccountantName_TextBox.TextChanged += ValidateInputs;

            CosntructLabel(ProductName_Label.Text, 0, panel);

            // Quantity
            left = textBox.Right + CustomControls.SpaceBetweenControls;
            textBox = CosntructTextBox(left, Quantity_TextBox.Width, TextBoxnames.quantity.ToString(), CustomControls.KeyPressValidation.OnlyNumbers, panel);
            CosntructLabel(Quantity_Label.Text, left, panel);

            // Price per unit
            left = textBox.Right + CustomControls.SpaceBetweenControls;
            textBox = CosntructTextBox(left, PricePerUnit_TextBox.Width, TextBoxnames.pricePerUnit.ToString(), CustomControls.KeyPressValidation.OnlyNumbersAndDecimal, panel);
            CosntructLabel(PricePerUnit_Label.Text, left, panel);

            // Add minus button unless this is the first panel
            left = textBox.Right + CustomControls.SpaceBetweenControls;
            if (_panelsForMultipleProducts_List.Count > 1)
            {
                CosntructMinusButton(new Point(left + CustomControls.SpaceBetweenControls, (_textBoxHeight - _circleButtonHeight) / 2 + textBox.Top), panel);
            }

            _flowPanel.SuspendLayout();
            _flowPanel.Controls.Add(panel);
            SetHeight();
            _flowPanel.ResumeLayout();
            _flowPanel.ScrollControlIntoView(panel);
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
        private Guna2TextBox CosntructTextBox(int left, int width, string name, CustomControls.KeyPressValidation keyPressValidation, Control parent)
        {
            Guna2TextBox textBox = new()
            {
                Size = new Size(width, _textBoxHeight),
                Name = name,
                Location = new Point(left, 28 + CustomControls.SpaceBetweenControls),
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
                case CustomControls.KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case CustomControls.KeyPressValidation.OnlyNumbersAndDecimal:
                    textBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
                    break;
                case CustomControls.KeyPressValidation.OnlyNumbers:
                    textBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case CustomControls.KeyPressValidation.OnlyLetters:
                    textBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
                    break;
                case CustomControls.KeyPressValidation.None:
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
                Size = new Size(_circleButtonHeight, _circleButtonHeight),
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

            _flowPanel.Controls.Remove(panel);
            _panelsForMultipleProducts_List.Remove(panel);
            _flowPanel.Height -= _initialHeightForPanel + _flowPanelMargin;
            SetHeight();
        }
        private Guna2CircleButton _addButton;
        private FlowLayoutPanel _flowPanel;
        private void ConstructFlowPanel()
        {
            int width = _initialWidthForPanel + _spaceOnSidesOfPanel;
            _flowPanel = new()
            {
                Anchor = AnchorStyles.Top,
                AutoScroll = false,
                Location = new Point((ClientSize.Width - width) / 2, 570),
                Size = new Size(width, 20 + CustomControls.SpaceBetweenControls + _textBoxHeight),
                Padding = new Padding(_spaceOnSidesOfPanel / 2, 0, _spaceOnSidesOfPanel / 2, 0),
                Margin = new Padding(_flowPanelMargin / 2, 0, _flowPanelMargin / 2, 0),
                MaximumSize = new Size(width, _flowPanelMaxHeight),
                Visible = false
            };
            _flowPanel.Click += CloseAllPanels;
            Controls.Add(_flowPanel);
        }
        private void CosntructAddButton()
        {
            _addButton = new()
            {
                FillColor = CustomColors.mainBackground,
                BackColor = CustomColors.mainBackground,
                Location = new Point(0, 60),
                Size = new Size(_circleButtonHeight, _circleButtonHeight),
                Image = Resources.AddWhite,
                ImageSize = new Size(32, 32),
                Left = _flowPanel.Left + _spaceOnSidesOfPanel / 2,
                PressedColor = CustomColors.controlBack,
                Visible = false
            };
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                _addButton.Image = Resources.AddWhite;
            }
            else
            {
                _addButton.Image = Resources.AddBlack;
            }
            _addButton.Click += (sender, e) =>
            {
                CloseAllPanels(null, null);
                ConstructControlsForMultipleProducts();
                ValidateInputs(null, null);
            };
            Controls.Add(_addButton);
        }
        private void SetHeight()
        {
            int totalHeight = _panelsForMultipleProducts_List.Sum(panel => panel.Height + _flowPanelMargin);
            _flowPanel.Height = Math.Min(totalHeight + _flowPanelMargin, _flowPanelMaxHeight);
            _flowPanel.AutoScroll = totalHeight + _flowPanelMargin > _flowPanelMaxHeight;

            Height = _flowPanel.Bottom + _extraSpaceForBottom;
            _addButton.Top = _flowPanel.Bottom + spaceBetweenPanels;
        }

        // Warning labels
        private void CheckIfAccountantsExist()
        {
            if (MainMenu_Form.Instance.AccountantList.Count == 0)
            {
                WarningAccountant_LinkLabel.Visible = true;
                WarningAccountant_PictureBox.Visible = true;
            }
            else
            {
                WarningAccountant_LinkLabel.Visible = false;
                WarningAccountant_PictureBox.Visible = false;
            }
        }
        private void CheckIfProductsExist()
        {
            if (MainMenu_Form.Instance.GetCategoryAndProductPurchaseNames().Count == 0)
            {
                WarningProduct_PictureBox.Visible = true;
                WarningProduct_LinkLabel.Visible = true;
            }
            else
            {
                WarningProduct_PictureBox.Visible = false;
                WarningProduct_LinkLabel.Visible = false;
            }
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
                allMultipleFieldsFilled = _panelsForMultipleProducts_List
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