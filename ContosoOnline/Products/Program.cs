using Microsoft.AspNetCore.Server.Kestrel.Core;
using Products;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();

builder.Services.AddObservability("Products");

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Listen(IPAddress.Any, 80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.MapGet("/", () => "Products");

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapGrpcService<ProductsGrpcService>();

app.Run();