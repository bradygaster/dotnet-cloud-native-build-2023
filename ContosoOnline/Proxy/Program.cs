using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("Proxy", builder.Configuration);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddConfigurationDrivenProxyFilter();

var app = builder.Build();

app.MapReverseProxy();
app.MapObservability();

app.Run();