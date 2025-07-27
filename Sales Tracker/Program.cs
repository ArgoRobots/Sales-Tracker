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
            ApplicationStartup.InitializeApplication();

            // Handle .ArgoSales file association - opens file if launched via double-click
            if (ApplicationStartup.TryOpenCompanyFromCommandLine(args))
            {
                Application.Run();
                return;
            }

            // Check if we should auto-open the most recent company after an update
            if (ApplicationStartup.TryAutoOpenRecentCompanyAfterUpdate())
            {
                Application.Run();
                return;
            }

            // Otherwise, normal application startup
            Startup_Form startupForm = new(args);
            startupForm.Show();
            Application.Run();
        }
    }
}