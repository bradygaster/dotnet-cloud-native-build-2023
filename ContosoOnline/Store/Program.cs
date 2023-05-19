using Grpc.Core;
using Grpc.Net.Client;
using MudBlazor.Services;
using Store;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("StoreUX");

builder.Services.AddSingleton(services =>
{
    var backendUrl = builder.Configuration["PRODUCTS_URL"] ?? throw new InvalidOperationException("PRODUCTS_URL is not set");

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
builder.Services.AddHttpClient<OrderServiceClient>(c =>
{
    var url = builder.Configuration["ORDERS_URL"] ?? throw new InvalidOperationException("ORDERS_URL is not set");

    c.BaseAddress = new(url);
});

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
