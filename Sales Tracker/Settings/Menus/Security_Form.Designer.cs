
namespace Sales_Tracker.Settings.Menus
{
    partial class Security_Form
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
            Security_Label = new Label();
            SuspendLayout();
            // 
            // Security_Label
            // 
            Security_Label.AutoSize = true;
            Security_Label.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold);
            Security_Label.Location = new Point(40, 22);
            Security_Label.Margin = new Padding(4, 0, 4, 0);
            Security_Label.Name = "Security_Label";
            Security_Label.Size = new Size(81, 25);
            Security_Label.TabIndex = 326;
            Security_Label.Text = "Security";
            // 
            // Security_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(875, 519);
            Controls.Add(Security_Label);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Security_Form";
            Resize += Security_form_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label Security_Label;
    }
}