namespace Store
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

        public async Task<bool> SubmitNewOrder(CartItem[] cart)
        {
            var orderId = Guid.NewGuid();

            _logger.LogInformation("Submitting new order {OrderId} to {Url}", orderId, _httpClient.BaseAddress);

            var order = new Order(cart, DateTime.UtcNow, orderId);

            var response = await _httpClient.PostAsJsonAsync("/orders", order);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully submitted order {orderId} to {Url}", order, _httpClient.BaseAddress);
                return true;
            }

            _logger.LogError("Failed to submit order {OrderId} to {Url}", orderId, _httpClient.BaseAddress);
            return false;
        }
    }

    public record CartItem(string ProductId)
    {
        public int Quantity { get; set; }
    }

    public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
}
