using Microsoft.AspNetCore.Http.HttpResults;

namespace Orders;

public static class OrdersApi
{
    public static RouteGroupBuilder MapOrdersApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/orders");

        group.MapGet("/", async (OrdersDbContext dbContext) =>
        {
            var orders = (dbContext.OrderItems.Where(x => x.HasShipped == false).ToList()).Select(p => new Order(p.OrderedAt, p.OrderId)).ToList();

            var cartItems = (dbContext.CartItems.ToList()).ToList();

            orders.ForEach(o => o.Cart =
                cartItems.Where(c => c.OrderId == o.OrderId)
                    .Select(c => new CartItem(c.ProductId, c.Quantity)).ToArray());

            return orders;
        });

        group.MapPost("/", async Task<Results<BadRequest, Created<Order>>> (OrdersDbContext dbContext, Order order, CancellationToken ct) =>
        {
            var newOrder = new OrderDatabaseRecord
            {
                OrderId = order.OrderId,
                OrderedAt = DateTime.UtcNow,
                HasShipped = false
            };

            dbContext.OrderItems.Add(newOrder);
            await dbContext.SaveChangesAsync(ct);

            if (newOrder is null)
            {
                return TypedResults.BadRequest();
            }

            if (order.Cart is not null)
            {
                foreach (var item in order.Cart)
                {
                    var newCartItem = new CartItemDatabaseRecord
                    {
                        CartItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };

                    dbContext.CartItems.Add(newCartItem);
                    await dbContext.SaveChangesAsync(ct);
                }
            }

            return TypedResults.Created($"/orders", new Order(newOrder.OrderedAt, newOrder.OrderId));
        });

        group.MapPut("/{orderId}", async Task<Results<NoContent, NotFound>> (OrdersDbContext dbContext, Guid orderId, Order order) =>
        {
            var b = dbContext.OrderItems.FirstOrDefault(x => x.OrderId == orderId);
            if (b is not null)
            {
                b.HasShipped = true;
                dbContext.Update(b);
                await dbContext.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
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
