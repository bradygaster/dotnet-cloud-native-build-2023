using Microsoft.Extensions.Http.Resilience;
using OrderProcessor;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<ProductServiceClient>();
builder.Services.AddGrpcClient<Products.Products.ProductsClient>(c =>
{
    var backendUrl = builder.Configuration["PRODUCTS_URL"] ?? throw new InvalidOperationException("PRODUCTS_URL is not set");

    c.Address = new(backendUrl);
})
//.AddStandardResilienceHandler() // think this has a bug for workers
;

builder.Services.AddHttpClient<OrderServiceClient>(c =>
{
    var url = builder.Configuration["ORDERS_URL"] ?? throw new InvalidOperationException("ORDERS_URL is not set");

    c.BaseAddress = new(url);
})
//.AddStandardResilienceHandler() // think this has a bug for workers
;

builder.Services.AddObservability("OrderProcessor", builder.Configuration, tracing =>
{
    tracing.AddWorkerInstrumentation();
});

builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddHostedService<OrderProcessingWorker>();
builder.Services.AddScoped<OrderProcessingRequest>();

var host = builder.Build();

host.Run();

