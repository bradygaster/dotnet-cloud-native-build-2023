using Orders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability("Orders");
builder.Services.AddDatabase();

var app = builder.Build();

app.MapGet("/", () => "Orders");
app.MapOrdersApi();
app.MapPrometheusScrapingEndpoint();

app.Run();
