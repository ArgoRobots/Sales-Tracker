using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

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
        public static Companies_Form Instance
        {
            get => _instance;
        }

        // Init.
        public Companies_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            AddEventHandlersToTextBoxes();
            ConstructDataGridViews();
            CenterSelectedDataGridView();
            LoadCompanies();
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
            MainMenu_Form.SortTheDataGridViewByFirstColumn(Company_DataGridView);
        }
        private void AddEventHandlersToTextBoxes()
        {
            Company_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Company_TextBox.KeyDown += UI.TextBox_KeyDown;
        }
        private void LoadCompanies()
        {
            MainMenu_Form.Instance.isProgramLoading = true;

            foreach (string accountant in MainMenu_Form.Instance.companyList)
            {
                Company_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(Company_DataGridView);
            MainMenu_Form.Instance.isProgramLoading = false;
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
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }
        private void Companies_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void AddCompany_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Company_TextBox.Text.Trim();
            MainMenu_Form.Instance.companyList.Add(name);
            int newRowIndex = Company_DataGridView.Rows.Add(name);
            MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added company '{name}'");

            Company_TextBox.Text = "";
            UI.SetGTextBoxToValid(Company_TextBox);
            HideCompanyWarning();
            Company_TextBox.Focus();
        }
        private void Company_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
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
                ShowShowingResultsForLabel(Search_TextBox.Text);
            }
            else
            {
                HideShowingResultsForLabel();
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
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(Width - 80, Height - topForDataGridView - 85);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - MainMenu_Form.spaceToOffsetFormNotCenter, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Company_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Company_DataGridView, size, ColumnHeaders);
            Company_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Company_DataGridView.Location = new Point((Width - Company_DataGridView.Width) / 2, topForDataGridView);
            Company_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Company;

            Controls.Add(Company_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Company_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Companies;
        }

        // Validate company name
        public void VaidateCompanyTextBox()
        {
            if (MainMenu_Form.Instance.companyList.Any(a => a == Company_TextBox.Text))
            {
                AddCompany_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Company_TextBox);
                ShowCompanyWarning();
            }
            else
            {
                AddCompany_Button.Enabled = true;
                UI.SetGTextBoxToValid(Company_TextBox);
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
            AddCompany_Button.Enabled = true;
            AddCompany_Button.Tag = true;
        }

        // SearchingFor_Label
        private void ShowShowingResultsForLabel(string text)
        {
            ShowingResultsFor_Label.Text = $"Showing results for: {text}";
            ShowingResultsFor_Label.Left = (Width - ShowingResultsFor_Label.Width) / 2 - 8;
            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
        }

        // Methods
        private void ValidateInputs()
        {
            AddCompany_Button.Enabled = !string.IsNullOrWhiteSpace(Company_TextBox.Text) && Company_TextBox.Tag.ToString() != "0";
        }
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}