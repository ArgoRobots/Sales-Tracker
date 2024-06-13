namespace Sales_Tracker
{
    public class Product(string productName, string sellerName, string countryOfOrigin)
    {
        public string Name = productName;
        public string SellerName = sellerName;
        public string CountryOfOrigin = countryOfOrigin;
    }
}