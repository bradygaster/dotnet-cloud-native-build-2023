using Products;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<ProductsGrpcService>();

app.Run();