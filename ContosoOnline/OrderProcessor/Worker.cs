using Grpc.Net.Client;

namespace OrderProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OrderServiceClient _ordersClient;
        private readonly ProductServiceClient _productsClient;

        public Worker(ILogger<Worker> logger,
            OrderServiceClient ordersClient,
            ProductServiceClient productsClient)
        {
            _logger = logger;
            _ordersClient = ordersClient;
            _productsClient = productsClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                var orders = await _ordersClient.GetOrders();

                foreach (Order? order in orders)
                {
                    bool canWeFulfillOrder = true;

                    _logger.LogInformation($"Checking inventory for Order {order.OrderId}");

                    foreach (var cartItem in order.Cart)
                    {
                        if (canWeFulfillOrder)
                        {
                            _logger.LogInformation($"Checking inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                            var inStock = await _productsClient.CanInventoryFulfill(cartItem.ProductId, cartItem.Quantity);

                            if (inStock)
                            {
                                _logger.LogInformation($"Inventory OK for product id {cartItem.ProductId} in order {order.OrderId}");
                            }
                            else
                            {
                                _logger.LogInformation($"Not enough inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                                canWeFulfillOrder = false;
                            }
                        }
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}