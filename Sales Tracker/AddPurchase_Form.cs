using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sales_Tracker
{
    public partial class AddPurchase_Form : Form
    {
        public AddPurchase_Form()
        {
            InitializeComponent();
        }

        private void AddPurchase_Button_Click(object sender, EventArgs e)
        {
            // Retrieve the input values
            string purchaseID = PurchaseID_TextBox.Text;
            string buyerName = BuyerName_TextBox.Text;
            string itemName = ItemName_TextBox.Text;
            int quantity = int.Parse(Quantity_TextBox.Text);
            decimal pricePerUnit = decimal.Parse(PricePerUnit_TextBox.Text);
            decimal shipping = decimal.Parse(Shipping_TextBox.Text);
            decimal tax = decimal.Parse(Tax_TextBox.Text);
            decimal totalPrice = decimal.Parse(TotalPrice_TextBox.Text);

            // Create a new DataGridViewRow and add it to the DataGridView
            DataGridViewRow row = new();
            row.CreateCells(MainMenu_Form.Instance.Items_DataGridView);

            row.Cells[0].Value = purchaseID;
            row.Cells[1].Value = buyerName;
            row.Cells[2].Value = itemName;
            row.Cells[3].Value = quantity;
            row.Cells[4].Value = pricePerUnit;
            row.Cells[5].Value = shipping;
            row.Cells[6].Value = tax;
            row.Cells[7].Value = totalPrice;

            MainMenu_Form.Instance.Items_DataGridView.Rows.Add(row);
            Close();
        }

        // Event handlers
        private void ImportAmazon_Button_Click(object sender, EventArgs e)
        {

        }
        private void ImportEbay_Button_Click(object sender, EventArgs e)
        {

        }
    }
}