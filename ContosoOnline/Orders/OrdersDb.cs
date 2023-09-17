using Npgsql;
using System.Collections.Generic;
using System.Threading;
﻿using System.Data;
using Nanorm;

namespace Orders;

public interface IOrdersDb
{
    public Task<List<OrderDatabaseRecord>> GetUnshippedOrdersAsync();
    public Task<List<CartItemDatabaseRecord>> GetCartItemsAsync();

    public Task<OrderDatabaseRecord?> AddOrderAsync(Guid orderId, CancellationToken cancellationToken);
    public Task<CartItemDatabaseRecord?> AddCartItemAsync(Guid orderId, string productId, int quantity, CancellationToken cancellationToken);
    public Task<bool> MarkOrderShippedAsync(Guid orderId);
}
public class OrdersDb(NpgsqlDataSource db) : IOrdersDb

{
    public Task<CartItemDatabaseRecord?> AddCartItemAsync(Guid orderId, string productId, int quantity, CancellationToken cancellationToken)
    {
        return db.QuerySingleAsync<CartItemDatabaseRecord>(
                $"INSERT INTO carts({nameof(CartItemDatabaseRecord.OrderId)}, {nameof(CartItemDatabaseRecord.ProductId)}, {nameof(CartItemDatabaseRecord.Quantity)}) Values({orderId}, {productId}, {quantity}) RETURNING *",
                cancellationToken);
    }

    public Task<OrderDatabaseRecord?> AddOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return db.QuerySingleAsync<OrderDatabaseRecord>(
               $"INSERT INTO orders({nameof(OrderDatabaseRecord.OrderedAt)}, {nameof(OrderDatabaseRecord.OrderId)}, {nameof(OrderDatabaseRecord.HasShipped)}) Values(CURRENT_DATE, {orderId}, false) RETURNING *",
               cancellationToken);
    }

    public Task<List<CartItemDatabaseRecord>> GetCartItemsAsync()
    {
        return db.QueryAsync<CartItemDatabaseRecord>("SELECT * FROM carts").ToListAsync();
    
    }

    public Task<List<OrderDatabaseRecord>> GetUnshippedOrdersAsync()
    {
        return db.QueryAsync<OrderDatabaseRecord>("SELECT * FROM orders WHERE hasshipped = false").ToListAsync();
    }

    public async Task<bool> MarkOrderShippedAsync(Guid orderId)
    {
        return await db.ExecuteAsync("UPDATE orders SET hasshipped = true WHERE orderid = {orderId}") == 1;
    }
}
