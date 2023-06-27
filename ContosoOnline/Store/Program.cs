using Microsoft.Extensions.Http.Resilience;
using MudBlazor.Services;
using Store;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("StoreUX", builder.Configuration);

builder.Services.AddGrpcClient<Products.Products.ProductsClient>(c =>
{
    var backendUrl = builder.Configuration["PRODUCTS_URL"] ?? throw new InvalidOperationException("PRODUCTS_URL is not set");

    c.Address = new(backendUrl);
})
.AddStandardResilienceHandler()
;

builder.Services.AddHttpClient<OrderServiceClient>(c =>
{
    var url = builder.Configuration["ORDERS_URL"] ?? throw new InvalidOperationException("ORDERS_URL is not set");

    c.BaseAddress = new(url);
})
.AddStandardResilienceHandler()
;

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapObservability();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
