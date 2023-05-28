using System.Diagnostics;

namespace OrderProcessor;

public class OrderProcessingRequest(Instrumentation instrumentation, 
                                    ILogger<OrderProcessingRequest> logger, 
                                    OrderServiceClient ordersClient, 
                                    ProductServiceClient productsClient)
{
    public async Task ProcessOrdersAsync(Activity? activity, CancellationToken stoppingToken)
    {
        var orders = await ordersClient.GetOrdersAsync(stoppingToken);
        activity?.AddTag("order-count", orders.Count());

        // REVIEW: Should we do this concurrently?
        foreach (var order in orders)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await ProcessOrderAsync(order);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing order {OrderId}", order.OrderId);
            }
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        using var orderActivity = instrumentation.ActivitySource.StartActivity("order-processor.process-order");

        logger.LogInformation("Checking inventory for Order {OrderId}", order.OrderId);

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
}
