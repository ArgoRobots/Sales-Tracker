namespace Sales_Tracker
{
    public class Category
    {
        // Properties
        private string _name;
        private List<Product> _productList = [];

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public List<Product> ProductList
        {
            get { return _productList; }
            set { _productList = value; }
        }

        // Default constructor required for deserialization
        public Category()
        { }

        public Category(string name)
        {
            _name = name;
        }

        public void AddProduct(Product product)
        {
            ProductList.Add(product);
        }
    }
}