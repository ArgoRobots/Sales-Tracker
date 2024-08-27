namespace Sales_Tracker
{
    partial class DateRange_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DateRange_Label = new Label();
            From_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            From_Label = new Label();
            To_DateTimePicker = new Guna.UI2.WinForms.Guna2DateTimePicker();
            To_Label = new Label();
            Apply_Button = new Guna.UI2.WinForms.Guna2Button();
            Reset_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // DateRange_Label
            // 
            DateRange_Label.AutoSize = true;
            DateRange_Label.Font = new Font("Segoe UI", 16F);
            DateRange_Label.Location = new Point(199, 21);
            DateRange_Label.Name = "DateRange_Label";
            DateRange_Label.Size = new Size(252, 45);
            DateRange_Label.TabIndex = 1;
            DateRange_Label.Text = "Set a date range";
            // 
            // From_DateTimePicker
            // 
            From_DateTimePicker.Anchor = AnchorStyles.Top;
            From_DateTimePicker.Checked = true;
            From_DateTimePicker.CustomizableEdges = customizableEdges1;
            From_DateTimePicker.FillColor = Color.White;
            From_DateTimePicker.Font = new Font("Segoe UI", 9F);
            From_DateTimePicker.Format = DateTimePickerFormat.Long;
            From_DateTimePicker.Location = new Point(175, 136);
            From_DateTimePicker.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            From_DateTimePicker.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            From_DateTimePicker.Name = "From_DateTimePicker";
            From_DateTimePicker.ShadowDecoration.CustomizableEdges = customizableEdges2;
            From_DateTimePicker.Size = new Size(300, 50);
            From_DateTimePicker.TabIndex = 8;
            From_DateTimePicker.Value = new DateTime(2024, 6, 6, 19, 37, 49, 128);
            // 
            // From_Label
            // 
            From_Label.Anchor = AnchorStyles.Top;
            From_Label.AutoSize = true;
            From_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            From_Label.Location = new Point(175, 102);
            From_Label.Name = "From_Label";
            From_Label.Size = new Size(66, 31);
            From_Label.TabIndex = 7;
            From_Label.Text = "From";
            // 
            // To_DateTimePicker
            // 
            To_DateTimePicker.Anchor = AnchorStyles.Top;
            To_DateTimePicker.Checked = true;
            To_DateTimePicker.CustomizableEdges = customizableEdges3;
            To_DateTimePicker.FillColor = Color.White;
            To_DateTimePicker.Font = new Font("Segoe UI", 9F);
            To_DateTimePicker.Format = DateTimePickerFormat.Long;
            To_DateTimePicker.Location = new Point(175, 223);
            To_DateTimePicker.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            To_DateTimePicker.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            To_DateTimePicker.Name = "To_DateTimePicker";
            To_DateTimePicker.ShadowDecoration.CustomizableEdges = customizableEdges4;
            To_DateTimePicker.Size = new Size(300, 50);
            To_DateTimePicker.TabIndex = 10;
            To_DateTimePicker.Value = new DateTime(2024, 6, 6, 19, 37, 49, 128);
            // 
            // To_Label
            // 
            To_Label.Anchor = AnchorStyles.Top;
            To_Label.AutoSize = true;
            To_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            To_Label.Location = new Point(175, 189);
            To_Label.Name = "To_Label";
            To_Label.Size = new Size(37, 31);
            To_Label.TabIndex = 9;
            To_Label.Text = "To";
            // 
            // Apply_Button
            // 
            Apply_Button.Anchor = AnchorStyles.Bottom;
            Apply_Button.BackColor = Color.Transparent;
            Apply_Button.BorderColor = Color.LightGray;
            Apply_Button.BorderRadius = 2;
            Apply_Button.BorderThickness = 1;
            Apply_Button.CustomizableEdges = customizableEdges5;
            Apply_Button.FillColor = Color.White;
            Apply_Button.Font = new Font("Segoe UI", 10F);
            Apply_Button.ForeColor = Color.Black;
            Apply_Button.Location = new Point(328, 326);
            Apply_Button.Name = "Apply_Button";
            Apply_Button.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Apply_Button.Size = new Size(215, 45);
            Apply_Button.TabIndex = 14;
            Apply_Button.Tag = "";
            Apply_Button.Text = "Apply";
            Apply_Button.Click += Apply_Button_Click;
            // 
            // Reset_Button
            // 
            Reset_Button.Anchor = AnchorStyles.Bottom;
            Reset_Button.BackColor = Color.Transparent;
            Reset_Button.BorderColor = Color.LightGray;
            Reset_Button.BorderRadius = 2;
            Reset_Button.BorderThickness = 1;
            Reset_Button.CustomizableEdges = customizableEdges7;
            Reset_Button.FillColor = Color.White;
            Reset_Button.Font = new Font("Segoe UI", 10F);
            Reset_Button.ForeColor = Color.Black;
            Reset_Button.Location = new Point(107, 326);
            Reset_Button.Name = "Reset_Button";
            Reset_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            Reset_Button.Size = new Size(215, 45);
            Reset_Button.TabIndex = 15;
            Reset_Button.Tag = "";
            Reset_Button.Text = "Reset";
            Reset_Button.Click += Reset_Button_Click;
            // 
            // DateRange_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(651, 392);
            Controls.Add(Reset_Button);
            Controls.Add(Apply_Button);
            Controls.Add(To_DateTimePicker);
            Controls.Add(To_Label);
            Controls.Add(From_DateTimePicker);
            Controls.Add(From_Label);
            Controls.Add(DateRange_Label);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "DateRange_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Shown += DateRange_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label DateRange_Label;
        private Guna.UI2.WinForms.Guna2DateTimePicker From_DateTimePicker;
        private Label From_Label;
        private Guna.UI2.WinForms.Guna2DateTimePicker To_DateTimePicker;
        private Label To_Label;
        private Guna.UI2.WinForms.Guna2Button Apply_Button;
        private Guna.UI2.WinForms.Guna2Button Reset_Button;
    }
}