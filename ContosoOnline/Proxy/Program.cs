using Proxy;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability("Proxy");
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddConfigFilter<CustomConfigFilter>();

var app = builder.Build();
app.MapReverseProxy();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();