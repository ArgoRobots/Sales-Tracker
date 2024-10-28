using Sales_Tracker.Classes;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Startup
{
    public partial class Startup_Form : Form
    {
        // Properties
        private static Startup_Form _instance;
        public readonly Form formGetStarted = new GetStarted_Form();
        public readonly Form FormConfigureProject = new ConfigureProject_Form();

        // Getters
        public static Startup_Form Instance => _instance;

        // Init.
        public Startup_Form(string[] args)
        {
            InitializeComponent();
            _instance = this;

            BackColor = CustomColors.mainBackground;

            if (args.Contains("autoClickButton"))
            {
                SwitchMainForm(FormConfigureProject);
                ConfigureProject_Form.Instance.Controls.Remove(ConfigureProject_Form.Instance.Back_Button);
            }
            else { SwitchMainForm(formGetStarted); }

            Theme.UseImmersiveDarkMode(Handle, Theme.CurrentTheme == Theme.ThemeType.Dark);

            // TO DO: make this only run once, during progam installation
            ArgoFiles.RegisterFileIcon(ArgoFiles.ArgoCompanyFileExtension, Properties.Resources.ArgoColor, 0);

            LanguageManager.UpdateLanguageForControl(this);
        }

        public void SwitchMainForm(Form mainForm)
        {
            Controls.Clear();
            mainForm.Dock = DockStyle.Fill;
            mainForm.TopLevel = false;
            Controls.Add(mainForm);
            mainForm.Show();
        }
    }
}