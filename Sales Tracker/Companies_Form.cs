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
        public static readonly List<string> thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Getters and setters
        public static Companies_Form Instance => _instance;

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
            SetDoNotTranslateControls();
            LanguageManager.UpdateLanguageForForm(this);
            LabelManager.SetTotalLabel(Total_Label, Company_DataGridView);
            Controls.Remove(ShowingResultsFor_Label);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Company_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void LoadCompanies()
        {
            MainMenu_Form.Instance.isProgramLoading = true;

            foreach (string accountant in MainMenu_Form.Instance.CompanyList)
            {
                Company_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(Company_DataGridView);
            MainMenu_Form.Instance.isProgramLoading = false;
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxManager.Attach(Company_TextBox);

            Company_DataGridView.RowsAdded += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, Company_DataGridView); };
            Company_DataGridView.RowsRemoved += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, Company_DataGridView); };
        }
        private void SetDoNotTranslateControls()
        {
            CompanyName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCacheText;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCacheText;
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
            Company_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCompany_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Company_TextBox.Text.Trim();
            MainMenu_Form.Instance.CompanyList.Add(name);
            int newRowIndex = Company_DataGridView.Rows.Add(name);
            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, name);
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
        public Guna2DataGridView Company_DataGridView;
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

            Company_DataGridView = new Guna2DataGridView();
            DataGridViewManager.InitializeDataGridView(Company_DataGridView, size, ColumnHeaders);
            Company_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            Company_DataGridView.Location = new Point((ClientSize.Width - Company_DataGridView.Width) / 2, topForDataGridView);
            Company_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Company;

            Controls.Add(Company_DataGridView);
            MainMenu_Form.Instance.SelectedDataGridView = Company_DataGridView;
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
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}