using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : BaseForm
    {
        // Init
        public ModifyRow_Form(DataGridViewRow selectedRow)
        {
            InitializeComponent();
            ConstructControls(selectedRow);
            Theme.SetThemeForForm(this);
        }
        private void ConstructControls(DataGridViewRow selectedRow)
        {
            Control control = Panel;
            int left = 0;
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.Cells[column.Index].Value?.ToString() ?? "";
                string text;
                switch (columnName)
                {
                    case nameof(SalesColumns.SalesID):
                    case nameof(PurchaseColumns.PurchaseID):
                        if (MainMenu_Form.Instance.Selected == Options.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[SalesColumns.SalesID];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.PurchaseID]; }

                        ConstructLabel(text, left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;

                    case nameof(SalesColumns.CustomerName):
                    case nameof(PurchaseColumns.BuyerName):
                        if (MainMenu_Form.Instance.Selected == Options.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[SalesColumns.CustomerName];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.BuyerName]; }

                        ConstructLabel(text, left, control);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, control);
                        break;

                    case nameof(PurchaseColumns.ItemName):
                        ConstructLabel("Item Name", left, control);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, control);
                        break;

                    case nameof(PurchaseColumns.CategoryName):
                        ConstructLabel("Category", left, control);
                        string[] array = MainMenu_Form.Instance.GetProductSaleNames().ToArray();
                        array = MainMenu_Form.Instance.GetProductPurchaseNames().ToArray();
                        ConstructGunaComboBox(left, columnName, array, cellValue, false, control);
                        left += 100;
                        break;

                    case nameof(PurchaseColumns.Date):
                        ConstructLabel("Date", left, control);
                        ConstructDatePicker(left, columnName, DateTime.Parse(cellValue), control);
                        break;

                    case nameof(PurchaseColumns.Quantity):
                        ConstructLabel("Quantity", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbers, control);
                        break;

                    case nameof(PurchaseColumns.PricePerUnit):
                        ConstructLabel("Price Per Unit", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;

                    case nameof(PurchaseColumns.Shipping):
                        ConstructLabel("Shipping", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;

                    case nameof(PurchaseColumns.Tax):
                        ConstructLabel("Tax", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;

                    case nameof(SalesColumns.TotalRevenue):
                    case nameof(PurchaseColumns.TotalExpenses):
                        if (MainMenu_Form.Instance.Selected == Options.Sales)
                        {
                            text = MainMenu_Form.Instance.SalesColumnHeaders[SalesColumns.TotalRevenue];
                        }
                        else { text = MainMenu_Form.Instance.PurchaseColumnHeaders[PurchaseColumns.TotalExpenses]; }

                        ConstructLabel("Total", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;
                }
                left += 100;
            }

            // Center controls
            Width = left + 50;
            Panel.Width = Width - 50;
            Panel.Left = (Width - Panel.Width) / 2;
        }

        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {
            SaveInRow();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }


        // Functions
        private void InputChanged(object sender, EventArgs e)
        {
            Control senderControl = sender as Control;
            foreach (Control control in senderControl.Parent.Controls)
            {
                if (control is Guna2TextBox gunaTextBox)
                {
                    if (string.IsNullOrEmpty(gunaTextBox.Text))
                    {
                        Save_Button.Enabled = false;
                        return;
                    }
                }
                else if (control is Guna2ComboBox gunaComboBox)
                {
                    if (string.IsNullOrEmpty(gunaComboBox.Text))
                    {
                        Save_Button.Enabled = false;
                        return;
                    }
                }
            }
            Save_Button.Enabled = true;
        }
        private void SaveInRow()
        {
            DataGridViewRow selectedRow = MainMenu_Form.Instance.selectedDataGridView.SelectedRows[0];
            foreach (Control control in Panel.Controls)
            {
                if (control is Guna2TextBox gTextBox)
                {
                    string columnName = gTextBox.Name;
                    selectedRow.Cells[columnName].Value = gTextBox.Text;
                }
                else if (control is Guna2ComboBox gComboBox)
                {
                    string columnName = gComboBox.Name;
                    selectedRow.Cells[columnName].Value = gComboBox.SelectedItem.ToString();
                }
                else if (control is Guna2DateTimePicker gDatePicker)
                {
                    string columnName = gDatePicker.Name;
                    selectedRow.Cells[columnName].Value = Tools.FormatDate(gDatePicker.Value);
                }
            }
            Close();
        }


        // Construct controls
        private readonly byte textBoxWidth = 80, comboBoxWidth = 180;
        private static Label ConstructLabel(string text, int left, Control control)
        {
            Label label = new()
            {
                ForeColor = CustomColors.text,
                Cursor = Cursors.Arrow,
                Location = new Point(left, 20),
                Text = text,
                Font = new Font("Segoe UI", 12),
                AutoSize = true
            };
            control.Controls.Add(label);

            return label;
        }
        // Define an enumeration for key press validation types
        public enum KeyPressValidation
        {
            OnlyNumbersAndDecimalAndMinus,
            OnlyNumbers,
            None
        }
        private Guna2TextBox ConstructTextBox(int left, string name, string text, int maxLength, KeyPressValidation keyPressValidation, Control control)
        {
            Guna2TextBox gTextBox = new()
            {
                Location = new Point(left, 45),
                Size = new Size(textBoxWidth, 30),
                Name = name,
                Text = text,
                ForeColor = CustomColors.text,
                BackColor = CustomColors.controlBack,
                Font = new Font("Segoe UI", 10),
                MaxLength = maxLength,
                FillColor = CustomColors.controlBack,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Cursor = Cursors.Hand,
                ShortcutsEnabled = false
            };
            gTextBox.FocusedState.FillColor = CustomColors.controlBack;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;

            // Assign the appropriate KeyPress event handler based on the keyPressValidation parameter
            switch (keyPressValidation)
            {
                case KeyPressValidation.OnlyNumbersAndDecimalAndMinus:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox;
                    break;
                case KeyPressValidation.OnlyNumbers:
                    gTextBox.KeyPress += Tools.OnlyAllowNumbersInTextBox;
                    break;
                case KeyPressValidation.None:
                    break;
            }

            gTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Remove Windows "ding" noise when user presses enter
                    e.SuppressKeyPress = true;

                    // Tab
                    Control nextControl = GetNextControl(this, true);
                    SendKeys.Send("{TAB}");
                }
            };
            gTextBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
            gTextBox.TextChanged += InputChanged;
            control.Controls.Add(gTextBox);

            return gTextBox;
        }
        private Guna2ComboBox ConstructGunaComboBox(int left, string name, string[] items, string text, bool addListOfTabs, Control control)
        {
            Guna2ComboBox gComboBox = new()
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(left, 45),
                Size = new Size(comboBoxWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name
            };
            gComboBox.HoverState.BorderColor = CustomColors.accent_blue;
            gComboBox.FocusedState.BorderColor = CustomColors.accent_blue;
            gComboBox.Items.AddRange(items);
            gComboBox.Text = text;
            gComboBox.SelectedIndexChanged += InputChanged;
            if (addListOfTabs)
            {

            }
            control.Controls.Add(gComboBox);

            return gComboBox;
        }
        private Guna2DateTimePicker ConstructDatePicker(int left, string name, DateTime value, Control control)
        {
            Guna2DateTimePicker gDatePicker = new()
            {
                Location = new Point(left, 45),
                Size = new Size(comboBoxWidth, 30),
                FillColor = CustomColors.controlBack,
                ForeColor = CustomColors.text,
                BorderColor = CustomColors.controlBorder,
                BorderRadius = 3,
                Name = name,
                Value = value
            };
            gDatePicker.HoverState.BorderColor = CustomColors.accent_blue;
            control.Controls.Add(gDatePicker);

            return gDatePicker;
        }
    }
}