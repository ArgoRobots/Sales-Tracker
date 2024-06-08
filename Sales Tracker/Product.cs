namespace Sales_Tracker
{
    public class Product(string productName, string sellerName, string countryOfOrigin)
    {
        public string ProductName = productName;
        public string SellerName = sellerName;
        public string CountryOfOrigin = countryOfOrigin;
    }
}