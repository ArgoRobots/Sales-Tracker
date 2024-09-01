using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class AddSale_Form : Form
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
        public AddSale_Form()
        {
            InitializeComponent();

            LoadingPanel.ShowBlankLoadingPanel(this);

            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            Date_DateTimePicker.Value = DateTime.Now;
            CheckIfProductsExist();
            CheckIfBuyersExist();
            Theme.SetThemeForForm(this);
            RemoveReceiptLabel();
            AmountCredited_Label.Text = $"{MainMenu_Form.CurrencySymbol} credited ({Properties.Settings.Default.Currency})";

            // Despite this being the default, it's still needed for some reason
            RemoveReceipt_ImageButton.HoverState.ImageSize = new Size(20, 20);
            RemoveReceipt_ImageButton.PressedState.ImageSize = new Size(20, 20);
        }
        private void AddEventHandlersToTextBoxes()
        {
            SaleNumber_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            SaleNumber_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            SaleNumber_TextBox.KeyDown += UI.TextBox_KeyDown;

            AccountantName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            AccountantName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            AccountantName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            AccountantName_TextBox.KeyDown += UI.TextBox_KeyDown;

            ProductName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            ProductName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            ProductName_TextBox.KeyDown += UI.TextBox_KeyDown;

            CountryOfDestinaion_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            CountryOfDestinaion_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            CountryOfDestinaion_TextBox.KeyDown += UI.TextBox_KeyDown;

            Quantity_TextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
            Quantity_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            Quantity_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Quantity_TextBox.KeyDown += UI.TextBox_KeyDown;

            PricePerUnit_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            PricePerUnit_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            PricePerUnit_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            PricePerUnit_TextBox.KeyDown += UI.TextBox_KeyDown;

            Shipping_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Shipping_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            Shipping_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Shipping_TextBox.KeyDown += UI.TextBox_KeyDown;

            Tax_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Tax_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            Tax_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Tax_TextBox.KeyDown += UI.TextBox_KeyDown;

            PaymentFee_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            PaymentFee_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            PaymentFee_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            PaymentFee_TextBox.KeyDown += UI.TextBox_KeyDown;

            Discount_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            Discount_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            Discount_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Discount_TextBox.KeyDown += UI.TextBox_KeyDown;

            AmountCredited_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            AmountCredited_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            AmountCredited_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            AmountCredited_TextBox.KeyDown += UI.TextBox_KeyDown;

            Notes_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            Notes_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Notes_TextBox.KeyDown += UI.TextBox_KeyDown;
        }
        private void AddSearchBoxEvents()
        {
            byte searchBoxMaxHeight = 255;

            AccountantName_TextBox.Click += (sender, e) => { ShowSearchBox(AccountantName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            AccountantName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(AccountantName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            AccountantName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, AccountantName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), this, searchBoxMaxHeight); };
            AccountantName_TextBox.TextChanged += ValidateInputs;
            AccountantName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            AccountantName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(AccountantName_TextBox, this, AddSale_Label, e); };

            ProductName_TextBox.Click += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), searchBoxMaxHeight); };
            ProductName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), searchBoxMaxHeight); };
            ProductName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), this, searchBoxMaxHeight); };
            ProductName_TextBox.TextChanged += ValidateInputs;
            ProductName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ProductName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductName_TextBox, this, AddSale_Label, e); };

            CountryOfDestinaion_TextBox.Click += (sender, e) => { SearchBox.ShowSearchBox(this, CountryOfDestinaion_TextBox, Country.countries, this, searchBoxMaxHeight); };
            CountryOfDestinaion_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, CountryOfDestinaion_TextBox, Country.countries, this, searchBoxMaxHeight); };
            CountryOfDestinaion_TextBox.TextChanged += ValidateInputs;
            CountryOfDestinaion_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            CountryOfDestinaion_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(CountryOfDestinaion_TextBox, this, AddSale_Label, e); };
        }
        private void ShowSearchBox(Guna2TextBox gTextBox, List<SearchResult> results, int maxHeight)
        {
            SearchBox.ShowSearchBox(this, gTextBox, results, this, maxHeight);
        }

        // Form event handlers
        private void AddSale_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddSale_Button_Click(object sender, EventArgs e)
        {
            if (MainMenu_Form.Instance.Selected != MainMenu_Form.SelectedOption.Sales)
            {
                MainMenu_Form.Instance.Sales_Button.PerformClick();
            }
            MainMenu_Form.Instance.selectedDataGridView = MainMenu_Form.Instance.Sales_DataGridView;

            if (panelsForMultipleProducts_List.Count == 0)
            {
                if (!AddSingleSale()) { return; }
            }
            // When the user selects "multiple items in this order" but only adds one, treat it as one
            else if (panelsForMultipleProducts_List.Count == 1)
            {
                // Extract details from the single panel and populate the single purchase fields
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
            new Products_Form(false).ShowDialog();
            CheckIfProductsExist();
        }
        private void WarningBuyer_LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Accountants_Form().ShowDialog();
            CheckIfBuyersExist();
        }
        private void Receipt_Button_Click(object sender, EventArgs e)
        {
            // Select file
            OpenFileDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                receiptFilePath = dialog.FileName;
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
        private void ImportExcel_Button_Click(object sender, EventArgs e)
        {
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
            if (saleNumber != "-" && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), saleNumber))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            string buyerName = AccountantName_TextBox.Text;
            string itemName = ProductName_TextBox.Text;
            string country = CountryOfDestinaion_TextBox.Text;
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal discount = decimal.Parse(Discount_TextBox.Text);
            string categoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categorySaleList, itemName);
            string company = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categorySaleList, itemName);
            decimal totalPrice = quantity * pricePerUnit - discount;
            string noteLabel = MainMenu_Form.emptyCell;
            string note = Notes_TextBox.Text.Trim();
            if (note != "")
            {
                noteLabel = MainMenu_Form.show_text;
            }

            // Round to 2 decimal places
            decimal amountCharged = decimal.Parse(AmountCredited_TextBox.Text);
            totalPrice = Math.Round(totalPrice, 2);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string message = $"Amount charged ({MainMenu_Form.CurrencySymbol}{amountCharged}) is not equal to the total price of the purchase (${totalPrice}). The difference will be accounted for.";
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }
            totalPrice += chargedDifference;

            string newFilePath = "";
            if (Controls.Contains(SelectedReceipt_Label))
            {
                if (!MainMenu_Form.SaveReceiptInFile(receiptFilePath, out newFilePath))
                {
                    return false;
                }
            }
            if (MainMenu_Form.CheckIfReceiptExists(newFilePath))
            {
                return false;
            }

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                saleNumber,
                buyerName,
                itemName,
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
                chargedDifference.ToString("N2"),
                totalPrice.ToString("N"),
                noteLabel
            );
            if (noteLabel == MainMenu_Form.show_text)
            {
                MainMenu_Form.AddNoteToCell(newRowIndex, note);
            }
            if (newFilePath != "")
            {
                MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = newFilePath;
            }

            MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, itemName);
            Log.Write(3, $"Added Sale '{itemName}'");

            return true;
        }
        private bool AddMultipleSales()
        {
            bool isCategoryNameConsistent = true, isCountryConsistent = true, isCompanyConsistent = true;

            string saleNumber = SaleNumber_TextBox.Text.Trim();

            // Check if sale ID already exists
            if (saleNumber != "-" && MainMenu_Form.DoesValueExistInDataGridView(MainMenu_Form.Instance.Sales_DataGridView, MainMenu_Form.Column.OrderNumber.ToString(), saleNumber))
            {
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", $"The sale #{saleNumber} already exists. Would you like to add this sale anyways?", CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                if (result != CustomMessageBoxResult.Yes)
                {
                    return false;
                }
            }

            string sellerName = AccountantName_TextBox.Text;
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

            List<string> items = new();

            string firstCategoryName = null, firstCountry = null, firstCompany = null;
            decimal totalPrice = 0;
            int totalQuantity = 0;

            foreach (Guna2Panel panel in panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string itemName = nameTextBox.Text.Trim();

                string currentCategoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categorySaleList, itemName);
                string currentCountry = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categorySaleList, itemName);
                string currentCompany = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categorySaleList, itemName);

                if (firstCategoryName == null)
                {
                    firstCategoryName = currentCategoryName;
                }
                else if (isCategoryNameConsistent && firstCategoryName != currentCategoryName)
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
                totalPrice += quantity * pricePerUnit;
                totalQuantity += quantity;

                string item = string.Join(",",
                    itemName,
                    currentCategoryName,
                    currentCountry,
                    currentCompany,
                    quantity.ToString(),
                    pricePerUnit.ToString("N2"),
                    (quantity * pricePerUnit).ToString("N2")
                );

                items.Add(item);
            }

            totalPrice -= discount;
            totalPrice = Math.Round(totalPrice, 2);

            decimal amountCharged = decimal.Parse(AmountCredited_TextBox.Text);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string message = $"Amount credited ({MainMenu_Form.CurrencySymbol}{amountCharged}) is not equal to the total price of the sale (${totalPrice}). The difference will be accounted for.";
                CustomMessageBoxResult result = CustomMessageBox.Show("Argo Sales Tracker", message, CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.OkCancel);

                if (result != CustomMessageBoxResult.Ok)
                {
                    return false;
                }
            }
            totalPrice += chargedDifference;

            string newFilePath = "";
            if (Controls.Contains(SelectedReceipt_Label))
            {
                if (!MainMenu_Form.SaveReceiptInFile(receiptFilePath, out newFilePath))
                {
                    return false;
                }
            }
            if (MainMenu_Form.CheckIfReceiptExists(newFilePath))
            {
                return false;
            }

            string finalCategoryName = isCategoryNameConsistent ? firstCategoryName : MainMenu_Form.emptyCell;
            string finalCountry = isCountryConsistent ? firstCountry : MainMenu_Form.emptyCell;
            string finalCompany = isCompanyConsistent ? firstCompany : MainMenu_Form.emptyCell;

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                saleNumber,
                sellerName,
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
            items.Add(MainMenu_Form.receipt_text + newFilePath);

            MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = items;

            MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(ThingsThatHaveChangedInFile, saleNumber);
            Log.Write(3, $"Added sale '{saleNumber}' with '{totalQuantity}' items");

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
            SaleNumber_TextBox.Left = (Width - SaleNumber_TextBox.Width - UI.spaceBetweenControls -
                AccountantName_TextBox.Width - UI.spaceBetweenControls -
                ProductName_TextBox.Width - UI.spaceBetweenControls -
                CountryOfDestinaion_TextBox.Width - UI.spaceBetweenControls -
                Receipt_Button.Width) / 2 - UI.spaceToOffsetFormNotCenter;

            SaleNumber_Label.Left = SaleNumber_TextBox.Left;
            AccountantName_TextBox.Left = SaleNumber_TextBox.Right + UI.spaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            ProductName_TextBox.Left = AccountantName_TextBox.Right + UI.spaceBetweenControls;
            ProductName_Label.Left = ProductName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = ProductName_TextBox.Right + UI.spaceBetweenControls;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;
            Receipt_Button.Left = CountryOfDestinaion_TextBox.Right + UI.spaceBetweenControls;

            Date_DateTimePicker.Left = (Width - Date_DateTimePicker.Width - UI.spaceBetweenControls -
                Quantity_TextBox.Width - UI.spaceBetweenControls -
                PricePerUnit_TextBox.Width - UI.spaceBetweenControls -
                Shipping_TextBox.Width - UI.spaceBetweenControls -
                Tax_TextBox.Width - UI.spaceBetweenControls -
                PaymentFee_TextBox.Width - UI.spaceBetweenControls -
                Discount_TextBox.Width - UI.spaceBetweenControls -
                AmountCredited_TextBox.Width) / 2 - UI.spaceToOffsetFormNotCenter;

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
            PaymentFee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + UI.spaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            AmountCredited_TextBox.Left = Discount_TextBox.Right + UI.spaceBetweenControls;
            AmountCredited_Label.Left = AmountCredited_TextBox.Left;

            // Add controls
            List<Control> controls = GetControlsForMultipleProducts();
            foreach (Control control in controls)
            {
                Controls.Add(control);
            }

            Controls.Remove(FlowPanel);
            Controls.Remove(AddButton);
            Height = 465;

            RelocateBuyerWarning();

            if (Controls.Contains(WarningProduct_PictureBox))
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + UI.spaceBetweenControls);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + UI.spaceBetweenControls, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            SaleNumber_TextBox.Left = (Width - SaleNumber_TextBox.Width - UI.spaceBetweenControls -
                AccountantName_TextBox.Width - UI.spaceBetweenControls -
                CountryOfDestinaion_TextBox.Width - UI.spaceBetweenControls -
                Receipt_Button.Width) / 2 - UI.spaceToOffsetFormNotCenter;

            SaleNumber_Label.Left = SaleNumber_TextBox.Left;
            AccountantName_TextBox.Left = SaleNumber_TextBox.Right + UI.spaceBetweenControls;
            AccountantName_Label.Left = AccountantName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = AccountantName_TextBox.Right + UI.spaceBetweenControls;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;
            Receipt_Button.Left = CountryOfDestinaion_TextBox.Right + UI.spaceBetweenControls;

            Date_DateTimePicker.Left = (Width -
                Date_DateTimePicker.Width - UI.spaceBetweenControls -
                Shipping_TextBox.Width - UI.spaceBetweenControls -
                Tax_TextBox.Width - UI.spaceBetweenControls -
                PaymentFee_TextBox.Width - UI.spaceBetweenControls -
                Discount_TextBox.Width - UI.spaceBetweenControls -
                AmountCredited_TextBox.Width) / 2 - UI.spaceToOffsetFormNotCenter;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + UI.spaceBetweenControls;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + UI.spaceBetweenControls;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + UI.spaceBetweenControls;
            PaymentFee_Label.Left = PaymentFee_TextBox.Left;
            Discount_TextBox.Left = PaymentFee_TextBox.Right + UI.spaceBetweenControls;
            Discount_Label.Left = Discount_TextBox.Left;
            AmountCredited_TextBox.Left = Discount_TextBox.Right + UI.spaceBetweenControls;
            AmountCredited_Label.Left = AmountCredited_TextBox.Left;

            // Remove controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Remove(control);
            }

            Controls.Add(FlowPanel);
            SetHeight();

            RelocateBuyerWarning();

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
            byte smallSearchBoxMaxHeight = 150;

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
            textBox.Click -= CloseAllPanels;
            textBox.Click += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), smallSearchBoxMaxHeight);
            };
            textBox.GotFocus += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), smallSearchBoxMaxHeight);
            };
            textBox.TextChanged += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                SearchBox.SearchTextBoxChanged(this, searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), this, smallSearchBoxMaxHeight);
            };
            textBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(textBox, this, AddSale_Label, e); };

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
                BorderColor = CustomColors.controlBorder,
                FillColor = CustomColors.controlBack,
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
            textBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            textBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            textBox.KeyDown += UI.TextBox_KeyDown;

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
                Location = new Point((Width - width) / 2, 570),
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
            if (MainMenu_Form.Instance.GetProductSaleNames().Count == 0)
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
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(SaleNumber_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(AccountantName_TextBox.Text) && AccountantName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PaymentFee_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(CountryOfDestinaion_TextBox.Text) && CountryOfDestinaion_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Discount_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(AmountCredited_TextBox.Text);

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
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}