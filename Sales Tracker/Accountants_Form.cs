using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Accountants_Form : Form
    {
        // Properties
        private static Accountants_Form _instance;
        public static readonly List<string> thingsThatHaveChangedInFile = [];
        private readonly MainMenu_Form.SelectedOption oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;

        // Getters and setters
        public static Accountants_Form Instance
        {
            get => _instance;
        }

        // Init.
        public Accountants_Form()
        {
            InitializeComponent();
            _instance = this;

            LoadingPanel.ShowBlankLoadingPanel(this);

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            CenterSelectedDataGridView();
            ConstructTotalLabel();
            LoadAccountants();
            Theme.SetThemeForForm(this);
            HideShowingResultsForLabel();
            MainMenu_Form.SortTheDataGridViewByFirstColumnAndSelectFirstRow(Accountants_DataGridView);
            AddEventHandlersToTextBoxes();
            SetTotalLabel();
        }
        private void AddEventHandlersToTextBoxes()
        {
            Accountant_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            Accountant_TextBox.PreviewKeyDown += UI.TextBox_PreviewKeyDown;
            Accountant_TextBox.KeyDown += UI.TextBox_KeyDown;

            Accountants_DataGridView.RowsAdded += (sender, e) => { SetTotalLabel(); };
            Accountants_DataGridView.RowsRemoved += (sender, e) => { SetTotalLabel(); };
        }
        private void LoadAccountants()
        {
            foreach (string accountant in MainMenu_Form.Instance.accountantList)
            {
                Accountants_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(Accountants_DataGridView);
        }

        // Form event handlers
        private void Accountants_Form_Shown(object sender, EventArgs e)
        {
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
            MainMenu_Form.Instance.selectedDataGridView = oldSelectedDataGridView;
        }

        // Event handlers
        private void AddAccountant_Button_Click(object sender, EventArgs e)
        {
            CloseAllPanels(null, null);
            string name = Accountant_TextBox.Text.Trim();
            MainMenu_Form.Instance.accountantList.Add(name);
            int newRowIndex = Accountants_DataGridView.Rows.Add(name);
            MainMenu_Form.Instance.DataGridViewRowsAdded(new DataGridViewRowsAddedEventArgs(newRowIndex, 1));

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
            ValidateInputs();
            VaidateAccountantTextBox();
        }
        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Tools.SearchSelectedDataGridView(Search_TextBox))
            {
                ShowShowingResultsForLabel(Search_TextBox.Text.Trim());
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
        private const byte topForDataGridView = 250;
        private void CenterSelectedDataGridView()
        {
            if (MainMenu_Form.Instance.selectedDataGridView == null) { return; }
            MainMenu_Form.Instance.selectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.selectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.selectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Accountants_DataGridView = new Guna2DataGridView();
            MainMenu_Form.Instance.InitializeDataGridView(Accountants_DataGridView, size, ColumnHeaders);
            Accountants_DataGridView.ColumnWidthChanged -= MainMenu_Form.Instance.DataGridView_ColumnWidthChanged;
            Accountants_DataGridView.Location = new Point((ClientSize.Width - Accountants_DataGridView.Width) / 2, topForDataGridView);
            Accountants_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Accountant;

            Controls.Add(Accountants_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Accountants_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.SelectedOption.Accountants;
        }

        // Label
        private Label totalLabel;
        private void ConstructTotalLabel()
        {
            totalLabel = new Label
            {
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                ForeColor = CustomColors.text,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(totalLabel);
        }
        private void SetTotalLabel()
        {
            totalLabel.Text = $"Total: {Accountants_DataGridView.Rows.Count}";
            totalLabel.Location = new Point(Accountants_DataGridView.Right - totalLabel.Width, Accountants_DataGridView.Bottom + 10);
        }

        // Validate accountant name
        public void VaidateAccountantTextBox()
        {
            bool exists = MainMenu_Form.Instance.accountantList.Any(a => string.Equals(a, Accountant_TextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
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
            ShowingResultsFor_Label.Left = (ClientSize.Width - ShowingResultsFor_Label.Width) / 2;
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
        public void CloseAllPanels(object sender, EventArgs? e)
        {
            UI.CloseAllPanels(null, null);
        }
    }
}