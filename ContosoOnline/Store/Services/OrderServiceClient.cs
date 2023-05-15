namespace Store
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
        
        public async Task<IEnumerable<Order>?> GetOrders()
        {
            _logger.LogInformation($"Getting orders from {ORDERS_URL}");
            
            var orders = await _httpClient.GetFromJsonAsync<IEnumerable<Order>>(ORDERS_URL);

            _logger.LogInformation($"Got {orders?.Count()} orders from {ORDERS_URL}");

            return orders;
        }

        public async Task<bool> SubmitNewOrder(CartItem[] cart)
        {
            var orderId = Guid.NewGuid();
            _logger.LogInformation($"Submitting new order {orderId} to {ORDERS_URL}");

            var order = new Order(cart, DateTime.UtcNow, orderId);

            var response = await _httpClient.PostAsJsonAsync(ORDERS_URL, order);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Successfully submitted order {orderId} to {ORDERS_URL}");
                return true;
            }

            _logger.LogError($"Failed to submit order {orderId} to {ORDERS_URL}");
            return false;
        }
    }

    public record CartItem(string ProductId)
    {
        public int Quantity { get; set; }
    }

    public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
}
