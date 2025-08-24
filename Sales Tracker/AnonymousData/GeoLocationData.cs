namespace Sales_Tracker.AnonymousData
{
    public class GeoLocationData
    {
        public string Country { get; set; } = "Unknown";
        public string CountryCode { get; set; } = "Unknown";
        public string Region { get; set; } = "Unknown";
        public string City { get; set; } = "Unknown";
        public string Timezone { get; set; } = "Unknown";
        public string ISP { get; set; } = "Unknown";
        public bool IsVPN { get; set; } = false;
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
    }
}