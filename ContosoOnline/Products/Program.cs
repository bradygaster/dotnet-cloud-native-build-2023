using Products;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddGrpc();
builder.Services.AddSingleton<IProductService, FakeProductService>();

var app = builder.Build();

app.MapGrpcService<ProductsGrpcService>();

app.MapDefaultEndpoints();

app.Run();