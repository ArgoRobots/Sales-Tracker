using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker
{
    public partial class ModifyRow_Form : BaseForm
    {
        // Init
        public ModifyRow_Form(DataGridViewCell selectedRow)
        {
            InitializeComponent();
            ConstructControls(selectedRow);
            Theme.SetThemeForForm(this);
        }
        private void ConstructControls(DataGridViewCell selectedRow)
        {
            Control control = this;
            int left = 0;
            foreach (DataGridViewColumn column in selectedRow.DataGridView.Columns)
            {
                string columnName = column.Name;
                string cellValue = selectedRow.OwningRow.Cells[column.Index].Value?.ToString() ?? "";

                switch (columnName)
                {
                    case nameof(PurchaseColumns.PurchaseID):
                    case nameof(SalesColumns.SalesID):
                        ConstructLabel("ID", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;

                    case nameof(PurchaseColumns.BuyerName):
                    case nameof(SalesColumns.CustomerName):
                        ConstructLabel("Name", left, control);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, control);
                        break;

                    case nameof(PurchaseColumns.ItemName):
                        ConstructLabel("Item Name", left, control);
                        ConstructTextBox(left, columnName, cellValue, 50, KeyPressValidation.None, control);
                        break;

                    case nameof(PurchaseColumns.CategoryName):
                        ConstructLabel("Category", left, control);
                        ConstructGunaComboBox(left, columnName, ["Category1", "Category2"], cellValue, false, control);
                        break;

                    case nameof(PurchaseColumns.Date):
                        ConstructLabel("Date", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.None, control);
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

                    case nameof(PurchaseColumns.TotalExpenses):
                    case nameof(SalesColumns.TotalRevenue):
                        ConstructLabel("Total", left, control);
                        ConstructTextBox(left, columnName, cellValue, 10, KeyPressValidation.OnlyNumbersAndDecimalAndMinus, control);
                        break;
                }

                left += 100;  // Adjust the spacing as per your requirement
            }
        }

        // Event handlers
        private void Save_Button_Click(object sender, EventArgs e)
        {

        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {

        }


        // Functions
        private void InputChanged(object sender, EventArgs e)
        {
            AllInputsFilled((Control)sender);
            SaveInRow();
        }
        public void AllInputsFilled(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
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
                    if (gunaComboBox.SelectedItem == null || string.IsNullOrEmpty(gunaComboBox.Text))
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

        }


        // Construct controls
        private readonly byte textBoxWidth = 70, comboBoxWidth = 180;
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
            // Make sure the text is not selected
            gTextBox.Enter += (sender, e) =>
            {
                Guna2TextBox senderTextBox = (Guna2TextBox)sender;
                senderTextBox.SelectionStart = senderTextBox.Text.Length;
                senderTextBox.SelectionLength = 0;
            };
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
    }
}
