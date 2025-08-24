using Sales_Tracker.Classes;

namespace Sales_Tracker.AnonymousData
{
    public static class ApiConfig
    {
        public static string? ApiKey => DotEnv.Get("UPLOAD_API_KEY");
        public static string UserAgent => $"ArgoSalesTracker/{Tools.GetVersionNumber()}";
        public static string ServerUrl => "https://argorobots.com/upload_data.php";
    }
}