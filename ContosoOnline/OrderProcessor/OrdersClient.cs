namespace OrderProcessor
{
    public class OrdersClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdersClient> _logger;
        private const string ORDERS_URL = "http://orders:8080/orders";

        public OrdersClient(IHttpClientFactory httpClientFactory, ILogger<OrdersClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Orders");
            _logger = logger;
        }
        
        public async Task<IEnumerable<Order>> GetOrders()
        {
            _logger.LogInformation($"Getting orders from {ORDERS_URL}");
            
            var orders = await _httpClient.GetFromJsonAsync<IEnumerable<Order>>(ORDERS_URL);

            _logger.LogInformation($"Got {orders.Count()} orders from {ORDERS_URL}");

            return orders;
        }

    }

    public record CartItem(string ProductId, int Quantity = 1);

    public record Order(CartItem[] Cart, DateTime OrderedAt, Guid OrderId);
}
