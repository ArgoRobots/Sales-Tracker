using Sales_Tracker.Startup.Menus;
using Sales_Tracker.Theme;

namespace Sales_Tracker.Startup
{
    public partial class Startup_Form : Form
    {
        // Properties
        private static Startup_Form _instance;
        private readonly GetStarted_Form _formGetStarted = new();
        private readonly ConfigureProject_Form _formConfigureProject = new();

        // Getters
        public static Startup_Form Instance => _instance;
        public GetStarted_Form FormGetStarted => _formGetStarted;
        public ConfigureProject_Form FormConfigureProject => _formConfigureProject;

        // Init.
        public Startup_Form() : this([]) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public Startup_Form(string[] args)
        {
            InitializeComponent();
            _instance = this;

            if (args.Contains("autoClickButton"))
            {
                SwitchMainForm(_formConfigureProject);
                _formConfigureProject.Controls.Remove(_formConfigureProject.Back_Button);
            }
            else { SwitchMainForm(_formGetStarted); }

            BackColor = CustomColors.MainBackground;
            ThemeManager.UseImmersiveDarkMode(Handle, ThemeManager.IsDarkTheme());
            FormThemeManager.RegisterForm(this);
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