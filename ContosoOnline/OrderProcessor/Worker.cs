namespace OrderProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OrdersClient _ordersClient;

        public Worker(ILogger<Worker> logger, OrdersClient ordersClient)
        {
            _logger = logger;
            _ordersClient = ordersClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                var orders = await _ordersClient.GetOrders();

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}