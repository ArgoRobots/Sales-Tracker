using Sales_Tracker.Classes;
using Sales_Tracker.Startup.Menus;
using static Sales_Tracker.Classes.Theme;

namespace Sales_Tracker.Startup
{
    public partial class Startup_Form : BaseForm
    {
        // Init
        public static Startup_Form Instance { get; private set; }
        public Startup_Form(string[] args)
        {
            InitializeComponent();
            Instance = this;

            Theme.UseImmersiveDarkMode(Handle, true);

            if (args.Contains("autoClickButton"))
            {
                SwitchMainForm(FormConfigureProject);
                ConfigureProject_Form.Instance.Controls.Remove(ConfigureProject_Form.Instance.Back_Button);
            }
            else { SwitchMainForm(formGetStarted); }

            Theme.UseImmersiveDarkMode(Handle, CurrentTheme == ThemeType.Dark);

            // TO DO: make this only run once, during progam installation
            ArgoFiles.RegisterFileIcon(ArgoFiles.ArgoCompanyFileExtension, "A_logo", 0);
        }

        public readonly Form formGetStarted = new GetStarted_Form();
        public readonly Form FormConfigureProject = new ConfigureProject_Form();

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