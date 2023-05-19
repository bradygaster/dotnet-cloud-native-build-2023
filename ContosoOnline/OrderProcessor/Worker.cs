using System.Diagnostics;

namespace OrderProcessor;

public class Worker(ILogger<Worker> logger, OrderServiceClient ordersClient, ProductServiceClient productsClient) 
    : BackgroundService
{
    static ActivitySource _activitySource = new ActivitySource(nameof(Worker));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var activity = _activitySource.StartActivity("order-processor.worker"))
            {
                logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                var orders = await ordersClient.GetOrders();
                activity?.AddTag("order-count", orders?.Count() ?? 0);

                var orderTasks = new List<Task>();
                foreach (Order? order in orders)
                {
                    orderTasks.Add(Task.Run(async () =>
                    {
                        logger.LogInformation($"Checking inventory for Order {order.OrderId}");

                        using (var orderActivity = _activitySource.StartActivity("order-processor.process-order"))
                        {
                            orderActivity?.AddTag("order-id", order.OrderId);
                            orderActivity?.AddTag("product-count", order.Cart.Length);

                            bool canWeFulfillOrder = true;
                            var itemTasks = new List<Task>();

                            foreach (var cartItem in order.Cart)
                            {
                                itemTasks.Add(Task.Run(async () =>
                                {
                                    logger.LogInformation($"Checking inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                                    var inStock = await productsClient.CanInventoryFulfill(cartItem.ProductId, cartItem.Quantity);

                                    if (inStock)
                                    {
                                        logger.LogInformation($"Inventory OK for product id {cartItem.ProductId} in order {order.OrderId}");
                                    }
                                    else
                                    {
                                        logger.LogInformation($"Not enough inventory for product id {cartItem.ProductId} in order {order.OrderId}");

                                        canWeFulfillOrder = canWeFulfillOrder && inStock;
                                    }
                                }));
                            }
                            await Task.WhenAll(itemTasks);
                            orderActivity?.SetTag("can-fulfill-order", canWeFulfillOrder);

                            if (canWeFulfillOrder)
                            {
                                var invTasks = new List<Task>();
                                foreach (var cartItem in order.Cart)
                                {
                                    invTasks.Add(Task.Run(async () =>
                                    {
                                        logger.LogInformation($"Removing {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");

                                        await productsClient.SubtractInventory(cartItem.ProductId, cartItem.Quantity);

                                        logger.LogInformation($"Removed {cartItem.Quantity} of product id {cartItem.ProductId} from inventory");
                                    }));
                                }

                                logger.LogInformation($"Marking order {order.OrderId} as ready for shipment");

                                invTasks.Add(ordersClient.MarkOrderReadyForShipment(order));

                                logger.LogInformation($"Marked order {order.OrderId} as ready for shipment");

                                await Task.WhenAll(invTasks);
                            }
                        }
                    }));

                    await Task.WhenAll(orderTasks);
                }
            }
            await Task.Delay(15000, stoppingToken);
        }
    }
}