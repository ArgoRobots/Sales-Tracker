using System.Windows.Forms;

namespace Sales_Tracker
{
    partial class RentOutItem_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.ProductName_Label = new Label();
            this.AvailableQuantity_Label = new Label();
            this.Customer_ComboBox = new Guna.UI2.WinForms.Guna2ComboBox();
            this.Quantity_NumericUpDown = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.RentalStartDate_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            this.DailyRate_RadioButton = new Guna.UI2.WinForms.Guna2CustomRadioButton();
            this.DailyRate_Label = new Label();
            this.WeeklyRate_RadioButton = new Guna.UI2.WinForms.Guna2CustomRadioButton();
            this.WeeklyRate_Label = new Label();
            this.MonthlyRate_RadioButton = new Guna.UI2.WinForms.Guna2CustomRadioButton();
            this.MonthlyRate_Label = new Label();
            this.SecurityDeposit_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.Notes_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.RentOut_Button = new Guna.UI2.WinForms.Guna2Button();
            this.Cancel_Button = new Guna.UI2.WinForms.Guna2Button();
            this.TotalCost_Label = new Label();
            this.NoCustomers_Label = new Label();
            this.SelectCustomer_Label = new Label();
            this.Quantity_Label = new Label();
            this.StartDate_Label = new Label();
            this.RateType_Label = new Label();
            this.SecurityDeposit_Label = new Label();
            this.Notes_Label = new Label();
            
            ((System.ComponentModel.ISupportInitialize)(this.Quantity_NumericUpDown)).BeginInit();
            this.SuspendLayout();
            
            // 
            // ProductName_Label
            // 
            this.ProductName_Label.AutoSize = false;
            this.ProductName_Label.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.ProductName_Label.Location = new System.Drawing.Point(30, 20);
            this.ProductName_Label.Name = "ProductName_Label";
            this.ProductName_Label.Size = new System.Drawing.Size(540, 35);
            this.ProductName_Label.TabIndex = 0;
            this.ProductName_Label.Text = "Product Name";
            
            // 
            // AvailableQuantity_Label
            // 
            this.AvailableQuantity_Label.AutoSize = true;
            this.AvailableQuantity_Label.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.AvailableQuantity_Label.Location = new System.Drawing.Point(30, 60);
            this.AvailableQuantity_Label.Name = "AvailableQuantity_Label";
            this.AvailableQuantity_Label.Size = new System.Drawing.Size(120, 23);
            this.AvailableQuantity_Label.TabIndex = 1;
            this.AvailableQuantity_Label.Text = "Available: 0";
            
            // 
            // SelectCustomer_Label
            // 
            this.SelectCustomer_Label.AutoSize = true;
            this.SelectCustomer_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.SelectCustomer_Label.Location = new System.Drawing.Point(30, 100);
            this.SelectCustomer_Label.Name = "SelectCustomer_Label";
            this.SelectCustomer_Label.Size = new System.Drawing.Size(140, 25);
            this.SelectCustomer_Label.TabIndex = 2;
            this.SelectCustomer_Label.Text = "Select Customer";
            
            // 
            // Customer_ComboBox
            // 
            this.Customer_ComboBox.BackColor = System.Drawing.Color.Transparent;
            this.Customer_ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.Customer_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Customer_ComboBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Customer_ComboBox.Location = new System.Drawing.Point(30, 130);
            this.Customer_ComboBox.Name = "Customer_ComboBox";
            this.Customer_ComboBox.Size = new System.Drawing.Size(540, 31);
            this.Customer_ComboBox.TabIndex = 3;
            
            // 
            // NoCustomers_Label
            // 
            this.NoCustomers_Label.AutoSize = true;
            this.NoCustomers_Label.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.NoCustomers_Label.ForeColor = System.Drawing.Color.Red;
            this.NoCustomers_Label.Location = new System.Drawing.Point(30, 165);
            this.NoCustomers_Label.Name = "NoCustomers_Label";
            this.NoCustomers_Label.Size = new System.Drawing.Size(350, 20);
            this.NoCustomers_Label.TabIndex = 4;
            this.NoCustomers_Label.Text = "No customers found. Please add a customer first.";
            this.NoCustomers_Label.Visible = false;
            
            // 
            // Quantity_Label
            // 
            this.Quantity_Label.AutoSize = true;
            this.Quantity_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.Quantity_Label.Location = new System.Drawing.Point(30, 190);
            this.Quantity_Label.Name = "Quantity_Label";
            this.Quantity_Label.Size = new System.Drawing.Size(85, 25);
            this.Quantity_Label.TabIndex = 5;
            this.Quantity_Label.Text = "Quantity";
            
            // 
            // Quantity_NumericUpDown
            // 
            this.Quantity_NumericUpDown.BackColor = System.Drawing.Color.Transparent;
            this.Quantity_NumericUpDown.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Quantity_NumericUpDown.Location = new System.Drawing.Point(30, 220);
            this.Quantity_NumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.Quantity_NumericUpDown.Name = "Quantity_NumericUpDown";
            this.Quantity_NumericUpDown.Size = new System.Drawing.Size(150, 36);
            this.Quantity_NumericUpDown.TabIndex = 6;
            this.Quantity_NumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.Quantity_NumericUpDown.ValueChanged += new System.EventHandler(this.Quantity_NumericUpDown_ValueChanged);
            
            // 
            // StartDate_Label
            // 
            this.StartDate_Label.AutoSize = true;
            this.StartDate_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.StartDate_Label.Location = new System.Drawing.Point(220, 190);
            this.StartDate_Label.Name = "StartDate_Label";
            this.StartDate_Label.Size = new System.Drawing.Size(95, 25);
            this.StartDate_Label.TabIndex = 7;
            this.StartDate_Label.Text = "Start Date";
            
            // 
            // RentalStartDate_DateTimePicker
            // 
            this.RentalStartDate_DateTimePicker.BackColor = System.Drawing.Color.Transparent;
            this.RentalStartDate_DateTimePicker.Checked = true;
            this.RentalStartDate_DateTimePicker.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.RentalStartDate_DateTimePicker.Format = DateTimePickerFormat.Short;
            this.RentalStartDate_DateTimePicker.Location = new System.Drawing.Point(220, 220);
            this.RentalStartDate_DateTimePicker.MaxDate = new System.DateTime(9998, 12, 31, 0, 0, 0, 0);
            this.RentalStartDate_DateTimePicker.MinDate = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.RentalStartDate_DateTimePicker.Name = "RentalStartDate_DateTimePicker";
            this.RentalStartDate_DateTimePicker.Size = new System.Drawing.Size(200, 36);
            this.RentalStartDate_DateTimePicker.TabIndex = 8;
            this.RentalStartDate_DateTimePicker.Value = System.DateTime.Now;
            
            // 
            // RateType_Label
            // 
            this.RateType_Label.AutoSize = true;
            this.RateType_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.RateType_Label.Location = new System.Drawing.Point(30, 270);
            this.RateType_Label.Name = "RateType_Label";
            this.RateType_Label.Size = new System.Drawing.Size(120, 25);
            this.RateType_Label.TabIndex = 9;
            this.RateType_Label.Text = "Rental Rate";
            
            // 
            // DailyRate_RadioButton
            // 
            this.DailyRate_RadioButton.CheckedState.BorderColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.DailyRate_RadioButton.CheckedState.BorderThickness = 0;
            this.DailyRate_RadioButton.CheckedState.FillColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.DailyRate_RadioButton.CheckedState.InnerColor = System.Drawing.Color.White;
            this.DailyRate_RadioButton.Location = new System.Drawing.Point(30, 305);
            this.DailyRate_RadioButton.Name = "DailyRate_RadioButton";
            this.DailyRate_RadioButton.Size = new System.Drawing.Size(20, 20);
            this.DailyRate_RadioButton.TabIndex = 10;
            this.DailyRate_RadioButton.UncheckedState.BorderColor = System.Drawing.Color.FromArgb(125, 137, 149);
            this.DailyRate_RadioButton.UncheckedState.BorderThickness = 2;
            this.DailyRate_RadioButton.UncheckedState.FillColor = System.Drawing.Color.Transparent;
            this.DailyRate_RadioButton.UncheckedState.InnerColor = System.Drawing.Color.Transparent;
            this.DailyRate_RadioButton.CheckedChanged += new System.EventHandler(this.RateType_CheckedChanged);
            
            // 
            // DailyRate_Label
            // 
            this.DailyRate_Label.AutoSize = true;
            this.DailyRate_Label.Cursor = Cursors.Hand;
            this.DailyRate_Label.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DailyRate_Label.Location = new System.Drawing.Point(55, 303);
            this.DailyRate_Label.Name = "DailyRate_Label";
            this.DailyRate_Label.Size = new System.Drawing.Size(100, 23);
            this.DailyRate_Label.TabIndex = 11;
            this.DailyRate_Label.Text = "Daily: $0.00";
            this.DailyRate_Label.Click += new System.EventHandler((s, e) => this.DailyRate_RadioButton.Checked = true);
            
            // 
            // WeeklyRate_RadioButton
            // 
            this.WeeklyRate_RadioButton.CheckedState.BorderColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.WeeklyRate_RadioButton.CheckedState.BorderThickness = 0;
            this.WeeklyRate_RadioButton.CheckedState.FillColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.WeeklyRate_RadioButton.CheckedState.InnerColor = System.Drawing.Color.White;
            this.WeeklyRate_RadioButton.Location = new System.Drawing.Point(200, 305);
            this.WeeklyRate_RadioButton.Name = "WeeklyRate_RadioButton";
            this.WeeklyRate_RadioButton.Size = new System.Drawing.Size(20, 20);
            this.WeeklyRate_RadioButton.TabIndex = 12;
            this.WeeklyRate_RadioButton.UncheckedState.BorderColor = System.Drawing.Color.FromArgb(125, 137, 149);
            this.WeeklyRate_RadioButton.UncheckedState.BorderThickness = 2;
            this.WeeklyRate_RadioButton.UncheckedState.FillColor = System.Drawing.Color.Transparent;
            this.WeeklyRate_RadioButton.UncheckedState.InnerColor = System.Drawing.Color.Transparent;
            this.WeeklyRate_RadioButton.CheckedChanged += new System.EventHandler(this.RateType_CheckedChanged);
            
            // 
            // WeeklyRate_Label
            // 
            this.WeeklyRate_Label.AutoSize = true;
            this.WeeklyRate_Label.Cursor = Cursors.Hand;
            this.WeeklyRate_Label.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.WeeklyRate_Label.Location = new System.Drawing.Point(225, 303);
            this.WeeklyRate_Label.Name = "WeeklyRate_Label";
            this.WeeklyRate_Label.Size = new System.Drawing.Size(120, 23);
            this.WeeklyRate_Label.TabIndex = 13;
            this.WeeklyRate_Label.Text = "Weekly: $0.00";
            this.WeeklyRate_Label.Click += new System.EventHandler((s, e) => this.WeeklyRate_RadioButton.Checked = true);
            
            // 
            // MonthlyRate_RadioButton
            // 
            this.MonthlyRate_RadioButton.CheckedState.BorderColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.MonthlyRate_RadioButton.CheckedState.BorderThickness = 0;
            this.MonthlyRate_RadioButton.CheckedState.FillColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.MonthlyRate_RadioButton.CheckedState.InnerColor = System.Drawing.Color.White;
            this.MonthlyRate_RadioButton.Location = new System.Drawing.Point(380, 305);
            this.MonthlyRate_RadioButton.Name = "MonthlyRate_RadioButton";
            this.MonthlyRate_RadioButton.Size = new System.Drawing.Size(20, 20);
            this.MonthlyRate_RadioButton.TabIndex = 14;
            this.MonthlyRate_RadioButton.UncheckedState.BorderColor = System.Drawing.Color.FromArgb(125, 137, 149);
            this.MonthlyRate_RadioButton.UncheckedState.BorderThickness = 2;
            this.MonthlyRate_RadioButton.UncheckedState.FillColor = System.Drawing.Color.Transparent;
            this.MonthlyRate_RadioButton.UncheckedState.InnerColor = System.Drawing.Color.Transparent;
            this.MonthlyRate_RadioButton.CheckedChanged += new System.EventHandler(this.RateType_CheckedChanged);
            
            // 
            // MonthlyRate_Label
            // 
            this.MonthlyRate_Label.AutoSize = true;
            this.MonthlyRate_Label.Cursor = Cursors.Hand;
            this.MonthlyRate_Label.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MonthlyRate_Label.Location = new System.Drawing.Point(405, 303);
            this.MonthlyRate_Label.Name = "MonthlyRate_Label";
            this.MonthlyRate_Label.Size = new System.Drawing.Size(130, 23);
            this.MonthlyRate_Label.TabIndex = 15;
            this.MonthlyRate_Label.Text = "Monthly: $0.00";
            this.MonthlyRate_Label.Click += new System.EventHandler((s, e) => this.MonthlyRate_RadioButton.Checked = true);
            
            // 
            // SecurityDeposit_Label
            // 
            this.SecurityDeposit_Label.AutoSize = true;
            this.SecurityDeposit_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.SecurityDeposit_Label.Location = new System.Drawing.Point(30, 345);
            this.SecurityDeposit_Label.Name = "SecurityDeposit_Label";
            this.SecurityDeposit_Label.Size = new System.Drawing.Size(155, 25);
            this.SecurityDeposit_Label.TabIndex = 16;
            this.SecurityDeposit_Label.Text = "Security Deposit";
            
            // 
            // SecurityDeposit_TextBox
            // 
            this.SecurityDeposit_TextBox.Cursor = Cursors.IBeam;
            this.SecurityDeposit_TextBox.DefaultText = "0.00";
            this.SecurityDeposit_TextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.SecurityDeposit_TextBox.Location = new System.Drawing.Point(30, 375);
            this.SecurityDeposit_TextBox.Name = "SecurityDeposit_TextBox";
            this.SecurityDeposit_TextBox.PasswordChar = '\0';
            this.SecurityDeposit_TextBox.PlaceholderText = "0.00";
            this.SecurityDeposit_TextBox.SelectedText = "";
            this.SecurityDeposit_TextBox.Size = new System.Drawing.Size(200, 36);
            this.SecurityDeposit_TextBox.TabIndex = 17;
            this.SecurityDeposit_TextBox.TextChanged += new System.EventHandler(this.SecurityDeposit_TextBox_TextChanged);
            this.SecurityDeposit_TextBox.KeyPress += new KeyPressEventHandler(this.SecurityDeposit_TextBox_KeyPress);
            
            // 
            // Notes_Label
            // 
            this.Notes_Label.AutoSize = true;
            this.Notes_Label.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.Notes_Label.Location = new System.Drawing.Point(30, 425);
            this.Notes_Label.Name = "Notes_Label";
            this.Notes_Label.Size = new System.Drawing.Size(181, 25);
            this.Notes_Label.TabIndex = 18;
            this.Notes_Label.Text = "Notes (Optional)";
            
            // 
            // Notes_TextBox
            // 
            this.Notes_TextBox.Cursor = Cursors.IBeam;
            this.Notes_TextBox.DefaultText = "";
            this.Notes_TextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Notes_TextBox.Location = new System.Drawing.Point(30, 455);
            this.Notes_TextBox.Multiline = true;
            this.Notes_TextBox.Name = "Notes_TextBox";
            this.Notes_TextBox.PasswordChar = '\0';
            this.Notes_TextBox.PlaceholderText = "Enter any additional notes...";
            this.Notes_TextBox.SelectedText = "";
            this.Notes_TextBox.Size = new System.Drawing.Size(540, 80);
            this.Notes_TextBox.TabIndex = 19;
            
            // 
            // TotalCost_Label
            // 
            this.TotalCost_Label.AutoSize = false;
            this.TotalCost_Label.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.TotalCost_Label.Location = new System.Drawing.Point(30, 555);
            this.TotalCost_Label.Name = "TotalCost_Label";
            this.TotalCost_Label.Size = new System.Drawing.Size(540, 30);
            this.TotalCost_Label.TabIndex = 20;
            this.TotalCost_Label.Text = "Total: $0.00";
            this.TotalCost_Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            
            // 
            // RentOut_Button
            // 
            this.RentOut_Button.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.RentOut_Button.Location = new System.Drawing.Point(380, 600);
            this.RentOut_Button.Name = "RentOut_Button";
            this.RentOut_Button.Size = new System.Drawing.Size(190, 45);
            this.RentOut_Button.TabIndex = 21;
            this.RentOut_Button.Text = "Rent Out";
            this.RentOut_Button.Click += new System.EventHandler(this.RentOut_Button_Click);
            
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.Cancel_Button.Location = new System.Drawing.Point(180, 600);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(190, 45);
            this.Cancel_Button.TabIndex = 22;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            
            // 
            // RentOutItem_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 670);
            this.Controls.Add(this.ProductName_Label);
            this.Controls.Add(this.AvailableQuantity_Label);
            this.Controls.Add(this.SelectCustomer_Label);
            this.Controls.Add(this.Customer_ComboBox);
            this.Controls.Add(this.NoCustomers_Label);
            this.Controls.Add(this.Quantity_Label);
            this.Controls.Add(this.Quantity_NumericUpDown);
            this.Controls.Add(this.StartDate_Label);
            this.Controls.Add(this.RentalStartDate_DateTimePicker);
            this.Controls.Add(this.RateType_Label);
            this.Controls.Add(this.DailyRate_RadioButton);
            this.Controls.Add(this.DailyRate_Label);
            this.Controls.Add(this.WeeklyRate_RadioButton);
            this.Controls.Add(this.WeeklyRate_Label);
            this.Controls.Add(this.MonthlyRate_RadioButton);
            this.Controls.Add(this.MonthlyRate_Label);
            this.Controls.Add(this.SecurityDeposit_Label);
            this.Controls.Add(this.SecurityDeposit_TextBox);
            this.Controls.Add(this.Notes_Label);
            this.Controls.Add(this.Notes_TextBox);
            this.Controls.Add(this.TotalCost_Label);
            this.Controls.Add(this.RentOut_Button);
            this.Controls.Add(this.Cancel_Button);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RentOutItem_Form";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Rent Out Item";
            this.Shown += new System.EventHandler(this.RentOutItem_Form_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.Quantity_NumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label ProductName_Label;
        private Label AvailableQuantity_Label;
        private Label SelectCustomer_Label;
        private Guna.UI2.WinForms.Guna2ComboBox Customer_ComboBox;
        private Label NoCustomers_Label;
        private Label Quantity_Label;
        private Guna.UI2.WinForms.Guna2NumericUpDown Quantity_NumericUpDown;
        private Label StartDate_Label;
        private Guna.UI2.WinForms.Guna2DateTimePicker RentalStartDate_DateTimePicker;
        private Label RateType_Label;
        private Guna.UI2.WinForms.Guna2CustomRadioButton DailyRate_RadioButton;
        private Label DailyRate_Label;
        private Guna.UI2.WinForms.Guna2CustomRadioButton WeeklyRate_RadioButton;
        private Label WeeklyRate_Label;
        private Guna.UI2.WinForms.Guna2CustomRadioButton MonthlyRate_RadioButton;
        private Label MonthlyRate_Label;
        private Label SecurityDeposit_Label;
        private Guna.UI2.WinForms.Guna2TextBox SecurityDeposit_TextBox;
        private Label Notes_Label;
        private Guna.UI2.WinForms.Guna2TextBox Notes_TextBox;
        private Label TotalCost_Label;
        private Guna.UI2.WinForms.Guna2Button RentOut_Button;
        private Guna.UI2.WinForms.Guna2Button Cancel_Button;
    }
}