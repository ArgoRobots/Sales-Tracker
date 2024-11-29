using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Accountants_Form : Form
    {
        // Properties
        private static Accountants_Form _instance;
        private static readonly List<string> _thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;

        // Getters
        public static Accountants_Form Instance => _instance;
        public static List<string> ThingsThatHaveChangedInFile => _thingsThatHaveChangedInFile;

        // Init.
        public Accountants_Form()
        {
            InitializeComponent();
            _instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            ConstructDataGridViews();
            CenterDataGridView();
            LoadAccountants();
            Theme.SetThemeForForm(this);
            Guna2TextBoxIconHoverEffect.Initialize(Search_TextBox);
            SetAccessibleDescriptions();
            LanguageManager.UpdateLanguageForControl(this);
            LabelManager.ShowTotalLabel(Total_Label, accountant_DataGridView);
            ShowingResultsFor_Label.Visible = false;
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(accountant_DataGridView);
            AddEventHandlersToTextBoxes();
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void LoadAccountants()
        {
            foreach (string accountant in MainMenu_Form.Instance.AccountantList)
            {
                accountant_DataGridView.Rows.Add(accountant);
            }
            DataGridViewManager.ScrollToTopOfDataGridView(accountant_DataGridView);
        }
        private void AddEventHandlersToTextBoxes()
        {
            TextBoxValidation.OnlyAllowLetters(Accountant_TextBox);
            TextBoxManager.Attach(Accountant_TextBox);
            TextBoxManager.Attach(Search_TextBox);

            accountant_DataGridView.RowsAdded += (_, _) => LabelManager.ShowTotalLabel(Total_Label, accountant_DataGridView);
            accountant_DataGridView.RowsRemoved += (_, _) => LabelManager.ShowTotalLabel(Total_Label, accountant_DataGridView);
        }
        private void SetAccessibleDescriptions()
        {
            AccountantName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Form event handlers
        private void Accountants_Form_Shown(object sender, EventArgs e)
        {
            accountant_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Accountants_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterDataGridView();
        }
        private void Accountants_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
        }

        // Event handlers
        private void AddAccountant_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Accountant_TextBox.Text.Trim();
            MainMenu_Form.Instance.AccountantList.Add(name);
            int newRowIndex = accountant_DataGridView.Rows.Add(name);
            DataGridViewManager.DataGridViewRowsAdded(accountant_DataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(_thingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added accountant '{name}'");

            Accountant_TextBox.Clear();
            CustomControls.SetGTextBoxToValid(Accountant_TextBox);
            HideAccountantWarning();
            Accountant_TextBox.Focus();
        }
        private void Accountant_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (AddAccountant_Button.Enabled)
                {
                    AddAccountant_Button.PerformClick();
                }
            }
        }
        private void Accountant_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
            VaidateAccountantTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (DataGridViewManager.SearchSelectedDataGridViewAndUpdateRowColors(accountant_DataGridView, Search_TextBox))
            {
                LabelManager.ShowShowingResultsLabel(ShowingResultsFor_Label, Search_TextBox.Text.Trim(), this);
            }
            else
            {
                ShowingResultsFor_Label.Visible = false;
            }
            LabelManager.ShowTotalLabel(Total_Label, accountant_DataGridView);
        }
        private void Search_TextBox_IconRightClick(object sender, EventArgs e)
        {
            Search_TextBox.Clear();
        }

        // DataGridView
        public enum Column
        {
            AccountantName,
        }
        public readonly Dictionary<Column, string> ColumnHeaders = new()
        {
            { Column.AccountantName, "Accountant" },
        };
        private Guna2DataGridView accountant_DataGridView;
        private const byte topForDataGridView = 250;
        private void CenterDataGridView()
        {
            if (accountant_DataGridView == null) { return; }
            accountant_DataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            accountant_DataGridView.Location = new Point((ClientSize.Width - accountant_DataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            accountant_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(accountant_DataGridView, "accountants_DataGridView", size, ColumnHeaders, null, this);
            accountant_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            accountant_DataGridView.Location = new Point((ClientSize.Width - accountant_DataGridView.Width) / 2, topForDataGridView);
            accountant_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Accountant;

            Controls.Add(accountant_DataGridView);
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Accountants;
        }

        // Validate accountant name
        public void VaidateAccountantTextBox()
        {
            bool exists = MainMenu_Form.Instance.AccountantList.Any(a => string.Equals(a, Accountant_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                AddAccountant_Button.Enabled = false;
                CustomControls.SetGTextBoxToInvalid(Accountant_TextBox);
                ShowAccountantWarning();
            }
            else
            {
                if (Accountant_TextBox.Text != "")
                {
                    AddAccountant_Button.Enabled = true;
                }
                CustomControls.SetGTextBoxToValid(Accountant_TextBox);
                HideAccountantWarning();
            }
        }
        private void ShowAccountantWarning()
        {
            WarningAccountantName_PictureBox.Visible = true;
            WarningAccountantName_Label.Visible = true;
            AddAccountant_Button.Enabled = false;
            AddAccountant_Button.Tag = false;
        }
        private void HideAccountantWarning()
        {
            WarningAccountantName_PictureBox.Visible = false;
            WarningAccountantName_Label.Visible = false;

            if (Accountant_TextBox.Text != "")
            {
                AddAccountant_Button.Enabled = true;
                AddAccountant_Button.Tag = true;
            }
        }

        // Methods
        private void ValidateInputs()
        {
            if (AddAccountant_Button.Tag is bool and true)
            {
                AddAccountant_Button.Enabled = !string.IsNullOrWhiteSpace(Accountant_TextBox.Text) && Accountant_TextBox.Tag.ToString() != "0";
            }
        }
        private void CloseAllPanels(object sender, EventArgs? e)
        {
            SearchBox.CloseSearchBox();
        }
    }
}