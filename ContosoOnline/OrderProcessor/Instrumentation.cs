using System.Diagnostics;

namespace OrderProcessor;

public class Instrumentation
{
    public ActivitySource ActivitySource { get; } = new ActivitySource(nameof(Worker));
}
