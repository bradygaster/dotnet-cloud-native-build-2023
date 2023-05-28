namespace OrderProcessor;

public class OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
{
    public async Task<IEnumerable<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting orders from {Url}", httpClient.BaseAddress);

        var orders = await httpClient.GetFromJsonAsync<IEnumerable<Order>>("/orders", cancellationToken);

        logger.LogInformation("Got {Count} orders from {Url}", orders?.Count() ?? 0, httpClient.BaseAddress);

        return orders ?? Enumerable.Empty<Order>();
    }

    public async Task MarkOrderReadyForShipment(Order order)
    {
        logger.LogInformation("Marking order {OrderId} as ready for shipment", order.OrderId);
        
        await httpClient.PutAsJsonAsync($"/orders/{order.OrderId}", order);
        
        logger.LogInformation("Marked order {OrderId} as ready for shipment", order.OrderId);
    }
}

public record CartItem(string ProductId, int Quantity = 1);

public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
