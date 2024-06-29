namespace Sales_Tracker
{
    public class Product
    {
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string CountryOfOrigin { get; set; }

        // Default constructor required for deserialization
        public Product() { }

        public Product(string productID, string productName, string countryOfOrigin)
        {
            ProductID = productID;
            Name = productName;
            CountryOfOrigin = countryOfOrigin;
        }
    }
}