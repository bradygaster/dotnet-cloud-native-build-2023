using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Products;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductService, FakeProductService>();
builder.Services.AddGrpc();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddEventCountersInstrumentation(c => {
                c.AddEventSources(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft-AspNetCore-Server-Kestrel",
                    "System.Net.Http",
                    "System.Net.Sockets");
            })
            .AddView("request-duration", new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            })
            .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
            .AddPrometheusExporter();
    })
    .WithTracing(tracing => {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "Products", serviceVersion: "1.0"))
            .AddAspNetCoreInstrumentation()
            .AddZipkinExporter(zipkin =>
            {
                zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Listen(IPAddress.Any, 80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.MapGet("/", () => "Products");

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapGrpcService<ProductsGrpcService>();

app.Run();