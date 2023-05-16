namespace Products
{
    public interface IProductService
    {
        public Task<Product[]?> GetProductsAsync();
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
                new Product { Name = "Apple", Price = 0.99f, ItemsInStock = 25, ProductId = "01" },
                new Product { Name = "Banana", Price = 0.99f, ItemsInStock = 25, ProductId = "02" },
                new Product { Name = "Orange", Price = 0.99f, ItemsInStock = 25, ProductId = "03" },
                new Product { Name = "Pear", Price = 0.99f, ItemsInStock = 25, ProductId = "04" },
                new Product { Name = "Pineapple", Price = 0.99f, ItemsInStock = 25, ProductId = "05" }
            });
        }

        public Task<Product[]?> GetProductsAsync()
        {
            var productsInStock = _fakeOrders?.Where(p => p.ItemsInStock > 0).ToArray();
            return Task.FromResult(productsInStock);
        }

        public Task<bool> CheckProductInventoryAsync(string productId, int quantityNeeded)
        {
            _logger.LogInformation($"Checking inventory for {productId} with quantity {quantityNeeded}");
            bool isEnoughInStock = _fakeOrders?.FirstOrDefault(p => p.ProductId == productId)?.ItemsInStock >= quantityNeeded;
            _logger.LogInformation($"Are enough items in stock for Product {productId}: {isEnoughInStock}");
            return Task.FromResult(isEnoughInStock);
        }

        public Task<bool> SubtractInventory(string productId, int quantityNeeded)
        {
            _logger.LogInformation($"Subtracting {quantityNeeded} from {productId} inventory"); 
            var product = _fakeOrders?.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                _logger.LogInformation($"Found product {product.Name} with {product.ItemsInStock} in stock");
                product.ItemsInStock -= quantityNeeded;
                _fakeOrders?.RemoveAll(p => p.ProductId == productId);
                _fakeOrders?.Add(product);
                _logger.LogInformation($"Subtracted {quantityNeeded} from {product.Name} inventory. New inventory is {product.ItemsInStock}");
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
