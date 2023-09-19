﻿using System.Data;
using Nanorm;
using Npgsql;

namespace Orders;

public class OrderDatabaseRecord : IDataRecordMapper<OrderDatabaseRecord>
{
    public Guid OrderId { get; set; }
    public DateTime OrderedAt { get; set; }
    public bool HasShipped { get; set; }

    public static OrderDatabaseRecord Map(IDataRecord dataRecord)
        => new()
        {
            OrderId = dataRecord.GetGuid(dataRecord.GetOrdinal(nameof(OrderId))),
            HasShipped = dataRecord.GetBoolean(dataRecord.GetOrdinal(nameof(HasShipped))),
            OrderedAt = dataRecord.GetDateTime(dataRecord.GetOrdinal(nameof(OrderedAt))),
        };
}

public class CartItemDatabaseRecord : IDataRecordMapper<CartItemDatabaseRecord>
{
    public int CartItemId { get; set; }
    public Guid OrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public static CartItemDatabaseRecord Map(IDataRecord dataRecord)
        => new()
        {
            CartItemId = dataRecord.GetInt32(dataRecord.GetOrdinal(nameof(CartItemId))),
            OrderId = dataRecord.GetGuid(dataRecord.GetOrdinal(nameof(OrderId))),
            ProductId = dataRecord.GetString(dataRecord.GetOrdinal(nameof(ProductId))),
            Quantity = dataRecord.GetInt32(dataRecord.GetOrdinal(nameof(Quantity)))
        };
}
