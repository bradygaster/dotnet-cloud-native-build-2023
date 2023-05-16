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
                        _logger.LogInformation($"Checking inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                        var inStock = await _productsClient.CanInventoryFulfill(cartItem.ProductId, cartItem.Quantity);

                        if (inStock)
                        {
                            _logger.LogInformation($"Inventory OK for product id {cartItem.ProductId} in order {order.OrderId}");
                        }
                        else
                        {
                            _logger.LogInformation($"Not enough inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                            canWeFulfillOrder = canWeFulfillOrder && inStock;
                        }
                    }

                    if(canWeFulfillOrder)
                    {
                        foreach (var cartItem in order.Cart)
                        {
                            _logger.LogInformation($"Removing {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");
                            
                            await _productsClient.SubtractInventory(cartItem.ProductId, cartItem.Quantity);
                            
                            _logger.LogInformation($"Removed {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");
                        }

                        _logger.LogInformation($"Marking order {order.OrderId} as ready for shipment");
                        
                        await _ordersClient.MarkOrderReadyForShipment(order);
                        
                        _logger.LogInformation($"Marked order {order.OrderId} as ready for shipment");
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}