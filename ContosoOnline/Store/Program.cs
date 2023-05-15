using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using Store;

var builder = WebApplication.CreateBuilder(args);
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddObservability("StoreUX");

builder.Services.AddSingleton(services =>
{
    var backendUrl = "http://products:8080";

    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions
    {
        Credentials = ChannelCredentials.Insecure,
        ServiceProvider = services
    });

    return channel;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<OrderServiceClient>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
