using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OrderProcessor;

public class Instrumentation
{
    public static readonly string ActivitySourceName = "Worker";

    public ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);
}

public static class InstrumentationExtensions
{
    public static TracerProviderBuilder AddWorkerInstrumentation(this TracerProviderBuilder tracerProviderBuilder)
    {
        return tracerProviderBuilder.AddSource(Instrumentation.ActivitySourceName);
    }
}
