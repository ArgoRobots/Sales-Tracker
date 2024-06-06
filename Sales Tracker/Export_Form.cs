using Sales_Tracker.Classes;

namespace Sales_Tracker
{
    public partial class Export_Form : Form
    {
        public Export_Form()
        {
            InitializeComponent();

            // Set theme
            string theme = Theme.SetThemeForForm(this);
            if (theme == "Light")
            {

            }
            else if (theme == "Dark")
            {

            }
        }
        private void Export_Form_Load(object sender, EventArgs e)
        {
            if (Sales_Tracker.Properties.Settings.Default.ExportDirectory == "")
            {
                Sales_Tracker.Properties.Settings.Default.ExportDirectory = Directories.desktop_dir;
                Sales_Tracker.Properties.Settings.Default.Save();
            }

            Directory_TextBox.Text = Sales_Tracker.Properties.Settings.Default.ExportDirectory;

            FileType_ComboBox.SelectedIndex = 0; ;

            Name_TextBox.Text = Directories.projectName + " " + Tools.FormatDate(DateTime.Today);
        }
        private void Export_Form_Shown(object sender, EventArgs e)
        {
            // This fixes a bug
            BeginInvoke(() => Export_Button.Focus());
        }

        private void Name_TextBox_TextChanged(object sender, EventArgs e)
        {
            // TEMP NOTE: the backround color of WarningDir_pictureBox is always white even in dark mode because the background is not transparent. It needs to be a .png

            if ("/#%&*|;".Any(Name_TextBox.Text.Contains) || Name_TextBox.Text == "")
            {
                Export_Button.Enabled = false;
                Name_TextBox.BorderColor = Color.Red;
                Name_TextBox.FocusedState.BorderColor = Color.Red;
                WarningName_Label.Visible = true;
                WarningName_PictureBox.Visible = true;
            }
            else
            {
                Export_Button.Enabled = true;
                Name_TextBox.BorderColor = CustomColors.controlBorder;
                Name_TextBox.FocusedState.BorderColor = CustomColors.controlBorder;
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
            switch (FileType_ComboBox.SelectedItem)
            {
                case ".ArgoProject (backup)":
                    Directories.CreateBackup(Directory_TextBox.Text + "\\" + Name_TextBox.Text, ".ArgoProject");
                    CustomMessageBox.Show("Argo Studio", $"Successfully backed up '{Directories.projectName}'", CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    Close();
                    break;

                case ".stl":
                    break;

                case ".obj":
                    break;
            }
        }
    }
}