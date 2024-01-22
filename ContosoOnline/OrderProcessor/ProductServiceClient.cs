public class ProductServiceClient(ILogger<ProductServiceClient> logger, Products.Products.ProductsClient client)
{
    public async Task<bool> CanInventoryFulfill(string productId, int amount)
    {
        logger.LogInformation("Checking inventory for {ProductId}", productId);

        var result = await client.CheckProductInventoryAsync(new Products.CheckProductInventoryRequest
        {
            ItemsRequested = amount,
            ProductId = productId
        });

        logger.LogInformation("Inventory check for {ProductId} complete. Available: {IsEnoughAvailable}", productId, result.IsEnoughAvailable);

        return result.IsEnoughAvailable;
    }

    public async Task SubtractInventory(string productId, int amount)
    {
        logger.LogInformation("Updating inventory for {ProductId}", productId);

        await client.SubtractInventoryAsync(new Products.InventorySubtractionRequest
        {
            ItemsRequested = amount,
            ProductId = productId
        });

        logger.LogInformation("Inventory update for {ProductId} complete.", productId);
    }
}
