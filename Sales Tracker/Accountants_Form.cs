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
        public static readonly List<string> thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Getters
        public static Accountants_Form Instance => _instance;

        // Init.
        public Accountants_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.SelectedDataGridView;
            ConstructDataGridViews();
            CenterSelectedDataGridView();
            LoadAccountants();
            Theme.SetThemeForForm(this);
            SetDoNotTranslateControls();
            LanguageManager.UpdateLanguageForControl(this);
            LabelManager.SetTotalLabel(Total_Label, accountants_DataGridView);
            Controls.Remove(ShowingResultsFor_Label);
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(accountants_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void LoadAccountants()
        {
            foreach (string accountant in MainMenu_Form.Instance.AccountantList)
            {
                accountants_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(accountants_DataGridView);
        }
        private void AddEventHandlersToTextBoxes()
        {
            Accountant_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(Accountant_TextBox);

            accountants_DataGridView.RowsAdded += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, accountants_DataGridView); };
            accountants_DataGridView.RowsRemoved += (sender, e) => { LabelManager.SetTotalLabel(Total_Label, accountants_DataGridView); };
        }
        private void SetDoNotTranslateControls()
        {
            AccountantName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCache;
        }

        // Form event handlers
        private void Accountants_Form_Shown(object sender, EventArgs e)
        {
            accountants_DataGridView.ClearSelection();
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void Accountants_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Accountants_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.SelectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void AddAccountant_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Accountant_TextBox.Text.Trim();
            MainMenu_Form.Instance.AccountantList.Add(name);
            int newRowIndex = accountants_DataGridView.Rows.Add(name);
            DataGridViewManager.DataGridViewRowsAdded(MainMenu_Form.Instance.SelectedDataGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added accountant '{name}'");

            Accountant_TextBox.Text = "";
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
            AccountantName,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.AccountantName, "Accountant" },
        };
        private Guna2DataGridView accountants_DataGridView;
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

            accountants_DataGridView = new();
            DataGridViewManager.InitializeDataGridView(accountants_DataGridView, "accountants_DataGridView", size, ColumnHeaders);
            accountants_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            accountants_DataGridView.Location = new Point((ClientSize.Width - accountants_DataGridView.Width) / 2, topForDataGridView);
            accountants_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Accountant;

            Controls.Add(accountants_DataGridView);
            MainMenu_Form.Instance.SelectedDataGridView = accountants_DataGridView;
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
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            CustomControls.CloseAllPanels(null, null);
        }
    }
}