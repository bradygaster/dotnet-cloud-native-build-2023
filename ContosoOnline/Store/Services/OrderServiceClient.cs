﻿namespace Store;

public class OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
{
    public async Task<IEnumerable<Order>?> GetOrders()
    {
        logger.LogInformation("Getting orders from {Url}", httpClient.BaseAddress);
        
        var orders = await httpClient.GetFromJsonAsync<IEnumerable<Order>>("/orders");

        logger.LogInformation("Got {Count} orders from {Url}", orders?.Count() ?? 0, httpClient.BaseAddress);

        return orders;
    }

    public async Task<bool> SubmitNewOrder(CartItem[] cart)
    {
        var orderId = Guid.NewGuid();

        logger.LogInformation("Submitting new order {OrderId} to {Url}", orderId, httpClient.BaseAddress);

        var order = new Order(cart, DateTime.UtcNow, orderId);

        var response = await httpClient.PostAsJsonAsync("/orders", order);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Successfully submitted order {orderId} to {Url}", order, httpClient.BaseAddress);
            return true;
        }

        logger.LogError("Failed to submit order {OrderId} to {Url}", orderId, httpClient.BaseAddress);
        return false;
    }
}

public record CartItem(string ProductId)
{
    public int Quantity { get; set; }
}

public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
