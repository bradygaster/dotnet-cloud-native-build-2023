using Orders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability("Orders", builder.Configuration);
builder.Services.AddDatabase();

var app = builder.Build();

app.MapGet("/", () => "Orders");
app.MapOrdersApi();
app.MapObservability();

app.Run();
