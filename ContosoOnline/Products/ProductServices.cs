namespace Products;

public interface IProductService
{
    public Task<Product[]?> GetProductsAsync();
    public Task<bool> CheckProductInventoryAsync(string productId, int quantityNeeded);
    public Task<bool> SubtractInventory(string productId, int quantityNeeded);
}

public class FakeProductService(ILogger<FakeProductService> logger) : IProductService
{
    private List<Product> _fakeOrders = new()
    {
        new () { Name = "Apple", Price = 0.99f, ItemsInStock = 25, ProductId = "01" },
        new () { Name = "Banana", Price = 0.99f, ItemsInStock = 25, ProductId = "02" },
        new () { Name = "Orange", Price = 0.99f, ItemsInStock = 25, ProductId = "03" },
        new () { Name = "Pear", Price = 0.99f, ItemsInStock = 25, ProductId = "04" },
        new () { Name = "Pineapple", Price = 0.99f, ItemsInStock = 25, ProductId = "05" }
    };

    public Task<Product[]?> GetProductsAsync()
    {
        var productsInStock = _fakeOrders?.Where(p => p.ItemsInStock > 0).ToArray();
        return Task.FromResult(productsInStock);
    }

    public Task<bool> CheckProductInventoryAsync(string productId, int quantityNeeded)
    {
        logger.LogInformation("Checking inventory for {ProductId} with quantity {QuantityNeeded}", productId, quantityNeeded);

        bool isEnoughInStock = _fakeOrders?.FirstOrDefault(p => p.ProductId == productId)?.ItemsInStock >= quantityNeeded;

        logger.LogInformation("Are enough items in stock for Product {ProductId}: {IsEnoughInStock}", productId, isEnoughInStock);

        return Task.FromResult(isEnoughInStock);
    }

    public Task<bool> SubtractInventory(string productId, int quantityNeeded)
    {
        logger.LogInformation("Subtracting {QuantityNeeded} from {ProductId} inventory", quantityNeeded, productId);

        var product = _fakeOrders.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            logger.LogInformation("Found product {Name} with {ItemsInStock} in stock", product.Name, product.ItemsInStock);

            product.ItemsInStock -= quantityNeeded;
            _fakeOrders.RemoveAll(p => p.ProductId == productId);
            _fakeOrders.Add(product);

            logger.LogInformation("Subtracted {QuantityNeeded} from {Name} inventory. New inventory is {ItemsInStock}", quantityNeeded, product.Name, product.ItemsInStock);

            return Task.FromResult(true);
        }
        else
        {
            logger.LogError("Unable to find product with id {ProductId}", productId);

            return Task.FromResult(false);
        }
    }
}
