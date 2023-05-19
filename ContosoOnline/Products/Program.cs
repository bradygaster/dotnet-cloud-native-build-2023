using Products;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();

builder.Services.AddObservability("Products");

var app = builder.Build();

app.MapGet("/", () => "Products");

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapGrpcService<ProductsGripService>();

app.Run();