namespace Sales_Tracker.ImportSpreadSheets
{
    partial class ImportSpreadSheets_Form
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
            ImportSpreadsheet_Label = new Label();
            SuspendLayout();
            // 
            // ImportSpreadsheet_Label
            // 
            ImportSpreadsheet_Label.Anchor = AnchorStyles.Top;
            ImportSpreadsheet_Label.AutoSize = true;
            ImportSpreadsheet_Label.Font = new Font("Segoe UI", 16F);
            ImportSpreadsheet_Label.Location = new Point(406, 30);
            ImportSpreadsheet_Label.Margin = new Padding(6, 0, 6, 0);
            ImportSpreadsheet_Label.Name = "ImportSpreadsheet_Label";
            ImportSpreadsheet_Label.Size = new Size(299, 45);
            ImportSpreadsheet_Label.TabIndex = 543;
            ImportSpreadsheet_Label.Text = "Import spreadsheet";
            // 
            // ImportSpreadSheets_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Controls.Add(ImportSpreadsheet_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "ImportSpreadSheets_Form";
            StartPosition = FormStartPosition.CenterScreen;
            Shown += ImportSpreadSheets_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ImportSpreadsheet_Label;
    }
}