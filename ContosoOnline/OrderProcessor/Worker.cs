using System.Diagnostics;

namespace OrderProcessor;

public class Worker(ILogger<Worker> logger, OrderServiceClient ordersClient, ProductServiceClient productsClient)
    : BackgroundService
{
    static ActivitySource _activitySource = new ActivitySource(nameof(Worker));

    private TimeSpan CheckOrderInterval => TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var activity = _activitySource.StartActivity("order-processor.worker"))
            {
                logger.LogInformation($"Worker running at: {DateTime.UtcNow}");

                var orders = await ordersClient.GetOrders() ?? Enumerable.Empty<Order>();
                activity?.AddTag("order-count", orders.Count());

                var orderTasks = new List<Task>();
                foreach (Order? order in orders)
                {
                    orderTasks.Add(Task.Run(async () =>
                    {
                        logger.LogInformation("Checking inventory for Order {OrderId}", order.OrderId);

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
                                    logger.LogInformation("Checking inventory for product id {ProductId} in order {OrderId}", cartItem.ProductId, order.OrderId);

                                    var inStock = await productsClient.CanInventoryFulfill(cartItem.ProductId, cartItem.Quantity);

                                    if (inStock)
                                    {
                                        logger.LogInformation("Inventory OK for product id {ProductId} in order {OrderId}", cartItem.ProductId, order.OrderId);
                                    }
                                    else
                                    {
                                        logger.LogInformation("Not enough inventory for product id {ProductId} in order {OrderId}", cartItem.ProductId, order.OrderId);

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
                                        logger.LogInformation("Removing {Quantity} of product id {ProductId} from inventory", cartItem.Quantity, cartItem.ProductId);

                                        await productsClient.SubtractInventory(cartItem.ProductId, cartItem.Quantity);

                                        logger.LogInformation("Removed {Quantity} of product id {ProductId} from inventory", cartItem.Quantity, cartItem.ProductId);
                                    }));
                                }

                                logger.LogInformation("Marking order {OrderId} as ready for shipment", order.OrderId);

                                invTasks.Add(ordersClient.MarkOrderReadyForShipment(order));

                                logger.LogInformation("Marked order {OrderId} as ready for shipment", order.OrderId);

                                await Task.WhenAll(invTasks);
                            }
                        }
                    }));

                    await Task.WhenAll(orderTasks);
                }
            }

            await Task.Delay(CheckOrderInterval, stoppingToken);
        }
    }
}