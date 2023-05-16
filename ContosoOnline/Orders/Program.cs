using Orders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability("Orders", builder.Configuration);
builder.Services.AddDatabase();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapGet("/", () => "Orders");

app.MapOrdersApi();
app.MapObservability();

app.Run();

[JsonSerializable(typeof(List<Order>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
