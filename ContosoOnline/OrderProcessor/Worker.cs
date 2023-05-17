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
                using (var activity = _activitySource.StartActivity("order-processor.worker"))
                {
                    _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                    var orders = await _ordersClient.GetOrders();
                    activity?.AddTag("order-count", orders?.Count ?? 0);

                    var orderTasks = new List<Task>();
                    foreach (Order? order in orders)
                    {
                        orderTasks.Add(ProcessOrder(order));
                    }
                    await Task.WhenAll(orderTasks);
                }
                await Task.Delay(15000, stoppingToken);
            }
        }

        private async Task ProcessOrder(Order order)
        {
            using (var orderActivity = _activitySource.StartActivity("order-processor.process-order"))
            {
                _logger.LogInformation($"Checking inventory for Order {order.OrderId}");

                orderActivity?.AddTag("order-id", order.OrderId);
                orderActivity?.AddTag("product-count", order.Cart.Length);

                var itemTasks = new List<Task<bool>>();
                foreach (var cartItem in order.Cart)
                {
                    itemTasks.Add(validateInventory(cartItem, order));
                }
                await Task.WhenAll(itemTasks);

                bool canWeFulfillOrder = true;
                itemTasks.ForEach(itemTask => canWeFulfillOrder = canWeFulfillOrder && itemTask.Result);

                orderActivity?.SetTag("can-fulfill-order", canWeFulfillOrder);

                if (canWeFulfillOrder)
                {
                    var invTasks = new List<Task>();
                    foreach (var cartItem in order.Cart)
                    {
                        invTasks.Add(updateInventory(cartItem));
                    }

                    _logger.LogInformation($"Marking order {order.OrderId} as ready for shipment");
                    invTasks.Add(_ordersClient.MarkOrderReadyForShipment(order));
                    await Task.WhenAll(invTasks);
                    _logger.LogInformation($"Marked order {order.OrderId} as ready for shipment");
                }
            }
        }

        private async Task<bool> validateInventory(CartItem cartItem, Order order)
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
            }
            return inStock;
        }

        private async Task updateInventory(CartItem cartItem)
        {
            _logger.LogInformation($"Removing {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");

            await _productsClient.SubtractInventory(cartItem.ProductId, cartItem.Quantity);

            _logger.LogInformation($"Removed {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");
        }
    }
}