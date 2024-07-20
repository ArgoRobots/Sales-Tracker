using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class AddSale_Form : BaseForm
    {
        // Properties
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init.
        public AddSale_Form()
        {
            InitializeComponent();

            AddEventHandlersToTextBoxes();
            AddSearchBoxEvents();
            Date_DateTimePicker.Value = DateTime.Now;
            CheckIfProductsExist();
            CheckIfBuyersExist();
            Theme.SetThemeForForm(this);
        }
        private void AddEventHandlersToTextBoxes()
        {
            SaleID_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            SaleID_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            SaleID_TextBox.KeyDown += UI.TextBox_KeyDown;

            BuyerName_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            BuyerName_TextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            BuyerName_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            BuyerName_TextBox.KeyDown += UI.TextBox_KeyDown;

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
        }
        private readonly byte searchBoxMaxHeight = 150;
        private void AddSearchBoxEvents()
        {
            BuyerName_TextBox.Click += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            BuyerName_TextBox.GotFocus += (sender, e) => { ShowSearchBox(BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), searchBoxMaxHeight); };
            BuyerName_TextBox.TextChanged += (sender, e) => { SearchBox.SearchTextBoxChanged(this, BuyerName_TextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.accountantList), this, searchBoxMaxHeight); };
            BuyerName_TextBox.TextChanged += ValidateInputs;
            BuyerName_TextBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            BuyerName_TextBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(BuyerName_TextBox, this, AddSale_Label, e); };

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
        private void ShowSearchBox(Guna2TextBox gTextBox, List<SearchBox.SearchResult> results, int maxHeight)
        {
            SearchBox.ShowSearchBox(this, gTextBox, results, this, maxHeight, true);
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
                AddSingleSale();
            }
            else
            {
                AddMultipleSales();
            }
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
        private void ImportAmazon_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportEbay_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportExcel_Button_Click(object sender, EventArgs e)
        {

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


        // Methods to add Sales
        private void AddSale(string itemName, int quantity, decimal pricePerUnit, decimal shipping, decimal tax)
        {
            string SaleID = SaleID_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string categoryName = MainMenu_Form.GetCategoryNameByProductName(MainMenu_Form.Instance.categorySaleList, itemName);
            string country = CountryOfDestinaion_TextBox.Text;
            string company = MainMenu_Form.GetCompanyProductNameIsFrom(MainMenu_Form.Instance.categorySaleList, itemName);
            string date = Tools.FormatDate(Date_DateTimePicker.Value);

            decimal fee = decimal.Parse(PaymentFee_TextBox.Text);
            decimal totalPrice = quantity * pricePerUnit + shipping + tax;

            MainMenu_Form.Instance.selectedDataGridView.Rows.Add(
                SaleID,
                buyerName,
                itemName,
                categoryName,
                country,
                company,
                date,
                quantity.ToString(),
                pricePerUnit.ToString("C"),
                shipping.ToString("N"),
                tax.ToString("N"),
                fee.ToString("N"),
                totalPrice.ToString("N")
            );

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, itemName);
            Log.Write(3, $"Added Sale '{itemName}'");
        }
        private void AddSingleSale()
        {
            string itemName = ProductName_TextBox.Text;
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);

            AddSale(itemName, quantity, pricePerUnit, shipping, tax);
        }
        private void AddMultipleSales()
        {
            foreach (Guna2Panel panel in panelsForMultipleProducts_List)
            {
                Guna2TextBox nameTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.name.ToString(), false).FirstOrDefault();
                string itemName = nameTextBox.Text;

                Guna2TextBox quantityTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.quantity.ToString(), false).FirstOrDefault();
                int quantity = int.Parse(quantityTextBox.Text);

                Guna2TextBox pricePerUnitTextBox = (Guna2TextBox)panel.Controls.Find(TextBoxnames.pricePerUnit.ToString(), false).FirstOrDefault();
                decimal pricePerUnit = decimal.Parse(pricePerUnitTextBox.Text);

                decimal shipping = decimal.Parse(Shipping_TextBox.Text) / quantity;
                decimal tax = decimal.Parse(Tax_TextBox.Text) / quantity;

                AddSale(itemName, quantity, pricePerUnit, shipping, tax);
            }
        }


        // Methods for multiple items
        private List<Control> GetControlsForMultipleProducts()
        {
            return [ProductName_TextBox, ProductName_Label,
                Quantity_TextBox, Quantity_Label,
                PricePerUnit_TextBox, PricePerUnit_Label];
        }
        private readonly byte spaceBetweenControlsHorizontally = 6, spaceBetweenControlsVertically = 3, spaceToOffsetFormNotCenter = 15,
            textBoxHeight = 36, circleButtonHeight = 25, extraSpaceForBottom = 200, spaceBetweenPanels = 10,
               initialHeightForPanel = 59, spaceOnSidesOfPanel = 100, flowPanelMargin = 6;
        private readonly short initialWidthForPanel = 449, maxFlowPanelHeight = 300;
        private void SetControlsForSingleProduct()
        {
            // Center controls
            SaleID_TextBox.Left = (Width - SaleID_TextBox.Width - spaceBetweenControlsHorizontally -
                BuyerName_TextBox.Width - spaceBetweenControlsHorizontally -
                ProductName_TextBox.Width - spaceBetweenControlsHorizontally -
                CountryOfDestinaion_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

            SaleID_Label.Left = SaleID_TextBox.Left;
            BuyerName_TextBox.Left = SaleID_TextBox.Right + spaceBetweenControlsHorizontally;
            BuyerName_Label.Left = BuyerName_TextBox.Left;
            ProductName_TextBox.Left = BuyerName_TextBox.Right + spaceBetweenControlsHorizontally;
            ProductName_Label.Left = ProductName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = ProductName_TextBox.Right + spaceBetweenControlsHorizontally;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;

            Date_DateTimePicker.Left = (Width - Date_DateTimePicker.Width - spaceBetweenControlsHorizontally -
                Quantity_TextBox.Width - spaceBetweenControlsHorizontally -
                PricePerUnit_TextBox.Width - spaceBetweenControlsHorizontally -
                Shipping_TextBox.Width - spaceBetweenControlsHorizontally -
                Tax_TextBox.Width - spaceBetweenControlsHorizontally -
                PaymentFee_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

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

            // Add controls
            List<Control> controls = GetControlsForMultipleProducts();
            foreach (Control control in controls)
            {
                Controls.Add(control);
            }

            Controls.Remove(FlowPanel);
            Controls.Remove(AddButton);
            Height = 400;

            RelocateBuyerWarning();

            if (Controls.Contains(WarningProduct_PictureBox))
            {
                WarningProduct_PictureBox.Location = new Point(ProductName_TextBox.Left, ProductName_TextBox.Bottom + spaceBetweenControlsVertically);
                WarningProduct_LinkLabel.Location = new Point(WarningProduct_PictureBox.Left + WarningProduct_PictureBox.Width + spaceBetweenControlsHorizontally, WarningProduct_PictureBox.Top);
            }
        }
        private void SetControlsForMultipleProducts()
        {
            // Center controls
            SaleID_TextBox.Left = (Width - SaleID_TextBox.Width - spaceBetweenControlsHorizontally -
                BuyerName_TextBox.Width - spaceBetweenControlsHorizontally -
                CountryOfDestinaion_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

            SaleID_Label.Left = SaleID_TextBox.Left;
            BuyerName_TextBox.Left = SaleID_TextBox.Right + spaceBetweenControlsHorizontally;
            BuyerName_Label.Left = BuyerName_TextBox.Left;
            CountryOfDestinaion_TextBox.Left = BuyerName_TextBox.Right + spaceBetweenControlsHorizontally;
            CountryOfDestination_Label.Left = CountryOfDestinaion_TextBox.Left;

            Date_DateTimePicker.Left = (Width -
                Date_DateTimePicker.Width - spaceBetweenControlsHorizontally -
                Shipping_TextBox.Width - spaceBetweenControlsHorizontally -
                Tax_TextBox.Width - spaceBetweenControlsHorizontally -
                PaymentFee_TextBox.Width - spaceToOffsetFormNotCenter) / 2;

            Date_Label.Left = Date_DateTimePicker.Left;
            Shipping_TextBox.Left = Date_DateTimePicker.Right + spaceBetweenControlsHorizontally;
            Shipping_Label.Left = Shipping_TextBox.Left;
            Tax_TextBox.Left = Shipping_TextBox.Right + spaceBetweenControlsHorizontally;
            Tax_Label.Left = Tax_TextBox.Left;
            PaymentFee_TextBox.Left = Tax_TextBox.Right + spaceBetweenControlsHorizontally;
            PaymentFee_Label.Left = PaymentFee_TextBox.Left;

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
            Guna2Panel panel = new()
            {
                Size = new Size(initialWidthForPanel, initialHeightForPanel),
                FillColor = CustomColors.mainBackground
            };
            panelsForMultipleProducts_List.Add(panel);

            Guna2TextBox textBox;
            int left;

            // Product name
            textBox = CosntructTextBox(0, ProductName_TextBox.Width, TextBoxnames.name.ToString(), panel);
            textBox.Click -= CloseAllPanels;
            textBox.Click += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), searchBoxMaxHeight);
            };
            textBox.GotFocus += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                ShowSearchBox(searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), searchBoxMaxHeight);
            };
            textBox.TextChanged += (sender, e) =>
            {
                Guna2TextBox searchTextBox = (Guna2TextBox)sender;
                SearchBox.SearchTextBoxChanged(this, searchTextBox, SearchBox.ConvertToSearchResults(MainMenu_Form.Instance.GetProductSaleNames()), this, searchBoxMaxHeight);
            };
            textBox.TextChanged += ValidateInputs;
            textBox.PreviewKeyDown += SearchBox.AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => { SearchBox.SearchBoxTextBox_KeyDown(textBox, this, AddSale_Label, e); };

            CosntructLabel(ProductName_Label.Text, 0, panel);

            // Quantity
            left = textBox.Right + spaceBetweenControlsHorizontally;
            textBox = CosntructTextBox(left, Quantity_TextBox.Width, TextBoxnames.quantity.ToString(), panel);
            CosntructLabel(Quantity_Label.Text, left, panel);

            // Price per unit
            left = textBox.Right + spaceBetweenControlsHorizontally;
            textBox = CosntructTextBox(left, PricePerUnit_TextBox.Width, TextBoxnames.pricePerUnit.ToString(), panel);
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
        private static void CosntructLabel(string text, int left, Control parent)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = CustomColors.text,
                Left = left,
                AutoSize = true
            };
            parent.Controls.Add(label);
        }
        private Guna2TextBox CosntructTextBox(int left, int width, string name, Control parent)
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
            textBox.Click += CloseAllPanels;

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
        private void ValidateInputs(object? sender, EventArgs e)
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(SaleID_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(BuyerName_TextBox.Text) && BuyerName_TextBox.Tag.ToString() != "0" &&
                                   !string.IsNullOrWhiteSpace(Shipping_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(Tax_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(PaymentFee_TextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(CountryOfDestinaion_TextBox.Text) && CountryOfDestinaion_TextBox.Tag.ToString() != "0";

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
        public void CloseAllPanels(object? sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox(this);
        }
    }
}