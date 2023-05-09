using Orders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IOrderService, FakeOrderService>();

var app = builder.Build();

app.MapGet("/orders", async (IOrderService service) =>
    await service.GetOrdersAsync()
);

app.MapPost("/orders", async (Order order, IOrderService service) =>
    await service.SaveOrderAsync(order)
);

app.Run();

public record CartItem(string ProductId, int Quantity = 1);

public record Order(CartItem[] Cart, DateTime OrderedAt);
