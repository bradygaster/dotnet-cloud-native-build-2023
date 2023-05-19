using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class DiagnosticServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName)
    {
        return services.AddObservability(serviceName, (tracing) =>
        {
            tracing.AddAspNetCoreInstrumentation()
                .AddZipkinExporter(zipkin =>
                {
                    zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                });
        });
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, 
        string serviceName, 
        Action<TracerProviderBuilder>? tracerBuilderAction = null)
    {
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: "1.0");

        services
            .AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resource)
                        .AddAspNetCoreInstrumentation()
                        .AddEventCountersInstrumentation(c =>
                        {
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
                .WithTracing(tracing =>
                {
                    tracing.SetResourceBuilder(resource);

                    tracerBuilderAction?.Invoke(tracing);
                });

        return services;
    }
}