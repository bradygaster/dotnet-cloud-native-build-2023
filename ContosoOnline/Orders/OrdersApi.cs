using Microsoft.AspNetCore.Http.HttpResults;

namespace Orders;

public static class OrdersApi
{
    public static RouteGroupBuilder MapOrdersApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/orders");

        group.MapGet("/", async (IOrdersDb db) =>
        {
            var orders = (await db.GetShippedOrdersAsync()).Select(p => new Order(p.OrderedAt, p.OrderId)).ToList();

            var cartItems = (await db.GetCartItemsAsync()).ToList();

            orders.ForEach(o => o.Cart =
                cartItems.Where(c => c.OrderId == o.OrderId)
                    .Select(c => new CartItem(c.ProductId, c.Quantity)).ToArray());

            return orders;
        });

        group.MapPost("/", async Task<Results<BadRequest, Created<Order>>> (Order order, IOrdersDb db, CancellationToken ct) =>
        {
            var createdOrder = await db.AddOrderAsync(order.OrderId, ct);

            if (createdOrder is null)
            {
                return TypedResults.BadRequest();
            }

            if (order.Cart is not null)
            {
                foreach (var item in order.Cart)
                {
                    await db.AddCartItemAsync(order.OrderId, item.ProductId, item.Quantity, ct);
                }
            }

            return TypedResults.Created($"/orders", new Order(createdOrder.OrderedAt, createdOrder.OrderId));
        });

        group.MapPut("/{orderId}", async Task<Results<NoContent, NotFound>> (Guid orderId, Order order, IOrdersDb db) =>
        {
            return await db.MarkOrderShippedAsync(orderId)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        return group;
    }
}

public record CartItem(string ProductId, int Quantity = 1);

public record Order(DateTime OrderedAt, Guid OrderId)
{
    public bool HasShipped { get; set; }
    public CartItem[]? Cart { get; set; }
}
