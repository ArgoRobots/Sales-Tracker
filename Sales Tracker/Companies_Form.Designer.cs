﻿using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    partial class Companies_Form
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
            WarningCompanyName_PictureBox = new PictureBox();
            WarningCompanyName_Label = new Label();
            CompanyName_Label = new Label();
            Company_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            AddCompany_Button = new Guna.UI2.WinForms.Guna2Button();
            AddCompany_Label = new Label();
            Search_TextBox = new Guna.UI2.WinForms.Guna2TextBox();
            ShowingResultsFor_Label = new Label();
            ((System.ComponentModel.ISupportInitialize)WarningCompanyName_PictureBox).BeginInit();
            SuspendLayout();
            // 
            // WarningCompanyName_PictureBox
            // 
            WarningCompanyName_PictureBox.Anchor = AnchorStyles.Top;
            WarningCompanyName_PictureBox.Image = Properties.Resources.Warning;
            WarningCompanyName_PictureBox.Location = new Point(114, 145);
            WarningCompanyName_PictureBox.Margin = new Padding(4, 3, 4, 3);
            WarningCompanyName_PictureBox.Name = "WarningCompanyName_PictureBox";
            WarningCompanyName_PictureBox.Size = new Size(19, 19);
            WarningCompanyName_PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WarningCompanyName_PictureBox.TabIndex = 53;
            WarningCompanyName_PictureBox.TabStop = false;
            WarningCompanyName_PictureBox.Visible = false;
            WarningCompanyName_PictureBox.Click += CloseAllPanels;
            // 
            // WarningCompanyName_Label
            // 
            WarningCompanyName_Label.Anchor = AnchorStyles.Top;
            WarningCompanyName_Label.AutoSize = true;
            WarningCompanyName_Label.Font = new Font("Segoe UI", 10F);
            WarningCompanyName_Label.Location = new Point(141, 145);
            WarningCompanyName_Label.Margin = new Padding(4, 0, 4, 0);
            WarningCompanyName_Label.Name = "WarningCompanyName_Label";
            WarningCompanyName_Label.Size = new Size(153, 19);
            WarningCompanyName_Label.TabIndex = 52;
            WarningCompanyName_Label.Text = "Company already exists";
            WarningCompanyName_Label.Visible = false;
            WarningCompanyName_Label.Click += CloseAllPanels;
            // 
            // CompanyName_Label
            // 
            CompanyName_Label.Anchor = AnchorStyles.Top;
            CompanyName_Label.AutoSize = true;
            CompanyName_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CompanyName_Label.Location = new Point(114, 80);
            CompanyName_Label.Name = "CompanyName_Label";
            CompanyName_Label.Size = new Size(113, 20);
            CompanyName_Label.TabIndex = 48;
            CompanyName_Label.Text = "Company name";
            CompanyName_Label.Click += CloseAllPanels;
            // 
            // Company_TextBox
            // 
            Company_TextBox.Anchor = AnchorStyles.Top;
            Company_TextBox.CustomizableEdges = customizableEdges1;
            Company_TextBox.DefaultText = "";
            Company_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Company_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Company_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Company_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Company_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Company_TextBox.Font = new Font("Segoe UI", 9F);
            Company_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Company_TextBox.Location = new Point(114, 103);
            Company_TextBox.MaxLength = 32;
            Company_TextBox.Name = "Company_TextBox";
            Company_TextBox.PasswordChar = '\0';
            Company_TextBox.PlaceholderText = "";
            Company_TextBox.SelectedText = "";
            Company_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Company_TextBox.ShortcutsEnabled = false;
            Company_TextBox.Size = new Size(200, 36);
            Company_TextBox.TabIndex = 50;
            Company_TextBox.Tag = "1";
            Company_TextBox.TextChanged += Company_TextBox_TextChanged;
            Company_TextBox.KeyDown += Company_TextBox_KeyDown;
            // 
            // AddCompany_Button
            // 
            AddCompany_Button.Anchor = AnchorStyles.Top;
            AddCompany_Button.BackColor = Color.Transparent;
            AddCompany_Button.BorderColor = Color.LightGray;
            AddCompany_Button.BorderRadius = 2;
            AddCompany_Button.BorderThickness = 1;
            AddCompany_Button.CustomizableEdges = customizableEdges3;
            AddCompany_Button.Enabled = false;
            AddCompany_Button.FillColor = Color.White;
            AddCompany_Button.Font = new Font("Segoe UI", 9.5F);
            AddCompany_Button.ForeColor = Color.Black;
            AddCompany_Button.Location = new Point(321, 103);
            AddCompany_Button.Margin = new Padding(4, 3, 4, 3);
            AddCompany_Button.Name = "AddCompany_Button";
            AddCompany_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            AddCompany_Button.Size = new Size(143, 36);
            AddCompany_Button.TabIndex = 51;
            AddCompany_Button.Tag = "";
            AddCompany_Button.Text = "Add company";
            AddCompany_Button.Click += AddCompany_Button_Click;
            // 
            // AddCompany_Label
            // 
            AddCompany_Label.Anchor = AnchorStyles.Top;
            AddCompany_Label.AutoSize = true;
            AddCompany_Label.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddCompany_Label.Location = new Point(314, 20);
            AddCompany_Label.Name = "AddCompany_Label";
            AddCompany_Label.Size = new Size(157, 30);
            AddCompany_Label.TabIndex = 49;
            AddCompany_Label.Text = "Add companies";
            AddCompany_Label.Click += CloseAllPanels;
            // 
            // Search_TextBox
            // 
            Search_TextBox.Anchor = AnchorStyles.Top;
            Search_TextBox.CustomizableEdges = customizableEdges5;
            Search_TextBox.DefaultText = "";
            Search_TextBox.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            Search_TextBox.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            Search_TextBox.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            Search_TextBox.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            Search_TextBox.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            Search_TextBox.Font = new Font("Segoe UI", 9F);
            Search_TextBox.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            Search_TextBox.Location = new Point(471, 103);
            Search_TextBox.MaxLength = 32;
            Search_TextBox.Name = "Search_TextBox";
            Search_TextBox.PasswordChar = '\0';
            Search_TextBox.PlaceholderText = "Search for products";
            Search_TextBox.SelectedText = "";
            Search_TextBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Search_TextBox.ShortcutsEnabled = false;
            Search_TextBox.Size = new Size(200, 36);
            Search_TextBox.TabIndex = 54;
            Search_TextBox.TextChanged += Search_TextBox_TextChanged;
            // 
            // ShowingResultsFor_Label
            // 
            ShowingResultsFor_Label.Anchor = AnchorStyles.Top;
            ShowingResultsFor_Label.AutoSize = true;
            ShowingResultsFor_Label.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ShowingResultsFor_Label.Location = new Point(325, 142);
            ShowingResultsFor_Label.Name = "ShowingResultsFor_Label";
            ShowingResultsFor_Label.Size = new Size(135, 20);
            ShowingResultsFor_Label.TabIndex = 55;
            ShowingResultsFor_Label.Text = "Showing results for";
            // 
            // Companies_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 471);
            Controls.Add(ShowingResultsFor_Label);
            Controls.Add(Search_TextBox);
            Controls.Add(WarningCompanyName_PictureBox);
            Controls.Add(WarningCompanyName_Label);
            Controls.Add(CompanyName_Label);
            Controls.Add(Company_TextBox);
            Controls.Add(AddCompany_Button);
            Controls.Add(AddCompany_Label);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            KeyPreview = true;
            Name = "Companies_Form";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += Companies_Form_FormClosed;
            Resize += Companies_Form_Resize;
            ((System.ComponentModel.ISupportInitialize)WarningCompanyName_PictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox WarningCompanyName_PictureBox;
        private Label WarningCompanyName_Label;
        private Label CompanyName_Label;
        private Guna.UI2.WinForms.Guna2TextBox Company_TextBox;
        private Guna.UI2.WinForms.Guna2Button AddCompany_Button;
        private Label AddCompany_Label;
        private Guna.UI2.WinForms.Guna2TextBox Search_TextBox;
        private Label ShowingResultsFor_Label;
    }
}