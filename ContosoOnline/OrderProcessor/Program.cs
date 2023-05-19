using Grpc.Core;
using Grpc.Net.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderProcessor;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(services =>
{
    var backendUrl = builder.Configuration["PRODUCTS_URL"] ?? throw new InvalidOperationException("PRODUCTS_URL is not set");

    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions
    {
        Credentials = ChannelCredentials.Insecure,
        ServiceProvider = services
    });

    return channel;
});

builder.Services.AddObservability("OrderProcessor", tracing =>
{
    tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(serviceName: "OrderProcessor", serviceVersion: "1.0"))
        .AddSource(nameof(Worker))
        .AddZipkinExporter(zipkin =>
        {
            zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
        });
});

builder.Services.AddHttpClient<OrderServiceClient>(c =>
{
    var url = builder.Configuration["ORDERS_URL"] ?? throw new InvalidOperationException("ORDERS_URL is not set");

    c.BaseAddress = new(url);
});

builder.Services.AddSingleton<ProductServiceClient>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();

