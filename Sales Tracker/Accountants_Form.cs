using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Accountants_Form : Form
    {
        // Properties
        public static readonly List<string> thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Init.
        public static Accountants_Form Instance { get; private set; }
        public Accountants_Form()
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            AddEventHandlersToTextBoxes();
            ConstructDataGridViews();
            LoadAccountants();
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
        }
        private void AddEventHandlersToTextBoxes()
        {
            Accountant_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            Accountant_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Accountant_TextBox.KeyDown += UI.TextBox_KeyDown;
        }
        private void LoadAccountants()
        {
            MainMenu_Form.Instance.isProgramLoading = true;

            foreach (string accountant in MainMenu_Form.Instance.accountantList)
            {
                Accountants_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(Accountants_DataGridView);
            MainMenu_Form.Instance.isProgramLoading = false;
        }

        // Form event handlers
        private void Accountants_Form_Resize(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            CenterSelectedDataGridView();
        }
        private void Accountants_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainMenu_Form.Instance.Selected = oldOption;
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void AddAccountant_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Accountant_TextBox.Text.Trim();
            MainMenu_Form.Instance.accountantList.Add(name);
            Accountants_DataGridView.Rows.Add(name);

            CustomMessage_Form.AddThingThatHasChanged(thingsThatHaveChangedInFile, name);
            Log.Write(3, $"Added accountant '{name}'");

            Accountant_TextBox.Text = "";
            UI.SetGTextBoxToValid(Accountant_TextBox);
            HideAccountantWarning();
            Accountant_TextBox.Focus();
        }
        private void Accountant_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
                if (AddAccountant_Button.Enabled)
                {
                    AddAccountant_Button.PerformClick();
                }
            }
        }
        private void Accountant_TextBox_TextChanged(object sender, EventArgs e)
        {
            VaidateAccountantTextBox();
            ValidateInputs();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in MainMenu_Form.Instance.selectedDataGridView.Rows)
            {
                bool isVisible = row.Cells.Cast<DataGridViewCell>()
                                          .Any(cell => cell.Value != null && cell.Value.ToString().Contains(Search_TextBox.Text, StringComparison.OrdinalIgnoreCase));
                row.Visible = isVisible;
            }
            if (Search_TextBox.Text != "")
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
            AccountantName,
        }
        public readonly Dictionary<Columns, string> ColumnHeaders = new()
        {
            { Columns.AccountantName, "Accountant" },
        };
        public Guna2DataGridView Accountants_DataGridView;
        private const byte topForDataGridView = 170;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(Width - 55, Height - topForDataGridView - 57);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2 - 8, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Accountants_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Accountants_DataGridView, size);
            Accountants_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            MainMenu_Form.LoadColumnsInDataGridView(Accountants_DataGridView, ColumnHeaders);
            Accountants_DataGridView.Location = new Point((Width - Accountants_DataGridView.Width) / 2, topForDataGridView);
            Accountants_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Accountant;

            Controls.Add(Accountants_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Accountants_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Accountants;
        }

        // Validate accountant name
        public void VaidateAccountantTextBox()
        {
            if (MainMenu_Form.Instance.accountantList.Any(a => a == Accountant_TextBox.Text))
            {
                AddAccountant_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Accountant_TextBox);
                ShowAccountantWarning();
            }
            else
            {
                AddAccountant_Button.Enabled = true;
                UI.SetGTextBoxToValid(Accountant_TextBox);
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
            AddAccountant_Button.Enabled = true;
            AddAccountant_Button.Tag = true;
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
            if (AddAccountant_Button.Tag is bool and true)
            {
                AddAccountant_Button.Enabled = !string.IsNullOrWhiteSpace(Accountant_TextBox.Text) && Accountant_TextBox.Tag.ToString() != "0";
            }
        }
        public void CloseAllPanels(object? sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}