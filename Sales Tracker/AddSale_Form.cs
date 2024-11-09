using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Properties;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class AddSale_Form : Form
    {
        // Properties
        private static readonly List<string> _thingsThatHaveChangedInFile = [];

        // Getters and setters
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;

        // Init.
        public AddSale_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            AddEventHandlersToTextBoxes();
            Date_DateTimePicker.Value = DateTime.Now;
            CheckIfProductsExist();
            CheckIfAccountantsExist();
            Theme.SetThemeForForm(this);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            RemoveReceiptLabel();
            string currency = DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType);
            Credited_Label.Text = $"{MainMenu_Form.CurrencySymbol} credited ({currency})";
        }
        private void AddEventHandlersToTextBoxes()
        {
            byte searchBoxMaxHeight = 255;

            TextBoxManager.Attach(SaleNumber_TextBox);

            AccountantName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(AccountantName_TextBox);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.AccountantList);
            SearchBox.Attach(AccountantName_TextBox, this, () => searchResult, searchBoxMaxHeight);
            AccountantName_TextBox.TextChanged += ValidateInputs;

            TextBoxManager.Attach(ProductName_TextBox);
            List<SearchResult> searchResult1 = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductSaleNames());
            SearchBox.Attach(ProductName_TextBox, this, () => searchResult1, searchBoxMaxHeight, false, true);
            ProductName_TextBox.TextChanged += ValidateInputs;

            TextBoxManager.Attach(CountryOfDestinaion_TextBox);
            SearchBox.Attach(CountryOfDestinaion_TextBox, this, () => Country.countries, searchBoxMaxHeight);
            CountryOfDestinaion_TextBox.TextChanged += ValidateInputs;

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

            Credited_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            TextBoxManager.Attach(Credited_TextBox);

            TextBoxManager.Attach(Notes_TextBox);
        }
        private void SetAccessibleDescriptions()
        {
            Label[] labelsToAlignLeftCenter =
            [
               MultipleItems_Label,
               SaleNumber_Label,
               AccountantName_Label,
               ProductName_Label,
               Date_Label,
               Quantity_Label,
               PricePerUnit_Label,
               Shipping_Label,
               Tax_Label,
               Discount_Label,
               Credited_Label,
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
        private void AddSale_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);

            if (MainMenu_Form.Instance.Selected != MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.Sales_Button.PerformClick();
            }
            MainMenu_Form.Instance.SelectedDataGridView = MainMenu_Form.Instance.Sale_DataGridView;

            if (panelsForMultipleProducts_List.Count == 0 || !MultipleItems_CheckBox.Checked)
            {
                if (!AddSingleSale()) { return; }
            }
            // When the user selects "multiple items in this order" but only adds one, treat it as one
            else if (panelsForMultipleProducts_List.Count == 1)
            {
                // Extract details from the single panel and populate the single sale fields
                Guna2Panel singlePanel = panelsForMultipleProducts_List[0];
                ProductName_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault()).Text;
                Quantity_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.quantity.ToString(), false).FirstOrDefault()).Text;
                PricePerUnit_TextBox.Text = ((Guna2TextBox)singlePanel.Controls.Find(TextBoxnames.pricePerUnit.ToString(), false).FirstOrDefault()).Text;

                if (!AddSingleSale()) { return; }
            }
            else if (!AddMultipleSales())
            {
                return;
            }

            // Reset
            RemoveReceiptLabel();
            SaleNumber_TextBox.Text = "";
        }
        private void MultipleItems_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            if (MultipleItems_CheckBox.Checked)
            {
                if (addButton == null)
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
            new Products_Form(false).ShowDialog();
            CheckIfProductsExist();
        }
        private void WarningAccountant_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
                receiptFilePath = ReadOnlyVariables.Receipt_text + dialog.FileName;
                ShowReceiptLabel(dialog.SafeFileName);
            }
        }
        private void RemoveReceipt_ImageButton_Click(object sender, EventArgs e)
        {
            RemoveReceiptLabel();
        }
        private void RemoveReceipt_ImageButton_MouseEnter(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.FileHover;
        }
        private void RemoveReceipt_ImageButton_MouseLeave(object sender, EventArgs e)
        {
            RemoveReceipt_ImageButton.BackColor = CustomColors.MainBackground;
        }
        private void MultipleItems_Label_Click(object sender, EventArgs e)
        {
            MultipleItems_CheckBox.Checked = !MultipleItems_CheckBox.Checked;
        }

        // Methods to add sales
        private bool AddSingleSale()
        {
            string saleNumber = SaleNumber_TextBox.Text.Trim();

            // Check if sale ID already exists
            if (saleNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Column.ID.ToString(), saleNumber))
            {
                string message = $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?";
                CustomMessageBoxResult result = CustomMessageBox.Show("Sale # already exists", message, CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            // Get values from TextBoxes
            string accountant = AccountantName_TextBox.Text;

            string[] items = ProductName_TextBox.Text.Split('>');
            string categoryName = items[0].Trim();
            string productName = items[1].Trim();

            string country = CountryOfDestinaion_TextBox.Text;
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal discount = decimal.Parse(Discount_TextBox.Text);
            string company = MainMenu_Form.GetCompanyProductIsFrom(MainMenu_Form.Instance.CategorySaleList, productName);
            decimal totalPrice = Math.Round(quantity * pricePerUnit - discount, 2);
            string noteLabel = ReadOnlyVariables.EmptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = ReadOnlyVariables.Show_text;
            }

            decimal credited = Math.Round(decimal.Parse(Credited_TextBox.Text), 2);
            decimal creditedDifference = credited - totalPrice;

            if (creditedDifference != 0)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Amount credited is different",
                    $"Amount credited ({MainMenu_Form.CurrencySymbol}{credited}) is not equal to the total price of the sale (${totalPrice}). The difference will be accounted for.",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }

            // Convert default currency to USD
            decimal exchangeRateToUSD = Currency.GetExchangeRate(DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType), "USD", date);
            if (exchangeRateToUSD == -1) { return false; }

            decimal pricePerUnitUSD = Math.Round(pricePerUnit * exchangeRateToUSD, 2);
            decimal shippingUSD = Math.Round(shipping * exchangeRateToUSD, 2);
            decimal taxUSD = Math.Round(tax * exchangeRateToUSD, 2);
            decimal feeUSD = Math.Round(fee * exchangeRateToUSD, 2);
            decimal discountUSD = Math.Round(discount * exchangeRateToUSD, 2);
            decimal chargedDifferenceUSD = Math.Round(creditedDifference * exchangeRateToUSD, 2);
            decimal creditedUSD = Math.Round(credited * exchangeRateToUSD, 2);

            // Store the USD values in the tag
            TagData saleData = new()
            {
                PricePerUnitUSD = pricePerUnitUSD,
                ShippingUSD = shippingUSD,
                TaxUSD = taxUSD,
                FeeUSD = feeUSD,
                DiscountUSD = discountUSD,
                ChargedDifferenceUSD = chargedDifferenceUSD,
                ChargedOrCreditedUSD = creditedUSD
            };

            // Save the receipt
            string newFilePath = "";
            if (!ReceiptsManager.CheckIfReceiptExists(receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = ReceiptsManager.SaveReceiptInFile(receiptFilePath);

                if (!saved)
                {
                    return false;
                }
            }

            // Add the row with the default values
            int newRowIndex = MainMenu_Form.Instance.SelectedDataGridView.Rows.Add(
                saleNumber,
                accountant,
                productName,
                categoryName,
                country,
                company,
                date,
                quantity.ToString(),
                pricePerUnit.ToString("N"),
                shipping.ToString("N"),
                tax.ToString("N"),
                fee.ToString("N"),
                discount.ToString("N"),
                creditedDifference.ToString("N2"),
                credited.ToString("N"),
                noteLabel
            );
            if (noteLabel == ReadOnlyVariables.Show_text)
            {
                DataGridViewManager.AddNoteToCell(newRowIndex, note);
            }

            // Set the tag
            if (newFilePath != "")
            {
                MainMenu_Form.Instance.SelectedDataGridView.Rows[newRowIndex].Tag = (newFilePath, saleData);
            }

            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            string logMessage = $"Added Sale '{saleNumber}'";
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, logMessage);
            Log.Write(3, logMessage);

            return true;
        }
        private bool AddMultipleSales()
        {
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;

            string saleNumber = SaleNumber_TextBox.Text.Trim();

            // Check if sale ID already exists
            if (saleNumber != ReadOnlyVariables.EmptyCell && DataGridViewManager.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.Column.ID.ToString(), saleNumber))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Sale # already exists",
                    $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            // Get values from TextBoxes
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

            List<string> items = [];

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            decimal totalPrice = 0;
            int totalQuantity = 0;

            // Convert default currency to USD
            decimal exchangeRateToUSD = Currency.GetExchangeRate(DataFileManager.GetValue(DataFileManager.AppDataSettings.DefaultCurrencyType), "USD", date);
            if (exchangeRateToUSD == -1) { return false; }

            foreach (Guna2Panel panel in panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string[] itemsInName = nameTextBox.Text.Split('>');
                string categoryName = itemsInName[0].Trim();
                string productName = itemsInName[1].Trim();

                string currentCountry = MainMenu_Form.GetCountryProductIsFrom(MainMenu_Form.Instance.CategorySaleList, productName);
                string currentCompany = MainMenu_Form.GetCompanyProductIsFrom(MainMenu_Form.Instance.CategorySaleList, productName);

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
                    pricePerUnit.ToString("F2"),
                    pricePerUnitUSD.ToString("F2")
                );

                items.Add(item);
            }

            totalPrice = Math.Round(totalPrice - discount, 2);
            decimal credited = Math.Round(decimal.Parse(Credited_TextBox.Text), 2);
            decimal creditedDifference = credited - totalPrice;

            if (creditedDifference != 0)
            {
                string message = $"Amount credited ({MainMenu_Form.CurrencySymbol}{credited}) is not equal to the total price of the sale (${totalPrice}). The difference will be accounted for.";
                CustomMessageBoxResult result = CustomMessageBox.Show("Amount credited is different", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }
            totalPrice += creditedDifference;

            string newFilePath = "";
            if (!ReceiptsManager.CheckIfReceiptExists(receiptFilePath))
            {
                return false;
            }
            if (Controls.Contains(SelectedReceipt_Label))
            {
                (newFilePath, bool saved) = ReceiptsManager.SaveReceiptInFile(receiptFilePath);

                if (!saved)
                {
                    return false;
                }
            }

            string finalCategoryName = isCategoryNameConsistent ? firstCategoryName : ReadOnlyVariables.EmptyCell;
            string finalCountry = isCountryConsistent ? firstCountry : ReadOnlyVariables.EmptyCell;
            string finalCompany = isCompanyConsistent ? firstCompany : ReadOnlyVariables.EmptyCell;

            int newRowIndex = MainMenu_Form.Instance.SelectedDataGridView.Rows.Add(
                saleNumber,
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
                creditedDifference.ToString("N2"),
                totalPrice.ToString("N2"),
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
            decimal shippingUSD = Math.Round(shipping * exchangeRateToUSD, 2);
            decimal taxUSD = Math.Round(tax * exchangeRateToUSD, 2);
            decimal feeUSD = Math.Round(fee * exchangeRateToUSD, 2);
            decimal discountUSD = Math.Round(discount * exchangeRateToUSD, 2);
            decimal chargedDifferenceUSD = Math.Round(creditedDifference * exchangeRateToUSD, 2);
            decimal creditedUSD = Math.Round(credited * exchangeRateToUSD, 2);

            // Store the money values in the tag
            TagData tagData = new()
            {
                ShippingUSD = shippingUSD,
                TaxUSD = taxUSD,
                FeeUSD = feeUSD,
                DiscountUSD = discountUSD,
                ChargedDifferenceUSD = chargedDifferenceUSD,
                ChargedOrCreditedUSD = creditedUSD
            };

            // Set the tag
            MainMenu_Form.Instance.SelectedDataGridView.Rows[newRowIndex].Tag = (items, tagData);

            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            string logMessage = $"Added sale '{saleNumber}' with '{totalQuantity}' items";
            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, logMessage);
            Log.Write(3, logMessage);

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
        private readonly byte textBoxHeight = 48, circleButtonHeight = 38, extraSpaceForBottom = 210, spaceBetweenPanels = 10,
               initialHeightForPanel = 88, spaceOnSidesOfPanel = 100, flowPanelMargin = 6;
        private readonly short initialWidthForPanel = 673, flowPanelMaxHeight = 300;
        private void SetControlsForSingleProduct()
        {
            // Center controls
            SaleNumber_TextBox.Left = ((ClientSize.Width - SaleNumber_TextBox.Width - CustomControls.SpaceBetweenControls -
                AccountantName_TextBox.Width - CustomControls.SpaceBetweenControls -
                ProductName_TextBox.Width - CustomControls.SpaceBetweenControls -
                CountryOfDestinaion_TextBox.Width - CustomControls.SpaceBetweenControls -
                Receipt_Button.Width) / 2);

            SaleNumber_Label.Left = SaleNumber_TextBox.Left;
            AccountantName_TextBox.Left = SaleNumber_TextBox.Right + CustomControls.SpaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            ProductName_TextBox.Left = AccountantName_TextBox.Right + CustomControls.SpaceBetweenControls;
            ProductName_Label.Left = ProductName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = ProductName_TextBox.Right + CustomControls.SpaceBetweenControls;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;
            Receipt_Button.Left = CountryOfDestinaion_TextBox.Right + CustomControls.SpaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width - Date_DateTimePicker.Width - CustomControls.SpaceBetweenControls -
                Quantity_TextBox.Width - CustomControls.SpaceBetweenControls -
                PricePerUnit_TextBox.Width - CustomControls.SpaceBetweenControls -
                Shipping_TextBox.Width - CustomControls.SpaceBetweenControls -
                Tax_TextBox.Width - CustomControls.SpaceBetweenControls -
                PaymentFee_TextBox.Width - CustomControls.SpaceBetweenControls -
                Discount_TextBox.Width - CustomControls.SpaceBetweenControls -
                Credited_TextBox.Width) / 2;

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
            Credited_TextBox.Left = Discount_TextBox.Right + CustomControls.SpaceBetweenControls;
            Credited_Label.Left = Credited_TextBox.Left;

            // Add controls
            List<Control> controls = GetControlsForMultipleProducts();
            foreach (Control control in controls)
            {
                Controls.Add(control);
            }

            flowPanel.Visible = false;
            addButton.Visible = false;
            MinimumSize = new Size(Width, 695);
            Size = MinimumSize;

            RelocateAccountantWarning();

            if (WarningProduct_PictureBox.Visible)
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + CustomControls.SpaceBetweenControls);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + CustomControls.SpaceBetweenControls, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            SaleNumber_TextBox.Left = (ClientSize.Width - SaleNumber_TextBox.Width - CustomControls.SpaceBetweenControls -
                AccountantName_TextBox.Width - CustomControls.SpaceBetweenControls -
                CountryOfDestinaion_TextBox.Width - CustomControls.SpaceBetweenControls -
                Receipt_Button.Width) / 2;

            SaleNumber_Label.Left = SaleNumber_TextBox.Left;
            AccountantName_TextBox.Left = SaleNumber_TextBox.Right + CustomControls.SpaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = AccountantName_TextBox.Right + CustomControls.SpaceBetweenControls;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;
            Receipt_Button.Left = CountryOfDestinaion_TextBox.Right + CustomControls.SpaceBetweenControls;

            Date_DateTimePicker.Left = (ClientSize.Width -
                Date_DateTimePicker.Width - CustomControls.SpaceBetweenControls -
                Shipping_TextBox.Width - CustomControls.SpaceBetweenControls -
                Tax_TextBox.Width - CustomControls.SpaceBetweenControls -
                PaymentFee_TextBox.Width - CustomControls.SpaceBetweenControls -
                Discount_TextBox.Width - CustomControls.SpaceBetweenControls -
                Credited_TextBox.Width) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + CustomControls.SpaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + CustomControls.SpaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + CustomControls.SpaceBetweenControls;
            Fee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + CustomControls.SpaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            Credited_TextBox.Left = Discount_TextBox.Right + CustomControls.SpaceBetweenControls;
            Credited_Label.Left = Credited_TextBox.Left;

            // Remove controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Remove(control);
            }

            flowPanel.Visible = true;
            SetHeight();

            RelocateAccountantWarning();

            if (WarningProduct_PictureBox.Visible)
            {
                WarningProduct_PictureBox.Location = new Point(addButton.Left + CustomControls.SpaceBetweenControls, addButton.Top - flowPanelMargin * 2);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + CustomControls.SpaceBetweenControls, WarningProduct_PictureBox.Top);
                addButton.Visible = false;
            }
            else
            {
                addButton.Visible = true;
            }
        }
        private void RelocateAccountantWarning()
        {
            WarningAccountant_PictureBox.Location = new Point(AccountantName_TextBox.Left, AccountantName_TextBox.Bottom + CustomControls.SpaceBetweenControls);
            WarningAccountant_LinkLabel.Location = new Point(WarningAccountant_PictureBox.Right + CustomControls.SpaceBetweenControls, WarningAccountant_PictureBox.Top);
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
                FillColor = CustomColors.MainBackground
            };
            panelsForMultipleProducts_List.Add(panel);

            Guna2TextBox textBox;
            int left;

            // Product name
            textBox = CosntructTextBox(0, ProductName_TextBox.Width, TextBoxnames.name.ToString(), CustomControls.KeyPressValidation.None, panel);
            List<SearchResult> searchResult = SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetCategoryAndProductSaleNames());
            SearchBox.Attach(textBox, this, () => searchResult, searchBoxMaxHeight, false, true);

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
            if (panelsForMultipleProducts_List.Count > 1)
            {
                CosntructMinusButton(new Point(left + CustomControls.SpaceBetweenControls, (textBoxHeight - circleButtonHeight) / 2 + textBox.Top), panel);
            }

            flowPanel.SuspendLayout();
            flowPanel.Controls.Add(panel);
            SetHeight();
            flowPanel.ResumeLayout();
            flowPanel.ScrollControlIntoView(panel);
        }
        private void CosntructLabel(string text, int left, Control parent)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.Text,
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
                Size = new Size(width, textBoxHeight),
                Name = name,
                Location = new Point(left, 28 + CustomControls.SpaceBetweenControls),
                BorderColor = CustomColors.ControlBorder,
                FillColor = CustomColors.ControlBack,
                ForeColor = CustomColors.Text,
                ShortcutsEnabled = false
            };
            textBox.HoverState.BorderColor = CustomColors.AccentBlue;
            textBox.FocusedState.BorderColor = CustomColors.AccentBlue;
            textBox.FocusedState.FillColor = CustomColors.ControlBack;

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
                FillColor = CustomColors.MainBackground,
                BackColor = CustomColors.MainBackground,
                Location = location,
                Size = new Size(circleButtonHeight, circleButtonHeight),
                ImageSize = new Size(32, 32),
                PressedColor = CustomColors.ControlBack
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

            flowPanel.Controls.Remove(panel);
            panelsForMultipleProducts_List.Remove(panel);
            flowPanel.Height -= initialHeightForPanel + flowPanelMargin;
            SetHeight();
        }
        private Guna2CircleButton addButton;
        private FlowLayoutPanel flowPanel;
        private void ConstructFlowPanel()
        {
            int width = initialWidthForPanel + spaceOnSidesOfPanel;
            flowPanel = new()
            {
                Anchor = AnchorStyles.Top,
                AutoScroll = false,
                Location = new Point((ClientSize.Width - width) / 2, 570),
                Size = new Size(width, 20 + CustomControls.SpaceBetweenControls + textBoxHeight),
                Padding = new Padding(spaceOnSidesOfPanel / 2, 0, spaceOnSidesOfPanel / 2, 0),
                Margin = new Padding(flowPanelMargin / 2, 0, flowPanelMargin / 2, 0),
                MaximumSize = new Size(width, flowPanelMaxHeight),
                Visible = false
            };
            flowPanel.Click += CloseAllPanels;
            Controls.Add(flowPanel);
        }
        private void CosntructAddButton()
        {
            addButton = new()
            {
                FillColor = CustomColors.MainBackground,
                BackColor = CustomColors.MainBackground,
                Location = new Point(0, 60),
                Size = new Size(circleButtonHeight, circleButtonHeight),
                ImageSize = new Size(32, 32),
                Left = flowPanel.Left + spaceOnSidesOfPanel / 2,
                PressedColor = CustomColors.ControlBack,
                Visible = false,
                Anchor = AnchorStyles.Top
            };
            if (Theme.CurrentTheme == Theme.ThemeType.Dark)
            {
                addButton.Image = Resources.AddWhite;
            }
            else
            {
                addButton.Image = Resources.AddBlack;
            }
            addButton.Click += delegate
            {
                CloseAllPanels(null, null);
                ConstructControlsForMultipleProducts();
                ValidateInputs(null, null);
            };
            Controls.Add(addButton);
        }
        private void SetHeight()
        {
            int totalHeight = panelsForMultipleProducts_List.Sum(panel => panel.Height + flowPanelMargin);
            flowPanel.Height = Math.Min(totalHeight + flowPanelMargin, flowPanelMaxHeight);
            flowPanel.AutoScroll = totalHeight + flowPanelMargin > flowPanelMaxHeight;

            MinimumSize = new Size(Width, flowPanel.Bottom + extraSpaceForBottom);
            addButton.Top = flowPanel.Bottom + spaceBetweenPanels;
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
            if (MainMenu_Form.Instance.GetCategoryAndProductSaleNames().Count == 0)
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
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(SaleNumber_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(AccountantName_TextBox.Text) && AccountantName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PaymentFee_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(CountryOfDestinaion_TextBox.Text) && CountryOfDestinaion_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Discount_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Credited_TextBox.Text);

            if (Properties.Settings.Default.SalesReceipts)
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
            AddSale_Button.Enabled = allFieldsFilled && allMultipleFieldsFilled;
        }
        private void CloseAllPanels(object sender, EventArgs e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}