using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Export_Form : Form
    {
        public Export_Form()
        {
            InitializeComponent();
            LoadingPanel.ShowLoadingPanel(this);
            Theme.SetThemeForForm(this);
        }
        private void Export_Form_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ExportDirectory == "")
            {
                Properties.Settings.Default.ExportDirectory = Directories.Desktop_dir;
                Properties.Settings.Default.Save();
            }

            Directory_TextBox.Text = Properties.Settings.Default.ExportDirectory;

            FileType_ComboBox.SelectedIndex = 0; ;

            Name_TextBox.Text = Directories.CompanyName + " " + Tools.FormatDate(DateTime.Today);
        }
        private void Export_Form_Shown(object sender, EventArgs e)
        {
            // This fixes a bug
            BeginInvoke(() => Export_Button.Focus());

            LoadingPanel.HideLoadingPanel(this);
        }

        private void Name_TextBox_TextChanged(object sender, EventArgs e)
        {
            // TEMP NOTE: the backround color of WarningDir_pictureBox is always white even in dark mode because the background is not transparent. It needs to be a .png

            if ("/#%&*|;".Any(Name_TextBox.Text.Contains) || Name_TextBox.Text == "")
            {
                Export_Button.Enabled = false;
                UI.SetGTextBoxToInvalid(Name_TextBox);
                WarningName_Label.Visible = true;
                WarningName_PictureBox.Visible = true;
            }
            else
            {
                Export_Button.Enabled = true;
                UI.SetGTextBoxToValid(Name_TextBox);
                WarningName_Label.Visible = false;
                WarningName_PictureBox.Visible = false;
            }
        }
        private void Directory_TextBox_TextChanged(object sender, EventArgs e)
        {
            // TEMP NOTE: the backround color of WarningDir_pictureBox is always white even in dark mode because the background is not transparent. It needs to be a .png

            if ("/#%&*|;".Any(Directory_TextBox.Text.Contains) || Directory_TextBox.Text == "" || !Directory_TextBox.Text.Contains('\\'))
            {
                Export_Button.Enabled = false;
                Directory_TextBox.BorderColor = Color.Red;
                Directory_TextBox.FocusedState.BorderColor = Color.Red;
                WarningDir_Label.Visible = true;
                WarningDir_PictureBox.Visible = true;
            }
            else
            {
                Export_Button.Enabled = true;
                Directory_TextBox.BorderColor = CustomColors.controlBorder;
                Directory_TextBox.FocusedState.BorderColor = CustomColors.controlBorder;
                WarningDir_Label.Visible = false;
                WarningDir_PictureBox.Visible = false;
            }
        }

        private void ThreeDots_Button_Click(object sender, EventArgs e)
        {
            // Select folder
            Ookii.Dialogs.WinForms.VistaFolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Directory_TextBox.Text = dialog.SelectedPath + @"\";
            }
        }
        private void Export_Button_Click(object sender, EventArgs e)
        {
            switch (FileType_ComboBox.Text)
            {
                case ".ArgoSales (.zip)":
                    Directories.CreateBackup(Directory_TextBox.Text + "\\" + Name_TextBox.Text, ArgoFiles.ArgoCompanyFileExtension);
                    CustomMessageBox.Show("Argo Sales Tracker", $"Successfully backed up '{Directories.CompanyName}'", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    Close();
                    break;
            }
        }
    }
}