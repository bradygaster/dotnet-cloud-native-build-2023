using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Configuration;

namespace Proxy
{
    public class CustomConfigFilter : IProxyConfigFilter
    {
        IConfiguration _configuration;
        ILogger<CustomConfigFilter> _logger;

        public CustomConfigFilter(IConfiguration configuration, ILogger<CustomConfigFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Matches {{env_var_name}} or {{my-name}} or {{123name}} etc.
        private readonly Regex _exp = new("\\{\\{(\\w+\\-?\\w+?)\\}\\}");

        public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig cluster, CancellationToken cancel)
        {
            // Each cluster has a dictionary of destinations, which is read-only, so we'll create a new one with our updates 
            var newDests = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var d in cluster.Destinations)
            {
                var origAddress = d.Value.Address;
                if (_exp.IsMatch(origAddress))
                {
                    // Get the name of the env variable from the destination and lookup value
                    var lookup = _exp.Matches(origAddress)[0].Groups[1].Value;
                    var newAddress = _configuration.GetValue<string>(lookup);

                    if (string.IsNullOrWhiteSpace(newAddress))
                    {
                        throw new System.ArgumentException($"Configuration Filter Error: Substitution for '{lookup}' in cluster '{d.Key}' not found in configuration.");
                    }

                    var modifiedDest = d.Value with { Address = newAddress };
                    newDests.Add(d.Key, modifiedDest);
                }
                else
                {
                    newDests.Add(d.Key, d.Value);
                }
            }

            return new ValueTask<ClusterConfig>(cluster with { Destinations = newDests });
        }

        public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig? cluster, CancellationToken cancel)
        {
            if (route.Order.HasValue && route.Order.Value < 1)
            {
                return new ValueTask<RouteConfig>(route with { Order = 1 });
            }

            return new ValueTask<RouteConfig>(route);
        }
    }
}
