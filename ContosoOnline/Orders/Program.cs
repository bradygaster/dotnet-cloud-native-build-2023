using OpenTelemetry.Metrics;
using Orders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IOrderService, FakeOrderService>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            .AddView("request-duration", new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                })
            .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
            .AddPrometheusExporter();
    });

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapGet("/orders", async (IOrderService service) =>
    await service.GetOrdersAsync()
);

app.MapPost("/orders", async (Order order, IOrderService service) =>
    await service.SaveOrderAsync(order)
);

app.Run();

public record CartItem(string ProductId, int Quantity = 1);

public record Order(CartItem[] Cart, DateTime OrderedAt);
