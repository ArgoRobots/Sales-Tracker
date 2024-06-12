namespace Sales_Tracker
{
    public class Category(string name)
    {
        public string CategoryName { get; private set; } = name;
        public List<Product> ProductList { get; private set; } = [];

        public void AddProduct(Product product)
        {
            ProductList.Add(product);
        }
    }
}