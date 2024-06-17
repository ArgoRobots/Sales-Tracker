namespace Sales_Tracker
{
    public class Product(string productName, string productCategory, string countryOfOrigin)
    {
        public string Name = productName;
        public string Category = productCategory;
        public string CountryOfOrigin = countryOfOrigin;
    }
}