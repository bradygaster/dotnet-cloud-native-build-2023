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
                new Order(new CartItem[] 
                { 
                    new CartItem("1", 2, 1.99m), 
                    new CartItem("2", 1, 19.99m), 
                    new CartItem("3", 4, 4.99m), 
                    new CartItem("4", 4, 5.99m),
                }, DateTime.Now),
            });
            return Task.FromResult(fakeOrders.ToArray());
        }

        public Task<bool> SaveOrderAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
