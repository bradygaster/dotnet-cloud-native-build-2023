using Nanorm.Npgsql;
using Npgsql;

namespace Orders
{
    public class OrderDatabaseRecord : IDataReaderMapper<OrderDatabaseRecord>
    {
        public Guid OrderId { get; set; }
        public DateTime OrderedAt { get; set; }
        public bool HasShipped { get; set; }

        public static OrderDatabaseRecord Map(NpgsqlDataReader dataReader)
            => new()
            {
                OrderId = dataReader.GetGuid(dataReader.GetOrdinal(nameof(OrderId))),
                HasShipped = dataReader.GetBoolean(dataReader.GetOrdinal(nameof(HasShipped))),
                OrderedAt = dataReader.GetDateTime(dataReader.GetOrdinal(nameof(OrderedAt))),
            };
    }

    public class CartItemDatabaseRecord : IDataReaderMapper<CartItemDatabaseRecord>
    {
        public int CartItemId { get; set; }
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public static CartItemDatabaseRecord Map(NpgsqlDataReader dataReader)
            => new()
            {
                CartItemId = dataReader.GetInt32(dataReader.GetOrdinal(nameof(CartItemId))),
                OrderId = dataReader.GetGuid(dataReader.GetOrdinal(nameof(OrderId))),
                ProductId = dataReader.GetString(dataReader.GetOrdinal(nameof(ProductId))),
                Quantity = dataReader.GetInt32(dataReader.GetOrdinal(nameof(Quantity)))
            };
    }
}
