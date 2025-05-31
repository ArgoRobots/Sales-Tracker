namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents a product with details such as ID, name, country of origin, and company of origin.
    /// </summary>
    public class Product
    {
        // Getters and setters
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