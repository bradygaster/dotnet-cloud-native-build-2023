namespace Orders
{
    public interface IOrderService
    {
        public Task<Order[]> GetOrdersAsync();
        public Task<bool> SaveOrderAsync(Order order);
    }

    public class FakeOrderService : IOrderService
    {
        public Task<Order[]> GetOrdersAsync()
        {
            var fakeOrders = new List<Order>();
            fakeOrders.AddRange(new Order[]
            {
                new Order(DateTime.Now, Guid.NewGuid())
                {
                    Cart = new CartItem[]
                    {
                        new CartItem("01", 1),
                        new CartItem("02", 2),
                        new CartItem("03", 1),
                        new CartItem("04", 6),
                        new CartItem("05", 6),
                    }
                },
            });
            return Task.FromResult(fakeOrders.ToArray());
        }

        public Task<bool> SaveOrderAsync(Order order)
        {
            return Task.FromResult(false);
        }
    }
}
