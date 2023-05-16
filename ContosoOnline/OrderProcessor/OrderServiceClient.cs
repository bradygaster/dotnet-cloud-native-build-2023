namespace OrderProcessor
{
    public class OrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;
        private const string ORDERS_URL = "http://orders:8080/orders";

        public OrderServiceClient(IHttpClientFactory httpClientFactory, ILogger<OrderServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Orders");
            _logger = logger;
        }
        
        public async Task<List<Order>?> GetOrders()
        {
            _logger.LogInformation($"Getting orders from {ORDERS_URL}");
            
            var orders = await _httpClient.GetFromJsonAsync<List<Order>>(ORDERS_URL);

            _logger.LogInformation($"Got {orders?.Count} orders from {ORDERS_URL}");

            return orders;
        }

        public async Task MarkOrderReadyForShipment(Order order)
        {
            _logger.LogInformation($"Marking order {order.OrderId} as ready for shipment");
            
            await _httpClient.PutAsJsonAsync($"{ORDERS_URL}/{order.OrderId}", order);
            
            _logger.LogInformation($"Marked order {order.OrderId} as ready for shipment");
        }
    }

    public record CartItem(string ProductId, int Quantity = 1);

    public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
}
