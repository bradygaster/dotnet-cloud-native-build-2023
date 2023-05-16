using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using Orders;
using Systems.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability("Orders");
builder.Services.AddDatabase();

var app = builder.Build();

app.MapGet("/orders", async (NpgsqlDataSource db) =>
{
    var orders = (await db.QueryAsync<OrderDatabaseRecord>("SELECT * FROM orders WHERE hasshipped = false").ToListAsync())
        .Select(p => new Order(p.OrderedAt, p.OrderId)).ToList();

    var cartItems = (await db.QueryAsync<CartItemDatabaseRecord>("SELECT * FROM carts").ToListAsync()).ToList();

    orders.ForEach(o => o.Cart =
        cartItems.Where(c => c.OrderId == o.OrderId)
            .Select(c => new CartItem(c.ProductId, c.Quantity)).ToArray());

    return orders;
});

app.MapPost("/orders", async Task<Created<Order>> (Order order, NpgsqlDataSource db, CancellationToken ct) =>
{
    var createdOrder = await db.QuerySingleAsync<OrderDatabaseRecord>(
        $"INSERT INTO orders({nameof(OrderDatabaseRecord.OrderedAt)}, {nameof(OrderDatabaseRecord.OrderId)}, {nameof(OrderDatabaseRecord.HasShipped)}) Values(CURRENT_DATE, $1, false) RETURNING *",
        ct,
        order.OrderId.AsTypedDbParameter());

    foreach (var item in order.Cart)
    {
        var createdCartItem = await db.QuerySingleAsync<CartItemDatabaseRecord>(
            $"INSERT INTO carts({nameof(CartItemDatabaseRecord.OrderId)}, {nameof(CartItemDatabaseRecord.ProductId)}, {nameof(CartItemDatabaseRecord.Quantity)}) Values($1, $2, $3) RETURNING *",
            ct,
            order.OrderId.AsTypedDbParameter(),
            item.ProductId.AsTypedDbParameter(),
            item.Quantity.AsTypedDbParameter());
    }

    return TypedResults.Created($"/orders", new Order(createdOrder.OrderedAt, createdOrder.OrderId));
});

app.MapPut("/orders/{orderId}", async Task<Results<NoContent, NotFound>> (Guid orderId, Order order, NpgsqlDataSource db) =>
{
    return await db.ExecuteAsync(
        "UPDATE orders SET hasshipped = true WHERE orderid = $1",
        orderId.AsTypedDbParameter()) == 1
        ? TypedResults.NoContent()
        : TypedResults.NotFound();
});

app.MapGet("/", () => "Orders");

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();

public record CartItem(string ProductId, int Quantity = 1);

public record Order(DateTime OrderedAt, Guid OrderId)
{
    public bool HasShipped { get; set; }
    public CartItem[] Cart { get; set; }
}
