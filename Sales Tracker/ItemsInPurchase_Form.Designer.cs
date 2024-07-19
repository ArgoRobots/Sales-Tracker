namespace Sales_Tracker
{
    partial class ItemsInPurchase_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Items_DataGridView = new Guna.UI2.WinForms.Guna2DataGridView();
            ExportReceipts_Label = new Label();
            ((System.ComponentModel.ISupportInitialize)Items_DataGridView).BeginInit();
            SuspendLayout();
            // 
            // Items_DataGridView
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            Items_DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            Items_DataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            Items_DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            Items_DataGridView.ColumnHeadersHeight = 4;
            Items_DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            Items_DataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            Items_DataGridView.GridColor = Color.FromArgb(231, 229, 255);
            Items_DataGridView.Location = new Point(12, 69);
            Items_DataGridView.Name = "Items_DataGridView";
            Items_DataGridView.RowHeadersVisible = false;
            Items_DataGridView.Size = new Size(1160, 280);
            Items_DataGridView.TabIndex = 11;
            Items_DataGridView.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            Items_DataGridView.ThemeStyle.AlternatingRowsStyle.Font = null;
            Items_DataGridView.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            Items_DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            Items_DataGridView.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            Items_DataGridView.ThemeStyle.BackColor = Color.White;
            Items_DataGridView.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            Items_DataGridView.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            Items_DataGridView.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            Items_DataGridView.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            Items_DataGridView.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            Items_DataGridView.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            Items_DataGridView.ThemeStyle.HeaderStyle.Height = 4;
            Items_DataGridView.ThemeStyle.ReadOnly = false;
            Items_DataGridView.ThemeStyle.RowsStyle.BackColor = Color.White;
            Items_DataGridView.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Items_DataGridView.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            Items_DataGridView.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            Items_DataGridView.ThemeStyle.RowsStyle.Height = 25;
            Items_DataGridView.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            Items_DataGridView.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // ExportReceipts_Label
            // 
            ExportReceipts_Label.Anchor = AnchorStyles.Top;
            ExportReceipts_Label.AutoSize = true;
            ExportReceipts_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ExportReceipts_Label.Location = new Point(504, 20);
            ExportReceipts_Label.Name = "ExportReceipts_Label";
            ExportReceipts_Label.Size = new Size(177, 30);
            ExportReceipts_Label.TabIndex = 12;
            ExportReceipts_Label.Text = "Items in purchase";
            // 
            // ItemsInPurchase_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 361);
            Controls.Add(ExportReceipts_Label);
            Controls.Add(Items_DataGridView);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(1200, 350);
            Name = "ItemsInPurchase_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)Items_DataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2DataGridView Items_DataGridView;
        private Label ExportReceipts_Label;
    }
}