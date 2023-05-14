using OrderProcessor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddSingleton<OrdersClient>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
