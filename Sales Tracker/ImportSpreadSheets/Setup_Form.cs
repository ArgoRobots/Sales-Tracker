﻿using Sales_Tracker.Classes;
using Sales_Tracker.ImportSpreadSheets;

namespace Sales_Tracker.ImportSpreadsheets
{
    public partial class Setup_Form : Form
    {
        // Properties
        private static Setup_Form _instance;
        public readonly Form FormTutorial, FormImport;

        // Getters
        public static Setup_Form Instance => _instance;

        // Init.
        public Setup_Form()
        {
            InitializeComponent();
            _instance = this;

            FormTutorial = new Tutorial_Form();
            FormImport = new ImportSpreadSheets_Form();

            Theme.SetThemeForForm(this);
            AddForms();
        }
        private void AddForms()
        {
            FormTutorial.TopLevel = false;
            FormTutorial.Dock = DockStyle.Fill;
            FormTutorial.FormBorderStyle = FormBorderStyle.None;
            FormTutorial.Visible = true;
            FormTutorial.MinimumSize = new Size(0, 0);
            FormBack_Panel.Controls.Add(FormTutorial);

            FormImport.TopLevel = false;
            FormImport.Dock = DockStyle.Fill;
            FormImport.FormBorderStyle = FormBorderStyle.None;
            FormImport.Visible = true;
            FormImport.MinimumSize = new Size(0, 0);
            FormBack_Panel.Controls.Add(FormImport);

            FormTutorial.BringToFront();
        }
    }
}