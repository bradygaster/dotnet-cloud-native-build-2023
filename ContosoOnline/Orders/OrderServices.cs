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
            throw new NotImplementedException();
        }

        public Task<bool> SaveOrderAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
