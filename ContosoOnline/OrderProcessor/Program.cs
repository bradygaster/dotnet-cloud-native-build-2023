using Grpc.Core;
using Grpc.Net.Client;
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
        
        services.AddHttpClient();
        services.AddSingleton<OrderServiceClient>();
        services.AddSingleton<ProductServiceClient>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

