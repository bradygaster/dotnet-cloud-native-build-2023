namespace OrderProcessor
{
    public class ProductServiceClient
    {
        private readonly Products.Products.ProductsClient _client;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(ILogger<ProductServiceClient> logger, Products.Products.ProductsClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<bool> CanInventoryFulfill(string productId, int amount)
        {
            _logger.LogInformation($"Checking inventory for {productId}");

            var result = await _client.CheckProductInventoryAsync(new Products.CheckProductInventoryRequest
            {
                ItemsRequested = amount,
                ProductId = productId
            });

            _logger.LogInformation($"Inventory check for {productId} complete. Available: {result.IsEnoughAvailable}");

            return result.IsEnoughAvailable;
        }

        public async Task SubtractInventory(string productId, int amount)
        {
            _logger.LogInformation($"Updating inventory for {productId}");

            var result = await _client.SubtractInventoryAsync(new Products.InventorySubtractionRequest
            {
                ItemsRequested = amount,
                ProductId = productId
            });

            _logger.LogInformation($"Inventory update for {productId} complete.");
        }
    }
}
