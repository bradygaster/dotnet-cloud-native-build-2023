using Grpc.Core;
using Grpc.Net.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderProcessor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(services =>
        {
            var backendUrl = "http://products:8080";

            var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Insecure,
                ServiceProvider = services
            });

            return channel;
        });
        services.AddObservability("OrderProcessor", tracing =>
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
        services.AddHttpClient();
        services.AddSingleton<OrderServiceClient>();
        services.AddSingleton<ProductServiceClient>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

