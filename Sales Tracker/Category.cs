namespace Sales_Tracker
{
    public class Category
    {
        public string Name { get; set; }
        public List<Product> ProductList { get; set; } = [];

        // Default constructor required for deserialization
        public Category() { }

        public Category(string name)
        {
            Name = name;
        }

        public void AddProduct(Product product)
        {
            ProductList.Add(product);
        }
    }
}