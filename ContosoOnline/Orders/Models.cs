namespace Orders;

public class OrderDatabaseRecord
{
    public Guid OrderId { get; set; }
    public DateTime OrderedAt { get; set; }
    public bool HasShipped { get; set; }
}

public class CartItemDatabaseRecord
{
    public Guid CartItemId { get; set; }
    public Guid OrderId { get; set; }
    public required string ProductId { get; set; }
    public int Quantity { get; set; }
}
