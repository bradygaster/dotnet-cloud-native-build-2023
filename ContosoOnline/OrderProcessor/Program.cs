var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddTransient<ProductServiceClient>();
builder.Services.AddGrpcClient<Products.Products.ProductsClient>(c => c.Address = new("http://products"));
builder.Services.AddHttpClient<OrderServiceClient>(c => c.BaseAddress = new("http://orders"));
builder.Services.AddHostedService<OrderProcessingWorker>();
builder.Services.AddScoped<OrderProcessingRequest>();

var host = builder.Build();

host.Run();

