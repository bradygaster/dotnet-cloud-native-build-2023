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
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                
                var db = new NpgsqlSlimDataSourceBuilder(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")).Build();

                return db;
            });
            services.AddHostedService<DatabaseInitializer>();
            return services;
        }
    }
}
