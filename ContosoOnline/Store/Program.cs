using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

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
                .AddService(serviceName: "Orders", serviceVersion: "1.0"))
            .AddAspNetCoreInstrumentation()
            .AddZipkinExporter(zipkin =>
            {
                zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
