using Npgsql;
using Orders;

namespace Microsoft.Extensions.Hosting
{
    internal static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddSingleton(static sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var connectionString = configuration.GetConnectionString("OrdersDb") ?? throw new InvalidDataException("Missing connection string");

                var db = new NpgsqlSlimDataSourceBuilder(connectionString).Build();

                return db;
            });

            services.AddHostedService<DatabaseInitializer>();

            return services;
        }
    }
}