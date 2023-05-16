using Grpc.Net.Client;
using System.Diagnostics;

namespace OrderProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OrderServiceClient _ordersClient;
        private readonly ProductServiceClient _productsClient;
        static ActivitySource _activitySource = new ActivitySource(nameof(Worker));

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
                using var activity = _activitySource.StartActivity("ProcessOrders");
                
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                activity?.AddEvent(new ActivityEvent("Calling Orders API"));
                var orders = await _ordersClient.GetOrders();
                activity?.AddEvent(new ActivityEvent("Obtained open orders from Orders API"));

                foreach (Order? order in orders)
                {
                    bool canWeFulfillOrder = true;

                    _logger.LogInformation($"Checking inventory for Order {order.OrderId}");

                    foreach (var cartItem in order.Cart)
                    {

                        activity?.AddEvent(new ActivityEvent("Calling Products API to check inventory"));

                        _logger.LogInformation($"Checking inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                        var inStock = await _productsClient.CanInventoryFulfill(cartItem.ProductId, cartItem.Quantity);

                        activity?.AddEvent(new ActivityEvent("Called Products API to check inventory"));

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

                            activity?.AddEvent(new ActivityEvent("Calling Products API to subtract inventory"));

                            await _productsClient.SubtractInventory(cartItem.ProductId, cartItem.Quantity);

                            activity?.AddEvent(new ActivityEvent("Called Products API to subtract inventory"));

                            _logger.LogInformation($"Removed {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");
                        }

                        _logger.LogInformation($"Marking order {order.OrderId} as ready for shipment");

                        activity?.AddEvent(new ActivityEvent("Calling Orders API to mark order ready for shipment"));

                        await _ordersClient.MarkOrderReadyForShipment(order);

                        activity?.AddEvent(new ActivityEvent("Called Orders API to mark order ready for shipment"));

                        _logger.LogInformation($"Marked order {order.OrderId} as ready for shipment");
                    }
                }

                activity?.Stop();

                await Task.Delay(15000, stoppingToken);
            }
        }
    }
}