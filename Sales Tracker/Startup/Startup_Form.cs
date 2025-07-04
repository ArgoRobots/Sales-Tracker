using Sales_Tracker.Classes;
using Sales_Tracker.Startup.Menus;
using Sales_Tracker.Theme;

namespace Sales_Tracker.Startup
{
    public partial class Startup_Form : BaseForm
    {
        // Properties
        private static Startup_Form _instance;
        public static bool CanExitApp { get; set; } = true;

        // Getters
        public static Startup_Form Instance => _instance;
        public GetStarted_Form FormGetStarted { get; } = new();
        public ConfigureCompany_Form FormConfigureCompany { get; } = new();

        // Init.
        public Startup_Form() : this([]) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public Startup_Form(string[] args)
        {
            InitializeComponent();
            _instance = this;

            if (args.Contains("autoClickButton"))
            {
                SwitchMainForm(FormConfigureCompany);
                FormConfigureCompany.Controls.Remove(FormConfigureCompany.Back_Button);
            }
            else { SwitchMainForm(FormGetStarted); }

            BackColor = CustomColors.MainBackground;
            ThemeManager.UseImmersiveDarkMode(Handle, ThemeManager.IsDarkTheme());
            FormThemeManager.RegisterForm(this);
        }
        public void SwitchMainForm(Form form)
        {
            Controls.Clear();
            form.Dock = DockStyle.Fill;
            form.TopLevel = false;
            Controls.Add(form);
            form.Show();
        }
        private void Startup_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (CanExitApp)
            {
                Application.Exit();
            }
        }
    }
}