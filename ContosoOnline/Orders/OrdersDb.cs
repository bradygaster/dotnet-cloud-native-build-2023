using Npgsql;
using Systems.Collections.Generic;

namespace Orders;

public interface IOrdersDb
{
    public Task<List<OrderDatabaseRecord>> GetShippedOrdersAsync();
    public Task<List<CartItemDatabaseRecord>> GetCartItemsAsync();

    public Task<OrderDatabaseRecord?> AddOrderAsync(Guid orderId, CancellationToken cancellationToken);
    public Task<CartItemDatabaseRecord?> AddCartItemAsync(Guid orderId, string productId, int quantity, CancellationToken cancellationToken);
    public Task<bool> MarkOrderShippedAsync(Guid orderId);
}

public class OrdersDb(NpgsqlDataSource db, DatabaseRetryPolicies policies) : IOrdersDb
{
    public Task<CartItemDatabaseRecord?> AddCartItemAsync(Guid orderId, string productId, int quantity, CancellationToken cancellationToken)
    {
        return db.QuerySingleAsync<CartItemDatabaseRecord>(
                $"INSERT INTO carts({nameof(CartItemDatabaseRecord.OrderId)}, {nameof(CartItemDatabaseRecord.ProductId)}, {nameof(CartItemDatabaseRecord.Quantity)}) Values($1, $2, $3) RETURNING *",
                cancellationToken,
                orderId.AsTypedDbParameter(),
                productId.AsTypedDbParameter(),
                quantity.AsTypedDbParameter());
    }

    public Task<OrderDatabaseRecord?> AddOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return db.QuerySingleAsync<OrderDatabaseRecord>(
               $"INSERT INTO orders({nameof(OrderDatabaseRecord.OrderedAt)}, {nameof(OrderDatabaseRecord.OrderId)}, {nameof(OrderDatabaseRecord.HasShipped)}) Values(CURRENT_DATE, $1, false) RETURNING *",
               cancellationToken,
               orderId.AsTypedDbParameter());
    }

    public Task<List<CartItemDatabaseRecord>> GetCartItemsAsync()
    {
        return policies.CartItemListPolicy.ExecuteAsync(() =>
            db.QueryAsync<CartItemDatabaseRecord>("SELECT * FROM carts").ToListAsync());
    }

    public Task<List<OrderDatabaseRecord>> GetShippedOrdersAsync()
    {
        return policies.OrderListPolicy.ExecuteAsync(() => 
            db.QueryAsync<OrderDatabaseRecord>("SELECT * FROM orders WHERE hasshipped = true").ToListAsync());
    }

    public Task<bool> MarkOrderShippedAsync(Guid orderId)
    {
        return policies.MarkOrderUpdatedPolicy.ExecuteAsync(async () =>
            await db.ExecuteAsync("UPDATE orders SET hasshipped = true WHERE orderid = $1", orderId.AsTypedDbParameter()) == 1
        );
    }
}
