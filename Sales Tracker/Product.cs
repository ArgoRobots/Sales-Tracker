namespace Sales_Tracker
{
    public class Product
    {
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string CountryOfOrigin { get; set; }
        public string CompanyOfOrigin { get; set; }

        // Default constructor required for deserialization
        public Product() { }

        public Product(string productID, string productName, string countryOfOrigin, string companyOfOrigin)
        {
            ProductID = productID;
            Name = productName;
            CountryOfOrigin = countryOfOrigin;
            CompanyOfOrigin = companyOfOrigin;
        }
    }
}