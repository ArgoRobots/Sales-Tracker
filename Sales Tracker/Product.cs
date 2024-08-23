namespace Sales_Tracker
{
    public class Product
    {
        // Properties
        private string _productID;
        private string _name;
        private string _countryOfOrigin;
        private string _companyOfOrigin;

        public string ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string CountryOfOrigin
        {
            get { return _countryOfOrigin; }
            set { _countryOfOrigin = value; }
        }
        public string CompanyOfOrigin
        {
            get { return _companyOfOrigin; }
            set { _companyOfOrigin = value; }
        }

        // Default constructor required for deserialization
        public Product()
        { }

        public Product(string productID, string productName, string countryOfOrigin, string companyOfOrigin)
        {
            ProductID = productID;
            Name = productName;
            CountryOfOrigin = countryOfOrigin;
            CompanyOfOrigin = companyOfOrigin;
        }
    }
}