using Npgsql;

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

        return services;
    }
}