using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Companies_Form : Form
    {
        // Properties
        private static Companies_Form _instance;
        private static readonly List<string> _thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Getters
        public static Companies_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;

        // Init.
        public Companies_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;
            ConstructDataGridViews();
            CenterSelectedDataGridView();
            LoadCompanies();
            Theme.SetThemeForForm(this);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            LabelManager.ShowTotalLabel(Total_Label, company_DataGridView);
            Controls.Remove(ShowingResultsFor_Label);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(company_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void LoadCompanies()
        {
            MainMenu_Form.IsProgramLoading = true;

            foreach (string accountant in MainMenu_Form.Instance.CompanyList)
            {
                company_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(company_DataGridView);
            MainMenu_Form.IsProgramLoading = false;
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Company_TextBox);

            company_DataGridView.RowsAdded += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, company_DataGridView); };
            company_DataGridView.RowsRemoved += (sender, e) => { LabelManager.ShowTotalLabel(Total_Label, company_DataGridView); };
        }
        private void SetAccessibleDescriptions()
        {
            CompanyName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Form event handlers
        private void Companies_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Companies_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }
        private void Companies_Form_Shown(object sender, EventArgs e)
        {
            company_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCompany_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Company_TextBox.Text.Trim();
            MainMenu_Form.Instance.CompanyList.Add(name);
            int newRowIndex = company_DataGridView.Rows.Add(name);
            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(_thingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added company '{name}'");

            Company_TextBox.Text = "";
            CustomControls.SetGTextBoxToValid(Company_TextBox);
            HideCompanyWarning();
            Company_TextBox.Focus();
        }
        private void Company_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (AddCompany_Button.Enabled)
                {
                    AddCompany_Button.PerformClick();
                }
            }
        }
        private void Company_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
            VaidateCompanyTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Tools.SearchSelectedDataGridView(Search_TextBox))
            {
                LabelManager.ShowShowingResultsLabel(ShowingResultsFor_Label, Search_TextBox.Text.Trim(), this);
            }
            else
            {
                Controls.Remove(ShowingResultsFor_Label);
            }
        }

        // DataGridView
        public enum Columns
        {
            Company,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.Company, "Company" },
        };
        private Guna2DataGridView company_DataGridView;
        private const byte topForDataGridView = 250;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.SelectedDataGridView == null) { return; }
            MainMenu_Form.Instance.SelectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.SelectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.SelectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            company_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(company_DataGridView, "company_DataGridView", size, ColumnHeaders);
            company_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            company_DataGridView.Location = new Point((ClientSize.Width - company_DataGridView.Width) / 2, topForDataGridView);
            company_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Company;

            Controls.Add(company_DataGridView);
            MainMenu_Form.Instance.SelectedDataGridView = company_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Companies;
        }

        // Validate company name
        public void VaidateCompanyTextBox()
        {
            bool exists = MainMenu_Form.Instance.CompanyList.Any(a => string.Equals(a, Company_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                AddCompany_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(Company_TextBox);
                ShowCompanyWarning();
            }
            else
            {
                if (Company_TextBox.Text != "")
                {
                    AddCompany_Button.Enabled = true;
                }
                CustomControls.SetGTextBoxToValid(Company_TextBox);
                HideCompanyWarning();
            }
        }
        private void ShowCompanyWarning()
        {
            WarningCompanyName_PictureBox.Visible = true;
            WarningCompanyName_Label.Visible = true;
            AddCompany_Button.Enabled = false;
            AddCompany_Button.Tag = false;
        }
        private void HideCompanyWarning()
        {
            WarningCompanyName_PictureBox.Visible = false;
            WarningCompanyName_Label.Visible = false;

            if (Company_TextBox.Text != "")
            {
                AddCompany_Button.Enabled = true;
                AddCompany_Button.Tag = true;
            }
        }

        // Methods
        private void ValidateInputs()
        {
            AddCompany_Button.Enabled = !string.IsNullOrWhiteSpace(Company_TextBox.Text) && Company_TextBox.Tag.ToString() != "0";
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}