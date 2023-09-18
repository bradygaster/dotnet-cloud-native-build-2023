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

            var connectionString = configuration.GetConnectionString("OrdersDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                var pgHost = configuration["POSTGRES_HOST"];
                var pgPassword = configuration["POSTGRES_PASSWORD"];
                var pgUser = configuration["POSTGRES_USERNAME"];
                var pgDatabase = configuration["POSTGRES_DATABASE"];
                
                connectionString = $"Host={pgHost};Database={pgDatabase};Username={pgUser};Password={pgPassword};Timeout=300";

                if (string.IsNullOrEmpty(pgUser) && string.IsNullOrEmpty(pgHost))
                {
                    throw new InvalidDataException("Missing postgres connection string or environment variables");
                }
            }

            return new NpgsqlSlimDataSourceBuilder(connectionString).Build();
        });

        services.AddHostedService<DatabaseInitializer>();
        services.AddSingleton<IOrdersDb, OrdersDb>();
        //services.AddDbResiliencePolicy<List<CartItemDatabaseRecord>>("cart");
        //services.AddDbResiliencePolicy<List<OrderDatabaseRecord>>("order");
        //services.AddDbResiliencePolicy<bool>("order-update");
        //services.AddSingleton<DatabaseRetryPolicies>();

        return services;
    }

    // public static IResiliencePipelineBuilder<T> AddDbResiliencePolicy<T>(this IServiceCollection services, string policyName)
    // {
    //     return services.AddResiliencePipeline<T>(policyName)
    //                    .AddRetryPolicy("db-retry")
    //                    .AddCircuitBreakerPolicy("db-cb");
    // }
}

// public class DatabaseRetryPolicies
// {
//     public DatabaseRetryPolicies(IResiliencePipelineProvider provider)
//     {
//         MarkOrderUpdatedPolicy = provider.GetPipeline<bool>("order-update");
//         CartItemListPolicy = provider.GetPipeline<List<CartItemDatabaseRecord>>("cart");
//         OrderListPolicy = provider.GetPipeline<List<OrderDatabaseRecord>>("order");
//     }

//     public IAsyncPolicy<bool> MarkOrderUpdatedPolicy { get; }
//     public IAsyncPolicy<List<CartItemDatabaseRecord>> CartItemListPolicy { get; }
//     public IAsyncPolicy<List<OrderDatabaseRecord>> OrderListPolicy { get; }
// }