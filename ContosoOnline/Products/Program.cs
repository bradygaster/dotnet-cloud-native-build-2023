using OpenTelemetry.Metrics;
using Products;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddView("request-duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
        builder.AddPrometheusExporter();
        builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
    });

var app = builder.Build();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapGrpcService<ProductsGrpcService>();

app.Run();