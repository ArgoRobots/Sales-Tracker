namespace Sales_Tracker
{
    public class Category(string name)
    {
        public string Name { get; set; } = name;
        public List<Product> ProductList { get; private set; } = [];

        public void AddProduct(Product product)
        {
            ProductList.Add(product);
        }
    }
}