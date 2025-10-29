namespace Sales_Tracker
{
    partial class ManageRentals_Form
    {
        private System.ComponentModel.IContainer components = null;

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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageRentals_Form));
            
            Title_Label = new Label();
            AddRentalItem_Button = new Guna.UI2.WinForms.Guna2Button();
            Search_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ShowingResultsFor_Label = new Label();
            Total_Label = new Label();
            
            SuspendLayout();
            
            // 
            // Title_Label
            // 
            Title_Label.AutoSize = true;
            Title_Label.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            Title_Label.Location = new Point(20, 20);
            Title_Label.Name = "Title_Label";
            Title_Label.Size = new Size(243, 30);
            Title_Label.TabIndex = 0;
            Title_Label.Text = "Manage Rental Inventory";
            
            // 
            // AddRentalItem_Button
            // 
            AddRentalItem_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddRentalItem_Button.BorderRadius = 4;
            AddRentalItem_Button.CustomizableEdges = customizableEdges1;
            AddRentalItem_Button.Font = new Font("Segoe UI", 10F);
            AddRentalItem_Button.ForeColor = Color.White;
            AddRentalItem_Button.Location = new Point(850, 20);
            AddRentalItem_Button.Name = "AddRentalItem_Button";
            AddRentalItem_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            AddRentalItem_Button.Size = new Size(180, 35);
            AddRentalItem_Button.TabIndex = 1;
            AddRentalItem_Button.Text = "Add Rental Item";
            AddRentalItem_Button.Click += AddRentalItem_Button_Click;
            
            // 
            // Search_TextBox
            // 
            Search_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Search_TextBox.CustomizableEdges = customizableEdges3;
            Search_TextBox.DefaultText = "";
            Search_TextBox.Font = new Font("Segoe UI", 9F);
            Search_TextBox.IconRight = Properties.Resources.CloseGray;
            Search_TextBox.IconRightOffset = new Point(5, 0);
            Search_TextBox.IconRightSize = new Size(22, 22);
            Search_TextBox.Location = new Point(1040, 22);
            Search_TextBox.MaxLength = 100;
            Search_TextBox.Name = "Search_TextBox";
            Search_TextBox.PlaceholderText = "Search rental items";
            Search_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges4;
            Search_TextBox.Size = new Size(250, 32);
            Search_TextBox.TabIndex = 2;
            Search_TextBox.IconRightClick += Search_TextBox_IconRightClick;
            Search_TextBox.TextChanged += Search_TextBox_TextChanged;
            
            // 
            // ShowingResultsFor_Label
            // 
            ShowingResultsFor_Label.Anchor = AnchorStyles.Top;
            ShowingResultsFor_Label.AutoSize = true;
            ShowingResultsFor_Label.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            ShowingResultsFor_Label.Location = new Point(500, 65);
            ShowingResultsFor_Label.Name = "ShowingResultsFor_Label";
            ShowingResultsFor_Label.Size = new Size(150, 20);
            ShowingResultsFor_Label.TabIndex = 3;
            ShowingResultsFor_Label.Text = "Showing results for";
            ShowingResultsFor_Label.TextAlign = ContentAlignment.MiddleCenter;
            ShowingResultsFor_Label.Visible = false;
            
            // 
            // Total_Label
            // 
            Total_Label.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Total_Label.AutoSize = true;
            Total_Label.Font = new Font("Segoe UI", 11F);
            Total_Label.Location = new Point(40, 650);
            Total_Label.Name = "Total_Label";
            Total_Label.Size = new Size(120, 20);
            Total_Label.TabIndex = 4;
            Total_Label.Text = "Total items: 0";
            
            // 
            // ManageRentals_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1300, 700);
            Controls.Add(Total_Label);
            Controls.Add(ShowingResultsFor_Label);
            Controls.Add(Search_TextBox);
            Controls.Add(AddRentalItem_Button);
            Controls.Add(Title_Label);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1000, 600);
            Name = "ManageRentals_Form";
            Text = "Manage Rental Inventory";
            FormClosed += ManageRentals_Form_FormClosed;
            Shown += ManageRentals_Form_Shown;
            Resize += ManageRentals_Form_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Title_Label;
        private Guna.UI2.WinForms.Guna2Button AddRentalItem_Button;
        private Guna.UI2.WinForms.Guna2TextBox Search_TextBox;
        private Label ShowingResultsFor_Label;
        private Label Total_Label;
    }
}