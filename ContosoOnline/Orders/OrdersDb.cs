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
public class OrdersDb(NpgsqlDataSource db, ILogger<OrdersDb> logger) : IOrdersDb
{
    public Task<CartItemDatabaseRecord?> AddCartItemAsync(Guid orderId, string productId, int quantity, CancellationToken cancellationToken)
    {
        string sql = $"INSERT INTO carts(OrderId, ProductId, Quantity) Values('{orderId}', '{productId}', '{quantity}') RETURNING *";
        logger.LogInformation(sql);
        return db.QuerySingleAsync<CartItemDatabaseRecord>(
                sql,
                cancellationToken);
    }

    public Task<OrderDatabaseRecord?> AddOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        string sql = $"INSERT INTO orders(OrderedAt,OrderId, HasShipped) Values(CURRENT_DATE, '{orderId}', false) RETURNING *";
        logger.LogInformation(sql);
        return db.QuerySingleAsync<OrderDatabaseRecord>(
               sql,
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
        string sql = $"UPDATE orders SET hasshipped = true WHERE orderid = '{orderId}'";
        logger.LogInformation(sql);
        return await db.ExecuteAsync(sql) == 1;
    }
}
