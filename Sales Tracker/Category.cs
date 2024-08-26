namespace Sales_Tracker
{
    public class Category
    {
        // Properties
        private string _name;
        private List<Product> _productList = [];

        // Getters and setters
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public List<Product> ProductList
        {
            get => _productList;
            set => _productList = value;
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