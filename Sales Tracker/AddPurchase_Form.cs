using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class AddPurchase_Form : BaseForm
    {
        // Properties
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init.
        public AddPurchase_Form()
        {
            InitializeComponent();

            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            Date_DateTimePicker.Value = DateTime.Now;
            Currency_ComboBox.DataSource = Enum.GetValues(typeof(Currency.CurrencyTypes));
            CheckIfProductsExist();
            CheckIfBuyersExist();
            Theme.SetThemeForForm(this);
            RemoveReceiptLabel();

            // Despite this being the default, it's still needed for some reason
            RemoveReceipt_ImageButton.HoverState.ImageSize = new Size(20, 20);
            RemoveReceipt_ImageButton.PressedState.ImageSize = new Size(20, 20);
        }
        private void AddEventHandlersToTextBoxes()
        {
            OrderNumber_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            OrderNumber_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            OrderNumber_TextBox.KeyDown += UI.TextBox_KeyDown;

            BuyerName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            BuyerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            BuyerName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            BuyerName_TextBox.KeyDown += UI.TextBox_KeyDown;

            ProductName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            ProductName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            ProductName_TextBox.KeyDown += UI.TextBox_KeyDown;

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

            AmountCharged_TextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox;
            AmountCharged_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            AmountCharged_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            AmountCharged_TextBox.KeyDown += UI.TextBox_KeyDown;
        }
        private void AddSearchBoxEvents()
        {
            byte searchBoxMaxHeight = 150;
            BuyerName_TextBox.Click += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            BuyerName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            BuyerName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), this, searchBoxMaxHeight); };
            BuyerName_TextBox.TextChanged += ValidateInputs;
            BuyerName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            BuyerName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(BuyerName_TextBox, this, AddPurchase_Label, e); };

            ProductName_TextBox.Click += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
            ProductName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), searchBoxMaxHeight); };
            ProductName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, ProductName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), this, searchBoxMaxHeight); };
            ProductName_TextBox.TextChanged += ValidateInputs;
            ProductName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            ProductName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(ProductName_TextBox, this, AddPurchase_Label, e); };
        }
        private void ShowSearchBox(Guna2TextBox gTextBox, List<SearchBox.SearchResult> results, int maxHeight)
        {
            SearchBox.ShowSearchBox(this, gTextBox, results, this, maxHeight, true);
        }


        // Event handlers
        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            if (MainMenu_Form.Instance.Selected != MainMenu_Form.SelectedOption.Purchases)
            {
                MainMenu_Form.Instance.Purchases_Button.PerformClick();
            }
            MainMenu_Form.Instance.selectedDataGridView = MainMenu_Form.Instance.Purchases_DataGridView;

            if (panelsForMultipleProducts_List.Count == 0)
            {
                if (!AddSinglePurchase())
                {
                    return;
                }
            }
            else if (!AddMultiplePurchases())
            {
                return;
            }

            RemoveReceiptLabel();
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

            RemoveReceipt_ImageButton.Location = new Point(Receipt_Button.Right - RemoveReceipt_ImageButton.Width, Receipt_Button.Bottom + spaceBetweenControlsVertically);
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


        // Methods to add purchases
        private bool AddSinglePurchase()
        {
            string purchaseID = OrderNumber_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string itemName = ProductName_TextBox.Text;
            string categoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categoryPurchaseList, itemName);
            string country = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, itemName);
            string company = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, itemName);
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal disount = decimal.Parse(Discount_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit + shipping + tax + fee - disount;

            // Convert currency
            if (Currency_ComboBox.Text != Currency.CurrencyTypes.CAD.ToString())
            {
                decimal exchangeRate = Currency.GetExchangeRate(Currency_ComboBox.Text, "CAD", date);
                pricePerUnit *= exchangeRate;
                shipping *= exchangeRate;
                tax *= exchangeRate;
                fee *= exchangeRate;
                totalPrice *= exchangeRate;
            }

            // Round to 2 decimal places
            decimal amountCharged = decimal.Parse(AmountCharged_TextBox.Text);
            totalPrice = Math.Round(totalPrice, 2);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string message = $"Amount charged (${amountCharged}) is not equal to the total price of the purchase (${totalPrice}). The difference will be accounted for.";
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

            MainMenu_Form.Instance.selectedDataGridView.RowsAdded -= MainMenu_Form.Instance.DataGridView_RowsAdded;

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                purchaseID,
                buyerName,
                itemName,
                categoryName,
                country,
                company,
                date,
                quantity.ToString(),
                pricePerUnit.ToString("N2"),
                shipping.ToString("N2"),
                tax.ToString("N2"),
                fee.ToString("N2"),
                chargedDifference.ToString("N2"),
                totalPrice.ToString("N2")
            );
            if (newFilePath != "")
            {
                MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = newFilePath;
            }
            MainMenu_Form.Instance.selectedDataGridView.RowsAdded += MainMenu_Form.Instance.DataGridView_RowsAdded;

            MainMenu_Form.Instance.DataGridViewRowChanged();

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, itemName);
            Log.Write(3, $"Added purchase '{itemName}'");

            return true;
        }
        private bool AddMultiplePurchases()
        {
            decimal totalShipping = 0, totalTax = 0, totalPrice = 0;
            int totalQuantity = 0;
            string categoryName = "", country = "", company = "";

            string purchaseID = OrderNumber_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string date = Tools.FormatDate(Date_DateTimePicker.Value);
            int quantity = 0;
            decimal pricePerUnit = 0;
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal disount = decimal.Parse(Discount_TextBox.Text);

            List<string> items = [];

            foreach (Guna2Panel panel in panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string itemName = nameTextBox.Text;

                categoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categoryPurchaseList, itemName);
                country = MainMenu_Form.GetCountryProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, itemName);
                company = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categoryPurchaseList, itemName);

                Guna2TextBox quantityTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.quantity.ToString(), false).FirstOrDefault();
                quantity = int.Parse(quantityTextBox.Text);

                Guna2TextBox pricePerUnitTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.pricePerUnit.ToString(), false).FirstOrDefault();
                pricePerUnit = decimal.Parse(pricePerUnitTextBox.Text);

                totalQuantity += quantity;
                totalShipping += decimal.Parse(Shipping_TextBox.Text) / panelsForMultipleProducts_List.Count;
                totalTax += decimal.Parse(Tax_TextBox.Text) / panelsForMultipleProducts_List.Count;
                totalPrice += quantity * pricePerUnit;

                string item = string.Join(",",
                    itemName,
                    categoryName,
                    country,
                    company,
                    quantity.ToString(),
                    pricePerUnit.ToString("N2"),
                    totalPrice.ToString("N2")
                );

                items.Add(item);
            }
            totalPrice += shipping + tax + fee - disount;

            // Convert currency
            if (Currency_ComboBox.Text != Currency.CurrencyTypes.CAD.ToString())
            {
                decimal exchangeRate = Currency.GetExchangeRate(Currency_ComboBox.Text, "CAD", date);
                pricePerUnit *= exchangeRate;
                shipping *= exchangeRate;
                tax *= exchangeRate;
                fee *= exchangeRate;
                totalPrice *= exchangeRate;
            }

            // Round to 2 decimal places
            decimal amountCharged = decimal.Parse(AmountCharged_TextBox.Text);
            totalPrice = Math.Round(totalPrice, 2);
            decimal chargedDifference = amountCharged - totalPrice;

            if (totalPrice != amountCharged)
            {
                string message = $"Amount charged (${amountCharged}) is not equal to the total price of the purchase (${totalPrice}). The difference will be accounted for.";
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

            MainMenu_Form.Instance.selectedDataGridView.RowsAdded -= MainMenu_Form.Instance.DataGridView_RowsAdded;

            int newRowIndex = MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                purchaseID,
                buyerName,
                MainMenu_Form.multupleItems,
                MainMenu_Form.emptyCell,
                MainMenu_Form.emptyCell,
                MainMenu_Form.emptyCell,
                date,
                quantity.ToString(),
                pricePerUnit.ToString("N2"),
                shipping.ToString("N2"),
                tax.ToString("N2"),
                fee.ToString("N2"),
                chargedDifference.ToString("N2"),
                totalPrice.ToString("N2")
            );

            items.Add(newFilePath);

            MainMenu_Form.Instance.selectedDataGridView.Rows[newRowIndex].Tag = items;
            MainMenu_Form.Instance.selectedDataGridView.RowsAdded += MainMenu_Form.Instance.DataGridView_RowsAdded;

            MainMenu_Form.Instance.DataGridViewRowChanged();

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, purchaseID);
            Log.Write(3, $"Added purchase '{purchaseID}' with '{quantity}' items");

            return true;
        }


        // Methods for multiple items
        private List<Control> GetControlsForMultipleProducts()
        {
            return [ProductName_TextBox, ProductName_Label,
                Quantity_TextBox, Quantity_Label,
                PricePerUnit_TextBox, PricePerUnit_Label];
        }
        private readonly byte spaceBetweenControlsHorizontally = 6, spaceBetweenControlsVertically = 3, spaceToOffsetFormNotCenter = 15,
            textBoxHeight = 36, circleButtonHeight = 25, extraSpaceForBottom = 150, spaceBetweenPanels = 10,
               initialHeightForPanel = 59, spaceOnSidesOfPanel = 100, flowPanelMargin = 6;
        private readonly short initialWidthForPanel = 449, maxFlowPanelHeight = 300;
        private void SetControlsForSingleProduct()
        {
            // Center controls
            Currency_ComboBox.Left = (Width - Currency_ComboBox.Width - spaceBetweenControlsHorizontally -
                OrderNumber_TextBox.Width - spaceBetweenControlsHorizontally -
                BuyerName_TextBox.Width - spaceBetweenControlsHorizontally -
                ProductName_TextBox.Width - spaceBetweenControlsHorizontally -
                Receipt_Button.Width - spaceToOffsetFormNotCenter) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + spaceBetweenControlsHorizontally;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            BuyerName_TextBox.Left = OrderNumber_TextBox.Right + spaceBetweenControlsHorizontally;
            BuyerName_Label.Left = BuyerName_TextBox.Left;
            ProductName_TextBox.Left = BuyerName_TextBox.Right + spaceBetweenControlsHorizontally;
            ProductName_Label.Left = ProductName_TextBox.Left;
            Receipt_Button.Left = ProductName_TextBox.Right + spaceBetweenControlsHorizontally;

            Date_DateTimePicker.Left = (Width -
                Date_DateTimePicker.Width - spaceBetweenControlsHorizontally -
                Quantity_TextBox.Width - spaceBetweenControlsHorizontally -
                PricePerUnit_TextBox.Width - spaceBetweenControlsHorizontally -
                Shipping_TextBox.Width - spaceBetweenControlsHorizontally -
                Tax_TextBox.Width - spaceBetweenControlsHorizontally -
                PaymentFee_TextBox.Width - spaceBetweenControlsHorizontally -
                AmountCharged_TextBox.Width - spaceBetweenControlsHorizontally -
                Discount_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Quantity_TextBox.Left = Date_DateTimePicker.Right + spaceBetweenControlsHorizontally;
            Quantity_Label.Left = Quantity_TextBox.Left;
            PricePerUnit_TextBox.Left = Quantity_TextBox.Right + spaceBetweenControlsHorizontally;
            PricePerUnit_Label.Left = PricePerUnit_TextBox.Left;
            Shipping_TextBox.Left = PricePerUnit_TextBox.Right + spaceBetweenControlsHorizontally;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + spaceBetweenControlsHorizontally;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + spaceBetweenControlsHorizontally;
            PaymentFee_Label.Left = PaymentFee_TextBox.Left;
            AmountCharged_TextBox.Left = PaymentFee_TextBox.Right + spaceBetweenControlsHorizontally;
            AmountCharged_Label.Left = AmountCharged_TextBox.Left;
            Discount_TextBox.Left = AmountCharged_TextBox.Right + spaceBetweenControlsHorizontally;
            Discount_Label.Left = Discount_TextBox.Left;

            // Add controls
            foreach (Control control in GetControlsForMultipleProducts())
            {
                Controls.Add(control);
            }

            Controls.Remove(FlowPanel);
            Controls.Remove(AddButton);
            Height = 400;

            RelocateBuyerWarning();
            SetReceiptLabelLocation();

            if (Controls.Contains(WarningProduct_PictureBox))
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + spaceBetweenControlsVertically);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + spaceBetweenControlsHorizontally, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            Currency_ComboBox.Left = (Width -
                Currency_ComboBox.Width - spaceBetweenControlsHorizontally -
                OrderNumber_TextBox.Width - spaceBetweenControlsHorizontally -
                BuyerName_TextBox.Width - spaceBetweenControlsHorizontally -
                Receipt_Button.Width - spaceToOffsetFormNotCenter) / 2;

            Currency_Label.Left = Currency_ComboBox.Left;
            OrderNumber_TextBox.Left = Currency_ComboBox.Right + spaceBetweenControlsHorizontally;
            OrderNumber_Label.Left = OrderNumber_TextBox.Left;
            BuyerName_TextBox.Left = OrderNumber_TextBox.Right + spaceBetweenControlsHorizontally;
            BuyerName_Label.Left = BuyerName_TextBox.Left;
            Receipt_Button.Left = BuyerName_TextBox.Right + spaceBetweenControlsHorizontally;

            Date_DateTimePicker.Left = (Width -
                Date_DateTimePicker.Width - spaceBetweenControlsHorizontally -
                Shipping_TextBox.Width - spaceBetweenControlsHorizontally -
                Tax_TextBox.Width - spaceBetweenControlsHorizontally -
                PaymentFee_TextBox.Width - spaceBetweenControlsHorizontally -
                AmountCharged_TextBox.Width - spaceBetweenControlsHorizontally -
                Discount_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + spaceBetweenControlsHorizontally;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + spaceBetweenControlsHorizontally;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + spaceBetweenControlsHorizontally;
            PaymentFee_Label.Left = PaymentFee_TextBox.Left;
            AmountCharged_TextBox.Left = PaymentFee_TextBox.Right + spaceBetweenControlsHorizontally;
            AmountCharged_Label.Left = AmountCharged_TextBox.Left;
            Discount_TextBox.Left = AmountCharged_TextBox.Right + spaceBetweenControlsHorizontally;
            Discount_Label.Left = Discount_TextBox.Left;

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
                WarningProduct_PictureBox.Location = new Point(AddButton.Left + spaceBetweenControlsHorizontally, AddButton.Top - flowPanelMargin * 2);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + spaceBetweenControlsHorizontally, WarningProduct_PictureBox.Top);
                Controls.Remove(AddButton);
            }
            else
            {
                Controls.Add(AddButton);
            }
        }
        private void RelocateBuyerWarning()
        {
            WarningBuyer_PictureBox.Location = new Point(BuyerName_TextBox.Left, BuyerName_TextBox.Bottom + spaceBetweenControlsHorizontally);
            WarningBuyer_LinkLabel.Location = new Point(WarningBuyer_PictureBox.Right + spaceBetweenControlsHorizontally, WarningBuyer_PictureBox.Top);
        }
        private readonly List<Guna2Panel> panelsForMultipleProducts_List = [];
        enum TextBoxnames
        {
            name,
            quantity,
            pricePerUnit
        }
        private void ConstructControlsForMultipleProducts()
        {
            byte smallSearchBoxMaxHeight = 100;

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
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), smallSearchBoxMaxHeight);
            };
            textBox.GotFocus += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), smallSearchBoxMaxHeight);
            };
            textBox.TextChanged += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                SearchBox.SearchTextBoxChanged(this, searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductPurchaseNames()), this, smallSearchBoxMaxHeight);
            };
            textBox.TextChanged += ValidateInputs;
            textBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(textBox, this, AddPurchase_Label, e); };

            CosntructLabel(ProductName_Label.Text, 0, panel);

            // Quantity
            left = textBox.Right + spaceBetweenControlsHorizontally;
            textBox = CosntructTextBox(left, Quantity_TextBox.Width, TextBoxnames.quantity.ToString(), UI.KeyPressValidation.OnlyNumbers, panel);
            CosntructLabel(Quantity_Label.Text, left, panel);

            // Price per unit
            left = textBox.Right + spaceBetweenControlsHorizontally;
            textBox = CosntructTextBox(left, PricePerUnit_TextBox.Width, TextBoxnames.pricePerUnit.ToString(), UI.KeyPressValidation.OnlyNumbersAndDecimal, panel);
            CosntructLabel(PricePerUnit_Label.Text, left, panel);

            // Add minus button unless this is the first panel
            left = textBox.Right + spaceBetweenControlsHorizontally;
            if (panelsForMultipleProducts_List.Count > 1)
            {
                CosntructMinusButton(new Point(left + spaceBetweenControlsHorizontally, (textBoxHeight - circleButtonHeight) / 2 + textBox.Top), panel);
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
                Location = new Point(left, 20 + spaceBetweenControlsVertically),
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
                BackColor = CustomColors.controlBack,
                FillColor = CustomColors.controlBack,
                Location = location,
                Size = new Size(circleButtonHeight, circleButtonHeight),
                Image = Resources.Minus,
                ImageSize = new Size(22, 22)
            };
            circleBtn.HoverState.FillColor = CustomColors.controlBack;
            circleBtn.PressedColor = CustomColors.controlBack;
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
        Guna2CircleButton AddButton;
        FlowLayoutPanel FlowPanel;
        private void ConstructFlowPanel()
        {
            int width = initialWidthForPanel + spaceOnSidesOfPanel;
            FlowPanel = new()
            {
                AutoScroll = false,
                Location = new Point((Width - width + 10) / 2 - 5, 270),
                Size = new Size(width, 20 + spaceBetweenControlsVertically + textBoxHeight),
                Padding = new Padding(spaceOnSidesOfPanel / 2, 0, spaceOnSidesOfPanel / 2, 0),
                Margin = new Padding(flowPanelMargin / 2, 0, flowPanelMargin / 2, 0),
                MaximumSize = new Size(initialWidthForPanel + spaceOnSidesOfPanel, maxFlowPanelHeight)
            };
            FlowPanel.Click += CloseAllPanels;
        }
        private void CosntructAddButton()
        {
            AddButton = new()
            {
                BackColor = CustomColors.controlBack,
                FillColor = CustomColors.controlBack,
                Location = new Point(0, 60),
                Size = new Size(circleButtonHeight, circleButtonHeight),
                Image = Resources.Plus,
                ImageSize = new Size(22, 22),
                Left = FlowPanel.Left + spaceOnSidesOfPanel / 2
            };
            AddButton.HoverState.FillColor = CustomColors.controlBack;
            AddButton.PressedColor = CustomColors.controlBack;
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
            if (MainMenu_Form.Instance.GetProductPurchaseNames().Count == 0)
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
        private void ValidateInputs(object? sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(OrderNumber_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(BuyerName_TextBox.Text) && BuyerName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PaymentFee_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Discount_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(AmountCharged_TextBox.Text);

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
        public void CloseAllPanels(object? sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}