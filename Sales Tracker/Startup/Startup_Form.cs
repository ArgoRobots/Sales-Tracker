using Sales_Tracker.Classes;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.UI;

namespace Sales_Tracker.Startup
{
    public partial class Startup_Form : Form
    {
        // Properties
        private static Startup_Form _instance;
        public readonly GetStarted_Form formGetStarted = new();
        public readonly ConfigureProject_Form FormConfigureProject = new();

        // Getters
        public static Startup_Form Instance => _instance;

        // Init.
        public Startup_Form(string[] args)
        {
            InitializeComponent();
            _instance = this;

            if (args.Contains("autoClickButton"))
            {
                SwitchMainForm(FormConfigureProject);
                ConfigureProject_Form.Instance.Controls.Remove(ConfigureProject_Form.Instance.Back_Button);
            }
            else { SwitchMainForm(formGetStarted); }

            BackColor = CustomColors.MainBackground;
            Theme.UseImmersiveDarkMode(Handle, Theme.CurrentTheme == Theme.ThemeType.Dark);
            FormThemeManager.RegisterForm(this);

            TextBoxManager.ConstructRightClickTextBoxMenu();

            // TO DO: make this only run once, during progam installation
            ArgoFiles.RegisterFileIcon(ArgoFiles.ArgoCompanyFileExtension, Properties.Resources.ArgoColor, 0);
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