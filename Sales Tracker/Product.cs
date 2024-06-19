namespace Sales_Tracker
{
    public class Product(string productID, string productName, string countryOfOrigin)
    {
        public string ProductID = productID,
                      Name = productName,
                      CountryOfOrigin = countryOfOrigin;
    }
}