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

app.MapPost("/orders", async (Order order) =>
{
    throw new NotImplementedException();
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
