using Npgsql;
using Nanorm;

namespace Orders;

public class DatabaseInitializer : IHostedService
{
    private const string Variable = "SEED_DATABASE";
    private readonly NpgsqlDataSource _db;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(NpgsqlDataSource db, ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await Initialize(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task Initialize(CancellationToken cancellationToken = default)
    {
        // NOTE: Npgsql removes the password from the connection string
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Ensuring database exists and is up to date");
        }

        // create the orders and cart tables
        var create = $"""
                CREATE TABLE IF NOT EXISTS public.orders
                (
                    {nameof(OrderDatabaseRecord.OrderId)} uuid PRIMARY KEY,
                    {nameof(OrderDatabaseRecord.OrderedAt)} date NOT NULL,
                    {nameof(OrderDatabaseRecord.HasShipped)} boolean NOT NULL DEFAULT false
                );
                DELETE FROM public.orders;
                """;

        await _db.ExecuteAsync(create, cancellationToken);

        var createCarts = $"""
                CREATE TABLE IF NOT EXISTS public.carts
                (
                    {nameof(CartItemDatabaseRecord.CartItemId)} SERIAL PRIMARY KEY,
                    {nameof(CartItemDatabaseRecord.OrderId)} uuid NOT NULL,
                    {nameof(CartItemDatabaseRecord.ProductId)} text NOT NULL,
                    {nameof(CartItemDatabaseRecord.Quantity)} int NOT NULL DEFAULT 1
                );
                DELETE FROM public.carts;
                """;

        await _db.ExecuteAsync(createCarts, cancellationToken);

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Variable))
            && Environment.GetEnvironmentVariable("SEED_DATABASE")?.ToLowerInvariant() == "true")
        {
            // random products
            var randomProductIDs = new string[] { "01", "02", "03", "04", "05" };

            // insert some random records
            for (int i = 0; i < 10; i++)
            {
                var orderId = Guid.NewGuid().ToString();

                // make an order
                var insertOrderSql = $"""
                INSERT INTO
                    public.orders (
                        {nameof(OrderDatabaseRecord.OrderId)}, 
                        {nameof(OrderDatabaseRecord.OrderedAt)}, 
                        {nameof(OrderDatabaseRecord.HasShipped)}
                    )
                VALUES
                    ('{orderId}', CURRENT_DATE, false);
                """;

                await _db.ExecuteAsync(insertOrderSql, cancellationToken);

                // add some cart items to associate with the order
                var cart = new List<string>();
                randomProductIDs.OrderBy(x => Guid.NewGuid()).Take(3).ToList().ForEach(x => cart.Add(x));

                foreach (var cartItem in cart)
                {
                    // add the line item for the order
                    var insertCartItemSql = $"""
                    INSERT INTO
                        public.carts (
                            {nameof(CartItemDatabaseRecord.OrderId)},
                            {nameof(CartItemDatabaseRecord.ProductId)},
                            {nameof(CartItemDatabaseRecord.Quantity)}
                        )
                    VALUES
                        ('{orderId}', '{cartItem}', {Random.Shared.Next(1, 10)})
                    """;

                    await _db.ExecuteAsync(insertCartItemSql, cancellationToken);
                }
            }
        }
    }
}
