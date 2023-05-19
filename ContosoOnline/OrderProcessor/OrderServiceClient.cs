namespace OrderProcessor
{
    public class OrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;

        public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>?> GetOrders()
        {
            _logger.LogInformation("Getting orders from {Url}", _httpClient.BaseAddress);

            var orders = await _httpClient.GetFromJsonAsync<IEnumerable<Order>>("/orders");

            _logger.LogInformation("Got {Count} orders from {Url}", orders?.Count() ?? 0, _httpClient.BaseAddress);

            return orders;
        }

        public async Task MarkOrderReadyForShipment(Order order)
        {
            _logger.LogInformation("Marking order {OrderId} as ready for shipment", order.OrderId);
            
            await _httpClient.PutAsJsonAsync($"/orders/{order.OrderId}", order);
            
            _logger.LogInformation("Marked order {OrderId} as ready for shipment", order.OrderId);
        }
    }

    public record CartItem(string ProductId, int Quantity = 1);

    public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
}
