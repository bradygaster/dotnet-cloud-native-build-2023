using Microsoft.Extensions.Resilience;
using Npgsql;
using Polly;

namespace Orders;

internal static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString("OrdersDb") ?? throw new InvalidDataException("Missing connection string");

            return new NpgsqlSlimDataSourceBuilder(connectionString).Build();
        });

        services.AddHostedService<DatabaseInitializer>();

        services.AddSingleton<IOrdersDb, OrdersDb>();

        services.AddDbResiliencePolicy<List<CartItemDatabaseRecord>>("cart");
        services.AddDbResiliencePolicy<List<OrderDatabaseRecord>>("order");
        services.AddDbResiliencePolicy<bool>("order-update");

        services.AddSingleton<DatabaseRetryPolicies>();

        return services;
    }

    public static IResiliencePipelineBuilder<T> AddDbResiliencePolicy<T>(this IServiceCollection services, string policyName)
    {
        return services.AddResiliencePipeline<T>(policyName)
                       .AddCircuitBreakerPolicy("db-cb")
                       .AddRetryPolicy("db-retry");
    }
}

public class DatabaseRetryPolicies
{
    public DatabaseRetryPolicies(IResiliencePipelineProvider provider)
    {
        MarkOrderUpdatedPolicy = provider.GetPipeline<bool>("order-update");
        CartItemListPolicy = provider.GetPipeline<List<CartItemDatabaseRecord>>("cart");
        OrderListPolicy = provider.GetPipeline<List<OrderDatabaseRecord>>("order");
    }

    public IAsyncPolicy<bool> MarkOrderUpdatedPolicy { get; }
    public IAsyncPolicy<List<CartItemDatabaseRecord>> CartItemListPolicy { get; }
    public IAsyncPolicy<List<OrderDatabaseRecord>> OrderListPolicy { get; }
}