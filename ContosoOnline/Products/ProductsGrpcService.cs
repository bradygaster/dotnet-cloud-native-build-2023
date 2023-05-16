using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Products
{
    public class ProductsGripService : Products.ProductsBase
    {
        private readonly IProductService _productService;

        public ProductsGripService(IProductService productService, ILogger<ProductsGripService> logger)
        {
            _productService = productService;
        }

        public override async Task<GetProductsResponse> GetProducts(Empty request, ServerCallContext context)
            => await _productService.GetProductsAsync()
                .ContinueWith(task => new GetProductsResponse { Products = { task.Result } });

        public override async Task<CheckProductInventoryResponse> CheckProductInventory(CheckProductInventoryRequest request, ServerCallContext context)
            => await _productService.CheckProductInventoryAsync(request.ProductId, request.ItemsRequested)
                .ContinueWith(task => new CheckProductInventoryResponse { IsEnoughAvailable = task.Result });

        public override async Task<InventorySubtractionResponse> SubtractInventory(InventorySubtractionRequest request, ServerCallContext context)
            => await _productService.SubtractInventory(request.ProductId, request.ItemsRequested)
                .ContinueWith(task => new InventorySubtractionResponse { InventoryUpdated = task.Result });
    }
}
