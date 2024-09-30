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
            LanguageManager.UpdateLanguageForForm(this);
            SetTotalLabel();
            HideShowingResultsForLabel();
            DataGridViewManager.SortFirstColumnAndSelectFirstRow(Accountants_DataGridView);
            AddEventHandlersToTextBoxes();
        }
        private void LoadAccountants()
        {
            foreach (string accountant in MainMenu_Form.Instance.AccountantList)
            {
                Accountants_DataGridView.Rows.Add(accountant);
            }
            Tools.ScrollToTopOfDataGridView(Accountants_DataGridView);
        }
        private void AddEventHandlersToTextBoxes()
        {
            Accountant_TextBox.KeyPress += Tools.OnlyAllowLettersInTextBox;
            TextBoxManager.Attach(Accountant_TextBox);

            Accountants_DataGridView.RowsAdded += (sender, e) => { SetTotalLabel(); };
            Accountants_DataGridView.RowsRemoved += (sender, e) => { SetTotalLabel(); };
        }
        private void SetDoNotTranslateControls()
        {
            AccountantName_Label.AccessibleDescription = AccessibleDescriptionStrings.AlignLeftCenter;
            ShowingResultsFor_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCacheText;
            Total_Label.AccessibleDescription = AccessibleDescriptionStrings.DoNotCacheText;
        }

        // Form event handlers
        private void Accountants_Form_Shown(object sender, EventArgs e)
        {
            Accountants_DataGridView.ClearSelection();
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
            int newRowIndex = Accountants_DataGridView.Rows.Add(name);
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
            if (MainMenu_Form.Instance.SelectedDataGridView == null) { return; }
            MainMenu_Form.Instance.SelectedDataGridView.Size = new Size(ClientSize.Width - 80, ClientSize.Height - topForDataGridView - 70);
            MainMenu_Form.Instance.SelectedDataGridView.Location = new Point((ClientSize.Width - MainMenu_Form.Instance.SelectedDataGridView.Width) / 2, topForDataGridView);
        }
        private void ConstructDataGridViews()
        {
            Size size = new(740, 280);

            Accountants_DataGridView = new Guna2DataGridView();
            DataGridViewManager.InitializeDataGridView(Accountants_DataGridView, size, ColumnHeaders);
            Accountants_DataGridView.ColumnWidthChanged -= DataGridViewManager.DataGridView_ColumnWidthChanged;
            Accountants_DataGridView.Location = new Point((ClientSize.Width - Accountants_DataGridView.Width) / 2, topForDataGridView);
            Accountants_DataGridView.Tag = MainMenu_Form.DataGridViewTag.Accountant;

            Controls.Add(Accountants_DataGridView);
            MainMenu_Form.Instance.SelectedDataGridView = Accountants_DataGridView;
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

        // Set labels
        private void ShowShowingResultsForLabel(string text)
        {
            // Keep the "Showing results for" the same if it's been translated
            string[] parts = ShowingResultsFor_Label.Text.Split(':');
            string baseText = parts[0].Trim();
            ShowingResultsFor_Label.Text = $"{baseText}: {text}";

            ShowingResultsFor_Label.Left = (ClientSize.Width - ShowingResultsFor_Label.Width) / 2;
            Controls.Add(ShowingResultsFor_Label);
        }
        private void HideShowingResultsForLabel()
        {
            Controls.Remove(ShowingResultsFor_Label);
        }
        private void SetTotalLabel()
        {
            // Keep the "Total" the same if it's been translated
            string[] parts = Total_Label.Text.Split(':');
            string baseText = parts[0].Trim();
            Total_Label.Text = $"{baseText}: {Accountants_DataGridView.Rows.Count}";
            Total_Label.Location = new Point(Accountants_DataGridView.Right - Total_Label.Width, Accountants_DataGridView.Bottom + 10);
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