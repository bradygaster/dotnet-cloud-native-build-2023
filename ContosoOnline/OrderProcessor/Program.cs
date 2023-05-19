using OpenTelemetry.Trace;
using OrderProcessor;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ProductServiceClient>();
builder.Services.AddGrpcClient<Products.Products.ProductsClient>(c =>
{
    var backendUrl = builder.Configuration["PRODUCTS_URL"] ?? throw new InvalidOperationException("PRODUCTS_URL is not set");

    c.Address = new(backendUrl);
});

builder.Services.AddHttpClient<OrderServiceClient>(c =>
{
    var url = builder.Configuration["ORDERS_URL"] ?? throw new InvalidOperationException("ORDERS_URL is not set");

    c.BaseAddress = new(url);
});

builder.Services.AddObservability("OrderProcessor", tracing =>
{
    tracing.AddSource(nameof(Worker))
        .AddZipkinExporter(zipkin =>
        {
            zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
        });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();

