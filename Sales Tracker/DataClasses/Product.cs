namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents a product with details such as ID, name, country of origin, and company of origin.
    /// </summary>
    public class Product
    {
        // Properties
        private string _productID;
        private string _name;
        private string _countryOfOrigin;
        private string _companyOfOrigin;

        // Getters and setters
        public string ProductID
        {
            get => _productID;
            set => _productID = value;
        }
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public string CountryOfOrigin
        {
            get => _countryOfOrigin;
            set => _countryOfOrigin = value;
        }
        public string CompanyOfOrigin
        {
            get => _companyOfOrigin;
            set => _companyOfOrigin = value;
        }

        // Default constructor required for deserialization
        public Product()
        { }

        public Product(string productID, string productName, string countryOfOrigin, string companyOfOrigin)
        {
            _productID = productID;
            _name = productName;
            _countryOfOrigin = countryOfOrigin;
            _companyOfOrigin = companyOfOrigin;
        }
    }
}