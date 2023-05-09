namespace Products
{
    public interface IProductService
    {
        public Task<Product[]> GetProductsAsync();
        public Task<bool> CheckProductInventoryAsync(string productId, int quantityNeeded);
        public Task<bool> SubtractInventory(string productId, int quantityNeeded);
    }

    public class FakeProductService : IProductService
    {
        private List<Product> _fakeOrders = new();
        private ILogger<FakeProductService> _logger;

        public FakeProductService(ILogger<FakeProductService> logger)
        {
            _logger = logger;
            _fakeOrders.AddRange(new Product[]
            {
                new Product { Name = "Apple", Price = 0.99f, ItemsInStock = 100, ProductId = "01" },
                new Product { Name = "Banana", Price = 0.99f, ItemsInStock = 100, ProductId = "02" },
                new Product { Name = "Orange", Price = 0.99f, ItemsInStock = 100, ProductId = "03" },
                new Product { Name = "Pear", Price = 0.99f, ItemsInStock = 100, ProductId = "04" },
                new Product { Name = "Pineapple", Price = 0.99f, ItemsInStock = 100, ProductId = "05" }
            });
        }

        public Task<Product[]> GetProductsAsync() 
            => Task.FromResult(_fakeOrders.ToArray());

        public Task<bool> CheckProductInventoryAsync(string productId, int quantityNeeded)
            => Task.FromResult(_fakeOrders?.FirstOrDefault(p => p.ProductId == productId)?.ItemsInStock >= quantityNeeded);

        public Task<bool> SubtractInventory(string productId, int quantityNeeded)
        {
            var product = _fakeOrders?.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                product.ItemsInStock -= quantityNeeded;
                _logger.LogInformation($"Subtracted {quantityNeeded} from {product.Name} inventory. New inventory is {product.ItemsInStock}");
                _fakeOrders.RemoveAll(p => p.ProductId == productId);
                _fakeOrders.Add(product);
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogError($"Unable to find product with id {productId}");
                return Task.FromResult(false);
            }
        }
    }
}
