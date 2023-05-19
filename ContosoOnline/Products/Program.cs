using Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();
builder.Services.AddObservability("Products", builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Products");

app.MapGrpcService<ProductsGrpcService>();
app.MapPrometheusScrapingEndpoint();

app.Run();