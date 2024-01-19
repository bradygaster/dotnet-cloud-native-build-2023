using System.Diagnostics;

namespace OrderProcessor;

public class Instrumentation
{
    public static readonly string ActivitySourceName = "Worker";

    public ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);
}
