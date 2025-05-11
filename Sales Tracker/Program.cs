using Sales_Tracker.Startup;

namespace Sales_Tracker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            Startup_Form startupForm = new(args);
            startupForm.Show();
            Application.Run();
        }
    }
}