using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Xml.Linq;

namespace Sales_Tracker
{
    public partial class Accountants_Form : Form
    {
        // Properties
        public readonly static List<string> thingsThatHaveChangedInFile = [];

        // Init.
        public static Accountants_Form Instance { get; private set; }
        private readonly MainMenu_Form.Options oldOption;
        private readonly Guna2DataGridView oldSelectedDataGridView;
        public Accountants_Form()
        {
            InitializeComponent();
            Instance = this;

            oldOption = MainMenu_Form.Instance.Selected;
            oldSelectedDataGridView = MainMenu_Form.Instance.selectedDataGridView;
            ConstructDataGridViews();
            LoadAccountants();
            Theme.SetThemeForForm(this);
        }
        private void LoadAccountants()
        {
            MainMenu_Form.Instance.isDataGridViewLoading = true;

            foreach (string accountant in MainMenu_Form.Instance.accountantList)
            {
                Accountants_DataGridView.Rows.Add(accountant);
            }
            MainMenu_Form.Instance.isDataGridViewLoading = false;
        }

        // Form event handlers
        private void Accountants_Form_Resize(object sender, EventArgs e)
        {
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
            string name = Accountant_TextBox.Text.Trim();
            MainMenu_Form.Instance.accountantList.Add(name);
            Accountants_DataGridView.Rows.Add(name);

            thingsThatHaveChangedInFile.Add(name);
            Log.Write(3, $"Added category '{name}'");

            Accountant_TextBox.Text = "";
            ValidateInputs();
        }
        private void Accountant_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise when user presses enter
                AddAccountant_Button.PerformClick();
            }
        }
        private void Accountant_TextBox_TextChanged(object sender, EventArgs e)
        {
            VaidateAccountantTextBox();
            ValidateInputs();
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
        private Guna2DataGridView Accountants_DataGridView;
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
            Accountants_DataGridView.Tag = MainMenu_Form.DataGridViewTags.Accountant;

            Controls.Add(Accountants_DataGridView);
            MainMenu_Form.Instance.selectedDataGridView = Accountants_DataGridView;
            MainMenu_Form.Instance.Selected = MainMenu_Form.Options.Accountants;
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
        }
        private void HideAccountantWarning()
        {
            WarningAccountantName_PictureBox.Visible = false;
            WarningAccountantName_Label.Visible = false;
        }

        // Methods
        private void ValidateInputs()
        {
            AddAccountant_Button.Enabled = !string.IsNullOrWhiteSpace(Accountant_TextBox.Text);
        }
    }
}